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
                EmailHelper emailHelper = new EmailHelper();
                emailHelper.From = "949908998@qq.com";
                emailHelper.To = new string[] { "chenjinliang3@huawei.com" };
                emailHelper.Subject = $"{this.GetType().FullName} error";
                emailHelper.Body = e.Message;
                emailHelper.FromEmailPassword = "edvgeqwasswhbefi";//Here input authorization code after openning POP3/SMTP service when QQ mail
                emailHelper.Host = "smtp.qq.com";
                emailHelper.Port = 25;
                emailHelper.Send();
            }
        }
    }
}
