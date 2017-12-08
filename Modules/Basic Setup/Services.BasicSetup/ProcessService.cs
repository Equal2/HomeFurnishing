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
    public interface IProcessService : IDisposable
    {
        Process Create(Process p);
        void Delete(int id);
        void Delete(Process p);        
        IEnumerable<Process> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Process p);
        Process Add(Process p);
        IEnumerable<Process> GetProcessList();
        IEnumerable<Process> GetProcessList(int prodyctTypeId);
        Task<IEquatable<Process>> GetAsync();
        Task<Process> FindAsync(int id);
        Process Find(string ProcessName);
        Process Find(int id);
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



    public class ProcessService : IProcessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Process> _processRepository;
        public ProcessService(IUnitOfWork unitOfWork, IRepository<Process> ProcessRepo)
        {
            _unitOfWork = unitOfWork;
            _processRepository = ProcessRepo;
        }
        public ProcessService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _processRepository = unitOfWork.Repository<Process>();
        }

        public Process Create(Process p)
        {
            p.ObjectState = ObjectState.Added;
            _processRepository.Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _processRepository.Delete(id);
        }

        public void Delete(Process p)
        {
            _processRepository.Delete(p);
        }

        public void Update(Process p)
        {
            p.ObjectState = ObjectState.Modified;
            _processRepository.Update(p);
        }

        public IEnumerable<Process> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _processRepository
                .Query()
                .Filter(q => !string.IsNullOrEmpty(q.ProcessName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public IEnumerable<Process> GetProcessList()
        {
            var p = _processRepository.Query().Get().OrderBy(m => m.ProcessName);

            return p;

        }

        public IEnumerable<Process> GetAccessoryList()
        {
            var p = _processRepository.Query().Get();
            return p;
        }



        public Process Find(string ProcessName)
        {

            Process p = _processRepository.Query().Get().Where(i => i.ProcessName == ProcessName).FirstOrDefault();

            return p;
        }

        public Process Find(int id)
        {
            return _processRepository.Find(id);
        }


        public IEnumerable<Process> GetProcessList(int productTypeId)
        {
            return _processRepository.Query().Get().OrderBy(m => m.ProcessName);
        }

        public Process Add(Process p)
        {
            _processRepository.Add(p);
            return p;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _processRepository.Instance
                        orderby p.ProcessName
                        select p.ProcessId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _processRepository.Instance
                        orderby p.ProcessName
                        select p.ProcessId).FirstOrDefault();
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

                temp = (from p in _processRepository.Instance
                        orderby p.ProcessName
                        select p.ProcessId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _processRepository.Instance
                        orderby p.ProcessName
                        select p.ProcessId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _processRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.ProcessName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.ProcessName
                        select new ComboBoxResult
                        {
                            text = pr.ProcessName,
                            id = pr.ProcessId.ToString()
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

            IEnumerable<Process> Processs = from pr in _processRepository.Instance
                                                  where pr.ProcessId == Id
                                                  select pr;

            ProductJson.id = Processs.FirstOrDefault().ProcessId.ToString();
            ProductJson.text = Processs.FirstOrDefault().ProcessName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Process> Processs = from pr in _processRepository.Instance
                                                      where pr.ProcessId == temp
                                                      select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Processs.FirstOrDefault().ProcessId.ToString(),
                    text = Processs.FirstOrDefault().ProcessName
                });
            }
            return ProductJson;
        }



        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<Process>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Process> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
