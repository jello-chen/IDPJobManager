namespace IDPJobManager.Web.Modules
{
    using Nancy;
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using System.Reflection;

    [Export(typeof(INancyModule))]
    public class ApiModule : NancyModule
    {
        [ImportingConstructor]
        public ApiModule(CompositionContainer container) : base("/Api")
        {
            Get["/"] = _ => "Api";
            Get["/List"] = _ =>
            {
                return "list";
            };
        }
    }
}
