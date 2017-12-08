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
    public interface IProdOrderCancelLineService : IDisposable
    {
        ProdOrderCancelLine Create(ProdOrderCancelLine p);
        void Delete(int id);
        void Delete(ProdOrderCancelLine p);
        IEnumerable<ProdOrderCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProdOrderCancelLine p);
        ProdOrderCancelLine Add(ProdOrderCancelLine p);
        IQueryable<ProdOrderCancelLineViewModel> GetProdOrderCancelLineListForHeader(int ProdOrderCancelHeaderId);
        Task<IEquatable<ProdOrderCancelLine>> GetAsync();
        Task<ProdOrderCancelLine> FindAsync(int id);
        ProdOrderCancelLine Find(int id);
        IEnumerable<ProdOrderCancelLineViewModel> GetProdOrderLineForMultiSelect(ProdOrderCancelFilterViewModel svm);
        ProdOrderCancelLineViewModel GetProdOrderCancelLine(int id);//ProdOrderCancelLine Id
        IEnumerable<ProdOrderLineBalance> GetProdOrderForProduct(int id);//ProductId

        IEnumerable<ProdOrderCancelLine> GetProdOrderCancelLineForMaterialPlanCancel(int id);

        IEnumerable<ProdOrderLineBalance> GetPendingProdOrdersWithPatternMatch(int HeaderId, string term, int Limiter);
        decimal GetBalanceQuantity(int id);//ProdOrderLineId
        IQueryable<ComboBoxResult> GetPendingProdOrders(int filter, string term);
        IQueryable<ComboBoxResult> GetPendingProductsForProdOrderCancel(int filter, string term);
    }

    public class ProdOrderCancelLineService : IProdOrderCancelLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProdOrderCancelLine> _ProdOrderCancelLineRepository;
        RepositoryQuery<ProdOrderCancelLine> ProdOrderCancelLineRepository;
        public ProdOrderCancelLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProdOrderCancelLineRepository = new Repository<ProdOrderCancelLine>(db);
            ProdOrderCancelLineRepository = new RepositoryQuery<ProdOrderCancelLine>(_ProdOrderCancelLineRepository);
        }

        public ProdOrderCancelLine Create(ProdOrderCancelLine p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProdOrderCancelLine>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProdOrderCancelLine>().Delete(id);
        }

        public void Delete(ProdOrderCancelLine p)
        {
            _unitOfWork.Repository<ProdOrderCancelLine>().Delete(p);
        }
        public IEnumerable<ProdOrderLineBalance> GetProdOrderForProduct(int id)
        {
            var Qry = from p in db.ViewProdOrderBalance
                      join t in db.ProdOrderLine on p.ProdOrderLineId equals t.ProdOrderLineId into table
                      from tab in table.DefaultIfEmpty()
                      join t1 in db.MaterialPlanLine on tab.MaterialPlanLineId equals t1.MaterialPlanLineId into table1
                      from tab1 in table1.DefaultIfEmpty()
                      join t2 in db.MaterialPlanHeader on tab1.MaterialPlanHeaderId equals t2.MaterialPlanHeaderId into table2
                      from tab2 in table2.DefaultIfEmpty()
                      where p.ProductId == id && p.BalanceQty > 0
                      select new ProdOrderLineBalance
                      {
                          ProdOrderDocNo = p.ProdOrderNo,
                          ProdOrderLineId = p.ProdOrderLineId,
                          MaterialPlanDocNo = tab2.DocNo,
                          BalanceQty = p.BalanceQty

                      };

            return (Qry);
        }


        public IEnumerable<ProdOrderLineBalance> GetPendingProdOrdersWithPatternMatch(int HeaderId, string term, int Limiter)
        {
            var ProdordercancelHeader = new ProdOrderCancelHeaderService(_unitOfWork).Find(HeaderId);

            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(ProdordercancelHeader.DocTypeId, ProdordercancelHeader.DivisionId, ProdordercancelHeader.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            string[] filterProducts = null;
            if (!string.IsNullOrEmpty(settings.filterProducts)) { filterProducts = settings.filterProducts.Split(",".ToCharArray()); }
            else { filterProducts = new string[] { "NA" }; }

            string[] filterProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { filterProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { filterProductTypes = new string[] { "NA" }; }

            string[] filterProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { filterProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { filterProductGroups = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Qry = (from p in db.ViewProdOrderBalance
                       join t in db.ProdOrderLine on p.ProdOrderLineId equals t.ProdOrderLineId into table
                       from tab in table.DefaultIfEmpty()
                       join t1 in db.MaterialPlanLine on tab.MaterialPlanLineId equals t1.MaterialPlanLineId into table1
                       from tab1 in table1.DefaultIfEmpty()
                       join t2 in db.MaterialPlanHeader on tab1.MaterialPlanHeaderId equals t2.MaterialPlanHeaderId into table2
                       from tab2 in table2.DefaultIfEmpty()
                       join ProTab in db.ProdOrderHeader on p.ProdOrderHeaderId equals ProTab.ProdOrderHeaderId
                       join Prod in db.Product on p.ProductId equals Prod.ProductId
                       where p.BalanceQty > 0 && ((string.IsNullOrEmpty(term) ? 1 == 1 : p.ProdOrderNo.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower()))
                        || (string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower())))
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProducts) ? 1 == 1 : filterProducts.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : filterProductGroups.Contains(Prod.ProductGroupId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : filterProductTypes.Contains(Prod.ProductGroup.ProductTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? ProTab.SiteId == CurrentSiteId : contraSites.Contains(ProTab.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? ProTab.DivisionId == CurrentDivisionId : contraDivisions.Contains(ProTab.DivisionId.ToString()))
                       select new ProdOrderLineBalance
                       {
                           ProdOrderDocNo = p.ProdOrderNo,
                           ProdOrderLineId = p.ProdOrderLineId,
                           MaterialPlanDocNo = tab2.DocNo,
                           ProductId = p.ProductId,
                           ProductName = p.Product.ProductName,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Name = p.Dimension2.Dimension2Name,
                           BalanceQty = p.BalanceQty

                       }).Take(Limiter);

            return (Qry);
        }

        public decimal GetBalanceQuantity(int id)
        {
            return (from p in db.ViewProdOrderBalance
                    where p.ProdOrderLineId == id
                    select p.BalanceQty
                        ).FirstOrDefault();
        }
        public void Update(ProdOrderCancelLine p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProdOrderCancelLine>().Update(p);
        }

        public IEnumerable<ProdOrderCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProdOrderCancelLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProdOrderCancelLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public ProdOrderCancelLine Find(int id)
        {
            return _unitOfWork.Repository<ProdOrderCancelLine>().Find(id);
        }


        public IQueryable<ProdOrderCancelLineViewModel> GetProdOrderCancelLineListForHeader(int ProdOrderCancelHeaderId)
        {

            return (from p in db.ProdOrderCancelLine
                    join t in db.ProdOrderLine on p.ProdOrderLineId equals t.ProdOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in db.ProdOrderHeader on tab.ProdOrderHeaderId equals t1.ProdOrderHeaderId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join t2 in db.Product on tab.ProductId equals t2.ProductId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t3 in db.MaterialPlanLine on tab.MaterialPlanLineId equals t3.MaterialPlanLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t4 in db.MaterialPlanHeader on tab3.MaterialPlanHeaderId equals t4.MaterialPlanHeaderId into table4
                    from tab4 in table4.DefaultIfEmpty()
                    join t5 in db.ProdOrderCancelHeader on p.ProdOrderCancelHeaderId equals t5.ProdOrderCancelHeaderId into table5
                    from tab5 in table5.DefaultIfEmpty()
                    where p.ProdOrderCancelHeaderId == ProdOrderCancelHeaderId
                    orderby p.ProdOrderCancelLineId
                    select new ProdOrderCancelLineViewModel
                    {
                        ProdOrderNo = tab1.DocNo,
                        ProdOrderCancelHeaderId = p.ProdOrderCancelHeaderId,
                        ProdOrderCancelLineId = p.ProdOrderCancelLineId,
                        ProdOrderLineId = p.ProdOrderLineId,
                        Qty = p.Qty,
                        ProductId = tab2.ProductId,
                        ProductName = tab2.ProductName,
                        UnitId = tab2.UnitId,
                        MaterialPlanDocNo = tab4.DocNo,
                        DivisionId = tab5.DivisionId,
                        unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                    });

        }

        public ProdOrderCancelLine Add(ProdOrderCancelLine p)
        {
            _unitOfWork.Repository<ProdOrderCancelLine>().Insert(p);
            return p;
        }
        public ProdOrderCancelLineViewModel GetProdOrderCancelLine(int id)
        {
            var QRy = from p in db.ProdOrderCancelLine
                      join t in db.ProdOrderLine on p.ProdOrderLineId equals t.ProdOrderLineId into table
                      from tab in table.DefaultIfEmpty()
                      join t2 in db.ProdOrderHeader on tab.ProdOrderHeaderId equals t2.ProdOrderHeaderId into table2
                      from tab2 in table2.DefaultIfEmpty()
                      join t1 in db.Product on tab.ProductId equals t1.ProductId into table1
                      from tab1 in table1.DefaultIfEmpty()
                      join t in db.ViewProdOrderBalance on p.ProdOrderLineId equals t.ProdOrderLineId into table3
                      from tab3 in table3.DefaultIfEmpty()
                      where p.ProdOrderCancelLineId == id
                      select new ProdOrderCancelLineViewModel
                      {
                          ProdOrderNo = tab2.DocNo,
                          ProductId = tab1.ProductId,
                          ProductName = tab1.ProductName,
                          ProdOrderCancelHeaderId = p.ProdOrderCancelHeaderId,
                          ProdOrderCancelLineId = p.ProdOrderCancelLineId,
                          Qty = p.Qty,
                          BalanceQty = (tab3 == null ? p.Qty : tab3.BalanceQty + p.Qty),
                          ProdOrderLineId = p.ProdOrderLineId,
                          LockReason = p.LockReason,
                      };

            var temp = (QRy).FirstOrDefault();
            return temp;
        }
        public void Dispose()
        {
        }

        public IEnumerable<ProdOrderCancelLineViewModel> GetProdOrderLineForMultiSelect(ProdOrderCancelFilterViewModel svm)
        {
            var Header = new ProdOrderCancelHeaderService(_unitOfWork).Find(svm.ProdOrderCancelHeaderId);

            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] Dimension2IdArr = null;
            if (!string.IsNullOrEmpty(svm.Dimension2Id)) { Dimension2IdArr = svm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dimension2IdArr = new string[] { "NA" }; }

            string[] Dimension1IdArr = null;
            if (!string.IsNullOrEmpty(svm.Dimension1Id)) { Dimension1IdArr = svm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dimension1IdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProdOrderId)) { SaleOrderIdArr = svm.ProdOrderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var temp = (from p in db.ViewProdOrderBalance
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join t3 in db.Units on tab2.UnitId equals t3.UnitId into table3
                        from tab3 in table3.DefaultIfEmpty()
                        join t2 in db.ProdOrderLine on p.ProdOrderLineId equals t2.ProdOrderLineId
                        where p.BalanceQty > 0
                        select new 
                        {
                            BalanceQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            ProdOrderNo = p.ProdOrderNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            ProdOrderCancelHeaderId = svm.ProdOrderCancelHeaderId,
                            ProdOrderLineId = p.ProdOrderLineId,
                            unitDecimalPlaces = (tab3 == null ? 0 : tab3.DecimalPlaces),
                            Dimension1Name = t2.Dimension1.Dimension1Name,
                            Dimension2Name = t2.Dimension2.Dimension2Name,
                            Specification = t2.Specification,
                            UnitName = tab2.Unit.UnitName,
                            ProdOrderHeaderId = p.ProdOrderHeaderId,
                            ProductGroupId = tab2.ProductGroupId,
                            OrderDocDate = p.IndentDate,
                            DocTypeId=p.DocTypeId,
                            SiteId=p.SiteId,
                            DivisionId=p.DivisionId,
                            Dimension1Id=p.Dimension1Id,
                            Dimension2Id=p.Dimension2Id,
                        });

            if (!string.IsNullOrEmpty(svm.ProductId))
                temp = temp.Where(m => ProductIdArr.Contains(m.ProductId.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension2Id))
                temp = temp.Where(m => Dimension2IdArr.Contains(m.Dimension2Id.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension1Id))
                temp = temp.Where(m => Dimension1IdArr.Contains(m.Dimension1Id.ToString()));

            if (!string.IsNullOrEmpty(svm.ProdOrderId))
                temp = temp.Where(m => SaleOrderIdArr.Contains(m.ProdOrderHeaderId.ToString()));

            if (!string.IsNullOrEmpty(svm.ProductGroupId))
                temp = temp.Where(m => ProductGroupIdArr.Contains(m.ProductGroupId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                temp = temp.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                temp = temp.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                temp = temp.Where(m => m.SiteId == Header.SiteId);

            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                temp = temp.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                temp = temp.Where(m => m.DivisionId == Header.DivisionId);

            if (svm.UpToDate.HasValue)
                temp = temp.Where(m => m.OrderDocDate <= svm.UpToDate);

            return from p in temp
                   orderby p.OrderDocDate descending, p.ProdOrderNo descending
                   select new ProdOrderCancelLineViewModel
                   {

                       BalanceQty = p.BalanceQty,
                       Qty = p.BalanceQty,
                       ProdOrderNo = p.ProdOrderNo,
                       ProductName = p.ProductName,
                       ProductId = p.ProductId,
                       ProdOrderCancelHeaderId = svm.ProdOrderCancelHeaderId,
                       ProdOrderLineId = p.ProdOrderLineId,
                       unitDecimalPlaces = p.unitDecimalPlaces,
                       Dimension1Name = p.Dimension1Name,
                       Dimension2Name = p.Dimension2Name,
                       Specification = p.Specification,
                       UnitName = p.UnitName,
                       ProdOrderHeaderId = p.ProdOrderHeaderId,
                       ProductGroupId = p.ProductGroupId,
                       OrderDocDate = p.OrderDocDate,

                   };

        }


        public IQueryable<ComboBoxResult> GetPendingProdOrders(int filter, string term)//DocTypeId
        {

            var Header = new ProdOrderCancelHeaderService(_unitOfWork).Find(filter);

            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            string[] filterProducts = null;
            if (!string.IsNullOrEmpty(settings.filterProducts)) { filterProducts = settings.filterProducts.Split(",".ToCharArray()); }
            else { filterProducts = new string[] { "NA" }; }

            string[] filterProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { filterProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { filterProductTypes = new string[] { "NA" }; }

            string[] filterProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { filterProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { filterProductGroups = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var Query = from p in db.ViewProdOrderBalance
                        join t in db.ProdOrderHeader on p.ProdOrderHeaderId equals t.ProdOrderHeaderId into ProdTable
                        from ProTab in ProdTable.DefaultIfEmpty()
                        join Prod in db.Product on p.ProductId equals Prod.ProductId
                        where p.BalanceQty > 0
                        select new
                        {
                            DocNo = ProTab.DocNo,
                            DocTypeId = p.DocTypeId,
                            ProductId = p.ProductId,
                            ProductGroupId = Prod.ProductGroupId,
                            ProductTypeId = Prod.ProductGroup.ProductTypeId,
                            SiteId = ProTab.SiteId,
                            DivisionId = ProTab.DivisionId,
                            ProdOrderHeaderId = p.ProdOrderHeaderId,
                            DocTypeName = ProTab.DocType.DocumentTypeName,
                        };

            if (!string.IsNullOrEmpty(term))
                Query = Query.Where(m => m.DocNo.ToLower().Contains(term.ToLower()));
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));
            if (!string.IsNullOrEmpty(settings.filterProducts))
                Query = Query.Where(m => filterProducts.Contains(m.ProductId.ToString()));
            if (!string.IsNullOrEmpty(settings.filterProductGroups))
                Query = Query.Where(m => filterProductGroups.Contains(m.ProductGroupId.ToString()));
            if (!string.IsNullOrEmpty(settings.filterProductTypes))
                Query = Query.Where(m => filterProductTypes.Contains(m.ProductTypeId.ToString()));
            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == CurrentSiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == CurrentDivisionId);

            return (from p in Query
                    group p by p.ProdOrderHeaderId into g
                    orderby g.Max(m => m.DocNo)
                    select new ComboBoxResult
                    {
                        id = g.Key.ToString(),
                        text = g.Max(m => m.DocNo) + " | " + g.Max(m => m.DocTypeName)
                    });

        }

        public IQueryable<ComboBoxResult> GetPendingProductsForProdOrderCancel(int filter, string term)//DocTypeId
        {

            var HEader = new ProdOrderCancelHeaderService(_unitOfWork).Find(filter);

            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(HEader.DocTypeId, HEader.DivisionId, HEader.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            string[] filterProducts = null;
            if (!string.IsNullOrEmpty(settings.filterProducts)) { filterProducts = settings.filterProducts.Split(",".ToCharArray()); }
            else { filterProducts = new string[] { "NA" }; }

            string[] filterProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { filterProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { filterProductTypes = new string[] { "NA" }; }

            string[] filterProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { filterProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { filterProductGroups = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var Query = from p in db.ViewProdOrderBalance
                        join t in db.ProdOrderHeader on p.ProdOrderHeaderId equals t.ProdOrderHeaderId into ProdTable
                        from ProTab in ProdTable.DefaultIfEmpty()
                        join Prod in db.Product on p.ProductId equals Prod.ProductId
                        where p.BalanceQty > 0
                        select new
                        {
                            ProductName = Prod.ProductName,
                            DocTypeId = p.DocTypeId,
                            ProductId = p.ProductId,
                            ProductGroupId = Prod.ProductGroupId,
                            ProductTypeId = Prod.ProductGroup.ProductTypeId,
                            SiteId = ProTab.SiteId,
                            DivisionId = ProTab.DivisionId,
                        };

            if (!string.IsNullOrEmpty(term))
                Query = Query.Where(m => m.ProductName.ToLower().Contains(term.ToLower()));

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterProducts))
                Query = Query.Where(m => filterProducts.Contains(m.ProductId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterProductGroups))
                Query = Query.Where(m => filterProductGroups.Contains(m.ProductGroupId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterProductTypes))
                Query = Query.Where(m => filterProductTypes.Contains(m.ProductTypeId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == CurrentSiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == CurrentDivisionId);


            return (from p in Query
                    group p by p.ProductId into g
                    orderby g.Max(m => m.ProductName)
                    select new ComboBoxResult
                    {
                        id = g.Key.ToString(),
                        text = g.Max(m => m.ProductName),
                    });
        }

        public IEnumerable<ProdOrderCancelLine> GetProdOrderCancelLineForMaterialPlanCancel(int id)
        {
            return (from p in db.ProdOrderCancelLine
                    where p.MaterialPlanCancelLineId == id
                    select p
                        );

        }

        public Task<IEquatable<ProdOrderCancelLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProdOrderCancelLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }


    }
}
