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
    public interface IDivisionService : IDisposable
    {
        Division Create(Division pt);
        void Delete(int id);
        void Delete(Division pt);
        Division Find(string Name);
        Division Find(int id);
        IEnumerable<Division> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Division pt);
        Division Add(Division pt);
        IQueryable<Division> GetDivisionList();
        Task<IEquatable<Division>> GetAsync();
        Task<Division> FindAsync(int id);
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

    public class DivisionService : IDivisionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Division> _DivisionRepository;
        public DivisionService(IUnitOfWork unitOfWork,IRepository<Division> DivisionRepo)
        {
            _unitOfWork = unitOfWork;
            _DivisionRepository = DivisionRepo;
        }
        public DivisionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DivisionRepository = unitOfWork.Repository<Division>();
        }

        public Division Find(string Name)
        {
            return _DivisionRepository.Query().Get().Where(i => i.DivisionName == Name).FirstOrDefault();
        }


        public Division Find(int id)
        {
            return _unitOfWork.Repository<Division>().Find(id);
        }

        public Division Create(Division pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Division>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Division>().Delete(id);
        }

        public void Delete(Division pt)
        {
            _unitOfWork.Repository<Division>().Delete(pt);
        }

        public void Update(Division pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Division>().Update(pt);
        }

        public IEnumerable<Division> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Division>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DivisionName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<Division> GetDivisionList()
        {
            var pt = _unitOfWork.Repository<Division>().Query().Get().OrderBy(m=>m.DivisionName);

            return pt;
        }      

        public Division Add(Division pt)
        {
            _unitOfWork.Repository<Division>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _DivisionRepository.Instance
                        orderby p.DivisionName
                        select p.DivisionId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _DivisionRepository.Instance
                        orderby p.DivisionName
                        select p.DivisionId).FirstOrDefault();
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

                temp = (from p in _DivisionRepository.Instance
                        orderby p.DivisionName
                        select p.DivisionId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _DivisionRepository.Instance
                        orderby p.DivisionName
                        select p.DivisionId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _DivisionRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.DivisionName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.DivisionName
                        select new ComboBoxResult
                        {
                            text = pr.DivisionName,
                            id = pr.DivisionId.ToString()
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

            IEnumerable<Division> Divisions = from pr in _DivisionRepository.Instance
                                            where pr.DivisionId == Id
                                            select pr;

            ProductJson.id = Divisions.FirstOrDefault().DivisionId.ToString();
            ProductJson.text = Divisions.FirstOrDefault().DivisionName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Division> Divisions = from pr in _DivisionRepository.Instance
                                                where pr.DivisionId == temp
                                                select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Divisions.FirstOrDefault().DivisionId.ToString(),
                    text = Divisions.FirstOrDefault().DivisionName
                });
            }
            return ProductJson;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<Division>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Division> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
