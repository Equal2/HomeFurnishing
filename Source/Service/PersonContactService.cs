using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModels;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;

namespace Service
{
    public interface IPersonContactService : IDisposable
    {
        PersonContact Create(PersonContact pt);
        void Delete(int id);
        void Delete(PersonContact pt);
        PersonContact GetPersonContact(int ptId);
        IEnumerable<PersonContact> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PersonContact pt);
        PersonContact Add(PersonContact pt);
        IEnumerable<PersonContact> GetPersonContactList();
        IEnumerable<PersonContact> GetPersonContactList(int id);
        IQueryable<PersonContactViewModel> GetPersonContactListForIndex(int PersonId);
        PersonContact Find(int id);
        // IEnumerable<PersonContact> GetPersonContactList(int buyerId);
        Task<IEquatable<PersonContact>> GetAsync();
        Task<PersonContact> FindAsync(int id);
        IEnumerable<PersonContact> GetPersonContactIdListByPersonId(int PersonId);
    }

    public class PersonContactService : IPersonContactService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PersonContact> _PersonContactRepository;
        RepositoryQuery<PersonContact> PersonContactRepository;
        public PersonContactService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PersonContactRepository = new Repository<PersonContact>(db);
            PersonContactRepository = new RepositoryQuery<PersonContact>(_PersonContactRepository);
        }

        public PersonContact GetPersonContact(int pt)
        {
            //return PersonContactRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            //return _unitOfWork.Repository<PersonContact>().Find(pt);
            //return PersonContactRepository.Get().Where(i => i.PersonContactId == pt).FirstOrDefault();

            return PersonContactRepository.Include(r => r.Person).Include(r => r.PersonContactType).Include(m=>m.Contact).Get().Where(i => i.PersonContactID == pt).FirstOrDefault();
        }

        public PersonContact Create(PersonContact pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PersonContact>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PersonContact>().Delete(id);
        }

        public void Delete(PersonContact pt)
        {
            _unitOfWork.Repository<PersonContact>().Delete(pt);
        }

        public void Update(PersonContact pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PersonContact>().Update(pt);
        }

        public IEnumerable<PersonContact> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PersonContact>()
                .Query()
                //.OrderBy(q => q.OrderBy(c => c.Supplier ))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PersonContact> GetPersonContactList()
        {
            var pt = _unitOfWork.Repository<PersonContact>().Query().Include(p => p.Person).Include(p =>  p.PersonContactType).Get();
            return pt;
        }

        public PersonContact Add(PersonContact pt)
        {
            _unitOfWork.Repository<PersonContact>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PersonContact>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PersonContact> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<PersonContact> GetPersonContactList(int id)
        {
            var pt = _unitOfWork.Repository<PersonContact>().Query().Include(m=>m.Person).Include(m=>m.PersonContactType).Include(m=>m.Contact).Get().Where(m => m.PersonId == id).ToList();
            return pt;
        }
        public PersonContact Find(int id)
        {
            return _unitOfWork.Repository<PersonContact>().Find(id);
        }

        public IEnumerable<PersonContact> GetPersonContactIdListByPersonId(int PersonId)
        {
            var pt = _unitOfWork.Repository<PersonContact>().Query().Get().Where(m => m.PersonId == PersonId).ToList();
            return pt;
        }

        public IQueryable<PersonContactViewModel> GetPersonContactListForIndex(int PersonId)
        {
            var temp = from pc in db.PersonContacts
                       join p in db.Persons on pc.ContactId equals p.PersonID into PersonTable
                       from PersonTab in PersonTable.DefaultIfEmpty()
                       orderby pc.PersonContactID
                       where pc.PersonId == PersonId
                       select new PersonContactViewModel
                       {
                           PersonContactId = pc.PersonContactID,
                           Name = PersonTab.Name,
                           Phone = PersonTab.Phone,
                           Mobile = PersonTab.Mobile,
                           Email = PersonTab.Email
                       };
            return temp;
        }

    }
}
