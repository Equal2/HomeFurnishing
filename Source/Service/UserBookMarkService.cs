using System.Collections.Generic;
using System.Linq;
using System;
using Model;
//using AdminSetup.Models.Models;
//using Infrastructure.IO;
//using ProjLib.ViewModels;
using Model.Models;
using Model.ViewModel;
using Data.Infrastructure;

namespace Service
{
    public interface IUserBookMarkService : IDisposable
    {
        UserBookMark Create(UserBookMark pt);
        void Delete(int id);
        void Delete(UserBookMark pt);
        UserBookMark Find(int id);
        void Update(UserBookMark pt);
        IEnumerable<UserBookMark> GetUserBookMarkList();
        UserBookMark FindUserBookMark(string userid, int menuid);
        void DeleteUserBookMark(string userid, int menuid);
        IEnumerable<UserBookMarkViewModel> GetUserBookMarkListForUser(string AppuserId);
        bool CheckBookMarkExists(string UserId, int MenuId);
        void AddUserBookMark(string AppUserId, int CAId);
    }

    public class UserBookMarkService : IUserBookMarkService
    {
        private readonly IRepository<UserBookMark> _userBookMarkRepository;
        private readonly IRepository<Menu> _menuRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UserBookMarkService(IUnitOfWork uow, IRepository<UserBookMark> userBookMark, IRepository<Menu> menuRepository)
        {
            _userBookMarkRepository = userBookMark;
            _unitOfWork = uow;
            _menuRepository = menuRepository;
        }


        public UserBookMark Find(int id)
        {
            return _userBookMarkRepository.Find(id);
        }
        public UserBookMark FindUserBookMark(string userid, int menuid)
        {
            return (from p in _userBookMarkRepository.Instance
                    where p.MenuId == menuid && p.ApplicationUserName == userid
                    select p
                        ).FirstOrDefault();

        }


        public IEnumerable<UserBookMarkViewModel> GetUserBookMarkListForUser(string AppuserId)
        {
            return (from p in _userBookMarkRepository.Instance
                    join t in _menuRepository.Instance on p.MenuId equals t.MenuId into table
                    from tab in table.DefaultIfEmpty()
                    where p.ApplicationUserName == AppuserId
                    select new UserBookMarkViewModel
                    {
                        MenuId = p.MenuId,
                        MenuName = tab.MenuName,
                        IconName = tab.IconName,
                    }
                        ).ToList();

        }

        public UserBookMark Create(UserBookMark pt)
        {
            pt.ObjectState = ObjectState.Added;
            _userBookMarkRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _userBookMarkRepository.Delete(id);
        }

        public void Delete(UserBookMark pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            _userBookMarkRepository.Delete(pt);
        }

        public void Update(UserBookMark pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _userBookMarkRepository.Update(pt);
        }

        public IEnumerable<UserBookMark> GetUserBookMarkList()
        {
            var pt = (from p in _userBookMarkRepository.Instance
                      select p
                          );

            return pt;
        }

        public void AddUserBookMark(string AppUserId,int CAId)
        {
            UserBookMark ub = new UserBookMark();
            ub.ApplicationUserName = AppUserId;
            ub.CreatedBy = AppUserId;
            ub.CreatedDate = DateTime.Now;
            ub.ModifiedBy = AppUserId;
            ub.ModifiedDate = DateTime.Now;
            ub.MenuId = CAId;
            ub.ObjectState = Model.ObjectState.Added;

            _userBookMarkRepository.Add(ub);

            _unitOfWork.Save();            
        }
        public bool CheckBookMarkExists(string UserId, int MenuId)
        {
            return (from p in _userBookMarkRepository.Instance
                    where p.MenuId == MenuId && p.ApplicationUserName == UserId
                    select p).Any();
        }

        public void DeleteUserBookMark(string AppUserId, int CAId)
        {
            var temp = FindUserBookMark(AppUserId, CAId);

            Delete(temp);

            _unitOfWork.Save();
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

    }
}
