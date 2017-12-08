using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using System.Data.Entity;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModels;
using Model.ViewModel;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using Model.ViewModels;

namespace Service
{
    public interface IJobReceiveQAHeaderService : IDisposable
    {
        JobReceiveQAHeader Create(JobReceiveQAHeader pt, string UserName);
        void Delete(int id);
        void Delete(JobReceiveQAHeader pt);
        JobReceiveQAHeader Find(int id);
        void Update(JobReceiveQAHeader pt, string UserName);
        IQueryable<JobReceiveQAHeaderViewModel> GetJobReceiveQAHeaderList(int DocTypeId, string Uname);//DocumentTypeId
        IQueryable<JobReceiveQAHeaderViewModel> GetJobReceiveQAHeaderListPendingToSubmit(int DocTypeId, string Uname);//DocumentTypeId
        IQueryable<JobReceiveQAHeaderViewModel> GetJobReceiveQAHeaderListPendingToReview(int DocTypeId, string Uname);//DocumentTypeId
        JobReceiveQAHeaderViewModel GetJobReceiveQAHeader(int id);
        Task<IEquatable<JobReceiveQAHeader>> GetAsync();
        Task<JobReceiveQAHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<DocumentType> FindByDocumentCategory(int DocumentCategoryId);
        string FGetNewDocNo(string FieldName, string TableName, int DocTypeId, DateTime DocDate, int DivisionId, int SiteId);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);

    }

    public class JobReceiveQAHeaderService : IJobReceiveQAHeaderService
    {
        ApplicationDbContext db;
        public JobReceiveQAHeaderService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public JobReceiveQAHeader Find(int id)
        {
            return db.JobReceiveQAHeader.Find(id);
        }

        public JobReceiveQAHeader Create(JobReceiveQAHeader pt,string UserName)
        {
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Added;
            db.JobReceiveQAHeader.Add(pt);
            return pt;
        }
        public JobReceiveQAHeaderViewModel GetJobReceiveQAHeader(int id)
        {
            return (from p in db.JobReceiveQAHeader
                    where p.JobReceiveQAHeaderId == id
                    select new JobReceiveQAHeaderViewModel
                    {
                        DivisionId=p.DivisionId,
                        DivisionName=p.Division.DivisionName,
                        DocTypeName=p.DocType.DocumentTypeName,
                        QAByName = p.QABy.Person.Name,
                        QAById=p.QAById,
                        JobWorkerId=p.JobWorkerId,
                        JobWorkerName=p.JobWorker.Person.Name,
                        ProcessId=p.ProcessId,
                        ProcessName=p.Process.ProcessName,
                        Status=p.Status,
                        SiteId=p.SiteId,
                        SiteName=p.Site.SiteName,
                        JobReceiveQAHeaderId=p.JobReceiveQAHeaderId,
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
            JobReceiveQAHeader temp = db.JobReceiveQAHeader.Find(id);
            temp.ObjectState = Model.ObjectState.Deleted;

            db.JobReceiveQAHeader.Remove(temp);
        }

        public void Delete(JobReceiveQAHeader pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobReceiveQAHeader.Remove(pt);
        }

        public void Update(JobReceiveQAHeader pt,string UserName)
        {
            pt.ModifiedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Modified;
            db.JobReceiveQAHeader.Add(pt);
        }      

        public IQueryable<JobReceiveQAHeaderViewModel> GetJobReceiveQAHeaderList(int DocTypeId, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.JobReceiveQAHeader
                    orderby p.DocDate descending,p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == DocTypeId
                    select new JobReceiveQAHeaderViewModel
                    {
                        JobReceiveQAHeaderId=p.JobReceiveQAHeaderId,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        JobWorkerName=p.JobWorker.Person.Name,
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
                temp = (from p in db.JobReceiveQAHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobReceiveQAHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobReceiveQAHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobReceiveQAHeaderId).FirstOrDefault();
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

                temp = (from p in db.JobReceiveQAHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobReceiveQAHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobReceiveQAHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobReceiveQAHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }         
        public IQueryable<JobReceiveQAHeaderViewModel> GetJobReceiveQAHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobReceiveQAHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }
        public IQueryable<JobReceiveQAHeaderViewModel> GetJobReceiveQAHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobReceiveQAHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }

        public IEnumerable<DocumentType> FindByDocumentCategory(int DocumentCategoryId)
        {
            //return _unitOfWork.Repository<DocumentType>().Query().Get().Where(i => i.DocumentCategoryId  == DocumentCategoryId );
            //return db.DocumentType.Where(i => i.DocumentCategoryId == DocumentCategoryId);

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var DocTypes = (from p in db.DocumentType
                           join t in db.DocumentTypeDivision.Where(m => m.DivisionId == DivisionId) on p.DocumentTypeId equals t.DocumentTypeId into table
                           from tabdiv in table.DefaultIfEmpty()
                           join t2 in db.DocumentTypeSite.Where(m => m.SiteId == SiteId) on p.DocumentTypeId equals t2.DocumentTypeId into table2
                           from tabsit in table2.DefaultIfEmpty()
                           where p.DocumentCategoryId == DocumentCategoryId && tabdiv == null && tabsit == null
                           orderby p.DocumentTypeName
                           select p).Include(m=>m.DocumentCategory);            

            return (DocTypes);
        }

        public string FGetNewDocNo(string FieldName, string TableName, int DocTypeId, DateTime DocDate, int DivisionId, int SiteId)
        {
            SqlParameter SqlParameterFieldName = new SqlParameter("@FieldName", FieldName);
            SqlParameter SqlParameterTableName = new SqlParameter("@TableName", TableName);
            SqlParameter SqlParameterDocTypeId = new SqlParameter("@DocTypeId", DocTypeId);
            SqlParameter SqlParameterDocDate = new SqlParameter("@DocDate", DocDate);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);

            NewDocNoViewModel NewDocNoViewModel = db.Database.SqlQuery<NewDocNoViewModel>("" + System.Configuration.ConfigurationManager.AppSettings["DataBaseSchema"] + ".GetNewDocNo @FieldName , @TableName ,@DocTypeId , @DocDate , @DivisionId , @SiteId ", SqlParameterFieldName, SqlParameterTableName, SqlParameterDocTypeId, SqlParameterDocDate, SqlParameterDivisionId, SqlParameterSiteId).FirstOrDefault();

            if (NewDocNoViewModel != null)
            {
                return NewDocNoViewModel.NewDocNo;
            }
            else
            {
                return null;
            }
        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = db.JobReceiveQASettings.Where(m => m.DocTypeId == DocTypeId && m.DivisionId == DivisionId && m.SiteId == SiteId).FirstOrDefault();

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


        public Task<IEquatable<JobReceiveQAHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveQAHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
