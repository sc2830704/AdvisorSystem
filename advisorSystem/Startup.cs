using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(advisorSystem.Startup))]
namespace advisorSystem
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
