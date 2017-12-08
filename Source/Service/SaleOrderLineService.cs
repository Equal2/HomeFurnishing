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


namespace Service
{
    public interface ISaleOrderLineService : IDisposable
    {
        SaleOrderLine Create(SaleOrderLine s);
        void Delete(int id);
        void Delete(SaleOrderLine s);
        SaleOrderLine GetSaleOrderLine(int id);
        SaleOrderLineViewModel GetSaleOrderLineModel(int id);
        SaleOrderCancelLineViewModel GetSaleOrderLineVM(int id);
        SaleOrderLine Find(int id);
        void Update(SaleOrderLine s);
        IEnumerable<SaleOrderLineIndexViewModel> GetSaleOrderLineList(int SaleOrderHeaderId);

        IQueryable<SaleOrderLineIndexViewModel> GetSaleOrderLineListForIndex(int SaleOrderHeaderId);
        IEnumerable<SaleOrderLineBalance> GetSaleOrderForProduct(int ProductId,int BuyerId);
        bool CheckForProductExists(int ProductId, int SaleOrderHEaderId, int SaleOrderLineId);
        bool CheckForProductExists(int ProductId, int SaleOrderHEaderId);
        string GetBuyerSKU(int ProductId, int SaleOrderHEaderId);
        SaleOrderLineBalance GetSaleOrder(int LineId);
        SaleOrderLineViewModel GetSaleOrderDetailForInvoice(int id);

        IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term);
    }

    public class SaleOrderLineService : ISaleOrderLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleOrderLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public SaleOrderLine Create(SaleOrderLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleOrderLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleOrderLine>().Delete(id);
        }

        public void Delete(SaleOrderLine s)
        {
            _unitOfWork.Repository<SaleOrderLine>().Delete(s);
        }

        public void Update(SaleOrderLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleOrderLine>().Update(s);
        }

        public SaleOrderLine GetSaleOrderLine(int id)
        {
            return _unitOfWork.Repository<SaleOrderLine>().Query().Get().Where(m => m.SaleOrderLineId == id).FirstOrDefault();
            //return (from p in db.SaleOrderLine
            //        join t in db.Products on p.ProductId equals t.ProductId into table from tab in table.DefaultIfEmpty()
            //        where p.SaleOrderLineId == id
            //        select new SaleOrderLineViewModel
            //        {
            //            Amount=p.Amount,
            //            CreatedBy=p.CreatedBy,
            //            CreatedDate=p.CreatedDate,
            //            DeliveryQty=p.DeliveryQty,
            //            DeliveryUnitId=p.DeliveryUnitId,
            //            DueDate=p.DueDate,
            //            ModifiedBy=p.ModifiedBy,
            //            ModifiedDate=p.ModifiedDate,
            //            Qty=p.Qty,
            //            Rate=p.Rate,
            //            Remark=p.Remark,
            //            SaleOrderHeaderId=p.SaleOrderHeaderId,
            //            SaleOrderLineId=p.SaleOrderLineId,
            //            Specification=p.Specification,
            //            Product=tab.ProductName,
            //        }

            //            ).FirstOrDefault();
        }
        public SaleOrderLineViewModel GetSaleOrderLineModel(int id)
        {
            //return _unitOfWork.Repository<SaleOrderLine>().Query().Get().Where(m => m.SaleOrderLineId == id).FirstOrDefault();
            return (from p in db.SaleOrderLine
                    join t in db.Product on p.ProductId equals t.ProductId into table
                    from tab in table.DefaultIfEmpty()
                    where p.SaleOrderLineId == id
                    select new SaleOrderLineViewModel
                    {
                        Amount = p.Amount,
                        CreatedBy = p.CreatedBy,
                        CreatedDate = p.CreatedDate,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        DueDate = p.DueDate,
                        ModifiedBy = p.ModifiedBy,
                        ModifiedDate = p.ModifiedDate,
                        Qty = p.Qty,
                        Rate = p.Rate,
                        Remark = p.Remark,
                        SaleOrderHeaderId = p.SaleOrderHeaderId,
                        SaleOrderLineId = p.SaleOrderLineId,
                        Specification = p.Specification,
                        ProductName = tab.ProductName,
                    }

                        ).FirstOrDefault();
        }
        public SaleOrderLine Find(int id)
        {
            return _unitOfWork.Repository<SaleOrderLine>().Find(id);
        }

        public SaleOrderLine Find_ByReferenceDocLineId(int ReferenceDocTypeId, int ReferenceDocLineId)
        {
            return _unitOfWork.Repository<SaleOrderLine>().Query().Get().Where(m => m.ReferenceDocTypeId == ReferenceDocTypeId && m.ReferenceDocLineId == ReferenceDocLineId).FirstOrDefault();
        }

        public IEnumerable<SaleOrderLineIndexViewModel> GetSaleOrderLineList(int SaleOrderHeaderId)
        {
            //return _unitOfWork.Repository<SaleOrderLine>().Query().Include(m => m.Product).Include(m=>m.SaleOrderHeader).Get().Where(m => m.SaleOrderHeaderId == SaleOrderHeaderId).ToList();

            return (from p in db.SaleOrderLine
                    where p.SaleOrderHeaderId == SaleOrderHeaderId
                    select new SaleOrderLineIndexViewModel
                    {
                        Amount = p.Amount,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        DueDate = p.DueDate,
                        ProductName = p.Product.ProductName,
                        Qty = p.Qty,
                        Rate = p.Rate,
                        SaleOrderHeaderId = p.SaleOrderHeaderId,
                        SaleOrderLineId = p.SaleOrderLineId,
                        SaleOrderHeaderDocNo=p.SaleOrderHeader.DocNo,
                        StockId = p.StockId
                    }

                );


        }

        public IQueryable<SaleOrderLineIndexViewModel> GetSaleOrderLineListForIndex(int SaleOrderHeaderId)
        {
            //return _unitOfWork.Repository<SaleOrderLine>().Query().Include(m=>m.Product).Get().Where(m => m.SaleOrderHeaderId == SaleOrderHeaderId);


            //TEsting Comment
            //var temp = from p in db.SaleOrderLine
            //           join product in db.Products on p.ProductId equals product.ProductId into temptable1
            //           from tt1 in temptable1.DefaultIfEmpty()
            //           orderby p.SaleOrderLineId
            //           select new SaleOrderLineIndexViewModel
            //           {
            //               Amount = p.Amount,
            //               DueDate = p.DueDate,
            //               Product = tt1.ProductName,
            //               Qty = p.Qty,
            //               SaleOrderHeaderId = p.SaleOrderHeaderId,
            //               SaleOrderLineId = p.SaleOrderLineId
            //           };
            //return temp;

            //from pb in db.ProductBuyer.Where(m=>m.ProductId==p.ProductId && m.BuyerId==tab1.SaleToBuyerId).DefaultIfEmpty()

            var temp = from p in db.SaleOrderLine
                       join t in db.ViewSaleOrderBalance on p.SaleOrderLineId equals t.SaleOrderLineId into table from svb in table.DefaultIfEmpty()
                       join t1 in db.SaleOrderHeader on p.SaleOrderHeaderId equals t1.SaleOrderHeaderId into table1 from tab1 in table1.DefaultIfEmpty()
                       join pb in db.ProductBuyer on new { p.ProductId, BuyerId = tab1.SaleToBuyerId } equals new { pb.ProductId, BuyerId = pb.BuyerId } into table2
                       from tab2 in table2.DefaultIfEmpty()
                       orderby p.SaleOrderLineId
                       where p.SaleOrderHeaderId==SaleOrderHeaderId
                       select new SaleOrderLineIndexViewModel
                       {
                           BuyerSku=tab2.BuyerSku,
                           DealQty = p.DealQty,
                           DealUnitId=p.DealUnitId,
                           Specification=p.Specification,
                           Rate=p.Rate,
                           Amount = p.Amount,
                           DueDate = p.DueDate,
                           ProductName = p.Product.ProductName,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Name = p.Dimension2.Dimension2Name,
                           Dimension3Name = p.Dimension3.Dimension3Name,
                           Dimension4Name = p.Dimension4.Dimension4Name,
                           Qty = p.Qty,
                           SaleOrderHeaderId = p.SaleOrderHeaderId,
                           SaleOrderLineId = p.SaleOrderLineId,
                           Remark=p.Remark,
                           ProgressPerc = (p.Qty == 0 ? 0 : (int)((((p.Qty - ((decimal?)svb.BalanceQty ?? (decimal)0)) / p.Qty) * 100))),
                           unitDecimalPlaces=p.Product.Unit.DecimalPlaces,
                           UnitId=p.Product.UnitId,
                       };
            return temp;
        }

        public SaleOrderCancelLineViewModel GetSaleOrderLineVM(int id)
        {
            return (from p in db.SaleOrderCancelLine
                    where p.SaleOrderCancelLineId == id
                    select new SaleOrderCancelLineViewModel
                    {
                        SaleOrderLineId=p.SaleOrderLineId,
                        DocNo=p.SaleOrderLine.SaleOrderHeader.DocNo
                    }
                        ).FirstOrDefault();
        }

        public IEnumerable<SaleOrderLineBalance> GetSaleOrderForProduct(int ProductId,int BuyerId)
        {
            //var temp = _unitOfWork.Repository<SaleOrderLine>().Query()
            //    .Include(m => m.SaleOrderHeader)
            //    .Include(m => m.Product)
            //    .Get().Where(m => m.ProductId == ProductId);

            //List<SaleOrderLineBalance> SaleOrderLineBalance = new List<SaleOrderLineBalance>();
            //foreach (var item in temp)
            //{
            //    SaleOrderLineBalance.Add(new SaleOrderLineBalance
            //    {
            //        SaleOrderLineId = item.SaleOrderLineId,
            //        SaleOrderDocNo = item.SaleOrderHeader.DocNo
            //    });
            //}

            //return (from p in db.SaleOrderLine
            //        where p.ProductId == ProductId && p.SaleOrderHeader.Status!=(int)StatusConstants.Closed
            //        select new SaleOrderLineBalance
            //        {
            //            SaleOrderLineId = p.SaleOrderLineId,
            //            SaleOrderDocNo = p.SaleOrderHeader.DocNo
            //        }


            //    ).ToList();

            return (from p in db.ViewSaleOrderBalance
                    where p.ProductId == ProductId && p.BuyerId == BuyerId && p.BalanceQty > 0
                    select new SaleOrderLineBalance
                    {
                        SaleOrderDocNo = p.SaleOrderNo,
                        SaleOrderLineId = p.SaleOrderLineId
                    }
                ).ToList();

        }
        public SaleOrderLineBalance GetSaleOrder(int LineId)
        {
            //var temp = _unitOfWork.Repository<SaleOrderLine>().Query()
            //    .Include(m => m.SaleOrderHeader)
            //    .Include(m => m.Product)
            //    .Get().Where(m => m.ProductId == ProductId);

            //List<SaleOrderLineBalance> SaleOrderLineBalance = new List<SaleOrderLineBalance>();
            //foreach (var item in temp)
            //{
            //    SaleOrderLineBalance.Add(new SaleOrderLineBalance
            //    {
            //        SaleOrderLineId = item.SaleOrderLineId,
            //        SaleOrderDocNo = item.SaleOrderHeader.DocNo
            //    });
            //}

            return (from p in db.SaleOrderLine
                    join t in db.SaleOrderHeader on p.SaleOrderHeaderId equals t.SaleOrderHeaderId into table from tab in table
                    where p.SaleOrderLineId == LineId
                    select new SaleOrderLineBalance
                    {
                        SaleOrderLineId = p.SaleOrderLineId,
                        SaleOrderDocNo = tab.DocNo
                    }


                ).FirstOrDefault();

        }

        public bool CheckForProductExists(int ProductId, int SaleOrderHeaderId, int SaleOrderLineId)
        {

            SaleOrderLine temp = (from p in db.SaleOrderLine
                                  where p.ProductId == ProductId && p.SaleOrderHeaderId == SaleOrderHeaderId &&p.SaleOrderLineId!=SaleOrderLineId
                                  select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }
        public bool CheckForProductExists(int ProductId, int SaleOrderHeaderId)
        {

            SaleOrderLine temp = (from p in db.SaleOrderLine
                                  where p.ProductId == ProductId && p.SaleOrderHeaderId == SaleOrderHeaderId
                                  select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }

        public string GetBuyerSKU(int ProductId, int SaleOrderHEaderId)
        {
            int BuyerID = new SaleOrderHeaderService(_unitOfWork).Find(SaleOrderHEaderId).SaleToBuyerId;

            string BuyerSku = (from p in db.ProductBuyer
                               where p.BuyerId == BuyerID && p.ProductId == ProductId
                               select p.BuyerSku
                                 ).FirstOrDefault();

            if(BuyerSku==null)
            {
                BuyerSku = "";
            }

            return BuyerSku;
        }

        public SaleOrderLineViewModel GetSaleOrderDetailForInvoice(int id)
        {

            return (from t in db.ViewSaleOrderBalance 
                    join p in db.SaleOrderLine on t.SaleOrderLineId equals p.SaleOrderLineId 
                    where p.SaleOrderLineId==id
                    select new SaleOrderLineViewModel
                    {
                        Amount = p.Amount,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        DueDate = p.DueDate,
                        ProductId = p.ProductId,
                        Qty = t.BalanceQty,
                        Rate = p.Rate,
                        Remark = p.Remark,
                        Dimension1Id = p.Dimension1Id,
                        Dimension2Id = p.Dimension2Id,
                        Dimension3Id = p.Dimension3Id,
                        Dimension4Id = p.Dimension4Id,
                        Dimension1Name = p.Dimension1.Dimension1Name,
                        Dimension2Name = p.Dimension2.Dimension2Name,
                        Dimension3Name = p.Dimension3.Dimension3Name,
                        Dimension4Name = p.Dimension4.Dimension4Name,
                        SaleOrderDocNo = p.SaleOrderHeader.DocNo,
                        SaleOrderHeaderId = p.SaleOrderHeaderId,
                        SaleOrderLineId = p.SaleOrderLineId,
                        Specification = p.Specification,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        UnitId = p.Product.UnitId,
                        UnitName=p.Product.Unit.UnitName,
                        
                    }

                        ).FirstOrDefault();


        }

        public IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term)
        {

            var SaleOrder = new SaleOrderHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(SaleOrder.DocTypeId, SaleOrder.DivisionId, SaleOrder.SiteId);

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

        public void Dispose()
        {
        }
    }
}
