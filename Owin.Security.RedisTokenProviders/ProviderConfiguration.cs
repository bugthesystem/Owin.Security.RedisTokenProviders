using System;

namespace Owin.Security.RedisTokenProviders
{
    public class ProviderConfiguration : IProviderConfiguration
    {
        public string ConnectionString { get; set; }
        public DateTimeOffset ExpiresUtc { get; set; }
        public int Db { get; set; }
        public bool AbortOnConnectFail { get; set; }
    }
}