using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Customize.Startup))]
namespace Customize
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
