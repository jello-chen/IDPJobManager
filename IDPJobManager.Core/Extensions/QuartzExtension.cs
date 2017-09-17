using IDPJobManager.Core.Domain;
using IDPJobManager.Core.SchedulerProviders;
using IDPJobManager.Core.Utils;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            Ensure.Requires<ArgumentNullException>(jobInfo != null, "jobInfo should not be null.");

            try
            {
                var jobGroup = string.IsNullOrWhiteSpace(jobInfo.JobGroup) ? null : jobInfo.JobGroup;
                var jobKey = new JobKey(jobInfo.JobName, jobGroup);
                if (!scheduler.CheckExists(jobKey))
                {
                    var appDomain = default(AppDomain);
                    var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Jobs", jobInfo.AssemblyName, $"{jobInfo.AssemblyName}.dll");
                    var job = AppDomainLoader<BaseJob>.Load(dllPath, jobInfo.ClassName, out appDomain);
                    return JobPoolManager.Instance.Add(jobInfo.ID, new JobRuntimeInfo
                    {
                        AppDomain = appDomain,
                        Job = job,
                        JobModel = jobInfo
                    });
                }
                else
                {
                    if (jobInfo.Status == 1) scheduler.ResumeJob(jobKey);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex.Message);
                return false;
            }
        }

        public static async Task<bool> ScheduleJobsAsync(this IScheduler scheduler, List<JobInfo> jobInfoList = null)
        {
            Ensure.Requires<ArgumentNullException>(scheduler != null, "sheduler should not be null.");
            if (jobInfoList == null)
                jobInfoList = await JobOperator.GetJobInfoListAsync();
            var result = false;
            foreach (var jobInfo in jobInfoList)
            {
                result |= ScheduleJob(scheduler, jobInfo);
            }
            return result;
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

        public static bool UnscheduleJob(this IScheduler scheduler, JobInfo jobInfo)
        {
            Ensure.Requires<ArgumentNullException>(scheduler != null, "sheduler should not be null.");
            var jobKey = new JobKey(jobInfo.JobName, jobInfo.JobGroup);
            if (scheduler.CheckExists(jobKey))
            {
                return scheduler.UnscheduleJob(new TriggerKey($"{jobInfo.JobName}Trigger", jobInfo.JobGroup));
            }
            return true;
        }

        public static async Task<bool> UnscheduleJobsAsync(this IScheduler scheduler, List<JobInfo> jobInfoList = null, bool isUpdatingDB = false)
        {
            Ensure.Requires<ArgumentNullException>(scheduler != null, "sheduler should not be null.");
            if (jobInfoList == null)
                jobInfoList = await JobOperator.GetJobInfoListAsync();
            var result = false;
            foreach (var jobInfo in jobInfoList)
            {
                result |= UnscheduleJob(scheduler, jobInfo);
            }
            return result;
        }

        public static bool ResumeJob(this IScheduler scheduler, JobInfo jobInfo)
        {
            var jobKey = new JobKey(jobInfo.JobName, jobInfo.JobGroup);
            if (scheduler.CheckExists(jobKey))
            {
                scheduler.ResumeJob(jobKey);
            }
            return true;
        }

        public static bool PauseJob(this IScheduler scheduler, JobInfo jobInfo, bool isUpdateDB = false)
        {
            var jobKey = new JobKey(jobInfo.JobName, jobInfo.JobGroup);
            if (scheduler.CheckExists(jobKey))
            {
                scheduler.PauseJob(jobKey);
                var jobDetail = scheduler.GetJobDetail(jobKey);
                if (jobDetail.JobType.GetInterface("IInterruptableJob") != null)
                {
                    scheduler.Interrupt(jobKey);
                }
            }
            if (isUpdateDB)
                scheduler.PauseJob(jobKey);
            return true;
        }

        public static async Task<bool> PauseJobsAsync(this IScheduler scheduler, List<JobInfo> jobInfos = null, bool isUpdateDB = false)
        {
            Ensure.Requires<ArgumentNullException>(scheduler != null, "sheduler should not be null.");
            if (jobInfos == null)
                jobInfos = await JobOperator.GetJobInfoListAsync();
            var result = false;
            foreach (var jobInfo in jobInfos)
            {
                result |= PauseJob(scheduler, jobInfo, isUpdateDB);
            }
            return result;
        }

        public static bool DeleteJob(this IScheduler scheduler, JobInfo jobInfo)
        {
            Ensure.Requires<ArgumentNullException>(scheduler != null, "sheduler should not be null.");
            var jobKey = new JobKey(jobInfo.JobName, jobInfo.JobGroup);
            if (scheduler.CheckExists(jobKey))
            {
                return scheduler.DeleteJob(jobKey);
            }
            return true;
        }
    }

    public class JobOperator
    {
        public static List<JobInfo> GetJobInfoList(string assemblyName)
        {
            Ensure.Requires<ArgumentNullException>(!string.IsNullOrEmpty(assemblyName), "assemblyName should not be null.");

            using (var dataContext = new IDPJobManagerDataContext())
            {
                return (from j in dataContext.T_Job
                        where j.IsDelete == 0 && j.AssemblyName == assemblyName
                        select j).ToList();
            }
        }

        public static List<JobInfo> GetJobInfoList()
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                return (from j in dataContext.T_Job
                       where j.IsDelete == 0
                      select j).ToList();
            }
        }

        public static async Task<List<JobInfo>> GetJobInfoListAsync()
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                return await (from j in dataContext.T_Job
                              where j.IsDelete == 0
                              select j).ToListAsync();
            }
        }

        public static bool UpdateJobStatus(Guid id, int status)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobInfoList = dataContext.Set<JobInfo>();
                var jobInfo = jobInfoList.FirstOrDefault(j => j.ID == id);
                if (jobInfo != null)
                {
                    jobInfo.Status = status;
                    dataContext.SaveChanges();
                }
                return true;
            }
        }

        public static bool UpdateAllJobsStatus(int status)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobInfoList = dataContext.Set<JobInfo>();
                foreach (var jobInfo in jobInfoList)
                {
                    jobInfo.Status = status;
                }
                dataContext.SaveChanges();
                return true;
            }
        }

        public static async Task<bool> UpdateJobStatusAsync(Guid id, int status)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobInfoList = dataContext.Set<JobInfo>();
                var jobInfo = jobInfoList.FirstOrDefault(j => j.ID == id);
                if (jobInfo != null)
                {
                    jobInfo.Status = status;
                    await dataContext.SaveChangesAsync();
                }
                return true;
            }
        }

        public static async Task<bool> DeleteJobAsync(Guid id)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobInfoList = dataContext.Set<JobInfo>();
                var jobInfo = jobInfoList.FirstOrDefault(j => j.ID == id);
                if (jobInfo != null)
                {
                    jobInfo.IsDelete = 1;
                    await dataContext.SaveChangesAsync();
                }
                return true;
            }
        }

        public static async Task<bool> UpdateRecentRunTimeAsync(string jobName, string jobGroup, DateTime recentRunTime)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobInfoList = dataContext.Set<JobInfo>();
                JobInfo jobInfo = null;
                if (string.IsNullOrWhiteSpace(jobGroup) || jobGroup.ToUpper() == "DEFAULT")
                    jobInfo = jobInfoList.FirstOrDefault(j => j.JobName == jobName);
                else
                    jobInfo = jobInfoList.FirstOrDefault(j => j.JobName == jobName && j.JobGroup == jobGroup);
                if (jobInfo != null)
                {
                    jobInfo.RecentRunTime = recentRunTime;
                    await dataContext.SaveChangesAsync();
                }
                return true;
            }
        }

        public static async Task<bool> UpdateNextFireTimeAsync(string jobName, string jobGroup, DateTime nextFireTime)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobInfoList = dataContext.Set<JobInfo>();
                JobInfo jobInfo = null;
                if (string.IsNullOrWhiteSpace(jobGroup) || jobGroup.ToUpper() == "DEFAULT")
                    jobInfo = jobInfoList.FirstOrDefault(j => j.JobName == jobName);
                else
                    jobInfo = jobInfoList.FirstOrDefault(j => j.JobName == jobName && j.JobGroup == jobGroup);
                if (jobInfo != null)
                {
                    jobInfo.NextFireTime = nextFireTime;
                    await dataContext.SaveChangesAsync();
                }
                return true;
            }
        }

        public static List<JobInfo> GetDependentJobInfoList(Guid id)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var query = from p in dataContext.T_JobDependency
                            join q in dataContext.T_Job
                            on p.DependentJobID equals q.ID into g
                            from j in g.DefaultIfEmpty()
                            where p.JobID == id
                            select j;
                return query.ToList();
            }
        }

        public static List<JobInfo> GetDependentJobInfoList(string jobName, string jobGroup)
        {
            var name = Config.GlobalConfiguration.Name;
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobInfo = dataContext.T_Job.FirstOrDefault(j => j.JobName == jobName && j.JobGroup == jobGroup);
                if (jobInfo != null)
                {
                    var query = from p in dataContext.T_JobDependency
                                join q in dataContext.T_Job
                                on p.DependentJobID equals q.ID into g
                                from j in g.DefaultIfEmpty()
                                where p.JobID == jobInfo.ID
                                select j;
                    return query.ToList();
                }
                return new List<JobInfo>();
            }
        }

        public static int GetJobStatus(string jobName, string jobGroup)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobInfoList = dataContext.Set<JobInfo>();
                JobInfo jobInfo = null;
                if (string.IsNullOrWhiteSpace(jobGroup) || jobGroup.ToUpper() == "DEFAULT")
                    jobInfo = jobInfoList.FirstOrDefault(j => j.JobName == jobName);
                else
                    jobInfo = jobInfoList.FirstOrDefault(j => j.JobName == jobName && j.JobGroup == jobGroup);
                if (jobInfo != null && jobInfo.Status.HasValue)
                    return jobInfo.Status.Value;
                return 0;
            }
        }

        public static bool AddJobPerformance(JobPerformance jobPerformance)
        {
            try
            {
                using (var dataContext = new IDPJobManagerDataContext())
                {
                    dataContext.T_JobPerformance.Add(jobPerformance);
                    dataContext.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex.Message);
                return false;
            }
        }

        public static List<PerformanceTrend> GetPerformanceTrend()
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var sql = @"WITH CTE 
                              AS (
                              SELECT CONVERT(NVARCHAR(10),StartTime) Date,JobName+'|'+JobGroup Job,SUM(DATEDIFF(MS,StartTime,EndTime)) ElapsedTime 
                                FROM [dbo].[T_JobPerformance] 
                            GROUP BY CONVERT(NVARCHAR(10),StartTime),JobName,JobGroup)
                              SELECT * FROM CTE ";
                return dataContext.Database.SqlQuery<PerformanceTrend>(sql).ToList();
            }
        }
    }
    public class PerformanceTrend
    {
        public string Date { get; set; }
        public string Job { get; set; }
        public int ElapsedTime { get; set; }
    }
}
