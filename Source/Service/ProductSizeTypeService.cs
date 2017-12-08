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
    public interface IProductSizeTypeService : IDisposable
    {
        ProductSizeType Create(ProductSizeType pt);
        void Delete(int id);
        void Delete(ProductSizeType pt);
        ProductSizeType Find(string Name);
        ProductSizeType Find(int id);
        IEnumerable<ProductSizeType> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductSizeType pt);
        ProductSizeType Add(ProductSizeType pt);
        IEnumerable<ProductSizeType> GetProductSizeTypeList();

        // IEnumerable<ProductSizeType> GetProductSizeTypeList(int buyerId);
        Task<IEquatable<ProductSizeType>> GetAsync();
        Task<ProductSizeType> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ProductSizeTypeService : IProductSizeTypeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductSizeType> _ProductSizeTypeRepository;
        RepositoryQuery<ProductSizeType> ProductSizeTypeRepository;
        public ProductSizeTypeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductSizeTypeRepository = new Repository<ProductSizeType>(db);
            ProductSizeTypeRepository = new RepositoryQuery<ProductSizeType>(_ProductSizeTypeRepository);
        }


        public ProductSizeType Find(string Name)
        {            
            return ProductSizeTypeRepository.Get().Where(i => i.ProductSizeTypeName == Name).FirstOrDefault();
        }


        public ProductSizeType Find(int id)
        {
            return _unitOfWork.Repository<ProductSizeType>().Find(id);            
        }

        public ProductSizeType Create(ProductSizeType pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductSizeType>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductSizeType>().Delete(id);
        }

        public void Delete(ProductSizeType pt)
        {
            _unitOfWork.Repository<ProductSizeType>().Delete(pt);
        }

        public void Update(ProductSizeType pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductSizeType>().Update(pt);
        }

        public IEnumerable<ProductSizeType> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductSizeType>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductSizeTypeName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductSizeType> GetProductSizeTypeList()
        {
            var pt = _unitOfWork.Repository<ProductSizeType>().Query().Get().OrderBy(m=>m.ProductSizeTypeName);

            return pt;
        }

        public ProductSizeType Add(ProductSizeType pt)
        {
            _unitOfWork.Repository<ProductSizeType>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductSizeType
                        orderby p.ProductSizeTypeName
                        select p.ProductSizeTypeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductSizeType
                        orderby p.ProductSizeTypeName
                        select p.ProductSizeTypeId).FirstOrDefault();
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

                temp = (from p in db.ProductSizeType
                        orderby p.ProductSizeTypeName
                        select p.ProductSizeTypeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductSizeType
                        orderby p.ProductSizeTypeName
                        select p.ProductSizeTypeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<ProductSizeType>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductSizeType> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
