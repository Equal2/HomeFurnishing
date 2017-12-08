using System.Collections.Generic;
using System.Linq;
using System;
using Infrastructure.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using Data;
using AdminSetup.Models.ViewModels;
using AdminSetup.Models.Models;
using ProjLib.DocumentConstants;
using Components.Logging;

namespace Service
{
    public interface IUserRolesService : IDisposable
    {
        IQueryable<IdentityRole> GetRolesList();
        IEnumerable<UserRoleViewModel> GetRolesListForIndex();
        UserRoleViewModel GetUserRoleDeail(string UserId);
        void UpdateUserRoles(UserRoleViewModel vm);
        void UpdateUserTempRoles(UserRoleViewModel vm);
        IEnumerable<object> GetUserRolesList(string UserId);
        IEnumerable<UserRoleViewModel> GetUserTempRoles(string UserId);
        void DeleteTempUserRoles(DateTime ExpiryDate, string UserId);
        IEnumerable<string> GetUserRolesForSession(string UserId);
        bool TryInsertUserRole(string UserId, string RoleId);
    }

    public class UserRolesService : IUserRolesService
    {
        private readonly IDataContext _context;
        private readonly ILogger _logger;
        private readonly IUsersService _userService;
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();
        public UserRolesService(IDataContext Con, ILogger log, IUsersService userServ)
        {
            _context = Con;
            _logger = log;
            _userService = userServ;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public IQueryable<IdentityRole> GetRolesList()
        {
            return (from p in ((ApplicationDbContext)_context).Roles
                    select p);
        }

        public IEnumerable<UserRoleViewModel> GetRolesListForIndex()
        {
            var UserRole = (from p in ((ApplicationDbContext)_context).Users
                            join t1 in ((ApplicationDbContext)_context).UserRole on p.Id equals t1.UserId into table
                            from tab in table.DefaultIfEmpty()
                            join t2 in ((ApplicationDbContext)_context).Roles on tab.RoleId equals t2.Id into table2
                            from tab2 in table2.DefaultIfEmpty()
                            where tab.ExpiryDate == null
                            select new UserRoleViewModel
                            {
                                UserName = p.UserName,
                                Email = p.Email,
                                UserId = p.Id,
                                RolesList = tab2.Name,
                            }).ToList();

            var GroupedRolesList = (from p in UserRole
                                    group p by p.UserId into g
                                    orderby g.Max(m => m.UserName)
                                    select new UserRoleViewModel
                                    {
                                        UserName = g.Max(m => m.UserName),
                                        Email = g.Max(m => m.Email),
                                        RolesList = string.Join(",", g.Select(m => m.RolesList)),
                                        UserId = g.Key,
                                    }).ToList();

            return GroupedRolesList;
        }

        public UserRoleViewModel GetUserRoleDeail(string UserId)
        {
            UserRoleViewModel vm = new UserRoleViewModel();
            vm.UserId = UserId;
            vm.UserName = ((ApplicationDbContext)_context).Users.Find(UserId).UserName;
            vm.RoleIdList = (string.Join(",", (from p in ((ApplicationDbContext)_context).UserRole
                                               where p.UserId == UserId && p.ExpiryDate == null
                                               select p.RoleId).ToList()));

            return vm;
        }

        public bool TryInsertUserRole(string UserId, string RoleId)
        {

            _userService.Syncs();

            bool SuccessFlag = false;
            string[] RoleIdArr = null;
            if (!string.IsNullOrEmpty(RoleId)) { RoleIdArr = RoleId.Split(",".ToCharArray()); }
            else { RoleIdArr = new string[] { "NA" }; }

            var RolesList = (from p in ((ApplicationDbContext)_context).UserRole
                             join t in ((ApplicationDbContext)_context).Roles on p.RoleId equals t.Id
                             where p.UserId == UserId && p.ExpiryDate == null && RoleIdArr.Contains(p.RoleId)
                             select p.RoleId).ToList();
          
            if(RolesList.Count() <= 0 && RolesList.Count <= 0)
            {
                foreach (var item in RoleId.Split(',').ToList())
                {
                    UserRole pt = new UserRole();
                    pt.UserId = UserId;
                    pt.RoleId = item;
                    pt.ObjectState = Model.ObjectState.Added;
                    ((ApplicationDbContext)_context).UserRole.Add(pt);
                }

                ((ApplicationDbContext)_context).SaveChanges();

                SuccessFlag = true;
            }

            return SuccessFlag;

        }

        public void UpdateUserRoles(UserRoleViewModel vm)
        {
            string NewUserRoles = "New Roles Assigned to User " + vm.UserId + " RoleIds: ";
            string OldUserRoles = "Roles Deleted From User " + vm.UserId + " RoleIds: ";


            if (!string.IsNullOrEmpty(vm.RoleIdList))
                foreach (var item in vm.RoleIdList.Split(',').ToList())
                {
                    UserRole pt = new UserRole();
                    pt.UserId = vm.UserId;
                    pt.RoleId = item;
                    pt.ObjectState = Model.ObjectState.Added;
                    ((ApplicationDbContext)_context).UserRole.Add(pt);

                }

            NewUserRoles += vm.RoleIdList;

            var ExistingRoles = (from p in ((ApplicationDbContext)_context).UserRole
                                 where p.UserId == vm.UserId
                                 && p.ExpiryDate == null
                                 select p).ToList();

            if (ExistingRoles.Count > 0)
                OldUserRoles += string.Join(",", ExistingRoles.Select(m => m.RoleId).ToList());

            foreach (var item in ExistingRoles)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                ((ApplicationDbContext)_context).UserRole.Add(item);
            }

            ((ApplicationDbContext)_context).SaveChanges();

            var DocTypeId = ((ApplicationDbContext)_context).DocumentType.Where(m => m.DocumentTypeName == MasterDocTypeConstants.UserRoles).FirstOrDefault().DocumentTypeId;

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = DocTypeId,
                    DocId = 0,
                    ActivityType = (int)ActivityTypeContants.Added,
                    Narration = NewUserRoles,
                }));

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = DocTypeId,
                DocId = 0,
                ActivityType = (int)ActivityTypeContants.Deleted,
                Narration = OldUserRoles,
            }));
        }

        public void UpdateUserTempRoles(UserRoleViewModel vm)
        {
            string NewUserRoles = "New Temporary Roles Assigned to User " + vm.UserId + " RoleIds: ";

            if (!string.IsNullOrEmpty(vm.RoleIdList))
                foreach (var item in vm.RoleIdList.Split(',').ToList())
                {
                    UserRole pt = new UserRole();
                    pt.UserId = vm.UserId;
                    pt.RoleId = item;
                    pt.ExpiryDate = vm.ExpiryDate;
                    pt.ObjectState = Model.ObjectState.Added;
                    ((ApplicationDbContext)_context).UserRole.Add(pt);
                }

            NewUserRoles += vm.RoleIdList;

            (_context).SaveChanges();

            var DocTypeId = ((ApplicationDbContext)_context).DocumentType.Where(m => m.DocumentTypeName == MasterDocTypeConstants.UserRoles).FirstOrDefault().DocumentTypeId;

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = DocTypeId,
                DocId = 0,
                ActivityType = (int)ActivityTypeContants.Added,
                Narration = NewUserRoles,
            }));
        }

        public IEnumerable<object> GetUserRolesList(string UserId)
        {
            var RolesList = (from p in ((ApplicationDbContext)_context).UserRole
                             join t in ((ApplicationDbContext)_context).Roles on p.RoleId equals t.Id
                             where p.UserId == UserId && p.ExpiryDate == null && t.Name != "SysAdmin"
                             select new
                             {
                                 id = p.RoleId,
                                 text = t.Name, 
                             }).ToArray();

            return RolesList;
        }

        public IEnumerable<UserRoleViewModel> GetUserTempRoles(string UserId)
        {
            var Today = DateTime.Now.Date;

            var RoleIdList = (from p in ((ApplicationDbContext)_context).UserRole
                              join t in ((ApplicationDbContext)_context).Roles on p.RoleId equals t.Id
                              join t2 in ((ApplicationDbContext)_context).Users on p.UserId equals t2.Id
                              where p.UserId == UserId && p.ExpiryDate != null && p.ExpiryDate >= Today
                              select new UserRoleViewModel
                              {
                                  UserId = t2.Id,
                                  ExpiryDate = p.ExpiryDate,
                                  RoleName = t.Name,
                              }).ToList();

            var GroupList = (from p in RoleIdList
                             group p by p.ExpiryDate into g
                             select new UserRoleViewModel
                             {
                                 UserId = g.Max(m => m.UserId),
                                 ExpiryDate = g.Key,
                                 RolesList = string.Join(",", g.Select(m => m.RoleName).ToList()),
                             }).ToList();

            return GroupList;
        }

        public void DeleteTempUserRoles(DateTime ExpiryDate, string UserId)
        {
            string OldUserRoles = "Temporary Roles Deleted From User " + UserId + " RoleIds: ";

            var TempUserRoleRecords = (from p in ((ApplicationDbContext)_context).UserRole
                                       where p.UserId == UserId && p.ExpiryDate == ExpiryDate
                                       select p).ToList();

            OldUserRoles += string.Join(",", TempUserRoleRecords.Select(m => m.RoleId).ToList());

            foreach (var item in TempUserRoleRecords)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                ((ApplicationDbContext)_context).UserRole.Remove(item);
            }

            (_context).SaveChanges();

            var DocTypeId = ((ApplicationDbContext)_context).DocumentType.Where(m => m.DocumentTypeName == MasterDocTypeConstants.UserRoles).FirstOrDefault().DocumentTypeId;

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = DocTypeId,
                DocId = 0,
                ActivityType = (int)ActivityTypeContants.Deleted,
                Narration = OldUserRoles,
            }));
        }

        public IEnumerable<string> GetUserRolesForSession(string UserId)
        {
            var Today = DateTime.Now.Date;

            return (from p in ((ApplicationDbContext)_context).UserRole
                    join t in ((ApplicationDbContext)_context).Roles on p.RoleId equals t.Id
                    where p.UserId == UserId && (p.ExpiryDate == null || p.ExpiryDate >= Today)
                    group t by t.Id into g
                    select g.Max(m => m.Name)).ToList();

        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
