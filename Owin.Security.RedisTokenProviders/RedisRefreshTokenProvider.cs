using System;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Serializer;
using Microsoft.Owin.Security.Infrastructure;
using StackExchange.Redis;

namespace Owin.Security.RedisTokenProviders
{
    public class RedisRefreshTokenProvider : IAuthenticationTokenProvider
    {
        private readonly IProviderConfiguration _configuration;
        private readonly ConnectionMultiplexer _redis;
        public Func<AuthenticationTicket, string, string> StoreKeyFunc { get; set; }

        public RedisRefreshTokenProvider(IProviderConfiguration configuration)
        {
            _configuration = configuration ?? new ProviderConfiguration { ConnectionString = "localhost:6379", ExpiresUtc = DateTime.UtcNow.AddYears(1), Db = 0 , AbortOnConnectFail = true};

            var options = ConfigurationOptions.Parse(_configuration.ConnectionString);
            options.AbortOnConnectFail = _configuration.AbortOnConnectFail;

            _redis = ConnectionMultiplexer.Connect(options);
        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var refreshToken = Guid.NewGuid().ToString();
            StoreKeyFunc = StoreKeyFunc ?? ((ctx, token) => token);

            var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                IssuedUtc = context.Ticket.Properties.IssuedUtc,
                ExpiresUtc = _configuration.ExpiresUtc
            };

            var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

            var key = StoreKeyFunc(context.Ticket, refreshToken);

            await StoreAsync(key, refreshTokenTicket);

            context.SetToken(refreshToken);
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            TicketResult result = await RemoveAsync(context);
            if (result.Deleted)
            {
                context.SetTicket(result.Ticket);
            }
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            var refreshToken = Guid.NewGuid().ToString();
            StoreKeyFunc = StoreKeyFunc ?? ((ctx, token) => token);

            var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                IssuedUtc = context.Ticket.Properties.IssuedUtc,
                ExpiresUtc = _configuration.ExpiresUtc
            };

            var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

            var key = StoreKeyFunc(context.Ticket, refreshToken);

            Store(key, refreshTokenTicket);

            context.SetToken(refreshToken);
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            TicketResult result = Remove(context);
            if (result.Deleted)
            {
                context.SetTicket(result.Ticket);
            }
        }

        private async Task<TicketResult> RemoveAsync(AuthenticationTokenReceiveContext context)
        {
            TicketResult result = new TicketResult();

            StoreKeyFunc = StoreKeyFunc ?? ((ctx, token) => token);
            string key = StoreKeyFunc(context.Ticket, context.Token);


            IDatabase database = _redis.GetDatabase(_configuration.Db);
            byte[] ticket = await database.StringGetAsync(key);

            if (ticket != null)
            {
                TicketSerializer serializer = new TicketSerializer();
                result.Ticket = serializer.Deserialize(ticket);
                result.Deleted = await database.KeyDeleteAsync(key);
            }
            else
            {
                await database.KeyDeleteAsync(context.Token);
            }

            return result;
        }

        private TicketResult Remove(AuthenticationTokenReceiveContext context)
        {
            TicketResult result = new TicketResult();

            StoreKeyFunc = StoreKeyFunc ?? ((ctx, token) => token);
            string key = StoreKeyFunc(context.Ticket, context.Token);

            IDatabase database = _redis.GetDatabase(_configuration.Db);
            byte[] ticket = database.StringGet(key);


            if (ticket.Length > default(int))
            {
                TicketSerializer serializer = new TicketSerializer();
                result.Ticket = serializer.Deserialize(ticket);
                result.Deleted = database.KeyDelete(key);
            }

            return result;
        }

        private async Task StoreAsync(string guid, AuthenticationTicket ticket)
        {
            TicketSerializer serializer = new TicketSerializer();
            byte[] serialize = serializer.Serialize(ticket);

            IDatabase database = _redis.GetDatabase(_configuration.Db);
            await database.StringSetAsync(guid, serialize, new TimeSpan(_configuration.ExpiresUtc.Ticks));
        }

        private void Store(string guid, AuthenticationTicket ticket)
        {
            TicketSerializer serializer = new TicketSerializer();
            byte[] serialize = serializer.Serialize(ticket);

            IDatabase database = _redis.GetDatabase(_configuration.Db);
            database.StringSet(guid, serialize, new TimeSpan(_configuration.ExpiresUtc.Ticks));
        }
    }
}