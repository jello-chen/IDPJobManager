namespace IDPJobManager.Web.Authentication
{
    using System.Collections.Generic;
    using Nancy.Security;

    public class IDPJobManagerUserIdentity : IUserIdentity
    {
        public string UserName { get; set; }

        public IEnumerable<string> Claims { get; set; }
    }
}
