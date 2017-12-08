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

namespace Service
{
    public interface ISaleInvoiceReturnLineService : IDisposable
    {
        SaleInvoiceReturnLine Create(SaleInvoiceReturnLine pt);
        void Delete(int id);
        void Delete(SaleInvoiceReturnLine pt);
        SaleInvoiceReturnLine Find(int id);
        IEnumerable<SaleInvoiceReturnLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleInvoiceReturnLine pt);
        SaleInvoiceReturnLine Add(SaleInvoiceReturnLine pt);
        IEnumerable<SaleInvoiceReturnLine> GetSaleInvoiceReturnLineList();
        IEnumerable<SaleInvoiceReturnLineIndexViewModel> GetLineListForIndex(int HeaderId);
        Task<IEquatable<SaleInvoiceReturnLine>> GetAsync();
        Task<SaleInvoiceReturnLine> FindAsync(int id);
        SaleInvoiceReturnLineViewModel GetSaleInvoiceReturnLine(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<SaleInvoiceReturnLineViewModel> GetSaleReceiptForFilters(SaleInvoiceReturnLineFilterViewModel vm);
        IEnumerable<SaleInvoiceReturnLineViewModel> GetSaleInvoiceForFilters(SaleInvoiceReturnLineFilterViewModel vm);
        IEnumerable<SaleInvoiceListViewModel> GetPendingSaleInvoiceHelpList(int Id, string term);
        IEnumerable<SaleDispatchListViewModel> GetPendingSaleReceiptHelpList(int Id, string term);
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);

        IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term);
        IEnumerable<ComboBoxResult> GetSaleInvoiceHelpListForProduct(int Id, string term);
    }

    public class SaleInvoiceReturnLineService : ISaleInvoiceReturnLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleInvoiceReturnLine> _SaleInvoiceReturnLineRepository;
        RepositoryQuery<SaleInvoiceReturnLine> SaleInvoiceReturnLineRepository;
        public SaleInvoiceReturnLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleInvoiceReturnLineRepository = new Repository<SaleInvoiceReturnLine>(db);
            SaleInvoiceReturnLineRepository = new RepositoryQuery<SaleInvoiceReturnLine>(_SaleInvoiceReturnLineRepository);
        }


        public SaleInvoiceReturnLine Find(int id)
        {
            return _unitOfWork.Repository<SaleInvoiceReturnLine>().Find(id);
        }

        public SaleInvoiceReturnLine Create(SaleInvoiceReturnLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleInvoiceReturnLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleInvoiceReturnLine>().Delete(id);
        }

        public void Delete(SaleInvoiceReturnLine pt)
        {
            _unitOfWork.Repository<SaleInvoiceReturnLine>().Delete(pt);
        }
        public IEnumerable<SaleInvoiceReturnLineViewModel> GetSaleReceiptForFilters(SaleInvoiceReturnLineFilterViewModel vm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.SaleDispatchHeaderId)) { SaleOrderIdArr = vm.SaleDispatchHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewSaleInvoiceBalance
                        join l in db.SaleInvoiceLine on p.SaleInvoiceLineId equals l.SaleInvoiceLineId into linetable
                        from linetab in linetable.DefaultIfEmpty()
                        join t in db.SaleInvoiceHeader on p.SaleInvoiceHeaderId equals t.SaleInvoiceHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join t1 in db.SaleDispatchLine on p.SaleDispatchLineId equals t1.SaleDispatchLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join packtab in db.PackingLine on tab1.PackingLineId equals packtab.PackingLineId
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.SaleDispatchHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleDispatchHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        select new SaleInvoiceReturnLineViewModel
                        {
                            Dimension1Name = packtab.Dimension1.Dimension1Name,
                            Dimension2Name = packtab.Dimension2.Dimension2Name,
                            Specification = packtab.Specification,
                            InvoiceBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            SaleInvoiceHeaderDocNo = tab.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            SaleInvoiceReturnHeaderId = vm.SaleInvoiceReturnHeaderId,
                            SaleInvoiceLineId = p.SaleInvoiceLineId,
                            UnitId = tab2.UnitId,
                            UnitConversionMultiplier = linetab.UnitConversionMultiplier ?? 0,
                            DealUnitId = linetab.DealUnitId,
                            Rate = linetab.Rate,
                            RateAfterDiscount = packtab.SaleOrderLine == null ? 0 : (packtab.SaleOrderLine.Amount / packtab.SaleOrderLine.DealQty) ,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces=linetab.DealUnit.DecimalPlaces,
                            DiscountPer = linetab.DiscountPer,
                            ProductUidName = packtab.ProductUid.ProductUidName,
                        }

                        );
            return temp;
        }
        public IEnumerable<SaleInvoiceReturnLineViewModel> GetSaleInvoiceForFilters(SaleInvoiceReturnLineFilterViewModel vm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.SaleInvoiceHeaderId)) { SaleOrderIdArr = vm.SaleInvoiceHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }
            //ToChange View to get Saleorders instead of goodsreceipts
            var temp = (from p in db.ViewSaleInvoiceBalance
                        join l in db.SaleInvoiceLine on p.SaleInvoiceLineId equals l.SaleInvoiceLineId into linetable
                        from linetab in linetable.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        join t1 in db.SaleDispatchLine on p.SaleDispatchLineId equals t1.SaleDispatchLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        from tab2 in table2.DefaultIfEmpty()
                        join t3 in db.PackingLine on tab1.PackingLineId equals t3.PackingLineId into table3 from tab3 in table3.DefaultIfEmpty()
                        join t4 in db.SaleOrderLine on tab3.SaleOrderLineId equals t4.SaleOrderLineId into table4 from tab4 in table4.DefaultIfEmpty()                        
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.SaleInvoiceHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleInvoiceHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        select new SaleInvoiceReturnLineViewModel
                        {
                            Dimension1Name = tab3.Dimension1.Dimension1Name,
                            Dimension2Name = tab3.Dimension2.Dimension2Name,
                            //Specification = t3.Specification,
                            InvoiceBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            SaleInvoiceHeaderDocNo = p.SaleInvoiceNo,
                            BaleNo = p.BaleNo, 
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            SaleInvoiceReturnHeaderId = vm.SaleInvoiceReturnHeaderId,
                            SaleInvoiceLineId = p.SaleInvoiceLineId,
                            UnitId = tab2.UnitId,
                            UnitConversionMultiplier = linetab.UnitConversionMultiplier ?? 0,
                            DealUnitId = linetab.DealUnitId,
                            Rate = linetab.Rate,
                            RateAfterDiscount = (linetab.Amount / linetab.DealQty),
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces=linetab.DealUnit.DecimalPlaces,
                            DiscountPer = linetab.DiscountPer,
                            ProductUidName = tab3.ProductUid.ProductUidName,
                            
                        }

                        );
            return temp;
        }
        public void Update(SaleInvoiceReturnLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleInvoiceReturnLine>().Update(pt);
        }

        public IEnumerable<SaleInvoiceReturnLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleInvoiceReturnLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SaleInvoiceReturnLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<SaleInvoiceReturnLineIndexViewModel> GetLineListForIndex(int HeaderId)
        {
            return (from p in db.SaleInvoiceReturnLine
                    join t in db.SaleInvoiceLine on p.SaleInvoiceLineId equals t.SaleInvoiceLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t in db.SaleDispatchLine on tab.SaleDispatchLineId equals t.SaleDispatchLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join packline in db.PackingLine on tab1.PackingLineId equals packline.PackingLineId
                    join t in db.SaleDispatchHeader on tab1.SaleDispatchHeaderId equals t.SaleDispatchHeaderId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t in db.Product on tab.ProductId equals t.ProductId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    where p.SaleInvoiceReturnHeaderId == HeaderId
                    orderby p.Sr
                    select new SaleInvoiceReturnLineIndexViewModel
                    {

                        ProductName = tab2.ProductName,
                        Qty = p.Qty,
                        SaleInvoiceReturnLineId = p.SaleInvoiceReturnLineId,
                        UnitId = tab2.UnitId,
                        Specification = packline.Specification,
                        Dimension1Name = tab.Dimension1.Dimension1Name,
                        Dimension2Name = tab.Dimension2.Dimension2Name,
                        LotNo = packline.LotNo,
                        SaleDispatchHeaderDocNo = tab3.DocNo,
                        SaleInvoiceHeaderDocNo = tab.SaleInvoiceHeader.DocNo,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                        DealunitDecimalPlaces = p.DealUnit.DecimalPlaces,
                        Rate = p.Rate,
                        Amount = p.Amount,
                        Remark = p.Remark,
                        DiscountPer = p.DiscountPer,
                        ProductUidName = packline.ProductUid.ProductUidName,
                        UnitName=tab2.Unit.UnitName,
                        DealUnitName=p.DealUnit.UnitName,
                    }
                        );
        }
        public SaleInvoiceReturnLineViewModel GetSaleInvoiceReturnLine(int id)
        {
            return (from p in db.SaleInvoiceReturnLine
                    join t in db.SaleInvoiceLine on p.SaleInvoiceLineId equals t.SaleInvoiceLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t4 in db.ViewSaleInvoiceBalance on p.SaleInvoiceLineId equals t4.SaleInvoiceLineId into table4
                    from tab4 in table4.DefaultIfEmpty()
                    join t3 in db.SaleDispatchLine on tab.SaleDispatchLineId equals t3.SaleDispatchLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t4 in db.PackingLine on tab3.PackingLineId equals t4.PackingLineId
                    join t2 in db.SaleInvoiceReturnHeader on p.SaleInvoiceReturnHeaderId equals t2.SaleInvoiceReturnHeaderId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t in db.SaleInvoiceHeader on tab.SaleInvoiceHeaderId equals t.SaleInvoiceHeaderId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.SaleInvoiceReturnLineId == id
                    select new SaleInvoiceReturnLineViewModel
                    {

                        BuyerId = tab2.BuyerId,
                        ProductId = tab.ProductId,
                        SaleInvoiceLineId = p.SaleInvoiceLineId,
                        SaleInvoiceHeaderDocNo = tab1.DocNo,
                        SaleInvoiceReturnHeaderId = p.SaleInvoiceReturnHeaderId,
                        SaleInvoiceReturnLineId = p.SaleInvoiceReturnLineId,
                        Rate = p.Rate,
                        Amount = p.Amount,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        Qty = p.Qty,
                        InvoiceBalQty = ((p.SaleInvoiceLineId == null || tab3 == null) ? p.Qty : p.Qty + tab4.BalanceQty),
                        Remark = p.Remark,
                        UnitId = tab.Product.UnitId,
                        Dimension1Id = tab.Dimension1Id,
                        Dimension1Name = tab.Dimension1.Dimension1Name,
                        Dimension2Id = tab.Dimension2Id,
                        Dimension2Name = tab.Dimension2.Dimension2Name,
                        Specification = t4.Specification,
                        LotNo = t4.LotNo,
                        DiscountPer = p.DiscountPer,
                        DiscountAmount = p.DiscountAmount
                    }).FirstOrDefault();
        }

        public IEnumerable<SaleInvoiceReturnLine> GetSaleInvoiceReturnLineList()
        {
            var pt = _unitOfWork.Repository<SaleInvoiceReturnLine>().Query().Get().OrderBy(m => m.SaleInvoiceReturnLineId);

            return pt;
        }

        public SaleInvoiceReturnLine Add(SaleInvoiceReturnLine pt)
        {
            _unitOfWork.Repository<SaleInvoiceReturnLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleInvoiceReturnLine
                        orderby p.SaleInvoiceReturnLineId
                        select p.SaleInvoiceReturnLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleInvoiceReturnLine
                        orderby p.SaleInvoiceReturnLineId
                        select p.SaleInvoiceReturnLineId).FirstOrDefault();
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

                temp = (from p in db.SaleInvoiceReturnLine
                        orderby p.SaleInvoiceReturnLineId
                        select p.SaleInvoiceReturnLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleInvoiceReturnLine
                        orderby p.SaleInvoiceReturnLineId
                        select p.SaleInvoiceReturnLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<SaleInvoiceListViewModel> GetPendingSaleInvoiceHelpList(int Id, string term)
        {

            var GoodsReceipt = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(GoodsReceipt.DocTypeId, GoodsReceipt.DivisionId, GoodsReceipt.SiteId);

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

            var list = (from p in db.ViewSaleInvoiceBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleInvoiceNo.ToLower().Contains(term.ToLower())) && p.SaleToBuyerId == GoodsReceipt.BuyerId && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.SaleInvoiceDocTypeId.ToString()))
                          && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.SaleInvoiceHeaderId into g
                        select new SaleInvoiceListViewModel
                        {
                            DocNo = g.Max(m => m.p.SaleInvoiceNo),
                            SaleInvoiceHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }

        public IEnumerable<SaleDispatchListViewModel> GetPendingSaleReceiptHelpList(int Id, string term)
        {

            var GoodsReceipt = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(GoodsReceipt.DocTypeId, GoodsReceipt.DivisionId, GoodsReceipt.SiteId);

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

            var list = (from p in db.ViewSaleInvoiceBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleDispatchNo.ToLower().Contains(term.ToLower())) && p.SaleToBuyerId == GoodsReceipt.BuyerId && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.SaleInvoiceDocTypeId.ToString()))
                         && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.SaleDispatchHeaderId into g
                        select new SaleDispatchListViewModel
                        {
                            DocNo = g.Max(m => m.p.SaleDispatchNo),
                            SaleDispatchHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }


        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var SaleInvoiceReturn = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoiceReturn.DocTypeId, SaleInvoiceReturn.DivisionId, SaleInvoiceReturn.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] Products = null;
            if (!string.IsNullOrEmpty(settings.filterProducts)) { Products = settings.filterProducts.Split(",".ToCharArray()); }
            else { Products = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }

            var list = (from p in db.Product
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProducts) ? 1 == 1 : Products.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(p.ProductGroupId.ToString()))
                        group new { p } by p.ProductId into g
                        select new ComboBoxList
                        {
                            PropFirst = g.Max(m => m.p.ProductName),
                            Id = g.Key,

                            //    DocumentTypeName=g.Max(p=>p.p.DocumentTypeShortName)
                        }
                          ).Take(20);

            return list.ToList();
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.SaleInvoiceReturnLine
                       where p.SaleInvoiceReturnHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }

        public IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term)
        {

            var SaleInvoice = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(Id);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoice.DocTypeId, SaleInvoice.DivisionId, SaleInvoice.SiteId);

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


        public IEnumerable<ComboBoxResult> GetSaleInvoiceHelpListForProduct(int Id, string term)
        {
            var SaleInvoiceReturnHeader = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoiceReturnHeader.DocTypeId, SaleInvoiceReturnHeader.DivisionId, SaleInvoiceReturnHeader.SiteId);

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            return (from VSib in db.ViewSaleInvoiceBalance
                    join L in db.SaleInvoiceLine on VSib.SaleInvoiceLineId equals L.SaleInvoiceLineId into SaleInvoiceLineTable
                    from SaleInvoiceLineTab in SaleInvoiceLineTable.DefaultIfEmpty()
                    where VSib.BalanceQty > 0 && SaleInvoiceLineTab.SaleInvoiceHeader.SaleToBuyerId == SaleInvoiceReturnHeader.BuyerId
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? VSib.SiteId == CurrentSiteId : contraSites.Contains(VSib.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? VSib.DivisionId == CurrentDivisionId : contraDivisions.Contains(VSib.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : SaleInvoiceLineTab.SaleInvoiceHeader.DocNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : SaleInvoiceLineTab.SaleInvoiceHeader.DocType.DocumentTypeShortName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : SaleInvoiceLineTab.Product.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : SaleInvoiceLineTab.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : SaleInvoiceLineTab.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : SaleInvoiceLineTab.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : SaleInvoiceLineTab.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower())
                        )
                    select new ComboBoxResult
                    {
                        id = VSib.SaleInvoiceLineId.ToString(),
                        text = SaleInvoiceLineTab.SaleInvoiceHeader.DocType.DocumentTypeShortName + "-" + SaleInvoiceLineTab.SaleInvoiceHeader.DocNo,
                        TextProp1 = "Balance :" + VSib.BalanceQty,
                        TextProp2 = "Date :" + SaleInvoiceLineTab.SaleInvoiceHeader.DocDate,
                        AProp1 = SaleInvoiceLineTab.Product.ProductName,
                        AProp2 = ((SaleInvoiceLineTab.Dimension1.Dimension1Name == null) ? "" : SaleInvoiceLineTab.Dimension1.Dimension1Name) +
                                    ((SaleInvoiceLineTab.Dimension2.Dimension2Name == null) ? "" : "," + SaleInvoiceLineTab.Dimension2.Dimension2Name) +
                                    ((SaleInvoiceLineTab.Dimension3.Dimension3Name == null) ? "" : "," + SaleInvoiceLineTab.Dimension3.Dimension3Name) +
                                    ((SaleInvoiceLineTab.Dimension4.Dimension4Name == null) ? "" : "," + SaleInvoiceLineTab.Dimension4.Dimension4Name)
                    });
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SaleInvoiceReturnLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleInvoiceReturnLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
