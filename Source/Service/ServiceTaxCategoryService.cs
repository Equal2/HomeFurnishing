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
    public interface IServiceTaxCategoryService : IDisposable
    {
        ServiceTaxCategory Create(ServiceTaxCategory pt);
        void Delete(int id);
        void Delete(ServiceTaxCategory pt);
        ServiceTaxCategory GetServiceTaxCategory(int ptId);
        ServiceTaxCategory Find(string Name);
        ServiceTaxCategory Find(int id);
        IEnumerable<ServiceTaxCategory> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ServiceTaxCategory pt);
        ServiceTaxCategory Add(ServiceTaxCategory pt);
        IEnumerable<ServiceTaxCategory> GetServiceTaxCategoryList();

        // IEnumerable<ServiceTaxCategory> GetServiceTaxCategoryList(int buyerId);
        Task<IEquatable<ServiceTaxCategory>> GetAsync();
        Task<ServiceTaxCategory> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ServiceTaxCategoryService : IServiceTaxCategoryService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ServiceTaxCategory> _ServiceTaxCategoryRepository;
        RepositoryQuery<ServiceTaxCategory> ServiceTaxCategoryRepository;
        public ServiceTaxCategoryService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ServiceTaxCategoryRepository = new Repository<ServiceTaxCategory>(db);
            ServiceTaxCategoryRepository = new RepositoryQuery<ServiceTaxCategory>(_ServiceTaxCategoryRepository);
        }

        public ServiceTaxCategory GetServiceTaxCategory(int pt)
        {            
            //return _unitOfWork.Repository<ServiceTaxCategory>().Find(pt);
            return ServiceTaxCategoryRepository.Get().Where(i => i.ServiceTaxCategoryId == pt).FirstOrDefault();
        }


        public ServiceTaxCategory Find(string Name)
        {            
            return ServiceTaxCategoryRepository.Get().Where(i => i.ServiceTaxCategoryName == Name).FirstOrDefault();
        }


        public ServiceTaxCategory Find(int id)
        {
            return _unitOfWork.Repository<ServiceTaxCategory>().Find(id);            
        }

        public ServiceTaxCategory Create(ServiceTaxCategory pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ServiceTaxCategory>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ServiceTaxCategory>().Delete(id);
        }

        public void Delete(ServiceTaxCategory pt)
        {
            _unitOfWork.Repository<ServiceTaxCategory>().Delete(pt);
        }

        public void Update(ServiceTaxCategory pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ServiceTaxCategory>().Update(pt);
        }

        public IEnumerable<ServiceTaxCategory> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ServiceTaxCategory>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ServiceTaxCategoryName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ServiceTaxCategory> GetServiceTaxCategoryList()
        {
            var pt = _unitOfWork.Repository<ServiceTaxCategory>().Query().Get().OrderBy(m=>m.ServiceTaxCategoryName);

            return pt;
        }

        public ServiceTaxCategory Add(ServiceTaxCategory pt)
        {
            _unitOfWork.Repository<ServiceTaxCategory>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ServiceTaxCategory                        
                        orderby p.ServiceTaxCategoryName
                        select p.ServiceTaxCategoryId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ServiceTaxCategory                        
                        orderby p.ServiceTaxCategoryName
                        select p.ServiceTaxCategoryId).FirstOrDefault();
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

                temp = (from p in db.ServiceTaxCategory
                        orderby p.ServiceTaxCategoryName
                        select p.ServiceTaxCategoryId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ServiceTaxCategory
                        orderby p.ServiceTaxCategoryName
                        select p.ServiceTaxCategoryId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<ServiceTaxCategory>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceTaxCategory> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
