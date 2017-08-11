using IDPJobManager.Core;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Nancy.Owin;
using Owin;
using System.Web.Http;

namespace IDPJobManager.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Enable CORS
            app.UseCors(CorsOptions.AllowAll);

            //Enable SignalR
            app.MapSignalR();

            // Configure Web API for self-host and use it. 
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            app.UseWebApi(config);

            // File Server
            var fileOptions = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = new PhysicalFileSystem("Jobs"),
                StaticFileOptions = { ContentTypeProvider = new ContentTypeProvider() },
                RequestPath = new Microsoft.Owin.PathString("/Directory")
            };
            app.UseFileServer(fileOptions);

            // Nancy
            var nancyOptions = new NancyOptions
            {
                Bootstrapper = new IDPJobManagerBootstrapper(JobPoolManager.Scheduler)
            };
            app.UseNancy(nancyOptions);
        }
    }
}
