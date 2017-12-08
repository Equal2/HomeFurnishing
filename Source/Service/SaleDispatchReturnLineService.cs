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
    public interface ISaleDispatchReturnLineService : IDisposable
    {
        SaleDispatchReturnLine Create(SaleDispatchReturnLine pt);
        void Delete(int id);
        void Delete(SaleDispatchReturnLine pt);
        SaleDispatchReturnLine Find(int id);
        SaleDispatchReturnLine FindByInvoiceReturn(int id);
        IEnumerable<SaleDispatchReturnLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleDispatchReturnLine pt);
        SaleDispatchReturnLine Add(SaleDispatchReturnLine pt);
        IEnumerable<SaleDispatchReturnLine> GetSaleDispatchReturnLineList();
        IEnumerable<SaleDispatchReturnLineIndexViewModel> GetLineListForIndex(int HeaderId);
        Task<IEquatable<SaleDispatchReturnLine>> GetAsync();
        Task<SaleDispatchReturnLine> FindAsync(int id);
        SaleDispatchReturnLineViewModel GetSaleDispatchReturnLine(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<SaleDispatchReturnLineViewModel> GetSaleOrderForFilters(SaleDispatchReturnLineFilterViewModel vm);
        IEnumerable<SaleDispatchReturnLineViewModel> GetSaleDispatchsForFilters(SaleDispatchReturnLineFilterViewModel vm);
        IEnumerable<SaleDispatchListViewModel> GetPendingSaleReceiptHelpList(int Id, string term);//SaleOrderHeaderId
        IEnumerable<SaleOrderLineListViewModel> GetPendingSaleOrderHelpList(int Id, string term);//SaleOrderHeaderId
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        ProductUidTransactionDetail GetProductUidTransactionDetail(int ProductUidId);
        int GetMaxSr(int id);
    }

    public class SaleDispatchReturnLineService : ISaleDispatchReturnLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleDispatchReturnLine> _SaleDispatchReturnLineRepository;
        RepositoryQuery<SaleDispatchReturnLine> SaleDispatchReturnLineRepository;
        public SaleDispatchReturnLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleDispatchReturnLineRepository = new Repository<SaleDispatchReturnLine>(db);
            SaleDispatchReturnLineRepository = new RepositoryQuery<SaleDispatchReturnLine>(_SaleDispatchReturnLineRepository);
        }


        public SaleDispatchReturnLine Find(int id)
        {
            return _unitOfWork.Repository<SaleDispatchReturnLine>().Find(id);
        }

        public SaleDispatchReturnLine FindByInvoiceReturn(int id)
        {
            return (from p in db.SaleDispatchReturnLine
                    join pi in db.SaleInvoiceReturnLine on p.SaleDispatchReturnLineId equals pi.SaleDispatchReturnLineId
                    where pi.SaleInvoiceReturnLineId == id
                    select p).FirstOrDefault();
        }

        public SaleDispatchReturnLine Create(SaleDispatchReturnLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDispatchReturnLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleDispatchReturnLine>().Delete(id);
        }

        public void Delete(SaleDispatchReturnLine pt)
        {
            _unitOfWork.Repository<SaleDispatchReturnLine>().Delete(pt);
        }
        public IEnumerable<SaleDispatchReturnLineViewModel> GetSaleOrderForFilters(SaleDispatchReturnLineFilterViewModel vm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.SaleOrderHeaderId)) { SaleOrderIdArr = vm.SaleOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewSaleDispatchBalance
                        join l in db.SaleDispatchLine on p.SaleDispatchLineId equals l.SaleDispatchLineId into linetable
                        from linetab in linetable.DefaultIfEmpty()
                        join t in db.SaleDispatchHeader on p.SaleDispatchHeaderId equals t.SaleDispatchHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join t1 in db.SaleDispatchLine on p.SaleDispatchLineId equals t1.SaleDispatchLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join t2 in db.PackingLine on tab1.PackingLineId equals t2.PackingLineId into InvoiceTab
                        from PackTab in InvoiceTab.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.SaleOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        select new SaleDispatchReturnLineViewModel
                        {
                            Dimension1Name = PackTab.Dimension1.Dimension1Name,
                            Dimension2Name = PackTab.Dimension2.Dimension2Name,                            
                            GoodsReceiptBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            SaleDispatchHeaderDocNo = tab.DocNo,
                            SaleOrderDocNo=PackTab.SaleOrderLine==null ?"" :PackTab.SaleOrderLine.SaleOrderHeader.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            SaleDispatchReturnHeaderId = vm.SaleDispatchReturnHeaderId,
                            SaleDispatchLineId = p.SaleDispatchLineId,
                            UnitId = tab2.UnitId,
                            //UnitConversionMultiplier = PackTab.UnitConversionMultiplier??0,
                            DealUnitId = PackTab.DealUnitId,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces=PackTab.DealUnit.DecimalPlaces,
                        }

                        );
            return temp;
        }
        public IEnumerable<SaleDispatchReturnLineViewModel> GetSaleDispatchsForFilters(SaleDispatchReturnLineFilterViewModel vm)
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
            //ToChange View to get Saleorders instead of goodsreceipts
            var temp = (from p in db.ViewSaleDispatchBalance
                        join l in db.SaleDispatchLine on p.SaleDispatchLineId equals l.SaleDispatchLineId into linetable
                        from linetab in linetable.DefaultIfEmpty()
                        join t3 in db.SaleInvoiceLine on linetab.SaleDispatchLineId equals t3.SaleDispatchLineId
                        join t in db.SaleDispatchHeader on p.SaleDispatchHeaderId equals t.SaleDispatchHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        join t1 in db.SaleDispatchLine on p.SaleDispatchLineId equals t1.SaleDispatchLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.SaleDispatchHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleDispatchHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        select new SaleDispatchReturnLineViewModel
                        {
                            Dimension1Name = t3.Dimension1.Dimension1Name,
                            Dimension2Name = t3.Dimension2.Dimension2Name,                            
                            GoodsReceiptBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            SaleDispatchHeaderDocNo = tab.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            SaleDispatchReturnHeaderId = vm.SaleDispatchReturnHeaderId,
                            SaleDispatchLineId = p.SaleDispatchLineId,
                            UnitId = tab2.UnitId,
                            UnitName = tab2.Unit.UnitName,
                            UnitConversionMultiplier = t3.UnitConversionMultiplier ?? 0,
                            DealUnitId = t3.DealUnitId,
                            DealUnitName = t3.DealUnit.UnitName,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces=t3.DealUnit.DecimalPlaces,
                        }

                        );
            return temp;
        }
        public void Update(SaleDispatchReturnLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDispatchReturnLine>().Update(pt);
        }

        public IEnumerable<SaleDispatchReturnLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleDispatchReturnLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SaleDispatchReturnLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<SaleDispatchReturnLineIndexViewModel> GetLineListForIndex(int HeaderId)
        {
            return (from p in db.SaleDispatchReturnLine
                    join t in db.SaleDispatchLine on p.SaleDispatchLineId equals t.SaleDispatchLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t in db.SaleDispatchLine on tab.SaleDispatchLineId equals t.SaleDispatchLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join PackTab in db.PackingLine on tab1.PackingLineId equals PackTab.PackingLineId
                    join t in db.SaleDispatchHeader on tab1.SaleDispatchHeaderId equals t.SaleDispatchHeaderId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t in db.Product on PackTab.ProductId equals t.ProductId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    where p.SaleDispatchReturnHeaderId == HeaderId
                    orderby p.Sr
                    select new SaleDispatchReturnLineIndexViewModel
                    {

                        ProductName = tab2.ProductName,
                        Qty = p.Qty,
                        SaleDispatchReturnLineId = p.SaleDispatchReturnLineId,
                        UnitId = tab2.UnitId,
                        Specification = PackTab.Specification,
                        Dimension1Name = PackTab.Dimension1.Dimension1Name,
                        Dimension2Name = PackTab.Dimension2.Dimension2Name,
                        LotNo = PackTab.LotNo,
                        SaleDispatchHeaderDocNo = tab3.DocNo,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                        DealunitDecimalPlaces=p.DealUnit.DecimalPlaces,
                        Remark = p.Remark,
                        StockId = p.StockId ?? 0

                    }
                        );
        }
        public SaleDispatchReturnLineViewModel GetSaleDispatchReturnLine(int id)
        {
            return (from p in db.SaleDispatchReturnLine
                    join t in db.SaleDispatchLine on p.SaleDispatchLineId equals t.SaleDispatchLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t4 in db.ViewSaleDispatchBalance on p.SaleDispatchLineId equals t4.SaleDispatchLineId into table4
                    from tab4 in table4.DefaultIfEmpty()
                    join t3 in db.SaleDispatchLine on tab.SaleDispatchLineId equals t3.SaleDispatchLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t2 in db.SaleDispatchReturnHeader on p.SaleDispatchReturnHeaderId equals t2.SaleDispatchReturnHeaderId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t in db.SaleDispatchHeader on tab.SaleDispatchHeaderId equals t.SaleDispatchHeaderId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join PackTab in db.PackingLine on tab3.PackingLineId equals PackTab.PackingLineId
                    join Pu in db.ProductUid on PackTab.ProductUidId equals Pu.ProductUIDId into ProductUidTable
                    from ProductUidTab in ProductUidTable.DefaultIfEmpty()
                    where p.SaleDispatchReturnLineId == id
                    select new SaleDispatchReturnLineViewModel
                    {

                        BuyerId = tab2.BuyerId,
                        ProductUidId = PackTab.ProductUidId,
                        ProductUidName = ProductUidTab.ProductUidName,
                        ProductId = PackTab.ProductId,
                        SaleDispatchLineId = p.SaleDispatchLineId,
                        SaleDispatchHeaderDocNo = tab1.DocNo,
                        SaleDispatchReturnHeaderId = p.SaleDispatchReturnHeaderId,
                        SaleDispatchReturnLineId = p.SaleDispatchReturnLineId,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        DealUnitName=p.DealUnit.UnitName,
                        Qty = p.Qty,
                        GoodsReceiptBalQty = ((p.SaleDispatchLineId == null || tab4 == null) ? p.Qty : p.Qty + tab4.BalanceQty),
                        Remark = p.Remark,
                        UnitId = PackTab.Product.UnitId,
                        UnitName = PackTab.Product.Unit.UnitName,
                        Dimension1Id = PackTab.Dimension1Id,
                        Dimension1Name = PackTab.Dimension1.Dimension1Name,
                        Dimension2Id = PackTab.Dimension2Id,
                        Dimension2Name = PackTab.Dimension2.Dimension2Name,
                        Specification = PackTab.Specification,
                    }
                        ).FirstOrDefault();
        }

        public IEnumerable<SaleDispatchReturnLine> GetSaleDispatchReturnLineList()
        {
            var pt = _unitOfWork.Repository<SaleDispatchReturnLine>().Query().Get().OrderBy(m => m.SaleDispatchReturnLineId);

            return pt;
        }

        public SaleDispatchReturnLine Add(SaleDispatchReturnLine pt)
        {
            _unitOfWork.Repository<SaleDispatchReturnLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleDispatchReturnLine
                        orderby p.SaleDispatchReturnLineId
                        select p.SaleDispatchReturnLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDispatchReturnLine
                        orderby p.SaleDispatchReturnLineId
                        select p.SaleDispatchReturnLineId).FirstOrDefault();
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

                temp = (from p in db.SaleDispatchReturnLine
                        orderby p.SaleDispatchReturnLineId
                        select p.SaleDispatchReturnLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDispatchReturnLine
                        orderby p.SaleDispatchReturnLineId
                        select p.SaleDispatchReturnLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public IEnumerable<SaleDispatchListViewModel> GetPendingSaleReceiptHelpList(int Id, string term)
        {

            var SaleInvoice = new SaleDispatchReturnHeaderService(_unitOfWork).Find(Id);

            //var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(SaleInvoice.DocTypeId, SaleInvoice.DivisionId, SaleInvoice.SiteId);

            //string[] contraDocTypes = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            //else { contraDocTypes = new string[] { "NA" }; }

            //string[] contraSites = null;
            //if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            //else { contraSites = new string[] { "NA" }; }

            //string[] contraDivisions = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            //else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var list = (from p in db.ViewSaleDispatchBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleDispatchNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && p.BuyerId == SaleInvoice.BuyerId
                        //&& (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                         //&& (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                   // && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.SaleDispatchHeaderId into g
                        select new SaleDispatchListViewModel
                        {
                            DocNo = g.Max(m => m.p.SaleDispatchNo),
                            SaleDispatchHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }

        public IEnumerable<SaleOrderLineListViewModel> GetPendingSaleOrderHelpList(int Id, string term)//SaleOrderHeaderId
        {
            var SaleInvoice = new SaleDispatchReturnHeaderService(_unitOfWork).Find(Id);

           // var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(SaleInvoice.DocTypeId, SaleInvoice.DivisionId, SaleInvoice.SiteId);

            //string[] contraDocTypes = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            //else { contraDocTypes = new string[] { "NA" }; }

            //string[] contraSites = null;
            //if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            //else { contraSites = new string[] { "NA" }; }

            //string[] contraDivisions = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            //else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var list = (from p in db.ViewSaleDispatchBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && p.BuyerId == SaleInvoice.BuyerId && p.SaleOrderLineId !=null
                        //&& (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        // && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                   // && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.SaleOrderHeaderId == null ? -1 : p.SaleOrderHeaderId into g
                        select new SaleOrderLineListViewModel
                        {
                            DocNo = g.Max(m => m.p.SaleOrderNo),
                            SaleOrderHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }

        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var SaleDispatchReturn = new SaleDispatchReturnHeaderService(_unitOfWork).Find(Id);

            //var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(SaleDispatchReturn.DocTypeId, SaleDispatchReturn.DivisionId, SaleDispatchReturn.SiteId);

            //string[] ProductTypes = null;
            //if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            //else { ProductTypes = new string[] { "NA" }; }

            var list = (from p in db.Product
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
                        //&& (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
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
            ProductUidTransactionDetail Temp = (from L in db.SaleDispatchLine
                                                join t2 in db.PackingLine on L.PackingLineId equals t2.PackingLineId
                                                join H in db.SaleDispatchHeader on L.SaleDispatchHeaderId equals H.SaleDispatchHeaderId into SaleDispatchHeaderTable
                                                from SaleDispatchHeaderTab in SaleDispatchHeaderTable.DefaultIfEmpty()
                                                where t2.ProductUidId == ProductUidId
                                                select new ProductUidTransactionDetail
                                                {
                                                    DocLineId = L.SaleDispatchLineId,
                                                    DocNo = SaleDispatchHeaderTab.DocNo
                                                }).FirstOrDefault();

            return Temp;
        }


        public int GetMaxSr(int id)
        {
            var Max = (from p in db.SaleDispatchReturnLine
                       where p.SaleDispatchReturnHeaderId == id
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


        public Task<IEquatable<SaleDispatchReturnLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleDispatchReturnLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
