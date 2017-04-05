namespace IDPJobManager.Web.Modules
{

    using Nancy;
    using Models;
    using Nancy.ModelBinding;

    public class AccountModule : NancyModule
    {
        public AccountModule() : base("/Account")
        {

            Get["/Login"] = _ =>
            {
                return View["Account/Login"];
            };

            Get["/LoginOut"] = _ =>
            {
                Session["LoginUser"] = null;
                return View["Account/Login"];
            };

            Post["/Login"] = parameters =>
            {
                var loginModel = this.Bind<LoginModel>();
                Session["LoginUser"] = loginModel;
                return View["Home/Index"];
            };
        }
    }
}
