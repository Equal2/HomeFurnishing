using Data.Infrastructure;
using Data.Models;
using Model;
using Model.Models;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;
using Model.ViewModel;

namespace Service
{
    public interface ISaleDeliveryOrderLineService : IDisposable
    {
        SaleDeliveryOrderLine Create(SaleDeliveryOrderLine s);
        void Delete(int id);
        void Delete(SaleDeliveryOrderLine s);
        SaleDeliveryOrderLine Find(int id);
        IQueryable<SaleDeliveryOrderLineViewModel> GetSaleDeliveryOrderLineList(int id);
        void Update(SaleDeliveryOrderLine s);
        IEnumerable<SaleDeliveryOrderLineViewModel> GetSaleOrderLineForMultiSelect(SaleDeliveryOrderFilterViewModel svm);
        SaleDeliveryOrderLineViewModel GetSaleDeliveryOrderLine(int id);
        IEnumerable<ComboBoxList> GetPendingProductsForSaleDelivery(int sid, string term);
        IEnumerable<ComboBoxList> GetPendingSaleOrders(int sid, string term);
        IEnumerable<SaleDeliveryProductHelpList> GetProductHelpList(int Id, string term, int Limit);
        SaleOrderLineViewModel GetSaleOrderDetailBalanceForDelivery(int id);
    }
    public class SaleDeliveryOrderLineService : ISaleDeliveryOrderLineService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleDeliveryOrderLineService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public SaleDeliveryOrderLine Create(SaleDeliveryOrderLine s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDeliveryOrderLine>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleDeliveryOrderLine>().Delete(id);
        }
        public void Delete(SaleDeliveryOrderLine s)
        {
            _unitOfWork.Repository<SaleDeliveryOrderLine>().Delete(s);
        }
        public void Update(SaleDeliveryOrderLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDeliveryOrderLine>().Update(s);
        }

        public SaleDeliveryOrderLine Find(int id)
        {
            return _unitOfWork.Repository<SaleDeliveryOrderLine>().Find(id);
        }



        public IQueryable<SaleDeliveryOrderLineViewModel> GetSaleDeliveryOrderLineList(int id)
        {            

            var temp = from p in db.SaleDeliveryOrderLine
                       where p.SaleDeliveryOrderHeaderId==id
                       orderby p.SaleDeliveryOrderLineId
                       select new SaleDeliveryOrderLineViewModel
                       {
                           DueDate=p.DueDate,
                           Qty=p.Qty,
                           Remark=p.Remark,
                           SaleDeliveryOrderLineId=p.SaleDeliveryOrderLineId,
                           SaleOrderDocNo=p.SaleOrderLine.SaleOrderHeader.DocNo,
                           SaleOrderLineId=p.SaleOrderLineId,
                           ProductName=p.SaleOrderLine.Product.ProductName,
                           UnitDecimalPlaces=p.SaleOrderLine.Product.Unit.DecimalPlaces,
                           UnitName=p.SaleOrderLine.Product.Unit.UnitName,
                       };
            return temp;
        }

        public IEnumerable<SaleDeliveryOrderLineViewModel> GetSaleOrderLineForMultiSelect(SaleDeliveryOrderFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.SaleOrderId)) { SaleOrderIdArr = svm.SaleOrderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewSaleOrderBalanceForCancellation
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (svm.BuyerId == 0 ? 1 == 1 : p.BuyerId == svm.BuyerId)
                        && (string.IsNullOrEmpty(svm.SaleOrderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        select new SaleDeliveryOrderLineViewModel
                        {
                            BalanceQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            SaleOrderDocNo = p.SaleOrderNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            SaleDeliveryOrderHeaderId = svm.SaleDeliveryOrderHeaderId,
                            SaleOrderLineId = p.SaleOrderLineId,
                            UnitDecimalPlaces=tab2.Unit.DecimalPlaces,
                        });
            return temp;
        }

        public SaleDeliveryOrderLineViewModel GetSaleDeliveryOrderLine(int id)
        {

            return (from p in db.SaleDeliveryOrderLine
                    join t in db.ViewSaleOrderBalanceForCancellation on p.SaleOrderLineId equals t.SaleOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    where p.SaleDeliveryOrderLineId == id
                    select new SaleDeliveryOrderLineViewModel
                    {
                        Qty = p.Qty,
                        BalanceQty = p.Qty + (tab == null ? 0 : tab.BalanceQty),
                        DueDate = p.DueDate,
                        ProductId = p.SaleOrderLine.ProductId,
                        ProductName = p.SaleOrderLine.Product.ProductName,
                        Remark = p.Remark,
                        SaleDeliveryOrderLineId = p.SaleDeliveryOrderLineId,
                        SaleOrderDocNo = p.SaleOrderLine.SaleOrderHeader.DocNo,
                        SaleOrderLineId = p.SaleOrderLineId,
                        SaleDeliveryOrderHeaderId=p.SaleDeliveryOrderHeaderId,
                        LockReason=p.LockReason,
                    }
                        ).FirstOrDefault();


        }

        public IEnumerable<ComboBoxList> GetPendingSaleOrders(int sid, string term)//DocTypeId
        {

            var SaleDelivery = new SaleDeliveryOrderHeaderService(_unitOfWork).Find(sid);

            //var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

            //string[] ContraSites = null;
            //if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            //else { ContraSites = new string[] { "NA" }; }

            //string[] ContraDivisions = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            //else { ContraDivisions = new string[] { "NA" }; }

            return (from p in db.ViewSaleOrderBalanceForCancellation
                    join t in db.ViewSaleOrderHeader on p.SaleOrderHeaderId equals t.SaleOrderHeaderId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && (ProTab.DocNo.ToLower().Contains(term.ToLower()) || ProTab.BuyerOrderNo.ToLower().Contains(term.ToLower()) ) && p.BuyerId == SaleDelivery.BuyerId                    
                    group new { p, ProTab } by p.SaleOrderHeaderId into g
                    select new ComboBoxList
                    {
                        Id = g.Key,
                        PropFirst = g.Max(m => m.ProTab.DocNo + " | " + m.ProTab.BuyerOrderNo),
                    }
                        ).Take(20);
        }

        public IEnumerable<ComboBoxList> GetPendingProductsForSaleDelivery(int sid, string term)//DocTypeId
        {

            var SaleDelivery = new SaleDeliveryOrderHeaderService(_unitOfWork).Find(sid);

           // var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

            //string[] ContraSites = null;
            //if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            //else { ContraSites = new string[] { "NA" }; }

            //string[] ContraDivisions = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            //else { ContraDivisions = new string[] { "NA" }; }

            return (from p in db.ViewSaleOrderBalanceForCancellation
                    join t in db.Product on p.ProductId equals t.ProductId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && ProTab.ProductName.ToLower().Contains(term.ToLower()) && SaleDelivery.BuyerId == p.BuyerId                   
                    group new { p, ProTab } by p.ProductId into g
                    orderby g.Key descending
                    select new ComboBoxList
                    {
                        Id = g.Key,
                        PropFirst = g.Max(m => m.ProTab.ProductName)
                    }
                        ).Take(20);
        }


        public IEnumerable<SaleDeliveryProductHelpList> GetProductHelpList(int Id, string term, int Limit)
        {
            var SAleDeliveryOrder = new SaleDeliveryOrderHeaderService(_unitOfWork).Find(Id);

            //var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);


            //string[] ProductTypes = null;
            //if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            //else { ProductTypes = new string[] { "NA" }; }

            //string[] ContraSites = null;
            //if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            //else { ContraSites = new string[] { "NA" }; }

            //string[] ContraDivisions = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            //else { ContraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewSaleOrderBalanceForCancellation                        
                        join t in db.SaleOrderHeader on p.SaleOrderHeaderId equals t.SaleOrderHeaderId
                        join t2 in db.SaleOrderLine on p.SaleOrderLineId equals t2.SaleOrderLineId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleOrderNo.ToLower().Contains(term.ToLower())                        
                        || string.IsNullOrEmpty(term) ? 1 == 1 : t.BuyerOrderNo.ToLower().Contains(term.ToLower())
                        )
                        && p.BuyerId == SAleDeliveryOrder.BuyerId
                        orderby t.DocDate, t.DocNo
                        select new SaleDeliveryProductHelpList
                        {
                            ProductName = p.Product.ProductName,
                            ProductId = p.ProductId,
                            Specification = t2.Specification,
                            SaleOrderDocNo=p.SaleOrderNo,
                            SaleOrderLineId=p.SaleOrderLineId,
                            Qty = p.BalanceQty,
                        }
                          ).Take(Limit);

            return list.ToList();
        }


        public SaleOrderLineViewModel GetSaleOrderDetailBalanceForDelivery(int id)
        {
            return (from b in db.ViewSaleOrderBalanceForCancellation
                    join p in db.SaleOrderLine on b.SaleOrderLineId equals p.SaleOrderLineId                                       
                    where b.SaleOrderLineId== id
                    select new SaleOrderLineViewModel
                    {
                        ProductId=p.ProductId,
                        ProductName=p.Product.ProductName,
                        Qty=b.BalanceQty,
                        Specification=p.Specification,
                        SaleOrderLineId=p.SaleOrderLineId,
                        SaleOrderDocNo=b.SaleOrderNo,
                        
                    }).FirstOrDefault();

        }

        public SaleDeliveryOrderCancelLineViewModel GetLineDetail(int id)
        {
            return (from p in db.ViewSaleDeliveryOrderBalance
                    join t1 in db.SaleDeliveryOrderLine on p.SaleDeliveryOrderLineId equals t1.SaleDeliveryOrderLineId
                    join sol in db.SaleOrderLine on t1.SaleOrderLineId equals sol.SaleOrderLineId
                    join t2 in db.Product on sol.ProductId equals t2.ProductId
                    where p.SaleDeliveryOrderLineId == id
                    select new SaleDeliveryOrderCancelLineViewModel
                    {
                        Dimension1Name = sol.Dimension1.Dimension1Name,
                        Dimension2Name = sol.Dimension2.Dimension2Name,
                        Dimension1Id = sol.Dimension1Id,
                        Dimension2Id = sol.Dimension2Id,
                        //LotNo = t1.LotNo,
                        Qty = p.BalanceQty,
                        Specification = sol.Specification,
                        UnitId = t2.UnitId,
                        DealUnitId = sol.DealUnitId,
                        OrderDealQty = sol.DealQty,
                        OrderQty = t1.Qty,
                        DealQty = (p.BalanceQty * sol.UnitConversionMultiplier) ?? 0,
                        UnitConversionMultiplier = sol.UnitConversionMultiplier,
                        DealunitDecimalPlaces = sol.DealUnit.DecimalPlaces,
                        Rate = sol.Rate,
                    }
                        ).FirstOrDefault();

        }


        public void Dispose()
        {
        }   
    }
}
