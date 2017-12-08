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
    public interface IGateService : IDisposable
    {
        Gate Create(Gate pt);
        void Delete(int id);
        void Delete(Gate pt);
        Gate Find(string Name);
        Gate Find(int id);
        IEnumerable<Gate> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Gate pt);
        Gate Add(Gate pt);
        IEnumerable<Gate> GetGateList(int SiteId);
        Task<IEquatable<Gate>> GetAsync();
        Task<Gate> FindAsync(int id);
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

    public class GateService : IGateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Gate> _GateRepository;
        public GateService(IUnitOfWork unitOfWork, IRepository<Gate> gateRepo)
        {
            _unitOfWork = unitOfWork;
            _GateRepository = gateRepo;
        }
        public GateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _GateRepository = unitOfWork.Repository<Gate>();
        }

        public Gate Find(string Name)
        {
            return _GateRepository.Query().Get().Where(i => i.GateName == Name).FirstOrDefault();
        }


        public Gate Find(int id)
        {
            return _unitOfWork.Repository<Gate>().Find(id);
        }

        public Gate Create(Gate pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Gate>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Gate>().Delete(id);
        }

        public void Delete(Gate pt)
        {
            _unitOfWork.Repository<Gate>().Delete(pt);
        }

        public void Update(Gate pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Gate>().Update(pt);
        }

        public IEnumerable<Gate> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Gate>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.GateName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Gate> GetGateList(int SiteId)
        {
            var pt = _unitOfWork.Repository<Gate>().Query().Get().OrderBy(m => m.GateName).Where(m => m.SiteId == SiteId);

            return pt;
        }

        public Gate Add(Gate pt)
        {
            _unitOfWork.Repository<Gate>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _GateRepository.Instance
                        orderby p.GateName
                        select p.GateId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _GateRepository.Instance
                        orderby p.GateName
                        select p.GateId).FirstOrDefault();
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

                temp = (from p in _GateRepository.Instance
                        orderby p.GateName
                        select p.GateId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _GateRepository.Instance
                        orderby p.GateName
                        select p.GateId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _GateRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.GateName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.GateName
                        select new ComboBoxResult
                        {
                            text = pr.GateName,
                            id = pr.GateId.ToString()
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

            IEnumerable<Gate> Gates = from pr in _GateRepository.Instance
                                            where pr.GateId == Id
                                            select pr;

            ProductJson.id = Gates.FirstOrDefault().GateId.ToString();
            ProductJson.text = Gates.FirstOrDefault().GateName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Gate> Gates = from pr in _GateRepository.Instance
                                                where pr.GateId == temp
                                                select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Gates.FirstOrDefault().GateId.ToString(),
                    text = Gates.FirstOrDefault().GateName
                });
            }
            return ProductJson;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<Gate>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Gate> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
