using Microsoft.Owin.StaticFiles.ContentTypes;

namespace IDPJobManager.Web
{
    public class ContentTypeProvider : FileExtensionContentTypeProvider
    {
        public ContentTypeProvider()
        {
            Mappings.Add(".json", "application/json");
            Mappings.Add(".log", "text/plain");
            Mappings.Add(".dll", "application/x-msdownload");
            Mappings.Add(".pdb", "application/x-msdownload");
            Mappings.Add(".config", "text/xml");
        }
    }
}
