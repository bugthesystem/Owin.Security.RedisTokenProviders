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
        private ConnectionMultiplexer _redis;

        public RedisRefreshTokenProvider(IProviderConfiguration configuration)
        {
            _configuration = configuration ?? new ProviderConfiguration { ConnectionString = "localhost:6379", ExpiresUtc = DateTime.UtcNow.AddYears(1), Db=0 };
        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var refreshToken = Guid.NewGuid().ToString();

            var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                IssuedUtc = context.Ticket.Properties.IssuedUtc,
                ExpiresUtc = _configuration.ExpiresUtc
            };
            var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

            await StoreAsync(refreshToken, refreshTokenTicket);

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

            // maybe only create a handle the first time, then re-use for same client
            // copy properties and set the desired lifetime of refresh token
            var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                IssuedUtc = context.Ticket.Properties.IssuedUtc,
                ExpiresUtc = _configuration.ExpiresUtc
            };
            var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

            Store(refreshToken, refreshTokenTicket);
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
            _redis = await ConnectionMultiplexer.ConnectAsync(_configuration.ConnectionString);
            IDatabase database = _redis.GetDatabase(_configuration.Db);
            byte[] ticket = await database.StringGetAsync(context.Token);
            if (ticket != null)
            {
                TicketSerializer serializer = new TicketSerializer();
                result.Ticket = serializer.Deserialize(ticket);
                result.Deleted = await database.KeyDeleteAsync(context.Token);
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
            _redis = ConnectionMultiplexer.Connect(_configuration.ConnectionString);
            IDatabase database = _redis.GetDatabase(_configuration.Db);
            byte[] ticket = database.StringGet(context.Token);

            if (ticket.Length > default(int))
            {
                TicketSerializer serializer = new TicketSerializer();
                result.Ticket = serializer.Deserialize(ticket);
                result.Deleted = database.KeyDelete(context.Token);
            }

            return result;
        }

        private async Task StoreAsync(string guid, AuthenticationTicket ticket)
        {
            TicketSerializer serializer = new TicketSerializer();
            byte[] serialize = serializer.Serialize(ticket);
            _redis = await ConnectionMultiplexer.ConnectAsync(_configuration.ConnectionString);
            IDatabase database = _redis.GetDatabase(_configuration.Db);
            await database.StringSetAsync(guid, serialize);
        }

        private void Store(string guid, AuthenticationTicket ticket)
        {
            TicketSerializer serializer = new TicketSerializer();
            byte[] serialize = serializer.Serialize(ticket);
            _redis = ConnectionMultiplexer.Connect(_configuration.ConnectionString);
            IDatabase database = _redis.GetDatabase(_configuration.Db);
            database.StringSet(guid, serialize);
        }
    }
}