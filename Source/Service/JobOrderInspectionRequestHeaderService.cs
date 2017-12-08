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
    public interface IJobOrderInspectionRequestHeaderService : IDisposable
    {
        JobOrderInspectionRequestHeader Create(JobOrderInspectionRequestHeader pt,string UserName);
        void Delete(int id);
        void Delete(JobOrderInspectionRequestHeader pt);
        JobOrderInspectionRequestHeader Find(int id);
        void Update(JobOrderInspectionRequestHeader pt,string UserName);
        IQueryable<JobOrderInspectionRequestHeaderViewModel> GetJobOrderInspectionRequestHeaderList(int id, string Uname);
        IQueryable<JobOrderInspectionRequestHeaderViewModel> GetJobOrderInspectionRequestHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<JobOrderInspectionRequestHeaderViewModel> GetJobOrderInspectionRequestHeaderListPendingToReview(int id, string Uname);
        Task<IEquatable<JobOrderInspectionRequestHeader>> GetAsync();
        Task<JobOrderInspectionRequestHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class JobOrderInspectionRequestHeaderService : IJobOrderInspectionRequestHeaderService
    {
        private ApplicationDbContext db;

        public JobOrderInspectionRequestHeaderService(ApplicationDbContext db)
        {
            this.db = db;

        }

        public JobOrderInspectionRequestHeader Find(int id)
        {
            return db.JobOrderInspectionRequestHeader.Find(id);
        }

        public JobOrderInspectionRequestHeader Create(JobOrderInspectionRequestHeader pt,string UserName)
        {
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Added;
            db.JobOrderInspectionRequestHeader.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            var header = db.JobOrderInspectionRequestHeader.Find(id);
            header.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionRequestHeader.Remove(header);
        }

        public void Delete(JobOrderInspectionRequestHeader pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionRequestHeader.Remove(pt);
        }

        public void Update(JobOrderInspectionRequestHeader pt,string UserName)
        {
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Modified;
            db.JobOrderInspectionRequestHeader.Add(pt);
        }        

        public IQueryable<JobOrderInspectionRequestHeaderViewModel> GetJobOrderInspectionRequestHeaderList(int id, string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in db.JobOrderInspectionRequestHeader
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id
                    select new JobOrderInspectionRequestHeaderViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        JobOrderInspectionRequestHeaderId = p.JobOrderInspectionRequestHeaderId,
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
                temp = (from p in db.JobOrderInspectionRequestHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionRequestHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionRequestHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionRequestHeaderId).FirstOrDefault();
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

                temp = (from p in db.JobOrderInspectionRequestHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionRequestHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionRequestHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionRequestHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IQueryable<JobOrderInspectionRequestHeaderViewModel> GetJobOrderInspectionRequestHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobOrderInspectionRequestHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<JobOrderInspectionRequestHeaderViewModel> GetJobOrderInspectionRequestHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobOrderInspectionRequestHeaderList(id, Uname).AsQueryable();

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


        public Task<IEquatable<JobOrderInspectionRequestHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderInspectionRequestHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
