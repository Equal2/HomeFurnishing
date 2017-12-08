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
using Model.ViewModel;

namespace Service
{
    public interface ICityService : IDisposable
    {
        City Create(City pt);
        void Delete(int id);
        void Delete(City pt);
        CityViewModel GetCity(int ptId);
        City Find(string Name);
        City Find(int id);
        IEnumerable<City> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(City pt);
        City Add(City pt);
        IQueryable<CityViewModel> GetCityList();
        IEnumerable<City> GetCityListForStateType(int Id);

        // IEnumerable<City> GetCityList(int buyerId);
        Task<IEquatable<City>> GetAsync();
        Task<City> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class CityService : ICityService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<City> _CityRepository;
        RepositoryQuery<City> CityRepository;
        public CityService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CityRepository = new Repository<City>(db);
            CityRepository = new RepositoryQuery<City>(_CityRepository);
        }

        public CityViewModel GetCity(int pt)
        {
            return (from p in db.City
                    where p.CityId == pt
                    select new CityViewModel
                    {

                        CityId=p.CityId,
                        CityName=p.CityName,
                        CountryId=p.State.Country.CountryId,
                        StateId=p.StateId,
                        IsActive=p.IsActive

                    }

                        ).FirstOrDefault();
        }


        public City Find(string Name)
        {            
            return CityRepository.Get().Where(i => i.CityName == Name).FirstOrDefault();
        }


        public City Find(int id)
        {
            return _unitOfWork.Repository<City>().Find(id);            
        }

        public City Create(City pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<City>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<City>().Delete(id);
        }

        public void Delete(City pt)
        {
            _unitOfWork.Repository<City>().Delete(pt);
        }

        public void Update(City pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<City>().Update(pt);
        }

        public IEnumerable<City> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<City>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CityName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<CityViewModel> GetCityList()
        {
            var pt = (from p in db.City
                      join t in db.State on p.StateId equals t.StateId into table
                      from tab in table.DefaultIfEmpty()
                      orderby p.CityName
                      select new CityViewModel
                      {
                          CityId = p.CityId,
                          CityName = p.CityName,
                          StateId = p.StateId,
                          StateName = tab.StateName,
                      }
                          );

            return pt;
        }

        public IEnumerable<City> GetCityListForStateType(int Id)
        {
            var pt = _unitOfWork.Repository<City>().Query().Get().Where(i => i.StateId == Id);

            return pt;
        }


        public City Add(City pt)
        {
            _unitOfWork.Repository<City>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.City
                        orderby p.CityName
                        select p.CityId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.City
                        orderby p.CityName
                        select p.CityId).FirstOrDefault();
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

                temp = (from p in db.City
                        orderby p.CityName
                        select p.CityId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.City
                        orderby p.CityName
                        select p.CityId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<City>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<City> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
