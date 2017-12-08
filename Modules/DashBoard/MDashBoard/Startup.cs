using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MDashBoard.Startup))]
namespace MDashBoard
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
