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


namespace Service
{
    public interface IPurchaseQuotationLineService : IDisposable
    {
        PurchaseQuotationLine Create(PurchaseQuotationLine s);
        void Delete(int id);
        void Delete(PurchaseQuotationLine s);
        PurchaseQuotationLineViewModel GetPurchaseQuotationLine(int id);
        PurchaseQuotationLine Find(int id);
        void Update(PurchaseQuotationLine s);
        IQueryable<PurchaseQuotationLineViewModel> GetPurchaseQuotationLineListForIndex(int PurchaseQuotationHeaderId);
        IEnumerable<PurchaseQuotationLineViewModel> GetPurchaseQuotationLineforDelete(int headerid);
        IEnumerable<PurchaseQuotationLineViewModel> GetPurchaseIndentForFilters(PurchaseQuotationLineFilterViewModel vm);
        IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseIndentHelpList(int Id, string term);//PurchaseOrderHeaderId
        dynamic GetProductHelpListforAC(int Id, string term, int Limit);
        IQueryable<ComboBoxResult> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);
        PurchaseQuotationLineViewModel GetPurchaseIndentDetailBalance(int id);
        IEnumerable<PurchaseIndentLineListViewModel> GetPendingIndentsForQuotation(int id, int PurchaseQuotationHeaderId);
    }

    public class PurchaseQuotationLineService : IPurchaseQuotationLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public PurchaseQuotationLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PurchaseQuotationLine Create(PurchaseQuotationLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseQuotationLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseQuotationLine>().Delete(id);
        }

        public void Delete(PurchaseQuotationLine s)
        {
            _unitOfWork.Repository<PurchaseQuotationLine>().Delete(s);
        }

        public void Update(PurchaseQuotationLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseQuotationLine>().Update(s);
        }


        public PurchaseQuotationLineViewModel GetPurchaseQuotationLine(int id)
        {
            return (from p in db.PurchaseQuotationLine
                    join t in db.PurchaseQuotationHeader on p.PurchaseQuotationHeaderId equals t.PurchaseQuotationHeaderId
                    join t2 in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t2.PurchaseIndentLineId into table
                    from tab2 in table.DefaultIfEmpty()
                    join t3 in db.PurchaseIndentHeader on tab2.PurchaseIndentHeaderId equals t3.PurchaseIndentHeaderId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    where p.PurchaseQuotationLineId == id
                    select new PurchaseQuotationLineViewModel
                    {
                        SupplierId = t.SupplierId,
                        Amount = p.Amount,
                        PurchaseIndentLineId = p.PurchaseIndentLineId,
                        PurchaseIndentDocNo = tab3.DocNo,
                        ProductId = p.ProductId,
                        Qty = p.Qty,
                        Rate = p.Rate,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        DealUnitId = p.DealUnitId,
                        UnitName = p.Product.Unit.UnitName,
                        DiscountPer = p.DiscountPer,
                        Remark = t.Remark,
                        PurchaseQuotationHeaderId = p.PurchaseQuotationHeaderId,
                        PurchaseQuotationLineId = p.PurchaseQuotationLineId,
                        ProductName = p.Product.ProductName,
                        UnitId = p.Product.UnitId,
                        LockReason = p.LockReason,
                        Dimension1Id = p.Dimension1Id,
                        Dimension2Id = p.Dimension2Id,
                        Specification = p.Specification,
                        LotNo = p.LotNo,
                    }

                        ).FirstOrDefault();
        }
        public PurchaseQuotationLine Find(int id)
        {
            return _unitOfWork.Repository<PurchaseQuotationLine>().Find(id);
        }


        public IQueryable<PurchaseQuotationLineViewModel> GetPurchaseQuotationLineListForIndex(int PurchaseQuotationHeaderId)
        {
            var temp = from p in db.PurchaseQuotationLine
                       where p.PurchaseQuotationHeaderId == PurchaseQuotationHeaderId
                       join t in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t.PurchaseIndentLineId into table
                       from tab in table.DefaultIfEmpty()
                       join t2 in db.PurchaseIndentHeader on tab.PurchaseIndentHeaderId equals t2.PurchaseIndentHeaderId into table3
                       from tab3 in table3.DefaultIfEmpty()
                       orderby p.Sr
                       select new PurchaseQuotationLineViewModel
                       {
                           ProductId = p.ProductId,
                           ProductName = p.Product.ProductName,
                           Qty = p.Qty,
                           UnitId = p.Product.UnitId,
                           PurchaseQuotationHeaderId = p.PurchaseQuotationHeaderId,
                           PurchaseQuotationLineId = p.PurchaseQuotationLineId,
                           PurchaseIndentDocNo = tab3.DocNo,
                           DealQty = p.DealQty,
                           DealUnitId = p.DealUnitId,
                           DealUnitName = p.DealUnit.UnitName,
                           DealunitDecimalPlaces = p.DealUnit.DecimalPlaces,
                           Rate = p.Rate,
                           DiscountPer = p.DiscountPer,
                           Amount = p.Amount,
                           UnitName = p.Product.Unit.UnitName,
                           unitDecimalPlaces = p.Product.Unit.DecimalPlaces,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Name = p.Dimension2.Dimension2Name,
                           Specification = p.Specification,
                           LotNo = p.LotNo,
                       };
            return temp;
        }

        public IEnumerable<PurchaseQuotationLineViewModel> GetPurchaseQuotationLineforDelete(int headerid)
        {
            return (from p in db.PurchaseQuotationLine
                    where p.PurchaseQuotationHeaderId == headerid
                    select new PurchaseQuotationLineViewModel
                    {
                        PurchaseQuotationLineId = p.PurchaseQuotationLineId
                    });
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.PurchaseQuotationLine
                       where p.PurchaseQuotationHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }

        public IEnumerable<PurchaseQuotationLineViewModel> GetPurchaseIndentForFilters(PurchaseQuotationLineFilterViewModel vm)
        {
            byte? UnitConvForId = db.PurchaseQuotationHeader.Find(vm.PurchaseQuotationHeaderId).UnitConversionForId;

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
                            select new PurchaseQuotationLineViewModel
                            {
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Dimension1Id = tab1.Dimension1Id,
                                Dimension2Id = tab1.Dimension2Id,
                                Specification = tab1.Specification,
                                IndentBalanceQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                Rate = vm.Rate,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                PurchaseQuotationHeaderId = vm.PurchaseQuotationHeaderId,
                                PurchaseIndentLineId = p.PurchaseIndentLineId,
                                UnitId = tab2.UnitId,
                                PurchaseIndentDocNo = p.PurchaseIndentNo,
                                DealUnitId = (tab3 == null ? tab2.UnitId : vm.DealUnitId),
                                UnitConversionMultiplier = (tab3 == null ? 1 : tab3.ToQty / tab3.FromQty),
                                UnitConversionException = tab3 == null ? true : false,
                                unitDecimalPlaces = tab2.Unit.DecimalPlaces,
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
                            select new PurchaseQuotationLineViewModel
                            {
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension1Id = tab1.Dimension1Id,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Dimension2Id = tab1.Dimension2Id,
                                Specification = tab1.Specification,
                                IndentBalanceQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                Rate = vm.Rate,
                                PurchaseIndentDocNo = tab.DocNo,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                PurchaseQuotationHeaderId = vm.PurchaseQuotationHeaderId,
                                PurchaseIndentLineId = p.PurchaseIndentLineId,
                                UnitId = tab2.UnitId,
                                DealUnitId = tab2.UnitId,
                                UnitConversionMultiplier = 1,
                                unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                                DealunitDecimalPlaces = tab2.Unit.DecimalPlaces,
                            }

                        );
                return temp;
            }

        }

        public IEnumerable<PurchaseOrderLineListViewModel> GetPendingPurchaseIndentHelpList(int Id, string term)
        {

            var PurchaseQuotation = db.PurchaseQuotationHeader.Find(Id);

            var settings = new PurchaseQuotationSettingService(_unitOfWork).GetPurchaseQuotationSettingForDocument(PurchaseQuotation.DocTypeId, PurchaseQuotation.DivisionId, PurchaseQuotation.SiteId);

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

        public dynamic GetProductHelpListforAC(int Id, string term, int Limit)
        {
            var PurchaseQuotation = db.PurchaseQuotationHeader.Find(Id);

            var settings = new PurchaseQuotationSettingService(_unitOfWork).GetPurchaseQuotationSettingForDocument(PurchaseQuotation.DocTypeId, PurchaseQuotation.DivisionId, PurchaseQuotation.SiteId);

            //var list = (from p in db.Product
            //            where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
            //            && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
            //            group new { p } by p.ProductId into g
            //            select new ComboBoxList
            //            {
            //                PropFirst = g.Max(m => m.p.ProductName),
            //                Id = g.Key,

            //                //    DocumentTypeName=g.Max(p=>p.p.DocumentTypeShortName)
            //            }
            //              ).Take(20);

            //return list.ToList();


            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            string[] ContraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { ContraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { ContraDocTypes = new string[] { "NA" }; }

            var query = (from p in db.ViewPurchaseIndentBalance
                         join t in db.PurchaseIndentHeader on p.PurchaseIndentHeaderId equals t.PurchaseIndentHeaderId
                         join rl in db.PurchaseIndentLine on p.PurchaseIndentLineId equals rl.PurchaseIndentLineId
                         join prod in db.Product on p.ProductId equals prod.ProductId
                         join pg in db.ProductGroups on prod.ProductGroupId equals pg.ProductGroupId
                         where p.BalanceQty > 0
                         //t.Status == (int)StatusConstants.Submitted
                         orderby t.DocDate, t.DocNo
                         select new
                         {
                             ProductName = rl.Product.ProductName,
                             ProductId = p.ProductId,
                             Specification = rl.Specification,
                             Dimension1Name = rl.Dimension1.Dimension1Name,
                             Dimension2Name = rl.Dimension2.Dimension2Name,
                             PurchaseIndentNo = p.PurchaseIndentNo,
                             PurchaseIndentLineId = p.PurchaseIndentLineId,
                             Qty = p.BalanceQty,
                             ProductType = pg.ProductTypeId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             DocTypeId = p.DocTypeId,
                         });

            if (!string.IsNullOrEmpty(settings.filterProductTypes))
                query = query.Where(m => ProductTypes.Contains(m.ProductType.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                query = query.Where(m => ContraSites.Contains(m.SiteId.ToString()));
            else
                query = query.Where(m => m.SiteId == PurchaseQuotation.SiteId);

            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                query = query.Where(m => ContraDivisions.Contains(m.DivisionId.ToString()));
            else
                query = query.Where(m => m.DivisionId == PurchaseQuotation.DivisionId);

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                query = query.Where(m => ContraDocTypes.Contains(m.DocTypeId.ToString()));

            if (!string.IsNullOrEmpty(term))
                query = query.Where(m => m.ProductName.ToLower().Contains(term.ToLower())
                 || m.Specification.ToLower().Contains(term.ToLower())
                 || m.Dimension1Name.ToLower().Contains(term.ToLower())
                 || m.Dimension2Name.ToLower().Contains(term.ToLower())
                 || m.PurchaseIndentNo.ToLower().Contains(term.ToLower())
                 );

            return (from p in query
                    select new
                    {
                        ProductName = p.ProductName,
                        ProductId = p.ProductId,
                        Specification = p.Specification,
                        Dimension1Name = p.Dimension1Name,
                        Dimension2Name = p.Dimension2Name,
                        PurchaseIndentNo = p.PurchaseIndentNo,
                        PurchaseIndentLineId = p.PurchaseIndentLineId,
                        Qty = p.Qty,
                    }).Take(Limit).ToList();

        }

        public IQueryable<ComboBoxResult> GetProductHelpList(int Id, string term)
        {
            var PurchaseQuotation = db.PurchaseQuotationHeader.Find(Id);

            var settings = new PurchaseQuotationSettingService(_unitOfWork).GetPurchaseQuotationSettingForDocument(PurchaseQuotation.DocTypeId, PurchaseQuotation.DivisionId, PurchaseQuotation.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            var query = (from p in db.Product
                         join pg in db.ProductGroups on p.ProductGroupId equals pg.ProductGroupId
                         //t.Status == (int)StatusConstants.Submitted
                         orderby p.ProductName
                         select new
                         {
                             ProductName = p.ProductName,
                             ProductId = p.ProductId,
                             ProductType = pg.ProductTypeId,
                         });

            if (!string.IsNullOrEmpty(settings.filterProductTypes))
                query = query.Where(m => ProductTypes.Contains(m.ProductType.ToString()));

            if (!string.IsNullOrEmpty(term))
                query = query.Where(m => m.ProductName.ToLower().Contains(term.ToLower()));

            return (from p in query
                    select new ComboBoxResult
                    {
                        text = p.ProductName,
                        id = p.ProductId.ToString(),
                    });

        }

        public PurchaseQuotationLineViewModel GetPurchaseIndentDetailBalance(int id)
        {
            return (from b in db.ViewPurchaseIndentBalance
                    join p in db.PurchaseIndentLine on b.PurchaseIndentLineId equals p.PurchaseIndentLineId
                    where b.PurchaseIndentLineId == id

                    select new PurchaseQuotationLineViewModel
                    {
                        Qty = b.BalanceQty,
                        Remark = p.Remark,
                        //Rate = tab.Rate,
                        //Amount = tab.Rate * (b.BalanceQty * p.UnitConversionMultiplier),
                        PurchaseIndentDocNo = p.PurchaseIndentHeader.DocNo,
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

        public IEnumerable<PurchaseIndentLineListViewModel> GetPendingIndentsForQuotation(int id, int PurchaseQuotationHeaderId)
        {


            var PurchaseQuotationHeader = db.PurchaseQuotationHeader.Find(PurchaseQuotationHeaderId);

            var settings = new PurchaseQuotationSettingService(_unitOfWork).GetPurchaseQuotationSettingForDocument(PurchaseQuotationHeader.DocTypeId, PurchaseQuotationHeader.DivisionId, PurchaseQuotationHeader.SiteId);

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


            return (from p in db.ViewPurchaseIndentBalance
                    join t1 in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t1.PurchaseIndentLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.ProductId == id && p.BalanceQty > 0
                     && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    select new PurchaseIndentLineListViewModel
                    {
                        PurchaseIndentLineId = p.PurchaseIndentLineId,
                        PurchaseIndentHeaderId = p.PurchaseIndentHeaderId,
                        DocNo = p.PurchaseIndentNo,
                        Dimension1Name = tab1.Dimension1.Dimension1Name,
                        Dimension2Name = tab1.Dimension2.Dimension2Name,
                    }
                        );
        }

        public void Dispose()
        {
        }
    }
}
