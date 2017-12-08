using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModels;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModel;


namespace Service
{
    public interface IUserRolesNewService : IDisposable
    {
        UserRole Create(UserRole s);
        void Delete(int id);
        void Delete(UserRole s);
        UserRole GetUserRole(int id);
        UserRole Find(int id);
        void Update(UserRole s);
        IEnumerable<UserRoleIndexViewModel> GetUserRoleListForIndex(string id);
    }

    public class UserRolesNewService : IUserRolesNewService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public UserRolesNewService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<UserRoleIndexViewModel> GetUserRoleListForIndex(string id)
        {
            return (from Ur in db.UserRole
                    join R in db.Roles on Ur.RoleId equals R.Id into RolesTable from RolesTab in RolesTable.DefaultIfEmpty()
                    where Ur.UserId == id
                    select new UserRoleIndexViewModel
                    {
                        UserRoleId = Ur.UserRoleId,
                        RoleId = RolesTab.Id,
                        RoleName = RolesTab.Name,
                        SiteName = Ur.Site.SiteName,
                        DivisionName = Ur.Division.DivisionName
                    });
        }

        public UserRole Create(UserRole S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<UserRole>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<UserRole>().Delete(id);
        }

        public void Delete(UserRole s)
        {
            _unitOfWork.Repository<UserRole>().Delete(s);
        }

        public void Update(UserRole s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<UserRole>().Update(s);
        }

        public UserRole GetUserRole(int id)
        {
            return _unitOfWork.Repository<UserRole>().Query().Get().Where(m => m.UserRoleId == id).FirstOrDefault();

        }
   
        public UserRole Find(int id)
        {
            return _unitOfWork.Repository<UserRole>().Find(id);
        }

        public void Dispose()
        {
        }
    }
}
