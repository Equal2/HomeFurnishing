using Infrastructure.IO;
using Models.Company.DatabaseViews;
using Notifier.Models;
using System.Data.Entity;

namespace Data
{
    public partial class LoginApplicationDbContext : DataContext
    {       
        public string strSchemaName = "Web";

        public LoginApplicationDbContext()
            : base("LoginDB", false)
        {
            ;
            Configuration.ProxyCreationEnabled = false;
        }        
        public DbSet<NotificationSubject> NotificationSubject { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<NotificationUser> NotificationUser { get; set; }
        public DbSet<_Users> _Users { get; set; }
        public DbSet<_Roles> _Roles { get; set; }
    }
}