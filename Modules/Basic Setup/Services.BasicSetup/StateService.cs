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
    public interface IStateService : IDisposable
    {
        State Create(State pt);
        void Delete(int id);
        void Delete(State pt);
        State Find(string Name);
        State Find(int id);
        IEnumerable<State> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(State pt);
        State Add(State pt);
        IEnumerable<State> GetStateList();
        IEnumerable<State> GetStateListForCountryType(int Id);
        Task<IEquatable<State>> GetAsync();
        Task<State> FindAsync(int id);
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

    public class StateService : IStateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<State> _StateRepository;
        public StateService(IUnitOfWork unitOfWork,IRepository<State> StateRepo)
        {
            _unitOfWork = unitOfWork;
            _StateRepository = StateRepo;
        }
        public StateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StateRepository = unitOfWork.Repository<State>();
        }

        public State Find(string Name)
        {
            return _StateRepository.Query().Get().Where(i => i.StateName == Name).FirstOrDefault();
        }


        public State Find(int id)
        {
            return _unitOfWork.Repository<State>().Find(id);            
        }

        public State Create(State pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<State>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<State>().Delete(id);
        }

        public void Delete(State pt)
        {
            _unitOfWork.Repository<State>().Delete(pt);
        }

        public void Update(State pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<State>().Update(pt);
        }

        public IEnumerable<State> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<State>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.StateName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<State> GetStateList()
        {
            var pt = _unitOfWork.Repository<State>().Query().Include(i => i.Country).Get().OrderBy(m=>m.StateName);

            return pt;
        }

        public IEnumerable<State> GetStateListForCountryType(int Id)
        {
            var pt = _unitOfWork.Repository<State>().Query().Get().Where(i => i.CountryId == Id);

            return pt;
        }


        public State Add(State pt)
        {
            _unitOfWork.Repository<State>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _StateRepository.Instance
                        orderby p.StateName
                        select p.StateId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _StateRepository.Instance
                        orderby p.StateName
                        select p.StateId).FirstOrDefault();
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

                temp = (from p in _StateRepository.Instance
                        orderby p.StateName
                        select p.StateId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _StateRepository.Instance
                        orderby p.StateName
                        select p.StateId).AsEnumerable().LastOrDefault();
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
            var list = (from pr in _StateRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.StateName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.StateName
                        select new ComboBoxResult
                        {
                            text = pr.StateName,
                            id = pr.StateId.ToString()
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

            IEnumerable<State> States = from pr in _StateRepository.Instance
                                            where pr.StateId == Id
                                            select pr;

            ProductJson.id = States.FirstOrDefault().StateId.ToString();
            ProductJson.text = States.FirstOrDefault().StateName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<State> States = from pr in _StateRepository.Instance
                                                where pr.StateId == temp
                                                select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = States.FirstOrDefault().StateId.ToString(),
                    text = States.FirstOrDefault().StateName
                });
            }
            return ProductJson;
        }

        public Task<IEquatable<State>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<State> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
