using Data.Infrastructure;
using Data.Models;
using Model;
using Model.Models;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;

namespace Service
{
    public interface IProductContentHeaderService : IDisposable
    {
        ProductContentHeader Create(ProductContentHeader s);
        void Delete(int id);
        void Delete(ProductContentHeader s);
        ProductContentHeader GetProductContentHeader(int id);
        IQueryable<ProductContentHeader> GetProductContentHeaderList();
        ProductContentHeader Find(int id);
        void Update(ProductContentHeader s);
        string GetMaxDocNo();
        ProductContentHeader FindByDocNo(string Docno);
        int NextId(int id);
        int PrevId(int id);
    }
    public class ProductContentHeaderService : IProductContentHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public ProductContentHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

     public ProductContentHeader Create(ProductContentHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductContentHeader>().Insert(s);
            return s;
        }
        public IQueryable<ProductContentHeader> GetProductContentHeaderList()
     {
         return (from p in db.ProductContentHeader
                 orderby p.ProductContentName
                 select p
             );
     }

       public void Delete(int id)
     {
         _unitOfWork.Repository<ProductContentHeader>().Delete(id);
     }
       public void Delete(ProductContentHeader s)
        {
            _unitOfWork.Repository<ProductContentHeader>().Delete(s);
        }
       public void Update(ProductContentHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductContentHeader>().Update(s);            
        }

       public ProductContentHeader GetProductContentHeader(int id)
        {
            return _unitOfWork.Repository<ProductContentHeader>().Query().Get().Where(m => m.ProductContentHeaderId == id).FirstOrDefault();
        }

     

       public ProductContentHeader Find(int id)
        {
            return _unitOfWork.Repository<ProductContentHeader>().Find(id);
        }
     
        public ProductContentHeader FindByDocNo(string Docno)
       {
         return  _unitOfWork.Repository<ProductContentHeader>().Query().Get().Where(m => m.ProductContentName == Docno).FirstOrDefault();

       }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<ProductContentHeader>().Query().Get().Select(i => i.ProductContentName).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductContentHeader
                        orderby p.ProductContentName
                        select p.ProductContentHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductContentHeader
                        orderby p.ProductContentName
                        select p.ProductContentHeaderId).FirstOrDefault();
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

                temp = (from p in db.ProductContentHeader
                        orderby p.ProductContentName
                        select p.ProductContentHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductContentHeader
                        orderby p.ProductContentName
                        select p.ProductContentHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }

    }
}
