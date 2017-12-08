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
    public interface IProductCollectionService : IDisposable
    {
        ProductCollection Create(ProductCollection pt);
        void Delete(int id);
        void Delete(ProductCollection pt);
        ProductCollection GetProductCollection(int ptId);
        ProductCollection Find(string Name);
        ProductCollection Find(int id);
        IEnumerable<ProductCollection> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductCollection pt);
        ProductCollection Add(ProductCollection pt);
        IEnumerable<ProductCollection> GetProductCollectionList(int id);

        // IEnumerable<ProductCollection> GetProductCollectionList(int buyerId);
        Task<IEquatable<ProductCollection>> GetAsync();
        Task<ProductCollection> FindAsync(int id);
        int NextId(int id,int ptypeid);
        int PrevId(int id,int ptypeid);
    }

    public class ProductCollectionService : IProductCollectionService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductCollection> _ProductCollectionRepository;
        RepositoryQuery<ProductCollection> ProductCollectionRepository;
        public ProductCollectionService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductCollectionRepository = new Repository<ProductCollection>(db);
            ProductCollectionRepository = new RepositoryQuery<ProductCollection>(_ProductCollectionRepository);
        }

        public ProductCollection GetProductCollection(int pt)
        {            
            //return _unitOfWork.Repository<ProductCollection>().Find(pt);
            return ProductCollectionRepository.Get().Where(i => i.ProductCollectionId == pt).FirstOrDefault();
        }


        public ProductCollection Find(string Name)
        {            
            return ProductCollectionRepository.Get().Where(i => i.ProductCollectionName == Name).FirstOrDefault();
        }


        public ProductCollection Find(int id)
        {
            return _unitOfWork.Repository<ProductCollection>().Find(id);            
        }

        public ProductCollection Create(ProductCollection pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductCollection>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductCollection>().Delete(id);
        }

        public void Delete(ProductCollection pt)
        {
            _unitOfWork.Repository<ProductCollection>().Delete(pt);
        }

        public void Update(ProductCollection pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductCollection>().Update(pt);
        }

        public IEnumerable<ProductCollection> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductCollection>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductCollectionName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductCollection> GetProductCollectionList(int id)
        {
            var pt = _unitOfWork.Repository<ProductCollection>().Query().Get().Where(m=>m.ProductTypeId==id).OrderBy(m=>m.ProductCollectionName);

            return pt;
        }

        public ProductCollection Add(ProductCollection pt)
        {
            _unitOfWork.Repository<ProductCollection>().Insert(pt);
            return pt;
        }

        public int NextId(int id,int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductCollections                        
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductCollectionName
                        select p.ProductCollectionId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductCollections                        
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductCollectionName
                        select p.ProductCollectionId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id,int ptypeid)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.ProductCollections
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductCollectionName
                        select p.ProductCollectionId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductCollections
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductCollectionName
                        select p.ProductCollectionId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<ProductCollection>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductCollection> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
