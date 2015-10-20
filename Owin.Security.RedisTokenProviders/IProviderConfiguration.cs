using System;

namespace Owin.Security.RedisTokenProviders
{
    public interface IProviderConfiguration
    {
        string ConnectionString { get; set; }
        DateTimeOffset ExpiresUtc { get; set; }
        int Db { get; set; }
    }
}