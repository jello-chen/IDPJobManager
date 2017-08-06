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
        private bool isStart;
        private List<string> changedJobs = new List<string>();
        private Action<List<string>> onChanged;

        public bool IsStart { get { return isStart; } }

        public JobWatcher(string jobDirectory, Action<List<string>> onChanged)
        {
            this.onChanged = onChanged;
            watcher = new FileSystemWatcher();
            watcher.Path = Path.IsPathRooted(jobDirectory) ? jobDirectory : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jobDirectory);
            watcher.Filter = "IDP*.dll";
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            watcher.Changed += new FileSystemEventHandler(this.JobWatcher_OnChanged);
            watcher.Created += new FileSystemEventHandler(this.JobWatcher_OnChanged);
            watcher.Deleted += new FileSystemEventHandler(this.JobWatcher_OnChanged);
            watcher.Renamed += new RenamedEventHandler(this.JobWatcher_OnChanged);
            timer = new Timer(new TimerCallback(OnWatchedFileChange), null, -1, -1);
        }

        public void Start()
        {
            if (!isStart)
            {
                watcher.EnableRaisingEvents = true;
                isStart = true;
            }
        }

        private void OnWatchedFileChange(object state)
        {
            onChanged?.Invoke(changedJobs);
            //var e = (FileSystemEventArgs)state;
            //Console.WriteLine($"[{DateTime.Now}]{e.ChangeType}，{e.FullPath},{e.Name}");
            //if (JobPoolManager.Instance.Remove(1))
            //{
            //    var job1 = new JobInfo
            //    {
            //        JobId = 1,
            //        JobName = "Job1",
            //        Group = "MyGroup",
            //        TaskCron = "0/2 * * * * ?"
            //    };
            //    var jobDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Jobs", "Job1.dll");
            //    var jobClassPath = "Job1.Job1";
            //    var appdomain = default(AppDomain);
            //    var job = AppDomainLoader.Load(jobDllPath, jobClassPath, out appdomain);
            //    var rt = new JobRuntimeInfo
            //    {
            //        AppDomain = appdomain,
            //        Job = job,
            //        JobModel = job1
            //    };
            //    JobPoolManager.Instance.Add(job1.JobId, rt);
            //}
        }

        private void JobWatcher_OnChanged(object sender, FileSystemEventArgs e)
        {
            if (!changedJobs.Contains(e.FullPath))
                changedJobs.Add(e.FullPath);
            timer.Change(5000, -1);
        }

        public void Dispose()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            timer.Dispose();
        }
    }
}
