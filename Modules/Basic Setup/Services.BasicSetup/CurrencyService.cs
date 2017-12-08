using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;

namespace Services.BasicSetup
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
        Task<IEquatable<Currency>> GetAsync();
        Task<Currency> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Currency> _CurrencyRepository;
        public CurrencyService(IUnitOfWork unitOfWork, IRepository<Currency> CurrencyRepo)
        {
            _unitOfWork = unitOfWork;
            _CurrencyRepository = CurrencyRepo;
        }
        public CurrencyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CurrencyRepository = unitOfWork.Repository<Currency>();
        }
        public Currency GetCurrencyByName(string currency)
        {
            return (from p in _CurrencyRepository.Instance
                    where p.Name == currency
                    select p).FirstOrDefault();
        }
        public Currency Find(int id)
        {
            return _CurrencyRepository.Find(id);
        }

        public Currency Create(Currency pt)
        {
            pt.ObjectState = ObjectState.Added;
            _CurrencyRepository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _CurrencyRepository.Delete(id);
        }

        public void Delete(Currency pt)
        {
            _CurrencyRepository.Delete(pt);
        }

        public void Update(Currency pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _CurrencyRepository.Update(pt);
        }

        public IEnumerable<Currency> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _CurrencyRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Currency> GetCurrencyList()
        {
            var pt = _CurrencyRepository.Query().Get().OrderBy(m => m.Name);

            return pt;
        }

        public Currency Add(Currency pt)
        {
            _CurrencyRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _CurrencyRepository.Instance
                        orderby p.Name
                        select p.ID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _CurrencyRepository.Instance
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

                temp = (from p in _CurrencyRepository.Instance
                        orderby p.Name
                        select p.ID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _CurrencyRepository.Instance
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
            _unitOfWork.Dispose();
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
