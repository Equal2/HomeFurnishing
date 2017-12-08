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
using System.Data.SqlClient;
using System.Configuration;
using Model.ViewModels;

namespace Service
{
    public interface IRequisitionCancelHeaderService : IDisposable
    {
        RequisitionCancelHeader Create(RequisitionCancelHeader pt);
        void Delete(int id);
        void Delete(RequisitionCancelHeader pt);
        RequisitionCancelHeader Find(int id);
        void Update(RequisitionCancelHeader pt);
        RequisitionCancelHeader Add(RequisitionCancelHeader pt);
        IQueryable<RequisitionCancelHeaderViewModel> GetRequisitionCancelHeaderList(int id, string UName);
        IQueryable<RequisitionCancelHeaderViewModel> GetRequisitionCancelHeaderListPendingToSubmit(int id, string UName);
        IQueryable<RequisitionCancelHeaderViewModel> GetRequisitionCancelHeaderListPendingToReview(int id, string UName);
        Task<IEquatable<RequisitionCancelHeader>> GetAsync();
        Task<RequisitionCancelHeader> FindAsync(int id);
        RequisitionCancelHeader GetRequisitionCancelHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        bool CancelPendingWeavingRequisitions(int HeaderId, string UserName, int DocType);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class RequisitionCancelHeaderService : IRequisitionCancelHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public RequisitionCancelHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public RequisitionCancelHeader GetRequisitionCancelHeaderByName(string terms)
        {
            return (from p in db.RequisitionCancelHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }


        public RequisitionCancelHeader Find(int id)
        {
            return _unitOfWork.Repository<RequisitionCancelHeader>().Find(id);
        }

        public RequisitionCancelHeader Create(RequisitionCancelHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RequisitionCancelHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RequisitionCancelHeader>().Delete(id);
        }

        public void Delete(RequisitionCancelHeader pt)
        {
            _unitOfWork.Repository<RequisitionCancelHeader>().Delete(pt);
        }

        public void Update(RequisitionCancelHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RequisitionCancelHeader>().Update(pt);
        }

        public IQueryable<RequisitionCancelHeaderViewModel> GetRequisitionCancelHeaderList(int id, string UName)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.RequisitionCancelHeader
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id
                    select new RequisitionCancelHeaderViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        RequisitionCancelHeaderId = p.RequisitionCancelHeaderId,
                        ReasonName = p.Reason.ReasonName,
                        Remark = p.Remark,
                        Status = p.Status,
                        PersonName = p.Person.Name,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(UName, p.ReviewBy) > 0),
                    }
                        );
        }

        public IQueryable<RequisitionCancelHeaderViewModel> GetRequisitionCancelHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var RequisitionCancelHeader = GetRequisitionCancelHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in RequisitionCancelHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<RequisitionCancelHeaderViewModel> GetRequisitionCancelHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var RequisitionCancelHeader = GetRequisitionCancelHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in RequisitionCancelHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public RequisitionCancelHeader Add(RequisitionCancelHeader pt)
        {
            _unitOfWork.Repository<RequisitionCancelHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.RequisitionCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.RequisitionCancelHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.RequisitionCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.RequisitionCancelHeaderId).FirstOrDefault();
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

                temp = (from p in db.RequisitionCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.RequisitionCancelHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.RequisitionCancelHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.RequisitionCancelHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public IEnumerable<MaterialRequestBalanceSummaryViewModel> GetPendingRequisitionsForcancel(DateTime FromDate, DateTime ToDate)//DocTypeId
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", SiteId);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", DivisionId);
            SqlParameter SqlParameterBranchRecord = new SqlParameter("@BranchRecord", DBNull.Value);
            SqlParameter SqlParameterSampleRecord = new SqlParameter("@SampleRecord", DBNull.Value);
            SqlParameter SqlParameterJobWorker = new SqlParameter("@JobWorker", DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);

            IEnumerable<MaterialRequestBalanceSummaryViewModel> temp = db.Database.SqlQuery<MaterialRequestBalanceSummaryViewModel>("Web.spRep_WeavingMaterialRequestBalanceSummary @Site, @Division, @FromDate, @BranchRecord, @SampleRecord, @JobWorker, @ToDate", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterBranchRecord, SqlParameterSampleRecord, SqlParameterJobWorker,SqlParameterToDate).ToList();

            return temp;
        }

        public bool CancelPendingWeavingRequisitions(int HeaderId, string UserName, int DocType)
        {
            bool Success = false;
            var PendingRequisitionLines = (from p in db.ViewRequisitionBalance
                                           where p.RequisitionHeaderId == HeaderId && p.BalanceQty > 0
                                           select new { p.RequisitionLineId, p.BalanceQty, p.PersonId }).ToList();

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var ReasonId = (from p in db.RequisitionSetting
                            where p.DocTypeId == DocType && p.DivisionId == DivisionId && p.SiteId == SiteId
                            select p.DefaultReasonId).FirstOrDefault();

            if (!ReasonId.HasValue)
                ReasonId = new ReasonService(_unitOfWork).GetReasonList(TransactionDocCategoryConstants.RequisitionCancel).FirstOrDefault().ReasonId;

            if (PendingRequisitionLines.Count() > 0)
            {
                RequisitionCancelHeader Header = new RequisitionCancelHeader();
                Header.CreatedBy = UserName;
                Header.CreatedDate = DateTime.Now;
                Header.DivisionId = DivisionId;
                Header.DocDate = DateTime.Now;
                Header.DocTypeId = DocType;
                Header.SiteId = SiteId;
                Header.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".RequisitionCancelHeaders", Header.DocTypeId, Header.DocDate, Header.DivisionId, Header.SiteId);

                Header.ModifiedBy = UserName;
                Header.ModifiedDate = DateTime.Now;
                Header.PersonId = PendingRequisitionLines.FirstOrDefault().PersonId;
                Header.ReasonId = ReasonId.Value;

                Header.ObjectState = Model.ObjectState.Added;
                db.RequisitionCancelHeader.Add(Header);


                foreach (var item in PendingRequisitionLines)
                {
                    RequisitionCancelLine Line = new RequisitionCancelLine();
                    Line.CreatedBy = UserName;
                    Line.CreatedDate = DateTime.Now;
                    Line.ModifiedBy = UserName;
                    Line.ModifiedDate = DateTime.Now;
                    Line.Qty = item.BalanceQty;
                    Line.RequisitionLineId = item.RequisitionLineId;
                    Line.RequisitionCancelHeaderId = Header.RequisitionCancelHeaderId;
                    Line.ObjectState = Model.ObjectState.Added;
                    db.RequisitionCancelLine.Add(Line);

                    RequisitionLineStatus LineStatus = db.RequisitionLineStatus.Find(item.RequisitionLineId);

                    if (LineStatus != null)
                    {
                        LineStatus.CancelQty = (LineStatus.CancelQty ?? 0) + Line.Qty;
                        LineStatus.ObjectState = Model.ObjectState.Modified;

                        db.RequisitionLineStatus.Add(LineStatus);
                    }
                }

                try
                {
                    db.SaveChanges();

                }
                catch (Exception Ex)
                {
                    Success = false;
                }
                Success = true;
            }
            return Success;

        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            //var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(DocTypeId, DivisionId, SiteId);
            var settings = new MaterialRequestSettingsService(_unitOfWork).GetMaterialRequestSettingsForDocument(DocTypeId, DivisionId, SiteId);

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
                        where (settings.ProcessId == null ? 1 == 1 : PersonProcessTab.ProcessId == settings.ProcessId)
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
            db.Dispose();
        }


        public Task<IEquatable<RequisitionCancelHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RequisitionCancelHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
