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
    public interface IProductConstructionService : IDisposable
    {
        

        ProductCategory Create(ProductCategory pt);
        void Delete(int id);
        void Delete(ProductCategory pt);
        ProductCategory Find(string Name);
        ProductCategory Find(int id);
        IEnumerable<ProductCategory> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductCategory pt);
        ProductCategory Add(ProductCategory pt);
        IQueryable<ProductCategory> GetProductConstructionList();
        Task<IEquatable<ProductCategory>> GetAsync();
        Task<ProductCategory> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ProductConstructionService : IProductConstructionService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductCategory> _ProductConstructionRepository;
        RepositoryQuery<ProductCategory> ProductConstructionRepository;

        public ProductConstructionService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductConstructionRepository = new Repository<ProductCategory>(db);
            ProductConstructionRepository = new RepositoryQuery<ProductCategory>(_ProductConstructionRepository);
        }

        public ProductCategory Find(string Name)
        {
            return ProductConstructionRepository.Get().Where(i => i.ProductCategoryName == Name).FirstOrDefault();
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
                .Query().Include(i => i.ProductType)
                .OrderBy(q => q.OrderBy(c => c.ProductCategoryName))
                .GetPage(pageNumber, pageSize, out totalRecords)
                .Where(i => i.ProductType.ProductTypeName == ProductTypeConstants.Rug);

            return so;
        }

        public IQueryable<ProductCategory> GetProductConstructionList()
        {           

            var Con = (from p in db.ProductCategory
                       join pt in db.ProductTypes on p.ProductTypeId equals pt.ProductTypeId into ProductTypeTable
                       from ProductTypetab in ProductTypeTable.DefaultIfEmpty()
                       where ProductTypetab.ProductTypeName == ProductTypeConstants.Rug
                       orderby p.ProductCategoryName
                       select p
                        );

            return Con;
        }

        public ProductCategory Add(ProductCategory pt)
        {
            _unitOfWork.Repository<ProductCategory>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductCategory
                        where p.ProductType.ProductTypeName == ProductTypeConstants.Rug
                        orderby p.ProductCategoryName
                        select p.ProductCategoryId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductCategory
                        where p.ProductType.ProductTypeName == ProductTypeConstants.Rug
                        orderby p.ProductCategoryName
                        select p.ProductCategoryId).FirstOrDefault();
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

                temp = (from p in db.ProductCategory
                        where p.ProductType.ProductTypeName == ProductTypeConstants.Rug
                        orderby p.ProductCategoryName
                        select p.ProductCategoryId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductCategory
                        where p.ProductType.ProductTypeName == ProductTypeConstants.Rug
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
