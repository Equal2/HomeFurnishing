
using AdminSetup.Models.Models;
using Components.Logging;
using Infrastructure.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using Models.Company.DatabaseViews;
using Models.Company.Models;
using System;
using System.Data.Entity;
using System.IO;

namespace Data
{
    public partial class ApplicationDbContext : DataContext
    {
        static string _errors = "";
        static string SchemaName = "Web";

        public string strSchemaName = "Web";

        public ApplicationDbContext()
            : base((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"] ?? "LoginDB", false)
        {
            Configuration.ProxyCreationEnabled = false;
            Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
            Database.CommandTimeout = 60;

            try
            {
                using (StreamWriter writer =
                 new StreamWriter(@"c:\temp\DbContextInitLog.txt", true))
                {
                    writer.WriteLine(" {0} : ", DateTime.Now);
                    writer.Close();
                }
            }
            catch (Exception x)
            {

            }

        }

        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(null); // Existing data, do nothing   
            //Database.SetInitializer(new ApplicationDbContextInitializer()); // Create New database
            //Database.SetInitializer(new LoginDbContextInitializer()); // Create New database
        }

        public new IDbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

        public DbSet<ActivityLog> ActivityLog { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<ControllerAction> ControllerAction { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<CompanySettings> CompanySettings { get; set; }
        public DbSet<MenuModule> MenuModule { get; set; }
        public DbSet<MvcController> MvcController { get; set; }
        public DbSet<RolesControllerAction> RolesControllerAction { get; set; }
        public DbSet<RolesDivision> RolesDivision { get; set; }
        public DbSet<RolesMenu> RolesMenu { get; set; }
        public DbSet<RolesSite> RolesSite { get; set; }
        public DbSet<UserBookMark> UserBookMark { get; set; }
        public DbSet<DocumentType> DocumentType { get; set; }
        public DbSet<Site> Site { get; set; }
        public DbSet<Division> Division { get; set; }
        public DbSet<_Roles> _Roles { get; set; }
        public DbSet<_Users> _Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.Properties<decimal>().Configure(config => config.HasPrecision(18, 4));

            // Change the name of the table to be Users instead of AspNetUsers
            modelBuilder.Entity<IdentityUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
        }
    }

}