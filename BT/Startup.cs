using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BT.Startup))]
namespace BT
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
