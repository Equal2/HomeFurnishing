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
using System.Data;


namespace Service
{
    public interface ISaleInvoiceLineService : IDisposable
    {
        SaleInvoiceLine Create(SaleInvoiceLine s);
        void Delete(int id);
        void Delete(SaleInvoiceLine s);
        SaleInvoiceLine GetSaleInvoiceLine(int id);
        IQueryable<SaleInvoiceLine> GetSaleInvoiceLineList(int SaleInvliceHeaderId);
        SaleInvoiceLine Find(int id);
        void Update(SaleInvoiceLine s);
        SaleInvoiceLineViewModel GetSaleInvoiceLineForLineId(int SaleInvoiceLineId);
        IEnumerable<SaleInvoiceLineViewModel> GetSaleInvoiceLineListForIndex(int SaleInvoiceHeaderId);
        IQueryable<SaleInvoiceLineViewModel> GetSaleInvoiceLineListForIndex(int SaleInvoiceHeaderId,int Iteration);
        IEnumerable<SaleInvoiceLineViewModel> GetDirectSaleInvoiceLineListForIndex(int SaleInvoiceHeaderId);
        bool CheckForProductExists(int ProductId, int SaleInvoiceHEaderId, int SaleInvoiceLineId);
        bool CheckForProductExists(int ProductId, int SaleInvoiceHEaderId);
        IEnumerable<SaleInvoiceLineViewModel> GetPackingLineForProductDetail(int PackingHeaderid);

        IEnumerable<ComboBoxList> GetPackginNoPendingForInvoice(int BuyerId, DateTime DocDate);
        string GetDescriptionOfGoods(int id);

        IEnumerable<ComboBoxResult> GetSaleOrderHelpListForProduct(int Id, int ProductId, string term, int Limit);
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term, int Limit);

        IEnumerable<ComboBoxList> GetPendingOrdersForInvoice(int id, string term, int Limit);

        
        DirectSaleInvoiceLineViewModel GetDirectSaleInvoiceLineForEdit(int id);
        IEnumerable<DirectSaleInvoiceLineViewModel> GetSaleOrdersForFilters(SaleInvoiceFilterViewModel vm);



        IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term);

        IQueryable<ComboBoxResult> GetSaleOrderHelpListForProduct(int Id, int PersonId, string term);

        IQueryable<ComboBoxResult> GetSaleDispatchHelpListForProduct(int PersonId, string term);

        int GetMaxSr(int id);

        List<DirectSaleInvoiceLineViewModel> GetSaleDispatchForFilters(SaleInvoiceFilterViewModel vm);

        DirectSaleInvoiceLineViewModel GetSaleInvoiceLineForEdit(int id);

        IQueryable<ComboBoxResult> GetPendingProductsForSaleInvoice(int Jid, string term);

        IEnumerable<ComboBoxResult> GetPendingDispatchForInvoice(int id, string term);

        IEnumerable<ComboBoxResult> FGetPromoCodeList(int ProductId, int BuyerId, DateTime DocDate);
        IEnumerable<ComboBoxResult> FGetProductUidHelpList(int Id, string term);
        IEnumerable<ComboBoxResult> GetPendingPackingHeaderForSaleInvoice(int SaleInvoiceHeaderId, string term);

        
    }

    public class SaleInvoiceLineService : ISaleInvoiceLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleInvoiceLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public SaleInvoiceLine Create(SaleInvoiceLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleInvoiceLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleInvoiceLine>().Delete(id);
        }

        public void Delete(SaleInvoiceLine s)
        {
            _unitOfWork.Repository<SaleInvoiceLine>().Delete(s);
        }

        public void Update(SaleInvoiceLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleInvoiceLine>().Update(s);
        }

        public SaleInvoiceLine GetSaleInvoiceLine(int id)
        {
            return _unitOfWork.Repository<SaleInvoiceLine>().Query().Get().Where(m => m.SaleInvoiceLineId == id).FirstOrDefault();
        }


        public SaleInvoiceLine Find(int id)
        {
            return _unitOfWork.Repository<SaleInvoiceLine>().Find(id);
        }

        public IQueryable<SaleInvoiceLine> GetSaleInvoiceLineList(int SaleInvoiceHeaderId)
        {
            return _unitOfWork.Repository<SaleInvoiceLine>().Query().Get().Where(m => m.SaleInvoiceHeaderId == SaleInvoiceHeaderId);
        }

        public SaleInvoiceLineViewModel GetSaleInvoiceLineForLineId(int SaleInvoiceLineId)
        {
            var temp = (from L in db.SaleInvoiceLine
                        join P in db.Product on L.ProductId equals P.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join Du in db.Units on L.DealUnitId equals Du.UnitId into DealUnitTable
                        from DealUnitTab in DealUnitTable.DefaultIfEmpty()
                        orderby L.SaleInvoiceLineId
                        where L.SaleInvoiceLineId == SaleInvoiceLineId
                        select new SaleInvoiceLineViewModel
                        {
                            SaleInvoiceHeaderId = L.SaleInvoiceHeaderId,
                            SaleInvoiceLineId = L.SaleInvoiceLineId,
                            SaleDispatchLineId = L.SaleDispatchLineId,
                            ProductName = ProductTab.ProductName,
                            Qty = L.Qty,
                            DealQty = L.DealQty,
                            DealUnitName = DealUnitTab.UnitName,
                            ProductInvoiceGroupId = L.ProductInvoiceGroupId,
                            Rate = L.Rate,
                            Amount = L.Amount,
                            Remark = L.Remark,
                            CreatedBy = L.CreatedBy,
                            CreatedDate = L.CreatedDate,
                        }).FirstOrDefault();
            return temp;
        }



        public IEnumerable<SaleInvoiceLineViewModel> GetSaleInvoiceLineListForIndex(int SaleInvoiceHeaderId)
        {
            IEnumerable<SaleInvoiceLineViewModel> SaleInvoiceLineViewModel = (from l in db.ViewSaleInvoiceLine
                                                                              join t in db.Product on l.ProductID equals t.ProductId into table
                                                                              from tab in table.DefaultIfEmpty()
                                                                              join t1 in db.SaleOrderLine on l.SaleOrderLineId equals t1.SaleOrderLineId into table1
                                                                              from tab1 in table1.DefaultIfEmpty()
                                                                              join t2 in db.SaleOrderHeader on tab1.SaleOrderHeaderId equals t2.SaleOrderHeaderId into table2
                                                                              from tab2 in table2.DefaultIfEmpty()
                                                                              join t3 in db.ProductUid on l.ProductUidId equals t3.ProductUIDId into table3
                                                                              from tab3 in table3.DefaultIfEmpty()
                                                                              join t4 in db.ProductInvoiceGroup on l.ProductInvoiceGroupId equals t4.ProductInvoiceGroupId into table4
                                                                              from tab4 in table4.DefaultIfEmpty()
                                                                              join u in db.Units on l.DealUnitId equals u.UnitId into DealUnitTable
                                                                              from DealUnitTab in DealUnitTable.DefaultIfEmpty()
                                                                              where l.SaleInvoiceHeaderId == SaleInvoiceHeaderId
                                                                              orderby l.SaleInvoiceLineId
                                                                              select new SaleInvoiceLineViewModel
                                                                              {
                                                                                  SaleInvoiceLineId = l.SaleInvoiceLineId,
                                                                                  ProductName = tab.ProductName,
                                                                                  Specification = l.Specification,
                                                                                  SaleOrderHeaderDocNo = tab2.DocNo,
                                                                                  ProductUidIdName = tab3.ProductUidName,
                                                                                  BaleNo = l.BaleNo,
                                                                                  ProductInvoiceGroupName = tab4.ProductInvoiceGroupName,
                                                                                  Qty = l.Qty,
                                                                                  UnitId = tab.UnitId,
                                                                                  DealQty = l.DealQty,
                                                                                  DealUnitId = DealUnitTab.UnitName,
                                                                                  DealUnitDecimalPlaces = DealUnitTab.DecimalPlaces,
                                                                                  Rate = l.Rate,
                                                                                  Amount = l.Amount,
                                                                                  Remark = l.Remark,
                                                                              }).Take(2000).ToList();


            double x = 0;
            var p = SaleInvoiceLineViewModel.OrderBy(sx => double.TryParse((sx.BaleNo ?? "") .Replace("-", "."), out x) ? x : 0);


            return p;
        }

        public IEnumerable<SaleInvoiceLineViewModel> GetDirectSaleInvoiceLineListForIndex(int SaleInvoiceHeaderId)
        {
            IEnumerable<SaleInvoiceLineViewModel> SaleInvoiceLineViewModel = (from l in db.ViewSaleInvoiceLine
                                                                              join t in db.Product on l.ProductID equals t.ProductId into table
                                                                              from tab in table.DefaultIfEmpty()
                                                                              join D1 in db.Dimension1 on l.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                                                                              from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                                                                              join D2 in db.Dimension2 on l.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                                                                              from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                                                                              join t1 in db.SaleOrderLine on l.SaleOrderLineId equals t1.SaleOrderLineId into table1
                                                                              from tab1 in table1.DefaultIfEmpty()
                                                                              join t2 in db.SaleOrderHeader on tab1.SaleOrderHeaderId equals t2.SaleOrderHeaderId into table2
                                                                              from tab2 in table2.DefaultIfEmpty()
                                                                              join t3 in db.ProductUid on l.ProductUidId equals t3.ProductUIDId into table3
                                                                              from tab3 in table3.DefaultIfEmpty()
                                                                              join t4 in db.ProductInvoiceGroup on l.ProductInvoiceGroupId equals t4.ProductInvoiceGroupId into table4
                                                                              from tab4 in table4.DefaultIfEmpty()
                                                                              join u in db.Units on l.DealUnitId equals u.UnitId into DealUnitTable
                                                                              from DealUnitTab in DealUnitTable.DefaultIfEmpty()
                                                                              join Sil in db.SaleInvoiceLine on l.SaleInvoiceLineId equals Sil.SaleInvoiceLineId into SaleInvoiceLineTable from SaleInvoiceLineTab in SaleInvoiceLineTable.DefaultIfEmpty()
                                                                              where l.SaleInvoiceHeaderId == SaleInvoiceHeaderId
                                                                              orderby l.Sr
                                                                              select new SaleInvoiceLineViewModel
                                                                              {
                                                                                  SaleInvoiceLineId = l.SaleInvoiceLineId,
                                                                                  ProductName = tab.ProductName,
                                                                                  Dimension1Name = Dimension1Tab.Dimension1Name,
                                                                                  Dimension2Name = Dimension2Tab.Dimension2Name,
                                                                                  Specification = l.Specification,                                                                                  
                                                                                  ProductUidIdName = tab3.ProductUidName,
                                                                                  BaleNo = l.BaleNo,
                                                                                  ProductInvoiceGroupName = tab4.ProductInvoiceGroupName,
                                                                                  Qty = l.Qty,
                                                                                  UnitId = tab.UnitId,
                                                                                  DealQty = l.DealQty,
                                                                                  DealUnitId = DealUnitTab.UnitName,
                                                                                  UnitDecimalPlaces = tab.Unit.DecimalPlaces,
                                                                                  DealUnitDecimalPlaces = DealUnitTab.DecimalPlaces,
                                                                                  Rate = l.Rate,
                                                                                  SaleDispatchDocNo = SaleInvoiceLineTab.SaleDispatchLine.SaleDispatchHeader.DocNo,
                                                                                  Amount = l.Amount,
                                                                                  Remark = l.Remark,
                                                                                  SaleOrderHeaderDocNo = tab2.DocNo,
                                                                              }).Take(2000).ToList();



            return SaleInvoiceLineViewModel;
        }

        public IQueryable<SaleInvoiceLineViewModel> GetSaleInvoiceLineListForIndex(int SaleInvoiceHeaderId,int Iteration)
        {

            IQueryable<SaleInvoiceLineViewModel> SaleInvoiceLineViewModel = (from l in db.ViewSaleInvoiceLine
                                                                              join t in db.Product on l.ProductID equals t.ProductId into table
                                                                              from tab in table.DefaultIfEmpty()
                                                                              join t1 in db.SaleOrderLine on l.SaleOrderLineId equals t1.SaleOrderLineId into table1
                                                                              from tab1 in table1.DefaultIfEmpty()
                                                                              join t2 in db.SaleOrderHeader on tab1.SaleOrderHeaderId equals t2.SaleOrderHeaderId into table2
                                                                              from tab2 in table2.DefaultIfEmpty()
                                                                              join t3 in db.ProductUid on l.ProductUidId equals t3.ProductUIDId into table3
                                                                              from tab3 in table3.DefaultIfEmpty()
                                                                              join t4 in db.ProductInvoiceGroup on l.ProductInvoiceGroupId equals t4.ProductInvoiceGroupId into table4
                                                                              from tab4 in table4.DefaultIfEmpty()
                                                                              join u in db.Units on l.DealUnitId equals u.UnitId into DealUnitTable
                                                                              from DealUnitTab in DealUnitTable.DefaultIfEmpty()
                                                                              where l.SaleInvoiceHeaderId == SaleInvoiceHeaderId
                                                                              orderby l.SaleInvoiceLineId
                                                                              select new SaleInvoiceLineViewModel
                                                                              {
                                                                                  SaleInvoiceLineId = l.SaleInvoiceLineId,
                                                                                  ProductName = tab.ProductName,
                                                                                  Specification = l.Specification,
                                                                                  SaleOrderHeaderDocNo = tab2.DocNo,
                                                                                  ProductUidIdName = tab3.ProductUidName,
                                                                                  BaleNo = l.BaleNo,
                                                                                  ProductInvoiceGroupName = tab4.ProductInvoiceGroupName,
                                                                                  Qty = l.Qty,
                                                                                  UnitId = tab.UnitId,
                                                                                  DealQty = l.DealQty,
                                                                                  DealUnitId = DealUnitTab.UnitName,
                                                                                  DealUnitDecimalPlaces = DealUnitTab.DecimalPlaces,
                                                                                  Rate = l.Rate,
                                                                                  Amount = l.Amount,
                                                                                  Remark = l.Remark,
                                                                              });





            return SaleInvoiceLineViewModel;
        }

        public bool CheckForProductExists(int ProductId, int SaleInvoiceHeaderId, int SaleInvoiceLineId)
        {

            SaleInvoiceLine temp = (from p in db.SaleInvoiceLine
                                    where p.SaleInvoiceHeaderId == SaleInvoiceHeaderId && p.SaleInvoiceLineId != SaleInvoiceLineId
                                    select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }

        public bool CheckForProductExists(int ProductId, int SaleInvoiceHeaderId)
        {

            SaleInvoiceLine temp = (from p in db.SaleInvoiceLine
                                    where p.SaleInvoiceHeaderId == SaleInvoiceHeaderId
                                    select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }

        public IEnumerable<SaleInvoiceLineViewModel> GetPackingLineForProductDetail(int PackingHeaderid)
        {
            var saledispatchline = from L in db.SaleDispatchLine
                                   join H in db.SaleDispatchHeader on L.SaleDispatchHeaderId equals H.SaleDispatchHeaderId into SaleDispatchHeaderTable
                                   from SaleDispatchHeaderTab in SaleDispatchHeaderTable.DefaultIfEmpty()
                                   join PL in db.PackingLine on L.PackingLineId equals PL.PackingLineId into PackingLineTable
                                   from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                                   group new { PackingLineTab } by new { PackingLineTab.PackingLineId } into Result
                                   select new
                                   {
                                       PackingLineId = Result.Key.PackingLineId,
                                       DispatchQty = Result.Sum(m => m.PackingLineTab.Qty)
                                   };


            var packingline = from L in db.PackingLine
                              join H in db.PackingHeader on L.PackingHeaderId equals H.PackingHeaderId into PackingHeaderTable
                              from PackingHeaderTab in PackingHeaderTable.DefaultIfEmpty()
                              join D in saledispatchline on L.PackingLineId equals D.PackingLineId into DispatchTable
                              from DispatchTab in DispatchTable.DefaultIfEmpty()
                              join P in db.FinishedProduct on L.ProductId equals P.ProductId into ProductTable
                              from ProductTab in ProductTable.DefaultIfEmpty()
                              join Pig in db.ProductInvoiceGroup on ProductTab.ProductInvoiceGroupId equals Pig.ProductInvoiceGroupId into ProductInvoiceGroupTable
                              from ProductInvoiceGroupTab in ProductInvoiceGroupTable.DefaultIfEmpty()
                              join Dog in db.DescriptionOfGoods on ProductInvoiceGroupTab.DescriptionOfGoodsId equals Dog.DescriptionOfGoodsId into DescriptionOfGoodsTable
                              from DescriptionOfGoodsTab in DescriptionOfGoodsTable.DefaultIfEmpty()
                              join Fc in db.ProductContentHeader on ProductTab.FaceContentId equals Fc.ProductContentHeaderId into FaceContentTable
                              from FaceContentTab in FaceContentTable.DefaultIfEmpty()
                              join Sol in db.SaleOrderLine on L.SaleOrderLineId equals Sol.SaleOrderLineId into SaleOrderLineTable
                              from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
                              where L.PackingHeaderId == PackingHeaderid && L.Qty - ((Decimal?)DispatchTab.DispatchQty ?? 0) > 0
                              select new SaleInvoiceLineViewModel
                              {
                                  PackingLineId = L.PackingLineId,
                                  LotNo = PackingHeaderTab.DocNo,
                                  GodownId = PackingHeaderTab.GodownId,
                                  SalesTaxGroupProductId = ProductTab.SalesTaxGroupProductId,
                                  SaleOrderLineId = L.SaleOrderLineId,
                                  ProductId = L.ProductId,
                                  ProductUidId = L.ProductUidId,
                                  DescriptionOfGoodsName = DescriptionOfGoodsTab.DescriptionOfGoodsName,
                                  FaceContentName = FaceContentTab.ProductContentName,
                                  ProductInvoiceGroupId = ProductInvoiceGroupTab.ProductInvoiceGroupId,
                                  ProductInvoiceGroupName = ProductInvoiceGroupTab.ProductInvoiceGroupName,
                                  BaleNo = L.BaleNo,
                                  Qty = L.Qty,
                                  DealQty = L.DealQty,
                                  UnitConversionMultiplier = L.DealQty / L.Qty,
                                  UnitId = ProductTab.UnitId,
                                  DealUnitId = L.DealUnitId,
                                  SaleOrderDealUnitId = SaleOrderLineTab.DealUnitId,
                                  SaleOrderRate = (Decimal?)SaleOrderLineTab.Rate ?? 0,
                                  Rate = (Decimal?)ProductInvoiceGroupTab.Rate ?? 0,
                                  Amount = (Decimal?)(L.DealQty * ProductInvoiceGroupTab.Rate) ?? 0,
                                  RateRemark = L.RateRemark,
                                  Remark = L.Remark
                              };

            return packingline;

        }

        public IEnumerable<ComboBoxList> GetPackginNoPendingForInvoice(int BuyerId, DateTime DocDate)
        {
            //SaleDispatchHeaderTab.SaleToBuyerId == BuyerId && 
            //PackingHeaderTab.BuyerId == BuyerId && 


            var saledispatchline = from L in db.SaleDispatchLine
                                   join H in db.SaleDispatchHeader on L.SaleDispatchHeaderId equals H.SaleDispatchHeaderId into SaleDispatchHeaderTable
                                   from SaleDispatchHeaderTab in SaleDispatchHeaderTable.DefaultIfEmpty()
                                   join PL in db.PackingLine on L.PackingLineId equals PL.PackingLineId into PackingLineTable
                                   from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                                   group new { PackingLineTab } by new { PackingLineTab.PackingLineId } into Result
                                   select new
                                   {
                                       PackingLineId = Result.Key.PackingLineId,
                                       DispatchQty = Result.Sum(m => m.PackingLineTab.Qty)
                                   };


            var packingline = from L in db.PackingLine
                              join H in db.PackingHeader on L.PackingHeaderId equals H.PackingHeaderId into PackingHeaderTable
                              from PackingHeaderTab in PackingHeaderTable.DefaultIfEmpty()
                              join D in saledispatchline on L.PackingLineId equals D.PackingLineId into DispatchTable
                              from DispatchTab in DispatchTable.DefaultIfEmpty()
                              where PackingHeaderTab.DocDate <= DocDate && PackingHeaderTab.Status == (int)StatusConstants.Approved && L.Qty - ((Decimal?)DispatchTab.DispatchQty ?? 0) > 0
                              select new
                              {
                                  PackginLineId = L.PackingLineId,
                                  PackingHeaderId = L.PackingHeaderId,
                                  PackingNo = PackingHeaderTab.DocNo
                              };


            IEnumerable<ComboBoxList> packingheader = from H in packingline
                                                      group new { H } by new { H.PackingHeaderId } into Result
                                                      select new ComboBoxList
                                                      {
                                                          Id = Result.Key.PackingHeaderId,
                                                          PropFirst = Result.Max(m => m.H.PackingNo)
                                                      };


            return packingheader;
        }


        public string GetDescriptionOfGoods(int id)
        {
            string DescriptionOfGoods = "";

            var saleinvoicegoods = (from L in db.SaleInvoiceLine
                                    join dl in db.SaleDispatchLine on L.SaleDispatchLineId equals dl.SaleDispatchLineId into SaleDispatchLineTable
                                    from SaleDispatchLineTab in SaleDispatchLineTable.DefaultIfEmpty()
                                    join Pl in db.PackingLine on SaleDispatchLineTab.PackingLineId equals Pl.PackingLineId into PackingLineTable
                                    from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                                    join P in db.FinishedProduct on PackingLineTab.ProductId equals P.ProductId into ProductTable
                                    from ProductTab in ProductTable.DefaultIfEmpty()
                                    join D in db.DescriptionOfGoods on ProductTab.DescriptionOfGoodsId equals D.DescriptionOfGoodsId into DescriptionOfGoodsTable
                                    from DescriptionOfGoodsTab in DescriptionOfGoodsTable.DefaultIfEmpty()
                                    where L.SaleInvoiceLineId == id
                                    group new { L, DescriptionOfGoodsTab } by new { L.SaleInvoiceHeaderId, DescriptionOfGoodsTab.DescriptionOfGoodsId } into Result
                                    select new
                                    {
                                        SaleInvoiceHeaderId = Result.Key.SaleInvoiceHeaderId,
                                        DescriptionOfGoodsName = Result.Max(m => m.DescriptionOfGoodsTab.DescriptionOfGoodsName)
                                    }).ToList();


            if (saleinvoicegoods != null)
            {
                foreach (var item in saleinvoicegoods)
                {
                    if (DescriptionOfGoods == "")
                    {
                        DescriptionOfGoods = item.DescriptionOfGoodsName;
                    }
                    else
                    {
                        DescriptionOfGoods = DescriptionOfGoods + "," + item.DescriptionOfGoodsName;
                    }
                }
            }
            return DescriptionOfGoods;
        }


        
        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term, int Limit)
        {

            var SaleInvoice = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(Id);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoice.DocTypeId, SaleInvoice.DivisionId, SaleInvoice.SiteId);


            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }


            var list = (from p in db.Product
                        where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
                        select new ComboBoxList
                        {
                            Id = p.ProductId,
                            PropFirst = p.ProductName,
                        }
                ).Take(Limit);

            return list.ToList();

        }

        public IEnumerable<ComboBoxList> GetPendingOrdersForInvoice(int id, string Term, int Limit)
        {


            var SaleInvoiceHeader = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(id);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoiceHeader.DocTypeId, SaleInvoiceHeader.DivisionId, SaleInvoiceHeader.SiteId);

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
                    group p by p.SaleOrderHeaderId into g
                    select new ComboBoxList
                    {
                        Id = g.Key,
                        PropFirst = g.Max(m => m.SaleOrderNo),
                    }
                        ).Take(Limit);
        }

        

        public DirectSaleInvoiceLineViewModel GetDirectSaleInvoiceLineForEdit(int id)
        {

            return (from p in db.SaleInvoiceLine
                    join Sid in db.SaleInvoiceLineDetail on p.SaleInvoiceLineId equals Sid.SaleInvoiceLineId into SaleInvoiceLineDetailTable from SaleInvoiceLineDetailTab in SaleInvoiceLineDetailTable.DefaultIfEmpty()
                    join t in db.SaleDispatchLine on p.SaleDispatchLineId equals t.SaleDispatchLineId into table
                    from Dl in table.DefaultIfEmpty()
                    join t2 in db.PackingLine on Dl.PackingLineId equals t2.PackingLineId into table2
                    from Pl in table2.DefaultIfEmpty()
                    join t3 in db.ViewSaleOrderBalance on p.SaleOrderLineId equals t3.SaleOrderLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join Dh in db.SaleDispatchHeader on Dl.SaleDispatchHeaderId equals Dh.SaleDispatchHeaderId into SaleDispatchHeaderTable
                    from SaleDispatchHeaderTab in SaleDispatchHeaderTable.DefaultIfEmpty()
                    where p.SaleInvoiceLineId == id
                    select new DirectSaleInvoiceLineViewModel
                    {
                        ProductUidId = Pl.ProductUidId,
                        ProductUidName = Pl.ProductUid.ProductUidName,
                        ProductCode = Pl.Product.ProductCode,
                        ProductId = Pl.ProductId,
                        ProductName = Pl.Product.ProductName,
                        SaleOrderHeaderDocNo = p.SaleOrderLine.SaleOrderHeader.DocNo,
                        Qty = Pl.Qty,
                        unitDecimalPlaces = Pl.Product.Unit.DecimalPlaces,
                        BalanceQty = (tab3 == null ? p.Qty : tab3.BalanceQty + p.Qty),
                        BaleNo = Pl.BaleNo,
                        UnitId = p.Product.UnitId,
                        UnitName = p.Product.Unit.UnitName,
                        DealUnitId = Pl.DealUnitId,
                        DealUnitName = Pl.DealUnit.UnitName,
                        DealQty = Pl.DealQty,
                        Remark = Pl.Remark,
                        Specification = Pl.Specification,
                        Dimension1Id = Pl.Dimension1Id,
                        Dimension2Id = Pl.Dimension2Id,
                        LotNo = Pl.LotNo,
                        GodownId = Dl.GodownId,
                        DiscountPer = p.DiscountPer,
                        DiscountAmount = p.DiscountAmount,
                        PromoCodeId = p.PromoCodeId,
                        Amount = p.Amount,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        Rate = p.Rate,
                        SaleInvoiceLineId = p.SaleInvoiceLineId,
                        SaleInvoiceHeaderId = p.SaleInvoiceHeaderId,
                        SaleDispatchLineId = p.SaleDispatchLineId,
                        SaleDispatchHeaderDocNo = SaleDispatchHeaderTab.DocNo,
                        PackingLineId = Pl.PackingLineId,
                        SaleOrderLineId = p.SaleOrderLineId,
                        Weight = p.Weight,
                        FreeQty = Pl.FreeQty,
                        RewardPoints = SaleInvoiceLineDetailTab.RewardPoints,
                        SalesTaxGroupProductId = p.SalesTaxGroupProductId
                    }).FirstOrDefault();

        }


        public IEnumerable<DirectSaleInvoiceLineViewModel> GetSaleOrdersForFilters(SaleInvoiceFilterViewModel vm)
        {
            var SaleInvoice = db.SaleInvoiceHeader.Find(vm.SaleInvoiceHeaderId);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoice.DocTypeId, SaleInvoice.DivisionId, SaleInvoice.SiteId);


            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.SaleOrderHeaderId)) { SaleOrderIdArr = vm.SaleOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] PackingIdArr = null;
            if (!string.IsNullOrEmpty(vm.PackingHeaderId)) { PackingIdArr = vm.PackingHeaderId.Split(",".ToCharArray()); }
            else { PackingIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }


            if ((settings.isVisiblePacking ?? false) == true)
            {
                var temp = (from p in db.ViewPackingBalance
                            join t in db.PackingHeader on p.PackingHeaderId equals t.PackingHeaderId into table
                            from tab in table.DefaultIfEmpty()
                            join t1 in db.PackingLine on p.PackingLineId equals t1.PackingLineId into table1
                            from tab1 in table1.DefaultIfEmpty()
                            join product in db.Product on p.ProductId equals product.ProductId into table2
                            from tab2 in table2.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.PackingHeaderId) ? 1 == 1 : PackingIdArr.Contains(p.PackingHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                            && p.BalanceQty > 0
                            orderby p.PackingLineId
                            select new DirectSaleInvoiceLineViewModel
                            {
                                //ProductUidIdName = tab1.ProductUid != null ? tab1.ProductUid.ProductUidName : "",
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Specification = tab1.Specification,
                                BalanceQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                PackingDocNo = tab.DocNo,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                SaleInvoiceHeaderId = vm.SaleInvoiceHeaderId,
                                PackingLineId = p.PackingLineId,
                                SaleOrderLineId = tab1.SaleOrderLineId,
                                SaleOrderHeaderDocNo = tab1.SaleOrderLine.SaleOrderHeader.DocNo,
                                UnitId = tab2.UnitId,
                                UnitName = tab2.Unit.UnitName,
                                DealUnitId = tab1.DealUnitId,
                                DealUnitName = tab1.DealUnit.UnitName,
                                unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                                DealunitDecimalPlaces = tab1.DealUnit.DecimalPlaces,
                                DealQty = p.BalanceQty * tab1.UnitConversionMultiplier,
                                UnitConversionMultiplier = tab1.UnitConversionMultiplier,
                                Rate = tab1.SaleOrderLine.Rate,
                                DiscountPer = tab1.SaleOrderLine.DiscountPer,
                                GodownId = tab.GodownId
                            });

                return temp;
            }
            else
            {
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
                            && p.BalanceQty > 0
                            orderby p.SaleOrderLineId
                            select new DirectSaleInvoiceLineViewModel
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
                                SaleInvoiceHeaderId = vm.SaleInvoiceHeaderId,
                                SaleOrderLineId = p.SaleOrderLineId,
                                UnitId = tab2.UnitId,
                                UnitName = tab2.Unit.UnitName,
                                DealUnitId = tab1.DealUnitId,
                                DealUnitName = tab1.DealUnit.UnitName,
                                unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                                DealunitDecimalPlaces = tab1.DealUnit.DecimalPlaces,
                                DealQty = (!tab1.UnitConversionMultiplier.HasValue || tab1.UnitConversionMultiplier <= 0) ? p.BalanceQty : p.BalanceQty * tab1.UnitConversionMultiplier.Value,
                                UnitConversionMultiplier = tab1.UnitConversionMultiplier,
                                Rate = tab1.Rate,
                                DiscountPer = tab1.DiscountPer,
                            });

                return temp;
            }


            
        }


        public SaleInvoiceLineViewModel GetSaleInvoiceLineBalance(int id)
        {
            return (from b in db.ViewSaleInvoiceBalance
                    join p in db.SaleInvoiceLine on b.SaleInvoiceLineId equals p.SaleInvoiceLineId
                    join t in db.SaleDispatchLine on p.SaleDispatchLineId equals t.SaleDispatchLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t2 in db.SaleInvoiceHeader on p.SaleInvoiceHeaderId equals t2.SaleInvoiceHeaderId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t in db.SaleDispatchHeader on tab.SaleDispatchHeaderId equals t.SaleDispatchHeaderId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.SaleInvoiceLineId == id
                    select new SaleInvoiceLineViewModel
                    {
                        Amount = p.Amount,
                        ProductId = p.ProductId,
                        SaleDispatchLineId = p.SaleDispatchLineId,
                        // SaleDispatchHeaderDocNo = tab1.DocNo,
                        SaleInvoiceHeaderId = p.SaleInvoiceHeaderId,
                        SaleInvoiceLineId = p.SaleInvoiceLineId,
                        Qty = b.BalanceQty,
                        Rate = p.Rate,
                        Remark = p.Remark,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        DealUnitId = p.DealUnitId,
                        DealQty = p.DealQty,
                        UnitId = p.Product.UnitId,
                        ProductName = p.Product.ProductName,
                        ProductUidId = tab.PackingLine.ProductUidId,
                        DiscountPer = ((p.Rate * p.DealQty) != 0) ? (p.DiscountPer != 0 && p.DiscountPer != null) ? p.DiscountPer : (p.DiscountAmount * 100 / (p.Rate * p.DealQty)) : 0,
                        //Dimension1Id = p.Dimension1Id,
                        //Dimension1Name = p.Dimension1.Dimension1Name,
                        //Dimension2Id = p.Dimension2Id,
                        //Dimension2Name = p.Dimension2.Dimension2Name,
                        //Specification = p.Specification,
                        //LotNo = tab.LotNo,
                        //DiscountPer = p.DiscountPer

                    }).FirstOrDefault();
        }


        public IEnumerable<ComboBoxResult> GetSaleOrderHelpListForProduct(int Id, int ProductId, string term, int Limit)
        {
            var SaleInvoice = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(Id);
            var Product = new ProductService(_unitOfWork).Find(ProductId);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoice.DocTypeId, SaleInvoice.DivisionId, SaleInvoice.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }


            var list = (from p in db.ViewSaleOrderBalance
                        join t in db.SaleOrderHeader on p.SaleOrderHeaderId equals t.SaleOrderHeaderId
                        join t2 in db.SaleOrderLine on p.SaleOrderLineId equals t2.SaleOrderLineId
                        where p.ProductId == ProductId && 
                        (string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleOrderNo.ToLower().Contains(term.ToLower())
                        )
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == SaleInvoice.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == SaleInvoice.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                        orderby t.DocDate, t.DocNo
                        select new ComboBoxResult
                        {
                            text = p.SaleOrderNo,
                            id = p.SaleOrderLineId.ToString(),
                            TextProp1 = "BalQty: " + p.BalanceQty.ToString(),
                        }
                          ).Take(Limit);

            return list.ToList();
        }




        public IQueryable<ComboBoxResult> GetSaleOrderHelpListForProduct(int Id, int PersonId, string term)
        {
            var Product = new ProductService(_unitOfWork).Find(Id);

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var list = (from p in db.ViewSaleOrderBalance
                        join t in db.SaleOrderHeader on p.SaleOrderHeaderId equals t.SaleOrderHeaderId
                        join t2 in db.SaleOrderLine on p.SaleOrderLineId equals t2.SaleOrderLineId
                        where p.ProductId == Id
                        && p.BuyerId == PersonId
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleOrderNo.ToLower().Contains(term.ToLower()))
                        && p.SiteId == CurrentSiteId
                        && p.DivisionId == CurrentDivisionId
                        orderby t.DocDate, t.DocNo
                        select new ComboBoxResult
                        {
                            text = p.SaleOrderNo,
                            id = p.SaleOrderLineId.ToString(),
                            TextProp1 = "BalQty: " + p.BalanceQty.ToString(),
                        });

            return list;
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
                    &&  p.IsActive == true  
                    orderby p.ProductName
                    select new ComboBoxResult
                    {
                        id = p.ProductId.ToString(),
                        text = p.ProductName,
                    });
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.SaleInvoiceLine
                       where p.SaleInvoiceHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }


        public List<DirectSaleInvoiceLineViewModel> GetSaleDispatchForFilters(SaleInvoiceFilterViewModel vm)
        {
            SaleInvoiceHeader Header = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(vm.SaleInvoiceHeaderId);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.SaleDispatchHeaderId)) { SaleOrderIdArr = vm.SaleDispatchHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            string[] Dimension1IdArr = null;
            if (!string.IsNullOrEmpty(vm.Dimension1Id)) { Dimension1IdArr = vm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dimension1IdArr = new string[] { "NA" }; }

            string[] Dimension2IdArr = null;
            if (!string.IsNullOrEmpty(vm.Dimension2Id)) { Dimension2IdArr = vm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dimension2IdArr = new string[] { "NA" }; }


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


            //ToChange View to get Saleorders instead of goodsreceipts
            List<DirectSaleInvoiceLineViewModel> temp = (from p in db.ViewSaleDispatchBalance
                                                         join l in db.SaleDispatchLine on p.SaleDispatchLineId equals l.SaleDispatchLineId into linetable
                                                         from linetab in linetable.DefaultIfEmpty()
                                                         join t3 in db.PackingLine on linetab.PackingLineId equals t3.PackingLineId into PackingLineTable
                                                         from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                                                         join t in db.SaleDispatchHeader on p.SaleDispatchHeaderId equals t.SaleDispatchHeaderId into table
                                                         from tab in table.DefaultIfEmpty()
                                                         join product in db.Product on p.ProductId equals product.ProductId into table2
                                                         from tab2 in table2.DefaultIfEmpty()
                                                         where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                                                         && (string.IsNullOrEmpty(vm.SaleDispatchHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleDispatchHeaderId.ToString()))
                                                         && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                                                         && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1IdArr.Contains(p.Dimension1Id.ToString()))
                                                         && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2IdArr.Contains(p.Dimension2Id.ToString()))
                                                         && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(tab.DocTypeId.ToString()))
                                                         && (string.IsNullOrEmpty(settings.filterContraSites) ? tab.SiteId == CurrentSiteId : contraSites.Contains(tab.SiteId.ToString()))
                                                         && (string.IsNullOrEmpty(settings.filterContraDivisions) ? tab.DivisionId == CurrentDivisionId : contraDivisions.Contains(tab.DivisionId.ToString()))
                                                         && ((vm.UpToDate == null) ? 1 == 1 : tab.DocDate <= vm.UpToDate)
                                                         && p.BuyerId == Header.SaleToBuyerId
                                                         && p.BalanceQty > 0
                                                         orderby p.SaleDispatchNo, p.Sr
                                                         select new DirectSaleInvoiceLineViewModel
                                                         {
                                                             Dimension1Name = PackingLineTab.Dimension1.Dimension1Name,
                                                             Dimension2Name = PackingLineTab.Dimension2.Dimension2Name,
                                                             BalanceQty = p.BalanceQty,
                                                             Qty = p.BalanceQty,
                                                             SaleDispatchHeaderDocNo = tab.DocNo,
                                                             SaleOrderHeaderDocNo = p.SaleOrderNo,
                                                             ProductName = tab2.ProductName,
                                                             ProductId = p.ProductId,
                                                             SaleInvoiceHeaderId = vm.SaleInvoiceHeaderId,
                                                             SaleDispatchLineId = p.SaleDispatchLineId,
                                                             UnitId = tab2.UnitId,
                                                             UnitName = tab2.Unit.UnitName,
                                                             UnitConversionMultiplier = PackingLineTab.UnitConversionMultiplier,
                                                             DealUnitId = PackingLineTab.DealUnitId,
                                                             DealUnitName = PackingLineTab.DealUnit.UnitName,
                                                             unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                                                             DealunitDecimalPlaces = PackingLineTab.DealUnit.DecimalPlaces,
                                                             Dimension1Id = p.Dimension1Id,
                                                             Dimension2Id = p.Dimension2Id,
                                                             SaleOrderLineId = p.SaleOrderLineId
                                                         }).ToList();

            return FGetProductRate(temp);
        }

        public IQueryable<ComboBoxResult> GetSaleDispatchHelpListForProduct(int PersonId, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var list = (from p in db.ViewSaleDispatchBalance
                        join t in db.SaleDispatchHeader on p.SaleDispatchHeaderId equals t.SaleDispatchHeaderId
                        join t2 in db.SaleDispatchLine on p.SaleDispatchLineId equals t2.SaleDispatchLineId
                        join pt in db.Product on p.ProductId equals pt.ProductId into ProductTable from ProductTab in ProductTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where p.BuyerId == PersonId
                        && ((string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleDispatchNo.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : ProductTab.ProductName.ToLower().Contains(term.ToLower())))
                        && p.SiteId == CurrentSiteId
                        && p.DivisionId == CurrentDivisionId
                        orderby t.DocDate, t.DocNo
                        select new ComboBoxResult
                        {
                            text = ProductTab.ProductName,
                            id = p.SaleDispatchLineId.ToString(),
                            TextProp1 = "Dispatch No: " + p.SaleDispatchNo.ToString(),
                            TextProp2 = "BalQty: " + p.BalanceQty.ToString(),
                            AProp1 = Dimension1Tab.Dimension1Name,
                            AProp2 = Dimension2Tab.Dimension2Name
                        });

            return list;
        }


        public DirectSaleInvoiceLineViewModel GetSaleInvoiceLineForEdit(int id)
        {

            return (from p in db.SaleInvoiceLine
                    join Sid in db.SaleInvoiceLineDetail on p.SaleInvoiceLineId equals Sid.SaleInvoiceLineId into SaleInvoiceLineDetailTable from SaleInvoiceLineDetailTab in SaleInvoiceLineDetailTable.DefaultIfEmpty()
                    join t in db.SaleDispatchLine on p.SaleDispatchLineId equals t.SaleDispatchLineId into table
                    from Dl in table.DefaultIfEmpty()
                    join pl in db.PackingLine on Dl.PackingLineId equals pl.PackingLineId into PackingLineTable
                    from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                    join t3 in db.ViewSaleDispatchBalance on p.SaleDispatchLineId equals t3.SaleDispatchLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join Dh in db.SaleDispatchHeader on Dl.SaleDispatchHeaderId equals Dh.SaleDispatchHeaderId into SaleDispatchHeaderTable
                    from SaleDispatchHeaderTab in SaleDispatchHeaderTable.DefaultIfEmpty()
                    where p.SaleInvoiceLineId == id
                    select new DirectSaleInvoiceLineViewModel
                    {
                        ProductId = p.ProductId,
                        ProductName = p.Product.ProductName,
                        Dimension1Id = PackingLineTab.Dimension1Id,
                        Dimension2Id = PackingLineTab.Dimension2Id,
                        Qty = p.Qty,
                        BalanceQty = (tab3 == null ? p.Qty : tab3.BalanceQty + p.Qty),
                        UnitId = p.Product.UnitId,
                        UnitName = p.Product.Unit.UnitName,
                        DealUnitId = p.DealUnitId,
                        DealUnitName = p.DealUnit.UnitName,
                        DealQty = p.DealQty,
                        Remark = p.Remark,
                        DiscountPer = p.DiscountPer,
                        Amount = p.Amount,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        Rate = p.Rate,
                        SaleInvoiceLineId = p.SaleInvoiceLineId,
                        SaleInvoiceHeaderId = p.SaleInvoiceHeaderId,
                        SaleDispatchLineId = p.SaleDispatchLineId,
                        SaleDispatchHeaderDocNo = SaleDispatchHeaderTab.DocNo,
                        PackingLineId = Dl.PackingLineId,
                        RewardPoints = SaleInvoiceLineDetailTab.RewardPoints
                    }
                        ).FirstOrDefault();

        }

        public IQueryable<ComboBoxResult> GetPendingProductsForSaleInvoice(int Jid, string term)//DocTypeId
        {

            var SaleInvoice = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(Jid);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoice.DocTypeId, SaleInvoice.DivisionId, SaleInvoice.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            return (from p in db.ViewSaleDispatchBalance
                    join t in db.Product on p.ProductId equals t.ProductId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && ProTab.ProductName.ToLower().Contains(term.ToLower()) && p.BuyerId == SaleInvoice.SaleToBuyerId 
                     && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == SaleInvoice.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                     && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == SaleInvoice.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                     && (string.IsNullOrEmpty(term) ? 1 == 1 : ProTab.ProductName.ToLower().Contains(term.ToLower()))
                    group new { p, ProTab } by p.ProductId into g
                    orderby g.Key descending
                    select new ComboBoxResult
                    {
                        id = g.Key.ToString(),
                        text = g.Max(m => m.ProTab.ProductName),

                    }
                        );
        }

        public IEnumerable<ComboBoxResult> GetPendingDispatchForInvoice(int id, string Term)
        {
            var SaleInvoiceHeader = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(id);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoiceHeader.DocTypeId, SaleInvoiceHeader.DivisionId, SaleInvoiceHeader.SiteId);

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




            return (from p in db.ViewSaleDispatchBalance
                    join t in db.SaleDispatchHeader on p.SaleDispatchHeaderId equals t.SaleDispatchHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    where p.BalanceQty > 0
                    && p.BuyerId == SaleInvoiceHeader.SaleToBuyerId
                    && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(tab.DocTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? tab.SiteId == CurrentSiteId : contraSites.Contains(tab.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? tab.DivisionId == CurrentDivisionId : contraDivisions.Contains(tab.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(Term) ? 1 == 1 : p.SaleDispatchNo.ToLower().Contains(Term.ToLower()))

                    group p by p.SaleDispatchHeaderId into g
                    select new ComboBoxResult
                    {
                        id = g.Key.ToString(),
                        text = g.Max(m => m.SaleDispatchNo),
                    }
                        );
        }

        public IEnumerable<ComboBoxResult> FGetPromoCodeList(int ProductId, int BuyerId, DateTime DocDate)
        {
            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterBuyerId = new SqlParameter("@BuyerId", BuyerId);
            SqlParameter SqlParameterDocDate = new SqlParameter("@DocDate", DocDate);

            IEnumerable<ComboBoxResult> PromoCodeList = db.Database.SqlQuery<ComboBoxResult>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetPromoCodeList @ProductId, @BuyerId, @DocDate", SqlParameterProductId, SqlParameterBuyerId, SqlParameterDocDate).ToList();

            return PromoCodeList;
        }

        public List<DirectSaleInvoiceLineViewModel> FGetProductRate(List<DirectSaleInvoiceLineViewModel> SaleInvoiceLineViewModel)
        {
            SaleInvoiceHeader Header = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(SaleInvoiceLineViewModel.FirstOrDefault().SaleInvoiceHeaderId);
            int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Sales).ProcessId;

            SqlParameter SqlParameterPersonId = new SqlParameter("@PersonId", Header.SaleToBuyerId);
            SqlParameter SqlParameterProcessId = new SqlParameter("@ProcessId", ProcessId);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", Header.DivisionId);
            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", Header.SiteId);
            SqlParameter SqlParameterDocDate = new SqlParameter("@DocDate", Header.DocDate);

            List<ProductRate> ProductData = (from L in SaleInvoiceLineViewModel
                                             select new ProductRate
                                             {
                                                 ProductId = L.ProductId,
                                                 Dimension1Id = L.Dimension1Id,
                                                 Dimension2Id = L.Dimension2Id,
                                                 Rate = L.Rate,
                                                 Weightage = L.Qty,
                                                 SaleDispatchLineId = L.SaleDispatchLineId
                                             }).ToList();


            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ProductId");
            dataTable.Columns.Add("Dimension1Id");
            dataTable.Columns.Add("Dimension2Id");
            dataTable.Columns.Add("Rate");
            dataTable.Columns.Add("Weightage");
            dataTable.Columns.Add("SaleDispatchLineId");


            foreach (var item in ProductData)
            {
                var dr = dataTable.NewRow();
                dr["ProductId"] = item.ProductId;
                dr["Dimension1Id"] = item.Dimension1Id;
                dr["Dimension2Id"] = item.Dimension2Id;
                dr["Rate"] = 0;
                dr["Weightage"] = item.Weightage;
                dr["SaleDispatchLineId"] = item.SaleDispatchLineId;
                dataTable.Rows.Add(dr);
            }



            SqlParameter SqlParameterProductData = new SqlParameter("@ProductData", dataTable);
            SqlParameterProductData.TypeName = ConfigurationManager.AppSettings["DataBaseSchema"] + ".TypeProductRate";


            IEnumerable<ProductRate> ProductRateList = db.Database.SqlQuery<ProductRate>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetProductRate @PersonId, @ProcessId, @DivisionId, @SiteId, @DocDate, @ProductData", SqlParameterPersonId, SqlParameterProcessId, SqlParameterDivisionId, SqlParameterSiteId, SqlParameterDocDate, SqlParameterProductData).ToList();


            List<DirectSaleInvoiceLineViewModel> SaleInvoiceLineViewModelWithRate = (from L in SaleInvoiceLineViewModel
                                                                                     join Pl in ProductRateList on new { A1 = L.ProductId, A2 = L.Dimension1Id, A3 = L.Dimension2Id, A4 = L.SaleDispatchLineId } equals new { A1 = Pl.ProductId, A2 = Pl.Dimension1Id, A3 = Pl.Dimension2Id, A4 = Pl.SaleDispatchLineId ?? 0 } into ProductRateListTable
                                                                                     from ProductRateListTab in ProductRateListTable.DefaultIfEmpty()
                                                                                     select new DirectSaleInvoiceLineViewModel
                                                                                     {
                                                                                         Dimension1Name = L.Dimension1Name,
                                                                                         Dimension2Name = L.Dimension2Name,
                                                                                         BalanceQty = L.BalanceQty,
                                                                                         Qty = L.Qty,
                                                                                         SaleDispatchHeaderDocNo = L.SaleDispatchHeaderDocNo,
                                                                                         SaleOrderHeaderDocNo = L.SaleOrderHeaderDocNo,
                                                                                         ProductName = L.ProductName,
                                                                                         ProductId = L.ProductId,
                                                                                         SaleInvoiceHeaderId = L.SaleInvoiceHeaderId,
                                                                                         SaleDispatchLineId = L.SaleDispatchLineId,
                                                                                         UnitId = L.UnitId,
                                                                                         UnitName = L.UnitName,
                                                                                         UnitConversionMultiplier = L.UnitConversionMultiplier,
                                                                                         DealUnitId = L.DealUnitId,
                                                                                         DealUnitName = L.DealUnitName,
                                                                                         unitDecimalPlaces = L.unitDecimalPlaces,
                                                                                         DealunitDecimalPlaces = L.DealunitDecimalPlaces,
                                                                                         Dimension1Id = L.Dimension1Id,
                                                                                         Dimension2Id = L.Dimension2Id,
                                                                                         SaleOrderLineId = L.SaleOrderLineId,
                                                                                         Rate = ProductRateListTab.Rate ?? 0
                                                                                     }).ToList();


            return SaleInvoiceLineViewModelWithRate;
        }

        public IEnumerable<ComboBoxResult> FGetProductUidHelpList(int Id, string term)
        {
            var SaleInvoiceHeader = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(Id);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoiceHeader.DocTypeId, SaleInvoiceHeader.DivisionId, SaleInvoiceHeader.SiteId);

            IEnumerable<ComboBoxResult> ProductUidList = db.Database.SqlQuery<ComboBoxResult>(settings.SqlProcProductUidHelpList).ToList();


            var temp = (from P in ProductUidList
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : P.text.ToLower().Contains(term.ToLower())
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : P.AProp1.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : P.AProp2.ToLower().Contains(term.ToLower()))
                        )
                        select new ComboBoxResult
                        {
                            id = P.id,
                            text = P.text,
                            TextProp1 = P.TextProp1,
                            TextProp2 = P.TextProp2,
                            AProp1 = P.AProp1,
                            AProp2 = P.AProp2
                        }).ToList();

            return temp;
        }

        public IEnumerable<ComboBoxResult> GetPendingPackingHeaderForSaleInvoice(int SaleInvoiceHeaderId, string term)
        {
            //var SaleInvoiceHeader = new SaleInvoiceHeaderService(_unitOfWork).Find(SaleInvoiceHeaderId);
            var SaleInvoiceHeader = db.SaleInvoiceHeader.Find(SaleInvoiceHeaderId);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(SaleInvoiceHeader.DocTypeId, SaleInvoiceHeader.DivisionId, SaleInvoiceHeader.SiteId);

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            return (from p in db.ViewPackingBalance
                    join L in db.PackingLine on p.PackingLineId equals L.PackingLineId into PackingLineTable
                    from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                    where p.BalanceQty > 0
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.PackingNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : PackingLineTab.PackingHeader.DocType.DocumentTypeShortName.ToLower().Contains(term.ToLower())
                    )
                    group new { p, StockTab = PackingLineTab } by new { PackingLineTab.PackingHeaderId } into Result
                    select new ComboBoxResult
                    {
                        id = Result.Key.PackingHeaderId.ToString(),
                        text = Result.Max(i => i.StockTab.PackingHeader.DocType.DocumentTypeShortName + "-" + i.StockTab.PackingHeader.DocNo),
                        TextProp1 = "Date :" + Result.Max(i => i.StockTab.PackingHeader.DocDate),
                        TextProp2 = "Balance :" + Result.Sum(i => i.p.BalanceQty),
                    });
        }

        

        
        public void Dispose()
        {
        }
    }

    public class PromoCodeList
    {
        public int PromoCodeId { get; set; }
        public string PromoCodeName { get; set; }
    }

    public class ProductRate
    {
        public int ProductId { get; set; }
	    public int? Dimension1Id { get; set; }
	    public int? Dimension2Id { get; set; }
        public Decimal? Rate { get; set; }
        public Decimal? Weightage { get; set; }
        public int? SaleDispatchLineId { get; set; }
    }
}
