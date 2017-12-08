using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.Company.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;

namespace Services.BasicSetup
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
        Task<IEquatable<City>> GetAsync();
        Task<City> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        #region HelpList Getter
        /// <summary>
        /// *General Function*
        /// This function will create the help list for Projects
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum);
        #endregion

        #region HelpList Setters
        /// <summary>
        /// *General Function*
        /// This function will return the object in (Id,Text) format based on the Id
        /// </summary>
        /// <param name="Id">Primarykey of the record</param>
        /// <returns>ComboBoxResult</returns>
        ComboBoxResult GetValue(int Id);

        /// <summary>
        /// *General Function*
        /// This function will return list of object in (Id,Text) format based on the Ids
        /// </summary>
        /// <param name="Id">PrimaryKey of the record(',' seperated string)</param>
        /// <returns>List<ComboBoxResult></returns>
        List<ComboBoxResult> GetListCsv(string Id);
        #endregion
    }

    public class CityService : ICityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<City> _CityRepository;
        public CityService(IUnitOfWork unitOfWork, IRepository<City> cityRepo)
        {
            _unitOfWork = unitOfWork;
            _CityRepository = cityRepo;
        }
        public CityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CityRepository = unitOfWork.Repository<City>();
        }

        public CityViewModel GetCity(int pt)
        {
            return (from p in _CityRepository.Instance
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
            return _CityRepository.Query().Get().Where(i => i.CityName == Name).FirstOrDefault();
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
            var pt = (from p in _CityRepository.Instance
                      join t in _unitOfWork.Repository<State>().Instance on p.StateId equals t.StateId into table
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
                temp = (from p in _CityRepository.Instance
                        orderby p.CityName
                        select p.CityId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _CityRepository.Instance
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

                temp = (from p in _CityRepository.Instance
                        orderby p.CityName
                        select p.CityId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _CityRepository.Instance
                        orderby p.CityName
                        select p.CityId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _CityRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.CityName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.CityName
                        select new ComboBoxResult
                        {
                            text = pr.CityName,
                            id = pr.CityId.ToString()
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

            IEnumerable<City> Citys = from pr in _CityRepository.Instance
                                            where pr.CityId == Id
                                            select pr;

            ProductJson.id = Citys.FirstOrDefault().CityId.ToString();
            ProductJson.text = Citys.FirstOrDefault().CityName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<City> Citys = from pr in _CityRepository.Instance
                                                where pr.CityId == temp
                                                select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Citys.FirstOrDefault().CityId.ToString(),
                    text = Citys.FirstOrDefault().CityName
                });
            }
            return ProductJson;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
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
