using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ProcessManagement.Startup))]
namespace ProcessManagement
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
