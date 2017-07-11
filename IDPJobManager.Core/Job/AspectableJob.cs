using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using IDPJobManager.Core.Utils;

namespace IDPJobManager.Core
{
    public abstract class AspectableJob : IAspectableJob
    {
        public virtual bool BeforeExecute(IJobExecutionContext context) { return true; }

        public virtual void AfterExecute(IJobExecutionContext context) { }

        public abstract void DoExecute(IJobExecutionContext context);

        public void Execute(IJobExecutionContext context)
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
