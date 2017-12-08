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
using Model.ViewModel;
using System.Data.Entity.SqlServer;

namespace Service
{
    public interface IJobOrderInspectionHeaderService : IDisposable
    {
        JobOrderInspectionHeader Create(JobOrderInspectionHeader pt,string UserName);
        void Delete(int id);
        void Delete(JobOrderInspectionHeader pt);
        JobOrderInspectionHeader Find(int id);
        void Update(JobOrderInspectionHeader pt,string UserName);
        IQueryable<JobOrderInspectionHeaderViewModel> GetJobOrderInspectionHeaderList(int DocTypeId, string Uname);//DocumentTypeId
        IQueryable<JobOrderInspectionHeaderViewModel> GetJobOrderInspectionHeaderListPendingToSubmit(int DocTypeId, string Uname);//DocumentTypeId
        IQueryable<JobOrderInspectionHeaderViewModel> GetJobOrderInspectionHeaderListPendingToReview(int DocTypeId, string Uname);//DocumentTypeId
        JobOrderInspectionHeaderViewModel GetJobOrderInspectionHeader(int id);
        Task<IEquatable<JobOrderInspectionHeader>> GetAsync();
        Task<JobOrderInspectionHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class JobOrderInspectionHeaderService : IJobOrderInspectionHeaderService
    {
        private ApplicationDbContext db;
        public JobOrderInspectionHeaderService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public JobOrderInspectionHeader Find(int id)
        {
            return db.JobOrderInspectionHeader.Find(id);
        }

        public JobOrderInspectionHeader Create(JobOrderInspectionHeader pt,string UserName)
        {
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Added;
            db.JobOrderInspectionHeader.Add(pt);
            return pt;
        }
        public JobOrderInspectionHeaderViewModel GetJobOrderInspectionHeader(int id)
        {
            return (from p in db.JobOrderInspectionHeader
                    where p.JobOrderInspectionHeaderId == id
                    select new JobOrderInspectionHeaderViewModel
                    {
                        DivisionId=p.DivisionId,
                        DivisionName=p.Division.DivisionName,
                        DocTypeName=p.DocType.DocumentTypeName,
                        InspectionByName=p.InspectionBy.Person.Name,
                        InspectionById=p.InspectionById,
                        JobWorkerId=p.JobWorkerId,
                        JobWorkerName=p.JobWorker.Person.Name,
                        ProcessId=p.ProcessId,
                        ProcessName=p.Process.ProcessName,
                        Status=p.Status,
                        SiteId=p.SiteId,
                        SiteName=p.Site.SiteName,
                        JobOrderInspectionHeaderId=p.JobOrderInspectionHeaderId,
                        DocTypeId = p.DocTypeId,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        Remark=p.Remark,
                        ModifiedBy=p.ModifiedBy,
                        CreatedDate=p.CreatedDate,
                        CreatedBy=p.CreatedBy,
                        LockReason=p.LockReason,
                    }
                        ).FirstOrDefault();
        }

        public void Delete(int id)
        {
            var header= db.JobOrderInspectionHeader.Find(id);
            header.ObjectState = Model.ObjectState.Deleted;

            db.JobOrderInspectionHeader.Remove(header);
        }

        public void Delete(JobOrderInspectionHeader pt)
        {
            db.JobOrderInspectionHeader.Remove(pt);
        }

        public void Update(JobOrderInspectionHeader pt,string UserName)
        {
            pt.ModifiedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Modified;
            db.JobOrderInspectionHeader.Add(pt);
        }

        public IQueryable<JobOrderInspectionHeaderViewModel> GetJobOrderInspectionHeaderList(int DocTypeId, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.JobOrderInspectionHeader
                    join J in db.Persons on p.JobWorkerId equals J.PersonID into JTable
                    from JTab in JTable.DefaultIfEmpty()
                    join E in db.Persons on p.InspectionById equals E.PersonID into ETable
                    from ETab in ETable.DefaultIfEmpty()
                    orderby p.DocDate descending,p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == DocTypeId
                    select new JobOrderInspectionHeaderViewModel
                    {
                        JobOrderInspectionHeaderId=p.JobOrderInspectionHeaderId,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        JobWorkerName= JTab.Name,
                        InspectionByName= ETab.Name,
                        DocTypeName = p.DocType.DocumentTypeName,
                        Remark = p.Remark,
                        Status=p.Status,
                        ModifiedBy=p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy=p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    }
                );
        }
     
        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobOrderInspectionHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionHeaderId).FirstOrDefault();
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

                temp = (from p in db.JobOrderInspectionHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobOrderInspectionHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
   
        public IQueryable<JobOrderInspectionHeaderViewModel> GetJobOrderInspectionHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobOrderInspectionHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }
        public IQueryable<JobOrderInspectionHeaderViewModel> GetJobOrderInspectionHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobOrderInspectionHeaderList(id, Uname).AsQueryable();

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

            var settings = db.JobOrderInspectionSettings.Where(m => m.DocTypeId == DocTypeId && m.DivisionId == DivisionId && m.SiteId == SiteId).FirstOrDefault();

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


        public Task<IEquatable<JobOrderInspectionHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderInspectionHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
