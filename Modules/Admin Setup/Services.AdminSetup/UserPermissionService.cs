using System.Collections.Generic;
using System.Linq;
using System;
using Infrastructure.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using Data;
using AdminSetup.Models.Models;

namespace Service
{
    public interface IUserPermissionService : IDisposable
    {
        void AddUserPermission(int MenuId, string UserName);
        void DeleteUserPermission(int MenuId);
        void AddPermissionForAction(int ActionId, string UserName);
        void RemovePermissionForAction(int ActionId);
        void AddPermissionForMenu(int MenuId, string UserName);
        void AddLinePermissionForMenu(int MenuId, string UserName);
        void RemovePermissionForMenu(int MenuId);
        void RemoveLinePermissionForMenu(int MenuId);
    }

    public class UserPermissionService : IUserPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRolesMenuService _rolesMenuService;
        private readonly IRolesControllerActionService _rolesControllerActionService;
        private readonly IRepository<ControllerAction> _controllerActionRepository;
        private readonly IRepository<Menu> _menuRepository;
        private readonly IRepository<RolesControllerAction> _rolesControllerActionRepository;
        private readonly IRepository<MvcController> _mvcControllerRepository;
        public UserPermissionService(IUnitOfWork uow, IRolesMenuService rolesmenuServ, IRolesControllerActionService rolesControllerActionService,
            IRepository<ControllerAction> ControllerActionRepo, IRepository<Menu> menuRepo, IRepository<RolesControllerAction> rolesControllerActionRepo,
            IRepository<MvcController> mvcControllerRepo)
        {
            _unitOfWork = uow;
            _rolesMenuService = rolesmenuServ;
            _rolesControllerActionService = rolesControllerActionService;
            _controllerActionRepository = ControllerActionRepo;
            _menuRepository = menuRepo;
            _rolesControllerActionRepository = rolesControllerActionRepo;
            _mvcControllerRepository = mvcControllerRepo;
        }


        public void AddUserPermission(int MenuId, string UserName)
        {

            string RoleId = (string)System.Web.HttpContext.Current.Session["RoleUId"];

            RolesMenu Rm = new RolesMenu();
            Rm.RoleId = RoleId;
            Rm.CreatedBy = UserName;
            Rm.CreatedDate = DateTime.Now;
            Rm.DivisionId = (int)System.Web.HttpContext.Current.Session["UserPermissionDivisionId"];
            Rm.SiteId = (int)System.Web.HttpContext.Current.Session["UserPermissionSiteId"];
            Rm.ModifiedBy = UserName;
            Rm.ModifiedDate = DateTime.Now;
            Rm.MenuId = MenuId;
            Rm.ObjectState = Model.ObjectState.Added;

            int AssignedActionsCount = _rolesMenuService.GetPermittedActionsCountForMenuId(MenuId, RoleId);
            if (AssignedActionsCount <= 0)
            {
                Rm.FullHeaderPermission = true;
            }

            int AssignedChildActionCount = _rolesMenuService.GetChildPermittedActionsCountForMenuId(MenuId, RoleId);
            if (AssignedChildActionCount <= 0)
            {
                Rm.FullLinePermission = true;
            }

            _rolesMenuService.Create(Rm);

            _unitOfWork.Save();
        }

        public void DeleteUserPermission(int MenuId)
        {
            string RoleId = (string)System.Web.HttpContext.Current.Session["RoleUId"];

            RolesMenu temp = _rolesMenuService.GetRoleMenuForRoleId(RoleId, MenuId);
            temp.ObjectState = Model.ObjectState.Deleted;
            _rolesMenuService.Delete(temp);

            _unitOfWork.Save();
        }

        public void AddPermissionForAction(int ActionId, string UserName)
        {
            string RoleId = (string)System.Web.HttpContext.Current.Session["RoleUId"];

            RolesControllerAction Rm = new RolesControllerAction();
            Rm.RoleId = RoleId;
            Rm.CreatedBy = UserName;
            Rm.CreatedDate = DateTime.Now;
            Rm.ModifiedBy = UserName;
            Rm.ModifiedDate = DateTime.Now;
            Rm.ControllerActionId = ActionId;

            _rolesControllerActionService.Create(Rm);
            _unitOfWork.Save();

        }

        public void RemovePermissionForAction(int ActionId)
        {
            string RoleId = (string)System.Web.HttpContext.Current.Session["RoleUId"];

            RolesControllerAction temp = _rolesControllerActionService.GetControllerActionForRoleId(RoleId, ActionId);

            _rolesControllerActionService.Delete(temp);

            _unitOfWork.Save();
        }

        public void AddPermissionForMenu(int MenuId, string UserName)
        {
            string RoleId = (string)System.Web.HttpContext.Current.Session["RoleUId"];

            var ControllerId = (from p in _menuRepository.Instance
                                where p.MenuId == MenuId
                                select p.ControllerAction.ControllerId).FirstOrDefault();

            List<int> ControllerActionIdList = (from p in _controllerActionRepository.Instance
                                                where p.ControllerId == ControllerId
                                                select p.ControllerActionId).ToList();

            var ExistingRoles = (from p in _rolesControllerActionRepository.Instance
                                 join t in _controllerActionRepository.Instance on p.ControllerActionId equals t.ControllerActionId
                                 where p.RoleId == RoleId && t.ControllerId == ControllerId
                                 select p
                                   );

            var PendingActionsToUpdate = (from p in ControllerActionIdList
                                          join t in ExistingRoles on p equals t.ControllerActionId
                                          into table
                                          from left in table.DefaultIfEmpty()
                                          where left == null
                                          select p);

            foreach (int item in PendingActionsToUpdate)
            {

                RolesControllerAction Rm = new RolesControllerAction();
                Rm.RoleId = RoleId;
                Rm.CreatedBy = UserName;
                Rm.CreatedDate = DateTime.Now;
                Rm.ModifiedBy = UserName;
                Rm.ModifiedDate = DateTime.Now;
                Rm.ControllerActionId = item;

                _rolesControllerActionService.Create(Rm);

            }
            _unitOfWork.Save();
        }

        public void AddLinePermissionForMenu(int MenuId, string UserName)
        {
            string RoleId = (string)System.Web.HttpContext.Current.Session["RoleUId"];

            var ControllerId = (from p in _menuRepository.Instance
                                where p.MenuId == MenuId
                                select p.ControllerAction.ControllerId).FirstOrDefault();

            string ControllerIDs = string.Join(",", (from p in _mvcControllerRepository.Instance
                                                     where p.ParentControllerId == ControllerId
                                                     select p.ControllerId
                                                      ).ToList());

            List<int> ControllerActionIdList = (from p in _controllerActionRepository.Instance
                                                where ControllerIDs.Contains(p.ControllerId.ToString())
                                                select p.ControllerActionId).ToList();

            var ExistingRoles = (from p in _rolesControllerActionRepository.Instance
                                 join t in _controllerActionRepository.Instance on p.ControllerActionId equals t.ControllerActionId
                                 where p.RoleId == RoleId && ControllerIDs.Contains(t.ControllerId.ToString())
                                 select p
                                   );

            var PendingActionsToUpdate = (from p in ControllerActionIdList
                                          join t in ExistingRoles on p equals t.ControllerActionId
                                          into table
                                          from left in table.DefaultIfEmpty()
                                          where left == null
                                          select p);

            foreach (int item in PendingActionsToUpdate)
            {
                RolesControllerAction Rm = new RolesControllerAction();
                Rm.RoleId = RoleId;
                Rm.CreatedBy = UserName;
                Rm.CreatedDate = DateTime.Now;
                Rm.ModifiedBy = UserName;
                Rm.ModifiedDate = DateTime.Now;
                Rm.ControllerActionId = item;

                _rolesControllerActionService.Create(Rm);
            }
            _unitOfWork.Save();
        }

        public void RemovePermissionForMenu(int MenuId)
        {
            string RoleId = (string)System.Web.HttpContext.Current.Session["RoleUId"];

            var ControllerActionList = (from p in _menuRepository.Instance
                                        where p.MenuId == MenuId
                                        select p.ControllerAction.ControllerId).FirstOrDefault();

            List<int> ControllerActionId = (from p in _controllerActionRepository.Instance
                                            where p.ControllerId == ControllerActionList
                                            select p.ControllerActionId).ToList();

            foreach (int item in ControllerActionId)
            {
                RolesControllerAction temp = _rolesControllerActionService.GetControllerActionForRoleId(RoleId, item);

                _rolesControllerActionService.Delete(temp);
            }
            _unitOfWork.Save();
        }

        public void RemoveLinePermissionForMenu(int MenuId)
        {
            string RoleId = (string)System.Web.HttpContext.Current.Session["RoleUId"];

            var ControllerActionId = (from p in _menuRepository.Instance
                                      where p.MenuId == MenuId
                                      select p.ControllerAction.ControllerId).FirstOrDefault();

            string ControllerIDs = string.Join(",", (from p in _mvcControllerRepository.Instance
                                                     where p.ParentControllerId == ControllerActionId
                                                     select p.ControllerId
                                                      ).ToList());

            List<int> ControllerActionIdList = (from p in _controllerActionRepository.Instance
                                                where ControllerIDs.Contains(p.ControllerId.ToString())
                                                select p.ControllerActionId).ToList();

            foreach (int item in ControllerActionIdList)
            {
                RolesControllerAction temp = _rolesControllerActionService.GetControllerActionForRoleId(RoleId, item);

                _rolesControllerActionService.Delete(temp);
            }
            _unitOfWork.Save();

        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}