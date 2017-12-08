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
    public interface IPersonRegistrationService : IDisposable
    {
        PersonRegistration Create(PersonRegistration pt);
        void Delete(int id);
        void Delete(PersonRegistration pt);
        PersonRegistration GetPersonRegistration(int ptId);
        IEnumerable<PersonRegistration> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PersonRegistration pt);
        PersonRegistration Add(PersonRegistration pt);
        IEnumerable<PersonRegistration> GetPersonRegistrationList();
        IEnumerable<PersonRegistration> GetPersonRegistrationList(int id);
        PersonRegistration Find(int id);
        // IEnumerable<PersonRegistration> GetPersonRegistrationList(int buyerId);
        Task<IEquatable<PersonRegistration>> GetAsync();
        Task<PersonRegistration> FindAsync(int id);
        IEnumerable<PersonRegistration> GetPersonRegistrationIdListByPersonId(int PersonId);
    }

    public class PersonRegistrationService : IPersonRegistrationService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PersonRegistration> _PersonRegistrationRepository;
        RepositoryQuery<PersonRegistration> PersonRegistrationRepository;
        public PersonRegistrationService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PersonRegistrationRepository = new Repository<PersonRegistration>(db);
            PersonRegistrationRepository = new RepositoryQuery<PersonRegistration>(_PersonRegistrationRepository);
        }

        public PersonRegistration GetPersonRegistration(int pt)
        {
            return PersonRegistrationRepository.Include(r => r.Person).Get().Where(i => i.PersonRegistrationID == pt).FirstOrDefault();
        }

        public PersonRegistration Create(PersonRegistration pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PersonRegistration>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PersonRegistration>().Delete(id);
        }

        public void Delete(PersonRegistration pt)
        {
            _unitOfWork.Repository<PersonRegistration>().Delete(pt);
        }

        public void Update(PersonRegistration pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PersonRegistration>().Update(pt);
        }

        public IEnumerable<PersonRegistration> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PersonRegistration>()
                .Query()
                //.OrderBy(q => q.OrderBy(c => c.Supplier ))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PersonRegistration> GetPersonRegistrationList()
        {
            var pt = _unitOfWork.Repository<PersonRegistration>().Query().Include(p => p.Person).Get();
            return pt;
        }

        public PersonRegistration Add(PersonRegistration pt)
        {
            _unitOfWork.Repository<PersonRegistration>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PersonRegistration>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PersonRegistration> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<PersonRegistration> GetPersonRegistrationList(int id)
        {
            var pt = _unitOfWork.Repository<PersonRegistration>().Query().Include(m=>m.Person).Get().Where(m => m.PersonId == id).ToList();
            return pt;
        }
        public PersonRegistration Find(int id)
        {
            return _unitOfWork.Repository<PersonRegistration>().Find(id);
        }

        public IEnumerable<PersonRegistration> GetPersonRegistrationIdListByPersonId(int PersonId)
        {
            var pt = _unitOfWork.Repository<PersonRegistration>().Query().Get().Where(m => m.PersonId == PersonId).ToList();
            return pt;
        }

    }
}
