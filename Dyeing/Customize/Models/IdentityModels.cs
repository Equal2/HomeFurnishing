using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Jobs.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
    }

    //public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    //{
    //    public ApplicationDbContext()
    //        : base("DefaultConnection",false)
    //    {
    //    }


    //    static ApplicationDbContext()
    //    {
    //        Database.SetInitializer<ApplicationDbContext>(null); // Existing data, do nothing
    //    }


    //    //protected override void OnModelCreating(DbModelBuilder modelBuilder)
    //    //{

    //    //    // Change the name of the table to be Users instead of AspNetUsers
    //    //    modelBuilder.Entity<IdentityUser>().ToTable("Users");
    //    //    modelBuilder.Entity<ApplicationUser>().ToTable("Users");

    //    //}

    //}
}