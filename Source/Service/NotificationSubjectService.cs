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
    public interface INotificationSubjectService : IDisposable
    {
        NotificationSubject Create(NotificationSubject pt);
        void Delete(int id);
        void Delete(NotificationSubject pt);
        NotificationSubject Find(int id);
        IEnumerable<NotificationSubject> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(NotificationSubject pt);
        NotificationSubject Add(NotificationSubject pt);
        IEnumerable<NotificationSubject> GetNotificationSubjectList();

        // IEnumerable<NotificationSubject> GetNotificationSubjectList(int buyerId);
        Task<IEquatable<NotificationSubject>> GetAsync();
        Task<NotificationSubject> FindAsync(int id);
    }

    public class NotificationSubjectService : INotificationSubjectService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<NotificationSubject> _NotificationSubjectRepository;
        RepositoryQuery<NotificationSubject> NotificationSubjectRepository;
        public NotificationSubjectService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _NotificationSubjectRepository = new Repository<NotificationSubject>(db);
            NotificationSubjectRepository = new RepositoryQuery<NotificationSubject>(_NotificationSubjectRepository);
        }

        public NotificationSubject Find(int id)
        {
            return _unitOfWork.Repository<NotificationSubject>().Find(id);
        }

        public NotificationSubject Create(NotificationSubject pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<NotificationSubject>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<NotificationSubject>().Delete(id);
        }

        public void Delete(NotificationSubject pt)
        {
            _unitOfWork.Repository<NotificationSubject>().Delete(pt);
        }

        public void Update(NotificationSubject pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<NotificationSubject>().Update(pt);
        }

        public IEnumerable<NotificationSubject> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<NotificationSubject>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<NotificationSubject> GetNotificationSubjectList()
        {
            var pt = _unitOfWork.Repository<NotificationSubject>().Query().Get();

            return pt;
        }

        public NotificationSubject Add(NotificationSubject pt)
        {
            _unitOfWork.Repository<NotificationSubject>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<NotificationSubject>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<NotificationSubject> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
