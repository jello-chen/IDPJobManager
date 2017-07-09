using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IDPJobManager.Core.Domain
{
    [Table("T_Job")]
    public partial class JobInfo
    {
        public Guid ID { get; set; }

        [StringLength(300)]
        public string JobName { get; set; }

        public string JobGroup { get; set; }

        public string JobParam { get; set; }

        [StringLength(200)]
        public string CronExpression { get; set; }

        [StringLength(150)]
        public string AssemblyName { get; set; }

        [StringLength(150)]
        public string ClassName { get; set; }

        public int? Status { get; set; }

        public int? IsDelete { get; set; }

        public DateTime? CreatedTime { get; set; }

        public DateTime? ModifyTime { get; set; }

        public DateTime? RecentRunTime { get; set; }

        public DateTime? NextFireTime { get; set; }

        [StringLength(300)]
        public string CronRemark { get; set; }

        [StringLength(1000)]
        public string Remark { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? StartTime { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? EndTime { get; set; }

        [NotMapped]
        public string StartTimeString { get { return StartTime?.ToString("yyyy-MM-dd HH:mm:ss"); } }

        [NotMapped]
        public string EndTimeString { get { return EndTime?.ToString("yyyy-MM-dd HH:mm:ss"); } }
    }
}
