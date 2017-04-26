namespace IDPJobManager.Web.Modules
{
    using Nancy;

    public class ApiModule : NancyModule
    {
        public ApiModule() : base("/Api")
        {
            Get["/"] = _ => "Api";
        }
    }
}
