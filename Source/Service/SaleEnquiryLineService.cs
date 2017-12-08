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
    public interface ISaleEnquiryLineService : IDisposable
    {
        SaleEnquiryLine Create(SaleEnquiryLine s);
        void Delete(int id);
        void Delete(SaleEnquiryLine s);
        SaleEnquiryLine GetSaleEnquiryLine(int id);

        IEnumerable<SaleEnquiryLine> GetSaleEnquiryLineListForHeader(int HeaderId);
        
        SaleEnquiryLineViewModel GetSaleEnquiryLineModel(int id);
        SaleEnquiryLine Find(int id);
        SaleEnquiryLine Find_WithLineDetail(int SaleEnquiryHeaderId, string BuyerSpecification, string BuyerSpecification1, string BuyerSpecification2, string BuyerSpecification3);
        void Update(SaleEnquiryLine s);
        IEnumerable<SaleEnquiryLineIndexViewModel> GetSaleEnquiryLineList(int SaleEnquiryHeaderId);

        IQueryable<SaleEnquiryLineIndexViewModel> GetSaleEnquiryLineListForIndex(int SaleEnquiryHeaderId);

        IQueryable<SaleEnquiryLineIndexViewModel> GetSaleEnquiryLineListForIndex();
        IEnumerable<SaleEnquiryLineBalance> GetSaleEnquiryForProduct(int ProductId,int BuyerId);
        bool CheckForProductExists(int ProductId, int SaleEnquiryHEaderId, int SaleEnquiryLineId);
        bool CheckForProductExists(int ProductId, int SaleEnquiryHEaderId);
        string GetBuyerSKU(int ProductId, int SaleEnquiryHEaderId);
        SaleEnquiryLineBalance GetSaleEnquiry(int LineId);
        SaleEnquiryLineViewModel GetSaleEnquiryDetailForInvoice(int id);

        IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term);
        string GetBuyerSku(int BuyerId);

        IQueryable<ComboBoxResult> GetBuyerSpecification(string term, int filter);
        IQueryable<ComboBoxResult> GetBuyerSpecification1(string term, int filter);
        IQueryable<ComboBoxResult> GetBuyerSpecification2(string term, int filter);
        IQueryable<ComboBoxResult> GetBuyerSpecification3(string term, int filter);
        SaleEnquiryLastTransaction GetLastTransactionDetail(int SaleEnquiryHeaderId);
        int NextId(int id);
        int PrevId(int id);
    }

    public class SaleEnquiryLineService : ISaleEnquiryLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleEnquiryLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public SaleEnquiryLine Create(SaleEnquiryLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleEnquiryLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleEnquiryLine>().Delete(id);
        }

        public void Delete(SaleEnquiryLine s)
        {
            _unitOfWork.Repository<SaleEnquiryLine>().Delete(s);
        }

        public void Update(SaleEnquiryLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleEnquiryLine>().Update(s);
        }

        public SaleEnquiryLine GetSaleEnquiryLine(int id)
        {
            return _unitOfWork.Repository<SaleEnquiryLine>().Query().Get().Where(m => m.SaleEnquiryLineId == id).FirstOrDefault();
            //return (from p in db.SaleEnquiryLine
            //        join t in db.Products on p.ProductId equals t.ProductId into table from tab in table.DefaultIfEmpty()
            //        where p.SaleEnquiryLineId == id
            //        select new SaleEnquiryLineViewModel
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
            //            SaleEnquiryHeaderId=p.SaleEnquiryHeaderId,
            //            SaleEnquiryLineId=p.SaleEnquiryLineId,
            //            Specification=p.Specification,
            //            Product=tab.ProductName,
            //        }

            //            ).FirstOrDefault();
        }



        public IEnumerable<SaleEnquiryLine> GetSaleEnquiryLineListForHeader(int HeaderId)
        {
            return _unitOfWork.Repository<SaleEnquiryLine>().Query().Get().Where(m => m.SaleEnquiryHeaderId == HeaderId).ToList();
        }

        public SaleEnquiryLineViewModel GetSaleEnquiryLineModel(int id)
        {
            //return _unitOfWork.Repository<SaleEnquiryLine>().Query().Get().Where(m => m.SaleEnquiryLineId == id).FirstOrDefault();
            return (from p in db.SaleEnquiryLine
                    join t in db.Product on p.ProductId equals t.ProductId into table
                    from tab in table.DefaultIfEmpty()
                    where p.SaleEnquiryLineId == id
                    select new SaleEnquiryLineViewModel
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
                        SaleEnquiryHeaderId = p.SaleEnquiryHeaderId,
                        SaleEnquiryLineId = p.SaleEnquiryLineId,
                        Specification = p.Specification,
                        ProductName = tab.ProductName,
                    }

                        ).FirstOrDefault();
        }
        public SaleEnquiryLine Find(int id)
        {
            return _unitOfWork.Repository<SaleEnquiryLine>().Find(id);
        }

        public SaleEnquiryLine Find_WithLineDetail(int SaleEnquiryHeaderId, string BuyerSpecification, string BuyerSpecification1, string BuyerSpecification2, string BuyerSpecification3)
        {
            //return _unitOfWork.Repository<SaleEnquiryLine>().Find(id);

            return (from p in db.SaleEnquiryLine
                    join t in db.SaleEnquiryLineExtended  on p.SaleEnquiryLineId equals t.SaleEnquiryLineId into table
                    from tab in table.DefaultIfEmpty()
                    where (p.SaleEnquiryHeaderId == SaleEnquiryHeaderId) && (tab.BuyerSpecification == BuyerSpecification) && (tab.BuyerSpecification1 == BuyerSpecification1) && (tab.BuyerSpecification2 == BuyerSpecification2) && (tab.BuyerSpecification3 == BuyerSpecification3) 
                    select p
            ).FirstOrDefault();
        }

        public IEnumerable<SaleEnquiryLineIndexViewModel> GetSaleEnquiryLineList(int SaleEnquiryHeaderId)
        {
            //return _unitOfWork.Repository<SaleEnquiryLine>().Query().Include(m => m.Product).Include(m=>m.SaleEnquiryHeader).Get().Where(m => m.SaleEnquiryHeaderId == SaleEnquiryHeaderId).ToList();

            return (from p in db.SaleEnquiryLine
                    where p.SaleEnquiryHeaderId == SaleEnquiryHeaderId
                    select new SaleEnquiryLineIndexViewModel
                    {
                        Amount = p.Amount,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        DueDate = p.DueDate,
                        ProductName = p.Product.ProductName,
                        ProductId = p.ProductId,
                        Qty = p.Qty,
                        Rate = p.Rate,
                        SaleEnquiryHeaderId = p.SaleEnquiryHeaderId,
                        SaleEnquiryLineId = p.SaleEnquiryLineId,
                        SaleEnquiryHeaderDocNo=p.SaleEnquiryHeader.DocNo
                    }


                );


        }

        public IQueryable<SaleEnquiryLineIndexViewModel> GetSaleEnquiryLineListForIndex(int SaleEnquiryHeaderId)
        {

            var temp = from p in db.SaleEnquiryLine
                       join Pe in db.SaleEnquiryLineExtended on p.SaleEnquiryLineId equals Pe.SaleEnquiryLineId into SaleEnquiryLineTable from SaleEnquiryLineTab in SaleEnquiryLineTable.DefaultIfEmpty()
                       join t in db.ViewSaleEnquiryBalance on p.SaleEnquiryLineId equals t.SaleEnquiryLineId into table from svb in table.DefaultIfEmpty()
                       join t1 in db.SaleEnquiryHeader on p.SaleEnquiryHeaderId equals t1.SaleEnquiryHeaderId into table1 from tab1 in table1.DefaultIfEmpty()
                       join pb in db.ViewProductBuyer on new { p.ProductId, BuyerId = tab1.SaleToBuyerId } equals new { pb.ProductId, BuyerId = pb.BuyerId } into table2
                       from tab2 in table2.DefaultIfEmpty()
                       orderby p.SaleEnquiryLineId
                       where p.SaleEnquiryHeaderId==SaleEnquiryHeaderId
                       select new SaleEnquiryLineIndexViewModel
                       {
                           BuyerSku=tab2.ProductName,
                           DealQty = p.DealQty,
                           DealUnitId=p.DealUnitId,
                           Specification=p.Specification,
                           Rate=p.Rate,
                           Amount = p.Amount,
                           DueDate = p.DueDate,
                           ProductName = tab2.ProductName,
                           BuyerSpecification = SaleEnquiryLineTab.BuyerSpecification,
                           BuyerSpecification1 = SaleEnquiryLineTab.BuyerSpecification1,
                           BuyerSpecification3 = SaleEnquiryLineTab.BuyerSpecification3,
                           BuyerSpecification2 = SaleEnquiryLineTab.BuyerSpecification2,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Name = p.Dimension2.Dimension2Name,
                           Dimension3Name = p.Dimension3.Dimension3Name,
                           Dimension4Name = p.Dimension4.Dimension4Name,
                           Qty = p.Qty,
                           SaleEnquiryHeaderId = p.SaleEnquiryHeaderId,
                           SaleEnquiryLineId = p.SaleEnquiryLineId,
                           Remark=p.Remark,
                           ProgressPerc = (p.Qty == 0 ? 0 : (int)((((p.Qty - ((decimal?)svb.BalanceQty ?? (decimal)0)) / p.Qty) * 100))),
                           unitDecimalPlaces=p.Unit.DecimalPlaces,
                           UnitId=p.UnitId,
                       };
            return temp;
        }

        

        public IEnumerable<SaleEnquiryLineBalance> GetSaleEnquiryForProduct(int ProductId,int BuyerId)
        {
            return (from p in db.ViewSaleEnquiryBalance
                    where p.ProductId == ProductId && p.BuyerId == BuyerId && p.BalanceQty > 0
                    select new SaleEnquiryLineBalance
                    {
                        SaleEnquiryDocNo = p.SaleEnquiryNo,
                        SaleEnquiryLineId = p.SaleEnquiryLineId
                    }
                ).ToList();

        }
        public SaleEnquiryLineBalance GetSaleEnquiry(int LineId)
        {
            //var temp = _unitOfWork.Repository<SaleEnquiryLine>().Query()
            //    .Include(m => m.SaleEnquiryHeader)
            //    .Include(m => m.Product)
            //    .Get().Where(m => m.ProductId == ProductId);

            //List<SaleEnquiryLineBalance> SaleEnquiryLineBalance = new List<SaleEnquiryLineBalance>();
            //foreach (var item in temp)
            //{
            //    SaleEnquiryLineBalance.Add(new SaleEnquiryLineBalance
            //    {
            //        SaleEnquiryLineId = item.SaleEnquiryLineId,
            //        SaleEnquiryDocNo = item.SaleEnquiryHeader.DocNo
            //    });
            //}

            return (from p in db.SaleEnquiryLine
                    join t in db.SaleEnquiryHeader on p.SaleEnquiryHeaderId equals t.SaleEnquiryHeaderId into table from tab in table
                    where p.SaleEnquiryLineId == LineId
                    select new SaleEnquiryLineBalance
                    {
                        SaleEnquiryLineId = p.SaleEnquiryLineId,
                        SaleEnquiryDocNo = tab.DocNo
                    }


                ).FirstOrDefault();

        }

        public bool CheckForProductExists(int ProductId, int SaleEnquiryHeaderId, int SaleEnquiryLineId)
        {

            SaleEnquiryLine temp = (from p in db.SaleEnquiryLine
                                  where p.ProductId == ProductId && p.SaleEnquiryHeaderId == SaleEnquiryHeaderId &&p.SaleEnquiryLineId!=SaleEnquiryLineId
                                  select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }
        public bool CheckForProductExists(int ProductId, int SaleEnquiryHeaderId)
        {

            SaleEnquiryLine temp = (from p in db.SaleEnquiryLine
                                  where p.ProductId == ProductId && p.SaleEnquiryHeaderId == SaleEnquiryHeaderId
                                  select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }

        public string GetBuyerSKU(int ProductId, int SaleEnquiryHEaderId)
        {
            int BuyerID = new SaleEnquiryHeaderService(_unitOfWork).Find(SaleEnquiryHEaderId).SaleToBuyerId;

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

        public SaleEnquiryLineViewModel GetSaleEnquiryDetailForInvoice(int id)
        {

            return (from t in db.ViewSaleEnquiryBalance 
                    join p in db.SaleEnquiryLine on t.SaleEnquiryLineId equals p.SaleEnquiryLineId 
                    where p.SaleEnquiryLineId==id
                    select new SaleEnquiryLineViewModel
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
                        SaleEnquiryDocNo = p.SaleEnquiryHeader.DocNo,
                        SaleEnquiryHeaderId = p.SaleEnquiryHeaderId,
                        SaleEnquiryLineId = p.SaleEnquiryLineId,
                        Specification = p.Specification,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        UnitId = p.Product.UnitId,
                        UnitName=p.Product.Unit.UnitName,
                        
                    }

                        ).FirstOrDefault();


        }

        public IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term)
        {

            var SaleEnquiry = new SaleEnquiryHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleEnquirySettingsService(_unitOfWork).GetSaleEnquirySettingsForDucument(SaleEnquiry.DocTypeId, SaleEnquiry.DivisionId, SaleEnquiry.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] Products = null;
            if (!string.IsNullOrEmpty(settings.filterProducts)) { Products = settings.filterProducts.Split(",".ToCharArray()); }
            else { Products = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }

            return (from pb in db.ViewProductBuyer
                    join Pt in db.Product on pb.ProductId equals Pt.ProductId into ProductTable from ProductTab in ProductTable.DefaultIfEmpty()
                    where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(ProductTab.ProductGroup.ProductTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProducts) ? 1 == 1 : Products.Contains(ProductTab.ProductId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(ProductTab.ProductGroupId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : pb.ProductName.ToLower().Contains(term.ToLower()))
                    && pb.BuyerId == SaleEnquiry.SaleToBuyerId
                    orderby pb.ProductName
                    select new ComboBoxResult
                    {
                        id = pb.ProductId.ToString(),
                        text = pb.ProductName,
                        AProp1 = pb.BuyerSpecification,
                        AProp2 = pb.BuyerSpecification1,
                        TextProp1 = pb.BuyerSpecification2,
                        TextProp2 = pb.BuyerSpecification3
                    });
        }

        public IQueryable<SaleEnquiryLineIndexViewModel> GetSaleEnquiryLineListForIndex()
        {

            var temp = from p in db.SaleEnquiryLine
                       join Pe in db.SaleEnquiryLineExtended on p.SaleEnquiryLineId equals Pe.SaleEnquiryLineId into SaleEnquiryLineTable
                       from SaleEnquiryLineTab in SaleEnquiryLineTable.DefaultIfEmpty()
                       join t1 in db.SaleEnquiryHeader on p.SaleEnquiryHeaderId equals t1.SaleEnquiryHeaderId into table1
                       from tab1 in table1.DefaultIfEmpty()
                       where p.ProductId == null
                       orderby p.SaleEnquiryLineId
                       select new SaleEnquiryLineIndexViewModel
                       {
                           SaleEnquiryLineId = p.SaleEnquiryLineId,
                           SaleEnquiryHeaderDocNo = tab1.DocNo,
                           SaleEnquiryHeaderDocDate = tab1.DocDate,
                           SaleToBuyerId = tab1.SaleToBuyerId,
                           SaleToBuyerName = tab1.SaleToBuyer.Name,
                           BuyerSpecification = SaleEnquiryLineTab.BuyerSpecification,
                           BuyerSpecification1 = SaleEnquiryLineTab.BuyerSpecification1,
                           BuyerSpecification3 = SaleEnquiryLineTab.BuyerSpecification3,
                           BuyerSpecification2 = SaleEnquiryLineTab.BuyerSpecification2,
                       };
            return temp;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                //temp = (from p in db.SaleEnquiryLine
                //        where p.ProductId == null
                //        orderby p.SaleEnquiryLineId
                //        select p.SaleEnquiryLineId).AsEnumerable().SkipWhile(p => p != id && p > id).Skip(1).FirstOrDefault();

                temp = (from p in db.SaleEnquiryLine
                        where p.ProductId == null && p.SaleEnquiryLineId > id
                        orderby p.SaleEnquiryLineId
                        select p.SaleEnquiryLineId).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleEnquiryLine
                        where p.ProductId == null
                        orderby p.SaleEnquiryLineId
                        select p.SaleEnquiryLineId).FirstOrDefault();
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
                //temp = (from p in db.SaleEnquiryLine
                //        where p.ProductId == null
                //        orderby p.SaleEnquiryLineId
                //        select p.SaleEnquiryLineId).AsEnumerable().TakeWhile(p => p != id && p < id).LastOrDefault();

                temp = (from p in db.SaleEnquiryLine
                        where p.ProductId == null && p.SaleEnquiryLineId < id
                        orderby p.SaleEnquiryLineId descending
                        select p.SaleEnquiryLineId).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleEnquiryLine
                        where p.ProductId == null
                        orderby p.SaleEnquiryLineId
                        select p.SaleEnquiryLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public string GetBuyerSku(int BuyerId)
        {
            var Query = (from Le in db.ProductBuyer
                         where Le.BuyerId == BuyerId && Le.BuyerSku != null
                         group new { Le } by new { Le.BuyerSku } into Result
                         select new
                         {
                             BuyerSku = Result.Key.BuyerSku,
                         }
                        );

            return string.Join(",", Query.Select(m => m.BuyerSku).ToList());
        }


        public IQueryable<ComboBoxResult> GetBuyerSpecification(string term, int filter)
        {
            var list = (from Le in db.SaleEnquiryLineExtended
                         join L in db.SaleEnquiryLine on Le.SaleEnquiryLineId equals L.SaleEnquiryLineId into SaleEnquiryLineTable from SaleEnquiryLineTab in SaleEnquiryLineTable.DefaultIfEmpty()
                         join H in db.SaleEnquiryHeader on SaleEnquiryLineTab.SaleEnquiryHeaderId equals H.SaleEnquiryHeaderId into SaleEnquiryHeaderTable from SaleEnquiryHeaderTab in SaleEnquiryHeaderTable.DefaultIfEmpty()
                         where SaleEnquiryHeaderTab.SaleToBuyerId == filter && Le.BuyerSpecification != null
                         && (string.IsNullOrEmpty(term) ? 1 == 1 : Le.BuyerSpecification .ToLower().Contains(term.ToLower()))
                         group new  { Le } by new { Le.BuyerSpecification } into Result
                         orderby Result.Key.BuyerSpecification
                         select new ComboBoxResult
                         {
                            id = Result.Key.BuyerSpecification,
                            text = Result.Key.BuyerSpecification
                         });
            return list;
        }

        public IQueryable<ComboBoxResult> GetBuyerSpecification1(string term, int filter)
        {
            var list = (from Le in db.SaleEnquiryLineExtended
                        join L in db.SaleEnquiryLine on Le.SaleEnquiryLineId equals L.SaleEnquiryLineId into SaleEnquiryLineTable
                        from SaleEnquiryLineTab in SaleEnquiryLineTable.DefaultIfEmpty()
                        join H in db.SaleEnquiryHeader on SaleEnquiryLineTab.SaleEnquiryHeaderId equals H.SaleEnquiryHeaderId into SaleEnquiryHeaderTable
                        from SaleEnquiryHeaderTab in SaleEnquiryHeaderTable.DefaultIfEmpty()
                        where SaleEnquiryHeaderTab.SaleToBuyerId == filter && Le.BuyerSpecification1 != null
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : Le.BuyerSpecification1.ToLower().Contains(term.ToLower()))
                        group new { Le } by new { Le.BuyerSpecification1 } into Result
                        orderby Result.Key.BuyerSpecification1
                        select new ComboBoxResult
                        {
                            id = Result.Key.BuyerSpecification1,
                            text = Result.Key.BuyerSpecification1
                        });
            return list;
        }

        public IQueryable<ComboBoxResult> GetBuyerSpecification2(string term, int filter)
        {
            var list = (from Le in db.SaleEnquiryLineExtended
                        join L in db.SaleEnquiryLine on Le.SaleEnquiryLineId equals L.SaleEnquiryLineId into SaleEnquiryLineTable
                        from SaleEnquiryLineTab in SaleEnquiryLineTable.DefaultIfEmpty()
                        join H in db.SaleEnquiryHeader on SaleEnquiryLineTab.SaleEnquiryHeaderId equals H.SaleEnquiryHeaderId into SaleEnquiryHeaderTable
                        from SaleEnquiryHeaderTab in SaleEnquiryHeaderTable.DefaultIfEmpty()
                        where SaleEnquiryHeaderTab.SaleToBuyerId == filter && Le.BuyerSpecification2 != null
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : Le.BuyerSpecification2.ToLower().Contains(term.ToLower()))
                        group new { Le } by new { Le.BuyerSpecification2 } into Result
                        orderby Result.Key.BuyerSpecification2
                        select new ComboBoxResult
                        {
                            id = Result.Key.BuyerSpecification2,
                            text = Result.Key.BuyerSpecification2
                        });
            return list;
        }

        public IQueryable<ComboBoxResult> GetBuyerSpecification3(string term, int filter)
        {
            var list = (from Le in db.SaleEnquiryLineExtended
                        join L in db.SaleEnquiryLine on Le.SaleEnquiryLineId equals L.SaleEnquiryLineId into SaleEnquiryLineTable
                        from SaleEnquiryLineTab in SaleEnquiryLineTable.DefaultIfEmpty()
                        join H in db.SaleEnquiryHeader on SaleEnquiryLineTab.SaleEnquiryHeaderId equals H.SaleEnquiryHeaderId into SaleEnquiryHeaderTable
                        from SaleEnquiryHeaderTab in SaleEnquiryHeaderTable.DefaultIfEmpty()
                        where SaleEnquiryHeaderTab.SaleToBuyerId == filter && Le.BuyerSpecification3 != null
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : Le.BuyerSpecification3.ToLower().Contains(term.ToLower()))
                        group new { Le } by new { Le.BuyerSpecification3 } into Result
                        orderby Result.Key.BuyerSpecification3
                        select new ComboBoxResult
                        {
                            id = Result.Key.BuyerSpecification3,
                            text = Result.Key.BuyerSpecification3
                        });
            return list;
        }



        public SaleEnquiryLastTransaction GetLastTransactionDetail(int SaleEnquiryHeaderId)
        {
            SaleEnquiryLastTransaction LastTransactionDetail = (from L in db.SaleEnquiryLine
                                                                join Le in db.SaleEnquiryLineExtended on L.SaleEnquiryLineId equals Le.SaleEnquiryLineId into SaleEnquiryLineExtendedTable
                                                                from SaleEnquiryLineExtendedTab in SaleEnquiryLineExtendedTable.DefaultIfEmpty()
                                                                orderby L.SaleEnquiryLineId descending
                                                                where L.SaleEnquiryHeaderId == SaleEnquiryHeaderId
                                                                select new SaleEnquiryLastTransaction
                                         {
                                             BuyerSpecification = SaleEnquiryLineExtendedTab.BuyerSpecification,
                                             BuyerSpecification1 = SaleEnquiryLineExtendedTab.BuyerSpecification1,
                                             BuyerSpecification2 = SaleEnquiryLineExtendedTab.BuyerSpecification2,
                                             BuyerSpecification3 = SaleEnquiryLineExtendedTab.BuyerSpecification3,
                                         }).FirstOrDefault();

            return LastTransactionDetail;
        }


        public IEnumerable<SaleEnquiryLineListViewModel> GetPendingSaleEnquiries(int id)
        {
            return (from p in db.ViewSaleEnquiryBalance
                    join t in db.SaleEnquiryHeader on p.SaleEnquiryHeaderId equals t.SaleEnquiryHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in db.SaleEnquiryLine on p.SaleEnquiryLineId equals t1.SaleEnquiryLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.ProductId == id && p.BalanceQty > 0
                    select new SaleEnquiryLineListViewModel
                    {
                        SaleEnquiryLineId = p.SaleEnquiryLineId,
                        SaleEnquiryHeaderId = p.SaleEnquiryHeaderId,
                        DocNo = tab.DocNo,
                    });
        }

        public void Dispose()
        {
        }
    }

    public class SaleEnquiryLastTransaction
    {
        public string BuyerSpecification { get; set; }
        public string BuyerSpecification1 { get; set; }
        public string BuyerSpecification2 { get; set; }
        public string BuyerSpecification3 { get; set; }
    }
}
