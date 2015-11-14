using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC5Play.Startup))]
namespace MVC5Play
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
