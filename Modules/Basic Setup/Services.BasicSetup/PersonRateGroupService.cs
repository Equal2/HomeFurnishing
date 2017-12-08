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
    public interface IPersonRateGroupService : IDisposable
    {
        PersonRateGroup Create(PersonRateGroup pt);
        void Delete(int id);
        void Delete(PersonRateGroup pt);
        PersonRateGroup Find(string Name);
        PersonRateGroup Find(int id);
        IEnumerable<PersonRateGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PersonRateGroup pt);
        PersonRateGroup Add(PersonRateGroup pt);
        IEnumerable<PersonRateGroup> GetPersonRateGroupList(int SiteId);
        IQueryable<PersonRateGroup> GetPersonRateGroupListForIndex(int SiteId);
        Task<IEquatable<PersonRateGroup>> GetAsync();
        Task<PersonRateGroup> FindAsync(int id);
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

    public class PersonRateGroupService : IPersonRateGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<PersonRateGroup> _PersonRateGroupRepository;
        public PersonRateGroupService(IUnitOfWork unitOfWork, IRepository<PersonRateGroup> PersonRateGroupRepo)
        {
            _unitOfWork = unitOfWork;
            _PersonRateGroupRepository = PersonRateGroupRepo;
        }
        public PersonRateGroupService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PersonRateGroupRepository = unitOfWork.Repository<PersonRateGroup>();
        }

        public PersonRateGroup Find(string Name)
        {
            return _PersonRateGroupRepository.Query().Get().Where(i => i.PersonRateGroupName == Name).FirstOrDefault();
        }


        public PersonRateGroup Find(int id)
        {
            return _unitOfWork.Repository<PersonRateGroup>().Find(id);
        }

        public PersonRateGroup Create(PersonRateGroup pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PersonRateGroup>().Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PersonRateGroup>().Delete(id);
        }

        public void Delete(PersonRateGroup pt)
        {
            _unitOfWork.Repository<PersonRateGroup>().Delete(pt);
        }

        public void Update(PersonRateGroup pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PersonRateGroup>().Update(pt);
        }

        public IEnumerable<PersonRateGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PersonRateGroup>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PersonRateGroupName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PersonRateGroup> GetPersonRateGroupList(int SiteId)
        {
            var pt = _unitOfWork.Repository<PersonRateGroup>().Query().Get().OrderBy(m => m.PersonRateGroupName).Where(m => m.SiteId == SiteId);

            return pt;
        }

        public IQueryable<PersonRateGroup> GetPersonRateGroupListForIndex(int SiteId)
        {
            var pt = _unitOfWork.Repository<PersonRateGroup>().Query().Get().OrderBy(m => m.PersonRateGroupName).Where(m => m.SiteId == SiteId);

            return pt;
        }

        public PersonRateGroup Add(PersonRateGroup pt)
        {
            _unitOfWork.Repository<PersonRateGroup>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _PersonRateGroupRepository.Instance
                        orderby p.PersonRateGroupName
                        select p.PersonRateGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _PersonRateGroupRepository.Instance
                        orderby p.PersonRateGroupName
                        select p.PersonRateGroupId).FirstOrDefault();
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

                temp = (from p in _PersonRateGroupRepository.Instance
                        orderby p.PersonRateGroupName
                        select p.PersonRateGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _PersonRateGroupRepository.Instance
                        orderby p.PersonRateGroupName
                        select p.PersonRateGroupId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _PersonRateGroupRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.PersonRateGroupName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.PersonRateGroupName
                        select new ComboBoxResult
                        {
                            text = pr.PersonRateGroupName,
                            id = pr.PersonRateGroupId.ToString()
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

            IEnumerable<PersonRateGroup> PersonRateGroups = from pr in _PersonRateGroupRepository.Instance
                                            where pr.PersonRateGroupId == Id
                                            select pr;

            ProductJson.id = PersonRateGroups.FirstOrDefault().PersonRateGroupId.ToString();
            ProductJson.text = PersonRateGroups.FirstOrDefault().PersonRateGroupName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<PersonRateGroup> PersonRateGroups = from pr in _PersonRateGroupRepository.Instance
                                                where pr.PersonRateGroupId == temp
                                                select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = PersonRateGroups.FirstOrDefault().PersonRateGroupId.ToString(),
                    text = PersonRateGroups.FirstOrDefault().PersonRateGroupName
                });
            }
            return ProductJson;
        }
        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<PersonRateGroup>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PersonRateGroup> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public bool CheckForNameExists(string Name)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _PersonRateGroupRepository.Instance
                        where pr.PersonRateGroupName == Name 
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

            var temp = (from pr in _PersonRateGroupRepository.Instance
                        where pr.PersonRateGroupName == Name && pr.PersonRateGroupId != Id 
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }
    }
}
