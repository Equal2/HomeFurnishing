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
    public interface IProductRelatedAccessoriesService : IDisposable
    {
        ProductRelatedAccessories Create(ProductRelatedAccessories pt);
        void Delete(int id);
        void Delete(ProductRelatedAccessories pt);
        ProductRelatedAccessories GetProductRelatedAccessories(int ptId);
        IEnumerable<ProductRelatedAccessories> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductRelatedAccessories pt);
        ProductRelatedAccessories Add(ProductRelatedAccessories pt);
        IEnumerable<ProductRelatedAccessories> GetProductRelatedAccessoriesList();
        IEnumerable<ProductRelatedAccessories> GetProductRelatedAccessoriesList(int id);
        ProductRelatedAccessories Find(int id);
        // IEnumerable<ProductRelatedAccessories> GetProductRelatedAccessoriesList(int buyerId);
        Task<IEquatable<ProductRelatedAccessories>> GetAsync();
        Task<ProductRelatedAccessories> FindAsync(int id);      
    }

    public class ProductRelatedAccessoriesService : IProductRelatedAccessoriesService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductRelatedAccessories> _ProductRelatedAccessoriesRepository;
        RepositoryQuery<ProductRelatedAccessories> ProductRelatedAccessoriesRepository;
        public ProductRelatedAccessoriesService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductRelatedAccessoriesRepository = new Repository<ProductRelatedAccessories>(db);
            ProductRelatedAccessoriesRepository = new RepositoryQuery<ProductRelatedAccessories>(_ProductRelatedAccessoriesRepository);
        }

        public ProductRelatedAccessories GetProductRelatedAccessories(int pt)
        {
            //return ProductRelatedAccessoriesRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            //return _unitOfWork.Repository<ProductRelatedAccessories>().Find(pt);
            //return ProductRelatedAccessoriesRepository.Get().Where(i => i.ProductRelatedAccessoriesId == pt).FirstOrDefault();

            return ProductRelatedAccessoriesRepository.Include(r => r.Product).Include(r => r.Accessory).Get().Where(i => i.ProductRelatedAccessoriesId == pt).FirstOrDefault();
        }

        public ProductRelatedAccessories Create(ProductRelatedAccessories pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductRelatedAccessories>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductRelatedAccessories>().Delete(id);
        }

        public void Delete(ProductRelatedAccessories pt)
        {
            _unitOfWork.Repository<ProductRelatedAccessories>().Delete(pt);
        }

        public void Update(ProductRelatedAccessories pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductRelatedAccessories>().Update(pt);
        }

        public IEnumerable<ProductRelatedAccessories> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductRelatedAccessories>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Accessory ))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductRelatedAccessories> GetProductRelatedAccessoriesList()
        {
            var pt = _unitOfWork.Repository<ProductRelatedAccessories>().Query().Include(p => p.Accessory).Include(p =>  p.Product).Get();
            return pt;
        }

        public ProductRelatedAccessories Add(ProductRelatedAccessories pt)
        {
            _unitOfWork.Repository<ProductRelatedAccessories>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductRelatedAccessories>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductRelatedAccessories> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<ProductRelatedAccessories> GetProductRelatedAccessoriesList(int id)
        {
            var pt = _unitOfWork.Repository<ProductRelatedAccessories>().Query().Include(m=>m.Accessory).Include(m=>m.Product).Get().Where(m => m.Product.ProductId == id).ToList();
            return pt;
        }
        public ProductRelatedAccessories Find(int id)
        {
            return _unitOfWork.Repository<ProductRelatedAccessories>().Find(id);
        }
    }
}
