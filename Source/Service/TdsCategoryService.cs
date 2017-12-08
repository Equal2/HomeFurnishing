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
    public interface ITdsCategoryService : IDisposable
    {
        TdsCategory Create(TdsCategory pt);
        void Delete(int id);
        void Delete(TdsCategory pt);
        TdsCategory GetTdsCategory(int ptId);
        TdsCategory Find(string Name);
        TdsCategory Find(int id);
        IEnumerable<TdsCategory> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(TdsCategory pt);
        TdsCategory Add(TdsCategory pt);
        IEnumerable<TdsCategory> GetTdsCategoryList();

        // IEnumerable<TdsCategory> GetTdsCategoryList(int buyerId);
        Task<IEquatable<TdsCategory>> GetAsync();
        Task<TdsCategory> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class TdsCategoryService : ITdsCategoryService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<TdsCategory> _TdsCategoryRepository;
        RepositoryQuery<TdsCategory> TdsCategoryRepository;
        public TdsCategoryService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _TdsCategoryRepository = new Repository<TdsCategory>(db);
            TdsCategoryRepository = new RepositoryQuery<TdsCategory>(_TdsCategoryRepository);
        }

        public TdsCategory GetTdsCategory(int pt)
        {            
            //return _unitOfWork.Repository<TdsCategory>().Find(pt);
            return TdsCategoryRepository.Get().Where(i => i.TdsCategoryId == pt).FirstOrDefault();
        }


        public TdsCategory Find(string Name)
        {            
            return TdsCategoryRepository.Get().Where(i => i.TdsCategoryName == Name).FirstOrDefault();
        }


        public TdsCategory Find(int id)
        {
            return _unitOfWork.Repository<TdsCategory>().Find(id);            
        }

        public TdsCategory Create(TdsCategory pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<TdsCategory>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<TdsCategory>().Delete(id);
        }

        public void Delete(TdsCategory pt)
        {
            _unitOfWork.Repository<TdsCategory>().Delete(pt);
        }

        public void Update(TdsCategory pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<TdsCategory>().Update(pt);
        }

        public IEnumerable<TdsCategory> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<TdsCategory>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.TdsCategoryName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<TdsCategory> GetTdsCategoryList()
        {
            var pt = _unitOfWork.Repository<TdsCategory>().Query().Get().OrderBy(m=>m.TdsCategoryName);

            return pt;
        }

        public TdsCategory Add(TdsCategory pt)
        {
            _unitOfWork.Repository<TdsCategory>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.TdsCategory                        
                        orderby p.TdsCategoryName
                        select p.TdsCategoryId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.TdsCategory                        
                        orderby p.TdsCategoryName
                        select p.TdsCategoryId).FirstOrDefault();
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

                temp = (from p in db.TdsCategory
                        orderby p.TdsCategoryName
                        select p.TdsCategoryId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.TdsCategory
                        orderby p.TdsCategoryName
                        select p.TdsCategoryId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<TdsCategory>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TdsCategory> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
