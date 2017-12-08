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
    public interface IProductCategoryService : IDisposable
    {
        ProductCategory Create(ProductCategory pt);
        void Delete(int id);
        void Delete(ProductCategory pt);
        ProductCategory Find(string Name);
        ProductCategory Find(int id);
        IEnumerable<ProductCategory> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductCategory pt);
        ProductCategory Add(ProductCategory pt);
        IQueryable<ProductCategory> GetProductCategoryList(int ProductTypeId);

        // IEnumerable<ProductCategory> GetProductCategoryList(int buyerId);
        Task<IEquatable<ProductCategory>> GetAsync();
        Task<ProductCategory> FindAsync(int id);
        IEnumerable<ProductCategory> GetProductCategoryForNature(int id);
        string GetProductNatureName(int id);
        int NextId(int id, int ptypeid);
        int PrevId(int id, int ptypeid);
    }

    public class ProductCategoryService : IProductCategoryService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductCategory> _ProductCategoryRepository;
        RepositoryQuery<ProductCategory> ProductCategoryRepository;
        public ProductCategoryService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductCategoryRepository = new Repository<ProductCategory>(db);
            ProductCategoryRepository = new RepositoryQuery<ProductCategory>(_ProductCategoryRepository);
        }


        public ProductCategory Find(string Name)
        {            
            return ProductCategoryRepository.Get().Where(i => i.ProductCategoryName == Name).FirstOrDefault();
        }


        public ProductCategory Find(int id)
        {
            return _unitOfWork.Repository<ProductCategory>().Find(id);            
        }

        public ProductCategory Create(ProductCategory pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductCategory>().Insert(pt);
            return pt;
        }
        public string GetProductNatureName(int id)
        {
            return (from p in db.ProductCategory
                    join t in db.ProductTypes on p.ProductTypeId equals t.ProductTypeId into table
                    from tab in table.DefaultIfEmpty()
                    where p.ProductCategoryId == id
                    select tab.ProductNature.ProductNatureName
                        ).FirstOrDefault();
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductCategory>().Delete(id);
        }

        public void Delete(ProductCategory pt)
        {
            _unitOfWork.Repository<ProductCategory>().Delete(pt);
        }

        public void Update(ProductCategory pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductCategory>().Update(pt);
        }

        public IEnumerable<ProductCategory> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductCategory>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductCategoryName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<ProductCategory> GetProductCategoryForNature(int id)
        {
            return (from p in db.ProductCategory
                        join t in db.ProductTypes on p.ProductTypeId equals t.ProductTypeId into table from tab in table.DefaultIfEmpty()
                        where tab.ProductNatureId==id
                        select p
                        );
        }

        public IQueryable<ProductCategory> GetProductCategoryList(int ProductTypeId)
        {
            var pt = _unitOfWork.Repository<ProductCategory>().Query().Get().OrderBy(m=>m.ProductCategoryName).Where(m=>m.ProductTypeId==ProductTypeId);

            return pt;
        }

        public ProductCategory Add(ProductCategory pt)
        {
            _unitOfWork.Repository<ProductCategory>().Insert(pt);
            return pt;
        }

        public int NextId(int id, int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductCategory
                        where p.ProductTypeId == ptypeid
                        orderby p.ProductCategoryName
                        select p.ProductCategoryId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductCategory
                        where p.ProductTypeId == ptypeid
                        orderby p.ProductCategoryName
                        select p.ProductCategoryId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id, int ptypeid)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.ProductCategory
                        where p.ProductTypeId == ptypeid
                        orderby p.ProductCategoryName
                        select p.ProductCategoryId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductCategory
                        where p.ProductTypeId == ptypeid
                        orderby p.ProductCategoryName
                        select p.ProductCategoryId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<ProductCategory>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductCategory> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
