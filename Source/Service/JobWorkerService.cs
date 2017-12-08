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
        JobWorkerViewModel GetJobWorkerViewModel(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<JobWorkerIndexViewModel> GetJobWorkerListForIndex();
    }

    public class JobWorkerService : IJobWorkerService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        ApplicationDbContext db = new ApplicationDbContext();
        public JobWorkerService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public JobWorker GetJobWorkerByName(string JobWorker)
        {
            return (from b in db.JobWorker
                    join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                    from PersonTab in PersonTable.DefaultIfEmpty()
                    where PersonTab.Name == JobWorker
                    select b
                        ).FirstOrDefault();
        }
        public JobWorker GetJobWorker(int JobWorkerId)
        {
            return _unitOfWork.Repository<JobWorker>().Find(JobWorkerId);
        }

        public JobWorker Find(int id)
        {
            return _unitOfWork.Repository<JobWorker>().Find(id);
        }

        public JobWorker Create(JobWorker JobWorker)
        {
            JobWorker.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobWorker>().Insert(JobWorker);
            return JobWorker;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobWorker>().Delete(id);
        }

        public void Delete(JobWorker JobWorker)
        {
            _unitOfWork.Repository<JobWorker>().Delete(JobWorker);
        }

        public void Update(JobWorker JobWorker)
        {
            JobWorker.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobWorker>().Update(JobWorker);
        }

        public IEnumerable<JobWorker> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var JobWorker = _unitOfWork.Repository<JobWorker>()
                .Query().Include(m => m.Person)
                .OrderBy(q => q.OrderBy(c => c.Person.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Person.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return JobWorker;
        }

        public IEnumerable<JobWorker> GetJobWorkerList()
        {
            var JobWorker = _unitOfWork.Repository<JobWorker>().Query().Include(m => m.Person).Get().Where(m => m.Person.IsActive == true).OrderBy(m => m.Person.Name);

            return JobWorker;
        }

        public JobWorker Add(JobWorker JobWorker)
        {
            _unitOfWork.Repository<JobWorker>().Insert(JobWorker);
            return JobWorker;
        }

        public void Dispose()
        {
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
                temp = (from b in db.JobWorker
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from b in db.JobWorker
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

                temp = (from b in db.JobWorker
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from b in db.JobWorker
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

        public JobWorkerViewModel GetJobWorkerViewModel(int id)
        {

            JobWorkerViewModel JobWorkerviewmodel = (from b in db.JobWorker
                                                     join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                                                     from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                                                     join p in db.Persons on BusinessEntityTab.PersonID equals p.PersonID into PersonTable
                                                     from PersonTab in PersonTable.DefaultIfEmpty()
                                                     join pa in db.PersonAddress on b.PersonID equals pa.PersonId into PersonAddressTable
                                                     from PersonAddressTab in PersonAddressTable.DefaultIfEmpty()
                                                     join ac in db.LedgerAccount on b.PersonID equals ac.PersonId into AccountTable
                                                     from AccountTab in AccountTable.DefaultIfEmpty()
                                                     where b.PersonID == id
                                                     select new JobWorkerViewModel
                                                     {
                                                         PersonId = b.PersonID,
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
                                                         PersonRateGroupId = BusinessEntityTab.PersonRateGroupId,
                                                         CreaditDays = BusinessEntityTab.CreaditDays,
                                                         CreaditLimit = BusinessEntityTab.CreaditLimit,
                                                         GuarantorId = BusinessEntityTab.GuarantorId,
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
                if (JobWorkerviewmodel.ProcessIds == "" || JobWorkerviewmodel.ProcessIds == null)
                {
                    JobWorkerviewmodel.ProcessIds = item.ProcessId.ToString();
                }
                else
                {
                    JobWorkerviewmodel.ProcessIds = JobWorkerviewmodel.ProcessIds + "," + item.ProcessId.ToString();
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
                        JobWorkerviewmodel.PersonRegistrationPanNoID = item.PersonRegistrationId;
                        JobWorkerviewmodel.PanNo = item.RregistrationNo;
                    }
                }
            }


            string Divisions = JobWorkerviewmodel.DivisionIds;
            if (Divisions != null)
            {
                Divisions = Divisions.Replace('|', ' ');
                JobWorkerviewmodel.DivisionIds = Divisions;
            }

            string Sites = JobWorkerviewmodel.SiteIds;
            if (Sites != null)
            {
                Sites = Sites.Replace('|', ' ');
                JobWorkerviewmodel.SiteIds = Sites;
            }

            return JobWorkerviewmodel;

        }

        public IQueryable<JobWorkerIndexViewModel> GetJobWorkerListForIndex()
        {
            var TempPan = from Pr in db.PersonRegistration
                          where Pr.RegistrationType == PersonRegistrationType.PANNo
                          select new
                          {
                              PersonId = Pr.PersonId,
                              PanNo = Pr.RegistrationNo
                          };

            var temp = from p in db.JobWorker
                       join p1 in db.Persons on p.PersonID equals p1.PersonID into PersonTable
                       from PersonTab in PersonTable.DefaultIfEmpty()
                       join Pan in TempPan on p.PersonID equals Pan.PersonId into PersonPanTable
                       from PersonPanTab in PersonPanTable.DefaultIfEmpty()
                       orderby PersonTab.Name
                       select new JobWorkerIndexViewModel
                       {
                           PersonId = PersonTab.PersonID,
                           Name = PersonTab.Name,
                           Code = PersonTab.Code,
                           Mobile = PersonTab.Mobile,
                           PanNo = PersonPanTab.PanNo
                       };
            return temp;
        }

        public int? GetJobWorkerForUser(string UserId)
        {
            var Emp = (from temp in db.JobWorker
                       join t in db.Persons on temp.PersonID equals t.PersonID
                       where t.ApplicationUser.Id == UserId
                       select temp.PersonID).FirstOrDefault();

            return Emp;
        }

       


    }



    public class JobWorkerDbService 
    {

        ApplicationDbContext db = new ApplicationDbContext();
        public JobWorkerDbService(ApplicationDbContext db)
        {
            this.db = db;
        }
        public int? GetJobWorkerForUser(string UserName)
        {
            var Emp = (from temp in db.Persons
                       join t in db.JobWorker on temp.PersonID equals t.PersonID
                       join t1 in db.UserPerson on temp.PersonID equals t1.PersonId
                       where t1.UserName==UserName
                       select t.PersonID).FirstOrDefault();

            return Emp;
        }

        public void Dispose()
        {

        }
    }

}
