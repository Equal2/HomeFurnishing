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
    public interface IJobInvoiceReturnHeaderService : IDisposable
    {
        JobInvoiceReturnHeader Create(JobInvoiceReturnHeader pt);
        void Delete(int id);
        void Delete(JobInvoiceReturnHeader pt);
        JobInvoiceReturnHeader Find(string Name);
        JobInvoiceReturnHeader Find(int id);
        void Update(JobInvoiceReturnHeader pt);
        JobInvoiceReturnHeaderViewModel GetJobInvoiceReturnHeader(int id);//HeadeRId
        IQueryable<JobInvoiceReturnHeaderViewModel> GetJobInvoiceReturnHeaderList(int id, string Uname);
        IQueryable<JobInvoiceReturnHeaderViewModel> GetJobInvoiceReturnPendingToSubmit(int id, string Uname);
        IQueryable<JobInvoiceReturnHeaderViewModel> GetJobInvoiceReturnPendingToReview(int id, string Uname);
        Task<IEquatable<JobInvoiceReturnHeader>> GetAsync();
        Task<JobInvoiceReturnHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term, int? ProcessId = null);
    }

    public class JobInvoiceReturnHeaderService : IJobInvoiceReturnHeaderService
    {
        private ApplicationDbContext db;
        public JobInvoiceReturnHeaderService(ApplicationDbContext db)
        {
            this.db = db;          
        }

        public JobInvoiceReturnHeader Find(string Name)
        {
            return db.JobInvoiceReturnHeader.Where(i => i.DocNo == Name).FirstOrDefault();
        }


        public JobInvoiceReturnHeader Find(int id)
        {
            return db.JobInvoiceReturnHeader.Find(id);
        }

        public JobInvoiceReturnHeader Create(JobInvoiceReturnHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            db.JobInvoiceReturnHeader.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            var temp = db.JobInvoiceReturnHeader.Find(id);
            temp.ObjectState = Model.ObjectState.Deleted;
            db.JobInvoiceReturnHeader.Remove(temp);
        }

        public void Delete(JobInvoiceReturnHeader pt)
        {
            db.JobInvoiceReturnHeader.Remove(pt);
        }

        public void Update(JobInvoiceReturnHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            db.JobInvoiceReturnHeader.Add(pt);
        }
        public JobInvoiceReturnHeaderViewModel GetJobInvoiceReturnHeader(int id)
        {
            return (from H in db.JobInvoiceReturnHeader
                    join Hr in db.JobReturnHeader on H.JobReturnHeaderId equals Hr.JobReturnHeaderId into JobReturnHeaderTable from JobReturnHeaderTab in JobReturnHeaderTable.DefaultIfEmpty()
                    where H.JobInvoiceReturnHeaderId == id
                    select new JobInvoiceReturnHeaderViewModel
                    {
                        JobInvoiceReturnHeaderId = H.JobInvoiceReturnHeaderId,
                        DivisionId = H.DivisionId,
                        DocNo = H.DocNo,
                        DocDate = H.DocDate,
                        DocTypeId = H.DocTypeId,
                        Remark = H.Remark,
                        SiteId = H.SiteId,
                        JobReturnHeaderId = H.JobReturnHeaderId,
                        Status = H.Status,
                        ProcessId=H.ProcessId,
                        JobWorkerId = H.JobWorkerId,
                        OrderById = JobReturnHeaderTab.OrderById,                        
                        GodownId = JobReturnHeaderTab.GodownId,
                        ReasonId = H.ReasonId,
                        ModifiedBy = H.ModifiedBy,
                        Nature = H.Nature,
                        SalesTaxGroupPersonId = H.SalesTaxGroupPersonId,
                        GatePassHeaderId = JobReturnHeaderTab.GatePassHeaderId,
                        GatePassDocNo = JobReturnHeaderTab.GatePassHeader.DocNo,
                        GatePassStatus = (JobReturnHeaderTab.GatePassHeader == null ? 0 : JobReturnHeaderTab.GatePassHeader.Status),
                        GatePassDocDate = JobReturnHeaderTab.GatePassHeader.DocDate,
                        CreatedDate = H.CreatedDate,
                        LockReason = H.LockReason,
                    }).FirstOrDefault();
        }
        public IQueryable<JobInvoiceReturnHeaderViewModel> GetJobInvoiceReturnHeaderList(int id, string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            var pt = (from p in db.JobInvoiceReturnHeader
                      join t3 in db._Users on p.ModifiedBy equals t3.UserName into table3
                      from tab3 in table3.DefaultIfEmpty()
                      join t in db.DocumentType on p.DocTypeId equals t.DocumentTypeId into table
                      from tab in table.DefaultIfEmpty()
                      join t1 in db.Persons on p.JobWorkerId equals t1.PersonID into table2
                      from tab2 in table2.DefaultIfEmpty()
                      orderby p.DocDate descending, p.DocNo descending
                      where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id
                      select new JobInvoiceReturnHeaderViewModel
                      {
                          DocDate = p.DocDate,
                          DocNo = p.DocNo,
                          DocTypeName = tab.DocumentTypeName,
                          JobInvoiceReturnHeaderId = p.JobInvoiceReturnHeaderId,
                          Remark = p.Remark,
                          Status = p.Status,
                          JobWorkerName = tab2.Name,
                          ModifiedBy = p.ModifiedBy,
                          FirstName = tab3.FirstName,
                          ReviewCount = p.ReviewCount,
                          ReviewBy = p.ReviewBy,
                          Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                          GatePassDocNo = p.JobReturnHeader.GatePassHeader.DocNo,
                          GatePassHeaderId = p.JobReturnHeader.GatePassHeaderId,
                          GatePassDocDate = p.JobReturnHeader.GatePassHeader.DocDate,
                          GatePassStatus = (p.JobReturnHeader.GatePassHeaderId != null ? p.JobReturnHeader.GatePassHeader.Status : 0),
                      }
                         );
            return pt;
        }        

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobInvoiceReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobInvoiceReturnHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobInvoiceReturnHeaderId).FirstOrDefault();
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

                temp = (from p in db.JobInvoiceReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobInvoiceReturnHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobInvoiceReturnHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IQueryable<JobInvoiceReturnHeaderViewModel> GetJobInvoiceReturnPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobInvoiceReturnHeader = GetJobInvoiceReturnHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in JobInvoiceReturnHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<JobInvoiceReturnHeaderViewModel> GetJobInvoiceReturnPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobInvoiceReturnHeader = GetJobInvoiceReturnHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in JobInvoiceReturnHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public IEnumerable<JobInvoiceListViewModel> GetPendingInvoices(int JobInvoiceReturnHeaderId, string term, int Limit)
        {

            var JobInvoiceReturnHeader = Find(JobInvoiceReturnHeaderId);

            var settings = db.JobInvoiceSettings
            .Where(m=>m.DocTypeId==JobInvoiceReturnHeader.DocTypeId && m.DivisionId==JobInvoiceReturnHeader.DivisionId && m.SiteId==JobInvoiceReturnHeader.SiteId).FirstOrDefault();

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            return (from p in db.ViewJobInvoiceBalance
                    join t in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals t.JobInvoiceHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in db.JobReceiveLine on p.JobReceiveLineId equals t1.JobReceiveLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join prod in db.Product on p.ProductId equals prod.ProductId
                    join jo in db.JobOrderLine on tab1.JobOrderLineId equals jo.JobOrderLineId
                    where tab.JobWorkerId == JobInvoiceReturnHeader.JobWorkerId && p.BalanceQty > 0
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : tab.DocNo.Contains(term))
                     && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    select new JobInvoiceListViewModel
                    {
                        ProductId=p.ProductId,
                        ProductName=prod.ProductName,
                        JobInvoiceLineId = p.JobInvoiceLineId,
                        JobInvoiceHeaderId = p.JobInvoiceHeaderId,
                        DocNo = tab.DocNo,
                        ReceiveDocNo = p.JobReceiveNo,
                        Dimension1Name = jo.Dimension1.Dimension1Name,
                        Dimension2Name = jo.Dimension2.Dimension2Name,
                        Dimension3Name = jo.Dimension3.Dimension3Name,
                        Dimension4Name = jo.Dimension4.Dimension4Name,
                        BalanceQty = p.BalanceQty,
                    }).Take(Limit);
        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term, int? ProcessId = null)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = db.JobInvoiceSettings.Where(m => m.DocTypeId == DocTypeId && m.DivisionId == DivisionId && m.SiteId == SiteId).FirstOrDefault();


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
                        where (ProcessId == null ? 1 == 1 : PersonProcessTab.ProcessId == ProcessId)
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


        public Task<IEquatable<JobInvoiceReturnHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobInvoiceReturnHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
