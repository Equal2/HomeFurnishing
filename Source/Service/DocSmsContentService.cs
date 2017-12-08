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
    public interface IDocSmsContentService : IDisposable
    {
        DocSmsContent Create(DocSmsContent pt);
        void Delete(int id);
        void Delete(DocSmsContent pt);
        DocSmsContent Find(int id);
        IEnumerable<DocSmsContent> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DocSmsContent pt);
        DocSmsContent Add(DocSmsContent pt);
        DocSmsContent GetDocSmsContentForDocument(int DocTypeId, int DivisionId, int SiteId, ActivityTypeContants ActivityType);
        Task<IEquatable<DocSmsContent>> GetAsync();
        Task<DocSmsContent> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
    }

    public class DocSmsContentService : IDocSmsContentService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<DocSmsContent> _DocSmsContentRepository;
        RepositoryQuery<DocSmsContent> DocSmsContentRepository;
        public DocSmsContentService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DocSmsContentRepository = new Repository<DocSmsContent>(db);
            DocSmsContentRepository = new RepositoryQuery<DocSmsContent>(_DocSmsContentRepository);
        }

        public DocSmsContent Find(int id)
        {
            return _unitOfWork.Repository<DocSmsContent>().Find(id);
        }

        public DocSmsContent GetDocSmsContentForDocument(int DocTypeId, int DivisionId, int SiteId, ActivityTypeContants ActivityType)
        {
            return (from p in db.DocSmsContent
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId && p.ActivityTypeId == (int)ActivityType
                    select p
                        ).FirstOrDefault();


        }
        public DocSmsContent Create(DocSmsContent pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DocSmsContent>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<DocSmsContent>().Delete(id);
        }

        public void Delete(DocSmsContent pt)
        {
            _unitOfWork.Repository<DocSmsContent>().Delete(pt);
        }

        public void Update(DocSmsContent pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DocSmsContent>().Update(pt);
        }

        public IEnumerable<DocSmsContent> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<DocSmsContent>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocSmsContentId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public DocSmsContent Add(DocSmsContent pt)
        {
            _unitOfWork.Repository<DocSmsContent>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.DocSmsContent
                        orderby p.DocSmsContentId
                        select p.DocSmsContentId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.DocSmsContent
                        orderby p.DocSmsContentId
                        select p.DocSmsContentId).FirstOrDefault();
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

                temp = (from p in db.DocSmsContent
                        orderby p.DocSmsContentId
                        select p.DocSmsContentId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.DocSmsContent
                        orderby p.DocSmsContentId
                        select p.DocSmsContentId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<DocSmsContent>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DocSmsContent> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
