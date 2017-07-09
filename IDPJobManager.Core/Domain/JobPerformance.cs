using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core.Domain
{
    [Table("T_JobPerformance")]
    public class JobPerformance
    {
        public Guid ID { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? Interval { get; set; }
        public decimal? CPU { get; set; }
        public decimal? Memory { get; set; }
        public DateTime CreateTime { get; set; }

        [NotMapped]
        public string StartTimeString { get { return StartTime?.ToString("yyyy-MM-dd HH:mm:ss.fff"); } }

        [NotMapped]
        public string EndTimeString { get { return EndTime?.ToString("yyyy-MM-dd HH:mm:ss.fff"); } }

        [NotMapped]
        public string CreateTimeString { get { return CreateTime.ToString("yyyy-MM-dd HH:mm:ss"); } }

        [NotMapped]
        public string ElapsedTime { get { return EndTime.Value.Subtract(StartTime.Value).ToString(); } }
    }
}
