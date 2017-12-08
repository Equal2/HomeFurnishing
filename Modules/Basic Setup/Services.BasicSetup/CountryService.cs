using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.Company.Models;
using Infrastructure.IO;
using Models.BasicSetup.ViewModels;

namespace Services.BasicSetup
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
        Task<IEquatable<Country>> GetAsync();
        Task<Country> FindAsync(int id);
        Country Find(int id);
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

    public class CountryService : ICountryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Country> _CountryRepository;
        public CountryService(IUnitOfWork unitOfWork, IRepository<Country> country)
        {
            _unitOfWork = unitOfWork;
            _CountryRepository = country;
        }
        public CountryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CountryRepository = unitOfWork.Repository<Country>();
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
            var pt = _unitOfWork.Repository<Country>().Query().Get().OrderBy(M => M.CountryName).ToList();

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
                temp = (from p in _CountryRepository.Instance
                        orderby p.CountryName
                        select p.CountryId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _CountryRepository.Instance
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

                temp = (from p in _CountryRepository.Instance
                        orderby p.CountryName
                        select p.CountryId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _CountryRepository.Instance
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
            _unitOfWork.Dispose();
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _CountryRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.CountryName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.CountryName
                        select new ComboBoxResult
                        {
                            text = pr.CountryName,
                            id = pr.CountryId.ToString()
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

            IEnumerable<Country> Countrys = from pr in _CountryRepository.Instance
                                            where pr.CountryId == Id
                                            select pr;

            ProductJson.id = Countrys.FirstOrDefault().CountryId.ToString();
            ProductJson.text = Countrys.FirstOrDefault().CountryName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Country> Countrys = from pr in _CountryRepository.Instance
                                                where pr.CountryId == temp
                                                select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Countrys.FirstOrDefault().CountryId.ToString(),
                    text = Countrys.FirstOrDefault().CountryName
                });
            }
            return ProductJson;
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
