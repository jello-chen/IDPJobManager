namespace IDPJobManager.Web.Authentication
{
    using Nancy.Authentication.Basic;
    using Nancy.Security;
    using IDPJobManager.Core;

    public class IDPJobManagerUserValidator : IUserValidator
    {
        public IUserIdentity Validate(string username, string password)
        {
            var authInfo = Configuration.ConfigProvider.GetInstance(JobPoolManager.Scheduler).AuthenticationInfo;
            if (username == authInfo.User && password == authInfo.Password)
            {
                return new IDPJobManagerUserIdentity { UserName = username };
            }
            return null;
        }
    }
}
