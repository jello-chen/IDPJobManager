using IDPJobManager.Core;
using log4net;
using Quartz;
using System;

namespace IDPJobManager.Jobs
{
    public class HelloJob : DependableJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob));

        //public void Execute(IJobExecutionContext context)
        //{
        //    logger.Info($"[HelloJob]{DateTime.Now.ToString()}");
        //}

        public override void DoExecute(IJobExecutionContext context)
        {
            logger.Info($"[HelloJob]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob2 : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob2));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"[HelloJob2]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob3 : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob3));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"[HelloJob3]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob4 : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob4));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"[HelloJob4]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob5 : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob5));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"[HelloJob5]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob6 : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob6));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"[HelloJob6]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob7 : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob7));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"[HelloJob7]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob8 : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob8));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"[HelloJob8]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob9 : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob9));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"[HelloJob9]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob10 : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob10));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"[HelloJob10]{DateTime.Now.ToString()}");
        }
    }

    public class HelloJob11 : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(HelloJob11));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"[HelloJob11]{DateTime.Now.ToString()}");
        }
    }
}
