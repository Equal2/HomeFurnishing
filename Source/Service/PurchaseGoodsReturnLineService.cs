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
    public interface IPurchaseGoodsReturnLineService : IDisposable
    {
        PurchaseGoodsReturnLine Create(PurchaseGoodsReturnLine pt);
        void Delete(int id);
        void Delete(PurchaseGoodsReturnLine pt);
        PurchaseGoodsReturnLine Find(int id);
        PurchaseGoodsReturnLine FindByInvoiceReturn(int id);
        IEnumerable<PurchaseGoodsReturnLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseGoodsReturnLine pt);
        PurchaseGoodsReturnLine Add(PurchaseGoodsReturnLine pt);
        IEnumerable<PurchaseGoodsReturnLine> GetPurchaseGoodsReturnLineList();
        IEnumerable<PurchaseGoodsReturnLineIndexViewModel> GetLineListForIndex(int HeaderId);
        Task<IEquatable<PurchaseGoodsReturnLine>> GetAsync();
        Task<PurchaseGoodsReturnLine> FindAsync(int id);
        PurchaseGoodsReturnLineViewModel GetPurchaseGoodsReturnLine(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<PurchaseGoodsReturnLineViewModel> GetPurchaseOrderForFilters(PurchaseGoodsReturnLineFilterViewModel vm);
        IEnumerable<PurchaseGoodsReturnLineViewModel> GetPurchaseGoodsReceiptsForFilters(PurchaseGoodsReturnLineFilterViewModel vm);
        IEnumerable<PurchaseGoodsReceiptListViewModel> GetPendingPurchaseReceiptHelpList(int Id, string term);//PurchaseOrderHeaderId
        IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseOrderHelpList(int Id, string term);//PurchaseOrderHeaderId
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        ProductUidTransactionDetail GetProductUidTransactionDetail(int ProductUidId);
        int GetMaxSr(int id);
    }

    public class PurchaseGoodsReturnLineService : IPurchaseGoodsReturnLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseGoodsReturnLine> _PurchaseGoodsReturnLineRepository;
        RepositoryQuery<PurchaseGoodsReturnLine> PurchaseGoodsReturnLineRepository;
        public PurchaseGoodsReturnLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseGoodsReturnLineRepository = new Repository<PurchaseGoodsReturnLine>(db);
            PurchaseGoodsReturnLineRepository = new RepositoryQuery<PurchaseGoodsReturnLine>(_PurchaseGoodsReturnLineRepository);
        }


        public PurchaseGoodsReturnLine Find(int id)
        {
            return _unitOfWork.Repository<PurchaseGoodsReturnLine>().Find(id);
        }

        public PurchaseGoodsReturnLine FindByInvoiceReturn(int id)
        {
            return (from p in db.PurchaseGoodsReturnLine
                    join pi in db.PurchaseInvoiceReturnLine on p.PurchaseGoodsReturnLineId equals pi.PurchaseGoodsReturnLineId
                    where pi.PurchaseInvoiceReturnLineId == id
                    select p).FirstOrDefault();
        }

        public PurchaseGoodsReturnLine Create(PurchaseGoodsReturnLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseGoodsReturnLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseGoodsReturnLine>().Delete(id);
        }

        public void Delete(PurchaseGoodsReturnLine pt)
        {
            _unitOfWork.Repository<PurchaseGoodsReturnLine>().Delete(pt);
        }
        public IEnumerable<PurchaseGoodsReturnLineViewModel> GetPurchaseOrderForFilters(PurchaseGoodsReturnLineFilterViewModel vm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.PurchaseOrderHeaderId)) { SaleOrderIdArr = vm.PurchaseOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewPurchaseGoodsReceiptBalance
                        join t1 in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t1.PurchaseGoodsReceiptLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join t in db.PurchaseGoodsReceiptHeader on p.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join t2 in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t2.PurchaseOrderLineId into tabl2 from POLinetab in tabl2.DefaultIfEmpty()
                        join t3 in db.PurchaseOrderHeader on POLinetab.PurchaseOrderHeaderId equals t3.PurchaseOrderHeaderId
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.PurchaseOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        orderby t3.DocDate, t3.DocNo, POLinetab.Sr
                        select new PurchaseGoodsReturnLineViewModel
                        {
                            Dimension1Name = tab1.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.Dimension2.Dimension2Name,
                            Specification = tab1.Specification,
                            GoodsReceiptBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            PurchaseGoodsReceiptHeaderDocNo = tab.DocNo,
                            PurchaseOrderDocNo=tab1.PurchaseOrderLine==null ?"" :tab1.PurchaseOrderLine.PurchaseOrderHeader.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            PurchaseGoodsReturnHeaderId = vm.PurchaseGoodsReturnHeaderId,
                            PurchaseGoodsReceiptLineId = p.PurchaseGoodsReceiptLineId,
                            UnitId = tab2.UnitId,
                            UnitConversionMultiplier = tab1.UnitConversionMultiplier,
                            DealUnitId = tab1.DealUnitId,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces = tab1.DealUnit.DecimalPlaces,
                        }

                        );
            return temp;
        }
        public IEnumerable<PurchaseGoodsReturnLineViewModel> GetPurchaseGoodsReceiptsForFilters(PurchaseGoodsReturnLineFilterViewModel vm)
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
            //ToChange View to get purchaseorders instead of goodsreceipts
            var temp = (from p in db.ViewPurchaseGoodsReceiptBalance
                        join l in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals l.PurchaseGoodsReceiptLineId into linetable
                        from linetab in linetable.DefaultIfEmpty()
                        join t in db.PurchaseGoodsReceiptHeader on p.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        join t1 in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t1.PurchaseGoodsReceiptLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.PurchaseGoodsReceiptHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseGoodsReceiptHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        orderby tab.DocDate,tab.DocNo,linetab.Sr
                        select new PurchaseGoodsReturnLineViewModel
                        {
                            Dimension1Name = tab1.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.Dimension2.Dimension2Name,
                            Specification = tab1.Specification,
                            GoodsReceiptBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            PurchaseGoodsReceiptHeaderDocNo = tab.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            PurchaseGoodsReturnHeaderId = vm.PurchaseGoodsReturnHeaderId,
                            PurchaseGoodsReceiptLineId = p.PurchaseGoodsReceiptLineId,
                            UnitId = tab2.UnitId,
                            UnitName = tab2.Unit.UnitName,
                            UnitConversionMultiplier = linetab.UnitConversionMultiplier,
                            DealUnitId = linetab.DealUnitId,
                            DealUnitName = linetab.DealUnit.UnitName,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces=linetab.DealUnit.DecimalPlaces,
                        }

                        );
            return temp;
        }
        public void Update(PurchaseGoodsReturnLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseGoodsReturnLine>().Update(pt);
        }

        public IEnumerable<PurchaseGoodsReturnLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseGoodsReturnLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PurchaseGoodsReturnLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<PurchaseGoodsReturnLineIndexViewModel> GetLineListForIndex(int HeaderId)
        {
            return (from p in db.PurchaseGoodsReturnLine
                    join t in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t.PurchaseGoodsReceiptLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t in db.PurchaseGoodsReceiptLine on tab.PurchaseGoodsReceiptLineId equals t.PurchaseGoodsReceiptLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join t in db.PurchaseGoodsReceiptHeader on tab1.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t in db.Product on tab1.ProductId equals t.ProductId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    where p.PurchaseGoodsReturnHeaderId == HeaderId
                    orderby p.Sr
                    select new PurchaseGoodsReturnLineIndexViewModel
                    {

                        ProductName = tab2.ProductName,
                        Qty = p.Qty,
                        PurchaseGoodsReturnLineId = p.PurchaseGoodsReturnLineId,
                        UnitId = tab2.UnitId,
                        Specification = tab1.Specification,
                        Dimension1Name = tab1.Dimension1.Dimension1Name,
                        Dimension2Name = tab1.Dimension2.Dimension2Name,
                        LotNo = tab1.LotNo,
                        PurchaseGoodsRecieptHeaderDocNo = tab3.DocNo,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                        DealunitDecimalPlaces=p.DealUnit.DecimalPlaces,
                        Remark = p.Remark,
                        StockId = p.StockId ?? 0,
                        ProductUidName=tab1.ProductUid.ProductUidName,
                    }
                        );
        }
        public PurchaseGoodsReturnLineViewModel GetPurchaseGoodsReturnLine(int id)
        {
            return (from p in db.PurchaseGoodsReturnLine
                    join t in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t.PurchaseGoodsReceiptLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t4 in db.ViewPurchaseGoodsReceiptBalance on p.PurchaseGoodsReceiptLineId equals t4.PurchaseGoodsReceiptLineId into table4
                    from tab4 in table4.DefaultIfEmpty()
                    join t3 in db.PurchaseGoodsReceiptLine on tab.PurchaseGoodsReceiptLineId equals t3.PurchaseGoodsReceiptLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t2 in db.PurchaseGoodsReturnHeader on p.PurchaseGoodsReturnHeaderId equals t2.PurchaseGoodsReturnHeaderId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t in db.PurchaseGoodsReceiptHeader on tab.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join Pu in db.ProductUid on tab.ProductUidId equals Pu.ProductUIDId into ProductUidTable
                    from ProductUidTab in ProductUidTable.DefaultIfEmpty()
                    where p.PurchaseGoodsReturnLineId == id
                    select new PurchaseGoodsReturnLineViewModel
                    {

                        SupplierId = tab2.SupplierId,
                        ProductUidId = tab.ProductUidId,
                        ProductUidName = ProductUidTab.ProductUidName,
                        ProductId = tab3.ProductId,
                        PurchaseGoodsReceiptLineId = p.PurchaseGoodsReceiptLineId,
                        PurchaseGoodsReceiptHeaderDocNo = tab1.DocNo,
                        PurchaseGoodsReturnHeaderId = p.PurchaseGoodsReturnHeaderId,
                        PurchaseGoodsReturnLineId = p.PurchaseGoodsReturnLineId,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        DealUnitName=p.DealUnit.UnitName,
                        Qty = p.Qty,
                        GoodsReceiptBalQty = ((p.PurchaseGoodsReceiptLineId == null || tab4 == null) ? p.Qty : p.Qty + tab4.BalanceQty),
                        Remark = p.Remark,
                        UnitId = tab3.Product.UnitId,
                        UnitName=tab3.Product.Unit.UnitName,
                        Dimension1Id = tab3.Dimension1Id,
                        Dimension1Name = tab3.Dimension1.Dimension1Name,
                        Dimension2Id = tab3.Dimension2Id,
                        Dimension2Name = tab3.Dimension2.Dimension2Name,
                        Specification = tab3.Specification,
                        LockReason=p.LockReason,
                    }
                        ).FirstOrDefault();
        }

        public IEnumerable<PurchaseGoodsReturnLine> GetPurchaseGoodsReturnLineList()
        {
            var pt = _unitOfWork.Repository<PurchaseGoodsReturnLine>().Query().Get().OrderBy(m => m.PurchaseGoodsReturnLineId);

            return pt;
        }

        public PurchaseGoodsReturnLine Add(PurchaseGoodsReturnLine pt)
        {
            _unitOfWork.Repository<PurchaseGoodsReturnLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseGoodsReturnLine
                        orderby p.PurchaseGoodsReturnLineId
                        select p.PurchaseGoodsReturnLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseGoodsReturnLine
                        orderby p.PurchaseGoodsReturnLineId
                        select p.PurchaseGoodsReturnLineId).FirstOrDefault();
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

                temp = (from p in db.PurchaseGoodsReturnLine
                        orderby p.PurchaseGoodsReturnLineId
                        select p.PurchaseGoodsReturnLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseGoodsReturnLine
                        orderby p.PurchaseGoodsReturnLineId
                        select p.PurchaseGoodsReturnLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public IEnumerable<PurchaseGoodsReceiptListViewModel> GetPendingPurchaseReceiptHelpList(int Id, string term)
        {

            var PurchaseInvoice = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(PurchaseInvoice.DocTypeId, PurchaseInvoice.DivisionId, PurchaseInvoice.SiteId);

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

            var list = (from p in db.ViewPurchaseGoodsReceiptBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseGoodsReceiptNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && p.SupplierId == PurchaseInvoice.SupplierId
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
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

        public IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseOrderHelpList(int Id, string term)//PurchaseOrderHeaderId
        {
            var PurchaseInvoice = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(PurchaseInvoice.DocTypeId, PurchaseInvoice.DivisionId, PurchaseInvoice.SiteId);

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

            var list = (from p in db.ViewPurchaseGoodsReceiptBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && p.SupplierId == PurchaseInvoice.SupplierId && p.PurchaseOrderLineId !=null
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                         && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.PurchaseOrderHeaderId == null ? -1 : p.PurchaseOrderHeaderId into g
                        select new PurchaseOrderLineListViewModel
                        {
                            DocNo = g.Max(m => m.p.PurchaseOrderNo),
                            PurchaseOrderHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }

        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var PurchaseGoodsReturn = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(PurchaseGoodsReturn.DocTypeId, PurchaseGoodsReturn.DivisionId, PurchaseGoodsReturn.SiteId);

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

        public ProductUidTransactionDetail GetProductUidTransactionDetail(int ProductUidId)
        {
            ProductUidTransactionDetail Temp = (from L in db.PurchaseGoodsReceiptLine
                                                join H in db.PurchaseGoodsReceiptHeader on L.PurchaseGoodsReceiptHeaderId equals H.PurchaseGoodsReceiptHeaderId into PurchaseGoodsReceiptHeaderTable
                                                from PurchaseGoodsReceiptHeaderTab in PurchaseGoodsReceiptHeaderTable.DefaultIfEmpty()
                                                where L.ProductUidId == ProductUidId
                                                select new ProductUidTransactionDetail
                                                {
                                                    DocLineId = L.PurchaseGoodsReceiptLineId,
                                                    DocNo = PurchaseGoodsReceiptHeaderTab.DocNo
                                                }).ToList().Last();

            return Temp;
        }


        public int GetMaxSr(int id)
        {
            var Max = (from p in db.PurchaseGoodsReturnLine
                       where p.PurchaseGoodsReturnHeaderId == id
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


        public Task<IEquatable<PurchaseGoodsReturnLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseGoodsReturnLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
