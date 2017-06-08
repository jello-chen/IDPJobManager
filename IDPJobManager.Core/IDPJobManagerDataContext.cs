using IDPJobManager.Core.Config;
using IDPJobManager.Core.Domain;
using System.ComponentModel.Composition;
using System.Data.Entity;

namespace IDPJobManager.Core
{
    [Export]
    public partial class IDPJobManagerDataContext : DbContext
    {
        public IDPJobManagerDataContext() 
            : base(GlobalConfiguration.Name)
        {

        }

        public virtual DbSet<JobInfo> T_Job { get; set; }

        public void FixEfProviderServicesProblem()
        {
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }
    }
}
