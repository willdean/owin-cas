using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OwinCasDemo.Startup))]
namespace OwinCasDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
