using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Unieboard_dev.Startup))]
namespace Unieboard_dev
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
