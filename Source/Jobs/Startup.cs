using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Jobs.Startup))]
namespace Jobs
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
