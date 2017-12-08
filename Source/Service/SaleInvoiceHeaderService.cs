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
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;
using Model.ViewModel;
using System.Data.Entity.SqlServer;

namespace Service
{
    public interface ISaleInvoiceHeaderService : IDisposable
    {
        SaleInvoiceHeader Create(SaleInvoiceHeader s);
        void Delete(int id);
        void Delete(SaleInvoiceHeader s);
        SaleInvoiceHeaderDetail GetSaleInvoiceHeaderDetail(int id);

        SaleInvoiceHeaderIndexViewModel GetSaleInvoiceHeaderVM(int id);
        SaleInvoiceHeaderDetail Find(int id);
        IQueryable<SaleInvoiceHeaderIndexViewModel> GetSaleInvoiceHeaderList(int id, string Uname);
        IQueryable<SaleInvoiceHeaderIndexViewModel> GetSaleInvoiceHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<SaleInvoiceHeaderIndexViewModel> GetSaleInvoiceHeaderListPendingToReview(int id, string Uname);
        
        void Update(SaleInvoiceHeader s);
        string GetMaxDocNo();
        SaleInvoiceHeader FindByDocNo(string Docno);
        IEnumerable<SaleInvoicePrintViewModel> FGetPrintData(int Id);
        IEnumerable<SaleInvoicePrintViewModel> FGetPrintInvoiceData(int Id);
        IEnumerable<SaleInvoicePrintViewModel> FGetPrintInvoiceWithCollectionData(int Id);
        IEnumerable<MasterKeyPrintViewModel> FGetPrintMasterKeyData(int Id);
        IEnumerable<SaleInvoicePrintViewModel> FGetPrintPackingListData(int Id);
        IEnumerable<SaleInvoicePrintViewModel> FGetPrintPackingListWithCollectionData(int Id);
        IEnumerable<SaleInvoiceHeader> GetSaleInvoiceListForReport(int BuyerId);
        int NextId(int id);
        int PrevId(int id);
        SaleInvoiceHeader FindDirectSaleInvoice(int id);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
        IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentHeaderAttribute(int id);
    }
    public class SaleInvoiceHeaderService : ISaleInvoiceHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleInvoiceHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public SaleInvoiceHeader Create(SaleInvoiceHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleInvoiceHeader>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleInvoiceHeader>().Delete(id);
        }
        public void Delete(SaleInvoiceHeader s)
        {
            _unitOfWork.Repository<SaleInvoiceHeader>().Delete(s);
        }
        public void Update(SaleInvoiceHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleInvoiceHeader>().Update(s);
        }

        public SaleInvoiceHeaderDetail GetSaleInvoiceHeaderDetail(int id)
        {
            //return _unitOfWork.Repository<SaleInvoiceHeaderDetail>().Query().Get().Where(m => m.SaleInvoiceHeaderId == id).FirstOrDefault();
            return (from H in db.SaleInvoiceHeaderDetail where H.SaleInvoiceHeaderId == id select H).FirstOrDefault();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.SaleInvoiceHeader
                        join t in db.Persons on p.BillToBuyerId equals t.PersonID
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleInvoiceHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();


            }
            else
            {
                temp = (from p in db.SaleInvoiceHeader
                        join t in db.Persons on p.BillToBuyerId equals t.PersonID
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleInvoiceHeaderId).FirstOrDefault();
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

                temp = (from p in db.SaleInvoiceHeader
                        join t in db.Persons on p.BillToBuyerId equals t.PersonID
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleInvoiceHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleInvoiceHeader
                        join t in db.Persons on p.BillToBuyerId equals t.PersonID
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleInvoiceHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public SaleInvoiceHeaderIndexViewModel GetSaleInvoiceHeaderVM(int id)
        {
            SaleInvoiceHeaderIndexViewModel temp = (from p in db.SaleInvoiceHeader
                                                    join t2 in db.Persons on p.BillToBuyerId equals t2.PersonID into table2
                                                    from tab2 in table2.DefaultIfEmpty()
                                                    where p.SaleInvoiceHeaderId == id
                                                    select new SaleInvoiceHeaderIndexViewModel
                                                    {
                                                        BillToBuyerName = tab2.Name,
                                                        CreatedBy = p.CreatedBy,
                                                        CreatedDate = p.CreatedDate,
                                                        DivisionName = p.Division.DivisionName,
                                                        DocDate = p.DocDate,
                                                        DocTypeId=p.DocTypeId,
                                                        SiteId=p.SiteId,
                                                        DivisionId=p.DivisionId,
                                                        DocNo = p.DocNo,
                                                        ModifiedBy = p.ModifiedBy,
                                                        ModifiedDate = p.ModifiedDate,
                                                        Remark = p.Remark,
                                                        SaleInvoiceHeaderId = p.SaleInvoiceHeaderId,
                                                        SiteName = p.Site.SiteName,
                                                        Status = p.Status,
                                                        DocumentTypeName = p.DocType.DocumentTypeName,
                                                        CurrencyName = p.Currency.Name,
                                                        LockReason=p.LockReason,
                                                    }

                ).FirstOrDefault();

            return temp;
        }

        public SaleInvoiceHeaderDetail Find(int id)
        {
            return _unitOfWork.Repository<SaleInvoiceHeaderDetail>().Find(id);
        }

        public SaleInvoiceHeader FindDirectSaleInvoice(int id)
        {
            return _unitOfWork.Repository<SaleInvoiceHeader>().Find(id);
        }
        public IQueryable<SaleInvoiceHeaderIndexViewModel> GetSaleInvoiceHeaderList(int id, string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var TempSaleInvoiceHeaderCharges = from H in db.SaleInvoiceHeader
                                              join Hc in db.SaleInvoiceHeaderCharge on H.SaleInvoiceHeaderId equals Hc.HeaderTableId into SaleInvoiceHeaderChargesTable
                                              from SaleInvoiceHeaderChargesTab in SaleInvoiceHeaderChargesTable.DefaultIfEmpty()
                                              join C in db.Charge on SaleInvoiceHeaderChargesTab.ChargeId equals C.ChargeId into ChargeTable
                                              from ChargeTab in ChargeTable.DefaultIfEmpty()
                                              where H.SiteId == SiteId && H.DivisionId == DivisionId && H.DocTypeId == id && ChargeTab.ChargeName == "Net Amount"
                                              select new
                                              {
                                                  SaleInvoiceHeaderId = H.SaleInvoiceHeaderId,
                                                  NetAmount = SaleInvoiceHeaderChargesTab.Amount
                                              };


            var temp = from p in db.SaleInvoiceHeader
                       join t in db.Persons on p.BillToBuyerId equals t.PersonID
                       join Hc in TempSaleInvoiceHeaderCharges on p.SaleInvoiceHeaderId equals Hc.SaleInvoiceHeaderId into SaleInvoiceHeaderChargesTable
                       from SaleInvoiceHeaderChargesTab in SaleInvoiceHeaderChargesTable.DefaultIfEmpty()
                       orderby p.DocDate descending, p.DocNo descending
                       where p.DivisionId == DivisionId && p.SiteId == SiteId && p.DocTypeId == id
                       select new SaleInvoiceHeaderIndexViewModel
                       {
                           Remark = p.Remark,
                           DocDate = p.DocDate,
                           SaleInvoiceHeaderId = p.SaleInvoiceHeaderId,
                           DocNo = p.DocNo,
                           BillToBuyerName = t.Name + ", " + t.Suffix + " [" + t.Code + "]",
                           Status = p.Status,
                           ModifiedBy = p.ModifiedBy,
                           ReviewCount = p.ReviewCount,
                           ReviewBy = p.ReviewBy,                                              
                           Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                           GatePassDocNo = p.SaleDispatchHeader.GatePassHeader.DocNo,
                           GatePassHeaderId = p.SaleDispatchHeader.GatePassHeader.GatePassHeaderId,
                           GatePassDocDate = p.SaleDispatchHeader.GatePassHeader.DocDate,
                           GatePassStatus = (p.SaleDispatchHeader.GatePassHeader.Status != null ? p.SaleDispatchHeader.GatePassHeader.Status : 0),
                           TotalQty = p.SaleInvoiceLines.Sum(m => m.Qty),
                           TotalAmount = SaleInvoiceHeaderChargesTab.NetAmount ?? (p.SaleInvoiceLines.Sum(m => m.Amount)),
                           DecimalPlaces = (from o in p.SaleInvoiceLines
                                            join rl in db.SaleDispatchLine on o.SaleDispatchLineId equals rl.SaleDispatchLineId
                                            join ol in db.PackingLine on rl.PackingLineId equals ol.PackingLineId
                                            join prod in db.Product on ol.ProductId equals prod.ProductId
                                            join u in db.Units on prod.UnitId equals u.UnitId
                                            select u.DecimalPlaces).Max(),

                       };
            return temp;
        }

        public IQueryable<SaleInvoiceHeaderIndexViewModel> GetSaleInvoiceHeaderListPendingToSubmit(int id, string Uname)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleInvoiceHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;
        }

        public IQueryable<SaleInvoiceHeaderIndexViewModel> GetSaleInvoiceHeaderListPendingToReview(int id, string Uname)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleInvoiceHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;
        }

        public SaleInvoiceHeader FindByDocNo(string Docno)
        {
            return _unitOfWork.Repository<SaleInvoiceHeader>().Query().Get().Where(m => m.DocNo == Docno).FirstOrDefault();
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<SaleInvoiceHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public void Dispose()
        {
        }

        public IEnumerable<SaleInvoicePrintViewModel> FGetPrintData(int Id)
        {
            IEnumerable<SaleInvoicePrintViewModel> SaleInvoiceprintviewmodel = db.Database.SqlQuery<SaleInvoicePrintViewModel>("Web.ProcSaleInvoicePrint @Id", new SqlParameter("@Id", Id)).ToList();
            return SaleInvoiceprintviewmodel;
        }

        public IEnumerable<SaleInvoicePrintViewModel> FGetPrintInvoiceData(int Id)
        {
            IEnumerable<SaleInvoicePrintViewModel> SaleInvoiceprintviewmodel = db.Database.SqlQuery<SaleInvoicePrintViewModel>("Web.ProcSaleInvoicePrint_ForInvoice @Id", new SqlParameter("@Id", Id)).ToList();
            return SaleInvoiceprintviewmodel;
        }

        public IEnumerable<SaleInvoicePrintViewModel> FGetPrintInvoiceWithCollectionData(int Id)
        {
            IEnumerable<SaleInvoicePrintViewModel> SaleInvoiceprintviewmodel = db.Database.SqlQuery<SaleInvoicePrintViewModel>("Web.ProcSaleInvoicePrint_ForInvoice_WithCollection @Id", new SqlParameter("@Id", Id)).ToList();
            return SaleInvoiceprintviewmodel;
        }

        public IEnumerable<MasterKeyPrintViewModel> FGetPrintMasterKeyData(int Id)
        {
            IEnumerable<MasterKeyPrintViewModel> SaleInvoiceprintviewmodel = db.Database.SqlQuery<MasterKeyPrintViewModel>("Web.ProcSaleInvoicePrint_ForMasterKey @Id", new SqlParameter("@Id", Id)).ToList();
            return SaleInvoiceprintviewmodel;
        }

        public IEnumerable<SaleInvoicePrintViewModel> FGetPrintPackingListData(int Id)
        {
            IEnumerable<SaleInvoicePrintViewModel> SaleInvoiceprintviewmodel = db.Database.SqlQuery<SaleInvoicePrintViewModel>("Web.ProcSalePackingListPrint @Id", new SqlParameter("@Id", Id)).ToList();
            return SaleInvoiceprintviewmodel;
        }

        public IEnumerable<SaleInvoicePrintViewModel> FGetPrintPackingListWithCollectionData(int Id)
        {
            IEnumerable<SaleInvoicePrintViewModel> SaleInvoiceprintviewmodel = db.Database.SqlQuery<SaleInvoicePrintViewModel>("Web.ProcSalePackingListPrint @Id", new SqlParameter("@Id", Id)).ToList();
            return SaleInvoiceprintviewmodel;
        }

        public IEnumerable<SaleInvoiceHeader> GetSaleInvoiceListForReport(int BuyerId)
        {
            return _unitOfWork.Repository<SaleInvoiceHeader>().Query().Include(m => m.DocType).Get().Where(m => m.BillToBuyerId == BuyerId);
        }

        public IEnumerable<SaleInvoiceListViewModel> GetPendingInvoices(int id, int SaleInvoiceReturnHeaderId)
        {

            var SaleInvoiceReturnHeader = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(SaleInvoiceReturnHeaderId);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoiceReturnHeader.DocTypeId, SaleInvoiceReturnHeader.DivisionId, SaleInvoiceReturnHeader.SiteId);

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


            return (from p in db.ViewSaleInvoiceBalance
                    join t in db.SaleInvoiceHeader on p.SaleInvoiceHeaderId equals t.SaleInvoiceHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in db.SaleDispatchLine on p.SaleDispatchLineId equals t1.SaleDispatchLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join t2 in db.SaleInvoiceLine on p.SaleDispatchLineId equals t2.SaleDispatchLineId into InvoiceTable
                    from InvTab in InvoiceTable.DefaultIfEmpty()
                    where InvTab.ProductId == id && tab.SaleToBuyerId == SaleInvoiceReturnHeader.BuyerId && p.BalanceQty > 0
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    select new SaleInvoiceListViewModel
                    {
                        SaleInvoiceLineId = p.SaleInvoiceLineId,
                        SaleInvoiceHeaderId = p.SaleInvoiceHeaderId,
                        DocNo = tab.DocNo,
                        Dimension1Name = InvTab.Dimension1.Dimension1Name,
                        Dimension2Name = InvTab.Dimension2.Dimension2Name,
                    }
                        );
        }

        public IEnumerable<SaleInvoiceListViewModel> GetPendingInvoicesWithterm(int id, int SaleInvoiceReturnHeaderId, string term)
        {

            var SaleInvoiceReturnHeader = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(SaleInvoiceReturnHeaderId);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoiceReturnHeader.DocTypeId, SaleInvoiceReturnHeader.DivisionId, SaleInvoiceReturnHeader.SiteId);

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


            return (from p in db.ViewSaleInvoiceBalance
                    join t in db.SaleInvoiceHeader on p.SaleInvoiceHeaderId equals t.SaleInvoiceHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in db.SaleDispatchLine on p.SaleDispatchLineId equals t1.SaleDispatchLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join t2 in db.SaleInvoiceLine on p.SaleDispatchLineId equals t2.SaleDispatchLineId into InvoiceTable
                    from InvTab in InvoiceTable.DefaultIfEmpty()
                    where InvTab.ProductId == id && tab.SaleToBuyerId == SaleInvoiceReturnHeader.BuyerId && p.BalanceQty > 0
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleInvoiceNo.ToLower().Contains(term.ToLower()))
                    select new SaleInvoiceListViewModel
                    {
                        SaleInvoiceLineId = p.SaleInvoiceLineId,
                        SaleInvoiceHeaderId = p.SaleInvoiceHeaderId,
                        DocNo = tab.DocNo,
                        Dimension1Name = InvTab.Dimension1.Dimension1Name,
                        Dimension2Name = InvTab.Dimension2.Dimension2Name,
                    }
                        );
        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(DocTypeId, DivisionId, SiteId);

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


        public IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentHeaderAttribute(int id)
        {
            var Header = db.SaleInvoiceHeader.Find(id);

            var temp = from Dta in db.DocumentTypeHeaderAttribute
                       join Ha in db.SaleInvoiceHeaderAttributes on Dta.DocumentTypeHeaderAttributeId equals Ha.DocumentTypeHeaderAttributeId into HeaderAttributeTable
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

    }
}
