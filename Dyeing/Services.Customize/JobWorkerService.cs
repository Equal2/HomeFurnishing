using System.Collections.Generic;
using System.Linq;
using Data;
using System;
using Model;
using System.Threading.Tasks;
using Models.Customize.Models;
using Models.Customize.ViewModels;
using Infrastructure.IO;
using Models.BasicSetup.Models;
using Models.BasicSetup.ViewModels;

namespace Services.Customize
{
    public interface IJobWorkerService : IDisposable
    {
        JobWorker Create(JobWorker JobWorker);
        void Delete(int id);
        void Delete(JobWorker JobWorker);
        JobWorker GetJobWorker(int JobWorkerId);
        IEnumerable<JobWorker> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobWorker JobWorker);
        JobWorker Add(JobWorker JobWorker);
        IEnumerable<JobWorker> GetJobWorkerList();
        Task<IEquatable<JobWorker>> GetAsync();
        Task<JobWorker> FindAsync(int id);
        JobWorker GetJobWorkerByName(string Name);
        JobWorker Find(int id);
        //JobWorkerViewModel GetJobWorkerViewModel(int id);
        int NextId(int id);
        int PrevId(int id);
        //IQueryable<JobWorkerIndexViewModel> GetJobWorkerListForIndex();

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
         /// <summary>
        /// *General Function*
        /// This function will create the help list for Projects
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <param name="ProcessId">ProcessId</param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetListWithProcess(string searchTerm, int pageSize, int pageNum, int ProcessId);
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

    public class JobWorkerService : IJobWorkerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobWorker> _JobworkerRepository;
        public JobWorkerService(IUnitOfWork unitOfWork, IRepository<JobWorker> JobWorkerRepo)
        {
            _unitOfWork = unitOfWork;
            _JobworkerRepository = JobWorkerRepo;
        }
        public JobWorkerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobworkerRepository = unitOfWork.Repository<JobWorker>();
        }

        public JobWorker GetJobWorkerByName(string JobWorker)
        {
            return (from b in _JobworkerRepository.Instance
                    join p in _unitOfWork.Repository<Person>().Instance on b.PersonID equals p.PersonID into PersonTable
                    from PersonTab in PersonTable.DefaultIfEmpty()
                    where PersonTab.Name == JobWorker
                    select b
                        ).FirstOrDefault();
        }
        public JobWorker GetJobWorker(int JobWorkerId)
        {
            return _JobworkerRepository.Find(JobWorkerId);
        }

        public JobWorker Find(int id)
        {
            return _JobworkerRepository.Find(id);
        }

        public JobWorker Create(JobWorker JobWorker)
        {
            JobWorker.ObjectState = ObjectState.Added;
            _JobworkerRepository.Add(JobWorker);
            return JobWorker;
        }

        public void Delete(int id)
        {
            _JobworkerRepository.Delete(id);
        }

        public void Delete(JobWorker JobWorker)
        {
            _JobworkerRepository.Delete(JobWorker);
        }

        public void Update(JobWorker JobWorker)
        {
            JobWorker.ObjectState = ObjectState.Modified;
            _JobworkerRepository.Update(JobWorker);
        }

        public IEnumerable<JobWorker> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var JobWorker = _JobworkerRepository
                .Query().Include(m => m.Person)
                .OrderBy(q => q.OrderBy(c => c.Person.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Person.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return JobWorker;
        }

        public IEnumerable<JobWorker> GetJobWorkerList()
        {
            var JobWorker = _JobworkerRepository.Query().Include(m => m.Person).Get().Where(m => m.Person.IsActive == true).OrderBy(m => m.Person.Name);

            return JobWorker;
        }

        public JobWorker Add(JobWorker JobWorker)
        {
            _JobworkerRepository.Insert(JobWorker);
            return JobWorker;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<JobWorker>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobWorker> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from b in _JobworkerRepository.Instance
                        join p in _unitOfWork.Repository<Person>().Instance on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from b in _JobworkerRepository.Instance
                        join p in _unitOfWork.Repository<Person>().Instance on b.PersonID equals p.PersonID into PersonTable
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

                temp = (from b in _JobworkerRepository.Instance
                        join p in _unitOfWork.Repository<Person>().Instance on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from b in _JobworkerRepository.Instance
                        join p in _unitOfWork.Repository<Person>().Instance on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        //public JobWorkerViewModel GetJobWorkerViewModel(int id)
        //{

        //    JobWorkerViewModel JobWorkerviewmodel = (from b in _JobworkerRepository.Instance
        //                                             join bus in _unitOfWork.Repository<BusinessEntity>().Instance on b.PersonID equals bus.PersonID into BusinessEntityTable
        //                                             from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
        //                                             join p in _unitOfWork.Repository<Person>().Instance on BusinessEntityTab.PersonID equals p.PersonID into PersonTable
        //                                             from PersonTab in PersonTable.DefaultIfEmpty()
        //                                             join pa in _unitOfWork.Repository<PersonAddress>().Instance on b.PersonID equals pa.PersonId into PersonAddressTable
        //                                             from PersonAddressTab in PersonAddressTable.DefaultIfEmpty()
        //                                             join ac in _unitOfWork.Repository<LedgerAccount>().Instance on b.PersonID equals ac.PersonId into AccountTable
        //                                             from AccountTab in AccountTable.DefaultIfEmpty()
        //                                             where b.PersonID == id
        //                                             select new JobWorkerViewModel
        //                                             {
        //                                                 PersonId = b.PersonID,
        //                                                 Name = PersonTab.Name,
        //                                                 Suffix = PersonTab.Suffix,
        //                                                 Code = PersonTab.Code,
        //                                                 Phone = PersonTab.Phone,
        //                                                 Mobile = PersonTab.Mobile,
        //                                                 Email = PersonTab.Email,
        //                                                 Address = PersonAddressTab.Address,
        //                                                 CityId = PersonAddressTab.CityId,
        //                                                 Zipcode = PersonAddressTab.Zipcode,
        //                                                 TdsCategoryId = BusinessEntityTab.TdsCategoryId,
        //                                                 TdsGroupId = BusinessEntityTab.TdsGroupId,
        //                                                 IsSisterConcern = BusinessEntityTab.IsSisterConcern,
        //                                                 PersonRateGroupId = BusinessEntityTab.PersonRateGroupId,
        //                                                 CreaditDays = BusinessEntityTab.CreaditDays,
        //                                                 CreaditLimit = BusinessEntityTab.CreaditLimit,
        //                                                 GuarantorId = BusinessEntityTab.GuarantorId,
        //                                                 IsActive = PersonTab.IsActive,
        //                                                 LedgerAccountGroupId = AccountTab.LedgerAccountGroupId,
        //                                                 CreatedBy = PersonTab.CreatedBy,
        //                                                 CreatedDate = PersonTab.CreatedDate,
        //                                                 PersonAddressID = PersonAddressTab.PersonAddressID,
        //                                                 AccountId = AccountTab.LedgerAccountId,
        //                                                 DivisionIds = BusinessEntityTab.DivisionIds,
        //                                                 SiteIds = BusinessEntityTab.SiteIds,
        //                                                 Tags = PersonTab.Tags,
        //                                                 ImageFileName = PersonTab.ImageFileName,
        //                                                 ImageFolderName = PersonTab.ImageFolderName
        //                                             }
        //           ).FirstOrDefault();

        //    var PersonProcess = (from pp in _unitOfWork.Repository<PersonProcess>().Instance
        //                         where pp.PersonId == id
        //                         select new
        //                         {
        //                             ProcessId = pp.ProcessId
        //                         }).ToList();

        //    foreach (var item in PersonProcess)
        //    {
        //        if (JobWorkerviewmodel.ProcessIds == "" || JobWorkerviewmodel.ProcessIds == null)
        //        {
        //            JobWorkerviewmodel.ProcessIds = item.ProcessId.ToString();
        //        }
        //        else
        //        {
        //            JobWorkerviewmodel.ProcessIds = JobWorkerviewmodel.ProcessIds + "," + item.ProcessId.ToString();
        //        }
        //    }

        //    var PersonRegistration = (from pp in _unitOfWork.Repository<PersonRegistration>().Instance
        //                              where pp.PersonId == id
        //                              select new
        //                              {
        //                                  PersonRegistrationId = pp.PersonRegistrationID,
        //                                  RregistrationType = pp.RegistrationType,
        //                                  RregistrationNo = pp.RegistrationNo
        //                              }).ToList();

        //    if (PersonRegistration != null)
        //    {
        //        foreach (var item in PersonRegistration)
        //        {
        //            if (item.RregistrationType == PersonRegistrationType.PANNo)
        //            {
        //                JobWorkerviewmodel.PersonRegistrationPanNoID = item.PersonRegistrationId;
        //                JobWorkerviewmodel.PanNo = item.RregistrationNo;
        //            }
        //        }
        //    }


        //    string Divisions = JobWorkerviewmodel.DivisionIds;
        //    if (Divisions != null)
        //    {
        //        Divisions = Divisions.Replace('|', ' ');
        //        JobWorkerviewmodel.DivisionIds = Divisions;
        //    }

        //    string Sites = JobWorkerviewmodel.SiteIds;
        //    if (Sites != null)
        //    {
        //        Sites = Sites.Replace('|', ' ');
        //        JobWorkerviewmodel.SiteIds = Sites;
        //    }

        //    return JobWorkerviewmodel;

        //}

        //    public IQueryable<JobWorkerIndexViewModel> GetJobWorkerListForIndex()
        //    {
        //        var TempPan = from Pr in db.PersonRegistration
        //                      where Pr.RegistrationType == PersonRegistrationType.PANNo
        //                      select new
        //                      {
        //                          PersonId = Pr.PersonId,
        //                          PanNo = Pr.RegistrationNo
        //                      };

        //        var temp = from p in db.JobWorker
        //                   join p1 in db.Persons on p.PersonID equals p1.PersonID into PersonTable
        //                   from PersonTab in PersonTable.DefaultIfEmpty()
        //                   join Pan in TempPan on p.PersonID equals Pan.PersonId into PersonPanTable
        //                   from PersonPanTab in PersonPanTable.DefaultIfEmpty()
        //                   orderby PersonTab.Name
        //                   select new JobWorkerIndexViewModel
        //                   {
        //                       PersonId = PersonTab.PersonID,
        //                       Name = PersonTab.Name,
        //                       Code = PersonTab.Code,
        //                       Mobile = PersonTab.Mobile,
        //                       PanNo = PersonPanTab.PanNo
        //                   };
        //        return temp;
        //    }

        //    public int? GetJobWorkerForUser(string UserId)
        //    {
        //        var Emp = (from temp in db.JobWorker
        //                   join t in db.Persons on temp.PersonID equals t.PersonID
        //                   where t.ApplicationUser.Id == UserId
        //                   select temp.PersonID).FirstOrDefault();

        //        return Emp;
        //    }


        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _JobworkerRepository.Instance
                        join t in _unitOfWork.Repository<Person>().Instance on pr.PersonID equals t.PersonID
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (t.Name.ToLower().Contains(searchTerm.ToLower())))
                        orderby t.Name
                        select new ComboBoxResult
                        {
                            text = t.Name,
                            id = pr.PersonID.ToString()
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

        public ComboBoxPagedResult GetListWithProcess(string searchTerm, int pageSize, int pageNum, int ProcessId)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            string DivId = "|" + CurrentDivisionId.ToString() + "|";
            string SiteId = "|" + CurrentSiteId.ToString() + "|";

            var list = (from b in _JobworkerRepository.Instance
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

        public ComboBoxResult GetValue(int Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Person> Jobworkers = from pr in _unitOfWork.Repository<Person>().Instance
                                             where pr.PersonID == Id
                                             select pr;

            ProductJson.id = Jobworkers.FirstOrDefault().PersonID.ToString();
            ProductJson.text = Jobworkers.FirstOrDefault().Name;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Person> Jobworkers = from pr in _unitOfWork.Repository<Person>().Instance
                                                 where pr.PersonID == temp
                                                 select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Jobworkers.FirstOrDefault().PersonID.ToString(),
                    text = Jobworkers.FirstOrDefault().Name
                });
            }
            return ProductJson;
        }

    }

}
