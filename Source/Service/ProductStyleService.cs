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
    public interface IProductStyleService : IDisposable
    {
        ProductStyle Create(ProductStyle pt);
        void Delete(int id);
        void Delete(ProductStyle pt);
        ProductStyle Find(string Name);
        ProductStyle Find(int id);
        IEnumerable<ProductStyle> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductStyle pt);
        ProductStyle Add(ProductStyle pt);
        IEnumerable<ProductStyle> GetProductStyleList();

        // IEnumerable<ProductStyle> GetProductStyleList(int buyerId);
        Task<IEquatable<ProductStyle>> GetAsync();
        Task<ProductStyle> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ProductStyleService : IProductStyleService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductStyle> _ProductStyleRepository;
        RepositoryQuery<ProductStyle> ProductStyleRepository;
        public ProductStyleService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductStyleRepository = new Repository<ProductStyle>(db);
            ProductStyleRepository = new RepositoryQuery<ProductStyle>(_ProductStyleRepository);
        }


        public ProductStyle Find(string Name)
        {            
            return ProductStyleRepository.Get().Where(i => i.ProductStyleName == Name).FirstOrDefault();
        }


        public ProductStyle Find(int id)
        {
            return _unitOfWork.Repository<ProductStyle>().Find(id);            
        }

        public ProductStyle Create(ProductStyle pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductStyle>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductStyle>().Delete(id);
        }

        public void Delete(ProductStyle pt)
        {
            _unitOfWork.Repository<ProductStyle>().Delete(pt);
        }

        public void Update(ProductStyle pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductStyle>().Update(pt);
        }

        public IEnumerable<ProductStyle> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductStyle>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductStyleName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductStyle> GetProductStyleList()
        {
            var pt = _unitOfWork.Repository<ProductStyle>().Query().Get().OrderBy(m=>m.ProductStyleName);

            return pt;
        }

        public ProductStyle Add(ProductStyle pt)
        {
            _unitOfWork.Repository<ProductStyle>().Insert(pt);
            return pt;
        }


        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductStyle
                        orderby p.ProductStyleName
                        select p.ProductStyleId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductStyle
                        orderby p.ProductStyleName
                        select p.ProductStyleId).FirstOrDefault();
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

                temp = (from p in db.ProductStyle
                        orderby p.ProductStyleName
                        select p.ProductStyleId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductStyle
                        orderby p.ProductStyleName
                        select p.ProductStyleId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<ProductStyle>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductStyle> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
