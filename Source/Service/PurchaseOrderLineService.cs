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
using System.Data.SqlClient;
using System.Configuration;

namespace Service
{
    public interface IPurchaseOrderLineService : IDisposable
    {
        PurchaseOrderLine Create(PurchaseOrderLine pt);
        void Delete(int id);
        void Delete(PurchaseOrderLine pt);
        PurchaseOrderLine Find(int id);
        IEnumerable<PurchaseOrderLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseOrderLine pt);
        PurchaseOrderLine Add(PurchaseOrderLine pt);
        IEnumerable<PurchaseOrderLine> GetPurchaseOrderLineList();
        IEnumerable<PurchaseOrderLineViewModel> GetLineListForIndex(int headerId);//HeaderId
        IEnumerable<PurchaseOrderLine> GetLineList(int headerId);

        // IEnumerable<PurchaseOrderLine> GetPurchaseOrderLineList(int buyerId);
        Task<IEquatable<PurchaseOrderLine>> GetAsync();
        Task<PurchaseOrderLine> FindAsync(int id);
        PurchaseOrderLineViewModel GetPurchaseOrderLine(int id);//Line Id
        IEnumerable<PurchaseOrderLineViewModel> GetPurchaseIndentForFilters(PurchaseOrderLineFilterViewModel vm);

        PurchaseOrderLineViewModel GetLineDetail(int id);//LineId
        int GetPendingPurchaseIndentCount(int ProductId);//ProductId
        List<String> GetProcGenProductUids(int DocTypeId, decimal Qty, int DivisionId, int SiteId);
        IEnumerable<PurchaseIndentLineListViewModel> GetPendingPurchaseIndentHelpList(int Id, string term);//PurchaseOrderHeaderId
        IEnumerable<PurchaseOrderLine> GetLineListForChargeCalculation(int id);//Purchase InvoiceHeader Id
        IEnumerable<PurchaseIndentLineListViewModel> GetPendingPurchaseIndentHelpListForIndentCancel(int Id, string term);
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        PurchaseIndentLineViewModel GetPurchaseIndentLineForOrder(int id);
        int GetMaxSr(int id);
    }

    public class PurchaseOrderLineService : IPurchaseOrderLineService
    {
        private ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseOrderLine> _PurchaseOrderLineRepository;
        RepositoryQuery<PurchaseOrderLine> PurchaseOrderLineRepository;
        public PurchaseOrderLineService(IUnitOfWorkForService unitOfWork)
        {
            this.db = new ApplicationDbContext();
            _unitOfWork = unitOfWork;
            _PurchaseOrderLineRepository = new Repository<PurchaseOrderLine>(db);
            PurchaseOrderLineRepository = new RepositoryQuery<PurchaseOrderLine>(_PurchaseOrderLineRepository);            
        }

        public PurchaseOrderLine Find(int id)
        {
            return _unitOfWork.Repository<PurchaseOrderLine>().Find(id);
        }

        public PurchaseOrderLine Create(PurchaseOrderLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseOrderLine>().Insert(pt);
            return pt;
        }

        public PurchaseOrderLineViewModel GetLineDetail(int id)
        {
            return (from p in db.ViewPurchaseOrderBalance
                    join t1 in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t1.PurchaseOrderLineId
                    join t2 in db.Product on p.ProductId equals t2.ProductId
                    join unit in db.Units on t2.UnitId equals unit.UnitId
                    join dealunit in db.Units on t1.DealUnitId equals dealunit.UnitId
                    where p.PurchaseOrderLineId == id
                    select new PurchaseOrderLineViewModel
                    {
                        Dimension1Name = t1.Dimension1.Dimension1Name,
                        Dimension2Name = t1.Dimension2.Dimension2Name,
                        Dimension1Id = t1.Dimension1Id,
                        Dimension2Id = t1.Dimension2Id,
                        LotNo = t1.LotNo,
                        Qty = p.BalanceQty,
                        Specification = t1.Specification,
                        UnitId = unit.UnitId,
                        UnitName=unit.UnitName,
                        DealUnitName=dealunit.UnitName,
                        ProductId=t1.ProductId,
                        ProductName=t1.Product.ProductName,
                        unitDecimalPlaces=unit.DecimalPlaces,
                        DealunitDecimalPlaces=dealunit.DecimalPlaces,                                                
                        DealUnitId = dealunit.UnitId,
                        OrderDealQty=t1.DealQty,
                        OrderQty=t1.Qty,
                        DealQty = p.BalanceQty * t1.UnitConversionMultiplier,
                        UnitConversionMultiplier = t1.UnitConversionMultiplier,
                        Rate = p.Rate,
                    }
                        ).FirstOrDefault();

        }
        public PurchaseOrderLineViewModel GetPurchaseOrderLine(int id)
        {
            return (from p in db.PurchaseOrderLine
                    where p.PurchaseOrderLineId == id
                    select new PurchaseOrderLineViewModel
                    {
                        PurchaseIndentDocNo = p.PurchaseIndentLine.PurchaseIndentHeader.DocNo,
                        Amount = p.Amount,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        Dimension1Id = p.Dimension1Id,
                        Dimension2Id = p.Dimension2Id,
                        DueDate = p.DueDate,
                        LotNo = p.LotNo,
                        ProductId = p.ProductId,
                        PurchaseIndentLineId = p.PurchaseIndentLineId,
                        PurchaseOrderHeaderDocNo = p.PurchaseOrderHeader.DocNo,
                        PurchaseOrderHeaderId = p.PurchaseOrderHeaderId,
                        PurchaseOrderLineId = p.PurchaseOrderLineId,
                        Qty = p.Qty,
                        Rate = p.Rate,
                        Remark = p.Remark,
                        ShipDate = p.ShipDate,
                        Specification = p.Specification,
                        UnitId = p.Product.UnitId,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        DiscountPer = p.DiscountPer,
                        LockReason=p.LockReason,

                    }

                        ).FirstOrDefault();
        }
        public IEnumerable<PurchaseOrderLineViewModel> GetPurchaseIndentForFilters(PurchaseOrderLineFilterViewModel vm)
        {

            byte? UnitConvForId = new PurchaseOrderHeaderService(_unitOfWork).Find(vm.PurchaseOrderHeaderId).UnitConversionForId;


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
                            orderby tab.DocDate,tab.DocNo,tab1.Sr
                            select new PurchaseOrderLineViewModel
                            {
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Specification = tab1.Specification,
                                IndentBalanceQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                PurchaseOrderHeaderDocNo = tab.DocNo,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                Rate=vm.Rate,
                                PurchaseOrderHeaderId = vm.PurchaseOrderHeaderId,
                                PurchaseIndentLineId = p.PurchaseIndentLineId,
                                UnitId = tab2.UnitId,
                                PurchaseIndentDocNo = p.PurchaseIndentNo,
                                DealUnitId = (tab3 == null ? tab2.UnitId : vm.DealUnitId),
                                UnitConversionMultiplier = (tab3 == null ? 1 : tab3.ToQty / tab3.FromQty),
                                UnitConversionException = tab3 == null ? true : false,
                                unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                                DealunitDecimalPlaces = (tab3 == null ? tab2.Unit.DecimalPlaces : Dealunit.DecimalPlaces)
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
                            orderby tab2.ProductName
                            select new PurchaseOrderLineViewModel
                            {
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Specification = tab1.Specification,
                                IndentBalanceQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                PurchaseOrderHeaderDocNo = tab.DocNo,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                Rate=vm.Rate,
                                PurchaseOrderHeaderId = vm.PurchaseOrderHeaderId,
                                PurchaseIndentLineId = p.PurchaseIndentLineId,
                                UnitId = tab2.UnitId,
                                PurchaseIndentDocNo = p.PurchaseIndentNo,
                                DealUnitId = tab2.UnitId,
                                UnitConversionMultiplier = 1,
                                unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                                DealunitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            }

                        );
                return temp;
            }

        }
        public PurchaseOrderLineViewModel GetPurchaseOrderLineJson(int id)
        {
            return (from p in db.PurchaseOrderLine
                    where p.PurchaseOrderLineId == id
                    select new PurchaseOrderLineViewModel
                    {
                        ProductId = p.ProductId,
                        DueDate = p.DueDate,
                        Qty = p.Qty,
                        Remark = p.Remark,
                        PurchaseOrderHeaderId = p.PurchaseOrderHeaderId,
                        PurchaseIndentLineId = p.PurchaseIndentLineId,
                        PurchaseOrderLineId = p.PurchaseOrderLineId,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        LotNo = p.LotNo,
                        ProductName = p.Product.ProductName,
                        Dimension1Id = p.Dimension1Id,
                        Dimension1Name = p.Dimension1.Dimension1Name,
                        Dimension2Name = p.Dimension2.Dimension2Name,
                        Dimension2Id = p.Dimension2Id,
                        Specification = p.Specification,
                        UnitId = p.Product.UnitId,
                        DealUnitId = p.DealUnitId,
                        DealQty = p.DealQty,
                    }

                        ).FirstOrDefault();
        }
        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseOrderLine>().Delete(id);
        }

        public void Delete(PurchaseOrderLine pt)
        {
            _unitOfWork.Repository<PurchaseOrderLine>().Delete(pt);
        }

        public void Update(PurchaseOrderLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseOrderLine>().Update(pt);
        }
        public IEnumerable<PurchaseOrderLineViewModel> GetLineListForIndex(int headerId)
        {
            var pt = (from p in db.PurchaseOrderLine
                      where p.PurchaseOrderHeaderId == headerId
                      orderby p.PurchaseOrderLineId
                      join t in db.Dimension1 on p.Dimension1Id equals t.Dimension1Id into table
                      from dim1 in table.DefaultIfEmpty()
                      join t1 in db.Dimension2 on p.Dimension2Id equals t1.Dimension2Id into table1
                      from dim2 in table1.DefaultIfEmpty()
                      join dbv in db.ViewPurchaseOrderBalance on p.PurchaseOrderLineId equals dbv.PurchaseOrderLineId into vd
                      from viewdata in vd.DefaultIfEmpty()
                      orderby p.Sr
                      select new PurchaseOrderLineViewModel
                      {
                          PurchaseOrderLineId = p.PurchaseOrderLineId,
                          ProductId = p.ProductId,
                          ProductName = p.Product.ProductName,
                          Specification = p.Specification,
                          Dimension1Name = dim1.Dimension1Name,
                          Dimension2Name = dim2.Dimension2Name,
                          LotNo = p.LotNo,
                          PurchaseIndentDocNo = p.PurchaseIndentLine.PurchaseIndentHeader.DocNo,
                          PurchaseIndentLineId=p.PurchaseIndentLineId,
                          IndentHeaderId=p.PurchaseIndentLine.PurchaseIndentHeaderId,
                          IndentDocTypeId=p.PurchaseIndentLine.PurchaseIndentHeader.DocTypeId,
                          Qty = p.Qty,
                          DealQty = p.DealQty,
                          Rate = p.Rate,
                          Amount = p.Amount,
                          DealUnitId = p.DealUnitId,
                          Remark = p.Remark,
                          DueDate = p.DueDate,
                          UnitId = p.Product.UnitId,
                          UnitName = p.Product.Unit.UnitName,
                          DealUnitName = p.DealUnit.UnitName,
                          DealunitDecimalPlaces = p.DealUnit.DecimalPlaces,
                          ProgressPerc = (int)((((p.Qty - viewdata.BalanceQty) / p.Qty) * 100)),
                          unitDecimalPlaces = p.Product.Unit.DecimalPlaces,
                          DiscountPer = p.DiscountPer
                      }
                        );

            return pt;


        }

        public IEnumerable<PurchaseOrderLine> GetLineList(int headerId)
        {
            var pt = (from p in db.PurchaseOrderLine
                      where p.PurchaseOrderHeaderId == headerId
                      select p
                          );

            return pt;


        }
        public IEnumerable<PurchaseOrderLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseOrderLine>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PurchaseOrderLine> GetPurchaseOrderLineList()
        {
            var pt = _unitOfWork.Repository<PurchaseOrderLine>().Query().Get();

            return pt;
        }

        public PurchaseOrderLine Add(PurchaseOrderLine pt)
        {
            _unitOfWork.Repository<PurchaseOrderLine>().Insert(pt);
            return pt;
        }

        public int GetPendingPurchaseIndentCount(int ProductId)//ProductId
        {
            return (from p in db.ViewPurchaseIndentBalance
                    where p.ProductId == ProductId && p.BalanceQty > 0
                    group p by p.PurchaseIndentNo into g
                    select g.Key
                        ).Count();
        }

        public List<String> GetProcGenProductUids(int DocTypeId, decimal Qty, int DivisionId, int SiteId)
        {
            string ProcName = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId).SqlProcGenProductUID;

            List<string> CalculationLineList = new List<String>();


            if (!string.IsNullOrEmpty(ProcName))
            {

                using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
                {
                    sqlConnection.Open();

                    int TypeId = DocTypeId;

                    SqlCommand Totalf = new SqlCommand("SELECT * FROM " + ProcName + "( " + TypeId + ", " + Qty + ")", sqlConnection);

                    SqlDataReader ExcessStockQty = (Totalf.ExecuteReader());
                    while (ExcessStockQty.Read())
                    {
                        CalculationLineList.Add((string)ExcessStockQty.GetValue(0));
                    }
                }
            }
            //IEnumerable<string> CalculationLineList = db.Database.SqlQuery<string>("SELECT * FROM " + ProcName + " ("+ SqlParameterDocType+"," +SqlParameterQty+") ").ToList();

            return CalculationLineList.ToList();

        }

        public IEnumerable<PurchaseIndentLineListViewModel> GetPendingPurchaseIndentHelpList(int Id, string term)
        {

            var PurchaseOrder = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(PurchaseOrder.DocTypeId, PurchaseOrder.DivisionId, PurchaseOrder.SiteId);

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
                        select new PurchaseIndentLineListViewModel
                        {
                            DocNo = g.Max(m => m.p.PurchaseIndentNo) + " | " + g.Max(m => m.p.DocType.DocumentTypeShortName),
                            PurchaseIndentHeaderId = g.Key,

                            //    DocumentTypeName=g.Max(p=>p.p.DocumentTypeShortName)
                        }
                          ).Take(20);

            return list.ToList();
        }

        public IEnumerable<PurchaseIndentLineListViewModel> GetPendingPurchaseIndentHelpListForIndentCancel(int Id, string term)
        {

            var PurchaseIndentCancel = new PurchaseIndentCancelHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(PurchaseIndentCancel.DocTypeId, PurchaseIndentCancel.DivisionId, PurchaseIndentCancel.SiteId);

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
                        select new PurchaseIndentLineListViewModel
                        {
                            DocNo = g.Max(m => m.p.PurchaseIndentNo) + " | " + g.Max(m => m.p.DocType.DocumentTypeShortName),
                            PurchaseIndentHeaderId = g.Key,

                            //    DocumentTypeName=g.Max(p=>p.p.DocumentTypeShortName)
                        }
                          ).Take(20);

            return list.ToList();
        }



        public IEnumerable<PurchaseOrderLine> GetLineListForChargeCalculation(int id)//Purchase InvoiceHeader Id
        {

            return (from p in db.PurchaseOrderLine
                    join t in db.PurchaseOrderLineCharge on p.PurchaseOrderLineId equals t.LineTableId into table
                    from tab in table.DefaultIfEmpty()
                    where tab == null && p.PurchaseOrderHeaderId == id
                    select p
                        );

        }

        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var PurchaseOrder = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(PurchaseOrder.DocTypeId, PurchaseOrder.DivisionId, PurchaseOrder.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            var list = (from p in db.Product
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
                        group new { p } by p.ProductId into g
                        select new ComboBoxList
                        {
                            PropFirst = g.Max(m => m.p.ProductName),
                            Id = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }


        public PurchaseIndentLineViewModel GetPurchaseIndentLineForOrder(int id)
        {
            return (from p in db.PurchaseIndentLine
                    join t in db.ViewPurchaseIndentBalance on p.PurchaseIndentLineId equals t.PurchaseIndentLineId into table
                    from tab in table.DefaultIfEmpty()
                    where p.PurchaseIndentLineId == id
                    select new PurchaseIndentLineViewModel
                    {
                        ProductId = p.ProductId,
                        DueDate = p.DueDate,
                        Qty = tab.BalanceQty,
                        Remark = p.Remark,
                        PurchaseIndentHeaderId = p.PurchaseIndentHeaderId,
                        PurchaseIndentLineId = p.PurchaseIndentLineId,
                        ProductName = p.Product.ProductName,
                        Dimension1Id = p.Dimension1Id,
                        Dimension1Name = p.Dimension1.Dimension1Name,
                        Dimension2Name = p.Dimension2.Dimension2Name,
                        Dimension2Id = p.Dimension2Id,
                        Specification = p.Specification,
                        UnitId = p.Product.UnitId,
                        MaterialPlanHeaderDocNo = p.MaterialPlanLine.MaterialPlanHeader.DocNo,
                        MaterialPlanLineId = p.MaterialPlanLineId,                        
                    }

                        ).FirstOrDefault();
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.PurchaseOrderLine
                            where p.PurchaseOrderHeaderId == id
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


        public Task<IEquatable<PurchaseOrderLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrderLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
