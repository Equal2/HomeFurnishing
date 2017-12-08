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
    public interface IDocEmailContentService : IDisposable
    {
        DocEmailContent Create(DocEmailContent pt);
        void Delete(int id);
        void Delete(DocEmailContent pt);
        DocEmailContent Find(int id);
        IEnumerable<DocEmailContent> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DocEmailContent pt);
        DocEmailContent Add(DocEmailContent pt);
        DocEmailContent GetDocEmailContentForDocument(int DocTypeId, int DivisionId, int SiteId, ActivityTypeContants ActivityType);
        Task<IEquatable<DocEmailContent>> GetAsync();
        Task<DocEmailContent> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
    }

    public class DocEmailContentService : IDocEmailContentService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<DocEmailContent> _DocEmailContentRepository;
        RepositoryQuery<DocEmailContent> DocEmailContentRepository;
        public DocEmailContentService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DocEmailContentRepository = new Repository<DocEmailContent>(db);
            DocEmailContentRepository = new RepositoryQuery<DocEmailContent>(_DocEmailContentRepository);
        }

        public DocEmailContent Find(int id)
        {
            return _unitOfWork.Repository<DocEmailContent>().Find(id);
        }

        public DocEmailContent GetDocEmailContentForDocument(int DocTypeId, int DivisionId, int SiteId, ActivityTypeContants ActivityType)
        {
            return (from p in db.DocEmailContent
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId && p.ActivityTypeId == (int)ActivityType
                    select p
                        ).FirstOrDefault();


        }
        public DocEmailContent Create(DocEmailContent pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DocEmailContent>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<DocEmailContent>().Delete(id);
        }

        public void Delete(DocEmailContent pt)
        {
            _unitOfWork.Repository<DocEmailContent>().Delete(pt);
        }

        public void Update(DocEmailContent pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DocEmailContent>().Update(pt);
        }

        public IEnumerable<DocEmailContent> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<DocEmailContent>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocEmailContentId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public DocEmailContent Add(DocEmailContent pt)
        {
            _unitOfWork.Repository<DocEmailContent>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.DocEmailContent
                        orderby p.DocEmailContentId
                        select p.DocEmailContentId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.DocEmailContent
                        orderby p.DocEmailContentId
                        select p.DocEmailContentId).FirstOrDefault();
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

                temp = (from p in db.DocEmailContent
                        orderby p.DocEmailContentId
                        select p.DocEmailContentId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.DocEmailContent
                        orderby p.DocEmailContentId
                        select p.DocEmailContentId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<DocEmailContent>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DocEmailContent> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
