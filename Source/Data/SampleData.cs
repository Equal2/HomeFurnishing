
namespace Data
{
    //public class SampleData : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    //{
    //    protected override void Seed(ApplicationDbContext context)
    //    {
           
    //        var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

    //        var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

    //        string name = "Admin";
    //        string password = "123456";


    //        //Create Role Admin if it does not exist
    //        if (!RoleManager.RoleExists(name))
    //        {
    //            var roleresult = RoleManager.Create(new IdentityRole(name));
    //        }

    //        //Create User=Admin with password=123456
    //        var user = new ApplicationUser();
    //        user.UserName = name;
    //        var adminresult = UserManager.Create(user, password);

    //        //Add User Admin to Role Admin
    //        if (adminresult.Succeeded)
    //        {
    //            var result = UserManager.AddToRole(user.Id, name);
    //        }
    //        //base.Seed(context);
    //        context.Commit();
                 

    //    }

    //}
}