using Nancy.ViewEngines.Razor;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace IDPJobManager.Web.Configuration
{
    [Export(typeof(IRazorConfiguration))]
    public class RazorConfiguration : IRazorConfiguration
    {
        public bool AutoIncludeModelNamespace => true;

        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "IDPJobManager.Web";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "IDPJobManager.Web";
        }
    }
}
