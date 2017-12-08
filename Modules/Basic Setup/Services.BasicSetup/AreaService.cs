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
    public interface IAreaService : IDisposable
    {
        Area Create(Area pt);
        void Delete(int id);
        void Delete(Area pt);
        Area Find(string Name);
        Area Find(int id);
        IEnumerable<Area> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Area pt);
        Area Add(Area pt);
        IEnumerable<Area> GetAreaList();
        IQueryable<Area> GetAreaListForIndex();
        Task<IEquatable<Area>> GetAsync();
        Task<Area> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        bool CheckForNameExists(string Name);
        bool CheckForNameExists(string Name, int Id);

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

    public class AreaService : IAreaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Area> _AreaRepository;
        public AreaService(IUnitOfWork unitOfWork, IRepository<Area> AreaRepo)
        {
            _unitOfWork = unitOfWork;
            _AreaRepository = AreaRepo;
        }
        public AreaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _AreaRepository = unitOfWork.Repository<Area>();
        }

        public Area Find(string Name)
        {
            return _AreaRepository.Query().Get().Where(i => i.AreaName == Name).FirstOrDefault();
        }


        public Area Find(int id)
        {
            return _unitOfWork.Repository<Area>().Find(id);
        }

        public Area Create(Area pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Area>().Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Area>().Delete(id);
        }

        public void Delete(Area pt)
        {
            _unitOfWork.Repository<Area>().Delete(pt);
        }

        public void Update(Area pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Area>().Update(pt);
        }

        public IEnumerable<Area> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Area>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.AreaName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Area> GetAreaList()
        {
            var pt = _unitOfWork.Repository<Area>().Query().Get().OrderBy(m => m.AreaName);

            return pt;
        }

        public IQueryable<Area> GetAreaListForIndex()
        {
            var pt = _unitOfWork.Repository<Area>().Query().Get().OrderBy(m => m.AreaName);

            return pt;
        }

        public Area Add(Area pt)
        {
            _unitOfWork.Repository<Area>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _AreaRepository.Instance
                        orderby p.AreaName
                        select p.AreaId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _AreaRepository.Instance
                        orderby p.AreaName
                        select p.AreaId).FirstOrDefault();
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

                temp = (from p in _AreaRepository.Instance
                        orderby p.AreaName
                        select p.AreaId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _AreaRepository.Instance
                        orderby p.AreaName
                        select p.AreaId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _AreaRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.AreaName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.AreaName
                        select new ComboBoxResult
                        {
                            text = pr.AreaName,
                            id = pr.AreaId.ToString()
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

            IEnumerable<Area> Areas = from pr in _AreaRepository.Instance
                                            where pr.AreaId == Id
                                            select pr;

            ProductJson.id = Areas.FirstOrDefault().AreaId.ToString();
            ProductJson.text = Areas.FirstOrDefault().AreaName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Area> Areas = from pr in _AreaRepository.Instance
                                                where pr.AreaId == temp
                                                select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Areas.FirstOrDefault().AreaId.ToString(),
                    text = Areas.FirstOrDefault().AreaName
                });
            }
            return ProductJson;
        }
        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<Area>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Area> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public bool CheckForNameExists(string Name)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _AreaRepository.Instance
                        where pr.AreaName == Name 
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

            var temp = (from pr in _AreaRepository.Instance
                        where pr.AreaName == Name && pr.AreaId != Id 
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }
    }
}
