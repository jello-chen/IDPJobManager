using IDPJobManager.Core.Config;
using IDPJobManager.Core.Domain;
using System.Data.Entity;

namespace IDPJobManager.Core
{
    public partial class IDPJobManagerDataContext : DbContext
    {
        public IDPJobManagerDataContext() 
            : base(GlobalConfiguration.Name)
        {

        }

        public virtual DbSet<JobInfo> T_Job { get; set; }

        public virtual DbSet<JobDependency> T_JobDependency { get; set; }

        public void FixEfProviderServicesProblem()
        {
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }
    }
}
