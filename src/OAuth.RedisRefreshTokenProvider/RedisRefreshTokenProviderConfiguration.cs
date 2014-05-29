using System;

namespace OAuth.RedisRefreshTokenProvider
{
    public class RedisRefreshTokenProviderConfiguration : IRedisRefreshTokenProviderConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public int Db { get; set; }
        public DateTimeOffset ExpiresUtc { get; set; }
    }
}