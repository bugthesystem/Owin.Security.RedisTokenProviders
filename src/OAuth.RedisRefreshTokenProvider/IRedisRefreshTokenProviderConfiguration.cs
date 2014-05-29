using System;

namespace OAuth.RedisRefreshTokenProvider
{
    public interface IRedisRefreshTokenProviderConfiguration
    {
        string Host { get; set; }
        int Port { get; set; }
        int Db { get; set; }
        DateTimeOffset ExpiresUtc { get; set; }
    }
}