using System.Data.Entity;
using Model.Models;
using Data.Infrastructure;

namespace Data.Models
{
    public partial class LogApplicationDbContext : DataContext
    {        
        static string SchemaName = "Web";

        public string strSchemaName = "Web";

        public LogApplicationDbContext()
            : base((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"], false)
        {
            Configuration.ProxyCreationEnabled = false;
            Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
            Database.CommandTimeout = 60;
        }
        static LogApplicationDbContext()
        {
            Database.SetInitializer<LogApplicationDbContext>(null); // Existing data, do nothing
        }

        public DbSet<ActivityLog> ActivityLog { get; set; }
        public DbSet<ActivityType> ActivityType { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(SchemaName);
        }
    }
}