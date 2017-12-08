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
    public interface IProductAttributeService : IDisposable
    {
        ProductAttributes Create(ProductAttributes p);
        void Delete(int id);
        void Delete(ProductAttributes p);
        ProductAttributes GetProductAttribute(int p);
        void Update(ProductAttributes p);

        IEnumerable<ProductAttributes> GetProductAttributesList();

        IEnumerable<ProductAttributes> GetProductAttributesWithPid(int id);

        IEnumerable<ProductAttributes> GetProductAttributesList(int prodyctTypeId);
        Task<IEquatable<ProductAttributes>> GetAsync();
        Task<ProductAttributes> FindAsync(int id);
        ProductAttributes Find(int ProductId, int AttributeId);
        ProductAttributes Find(int attributeId);
        
    }

    public class ProductAttributeService : IProductAttributeService
    {
        ApplicationDbContext db =new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductAttributes> _productAttributeRepository;
        RepositoryQuery<ProductAttributes> productAttributeRepositoryQry;
        public ProductAttributeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productAttributeRepository = new Repository<ProductAttributes>(db);
            productAttributeRepositoryQry = new RepositoryQuery<ProductAttributes>(_productAttributeRepository);
        }

        public ProductAttributes GetProductAttribute(int pId)
        {
            return productAttributeRepositoryQry.Get().Where(i => i.ProductAttributeId == pId).FirstOrDefault();
           //return _unitOfWork.Repository<SalesOrder>().Find(soId);
        }

        public ProductAttributes Find(int ProductID,int AttributeId)
        {
            return _unitOfWork.Repository<ProductAttributes>().Query().Get().Where(m => m.ProductId == ProductID && m.ProductTypeAttributeId == AttributeId).FirstOrDefault();
        }
        public ProductAttributes Find(int attributeId)
        {
            return _unitOfWork.Repository<ProductAttributes>().Find(attributeId);
        }
        public ProductAttributes Create(ProductAttributes p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductAttributes>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductAttributes>().Delete(id);
        }

        public void Delete(ProductAttributes p)
        {
            _unitOfWork.Repository<ProductAttributes>().Delete(p);
        }

        public void Update(ProductAttributes p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductAttributes>().Update(p);
        }


        public IEnumerable<ProductAttributes> GetProductAttributesList()
        {
            var p = _unitOfWork.Repository<ProductAttributes>().Query().Get();           
            
            return p;
        }



        public IEnumerable<ProductAttributes> GetProductAttributesList(int productTypeId)
        {
           // var p = _unitOfWork.Repository<ProductAttributes>().Query().Get();
            //return p.Where(b => b.Product.ProductType.ProductTypeId == productTypeId);
            throw new NotImplementedException();
        }

        public IEnumerable<ProductAttributes> GetProductAttributesWithPid(int id)
        {
           // return _unitOfWork.Repository<ProductAttributes>().Query().Get().Where(m=>m.ProductId==id);

            return (from p in db.ProductAttributes
                    where p.ProductId == id
                    select p
                        );

        }




        public void Dispose()
        {
        }


        public Task<IEquatable<ProductAttributes>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductAttributes> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }


}
