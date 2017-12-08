using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;
using Models.BasicSetup.ViewModels;

namespace Services.BasicSetup
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
        IQueryable<LedgerAccount> GetLedgerAccountList();
        LedgerAccount GetLedgerAccountByPersondId(int PersonId);
        Task<IEquatable<LedgerAccount>> GetAsync();
        Task<LedgerAccount> FindAsync(int id);
        string GetLedgerAccountnature(int LedgerAccountId);
        int NextId(int id);
        int PrevId(int id);
        ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum);
        ComboBoxResult GetValue(int Id);
    }

    public class LedgerAccountService : ILedgerAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<LedgerAccount> _LedgerAccountRepository;
        public LedgerAccountService(IUnitOfWork unitOfWork, IRepository<LedgerAccount> LedgerAccountRepo)
        {
            _unitOfWork = unitOfWork;
            _LedgerAccountRepository = LedgerAccountRepo;
        }

        public LedgerAccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LedgerAccountRepository = unitOfWork.Repository<LedgerAccount>();
        }
        public LedgerAccount Find(string Name)
        {
            return _LedgerAccountRepository.Query().Get().Where(i => i.LedgerAccountName == Name).FirstOrDefault();
        }


        public LedgerAccount Find(int id)
        {
            return _LedgerAccountRepository.Find(id);
        }

        public LedgerAccount Create(LedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Added;
            _LedgerAccountRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _LedgerAccountRepository.Delete(id);
        }

        public void Delete(LedgerAccount pt)
        {
            _LedgerAccountRepository.Delete(pt);
        }

        public void Update(LedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _LedgerAccountRepository.Update(pt);
        }

        public IEnumerable<LedgerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _LedgerAccountRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.LedgerAccountName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<LedgerAccount> GetLedgerAccountList()
        {
            var pt = _LedgerAccountRepository.Query().Get().Where(m => m.PersonId == null).OrderBy(m => m.LedgerAccountName);

            return pt;
        }

        public LedgerAccount GetLedgerAccountByPersondId(int PersonId)
        {
            LedgerAccount LedgerAccount = (from L in _LedgerAccountRepository.Instance
                                           where L.PersonId == PersonId
                                           select L).FirstOrDefault();

            return LedgerAccount;
        }

        public LedgerAccount Add(LedgerAccount pt)
        {
            _LedgerAccountRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _LedgerAccountRepository.Instance
                        orderby p.LedgerAccountName
                        select p.LedgerAccountId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _LedgerAccountRepository.Instance
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

                temp = (from p in _LedgerAccountRepository.Instance
                        orderby p.LedgerAccountName
                        select p.LedgerAccountId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _LedgerAccountRepository.Instance
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
            return (from L in _LedgerAccountRepository.Instance
                    join Lg in _unitOfWork.Repository<LedgerAccountGroup>().Instance on L.LedgerAccountGroupId equals Lg.LedgerAccountGroupId into LedgerAccountGroupTable
                    from LedgerAccountGroupTab in LedgerAccountGroupTable.DefaultIfEmpty()
                    where L.LedgerAccountId == LedgerAccountId
                    select new { LedgerAccountNature = LedgerAccountGroupTab.LedgerAccountNature }).FirstOrDefault().LedgerAccountNature;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _LedgerAccountRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.LedgerAccountName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.LedgerAccountName
                        select new ComboBoxResult
                        {
                            text = pr.LedgerAccountName,
                            id = pr.LedgerAccountId.ToString()
                        }
              );

            var temp = list
               .Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = list.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return Data;
        }

        public ComboBoxResult GetValue(int Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<LedgerAccount> LedgerAccounts = from pr in _LedgerAccountRepository.Instance
                                                    where pr.LedgerAccountId == Id
                                                    select pr;

            ProductJson.id = LedgerAccounts.FirstOrDefault().LedgerAccountId.ToString();
            ProductJson.text = LedgerAccounts.FirstOrDefault().LedgerAccountName;

            return ProductJson;
        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
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
