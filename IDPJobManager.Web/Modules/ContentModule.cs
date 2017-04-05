namespace IDPJobManager.Web.Modules
{
    using System;
    using Nancy;

    public class ContentModule : NancyModule
    {
        public ContentModule()
            : base()
        {
            Get["/Styles/{file}"] = p =>
            {
                const string type = FileType.Style;
                var file = (string)p.file;
                return new ContentResponse(type, file);
            };
            Get["/Images/{file}"] = p =>
            {
                const string type = FileType.Image;
                var file = (string)p.file;
                return new ContentResponse(type, file);
            };
            Get["/Scripts/{file}"] = p =>
            {
                const string type = FileType.Script;
                var file = (string)p.file;
                return new ContentResponse(type, file);
            };
            Get["/Fonts/{file}"] = p =>
            {
                const string type = FileType.Font;
                var file = (string)p.file;
                return new ContentResponse(type, file);
            };
        }
    }

    internal static class FileType
    {
        internal const string Style = "style";
        internal const string Image = "image";
        internal const string Script = "script";
        internal const string Font = "font";
    }

    internal class ContentResponse : Response
    {
        internal ContentResponse(string type, string file)
        {
            ContentType = GetContentType(type);
            Contents = s =>
            {
                using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(GetEmbeddedResourceFullName(type, file)))
                {
                    if (stream != null)
                    {
                        var bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        stream.Seek(0, System.IO.SeekOrigin.Begin);
                        s.Write(bytes, 0, bytes.Length);
                    }
                    s.Flush();
                }
            };
        }

        private static string GetContentType(string type)
        {
            if (type.Equals(FileType.Style, StringComparison.InvariantCultureIgnoreCase))
            {
                return "text/css";
            }
            else if (type.Equals(FileType.Image, StringComparison.InvariantCultureIgnoreCase))
            {
                return "image/png";
            }
            else if (type.Equals(FileType.Script, StringComparison.InvariantCultureIgnoreCase))
            {
                return "text/javascript";
            }
            else if (type.Equals(FileType.Font, StringComparison.InvariantCultureIgnoreCase))
            {
                return "text/html";
            }
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("Invalid file type");
        }

        private static string GetEmbeddedResourceFullName(string type, string file)
        {
            if (type.Equals(FileType.Style, StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Concat("IDPJobManager.Web.Content.Styles.", file);
            }
            else if (type.Equals(FileType.Image, StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Concat("IDPJobManager.Web.Content.Images.", file);
            }
            else if (type.Equals(FileType.Script, StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Concat("IDPJobManager.Web.Content.Scripts.", file);
            }
            else if (type.Equals(FileType.Font, StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Concat("IDPJobManager.Web.Content.Fonts.", file);
            }
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("Invalid file type");
        }
    }
}
