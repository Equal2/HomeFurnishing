using System.Collections.Generic;
using System.Linq;
using Infrastructure.IO;
using System;
using Models.Company.DatabaseViews;
using Models.BasicSetup.ViewModels;
using Models.BasicSetup.Models;

namespace Service
{
    public interface IComboHelpListService : IDisposable
    {
        #region Getters

        /// <summary>
        /// *General Function*
        /// This function will create the help list for Users with Name,Name
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetUsers(string searchTerm, int pageSize, int pageNum);

        /// <summary>
        /// *General Function*
        /// This function will create the help list for Employee with Name,Name
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetEmployees(string searchTerm, int pageSize, int pageNum);

        /// <summary>
        /// *General Function*
        /// This function will create the help list for Employee with Name,Name
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetEmployeesWithProcess(string searchTerm, int pageSize, int pageNum, int filter);

        /// <summary>
        /// *General Function*
        /// This function will create the help list for Users with Id, Name
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetUsersById(string searchTerm, int pageSize, int pageNum);

        /// <summary>
        /// *General Function*
        /// This function will create the help list for Roles
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetRoles(string searchTerm, int pageSize, int pageNum);
        #endregion

        #region Setters

        /// <summary>
        /// *General Function*
        /// This function will return the object based on the Id
        /// </summary>
        /// <param name="Id">PrimaryKey of the record</param>
        /// <returns>ComboBoxResult</returns>
        ComboBoxResult GetUser(string Id);

        /// <summary>
        /// *General Function*
        /// This function will return the object based on the Id
        /// </summary>
        /// <param name="Id">PrimaryKey of the record</param>
        /// <returns>ComboBoxResult</returns>
        ComboBoxResult GetEmployee(int Id);

        /// <summary>
        /// *General Function*
        /// This function will return the object based on the Id
        /// </summary>
        /// <param name="Id">PrimaryKey of the record</param>
        /// <returns>ComboBoxResult</returns>
        ComboBoxResult GetRole(string Id);

        /// <summary>
        /// *General Function*
        /// This function will return the object based on the Id
        /// </summary>
        /// <param name="Id">PrimaryKey of the record</param>
        /// <returns>ComboBoxResult</returns>
        ComboBoxResult GetUserById(string Id);

        /// <summary>
        /// *General Function*
        /// This function will return list of object based on the Ids
        /// </summary>
        /// <param name="Id">PrimaryKey of the record(',' seperated string)</param>
        /// <returns>List<ComboBoxResult></returns>
        List<ComboBoxResult> GetMultipleUsers(string Id);

        /// <summary>
        /// *General Function*
        /// This function will return list of object based on the Ids
        /// </summary>
        /// <param name="Id">PrimaryKey of the record(',' seperated string)</param>
        /// <returns>List<ComboBoxResult></returns>
        List<ComboBoxResult> GetMultipleEmployees(string Id);

        /// <summary>
        /// *General Function*
        /// This function will return list of object based on the Ids
        /// </summary>
        /// <param name="Id">PrimaryKey of the record(',' seperated string)</param>
        /// <returns>List<ComboBoxResult></returns>
        List<ComboBoxResult> GetMultipleRoles(string Id);

        /// <summary>
        /// *General Function*
        /// This function will return list of object based on the Ids
        /// </summary>
        /// <param name="Id">PrimaryKey of the record(',' seperated string)</param>
        /// <returns>List<ComboBoxResult></returns>
        List<ComboBoxResult> GetMultipleUsersById(string Id);

        #endregion
    }

    public class ComboHelpListService : IComboHelpListService
    {
        private readonly IRepository<_Users> _UserRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<_Roles> _RoleRepository;
        private readonly IRepository<_Employee> _EmployeeRepository;

        public ComboHelpListService(IRepository<_Users> User, IUnitOfWork unitOfWork, IRepository<_Roles> roles, IRepository<_Employee> EmployeeRepo)
        {
            _unitOfWork = unitOfWork;
            _UserRepository = User;
            _RoleRepository = roles;
            _EmployeeRepository = EmployeeRepo;
        }
        public ComboHelpListService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _UserRepository = unitOfWork.Repository<_Users>();
            _RoleRepository = unitOfWork.Repository<_Roles>();
            _EmployeeRepository = unitOfWork.Repository<_Employee>();
        }

        public ComboBoxPagedResult GetUsers(string searchTerm, int pageSize, int pageNum)
        {

            var Query = (from ur in _UserRepository.Instance
                         where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (ur.UserName.ToLower().Contains(searchTerm.ToLower())))
                         orderby ur.UserName
                         select new ComboBoxResult
                         {
                             text = ur.UserName,
                             id = ur.UserName,
                         }
            );

            var records = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = records;
            Data.Total = count;

            return Data;

        }



        public ComboBoxResult GetUser(string Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<_Users> users = from p in _UserRepository.Instance
                                        where p.UserName == Id
                                        select p;

            ProductJson.id = users.FirstOrDefault().UserName.ToString();
            ProductJson.text = users.FirstOrDefault().UserName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetMultipleUsers(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                string temp = subStr[i];
                IEnumerable<_Users> users = from p in _UserRepository.Instance
                                            where p.UserName == temp
                                            select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = users.FirstOrDefault().UserName.ToString(),
                    text = users.FirstOrDefault().UserName
                });
            }

            return ProductJson;
        }

        public ComboBoxPagedResult GetUsersById(string searchTerm, int pageSize, int pageNum)
        {

            var Query = (from ur in _UserRepository.Instance
                         where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (ur.UserName.ToLower().Contains(searchTerm.ToLower())))
                         orderby ur.UserName
                         select new ComboBoxResult
                         {
                             text = ur.UserName,
                             id = ur.Id,
                         }
            );

            var records = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = records;
            Data.Total = count;

            return Data;

        }

        public ComboBoxResult GetUserById(string Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<_Users> users = from p in _UserRepository.Instance
                                        where p.Id == Id
                                        select p;

            ProductJson.id = users.FirstOrDefault().Id.ToString();
            ProductJson.text = users.FirstOrDefault().UserName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetMultipleUsersById(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                string temp = subStr[i];
                IEnumerable<_Users> users = from p in _UserRepository.Instance
                                            where p.Id == temp
                                            select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = users.FirstOrDefault().Id.ToString(),
                    text = users.FirstOrDefault().UserName
                });
            }

            return ProductJson;
        }


        public ComboBoxPagedResult GetRoles(string searchTerm, int pageSize, int pageNum)
        {

            var Query = (from ur in _RoleRepository.Instance
                         where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (ur.Name.ToLower().Contains(searchTerm.ToLower())))
                         && ur.Name != "SysAdmin"
                         orderby ur.Name
                         select new ComboBoxResult
                         {
                             text = ur.Name,
                             id = ur.Id,
                         }
            );

            var records = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = records;
            Data.Total = count;

            return Data;

        }



        public ComboBoxResult GetRole(string Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<_Roles> Roles = from p in _RoleRepository.Instance
                                        where p.Id == Id
                                        select p;

            ProductJson.id = Roles.FirstOrDefault().Id;
            ProductJson.text = Roles.FirstOrDefault().Name;

            return ProductJson;
        }

        public List<ComboBoxResult> GetMultipleRoles(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                string temp = subStr[i];
                IEnumerable<_Roles> Roles = from p in _RoleRepository.Instance
                                            where p.Id == temp
                                            select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Roles.FirstOrDefault().Id,
                    text = Roles.FirstOrDefault().Name
                });
            }

            return ProductJson;
        }

        public ComboBoxPagedResult GetEmployees(string searchTerm, int pageSize, int pageNum)
        {
            
            var Query = (from ur in _EmployeeRepository.Instance
                         join t in _unitOfWork.Repository<Person>().Instance on ur.PersonID equals t.PersonID
                         where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (t.Name.ToLower().Contains(searchTerm.ToLower())))
                         orderby t.Name
                         select new ComboBoxResult
                         {
                             text = t.Name,
                             id = t.PersonID.ToString(),
                         }
            );

            var records = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = records;
            Data.Total = count;

            return Data;

        }

        public ComboBoxPagedResult GetEmployeesWithProcess(string searchTerm, int pageSize, int pageNum, int ProcessId)
        {
             int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            string DivId = "|" + CurrentDivisionId.ToString() + "|";
            string SiteId = "|" + CurrentSiteId.ToString() + "|";

            var Query = from b in _EmployeeRepository.Instance
                        join bus in _unitOfWork.Repository<BusinessEntity>().Instance on b.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join p in _unitOfWork.Repository<Person>().Instance on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        join pp in _unitOfWork.Repository<PersonProcess>().Instance on b.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        where PersonProcessTab.ProcessId == ProcessId
                        && (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (PersonTab.Name.ToLower().Contains(searchTerm.ToLower()) || PersonTab.Code.ToLower().Contains(searchTerm.ToLower())))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1
                        && BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1
                        orderby PersonTab.Name
                        select new ComboBoxResult
                        {
                            id = b.PersonID.ToString(),
                            text = PersonTab.Name + " | " + PersonTab.Code
                        };

            var records = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = records;
            Data.Total = count;

            return Data;

        }



        public ComboBoxResult GetEmployee(int Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Person> Employees = from p in _EmployeeRepository.Instance
                                               join t in _unitOfWork.Repository<Person>().Instance on p.PersonID equals t.PersonID
                                        where t.PersonID == Id
                                        select t;

            ProductJson.id = Employees.FirstOrDefault().PersonID.ToString();
            ProductJson.text = Employees.FirstOrDefault().Name;

            return ProductJson;
        }

        public List<ComboBoxResult> GetMultipleEmployees(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Person> Employees = from p in _EmployeeRepository.Instance
                                                join t in _unitOfWork.Repository<Person>().Instance on p.PersonID equals t.PersonID
                                            where p.PersonID == temp
                                            select t;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Employees.FirstOrDefault().PersonID.ToString(),
                    text = Employees.FirstOrDefault().Name
                });
            }

            return ProductJson;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
