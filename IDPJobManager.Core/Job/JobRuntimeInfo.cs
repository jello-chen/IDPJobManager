using IDPJobManager.Core.Domain;
using System;

namespace IDPJobManager.Core
{
    public class JobRuntimeInfo
    {
        public AppDomain AppDomain;
        public BaseJob Job { get; set; }

        public JobInfo JobModel { get; set; }
    }
}
