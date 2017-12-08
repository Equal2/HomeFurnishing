

using Infrastructure.IO;
using Model.Tasks.Models;
using Models.Company.DatabaseViews;
using Notifier.Models;
using Notifier.Models.Infrastructure;
using System.Data.Entity;

namespace Data.Initial
{
    public partial class LoginApplicationDbContext : DataContext, IStandardNotifierModelDbSets 
    {       
        public string strSchemaName = "Web";

        public LoginApplicationDbContext()
            : base("LoginDB", false)
        {
            ;
            
        }        
        public DbSet<NotificationSubject> NotificationSubject { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<NotificationUser> NotificationUser { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<DAR> DAR { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<UserTeam> UserTeam { get; set; }
        public DbSet<_Users> _Users { get; set; }
        public DbSet<_Roles> _Roles { get; set; }
    }
}