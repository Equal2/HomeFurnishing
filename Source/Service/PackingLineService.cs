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
using System.Data.SqlClient;
using System.Configuration;


namespace Service
{
    public interface IPackingLineService : IDisposable
    {
        PackingLine Create(PackingLine s);
        void Delete(int id);
        void Delete(PackingLine s);
        void Update(PackingLine s);
        PackingLine Find(int id);

        void DeleteForPackingHeaderId(int PackingHeaderid);
        string FGetRandonNoForLabel();

        IQueryable<PackingLine> GetPackingLineList();
        IQueryable<PackingLine> GetPackingLineForHeaderId(int PackingHeaderId);
        PackingLine GetPackingLineForLineId(int PackingLineId);
        PackingLineViewModel GetPackingLineViewModelForLineId(int PackingLineId);
        PackingLineViewModel GetPackingLineWithExtendedViewModelForLineId(int PackingLineId);
        //IQueryable<PackingLineViewModel> GetPackingLineViewModelForHeaderId(int PackingHeaderId);
        IEnumerable<PackingLineViewModel> GetPackingLineViewModelForHeaderId(int PackingHeaderId);
        //PendingOrderListForPacking FGetFifoSaleOrder(int ProductId, int BuyerId);
        ProductAreaDetail FGetProductArea(int ProductId);
        PackingBaleNo FGetNewPackingBaleNo(int PackingHeaderId, int? ProductInvoiceGroupId, int? SaleOrderLineId, int BaleNoPatternId, decimal DealQty);
        PackingBaleNo FGetNewPackingBaleNo_WithProductId(int PackingHeaderId, int? ProductInvoiceGroupId, int? SaleOrderLineId, int BaleNoPatternId, decimal DealQty, int? ProductId);
        IQueryable<ComboBoxResult> GetCustomProductsWithBuyerSku_ForAllProducts(int Id, string term, Boolean isShowAllProducts);
        Decimal FGetStockForPacking(int ProductId, int SiteId, int PackingLineId);
        Decimal FGetQCStockForPacking(int ProductId, int SiteId, Decimal StockInHand);
        IEnumerable<PendingOrderListForPacking> FGetPendingOrderListForPacking(int ProductId, int BuyerId, int PackingLineId);

        IEnumerable<PendingDeliveryOrderListForPacking> FGetPendingDeliveryOrderListForPacking(int ProductId, int BuyerId, int PackingLineId);
        Decimal FGetPendingOrderQtyForPacking(int SaleOrderLineId, int PackingLineId);
        Decimal FGetPendingOrderQtyForDispatch(int SaleOrderilneId);

        Decimal FGetPendingOrderQtyForPacking(int SaleOrderilneId);

        Decimal FGetPendingDeliveryOrderQtyForPacking(int SaleDeliveryOrderLineId, int PackingLineId);
        Decimal FGetPendingDeliveryOrderQtyForDispatch(int SaleDeliveryOrderilneId);
        Decimal FGetPendingDeliveryOrderQtyForPacking(int SaleDeliveryOrderilneId);

        bool FSaleOrderProductMatchWithPacking(int SaleOrderLineId, int ProductId);
        IQueryable<ComboBoxResult> GetCustomProductsWithBuyerSku(int Id, string term);
        IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term);

        IEnumerable<PendingOrderListForPacking> FGetPendingOrderListForPackingForProductUid(int ProductUidId, int BuyerId);

        IEnumerable<PackingLineViewModel> GetPackingLineListForIndex(int PackingHeaderId);
        IEnumerable<PackingLineViewModel> GetSaleOrdersForFilters(PackingFilterViewModel vm);

        IQueryable<ComboBoxResult> GetPendingProductsForPacking(int id, string term);//DocTypeId
        IQueryable<ComboBoxResult> GetSaleOrderHelpListForProduct(int filter, string term);
        IEnumerable<ComboBoxResult> GetPendingOrdersForPacking(int id, string term);
        PackingLineViewModel GetPackingLineForEdit(int id);
        IEnumerable<ComboBoxResult> GetPendingStockInForPacking(int id, int ProductId, int GodownId, int? Dimension1Id, int? Dimension2Id, string term);

        IEnumerable<ComboBoxResult> GetPendingStockInForIssue(int PackingHeaderId, int? ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, string term);
    }

    public class PackingLineService : IPackingLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public PackingLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void Dispose()
        {
        }

        public PackingLine Create(PackingLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PackingLine>().Insert(S);
            return S;
        }

        public PackingLine Find(int id)
        {
            return _unitOfWork.Repository<PackingLine>().Find(id);
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PackingLine>().Delete(id);
        }

        public void Delete(PackingLine s)
        {
            _unitOfWork.Repository<PackingLine>().Delete(s);
        }

        public void DeleteForPackingHeaderId(int PackingHeaderid)
        {
            var PackingLine = from L in db.PackingLine where L.PackingHeaderId == PackingHeaderid select new { PackingLindId = L.PackingLineId };

            foreach (var item in PackingLine)
            {
                Delete(item.PackingLindId);
            }
        }

        public void Update(PackingLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PackingLine>().Update(s);
        }

        public IQueryable<PackingLine> GetPackingLineList()
        {
            return _unitOfWork.Repository<PackingLine>().Query().Get();
        }

        public IQueryable<PackingLine> GetPackingLineForHeaderId(int PackingHeaderId)
        {
            return _unitOfWork.Repository<PackingLine>().Query().Get().Where(m => m.PackingHeaderId == PackingHeaderId);
        }

        public PackingLine GetPackingLineForLineId(int PackingLineId)
        {
            return _unitOfWork.Repository<PackingLine>().Query().Get().Where(m => m.PackingLineId == PackingLineId).FirstOrDefault();
        }

        public string FGetRandonNoForLabel()
        {
            string RandomNo = "";
            Random random = new Random();
            int i = 0;
            string StrSalt = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            for (i = 1; i <= 5; i++)
            {
                RandomNo += StrSalt[random.Next(0, StrSalt.Length)];
            }


            return RandomNo;
        }

        //public IQueryable<PackingLineViewModel> GetPackingLineViewModelForHeaderId(int PackingHeaderId)
        //{
        //    IQueryable<PackingLineViewModel> packinglineviewmodel = from L in db.PackingLine
        //            join P in db.Product on L.ProductId equals P.ProductId into ProductTable from Producttab in ProductTable.DefaultIfEmpty()
        //            join S in db.SaleOrderLine on L.SaleOrderLineId equals S.SaleOrderLineId into SaleOrderLineTable from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
        //            join So in db.SaleOrderHeader on SaleOrderLineTab.SaleOrderHeaderId equals So.SaleOrderHeaderId into SaleOrderHeaderTable
        //            from SaleOrderHeaderTab in SaleOrderHeaderTable.DefaultIfEmpty()
        //            join Du in db.Units on L.DealUnitId equals Du.UnitId into DeliveryUnitTable from DeliveryUnitTab in DeliveryUnitTable.DefaultIfEmpty()
        //            join Pu in db.ProductUid on L.ProductUidId equals Pu.ProductUIDId into ProductUidTable
        //            from ProductUidTab in ProductUidTable.DefaultIfEmpty()
        //            orderby L.PackingLineId
        //            where L.PackingHeaderId == PackingHeaderId
        //            select new PackingLineViewModel
        //            {
        //                PackingHeaderId = L.PackingHeaderId,
        //                PackingLineId = L.PackingLineId,
        //                ProductUidId = L.ProductUidId,
        //                ProductUidName = ProductUidTab.ProductUidName,
        //                ProductId = L.ProductId,
        //                ProductName = Producttab.ProductName,
        //                Qty = L.Qty,
        //                SaleOrderLineId = L.SaleOrderLineId,
        //                SaleOrderNo = SaleOrderHeaderTab.DocNo,
        //                DealUnitId = L.DealUnitId,
        //                DealUnitName = DeliveryUnitTab.UnitName,
        //                DealQty = L.DealQty,
        //                BaleNo = L.BaleNo,
        //                GrossWeight = L.GrossWeight,
        //                NetWeight = L.NetWeight,
        //                Remark = L.Remark
        //            };


        //    return packinglineviewmodel;
        //}

        public IEnumerable<PackingLineViewModel> GetPackingLineViewModelForHeaderId(int PackingHeaderId)
        {
            IEnumerable<PackingLineViewModel> packinglineviewmodel = (from L in db.PackingLine
                                                                      join P in db.Product on L.ProductId equals P.ProductId into ProductTable
                                                                      from Producttab in ProductTable.DefaultIfEmpty()
                                                                      join S in db.SaleOrderLine on L.SaleOrderLineId equals S.SaleOrderLineId into SaleOrderLineTable
                                                                      from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
                                                                      join So in db.SaleOrderHeader on SaleOrderLineTab.SaleOrderHeaderId equals So.SaleOrderHeaderId into SaleOrderHeaderTable
                                                                      from SaleOrderHeaderTab in SaleOrderHeaderTable.DefaultIfEmpty()
                                                                      join Du in db.Units on L.DealUnitId equals Du.UnitId into DeliveryUnitTable
                                                                      from DeliveryUnitTab in DeliveryUnitTable.DefaultIfEmpty()
                                                                      join Pu in db.ProductUid on L.ProductUidId equals Pu.ProductUIDId into ProductUidTable
                                                                      from ProductUidTab in ProductUidTable.DefaultIfEmpty()
                                                                      orderby L.PackingLineId
                                                                      where L.PackingHeaderId == PackingHeaderId
                                                                      select new PackingLineViewModel
                                                                      {
                                                                          PackingHeaderId = L.PackingHeaderId,
                                                                          PackingLineId = L.PackingLineId,
                                                                          ProductUidId = L.ProductUidId,
                                                                          ProductUidName = ProductUidTab.ProductUidName,
                                                                          ProductId = L.ProductId,
                                                                          ProductName = Producttab.ProductName,
                                                                          Qty = L.Qty,
                                                                          SaleOrderLineId = L.SaleOrderLineId,
                                                                          SaleOrderNo = SaleOrderHeaderTab.DocNo,
                                                                          DealUnitId = L.DealUnitId,
                                                                          DealUnitName = DeliveryUnitTab.UnitName,
                                                                          DealQty = L.DealQty,
                                                                          BaleNo = L.BaleNo,
                                                                          GrossWeight = L.GrossWeight,
                                                                          NetWeight = L.NetWeight,
                                                                          Remark = L.Remark
                                                                      }).ToList();

            double x = 0;
            var p = packinglineviewmodel.OrderBy(sx => double.TryParse(sx.BaleNo, out x) ? x : 0);


            return p;
        }

        public PackingBaleNo FGetNewPackingBaleNo_WithProductId(int PackingHeaderId, int? ProductInvoiceGroupId, int? SaleOrderLineId, int BaleNoPatternId, decimal DealQty, int? ProductId)
        {
            int x = 0;
            int MaxBaleNo = 0;

            if (BaleNoPatternId == (int)(BaleNoPatternConstants.SaleOrder))
            {
                int SaleOrderHeaderId = (from L in db.SaleOrderLine where L.SaleOrderLineId == SaleOrderLineId select new { SaleOrderHeaderId = L.SaleOrderHeaderId }).FirstOrDefault().SaleOrderHeaderId;

                var BaleNoList = (from L in db.PackingLine
                                  join Sl in db.SaleOrderLine on L.SaleOrderLineId equals Sl.SaleOrderLineId into SaleOrderLineTable
                                  from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
                                  where L.PackingHeaderId == PackingHeaderId && SaleOrderLineTab.SaleOrderHeaderId == SaleOrderHeaderId
                                  select new
                                  {
                                      BaleNo = L.BaleNo
                                  }).ToList();

                if (BaleNoList != null && BaleNoList.Count != 0)
                {
                    MaxBaleNo = BaleNoList.Select(sx => int.TryParse(sx.BaleNo, out x) ? x : 0).Max();
                }
            }
            else if (BaleNoPatternId == (int)(BaleNoPatternConstants.SaleOrderSize))
            {
                int SaleOrderHeaderId = (from L in db.SaleOrderLine where L.SaleOrderLineId == SaleOrderLineId select new { SaleOrderHeaderId = L.SaleOrderHeaderId }).FirstOrDefault().SaleOrderHeaderId;

                var BaleNoList = (from L in db.PackingLine
                                  join Sl in db.SaleOrderLine on L.SaleOrderLineId equals Sl.SaleOrderLineId into SaleOrderLineTable
                                  from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
                                  where L.PackingHeaderId == PackingHeaderId && SaleOrderLineTab.SaleOrderHeaderId == SaleOrderHeaderId && L.DealQty == DealQty
                                  select new
                                  {
                                      BaleNo = L.BaleNo
                                  }).ToList();

                if (BaleNoList != null && BaleNoList.Count != 0)
                {
                    MaxBaleNo = BaleNoList.Select(sx => int.TryParse(sx.BaleNo, out x) ? x : 0).Max();
                }
            }

            else if (BaleNoPatternId == (int)(BaleNoPatternConstants.SmallSizes))
            {
                var company = (from C in db.Company
                               where C.CompanyId == 1
                               select new
                               {
                                   CompanyName = C.CompanyName
                               }).ToList();

                if (company.FirstOrDefault().CompanyName == "M/S BHADOHI CARPETS")
                {
                    var BaleNoList = (from L in db.PackingLine
                                      join P in db.FinishedProduct on L.ProductId equals P.ProductId into ProductTable
                                      from ProductTab in ProductTable.DefaultIfEmpty()
                                      where L.PackingHeaderId == PackingHeaderId
                                      select new
                                      {
                                          BaleNo = L.BaleNo
                                      }).ToList();

                    if (BaleNoList != null && BaleNoList.Count != 0)
                    {
                        MaxBaleNo = BaleNoList.Select(sx => int.TryParse(sx.BaleNo, out x) ? x : 0).Max();

                        var Area = (from P in db.ViewRugSize
                                    where P.ProductId == ProductId
                                    select new
                                    {
                                        Area = P.StandardSizeArea
                                    }).ToList();

                        if ((Decimal)Area.FirstOrDefault().Area <= 6)
                        {
                            MaxBaleNo -= 1;
                        }


                    }
                }
                else
                {
                    var BaleNoList = (from L in db.PackingLine
                                      join P in db.FinishedProduct on L.ProductId equals P.ProductId into ProductTable
                                      from ProductTab in ProductTable.DefaultIfEmpty()
                                      where L.PackingHeaderId == PackingHeaderId && ProductTab.ProductInvoiceGroupId == ProductInvoiceGroupId && L.DealQty == DealQty
                                      select new
                                      {
                                          BaleNo = L.BaleNo
                                      }).ToList();

                    if (BaleNoList != null && BaleNoList.Count != 0)
                    {
                        MaxBaleNo = BaleNoList.Select(sx => int.TryParse(sx.BaleNo, out x) ? x : 0).Max();
                        MaxBaleNo -= 1;
                    }
                }


            }
            else
            {
                var BaleNoList = (from L in db.PackingLine
                                  join P in db.FinishedProduct on L.ProductId equals P.ProductId into ProductTable
                                  from ProductTab in ProductTable.DefaultIfEmpty()
                                  where L.PackingHeaderId == PackingHeaderId && ProductTab.ProductInvoiceGroupId == ProductInvoiceGroupId
                                  select new
                                  {
                                      BaleNo = L.BaleNo
                                  }).ToList();

                if (BaleNoList != null && BaleNoList.Count != 0)
                {
                    MaxBaleNo = BaleNoList.Select(sx => int.TryParse(sx.BaleNo, out x) ? x : 0).Max();
                }

            }


            PackingBaleNo packingbaleno = new PackingBaleNo();
            packingbaleno.PackingHeaderId = PackingHeaderId;
            packingbaleno.NewBaleNo = MaxBaleNo + 1;

            return packingbaleno;
        }


        public IQueryable<ComboBoxResult> GetCustomProductsWithBuyerSku_ForAllProducts(int Id, string term, Boolean isShowAllProducts)
        {

            var PackingHeader = new PackingHeaderService(_unitOfWork).Find(Id);

            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(PackingHeader.DocTypeId, PackingHeader.DivisionId, PackingHeader.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] Products = null;
            if (!string.IsNullOrEmpty(settings.filterProducts)) { Products = settings.filterProducts.Split(",".ToCharArray()); }
            else { Products = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }


            if (isShowAllProducts == true)
                return (from pb in db.Product
                        join Pt in db.ViewProductBuyer on new { A = pb.ProductId, B = PackingHeader.BuyerId } equals new { A = Pt.ProductId ?? 0, B = Pt.BuyerId } into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(pb.ProductGroup.ProductTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProducts) ? 1 == 1 : Products.Contains(pb.ProductId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(pb.ProductGroupId.ToString()))
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : pb.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : ProductTab.ProductName.ToLower().Contains(term.ToLower()))
                        orderby pb.ProductName
                        select new ComboBoxResult
                        {
                            id = pb.ProductId.ToString(),
                            text = pb.ProductName,
                            AProp1 = ProductTab.ProductName ?? ""
                        });
            else
                return (from pb in db.ViewProductBuyer
                        join Pt in db.Product on pb.ProductId equals Pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        where pb.BuyerId == PackingHeader.BuyerId
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : pb.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : ProductTab.ProductName.ToLower().Contains(term.ToLower()))
                        orderby pb.ProductName
                        select new ComboBoxResult
                        {
                            id = pb.ProductId.ToString(),
                            text = ProductTab.ProductName,
                            AProp1 = pb.ProductName
                        });
        }


        public PackingLineViewModel GetPackingLineViewModelForLineId(int PackingLineId)
        {
            return (from L in db.PackingLine
                    join P in db.FinishedProduct on L.ProductId equals P.ProductId into ProductTable
                    from ProductTab in ProductTable.DefaultIfEmpty()
                    join PL in db.PackingLineExtended on L.PackingLineId equals PL.PackingLineId into PLTable
                    from PLTab in PLTable.DefaultIfEmpty()
                    join Pig in db.ProductInvoiceGroup on ProductTab.ProductInvoiceGroupId equals Pig.ProductInvoiceGroupId into ProductInvoiceGroupTable
                    from ProductInvoiceGroupTab in ProductInvoiceGroupTable.DefaultIfEmpty()
                    join S in db.SaleOrderLine on L.SaleOrderLineId equals S.SaleOrderLineId into SaleOrderLineTable
                    from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
                    join So in db.SaleOrderHeader on SaleOrderLineTab.SaleOrderHeaderId equals So.SaleOrderHeaderId into SaleOrderHeaderTable
                    from SaleOrderHeaderTab in SaleOrderHeaderTable.DefaultIfEmpty()
                    join Du in db.Units on L.DealUnitId equals Du.UnitId into DeliveryUnitTable
                    from DeliveryUnitTab in DeliveryUnitTable.DefaultIfEmpty()
                    join Du1 in db.Units on DeliveryUnitTab.DimensionUnitId equals Du1.UnitId into Du1Table
                    from Du1Tab in Du1Table.DefaultIfEmpty()
                    join Pu in db.ProductUid on L.ProductUidId equals Pu.ProductUIDId into ProductUidTable
                    from ProductUidTab in ProductUidTable.DefaultIfEmpty()
                    orderby L.PackingLineId
                    where L.PackingLineId == PackingLineId
                    select new PackingLineViewModel
                    {
                        PackingHeaderId = L.PackingHeaderId,
                        PackingLineId = L.PackingLineId,
                        ProductUidId = L.ProductUidId,
                        ProductUidName = ProductUidTab.ProductUidName,
                        ProductId = L.ProductId,
                        ProductName = ProductTab.ProductName,
                        LotNo  = L.LotNo,
                        ProductInvoiceGroupId = ProductTab.ProductInvoiceGroupId,
                        ProductInvoiceGroupName = ProductInvoiceGroupTab.ProductInvoiceGroupName,
                        Qty = L.Qty,
                        SaleOrderLineId = L.SaleOrderLineId,
                        SaleOrderNo = SaleOrderHeaderTab.DocNo,
                        SaleDeliveryOrderLineId = L.SaleDeliveryOrderLineId,
                        SaleDeliveryOrderNo = L.SaleDeliveryOrderLine.SaleDeliveryOrderHeader.DocNo,
                        DealUnitId = L.DealUnitId,
                        DealUnitName = DeliveryUnitTab.UnitName,
                        DimensionUnitDecimalPlaces= Du1Tab.DecimalPlaces,
                        DealQty = L.DealQty,
                        BaleNo = L.BaleNo,
                        Length =PLTab.Length,
                        Width  = PLTab.Width,
                        Height = PLTab.Height,
                        GrossWeight = L.GrossWeight,
                        NetWeight = L.NetWeight,
                        Remark = L.Remark,
                        SealNo = L.SealNo,
                        RateRemark = L.RateRemark,
                        ImageFolderName = ProductTab.ImageFolderName,
                        ImageFileName = ProductTab.ImageFileName,
                        CreatedBy = L.CreatedBy,
                        CreatedDate = L.CreatedDate,
                        StockInId = L.StockInId,
                        StockInNo = L.StockIn.StockHeader.DocNo
                    }).FirstOrDefault();
        }


        public PackingLineViewModel GetPackingLineWithExtendedViewModelForLineId(int PackingLineId)
        {
            return (from L in db.PackingLine
                    join P in db.FinishedProduct on L.ProductId equals P.ProductId into ProductTable
                    from ProductTab in ProductTable.DefaultIfEmpty()
                    join Pig in db.ProductInvoiceGroup on ProductTab.ProductInvoiceGroupId equals Pig.ProductInvoiceGroupId into ProductInvoiceGroupTable
                    from ProductInvoiceGroupTab in ProductInvoiceGroupTable.DefaultIfEmpty()
                    join S in db.SaleOrderLine on L.SaleOrderLineId equals S.SaleOrderLineId into SaleOrderLineTable
                    from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
                    join So in db.SaleOrderHeader on SaleOrderLineTab.SaleOrderHeaderId equals So.SaleOrderHeaderId into SaleOrderHeaderTable
                    from SaleOrderHeaderTab in SaleOrderHeaderTable.DefaultIfEmpty()
                    join Du in db.Units on L.DealUnitId equals Du.UnitId into DeliveryUnitTable
                    from DeliveryUnitTab in DeliveryUnitTable.DefaultIfEmpty()
                    join Pu in db.ProductUid on L.ProductUidId equals Pu.ProductUIDId into ProductUidTable
                    from ProductUidTab in ProductUidTable.DefaultIfEmpty()
                    join Le in db.PackingLineExtended on L.PackingLineId equals Le.PackingLineId into PackingLineExtendedTable
                    from PackingLineExtendedTab in PackingLineExtendedTable.DefaultIfEmpty()
                    orderby L.PackingLineId
                    where L.PackingLineId == PackingLineId
                    select new PackingLineViewModel
                    {
                        PackingHeaderId = L.PackingHeaderId,
                        PackingLineId = L.PackingLineId,
                        ProductUidId = L.ProductUidId,
                        ProductUidName = ProductUidTab.ProductUidName,
                        ProductId = L.ProductId,
                        ProductName = ProductTab.ProductName,
                        ProductInvoiceGroupId = ProductTab.ProductInvoiceGroupId,
                        ProductInvoiceGroupName = ProductInvoiceGroupTab.ProductInvoiceGroupName,
                        Qty = L.Qty,
                        SaleOrderLineId = L.SaleOrderLineId,
                        SaleOrderNo = SaleOrderHeaderTab.DocNo,
                        SaleDeliveryOrderLineId = L.SaleDeliveryOrderLineId,
                        SaleDeliveryOrderNo = L.SaleDeliveryOrderLine.SaleDeliveryOrderHeader.DocNo,
                        DealUnitId = L.DealUnitId,
                        DealUnitName = DeliveryUnitTab.UnitName,
                        DealQty = L.DealQty,
                        BaleNo = L.BaleNo,
                        GrossWeight = L.GrossWeight,
                        NetWeight = L.NetWeight,
                        Remark = L.Remark,
                        Length = PackingLineExtendedTab.Length,
                        Width = PackingLineExtendedTab.Width,
                        Height = PackingLineExtendedTab.Height,
                        ImageFolderName = ProductTab.ImageFolderName,
                        ImageFileName = ProductTab.ImageFileName,
                        CreatedBy = L.CreatedBy,
                        CreatedDate = L.CreatedDate
                    }).FirstOrDefault();
        }

        //public ProductUidDetail FGetProductUidDetail(string ProductUidNo)
        //{
        //    ProductUidDetail UidDetail = (from Pu in db.ProductUid
        //            join P in db.Products on Pu.ProductId equals P.ProductId into ProductTable
        //            from Producttab in ProductTable.DefaultIfEmpty()
        //            where Pu.ProductUidName == ProductUidNo
        //            select new ProductUidDetail
        //            {
        //                ProductUidId = Pu.ProductUIDId,
        //                ProductId = Pu.ProductId,
        //                ProductName = Producttab.ProductName
        //            }).FirstOrDefault();

        //    return UidDetail;
        //}

        public PendingOrderListForPacking FGetFifoSaleOrder(int ProductId, int BuyerId)
        {
            var Packing = from L in db.PackingLine
                          where L.ProductId == ProductId
                          group new { L } by new { L.SaleOrderLineId } into Result
                          select new
                          {
                              SaleOrderLineId = Result.Key.SaleOrderLineId,
                              PackedQty = Result.Sum(i => i.L.Qty)
                          };

            PendingOrderListForPacking FifoSaleOrderLine = (from L in db.SaleOrderLine
                                                            join H in db.SaleOrderHeader on L.SaleOrderHeaderId equals H.SaleOrderHeaderId into SaleOrderHeaderTable
                                                            from SaleOrderHeaderTab in SaleOrderHeaderTable.DefaultIfEmpty()
                                                            join VPack in Packing on L.SaleOrderLineId equals VPack.SaleOrderLineId into VPackTable
                                                            from VPackTab in VPackTable.DefaultIfEmpty()
                                                            where SaleOrderHeaderTab.SaleToBuyerId == BuyerId && L.ProductId == ProductId
                                                            group new { L, SaleOrderHeaderTab, VPackTab } by new { L.SaleOrderLineId } into Result
                                                            where ((Decimal?)Result.Sum(i => i.L.Qty) ?? 0) - ((Decimal?)Result.Sum(i => i.VPackTab.PackedQty) ?? 0) > 0
                                                            orderby Result.Max(i => i.SaleOrderHeaderTab.Priority) descending, Result.Max(i => i.SaleOrderHeaderTab.DocDate)
                                                            select new PendingOrderListForPacking
                                                            {
                                                                SaleOrderLineId = Result.Key.SaleOrderLineId,
                                                                SaleOrderNo = Result.Max(i => i.SaleOrderHeaderTab.DocNo)
                                                            }).Take(1).FirstOrDefault();

            return FifoSaleOrderLine;
        }




        //public IEnumerable<FiFoSaleOrderForProduct> FGetFifoSaleOrderList(int ProductId, int BuyerId)
        //{
        //    var Packing = from L in db.PackingLine
        //                  where L.ProductId == ProductId 
        //                  group new { L } by new { L.SaleOrderLineId } into Result
        //                  select new
        //                  {
        //                      SaleOrderLineId = Result.Key.SaleOrderLineId,
        //                      PackedQty = Result.Sum(i => i.L.Qty)
        //                  };

        //    IQueryable<FiFoSaleOrderForProduct> FifoSaleOrderLineList = (from L in db.SaleOrderLine
        //                                                                 join H in db.SaleOrderHeader on L.SaleOrderHeaderId equals H.SaleOrderHeaderId into SaleOrderHeaderTable
        //                                                                 from SaleOrderHeaderTab in SaleOrderHeaderTable.DefaultIfEmpty()
        //                                                                 join VPack in Packing on L.SaleOrderLineId equals VPack.SaleOrderLineId into VPackTable
        //                                                                 from VPackTab in VPackTable.DefaultIfEmpty()
        //                                                                 where SaleOrderHeaderTab.SaleToBuyerId == BuyerId && L.ProductId == ProductId
        //                                                                 group new { L, SaleOrderHeaderTab, VPackTab } by new { L.SaleOrderLineId } into Result
        //                                                                 where ((Decimal?)Result.Sum(i => i.L.Qty) ?? 0) - ((Decimal?)Result.Sum(i => i.VPackTab.PackedQty) ?? 0) > 0
        //                                                                 orderby Result.Max(i => i.SaleOrderHeaderTab.Priority) descending, Result.Max(i => i.SaleOrderHeaderTab.DocDate) descending
        //                                                                 select new FiFoSaleOrderForProduct
        //                                                                 {
        //                                                                     SaleOrderLineId = Result.Key.SaleOrderLineId,
        //                                                                     SaleOrderNo = Result.Max(i => i.SaleOrderHeaderTab.DocNo)
        //                                                                 });


        //    return FifoSaleOrderLineList;
        //}




        public ProductAreaDetail FGetProductArea(int ProductId)
        {

            int a = (int)ProductSizeTypeConstants.StandardSize;
            ProductAreaDetail productarea = (from Ps in db.ProductSize
                                             join S in db.Size on Ps.SizeId equals S.SizeId into SizeTable
                                             from SizeTab in SizeTable.DefaultIfEmpty()
                                             where Ps.ProductId == ProductId && Ps.ProductSizeTypeId == ((int)ProductSizeTypeConstants.StandardSize)
                                             select new ProductAreaDetail
                                             {
                                                 ProductId = Ps.ProductId,
                                                 ProductArea = SizeTab.Area
                                             }).FirstOrDefault();
            return productarea;
        }







        public PackingBaleNo FGetNewPackingBaleNo(int PackingHeaderId, int? ProductInvoiceGroupId, int? SaleOrderLineId, int BaleNoPatternId, decimal DealQty)
        {
            int x = 0;
            int MaxBaleNo = 0;

            if (BaleNoPatternId == (int)(BaleNoPatternConstants.SaleOrder))
            {
                int SaleOrderHeaderId = (from L in db.SaleOrderLine where L.SaleOrderLineId == SaleOrderLineId select new { SaleOrderHeaderId = L.SaleOrderHeaderId }).FirstOrDefault().SaleOrderHeaderId;

                var BaleNoList = (from L in db.PackingLine
                                  join Sl in db.SaleOrderLine on L.SaleOrderLineId equals Sl.SaleOrderLineId into SaleOrderLineTable
                                  from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
                                  where L.PackingHeaderId == PackingHeaderId && SaleOrderLineTab.SaleOrderHeaderId == SaleOrderHeaderId
                                  select new
                                  {
                                      BaleNo = L.BaleNo
                                  }).ToList();

                if (BaleNoList != null && BaleNoList.Count != 0)
                {
                    MaxBaleNo = BaleNoList.Select(sx => int.TryParse(sx.BaleNo, out x) ? x : 0).Max();
                }
            }
            else if (BaleNoPatternId == (int)(BaleNoPatternConstants.SaleOrderSize ))
            {
                int SaleOrderHeaderId = (from L in db.SaleOrderLine where L.SaleOrderLineId == SaleOrderLineId select new { SaleOrderHeaderId = L.SaleOrderHeaderId }).FirstOrDefault().SaleOrderHeaderId;

                var BaleNoList = (from L in db.PackingLine
                                  join Sl in db.SaleOrderLine on L.SaleOrderLineId equals Sl.SaleOrderLineId into SaleOrderLineTable
                                  from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
                                  where L.PackingHeaderId == PackingHeaderId && SaleOrderLineTab.SaleOrderHeaderId == SaleOrderHeaderId && L.DealQty == DealQty
                                  select new
                                  {
                                      BaleNo = L.BaleNo
                                  }).ToList();

                if (BaleNoList != null && BaleNoList.Count != 0)
                {
                    MaxBaleNo = BaleNoList.Select(sx => int.TryParse(sx.BaleNo, out x) ? x : 0).Max();
                }
            }

            else if (BaleNoPatternId == (int)(BaleNoPatternConstants.SmallSizes))
            {               
                var BaleNoList = (from L in db.PackingLine
                                  join P in db.FinishedProduct on L.ProductId equals P.ProductId into ProductTable
                                  from ProductTab in ProductTable.DefaultIfEmpty()
                                  where L.PackingHeaderId == PackingHeaderId && ProductTab.ProductInvoiceGroupId == ProductInvoiceGroupId && L.DealQty==DealQty
                                  select new
                                  {
                                      BaleNo = L.BaleNo
                                  }).ToList();

                if (BaleNoList != null && BaleNoList.Count != 0)
                {
                    MaxBaleNo = BaleNoList.Select(sx => int.TryParse(sx.BaleNo, out x) ? x : 0).Max();
                    MaxBaleNo -= 1;
                }
            }
            else
            {
                var BaleNoList = (from L in db.PackingLine
                                  join P in db.FinishedProduct on L.ProductId equals P.ProductId into ProductTable
                                  from ProductTab in ProductTable.DefaultIfEmpty()
                                  where L.PackingHeaderId == PackingHeaderId && ProductTab.ProductInvoiceGroupId == ProductInvoiceGroupId
                                  select new
                                  {
                                      BaleNo = L.BaleNo
                                  }).ToList();

                if (BaleNoList != null && BaleNoList.Count != 0)
                {
                    MaxBaleNo = BaleNoList.Select(sx => int.TryParse(sx.BaleNo, out x) ? x : 0).Max();
                }

            }


            PackingBaleNo packingbaleno = new PackingBaleNo();
            packingbaleno.PackingHeaderId = PackingHeaderId;
            packingbaleno.NewBaleNo = MaxBaleNo + 1;

            return packingbaleno;
        }

        public IEnumerable<PendingOrderListForPacking> FGetPendingOrderListForPacking(int ProductId, int BuyerId, int PackingLineId)
        {
            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterBuyerId = new SqlParameter("@BuyerId", BuyerId);
            SqlParameter SqlParameterPackingLineId = new SqlParameter("@PackingLineId", PackingLineId);

            IEnumerable<PendingOrderListForPacking> FifoSaleOrderLineList = db.Database.SqlQuery<PendingOrderListForPacking>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetPendingOrderListForPacking @ProductId, @BuyerId, @PackingLineId", SqlParameterProductId, SqlParameterBuyerId, SqlParameterPackingLineId).ToList();

            return FifoSaleOrderLineList;
        }

        public IEnumerable<PendingOrderListForPacking> FGetPendingOrderListForPackingForProductUid(int ProductUidId, int BuyerId)
        {
            SqlParameter SqlParameterProductUidId = new SqlParameter("@ProductUidId", ProductUidId);
            SqlParameter SqlParameterBuyerId = new SqlParameter("@BuyerId", BuyerId);

            IEnumerable<PendingOrderListForPacking> FifoSaleOrderLineList = db.Database.SqlQuery<PendingOrderListForPacking>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetPendingOrderListForPackingForProductUid @ProductUidId, @BuyerId", SqlParameterProductUidId, SqlParameterBuyerId).ToList();

            return FifoSaleOrderLineList;
        }


        public IEnumerable<PendingDeliveryOrderListForPacking> FGetPendingDeliveryOrderListForPacking(int ProductId, int BuyerId, int PackingLineId)
        {
            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterBuyerId = new SqlParameter("@BuyerId", BuyerId);
            SqlParameter SqlParameterPackingLineId = new SqlParameter("@PackingLineId", PackingLineId);

            IEnumerable<PendingDeliveryOrderListForPacking> FifoDeliveryOrderLineList = db.Database.SqlQuery<PendingDeliveryOrderListForPacking>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetPendingDeliveryOrderListForPacking @ProductId, @BuyerId, @PackingLineId", SqlParameterProductId, SqlParameterBuyerId, SqlParameterPackingLineId).ToList();

            return FifoDeliveryOrderLineList;
        }

        public Decimal FGetStockForPacking(int ProductId, int SiteId, int PackingLineId)
        {
            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterPackingLineId = new SqlParameter("@PackingLineId", PackingLineId);

            StockAvailableForPacking StockAvailableForPacking = db.Database.SqlQuery<StockAvailableForPacking>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetStockForPacking @ProductId, @SiteId, @PackingLineId", SqlParameterProductId, SqlParameterSiteId, SqlParameterPackingLineId).FirstOrDefault();

            if (StockAvailableForPacking != null)
            {
                return StockAvailableForPacking.Qty;
            }
            else
            {
                return 0;
            }
        }


        public Decimal FGetQCStockForPacking(int ProductId, int SiteId, Decimal StockInHand)
        {
            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterStockIndHand = new SqlParameter("@StockInHand", StockInHand);

            StockAvailableForPacking StockAvailableForPacking = db.Database.SqlQuery<StockAvailableForPacking>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetQCStockForPacking @ProductId, @SiteId, @StockInHand", SqlParameterProductId, SqlParameterSiteId, SqlParameterStockIndHand).FirstOrDefault();

            if (StockAvailableForPacking != null)
            {
                return StockAvailableForPacking.Qty;
            }
            else
            {
                return 0;
            }
        }





        public Decimal FGetPendingOrderQtyForPacking(int SaleOrderilneId, int PackingLineId)
        {
            SqlParameter SqlParameterSaleOrderLineId = new SqlParameter("@SaleOrderLineId", SaleOrderilneId);
            SqlParameter SqlParameterPackingLineId = new SqlParameter("@PackingLineId", PackingLineId);

            PendingOrderQtyForPacking PendingOrderQtyForPacking = db.Database.SqlQuery<PendingOrderQtyForPacking>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetPendingOrderQtyForPacking @SaleOrderLineId, @PackingLineId", SqlParameterSaleOrderLineId, SqlParameterPackingLineId).FirstOrDefault();

            if (PendingOrderQtyForPacking != null)
            {
                return PendingOrderQtyForPacking.Qty;
            }
            else
            {
                return 0;
            }

        }

        public bool FSaleOrderProductMatchWithPacking(int SaleOrderLineId, int ProductId)
        {
            var temp = (from L in db.SaleOrderLine
                       where L.SaleOrderLineId == SaleOrderLineId
                       select new {
                           ProductId = L.ProductId
                       }).FirstOrDefault();

            if (temp != null)
            {
                if (temp.ProductId == ProductId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else{
                return false;
            }
        }

        public Decimal FGetPendingOrderQtyForDispatch(int SaleOrderilneId)
        {
            var temp = (from L in db.ViewSaleOrderBalance
                        where L.SaleOrderLineId == SaleOrderilneId
                        select L).FirstOrDefault();

            if (temp != null)
            {
                return temp.BalanceQty;
            }
            else
            {
                return 0;
            }
        }


        public Decimal FGetPendingOrderQtyForPacking(int SaleOrderilneId)
        {
            var temp = (from L in db.ViewSaleOrderBalanceForCancellation
                        where L.SaleOrderLineId == SaleOrderilneId
                        select L).FirstOrDefault();

            if (temp != null)
            {
                return temp.BalanceQty;
            }
            else
            {
                return 0;
            }
        }



        public Decimal FGetPendingDeliveryOrderQtyForPacking(int SaleDeliveryOrderilneId, int PackingLineId)
        {
            SqlParameter SqlParameterSaleDeliveryOrderLineId = new SqlParameter("@SaleDeliveryOrderLineId", SaleDeliveryOrderilneId);
            SqlParameter SqlParameterPackingLineId = new SqlParameter("@PackingLineId", PackingLineId);

            PendingOrderQtyForPacking PendingDeliveryOrderQtyForPacking = db.Database.SqlQuery<PendingOrderQtyForPacking>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetPendingDeliveryOrderQtyForPacking @SaleDeliveryOrderLineId, @PackingLineId", SqlParameterSaleDeliveryOrderLineId, SqlParameterPackingLineId).FirstOrDefault();

            if (PendingDeliveryOrderQtyForPacking != null)
            {
                return PendingDeliveryOrderQtyForPacking.Qty;
            }
            else
            {
                return 0;
            }

        }

        public Decimal FGetPendingDeliveryOrderQtyForDispatch(int SaleDeliveryOrderilneId)
        {
            var temp = (from L in db.ViewSaleDeliveryOrderBalance
                        where L.SaleDeliveryOrderLineId == SaleDeliveryOrderilneId
                        select L).FirstOrDefault();

            if (temp != null)
            {
                return temp.BalanceQty;
            }
            else
            {
                return 0;
            }
        }
        public Decimal FGetPendingDeliveryOrderQtyForPacking(int SaleDeliveryOrderilneId)
        {
            var temp = (from L in db.ViewSaleDeliveryOrderBalance
                        where L.SaleDeliveryOrderLineId == SaleDeliveryOrderilneId
                        select L).FirstOrDefault();

            if (temp != null)
            {
                return temp.BalanceQty;
            }
            else
            {
                return 0;
            }
        }

        public IQueryable<ComboBoxResult> GetCustomProductsWithBuyerSku(int Id, string term)
        {

            var PackingHeader = new PackingHeaderService(_unitOfWork).Find(Id);

            //var settings = new PackingSettingsService(_unitOfWork).GetPacking(SaleEnquiry.DocTypeId, SaleEnquiry.DivisionId, SaleEnquiry.SiteId);

            //string[] ProductTypes = null;
            //if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            //else { ProductTypes = new string[] { "NA" }; }

            //string[] Products = null;
            //if (!string.IsNullOrEmpty(settings.filterProducts)) { Products = settings.filterProducts.Split(",".ToCharArray()); }
            //else { Products = new string[] { "NA" }; }

            //string[] ProductGroups = null;
            //if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            //else { ProductGroups = new string[] { "NA" }; }

            //return (from pb in db.ViewProductBuyer
            //        join Pt in db.Product on pb.ProductId equals Pt.ProductId into ProductTable
            //        from ProductTab in ProductTable.DefaultIfEmpty()
            //        where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(ProductTab.ProductGroup.ProductTypeId.ToString()))
            //        && (string.IsNullOrEmpty(settings.filterProducts) ? 1 == 1 : Products.Contains(ProductTab.ProductId.ToString()))
            //        && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(ProductTab.ProductGroupId.ToString()))
            //        && (string.IsNullOrEmpty(term) ? 1 == 1 : pb.ProductName.ToLower().Contains(term.ToLower()))
            //        && pb.BuyerId == SaleEnquiry.SaleToBuyerId
            //        orderby pb.ProductName
            //        select new ComboBoxResult
            //        {
            //            id = pb.ProductId.ToString(),
            //            text = ProductTab.ProductName,
            //            AProp1 = pb.ProductName
            //        });


            return (from pb in db.ViewProductBuyer
                    join Pt in db.Product on pb.ProductId equals Pt.ProductId into ProductTable
                    from ProductTab in ProductTable.DefaultIfEmpty()
                    where pb.BuyerId == PackingHeader.BuyerId
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : pb.ProductName.ToLower().Contains(term.ToLower())
                    || string.IsNullOrEmpty(term) ? 1 == 1 : ProductTab.ProductName.ToLower().Contains(term.ToLower()))
                    orderby pb.ProductName
                    select new ComboBoxResult
                    {
                        id = pb.ProductId.ToString(),
                        text = ProductTab.ProductName,
                        AProp1 = pb.ProductName
                    });
        }


        //New Functions For New Packing

        public IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term)
        {

            var Packing = new PackingHeaderService(_unitOfWork).Find(Id);

            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(Packing.DocTypeId, Packing.DivisionId, Packing.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] Products = null;
            if (!string.IsNullOrEmpty(settings.filterProducts)) { Products = settings.filterProducts.Split(",".ToCharArray()); }
            else { Products = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }

            string[] ProductDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterProductDivision)) { ProductDivisions = settings.filterProductDivision.Split(",".ToCharArray()); }
            else { ProductDivisions = new string[] { "NA" }; }

            return (from p in db.Product
                    where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProducts) ? 1 == 1 : Products.Contains(p.ProductId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(p.ProductGroupId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductDivision) ? 1 == 1 : ProductDivisions.Contains(p.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
                    && p.IsActive == true
                    orderby p.ProductName
                    select new ComboBoxResult
                    {
                        id = p.ProductId.ToString(),
                        text = p.ProductName,
                    });
        }

        public IEnumerable<PackingLineViewModel> GetPackingLineListForIndex(int PackingHeaderId)
        {
            IEnumerable<PackingLineViewModel> PackingLineViewModel = (from l in db.PackingLine
                                                                    join t1 in db.SaleOrderLine on l.SaleOrderLineId equals t1.SaleOrderLineId into table1
                                                                    from tab1 in table1.DefaultIfEmpty()
                                                                    join t2 in db.SaleOrderHeader on tab1.SaleOrderHeaderId equals t2.SaleOrderHeaderId into table2
                                                                    from tab2 in table2.DefaultIfEmpty()
                                                                    join t3 in db.ProductUid on l.ProductUidId equals t3.ProductUIDId into table3
                                                                    from tab3 in table3.DefaultIfEmpty()
                                                                    join u in db.Units on l.DealUnitId equals u.UnitId into DealUnitTable
                                                                    from DealUnitTab in DealUnitTable.DefaultIfEmpty()
                                                                    join Si in db.Stock on l.StockInId equals Si.StockId into StockInTable
                                                                    from StockInTab in StockInTable.DefaultIfEmpty()
                                                                    join Sih in db.StockHeader on StockInTab.StockHeaderId equals Sih.StockHeaderId into StockHeaderTable
                                                                    from StockHeaderTab in StockHeaderTable.DefaultIfEmpty()
                                                                    where l.PackingHeaderId == PackingHeaderId
                                                                    orderby l.PackingLineId
                                                                    select new PackingLineViewModel
                                                                    {
                                                                        PackingLineId = l.PackingLineId,
                                                                        ProductName = l.Product.ProductName,
                                                                        Dimension1Name = l.Dimension1.Dimension1Name,
                                                                        Dimension2Name = l.Dimension2.Dimension2Name,
                                                                        Specification = l.Specification,
                                                                        SaleOrderNo = tab2.DocNo,
                                                                        ProductUidName = tab3.ProductUidName,
                                                                        BaleNo = l.BaleNo,
                                                                        Qty = l.Qty,
                                                                        UnitId = l.Product.UnitId,
                                                                        DealQty = l.DealQty,
                                                                        DealUnitId = DealUnitTab.UnitName,
                                                                        DealUnitDecimalPlaces = l.DealUnit.DecimalPlaces,
                                                                        unitDecimalPlaces = l.Product.Unit.DecimalPlaces,
                                                                        StockInId = StockInTab.StockId,
                                                                        StockInNo = StockHeaderTab.DocNo,
                                                                        Remark = l.Remark,
                                                                        PackingHeaderId = l.PackingHeaderId
                                                                    }).Take(2000).ToList();

            return PackingLineViewModel;
        }

        public IEnumerable<PackingLineViewModel> GetSaleOrdersForFilters(PackingFilterViewModel vm)
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

            var temp = (from p in db.ViewSaleOrderBalanceForCancellation
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
                        select new PackingLineViewModel
                        {
                            //ProductUidIdName = tab1.ProductUid != null ? tab1.ProductUid.ProductUidName : "",
                            Dimension1Name = tab1.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.Dimension2.Dimension2Name,
                            Specification = tab1.Specification,
                            BalanceQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            SaleOrderNo = tab.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            Dimension1Id = p.Dimension1Id,
                            Dimension2Id = p.Dimension2Id,
                            PackingHeaderId = vm.PackingHeaderId,
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

        public IEnumerable<ComboBoxResult> GetPendingStockInForPacking(int PackingHeaderId, int GodownId, int ProductId, int? Dimension1Id, int? Dimension2Id, string term)
        {

            var PackingHeader = new PackingHeaderService(_unitOfWork).Find(PackingHeaderId);

            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(PackingHeader.DocTypeId, PackingHeader.DivisionId, PackingHeader.SiteId);


            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            SqlParameter SqlParameterPackingHeaderId = new SqlParameter("@PackingHeaderId", PackingHeaderId);
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


            IEnumerable<PendingStockInForPacking> PendingStockInForPacking = db.Database.SqlQuery<PendingStockInForPacking>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spGetHelpListPendingStockInForPacking @PackingHeaderId, @GodownId, @ProductId, @Dimension1Id, @Dimension2Id", SqlParameterPackingHeaderId, SqlParameterGodownId, SqlParameterProductId, SqlParameterDimension1Id, SqlParameterDimension2Id).ToList();


            return (from p in PendingStockInForPacking
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



        }

        public IEnumerable<ComboBoxResult> GetPendingOrdersForPacking(int id, string term)
        {


            var PackingHeader = new PackingHeaderService(_unitOfWork).Find(id);

            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(PackingHeader.DocTypeId, PackingHeader.DivisionId, PackingHeader.SiteId);

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


            return (from p in db.ViewSaleOrderBalanceForCancellation
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

        public IQueryable<ComboBoxResult> GetPendingProductsForPacking(int id, string term)//DocTypeId
        {

            var Packing = new PackingHeaderService(_unitOfWork).Find(id);

            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(Packing.DocTypeId, Packing.DivisionId, Packing.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            return (from p in db.ViewSaleOrderBalanceForCancellation
                    join t in db.Product on p.ProductId equals t.ProductId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && ProTab.ProductName.ToLower().Contains(term.ToLower()) && p.BuyerId == Packing.BuyerId
                     && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == Packing.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                     && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == Packing.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
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

        public IQueryable<ComboBoxResult> GetSaleOrderHelpListForProduct(int filter, string term)
        {
            var PackingHeader = new PackingHeaderService(_unitOfWork).Find(filter);

            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(PackingHeader.DocTypeId, PackingHeader.DivisionId, PackingHeader.SiteId);

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


            var list = (from p in db.ViewSaleOrderBalanceForCancellation
                        join t in db.SaleOrderHeader on p.SaleOrderHeaderId equals t.SaleOrderHeaderId
                        join t2 in db.SaleOrderLine on p.SaleOrderLineId equals t2.SaleOrderLineId
                        join pt in db.Product on p.ProductId equals pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where p.BuyerId == PackingHeader.BuyerId
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

        public PackingLineViewModel GetPackingLineForEdit(int id)
        {

            return (from Pl in db.PackingLine
                    join t3 in db.ViewSaleOrderBalance on Pl.SaleOrderLineId equals t3.SaleOrderLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join Si in db.Stock on Pl.StockInId equals Si.StockId into StockInTable
                    from StockInTab in StockInTable.DefaultIfEmpty()
                    join Sih in db.StockHeader on StockInTab.StockHeaderId equals Sih.StockHeaderId into StockHeaderTable
                    from StockHeaderTab in StockHeaderTable.DefaultIfEmpty()
                    where Pl.PackingLineId == id
                    select new PackingLineViewModel
                    {
                        ProductUidId = Pl.ProductUidId,
                        ProductUidName = Pl.ProductUid.ProductUidName,
                        ProductCode = Pl.Product.ProductCode,
                        ProductId = Pl.ProductId,
                        ProductName = Pl.Product.ProductName,
                        SaleOrderNo = Pl.SaleOrderLine.SaleOrderHeader.DocNo,
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
                        UnitConversionMultiplier = Pl.UnitConversionMultiplier,
                        PackingHeaderId = Pl.PackingHeaderId,
                        PackingLineId = Pl.PackingLineId,
                        SaleOrderLineId = Pl.SaleOrderLineId,
                        Weight = Pl.NetWeight,
                        StockInId = Pl.StockInId,
                        StockInNo = StockHeaderTab.DocNo
                    }
                        ).FirstOrDefault();

        }

        public IEnumerable<ComboBoxResult> GetPendingStockInForIssue(int PackingHeaderId, int? ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, string term)
        {
            var PackingHeader = new PackingHeaderService(_unitOfWork).Find(PackingHeaderId);

            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(PackingHeader.DocTypeId, PackingHeader.DivisionId, PackingHeader.SiteId);


            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            return (from p in db.ViewStockInBalance
                    join L in db.Stock on p.StockInId equals L.StockId into StockTable
                    from StockTab in StockTable.DefaultIfEmpty()
                    join pt in db.Product on p.ProductId equals pt.ProductId into ProductTable
                    from ProductTab in ProductTable.DefaultIfEmpty()
                    join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                    from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                    join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                    from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                    join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                    from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                    join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                    from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                    where p.BalanceQty > 0 && StockTab.GodownId == PackingHeader.GodownId
                        && StockTab.StockHeader.DocTypeId != PackingHeader.DocTypeId
                    && (ProductId == null || ProductId == 0 ? 1 == 1 : p.ProductId == ProductId)
                    && (Dimension1Id == null ? 1 == 1 : p.Dimension1Id == Dimension1Id)
                    && (Dimension2Id == null ? 1 == 1 : p.Dimension2Id == Dimension2Id)
                    && (Dimension3Id == null ? 1 == 1 : p.Dimension3Id == Dimension3Id)
                    && (Dimension4Id == null ? 1 == 1 : p.Dimension4Id == Dimension4Id)
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.StockInNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : StockTab.StockHeader.DocType.DocumentTypeShortName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : ProductTab.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension1Tab.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension2Tab.Dimension2Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension3Tab.Dimension3Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension4Tab.Dimension4Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : StockTab.LotNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : StockTab.ProductUid.ProductUidName.ToLower().Contains(term.ToLower())
                        )
                    select new ComboBoxResult
                    {
                        id = p.StockInId.ToString(),
                        text = StockTab.StockHeader.DocType.DocumentTypeShortName + "-" + p.StockInNo,
                        TextProp1 = "Balance :" + p.BalanceQty,
                        TextProp2 = "Date :" + p.StockInDate,
                        AProp1 = ProductTab.ProductName + ((StockTab.ProductUid.ProductUidName == null) ? "" : "," + StockTab.ProductUid.ProductUidName),
                        AProp2 = ((Dimension1Tab.Dimension1Name == null) ? "" : Dimension1Tab.Dimension1Name) +
                                    ((Dimension2Tab.Dimension2Name == null) ? "" : "," + Dimension2Tab.Dimension2Name) +
                                    ((Dimension3Tab.Dimension3Name == null) ? "" : "," + Dimension3Tab.Dimension3Name) +
                                    ((Dimension4Tab.Dimension4Name == null) ? "" : "," + Dimension4Tab.Dimension4Name) +
                                    ((p.LotNo == null) ? "" : "," + p.LotNo)
                    });
        }

    }







    public class PendingOrderListForPacking
    {
        public int SaleOrderLineId { get; set; }
        public string SaleOrderNo { get; set; }
    }

    public class PendingDeliveryOrderListForPacking
    {
        public int SaleDeliveryOrderLineId { get; set; }
        public string SaleDeliveryOrderNo { get; set; }
        public int? ShipMethodId { get; set; }
        public int? OtherBuyerDeliveryOrders { get; set; }
    }

    public class ProductAreaDetail
    {
        public int ProductId { get; set; }
        public Decimal ProductArea { get; set; }
    }

    public class PackingBaleNo
    {
        public int PackingHeaderId { get; set; }
        public int NewBaleNo { get; set; }
    }

    public class StockAvailableForPacking
    {
        public int ProductId { get; set; }
        public Decimal Qty { get; set; }
    }

    public class PendingOrderQtyForPacking
    {
        public Decimal Qty { get; set; }
    }

    public class PendingStockInForPacking
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

