using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.Company.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;
using Models.BasicSetup.Models;

namespace Services.BasicSetup
{
    public interface IBinLocationService : IDisposable
    {
        BinLocation Create(BinLocation pt);
        void Delete(int id);
        void Delete(BinLocation pt);
        BinLocation Find(string Name);
        BinLocation Find(int id);
        IEnumerable<BinLocation> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(BinLocation pt);
        BinLocation Add(BinLocation pt);
        IEnumerable<BinLocation> GetBinLocationList();
        IEnumerable<BinLocation> GetBinLocationListForGodown(int id);
        IQueryable<BinLocation> GetBinLocationListForIndex(int id);
        Task<IEquatable<BinLocation>> GetAsync();
        Task<BinLocation> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        bool CheckForNameExists(string Name);
        bool CheckForNameExists(string Name, int Id);
        bool IsDuplicateBinLocationName(int GodownId, string BinLocationName, int BinLocationId);
        bool IsDuplicateBinLocationCode(int GodownId, string BinLocationCode, int BinLocationId);

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
        ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum, int filterid);
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

    public class BinLocationService : IBinLocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<BinLocation> _BinLocationRepository;
        public BinLocationService(IUnitOfWork unitOfWork, IRepository<BinLocation> BinLocationRepo)
        {
            _unitOfWork = unitOfWork;
            _BinLocationRepository = BinLocationRepo;
        }
        public BinLocationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _BinLocationRepository = unitOfWork.Repository<BinLocation>();
        }

        public BinLocation Find(string Name)
        {
            return _BinLocationRepository.Query().Get().Where(i => i.BinLocationName == Name).FirstOrDefault();
        }


        public BinLocation Find(int id)
        {
            return _unitOfWork.Repository<BinLocation>().Find(id);
        }

        public BinLocation Create(BinLocation pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<BinLocation>().Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<BinLocation>().Delete(id);
        }

        public void Delete(BinLocation pt)
        {
            _unitOfWork.Repository<BinLocation>().Delete(pt);
        }

        public void Update(BinLocation pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<BinLocation>().Update(pt);
        }

        public IEnumerable<BinLocation> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<BinLocation>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.BinLocationName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<BinLocation> GetBinLocationList()
        {
            var pt = _unitOfWork.Repository<BinLocation>().Query().Get().OrderBy(m => m.BinLocationName);

            return pt;
        }

        public IEnumerable<BinLocation> GetBinLocationListForGodown(int id)
        {
            var pt = _unitOfWork.Repository<BinLocation>().Query().Get().Where(m => m.GodownId == id).OrderBy(m => m.BinLocationName);

            return pt;
        }

        public IQueryable<BinLocation> GetBinLocationListForIndex(int id)
        {
            var pt = _unitOfWork.Repository<BinLocation>().Query().Get().Where(m => m.GodownId == id).OrderBy(m => m.BinLocationName);

            return pt;
        }

        public BinLocation Add(BinLocation pt)
        {
            _unitOfWork.Repository<BinLocation>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _BinLocationRepository.Instance
                        orderby p.BinLocationName
                        select p.BinLocationId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _BinLocationRepository.Instance
                        orderby p.BinLocationName
                        select p.BinLocationId).FirstOrDefault();
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

                temp = (from p in _BinLocationRepository.Instance
                        orderby p.BinLocationName
                        select p.BinLocationId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _BinLocationRepository.Instance
                        orderby p.BinLocationName
                        select p.BinLocationId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _BinLocationRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.BinLocationName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.BinLocationName
                        select new ComboBoxResult
                        {
                            text = pr.BinLocationCode + "|" + pr.BinLocationName,
                            id = pr.BinLocationId.ToString()
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

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum, int filterid)
        {
            var list = (from pr in _BinLocationRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.BinLocationName.ToLower().Contains(searchTerm.ToLower())))
                        && pr.GodownId == filterid
                        orderby pr.BinLocationName
                        select new ComboBoxResult
                        {
                            text = pr.BinLocationCode + "|" + pr.BinLocationName,
                            id = pr.BinLocationId.ToString()
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

            IEnumerable<BinLocation> BinLocations = from pr in _BinLocationRepository.Instance
                                            where pr.BinLocationId == Id
                                            select pr;

            ProductJson.id = BinLocations.FirstOrDefault().BinLocationId.ToString();
            ProductJson.text = BinLocations.FirstOrDefault().BinLocationName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<BinLocation> BinLocations = from pr in _BinLocationRepository.Instance
                                                where pr.BinLocationId == temp
                                                select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = BinLocations.FirstOrDefault().BinLocationId.ToString(),
                    text = BinLocations.FirstOrDefault().BinLocationName
                });
            }
            return ProductJson;
        }
        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<BinLocation>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BinLocation> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public bool CheckForNameExists(string Name)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _BinLocationRepository.Instance
                        where pr.BinLocationName == Name 
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;

        }
        public bool CheckForNameExists(string Name, int Id)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _BinLocationRepository.Instance
                        where pr.BinLocationName == Name && pr.BinLocationId != Id 
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }

        public bool IsDuplicateBinLocationName(int GodownId,string BinLocationName, int BinLocationId)
        {
            var temp = (from L in _unitOfWork.Repository<BinLocation>().Instance
                        where L.GodownId == GodownId && L.BinLocationName == BinLocationName && L.BinLocationId != BinLocationId
                        select L).FirstOrDefault();

            if (temp != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsDuplicateBinLocationCode(int GodownId, string BinLocationCode, int BinLocationId)
        {
            var temp = (from L in _unitOfWork.Repository<BinLocation>().Instance
                        where L.GodownId == GodownId && L.BinLocationCode == BinLocationCode && L.BinLocationId != BinLocationId
                        select L).FirstOrDefault();

            if (temp != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
