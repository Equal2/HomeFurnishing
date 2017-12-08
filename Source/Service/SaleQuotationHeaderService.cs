using Data.Infrastructure;
using Data.Models;
using Model;
using Model.Models;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.ViewModels;
using Model.ViewModel;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.Data.Linq;
using Model.DatabaseViews;
using Model.ViewModels;
using AutoMapper;

namespace Service
{
    public interface ISaleQuotationHeaderService : IDisposable
    {
        SaleQuotationHeader Create(SaleQuotationHeader s);
        void Delete(int id);
        void Delete(SaleQuotationHeader s);
        SaleQuotationHeaderViewModel GetSaleQuotationHeader(int id);
        SaleQuotationHeader Find(int id);
        IQueryable<SaleQuotationHeaderViewModel> GetSaleQuotationHeaderListByCostCenter(int CostCenterId);
        IQueryable<SaleQuotationHeaderViewModel> GetSaleQuotationHeaderList(int DocumentTypeId, string Uname);
        IQueryable<SaleQuotationHeaderViewModel> GetSaleQuotationHeaderListPendingToSubmit(int DocumentTypeId, string Uname);
        IQueryable<SaleQuotationHeaderViewModel> GetSaleQuotationHeaderListPendingToReview(int DocumentTypeId, string Uname);
        void Update(SaleQuotationHeader s);
        string GetMaxDocNo();
        string FGetSaleQuotationCostCenter(int DocTypeId, DateTime DocDate, int DivisionId, int SiteId);
        string ValidateCostCenter(int DocTypeId, int HeaderId, int JobWorkerId, string CostCenterName);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
        IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentHeaderAttribute(int id);
    }
    public class SaleQuotationHeaderService : ISaleQuotationHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleQuotationHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public SaleQuotationHeader Create(SaleQuotationHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleQuotationHeader>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleQuotationHeader>().Delete(id);
        }
        public void Delete(SaleQuotationHeader s)
        {
            _unitOfWork.Repository<SaleQuotationHeader>().Delete(s);
        }
        public void Update(SaleQuotationHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleQuotationHeader>().Update(s);
        }


        public SaleQuotationHeader Find(int id)
        {
            return _unitOfWork.Repository<SaleQuotationHeader>().Find(id);
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<SaleQuotationHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<SaleQuotationHeaderViewModel> GetSaleQuotationHeaderListByCostCenter(int CostCenterId)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in db.SaleQuotationHeader
                    orderby p.DocDate descending, p.DocNo descending
                    where p.CostCenterId == CostCenterId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select new SaleQuotationHeaderViewModel
                    {
                        DueDate = p.DueDate,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        Remark = p.Remark,
                        Status = p.Status,
                        SaleQuotationHeaderId = p.SaleQuotationHeaderId,
                        ModifiedBy = p.ModifiedBy,
                    });
        }

        public IQueryable<SaleQuotationHeaderViewModel> GetSaleQuotationHeaderList(int DocumentTypeId, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];      


            
            return (from p in db.SaleQuotationHeader
                    join t in db.Persons on p.SaleToBuyerId equals t.PersonID
                    join dt in db.DocumentType on p.DocTypeId equals dt.DocumentTypeId
                    orderby p.DocDate descending, p.DocNo descending
                    where p.DocTypeId == DocumentTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select new SaleQuotationHeaderViewModel
                    {
                        DocTypeName = dt.DocumentTypeName,
                        DueDate = p.DueDate,
                        SaleToBuyerName = t.Name + "," + t.Suffix,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        CostCenterName = p.CostCenter.CostCenterName,
                        Remark = p.Remark,
                        Status = p.Status,
                        SaleQuotationHeaderId = p.SaleQuotationHeaderId,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    });
        }

        public SaleQuotationHeaderViewModel GetSaleQuotationHeader(int id)
        {
            return (from H in db.SaleQuotationHeader
                    join Hd in db.SaleQuotationHeaderDetail on H.SaleQuotationHeaderId equals Hd.SaleQuotationHeaderId into SaleQuotationHeaderTable from SaleQuotationHeaderTab in SaleQuotationHeaderTable.DefaultIfEmpty()
                    where H.SaleQuotationHeaderId == id
                    select new SaleQuotationHeaderViewModel
                    {
                        DocTypeName = H.DocType.DocumentTypeName,
                        DocDate = H.DocDate,
                        DocNo = H.DocNo,
                        Remark = H.Remark,
                        SaleQuotationHeaderId = H.SaleQuotationHeaderId,
                        Status = H.Status,
                        DocTypeId = H.DocTypeId,
                        DueDate = H.DueDate,
                        ExpiryDate = H.ExpiryDate,
                        ProcessId = H.ProcessId,
                        SaleToBuyerId = H.SaleToBuyerId,
                        UnitConversionForId = H.UnitConversionForId,
                        CostCenterId = H.CostCenterId,
                        TermsAndConditions = H.TermsAndConditions,
                        CreditDays = SaleQuotationHeaderTab.CreditDays,
                        DivisionId = H.DivisionId,
                        SiteId = H.SiteId,
                        LockReason = H.LockReason,
                        CostCenterName = H.CostCenter.CostCenterName,
                        ModifiedBy = H.ModifiedBy,
                        CreatedDate = H.CreatedDate,
                        DeliveryTermsId = SaleQuotationHeaderTab.DeliveryTermsId,
                        CurrencyId = H.CurrencyId,
                        SalesTaxGroupPersonId = H.SalesTaxGroupPersonId,
                        ShipMethodId = SaleQuotationHeaderTab.ShipMethodId,
                        AgentId = SaleQuotationHeaderTab.AgentId,
                        FinancierId = SaleQuotationHeaderTab.FinancierId,
                        SalesExecutiveId = SaleQuotationHeaderTab.SalesExecutiveId,
                        IsDoorDelivery = SaleQuotationHeaderTab.IsDoorDelivery ?? false,
                        PayTermAdvancePer = SaleQuotationHeaderTab.PayTermAdvancePer,
                        PayTermOnDeliveryPer = SaleQuotationHeaderTab.PayTermOnDeliveryPer,
                        PayTermOnDueDatePer = SaleQuotationHeaderTab.PayTermOnDueDatePer,
                        PayTermCashPer = SaleQuotationHeaderTab.PayTermCashPer,
                        PayTermBankPer = SaleQuotationHeaderTab.PayTermBankPer,
                        PayTermDescription = SaleQuotationHeaderTab.PayTermDescription,
                    }).FirstOrDefault();
        }



        public IQueryable<SaleQuotationHeaderViewModel> GetSaleQuotationHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var SaleQuotationHeader = GetSaleQuotationHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in SaleQuotationHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }


        public IQueryable<SaleQuotationHeaderViewModel> GetSaleQuotationHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var SaleQuotationHeader = GetSaleQuotationHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in SaleQuotationHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public string FGetSaleQuotationCostCenter(int DocTypeId, DateTime DocDate, int DivisionId, int SiteId)
        {
            SqlParameter SqlParameterDocTypeId = new SqlParameter("@DocTypeId", DocTypeId);
            SqlParameter SqlParameterDocDate = new SqlParameter("@DocDate", DocDate);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);

            NewDocNoViewModel NewDocNoViewModel = db.Database.SqlQuery<NewDocNoViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".GetSaleQuotationCostCenter @DocTypeId , @DocDate , @DivisionId , @SiteId ", SqlParameterDocTypeId, SqlParameterDocDate, SqlParameterDivisionId, SqlParameterSiteId).FirstOrDefault();

            if (NewDocNoViewModel != null)
            {
                return NewDocNoViewModel.NewDocNo;
            }
            else
            {
                return null;
            }
        }

        public string ValidateCostCenter(int DocTypeId, int HeaderId, int JobWorkerId, string CostCenterName)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(DocTypeId, DivisionId, SiteId);
            var LedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(JobWorkerId).LedgerAccountId;

            string ValidationMsg = "";

            if (Settings.IsPersonWiseCostCenter == true)
            {
                var CostCenter = (db.CostCenter.AsNoTracking().Where(m => m.CostCenterName == CostCenterName
                    && m.ReferenceDocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId).FirstOrDefault());
                if (CostCenter != null)
                    if (CostCenter.LedgerAccountId != LedgerAccountId)
                        ValidationMsg += "CostCenter belongs to a different person. ";
            }

            if (Settings.isUniqueCostCenter == true)
            {
                var CostCenter = db.CostCenter.AsNoTracking().Where(m => m.CostCenterName == CostCenterName
                    && m.ReferenceDocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId).FirstOrDefault();
                if (CostCenter != null)
                {
                    var UniqueCostCenter = (from p in db.SaleQuotationHeader
                                            where p.CostCenterId == CostCenter.CostCenterId && p.SaleQuotationHeaderId != HeaderId && p.DocTypeId == DocTypeId
                                            && p.SiteId == SiteId && p.DivisionId == DivisionId
                                            select p
                                         ).FirstOrDefault();
                    if (UniqueCostCenter != null)
                        ValidationMsg += "CostCenter Already exists";
                }
            }

            return ValidationMsg;

        }

        public DateTime AddDueDate(DateTime Base, int DueDays)
        {
            DateTime DueDate = Base.AddDays((double)DueDays);
            if (DueDate.DayOfWeek == DayOfWeek.Sunday)
                DueDate = DueDate.AddDays(1);

            return DueDate;
        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(DocTypeId, DivisionId, SiteId);

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
                        join pr in db.PersonRole on p.PersonID equals pr.PersonId into PersonRoleTable from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
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

        public IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentHeaderAttribute(int id)
        {
            var Header = db.SaleQuotationHeader.Find(id);

            var temp = from Dta in db.DocumentTypeHeaderAttribute
                       join Ha in db.SaleQuotationHeaderAttributes on Dta.DocumentTypeHeaderAttributeId equals Ha.DocumentTypeHeaderAttributeId into HeaderAttributeTable
                       from HeaderAttributeTab in HeaderAttributeTable.Where(m => m.HeaderTableId == id).DefaultIfEmpty()
                       where (Dta.DocumentTypeId == Header.DocTypeId)
                       select new DocumentTypeHeaderAttributeViewModel
                       {
                           ListItem = Dta.ListItem,
                           DataType = Dta.DataType,
                           Value = HeaderAttributeTab.Value,
                           Name = Dta.Name,
                           DocumentTypeHeaderAttributeId = Dta.DocumentTypeHeaderAttributeId,
                       };

            return temp;
        }

        public void Dispose()
        {
        }
    }
}
