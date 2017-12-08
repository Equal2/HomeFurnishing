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
    public interface ISaleQuotationLineService : IDisposable
    {
        SaleQuotationLine Create(SaleQuotationLine s);
        void Delete(int id);
        void Delete(SaleQuotationLine s);
        SaleQuotationLineViewModel GetSaleQuotationLine(int id);
        SaleQuotationLine Find(int id);
        void Update(SaleQuotationLine s);
        IQueryable<SaleQuotationLineViewModel> GetSaleQuotationLineListForIndex(int SaleQuotationHeaderId);
        IEnumerable<SaleQuotationLineViewModel> GetSaleQuotationLineforDelete(int headerid);
        IEnumerable<SaleQuotationLineViewModel> GetSaleEnquiriesForFilters(SaleQuotationLineFilterViewModel vm);
        IQueryable<ComboBoxResult> GetPendingSaleEnquiryHelpList(int Id, string term);//PurchaseOrderHeaderId

        IEnumerable<SaleEnquiryHeaderListViewModel> GetPendingSaleEnquiriesWithPatternMatch(int Id, string term, int Limiter);
        //IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term);
        int GetMaxSr(int id);

        decimal GetUnitConversionForSaleEnquiryLine(int ProdLineId, byte UnitConvForId, string DealUnitId);
        IQueryable<ComboBoxResult> GetCustomProductGroups(int Id, string term);
        IQueryable<ComboBoxResult> GetSaleEnquiryHelpListForProduct(int PersonId, string term);
        SaleEnquiryLineViewModel GetSaleEnquiryDetailForQuotation(int id);
    }

    public class SaleQuotationLineService : ISaleQuotationLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleQuotationLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public SaleQuotationLine Create(SaleQuotationLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleQuotationLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleQuotationLine>().Delete(id);
        }

        public void Delete(SaleQuotationLine s)
        {
            _unitOfWork.Repository<SaleQuotationLine>().Delete(s);
        }

        public void Update(SaleQuotationLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleQuotationLine>().Update(s);
        }


        public SaleQuotationLineViewModel GetSaleQuotationLine(int id)
        {
            var temp = (from p in db.SaleQuotationLine
                        where p.SaleQuotationLineId == id
                        select new SaleQuotationLineViewModel
                        {
                            ProductId = p.ProductId,
                            Qty = p.Qty,
                            Remark = p.Remark,
                            SaleQuotationHeaderId = p.SaleQuotationHeaderId,
                            SaleQuotationLineId = p.SaleQuotationLineId,
                            ProductName = p.Product.ProductName,
                            Dimension1Id = p.Dimension1Id,
                            Dimension2Id = p.Dimension2Id,
                            Dimension3Id = p.Dimension3Id,
                            Dimension4Id = p.Dimension4Id,
                            Dimension1Name = p.Dimension1.Dimension1Name,
                            Dimension2Name = p.Dimension2.Dimension2Name,
                            Dimension3Name = p.Dimension3.Dimension3Name,
                            Dimension4Name = p.Dimension4.Dimension4Name,
                            SaleEnquiryLineId = p.SaleEnquiryLineId,
                            SaleEnquiryDocNo = p.SaleEnquiryLine.SaleEnquiryHeader.DocNo,
                            DealUnitId = p.DealUnitId,
                            DealQty = p.DealQty,
                            LockReason = p.LockReason,
                            Rate = p.Rate,
                            Amount = p.Amount,
                            SalesTaxGroupProductId = p.SalesTaxGroupProductId,
                            SalesTaxGroupPersonId = p.SaleQuotationHeader.SalesTaxGroupPersonId,
                            DiscountPer = p.DiscountPer,
                            DiscountAmount = p.DiscountAmount,
                            UnitId = p.Product.UnitId,
                            UnitName = p.Product.Unit.UnitName,
                            UnitConversionMultiplier = p.UnitConversionMultiplier ?? 0,
                            UnitDecimalPlaces = p.Product.Unit.DecimalPlaces,
                            DealUnitDecimalPlaces = p.DealUnit.DecimalPlaces,
                            Specification = p.Specification,
                        }).FirstOrDefault();

            return temp;
        }

        public SaleQuotationLine GetMainSaleQuotationLine(int id)
        {
            var temp = (from p in db.SaleQuotationLine
                        join POL in db.SaleEnquiryLine on p.SaleEnquiryLineId equals POL.SaleEnquiryLineId into tablePOL
                        from TabPOL in tablePOL.DefaultIfEmpty()
                        join JOL in db.SaleQuotationLine on TabPOL.ReferenceDocLineId equals JOL.SaleQuotationLineId into tableJOL
                        from TabJOL in tableJOL.DefaultIfEmpty()
                        where p.SaleQuotationLineId == id
                        select new SaleQuotationLine
                        {
                            ProductId = TabJOL.ProductId,
                            Qty = TabJOL.Qty,
                            Remark = TabJOL.Remark,
                            SaleQuotationHeaderId = TabJOL.SaleQuotationHeaderId,
                            SaleQuotationLineId = TabJOL.SaleQuotationLineId,
                            Dimension1Id = TabJOL.Dimension1Id,
                            Dimension2Id = TabJOL.Dimension2Id,
                            Dimension3Id = TabJOL.Dimension3Id,
                            Dimension4Id = TabJOL.Dimension4Id,
                            SaleEnquiryLineId = TabJOL.SaleEnquiryLineId,
                            DealUnitId = TabJOL.DealUnitId,
                            DealQty = TabJOL.DealQty,
                            LockReason = TabJOL.LockReason,
                            Rate = TabJOL.Rate,
                            Amount = TabJOL.Amount,
                            UnitConversionMultiplier = TabJOL.UnitConversionMultiplier,
                            Specification = TabJOL.Specification,
                        }).FirstOrDefault();
            
            return temp;
        }

        public SaleQuotationLine Find(int id)
        {
            return _unitOfWork.Repository<SaleQuotationLine>().Find(id);
        }



        public IQueryable<SaleQuotationLineViewModel> GetSaleQuotationLineListForIndex(int SaleQuotationHeaderId)
        {
            var temp = from p in db.SaleQuotationLine
                       where p.SaleQuotationHeaderId == SaleQuotationHeaderId
                       orderby p.Sr
                       select new SaleQuotationLineViewModel
                       {
                           SaleEnquiryDocNo = p.SaleEnquiryLine.SaleEnquiryHeader.DocNo,
                           DealUnitId = p.DealUnitId,
                           DealUnitName = p.DealUnit.UnitName,
                           DealQty = p.DealQty,
                           Rate = p.Rate,
                           Amount = p.Amount,
                           Dimension1Id = p.Dimension1Id,
                           Dimension2Id = p.Dimension2Id,
                           Dimension3Id = p.Dimension3Id,
                           Dimension4Id = p.Dimension4Id,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Name = p.Dimension2.Dimension2Name,
                           Dimension3Name = p.Dimension3.Dimension3Name,
                           Dimension4Name = p.Dimension4.Dimension4Name,
                           Specification = p.Specification,
                           UnitConversionMultiplier = p.UnitConversionMultiplier ?? 0,
                           UnitId = p.Product.UnitId,
                           UnitName = p.Product.Unit.UnitName,
                           UnitDecimalPlaces = p.Product.Unit.DecimalPlaces,
                           DealUnitDecimalPlaces = p.DealUnit.DecimalPlaces,
                           Remark = p.Remark,
                           ProductId = p.ProductId,
                           ProductName = p.Product.ProductName,
                           Qty = p.Qty,
                           SaleQuotationHeaderId = p.SaleQuotationHeaderId,
                           SaleQuotationLineId = p.SaleQuotationLineId,
                           EnquiryDocTypeId = p.SaleEnquiryLine.SaleEnquiryHeader.DocTypeId,
                           EnquiryHeaderId = p.SaleEnquiryLine.SaleEnquiryHeaderId,
                       };
            return temp;
        }

        public IEnumerable<SaleQuotationLineViewModel> GetSaleQuotationLineforDelete(int headerid)
        {
            return (from p in db.SaleQuotationLine
                    where p.SaleQuotationHeaderId == headerid
                    select new SaleQuotationLineViewModel
                    {
                        SaleQuotationLineId = p.SaleQuotationLineId,
                    });
        }




        public IEnumerable<SaleQuotationLineViewModel> GetSaleEnquiriesForFilters(SaleQuotationLineFilterViewModel vm)
        {
            byte? UnitConvForId = new SaleQuotationHeaderService(_unitOfWork).Find(vm.SaleQuotationHeaderId).UnitConversionForId;

            var SaleQuotation = new SaleQuotationHeaderService(_unitOfWork).Find(vm.SaleQuotationHeaderId);

            var Settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(SaleQuotation.DocTypeId, SaleQuotation.DivisionId, SaleQuotation.SiteId);




            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.SaleEnquiryHeaderId)) { SaleOrderIdArr = vm.SaleEnquiryHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] Dimension1 = null;
            if (!string.IsNullOrEmpty(vm.Dimension1Id)) { Dimension1 = vm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dimension1 = new string[] { "NA" }; }

            string[] Dimension2 = null;
            if (!string.IsNullOrEmpty(vm.Dimension2Id)) { Dimension2 = vm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dimension2 = new string[] { "NA" }; }


            string[] Dimension3 = null;
            if (!string.IsNullOrEmpty(vm.Dimension3Id)) { Dimension3 = vm.Dimension3Id.Split(",".ToCharArray()); }
            else { Dimension3 = new string[] { "NA" }; }

            string[] Dimension4 = null;
            if (!string.IsNullOrEmpty(vm.Dimension4Id)) { Dimension4 = vm.Dimension4Id.Split(",".ToCharArray()); }
            else { Dimension4 = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }





            if (!string.IsNullOrEmpty(vm.DealUnitId))
            {
                Unit Dealunit = new UnitService(_unitOfWork).Find(vm.DealUnitId);

                var temp = (from p in db.ViewSaleEnquiryBalanceForQuotation
                            join t in db.SaleEnquiryHeader on p.SaleEnquiryHeaderId equals t.SaleEnquiryHeaderId into table
                            from tab in table.DefaultIfEmpty()
                            join product in db.Product on p.ProductId equals product.ProductId into table2
                            join t1 in db.SaleEnquiryLine on p.SaleEnquiryLineId equals t1.SaleEnquiryLineId into table1
                            from tab1 in table1.DefaultIfEmpty()
                            from tab2 in table2.DefaultIfEmpty()
                            join t3 in db.UnitConversion on new { p1 = p.ProductId, DU1 = vm.DealUnitId, U1 = UnitConvForId ?? 0 } equals new { p1 = t3.ProductId ?? 0, DU1 = t3.ToUnitId, U1 = t3.UnitConversionForId } into table3
                            join FP in db.FinishedProduct on p.ProductId equals FP.ProductId into tableFinishedProduct
                            from tabFinishedProduct in tableFinishedProduct.DefaultIfEmpty()
                            from tab3 in table3.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.SaleEnquiryHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleEnquiryHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1.Contains(p.Dimension1Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2.Contains(p.Dimension2Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension3Id) ? 1 == 1 : Dimension3.Contains(p.Dimension3Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension4Id) ? 1 == 1 : Dimension4.Contains(p.Dimension4Id.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                            && p.BalanceQty > 0
                            orderby tab.DocDate, tab.DocNo
                            select new SaleQuotationLineViewModel
                            {
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Dimension3Name = tab1.Dimension3.Dimension3Name,
                                Dimension4Name = tab1.Dimension4.Dimension4Name,
                                Dimension1Id = p.Dimension1Id,
                                Dimension2Id = p.Dimension2Id,
                                Dimension3Id = p.Dimension3Id,
                                Dimension4Id = p.Dimension4Id,
                                Specification = tab1.Specification,
                                SaleEnquiryBalanceQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                Rate = vm.Rate,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                SaleQuotationHeaderId = vm.SaleQuotationHeaderId,
                                SaleEnquiryLineId = p.SaleEnquiryLineId,
                                UnitId = tab2.UnitId,
                                SaleEnquiryDocNo = p.SaleEnquiryNo,
                                DealUnitId = (vm.DealUnitId),
                                UnitConversionMultiplier = Math.Round( (tab3 == null ? 1 : tab3.ToQty / tab3.FromQty), (tab3 == null ? tab2.Unit.DecimalPlaces : Dealunit.DecimalPlaces)),
                                UnitDecimalPlaces = tab2.Unit.DecimalPlaces,
                                DealUnitDecimalPlaces = (tab3 == null ? tab2.Unit.DecimalPlaces : Dealunit.DecimalPlaces)
                            });
                return temp;
            }
            else
            {
                var temp = (from p in db.ViewSaleEnquiryBalanceForQuotation
                            join t in db.SaleEnquiryHeader on p.SaleEnquiryHeaderId equals t.SaleEnquiryHeaderId into table
                            from tab in table.DefaultIfEmpty()
                            join product in db.Product on p.ProductId equals product.ProductId into table2
                            join t1 in db.SaleEnquiryLine on p.SaleEnquiryLineId equals t1.SaleEnquiryLineId into table1
                            from tab1 in table1.DefaultIfEmpty()
                            from tab2 in table2.DefaultIfEmpty()
                            join FP in db.FinishedProduct on p.ProductId equals FP.ProductId into tableFinishedProduct
                            from tabFinishedProduct in tableFinishedProduct.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.SaleEnquiryHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleEnquiryHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1.Contains(p.Dimension1Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2.Contains(p.Dimension2Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension3Id) ? 1 == 1 : Dimension3.Contains(p.Dimension3Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension4Id) ? 1 == 1 : Dimension4.Contains(p.Dimension4Id.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                            && p.BalanceQty > 0
                            select new SaleQuotationLineViewModel
                            {
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension1Id = p.Dimension1Id,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Dimension2Id = p.Dimension2Id,
                                Dimension3Name = tab1.Dimension3.Dimension3Name,
                                Dimension3Id = p.Dimension3Id,
                                Dimension4Name = tab1.Dimension4.Dimension4Name,
                                Dimension4Id = p.Dimension4Id,
                                Specification = tab1.Specification,
                                SaleEnquiryBalanceQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                Rate = vm.Rate,
                                SaleEnquiryDocNo = tab.DocNo,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                SaleQuotationHeaderId = vm.SaleQuotationHeaderId,
                                SaleEnquiryLineId = p.SaleEnquiryLineId,
                                UnitId = tab2.UnitId,
                                DealUnitId = tab2.UnitId,
                                UnitConversionMultiplier = 1,
                                UnitDecimalPlaces = tab2.Unit.DecimalPlaces,
                                DealUnitDecimalPlaces = tab2.Unit.DecimalPlaces,
                            }

                        );
                return temp;
            }

        }


        public IQueryable<ComboBoxResult> GetPendingSaleEnquiryHelpList(int Id, string term)
        {
            var SaleQuotationHeader = new SaleQuotationHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(SaleQuotationHeader.DocTypeId, SaleQuotationHeader.DivisionId, SaleQuotationHeader.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }



            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var list = (from p in db.ViewSaleEnquiryBalanceForQuotation
                        join t in db.Persons on p.BuyerId equals t.PersonID into table
                        from tab in table.DefaultIfEmpty()
                        join Pt in db.Product on p.ProductId equals Pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join Fp in db.FinishedProduct on p.ProductId equals Fp.ProductId into FinishedProductTable
                        from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
                        join D in db.DocumentType on p.DocTypeId equals D.DocumentTypeId into DocumentTypeTable
                        from DocumentTyoeTab in DocumentTypeTable.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleEnquiryNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(ProductTab.ProductGroup.ProductTypeId.ToString()))
                        group new { p, tab.Code, DocumentTyoeTab } by p.SaleEnquiryHeaderId into g
                        orderby g.Max(m => m.p.EnquiryDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.DocumentTyoeTab.DocumentTypeShortName) + "-" + g.Max(m => m.p.SaleEnquiryNo) + " {" + g.Max(m => m.Code) + "}",
                            id = g.Key.ToString(),
                        });

            
             
            return list;
        }


        public IEnumerable<SaleEnquiryHeaderListViewModel> GetPendingSaleEnquiriesWithPatternMatch(int Id, string term, int Limiter)
        {
            var SaleQuotationHeader = new SaleQuotationHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(SaleQuotationHeader.DocTypeId, SaleQuotationHeader.DivisionId, SaleQuotationHeader.SiteId);

            string settingProductTypes = "";
            string settingProductDivision = "";
            string settingProductCategory = "";

            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { settingProductTypes = "|" + settings.filterProductTypes.Replace(",", "|,|") + "|"; }
            if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { settingProductDivision = "|" + settings.FilterProductDivision.Replace(",", "|,|") + "|"; }


            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settingProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ProductDivision = null;
            if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { ProductDivision = settingProductDivision.Split(",".ToCharArray()); }
            else { ProductDivision = new string[] { "NA" }; }




            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var list = (from p in db.ViewSaleEnquiryBalanceForQuotation
                        join Pt in db.Product on p.ProductId equals Pt.ProductId into ProductTable from ProductTab in ProductTable.DefaultIfEmpty()
                        join Fp in db.FinishedProduct on p.ProductId equals Fp.ProductId into FinishedProductTable from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        where (
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleEnquiryNo.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : ProductTab.ProductName.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : Dimension1Tab.Dimension1Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : Dimension2Tab.Dimension2Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : Dimension3Tab.Dimension3Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : Dimension4Tab.Dimension4Name.ToLower().Contains(term.ToLower())
                        ) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains("|" + ProductTab.ProductGroup.ProductTypeId.ToString() + "|"))
                        && (string.IsNullOrEmpty(settings.FilterProductDivision) ? 1 == 1 : ProductDivision.Contains("|" + ProductTab.DivisionId.ToString() + "|"))
                        orderby p.SaleEnquiryNo
                        select new SaleEnquiryHeaderListViewModel
                        {
                            DocNo = p.SaleEnquiryNo,
                            SaleEnquiryLineId = p.SaleEnquiryLineId,
                            ProductName = ProductTab.ProductName,
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                        }).Take(Limiter);

            return (list);
        }

        //public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        //{
        //    var SaleQuotation = new SaleQuotationHeaderService(_unitOfWork).Find(Id);

        //    var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(SaleQuotation.DocTypeId, SaleQuotation.DivisionId, SaleQuotation.SiteId);

        //    string settingProductTypes = "";
        //    string settingProductDivision = "";


        //    if (!string.IsNullOrEmpty(settings.filterProductTypes)) { settingProductTypes = "|" + settings.filterProductTypes.Replace(",", "|,|") + "|"; }
        //    if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { settingProductDivision = "|" + settings.FilterProductDivision.Replace(",", "|,|") + "|"; }


        //    string[] ProductTypes = null;
        //    if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settingProductTypes.Split(",".ToCharArray()); }
        //    else { ProductTypes = new string[] { "NA" }; }

        //    string[] ProductDivision = null;
        //    if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { ProductDivision = settingProductDivision.Split(",".ToCharArray()); }
        //    else { ProductDivision = new string[] { "NA" }; }




        //    var list = (from p in db.Product
        //                join Fp in db.FinishedProduct on p.ProductId equals Fp.ProductId into FinishedProductTable from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
        //                where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
        //                && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains("|" + p.ProductGroup.ProductTypeId.ToString() + "|"))
        //                && (string.IsNullOrEmpty(settings.FilterProductDivision) ? 1 == 1 : ProductDivision.Contains("|" + p.DivisionId.ToString() + "|"))
        //                group new { p } by p.ProductId into g
        //                select new ComboBoxList
        //                {
        //                    PropFirst = g.Max(m => m.p.ProductName),
        //                    Id = g.Key,
        //                }
        //                  ).Take(20);

        //    return list.ToList();
        //}

        public IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term)
        {
            var SaleQuotation = new SaleQuotationHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(SaleQuotation.DocTypeId, SaleQuotation.DivisionId, SaleQuotation.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }

            string[] ProductDivision = null;
            if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { ProductDivision = settings.FilterProductDivision.Split(",".ToCharArray()); }
            else { ProductDivision = new string[] { "NA" }; }

            string[] ProductCategory = null;
            if (!string.IsNullOrEmpty(settings.filterProductCategories)) { ProductCategory = settings.filterProductCategories.Split(",".ToCharArray()); }
            else { ProductCategory = new string[] { "NA" }; }

            return (from p in db.Product
                    where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(p.ProductGroupId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductCategories) ? 1 == 1 : ProductCategory.Contains(p.ProductCategoryId.ToString()))
                    && (string.IsNullOrEmpty(settings.FilterProductDivision) ? 1 == 1 : ProductDivision.Contains(p.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductGroup.ProductGroupName.ToLower().Contains(term.ToLower()))
                    orderby p.ProductName
                    select new ComboBoxResult
                    {
                        id = p.ProductId.ToString(),
                        text = p.ProductName,
                        AProp1 = p.ProductGroup.ProductGroupName,
                    });
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.SaleQuotationLine
                       where p.SaleQuotationHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m) + 1;
            else
                return (1);
        }



        public decimal GetUnitConversionForSaleEnquiryLine(int ProdLineId, byte UnitConvForId, string DealUnitId)
        {
            var Query = (from t1 in db.SaleEnquiryLine
                         join t2 in db.Product on t1.ProductId equals t2.ProductId
                         join t3 in db.UnitConversion on new { p1 = t1.ProductId, DU1 = DealUnitId, U1 = UnitConvForId } equals new { p1 = t3.ProductId, DU1 = t3.ToUnitId, U1 = t3.UnitConversionForId } into table3
                         from tab3 in table3.DefaultIfEmpty()
                         select tab3).FirstOrDefault();

            if (Query != null)
            {
                return (Query.ToQty / Query.FromQty);
            }
            else
                return 0;
        }

        public IQueryable<ComboBoxResult> GetCustomProductGroups(int Id, string term)
        {
            var SaleQuotation = new SaleQuotationHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(SaleQuotation.DocTypeId, SaleQuotation.DivisionId, SaleQuotation.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }

            return (from p in db.ProductGroups
                    where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(p.ProductGroupId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductGroupName.ToLower().Contains(term.ToLower()))
                    orderby p.ProductGroupName
                    select new ComboBoxResult
                    {
                        id = p.ProductGroupId.ToString(),
                        text = p.ProductGroupName,
                    });
        }

        public IQueryable<ComboBoxResult> GetSaleEnquiryHelpListForProduct(int PersonId, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var list = (from p in db.ViewSaleEnquiryBalanceForQuotation
                        join t in db.SaleEnquiryHeader on p.SaleEnquiryHeaderId equals t.SaleEnquiryHeaderId
                        join t2 in db.SaleEnquiryLine on p.SaleEnquiryLineId equals t2.SaleEnquiryLineId
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
                        where p.BuyerId == PersonId
                        && ((string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleEnquiryNo.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : ProductTab.ProductName.ToLower().Contains(term.ToLower())))
                        && p.SiteId == CurrentSiteId
                        && p.DivisionId == CurrentDivisionId
                        orderby t.DocDate, t.DocNo
                        select new ComboBoxResult
                        {
                            text = ProductTab.ProductName,
                            id = p.SaleEnquiryLineId.ToString(),
                            TextProp1 = "Enquiry No: " + p.SaleEnquiryNo.ToString(),
                            TextProp2 = "BalQty: " + p.BalanceQty.ToString(),
                            AProp1 = Dimension1Tab.Dimension1Name + (string.IsNullOrEmpty(Dimension1Tab.Dimension1Name) ? "" : ",") + Dimension2Tab.Dimension2Name,
                            AProp2 = Dimension3Tab.Dimension3Name + (string.IsNullOrEmpty(Dimension3Tab.Dimension3Name) ? "" : ",") + Dimension4Tab.Dimension4Name,
                        });

            return list;
        }

        public SaleEnquiryLineViewModel GetSaleEnquiryDetailForQuotation(int id)
        {

            return (from t in db.ViewSaleEnquiryBalanceForQuotation
                    join p in db.SaleEnquiryLine on t.SaleEnquiryLineId equals p.SaleEnquiryLineId into SaleEnquiryLineTable
                    from SaleEnquiryLineTab in SaleEnquiryLineTable.DefaultIfEmpty()
                    where t.SaleEnquiryLineId == id
                    select new SaleEnquiryLineViewModel
                    {
                        DealQty = SaleEnquiryLineTab.DealQty,
                        DealUnitId = SaleEnquiryLineTab.DealUnitId,
                        ProductId = SaleEnquiryLineTab.ProductId,
                        SaleEnquiryDocNo = t.SaleEnquiryNo,
                        Qty = t.BalanceQty,
                        Rate = SaleEnquiryLineTab.Rate,
                        Remark = SaleEnquiryLineTab.Remark,
                        Dimension1Id = SaleEnquiryLineTab.Dimension1Id,
                        Dimension2Id = SaleEnquiryLineTab.Dimension2Id,
                        Dimension3Id = SaleEnquiryLineTab.Dimension3Id,
                        Dimension4Id = SaleEnquiryLineTab.Dimension4Id,
                        Dimension1Name = SaleEnquiryLineTab.Dimension1.Dimension1Name,
                        Dimension2Name = SaleEnquiryLineTab.Dimension2.Dimension2Name,
                        Dimension3Name = SaleEnquiryLineTab.Dimension3.Dimension3Name,
                        Dimension4Name = SaleEnquiryLineTab.Dimension4.Dimension4Name,
                        Specification = SaleEnquiryLineTab.Specification,
                        UnitConversionMultiplier = SaleEnquiryLineTab.UnitConversionMultiplier,
                        UnitId = SaleEnquiryLineTab.Product.UnitId,
                        UnitName = SaleEnquiryLineTab.Product.Unit.UnitName,
                    }).FirstOrDefault();
        }

        public void Dispose()
        {
        }
    }


}
