﻿using IDPJobManager.Core.Domain;
using IDPJobManager.Core.Utils;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IDPJobManager.Core.Extensions
{
    public static class QuartzExtension
    {

        public static IScheduler StartRunning(this IScheduler scheduler, TimeSpan? delay = null)
        {
            Ensure.Requires<ArgumentNullException>(scheduler != null, "sheduler should not be null.");
            if (!scheduler.IsStarted)
            {
                if (delay == null)
                    scheduler.Start();
                else
                    scheduler.StartDelayed(delay.Value);
            }
            return scheduler;
        }

        public static bool ScheduleJob(this IScheduler scheduler, JobInfo jobInfo)
        {
            Ensure.Requires<ArgumentNullException>(scheduler != null, "sheduler should not be null.");

            var jobIdentity = jobInfo.ID.ToString();
            var jobKey = new JobKey(jobIdentity);
            if (!scheduler.CheckExists(jobKey))
            {
                var assemblyScanner = new AssemblyScanner(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Jobs"));
                var jobType = assemblyScanner.GetType(jobInfo.AssemblyName, jobInfo.ClassName);
                var jobDetail = new JobDetailImpl(jobIdentity, jobType);
                var jobTrigger = new CronTriggerImpl();
                jobTrigger.CronExpressionString = jobInfo.CronExpression;
                jobTrigger.Name = jobIdentity;
                jobTrigger.Description = jobInfo.JobName;
                jobTrigger.StartTimeUtc = jobInfo.StartTime;
                jobTrigger.EndTimeUtc = jobInfo.EndTime;
                scheduler.ScheduleJob(jobDetail, jobTrigger);
                if (jobInfo.Status == 0) scheduler.PauseJob(jobKey);
            }
            else
            {
                if (jobInfo.Status == 1) scheduler.ResumeJob(jobIdentity);
            }
            
            return true;
        }

        public static bool ScheduleJobs(this IScheduler scheduler, List<JobInfo> jobInfoList = null)
        {
            Ensure.Requires<ArgumentNullException>(scheduler != null, "sheduler should not be null.");
            if (jobInfoList == null)
                jobInfoList = JobOperator.GetJobInfoList();
            var result = false;
            foreach (var jobInfo in jobInfoList)
            {
                result |= ScheduleJob(scheduler, jobInfo);
            }
            return result;
        }

        public static bool ResumeJob(this IScheduler scheduler,string jobKey)
        {
            var _jobKey = new JobKey(jobKey);
            if(scheduler.CheckExists(_jobKey))
            {
                scheduler.ResumeJob(_jobKey);
            }
            return true;
        }

        public static bool PauseJob(this IScheduler scheduler,string jobKey)
        {
            var _jobKey = new JobKey(jobKey);
            if(scheduler.CheckExists(_jobKey))
            {
                scheduler.PauseJob(_jobKey);
                var jobDetail = scheduler.GetJobDetail(_jobKey);
                if(jobDetail.JobType.GetInterface("IInterruptableJob") != null)
                {
                    scheduler.Interrupt(_jobKey);
                }
            }
            return true;
        }

        public static bool DeleteJob(this IScheduler scheduler, string jobKey)
        {
            Ensure.Requires<ArgumentNullException>(scheduler != null, "sheduler should not be null.");
            var _jobKey = new JobKey(jobKey);
            if (scheduler.CheckExists(_jobKey))
            {
                return scheduler.DeleteJob(_jobKey);
            }
            return true;
        }

    }

    public class JobOperator
    {
        public static List<JobInfo> GetJobInfoList()
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                return (from j in dataContext.Set<JobInfo>()
                        where j.IsDelete == 0
                        select j).ToList();
            }
        }
    }
}
