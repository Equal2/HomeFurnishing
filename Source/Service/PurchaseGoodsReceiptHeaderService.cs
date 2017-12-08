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
    public interface IPurchaseGoodsReceiptHeaderService : IDisposable
    {
        PurchaseGoodsReceiptHeader Create(PurchaseGoodsReceiptHeader pt);
        void Delete(int id);
        void Delete(PurchaseGoodsReceiptHeader pt);
        PurchaseGoodsReceiptHeader Find(int id);
        IEnumerable<PurchaseGoodsReceiptHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseGoodsReceiptHeader pt);
        PurchaseGoodsReceiptHeader Add(PurchaseGoodsReceiptHeader pt);
        IQueryable<PurchaseGoodsReceiptIndexViewModel> GetPurchaseGoodsReceiptHeaderList(int id,string Name);
        IQueryable<PurchaseGoodsReceiptIndexViewModel> GetPurchaseGoodsReceiptPendingToSubmit(int id, string Uname);
        IQueryable<PurchaseGoodsReceiptIndexViewModel> GetPurchaseGoodsReceiptPendingToReview(int id, string Uname);
        PurchaseGoodsReceiptHeaderViewModel GetPurchaseGoodsReceiptHeader(int id);

        // IEnumerable<PurchaseGoodsReceiptHeader> GetPurchaseGoodsReceiptHeaderList(int buyerId);
        Task<IEquatable<PurchaseGoodsReceiptHeader>> GetAsync();
        Task<PurchaseGoodsReceiptHeader> FindAsync(int id);
        PurchaseGoodsReceiptHeader GetPurchaseGoodsReceiptHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
        IEnumerable<PurchaseGoodsReceiptListViewModel> GetPendingReceipts(int id, int PurchaseGoodsReturnHeaderId);//ProductId
        IEnumerable<PurchaseGoodsReceiptListViewModel> GetPendingReceiptsForInvoice(int id, int PurchaseInvoiceHeaderId);

        IEnumerable<PurchaseGoodsReceiptLine> GetPurchaseGoodsReceiptLineList(int PurchaseGoodsReceiptHeaderId);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
        

    }

    public class PurchaseGoodsReceiptHeaderService : IPurchaseGoodsReceiptHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseGoodsReceiptHeader> _PurchaseGoodsReceiptHeaderRepository;
        RepositoryQuery<PurchaseGoodsReceiptHeader> PurchaseGoodsReceiptHeaderRepository;
        public PurchaseGoodsReceiptHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseGoodsReceiptHeaderRepository = new Repository<PurchaseGoodsReceiptHeader>(db);
            PurchaseGoodsReceiptHeaderRepository = new RepositoryQuery<PurchaseGoodsReceiptHeader>(_PurchaseGoodsReceiptHeaderRepository);
        }
        public PurchaseGoodsReceiptHeader GetPurchaseGoodsReceiptHeaderByName(string terms)
        {
            return (from p in db.PurchaseGoodsReceiptHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }


        public PurchaseGoodsReceiptHeader Find(int id)
        {
            return _unitOfWork.Repository<PurchaseGoodsReceiptHeader>().Find(id);
        }

        public PurchaseGoodsReceiptHeader Create(PurchaseGoodsReceiptHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseGoodsReceiptHeader>().Insert(pt);
            return pt;
        }
        public PurchaseGoodsReceiptHeaderViewModel GetPurchaseGoodsReceiptHeader(int id)
        {
            return (from p in db.PurchaseGoodsReceiptHeader
                    where p.PurchaseGoodsReceiptHeaderId == id
                    select new PurchaseGoodsReceiptHeaderViewModel
                    {
                        PurchaseGoodsReceiptHeaderId=p.PurchaseGoodsReceiptHeaderId,
                        DocTypeId = p.DocTypeId,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        SupplierId = p.SupplierId,     
                        GodownId=p.GodownId,
                        SupplierDocNo=p.SupplierDocNo,
                        PurchaseWaybillId=p.PurchaseWaybillId,
                        SupplierDocDate=p.SupplierDocDate,
                        RoadPermitFormId=p.RoadPermitFormId,
                        GateInId=p.GateInId,
                        SiteId=p.SiteId,
                        DivisionId=p.DivisionId,
                        Remark=p.Remark,
                        Status=p.Status,
                        UnitConversionForId=p.UnitConversionForId,
                        ModifiedBy=p.ModifiedBy,
                        CreatedDate=p.CreatedDate,
                        CreatedBy=p.CreatedBy,
                        LockReason=p.LockReason,
                    }
                        ).FirstOrDefault();
        }
        public IEnumerable<PurchaseGoodsReceiptListViewModel> GetPendingReceipts(int id, int PurchaseGoodsReturnHeaderId)
        {

            var PurchaseGoodsReturn = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(PurchaseGoodsReturnHeaderId);

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(PurchaseGoodsReturn.DocTypeId, PurchaseGoodsReturn.DivisionId, PurchaseGoodsReturn.SiteId);

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


            return (from p in db.ViewPurchaseGoodsReceiptBalance
                    join t in db.PurchaseGoodsReceiptHeader on p.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId into table                    
                    from tab in table.DefaultIfEmpty()
                    join t1 in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t1.PurchaseOrderLineId into table1 from tab1 in table1.DefaultIfEmpty()
                    where p.ProductId == id && tab.SupplierId == PurchaseGoodsReturn.SupplierId && p.BalanceQty > 0
                    && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    select new PurchaseGoodsReceiptListViewModel
                    {
                        PurchaseGoodsReceiptLineId = p.PurchaseGoodsReceiptLineId,
                        PurchaseGoodsReceiptHeaderId = p.PurchaseGoodsReceiptHeaderId,
                        DocNo = tab.DocNo,
                        Dimension1Name=tab1.Dimension1.Dimension1Name,
                        Dimension2Name=tab1.Dimension2.Dimension2Name,
                    }
                        );
        }

        public IEnumerable<PurchaseGoodsReceiptListViewModel> GetPendingReceiptsForInvoice(int id, int PurchaseInvoiceHeaderId)
        {


            var PurchaseInvoiceHeader = new PurchaseInvoiceHeaderService(_unitOfWork).Find(PurchaseInvoiceHeaderId);

            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(PurchaseInvoiceHeader.DocTypeId, PurchaseInvoiceHeader.DivisionId, PurchaseInvoiceHeader.SiteId);

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


            return (from p in db.ViewPurchaseGoodsReceiptBalance
                    join t2 in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t2.PurchaseGoodsReceiptLineId                    
                    join t in db.PurchaseGoodsReceiptHeader on p.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t1.PurchaseOrderLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.ProductId == id && tab.SupplierId == PurchaseInvoiceHeader.SupplierId && p.BalanceQty > 0
                     && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    select new PurchaseGoodsReceiptListViewModel
                    {
                        PurchaseGoodsReceiptLineId = p.PurchaseGoodsReceiptLineId,
                        PurchaseGoodsReceiptHeaderId = p.PurchaseGoodsReceiptHeaderId,
                        DocNo = tab.DocNo,
                        Dimension1Name = tab1.Dimension1.Dimension1Name,
                        Dimension2Name = tab1.Dimension2.Dimension2Name,
                        ProductUidName=t2.ProductUid.ProductUidName,
                    }
                        );
        }

        public IEnumerable<PurchaseIndentLineListViewModel> GetPendingIndentsForInvoice(int id, int PurchaseInvoiceHeaderId)
        {


            var PurchaseInvoiceHeader = new PurchaseInvoiceHeaderService(_unitOfWork).Find(PurchaseInvoiceHeaderId);

            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(PurchaseInvoiceHeader.DocTypeId, PurchaseInvoiceHeader.DivisionId, PurchaseInvoiceHeader.SiteId);

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


            return (from p in db.ViewPurchaseIndentBalance                    
                    join t1 in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t1.PurchaseIndentLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.ProductId == id && p.BalanceQty > 0
                     && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    select new PurchaseIndentLineListViewModel
                    {
                        PurchaseIndentLineId = p.PurchaseIndentLineId,
                        PurchaseIndentHeaderId = p.PurchaseIndentHeaderId,
                        DocNo = p.PurchaseIndentNo,
                        Dimension1Name = tab1.Dimension1.Dimension1Name,
                        Dimension2Name = tab1.Dimension2.Dimension2Name,
                    }
                        );
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseGoodsReceiptHeader>().Delete(id);
        }

        public void Delete(PurchaseGoodsReceiptHeader pt)
        {
            _unitOfWork.Repository<PurchaseGoodsReceiptHeader>().Delete(pt);
        }

        public void Update(PurchaseGoodsReceiptHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseGoodsReceiptHeader>().Update(pt);
        }

        public IEnumerable<PurchaseGoodsReceiptHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseGoodsReceiptHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<PurchaseGoodsReceiptIndexViewModel> GetPurchaseGoodsReceiptHeaderList(int id, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"]; 

            return (from p in db.PurchaseGoodsReceiptHeader
                    join t in db._Users on p.ModifiedBy equals t.UserName into table
                    from tab in table.DefaultIfEmpty()
                    orderby p.DocDate descending,p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id 
                    select new PurchaseGoodsReceiptIndexViewModel
                    {
                        PurchaseGoodsReceiptHeaderId=p.PurchaseGoodsReceiptHeaderId,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        DocTypeName = p.DocType.DocumentTypeName,                        
                        Remark = p.Remark, 
                        Status=p.Status,
                        SupplierName=p.Supplier.Name,
                        ModifiedBy = p.ModifiedBy,
                        FirstName = tab.FirstName,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    }
                );
        }



        public PurchaseGoodsReceiptHeader Add(PurchaseGoodsReceiptHeader pt)
        {
            _unitOfWork.Repository<PurchaseGoodsReceiptHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseGoodsReceiptHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseGoodsReceiptHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseGoodsReceiptHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseGoodsReceiptHeaderId).FirstOrDefault();
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

                temp = (from p in db.PurchaseGoodsReceiptHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseGoodsReceiptHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseGoodsReceiptHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseGoodsReceiptHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<PurchaseGoodsReceiptHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }


        public IQueryable<PurchaseGoodsReceiptIndexViewModel> GetPurchaseGoodsReceiptPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseGoodsReceiptHeader = GetPurchaseGoodsReceiptHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in PurchaseGoodsReceiptHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<PurchaseGoodsReceiptIndexViewModel> GetPurchaseGoodsReceiptPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseGoodsReceiptHeader = GetPurchaseGoodsReceiptHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in PurchaseGoodsReceiptHeader
                                   where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }

        public IEnumerable<PurchaseGoodsReceiptLine> GetPurchaseGoodsReceiptLineList(int PurchaseGoodsReceiptHeaderId)
        {
            IEnumerable<PurchaseGoodsReceiptLine> PurchaseGoodsReceiptLineList = (from L in db.PurchaseGoodsReceiptLine
                                                                                 where L.PurchaseGoodsReceiptHeaderId == PurchaseGoodsReceiptHeaderId
                                                                                 select L).ToList();
            return PurchaseGoodsReceiptLineList;
        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(DocTypeId, DivisionId, SiteId);

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


        public Task<IEquatable<PurchaseGoodsReceiptHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseGoodsReceiptHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
