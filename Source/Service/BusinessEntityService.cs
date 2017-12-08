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
    public interface IBusinessEntityService : IDisposable
    {
        BusinessEntity Create(BusinessEntity BusinessEntity);
        void Delete(int id);
        void Delete(BusinessEntity BusinessEntity);
        BusinessEntity GetBusinessEntity(int BusinessEntityId);
        IEnumerable<BusinessEntity> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(BusinessEntity BusinessEntity);
        BusinessEntity Add(BusinessEntity BusinessEntity);
        IEnumerable<BusinessEntity> GetBusinessEntityList();
        Task<IEquatable<BusinessEntity>> GetAsync();
        Task<BusinessEntity> FindAsync(int id);
        BusinessEntity GetBusinessEntityByName(string Name);
        BusinessEntity Find(int id);
    }

    public class BusinessEntityService : IBusinessEntityService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        ApplicationDbContext db = new ApplicationDbContext();
        public BusinessEntityService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public BusinessEntity GetBusinessEntityByName(string BusinessEntity)
        {
            return (from b in db.BusinessEntity
                    join p in db.Persons on b.PersonID equals p.PersonID into PersonTable from PersonTab in PersonTable.DefaultIfEmpty()
                    where PersonTab.Name == BusinessEntity
                    select b
                        ).FirstOrDefault();
        }
        public BusinessEntity GetBusinessEntity(int BusinessEntityId)
        {
            return _unitOfWork.Repository<BusinessEntity>().Find(BusinessEntityId);
        }

        public BusinessEntity Find(int id)
        {
            return _unitOfWork.Repository<BusinessEntity>().Find(id);
        }

        public BusinessEntity Create(BusinessEntity BusinessEntity)
        {
            BusinessEntity.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<BusinessEntity>().Insert(BusinessEntity);
            return BusinessEntity;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<BusinessEntity>().Delete(id);
        }

        public void Delete(BusinessEntity BusinessEntity)
        {
            _unitOfWork.Repository<BusinessEntity>().Delete(BusinessEntity);
        }

        public void Update(BusinessEntity BusinessEntity)
        {
            BusinessEntity.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<BusinessEntity>().Update(BusinessEntity);
        }

        public IEnumerable<BusinessEntity> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var BusinessEntity = _unitOfWork.Repository<BusinessEntity>()
                .Query().Include(m => m.Person)
                .OrderBy(q => q.OrderBy(c => c.Person.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Person.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return BusinessEntity;
        }

        public IEnumerable<BusinessEntity> GetBusinessEntityList()
        {
            var BusinessEntity = _unitOfWork.Repository<BusinessEntity>().Query().Include(m => m.Person).Get().Where(m => m.Person.IsActive == true).OrderBy(m => m.Person.Name);              

            return BusinessEntity;
        }

        public BusinessEntity Add(BusinessEntity BusinessEntity)
        {
            _unitOfWork.Repository<BusinessEntity>().Insert(BusinessEntity);
            return BusinessEntity;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<BusinessEntity>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BusinessEntity> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

    }

}
