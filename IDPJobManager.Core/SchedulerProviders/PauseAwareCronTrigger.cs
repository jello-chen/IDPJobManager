using Quartz;
using System;
using Quartz.Spi;

namespace IDPJobManager.Core.SchedulerProviders
{
    public class PauseAwareCronTrigger : Quartz.Impl.Triggers.CronTriggerImpl
    {
        public override DateTimeOffset? GetNextFireTimeUtc()
        {
            var nextFireTimeUtc = base.GetNextFireTimeUtc();
            Console.WriteLine($"next fire time:{nextFireTimeUtc}");
            if (nextFireTimeUtc?.UtcTicks < DateTime.UtcNow.Ticks)
            {
                // next fire time after now
                nextFireTimeUtc = GetFireTimeAfter(null);
                Console.WriteLine($"set next fire time:{nextFireTimeUtc}");
                SetNextFireTimeUtc(nextFireTimeUtc);
            }
            return nextFireTimeUtc;
        }
    }

    public class PauseAwareCronScheduleBuilder : ScheduleBuilder<ICronTrigger>
    {
        private readonly CronExpression cronExpression;

        private int misfireInstruction;

        protected PauseAwareCronScheduleBuilder(CronExpression cronExpression)
        {
            this.cronExpression = cronExpression ?? throw new ArgumentNullException("cronExpression", "cronExpression cannot be null");
        }

        public override IMutableTrigger Build()
        {
            return new PauseAwareCronTrigger
            {
                CronExpression = this.cronExpression,
                TimeZone = this.cronExpression.TimeZone,
                MisfireInstruction = this.misfireInstruction
            };
        }

        public static PauseAwareCronScheduleBuilder CronSchedule(string cronExpression)
        {
            CronExpression.ValidateExpression(cronExpression);
            return PauseAwareCronScheduleBuilder.CronScheduleNoParseException(cronExpression);
        }

        private static PauseAwareCronScheduleBuilder CronScheduleNoParseException(string presumedValidCronExpression)
        {
            PauseAwareCronScheduleBuilder result;
            try
            {
                result = PauseAwareCronScheduleBuilder.CronSchedule(new CronExpression(presumedValidCronExpression));
            }
            catch (FormatException innerException)
            {
                throw new Exception("CronExpression '" + presumedValidCronExpression + "' is invalid, which should not be possible, please report bug to Quartz developers.", innerException);
            }
            return result;
        }

        public static PauseAwareCronScheduleBuilder CronSchedule(CronExpression cronExpression)
        {
            return new PauseAwareCronScheduleBuilder(cronExpression);
        }

        public static PauseAwareCronScheduleBuilder DailyAtHourAndMinute(int hour, int minute)
        {
            DateBuilder.ValidateHour(hour);
            DateBuilder.ValidateMinute(minute);
            string presumedValidCronExpression = string.Format("0 {0} {1} ? * *", minute, hour);
            return PauseAwareCronScheduleBuilder.CronScheduleNoParseException(presumedValidCronExpression);
        }

        public static PauseAwareCronScheduleBuilder AtHourAndMinuteOnGivenDaysOfWeek(int hour, int minute, params DayOfWeek[] daysOfWeek)
        {
            if (daysOfWeek == null || daysOfWeek.Length == 0)
            {
                throw new ArgumentException("You must specify at least one day of week.");
            }
            DateBuilder.ValidateHour(hour);
            DateBuilder.ValidateMinute(minute);
            string text = string.Format("0 {0} {1} ? * {2}", minute, hour, (int)(daysOfWeek[0] + 1));
            for (int i = 1; i < daysOfWeek.Length; i++)
            {
                text = text + "," + (int)(daysOfWeek[i] + 1);
            }
            return PauseAwareCronScheduleBuilder.CronScheduleNoParseException(text);
        }

        public static PauseAwareCronScheduleBuilder WeeklyOnDayAndHourAndMinute(DayOfWeek dayOfWeek, int hour, int minute)
        {
            DateBuilder.ValidateHour(hour);
            DateBuilder.ValidateMinute(minute);
            string presumedValidCronExpression = string.Format("0 {0} {1} ? * {2}", minute, hour, (int)(dayOfWeek + 1));
            return PauseAwareCronScheduleBuilder.CronScheduleNoParseException(presumedValidCronExpression);
        }

        public static PauseAwareCronScheduleBuilder MonthlyOnDayAndHourAndMinute(int dayOfMonth, int hour, int minute)
        {
            DateBuilder.ValidateDayOfMonth(dayOfMonth);
            DateBuilder.ValidateHour(hour);
            DateBuilder.ValidateMinute(minute);
            string presumedValidCronExpression = string.Format("0 {0} {1} {2} * ?", minute, hour, dayOfMonth);
            return PauseAwareCronScheduleBuilder.CronScheduleNoParseException(presumedValidCronExpression);
        }

        public PauseAwareCronScheduleBuilder InTimeZone(TimeZoneInfo tz)
        {
            this.cronExpression.TimeZone = tz;
            return this;
        }

        public PauseAwareCronScheduleBuilder WithMisfireHandlingInstructionIgnoreMisfires()
        {
            this.misfireInstruction = -1;
            return this;
        }

        public PauseAwareCronScheduleBuilder WithMisfireHandlingInstructionDoNothing()
        {
            this.misfireInstruction = 2;
            return this;
        }

        public PauseAwareCronScheduleBuilder WithMisfireHandlingInstructionFireAndProceed()
        {
            this.misfireInstruction = 1;
            return this;
        }

        internal PauseAwareCronScheduleBuilder WithMisfireHandlingInstruction(int readMisfireInstructionFromString)
        {
            this.misfireInstruction = readMisfireInstructionFromString;
            return this;
        }
    }

}
