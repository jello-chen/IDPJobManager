using System;
using IDPJobManager.Core.Utils;

namespace IDPJobManager.Core.Job
{
    public abstract class AggregateJob : AspectableJob
    {
        private readonly AspectableJob[] jobs;

        public AggregateJob(params AspectableJob[] jobs)
        {
            Ensure.Requires<ArgumentNullException>(jobs != null);
            this.jobs = jobs;
        }

        public override bool BeforeExecute(JobExecutionContext context)
        {
            var beExecuted = true;
            foreach (var job in jobs)
            {
                beExecuted = job.BeforeExecute(context);
            }
            return beExecuted;
        }
    }
}
