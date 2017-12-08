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

namespace Service
{
    public interface IPurchaseGoodsReceiptLineService : IDisposable
    {
        PurchaseGoodsReceiptLine Create(PurchaseGoodsReceiptLine pt);
        void Delete(int id);
        void Delete(PurchaseGoodsReceiptLine pt);
        PurchaseGoodsReceiptLine Find(int id);
        IEnumerable<PurchaseGoodsReceiptLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseGoodsReceiptLine pt);
        PurchaseGoodsReceiptLine Add(PurchaseGoodsReceiptLine pt);
        IEnumerable<PurchaseGoodsReceiptLine> GetPurchaseGoodsReceiptLineList();
        IEnumerable<PurchaseGoodsReceiptLineViewModel> GetLineListForIndex(int headerId);//HeaderId
        Task<IEquatable<PurchaseGoodsReceiptLine>> GetAsync();
        Task<PurchaseGoodsReceiptLine> FindAsync(int id);
        PurchaseGoodsReceiptLineViewModel GetPurchaseGoodsReceiptLine(int id);//Line Id
        IEnumerable<PurchaseGoodsReceiptLineViewModel> GetPurchaseOrdersForFilters(PurchaseGoodsReceiptLineFilterViewModel vm);
        PurchaseGoodsReceiptLineViewModel GetPurchaseGoodsReceiptDetail(int id);//Line Id
        PurchaseGoodsReceiptLineViewModel GetPurchaseGoodsReceiptDetailBalance(int id);
        IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseOrderHelpList(int Id, string term);
        IEnumerable<ComboBoxList> GetProductsHelpList(string term);
        int GetMaxSr(int id);
        

    }

    public class PurchaseGoodsReceiptLineService : IPurchaseGoodsReceiptLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseGoodsReceiptLine> _PurchaseGoodsReceiptLineRepository;
        RepositoryQuery<PurchaseGoodsReceiptLine> PurchaseGoodsReceiptLineRepository;
        public PurchaseGoodsReceiptLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseGoodsReceiptLineRepository = new Repository<PurchaseGoodsReceiptLine>(db);
            PurchaseGoodsReceiptLineRepository = new RepositoryQuery<PurchaseGoodsReceiptLine>(_PurchaseGoodsReceiptLineRepository);
        }

        public IEnumerable<PurchaseGoodsReceiptLineViewModel> GetPurchaseOrdersForFilters(PurchaseGoodsReceiptLineFilterViewModel vm)
        {
            var GoodsReceipt = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(vm.PurchaseGoodsReceiptHeaderId);

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.PurchaseOrderHeaderId)) { SaleOrderIdArr = vm.PurchaseOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewPurchaseOrderBalance
                        join t in db.PurchaseOrderHeader on p.PurchaseOrderHeaderId equals t.PurchaseOrderHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        join t1 in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t1.PurchaseOrderLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.PurchaseOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0 && p.SupplierId == vm.SupplierId
                        && p.SiteId == GoodsReceipt.SiteId && p.DivisionId == GoodsReceipt.DivisionId 
                        orderby tab.DocDate, tab.DocNo, tab1.Sr
                        select new PurchaseGoodsReceiptLineViewModel
                        {
                            Dimension1Name = tab1.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.Dimension2.Dimension2Name,
                            Specification = tab1.Specification,
                            OrderBalanceQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            DocQty=p.BalanceQty,
                            PurchaseGoodsReceiptHeaderDocNo = tab.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            PurchaseGoodsReceiptHeaderId = vm.PurchaseGoodsReceiptHeaderId,
                            PurchaseOrderLineId = p.PurchaseOrderLineId,
                            UnitId = tab2.UnitId,
                            PurchaseOrderDocNo = p.PurchaseOrderNo,
                            DealUnitId = tab1.DealUnitId,
                            OrderDealQty=tab1.DealQty,
                            OrderQty=tab1.Qty,
                            DealunitDecimalPlaces=tab1.DealUnit.DecimalPlaces,
                            UnitConversionMultiplier = tab1.UnitConversionMultiplier,
                            DealQty = (tab1.UnitConversionMultiplier == null || tab1.UnitConversionMultiplier == 0) ? p.BalanceQty : p.BalanceQty * tab1.UnitConversionMultiplier,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,                            
                        }



                        );
            return temp;
        }

        public PurchaseGoodsReceiptLine Find(int id)
        {
            return _unitOfWork.Repository<PurchaseGoodsReceiptLine>().Find(id);
        }

        public PurchaseGoodsReceiptLine Create(PurchaseGoodsReceiptLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseGoodsReceiptLine>().Insert(pt);
            return pt;
        }

        public PurchaseGoodsReceiptLineViewModel GetPurchaseGoodsReceiptLine(int id)
        {
            return (from p in db.PurchaseGoodsReceiptLine
                    join t in db.ViewPurchaseOrderBalance on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t in db.ViewPurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    where p.PurchaseGoodsReceiptLineId == id
                    select new PurchaseGoodsReceiptLineViewModel
                    {
                        ProductUidId = p.ProductUidId,
                        ProductUidName = p.ProductUid.ProductUidName,
                        ProductId = p.ProductId,
                        PurchaseOrderLineId = p.PurchaseOrderLineId,
                        Specification = p.Specification,
                        Dimension1Id = p.Dimension1Id,
                        Dimension2Id = p.Dimension2Id,
                        DealQty = p.DealQty,
                        Qty = p.Qty,
                        OrderBalanceQty = ((p.PurchaseOrderLineId == null || tab == null) ? p.Qty : p.Qty + tab.BalanceQty),
                        DealUnitId = p.DealUnitId,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        LotNo = p.LotNo,
                        PurchaseGoodsReceiptHeaderId = p.PurchaseGoodsReceiptHeaderId,
                        PurchaseGoodsReceiptHeaderDocNo = p.PurchaseGoodsReceiptHeader.DocNo,
                        PurchaseGoodsReceiptLineId = p.PurchaseGoodsReceiptLineId,
                        Remark = p.Remark,
                        UnitId = p.Product.UnitId,
                        OrderQty= (Decimal?)tab2.OrderQty ?? 0,
                        OrderDealQty = (Decimal?)tab2.OrderDeliveryQty ?? 0,
                        PurchaseOrderDocNo = p.PurchaseOrderLine.PurchaseOrderHeader.DocNo,
                        SupplierId = p.PurchaseGoodsReceiptHeader.SupplierId,
                        GodownId = p.PurchaseGoodsReceiptHeader.GodownId,
                        DebitNoteAmount = p.DebitNoteAmount,
                        DebitNoteReason = p.DebitNoteReason,
                        DealunitDecimalPlaces=p.DealUnit.DecimalPlaces,
                        Rate = tab2.Rate,
                        DocQty=p.DocQty,
                        LockReason=p.LockReason,
                    }

                        ).FirstOrDefault();
        }
        public PurchaseGoodsReceiptLineViewModel GetPurchaseGoodsReceiptDetail(int id)
        {
            return (from p in db.PurchaseGoodsReceiptLine
                    where p.PurchaseGoodsReceiptLineId == id
                    join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    select new PurchaseGoodsReceiptLineViewModel
                    {
                        Qty = p.Qty,
                        Remark = p.Remark,
                        Rate = tab.Rate,
                        Amount = tab.Amount,
                        Dimension1Id = p.Dimension1Id,
                        Dimension1Name = p.Dimension1.Dimension1Name,
                        Dimension2Id = p.Dimension2Id,
                        Dimension2Name = p.Dimension2.Dimension2Name,
                        Specification = p.Specification,
                        LotNo = p.LotNo,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        UnitId = p.Product.UnitId,
                        DealUnitId = p.DealUnitId,
                        DealQty = p.DealQty,
                    }).FirstOrDefault();

        }

        public PurchaseGoodsReceiptLineViewModel GetPurchaseGoodsReceiptDetailBalance(int id)
        {
            return (from b in db.ViewPurchaseGoodsReceiptBalance
                    join p in db.PurchaseGoodsReceiptLine on b.PurchaseGoodsReceiptLineId equals p.PurchaseGoodsReceiptLineId
                    join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    where b.PurchaseGoodsReceiptLineId == id

                    select new PurchaseGoodsReceiptLineViewModel
                    {
                        Qty = b.BalanceQty,
                        Remark = p.Remark,
                        Rate = tab.Rate,
                        Amount = tab.Rate * (b.BalanceQty * p.UnitConversionMultiplier),
                        Dimension1Id = p.Dimension1Id,
                        Dimension1Name = p.Dimension1.Dimension1Name,
                        Dimension2Id = p.Dimension2Id,
                        Dimension2Name = p.Dimension2.Dimension2Name,
                        Specification = p.Specification,                        
                        LotNo = p.LotNo,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        UnitId = p.Product.UnitId,
                        UnitName=p.Product.Unit.UnitName,
                        DealUnitId = p.DealUnitId,
                        DealUnitName=p.DealUnit.UnitName,
                        DealQty = b.BalanceQty * p.UnitConversionMultiplier,
                        DiscountPer = tab.DiscountPer
                    }).FirstOrDefault();

        }

        public PurchaseInvoiceLineViewModel GetPurchaseGoodsReceiptDetailBalanceForInvoice(int id)
        {
            var temp= (from b in db.ViewPurchaseGoodsReceiptBalance
                    join p in db.PurchaseGoodsReceiptLine on b.PurchaseGoodsReceiptLineId equals p.PurchaseGoodsReceiptLineId
                    join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    where b.PurchaseGoodsReceiptLineId == id

                    select new PurchaseInvoiceLineViewModel
                    {
                        Qty = b.BalanceQty,
                        Remark = p.Remark,
                        Rate = tab.Rate ?? 0,
                        //Amount = tab.Rate * (b.BalanceDocQty * p.UnitConversionMultiplier),
                        ReceiptBalDocQty=b.BalanceDocQty,
                        ReceiptBalQty=b.BalanceQty,
                        ReceiptBalDealQty=b.BalanceDealQty,
                        Dimension1Id = p.Dimension1Id,
                        Dimension1Name = p.Dimension1.Dimension1Name,
                        Dimension2Id = p.Dimension2Id,
                        Dimension2Name = p.Dimension2.Dimension2Name,
                        Specification = p.Specification,
                        DocQty = b.BalanceDocQty,
                        LotNo = p.LotNo,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        UnitId = p.Product.UnitId,
                        UnitName = p.Product.Unit.UnitName,
                        DealUnitId = p.DealUnitId,
                        DealUnitName = p.DealUnit.UnitName,     
                        unitDecimalPlaces=p.DealUnit.DecimalPlaces,
                        //DealQty = b.BalanceDocQty * p.UnitConversionMultiplier,
                        DiscountPer = tab.DiscountPer
                    }).FirstOrDefault();

            if (temp.UnitConversionMultiplier != 0)
            {
                temp.DealQty = temp.DocQty * temp.UnitConversionMultiplier;                
            }
            else if(temp.ReceiptBalDocQty== temp.ReceiptBalQty)
            {
                temp.DealQty = temp.ReceiptBalDealQty;
            }
            else
            {
                temp.DealQty = temp.ReceiptBalDocQty * (temp.ReceiptBalDealQty / temp.ReceiptBalQty);
            }
            temp.Amount = temp.Rate * temp.DealQty;

            var PurchaseOrderLineId = (from p in db.PurchaseGoodsReceiptLine
                                  where p.PurchaseGoodsReceiptLineId == id
                                  join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId
                                  select new { LineId = p.PurchaseOrderLineId, HeaderId = t.PurchaseOrderHeaderId }).FirstOrDefault();


            if (PurchaseOrderLineId != null)
            { 
                var Charges = (from p in db.PurchaseOrderLineCharge
                               where p.LineTableId == PurchaseOrderLineId.LineId
                               join t in db.Charge on p.ChargeId equals t.ChargeId
                               select new LineCharges
                               {
                                   ChargeCode = t.ChargeCode,
                                   Rate = p.Rate,
                               }).ToList();

                var HeaderCharges = (from p in db.PurchaseOrderHeaderCharges
                                     where p.HeaderTableId == PurchaseOrderLineId.HeaderId
                                     join t in db.Charge on p.ChargeId equals t.ChargeId
                                     select new HeaderCharges
                                     {
                                         ChargeCode = t.ChargeCode,
                                         Rate = p.Rate,
                                     }).ToList();

                temp.RHeaderCharges = HeaderCharges;
                temp.RLineCharges= Charges;
            }
            return temp;
        }

        public PurchaseGoodsReceiptLineViewModel GetPurchaseIndentDetailBalance(int id)
        {
            return (from b in db.ViewPurchaseIndentBalance
                    join p in db.PurchaseIndentLine on b.PurchaseIndentLineId equals p.PurchaseIndentLineId                   
                    where b.PurchaseIndentLineId == id

                    select new PurchaseGoodsReceiptLineViewModel
                    {
                        Qty = b.BalanceQty,
                        Remark = p.Remark,
                        //Rate = tab.Rate,
                        //Amount = tab.Rate * (b.BalanceQty * p.UnitConversionMultiplier),
                        Dimension1Id = p.Dimension1Id,
                        Dimension1Name = p.Dimension1.Dimension1Name,
                        Dimension2Id = p.Dimension2Id,
                        Dimension2Name = p.Dimension2.Dimension2Name,
                        Specification = p.Specification,
                        //LotNo = p.LotNo,
                        UnitConversionMultiplier = 1,
                        UnitId = p.Product.UnitId,
                        UnitName = p.Product.Unit.UnitName,
                        DealUnitId = p.Product.UnitId,
                        DealUnitName = p.Product.Unit.UnitName,
                        //DealQty = b.BalanceQty * p.UnitConversionMultiplier,
                    }).FirstOrDefault();

        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseGoodsReceiptLine>().Delete(id);
        }

        public void Delete(PurchaseGoodsReceiptLine pt)
        {
            _unitOfWork.Repository<PurchaseGoodsReceiptLine>().Delete(pt);
        }

        public void Update(PurchaseGoodsReceiptLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseGoodsReceiptLine>().Update(pt);
        }
        public IEnumerable<PurchaseGoodsReceiptLineViewModel> GetLineListForIndex(int headerId)
        {
            var pt = (from p in db.PurchaseGoodsReceiptLine
                      join t in db.Dimension1 on p.Dimension1Id equals t.Dimension1Id into table
                      from dim1 in table.DefaultIfEmpty()
                      join t1 in db.Dimension2 on p.Dimension2Id equals t1.Dimension2Id into table2
                      from dim2 in table2.DefaultIfEmpty()
                      join t3 in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t3.PurchaseOrderLineId into table3 from tab3 in table3.DefaultIfEmpty()
                      join t4 in db.PurchaseOrderHeader on tab3.PurchaseOrderHeaderId equals t4.PurchaseOrderHeaderId into table4 from tab4 in table4.DefaultIfEmpty()
                      where p.PurchaseGoodsReceiptHeaderId == headerId
                      orderby p.Sr
                      select new PurchaseGoodsReceiptLineViewModel
                      {
                          PurchaseOrderDocNo = tab4.DocNo,
                          PurchaseGoodsReceiptLineId = p.PurchaseGoodsReceiptLineId,
                          ProductUidName = p.ProductUid.ProductUidName,
                          PurchaseOrderLineId=tab3.PurchaseOrderLineId,
                          OrderDocTypeId = tab4.DocTypeId,
                          OrderHeaderId = tab4.PurchaseOrderHeaderId,
                          ProductId = p.ProductId,
                          ProductName = p.Product.ProductName,
                          Specification = p.Specification,
                          Dimension1Name = dim1.Dimension1Name,
                          Dimension2Name = dim2.Dimension2Name,
                          Qty = p.DocQty,
                          unitDecimalPlaces = p.Product.Unit.DecimalPlaces,
                          UnitName = p.Product.Unit.UnitName,
                          DealunitDecimalPlaces = p.DealUnit.DecimalPlaces,
                          DealQty = p.DealQty,
                          DealUnitName = p.DealUnit.UnitName,
                          Remark = p.Remark,
                          LotNo=p.LotNo,
                          StockId = p.StockId,

                      }
                        );

            return pt;


        }
        public IEnumerable<PurchaseGoodsReceiptLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseGoodsReceiptLine>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PurchaseGoodsReceiptLine> GetPurchaseGoodsReceiptLineList()
        {
            var pt = _unitOfWork.Repository<PurchaseGoodsReceiptLine>().Query().Get();

            return pt;
        }

        public PurchaseGoodsReceiptLine Add(PurchaseGoodsReceiptLine pt)
        {
            _unitOfWork.Repository<PurchaseGoodsReceiptLine>().Insert(pt);
            return pt;
        }


        public IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseOrderHelpList(int Id, string term)
        {

            var GoodsReceipt = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(GoodsReceipt.DocTypeId, GoodsReceipt.DivisionId, GoodsReceipt.SiteId);

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

            var list = (from p in db.ViewPurchaseOrderBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseOrderNo.ToLower().Contains(term.ToLower())) && p.SupplierId == GoodsReceipt.SupplierId && p.BalanceQty > 0
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

        public IEnumerable<ComboBoxList> GetProductsHelpList(string term)
        {
            var list = (from p in db.Product
                        where p.ProductName.ToLower().Contains(term.ToLower())
                        select new ComboBoxList
                        {
                            Id = p.ProductId,
                            PropFirst = p.ProductName,
                        }

                          ).Take(20);
            return list.ToList();
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.PurchaseGoodsReceiptLine
                       where p.PurchaseGoodsReceiptHeaderId == id
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


        public Task<IEquatable<PurchaseGoodsReceiptLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseGoodsReceiptLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

