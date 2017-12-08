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
using System.Data.SqlClient;
using System.Data.Entity.SqlServer;

namespace Service
{
    public interface IPurchaseOrderHeaderService : IDisposable
    {
        PurchaseOrderHeader Create(PurchaseOrderHeader pt);
        void Delete(int id);
        void Delete(PurchaseOrderHeader pt);
        PurchaseOrderHeader Find(int id);
        PurchaseOrderHeader Find(string DocNo);
        IEnumerable<PurchaseOrderHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseOrderHeader pt);
        PurchaseOrderHeader Add(PurchaseOrderHeader pt);
        IQueryable<PurchaseOrderIndexViewModel> GetPurchaseOrderHeaderList(int id, string Uname);
        IQueryable<PurchaseOrderIndexViewModel> GetPurchaseOrderPendingToSubmit(int id, string Uname);
        IQueryable<PurchaseOrderIndexViewModel> GetPurchaseOrderPendingToReview(int id, string Uname);
        PurchaseOrderHeaderViewModel GetPurchaseOrderHeader(int id);

        // IEnumerable<PurchaseOrderHeader> GetPurchaseOrderHeaderList(int buyerId);
        Task<IEquatable<PurchaseOrderHeader>> GetAsync();
        Task<PurchaseOrderHeader> FindAsync(int id);
        PurchaseOrderHeader GetPurchaseOrderHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
        IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseOrders(int ProductId, int PurchaseOrderCancelHeaderId);

        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class PurchaseOrderHeaderService : IPurchaseOrderHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseOrderHeader> _PurchaseOrderHeaderRepository;
        RepositoryQuery<PurchaseOrderHeader> PurchaseOrderHeaderRepository;
        public PurchaseOrderHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseOrderHeaderRepository = new Repository<PurchaseOrderHeader>(db);
            PurchaseOrderHeaderRepository = new RepositoryQuery<PurchaseOrderHeader>(_PurchaseOrderHeaderRepository);
        }
        public PurchaseOrderHeader GetPurchaseOrderHeaderByName(string terms)
        {
            return (from p in db.PurchaseOrderHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }
        public PurchaseOrderHeader Find(string DocNo)
        {
            return (from p in db.PurchaseOrderHeader
                    where p.DocNo == DocNo
                    select p
                        ).FirstOrDefault();
        }


        public PurchaseOrderHeader Find(int id)
        {
            return _unitOfWork.Repository<PurchaseOrderHeader>().Find(id);
        }

        public PurchaseOrderHeader Create(PurchaseOrderHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseOrderHeader>().Insert(pt);
            return pt;
        }
        public PurchaseOrderHeaderViewModel GetPurchaseOrderHeader(int id)
        {
            return (from p in db.PurchaseOrderHeader
                    where p.PurchaseOrderHeaderId == id
                    select new PurchaseOrderHeaderViewModel
                    {
                        PurchaseOrderHeaderId = p.PurchaseOrderHeaderId,
                        DocTypeId = p.DocTypeId,
                        DocDate = p.DocDate,
                        DueDate = p.DueDate,
                        SiteId = p.SiteId,
                        DivisionId = p.DivisionId,
                        DocNo = p.DocNo,
                        SupplierId = p.SupplierId,
                        CurrencyId = p.CurrencyId,
                        SalesTaxGroupPersonId = p.SalesTaxGroupPersonId,
                        TermsAndConditions = p.TermsAndConditions,
                        CreditDays = p.CreditDays,
                        Remark = p.Remark,
                        UnitConversionForId = p.UnitConversionForId,
                        ShipMethodId = p.ShipMethodId,
                        ShipAddress = p.ShipAddress,
                        DeliveryTermsId = p.DeliveryTermsId,
                        Status = p.Status,
                        ModifiedBy=p.ModifiedBy,
                        CreatedDate=p.CreatedDate,
                        CreatedBy=p.CreatedBy,
                        LockReason=p.LockReason,
                    }
                        ).FirstOrDefault();
        }
        public IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseOrders(int ProductId, int PurchaseGoodsReceiptHeaderId)//Product Id
        {

            var PurchaseReceipt = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(PurchaseGoodsReceiptHeaderId);

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(PurchaseReceipt.DocTypeId, PurchaseReceipt.DivisionId, PurchaseReceipt.SiteId);

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

            var tem = from p in db.ViewPurchaseOrderBalance
                      join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                      from tab in table.DefaultIfEmpty()
                      where p.ProductId == ProductId && p.BalanceQty > 0 && p.SupplierId == PurchaseReceipt.SupplierId
                      && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                           && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                      && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                      orderby p.PurchaseOrderNo
                      select new PurchaseOrderLineListViewModel
                      {
                          DocNo = p.PurchaseOrderNo,
                          PurchaseOrderLineId = p.PurchaseOrderLineId,
                          Dimension1Name = tab.Dimension1.Dimension1Name,
                          Dimension2Name = tab.Dimension2.Dimension2Name,
                      };

            return (tem);
        }


        public IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseOrdersForOrderCancel(int ProductId, int PurchaseOrderCancelHeaderId)//Product Id
        {

            var PurchaseOrderCancel = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(PurchaseOrderCancelHeaderId);

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(PurchaseOrderCancel.DocTypeId, PurchaseOrderCancel.DivisionId, PurchaseOrderCancel.SiteId);

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

            var tem = from p in db.ViewPurchaseOrderBalance
                      join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                      from tab in table.DefaultIfEmpty()
                      where p.ProductId == ProductId && p.BalanceQty > 0 && p.SupplierId == PurchaseOrderCancel.SupplierId
                      && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                      && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                      && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                      orderby p.PurchaseOrderNo
                      select new PurchaseOrderLineListViewModel
                      {
                          DocNo = p.PurchaseOrderNo,
                          PurchaseOrderLineId = p.PurchaseOrderLineId,
                          Dimension1Name = tab.Dimension1.Dimension1Name,
                          Dimension2Name = tab.Dimension2.Dimension2Name,
                      };

            return (tem);
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseOrderHeader>().Delete(id);
        }

        public void Delete(PurchaseOrderHeader pt)
        {
            _unitOfWork.Repository<PurchaseOrderHeader>().Delete(pt);
        }

        public void Update(PurchaseOrderHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseOrderHeader>().Update(pt);
        }

        public IEnumerable<PurchaseOrderHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseOrderHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<PurchaseOrderIndexViewModel> GetPurchaseOrderHeaderList(int id,string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"]; 


            return (from p in db.PurchaseOrderHeader
                    join t in db._Users on p.ModifiedBy equals t.UserName into table from tab in table.DefaultIfEmpty()
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id 
                    select new PurchaseOrderIndexViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        DocTypeName = p.DocType.DocumentTypeName,
                        PurchaseOrderHeaderId = p.PurchaseOrderHeaderId,
                        Remark = p.Remark,
                        SupplierName = p.Supplier.Name,
                        Status = p.Status, 
                        ModifiedBy=p.ModifiedBy,
                        FirstName=tab.FirstName,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    }
                );
        }

        public PurchaseOrderHeader Add(PurchaseOrderHeader pt)
        {
                
            _unitOfWork.Repository<PurchaseOrderHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseOrderHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseOrderHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseOrderHeaderId).FirstOrDefault();
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

                temp = (from p in db.PurchaseOrderHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseOrderHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseOrderHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<PurchaseOrderHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<PurchaseOrderIndexViewModel> GetPurchaseOrderPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseOrderHeader = GetPurchaseOrderHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in PurchaseOrderHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<PurchaseOrderIndexViewModel> GetPurchaseOrderPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseOrderHeader = GetPurchaseOrderHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in PurchaseOrderHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        
        }


        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

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


        public Task<IEquatable<PurchaseOrderHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrderHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }


    }
}
