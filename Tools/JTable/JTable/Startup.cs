using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(JTable.Startup))]
namespace JTable
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
