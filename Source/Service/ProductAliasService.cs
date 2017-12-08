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
    public interface IProductAliasService : IDisposable
    {
        ProductAlias Create(ProductAlias pt);
        void Delete(int id);
        void Delete(ProductAlias pt);
        IEnumerable<ProductAlias> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductAlias pt);
        ProductAlias Add(ProductAlias pt);
        IEnumerable<ProductAlias> GetProductAliasList();

        // IEnumerable<ProductAlias> GetProductAliasList(int buyerId);
        Task<IEquatable<ProductAlias>> GetAsync();
        Task<ProductAlias> FindAsync(int id);
        ProductAlias Find(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ProductAliasService : IProductAliasService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductAlias> _ProductAliasRepository;
        RepositoryQuery<ProductAlias> ProductAliasRepository;
        public ProductAliasService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductAliasRepository = new Repository<ProductAlias>(db);
            ProductAliasRepository = new RepositoryQuery<ProductAlias>(_ProductAliasRepository);
        }

        public ProductAlias Find(int id)
        {
            return _unitOfWork.Repository<ProductAlias>().Find(id);
        }

        public ProductAlias Create(ProductAlias pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductAlias>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductAlias>().Delete(id);
        }

        public void Delete(ProductAlias pt)
        {
            _unitOfWork.Repository<ProductAlias>().Delete(pt);
        }

        public void Update(ProductAlias pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductAlias>().Update(pt);
        }

        public IEnumerable<ProductAlias> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductAlias>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductAliasName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductAlias> GetProductAliasList()
        {
            var pt = (from p in db.ProductAlias
                      orderby p.ProductAliasName
                      select p
                          );
            return pt;
        }

        public ProductAlias Add(ProductAlias pt)
        {
            _unitOfWork.Repository<ProductAlias>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductAlias
                        orderby p.ProductAliasName
                        select p.ProductAliasId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductAlias
                        orderby p.ProductAliasName
                        select p.ProductAliasId).FirstOrDefault();
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

                temp = (from p in db.ProductAlias
                        orderby p.ProductAliasName
                        select p.ProductAliasId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductAlias
                        orderby p.ProductAliasName
                        select p.ProductAliasId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductAlias>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductAlias> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
