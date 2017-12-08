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
    public interface IProductPhotoService : IDisposable
    {
        ProductPhoto Create(ProductPhoto pt);
        void Delete(int id);
        void Delete(ProductPhoto pt);
        ProductPhoto GetProductPhoto(int ptId);
        
        void Update(ProductPhoto pt);
        ProductPhoto Add(ProductPhoto pt);
        IEnumerable<ProductPhoto> GetProductPhotoList();
        IEnumerable<ProductPhoto> GetProductPhotoList(int ProductId);

        // IEnumerable<ProductPhoto> GetProductPhotoList(int buyerId);
        Task<IEquatable<ProductPhoto>> GetAsync();
        Task<ProductPhoto> FindAsync(int id);
    }

    public class ProductPhotoService : IProductPhotoService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductPhoto> _ProductPhotoRepository;
        RepositoryQuery<ProductPhoto> ProductPhotoRepository;
        public ProductPhotoService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductPhotoRepository = new Repository<ProductPhoto>(db);
            ProductPhotoRepository = new RepositoryQuery<ProductPhoto>(_ProductPhotoRepository);
        }

        public ProductPhoto GetProductPhoto(int pt)
        {
            //return ProductPhotoRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            return _unitOfWork.Repository<ProductPhoto>().Find(pt);
        }

        public ProductPhoto Create(ProductPhoto pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductPhoto>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductPhoto>().Delete(id);
        }

        public void Delete(ProductPhoto pt)
        {
            _unitOfWork.Repository<ProductPhoto>().Delete(pt);
        }

        public void Update(ProductPhoto pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductPhoto>().Update(pt);
        }


        public IEnumerable<ProductPhoto> GetProductPhotoList()
        {
            var pt = _unitOfWork.Repository<ProductPhoto>().Query().Get();

            return pt;
        }


        public IEnumerable<ProductPhoto> GetProductPhotoList(int ProductId)
        {
            var pt = _unitOfWork.Repository<ProductPhoto>().Query().Get();

            return pt;
        }

        public ProductPhoto Add(ProductPhoto pt)
        {
            _unitOfWork.Repository<ProductPhoto>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductPhoto>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductPhoto> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
