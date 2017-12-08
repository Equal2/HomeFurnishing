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
using Model.ViewModel;

namespace Service
{
    public interface IDocNotificationContentService : IDisposable
    {
        DocNotificationContent Create(DocNotificationContent pt);
        void Delete(int id);
        void Delete(DocNotificationContent pt);
        DocNotificationContent Find(int id);
        IEnumerable<DocNotificationContent> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DocNotificationContent pt);
        DocNotificationContent Add(DocNotificationContent pt);
        DocNotificationContent GetDocNotificationContentForDocument(int DocTypeId, int DivisionId, int SiteId, ActivityTypeContants ActivityType);
        Task<IEquatable<DocNotificationContent>> GetAsync();
        Task<DocNotificationContent> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
    }

    public class DocNotificationContentService : IDocNotificationContentService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<DocNotificationContent> _DocNotificationContentRepository;
        RepositoryQuery<DocNotificationContent> DocNotificationContentRepository;
        public DocNotificationContentService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DocNotificationContentRepository = new Repository<DocNotificationContent>(db);
            DocNotificationContentRepository = new RepositoryQuery<DocNotificationContent>(_DocNotificationContentRepository);
        }

        public DocNotificationContent Find(int id)
        {
            return _unitOfWork.Repository<DocNotificationContent>().Find(id);
        }

        public DocNotificationContent GetDocNotificationContentForDocument(int DocTypeId, int DivisionId, int SiteId, ActivityTypeContants ActivityType)
        {
            return (from p in db.DocNotificationContent
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId && p.ActivityTypeId == (int)ActivityType
                    select p
                        ).FirstOrDefault();


        }
        public DocNotificationContent Create(DocNotificationContent pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DocNotificationContent>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<DocNotificationContent>().Delete(id);
        }

        public void Delete(DocNotificationContent pt)
        {
            _unitOfWork.Repository<DocNotificationContent>().Delete(pt);
        }

        public void Update(DocNotificationContent pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DocNotificationContent>().Update(pt);
        }

        public IEnumerable<DocNotificationContent> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<DocNotificationContent>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNotificationContentId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public DocNotificationContent Add(DocNotificationContent pt)
        {
            _unitOfWork.Repository<DocNotificationContent>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.DocNotificationContent
                        orderby p.DocNotificationContentId
                        select p.DocNotificationContentId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.DocNotificationContent
                        orderby p.DocNotificationContentId
                        select p.DocNotificationContentId).FirstOrDefault();
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

                temp = (from p in db.DocNotificationContent
                        orderby p.DocNotificationContentId
                        select p.DocNotificationContentId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.DocNotificationContent
                        orderby p.DocNotificationContentId
                        select p.DocNotificationContentId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<DocNotificationContent>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DocNotificationContent> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
