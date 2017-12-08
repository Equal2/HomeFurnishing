using System.Collections.Generic;
using System.Linq;
using System;
//using Infrastructure.IO;
//using Models.Company.Models;
//using AdminSetup.Models.ViewModels;
//using Models.Company.ViewModels;
using AutoMapper;
//using AdminSetup.Models.Models;
using Model.ViewModel;
using Data.Infrastructure;
using Model.Models;

namespace Service
{
    public interface IAdminSetupService : IDisposable
    {
        IEnumerable<SiteDivisionSummaryViewModel> GetSiteDivisionSummary(string SiteId, string DivisionId, string RoleId);
        SiteViewModel FindSiteViewModel(int id);
        IEnumerable<object> GetMenuActions(int MenuId);
        IEnumerable<object> GetMenuLineActions(int MenuId);
        void CopyPermission(CopyRolesViewModel vm, string UserName);
    }

    public class AdminSetupService : IAdminSetupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRolesSiteService _rolesSiteService;
        private readonly IRolesDivisionService _rolesDivisionService;
        private readonly IRolesMenuService _rolesMenuService;
        private readonly IRolesControllerActionService _rolesControllerActionService;

        public AdminSetupService(IUnitOfWork uOW, IRolesSiteService rolesSiteService, IRolesDivisionService rolesDivisionService,
            IRolesMenuService rolesMenuServ, IRolesControllerActionService rolesControllerActionServ)
        {
            _unitOfWork = uOW;
            _rolesSiteService = rolesSiteService;
            _rolesDivisionService = rolesDivisionService;
            _rolesMenuService = rolesMenuServ;
            _rolesControllerActionService = rolesControllerActionServ;
        }

        public IEnumerable<SiteDivisionSummaryViewModel> GetSiteDivisionSummary(string SiteId, string DivisionId, string RoleId)
        {

            List<int> NewDivisionIds = new List<int>();
            List<int> NewSiteIds = new List<int>();

            if (!string.IsNullOrEmpty(DivisionId))
                NewDivisionIds = DivisionId.Split(',').Select(Int32.Parse).ToList();

            if (!string.IsNullOrEmpty(SiteId))
                NewSiteIds = SiteId.Split(',').Select(Int32.Parse).ToList();

            var SelectedSites = from p in _unitOfWork.Repository<Site>().Instance
                                where SiteId.Contains(p.SiteId.ToString())
                                select p;

            var SelectedDivisions = from p in _unitOfWork.Repository<Division>().Instance
                                    where DivisionId.Contains(p.DivisionId.ToString())
                                    select p;


            var summary = from p in SelectedSites
                          from t in SelectedDivisions
                          select new SiteDivisionSummaryViewModel
                          {
                              SiteId = p.SiteId,
                              SiteName = p.SiteCode,
                              SiteColour = p.ThemeColour,
                              DivisionId = t.DivisionId,
                              DivisionName = t.DivisionName,
                              DivisionColour = p.ThemeColour,
                              RoleId = RoleId,
                          };
            return summary.ToList();

        }

        public SiteViewModel FindSiteViewModel(int id)
        {
            var obj = _unitOfWork.Repository<Site>().Find(id);
            return Mapper.Map<SiteViewModel>(obj);
        }

        public IEnumerable<object> GetMenuActions(int MenuId)
        {
            string RoleId = (string)System.Web.HttpContext.Current.Session["RoleUId"];

            int ControllerId = (from p in _unitOfWork.Repository<Menu>().Instance
                                join t in _unitOfWork.Repository<ControllerAction>().Instance on p.ControllerActionId equals t.ControllerActionId
                                join t2 in _unitOfWork.Repository<MvcController>().Instance on t.ControllerId equals t2.ControllerId
                                where p.MenuId == MenuId
                                select
                                    t2.ControllerId
                        ).FirstOrDefault();

            var list = (from p in _unitOfWork.Repository<ControllerAction>().Instance
                        join t in _unitOfWork.Repository<RolesControllerAction>().Instance.Where(m => m.RoleId == RoleId) on p.ControllerActionId equals t.ControllerActionId into table
                        from tab in table.DefaultIfEmpty()
                        where p.ControllerId == ControllerId
                        orderby p.ActionName
                        select new
                        {
                            p.ControllerActionId,
                            p.ActionName,
                            IsAssigned = tab == null ? false : true,
                        }).ToList();

            return list;
        }

        public IEnumerable<object> GetMenuLineActions(int MenuId)
        {
            string RoleId = (string)System.Web.HttpContext.Current.Session["RoleUId"];

            string ControllerId = string.Join(",", (from p in _unitOfWork.Repository<Menu>().Instance
                                                    join t in _unitOfWork.Repository<MvcController>().Instance on p.ControllerAction.ControllerId equals t.ParentControllerId
                                                    join t2 in _unitOfWork.Repository<MvcController>().Instance on t.ControllerId equals t2.ControllerId
                                                    where p.MenuId == MenuId
                                                    select
                                                        t2.ControllerId
              ).ToList());

            var list = (from p in _unitOfWork.Repository<ControllerAction>().Instance
                        join t in _unitOfWork.Repository<RolesControllerAction>().Instance.Where(m => m.RoleId == RoleId) on p.ControllerActionId equals t.ControllerActionId into table
                        from tab in table.DefaultIfEmpty()
                        where ControllerId.Contains(p.ControllerId.ToString())
                        orderby p.ActionName
                        select new
                        {
                            p.ControllerActionId,
                            p.ActionName,
                            ControllerName = p.Controller.ControllerName,
                            IsAssigned = tab == null ? false : true,
                        }).ToList();

            return list;
        }

        public void CopyPermission(CopyRolesViewModel vm, string UserName)
        {

            var RolesSites = _rolesSiteService.GetRolesSiteList(vm.FromRoleId);
            var ExistingRolesSites = _rolesSiteService.GetRolesSiteList(vm.ToRoleId);

            var PendingToUpdate = from p in RolesSites
                                  join t in ExistingRolesSites on p.RolesSiteId equals t.RolesSiteId into table
                                  from left in table.DefaultIfEmpty()
                                  where left == null
                                  select p;

            foreach (var item in PendingToUpdate)
            {
                RolesSite site = new RolesSite();
                site.CreatedBy = UserName;
                site.CreatedDate = DateTime.Now;
                site.ModifiedBy = UserName;
                site.ModifiedDate = DateTime.Now;
                site.RoleId = vm.ToRoleId;
                site.SiteId = item.SiteId;
                site.ObjectState = Model.ObjectState.Added;
                _rolesSiteService.Create(site);

            }

            var RolesDivisions = _rolesDivisionService.GetRolesDivisionList(vm.FromRoleId);
            var ExistingRolesDivisions = _rolesDivisionService.GetRolesDivisionList(vm.ToRoleId);

            var PendingDivisionsToUpdate = from p in RolesDivisions
                                           join t in ExistingRolesDivisions on p.RolesDivisionId equals t.RolesDivisionId into table
                                           from left in table.DefaultIfEmpty()
                                           where left == null
                                           select p;

            foreach (var item in PendingDivisionsToUpdate)
            {
                RolesDivision division = new RolesDivision();
                division.CreatedBy = UserName;
                division.CreatedDate = DateTime.Now;
                division.DivisionId = item.DivisionId;
                division.ModifiedBy = UserName;
                division.ModifiedDate = DateTime.Now;
                division.RoleId = vm.ToRoleId;
                division.ObjectState = Model.ObjectState.Added;
                _rolesDivisionService.Create(division);
            }

            var RolesMenus = _rolesMenuService.GetRolesMenuList(vm.FromRoleId);
            var ExistingRolesMenus = _rolesMenuService.GetRolesMenuList(vm.ToRoleId);

            var PendingMenusToUpDate = from p in RolesMenus
                                       join t in ExistingRolesMenus on p.RolesMenuId equals t.RolesMenuId into table
                                       from left in table.DefaultIfEmpty()
                                       where left == null
                                       select p;

            foreach (var item in PendingMenusToUpDate)
            {
                RolesMenu menu = new RolesMenu();
                menu.CreatedBy = UserName;
                menu.CreatedDate = DateTime.Now;
                menu.FullHeaderPermission = item.FullHeaderPermission;
                menu.FullLinePermission = item.FullLinePermission;
                menu.MenuId = item.MenuId;
                menu.SiteId = item.SiteId;
                menu.DivisionId = item.DivisionId;
                menu.ModifiedBy = UserName;
                menu.ModifiedDate = DateTime.Now;
                menu.RoleId = vm.ToRoleId;
                menu.ObjectState = Model.ObjectState.Added;
                _rolesMenuService.Create(menu);
            }

            var RolesActions = _rolesControllerActionService.GetRolesControllerActionList(vm.FromRoleId);
            var ExistingRolesActions = _rolesControllerActionService.GetRolesControllerActionList(vm.ToRoleId);

            var PendingRolesActionsToUpdate = from p in RolesActions
                                              join t in ExistingRolesActions on p.RolesControllerActionId equals t.RolesControllerActionId into table
                                              from left in table.DefaultIfEmpty()
                                              where left == null
                                              select p;


            foreach (var item in PendingRolesActionsToUpdate)
            {
                RolesControllerAction Actions = new RolesControllerAction();
                Actions.ControllerActionId = item.ControllerActionId;
                Actions.CreatedBy = UserName;
                Actions.CreatedDate = DateTime.Now;
                Actions.ModifiedBy = UserName;
                Actions.ModifiedDate = DateTime.Now;
                Actions.RoleId = vm.ToRoleId;
                Actions.ObjectState = Model.ObjectState.Added;
                _rolesControllerActionService.Create(Actions);
            }

            _unitOfWork.Save();
        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
