using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using IDPJobManager.Core.Extensions;
using IDPJobManager.Core.SchedulerProviders;
using Quartz.Impl.Matchers;
using IDPJobManager.Core.Utils;
using System.Linq;

namespace IDPJobManager.Core
{
    public class JobPoolManager : IDisposable
    {
        private static readonly ConcurrentDictionary<Guid, JobRuntimeInfo> JobRuntimePool =
            new ConcurrentDictionary<Guid, JobRuntimeInfo>();

        private static IScheduler scheduler;
        private static JobPoolManager instance;
        private static object syncLock = new object();

        private JobPoolManager() { }

        static JobPoolManager()
        {
            instance = new JobPoolManager();
            scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.ListenerManager.AddTriggerListener(new DefaultTriggerListener(), GroupMatcher<TriggerKey>.AnyGroup());
            scheduler.Start();
        }

        public static JobPoolManager Instance
        {
            get { return instance; }
        }

        public List<Guid> Keys { get { return new List<Guid>(JobRuntimePool.Keys); } }

        public static IScheduler Scheduler
        {
            get { return scheduler; }
        }

        public bool Add(Guid jobId, JobRuntimeInfo jobRuntimeInfo)
        {
            lock (syncLock)
            {
                if (!JobRuntimePool.ContainsKey(jobId))
                {
                    var jobInfo = jobRuntimeInfo.JobModel;
                    var jobName = jobInfo.JobName;
                    var jobGroup = string.IsNullOrWhiteSpace(jobInfo.JobGroup) ? null : jobInfo.JobGroup;
                    var jobKey = new JobKey(jobName, jobGroup);
                    if (JobRuntimePool.TryAdd(jobId, jobRuntimeInfo))
                    {
                        //Set job status to stop
                        if (JobOperator.UpdateJobStatus(jobInfo.ID, 0))
                            jobInfo.Status = 0;

                        IDictionary<string, object> jobData = new Dictionary<string, object>()
                        {
                            ["JobId"] = jobInfo.ID.ToString()
                        };

                        //Builds the job detail
                        var jobDetail = JobBuilder.Create<QuartzJob>()
                            .WithIdentity(jobKey)
                            .SetJobData(new JobDataMap(jobData))
                            .Build();

                        //Builds the job trigger
                        var jobTriggerBuilder = TriggerBuilder.Create()
                            .WithIdentity($"{jobName}Trigger", jobGroup)
                            .WithDescription($"{jobName} Trigger")
                            .WithCronSchedule(jobInfo.CronExpression,x => x.WithMisfireHandlingInstructionDoNothing());
                        //if (jobInfo.StartTime.HasValue)
                        //    jobTriggerBuilder.StartAt(jobInfo.StartTime.Value);
                        //if (jobInfo.EndTime.HasValue)
                        //    jobTriggerBuilder.EndAt(jobInfo.EndTime.Value);
                        var jobTrigger = jobTriggerBuilder.Build();

                        //Decides whether to schedule the job by status
                        scheduler.ScheduleJob(jobDetail, jobTrigger);
                        //if (jobInfo.Status == 0) scheduler.PauseJob(jobKey);
                        return true;
                    }
                }
                return false;
            }
        }

        public JobRuntimeInfo Get(Guid jobId)
        {
            if (!JobRuntimePool.ContainsKey(jobId))
            {
                return null;
            }
            lock (syncLock)
            {
                if (JobRuntimePool.ContainsKey(jobId))
                {
                    JobRuntimeInfo jobRuntimeInfo = null;
                    JobRuntimePool.TryGetValue(jobId, out jobRuntimeInfo);
                    return jobRuntimeInfo;
                }
                return null;
            }
        }

        public bool Remove(Guid jobId)
        {
            lock (syncLock)
            {
                if (JobRuntimePool.ContainsKey(jobId))
                {
                    JobRuntimeInfo jobRuntimeInfo = null;
                    JobRuntimePool.TryGetValue(jobId, out jobRuntimeInfo);
                    if (jobRuntimeInfo != null)
                    {
                        var tiggerKey = new TriggerKey(jobRuntimeInfo.JobModel.JobName,
                            jobRuntimeInfo.JobModel.JobGroup);
                        scheduler.PauseTrigger(tiggerKey);
                        scheduler.UnscheduleJob(tiggerKey);
                        scheduler.DeleteJob(new JobKey(jobRuntimeInfo.JobModel.JobName,
                            jobRuntimeInfo.JobModel.JobGroup));
                        JobRuntimePool.TryRemove(jobId, out jobRuntimeInfo);
                        JobOperator.UpdateJobStatus(jobRuntimeInfo.JobModel.ID, 0);
                        if (!JobRuntimePool.Any(p => p.Value.AppDomain == jobRuntimeInfo.AppDomain))
                            AppDomainLoader<BaseJob>.UnLoad(jobRuntimeInfo.AppDomain);
                        return true;
                    }
                }
                return false;
            }
        }

        public bool RemoveAll(List<Guid> jboIds)
        {
            foreach (var jobId in jboIds)
            {
                JobOperator.UpdateJobStatus(jobId, 0);
                Remove(jobId);
            }
            return true;
        }

        public virtual void Dispose()
        {
            if (scheduler != null && !scheduler.IsShutdown)
            {
                foreach (var jobId in JobRuntimePool.Keys)
                {
                    JobOperator.UpdateJobStatus(jobId, 0);
                }
                scheduler.Shutdown();
            }
        }
    }
}
