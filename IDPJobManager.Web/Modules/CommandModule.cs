namespace IDPJobManager.Web.Modules
{
    using Configuration;
    using Core;
    using Core.SchedulerProviders;
    using Nancy;
    using Quartz;
    using Quartz.Impl.Matchers;

    public class CommandModule : BaseModule
    {
        private static readonly IScheduler Scheduler;

        static CommandModule()
        {
            Scheduler = IDPJobManagerStarter.Scheduler;
        }

        public CommandModule()
            : base()
        {
            #region Trigger Commands
            Get["/pausetrigger/{trigger}/{group}"] = _ =>
                {
                    var trigger = (string)_.trigger;
                    var group = (string)_.group;

                    try
                    {
                        Scheduler.PauseTrigger(new TriggerKey(trigger, group));

                        return Response.AsJson(new { r = true });
                    }
                    catch (System.Exception ex)
                    {
                        return Response.AsJson(new { r = false, m = ex.Message });
                    }
                };
            Get["/resumetrigger/{trigger}/{group}"] = _ =>
            {
                var trigger = (string)_.trigger;
                var group = (string)_.group;

                try
                {
                    Scheduler.ResumeTrigger(new TriggerKey(trigger, group));

                    return Response.AsJson(new { r = true });
                }
                catch (System.Exception ex)
                {
                    return Response.AsJson(new { r = false, m = ex.Message });
                }
            };
            #endregion
            #region Job Commands
            Get["/pausejob/{job}/{group}"] = _ =>
                {
                    var job = (string)_.job;
                    var group = (string)_.group;

                    try
                    {
                        Scheduler.PauseJob(new JobKey(job, group));

                        return Response.AsJson(new { r = true });
                    }
                    catch (System.Exception ex)
                    {
                        return Response.AsJson(new { r = false, m = ex.Message });
                    }
                };
            Get["/resumejob/{job}/{group}"] = _ =>
            {
                var job = (string)_.job;
                var group = (string)_.group;

                try
                {
                    Scheduler.ResumeJob(new JobKey(job, group));

                    return Response.AsJson(new { r = true });
                }
                catch (System.Exception ex)
                {
                    return Response.AsJson(new { r = false, m = ex.Message });
                }
            };
            Get["/firejob/{job}/{group}"] = _ =>
            {
                var job = (string)_.job;
                var group = (string)_.group;

                try
                {
                    Scheduler.TriggerJob(new JobKey(job, group));

                    return Response.AsJson(new { r = true });
                }
                catch (System.Exception ex)
                {
                    return Response.AsJson(new { r = false, m = ex.Message });
                }
            };
            #endregion
            #region Group Commands
            Get["/pausegroup/{group}"] = _ =>
                {
                    var group = (string)_.group;

                    try
                    {
                        Scheduler.PauseJobs(GroupMatcher<JobKey>.GroupEquals(group));

                        return Response.AsJson(new { r = true });
                    }
                    catch (System.Exception ex)
                    {
                        return Response.AsJson(new { r = false, m = ex.Message });
                    }
                };

            Get["/resumegroup/{group}"] = _ =>
            {
                var group = (string)_.group;

                try
                {
                    Scheduler.ResumeJobs(GroupMatcher<JobKey>.GroupEquals(group));

                    return Response.AsJson(new { r = true });
                }
                catch (System.Exception ex)
                {
                    return Response.AsJson(new { r = false, m = ex.Message });
                }
            };
            #endregion
        }
    }
}
