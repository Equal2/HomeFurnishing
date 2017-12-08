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
    public interface IPurchaseInvoiceReturnLineService : IDisposable
    {
        PurchaseInvoiceReturnLine Create(PurchaseInvoiceReturnLine pt);
        void Delete(int id);
        void Delete(PurchaseInvoiceReturnLine pt);
        PurchaseInvoiceReturnLine Find(int id);
        IEnumerable<PurchaseInvoiceReturnLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseInvoiceReturnLine pt);
        PurchaseInvoiceReturnLine Add(PurchaseInvoiceReturnLine pt);
        IEnumerable<PurchaseInvoiceReturnLine> GetPurchaseInvoiceReturnLineList();
        IEnumerable<PurchaseInvoiceReturnLineIndexViewModel> GetLineListForIndex(int HeaderId);
        Task<IEquatable<PurchaseInvoiceReturnLine>> GetAsync();
        Task<PurchaseInvoiceReturnLine> FindAsync(int id);
        PurchaseInvoiceReturnLineViewModel GetPurchaseInvoiceReturnLine(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<PurchaseInvoiceReturnLineViewModel> GetPurchaseReceiptForFilters(PurchaseInvoiceReturnLineFilterViewModel vm);
        IEnumerable<PurchaseInvoiceReturnLineViewModel> GetPurchaseInvoiceForFilters(PurchaseInvoiceReturnLineFilterViewModel vm);
        IEnumerable<PurchaseInvoiceListViewModel> GetPendingPurchaseInvoiceHelpList(int Id, string term);
        IEnumerable<PurchaseGoodsReceiptListViewModel> GetPendingPurchaseReceiptHelpList(int Id, string term);
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);
    }

    public class PurchaseInvoiceReturnLineService : IPurchaseInvoiceReturnLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseInvoiceReturnLine> _PurchaseInvoiceReturnLineRepository;
        RepositoryQuery<PurchaseInvoiceReturnLine> PurchaseInvoiceReturnLineRepository;
        public PurchaseInvoiceReturnLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseInvoiceReturnLineRepository = new Repository<PurchaseInvoiceReturnLine>(db);
            PurchaseInvoiceReturnLineRepository = new RepositoryQuery<PurchaseInvoiceReturnLine>(_PurchaseInvoiceReturnLineRepository);
        }


        public PurchaseInvoiceReturnLine Find(int id)
        {
            return _unitOfWork.Repository<PurchaseInvoiceReturnLine>().Find(id);
        }

        public PurchaseInvoiceReturnLine Create(PurchaseInvoiceReturnLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseInvoiceReturnLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseInvoiceReturnLine>().Delete(id);
        }

        public void Delete(PurchaseInvoiceReturnLine pt)
        {
            _unitOfWork.Repository<PurchaseInvoiceReturnLine>().Delete(pt);
        }
        public IEnumerable<PurchaseInvoiceReturnLineViewModel> GetPurchaseReceiptForFilters(PurchaseInvoiceReturnLineFilterViewModel vm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.PurchaseGoodsReceiptHeaderId)) { SaleOrderIdArr = vm.PurchaseGoodsReceiptHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewPurchaseInvoiceBalance
                        join l in db.PurchaseInvoiceLine on p.PurchaseInvoiceLineId equals l.PurchaseInvoiceLineId into linetable
                        from linetab in linetable.DefaultIfEmpty()
                        join t in db.PurchaseInvoiceHeader on p.PurchaseInvoiceHeaderId equals t.PurchaseInvoiceHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join t1 in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t1.PurchaseGoodsReceiptLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join t2 in db.PurchaseGoodsReceiptHeader on tab1.PurchaseGoodsReceiptHeaderId equals t2.PurchaseGoodsReceiptHeaderId
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.PurchaseGoodsReceiptHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseGoodsReceiptHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        orderby t2.DocDate, t2.DocNo, tab1.Sr
                        select new PurchaseInvoiceReturnLineViewModel
                        {
                            Dimension1Name = tab1.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.Dimension2.Dimension2Name,
                            Specification = tab1.Specification,
                            InvoiceBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            PurchaseInvoiceHeaderDocNo = tab.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            PurchaseInvoiceReturnHeaderId = vm.PurchaseInvoiceReturnHeaderId,
                            PurchaseInvoiceLineId = p.PurchaseInvoiceLineId,
                            UnitId = tab2.UnitId,
                            UnitConversionMultiplier = linetab.UnitConversionMultiplier,
                            DealUnitId = linetab.DealUnitId,
                            Rate = linetab.Rate,
                            RateAfterDiscount = (linetab.Amount / linetab.DealQty) ,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces=linetab.DealUnit.DecimalPlaces,
                            DiscountPer = linetab.DiscountPer,
                            ProductUidName=tab1.ProductUid.ProductUidName,
                        }

                        );
            return temp;
        }
        public IEnumerable<PurchaseInvoiceReturnLineViewModel> GetPurchaseInvoiceForFilters(PurchaseInvoiceReturnLineFilterViewModel vm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.PurchaseInvoiceHeaderId)) { SaleOrderIdArr = vm.PurchaseInvoiceHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }
            //ToChange View to get purchaseorders instead of goodsreceipts
            var temp = (from p in db.ViewPurchaseInvoiceBalance
                        join l in db.PurchaseInvoiceLine on p.PurchaseInvoiceLineId equals l.PurchaseInvoiceLineId into linetable
                        from linetab in linetable.DefaultIfEmpty()
                        join h in db.PurchaseInvoiceHeader on linetab.PurchaseInvoiceHeaderId equals h.PurchaseInvoiceHeaderId
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        join t1 in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t1.PurchaseGoodsReceiptLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.PurchaseInvoiceHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseInvoiceHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        orderby h.DocDate, h.DocNo, linetab.Sr
                        select new PurchaseInvoiceReturnLineViewModel
                        {
                            Dimension1Name = tab1.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.Dimension2.Dimension2Name,
                            Specification = tab1.Specification,
                            InvoiceBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            PurchaseInvoiceHeaderDocNo = p.PurchaseInvoiceNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            PurchaseInvoiceReturnHeaderId = vm.PurchaseInvoiceReturnHeaderId,
                            PurchaseInvoiceLineId = p.PurchaseInvoiceLineId,
                            UnitId = tab2.UnitId,
                            UnitConversionMultiplier = linetab.UnitConversionMultiplier,
                            DealUnitId = linetab.DealUnitId,
                            Rate = linetab.Rate,
                            RateAfterDiscount = (linetab.Amount / linetab.DealQty),
                           unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                           DealunitDecimalPlaces=linetab.DealUnit.DecimalPlaces,
                            DiscountPer = linetab.DiscountPer,
                            ProductUidName=tab1.ProductUid.ProductUidName,
                            
                        }

                        );
            return temp;
        }
        public void Update(PurchaseInvoiceReturnLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseInvoiceReturnLine>().Update(pt);
        }

        public IEnumerable<PurchaseInvoiceReturnLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseInvoiceReturnLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PurchaseInvoiceReturnLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<PurchaseInvoiceReturnLineIndexViewModel> GetLineListForIndex(int HeaderId)
        {
            return (from p in db.PurchaseInvoiceReturnLine
                    join t in db.PurchaseInvoiceLine on p.PurchaseInvoiceLineId equals t.PurchaseInvoiceLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t in db.PurchaseGoodsReceiptLine on tab.PurchaseGoodsReceiptLineId equals t.PurchaseGoodsReceiptLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join t in db.PurchaseGoodsReceiptHeader on tab1.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t in db.Product on tab1.ProductId equals t.ProductId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    where p.PurchaseInvoiceReturnHeaderId == HeaderId
                    orderby p.Sr
                    select new PurchaseInvoiceReturnLineIndexViewModel
                    {

                        ProductName = tab2.ProductName,
                        Qty = p.Qty,
                        PurchaseInvoiceReturnLineId = p.PurchaseInvoiceReturnLineId,
                        UnitId = tab2.UnitId,
                        Specification = tab1.Specification,
                        Dimension1Name = tab1.Dimension1.Dimension1Name,
                        Dimension2Name = tab1.Dimension2.Dimension2Name,
                        LotNo = tab1.LotNo,
                        PurchaseGoodsRecieptHeaderDocNo = tab3.DocNo,
                        PurchaseInvoiceHeaderDocNo = tab.PurchaseInvoiceHeader.DocNo,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                        DealunitDecimalPlaces = p.DealUnit.DecimalPlaces,
                        Rate = p.Rate,
                        Amount = p.Amount,
                        Remark = p.Remark,
                        DiscountPer = p.DiscountPer,
                        ProductUidName=tab1.ProductUid.ProductUidName,
                        UnitName=tab2.Unit.UnitName,
                        DealUnitName=p.DealUnit.UnitName,
                    }
                        );
        }
        public PurchaseInvoiceReturnLineViewModel GetPurchaseInvoiceReturnLine(int id)
        {
            return (from p in db.PurchaseInvoiceReturnLine
                    join t in db.PurchaseInvoiceLine on p.PurchaseInvoiceLineId equals t.PurchaseInvoiceLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t4 in db.ViewPurchaseInvoiceBalance on p.PurchaseInvoiceLineId equals t4.PurchaseInvoiceLineId into table4
                    from tab4 in table4.DefaultIfEmpty()
                    join t3 in db.PurchaseGoodsReceiptLine on tab.PurchaseGoodsReceiptLineId equals t3.PurchaseGoodsReceiptLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t2 in db.PurchaseInvoiceReturnHeader on p.PurchaseInvoiceReturnHeaderId equals t2.PurchaseInvoiceReturnHeaderId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t in db.PurchaseInvoiceHeader on tab.PurchaseInvoiceHeaderId equals t.PurchaseInvoiceHeaderId into table1
                    from tab1 in table1.DefaultIfEmpty()

                    where p.PurchaseInvoiceReturnLineId == id
                    select new PurchaseInvoiceReturnLineViewModel
                    {

                        SupplierId = tab2.SupplierId,
                        ProductId = tab3.ProductId,
                        PurchaseInvoiceLineId = p.PurchaseInvoiceLineId,
                        PurchaseInvoiceHeaderDocNo = tab1.DocNo,
                        PurchaseInvoiceReturnHeaderId = p.PurchaseInvoiceReturnHeaderId,
                        PurchaseInvoiceReturnLineId = p.PurchaseInvoiceReturnLineId,
                        Rate = p.Rate,
                        Amount = p.Amount,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        Qty = p.Qty,
                        InvoiceBalQty = ((p.PurchaseInvoiceLineId == null || tab3 == null) ? p.Qty : p.Qty + tab4.BalanceQty),
                        Remark = p.Remark,
                        UnitId = tab3.Product.UnitId,
                        Dimension1Id = tab3.Dimension1Id,
                        Dimension1Name = tab3.Dimension1.Dimension1Name,
                        Dimension2Id = tab3.Dimension2Id,
                        Dimension2Name = tab3.Dimension2.Dimension2Name,
                        Specification = tab3.Specification,
                        LotNo = tab3.LotNo,
                        DiscountPer = p.DiscountPer,
                        LockReason=p.LockReason,

                    }
                        ).FirstOrDefault();
        }

        public IEnumerable<PurchaseInvoiceReturnLine> GetPurchaseInvoiceReturnLineList()
        {
            var pt = _unitOfWork.Repository<PurchaseInvoiceReturnLine>().Query().Get().OrderBy(m => m.PurchaseInvoiceReturnLineId);

            return pt;
        }

        public PurchaseInvoiceReturnLine Add(PurchaseInvoiceReturnLine pt)
        {
            _unitOfWork.Repository<PurchaseInvoiceReturnLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseInvoiceReturnLine
                        orderby p.PurchaseInvoiceReturnLineId
                        select p.PurchaseInvoiceReturnLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseInvoiceReturnLine
                        orderby p.PurchaseInvoiceReturnLineId
                        select p.PurchaseInvoiceReturnLineId).FirstOrDefault();
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

                temp = (from p in db.PurchaseInvoiceReturnLine
                        orderby p.PurchaseInvoiceReturnLineId
                        select p.PurchaseInvoiceReturnLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseInvoiceReturnLine
                        orderby p.PurchaseInvoiceReturnLineId
                        select p.PurchaseInvoiceReturnLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<PurchaseInvoiceListViewModel> GetPendingPurchaseInvoiceHelpList(int Id, string term)
        {

            var GoodsReceipt = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(GoodsReceipt.DocTypeId, GoodsReceipt.DivisionId, GoodsReceipt.SiteId);

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

            var list = (from p in db.ViewPurchaseInvoiceBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseInvoiceNo.ToLower().Contains(term.ToLower())) && p.SupplierId == GoodsReceipt.SupplierId && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.PurchaseInvoiceDocTypeId.ToString()))
                          && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.PurchaseInvoiceHeaderId into g
                        select new PurchaseInvoiceListViewModel
                        {
                            DocNo = g.Max(m => m.p.PurchaseInvoiceNo),
                            PurchaseInvoiceHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }

        public IEnumerable<PurchaseGoodsReceiptListViewModel> GetPendingPurchaseReceiptHelpList(int Id, string term)
        {

            var GoodsReceipt = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(GoodsReceipt.DocTypeId, GoodsReceipt.DivisionId, GoodsReceipt.SiteId);

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

            var list = (from p in db.ViewPurchaseInvoiceBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseGoodsReceiptNo.ToLower().Contains(term.ToLower())) && p.SupplierId == GoodsReceipt.SupplierId && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.PurchaseInvoiceDocTypeId.ToString()))
                         && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.PurchaseGoodsReceiptHeaderId into g
                        select new PurchaseGoodsReceiptListViewModel
                        {
                            DocNo = g.Max(m => m.p.PurchaseGoodsReceiptNo),
                            PurchaseGoodsReceiptHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }


        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var PurchaseInvoiceReturn = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(PurchaseInvoiceReturn.DocTypeId, PurchaseInvoiceReturn.DivisionId, PurchaseInvoiceReturn.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            var list = (from p in db.Product
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
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
            var Max = (from p in db.PurchaseInvoiceReturnLine
                       where p.PurchaseInvoiceReturnHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseInvoiceReturnLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseInvoiceReturnLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
