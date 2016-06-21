using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Sudoku.Web.Startup))]
namespace Sudoku.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
