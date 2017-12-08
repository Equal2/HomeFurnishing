using Infrastructure.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

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
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.Properties<decimal>().Configure(config => config.HasPrecision(18, 4));

            // Change the name of the table to be Users instead of AspNetUsers
            modelBuilder.Entity<IdentityUser>().ToTable("Users");
        }
    }

}