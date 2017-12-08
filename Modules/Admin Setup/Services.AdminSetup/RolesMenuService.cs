using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using AdminSetup.Models.Models;
using AdminSetup.Models.ViewModels;
using Infrastructure.IO;

namespace Service
{
    public interface IRolesMenuService : IDisposable
    {
        RolesMenu Create(RolesMenu pt);
        void Delete(int id);
        void Delete(RolesMenu pt);
        RolesMenu Find(int ptId);
        void Update(RolesMenu pt);
        RolesMenu Add(RolesMenu pt);
        IEnumerable<RolesMenu> GetRolesMenuList();
        RolesMenu Find(int MenuId, string RoleId);
        IEnumerable<RolesMenuViewModel> GetRolesMenuList(string RoleId);
        int GetPermittedActionsCountForMenuId(int MenuId, string RoleId);
        int GetChildPermittedActionsCountForMenuId(int MenuId, string RoleId);
        RolesMenu GetRoleMenuForRoleId(string RoleId, int MenuId);
    }

    public class RolesMenuService : IRolesMenuService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<RolesMenu> _RolesMenuRepository;
        private readonly IRepository<Menu> _MenuRepository;
        private readonly IRepository<ControllerAction> _ControllerActionRepository;
        private readonly IRepository<MvcController> _MvcControllerRepository;
        private readonly IRepository<RolesControllerAction> _RolesControllerActionRepository;

        public RolesMenuService(IUnitOfWork unitOfWork,IRepository<RolesMenu> RolesRepo,
            IRepository<Menu> MenuRepo,
            IRepository<ControllerAction> ControllerActionRepo,
            IRepository<MvcController> MvcControllerRepo,
            IRepository<RolesControllerAction> RolesControllerRepo)
        {
            _unitOfWork = unitOfWork;
            _RolesMenuRepository = RolesRepo;
            _MenuRepository = MenuRepo;
            _ControllerActionRepository = ControllerActionRepo;
            _MvcControllerRepository = MvcControllerRepo;
            _RolesControllerActionRepository = RolesControllerRepo;
        }

        public RolesMenu Find(int pt)
        {
            return _unitOfWork.Repository<RolesMenu>().Find(pt);
        }

        public RolesMenu Create(RolesMenu pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RolesMenu>().Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RolesMenu>().Delete(id);
        }

        public void Delete(RolesMenu pt)
        {
            _unitOfWork.Repository<RolesMenu>().Delete(pt);
        }

        public void Update(RolesMenu pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RolesMenu>().Update(pt);
        }

        public IEnumerable<RolesMenu> GetRolesMenuList()
        {
            var pt = _unitOfWork.Repository<RolesMenu>().Query().Get();

            return pt;
        }


        public RolesMenu Add(RolesMenu pt)
        {
            _unitOfWork.Repository<RolesMenu>().Insert(pt);
            return pt;
        }

        public RolesMenu Find(int MenuId, string RoleId)
        {
            return _unitOfWork.Repository<RolesMenu>().Query().Get().Where(m=>m.RoleId==RoleId && m.MenuId==MenuId).FirstOrDefault();
        }

        public IEnumerable<RolesMenuViewModel> GetRolesMenuList(string RoleId)
        {
            return (from p in _RolesMenuRepository.Instance
                    where p.RoleId == RoleId
                    select new RolesMenuViewModel
                    {
                        MenuId = p.MenuId,
                        RoleId = p.RoleId,
                        RolesMenuId = p.RolesMenuId,
                        MenuName = p.Menu.MenuName,
                        RoleName = p.Role.Name,
                        FullHeaderPermission=p.FullHeaderPermission,
                        FullLinePermission=p.FullLinePermission,
                        SiteId=p.SiteId,
                        DivisionId=p.DivisionId,
                    });
        }
        public RolesMenu GetRoleMenuForRoleId(string RoleId,int MenuId)
        {

            return (from p in _RolesMenuRepository.Instance
                    where p.RoleId == RoleId && p.MenuId == MenuId
                    select p).FirstOrDefault();

        }
        public int GetPermittedActionsCountForMenuId(int MenuId, string RoleId)
        {

            int ControllerId = (from p in _MenuRepository.Instance
                                join t in _ControllerActionRepository.Instance on p.ControllerActionId equals t.ControllerActionId
                                join t2 in _MvcControllerRepository.Instance on t.ControllerId equals t2.ControllerId
                                where p.MenuId == MenuId
                                select
                                    t2.ControllerId
            ).FirstOrDefault();

            int list = (from p in _ControllerActionRepository.Instance
                        join t in _RolesControllerActionRepository.Instance.Where(m => m.RoleId == RoleId) on p.ControllerActionId equals t.ControllerActionId
                        where p.ControllerId == ControllerId
                        select t).Count();
            return list;
        }

        public int GetChildPermittedActionsCountForMenuId(int MenuId, string RoleId)
        {
            string ControllerId= string.Join(",",(from p in _MenuRepository.Instance
                                join t in _MvcControllerRepository.Instance on p.ControllerAction.ControllerId equals t.ParentControllerId
                                join t2 in _MvcControllerRepository.Instance on t.ControllerId equals t2.ControllerId
                                where p.MenuId == MenuId
                                select
                                    t2.ControllerId
            ).ToList());            

            int list = (from p in _ControllerActionRepository.Instance
                        join t in _RolesControllerActionRepository.Instance.Where(m => m.RoleId == RoleId) on p.ControllerActionId equals t.ControllerActionId
                        where ControllerId.Contains(p.ControllerId.ToString())
                        select t).Count();
            return list;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
