using IDPJobManager.Core;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Nancy.Owin;
using Owin;

namespace IDPJobManager.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
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
