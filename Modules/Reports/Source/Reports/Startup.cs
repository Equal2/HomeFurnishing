using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Reports.Startup))]
namespace Reports
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
