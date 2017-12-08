using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TaskManagement.Startup))]
namespace TaskManagement
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
