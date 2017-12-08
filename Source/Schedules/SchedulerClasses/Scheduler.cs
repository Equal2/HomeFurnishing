using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Schedules.SchedulerClasses;
using System.Configuration;
using System.IO;

namespace Schedules.SchedulerClasses
{
    public class Scheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            //int Hr = Convert.ToInt32(ConfigurationManager.AppSettings["ScheduleHour"]);
            //int Mn = Convert.ToInt32(ConfigurationManager.AppSettings["ScheduleMinute"]);

            //IJobDetail job = JobBuilder.Create<PackingSummaryEmailJob>().Build();

            //ITrigger trigger = TriggerBuilder.Create()
            //    .WithDailyTimeIntervalSchedule
            //      (s =>
            //         s.WithIntervalInHours(24)
            //        .OnEveryDay()
            //        .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(Hr, Mn))
            //      )
            //    .Build();


            //For Weekly Reports Create New Job & Trigger 

            int WeeklyDay = Convert.ToInt32(ConfigurationManager.AppSettings["WeeklyScheduleDay"]);
            int WeeklyTimeHour = Convert.ToInt32(ConfigurationManager.AppSettings["WeeklyScheduleTimeHour"]);
            int WeeklyTimeMinut = Convert.ToInt32(ConfigurationManager.AppSettings["WeeklyScheduleTimeMinut"]);

            IJobDetail WeeklyJob = JobBuilder.Create<WeeklyEmailJob>().Build();

            ITrigger Weeklytrigger = TriggerBuilder.Create()
                .WithSchedule(CronScheduleBuilder
                    .WeeklyOnDayAndHourAndMinute((DayOfWeek)WeeklyDay, WeeklyTimeHour, WeeklyTimeMinut)
                    .InTimeZone(TimeZoneInfo.Local))
                .Build();


            //For Monthly Reports Create New Job & Trigger 

            int MonthlyTime = Convert.ToInt32(ConfigurationManager.AppSettings["MonthlyScheduleTime"]);
            IJobDetail MonthlyJob = JobBuilder.Create<MonthlyEmailJob>().Build();

            ITrigger Monthlytrigger = TriggerBuilder.Create()
                .WithSchedule(CronScheduleBuilder
                    .MonthlyOnDayAndHourAndMinute(DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month), MonthlyTime, 15)
                    .InTimeZone(TimeZoneInfo.Local))
                .Build();



            //For Daily Notification Create New Job & Trigger 

            int NotificationHr = Convert.ToInt32(ConfigurationManager.AppSettings["DailyNotificationTime"]);

            IJobDetail DailyNotificationjob = JobBuilder.Create<DailyNotificationJob>().Build();

            ITrigger DailyNotificationtrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(NotificationHr, 00))
                  )
                .Build();



            //For Daily Mail Create New Job & Trigger 
            int DailyHours = Convert.ToInt32(ConfigurationManager.AppSettings["DailyScheduleHours"]);
            int DailyMinuts = Convert.ToInt32(ConfigurationManager.AppSettings["DailyScheduleMinut"]);

            IJobDetail Dailyjob = JobBuilder.Create<DailyEmailJob>().Build();

            ITrigger Dailytrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(DailyHours, DailyMinuts))
                  )
                .Build();


            //For Daily Noon Mail Create New Job & Trigger 
            int DailyNoonHours = Convert.ToInt32(ConfigurationManager.AppSettings["DailyNoonHours"]);
            int DailyNoonMinuts = Convert.ToInt32(ConfigurationManager.AppSettings["DailyNoonMinut"]);

            IJobDetail DailyNoonjob = JobBuilder.Create<DailyNoonEmailJob>().Build();

            ITrigger DailyNoontrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(DailyNoonHours, DailyNoonMinuts))
                  )
                .Build();



            //For Daily Evening Mail Create New Job & Trigger 
            int DailyEveningHours = Convert.ToInt32(ConfigurationManager.AppSettings["DailyEveningHours"]);
            int DailyEveningMinuts = Convert.ToInt32(ConfigurationManager.AppSettings["DailyEveningMinut"]);

            IJobDetail DailyEveningjob = JobBuilder.Create<DailyEveningEmailJob>().Build();

            ITrigger DailyEveningtrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(DailyEveningHours, DailyEveningMinuts))
                  )
                .Build();


            //For Daily Night Mail Create New Job & Trigger 
            int DailyNightHours = Convert.ToInt32(ConfigurationManager.AppSettings["DailyNightHours"]);
            int DailyNightMinuts = Convert.ToInt32(ConfigurationManager.AppSettings["DailyNightMinut"]);

            IJobDetail DailyNightjob = JobBuilder.Create<DailyNightEmailJob>().Build();

            ITrigger DailyNighttrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(DailyNightHours, DailyNightMinuts))
                  )
                .Build();


            //For Daily Night Data Execution New Job & Trigger 
            IJobDetail DailyNightDataExecutionjob = JobBuilder.Create<DailyNightDataExecutionJob>().Build();

            ITrigger DailyNightDataExecutiontrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(DailyNightHours, DailyNightMinuts))
                  )
                .Build();






            //scheduler.ScheduleJob(job, trigger);
            scheduler.ScheduleJob(WeeklyJob, Weeklytrigger);
            scheduler.ScheduleJob(MonthlyJob, Monthlytrigger);
            scheduler.ScheduleJob(DailyNotificationjob, DailyNotificationtrigger);
            scheduler.ScheduleJob(Dailyjob, Dailytrigger);
            scheduler.ScheduleJob(DailyNoonjob, DailyNoontrigger);
            scheduler.ScheduleJob(DailyEveningjob, DailyEveningtrigger);
            scheduler.ScheduleJob(DailyNightjob, DailyNighttrigger);
            scheduler.ScheduleJob(DailyNightDataExecutionjob, DailyNightDataExecutiontrigger);
        }
    }
}