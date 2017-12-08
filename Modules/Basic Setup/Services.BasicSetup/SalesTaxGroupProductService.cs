using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;

namespace Services.BasicSetup
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
        Task<IEquatable<SalesTaxGroupProduct>> GetAsync();
        Task<SalesTaxGroupProduct> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class SalesTaxGroupProductService : ISalesTaxGroupProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<SalesTaxGroupProduct> _SalesTaxGroupProductRepository;
        public SalesTaxGroupProductService(IUnitOfWork unitOfWork, IRepository<SalesTaxGroupProduct> SalesTaxGroupProdRepo)
        {
            _unitOfWork = unitOfWork;
            _SalesTaxGroupProductRepository = SalesTaxGroupProdRepo;
        }
        public SalesTaxGroupProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SalesTaxGroupProductRepository = unitOfWork.Repository<SalesTaxGroupProduct>();
        }


        public SalesTaxGroupProduct Find(string Name)
        {
            return _SalesTaxGroupProductRepository.Query().Get().Where(i => i.SalesTaxGroupProductName == Name).FirstOrDefault();
        }


        public SalesTaxGroupProduct Find(int id)
        {
            return _SalesTaxGroupProductRepository.Find(id);            
        }

        public SalesTaxGroupProduct Create(SalesTaxGroupProduct pt)
        {
            pt.ObjectState = ObjectState.Added;
            _SalesTaxGroupProductRepository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _SalesTaxGroupProductRepository.Delete(id);
        }

        public void Delete(SalesTaxGroupProduct pt)
        {
            _SalesTaxGroupProductRepository.Delete(pt);
        }

        public void Update(SalesTaxGroupProduct pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _SalesTaxGroupProductRepository.Update(pt);
        }

        public IEnumerable<SalesTaxGroupProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _SalesTaxGroupProductRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SalesTaxGroupProductName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SalesTaxGroupProduct> GetSalesTaxGroupProductList()
        {
            var pt = _SalesTaxGroupProductRepository.Query().Get().OrderBy(m => m.SalesTaxGroupProductName);

            return pt;
        }

        public SalesTaxGroupProduct Add(SalesTaxGroupProduct pt)
        {
            _SalesTaxGroupProductRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _SalesTaxGroupProductRepository.Instance
                        orderby p.SalesTaxGroupProductName
                        select p.SalesTaxGroupProductId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _SalesTaxGroupProductRepository.Instance
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

                temp = (from p in _SalesTaxGroupProductRepository.Instance
                        orderby p.SalesTaxGroupProductName
                        select p.SalesTaxGroupProductId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _SalesTaxGroupProductRepository.Instance
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
            _unitOfWork.Dispose();
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
