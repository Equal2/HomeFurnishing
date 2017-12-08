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
    public interface IReasonService : IDisposable
    {
        Reason Create(Reason pt);
        void Delete(int id);
        void Delete(Reason pt);
        Reason Find(string Name);
        Reason Find(int id);
        void Update(Reason pt);
        Reason Add(Reason pt);
        IEnumerable<Reason> GetReasonList();
        IEnumerable<Reason> GetReasonList(string DocumentCategoryName);
        Task<IEquatable<Reason>> GetAsync();
        Task<Reason> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ReasonService : IReasonService
    {
        private ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Reason> _ReasonRepository;
        RepositoryQuery<Reason> ReasonRepository;
        public ReasonService(IUnitOfWorkForService unitOfWork)
        {
            this.db = new ApplicationDbContext();
            _unitOfWork = unitOfWork;
            _ReasonRepository = new Repository<Reason>(db);
            ReasonRepository = new RepositoryQuery<Reason>(_ReasonRepository);            
        }

        public Reason Find(string Name)
        {
            return ReasonRepository.Get().Where(i => i.ReasonName == Name).FirstOrDefault();
        }


        public Reason Find(int id)
        {
            return _unitOfWork.Repository<Reason>().Find(id);
        }

        public Reason Create(Reason pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Reason>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Reason>().Delete(id);
        }

        public void Delete(Reason pt)
        {
            _unitOfWork.Repository<Reason>().Delete(pt);
        }

        public void Update(Reason pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Reason>().Update(pt);
        }

        public IEnumerable<Reason> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Reason>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ReasonName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Reason> GetReasonList()
        {
            var pt = _unitOfWork.Repository<Reason>().Query().Get().OrderBy(m => m.ReasonName);

            return pt;
        }

        public IEnumerable<Reason> GetReasonList(string DocumentCategoryName)
        {
            //var pt = _unitOfWork.Repository<Reason>().Query().Get()
            //                .Where(i => i.DocumentCategory.DocumentCategoryName == DocumentCategoryName);

            var pt = (from p in db.Reason
                      where p.DocumentCategory.DocumentCategoryName == DocumentCategoryName
                      select p).ToList();


            return pt;
        }

        public IEnumerable<Reason> FindByDocumentType(int DocumentTypeId)
        {
            int DocumentCategoryId = db.DocumentType.Find(DocumentTypeId).DocumentCategoryId;
            return _unitOfWork.Repository<Reason>().Query().Get().Where(i => i.DocumentCategoryId == DocumentCategoryId);
        }

        public IEnumerable<Reason> FindByDocumentCategory(int DocumentCategoryId)
        {
            return _unitOfWork.Repository<Reason>().Query().Get().Where(i => i.DocumentCategoryId == DocumentCategoryId);
        }



        public Reason FindByName(string ReasonName)
        {
            return _unitOfWork.Repository<Reason>().Query().Get().Where(i => i.DocumentCategory.DocumentCategoryName == ReasonName).FirstOrDefault();
        }


        public Reason Add(Reason pt)
        {
            _unitOfWork.Repository<Reason>().Insert(pt);
            return pt;
        }


        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Reason
                        orderby p.ReasonName
                        select p.ReasonId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Reason
                        orderby p.ReasonName
                        select p.ReasonId).FirstOrDefault();
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

                temp = (from p in db.Reason
                        orderby p.ReasonName
                        select p.ReasonId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Reason
                        orderby p.ReasonName
                        select p.ReasonId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<Reason>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Reason> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
