using Microsoft.Owin.StaticFiles.ContentTypes;

namespace IDPJobManager.Web
{
    public class JsonContentTypeProvider : FileExtensionContentTypeProvider
    {
        public JsonContentTypeProvider()
        {
            Mappings.Add(".json", "application/json");
        }
    }
}
