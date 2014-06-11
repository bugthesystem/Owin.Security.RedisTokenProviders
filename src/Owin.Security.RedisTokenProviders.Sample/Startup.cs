using Microsoft.Owin;
using Owin.Security.RedisTokenProviders.Sample;

[assembly: OwinStartup(typeof(Startup))]

namespace Owin.Security.RedisTokenProviders.Sample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
