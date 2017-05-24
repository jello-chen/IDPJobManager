using log4net;
using Quartz;
using System;

namespace IDPJobManager.Jobs
{
    public class HelloJob : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info(DateTime.Now.ToString());
        }
    }
}
