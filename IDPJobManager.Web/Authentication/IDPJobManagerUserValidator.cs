namespace IDPJobManager.Web.Authentication
{
    using Nancy.Authentication.Basic;
    using Nancy.Security;

    public class IDPJobManagerUserValidator : IUserValidator
    {
        public IUserIdentity Validate(string username, string password)
        {
            var authInfo = Configuration.ConfigProvider.GetInstance(IDPJobManagerStarter.Scheduler).AuthenticationInfo;
            if (username == authInfo.User && password == authInfo.Password)
            {
                return new IDPJobManagerUserIdentity { UserName = username };
            }

            return null;
        }
    }
}
