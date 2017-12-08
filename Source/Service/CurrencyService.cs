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
    public interface ICurrencyService : IDisposable
    {
        Currency Create(Currency pt);
        void Delete(int id);
        void Delete(Currency pt);
        IEnumerable<Currency> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Currency pt);
        Currency Add(Currency pt);
        IEnumerable<Currency> GetCurrencyList();
        Currency GetCurrencyByName(string Currency);
        Currency Find(int Id);

        // IEnumerable<Currency> GetCurrencyList(int buyerId);
        Task<IEquatable<Currency>> GetAsync();
        Task<Currency> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class CurrencyService : ICurrencyService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Currency> _CurrencyRepository;
        RepositoryQuery<Currency> CurrencyRepository;
        public CurrencyService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CurrencyRepository = new Repository<Currency>(db);
            CurrencyRepository = new RepositoryQuery<Currency>(_CurrencyRepository);
        }
        public Currency GetCurrencyByName(string currency)
        {
            return (from p in db.Currency
                    where p.Name == currency
                    select p).FirstOrDefault();
        }
        public Currency Find(int id)
        {
            return _unitOfWork.Repository<Currency>().Find(id);
        }

        public Currency Create(Currency pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Currency>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Currency>().Delete(id);
        }

        public void Delete(Currency pt)
        {
            _unitOfWork.Repository<Currency>().Delete(pt);
        }

        public void Update(Currency pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Currency>().Update(pt);
        }

        public IEnumerable<Currency> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Currency>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Name))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Currency> GetCurrencyList()
        {
            var pt = _unitOfWork.Repository<Currency>().Query().Get().OrderBy(m=>m.Name);

            return pt;
        }

        public Currency Add(Currency pt)
        {
            _unitOfWork.Repository<Currency>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Currency
                        orderby p.Name
                        select p.ID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Currency
                        orderby p.Name
                        select p.ID).FirstOrDefault();
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

                temp = (from p in db.Currency
                        orderby p.Name
                        select p.ID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Currency
                        orderby p.Name
                        select p.ID).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Currency>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Currency> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
