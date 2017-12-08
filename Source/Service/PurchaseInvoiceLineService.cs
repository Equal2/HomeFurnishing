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
    public interface IPurchaseInvoiceLineService : IDisposable
    {
        PurchaseInvoiceLine Create(PurchaseInvoiceLine pt);
        void Delete(int id);
        void Delete(PurchaseInvoiceLine pt);
        PurchaseInvoiceLine Find(int id);
        IEnumerable<PurchaseInvoiceLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseInvoiceLine pt);
        PurchaseInvoiceLine Add(PurchaseInvoiceLine pt);
        IEnumerable<PurchaseInvoiceLine> GetPurchaseInvoiceLineList();
        IEnumerable<PurchaseInvoiceLineIndexViewModel> GetLineListForIndex(int HeaderId);
        IEnumerable<PurchaseInvoiceLineIndexViewModel> GetLineListForIndexDirectPurchaseInvoice(int HeaderId);
        Task<IEquatable<PurchaseInvoiceLine>> GetAsync();
        Task<PurchaseInvoiceLine> FindAsync(int id);
        PurchaseInvoiceLineViewModel GetPurchaseInvoiceLine(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<PurchaseInvoiceLineViewModel> GetPurchaseReceiptForFilters(PurchaseInvoiceLineFilterViewModel vm);
        IEnumerable<PurchaseInvoiceLineViewModel> GetPurchaseOrderForFilters(PurchaseInvoiceLineFilterViewModel vm);
        IEnumerable<PurchaseInvoiceLineViewModel> GetPurchaseIndentForFilters(PurchaseInvoiceLineFilterViewModel vm);
        PurchaseInvoiceLineViewModel GetPurchaseInvoiceLineBalance(int id);
        IEnumerable<PurchaseGoodsReceiptListViewModel> GetPendingPurchaseReceiptHelpList(int Id, string term);//PurchaseOrderHeaderId
        IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseOrderHelpList(int Id, string term);//PurchaseOrderHeaderId
        IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseIndentHelpList(int Id, string term);//PurchaseOrderHeaderId
        IEnumerable<PurchaseInvoiceLine> GetLineListForChargeCalculation(int id);//Purchase InvoiceHeader Id
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);

    }

    public class PurchaseInvoiceLineService : IPurchaseInvoiceLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseInvoiceLine> _PurchaseInvoiceLineRepository;
        RepositoryQuery<PurchaseInvoiceLine> PurchaseInvoiceLineRepository;
        public PurchaseInvoiceLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseInvoiceLineRepository = new Repository<PurchaseInvoiceLine>(db);
            PurchaseInvoiceLineRepository = new RepositoryQuery<PurchaseInvoiceLine>(_PurchaseInvoiceLineRepository);
        }


        public PurchaseInvoiceLine Find(int id)
        {
            return _unitOfWork.Repository<PurchaseInvoiceLine>().Find(id);
        }

        public PurchaseInvoiceLine Create(PurchaseInvoiceLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseInvoiceLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseInvoiceLine>().Delete(id);
        }

        public void Delete(PurchaseInvoiceLine pt)
        {
            _unitOfWork.Repository<PurchaseInvoiceLine>().Delete(pt);
        }
        public IEnumerable<PurchaseInvoiceLineViewModel> GetPurchaseReceiptForFilters(PurchaseInvoiceLineFilterViewModel vm)
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

            var temp = (from p in db.ViewPurchaseGoodsReceiptBalance 
                        join t in db.PurchaseGoodsReceiptHeader on p.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join t1 in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t1.PurchaseGoodsReceiptLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join Pol in db.PurchaseOrderLine on tab1.PurchaseOrderLineId equals Pol.PurchaseOrderLineId into PurchaseOrderLineTable
                        from PurchaseOrderLineTab in PurchaseOrderLineTable.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2                        
                        from tab2 in table2.DefaultIfEmpty()                        
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.PurchaseGoodsReceiptHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseGoodsReceiptHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        orderby tab.DocDate, tab.DocNo, tab1.Sr
                        select new PurchaseInvoiceLineViewModel
                        {
                            ProductUidName = tab1.ProductUid!=null?tab1.ProductUid.ProductUidName:"",
                            Dimension1Name = tab1.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.Dimension2.Dimension2Name,
                            Specification = tab1.Specification,
                            ReceiptBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            DocQty=p.BalanceDocQty,
                            ReceiptBalDocQty=p.BalanceDocQty,
                            PurchaseGoodsReceiptHeaderDocNo= tab.DocNo,
                            ShortQty = p.BalanceDocQty - p.BalanceQty,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            PurchaseInvoiceHeaderId = vm.PurchaseInvoiceHeaderId,
                            PurchaseGoodsReceiptLineId = p.PurchaseGoodsReceiptLineId,
                            UnitId = tab2.UnitId,
                            UnitName=tab2.Unit.UnitName,
                            DealUnitId=tab1.DealUnitId,
                            DealUnitName=tab1.DealUnit.UnitName,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces=tab1.DealUnit.DecimalPlaces,
                            ReceiptBalDealQty=p.BalanceDealQty,
                            DealQty = (tab1.UnitConversionMultiplier==null||tab1.UnitConversionMultiplier==0)?p.BalanceQty:p.BalanceQty * tab1.UnitConversionMultiplier,
                            UnitConversionMultiplier=tab1.UnitConversionMultiplier,
                            Rate = tab1.PurchaseOrderLine == null ? 0 : tab1.PurchaseOrderLine.Rate ?? 0,
                            RateAfterDiscount = tab1.PurchaseOrderLine == null ? 0 : (tab1.PurchaseOrderLine.Amount / tab1.PurchaseOrderLine.DealQty) ?? 0,
                            DiscountPer = PurchaseOrderLineTab.DiscountPer
                        }

                        );
            return temp;
        }
        public IEnumerable<PurchaseInvoiceLineViewModel> GetPurchaseOrderForFilters(PurchaseInvoiceLineFilterViewModel vm)
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
            //ToChange View to get purchaseorders instead of goodsreceipts
            var temp = (from p in db.ViewPurchaseGoodsReceiptBalance
                        join t in db.PurchaseGoodsReceiptHeader on p.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        join t1 in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t1.PurchaseGoodsReceiptLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        from tab2 in table2.DefaultIfEmpty()                        
                        join Pol in db.PurchaseOrderLine on tab1.PurchaseOrderLineId equals Pol.PurchaseOrderLineId into PurchaseOrderLineTable from PurchaseOrderLineTab in PurchaseOrderLineTable.DefaultIfEmpty()
                        join POH in db.PurchaseOrderHeader on PurchaseOrderLineTab.PurchaseOrderHeaderId equals POH.PurchaseOrderHeaderId
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.PurchaseOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        orderby POH.DocDate, POH.DocNo, PurchaseOrderLineTab.Sr
                        select new PurchaseInvoiceLineViewModel
                        {
                            ProductUidName = tab1.ProductUid != null ? tab1.ProductUid.ProductUidName : "",
                            Dimension1Name = tab1.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.Dimension2.Dimension2Name,
                            Specification = tab1.Specification,
                            ReceiptBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            ReceiptBalDocQty=p.BalanceDocQty,
                            DocQty=p.BalanceDocQty,
                            ShortQty = p.BalanceDocQty - p.BalanceQty,
                            PurchaseGoodsReceiptHeaderDocNo = tab.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            PurchaseInvoiceHeaderId = vm.PurchaseInvoiceHeaderId,
                            PurchaseGoodsReceiptLineId = p.PurchaseGoodsReceiptLineId,
                            UnitId = tab2.UnitId,
                            UnitName=tab2.Unit.UnitName,
                            DealUnitName=tab1.DealUnit.UnitName,
                            DealUnitId = tab1.DealUnitId,
                            unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces = tab1.DealUnit.DecimalPlaces,
                            ReceiptBalDealQty = p.BalanceDealQty,
                            DealQty = (tab1.UnitConversionMultiplier == null || tab1.UnitConversionMultiplier == 0) ? p.BalanceQty : p.BalanceQty * tab1.UnitConversionMultiplier,
                            UnitConversionMultiplier = tab1.UnitConversionMultiplier,
                            Rate = tab1.PurchaseOrderLine == null ? 0 : tab1.PurchaseOrderLine.Rate ?? 0,
                            RateAfterDiscount = tab1.PurchaseOrderLine == null ? 0 : (tab1.PurchaseOrderLine.Amount / tab1.PurchaseOrderLine.DealQty) ?? 0,
                            DiscountPer = PurchaseOrderLineTab.DiscountPer
                        }

                        );
            return temp;
        }

        public IEnumerable<PurchaseInvoiceLineViewModel> GetPurchaseIndentForFilters(PurchaseInvoiceLineFilterViewModel vm)
        {
            byte? UnitConvForId = new PurchaseInvoiceHeaderService(_unitOfWork).Find(vm.PurchaseInvoiceHeaderId).UnitConversionForId;            

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.PurchaseIndentHeaderId)) { SaleOrderIdArr = vm.PurchaseIndentHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }


            if (!string.IsNullOrEmpty(vm.DealUnitId))
            {
                Unit Dealunit = new UnitService(_unitOfWork).Find(vm.DealUnitId);

                var temp = (from p in db.ViewPurchaseIndentBalance
                            join t in db.PurchaseIndentHeader on p.PurchaseIndentHeaderId equals t.PurchaseIndentHeaderId into table
                            from tab in table.DefaultIfEmpty()
                            join product in db.Product on p.ProductId equals product.ProductId into table2
                            join t1 in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t1.PurchaseIndentLineId into table1
                            from tab1 in table1.DefaultIfEmpty()
                            from tab2 in table2.DefaultIfEmpty()
                            join t3 in db.UnitConversion on new { p1 = p.ProductId, DU1 = vm.DealUnitId, U1 = UnitConvForId ?? 0 } equals new { p1 = t3.ProductId ?? 0, DU1 = t3.ToUnitId, U1 = t3.UnitConversionForId } into table3
                            from tab3 in table3.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.PurchaseIndentHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseIndentHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                            && p.BalanceQty > 0                            
                            select new PurchaseInvoiceLineViewModel
                            {
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Dimension1Id = tab1.Dimension1Id,
                                Dimension2Id = tab1.Dimension2Id,
                                Specification = tab1.Specification,
                                IndentBalQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                Rate = vm.Rate,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                PurchaseInvoiceHeaderId = vm.PurchaseInvoiceHeaderId,
                                PurchaseIndentLineId = p.PurchaseIndentLineId,
                                UnitId = tab2.UnitId,
                                PurchaseIndentHeaderDocNo = p.PurchaseIndentNo,
                                DealUnitId = (tab3 == null ? tab2.UnitId : vm.DealUnitId),
                                UnitConversionMultiplier = (tab3 == null ? 1 : tab3.ToQty / tab3.FromQty),
                                UnitConversionException = tab3 == null ? true : false,
                                unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                                DealunitDecimalPlaces = (tab3 == null ? tab2.Unit.DecimalPlaces : Dealunit.DecimalPlaces),
                            }

                        );
                return temp;
            }
            else
            {
                var temp = (from p in db.ViewPurchaseIndentBalance
                            join t in db.PurchaseIndentHeader on p.PurchaseIndentHeaderId equals t.PurchaseIndentHeaderId into table
                            from tab in table.DefaultIfEmpty()
                            join product in db.Product on p.ProductId equals product.ProductId into table2
                            join t1 in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t1.PurchaseIndentLineId into table1
                            from tab1 in table1.DefaultIfEmpty()
                            from tab2 in table2.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.PurchaseIndentHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseIndentHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                            && p.BalanceQty > 0
                            select new PurchaseInvoiceLineViewModel
                            {
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension1Id = tab1.Dimension1Id,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Dimension2Id = tab1.Dimension2Id,
                                Specification = tab1.Specification,
                                IndentBalQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                Rate = vm.Rate,
                                PurchaseIndentHeaderDocNo = tab.DocNo,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                PurchaseInvoiceHeaderId = vm.PurchaseInvoiceHeaderId,
                                PurchaseIndentLineId = p.PurchaseIndentLineId,
                                UnitId = tab2.UnitId,
                                DealUnitId = tab2.UnitId,
                                UnitConversionMultiplier = 1,
                                unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                                DealunitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            }

                        );
                return temp;
            }

        }


        public void Update(PurchaseInvoiceLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseInvoiceLine>().Update(pt);
        }

        public IEnumerable<PurchaseInvoiceLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseInvoiceLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PurchaseInvoiceLineId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<PurchaseInvoiceLineIndexViewModel> GetLineListForIndex(int HeaderId)
        {
            return (from p in db.PurchaseInvoiceLine
                    join t in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t.PurchaseGoodsReceiptLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t in db.PurchaseOrderLine on tab.PurchaseOrderLineId equals t.PurchaseOrderLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join t in db.PurchaseOrderHeader on tab1.PurchaseOrderHeaderId equals t.PurchaseOrderHeaderId into table3 from tab3 in table3.DefaultIfEmpty()
                    join t in db.Product on tab.ProductId equals t.ProductId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    where p.PurchaseInvoiceHeaderId==HeaderId
                    orderby p.Sr
                    select new PurchaseInvoiceLineIndexViewModel
                    {
                        
                        ProductName=tab2.ProductName,
                        Amount=p.Amount,
                        Rate=p.Rate,
                        Qty=p.DocQty,
                        PurchaseOrderDocNo=tab3.DocNo,
                        PurchaseInvoiceLineId=p.PurchaseInvoiceLineId,
                        UnitId=tab2.UnitId,
                        ProductUidName=tab.ProductUid.ProductUidName,
                        Specification=tab.Specification,
                        Dimension1Name=tab.Dimension1.Dimension1Name,
                        Dimension2Name=tab.Dimension2.Dimension2Name,
                        LotNo=tab.LotNo,                        
                        PurchaseGoodsRecieptHeaderDocNo=tab.PurchaseGoodsReceiptHeader.DocNo,
                        PurchaseOrderHeaderDocNo=tab1.PurchaseOrderHeader.DocNo,
                        ReceiptHeaderId=tab.PurchaseGoodsReceiptHeaderId,
                        ReceiptDocTypeId=tab.PurchaseGoodsReceiptHeader.DocTypeId,
                        ReceiptLineId=tab.PurchaseGoodsReceiptLineId,
                        OrderHeaderId=tab1.PurchaseOrderHeaderId,
                        OrderDocTypeId=tab1.PurchaseOrderHeader.DocTypeId,
                        OrderLineId=tab1.PurchaseOrderLineId,
                        DealQty=p.DealQty,
                        DealUnitId=p.DealUnitId,
                        UnitName=tab2.Unit.UnitName,
                        DealUnitName=p.DealUnit.UnitName,
                        Remark=p.Remark,
                        ShortQty=tab.DocQty-p.DocQty,
                        unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                        DealunitDecimalPlaces=p.DealUnit.DecimalPlaces,
                        DiscountPer = p.DiscountPer
                       
                    }
                        );
        }


        public IEnumerable<PurchaseInvoiceLineIndexViewModel> GetLineListForIndexDirectPurchaseInvoice(int HeaderId)
        {
            return (from p in db.PurchaseInvoiceLine
                    join t in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t.PurchaseGoodsReceiptLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t in db.PurchaseIndentLine on tab.PurchaseIndentLineId equals t.PurchaseIndentLineId into table1 from tab1 in table1.DefaultIfEmpty()
                    join t in db.PurchaseIndentHeader on tab1.PurchaseIndentHeaderId equals t.PurchaseIndentHeaderId into table3 from tab3 in table3.DefaultIfEmpty()
                    join t in db.Product on tab.ProductId equals t.ProductId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    where p.PurchaseInvoiceHeaderId == HeaderId
                    orderby p.Sr
                    select new PurchaseInvoiceLineIndexViewModel
                    {

                        ProductName = tab2.ProductName,
                        Amount = p.Amount,
                        Rate = p.Rate,
                        Qty = tab.Qty,                        
                        PurchaseInvoiceLineId = p.PurchaseInvoiceLineId,
                        UnitId = tab2.UnitId,                        
                        Specification = tab.Specification,
                        Dimension1Name = tab.Dimension1.Dimension1Name,
                        Dimension2Name = tab.Dimension2.Dimension2Name,
                        LotNo = tab.LotNo,                        
                        PurchaseIndentDocNo = tab3.DocNo,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        UnitName = tab2.Unit.UnitName,
                        DealUnitName = p.DealUnit.UnitName,
                        Remark = p.Remark,
                        unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                        DealunitDecimalPlaces = p.DealUnit.DecimalPlaces,
                        DiscountPer = p.DiscountPer
                    }
                        );
        }

        public PurchaseInvoiceLineViewModel GetPurchaseInvoiceLine(int id)
        {
            return (from p in db.PurchaseInvoiceLine
                    join t in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t.PurchaseGoodsReceiptLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t2 in db.PurchaseInvoiceHeader on p.PurchaseInvoiceHeaderId equals t2.PurchaseInvoiceHeaderId into table2 from tab2 in table2.DefaultIfEmpty()
                    join t in db.PurchaseGoodsReceiptHeader on tab.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId into table1
                    from tab1 in table1.DefaultIfEmpty()

                    where p.PurchaseInvoiceLineId == id
                    select new PurchaseInvoiceLineViewModel
                    {

                        SupplierId=tab2.SupplierId,
                        Amount = p.Amount,
                        ProductId = tab.ProductId,
                        PurchaseGoodsReceiptLineId = p.PurchaseGoodsReceiptLineId,
                        PurchaseIndentLineId=tab.PurchaseIndentLineId,
                        PurchaseGoodsRecieptHeaderDocNo = tab1.DocNo,
                        PurchaseInvoiceHeaderId = p.PurchaseInvoiceHeaderId,
                        PurchaseInvoiceLineId = p.PurchaseInvoiceLineId,
                        //Qty = tab.Qty,
                        ShortQty=tab.DocQty-tab.Qty,
                        AdjShortQty = tab.DocQty - p.DocQty,
                        DocQty=p.DocQty,
                        Rate = p.Rate,
                        Remark = p.Remark,
                        UnitConversionMultiplier=p.UnitConversionMultiplier,                        
                        DealUnitId=p.DealUnitId,
                        DealQty=p.DealQty,
                        UnitName=tab.Product.Unit.UnitName,
                        UnitId=tab.Product.UnitId,
                        Dimension1Id=tab.Dimension1Id,
                        Dimension1Name=tab.Dimension1.Dimension1Name,
                        Dimension2Id=tab.Dimension2Id,
                        Dimension2Name=tab.Dimension2.Dimension2Name,
                        Specification=tab.Specification,
                        LotNo=tab.LotNo,
                        DiscountPer = p.DiscountPer,
                        LockReason=p.LockReason,
                        
                    }
                        ).FirstOrDefault();
        }

        public PurchaseInvoiceLineViewModel GetPurchaseInvoiceLineBalance(int id)
        {
            return (from b in db.ViewPurchaseInvoiceBalance
                    join p in db.PurchaseInvoiceLine on b.PurchaseInvoiceLineId equals p.PurchaseInvoiceLineId 
                    join t in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t.PurchaseGoodsReceiptLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t2 in db.PurchaseInvoiceHeader on p.PurchaseInvoiceHeaderId equals t2.PurchaseInvoiceHeaderId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t in db.PurchaseGoodsReceiptHeader on tab.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId into table1
                    from tab1 in table1.DefaultIfEmpty()

                    where p.PurchaseInvoiceLineId == id
                    select new PurchaseInvoiceLineViewModel
                    {

                        SupplierId = tab2.SupplierId,
                        Amount = p.Amount,
                        ProductId = tab.ProductId,
                        PurchaseGoodsReceiptLineId = p.PurchaseGoodsReceiptLineId,
                        PurchaseGoodsRecieptHeaderDocNo = tab1.DocNo,
                        PurchaseInvoiceHeaderId = p.PurchaseInvoiceHeaderId,
                        PurchaseInvoiceLineId = p.PurchaseInvoiceLineId,
                        Qty = b.BalanceQty,
                        Rate = p.Rate,
                        Remark = p.Remark,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        DealUnitId = p.DealUnitId,
                        DealQty = p.DealQty,
                        UnitId = tab.Product.UnitId,
                        Dimension1Id = tab.Dimension1Id,
                        Dimension1Name = tab.Dimension1.Dimension1Name,
                        Dimension2Id = tab.Dimension2Id,
                        Dimension2Name = tab.Dimension2.Dimension2Name,
                        Specification = tab.Specification,
                        LotNo = tab.LotNo,
                        DiscountPer = p.DiscountPer

                    }
                        ).FirstOrDefault();
        }

        public IEnumerable<PurchaseInvoiceLine> GetPurchaseInvoiceLineList()
        {
            var pt = _unitOfWork.Repository<PurchaseInvoiceLine>().Query().Get().OrderBy(m=>m.PurchaseInvoiceLineId);

            return pt;
        }

        public PurchaseInvoiceLine Add(PurchaseInvoiceLine pt)
        {
            _unitOfWork.Repository<PurchaseInvoiceLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseInvoiceLine
                        orderby p.PurchaseInvoiceLineId
                        select p.PurchaseInvoiceLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseInvoiceLine
                        orderby p.PurchaseInvoiceLineId
                        select p.PurchaseInvoiceLineId).FirstOrDefault();
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

                temp = (from p in db.PurchaseInvoiceLine
                        orderby p.PurchaseInvoiceLineId
                        select p.PurchaseInvoiceLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseInvoiceLine
                        orderby p.PurchaseInvoiceLineId
                        select p.PurchaseInvoiceLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<PurchaseGoodsReceiptListViewModel> GetPendingPurchaseReceiptHelpList(int Id, string term)
        {

            var PurchaseInvoice = new PurchaseInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(PurchaseInvoice.DocTypeId, PurchaseInvoice.DivisionId, PurchaseInvoice.SiteId);

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
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseGoodsReceiptNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && p.SupplierId==PurchaseInvoice.SupplierId
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

        public IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseOrderHelpList(int Id, string term)
        {

            var PurchaseInvoice = new PurchaseInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(PurchaseInvoice.DocTypeId, PurchaseInvoice.DivisionId, PurchaseInvoice.SiteId);

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
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && p.SupplierId == PurchaseInvoice.SupplierId
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.PurchaseOrderHeaderId into g
                        select new PurchaseOrderLineListViewModel
                        {
                            DocNo = g.Max(m => m.p.PurchaseOrderNo),
                            PurchaseOrderHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }


        public IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseIndentHelpList(int Id, string term)
        {

            var PurchaseInvoice = new PurchaseInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(PurchaseInvoice.DocTypeId, PurchaseInvoice.DivisionId, PurchaseInvoice.SiteId);

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

            var list = (from p in db.ViewPurchaseIndentBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseIndentNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.PurchaseIndentHeaderId into g
                        select new PurchaseOrderLineListViewModel
                        {
                            DocNo = g.Max(m => m.p.PurchaseIndentNo),
                            PurchaseIndentHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }

        public IEnumerable<PurchaseInvoiceLine> GetLineListForChargeCalculation(int id)//Purchase InvoiceHeader Id
        {

            return (from p in db.PurchaseInvoiceLine
                    join t in db.PurchaseInvoiceLineCharge on p.PurchaseInvoiceLineId equals t.LineTableId into table
                    from tab in table.DefaultIfEmpty()
                    where tab == null && p.PurchaseInvoiceHeaderId==id
                    select p
                        );

        }

        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var PurchaseInvoice = new PurchaseInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(PurchaseInvoice.DocTypeId, PurchaseInvoice.DivisionId, PurchaseInvoice.SiteId);

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
            var Max = (from p in db.PurchaseInvoiceLine
                       where p.PurchaseInvoiceHeaderId == id
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


        public Task<IEquatable<PurchaseInvoiceLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseInvoiceLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
