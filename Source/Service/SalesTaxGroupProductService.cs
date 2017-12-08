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
    public interface ISalesTaxGroupProductService : IDisposable
    {
        SalesTaxGroupProduct Create(SalesTaxGroupProduct pt);
        void Delete(int id);
        void Delete(SalesTaxGroupProduct pt);
        SalesTaxGroupProduct Find(string Name);
        SalesTaxGroupProduct Find(int id);
        IEnumerable<SalesTaxGroupProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SalesTaxGroupProduct pt);
        SalesTaxGroupProduct Add(SalesTaxGroupProduct pt);
        IEnumerable<SalesTaxGroupProduct> GetSalesTaxGroupProductList();

        // IEnumerable<SalesTaxGroupProduct> GetSalesTaxGroupProductList(int buyerId);
        Task<IEquatable<SalesTaxGroupProduct>> GetAsync();
        Task<SalesTaxGroupProduct> FindAsync(int id);

        int NextId(int id);
        int PrevId(int id);
    }

    public class SalesTaxGroupProductService : ISalesTaxGroupProductService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SalesTaxGroupProduct> _SalesTaxGroupProductRepository;
        RepositoryQuery<SalesTaxGroupProduct> SalesTaxGroupProductRepository;
        public SalesTaxGroupProductService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SalesTaxGroupProductRepository = new Repository<SalesTaxGroupProduct>(db);
            SalesTaxGroupProductRepository = new RepositoryQuery<SalesTaxGroupProduct>(_SalesTaxGroupProductRepository);
        }


        public SalesTaxGroupProduct Find(string Name)
        {            
            return SalesTaxGroupProductRepository.Get().Where(i => i.SalesTaxGroupProductName == Name).FirstOrDefault();
        }


        public SalesTaxGroupProduct Find(int id)
        {
            return _unitOfWork.Repository<SalesTaxGroupProduct>().Find(id);            
        }

        public SalesTaxGroupProduct Create(SalesTaxGroupProduct pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SalesTaxGroupProduct>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SalesTaxGroupProduct>().Delete(id);
        }

        public void Delete(SalesTaxGroupProduct pt)
        {
            _unitOfWork.Repository<SalesTaxGroupProduct>().Delete(pt);
        }

        public void Update(SalesTaxGroupProduct pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SalesTaxGroupProduct>().Update(pt);
        }

        public IEnumerable<SalesTaxGroupProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SalesTaxGroupProduct>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SalesTaxGroupProductName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SalesTaxGroupProduct> GetSalesTaxGroupProductList()
        {
            var pt = _unitOfWork.Repository<SalesTaxGroupProduct>().Query().Get().OrderBy(m=>m.SalesTaxGroupProductName);

            return pt;
        }

        public SalesTaxGroupProduct Add(SalesTaxGroupProduct pt)
        {
            _unitOfWork.Repository<SalesTaxGroupProduct>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SalesTaxGroupProduct
                        orderby p.SalesTaxGroupProductName
                        select p.SalesTaxGroupProductId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SalesTaxGroupProduct
                        orderby p.SalesTaxGroupProductName
                        select p.SalesTaxGroupProductId).FirstOrDefault();
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

                temp = (from p in db.SalesTaxGroupProduct
                        orderby p.SalesTaxGroupProductName
                        select p.SalesTaxGroupProductId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SalesTaxGroupProduct
                        orderby p.SalesTaxGroupProductName
                        select p.SalesTaxGroupProductId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<SalesTaxGroupProduct>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SalesTaxGroupProduct> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
