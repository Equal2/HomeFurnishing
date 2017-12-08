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
   public interface IProductTypeQaAttributeService : IDisposable
   {
       ProductTypeQaAttribute Create(ProductTypeQaAttribute pq);
       void Delete(int id);
       void Delete(ProductTypeQaAttribute pq);
       void Update(ProductTypeQaAttribute pq);
       ProductTypeQaAttribute GetProductTypeQaAttribute(int id);      
       IEnumerable<ProductTypeQaAttribute> GetProductTypeQaAttributeList();
       IEnumerable<ProductTypeQaAttribute> GetProductTypeQaAttributeListForProductTypeId(int id);
       ProductTypeQaAttribute Find(int id);
                     
   }

    public class ProductTypeQaAttributeService : IProductTypeQaAttributeService
    {

        ApplicationDbContext db = new ApplicationDbContext();

        private readonly IUnitOfWork _uniOfwork;

        public ProductTypeQaAttributeService(IUnitOfWork unitofwork)
        {
            _uniOfwork = unitofwork;
        }

        public ProductTypeQaAttribute Create(ProductTypeQaAttribute pq)
        {
            pq.ObjectState = ObjectState.Added;
            _uniOfwork.Repository<ProductTypeQaAttribute>().Insert(pq);
            return pq;
        }

        public void Delete(int id)
        {
            _uniOfwork.Repository<ProductTypeQaAttribute>().Delete(id);
        }

        public void Delete(ProductTypeQaAttribute pq)
        {
            _uniOfwork.Repository<ProductTypeQaAttribute>().Delete(pq);
        }

        public ProductTypeQaAttribute GetProductTypeQaAttribute(int id)
        {
            var pq= _uniOfwork.Repository<ProductTypeQaAttribute>().Query().Get().Where(m => m.Id == id).FirstOrDefault();
            return pq;
        }

        public void Update(ProductTypeQaAttribute pq)
        {
            pq.ObjectState = ObjectState.Modified;
            _uniOfwork.Repository<ProductTypeQaAttribute>().Update(pq);
        }

        public IEnumerable<ProductTypeQaAttribute> GetProductTypeQaAttributeList()
        {
            return _uniOfwork.Repository<ProductTypeQaAttribute>().Query().Get();
        }

        public IEnumerable<ProductTypeQaAttribute> GetProductTypeQaAttributeListForProductTypeId(int id)
        {
            return _uniOfwork.Repository<ProductTypeQaAttribute>().Query().Get().Where(m => m.ProductTypeId == id);
        }

        public ProductTypeQaAttribute Find(int id)
        {
            return _uniOfwork.Repository<ProductTypeQaAttribute>().Find(id);
        }

        public void Dispose()
        {
        }
    }
}
