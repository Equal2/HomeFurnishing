using Login.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Login
{
    public class ApplicationDbContextInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            InitializeIdentityForEF(context);
            base.Seed(context);
        }

        private void InitializeIdentityForEF(ApplicationDbContext context)
        {
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));            
            string name = "Admin";
            string password = "Surya@123";
            //string test = "test";

            //Create Role Test and User Test
            //RoleManager.Create(new IdentityRole() { Name ="DEO Packing"});
            //RoleManager.Create(new IdentityRole() { Name = "Packing Manager" });
            //RoleManager.Create(new IdentityRole() { Name = "General Manager" });
            //UserManager.Create(new ApplicationUser() { UserName = "test" });
                     
            
            
            //Create Role Admin if it does not exist
            if (!RoleManager.RoleExists(name))
            {
                var roleresult = RoleManager.Create(new IdentityRole(name));
            }

            //Create User=Admin with password=123456         

            //Add User Admin to Role Admin
            //if (adminresult.Succeeded)
            //{
            //    var result = UserManager.AddToRole(user.Id, name);
            //}
        }
    }   
}