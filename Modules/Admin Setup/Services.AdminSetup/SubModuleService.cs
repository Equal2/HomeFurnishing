using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using AdminSetup.Models.Models;
using AdminSetup.Models.ViewModels;
using Infrastructure.IO;

namespace Service
{
    public interface ISubModuleService : IDisposable
    {
        MenuSubModule Create(MenuSubModule pt);
        void Delete(int id);
        void Delete(MenuSubModule pt);
        MenuSubModule Find(string Name);
        MenuSubModule Find(int id);
        void Update(MenuSubModule pt);
        MenuSubModule Add(MenuSubModule pt);
        IEnumerable<MenuSubModule> GetSubModuleList();
        MenuSubModule GetSubModuleByName(string terms);
        IEnumerable<SubModuleViewModel> GetSubModuleFromModule(int id, string appuserid);//Module Id,Application User Id
        IEnumerable<SubModuleViewModel> GetSubModuleFromModuleForUsers(int id, string appuserid, List<string> RoleIds, int SiteId, int DivisionId);//Module Id,Application User Id
        IEnumerable<SubModuleViewModel> GetSubModuleFromModuleForPermissions(int id, string RoleId, int SiteId, int DivisionId);//Module Id,Application User Id
        IEnumerable<SubModuleViewModel> GetSubModule(int id, bool? RolePerm, string UserName);
    }

    public class SubModuleService : ISubModuleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRolesService _userRolesService;
        private readonly IRepository<MenuSubModule> _SubModuleRepository;
        private readonly IRepository<Menu> _MenuRepository;
        private readonly IRepository<UserBookMark> _UserBookMarkRepository;
        private readonly IRepository<RolesMenu> _RolesMenuRepository;
        public SubModuleService(IUnitOfWork unitOfWork, IRepository<MenuSubModule> submoduleRepo, IRepository<Menu> menuRepo,
            IRepository<UserBookMark> userBookMarkRepo, IRepository<RolesMenu> userRoleRepo, IUserRolesService userRolesServ)
        {
            _unitOfWork = unitOfWork;
            _SubModuleRepository = submoduleRepo;
            _MenuRepository = menuRepo;
            _UserBookMarkRepository = userBookMarkRepo;
            _RolesMenuRepository = userRoleRepo;
            _userRolesService = userRolesServ;

        }
        public MenuSubModule GetSubModuleByName(string terms)
        {
            return (from p in _SubModuleRepository.Instance
                    where p.SubModuleName == terms
                    select p).FirstOrDefault();
        }

        public MenuSubModule Find(string Name)
        {
            return _SubModuleRepository.Query().Get().Where(i => i.SubModuleName == Name).FirstOrDefault();
        }

        public IEnumerable<SubModuleViewModel> GetSubModuleFromModule(int id, string appuserid)
        {

            var temp2 = (from p in _MenuRepository.Instance
                         where (p.IsVisible.HasValue ? p.IsVisible == true : 1 == 1) && p.ModuleId == id
                         group p by new { p.SubModuleId, p.SubModule.SubModuleName } into res
                         join t in _SubModuleRepository.Instance on res.Key.SubModuleId equals t.SubModuleId into table
                         from tab in table.DefaultIfEmpty()
                         select new SubModuleViewModel
                         {
                             ModuleId = id,
                             SubModuleIconName = tab.IconName,
                             SubModuleId = res.Key.SubModuleId,
                             SubModuleName = res.Key.SubModuleName,
                             Srl = res.Max(m => m.Srl),
                             MenuViewModel = (from temp in res
                                              orderby temp.Srl
                                              select new MenuViewModel
                                              {
                                                  MenuId = temp.MenuId,
                                                  MenuName = temp.MenuName,
                                                  ModuleId = temp.ModuleId,
                                                  SubModuleId = temp.SubModuleId,
                                                  ControllerActionId = temp.ControllerActionId,
                                                  Description = temp.Description,
                                                  IconName = temp.IconName,
                                                  Srl = temp.Srl,
                                                  URL = temp.URL,
                                                  BookMarked = ((from tek in _UserBookMarkRepository.Instance
                                                                 where tek.ApplicationUserName == appuserid && tek.MenuId == temp.MenuId
                                                                 select tek).Any()
                                                                               ),
                                              }
                                         ).ToList()
                         }).ToList();

            double x = 0;
            var SubModuleList = temp2.OrderBy(m => m.SubModuleId).ThenBy(sx => double.TryParse(sx.Srl, out x) ? x : 0);



            return SubModuleList;

        }

        public IEnumerable<SubModuleViewModel> GetSubModuleFromModuleForUsers(int id, string appuserid, List<string> RoleIds, int SiteId, int DivisionId)
        {

            //Testing Block

            var Roles = _userRolesService.GetRolesList().ToList();

            var RoleId = string.Join(",", from p in Roles
                                          where RoleIds.Contains(p.Name)
                                          select p.Id.ToString());
            //End


            var temp2 = (from p in _MenuRepository.Instance
                         join t in _RolesMenuRepository.Instance on p.MenuId equals t.MenuId
                         where (p.IsVisible.HasValue ? p.IsVisible == true : 1 == 1) && p.ModuleId == id && t.SiteId == SiteId && t.DivisionId == DivisionId && RoleId.Contains(t.RoleId)
                         group p by new { p.SubModuleId, p.SubModule.SubModuleName } into res
                         join t in _SubModuleRepository.Instance on res.Key.SubModuleId equals t.SubModuleId into table
                         from tab in table.DefaultIfEmpty()
                         select new SubModuleViewModel
                         {
                             ModuleId = id,
                             SubModuleIconName = tab.IconName,
                             SubModuleId = res.Key.SubModuleId,
                             SubModuleName = res.Key.SubModuleName,
                             Srl = res.Max(m => m.Srl),
                             MenuViewModel = (from temp in res
                                              group temp by temp.MenuId into g
                                              orderby g.Max(m => m.Srl)
                                              select new MenuViewModel
                                              {
                                                  MenuId = g.Key,
                                                  MenuName = g.FirstOrDefault().MenuName,
                                                  ModuleId = g.FirstOrDefault().ModuleId,
                                                  SubModuleId = g.FirstOrDefault().SubModuleId,
                                                  ControllerActionId = g.FirstOrDefault().ControllerActionId,
                                                  Description = g.FirstOrDefault().Description,
                                                  IconName = g.FirstOrDefault().IconName,
                                                  Srl = g.FirstOrDefault().Srl,
                                                  URL = g.FirstOrDefault().URL,
                                                  BookMarked = ((from tek in _UserBookMarkRepository.Instance
                                                                 where tek.ApplicationUserName == appuserid && tek.MenuId == g.FirstOrDefault().MenuId
                                                                 select tek).Any()
                                                                               ),

                                              }
                                         ).ToList()

                         }).ToList();



            double x = 0;
            var SubModuleList = temp2.OrderBy(m => m.SubModuleId).ThenBy(sx => double.TryParse(sx.Srl, out x) ? x : 0);

            var tempSubModList = from p in SubModuleList
                                 group p by p.SubModuleId into g
                                 select g.FirstOrDefault();

            return tempSubModList;

        }



        public IEnumerable<SubModuleViewModel> GetSubModuleFromModuleForPermissions(int id, string RoleId, int SiteId, int DivisionId)
        {
            var temp2 = (from p in _MenuRepository.Instance
                         where (p.IsVisible.HasValue ? p.IsVisible == true : 1 == 1) && p.ModuleId == id
                         group p by new { p.SubModuleId, p.SubModule.SubModuleName } into res
                         join t in _SubModuleRepository.Instance on res.Key.SubModuleId equals t.SubModuleId into table
                         from tab in table.DefaultIfEmpty()
                         select new SubModuleViewModel
                         {
                             SubModuleIconName = tab.IconName,
                             ModuleId = id,
                             SubModuleId = res.Key.SubModuleId,
                             SubModuleName = res.Key.SubModuleName,
                             Srl = res.Max(m => m.Srl),
                             MenuViewModel = (from temp in res
                                              orderby temp.Srl
                                              select new MenuViewModel
                                              {
                                                  MenuId = temp.MenuId,
                                                  MenuName = temp.MenuName,
                                                  ModuleId = temp.ModuleId,
                                                  SubModuleId = temp.SubModuleId,
                                                  ControllerActionId = temp.ControllerActionId,
                                                  Description = temp.Description,
                                                  IconName = temp.IconName,
                                                  Srl = temp.Srl,
                                                  URL = temp.URL,
                                                  PermissionAssigned = ((from tek in _RolesMenuRepository.Instance
                                                                         where tek.RoleId == RoleId && tek.MenuId == temp.MenuId && tek.SiteId == SiteId && tek.DivisionId == DivisionId
                                                                         select tek).Any()
                                                                               ),

                                              }
                                         ).ToList()

                         }).ToList();



            double x = 0;
            var SubModuleList = temp2.OrderBy(m => m.SubModuleId).ThenBy(sx => double.TryParse(sx.Srl, out x) ? x : 0);



            return SubModuleList;
        }


        public MenuSubModule Find(int id)
        {
            return _unitOfWork.Repository<MenuSubModule>().Find(id);
        }

        public MenuSubModule Create(MenuSubModule pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MenuSubModule>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MenuSubModule>().Delete(id);
        }

        public void Delete(MenuSubModule pt)
        {
            _unitOfWork.Repository<MenuSubModule>().Delete(pt);
        }

        public void Update(MenuSubModule pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MenuSubModule>().Update(pt);
        }

        public IEnumerable<MenuSubModule> GetSubModuleList()
        {
            var pt = (from p in _SubModuleRepository.Instance
                      orderby p.SubModuleName
                      select p
                          );

            return pt;
        }

        public MenuSubModule Add(MenuSubModule pt)
        {
            _unitOfWork.Repository<MenuSubModule>().Insert(pt);
            return pt;
        }

        public IEnumerable<SubModuleViewModel> GetSubModule(int id, bool? RolePerm, string UserName)
        {
            int SiteID = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            List<SubModuleViewModel> vm = new List<SubModuleViewModel>();

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            string RoleId = (string)System.Web.HttpContext.Current.Session["RoleUId"];

            string appuserid = UserName;

            if (UserRoles.Contains("Admin") && RolePerm.HasValue && RolePerm.Value == true)
            {
                int RolesDivisionId = (int)System.Web.HttpContext.Current.Session["UserPermissionDivisionId"];
                int RolesSIteId = (int)System.Web.HttpContext.Current.Session["UserPermissionSiteId"];

                vm = GetSubModuleFromModuleForPermissions(id, RoleId, RolesSIteId, RolesDivisionId).ToList();
            }
            else if (UserRoles.Contains("Admin"))
            {
                vm = GetSubModuleFromModule(id, appuserid).ToList();
            }
            else
            {
                vm = GetSubModuleFromModuleForUsers(id, appuserid, UserRoles, SiteID, DivisionId).ToList();
            }

            return vm;

        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

    }
}
