using System;
using System.Threading.Tasks;

namespace IDPJobManager.Core
{
    public abstract class AspectableJob : BaseJob, IAspectableJob
    {
        public virtual bool BeforeExecute(JobExecutionContext context) { return true; }

        public virtual void AfterExecute(JobExecutionContext context) { }

        public abstract void DoExecute(JobExecutionContext context);

        public override void Execute(JobExecutionContext context)
        {
            try
            {
                if (BeforeExecute(context))
                {
                    Task.Run(() => DoExecute(context)).Wait();
                }
                AfterExecute(context);
            }
            catch (Exception e)
            {
                //Send mail?
            }
        }
    }
}
