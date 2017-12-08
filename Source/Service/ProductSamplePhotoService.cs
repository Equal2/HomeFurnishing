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
    public interface IProductSamplePhotoService : IDisposable
    {
        ProductSamplePhoto Create(ProductSamplePhoto pt);
        void Delete(int id);
        void Delete(ProductSamplePhoto pt);
     
        void Update(ProductSamplePhoto pt);
        ProductSamplePhoto Add(ProductSamplePhoto pt);
        IEnumerable<ProductSamplePhoto> GetProductSamplePhoto();

        IEnumerable<ProductSamplePhoto> GetProductListPhotoByProductSampleId(int ProductSampleId);
        Task<IEquatable<ProductSamplePhoto>> GetAsync();
        Task<ProductSamplePhoto> FindAsync(int id);
    }

    public class ProductSamplePhotoService : IProductSamplePhotoService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductSamplePhoto> _ProductSamplePhotoRepository;
        RepositoryQuery<ProductSamplePhoto> ProductSamplePhotoRepository;
        public ProductSamplePhotoService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductSamplePhotoRepository = new Repository<ProductSamplePhoto>(db);
            ProductSamplePhotoRepository = new RepositoryQuery<ProductSamplePhoto>(_ProductSamplePhotoRepository);
        }

        public ProductSamplePhoto GetProductSamplePhoto(int pt)
        {
            //return ProductPhotoRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            return _unitOfWork.Repository<ProductSamplePhoto>().Find(pt);
        }

        public ProductSamplePhoto Create(ProductSamplePhoto pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductSamplePhoto>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductSamplePhoto>().Delete(id);
        }

        public void Delete(ProductSamplePhoto pt)
        {
            _unitOfWork.Repository<ProductSamplePhoto>().Delete(pt);
        }

        public void Update(ProductSamplePhoto pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductSamplePhoto>().Update(pt);
        }


        public IEnumerable<ProductSamplePhoto> GetProductSamplePhotoList()
        {
            var pt = _unitOfWork.Repository<ProductSamplePhoto>().Query().Get();

            return pt;
        }

       public IEnumerable<ProductSamplePhoto> GetProductListPhotoByProductSampleId(int ProductSampleId)
        {
            var pt = _unitOfWork.Repository<ProductSamplePhoto>().Query().Include(m=>m.ProductSample).Get().Where(m => m.ProductSample.ProductSampleId == ProductSampleId).ToList();
            return pt;
        }

        public IEnumerable<ProductSamplePhoto> GetProductSamplePhoto()
        {
            throw new NotImplementedException();
        }

      

        public ProductSamplePhoto Add(ProductSamplePhoto pt)
        {
            _unitOfWork.Repository<ProductSamplePhoto>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductSamplePhoto>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductSamplePhoto> FindAsync(int id)
        {
            throw new NotImplementedException();
        }


    }
}
