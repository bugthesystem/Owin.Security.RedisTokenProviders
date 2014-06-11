using System;

namespace Owin.Security.RedisTokenProviders
{
    public class ProviderConfiguration : IProviderConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public int Db { get; set; }
        public DateTimeOffset ExpiresUtc { get; set; }
    }
}