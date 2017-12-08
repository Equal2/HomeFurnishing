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
    public interface IPersonRoleService : IDisposable
    {
        PersonRole Create(PersonRole pt);
        void Delete(int id);
        void Delete(PersonRole pt);
        PersonRole GetPersonRole(int ptId);
        IEnumerable<PersonRole> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PersonRole pt);
        PersonRole Add(PersonRole pt);
        IEnumerable<PersonRole> GetPersonRoleList();
        IEnumerable<PersonRole> GetPersonRoleList(int id);
        PersonRole Find(int id);
        // IEnumerable<PersonRole> GetPersonRoleList(int buyerId);
        Task<IEquatable<PersonRole>> GetAsync();
        Task<PersonRole> FindAsync(int id);
        IEnumerable<PersonRole> GetPersonRoleIdListByPersonId(int PersonId);
    }

    public class PersonRoleService : IPersonRoleService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PersonRole> _PersonRoleRepository;
        RepositoryQuery<PersonRole> PersonRoleRepository;
        public PersonRoleService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PersonRoleRepository = new Repository<PersonRole>(db);
            PersonRoleRepository = new RepositoryQuery<PersonRole>(_PersonRoleRepository);
        }

        public PersonRole GetPersonRole(int pt)
        {
            //return PersonRoleRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            return _unitOfWork.Repository<PersonRole>().Find(pt);
            //return PersonRoleRepository.Get().Where(i => i.PersonRoleId == pt).FirstOrDefault();

            //return PersonRoleRepository.Include(r => r.Person).Include(r => r.PersonRoleType).Include(m=>m.Contact).Get().Where(i => i.PersonRoleID == pt).FirstOrDefault();
        }

        public PersonRole Create(PersonRole pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PersonRole>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PersonRole>().Delete(id);
        }

        public void Delete(PersonRole pt)
        {
            _unitOfWork.Repository<PersonRole>().Delete(pt);
        }

        public void Update(PersonRole pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PersonRole>().Update(pt);
        }

        public IEnumerable<PersonRole> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PersonRole>()
                .Query()
                //.OrderBy(q => q.OrderBy(c => c.Supplier ))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PersonRole> GetPersonRoleList()
        {
            var pt = _unitOfWork.Repository<PersonRole>().Query().Get();
            return pt;
        }

        public PersonRole Add(PersonRole pt)
        {
            _unitOfWork.Repository<PersonRole>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PersonRole>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PersonRole> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<PersonRole> GetPersonRoleList(int id)
        {
            var pt = _unitOfWork.Repository<PersonRole>().Query().Get().Where(m => m.PersonId == id).ToList();
            return pt;
        }
        public PersonRole Find(int id)
        {
            return _unitOfWork.Repository<PersonRole>().Find(id);
        }

        public IEnumerable<PersonRole> GetPersonRoleIdListByPersonId(int PersonId)
        {
            var pt = _unitOfWork.Repository<PersonRole>().Query().Get().Where(m => m.PersonId == PersonId).ToList();
            return pt;
        }

        

    }
}
