using System.Collections.Generic;
using System.Linq;
using Surya.India.Data;
using Surya.India.Data.Infrastructure;
using Surya.India.Model.Models;

using Surya.India.Core.Common;
using System;
using Surya.India.Model;
using System.Threading.Tasks;
using Surya.India.Data.Models;

namespace Surya.India.Service
{
    public interface IBuyerAccountService : IDisposable
    {
        BuyerAccount Create(BuyerAccount ba);
        void Delete(int id);
        void Delete(BuyerAccount ba);
        BuyerAccount GetBuyerAccount(int baId);
        IEnumerable<BuyerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(BuyerAccount ba);
        BuyerAccount Add(BuyerAccount ba);
        IEnumerable<BuyerAccount> GetBuyerAccountList(int BuyerId);
        Task<IEquatable<BuyerAccount>> GetAsync();
        Task<BuyerAccount> FindAsync(int id);

        Buyer GetBuyer(int baId);
    }

    public class BuyerAccountService : IBuyerAccountService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<BuyerAccount> _BuyerAccountRepository;
        RepositoryQuery<BuyerAccount> buyerAccountRepository;
        ApplicationDbContext db = new ApplicationDbContext();
        public BuyerAccountService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _BuyerAccountRepository = new Repository<BuyerAccount>(db);
            buyerAccountRepository = new RepositoryQuery<BuyerAccount>(_BuyerAccountRepository);
        }

        public BuyerAccount GetBuyerAccount(int baId)
        {
            return  buyerAccountRepository.Include(r => r.Buyer).Get().Where(i => i.BuyerAccountID == baId).FirstOrDefault();
        }

        public BuyerAccount Create(BuyerAccount ba)
        {
            ba.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<BuyerAccount>().Insert(ba);
            return ba;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<BuyerAccount>().Delete(id);
        }

        public void Delete(BuyerAccount ba)
        {
            _unitOfWork.Repository<BuyerAccount>().Delete(ba);
        }

        public void Update(BuyerAccount ba)
        {
            ba.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<BuyerAccount>().Update(ba);
        }

        public IEnumerable<BuyerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var ba = _unitOfWork.Repository<BuyerAccount>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.BankAccount))
                .Filter(q => !string.IsNullOrEmpty(q.BankName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return ba;
        }
        public IEnumerable<BuyerAccount> GetBuyerAccountList(int buyerId)
        {
            var buyerDetails = _unitOfWork.Repository<BuyerAccount>().Query().Get();
            return buyerDetails.Where(q => q.Buyer.BuyerID == buyerId);
        }
        public IEnumerable<BuyerAccount> GetBuyerAccountList()
        {
            var baList = _unitOfWork.Repository<BuyerAccount>().Query().Get(); 
            return baList;
        }

        public BuyerAccount Add(BuyerAccount ba)
        {
            _unitOfWork.Repository<BuyerAccount>().Insert(ba);
            return ba;
        }

        public Buyer GetBuyer(int baId)
        {
            var b = GetBuyerAccount(baId);
           return b.Buyer;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<BuyerAccount>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BuyerAccount> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

      
    }
    
}
