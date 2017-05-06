using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core.Domain
{
    [Table("T_Job")]
    public class JobInfo
    {
        public Guid ID { get; set; }
        public virtual string JobName { get; set; }
        public virtual string JobParam { get; set; }
        public virtual string CronExpression { get; set; }
        public virtual string AssemblyName { get; set; }
        public virtual string ClassName { get; set; }
        public virtual int Status { get; set; }
        public virtual int IsDelete { get; set; }
        public virtual DateTime CreatedTime { get; set; }
        public virtual DateTime ModifyTime { get; set; }
        public virtual DateTime RecentRunTime { get; set; }
        public virtual DateTime NextFireTime { get; set; }
        public virtual string CronRemark { get; set; }
        public virtual string Remark { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime EndTime { get; set; }
    }
}
