using Nancy.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using System.Reflection;
using System.IO;
using Nancy.IO;

namespace IDPJobManager.Web.ErrorHandling
{
    public class NotFoundHandler : IStatusCodeHandler
    {
        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            context.Response.ContentType = "text/html";
            context.Response.Contents = s =>
            {
                using (var writer = new StreamWriter(new UnclosableStreamWrapper(s), Encoding.UTF8))
                {
                    writer.Write(LoadResource("404.html"));
                }
            };
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.NotFound;
        }

        private static string LoadResource(string filename)
        {
            var resourceStream = Assembly.GetCallingAssembly().GetManifestResourceStream(string.Format("IDPJobManager.Web.ErrorHandling.Resources.{0}", filename));

            if (resourceStream == null)
            {
                return string.Empty;
            }

            using (var reader = new StreamReader(resourceStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
