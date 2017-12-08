using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using Models.BasicSetup.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface IPaymentModeLedgerAccountService : IDisposable
    {
        PaymentModeLedgerAccount Create(PaymentModeLedgerAccount pt);
        void Delete(int id);
        void Delete(PaymentModeLedgerAccount pt);
        PaymentModeLedgerAccount Find(int id);
        IEnumerable<PaymentModeLedgerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PaymentModeLedgerAccount pt);
        PaymentModeLedgerAccount Add(PaymentModeLedgerAccount pt);
        IEnumerable<PaymentModeLedgerAccount> GetPaymentModeLedgerAccountList();
        Task<IEquatable<PaymentModeLedgerAccount>> GetAsync();
        Task<PaymentModeLedgerAccount> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        PaymentModeLedgerAccount FindByPaymentMode(int id, int SiteId, int DivisionId);
    }

    public class PaymentModeLedgerAccountService : IPaymentModeLedgerAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<PaymentModeLedgerAccount> _PaymentModeLedgerAccountRepository;
        public PaymentModeLedgerAccountService(IUnitOfWork unitOfWork, IRepository<PaymentModeLedgerAccount> PaymentModeLedgerAccountRepo)
        {
            _unitOfWork = unitOfWork;
            _PaymentModeLedgerAccountRepository = PaymentModeLedgerAccountRepo;
        }
        public PaymentModeLedgerAccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PaymentModeLedgerAccountRepository = unitOfWork.Repository<PaymentModeLedgerAccount>();
        }
        public PaymentModeLedgerAccount FindByPaymentMode(int id, int SiteId, int DivisionId)
        {
            var pt = _PaymentModeLedgerAccountRepository.Query().Get().Where(m => m.PaymentModeId == id && m.SiteId == SiteId && m.DivisionId == DivisionId).OrderBy(m => m.PaymentModeLedgerAccountId).FirstOrDefault();

            return pt;
        }

        public PaymentModeLedgerAccount Find(int id)
        {
            return _PaymentModeLedgerAccountRepository.Find(id);
        }

        public PaymentModeLedgerAccount Create(PaymentModeLedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Added;
            _PaymentModeLedgerAccountRepository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _PaymentModeLedgerAccountRepository.Delete(id);
        }

        public void Delete(PaymentModeLedgerAccount pt)
        {
            _PaymentModeLedgerAccountRepository.Delete(pt);
        }
        public void Update(PaymentModeLedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _PaymentModeLedgerAccountRepository.Update(pt);
        }

        public IEnumerable<PaymentModeLedgerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _PaymentModeLedgerAccountRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PaymentModeLedgerAccountId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PaymentModeLedgerAccount> GetPaymentModeLedgerAccountList()
        {
            var pt = _PaymentModeLedgerAccountRepository.Query().Get().OrderBy(m => m.PaymentModeLedgerAccountId);

            return pt;
        }

        public PaymentModeLedgerAccount Add(PaymentModeLedgerAccount pt)
        {
            _PaymentModeLedgerAccountRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _PaymentModeLedgerAccountRepository.Instance
                        orderby p.PaymentModeLedgerAccountId
                        select p.PaymentModeLedgerAccountId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _PaymentModeLedgerAccountRepository.Instance
                        orderby p.PaymentModeLedgerAccountId
                        select p.PaymentModeLedgerAccountId).FirstOrDefault();
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

                temp = (from p in _PaymentModeLedgerAccountRepository.Instance
                        orderby p.PaymentModeLedgerAccountId
                        select p.PaymentModeLedgerAccountId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _PaymentModeLedgerAccountRepository.Instance
                        orderby p.PaymentModeLedgerAccountId
                        select p.PaymentModeLedgerAccountId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<PaymentModeLedgerAccount>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PaymentModeLedgerAccount> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}



