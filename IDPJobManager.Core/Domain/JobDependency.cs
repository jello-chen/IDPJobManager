using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core.Domain
{
    [Table("T_JobDependency")]
    public class JobDependency
    {
        public Guid ID { get; set; }
        public Guid JobID { get; set; }
        public Guid DependentJobID { get; set; }
    }
}
