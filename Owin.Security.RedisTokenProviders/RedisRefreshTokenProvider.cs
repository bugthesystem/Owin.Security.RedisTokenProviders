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
        private readonly Lazy<IConnectionMultiplexer> _multiplexer;

        [Obsolete("Renamed to RedisKeyGenerator, use it instead", true)]
        public Func<AuthenticationTicket, string, string> StoreKeyFunc { get; set; }

        public Func<AuthenticationTicket, string, string> RedisKeyGenerator { get; set; }
        public Func<string> RefreshTokenGenerator { get; set; }

        private IDatabase Db => _multiplexer.Value.GetDatabase(_configuration.Db);

        public RedisRefreshTokenProvider(IProviderConfiguration configuration)
        {
            _configuration = configuration ?? new ProviderConfiguration
            {
                ConnectionString = "localhost:6379",
                ExpiresUtc = DateTime.UtcNow.AddYears(1),
                Db = 0,
                AbortOnConnectFail = true
            };

            var options = ConfigurationOptions.Parse(_configuration.ConnectionString);
            options.AbortOnConnectFail = _configuration.AbortOnConnectFail;

            _multiplexer = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));

        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            RefreshTokenGenerator = RefreshTokenGenerator ?? (() => Guid.NewGuid().ToString());
            var refreshToken = RefreshTokenGenerator();
            RedisKeyGenerator = RedisKeyGenerator ?? ((ctx, token) => token);

            var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                IssuedUtc = context.Ticket.Properties.IssuedUtc,
                ExpiresUtc = _configuration.ExpiresUtc
            };

            var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

            var key = RedisKeyGenerator(context.Ticket, refreshToken);

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
            RedisKeyGenerator = RedisKeyGenerator ?? ((ctx, token) => token);

            var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                IssuedUtc = context.Ticket.Properties.IssuedUtc,
                ExpiresUtc = _configuration.ExpiresUtc
            };

            var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

            var key = RedisKeyGenerator(context.Ticket, refreshToken);

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

            RedisKeyGenerator = RedisKeyGenerator ?? ((ctx, token) => token);
            string key = RedisKeyGenerator(context.Ticket, context.Token);

            byte[] ticket = await Db.StringGetAsync(key);

            if (ticket != null)
            {
                TicketSerializer serializer = new TicketSerializer();
                result.Ticket = serializer.Deserialize(ticket);
                result.Deleted = await Db.KeyDeleteAsync(key);
            }
            else
            {
                await Db.KeyDeleteAsync(context.Token);
            }

            return result;
        }

        private TicketResult Remove(AuthenticationTokenReceiveContext context)
        {
            TicketResult result = new TicketResult();

            RedisKeyGenerator = RedisKeyGenerator ?? ((ctx, token) => token);
            string key = RedisKeyGenerator(context.Ticket, context.Token);

            byte[] ticket = Db.StringGet(key);


            if (ticket.Length > default(int))
            {
                TicketSerializer serializer = new TicketSerializer();
                result.Ticket = serializer.Deserialize(ticket);
                result.Deleted = Db.KeyDelete(key);
            }

            return result;
        }

        private async Task StoreAsync(string guid, AuthenticationTicket ticket)
        {
            TicketSerializer serializer = new TicketSerializer();
            byte[] serialize = serializer.Serialize(ticket);

            await Db.StringSetAsync(guid, serialize, _configuration.ExpiresUtc.TimeOfDay);
        }

        private void Store(string guid, AuthenticationTicket ticket)
        {
            TicketSerializer serializer = new TicketSerializer();
            byte[] serialize = serializer.Serialize(ticket);

            Db.StringSet(guid, serialize, _configuration.ExpiresUtc.TimeOfDay);
        }
    }
}