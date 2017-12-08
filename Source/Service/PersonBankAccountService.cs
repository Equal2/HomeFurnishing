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
    public interface IPersonBankAccountService : IDisposable
    {
        PersonBankAccount Create(PersonBankAccount pt);
        void Delete(int id);
        void Delete(PersonBankAccount pt);
        PersonBankAccount GetPersonBankAccount(int ptId);
        IEnumerable<PersonBankAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PersonBankAccount pt);
        PersonBankAccount Add(PersonBankAccount pt);
        IEnumerable<PersonBankAccount> GetPersonBankAccountList();
        IEnumerable<PersonBankAccount> GetPersonBankAccountList(int id);
        PersonBankAccount Find(int id);
        // IEnumerable<PersonBankAccount> GetPersonBankAccountList(int buyerId);
        Task<IEquatable<PersonBankAccount>> GetAsync();
        Task<PersonBankAccount> FindAsync(int id);
        IQueryable<PersonBankAccountViewModel> GetPersonBankAccountListForIndex(int PersonId);
    }

    public class PersonBankAccountService : IPersonBankAccountService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PersonBankAccount> _PersonBankAccountRepository;
        RepositoryQuery<PersonBankAccount> PersonBankAccountRepository;
        public PersonBankAccountService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PersonBankAccountRepository = new Repository<PersonBankAccount>(db);
            PersonBankAccountRepository = new RepositoryQuery<PersonBankAccount>(_PersonBankAccountRepository);
        }

        public PersonBankAccount GetPersonBankAccount(int pt)
        {
            //return PersonBankAccountRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            //return _unitOfWork.Repository<PersonBankAccount>().Find(pt);
            //return PersonBankAccountRepository.Get().Where(i => i.PersonBankAccountId == pt).FirstOrDefault();

            return PersonBankAccountRepository.Include(r => r.Person).Get().Where(i => i.PersonBankAccountID == pt).FirstOrDefault();
        }

        public PersonBankAccount Create(PersonBankAccount pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PersonBankAccount>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PersonBankAccount>().Delete(id);
        }

        public void Delete(PersonBankAccount pt)
        {
            _unitOfWork.Repository<PersonBankAccount>().Delete(pt);
        }

        public void Update(PersonBankAccount pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PersonBankAccount>().Update(pt);
        }

        public IEnumerable<PersonBankAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PersonBankAccount>()
                .Query()
                //.OrderBy(q => q.OrderBy(c => c.Supplier ))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PersonBankAccount> GetPersonBankAccountList()
        {
            var pt = _unitOfWork.Repository<PersonBankAccount>().Query().Include(p => p.Person).Get();
            return pt;
        }

        public PersonBankAccount Add(PersonBankAccount pt)
        {
            _unitOfWork.Repository<PersonBankAccount>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PersonBankAccount>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PersonBankAccount> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<PersonBankAccount> GetPersonBankAccountList(int id)
        {
            var pt = _unitOfWork.Repository<PersonBankAccount>().Query().Include(m=>m.Person).Get().Where(m => m.PersonId == id).ToList();
            return pt;
        }
        public PersonBankAccount Find(int id)
        {
            return _unitOfWork.Repository<PersonBankAccount>().Find(id);
        }

        public IEnumerable<PersonBankAccount> GetPersonBankAccountIdListByPersonId(int PersonId)
        {
            var pt = _unitOfWork.Repository<PersonBankAccount>().Query().Get().Where(m => m.PersonId == PersonId).ToList();
            return pt;
        }

        public IQueryable<PersonBankAccountViewModel> GetPersonBankAccountListForIndex(int PersonId)
        {
            var temp = from pc in db.PersonBankAccount
                       orderby pc.PersonBankAccountID
                       where pc.PersonId == PersonId
                       select new PersonBankAccountViewModel
                       {
                           PersonBankAccountID = pc.PersonBankAccountID,
                           PersonId = pc.PersonId,
                           BankName = pc.BankName,
                           BankCode = pc.BankCode,
                           BankBranch = pc.BankBranch,
                           AccountNo = pc.AccountNo,
                           Remark = pc.Remark
                       };
            return temp;
        }

    }
}
