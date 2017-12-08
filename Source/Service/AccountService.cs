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
    public interface IAccountService : IDisposable
    {
        LedgerAccount Create(LedgerAccount pt);
        void Delete(int id);
        void Delete(LedgerAccount pt);
        LedgerAccount Find(string Name);
        LedgerAccount Find(int id);
        LedgerAccount FindByPersonId(int PersonId);
        void Update(LedgerAccount pt);
        LedgerAccount Add(LedgerAccount pt);
        IEnumerable<LedgerAccount> GetAccountList();
        LedgerAccount GetAccountByName(string terms);
        LedgerAccount GetLedgerAccountFromPersonId(int PersonId);
    }

    public class AccountService : IAccountService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<LedgerAccount> _AccountRepository;
        RepositoryQuery<LedgerAccount> AccountRepository;

        public AccountService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _AccountRepository = new Repository<LedgerAccount>(db);
            AccountRepository = new RepositoryQuery<LedgerAccount>(_AccountRepository);
        }
        public LedgerAccount GetAccountByName(string terms)
        {
            return (from p in db.LedgerAccount
                    where p.LedgerAccountName == terms
                    select p).FirstOrDefault();
        }

        public LedgerAccount Find(string Name)
        {
            return AccountRepository.Get().Where(i => i.LedgerAccountName == Name).FirstOrDefault();
        }


        public LedgerAccount Find(int id)
        {
            return _unitOfWork.Repository<LedgerAccount>().Find(id);
        }


        public LedgerAccount FindByPersonId(int PersonId)
        {
            return AccountRepository.Get().Where(i => i.PersonId == PersonId).FirstOrDefault();
        }

        public LedgerAccount Create(LedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<LedgerAccount>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<LedgerAccount>().Delete(id);
        }

        public void Delete(LedgerAccount pt)
        {
            _unitOfWork.Repository<LedgerAccount>().Delete(pt);
        }

        public void Update(LedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<LedgerAccount>().Update(pt);
        }

        public IEnumerable<LedgerAccount> GetAccountList()
        {
            var pt = _unitOfWork.Repository<LedgerAccount>().Query().Get().OrderBy(m=>m.LedgerAccountName);

            return pt;
        }

        public LedgerAccount Add(LedgerAccount pt)
        {
            _unitOfWork.Repository<LedgerAccount>().Insert(pt);
            return pt;
        }

        public LedgerAccount GetLedgerAccountFromPersonId(int PersonId)
        {
            LedgerAccount account = (from L in db.LedgerAccount
                                     where L.PersonId == PersonId
                                     select L).FirstOrDefault();

            return account;
        }

        public void Dispose()
        {
        }

    }
}
