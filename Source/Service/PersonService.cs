using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;

using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModels;
using System.Data.SqlClient;

namespace Service
{
    public interface IPersonService : IDisposable
    {
        Person Create(Person person);
        void Delete(int id);
        void Delete(Person person);
        Person GetPerson(int personId);
        IEnumerable<Person> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Person person);
        Person Add(Person person);
        IEnumerable<Person> GetPersonList();
        Person GetPersonByLoginId(string loginId);
        Person Find(int id);
        Person FindByName(string PersonName);
        Person FindByCode(string PersonCode);
        Supplier GetSupplierByLoginId(string loginId);
        Employee GetEmployeeByLoginId(string employeeLoginId);
        Task<IEquatable<Person>> GetAsync();
        Task<Person> FindAsync(int id);
        IEnumerable<Person> GetEmployeeList();
        IEnumerable<Person> GetSupplierList();
        string GetMaxCode();
        Employee GetEmployee(int PersonId);
        bool CheckDuplicate(string Name, string Sufix, int PersonId = 0);

        string GetNewPersonCode();


        //New Person Master Mothods
        IQueryable<PersonIndexViewModel> GetPersonListForIndex(int id);
        int NextId(int id);
        int PrevId(int id);
        PersonViewModel GetPersonViewModelForEdit(int id);
        string FGetNewCode(int DocTypeId, int DivisionId, int SiteId, string ProcName);

    }

    public class PersonService : IPersonService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Person> _personRepository;
        RepositoryQuery<Person> PersonRepository; 
        ApplicationDbContext db = new ApplicationDbContext();

        public PersonService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _personRepository = new Repository<Person>(db);
            PersonRepository = new RepositoryQuery<Person>(_personRepository);
        }

        public Person GetPerson(int personId)
        {
            return _unitOfWork.Repository<Person>().Query()
                        .Include(m => m.ApplicationUser)
                        .Get().Where(m => m.PersonID == personId).FirstOrDefault ();
        }

        public Employee GetEmployee(int PersonId)
        {
            return _unitOfWork.Repository<Employee>().Query()
                .Include(m => m.Person)
                .Include(m => m.Person.ApplicationUser)
                .Get().Where(m => m.PersonID == PersonId).FirstOrDefault();
        }

        public Person Find(int id)
        {
            return _unitOfWork.Repository<Person>().Find(id);
        }

        public Person Create(Person person)
        {
            person.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Person>().Insert(person);
            return person;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Person>().Delete(id);
        }

        public void Delete(Person person)
        {
            _unitOfWork.Repository<Person>().Delete(person);
        }

        public void Update(Person person)
        {
            person.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Person>().Update(person);
        }


        public Person FindByName(string PersonName)
        {

            Person p = _unitOfWork.Repository<Person>().Query().Get().Where(i => i.Name == PersonName).FirstOrDefault();

            return p;
        }
       
        public Person FindByCode(string PersonCode)
        {

            Person p = _unitOfWork.Repository<Person>().Query().Get().Where(i => i.Code  == PersonCode).FirstOrDefault();

            return p;
        }


        public IEnumerable<Person> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var person = _unitOfWork.Repository<Person>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return person;
        }

        public IEnumerable<Person> GetEmployeeList()
        {
            var emp = _unitOfWork.Repository<Person>().Query().Get().Where(m => m.IsActive == true).ToList().OrderBy(m => m.Name);

            return emp;
        }

        public IEnumerable<Person> GetSupplierList()
        {
            var sup = _unitOfWork.Repository<Person>().Query().Get().Where(m=>m.IsActive==true).ToList().OrderBy(m => m.Name);

            return sup;
        }


        public IEnumerable<Person> GetPersonList()
        {
            var person = _unitOfWork.Repository<Person>().Query().Get().Where(m => m.IsActive == true).ToList().OrderBy(m => m.Name);              

            return person;
        }

        public Person Add(Person person)
        {
            _unitOfWork.Repository<Person>().Insert(person);
            return person;
        }

        public Person GetPersonByLoginId(string loginId)
        {
            Person p = _unitOfWork.Repository<Person>().Find(loginId);
             return p;
        }

        public Supplier GetSupplierByLoginId(string loginId)
        {
           //var supplier= _unitOfWork.Repository<Person>().Query().Get().Where(s => s.ApplicationUser.Id == loginId).FirstOrDefault();

            Supplier supplier = new Supplier();
           return supplier;
        }

        public Employee GetEmployeeByLoginId(string employeeLoginId)
        {
            var employee = _unitOfWork.Repository<Employee>().Query().Include(m => m.Person).Get().Where(s => s.Person.ApplicationUser.Id == employeeLoginId).FirstOrDefault();
            return employee;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<Person>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Person> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public string GetMaxCode()
        {
            int x;
            
            var maxVal = db.Persons.Select(z => z.Code).DefaultIfEmpty().ToList()
                        .Select(sx => int.TryParse(sx, out x) ? x : 0).Max();

            return (maxVal + 1).ToString();
        }

        public bool CheckDuplicate(string Name, string Suffix, int PersonId = 0)
        {
            var temp = (from P in db.Persons
                        where P.Name == Name && P.Suffix == Suffix && P.PersonID != PersonId
                        select new
                        {
                            PersonId = P.PersonID
                        }).FirstOrDefault();


            if (temp != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetNewPersonCode()
        {
            IEnumerable<PersonCodeViewModel> temp = db.Database.SqlQuery<PersonCodeViewModel>("Web.sp_GetPersonCode").ToList();

            if (temp != null)
            {
                return temp.FirstOrDefault().PersonCode;
            }
            else
            {
                return null;
            }
            
        }

        public IQueryable<PersonIndexViewModel> GetPersonListForIndex(int id)
        {
            var PersonAddress = from Pa in db.PersonAddress
                                group new { Pa } by new { Pa.PersonId } into Result
                                select new
                                {
                                    PersonId = Result.Key.PersonId,
                                    Address = Result.Max(m => m.Pa.Address)
                                };

            var temp = from p in db.Persons
                       join Pr in db.PersonRole on p.PersonID equals Pr.PersonId into PersonRoleTable from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                       join Pa in PersonAddress on p.PersonID equals Pa.PersonId into PersonAddressTable from PersonAddressTab in PersonAddressTable.DefaultIfEmpty()
                       where PersonRoleTab.RoleDocTypeId == id
                       orderby p.Name
                       select new PersonIndexViewModel
                       {
                           PersonId = p.PersonID,
                           DocTypeId = PersonRoleTab.RoleDocTypeId,
                           Name = p.Name,
                           Suffix = p.Suffix,
                           Code = p.Code,
                           Mobile = p.Mobile,
                           Email = p.Email,
                           Address = PersonAddressTab.Address
                       };

            return temp;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Persons
                        orderby p.Name
                        select p.PersonID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Persons
                        orderby p.Name
                        select p.PersonID).FirstOrDefault();
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

                temp = (from p in db.Persons
                        orderby p.Name
                        select p.PersonID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Persons 
                        orderby p.Name
                        select p.PersonID).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public PersonViewModel GetPersonViewModelForEdit(int id)
        {

            PersonViewModel Personviewmodel = (from b in db.Persons
                                                     join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                                                     from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                                                     join ac in db.LedgerAccount on b.PersonID equals ac.PersonId into AccountTable
                                                     from AccountTab in AccountTable.DefaultIfEmpty()
                                                     where b.PersonID == id
                                               select new PersonViewModel
                                                     {
                                                         PersonID = b.PersonID,
                                                         DocTypeId = b.DocTypeId,
                                                         Name = b.Name,
                                                         Suffix = b.Suffix,
                                                         Code = b.Code,
                                                         Phone = b.Phone,
                                                         Mobile = b.Mobile,
                                                         Email = b.Email,
                                                         TdsCategoryId = BusinessEntityTab.TdsCategoryId,
                                                         TdsGroupId = BusinessEntityTab.TdsGroupId,
                                                         IsSisterConcern = BusinessEntityTab.IsSisterConcern,
                                                         CreaditDays = BusinessEntityTab.CreaditDays,
                                                         CreaditLimit = BusinessEntityTab.CreaditLimit,
                                                         GuarantorId = BusinessEntityTab.GuarantorId,
                                                         IsActive = b.IsActive,
                                                         LedgerAccountGroupId = AccountTab.LedgerAccountGroupId,
                                                         SalesTaxGroupPartyId = BusinessEntityTab.SalesTaxGroupPartyId,
                                                         SalesTaxGroupPartyName = BusinessEntityTab.SalesTaxGroupParty.ChargeGroupPersonName,
                                                         CreatedBy = b.CreatedBy,
                                                         CreatedDate = b.CreatedDate,
                                                         AccountId = AccountTab.LedgerAccountId,
                                                         DivisionIds = BusinessEntityTab.DivisionIds,
                                                         SiteIds = BusinessEntityTab.SiteIds,
                                                         Tags = b.Tags,
                                                         ImageFileName = b.ImageFileName,
                                                         ImageFolderName = b.ImageFolderName
                                                     }
                   ).FirstOrDefault();


            var PersonAddress = (from Pa in db.PersonAddress
                                 where Pa.PersonId == id && Pa.AddressType == null
                                 select new
                                 {
                                     Address = Pa.Address,
                                     CityId = Pa.CityId,
                                     Zipcode = Pa.Zipcode,
                                     PersonAddressID = Pa.PersonAddressID,
                                 }).FirstOrDefault();

            if (Personviewmodel != null)
            {
                Personviewmodel.PersonAddressID = PersonAddress.PersonAddressID;
                Personviewmodel.Address = PersonAddress.Address;
                Personviewmodel.CityId = PersonAddress.CityId;
                Personviewmodel.Zipcode = PersonAddress.Zipcode;
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
                        Personviewmodel.PersonRegistrationPanNoID = item.PersonRegistrationId;
                        Personviewmodel.PanNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.GstNo)
                    {
                        Personviewmodel.PersonRegistrationGstNoID = item.PersonRegistrationId;
                        Personviewmodel.GstNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.CstNo)
                    {
                        Personviewmodel.PersonRegistrationCstNoID = item.PersonRegistrationId;
                        Personviewmodel.CstNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.TinNo)
                    {
                        Personviewmodel.PersonRegistrationTinNoID = item.PersonRegistrationId;
                        Personviewmodel.TinNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.AadharNo)
                    {
                        Personviewmodel.PersonRegistrationAadharNoID = item.PersonRegistrationId;
                        Personviewmodel.AadharNo = item.RregistrationNo;
                    }
                }
            }


            string Divisions = Personviewmodel.DivisionIds;
            if (Divisions != null)
            {
                Divisions = Divisions.Replace('|', ' ');
                Personviewmodel.DivisionIds = Divisions;
            }

            string Sites = Personviewmodel.SiteIds;
            if (Sites != null)
            {
                Sites = Sites.Replace('|', ' ');
                Personviewmodel.SiteIds = Sites;
            }

            return Personviewmodel;

        }


        public string FGetNewCode(int DocTypeId, int DivisionId, int SiteId, string ProcName)
        {
            SqlParameter SqlParameterDocTypeId = new SqlParameter("@DocTypeId", DocTypeId);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);

            if (ProcName != "" && ProcName != null)
            {
                NewCodeViewModel NewCodeViewModel = db.Database.SqlQuery<NewCodeViewModel>(ProcName + " @DocTypeId , @DivisionId , @SiteId ", SqlParameterDocTypeId, SqlParameterDivisionId, SqlParameterSiteId).FirstOrDefault();

                if (NewCodeViewModel != null)
                {
                    return NewCodeViewModel.NewCode;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
                {
                    sqlConnection.Open();
                    SqlCommand Cmd = new SqlCommand(" SELECT replace(str(Convert(NVARCHAR,IsNull(Max(Try_parse(Code AS BIGINT)),0) + 1),8),' ','0') AS NewCode FROM Web.People WHERE Code LIKE '00%'  ", sqlConnection);
                    string PersonCode = Cmd.ExecuteScalar().ToString();
                    return PersonCode;
                }
            }
        }
    }

    public class PersonCodeViewModel
    {
        public string PersonCode { get; set; }
    }

    
}
