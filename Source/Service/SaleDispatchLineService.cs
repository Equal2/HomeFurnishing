using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModels;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModel;
using System.Data.SqlClient;
using System.Configuration;


namespace Service
{
    public interface ISaleDispatchLineService : IDisposable
    {
        SaleDispatchLine Create(SaleDispatchLine s);
        void Delete(int id);
        void Delete(SaleDispatchLine s);
        SaleDispatchLine GetSaleDispatchLine(int id);
        IQueryable<SaleDispatchLine> GetSaleDispatchLineList(int SaleDispatchHeaderId);
        SaleDispatchLine Find(int id);
        void Update(SaleDispatchLine s);

        bool CheckForProductExists(int ProductId, int SaleDispatchHEaderId, int SaleDispatchLineId);
        bool CheckForProductExists(int ProductId, int SaleDispatchHEaderId);

        IEnumerable<SaleDispatchLineViewModel> GetSaleDispatchLineListForIndex(int SaleDispatchHeaderId);

        IEnumerable<SaleDispatchLineViewModel> GetSaleOrdersForFilters(SaleDispatchFilterViewModel vm);

        SaleDispatchLineViewModel GetSaleDispatchLineForEdit(int id);

        

        IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term);

        IQueryable<ComboBoxResult> GetSaleOrderHelpListForProduct(int PersonId, string term);
        SaleDispatchLineViewModel GetSaleDispatchDetailBalance(int id);

        SaleDispatchLineViewModel GetSaleDispatchDetailForInvoice(int id);

        

        IQueryable<ComboBoxResult> GetPendingProductsForSaleDispatch(int id, string term);

        IEnumerable<ComboBoxResult> GetPendingOrdersForDispatch(int id, string term);

        IEnumerable<ComboBoxResult> GetPendingStockInForDispatch(int id, int ProductId, int GodownId, int? Dimension1Id, int? Dimension2Id, string term);

        


    }

    public class SaleDispatchLineService : ISaleDispatchLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleDispatchLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public SaleDispatchLine Create(SaleDispatchLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDispatchLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleDispatchLine>().Delete(id);
        }

        public void Delete(SaleDispatchLine s)
        {
            _unitOfWork.Repository<SaleDispatchLine>().Delete(s);
        }

        public void Update(SaleDispatchLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDispatchLine>().Update(s);
        }

        public SaleDispatchLine GetSaleDispatchLine(int id)
        {
            return _unitOfWork.Repository<SaleDispatchLine>().Query().Get().Where(m => m.SaleDispatchLineId == id).FirstOrDefault();
        }



        public SaleDispatchLine Find(int id)
        {
            return _unitOfWork.Repository<SaleDispatchLine>().Find(id);
        }

        public IQueryable<SaleDispatchLine> GetSaleDispatchLineList(int SaleDispatchHeaderId)
        {
            return _unitOfWork.Repository<SaleDispatchLine>().Query().Get().Where(m => m.SaleDispatchHeaderId == SaleDispatchHeaderId);
        }

        public bool CheckForProductExists(int ProductId, int SaleDispatchHeaderId, int SaleDispatchLineId)
        {

            SaleDispatchLine temp = (from p in db.SaleDispatchLine
                                  where p.SaleDispatchHeaderId == SaleDispatchHeaderId &&p.SaleDispatchLineId!=SaleDispatchLineId
                                  select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }

        public bool CheckForProductExists(int ProductId, int SaleDispatchHeaderId)
        {

            SaleDispatchLine temp = (from p in db.SaleDispatchLine
                                  where p.SaleDispatchHeaderId == SaleDispatchHeaderId
                                  select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }

        public IEnumerable<SaleDispatchLineViewModel> GetSaleDispatchLineListForIndex(int SaleDispatchHeaderId)
        {
            IEnumerable<SaleDispatchLineViewModel> SaleDispatchLineViewModel = (from l in db.SaleDispatchLine
                                                                                join Pl in db.PackingLine on l.PackingLineId equals Pl.PackingLineId into PackingLineTable
                                                                                from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                                                                              join t in db.Product on PackingLineTab.ProductId equals t.ProductId into table
                                                                              from tab in table.DefaultIfEmpty()
                                                                              join t1 in db.SaleOrderLine on PackingLineTab.SaleOrderLineId equals t1.SaleOrderLineId into table1
                                                                              from tab1 in table1.DefaultIfEmpty()
                                                                              join t2 in db.SaleOrderHeader on tab1.SaleOrderHeaderId equals t2.SaleOrderHeaderId into table2
                                                                              from tab2 in table2.DefaultIfEmpty()
                                                                              join t3 in db.ProductUid on PackingLineTab.ProductUidId equals t3.ProductUIDId into table3
                                                                              from tab3 in table3.DefaultIfEmpty()
                                                                              join u in db.Units on PackingLineTab.DealUnitId equals u.UnitId into DealUnitTable
                                                                              from DealUnitTab in DealUnitTable.DefaultIfEmpty()
                                                                              join Si in db.Stock on l.StockId equals Si.StockId into StockInTable
                                                                              from StockInTab in StockInTable.DefaultIfEmpty()
                                                                              join Sih in db.StockHeader on StockInTab.StockHeaderId equals Sih.StockHeaderId into StockHeaderTable
                                                                              from StockHeaderTab in StockHeaderTable.DefaultIfEmpty()
                                                                                where l.SaleDispatchHeaderId == SaleDispatchHeaderId
                                                                              orderby l.SaleDispatchLineId
                                                                                select new SaleDispatchLineViewModel
                                                                              {
                                                                                  SaleDispatchLineId = l.SaleDispatchLineId,
                                                                                  ProductName = tab.ProductName,
                                                                                  Dimension1Name = PackingLineTab.Dimension1.Dimension1Name,
                                                                                  Dimension2Name = PackingLineTab.Dimension2.Dimension2Name,
                                                                                  Specification = PackingLineTab.Specification,
                                                                                  SaleOrderHeaderDocNo = tab2.DocNo,
                                                                                  ProductUidName = tab3.ProductUidName,
                                                                                  BaleNo = PackingLineTab.BaleNo,
                                                                                  Qty = PackingLineTab.Qty,
                                                                                  UnitId = tab.UnitId,
                                                                                  DealQty = PackingLineTab.DealQty,
                                                                                  DealUnitId = DealUnitTab.UnitName,
                                                                                  DealUnitDecimalPlaces = DealUnitTab.DecimalPlaces,
                                                                                  unitDecimalPlaces = PackingLineTab.Product.Unit.DecimalPlaces,
                                                                                  StockInId = StockInTab.StockId,
                                                                                  StockInNo = StockHeaderTab.DocNo,
                                                                                  Remark = l.Remark,
                                                                                  LossQty=PackingLineTab.LossQty,
                                                                                  SaleDispatchHeaderId=l.SaleDispatchHeaderId
                                                                              }).Take(2000).ToList();

            return SaleDispatchLineViewModel;
        }

        public IEnumerable<SaleDispatchLineViewModel> GetSaleOrdersForFilters(SaleDispatchFilterViewModel vm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] Dimension1IdArr = null;
            if (!string.IsNullOrEmpty(vm.Dimension1Id)) { Dimension1IdArr = vm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dimension1IdArr = new string[] { "NA" }; }

            string[] Dimension2IdArr = null;
            if (!string.IsNullOrEmpty(vm.Dimension2Id)) { Dimension2IdArr = vm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dimension2IdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.SaleOrderHeaderId)) { SaleOrderIdArr = vm.SaleOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewSaleOrderBalance
                        join t in db.SaleOrderHeader on p.SaleOrderHeaderId equals t.SaleOrderHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join t1 in db.SaleOrderLine on p.SaleOrderLineId equals t1.SaleOrderLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.SaleOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1IdArr.Contains(p.Dimension1Id.ToString()))
                        && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2IdArr.Contains(p.Dimension2Id.ToString()))
                        && p.BalanceQty > 0
                        orderby p.SaleOrderLineId
                        select new SaleDispatchLineViewModel
                        {
                            //ProductUidIdName = tab1.ProductUid != null ? tab1.ProductUid.ProductUidName : "",
                            Dimension1Name = tab1.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.Dimension2.Dimension2Name,
                            Specification = tab1.Specification,
                            BalanceQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            SaleOrderHeaderDocNo = tab.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            Dimension1Id = p.Dimension1Id,
                            Dimension2Id = p.Dimension2Id,
                            SaleDispatchHeaderId = vm.SaleDispatchHeaderId,
                            SaleOrderLineId = p.SaleOrderLineId,
                            UnitId = tab2.UnitId,
                            UnitName = tab2.Unit.UnitName,
                            DealUnitId = tab1.DealUnitId,
                            DealUnitName = tab1.DealUnit.UnitName,
                            unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                            DealUnitDecimalPlaces = tab1.DealUnit.DecimalPlaces,
                            DealQty = (!tab1.UnitConversionMultiplier.HasValue || tab1.UnitConversionMultiplier <= 0) ? p.BalanceQty : p.BalanceQty * tab1.UnitConversionMultiplier.Value,
                            UnitConversionMultiplier = tab1.UnitConversionMultiplier,
                        }

                        );
            return temp;
        }


        public SaleDispatchLineViewModel GetSaleDispatchLineForEdit(int id)
        {

            return (from p in db.SaleDispatchLine
                    join t2 in db.PackingLine on p.PackingLineId equals t2.PackingLineId into table2
                    from Pl in table2.DefaultIfEmpty()
                    join t3 in db.ViewSaleOrderBalance on Pl.SaleOrderLineId equals t3.SaleOrderLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join Si in db.Stock on p.StockInId equals Si.StockId into StockInTable
                    from StockInTab in StockInTable.DefaultIfEmpty()
                    join Sih in db.StockHeader on StockInTab.StockHeaderId equals Sih.StockHeaderId into StockHeaderTable
                    from StockHeaderTab in StockHeaderTable.DefaultIfEmpty()
                    where p.SaleDispatchLineId == id
                    select new SaleDispatchLineViewModel
                    {
                        ProductUidId = Pl.ProductUidId,
                        ProductUidName = Pl.ProductUid.ProductUidName,
                        ProductCode = Pl.Product.ProductCode,
                        ProductId = Pl.ProductId,
                        ProductName = Pl.Product.ProductName,
                        SaleOrderHeaderDocNo = Pl.SaleOrderLine.SaleOrderHeader.DocNo,
                        LossQty = Pl.LossQty,
                        FreeQty = Pl.FreeQty,
                        PassQty = Pl.PassQty,
                        Qty = Pl.Qty,
                        BalanceQty = (tab3 == null ? (decimal)Pl.PassQty : tab3.BalanceQty + (decimal)Pl.PassQty),
                        BaleNo = Pl.BaleNo,
                        UnitId = Pl.Product.UnitId,
                        UnitName = Pl.Product.Unit.UnitName,
                        DealUnitId = Pl.DealUnitId,
                        DealUnitName = Pl.DealUnit.UnitName,
                        DealQty = Pl.DealQty,
                        Remark = Pl.Remark,
                        Specification = Pl.Specification,
                        Dimension1Id = Pl.Dimension1Id,
                        Dimension2Id = Pl.Dimension2Id,
                        LotNo = Pl.LotNo,
                        GodownId = p.GodownId,
                        UnitConversionMultiplier = Pl.UnitConversionMultiplier,
                        SaleDispatchHeaderId = p.SaleDispatchHeaderId,
                        SaleDispatchLineId = p.SaleDispatchLineId,
                        PackingLineId = Pl.PackingLineId,
                        SaleOrderLineId = Pl.SaleOrderLineId,
                        Weight = Pl.NetWeight,
                        StockInId = p.StockInId,
                        StockInNo = StockHeaderTab.DocNo
                    }
                        ).FirstOrDefault();

        }

        public IEnumerable<ComboBoxResult> GetPendingOrdersForDispatch(int id, string term)
        {


            var SaleDispatchHeader = new SaleDispatchHeaderService(_unitOfWork).Find(id);

            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(SaleDispatchHeader.DocTypeId, SaleDispatchHeader.DivisionId, SaleDispatchHeader.SiteId);

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


            return (from p in db.ViewSaleOrderBalance
                    join t in db.SaleOrderHeader on p.SaleOrderHeaderId equals t.SaleOrderHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    where p.BalanceQty > 0
                    && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(tab.DocTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? tab.SiteId == CurrentSiteId : contraSites.Contains(tab.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? tab.DivisionId == CurrentDivisionId : contraDivisions.Contains(tab.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleOrderNo.ToLower().Contains(term.ToLower()))
                    group p by p.SaleOrderHeaderId into g
                    select new ComboBoxResult
                    {
                        id = g.Key.ToString(),
                        text = g.Max(m => m.SaleOrderNo),
                    }
                        );
        }

        public IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term)
        {

            var SaleDispatch = new SaleDispatchHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(SaleDispatch.DocTypeId, SaleDispatch.DivisionId, SaleDispatch.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] Products = null;
            if (!string.IsNullOrEmpty(settings.filterProducts)) { Products = settings.filterProducts.Split(",".ToCharArray()); }
            else { Products = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }

            return (from p in db.Product
                    where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProducts) ? 1 == 1 : Products.Contains(p.ProductId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(p.ProductGroupId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
                    orderby p.ProductName
                    select new ComboBoxResult
                    {
                        id = p.ProductId.ToString(),
                        text = p.ProductName,
                    });
        }

        public IQueryable<ComboBoxResult> GetSaleOrderHelpListForProduct(int filter, string term)
        {
            var SaleDispatchHeader = new SaleDispatchHeaderService(_unitOfWork).Find(filter);

            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(SaleDispatchHeader.DocTypeId, SaleDispatchHeader.DivisionId, SaleDispatchHeader.SiteId);

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var list = (from p in db.ViewSaleOrderBalance
                        join t in db.SaleOrderHeader on p.SaleOrderHeaderId equals t.SaleOrderHeaderId
                        join t2 in db.SaleOrderLine on p.SaleOrderLineId equals t2.SaleOrderLineId
                        join pt in db.Product on p.ProductId equals pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where p.BuyerId == SaleDispatchHeader.SaleToBuyerId
                        && ((string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleOrderNo.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : ProductTab.ProductName.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension1Tab.Dimension1Name.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension2Tab.Dimension2Name.ToLower().Contains(term.ToLower())))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        orderby t.DocDate, t.DocNo
                        select new ComboBoxResult
                        {
                            text = ProductTab.ProductName,
                            id = p.SaleOrderLineId.ToString(),
                            TextProp1 = "Order No: " + p.SaleOrderNo.ToString(),
                            TextProp2 = "BalQty: " + p.BalanceQty.ToString(),
                            AProp1 = Dimension1Tab.Dimension1Name,
                            AProp2 = Dimension2Tab.Dimension2Name
                        });

            return list;
        }

        public SaleDispatchLineViewModel GetSaleDispatchDetailBalance(int id)
        {
            return (from b in db.ViewSaleDispatchBalance
                    join p in db.SaleDispatchLine on b.SaleDispatchLineId equals p.SaleDispatchLineId
                    join pack in db.PackingLine on p.PackingLineId equals pack.PackingLineId
                    join t in db.SaleOrderLine on pack.SaleOrderLineId equals t.SaleOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    where b.SaleDispatchLineId == id

                    select new SaleDispatchLineViewModel
                    {
                        Qty = b.BalanceQty,
                        Remark = p.Remark,
                        Dimension1Id = pack.Dimension1Id,
                        Dimension1Name = pack.Dimension1.Dimension1Name,
                        Dimension2Id = pack.Dimension2Id,
                        Dimension2Name = pack.Dimension2.Dimension2Name,
                        Specification = pack.Specification,
                        LotNo = pack.LotNo,
                        UnitConversionMultiplier = tab.UnitConversionMultiplier ?? 0,
                        UnitId = pack.Product.UnitId,
                        UnitName = pack.Product.Unit.UnitName,
                        DealUnitId = pack.DealUnitId,
                        DealUnitName = pack.DealUnit.UnitName,
                        DealQty = b.BalanceQty * (tab.UnitConversionMultiplier ?? 0),
                    }).FirstOrDefault();

        }


        public SaleDispatchLineViewModel GetSaleDispatchDetailForInvoice(int id)
        {

            return (from t in db.ViewSaleDispatchBalance
                    join p in db.SaleDispatchLine on t.SaleDispatchLineId equals p.SaleDispatchLineId into SaleDispatchLineTable from SaleDispatchLineTab in SaleDispatchLineTable.DefaultIfEmpty()
                    join pl in db.PackingLine on SaleDispatchLineTab.PackingLineId equals pl.PackingLineId into PackingLineTable from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                    join sol in db.SaleOrderLine on t.SaleOrderLineId equals sol.SaleOrderLineId into SaleOrderLineTable from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
                    where t.SaleDispatchLineId == id
                    select new SaleDispatchLineViewModel
                    {
                        DealQty = PackingLineTab.DealQty,
                        DealUnitId = PackingLineTab.DealUnitId,
                        ProductId = PackingLineTab.ProductId,
                        Qty = t.BalanceQty,
                        Rate = SaleOrderLineTab.Rate,
                        Remark = PackingLineTab.Remark,
                        Dimension1Id = PackingLineTab.Dimension1Id,
                        Dimension2Id = PackingLineTab.Dimension2Id,
                        Dimension1Name = PackingLineTab.Dimension1.Dimension1Name,
                        Dimension2Name = PackingLineTab.Dimension2.Dimension2Name,
                        SaleOrderLineId = PackingLineTab.SaleOrderLineId,
                        Specification = PackingLineTab.Specification,
                        UnitConversionMultiplier = PackingLineTab.UnitConversionMultiplier,
                        UnitId = PackingLineTab.Product.UnitId,
                        UnitName = PackingLineTab.Product.Unit.UnitName,
                    }

                        ).FirstOrDefault();


        }

        public IQueryable<ComboBoxResult> GetPendingProductsForSaleDispatch(int id, string term)//DocTypeId
        {

            var SaleDispatch = new SaleDispatchHeaderService(_unitOfWork).Find(id);

            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(SaleDispatch.DocTypeId, SaleDispatch.DivisionId, SaleDispatch.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            return (from p in db.ViewSaleOrderBalance
                    join t in db.Product on p.ProductId equals t.ProductId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && ProTab.ProductName.ToLower().Contains(term.ToLower()) && p.BuyerId == SaleDispatch.SaleToBuyerId
                     && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == SaleDispatch.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                     && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == SaleDispatch.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                     && (string.IsNullOrEmpty(term) ? 1 == 1 : ProTab.ProductName.ToLower().Contains(term.ToLower()))
                    group new { p, ProTab } by p.ProductId into g
                    orderby g.Key descending
                    select new ComboBoxResult
                    {
                        id = g.Key.ToString(),
                        text = g.Max(m => m.ProTab.ProductName)
                    }
                        );
        }

        public IEnumerable<ComboBoxResult> GetPendingStockInForDispatch(int SaleDispatchHeaderId, int GodownId, int ProductId, int? Dimension1Id, int? Dimension2Id, string term)
        {

            var SaleDispatchHeader = new SaleDispatchHeaderService(_unitOfWork).Find(SaleDispatchHeaderId);

            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(SaleDispatchHeader.DocTypeId, SaleDispatchHeader.DivisionId, SaleDispatchHeader.SiteId);


            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            SqlParameter SqlParameterSaleDispatchHeaderId = new SqlParameter("@SaleDispatchHeaderId", SaleDispatchHeaderId);
            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterGodownId = new SqlParameter("@GodownId", GodownId);
            SqlParameter SqlParameterDimension1Id = new SqlParameter("@Dimension1Id", Dimension1Id);
            SqlParameter SqlParameterDimension2Id = new SqlParameter("@Dimension2Id", Dimension2Id);

            if (Dimension1Id == null)
            {
                SqlParameterDimension1Id.Value = DBNull.Value;
            }

            if (Dimension2Id == null)
            {
                SqlParameterDimension2Id.Value = DBNull.Value;
            }


            IEnumerable<PendingStockInForDispatch> PendingStockInForDispatch = db.Database.SqlQuery<PendingStockInForDispatch>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spGetHelpListPendingStockInForDispatch @SaleDispatchHeaderId, @GodownId, @ProductId, @Dimension1Id, @Dimension2Id", SqlParameterSaleDispatchHeaderId, SqlParameterGodownId, SqlParameterProductId, SqlParameterDimension1Id, SqlParameterDimension2Id).ToList();


            return (from p in PendingStockInForDispatch
                    where (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.StockInNo.ToLower().Contains(term.ToLower()))
                    select new ComboBoxResult
                    {
                        id = p.StockInId.ToString(),
                        text = p.StockInNo,
                        TextProp1 = "Lot No :" + p.LotNo,
                        TextProp2 = "Balance :" + p.BalanceQty,
                        AProp1 = p.ProductName + ", " + p.Dimension1Name + ", " + p.Dimension2Name,
                        AProp2 = "Date :" + p.StockInDate
                    });


            //return (from p in db.ViewStockInBalance
            //        join pt in db.Product on p.ProductId equals pt.ProductId into ProductTable from ProductTab in ProductTable.DefaultIfEmpty()
            //        join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
            //        join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
            //        where p.BalanceQty > 0
            //        && p.PersonId == SaleDispatchHeader.SaleToBuyerId
            //        && p.Dimension1Id == Dimension1Id
            //        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
            //        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
            //        && (string.IsNullOrEmpty(term) ? 1 == 1 : p.StockInNo.ToLower().Contains(term.ToLower()))
            //        select new ComboBoxResult
            //        {
            //            id = p.StockInId.ToString(),
            //            text = p.StockInNo,
            //            TextProp1 = "Lot No :" + p.LotNo,
            //            TextProp2 = "Balance :" + p.BalanceQty,
            //            AProp1 = ProductTab.ProductName + ", " + Dimension1Tab.Dimension1Name + ", " + Dimension2Tab.Dimension2Name, 
            //            AProp2 = "Date :" + p.StockInDate
            //        });
        }

        

        public void Dispose()
        {
        }
    }

    public class PendingStockInForDispatch
    {
        public int StockInId { get; set; } 
        public string StockInNo { get; set; }
        public string LotNo { get; set; } 
        public Decimal BalanceQty { get; set; }
        public string ProductName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; } 
        public DateTime StockInDate { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; } 
    }
}
