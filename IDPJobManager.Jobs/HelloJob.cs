using Quartz;
using System;

namespace IDPJobManager.Jobs
{
    public class HelloJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine(DateTime.Now);
        }
    }
}
