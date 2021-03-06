﻿using IDPJobManager.Core.Domain;
using IDPJobManager.Core.Extensions;
using IDPJobManager.Core.Utils;
using Quartz;
using System;
using System.Diagnostics;

namespace IDPJobManager.Core
{
    public abstract class PerformanceJob : AspectableJob
    {
        private readonly Stopwatch _stopwatch;
        private DateTime _startTime;

        public PerformanceJob()
        {
            _stopwatch = new Stopwatch();
        }

        public override bool BeforeExecute(JobExecutionContext context)
        {
            _stopwatch.Restart();
            _startTime = DateTime.Now;
            return true;
        }

        public override void AfterExecute(JobExecutionContext context)
        {
            _stopwatch.Stop();
            Logger.Instance.InfoFormat("Execute `{0}` elapsed time:{1} ms.", GetType().FullName, _stopwatch.ElapsedMilliseconds);
            JobOperator.AddJobPerformance(new JobPerformance
            {
                ID = Guid.NewGuid(),
                JobName = context.JobName,
                JobGroup = context.JobGroup,
                StartTime = _startTime,
                EndTime = DateTime.Now,
                CPU = (decimal)AppDomain.CurrentDomain.MonitoringTotalProcessorTime.TotalSeconds,
                Memory = AppDomain.CurrentDomain.MonitoringSurvivedMemorySize * 1m / 1024 / 1024,
                CreateTime = DateTime.Now
            });
        }
    }
}
