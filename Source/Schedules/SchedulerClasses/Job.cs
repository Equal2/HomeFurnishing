using EmailContents;
using Notifier.Hubs;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Schedules.SchedulerClasses
{

    //public class PackingSummaryEmailJob : IJob
    //{

    //    public virtual void Execute(IJobExecutionContext context)
    //    {
    //        PackingSummaryEmailContent.DailyPackingReviewEmail();
    //    }


    //}

    public class DailyEmailJob : IJob
    {
        public virtual void Execute(IJobExecutionContext context)
        {
            DailyEmailContent.DataCheckupEMail();
        }
    }


    public class DailyNoonEmailJob : IJob
    {
        public virtual void Execute(IJobExecutionContext context)
        {
            DailyNoonContent e = new DailyNoonContent();
            e.SendEMail();
        }
    }

    public class DailyNightEmailJob : IJob
    {
        public virtual void Execute(IJobExecutionContext context)
        {
            DailyNightContent e = new DailyNightContent();
            e.SendEMail();
        }
    }

    public class DailyEveningEmailJob : IJob
    {
        public virtual void Execute(IJobExecutionContext context)
        {
            DailyEveningContent e = new DailyEveningContent();
            e.SendEMail();
        }
    }



    public class DailyNotificationJob : IJob
    {
        public virtual void Execute(IJobExecutionContext context)
        {

            DailyNotificationScheduler.DailyNotifications();
        }
    }


    public class WeeklyEmailJob : IJob
    {
        public virtual void Execute(IJobExecutionContext context)
        {
            WeeklyEmailContent e = new WeeklyEmailContent();
            e.SendEMail();
        }
    }


    public class MonthlyEmailJob : IJob
    {
        public virtual void Execute(IJobExecutionContext context)
        {
            MonthlyEmailContent e = new MonthlyEmailContent();
            e.SendEMail();

        }
    }

    public class DailyNightDataExecutionJob : IJob
    {
        public virtual void Execute(IJobExecutionContext context)
        {
            DataExecution.DataExecutionProcess();
        }
    }
}