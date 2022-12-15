using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AMSS.Startup))]
namespace AMSS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
