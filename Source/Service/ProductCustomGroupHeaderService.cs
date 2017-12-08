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
    public interface IProductCustomGroupHeaderService : IDisposable
    {
        ProductCustomGroupHeader Create(ProductCustomGroupHeader s);
        void Delete(int id);
        void Delete(ProductCustomGroupHeader s);
        ProductCustomGroupHeader GetProductCustomGroupHeader(int id);
        IQueryable<ProductCustomGroupHeader> GetProductCustomGroupHeaderList();
        ProductCustomGroupHeader Find(int id);
        void Update(ProductCustomGroupHeader s);
        string GetMaxDocNo();
        ProductCustomGroupHeader FindByDocNo(string Docno);
        int NextId(int id);
        int PrevId(int id);
    }
    public class ProductCustomGroupHeaderService : IProductCustomGroupHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public ProductCustomGroupHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

     public ProductCustomGroupHeader Create(ProductCustomGroupHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductCustomGroupHeader>().Insert(s);
            return s;
        }
        public IQueryable<ProductCustomGroupHeader> GetProductCustomGroupHeaderList()
     {
         return (from p in db.ProductCustomGroupHeader
                 orderby p.ProductCustomGroupName
                 select p
             );
     }

       public void Delete(int id)
     {
         _unitOfWork.Repository<ProductCustomGroupHeader>().Delete(id);
     }
       public void Delete(ProductCustomGroupHeader s)
        {
            _unitOfWork.Repository<ProductCustomGroupHeader>().Delete(s);
        }
       public void Update(ProductCustomGroupHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductCustomGroupHeader>().Update(s);            
        }

       public ProductCustomGroupHeader GetProductCustomGroupHeader(int id)
        {
            return _unitOfWork.Repository<ProductCustomGroupHeader>().Query().Get().Where(m => m.ProductCustomGroupId == id).FirstOrDefault();
        }

     

       public ProductCustomGroupHeader Find(int id)
        {
            return _unitOfWork.Repository<ProductCustomGroupHeader>().Find(id);
        }
     
        public ProductCustomGroupHeader FindByDocNo(string Docno)
       {
         return  _unitOfWork.Repository<ProductCustomGroupHeader>().Query().Get().Where(m => m.ProductCustomGroupName == Docno).FirstOrDefault();

       }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<ProductCustomGroupHeader>().Query().Get().Select(i => i.ProductCustomGroupName).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductCustomGroupHeader
                        orderby p.ProductCustomGroupName
                        select p.ProductCustomGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductCustomGroupHeader
                        orderby p.ProductCustomGroupName
                        select p.ProductCustomGroupId).FirstOrDefault();
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

                temp = (from p in db.ProductCustomGroupHeader
                        orderby p.ProductCustomGroupName
                        select p.ProductCustomGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductCustomGroupHeader
                        orderby p.ProductCustomGroupName
                        select p.ProductCustomGroupId).AsEnumerable().LastOrDefault();
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
