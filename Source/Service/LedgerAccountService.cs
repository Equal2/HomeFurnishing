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
    public interface ILedgerAccountService : IDisposable
    {
        LedgerAccount Create(LedgerAccount pt);
        void Delete(int id);
        void Delete(LedgerAccount pt);
        LedgerAccount Find(string Name);
        LedgerAccount Find(int id);
        IEnumerable<LedgerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(LedgerAccount pt);
        LedgerAccount Add(LedgerAccount pt);
        IQueryable<LedgerAccountViewModel> GetLedgerAccountList();

        LedgerAccount GetLedgerAccountByPersondId(int PersonId);
        // IEnumerable<LedgerAccount> GetLedgerAccountList(int buyerId);
        Task<IEquatable<LedgerAccount>> GetAsync();
        Task<LedgerAccount> FindAsync(int id);
        string GetLedgerAccountnature(int LedgerAccountId);
        LedgerAccountViewModel GetLedgerAccountForEdit(int LedgerAccountId);
        int NextId(int id);
        int PrevId(int id);
    }

    public class LedgerAccountService : ILedgerAccountService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<LedgerAccount> _LedgerAccountRepository;
        RepositoryQuery<LedgerAccount> LedgerAccountRepository;
        public LedgerAccountService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LedgerAccountRepository = new Repository<LedgerAccount>(db);
            LedgerAccountRepository = new RepositoryQuery<LedgerAccount>(_LedgerAccountRepository);
        }


        public LedgerAccount Find(string Name)
        {            
            return LedgerAccountRepository.Get().Where(i => i.LedgerAccountName == Name).FirstOrDefault();
        }


        public LedgerAccount Find(int id)
        {
            return _unitOfWork.Repository<LedgerAccount>().Find(id);            
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

        public IEnumerable<LedgerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<LedgerAccount>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.LedgerAccountName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<LedgerAccountViewModel> GetLedgerAccountList()
        {
            //var pt = _unitOfWork.Repository<LedgerAccount>().Query().Get().Where(m=>m.PersonId==null).OrderBy(m=>m.LedgerAccountName);
            var pt = from L in db.LedgerAccount
                     orderby L.LedgerAccountName
                     select new LedgerAccountViewModel
                     {
                         LedgerAccountId = L.LedgerAccountId,
                         LedgerAccountName = L.LedgerAccountName,
                         LedgerAccountSuffix = L.LedgerAccountSuffix,
                         LedgerAccountGroupName = L.LedgerAccountGroup.LedgerAccountGroupName,
                         IsActive = L.IsActive,
                         IsSystemDefine = L.IsSystemDefine
                     };

            return pt;
        }

        public LedgerAccount GetLedgerAccountByPersondId(int PersonId)
        {
            LedgerAccount LedgerAccount = (from L in db.LedgerAccount
                                           where L.PersonId == PersonId
                                           select L).FirstOrDefault();

            return LedgerAccount;
        }

        public LedgerAccount Add(LedgerAccount pt)
        {
            _unitOfWork.Repository<LedgerAccount>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.LedgerAccount
                        orderby p.LedgerAccountName
                        select p.LedgerAccountId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.LedgerAccount
                        orderby p.LedgerAccountName
                        select p.LedgerAccountId).FirstOrDefault();
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

                temp = (from p in db.LedgerAccount
                        orderby p.LedgerAccountName
                        select p.LedgerAccountId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.LedgerAccount
                        orderby p.LedgerAccountName
                        select p.LedgerAccountId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public string GetLedgerAccountnature(int LedgerAccountId)
        {
            return (from L in db.LedgerAccount
                        join Lg in db.LedgerAccountGroup on L.LedgerAccountGroupId equals Lg.LedgerAccountGroupId into LedgerAccountGroupTable
                        from LedgerAccountGroupTab in LedgerAccountGroupTable.DefaultIfEmpty()
                        where L.LedgerAccountId == LedgerAccountId
                        select new { LedgerAccountNature = LedgerAccountGroupTab.LedgerAccountNature }).FirstOrDefault().LedgerAccountNature;
        }


        public LedgerAccountViewModel GetLedgerAccountForEdit(int LedgerAccountId)
        {
            return (from L in db.LedgerAccount
                    where L.LedgerAccountId == LedgerAccountId
                    select new LedgerAccountViewModel
                    {
                        LedgerAccountId = L.LedgerAccountId,
                        LedgerAccountName = L.LedgerAccountName,
                        LedgerAccountSuffix = L.LedgerAccountSuffix,
                        PersonId = L.PersonId,
                        LedgerAccountGroupId = L.LedgerAccountGroupId,
                        SalesTaxGroupProductId = L.Product.SalesTaxGroupProductId,
                        IsActive = L.IsActive,
                        IsSystemDefine = L.IsSystemDefine,
                        CreatedBy = L.CreatedBy,
                        ModifiedBy = L.ModifiedBy,
                        CreatedDate = L.CreatedDate,
                    }).FirstOrDefault();
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<LedgerAccount>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<LedgerAccount> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
