using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;

using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
//using Models.Login.Models;

namespace Service
{
    public interface INotificationUserService : IDisposable
    {
        NotificationUser Create(NotificationUser pt);
        void Delete(int id);
        void Delete(NotificationUser pt);
        NotificationUser Find(int id);
        IEnumerable<NotificationUser> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(NotificationUser pt);
        NotificationUser Add(NotificationUser pt);
        IEnumerable<NotificationUser> GetNotificationUserList();

        // IEnumerable<NotificationUser> GetNotificationUserList(int buyerId);
        Task<IEquatable<NotificationUser>> GetAsync();
        Task<NotificationUser> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class NotificationUserService : INotificationUserService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<NotificationUser> _NotificationUserRepository;
        RepositoryQuery<NotificationUser> NotificationUserRepository;
        public NotificationUserService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _NotificationUserRepository = new Repository<NotificationUser>(db);
            NotificationUserRepository = new RepositoryQuery<NotificationUser>(_NotificationUserRepository);
        }

        public NotificationUser Find(int id)
        {
            return _unitOfWork.Repository<NotificationUser>().Find(id);

        }

        public NotificationUser Create(NotificationUser pt)
        {
            //pt.ObjectState = ObjectState.Added;
            //_unitOfWork.Repository<NotificationUser>().Insert(pt);
            pt.ObjectState = ObjectState.Added;
            db.NotificationUser.Add(pt);
            db.SaveChanges();
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<NotificationUser>().Delete(id);
        }

        public void Delete(NotificationUser pt)
        {
            _unitOfWork.Repository<NotificationUser>().Delete(pt);
        }

        public void Update(NotificationUser pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<NotificationUser>().Update(pt);
        }

        public IEnumerable<NotificationUser> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<NotificationUser>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<NotificationUser> GetNotificationUserList()
        {
            var pt = _unitOfWork.Repository<NotificationUser>().Query().Get();

            return pt;
        }

        public NotificationUser Add(NotificationUser pt)
        {
            _unitOfWork.Repository<NotificationUser>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.NotificationUser
                        select p.NotificationUserId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.NotificationUser
                        select p.NotificationUserId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.NotificationUser
                        select p.NotificationUserId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.NotificationUser
                        select p.NotificationUserId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<NotificationUser>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<NotificationUser> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
