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
    public interface IJobOrderInspectionRequestCancelHeaderService : IDisposable
    {
        JobOrderInspectionRequestCancelHeader Create(JobOrderInspectionRequestCancelHeader pt, string UserName);
        void Delete(int id);
        void Delete(JobOrderInspectionRequestCancelHeader pt);
        JobOrderInspectionRequestCancelHeader Find(int id);
        void Update(JobOrderInspectionRequestCancelHeader pt, string UserName);
        IQueryable<JobOrderInspectionRequestCancelHeaderViewModel> GetJobOrderInspectionRequestCancelHeaderList(int id, string Uname);
        IQueryable<JobOrderInspectionRequestCancelHeaderViewModel> GetJobOrderInspectionRequestCancelHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<JobOrderInspectionRequestCancelHeaderViewModel> GetJobOrderInspectionRequestCancelHeaderListPendingToReview(int id, string Uname);
        Task<IEquatable<JobOrderInspectionRequestCancelHeader>> GetAsync();
        Task<JobOrderInspectionRequestCancelHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class JobOrderInspectionRequestCancelHeaderService : IJobOrderInspectionRequestCancelHeaderService
    {
        private ApplicationDbContext db;

        public JobOrderInspectionRequestCancelHeaderService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public JobOrderInspectionRequestCancelHeader Find(int id)
        {
            return db.JobOrderInspectionRequestCancelHeader.Find(id);
        }

        public JobOrderInspectionRequestCancelHeader Create(JobOrderInspectionRequestCancelHeader pt, string UserName)
        {
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Added;
            db.JobOrderInspectionRequestCancelHeader.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            var header = db.JobOrderInspectionRequestCancelHeader.Find(id);
            header.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionRequestCancelHeader.Remove(header);
        }

        public void Delete(JobOrderInspectionRequestCancelHeader pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionRequestCancelHeader.Remove(pt);
        }

        public void Update(JobOrderInspectionRequestCancelHeader pt, string UserName)
        {
            pt.ModifiedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Modified;
            db.JobOrderInspectionRequestCancelHeader.Add(pt);
        }

        public IQueryable<JobOrderInspectionRequestCancelHeaderViewModel> GetJobOrderInspectionRequestCancelHeaderList(int id, string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in db.JobOrderInspectionRequestCancelHeader
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id
                    select new JobOrderInspectionRequestCancelHeaderViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        JobOrderInspectionRequestCancelHeaderId = p.JobOrderInspectionRequestCancelHeaderId,
                        Remark = p.Remark,
                        Status = p.Status,
                        JobWorkerName = p.JobWorker.Person.Name,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    }
                        );
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobOrderInspectionRequestCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionRequestCancelHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionRequestCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionRequestCancelHeaderId).FirstOrDefault();
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

                temp = (from p in db.JobOrderInspectionRequestCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionRequestCancelHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionRequestCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionRequestCancelHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public IQueryable<JobOrderInspectionRequestCancelHeaderViewModel> GetJobOrderInspectionRequestCancelHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobOrderInspectionRequestCancelHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<JobOrderInspectionRequestCancelHeaderViewModel> GetJobOrderInspectionRequestCancelHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobOrderInspectionRequestCancelHeaderList(id, Uname).AsQueryable();

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

            var settings = db.JobOrderInspectionRequestSettings.Where(m => m.DocTypeId == DocTypeId && m.DivisionId == DivisionId && m.SiteId == SiteId).FirstOrDefault();

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


        public Task<IEquatable<JobOrderInspectionRequestCancelHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderInspectionRequestCancelHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
