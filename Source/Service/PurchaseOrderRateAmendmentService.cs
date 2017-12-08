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
using Model.ViewModels;
using System.Data.SqlClient;

namespace Service
{
    public interface IPurchaseOrderRateAmendmentLineService : IDisposable
    {
        PurchaseOrderRateAmendmentLine Create(PurchaseOrderRateAmendmentLine pt);
        void Delete(int id);
        void Delete(PurchaseOrderRateAmendmentLine pt);
        PurchaseOrderRateAmendmentLine Find(int id);
        void Update(PurchaseOrderRateAmendmentLine pt);
        IEnumerable<PurchaseOrderRateAmendmentLineViewModel> GetPurchaseOrderRateAmendmentLineForHeader(int id);//Header Id
        Task<IEquatable<PurchaseOrderRateAmendmentLine>> GetAsync();
        Task<PurchaseOrderRateAmendmentLine> FindAsync(int id);
        IEnumerable<PurchaseOrderRateAmendmentLineViewModel> GetPurchaseOrderLineForMultiSelect(PurchaseOrderAmendmentFilterViewModel svm);
        PurchaseOrderRateAmendmentLineViewModel GetPurchaseOrderRateAmendmentLine(int id);//Line Id       
        IQueryable<ComboBoxResult> GetPendingPurchaseOrdersForRateAmendment(int id, string term);
        IQueryable<ComboBoxResult> GetPendingProductsForRateAmndmt(int id, string term);
        IEnumerable<PurchaseOrderListViewModel> GetPendingPurchaseOrdersForRateAmndmt(int HeaderId, string term, int Limit);//ProductId
        int GetMaxSr(int id);
        IQueryable<ComboBoxResult> GetPendingPurchaseOrderHelpList(int Id, string term);
        bool ValidatePurchaseOrder(int lineid, int headerid);
    }

    public class PurchaseOrderRateAmendmentLineService : IPurchaseOrderRateAmendmentLineService
    {
        private ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;
        public PurchaseOrderRateAmendmentLineService(IUnitOfWorkForService unitOfWork)
        {
            this.db = new ApplicationDbContext(); ;
            _unitOfWork = unitOfWork;
        }


        public PurchaseOrderRateAmendmentLine Find(int id)
        {
            return db.PurchaseOrderRateAmendmentLine.Find(id);
        }

        public PurchaseOrderRateAmendmentLine Create(PurchaseOrderRateAmendmentLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            db.PurchaseOrderRateAmendmentLine.Add(pt);
            return pt;
        }

        public void CreateRange(List<PurchaseOrderRateAmendmentLine> pt)
        {
            db.PurchaseOrderRateAmendmentLine.AddRange(pt);
        }

        public void Delete(int id)
        {
            PurchaseOrderRateAmendmentLine Line = db.PurchaseOrderRateAmendmentLine.Find(id);
            db.PurchaseOrderRateAmendmentLine.Remove(Line);
        }

        public void Delete(PurchaseOrderRateAmendmentLine pt)
        {
            db.PurchaseOrderRateAmendmentLine.Remove(pt);
        }

        public void Update(PurchaseOrderRateAmendmentLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            db.PurchaseOrderRateAmendmentLine.Add(pt);
        }

        public IEnumerable<PurchaseOrderRateAmendmentLineViewModel> GetPurchaseOrderRateAmendmentLineForHeader(int id)
        {
            return (from p in db.PurchaseOrderRateAmendmentLine
                    join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    join dealunits in db.Units on tab.DealUnitId equals dealunits.UnitId
                    into dealunittable from dealunittab in dealunittable.DefaultIfEmpty()
                    join t5 in db.PurchaseOrderHeader on tab.PurchaseOrderHeaderId equals t5.PurchaseOrderHeaderId
                    join t1 in db.Dimension1 on tab.Dimension1Id equals t1.Dimension1Id into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join t2 in db.Dimension2 on tab.Dimension2Id equals t2.Dimension2Id into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t3 in db.Product on tab.ProductId equals t3.ProductId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join units in db.Units on tab3.UnitId equals units.UnitId
                    into unittable from unittab in unittable.DefaultIfEmpty()
                    join t4 in db.PurchaseOrderAmendmentHeader on p.PurchaseOrderAmendmentHeaderId equals t4.PurchaseOrderAmendmentHeaderId
                    join t6 in db.Persons on t4.SupplierId equals t6.PersonID into table6
                    from tab6 in table6.DefaultIfEmpty()
                    where p.PurchaseOrderAmendmentHeaderId == id
                    orderby p.Sr
                    select new PurchaseOrderRateAmendmentLineViewModel
                    {
                        Dimension1Name = tab1.Dimension1Name,
                        Dimension2Name = tab2.Dimension2Name,
                        LotNo = tab.LotNo,
                        ProductId = tab.ProductId,
                        ProductName = tab3.ProductName,
                        DocNo = t4.DocNo,
                        PurchaseOrderAmendmentHeaderId = p.PurchaseOrderAmendmentHeaderId,
                        PurchaseOrderRateAmendmentLineId = p.PurchaseOrderRateAmendmentLineId,
                        PurchaseOrderDocNo = t5.DocNo,
                        PurchaseOrderLineId = tab.PurchaseOrderLineId,
                        Qty = p.Qty,
                        DealQty = p.Qty * tab.UnitConversionMultiplier,
                        DealUnitName = dealunittab.UnitName,
                        Specification = tab.Specification,
                        SupplierName = tab6.Name,
                        unitDecimalPlaces = unittab.DecimalPlaces,
                        UnitId = unittab.UnitId,
                        UnitName = unittab.UnitName,
                        dealUnitDecimalPlaces = dealunittab.DecimalPlaces,
                        DealUnitId = dealunittab.UnitId,
                        Remark = p.Remark,
                        Rate = p.Rate,
                        AmendedRate = p.AmendedRate,
                        Amount = p.Amount,
                        PurchaseOrderRate = tab.Rate ?? 0,
                    }
                        );

        }

        public IEnumerable<PurchaseOrderRateAmendmentLineViewModel> GetPurchaseOrderLineForMultiSelect(PurchaseOrderAmendmentFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.PurchaseOrderId)) { SaleOrderIdArr = svm.PurchaseOrderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            string[] Dim1Id = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { Dim1Id = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { Dim1Id = new string[] { "NA" }; }

            string[] Dim2Id = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { Dim2Id = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { Dim2Id = new string[] { "NA" }; }

            var temp = (from p in db.ViewPurchaseOrderBalanceForInvoice
                        join t3 in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t3.PurchaseOrderLineId into table3
                        from tab3 in table3.DefaultIfEmpty()
                        join t2 in db.PurchaseOrderHeader on p.PurchaseOrderHeaderId equals t2.PurchaseOrderHeaderId
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join AL in db.PurchaseOrderRateAmendmentLine on p.PurchaseOrderLineId equals AL.PurchaseOrderLineId into ALTable
                        from AlTab in ALTable.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(svm.PurchaseOrderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && (string.IsNullOrEmpty(svm.Dimension1Id) ? 1 == 1 : Dim1Id.Contains(p.Dimension1Id.ToString()))
                        && (string.IsNullOrEmpty(svm.Dimension2Id) ? 1 == 1 : Dim2Id.Contains(p.Dimension2Id.ToString()))
                        && p.BalanceQty > 0 && ((svm.SupplierId.HasValue && svm.SupplierId.Value > 0) ? p.SupplierId == svm.SupplierId : 1 == 1)
                        && (svm.UpToDate.HasValue ? t2.DocDate <= svm.UpToDate : 1 == 1)
                        orderby t2.DocDate, t2.DocNo, tab3.Sr
                        select new PurchaseOrderRateAmendmentLineViewModel
                        {
                            Dimension1Name = tab3.Dimension1.Dimension1Name,
                            Dimension2Name = tab3.Dimension2.Dimension2Name,
                            UnitName = tab2.Unit.UnitName,
                            DealUnitName = tab3.DealUnit.UnitName,
                            DealQty = p.BalanceQty * tab3.UnitConversionMultiplier,
                            UnitConversionMultiplier = tab3.UnitConversionMultiplier,
                            PurchaseOrderRate = p.Rate,
                            AmendedRate = (svm.Rate == 0 ? p.Rate : svm.Rate),
                            Qty = p.BalanceQty,
                            PurchaseOrderDocNo = p.PurchaseOrderNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            PurchaseOrderAmendmentHeaderId = svm.PurchaseOrderAmendmentHeaderId,
                            PurchaseOrderLineId = p.PurchaseOrderLineId,
                            unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                            dealUnitDecimalPlaces = tab3.DealUnit.DecimalPlaces,
                            AAmended = (AlTab == null ? false : true)
                        }
                        );
            return temp;
        }
        public PurchaseOrderRateAmendmentLineViewModel GetPurchaseOrderRateAmendmentLine(int id)
        {
            var temp = (from p in db.PurchaseOrderRateAmendmentLine
                        join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                        from tab in table.DefaultIfEmpty()
                        join t5 in db.PurchaseOrderHeader on tab.PurchaseOrderHeaderId equals t5.PurchaseOrderHeaderId into table5
                        from tab5 in table5.DefaultIfEmpty()
                        join t1 in db.Dimension1 on tab.Dimension1Id equals t1.Dimension1Id into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join t2 in db.Dimension2 on tab.Dimension2Id equals t2.Dimension2Id into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join t3 in db.Product on tab.ProductId equals t3.ProductId into table3
                        from tab3 in table3.DefaultIfEmpty()
                        join t4 in db.PurchaseOrderAmendmentHeader on p.PurchaseOrderAmendmentHeaderId equals t4.PurchaseOrderAmendmentHeaderId into table4
                        from tab4 in table4.DefaultIfEmpty()
                        join t6 in db.Persons on tab4.SupplierId equals t6.PersonID into table6
                        from tab6 in table6.DefaultIfEmpty()
                        join t7 in db.ViewPurchaseOrderBalanceForInvoice on p.PurchaseOrderLineId equals t7.PurchaseOrderLineId into table7
                        from tab7 in table7.DefaultIfEmpty()
                        orderby p.PurchaseOrderRateAmendmentLineId
                        where p.PurchaseOrderRateAmendmentLineId == id
                        select new PurchaseOrderRateAmendmentLineViewModel
                        {
                            Dimension1Name = tab1.Dimension1Name,
                            Dimension2Name = tab2.Dimension2Name,
                            LotNo = tab.LotNo,
                            ProductId = tab.ProductId,
                            ProductName = tab3.ProductName,
                            DocNo = tab4.DocNo,
                            PurchaseOrderAmendmentHeaderId = p.PurchaseOrderAmendmentHeaderId,
                            PurchaseOrderRateAmendmentLineId = p.PurchaseOrderRateAmendmentLineId,
                            PurchaseOrderDocNo = tab5.DocNo,
                            PurchaseOrderLineId = tab.PurchaseOrderLineId,
                            Qty = p.Qty,
                            Specification = tab.Specification,
                            SupplierName = tab6.Name,
                            UnitId = tab3.UnitId,
                            UnitName = tab3.Unit.UnitName,
                            DealUnitName = tab.DealUnit.UnitName,
                            UnitConversionMultiplier = tab.UnitConversionMultiplier,
                            DealQty = (p.Qty * tab.UnitConversionMultiplier),
                            PurchaseOrderRate = tab.Rate ?? 0,
                            LockReason=p.LockReason,
                            AmendedRate = p.AmendedRate,
                            Rate = p.Rate,
                            Amount = p.Amount,
                            Remark = p.Remark,
                        }

                      ).FirstOrDefault();

            return temp;

        }

        public IQueryable<ComboBoxResult> GetPendingPurchaseOrdersForRateAmendment(int id, string term)//DocTypeId
        {

            var PurchaseOrderAmendmentHeader = new PurchaseOrderAmendmentHeaderService(db).Find(id);

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(PurchaseOrderAmendmentHeader.DocTypeId, PurchaseOrderAmendmentHeader.DivisionId, PurchaseOrderAmendmentHeader.SiteId);

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

            var list = (from p in db.ViewPurchaseOrderBalanceForInvoice
                        join t in db.PurchaseOrderHeader on p.PurchaseOrderHeaderId equals t.PurchaseOrderHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        && ( t.SupplierId == PurchaseOrderAmendmentHeader.SupplierId)
                        group new { p } by p.PurchaseOrderHeaderId into g
                        orderby g.Max(m => m.p.PurchaseOrderNo)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.PurchaseOrderNo),
                            id = g.Key.ToString(),
                        }
                        );
            return list;
        }

        public IQueryable<ComboBoxResult> GetPendingProductsForRateAmndmt(int id, string term)//DocTypeId
        {

            var PurchaseOrderAmendmentHeader = new PurchaseOrderAmendmentHeaderService(db).Find(id);

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(PurchaseOrderAmendmentHeader.DocTypeId, PurchaseOrderAmendmentHeader.DivisionId, PurchaseOrderAmendmentHeader.SiteId);

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

            var list = (from p in db.ViewPurchaseOrderBalanceForInvoice
                        join t in db.PurchaseOrderHeader on p.PurchaseOrderHeaderId equals t.PurchaseOrderHeaderId
                        join t2 in db.Product on p.ProductId equals t2.ProductId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : t2.ProductName.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                         && (t.SupplierId == PurchaseOrderAmendmentHeader.SupplierId )
                        group new { p, t2 } by p.ProductId into g
                        orderby g.Max(m => m.t2.ProductName)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t2.ProductName),
                            id = g.Key.ToString(),
                        }
                        );
            return list;
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.PurchaseOrderRateAmendmentLine
                       where p.PurchaseOrderAmendmentHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }


        public IEnumerable<PurchaseOrderListViewModel> GetPendingPurchaseOrdersForRateAmndmt(int HeaderId, string term, int Limit)//Product Id
        {

            var Header = new PurchaseOrderAmendmentHeaderService(db).Find(HeaderId);

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

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

            var tem = from p in db.ViewPurchaseOrderBalanceForInvoice
                      join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId
                      join t2 in db.Product on p.ProductId equals t2.ProductId
                      join t3 in db.PurchaseOrderHeader on p.PurchaseOrderHeaderId equals t3.PurchaseOrderHeaderId
                      where p.BalanceQty > 0 && (p.SupplierId == Header.SupplierId)
                      && (string.IsNullOrEmpty(term) ? 1 == 1 : (p.PurchaseOrderNo.ToLower().Contains(term.ToLower()) || t2.ProductName.ToLower().Contains(term.ToLower())
                      || t.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower()) || t.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower())))
                      && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t3.DocTypeId.ToString()))
                      && (string.IsNullOrEmpty(settings.filterContraSites) ? t3.SiteId == CurrentSiteId : contraSites.Contains(t3.SiteId.ToString()))
                      && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t3.DivisionId == CurrentDivisionId : contraDivisions.Contains(t3.DivisionId.ToString()))
                      orderby p.PurchaseOrderNo
                      select new PurchaseOrderListViewModel
                      {
                          DocNo = p.PurchaseOrderNo,
                          PurchaseOrderLineId = p.PurchaseOrderLineId,
                          Dimension1Name = t.Dimension1.Dimension1Name,
                          Dimension2Name = t.Dimension2.Dimension2Name,
                          ProductName = t2.ProductName,
                          BalanceQty = p.BalanceQty,

                      };

            return (tem.Take(Limit).ToList());
        }

        public IQueryable<ComboBoxResult> GetPendingPurchaseOrderHelpList(int Id, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewPurchaseOrderBalanceForInvoice
                        join t in db.PurchaseOrderHeader on p.PurchaseOrderHeaderId equals t.PurchaseOrderHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        group new { p, t } by p.PurchaseOrderHeaderId into g
                        orderby g.Max(m => m.p.OrderDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.PurchaseOrderNo) + " | " + g.Max(m => m.t.DocType.DocumentTypeShortName),
                            id = g.Key.ToString(),
                        }
                        );

            return list;
        }


        public IQueryable<ComboBoxResult> GetPendingSupplierHelpList(int Id, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewPurchaseOrderBalanceForInvoice
                        join t in db.PurchaseOrderHeader on p.PurchaseOrderHeaderId equals t.PurchaseOrderHeaderId
                        join t2 in db.Persons on t.SupplierId equals t2.PersonID
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : t2.Name.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        group new { p, t,t2 } by p.SupplierId into g
                        orderby g.Max(m => m.p.OrderDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t2.Name),
                            id = g.Key.ToString(),
                        }
                        );

            return list;
        }

        public IQueryable<ComboBoxResult> GetPendingProductHelpList(int Id, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewPurchaseOrderBalanceForInvoice
                        join t in db.PurchaseOrderHeader on p.PurchaseOrderHeaderId equals t.PurchaseOrderHeaderId
                        join prod in db.Product on p.ProductId equals prod.ProductId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : prod.ProductName.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        group new { p, t,prod } by p.ProductId into g
                        orderby g.Max(m => m.p.OrderDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.prod.ProductName),
                            id = g.Key.ToString(),
                        }
                        );

            return list;
        }

        public bool ValidatePurchaseOrder(int lineid, int headerid)
        {
            var temp = (from p in db.PurchaseOrderRateAmendmentLine
                        where p.PurchaseOrderLineId == lineid && p.PurchaseOrderAmendmentHeaderId == headerid
                        select p).FirstOrDefault();
            if (temp != null)
                return false;
            else
                return true;

        }


        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseOrderRateAmendmentLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrderRateAmendmentLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
