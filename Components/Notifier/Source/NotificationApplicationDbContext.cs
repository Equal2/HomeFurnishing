using Infrastructure.IO;
using Notifier.Models;
using System.Data.Entity;


namespace Notifier.Data
{   

    public partial class NotificationApplicationDbContext : DataContext
    {
    
        public string strSchemaName = "Web";

        public NotificationApplicationDbContext()
            : base("LoginDB", false)
        {
            ;
            //Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<NotificationSubject> NotificationSubject { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<NotificationUser> NotificationUser { get; set; }
    }
}