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
using Model.ViewModels;


namespace Service
{
    public interface IPersonAddressService : IDisposable
    {
        PersonAddress Create(PersonAddress so);
        void Delete(int id);
        void Delete(PersonAddress so);
        PersonAddress GetPersonAddress(int soId);
        IEnumerable<PersonAddress> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PersonAddress so);
        PersonAddress Add(PersonAddress so);
        IEnumerable<PersonAddress> GetPersonAddressList();
        void Detach(PersonAddress pc);
        IEnumerable<PersonAddress> GetPersonAddressList(int personId);
        IEnumerable<PersonAddressViewModel> GetPersonAddressListForIndex(int personId);
        Task<IEquatable<PersonAddress>> GetAsync();
        Task<PersonAddress> FindAsync(int id);
        PersonAddress GetShipAddressByPersonId(int Pid);

        PersonAddress Find(int id);

    }

    public class PersonAddressService : IPersonAddressService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PersonAddress> _personAddressRepository;
        RepositoryQuery<PersonAddress> personAddressRepository;
        public PersonAddressService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _personAddressRepository = new Repository<PersonAddress>(db);
            personAddressRepository = new RepositoryQuery<PersonAddress>(_personAddressRepository);
        }

        public PersonAddress GetPersonAddress(int soId)
        {
            return personAddressRepository.Include(r => r.Person).Include(r => r.City).Include(r => r.City.State).Include(r => r.City.State.Country).Get().Where(i => i.PersonAddressID == soId).FirstOrDefault();           
        }

        public PersonAddress GetShipAddressByPersonId(int personId)
        {
            //return personAddressRepository.Include(r => r.Person).Get().Where(i => i.Person.PersonID == personId).FirstOrDefault();
            var add = _unitOfWork.Repository<PersonAddress>().Query().Get().Where(m => m.Person.PersonID  == personId);
            return add.FirstOrDefault();
        }

        public PersonAddress Create(PersonAddress so)
        {
            so.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PersonAddress>().Insert(so);
            return so;
        }

        public PersonAddress Find(int id)
        {
            return _unitOfWork.Repository<PersonAddress>().Find(id);
        }


        public void Delete(int id)
        {
            _unitOfWork.Repository<PersonAddress>().Delete(id);
        }

        public void Delete(PersonAddress so)
        {
            _unitOfWork.Repository<PersonAddress>().Delete(so);
        }

        public void Update(PersonAddress so)
        {
            so.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PersonAddress>().Update(so);
        }

        public IEnumerable<PersonAddress> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PersonAddress>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Zipcode))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PersonAddress> GetPersonAddressList()
        {
            var dd = _unitOfWork.Repository<PersonAddress>().Query().Include(m=>m.Person).Get().OrderBy(m=>m.Person.Name);// ToDo: To Investigate why OrderBy Required?
            return dd;
        }

        public IEnumerable<PersonAddress> GetPersonAddressList(int personId)
        {
            var so = _unitOfWork.Repository<PersonAddress>().Query()
                        .Get().Where(m => m.Person.PersonID == personId).ToList();
            return so;
        }

        public IEnumerable<PersonAddressViewModel> GetPersonAddressListForIndex(int personId)
        {
            var so = (from L in db.PersonAddress
                      where L.PersonId == personId && L.AddressType != null
                      select new PersonAddressViewModel
                      {
                          PersonAddressId = L.PersonAddressID,
                          AddressType = L.AddressType,
                          Address = L.Address,
                          CityId = L.CityId,
                          CityName = L.City.CityName,
                          Zipcode = L.Zipcode
                      }).ToList();
            return so;
        }

        public PersonAddress Add(PersonAddress so)
        {
            _unitOfWork.Repository<PersonAddress>().Insert(so);
            return so;
        }
        public void Dispose()
        {
        }
        
        public void Detach(PersonAddress pc)
        {
            _unitOfWork.Repository<PersonAddress>().Detach(pc);
        }
        public Task<IEquatable<PersonAddress>> GetAsync()
        {
            throw new NotImplementedException();
        }
        public Task<PersonAddress> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}