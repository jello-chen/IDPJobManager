using System;
using System.Collections.Generic;

namespace IDPJobManager.Core
{
    public sealed class JobConfigurator
    {
        private static JobWatcher jobWatcher;

        /// <summary>
        /// Configure the job directory and register the changed callback
        /// </summary>
        /// <param name="jobDirectory"></param>
        /// <param name="onChanged"></param>
        public static void ConfigureAndWatch(string jobDirectory, Action<List<string>> onChanged)
        {
            if (jobWatcher != null)
                jobWatcher.Dispose();
            jobWatcher = new JobWatcher(jobDirectory, onChanged);
            jobWatcher.Start();
        }
    }
}
