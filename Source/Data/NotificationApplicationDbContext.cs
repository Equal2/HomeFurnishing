using System.Data.Entity;
using Data.Infrastructure;
using Model.Models;
//using Models.Login.Models;

namespace Data.Models
{   

    public partial class NotificationApplicationDbContext : DataContext
    {
    
        public string strSchemaName = "Web";

        public NotificationApplicationDbContext()
            : base("LoginDB", false)
        {
            ;
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<NotificationSubject> NotificationSubject { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<NotificationUser> NotificationUser { get; set; }
    }
}