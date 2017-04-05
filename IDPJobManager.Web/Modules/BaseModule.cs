namespace IDPJobManager.Web.Modules
{
    using Nancy;
    using Nancy.Security;
    using Configuration;

    public abstract class BaseModule : NancyModule
    {
        private static readonly dynamic AuthInfo = ConfigProvider.GetInstance(IDPJobManagerStarter.Scheduler).AuthenticationInfo;

        protected BaseModule()
            : base()
        {
            if (AuthInfo.Validate)
            {
                this.RequiresAuthentication();
            }
        }
    }
}
