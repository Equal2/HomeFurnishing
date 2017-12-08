using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notifier.Models ;

namespace Notifier.Models.Infrastructure
{
    public interface IStandardNotifierModelDbSets
    {
        DbSet<NotificationUser> NotificationUser { get; set; }
        DbSet<Notification> Notification { get; set; }
        DbSet<NotificationSubject> NotificationSubject { get; set; }
    }
}
