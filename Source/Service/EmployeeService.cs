using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModels;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;

namespace Service
{
    public interface IEmployeeService : IDisposable
    {
        Employee Create(Employee Employee);
        void Delete(int id);
        void Delete(Employee Employee);
        Employee GetEmployee(int EmployeeId);
        IEnumerable<Employee> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Employee Employee);
        Employee Add(Employee Employee);
        IEnumerable<Employee> GetEmployeeList();
        Task<IEquatable<Employee>> GetAsync();
        Task<Employee> FindAsync(int id);
        Employee GetEmployeeByName(string Name);
        Employee Find(int id);
        EmployeeViewModel GetEmployeeViewModel(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<EmployeeIndexViewModel> GetEmployeeListForIndex();
        int ? GetEmloyeeForUser(string UserId);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        ApplicationDbContext db = new ApplicationDbContext();
        public EmployeeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Employee GetEmployeeByName(string Employee)
        {
            return (from b in db.Employee
                    join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                    from PersonTab in PersonTable.DefaultIfEmpty()
                    where PersonTab.Name == Employee
                    select b
                        ).FirstOrDefault();
        }
        public Employee GetEmployee(int EmployeeId)
        {
            return _unitOfWork.Repository<Employee>().Find(EmployeeId);
        }

        public Employee Find(int id)
        {
            return _unitOfWork.Repository<Employee>().Find(id);
        }

        public Employee Create(Employee Employee)
        {
            Employee.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Employee>().Insert(Employee);
            return Employee;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Employee>().Delete(id);
        }

        public void Delete(Employee Employee)
        {
            _unitOfWork.Repository<Employee>().Delete(Employee);
        }

        public void Update(Employee Employee)
        {
            Employee.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Employee>().Update(Employee);
        }

        public IEnumerable<Employee> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var Employee = _unitOfWork.Repository<Employee>()
                .Query().Include(m => m.Person)
                .OrderBy(q => q.OrderBy(c => c.Person.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Person.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return Employee;
        }

        public IEnumerable<Employee> GetEmployeeList()
        {
            var Employee = _unitOfWork.Repository<Employee>().Query().Include(m => m.Person).Get().Where(m => m.Person.IsActive == true).OrderBy(m => m.Person.Name);

            return Employee;
        }

        public Employee Add(Employee Employee)
        {
            _unitOfWork.Repository<Employee>().Insert(Employee);
            return Employee;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Employee>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Employee> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from b in db.Employee
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from b in db.Employee
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).FirstOrDefault();
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

                temp = (from b in db.Employee
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from b in db.Employee
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public EmployeeViewModel GetEmployeeViewModel(int id)
        {
            EmployeeViewModel Employeeviewmodel = (from b in db.Employee
                                                     join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                                                     from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                                                     join p in db.Persons on BusinessEntityTab.PersonID equals p.PersonID into PersonTable
                                                     from PersonTab in PersonTable.DefaultIfEmpty()
                                   join pa in db.PersonAddress on b.PersonID equals pa.PersonId into PersonAddressTable
                                   from PersonAddressTab in PersonAddressTable.DefaultIfEmpty()
                                   join ac in db.LedgerAccount on b.PersonID equals ac.PersonId into AccountTable
                                   from AccountTab in AccountTable.DefaultIfEmpty()
                                   where b.PersonID == id
                                   select new EmployeeViewModel
                                   {
                                       PersonId = b.PersonID,
                                       DocTypeId = PersonTab.DocTypeId,
                                       Name = PersonTab.Name,
                                       Suffix = PersonTab.Suffix,
                                       Code = PersonTab.Code,
                                       Phone = PersonTab.Phone,
                                       Mobile = PersonTab.Mobile,
                                       Email = PersonTab.Email,
                                       Address = PersonAddressTab.Address,
                                       CityId = PersonAddressTab.CityId,
                                       Zipcode = PersonAddressTab.Zipcode,
                                       TdsCategoryId = BusinessEntityTab.TdsCategoryId,
                                       TdsGroupId = BusinessEntityTab.TdsGroupId,
                                       IsSisterConcern = BusinessEntityTab.IsSisterConcern,
                                       DesignationId = b.DesignationID,
                                       DepartmentId = b.DepartmentID,
                                       CreaditDays = BusinessEntityTab.CreaditDays,
                                       CreaditLimit = BusinessEntityTab.CreaditLimit,
                                       IsActive = PersonTab.IsActive,
                                       LedgerAccountGroupId = AccountTab.LedgerAccountGroupId,
                                       CreatedBy = PersonTab.CreatedBy,
                                       CreatedDate = PersonTab.CreatedDate,
                                       PersonAddressID = PersonAddressTab.PersonAddressID,
                                       AccountId = AccountTab.LedgerAccountId,
                                       DivisionIds = BusinessEntityTab.DivisionIds,
                                       SiteIds = BusinessEntityTab.SiteIds,
                                       Tags = PersonTab.Tags,
                                       ImageFileName = PersonTab.ImageFileName,
                                       ImageFolderName = PersonTab.ImageFolderName
                                   }
                   ).FirstOrDefault();

            var PersonProcess = (from pp in db.PersonProcess
                                 where pp.PersonId == id
                                 select new
                                 {
                                     ProcessId = pp.ProcessId
                                 }).ToList();

            foreach (var item in PersonProcess)
            {
                if (Employeeviewmodel.ProcessIds == "" || Employeeviewmodel.ProcessIds == null)
                {
                    Employeeviewmodel.ProcessIds = item.ProcessId.ToString();
                }
                else
                {
                    Employeeviewmodel.ProcessIds = Employeeviewmodel.ProcessIds + "," + item.ProcessId.ToString();
                }
            }


            var PersonRegistration = (from pp in db.PersonRegistration
                                      where pp.PersonId == id
                                      select new
                                      {
                                          PersonRegistrationId = pp.PersonRegistrationID,
                                          RregistrationType = pp.RegistrationType,
                                          RregistrationNo = pp.RegistrationNo
                                      }).ToList();

            if (PersonRegistration != null)
            {
                foreach (var item in PersonRegistration)
                {
                    if (item.RregistrationType == PersonRegistrationType.PANNo)
                    {
                        Employeeviewmodel.PersonRegistrationPanNoID = item.PersonRegistrationId;
                        Employeeviewmodel.PanNo = item.RregistrationNo;
                    }
                }
            }

            string Divisions = Employeeviewmodel.DivisionIds;
            if (Divisions != null)
            {
                Divisions = Divisions.Replace('|', ' ');
                Employeeviewmodel.DivisionIds = Divisions;
            }

            string Sites = Employeeviewmodel.SiteIds;
            if (Sites != null)
            {
                Sites = Sites.Replace('|', ' ');
                Employeeviewmodel.SiteIds = Sites;
            }

            return Employeeviewmodel;

          }

        public IQueryable<EmployeeIndexViewModel> GetEmployeeListForIndex()
        {
            var temp = from p in db.Employee
                       join p1 in db.Persons on p.PersonID equals p1.PersonID into PersonTable
                       from PersonTab in PersonTable.DefaultIfEmpty()
                       orderby PersonTab.Name
                       select new EmployeeIndexViewModel
                       {
                           PersonId = PersonTab.PersonID,
                           Name = PersonTab.Name,
                           Code=PersonTab.Code,
                           Mobile=PersonTab.Mobile,
                           Phone=PersonTab.Phone,
                           Suffix = PersonTab.Suffix,
                       };
            return temp;
        }

        public int ? GetEmloyeeForUser(string UserId)
        {
            var Emp = (from temp in db.Employee
                       join t in db.Persons on temp.PersonID equals t.PersonID
                       where t.ApplicationUser.Id == UserId
                       select temp.PersonID).FirstOrDefault();

            return Emp;


        }

    }

}
