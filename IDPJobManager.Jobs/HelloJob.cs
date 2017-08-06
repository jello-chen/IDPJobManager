using IDPJobManager.Core;
using log4net;
using System;

namespace IDPJobManager.Jobs
{
    public class HelloJob : DependableJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob));

        public override void DoExecute(JobExecutionContext context)
        {
            logger.Info($"[HelloJob]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob2 : BaseJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob2));

        public override void Execute(JobExecutionContext context)
        {
            logger.Info($"[HelloJob2]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob3 : BaseJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob3));

        public override void Execute(JobExecutionContext context)
        {
            logger.Info($"[HelloJob3]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob4 : DependableJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob4));

        public override void DoExecute(JobExecutionContext context)
        {
            logger.Info($"[HelloJob4]{DateTime.Now.ToString()}");
            //throw new InvalidCastException("`HelloJob4` raises some errors");
        }
    }

    public class HelloJob5 : BaseJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob5));

        public override void Execute(JobExecutionContext context)
        {
            logger.Info($"[HelloJob5]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob6 : BaseJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob6));

        public override void Execute(JobExecutionContext context)
        {
            logger.Info($"[HelloJob6]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob7 : BaseJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob7));

        public override void Execute(JobExecutionContext context)
        {
            logger.Info($"[HelloJob7]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob8 : BaseJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob8));

        public override void Execute(JobExecutionContext context)
        {
            logger.Info($"[HelloJob8]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob9 : BaseJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob9));

        public override void Execute(JobExecutionContext context)
        {
            logger.Info($"[HelloJob9]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob10 : BaseJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob10));

        public override void Execute(JobExecutionContext context)
        {
            logger.Info($"[HelloJob10123]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob11 : BaseJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob11));

        public override void Execute(JobExecutionContext context)
        {
            logger.Info($"[HelloJob11]{DateTime.Now.ToString()}");
        }
    }
}
