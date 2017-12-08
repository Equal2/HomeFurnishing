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
    public interface IGodownService : IDisposable
    {
        Godown Create(Godown pt);
        void Delete(int id);
        void Delete(Godown pt);
        Godown Find(string Name);
        Godown Find(int id);
        IEnumerable<Godown> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Godown pt);
        Godown Add(Godown pt);
        IEnumerable<Godown> GetGodownList(int SiteId);
        IQueryable<Godown> GetGodownListForIndex(int SiteId);
        Task<IEquatable<Godown>> GetAsync();
        Task<Godown> FindAsync(int id);
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

    public class GodownService : IGodownService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Godown> _GodownRepository;
        public GodownService(IUnitOfWork unitOfWork, IRepository<Godown> GodownRepo)
        {
            _unitOfWork = unitOfWork;
            _GodownRepository = GodownRepo;
        }
        public GodownService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _GodownRepository = unitOfWork.Repository<Godown>();
        }

        public Godown Find(string Name)
        {
            return _GodownRepository.Query().Get().Where(i => i.GodownName == Name).FirstOrDefault();
        }


        public Godown Find(int id)
        {
            return _unitOfWork.Repository<Godown>().Find(id);
        }

        public Godown Create(Godown pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Godown>().Add(pt);


            return pt;
        }

        public void Delete(int id)
        {
            Godown pt = Find(id);
            pt.ObjectState = ObjectState.Deleted;
            _unitOfWork.Repository<Godown>().Delete(pt);
        }

        public void Delete(Godown pt)
        {
            pt.ObjectState = ObjectState.Deleted;
            _unitOfWork.Repository<Godown>().Delete(pt);
        }

        public void Update(Godown pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Godown>().Update(pt);
        }

        public IEnumerable<Godown> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Godown>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.GodownName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Godown> GetGodownList(int SiteId)
        {
            var pt = _unitOfWork.Repository<Godown>().Query().Get().OrderBy(m => m.GodownName).Where(m => m.SiteId == SiteId);

            return pt;
        }

        public IQueryable<Godown> GetGodownListForIndex(int SiteId)
        {
            var pt = _unitOfWork.Repository<Godown>().Query().Get().OrderBy(m => m.GodownName).Where(m => m.SiteId == SiteId);

            return pt;
        }

        public Godown Add(Godown pt)
        {
            _unitOfWork.Repository<Godown>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _GodownRepository.Instance
                        orderby p.GodownName
                        select p.GodownId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _GodownRepository.Instance
                        orderby p.GodownName
                        select p.GodownId).FirstOrDefault();
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

                temp = (from p in _GodownRepository.Instance
                        orderby p.GodownName
                        select p.GodownId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _GodownRepository.Instance
                        orderby p.GodownName
                        select p.GodownId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _GodownRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.GodownName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.GodownName
                        select new ComboBoxResult
                        {
                            text = pr.GodownCode + "|" + pr.GodownName,
                            id = pr.GodownId.ToString()
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

            IEnumerable<Godown> Godowns = from pr in _GodownRepository.Instance
                                            where pr.GodownId == Id
                                            select pr;

            ProductJson.id = Godowns.FirstOrDefault().GodownId.ToString();
            ProductJson.text = Godowns.FirstOrDefault().GodownName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Godown> Godowns = from pr in _GodownRepository.Instance
                                                where pr.GodownId == temp
                                                select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Godowns.FirstOrDefault().GodownId.ToString(),
                    text = Godowns.FirstOrDefault().GodownName
                });
            }
            return ProductJson;
        }
        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<Godown>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Godown> FindAsync(int id)
        {
            throw new NotImplementedException();
        }


        public bool CheckForNameExists(string Name)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _GodownRepository.Instance
                        where pr.GodownName == Name && pr.SiteId == SiteId 
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

            var temp = (from pr in _GodownRepository.Instance
                        where pr.GodownName == Name && pr.GodownId != Id && pr.SiteId == SiteId
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }
    }
}
