using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AdminSetup.Startup))]
namespace AdminSetup
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
