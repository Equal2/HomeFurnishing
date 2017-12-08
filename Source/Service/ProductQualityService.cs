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
    public interface IProductQualityService : IDisposable
    {
        ProductQuality Create(ProductQuality pt);
        void Delete(int id);
        void Delete(ProductQuality pt);
        ProductQuality Find(string Name);
        ProductQuality Find(int id);
        IEnumerable<ProductQuality> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductQuality pt);
        ProductQuality Add(ProductQuality pt);
        IQueryable<ProductQuality> GetProductQualityList(int id);

        // IEnumerable<ProductQuality> GetProductQualityList(int buyerId);
        Task<IEquatable<ProductQuality>> GetAsync();
        Task<ProductQuality> FindAsync(int id);
        int NextId(int id,int ptypeid);
        int PrevId(int id,int ptypeid);
    }

    public class ProductQualityService : IProductQualityService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductQuality> _ProductQualityRepository;
        RepositoryQuery<ProductQuality> ProductQualityRepository;
        public ProductQualityService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductQualityRepository = new Repository<ProductQuality>(db);
            ProductQualityRepository = new RepositoryQuery<ProductQuality>(_ProductQualityRepository);
        }


        public ProductQuality Find(string Name)
        {            
            return ProductQualityRepository.Get().Where(i => i.ProductQualityName == Name).FirstOrDefault();
        }


        public ProductQuality Find(int id)
        {
            return _unitOfWork.Repository<ProductQuality>().Find(id);            
        }

        public ProductQuality Create(ProductQuality pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductQuality>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductQuality>().Delete(id);
        }

        public void Delete(ProductQuality pt)
        {
            _unitOfWork.Repository<ProductQuality>().Delete(pt);
        }

        public void Update(ProductQuality pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductQuality>().Update(pt);
        }

        public IEnumerable<ProductQuality> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductQuality>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductQualityName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<ProductQuality> GetProductQualityList(int id)
        {
            var pt = _unitOfWork.Repository<ProductQuality>().Query().Get().Where(m=>m.ProductTypeId==id).OrderBy(m=>m.ProductQualityName);

            return pt;
        }

        public ProductQuality Add(ProductQuality pt)
        {
            _unitOfWork.Repository<ProductQuality>().Insert(pt);
            return pt;
        }

        public ProductQuality FindProductQuality(int id)
        {
            return (from p in db.ProductQuality
                    join t in db.FinishedProduct on p.ProductQualityId equals t.ProductQualityId
                    where t.ProductId == id
                    select p
                        ).FirstOrDefault();
        }

        public int NextId(int id,int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductQuality
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductQualityName
                        select p.ProductQualityId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductQuality
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductQualityName
                        select p.ProductQualityId).FirstOrDefault();
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

                temp = (from p in db.ProductQuality
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductQualityName
                        select p.ProductQualityId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductQuality
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductQualityName
                        select p.ProductQualityId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductQuality>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductQuality> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
