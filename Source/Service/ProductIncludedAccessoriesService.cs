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
    public interface IProductIncludedAccessoriesService : IDisposable
    {
        ProductIncludedAccessories Create(ProductIncludedAccessories pt);
        void Delete(int id);
        void Delete(ProductIncludedAccessories pt);
        ProductIncludedAccessories GetProductIncludedAccessories(int ptId);
        IEnumerable<ProductIncludedAccessories> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductIncludedAccessories pt);
        ProductIncludedAccessories Add(ProductIncludedAccessories pt);
        IEnumerable<ProductIncludedAccessories> GetProductIncludedAccessoriesList();
        IEnumerable<ProductIncludedAccessories> GetProductIncludedAccessoriesList(int id);
        ProductIncludedAccessories Find(int id);
        // IEnumerable<ProductIncludedAccessories> GetProductIncludedAccessoriesList(int buyerId);
        Task<IEquatable<ProductIncludedAccessories>> GetAsync();
        Task<ProductIncludedAccessories> FindAsync(int id);      
    }

    public class ProductIncludedAccessoriesService : IProductIncludedAccessoriesService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductIncludedAccessories> _ProductIncludedAccessoriesRepository;
        RepositoryQuery<ProductIncludedAccessories> ProductIncludedAccessoriesRepository;
        public ProductIncludedAccessoriesService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductIncludedAccessoriesRepository = new Repository<ProductIncludedAccessories>(db);
            ProductIncludedAccessoriesRepository = new RepositoryQuery<ProductIncludedAccessories>(_ProductIncludedAccessoriesRepository);
        }

        public ProductIncludedAccessories GetProductIncludedAccessories(int pt)
        {
            //return ProductIncludedAccessoriesRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            //return _unitOfWork.Repository<ProductIncludedAccessories>().Find(pt);
            //return ProductIncludedAccessoriesRepository.Get().Where(i => i.ProductIncludedAccessoriesId == pt).FirstOrDefault();

            return ProductIncludedAccessoriesRepository.Include(r => r.Product).Include(r => r.Accessory).Get().Where(i => i.ProductIncludedAccessoriesId == pt).FirstOrDefault();
        }

        public ProductIncludedAccessories Create(ProductIncludedAccessories pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductIncludedAccessories>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductIncludedAccessories>().Delete(id);
        }

        public void Delete(ProductIncludedAccessories pt)
        {
            _unitOfWork.Repository<ProductIncludedAccessories>().Delete(pt);
        }

        public void Update(ProductIncludedAccessories pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductIncludedAccessories>().Update(pt);
        }

        public IEnumerable<ProductIncludedAccessories> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductIncludedAccessories>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Accessory ))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductIncludedAccessories> GetProductIncludedAccessoriesList()
        {
            var pt = _unitOfWork.Repository<ProductIncludedAccessories>().Query().Include(p => p.Accessory).Include(p =>  p.Product).Get();
            return pt;
        }

        public ProductIncludedAccessories Add(ProductIncludedAccessories pt)
        {
            _unitOfWork.Repository<ProductIncludedAccessories>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductIncludedAccessories>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductIncludedAccessories> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<ProductIncludedAccessories> GetProductIncludedAccessoriesList(int id)
        {
            var pt = _unitOfWork.Repository<ProductIncludedAccessories>().Query().Include(m=>m.Accessory).Include(m=>m.Product).Get().Where(m => m.Product.ProductId == id).ToList();
            return pt;
        }
        public ProductIncludedAccessories Find(int id)
        {
            return _unitOfWork.Repository<ProductIncludedAccessories>().Find(id);
        }
    }
}
