namespace IDPJobManager.Web.Modules
{
    using Nancy;
    using Nancy.Security;
    using Configuration;
    using IDPJobManager.Web.Features;

    public abstract class BaseModule : NancyModule
    {
        private static readonly dynamic AuthInfo = ConfigProvider.GetInstance(IDPJobManagerStarter.Scheduler).AuthenticationInfo;

        protected BaseModule()
            : this(string.Empty)
        {

        }

        protected BaseModule(string modulePath) : base(modulePath)
        {
            if (AuthInfo?.Validate)
            {
                this.RequiresAuthentication();
            }
            After.AddItemToEndOfPipeline(NancyCompressionExtenstion.CheckForCompression);
        }
    }
}
