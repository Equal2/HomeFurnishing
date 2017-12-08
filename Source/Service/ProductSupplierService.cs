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
    public interface IProductSupplierService : IDisposable
    {
        ProductSupplier Create(ProductSupplier pt);
        void Delete(int id);
        void Delete(ProductSupplier pt);
        ProductSupplier GetProductSupplier(int ptId);
        IEnumerable<ProductSupplier> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductSupplier pt);
        ProductSupplier Add(ProductSupplier pt);
        IEnumerable<ProductSupplier> GetProductSupplierList();
        IEnumerable<ProductSupplier> GetProductSupplierList(int id);
        ProductSupplier Find(int id);
        // IEnumerable<ProductSupplier> GetProductSupplierList(int buyerId);
        Task<IEquatable<ProductSupplier>> GetAsync();
        Task<ProductSupplier> FindAsync(int id);      
    }

    public class ProductSupplierService : IProductSupplierService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductSupplier> _ProductSupplierRepository;
        RepositoryQuery<ProductSupplier> ProductSupplierRepository;
        public ProductSupplierService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductSupplierRepository = new Repository<ProductSupplier>(db);
            ProductSupplierRepository = new RepositoryQuery<ProductSupplier>(_ProductSupplierRepository);
        }

        public ProductSupplier GetProductSupplier(int pt)
        {
            //return ProductSupplierRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            //return _unitOfWork.Repository<ProductSupplier>().Find(pt);
            //return ProductSupplierRepository.Get().Where(i => i.ProductSupplierId == pt).FirstOrDefault();

            return ProductSupplierRepository.Get().Where(i => i.ProductSupplierId == pt).FirstOrDefault();
        }

        public ProductSupplier Create(ProductSupplier pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductSupplier>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductSupplier>().Delete(id);
        }

        public void Delete(ProductSupplier pt)
        {
            _unitOfWork.Repository<ProductSupplier>().Delete(pt);
        }

        public void Update(ProductSupplier pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductSupplier>().Update(pt);
        }

        public IEnumerable<ProductSupplier> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductSupplier>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Supplier ))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductSupplier> GetProductSupplierList()
        {
            var pt = _unitOfWork.Repository<ProductSupplier>().Query().Include(p => p.Supplier).Include(p =>  p.Product).Get();
            return pt;
        }

        public ProductSupplier Add(ProductSupplier pt)
        {
            _unitOfWork.Repository<ProductSupplier>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductSupplier>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductSupplier> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<ProductSupplier> GetProductSupplierList(int id)
        {
            var pt = _unitOfWork.Repository<ProductSupplier>().Query().Get().Where(m => m.ProductId == id).ToList();
            return pt;
        }

        public ProductSupplier GetProductSupplier(int ProductId,int SupplierId)
        {
            return _unitOfWork.Repository<ProductSupplier>().Query().Get().Where(m => m.ProductId == ProductId && m.SupplierId == SupplierId).FirstOrDefault();
        }

        public ProductSupplier GetDefaultSupplier(int ProductId)
        {
            return _unitOfWork.Repository<ProductSupplier>().Query().Get().Where(m => m.ProductId == ProductId && m.Default == true).OrderByDescending(m=>m.ProductSupplierId).FirstOrDefault();
        }

        public ProductSupplier Find(int id)
        {
            return _unitOfWork.Repository<ProductSupplier>().Find(id);
        }
    }
}
