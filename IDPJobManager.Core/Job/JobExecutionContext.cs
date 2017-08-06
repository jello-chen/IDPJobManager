using System;

namespace IDPJobManager.Core
{
    [Serializable]
    public class JobExecutionContext : MarshalByRefObject
    {
        public string JobName { get; set; }
        public string JobGroup { get; set; }
    }
}
