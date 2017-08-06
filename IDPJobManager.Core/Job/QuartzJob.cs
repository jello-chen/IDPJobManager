using System;
using Quartz;
using log4net;

namespace IDPJobManager.Core
{
    public class QuartzJob : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(QuartzJob));

        public void Execute(IJobExecutionContext context)
        {
            var jobId = context.JobDetail.JobDataMap.GetString("JobId");
            var jobRuntimeInfo = JobPoolManager.Instance.Get(Guid.Parse(jobId));
            try
            {
                jobRuntimeInfo.Job.Execute(new JobExecutionContext { JobName = context.JobDetail.Key.Name, JobGroup = context.JobDetail.Key.Group });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
    }
}
