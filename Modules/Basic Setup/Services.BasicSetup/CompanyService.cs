using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using Models.Company.Models;
using Infrastructure.IO;
using Models.BasicSetup.ViewModels;

namespace Services.BasicSetup
{
    public interface ICompanyService : IDisposable
    {
        Company Create(Company pt);
        void Delete(int id);
        void Delete(Company s);
        Company Find(int Id);
        void Update(Company s);
        IEnumerable<Company> GetCompanyList();
        IQueryable<Company> GetCompanyListForIndex();
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

    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Company> _CompanyRepository;
        public CompanyService(IUnitOfWork unitOfWork, IRepository<Company> CompanyRepo)
        {
            _unitOfWork = unitOfWork;
            _CompanyRepository = CompanyRepo;
        }
        public CompanyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CompanyRepository = unitOfWork.Repository<Company>();
        }

        public Company Find(int id)
        {
            return _unitOfWork.Repository<Company>().Query().Include(m => m.City).Get().Where(m => m.CompanyId == id).FirstOrDefault();

        }

        public Company Create(Company s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Company>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Company>().Delete(id);
        }

        public void Delete(Company s)
        {
            _unitOfWork.Repository<Company>().Delete(s);
        }

        public void Update(Company s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Company>().Update(s);
        }


        public IEnumerable<Company> GetCompanyList()
        {
            var pt = (from p in _CompanyRepository.Instance
                      orderby p.CompanyName
                      select p);

            return pt;
        }

        public IQueryable<Company> GetCompanyListForIndex()
        {
            var pt = _unitOfWork.Repository<Company>().Query().Get().OrderBy(m => m.CompanyName);

            return pt;
        }
        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _CompanyRepository.Instance
                        orderby p.CompanyName
                        select p.CompanyId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _CompanyRepository.Instance
                        orderby p.CompanyName
                        select p.CompanyId).FirstOrDefault();
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

                temp = (from p in _CompanyRepository.Instance
                        orderby p.CompanyName
                        select p.CompanyId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _CompanyRepository.Instance
                        orderby p.CompanyName
                        select p.CompanyId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }



        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _CompanyRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.CompanyName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.CompanyName
                        select new ComboBoxResult
                        {
                            text = pr.CompanyName,
                            id = pr.CompanyId.ToString()
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

            IEnumerable<Company> Companys = from pr in _CompanyRepository.Instance
                                            where pr.CompanyId == Id
                                            select pr;

            ProductJson.id = Companys.FirstOrDefault().CompanyId.ToString();
            ProductJson.text = Companys.FirstOrDefault().CompanyName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Company> Companys = from pr in _CompanyRepository.Instance
                                                where pr.CompanyId == temp
                                                select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Companys.FirstOrDefault().CompanyId.ToString(),
                    text = Companys.FirstOrDefault().CompanyName
                });
            }
            return ProductJson;
        }


        public bool CheckForNameExists(string Name)
        {
            int CompanyId = (int)System.Web.HttpContext.Current.Session["CompanyId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _CompanyRepository.Instance
                        where pr.CompanyName == Name
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;

        }
        public bool CheckForNameExists(string Name, int Id)
        {
            int CompanyId = (int)System.Web.HttpContext.Current.Session["CompanyId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _CompanyRepository.Instance
                        where pr.CompanyName == Name && pr.CompanyId != Id
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

    }
}
