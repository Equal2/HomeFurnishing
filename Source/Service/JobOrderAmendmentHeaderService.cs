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
using Model.ViewModel;
using System.Data.Entity.SqlServer;
using Model.ViewModels;

namespace Service
{
    public interface IJobOrderAmendmentHeaderService : IDisposable
    {
        JobOrderAmendmentHeader Create(JobOrderAmendmentHeader pt);
        void Delete(int id);
        void Delete(JobOrderAmendmentHeader pt);
        JobOrderAmendmentHeader Find(int id);
        IEnumerable<JobOrderAmendmentHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobOrderAmendmentHeader pt);
        JobOrderAmendmentHeader Add(JobOrderAmendmentHeader pt);
        IQueryable<JobOrderAmendmentHeaderIndexViewModel> GetJobOrderAmendmentHeaderList(int id, string Uname);
        IQueryable<JobOrderAmendmentHeaderIndexViewModel> GetJobOrderAmendmentHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<JobOrderAmendmentHeaderIndexViewModel> GetJobOrderAmendmentHeaderListPendingToReview(int id, string Uname); 
        Task<IEquatable<JobOrderAmendmentHeader>> GetAsync();
        Task<JobOrderAmendmentHeader> FindAsync(int id);
        JobOrderAmendmentHeader GetJobOrderAmendmentHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class JobOrderAmendmentHeaderService : IJobOrderAmendmentHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public JobOrderAmendmentHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public JobOrderAmendmentHeader GetJobOrderAmendmentHeaderByName(string terms)
        {
            return (from p in db.JobOrderAmendmentHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }


        public JobOrderAmendmentHeader Find(int id)
        {
            return _unitOfWork.Repository<JobOrderAmendmentHeader>().Find(id);
        }

        public JobOrderAmendmentHeader Create(JobOrderAmendmentHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderAmendmentHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderAmendmentHeader>().Delete(id);
        }

        public void Delete(JobOrderAmendmentHeader pt)
        {
            _unitOfWork.Repository<JobOrderAmendmentHeader>().Delete(pt);
        }

        public void Update(JobOrderAmendmentHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderAmendmentHeader>().Update(pt);
        }

        public IEnumerable<JobOrderAmendmentHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobOrderAmendmentHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<JobOrderAmendmentHeaderIndexViewModel> GetJobOrderAmendmentHeaderList(int id,string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"]; 

            return (from p in db.JobOrderAmendmentHeader
                    orderby p.DocNo descending, p.DocDate descending
                    where p.SiteId == SiteId && p.DivisionId == p.DivisionId && p.DocTypeId == id
                    select new JobOrderAmendmentHeaderIndexViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        JobOrderAmendmentHeaderId = p.JobOrderAmendmentHeaderId,
                        ProcessName=p.Process.ProcessName,
                        OrderByName=p.OrderBy.Person.Name,
                        Remark = p.Remark,
                        Status=p.Status,
                        JobWorkerName = p.JobWorker.Person.Name,
                        ModifiedBy=p.ModifiedBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) == 1),
                        ReviewCount=p.ReviewCount,
                        ReviewBy=p.ReviewBy,
                    }
                        );
        }

        public JobOrderAmendmentHeader Add(JobOrderAmendmentHeader pt)
        {
            _unitOfWork.Repository<JobOrderAmendmentHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobOrderAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.JobOrderAmendmentHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.JobOrderAmendmentHeaderId).FirstOrDefault();
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

                temp = (from p in db.JobOrderAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.JobOrderAmendmentHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.JobOrderAmendmentHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<JobOrderAmendmentHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }


        public IQueryable<JobOrderAmendmentHeaderIndexViewModel> GetJobOrderAmendmentHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobOrderAmendmentHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<JobOrderAmendmentHeaderIndexViewModel> GetJobOrderAmendmentHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobOrderAmendmentHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in JobOrderHeader
                                   where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

            string[] PersonRoles = null;
            if (!string.IsNullOrEmpty(settings.filterPersonRoles)) { PersonRoles = settings.filterPersonRoles.Split(",".ToCharArray()); }
            else { PersonRoles = new string[] { "NA" }; }

            string DivIdStr = "|" + DivisionId.ToString() + "|";
            string SiteIdStr = "|" + SiteId.ToString() + "|";

            var list = (from p in db.Persons
                        join bus in db.BusinessEntity on p.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join pp in db.PersonProcess on p.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        join pr in db.PersonRole on p.PersonID equals pr.PersonId into PersonRoleTable
                        from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        where PersonProcessTab.ProcessId == settings.ProcessId
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (p.Name.ToLower().Contains(term.ToLower()) || p.Code.ToLower().Contains(term.ToLower())))
                        && (string.IsNullOrEmpty(settings.filterPersonRoles) ? 1 == 1 : PersonRoles.Contains(PersonRoleTab.RoleDocTypeId.ToString()))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivIdStr) != -1
                        && BusinessEntityTab.SiteIds.IndexOf(SiteIdStr) != -1
                        && (p.IsActive == null ? 1 == 1 : p.IsActive == true)
                        group new { p } by new { p.PersonID } into Result
                        orderby Result.Max(m => m.p.Name)
                        select new ComboBoxResult
                        {
                            id = Result.Key.PersonID.ToString(),
                            text = Result.Max(m => m.p.Name + ", " + m.p.Suffix + " [" + m.p.Code + "]"),
                        }
              );

            return list;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<JobOrderAmendmentHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderAmendmentHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
