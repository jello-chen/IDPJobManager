using System;

namespace IDPJobManager.Core
{
    [Serializable]
    public abstract class BaseJob : MarshalByRefObject
    {
        static BaseJob()
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.config"));
            Config.GlobalConfiguration.ConfigureDBConnectionString();
        }

        public abstract void Execute(JobExecutionContext context);


        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
