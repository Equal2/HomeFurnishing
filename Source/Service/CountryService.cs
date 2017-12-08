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
    public interface ICountryService : IDisposable
    {
        Country Create(Country pt);
        void Delete(int id);
        void Delete(Country pt);
        IEnumerable<Country> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Country pt);
        Country Add(Country pt);
        IEnumerable<Country> GetCountryList();

        // IEnumerable<Country> GetCountryList(int buyerId);
        Task<IEquatable<Country>> GetAsync();
        Task<Country> FindAsync(int id);
        Country Find(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class CountryService : ICountryService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Country> _CountryRepository;
        RepositoryQuery<Country> CountryRepository;
        public CountryService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CountryRepository = new Repository<Country>(db);
            CountryRepository = new RepositoryQuery<Country>(_CountryRepository);
        }

        public Country Find(int id)
        {
            return _unitOfWork.Repository<Country>().Find(id);
        }

        public Country Create(Country pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Country>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Country>().Delete(id);
        }

        public void Delete(Country pt)
        {
            _unitOfWork.Repository<Country>().Delete(pt);
        }

        public void Update(Country pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Country>().Update(pt);
        }

        public IEnumerable<Country> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Country>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CountryName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Country> GetCountryList()
        {
            var pt = _unitOfWork.Repository<Country>().Query().Get().OrderBy(M=>M.CountryName).ToList();

            return pt;
        }

        public Country Add(Country pt)
        {
            _unitOfWork.Repository<Country>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Country
                        orderby p.CountryName
                        select p.CountryId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Country
                        orderby p.CountryName
                        select p.CountryId).FirstOrDefault();
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

                temp = (from p in db.Country
                        orderby p.CountryName
                        select p.CountryId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Country
                        orderby p.CountryName
                        select p.CountryId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Country>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Country> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
