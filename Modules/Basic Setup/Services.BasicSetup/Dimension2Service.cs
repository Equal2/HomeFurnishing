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
    public interface IDimension2Service : IDisposable
    {
        Dimension2 Create(Dimension2 pt);
        void Delete(int id);
        void Delete(Dimension2 pt);
        Dimension2 Find(string Name);
        Dimension2 Find(int id);
        IEnumerable<Dimension2> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Dimension2 pt);
        Dimension2 Add(Dimension2 pt);
        IQueryable<Dimension2> GetDimension2List(int id);
        Task<IEquatable<Dimension2>> GetAsync();
        Task<Dimension2> FindAsync(int id);
        int NextId(int id,int ptypeid);
        int PrevId(int id,int ptypeid);
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

    public class Dimension2Service : IDimension2Service
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Dimension2> _Dimension2Repository;
        public Dimension2Service(IUnitOfWork unitOfWork, IRepository<Dimension2> Dimension2Repo)
        {
            _unitOfWork = unitOfWork;
            _Dimension2Repository = Dimension2Repo;
        }
        public Dimension2Service(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _Dimension2Repository = unitOfWork.Repository<Dimension2>();
        }

        public Dimension2 Find(string Name)
        {
            return _Dimension2Repository.Query().Get().Where(i => i.Dimension2Name == Name).FirstOrDefault();
        }


        public Dimension2 Find(int id)
        {
            return _Dimension2Repository.Find(id);            
        }

        public Dimension2 Create(Dimension2 pt)
        {
            pt.ObjectState = ObjectState.Added;
            _Dimension2Repository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _Dimension2Repository.Delete(id);
        }

        public void Delete(Dimension2 pt)
        {
            _Dimension2Repository.Delete(pt);
        }

        public void Update(Dimension2 pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _Dimension2Repository.Update(pt);
        }

        public IEnumerable<Dimension2> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _Dimension2Repository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Dimension2Name))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<Dimension2> GetDimension2List(int id)
        {
            //var pt = _unitOfWork.Repository<Dimension2>().Query().Get().Where(m=>m.ProductTypeId==id).OrderBy(m=>m.Dimension2Name);

            return (from p in _Dimension2Repository.Instance
                         where p.ProductTypeId==id 
                         orderby p.Dimension2Name
                         select p
                         );
        }

        public Dimension2 Add(Dimension2 pt)
        {
            _Dimension2Repository.Insert(pt);
            return pt;
        }

        public int NextId(int id,int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _Dimension2Repository.Instance
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension2Name
                        select p.Dimension2Id).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _Dimension2Repository.Instance
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension2Name
                        select p.Dimension2Id).FirstOrDefault();
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

                temp = (from p in _Dimension2Repository.Instance
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension2Name
                        select p.Dimension2Id).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _Dimension2Repository.Instance
                        where p.ProductTypeId==ptypeid
                        orderby p.Dimension2Name
                        select p.Dimension2Id).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _Dimension2Repository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.Dimension2Name.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.Dimension2Name
                        select new ComboBoxResult
                        {
                            text = pr.Dimension2Name,
                            id = pr.Dimension2Id.ToString()
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

            IEnumerable<Dimension2> Dimension2s = from pr in _Dimension2Repository.Instance
                                          where pr.Dimension2Id == Id
                                          select pr;

            ProductJson.id = Dimension2s.FirstOrDefault().Dimension2Id.ToString();
            ProductJson.text = Dimension2s.FirstOrDefault().Dimension2Name;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Dimension2> Dimension2s = from pr in _Dimension2Repository.Instance
                                              where pr.Dimension2Id == temp
                                              select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Dimension2s.FirstOrDefault().Dimension2Id.ToString(),
                    text = Dimension2s.FirstOrDefault().Dimension2Name
                });
            }
            return ProductJson;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<Dimension2>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Dimension2> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
