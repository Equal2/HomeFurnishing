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
    public interface IProductDesignPatternService : IDisposable
    {
        ProductDesignPattern Create(ProductDesignPattern pt);
        void Delete(int id);
        void Delete(ProductDesignPattern pt);
        ProductDesignPattern GetProductDesignPattern(int ptId);
        ProductDesignPattern Find(string Name);
        ProductDesignPattern Find(int id);
        IEnumerable<ProductDesignPattern> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductDesignPattern pt);
        ProductDesignPattern Add(ProductDesignPattern pt);
        IEnumerable<ProductDesignPattern> GetProductDesignPatternList(int id);

        // IEnumerable<ProductDesignPattern> GetProductDesignPatternList(int buyerId);
        Task<IEquatable<ProductDesignPattern>> GetAsync();
        Task<ProductDesignPattern> FindAsync(int id);
        int NextId(int id,int ptypeid);
        int PrevId(int id,int ptypeid);
    }

    public class ProductDesignPatternService : IProductDesignPatternService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductDesignPattern> _ProductDesignPatternRepository;
        RepositoryQuery<ProductDesignPattern> ProductDesignPatternRepository;
        public ProductDesignPatternService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductDesignPatternRepository = new Repository<ProductDesignPattern>(db);
            ProductDesignPatternRepository = new RepositoryQuery<ProductDesignPattern>(_ProductDesignPatternRepository);
        }

        public ProductDesignPattern GetProductDesignPattern(int pt)
        {            
            //return _unitOfWork.Repository<ProductDesignPattern>().Find(pt);
            return ProductDesignPatternRepository.Get().Where(i => i.ProductDesignPatternId == pt).FirstOrDefault();
        }


        public ProductDesignPattern Find(string Name)
        {            
            return ProductDesignPatternRepository.Get().Where(i => i.ProductDesignPatternName == Name).FirstOrDefault();
        }


        public ProductDesignPattern Find(int id)
        {
            return _unitOfWork.Repository<ProductDesignPattern>().Find(id);            
        }

        public ProductDesignPattern Create(ProductDesignPattern pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductDesignPattern>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductDesignPattern>().Delete(id);
        }

        public void Delete(ProductDesignPattern pt)
        {
            _unitOfWork.Repository<ProductDesignPattern>().Delete(pt);
        }

        public void Update(ProductDesignPattern pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductDesignPattern>().Update(pt);
        }

        public IEnumerable<ProductDesignPattern> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductDesignPattern>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductDesignPatternName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductDesignPattern> GetProductDesignPatternList(int id)
        {
            var pt = _unitOfWork.Repository<ProductDesignPattern>().Query().Get().Where(m=>m.ProductTypeId==id).OrderBy(m=>m.ProductDesignPatternName);

            return pt;
        }

        public ProductDesignPattern Add(ProductDesignPattern pt)
        {
            _unitOfWork.Repository<ProductDesignPattern>().Insert(pt);
            return pt;
        }

        public int NextId(int id,int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductDesignPattern                        
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductDesignPatternName
                        select p.ProductDesignPatternId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductDesignPattern                        
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductDesignPatternName
                        select p.ProductDesignPatternId).FirstOrDefault();
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

                temp = (from p in db.ProductDesignPattern
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductDesignPatternName
                        select p.ProductDesignPatternId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductDesignPattern
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductDesignPatternName
                        select p.ProductDesignPatternId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<ProductDesignPattern>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductDesignPattern> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
