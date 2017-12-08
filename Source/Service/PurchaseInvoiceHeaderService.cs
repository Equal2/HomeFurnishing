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
    public interface IPurchaseInvoiceHeaderService : IDisposable
    {
        PurchaseInvoiceHeader Create(PurchaseInvoiceHeader pt);
        void Delete(int id);
        void Delete(PurchaseInvoiceHeader pt);
        PurchaseInvoiceHeader Find(string Name);
        PurchaseInvoiceHeader Find(int id);
        IEnumerable<PurchaseInvoiceHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseInvoiceHeader pt);
        PurchaseInvoiceHeader Add(PurchaseInvoiceHeader pt);
        PurchaseInvoiceHeaderViewModel GetPurchaseInvoiceHeader(int id);//HeadeRId
        IQueryable<PurchaseInvoiceHeaderViewModel> GetPurchaseInvoiceHeaderList(int id, string Uname);
        IQueryable<PurchaseInvoiceHeaderViewModel> GetPurchaseInvoicePendingToSubmit(int id, string Uname);
        IQueryable<PurchaseInvoiceHeaderViewModel> GetPurchaseInvoicePendingToReview(int id, string Uname);
        Task<IEquatable<PurchaseInvoiceHeader>> GetAsync();
        Task<PurchaseInvoiceHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class PurchaseInvoiceHeaderService : IPurchaseInvoiceHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseInvoiceHeader> _PurchaseInvoiceHeaderRepository;
        RepositoryQuery<PurchaseInvoiceHeader> PurchaseInvoiceHeaderRepository;
        public PurchaseInvoiceHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseInvoiceHeaderRepository = new Repository<PurchaseInvoiceHeader>(db);
            PurchaseInvoiceHeaderRepository = new RepositoryQuery<PurchaseInvoiceHeader>(_PurchaseInvoiceHeaderRepository);
        }

        public PurchaseInvoiceHeader Find(string Name)
        {
            return PurchaseInvoiceHeaderRepository.Get().Where(i => i.DocNo == Name).FirstOrDefault();
        }


        public PurchaseInvoiceHeader Find(int id)
        {
            return _unitOfWork.Repository<PurchaseInvoiceHeader>().Find(id);
        }

        public PurchaseInvoiceHeader Create(PurchaseInvoiceHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseInvoiceHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseInvoiceHeader>().Delete(id);
        }

        public void Delete(PurchaseInvoiceHeader pt)
        {
            _unitOfWork.Repository<PurchaseInvoiceHeader>().Delete(pt);
        }

        public void Update(PurchaseInvoiceHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseInvoiceHeader>().Update(pt);
        }

        public IEnumerable<PurchaseInvoiceHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseInvoiceHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public PurchaseInvoiceHeaderViewModel GetPurchaseInvoiceHeader(int id)
        {
            return (from p in db.PurchaseInvoiceHeader
                    where p.PurchaseInvoiceHeaderId == id
                    select new PurchaseInvoiceHeaderViewModel
                    {
                        PurchaseInvoiceHeaderId = p.PurchaseInvoiceHeaderId,
                        DivisionId = p.DivisionId,
                        DocNo = p.DocNo,
                        DocDate = p.DocDate,
                        PurchaseGoodsReceiptHeaderId = p.PurchaseGoodsReceiptHeaderId,
                        BillingAccountId = p.BillingAccountId,
                        DocTypeId = p.DocTypeId,
                        Remark = p.Remark,
                        SiteId = p.SiteId,
                        Status = p.Status,
                        UnitConversionForId = p.UnitConversionForId,
                        SupplierId = p.SupplierId,
                        TermsAndConditions = p.TermsAndConditions,
                        SalesTaxGroupId = p.SalesTaxGroupId,
                        CurrencyId = p.CurrencyId,
                        SupplierDocDate = p.SupplierDocDate,
                        SupplierDocNo = p.SupplierDocNo,
                        CreditDays = p.CreditDays,
                        ModifiedBy = p.ModifiedBy,
                        CreatedDate = p.CreatedDate,
                        CreatedBy = p.CreatedBy,
                        LockReason = p.LockReason,
                        ShipMethodId = p.ShipMethodId,
                        DeliveryTermsId = p.DeliveryTermsId,
                    }

                        ).FirstOrDefault();
        }
        public IQueryable<PurchaseInvoiceHeaderViewModel> GetPurchaseInvoiceHeaderList(int id, string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            var pt = (from p in db.PurchaseInvoiceHeader
                      join t in db.DocumentType on p.DocTypeId equals t.DocumentTypeId into table
                      from tab in table.DefaultIfEmpty()
                      join t1 in db.Persons on p.SupplierId equals t1.PersonID into table2
                      from tab2 in table2.DefaultIfEmpty()
                      join t in db._Users on p.ModifiedBy equals t.UserName into table3
                      from tab3 in table3.DefaultIfEmpty()
                      orderby p.DocDate descending, p.DocNo descending
                      where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id
                      select new PurchaseInvoiceHeaderViewModel
                      {
                          DocDate = p.DocDate,
                          DocNo = p.DocNo,
                          DocTypeName = tab.DocumentTypeName,
                          PurchaseInvoiceHeaderId = p.PurchaseInvoiceHeaderId,
                          SupplierDocNo = p.SupplierDocNo,
                          Remark = p.Remark,
                          Status = p.Status,
                          SupplierName = tab2.Name,
                          ModifiedBy = p.ModifiedBy,
                          FirstName = tab3.FirstName,
                          ReviewCount = p.ReviewCount,
                          ReviewBy = p.ReviewBy,
                          Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                      }
                         );
            return pt;
        }
        public IEnumerable<PurchaseInvoiceListViewModel> GetPendingInvoices(int id, int PurchaseInvoiceReturnHeaderId, string term, int Limit)
        {

            var PurchaseInvoiceReturnHeader = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(PurchaseInvoiceReturnHeaderId);

            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(PurchaseInvoiceReturnHeader.DocTypeId, PurchaseInvoiceReturnHeader.DivisionId, PurchaseInvoiceReturnHeader.SiteId);

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


            return (from p in db.ViewPurchaseInvoiceBalance
                    join t in db.PurchaseInvoiceHeader on p.PurchaseInvoiceHeaderId equals t.PurchaseInvoiceHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t1.PurchaseGoodsReceiptLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where tab1.ProductId == id && tab.SupplierId == PurchaseInvoiceReturnHeader.SupplierId && p.BalanceQty > 0
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : tab.DocNo.Contains(term))
                     && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    select new PurchaseInvoiceListViewModel
                    {
                        PurchaseInvoiceLineId = p.PurchaseInvoiceLineId,
                        PurchaseInvoiceHeaderId = p.PurchaseInvoiceHeaderId,
                        DocNo = tab.DocNo,
                        GoodsReceiptDocNo = p.PurchaseGoodsReceiptNo,
                        Dimension1Name = tab1.Dimension1.Dimension1Name,
                        Dimension2Name = tab1.Dimension2.Dimension2Name,
                        BalanceQty = p.BalanceQty,
                    }
                        ).Take(Limit);
        }
        public PurchaseInvoiceHeader Add(PurchaseInvoiceHeader pt)
        {
            _unitOfWork.Repository<PurchaseInvoiceHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseInvoiceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseInvoiceHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseInvoiceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseInvoiceHeaderId).FirstOrDefault();
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

                temp = (from p in db.PurchaseInvoiceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseInvoiceHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseInvoiceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseInvoiceHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<PurchaseInvoiceHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<PurchaseInvoiceHeaderViewModel> GetPurchaseInvoicePendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseInvoiceHeader = GetPurchaseInvoiceHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in PurchaseInvoiceHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<PurchaseInvoiceHeaderViewModel> GetPurchaseInvoicePendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseInvoiceHeader = GetPurchaseInvoiceHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in PurchaseInvoiceHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(DocTypeId, DivisionId, SiteId);

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


        public Task<IEquatable<PurchaseInvoiceHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseInvoiceHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
