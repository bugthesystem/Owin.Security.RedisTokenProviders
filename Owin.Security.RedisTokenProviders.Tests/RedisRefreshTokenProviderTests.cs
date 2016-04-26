using System;
using Common.Testing.NUnit;
using FluentAssertions;
using NUnit.Framework;

namespace Owin.Security.RedisTokenProviders.Tests
{
    public class RedisRefreshTokenProviderTests : TestBase
    {
        private RedisRefreshTokenProvider _provider;

        [Test]
        public void should_create_instance()
        {
            _provider = new RedisRefreshTokenProvider(new ProviderConfiguration
            {
                ConnectionString = "localhost:6379",
                ExpiresUtc = DateTime.UtcNow.AddYears(1),
                Db = 0,
                AbortOnConnectFail = false
            });

            _provider.Should().NotBeNull();
        }
    }
}
