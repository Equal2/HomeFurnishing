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


namespace Service
{
    public interface ISaleDeliveryLineService : IDisposable
    {
        SaleDeliveryLine Create(SaleDeliveryLine s);
        void Delete(int id);
        void Delete(SaleDeliveryLine s);
        SaleDeliveryLine GetSaleDeliveryLine(int id);
        IQueryable<SaleDeliveryLine> GetSaleDeliveryLineList(int SaleDeliveryHeaderId);
        SaleDeliveryLine Find(int id);
        void Update(SaleDeliveryLine s);

        bool CheckForProductExists(int ProductId, int SaleDeliveryHEaderId, int SaleDeliveryLineId);
        bool CheckForProductExists(int ProductId, int SaleDeliveryHEaderId);

        IEnumerable<SaleDeliveryLineViewModel> GetSaleDeliveryLineListForIndex(int SaleDeliveryHeaderId);

        IEnumerable<SaleDeliveryLineViewModel> GetSaleInvoicesForFilters(SaleDeliveryFilterViewModel vm);

        SaleDeliveryLineViewModel GetSaleDeliveryLineForEdit(int id);

        

        IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term);

        IQueryable<ComboBoxResult> GetSaleInvoiceHelpListForProduct(int PersonId, string term);
        //SaleDeliveryLineViewModel GetSaleDeliveryDetailBalance(int id);

        //SaleDeliveryLineViewModel GetSaleDeliveryDetailForInvoice(int id);

        

        IQueryable<ComboBoxResult> GetPendingProductsForSaleDelivery(int id, string term);

        IEnumerable<ComboBoxResult> GetPendingSaleInvoiceForDelivery(int id, string term);



    }

    public class SaleDeliveryLineService : ISaleDeliveryLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleDeliveryLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public SaleDeliveryLine Create(SaleDeliveryLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDeliveryLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleDeliveryLine>().Delete(id);
        }

        public void Delete(SaleDeliveryLine s)
        {
            _unitOfWork.Repository<SaleDeliveryLine>().Delete(s);
        }

        public void Update(SaleDeliveryLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDeliveryLine>().Update(s);
        }

        public SaleDeliveryLine GetSaleDeliveryLine(int id)
        {
            return _unitOfWork.Repository<SaleDeliveryLine>().Query().Get().Where(m => m.SaleDeliveryLineId == id).FirstOrDefault();
        }



        public SaleDeliveryLine Find(int id)
        {
            return _unitOfWork.Repository<SaleDeliveryLine>().Find(id);
        }

        public IQueryable<SaleDeliveryLine> GetSaleDeliveryLineList(int SaleDeliveryHeaderId)
        {
            return _unitOfWork.Repository<SaleDeliveryLine>().Query().Get().Where(m => m.SaleDeliveryHeaderId == SaleDeliveryHeaderId);
        }

        public bool CheckForProductExists(int ProductId, int SaleDeliveryHeaderId, int SaleDeliveryLineId)
        {

            SaleDeliveryLine temp = (from p in db.SaleDeliveryLine
                                  where p.SaleDeliveryHeaderId == SaleDeliveryHeaderId &&p.SaleDeliveryLineId!=SaleDeliveryLineId
                                  select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }

        public bool CheckForProductExists(int ProductId, int SaleDeliveryHeaderId)
        {

            SaleDeliveryLine temp = (from p in db.SaleDeliveryLine
                                  where p.SaleDeliveryHeaderId == SaleDeliveryHeaderId
                                  select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }

        public IEnumerable<SaleDeliveryLineViewModel> GetSaleDeliveryLineListForIndex(int SaleDeliveryHeaderId)
        {
            IEnumerable<SaleDeliveryLineViewModel> SaleDeliveryLineViewModel = (from L in db.SaleDeliveryLine
                                                                                where L.SaleDeliveryHeaderId == SaleDeliveryHeaderId
                                                                              orderby L.SaleDeliveryLineId
                                                                                select new SaleDeliveryLineViewModel
                                                                              {
                                                                                  SaleDeliveryLineId = L.SaleDeliveryLineId,
                                                                                  ProductName = L.SaleInvoiceLine.SaleDispatchLine.PackingLine.Product.ProductName,
                                                                                  Dimension1Name = L.SaleInvoiceLine.SaleDispatchLine.PackingLine.Dimension1.Dimension1Name,
                                                                                  Dimension2Name = L.SaleInvoiceLine.SaleDispatchLine.PackingLine.Dimension2.Dimension2Name,
                                                                                  SaleInvoiceHeaderDocNo = L.SaleInvoiceLine.SaleInvoiceHeader.DocNo,
                                                                                  ProductUidName = L.SaleInvoiceLine.SaleDispatchLine.PackingLine.ProductUid.ProductUidName,
                                                                                  Qty = L.Qty,
                                                                                  UnitId = L.SaleInvoiceLine.SaleDispatchLine.PackingLine.Product.UnitId,
                                                                                  DealQty = L.DealQty,
                                                                                  DealUnitId = L.DealUnit.UnitName,
                                                                                  DealUnitDecimalPlaces = L.DealUnit.DecimalPlaces,
                                                                                  unitDecimalPlaces = L.SaleInvoiceLine.SaleDispatchLine.PackingLine.Product.Unit.DecimalPlaces,
                                                                                  Remark = L.Remark,
                                                                                  SaleDeliveryHeaderId=L.SaleDeliveryHeaderId
                                                                              }).Take(2000).ToList();

            return SaleDeliveryLineViewModel;
        }

        public IEnumerable<SaleDeliveryLineViewModel> GetSaleInvoicesForFilters(SaleDeliveryFilterViewModel vm)
        {
            var SaleDeliveryHeader = new SaleDeliveryHeaderService(_unitOfWork).Find(vm.SaleDeliveryHeaderId);

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] Dimension1IdArr = null;
            if (!string.IsNullOrEmpty(vm.Dimension1Id)) { Dimension1IdArr = vm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dimension1IdArr = new string[] { "NA" }; }

            string[] Dimension2IdArr = null;
            if (!string.IsNullOrEmpty(vm.Dimension2Id)) { Dimension2IdArr = vm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dimension2IdArr = new string[] { "NA" }; }

            string[] SaleInvoiceIdArr = null;
            if (!string.IsNullOrEmpty(vm.SaleInvoiceHeaderId)) { SaleInvoiceIdArr = vm.SaleInvoiceHeaderId.Split(",".ToCharArray()); }
            else { SaleInvoiceIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewSaleInvoiceBalanceForDelivery
                        join t in db.SaleInvoiceHeader on p.SaleInvoiceHeaderId equals t.SaleInvoiceHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join t1 in db.SaleInvoiceLine on p.SaleInvoiceLineId equals t1.SaleInvoiceLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        where p.SaleToBuyerId == SaleDeliveryHeader.SaleToBuyerId
                        && (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.SaleInvoiceHeaderId) ? 1 == 1 : SaleInvoiceIdArr.Contains(p.SaleInvoiceHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1IdArr.Contains(p.Dimension1Id.ToString()))
                        && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2IdArr.Contains(p.Dimension2Id.ToString()))
                        && p.BalanceQty > 0
                        orderby p.SaleInvoiceLineId
                        select new SaleDeliveryLineViewModel
                        {
                            //ProductUidIdName = tab1.ProductUid != null ? tab1.ProductUid.ProductUidName : "",
                            Dimension1Name = tab1.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.Dimension2.Dimension2Name,
                            BalanceQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            SaleInvoiceHeaderDocNo = tab.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            Dimension1Id = p.Dimension1Id,
                            Dimension2Id = p.Dimension2Id,
                            SaleDeliveryHeaderId = vm.SaleDeliveryHeaderId,
                            SaleInvoiceLineId = p.SaleInvoiceLineId,
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


        public SaleDeliveryLineViewModel GetSaleDeliveryLineForEdit(int id)
        {
            return (from p in db.SaleDeliveryLine
                    join t3 in db.ViewSaleInvoiceBalanceForDelivery on p.SaleInvoiceLineId equals t3.SaleInvoiceLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    where p.SaleDeliveryLineId == id
                    select new SaleDeliveryLineViewModel
                    {
                        ProductUidId = p.SaleInvoiceLine.SaleDispatchLine.PackingLine.ProductUidId,
                        ProductUidName = p.SaleInvoiceLine.SaleDispatchLine.PackingLine.ProductUid.ProductUidName,
                        ProductId = p.SaleInvoiceLine.SaleDispatchLine.PackingLine.ProductId,
                        ProductName = p.SaleInvoiceLine.SaleDispatchLine.PackingLine.Product.ProductName,
                        SaleInvoiceHeaderDocNo = p.SaleInvoiceLine.SaleInvoiceHeader.DocNo,
                        Qty = p.Qty,
                        BalanceQty = (tab3 == null ? (decimal)p.Qty : tab3.BalanceQty + (decimal)p.Qty),
                        UnitId = p.SaleInvoiceLine.SaleDispatchLine.PackingLine.Product.UnitId,
                        UnitName = p.SaleInvoiceLine.SaleDispatchLine.PackingLine.Product.Unit.UnitName,
                        DealUnitId = p.DealUnitId,
                        DealUnitName = p.DealUnit.UnitName,
                        DealQty = p.DealQty,
                        Remark = p.Remark,
                        Dimension1Id = p.SaleInvoiceLine.SaleDispatchLine.PackingLine.Dimension1Id,
                        Dimension2Id = p.SaleInvoiceLine.SaleDispatchLine.PackingLine.Dimension2Id,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        SaleDeliveryHeaderId = p.SaleDeliveryHeaderId,
                        SaleDeliveryLineId = p.SaleDeliveryLineId,
                        SaleInvoiceLineId = p.SaleInvoiceLineId,
                    }).FirstOrDefault();

        }

        public IEnumerable<ComboBoxResult> GetPendingSaleInvoiceForDelivery(int id, string term)
        {
            var SaleDeliveryHeader = new SaleDeliveryHeaderService(_unitOfWork).Find(id);

            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(SaleDeliveryHeader.DocTypeId, SaleDeliveryHeader.DivisionId, SaleDeliveryHeader.SiteId);

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


            return (from p in db.ViewSaleInvoiceBalanceForDelivery
                    join t in db.SaleInvoiceHeader on p.SaleInvoiceHeaderId equals t.SaleInvoiceHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    where p.BalanceQty > 0
                    && p.SaleToBuyerId == SaleDeliveryHeader.SaleToBuyerId
                    && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(tab.DocTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? tab.SiteId == CurrentSiteId : contraSites.Contains(tab.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? tab.DivisionId == CurrentDivisionId : contraDivisions.Contains(tab.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleInvoiceNo.ToLower().Contains(term.ToLower()))
                    group p by p.SaleInvoiceHeaderId into g
                    select new ComboBoxResult
                    {
                        id = g.Key.ToString(),
                        text = g.Max(m => m.SaleInvoiceNo),
                    }
                        );
        }

        public IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term)
        {

            var SaleDelivery = new SaleDeliveryHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(SaleDelivery.DocTypeId, SaleDelivery.DivisionId, SaleDelivery.SiteId);

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

        public IQueryable<ComboBoxResult> GetSaleInvoiceHelpListForProduct(int filter, string term)
        {
            var SaleDeliveryHeader = new SaleDeliveryHeaderService(_unitOfWork).Find(filter);

            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(SaleDeliveryHeader.DocTypeId, SaleDeliveryHeader.DivisionId, SaleDeliveryHeader.SiteId);

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


            var list = (from p in db.ViewSaleInvoiceBalanceForDelivery
                        join t in db.SaleInvoiceHeader on p.SaleInvoiceHeaderId equals t.SaleInvoiceHeaderId
                        join t2 in db.SaleInvoiceLine on p.SaleInvoiceLineId equals t2.SaleInvoiceLineId
                        join pt in db.Product on p.ProductId equals pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where p.SaleToBuyerId == SaleDeliveryHeader.SaleToBuyerId
                        && ((string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleInvoiceNo.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : ProductTab.ProductName.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension1Tab.Dimension1Name.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : Dimension2Tab.Dimension2Name.ToLower().Contains(term.ToLower())))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && p.BalanceQty > 0
                        orderby t.DocDate, t.DocNo
                        select new ComboBoxResult
                        {
                            text = ProductTab.ProductName,
                            id = p.SaleInvoiceLineId.ToString(),
                            TextProp1 = "Order No: " + p.SaleInvoiceNo.ToString(),
                            TextProp2 = "BalQty: " + p.BalanceQty.ToString(),
                            AProp1 = Dimension1Tab.Dimension1Name,
                            AProp2 = Dimension2Tab.Dimension2Name
                        });

            return list;
        }

        public SaleDeliveryLineViewModel GetSaleInvoiceLineDetail(int id)
        {
            return (from b in db.ViewSaleInvoiceBalanceForDelivery
                    join t in db.SaleInvoiceLine on b.SaleInvoiceLineId equals t.SaleInvoiceLineId into table
                    from tab in table.DefaultIfEmpty()
                    where b.SaleInvoiceLineId == id
                    select new SaleDeliveryLineViewModel
                    {
                        Qty = b.BalanceQty,
                        Remark = tab.Remark,
                        Dimension1Id = tab.SaleDispatchLine.PackingLine.Dimension1Id,
                        Dimension1Name = tab.SaleDispatchLine.PackingLine.Dimension1.Dimension1Name,
                        Dimension2Id = tab.SaleDispatchLine.PackingLine.Dimension2Id,
                        Dimension2Name = tab.SaleDispatchLine.PackingLine.Dimension2.Dimension2Name,
                        UnitConversionMultiplier = tab.UnitConversionMultiplier ?? 0,
                        UnitId = tab.SaleDispatchLine.PackingLine.Product.UnitId,
                        UnitName = tab.SaleDispatchLine.PackingLine.Product.Unit.UnitName,
                        DealUnitId = tab.SaleDispatchLine.PackingLine.DealUnitId,
                        DealUnitName = tab.SaleDispatchLine.PackingLine.DealUnit.UnitName,
                        DealQty = b.BalanceQty * (tab.UnitConversionMultiplier ?? 0),
                    }).FirstOrDefault();
        }



        public IQueryable<ComboBoxResult> GetPendingProductsForSaleDelivery(int id, string term)//DocTypeId
        {

            var SaleDelivery = new SaleDeliveryHeaderService(_unitOfWork).Find(id);

            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(SaleDelivery.DocTypeId, SaleDelivery.DivisionId, SaleDelivery.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            return (from p in db.ViewSaleInvoiceBalanceForDelivery
                    join t in db.Product on p.ProductId equals t.ProductId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && p.SaleToBuyerId == SaleDelivery.SaleToBuyerId
                     && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == SaleDelivery.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                     && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == SaleDelivery.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
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



        public void Dispose()
        {
        }
    }


}
