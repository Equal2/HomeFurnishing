using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;
using Models.BasicSetup.ViewModels;

namespace Services.BasicSetup
{
    public interface IDimension1Service : IDisposable
    {
        Dimension1 Create(Dimension1 pt);
        void Delete(int id);
        void Delete(Dimension1 pt);
        Dimension1 Find(string Name);
        Dimension1 Find(int id);
        IEnumerable<Dimension1> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Dimension1 pt);
        Dimension1 Add(Dimension1 pt);
        IQueryable<Dimension1> GetDimension1List(int id);
        Task<IEquatable<Dimension1>> GetAsync();
        Task<Dimension1> FindAsync(int id);
        int NextId(int id,int ptypeid);
        int PrevId(int id,int ptypeid);

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

    public class Dimension1Service : IDimension1Service
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Dimension1> _Dimension1Repository;
        public Dimension1Service(IUnitOfWork unitOfWork, IRepository<Dimension1> Dimension1Repo)
        {
            _unitOfWork = unitOfWork;
            _Dimension1Repository = Dimension1Repo;
        }
        public Dimension1Service(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _Dimension1Repository = unitOfWork.Repository<Dimension1>();
        }

        public Dimension1 Find(string Name)
        {
            return _Dimension1Repository.Query().Get().Where(i => i.Dimension1Name == Name).FirstOrDefault();
        }


        public Dimension1 Find(int id)
        {
            return _Dimension1Repository.Find(id);            
        }

        public Dimension1 Create(Dimension1 pt)
        {
            pt.ObjectState = ObjectState.Added;
            _Dimension1Repository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _Dimension1Repository.Delete(id);
        }

        public void Delete(Dimension1 pt)
        {
            _Dimension1Repository.Delete(pt);
        }

        public void Update(Dimension1 pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _Dimension1Repository.Update(pt);
        }

        public IEnumerable<Dimension1> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _Dimension1Repository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Dimension1Name))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<Dimension1> GetDimension1List(int id)
        {
            //var pt = _unitOfWork.Repository<Dimension1>().Query().Get().Where(m=>m.ProductTypeId==id).OrderBy(m=>m.Dimension1Name);

            return (from p in _Dimension1Repository.Instance
                         where p.ProductTypeId==id 
                         orderby p.Dimension1Name
                         select p
                         );
        }

        public Dimension1 Add(Dimension1 pt)
        {
            _unitOfWork.Repository<Dimension1>().Insert(pt);
            return pt;
        }

        public int NextId(int id,int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _Dimension1Repository.Instance
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension1Name
                        select p.Dimension1Id).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _Dimension1Repository.Instance
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension1Name
                        select p.Dimension1Id).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id,int ptypeid)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in _Dimension1Repository.Instance
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension1Name
                        select p.Dimension1Id).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _Dimension1Repository.Instance
                        where p.ProductTypeId == ptypeid
                        orderby p.Dimension1Name
                        select p.Dimension1Id).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _Dimension1Repository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.Dimension1Name.ToLower().Contains(searchTerm.ToLower())))
                        && pr.IsActive == true
                        orderby pr.Dimension1Name
                        select new ComboBoxResult
                        {
                            text = pr.Dimension1Name,
                            id = pr.Dimension1Id.ToString()
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

            IEnumerable<Dimension1> Dimension1s = from pr in _Dimension1Repository.Instance
                                          where pr.Dimension1Id == Id
                                          select pr;

            ProductJson.id = Dimension1s.FirstOrDefault().Dimension1Id.ToString();
            ProductJson.text = Dimension1s.FirstOrDefault().Dimension1Name;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Dimension1> Dimension1s = from pr in _Dimension1Repository.Instance
                                              where pr.Dimension1Id == temp
                                              select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Dimension1s.FirstOrDefault().Dimension1Id.ToString(),
                    text = Dimension1s.FirstOrDefault().Dimension1Name
                });
            }
            return ProductJson;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<Dimension1>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Dimension1> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public bool CheckForNameExists(string Name)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _Dimension1Repository.Instance
                        where pr.Dimension1Name == Name
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

            var temp = (from pr in _Dimension1Repository.Instance
                        where pr.Dimension1Name == Name && pr.Dimension1Id != Id
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }
    }
}
