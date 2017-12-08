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
    public interface IPersonContactTypeService : IDisposable
    {
        PersonContactType Create(PersonContactType pt);
        void Delete(int id);
        void Delete(PersonContactType pt);
        PersonContactType Find(string Name);
        PersonContactType Find(int id);
        IEnumerable<PersonContactType> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PersonContactType pt);
        PersonContactType Add(PersonContactType pt);
        IEnumerable<PersonContactType> GetPersonContactTypeList();

        // IEnumerable<PersonContactType> GetPersonContactTypeList(int buyerId);
        Task<IEquatable<PersonContactType>> GetAsync();
        Task<PersonContactType> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class PersonContactTypeService : IPersonContactTypeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PersonContactType> _PersonContactTypeRepository;
        RepositoryQuery<PersonContactType> PersonContactTypeRepository;
        public PersonContactTypeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PersonContactTypeRepository = new Repository<PersonContactType>(db);
            PersonContactTypeRepository = new RepositoryQuery<PersonContactType>(_PersonContactTypeRepository);
        }

        public PersonContactType Find(string Name)
        {
            return PersonContactTypeRepository.Get().Where(i => i.PersonContactTypeName == Name).FirstOrDefault();
        }


        public PersonContactType Find(int id)
        {
            return _unitOfWork.Repository<PersonContactType>().Find(id);
        }

        public PersonContactType Create(PersonContactType pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PersonContactType>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PersonContactType>().Delete(id);
        }

        public void Delete(PersonContactType pt)
        {
            _unitOfWork.Repository<PersonContactType>().Delete(pt);
        }

        public void Update(PersonContactType pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PersonContactType>().Update(pt);
        }

        public IEnumerable<PersonContactType> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PersonContactType>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PersonContactTypeName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PersonContactType> GetPersonContactTypeList()
        {
            var pt = _unitOfWork.Repository<PersonContactType>().Query().Get();

            return pt;
        }

        public PersonContactType Add(PersonContactType pt)
        {
            _unitOfWork.Repository<PersonContactType>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PersonContactType
                        orderby p.PersonContactTypeName
                        select p.PersonContactTypeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PersonContactType
                        orderby p.PersonContactTypeName
                        select p.PersonContactTypeId).FirstOrDefault();
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

                temp = (from p in db.PersonContactType
                        orderby p.PersonContactTypeName
                        select p.PersonContactTypeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PersonContactType
                        orderby p.PersonContactTypeName
                        select p.PersonContactTypeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PersonContactType>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PersonContactType> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
