using System.Collections.Generic;
using System.Linq;
using System;
using Infrastructure.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using Data;
using AdminSetup.Models.Models;

namespace Service
{
    public interface IUsersService : IDisposable
    {
        IQueryable<IdentityUser> GetUsersList();
        IdentityUser Find(string Id);
        void Syncs();

    }

    public class UsersService : IUsersService
    {
        private readonly IDataContext _context;
        public UsersService(IDataContext Con)
        {
            _context = Con;
        }

        public IQueryable<IdentityUser> GetUsersList()
        {
            return (from p in ((ApplicationDbContext)_context).Users
                    select p);
        }

        public IdentityUser Find(string Id)
        {
            return ((ApplicationDbContext)_context).Users.Find(Id);
        }

        public void Syncs()
        {
            IEnumerable<ApplicationUser> FromList;
            IEnumerable<ApplicationUser> ToList;

            int AppId = (int)System.Web.HttpContext.Current.Session["ApplicationId"];

            using (LoginApplicationDbContext dbContext = new LoginApplicationDbContext())
            {
                FromList = dbContext.Database.SqlQuery<ApplicationUser>(" SELECT A.* FROM AspNetUsers A JOIN UserApplications UA ON A.Id = UA.UserId WHERE UA.ApplicationId=" + AppId).ToList();
            }


            ToList = ((ApplicationDbContext)_context).Database.SqlQuery<ApplicationUser>(" SELECT * FROM Web.Users ").ToList();


            IEnumerable<ApplicationUser> PendingToUpdate;

            PendingToUpdate = from p in FromList
                              join t in ToList on p.Id equals t.Id into Left
                              from lef in Left.DefaultIfEmpty()
                              where lef == null
                              select p;

            foreach (var item in PendingToUpdate)
            {
                ApplicationUser NewRec = new ApplicationUser();
                NewRec = item;
                NewRec.ObjectState = Model.ObjectState.Added;
                ((ApplicationDbContext)_context).Users.Add(NewRec);
            }

            _context.SaveChanges();

        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
