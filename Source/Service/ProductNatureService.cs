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
    public interface IProductNatureService : IDisposable
    {
        ProductNature Create(ProductNature pt);
        void Delete(int id);
        void Delete(ProductNature pt);
        ProductNature Find(string Name);
        ProductNature Find(int id);
        IEnumerable<ProductNature> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductNature pt);
        ProductNature Add(ProductNature pt);
        IEnumerable<ProductNature> GetProductNatureList();

        // IEnumerable<ProductNature> GetProductNatureList(int buyerId);
        Task<IEquatable<ProductNature>> GetAsync();
        Task<ProductNature> FindAsync(int id);
        ProductNature GetProductNatureByName(string name);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ProductNatureService : IProductNatureService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductNature> _ProductNatureRepository;
        RepositoryQuery<ProductNature> ProductNatureRepository;
        public ProductNatureService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductNatureRepository = new Repository<ProductNature>(db);
            ProductNatureRepository = new RepositoryQuery<ProductNature>(_ProductNatureRepository);
        }


        public ProductNature Find(string Name)
        {            
            return ProductNatureRepository.Get().Where(i => i.ProductNatureName == Name).FirstOrDefault();
        }


        public ProductNature Find(int id)
        {
            return _unitOfWork.Repository<ProductNature>().Find(id);            
        }

        public ProductNature Create(ProductNature pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductNature>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductNature>().Delete(id);
        }
        public ProductNature GetProductNatureByName(string name)
        {
            return (from p in db.ProductNature
                    where p.ProductNatureName == name
                    select p
                        ).FirstOrDefault();
        }

        public void Delete(ProductNature pt)
        {
            _unitOfWork.Repository<ProductNature>().Delete(pt);
        }

        public void Update(ProductNature pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductNature>().Update(pt);
        }

        public IEnumerable<ProductNature> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductNature>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductNatureName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductNature> GetProductNatureList()
        {
            var pt = _unitOfWork.Repository<ProductNature>().Query().Get().OrderBy(m=>m.ProductNatureName);

            return pt;
        }

        public ProductNature Add(ProductNature pt)
        {
            _unitOfWork.Repository<ProductNature>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductNature
                        orderby p.ProductNatureName
                        select p.ProductNatureId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductNature
                        orderby p.ProductNatureName
                        select p.ProductNatureId).FirstOrDefault();
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

                temp = (from p in db.ProductNature
                        orderby p.ProductNatureName
                        select p.ProductNatureId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductNature
                        orderby p.ProductNatureName
                        select p.ProductNatureId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductNature>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductNature> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
