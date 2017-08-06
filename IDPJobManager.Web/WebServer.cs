using IDPJobManager.Core;
using IDPJobManager.Core.Utils;
using IDPJobManager.Web.Configuration;
using Microsoft.Owin.Hosting;
using System;
using IDPJobManager.Core.Extensions;

namespace IDPJobManager.Web
{
    public class WebServer
    {
        private IDisposable _webapp;
        private JobWatcher jobWatcher;

        public WebServer(JobWatcher jobWatcher)
        {
            Ensure.Requires<ArgumentNullException>(jobWatcher != null, "`jobWatcher` should not be null.");
            this.jobWatcher = jobWatcher;
        }

        public void Start()
        {
            var url = IDPJobManagerConfiguration.Config.Provider.Uri;
            Ensure.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(url), "`url` should not be null.");
            _webapp = WebApp.Start<Startup>(url);
            jobWatcher.Start();
            Console.WriteLine($"Web host started on {url}.");
            JobPoolManager.Scheduler.StartRunning().ScheduleJobs();
        }

        public void Stop()
        {
            jobWatcher.Stop();
            _webapp?.Dispose();
        }
    }
}
