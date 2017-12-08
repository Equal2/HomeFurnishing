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
        Task<IEquatable<Person>> GetAsync();
        Task<Person> FindAsync(int id);
        string GetMaxCode();
        bool CheckDuplicate(string Name, string Sufix, int PersonId = 0);
        string GetNewPersonCode();
    }

    public class PersonService : IPersonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Person> _personRepository;

        public PersonService(IUnitOfWork unitOfWork, IRepository<Person> personRepo)
        {
            _unitOfWork = unitOfWork;
            _personRepository = personRepo;
        }
        public PersonService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _personRepository = unitOfWork.Repository<Person>();
        }

        public Person GetPerson(int personId)
        {
            return _personRepository.Query()                        
                        .Get().Where(m => m.PersonID == personId).FirstOrDefault ();
        }

        public Person Find(int id)
        {
            return _personRepository.Find(id);
        }

        public Person Create(Person person)
        {
            person.ObjectState = ObjectState.Added;
            _personRepository.Insert(person);
            return person;
        }

        public void Delete(int id)
        {
            _personRepository.Delete(id);
        }

        public void Delete(Person person)
        {
            _personRepository.Delete(person);
        }

        public void Update(Person person)
        {
            person.ObjectState = ObjectState.Modified;
            _personRepository.Update(person);
        }


        public Person FindByName(string PersonName)
        {

            Person p = _personRepository.Query().Get().Where(i => i.Name == PersonName).FirstOrDefault();

            return p;
        }
       
        public Person FindByCode(string PersonCode)
        {

            Person p = _personRepository.Query().Get().Where(i => i.Code == PersonCode).FirstOrDefault();

            return p;
        }


        public IEnumerable<Person> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var person = _personRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return person;
        }

        public IEnumerable<Person> GetEmployeeList()
        {
            var emp = _personRepository.Query().Get().Where(m => m.IsActive == true).ToList().OrderBy(m => m.Name);

            return emp;
        }

        public IEnumerable<Person> GetSupplierList()
        {
            var sup = _personRepository.Query().Get().Where(m => m.IsActive == true).ToList().OrderBy(m => m.Name);

            return sup;
        }


        public IEnumerable<Person> GetPersonList()
        {
            var person = _personRepository.Query().Get().Where(m => m.IsActive == true).ToList().OrderBy(m => m.Name);              

            return person;
        }

        public Person Add(Person person)
        {
            _personRepository.Insert(person);
            return person;
        }

        public Person GetPersonByLoginId(string loginId)
        {
            Person p = _personRepository.Find(loginId);
             return p;
        }
        public void Dispose()
        {
            _unitOfWork.Dispose();
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
            
            var maxVal = _personRepository.Query().Get().Select(z => z.Code).DefaultIfEmpty().ToList()
                        .Select(sx => int.TryParse(sx, out x) ? x : 0).Max();

            return (maxVal + 1).ToString();
        }

        public bool CheckDuplicate(string Name, string Suffix, int PersonId = 0)
        {
            var temp = (from P in _personRepository.Instance
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
            IEnumerable<PersonCodeViewModel> temp = _unitOfWork.SqlQuery<PersonCodeViewModel>("Web.sp_GetPersonCode").ToList();

            if (temp != null)
            {
                return temp.FirstOrDefault().PersonCode;
            }
            else
            {
                return null;
            }
            
        }

        public ComboBoxPagedResult GetListWithProcess(string searchTerm, int pageSize, int pageNum, int ProcessId)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            string DivId = "|" + CurrentDivisionId.ToString() + "|";
            string SiteId = "|" + CurrentSiteId.ToString() + "|";

            var list = (from b in _personRepository.Instance
                        join bus in _unitOfWork.Repository<BusinessEntity>().Instance on b.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join p in _unitOfWork.Repository<Person>().Instance on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        join pp in _unitOfWork.Repository<PersonProcess>().Instance on b.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        where PersonProcessTab.ProcessId == ProcessId
                        && (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (PersonTab.Name.ToLower().Contains(searchTerm.ToLower()) || PersonTab.Code.ToLower().Contains(searchTerm.ToLower())))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1
                        && (PersonTab.IsActive == null ? 1 == 1 : PersonTab.IsActive == true)
                        && BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1
                        orderby PersonTab.Name
                        select new ComboBoxResult
                        {
                            id = b.PersonID.ToString(),
                            text = PersonTab.Name + "|" + PersonTab.Code,
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

        public ComboBoxPagedResult GetListWithDocType(string searchTerm, int pageSize, int pageNum, int DocTypeId)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            string DivId = "|" + CurrentDivisionId.ToString() + "|";
            string SiteId = "|" + CurrentSiteId.ToString() + "|";

            var list = (from b in _personRepository.Instance
                        join bus in _unitOfWork.Repository<BusinessEntity>().Instance on b.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join p in _unitOfWork.Repository<Person>().Instance on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        where b.DocTypeId == DocTypeId
                        && (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (PersonTab.Name.ToLower().Contains(searchTerm.ToLower()) || PersonTab.Code.ToLower().Contains(searchTerm.ToLower())))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1
                        && (PersonTab.IsActive == null ? 1 == 1 : PersonTab.IsActive == true)
                        && BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1
                        orderby PersonTab.Name
                        select new ComboBoxResult
                        {
                            id = b.PersonID.ToString(),
                            text = PersonTab.Name + "|" + PersonTab.Code,
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

            IEnumerable<Person> Person = from pr in _unitOfWork.Repository<Person>().Instance
                                             where pr.PersonID == Id
                                             select pr;

            ProductJson.id = Person.FirstOrDefault().PersonID.ToString();
            ProductJson.text = Person.FirstOrDefault().Name;

            return ProductJson;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            string DivId = "|" + CurrentDivisionId.ToString() + "|";
            string SiteId = "|" + CurrentSiteId.ToString() + "|";

            var list = (from b in _personRepository.Instance
                        join bus in _unitOfWork.Repository<BusinessEntity>().Instance on b.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join p in _unitOfWork.Repository<Person>().Instance on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (PersonTab.Name.ToLower().Contains(searchTerm.ToLower()) || PersonTab.Code.ToLower().Contains(searchTerm.ToLower())))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1
                        && (PersonTab.IsActive == null ? 1 == 1 : PersonTab.IsActive == true)
                        && BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1
                        orderby PersonTab.Name
                        select new ComboBoxResult
                        {
                            id = b.PersonID.ToString(),
                            text = PersonTab.Name + "|" + PersonTab.Code,
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
    }

    public class PersonCodeViewModel
    {
        public string PersonCode { get; set; }
    }
}
