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

namespace Service
{
    public interface IDocumentTypeTimePlanService : IDisposable
    {
        DocumentTypeTimePlan Create(DocumentTypeTimePlan pt);
        void Delete(int id);
        void Delete(DocumentTypeTimePlan pt);
        IEnumerable<DocumentTypeTimePlan> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DocumentTypeTimePlan pt);
        DocumentTypeTimePlan Add(DocumentTypeTimePlan pt);
        IEnumerable<DocumentTypeTimePlan> GetDocumentTypeTimePlanList();

        // IEnumerable<DocumentTypeTimePlan> GetDocumentTypeTimePlanList(int buyerId);
        Task<IEquatable<DocumentTypeTimePlan>> GetAsync();
        Task<DocumentTypeTimePlan> FindAsync(int id);
        DocumentTypeTimePlan Find(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class DocumentTypeTimePlanService : IDocumentTypeTimePlanService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<DocumentTypeTimePlan> _DocumentTypeTimePlanRepository;
        RepositoryQuery<DocumentTypeTimePlan> DocumentTypeTimePlanRepository;
        public DocumentTypeTimePlanService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DocumentTypeTimePlanRepository = new Repository<DocumentTypeTimePlan>(db);
            DocumentTypeTimePlanRepository = new RepositoryQuery<DocumentTypeTimePlan>(_DocumentTypeTimePlanRepository);
        }

        public DocumentTypeTimePlan Find(int id)
        {
            return _unitOfWork.Repository<DocumentTypeTimePlan>().Find(id);
        }

        public DocumentTypeTimePlan Create(DocumentTypeTimePlan pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DocumentTypeTimePlan>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<DocumentTypeTimePlan>().Delete(id);
        }

        public void Delete(DocumentTypeTimePlan pt)
        {
            _unitOfWork.Repository<DocumentTypeTimePlan>().Delete(pt);
        }

        public void Update(DocumentTypeTimePlan pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DocumentTypeTimePlan>().Update(pt);
        }

        public IEnumerable<DocumentTypeTimePlan> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<DocumentTypeTimePlan>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocumentTypeTimePlanId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<DocumentTypeTimePlan> GetDocumentTypeTimePlanList()
        {
            var pt = _unitOfWork.Repository<DocumentTypeTimePlan>().Query().Get().OrderBy(M => M.DocumentTypeTimePlanId).ToList();

            return pt;
        }

        public DocumentTypeTimePlan Add(DocumentTypeTimePlan pt)
        {
            _unitOfWork.Repository<DocumentTypeTimePlan>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.DocumentTypeTimePlan
                        orderby p.DocumentTypeTimePlanId
                        select p.DocumentTypeTimePlanId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.DocumentTypeTimePlan
                        orderby p.DocumentTypeTimePlanId
                        select p.DocumentTypeTimePlanId).FirstOrDefault();
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

                temp = (from p in db.DocumentTypeTimePlan
                        orderby p.DocumentTypeTimePlanId
                        select p.DocumentTypeTimePlanId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.DocumentTypeTimePlan
                        orderby p.DocumentTypeTimePlanId
                        select p.DocumentTypeTimePlanId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<DocumentTypeTimePlan>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DocumentTypeTimePlan> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
