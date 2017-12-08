using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface IProductNatureService : IDisposable
    {
        ProductNature Create(ProductNature pt);
        void Delete(int id);
        void Delete(ProductNature pt);
        ProductNature Find(string Name);
        ProductNature Find(int id);
        IEnumerable<ProductNature> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductNature pt);
        ProductNature Add(ProductNature pt);
        IEnumerable<ProductNature> GetProductNatureList();
        Task<IEquatable<ProductNature>> GetAsync();
        Task<ProductNature> FindAsync(int id);
        ProductNature GetProductNatureByName(string name);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ProductNatureService : IProductNatureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ProductNature> _ProductNatureRepository;
        public ProductNatureService(IUnitOfWork unitOfWork, IRepository<ProductNature> ProductNatureRepo)
        {
            _unitOfWork = unitOfWork;
            _ProductNatureRepository = ProductNatureRepo;
        }
        public ProductNatureService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductNatureRepository = unitOfWork.Repository<ProductNature>();
        }

        public ProductNature Find(string Name)
        {
            return _ProductNatureRepository.Query().Get().Where(i => i.ProductNatureName == Name).FirstOrDefault();
        }


        public ProductNature Find(int id)
        {
            return _ProductNatureRepository.Find(id);            
        }

        public ProductNature Create(ProductNature pt)
        {
            pt.ObjectState = ObjectState.Added;
            _ProductNatureRepository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _ProductNatureRepository.Delete(id);
        }
        public ProductNature GetProductNatureByName(string name)
        {
            return (from p in _ProductNatureRepository.Instance
                    where p.ProductNatureName == name
                    select p
                        ).FirstOrDefault();
        }

        public void Delete(ProductNature pt)
        {
            _ProductNatureRepository.Delete(pt);
        }

        public void Update(ProductNature pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _ProductNatureRepository.Update(pt);
        }

        public IEnumerable<ProductNature> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _ProductNatureRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductNatureName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductNature> GetProductNatureList()
        {
            var pt = _ProductNatureRepository.Query().Get().OrderBy(m => m.ProductNatureName);

            return pt;
        }

        public ProductNature Add(ProductNature pt)
        {
            _ProductNatureRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _ProductNatureRepository.Instance
                        orderby p.ProductNatureName
                        select p.ProductNatureId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _ProductNatureRepository.Instance
                        orderby p.ProductNatureName
                        select p.ProductNatureId).FirstOrDefault();
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

                temp = (from p in _ProductNatureRepository.Instance
                        orderby p.ProductNatureName
                        select p.ProductNatureId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _ProductNatureRepository.Instance
                        orderby p.ProductNatureName
                        select p.ProductNatureId).AsEnumerable().LastOrDefault();
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


        public Task<IEquatable<ProductNature>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductNature> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
