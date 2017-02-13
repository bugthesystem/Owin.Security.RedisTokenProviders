OAuth.RedisRefreshTokenProvider
================================================================

A RefreshTokenProvider using Redis as token store.

**To install Owin.Security.RedisTokenProviders**

```csharp
Install-Package Owin.Security.RedisTokenProviders
```


**Set RefreshTokenProvider property of OAuthAuthorizationServerOptions**

```csharp
 OAuthOptions = new OAuthAuthorizationServerOptions
 {
    //Other configurations
    
    RefreshTokenProvider = new RedisRefreshTokenProvider(new ProviderConfiguration
    {
       Db = 0,
       ExpiresUtc = DateTime.UtcNow.AddYears(1),
       Port = 6379,
       Host = "localhost",
       AbortOnConnectFail =true //Optional (default:true)
    })
    {
       RedisKeyGenerator = (authenticationTicket, token) => token, // Optional
       RefreshTokenGenerator = () => new Guid().ToString()         // Optional
    }
 };

```

Open Source Projects in use
---------------------
* [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) by StackExchange
