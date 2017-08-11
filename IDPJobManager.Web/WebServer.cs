using IDPJobManager.Core;
using IDPJobManager.Core.Utils;
using IDPJobManager.Web.Configuration;
using Microsoft.Owin.Hosting;
using System;
using IDPJobManager.Core.Extensions;
using Microsoft.AspNet.SignalR;

namespace IDPJobManager.Web
{
    public class WebServer
    {
        private IDisposable _webapp;
        private JobWatcher jobWatcher;

        public WebServer(JobWatcher jobWatcher)
        {
            Ensure.Requires<ArgumentNullException>(jobWatcher != null, "`jobWatcher` should not be null.");
            jobWatcher.OnChangeCompleted = Reload;
            this.jobWatcher = jobWatcher;
        }

        public void Start()
        {
            var url = IDPJobManagerConfiguration.Config.Provider.Uri;
            Ensure.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(url), "`url` should not be null.");
            _webapp = WebApp.Start<Startup>(url);
            jobWatcher.Start();
            Logger.Instance.Info($"Web host started on {url}.");
            JobPoolManager.Scheduler.StartRunning().ScheduleJobs();
        }

        private void Reload()
        {
            GlobalHost.ConnectionManager.GetHubContext<JobHub>()?.Clients.All.Reload();
        }

        public void Stop()
        {
            jobWatcher.Stop();
            _webapp?.Dispose();
        }
    }
}
