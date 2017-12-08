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
    public interface IProductShapeService : IDisposable
    {
        ProductShape Create(ProductShape pt);
        void Delete(int id);
        void Delete(ProductShape pt);
        ProductShape Find(string Name);
        ProductShape Find(int id);
        IEnumerable<ProductShape> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductShape pt);
        ProductShape Add(ProductShape pt);
        IEnumerable<ProductShape> GetProductShapeList();

        // IEnumerable<ProductShape> GetProductShapeList(int buyerId);
        Task<IEquatable<ProductShape>> GetAsync();
        Task<ProductShape> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ProductShapeService : IProductShapeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductShape> _ProductShapeRepository;
        RepositoryQuery<ProductShape> ProductShapeRepository;
        public ProductShapeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductShapeRepository = new Repository<ProductShape>(db);
            ProductShapeRepository = new RepositoryQuery<ProductShape>(_ProductShapeRepository);
        }


        public ProductShape Find(string Name)
        {            
            return ProductShapeRepository.Get().Where(i => i.ProductShapeName == Name).FirstOrDefault();
        }


        public ProductShape Find(int id)
        {
            return _unitOfWork.Repository<ProductShape>().Find(id);            
        }

        public ProductShape Create(ProductShape pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductShape>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductShape>().Delete(id);
        }

        public void Delete(ProductShape pt)
        {
            _unitOfWork.Repository<ProductShape>().Delete(pt);
        }

        public void Update(ProductShape pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductShape>().Update(pt);
        }

        public IEnumerable<ProductShape> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductShape>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductShapeName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductShape> GetProductShapeList()
        {
            var pt = _unitOfWork.Repository<ProductShape>().Query().Get().OrderBy(m=>m.ProductShapeName);

            return pt;
        }

        public ProductShape Add(ProductShape pt)
        {
            _unitOfWork.Repository<ProductShape>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductShape
                        orderby p.ProductShapeName
                        select p.ProductShapeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductShape
                        orderby p.ProductShapeName
                        select p.ProductShapeId).FirstOrDefault();
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

                temp = (from p in db.ProductShape
                        orderby p.ProductShapeName
                        select p.ProductShapeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductShape
                        orderby p.ProductShapeName
                        select p.ProductShapeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<ProductShape>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductShape> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
