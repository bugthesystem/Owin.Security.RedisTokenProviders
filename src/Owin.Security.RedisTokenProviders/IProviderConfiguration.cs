using System;

namespace Owin.Security.RedisTokenProviders
{
    public interface IProviderConfiguration
    {
        string Host { get; set; }
        int Port { get; set; }
        int Db { get; set; }
        DateTimeOffset ExpiresUtc { get; set; }
    }
}