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
    public interface IProductSampleAttributeService : IDisposable
    {
        ProductSampleAttributes Create(ProductSampleAttributes p);
        void Delete(int id);
        void Delete(ProductSampleAttributes p);
        ProductSampleAttributes GetProductSampleAttribute(int p);
        void Update(ProductSampleAttributes p);

        IEnumerable<ProductSampleAttributes> GetProductSampleAttributeList();

        IEnumerable<ProductSampleAttributes> GetProductSampleAttributeList(int prodyctTypeId);
        Task<IEquatable<ProductSampleAttributes>> GetAsync();
        Task<ProductSampleAttributes> FindAsync(int id);
        IEnumerable<ProductSampleAttributes> GetProductSampleAttributeListWithSampleId(int ProductSampleId);
        
    }

    public class ProductSampleAttributeService : IProductSampleAttributeService
    {
        ApplicationDbContext db =new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductSampleAttributes> _productSampleAttributesRepository;
        RepositoryQuery<ProductSampleAttributes> productSampleAttributesRepositoryQry;
        public ProductSampleAttributeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productSampleAttributesRepository = new Repository<ProductSampleAttributes>(db);
            productSampleAttributesRepositoryQry = new RepositoryQuery<ProductSampleAttributes>(_productSampleAttributesRepository);
        }

        public ProductSampleAttributes GetProductSampleAttribute(int pId)
        {
            return productSampleAttributesRepositoryQry.Get().Where(i => i.ProductSampleAttributeId == pId).FirstOrDefault();
           // return _unitOfWork.Repository<ProductSampleAttributes>().Query().Include(m => m.ProductTypeAttribute).Get().Where(m => m.ProductSampleAttributeId == pId).FirstOrDefault();
           //return _unitOfWork.Repository<SalesOrder>().Find(soId);
        }

        public IEnumerable<ProductSampleAttributes> GetProductSampleAttributeListWithSampleId(int ProductSampleId)
        {
            return _unitOfWork.Repository<ProductSampleAttributes>().Query().Include(m => m.ProductTypeAttribute).Get().Where(m => m.ProductSampleID == ProductSampleId).OrderBy(m=>m.ProductTypeAttributeId).ToList();
        }

        public ProductSampleAttributes Create(ProductSampleAttributes p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductSampleAttributes>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductSampleAttributes>().Delete(id);
        }

        public void Delete(ProductSampleAttributes p)
        {
            _unitOfWork.Repository<ProductSampleAttributes>().Delete(p);
        }

        public void Update(ProductSampleAttributes p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductSampleAttributes>().Update(p);
        }


        public IEnumerable<ProductSampleAttributes> GetProductSampleAttributeList()
        {
            var p = _unitOfWork.Repository<ProductSampleAttributes>().Query().Get();           
            
            return p;
        }



        public IEnumerable<ProductSampleAttributes> GetProductSampleAttributeList(int productTypeId)
        {
           // var p = _unitOfWork.Repository<ProductAttributes>().Query().Get();
            //return p.Where(b => b.Product.ProductType.ProductTypeId == productTypeId);
            throw new NotImplementedException();
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<ProductSampleAttributes>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductSampleAttributes> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }


}
