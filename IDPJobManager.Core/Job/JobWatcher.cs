using IDPJobManager.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace IDPJobManager.Core
{
    public class JobWatcher : IDisposable
    {
        private FileSystemWatcher watcher;
        private Timer timer;
        private List<string> changedJobs = new List<string>();
        private Action<List<string>> onChanged;
        public Action OnChangeCompleted;

        public bool IsStart { get; private set; }

        public JobWatcher(string jobDirectory, Action<List<string>> onChanged)
        {
            this.onChanged = onChanged;
            watcher = new FileSystemWatcher();
            watcher.Path = Path.IsPathRooted(jobDirectory) ? jobDirectory : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jobDirectory);
            watcher.Filter = "IDP*.dll";
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            watcher.Changed += new FileSystemEventHandler(this.JobWatcher_OnChanged);
            //watcher.Created += new FileSystemEventHandler(this.JobWatcher_OnChanged);
            //watcher.Deleted += new FileSystemEventHandler(this.JobWatcher_OnChanged);
            //watcher.Renamed += new RenamedEventHandler(this.JobWatcher_OnChanged);
            timer = new Timer(new TimerCallback(OnWatchedFileChange), null, -1, -1);
        }

        public void Start()
        {
            if (!IsStart)
            {
                watcher.EnableRaisingEvents = true;
                IsStart = true;
            }
        }

        private void OnWatchedFileChange(object state)
        {
            onChanged?.Invoke(changedJobs);
            OnChangeCompleted?.Invoke();
        }

        private void JobWatcher_OnChanged(object sender, FileSystemEventArgs e)
        {
            if (!changedJobs.Contains(e.FullPath))
                changedJobs.Add(e.FullPath);
            timer.Change(5000, -1);
        }

        public void Stop()
        {
            if(IsStart)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                timer.Dispose();
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
