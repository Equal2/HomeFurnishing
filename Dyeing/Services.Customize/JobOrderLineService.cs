using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Data.SqlClient;
using System.Configuration;
using Infrastructure.IO;
using Models.Customize.Models;
using Models.Customize.ViewModels;
using Models.BasicSetup.ViewModels;
using Models.BasicSetup.Models;
using Models.Customize.DataBaseViews;
using Models.Company.Models;
using Components.Logging;
using ProjLib.Constants;
using AutoMapper;
using Models.Company.ViewModels;
using Services.BasicSetup;
using ProjLib;
using System.Xml.Linq;

namespace Services.Customize
{
    public interface IJobOrderLineService : IDisposable
    {
        JobOrderLine Create(JobOrderLine s);
        JobOrderLineViewModel Create(JobOrderLineViewModel svm, string UserName);
        void SaveMultiple(JobOrderMasterDetailModel vm, string UserName);
        void Delete(int id);
        void Delete(JobOrderLine s);
        void Delete(JobOrderLineViewModel vm, string UserName);
        JobOrderLineViewModel GetJobOrderLine(int id);
        JobOrderLine Find(int id);
        void Update(JobOrderLine s);
        void Update(JobOrderLineViewModel svm, string UserName);
        IQueryable<JobOrderLineViewModel> GetJobOrderLineListForIndex(int JobOrderHeaderId);
        IQueryable<JobOrderBomViewModel> GetConsumptionLineListForIndex(int JobOrderHeaderId);
        IEnumerable<JobOrderLineViewModel> GetJobOrderLineforDelete(int headerid);
        IEnumerable<ProcGetBomForWeavingViewModel> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int ProcessId, decimal Qty, int DocTypeId, string ProcName);
        List<String> GetProcGenProductUids(int DocTypeId, decimal Qty, int DivisionId, int SiteId);
        JobOrderLineViewModel GetLineDetailFromUId(string UID);
        IEnumerable<JobOrderLineViewModel> GetProdOrdersForFilters(JobOrderLineFilterViewModel vm);
        IQueryable<ComboBoxResult> GetPendingProdOrderHelpList(int Id, string term);//PurchaseOrderHeaderId
        IEnumerable<ProdOrderHeaderListViewModel> GetPendingProdOrdersWithPatternMatch(int Id, string term, int Limiter);
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);
        List<ComboBoxResult> GetBarCodesForWeavingWizard(int id, string term);
        IEnumerable<JobRate> GetJobRate(int JobOrderHeaderId, int ProductId);
        decimal GetUnitConversionForProdOrderLine(int ProdLineId, byte UnitConvForId, string DealUnitId);
        IEnumerable<JobOrderLine> GetJobOrderLineListForHeader(int HeaderId);

        void UpdateProdQtyOnJobOrder(int id, int JobOrderLineId, DateTime DocDate, decimal Qty);

        #region Helper Methods
        IEnumerable<Unit> GetUnitList();
        Unit FindUnit(string UnitId);
        ProductViewModel GetProduct(int ProductId);
        UnitConversion GetUnitConversion(int Id, string UnitId, int conversionForId, string DealUnitId);
        ProductUid FindProductUid(int Id);
        ProductUid FindProductUid(string Name);
        bool CheckProductUidProcessDone(string ProductUidName, int ProcessID);
        IEnumerable<ProdOrderHeaderListViewModel> GetPendingProdOrders(int ProductId);
        ProdOrderLineViewModel GetProdOrderDetailBalance(int id);
        ProdOrderLineViewModel GetProdOrderForProdUid(int id);
        #endregion
    }

    public class JobOrderLineService : IJobOrderLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobOrderLine> _jobOrderLineRepository;
        private readonly IRepository<JobOrderHeader> _jobOrderHeaderRepository;
        private readonly IStockService _stockService;
        private readonly IStockProcessService _stockProcessService;
        private readonly ILogger _logger;
        private readonly IModificationCheck _modificationCheck;
        //private readonly IJobReceiveSettingsService _jobreceiveSettingsService;

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        public JobOrderLineService(IUnitOfWork unitOfWork, IRepository<JobOrderLine> joborderLineRepo, IRepository<JobOrderHeader> joborderHeaderRepo,
             ILogger log
            , IStockService stockSErv, IStockProcessService stockProcServ,
            IModificationCheck modificationCheck
            //, IJobReceiveSettingsService JobReceiveSettingsServ
            )
        {
            _unitOfWork = unitOfWork;
            _jobOrderLineRepository = joborderLineRepo;
            _jobOrderHeaderRepository = joborderHeaderRepo;
            _stockProcessService = stockProcServ;
            _stockService = stockSErv;
            _modificationCheck = modificationCheck;
            _logger = log;
            // _jobreceiveSettingsService = JobReceiveSettingsServ;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }      

        public JobOrderLine Create(JobOrderLine S)
        {
            S.ObjectState = ObjectState.Added;
            _jobOrderLineRepository.Add(S);
            return S;
        }

        public void Delete(int id)
        {
            _jobOrderLineRepository.Delete(id);
        }

        public void Delete(JobOrderLine s)
        {
            _jobOrderLineRepository.Delete(s);
        }

        public void Update(JobOrderLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _jobOrderLineRepository.Update(s);
        }


        public JobOrderLineViewModel GetJobOrderLine(int id)
        {
            var temp = (from p in _jobOrderLineRepository.Instance
                        where p.JobOrderLineId == id
                        select new JobOrderLineViewModel
                        {
                            ProductId = p.ProductId,
                            ProductUidHeaderId = p.ProductUidHeaderId,
                            DueDate = p.DueDate,
                            Qty = p.Qty,
                            Remark = p.Remark,
                            JobOrderHeaderId = p.JobOrderHeaderId,
                            JobOrderLineId = p.JobOrderLineId,
                            ProductName = p.Product.ProductName,
                            Dimension1Id = p.Dimension1Id,
                            Dimension1Name = p.Dimension1.Dimension1Name,
                            Dimension2Name = p.Dimension2.Dimension2Name,
                            Dimension2Id = p.Dimension2Id,
                            ProductUidId = p.ProductUidId,
                            ProductUidName = p.ProductUid.ProductUidName,
                            ProdOrderLineId = p.ProdOrderLineId,
                            ProdOrderDocNo = p.ProdOrderLine.ProdOrderHeader.DocNo,
                            LotNo = p.LotNo,
                            FromProcessId = p.FromProcessId,
                            DealUnitId = p.DealUnitId,
                            DealQty = p.DealQty,
                            LockReason = p.LockReason,
                            LossQty = p.LossQty,
                            Rate = p.Rate,
                            Amount = p.Amount,
                            NonCountedQty = p.NonCountedQty,
                            UnitId = p.UnitId,
                            UnitName = p.Unit.UnitName,
                            UnitConversionMultiplier = p.UnitConversionMultiplier,
                            UnitDecimalPlaces = p.Unit.DecimalPlaces,
                            DealUnitDecimalPlaces = p.DealUnit.DecimalPlaces,
                            Specification = p.Specification,
                        }).FirstOrDefault();

            temp.ProdOrderBalanceQty = (from p in _unitOfWork.Repository<ViewProdOrderBalance>().Instance
                                        where p.ProdOrderLineId == id
                                        where p.BalanceQty > 0
                                        select p.BalanceQty
                        ).FirstOrDefault() + temp.Qty;


            return temp;
        }
        public JobOrderLine Find(int id)
        {
            return _jobOrderLineRepository.Find(id);
        }




        public IQueryable<JobOrderBomViewModel> GetConsumptionLineListForIndex(int JobOrderHeaderId)
        {
            var temp = from p in _unitOfWork.Repository<JobOrderBom>().Instance
                       join t in _unitOfWork.Repository<Dimension1>().Instance on p.Dimension1Id equals t.Dimension1Id into table
                       from Dim1 in table.DefaultIfEmpty()
                       join t1 in _unitOfWork.Repository<Dimension2>().Instance on p.Dimension2Id equals t1.Dimension2Id into table1
                       from Dim2 in table1.DefaultIfEmpty()
                       join t2 in _unitOfWork.Repository<Product>().Instance on p.ProductId equals t2.ProductId into table2
                       from ProdTab in table2.DefaultIfEmpty()
                       orderby p.JobOrderLineId
                       where p.JobOrderHeaderId == JobOrderHeaderId
                       select new JobOrderBomViewModel
                       {
                           UnitName = ProdTab.Unit.UnitName,
                           UnitDecimalPlaces = ProdTab.Unit.DecimalPlaces,
                           Dimension1Name = Dim1.Dimension1Name,
                           Dimension2Name = Dim2.Dimension2Name,
                           JobOrderBomId = p.JobOrderBomId,
                           JobOrderHeaderId = p.JobOrderHeaderId,
                           JobOrderLineId = p.JobOrderLineId,
                           ProductName = ProdTab.ProductName,
                           Qty = p.Qty,
                       };
            return temp;
        }

        public IQueryable<JobOrderLineViewModel> GetJobOrderLineListForIndex(int JobOrderHeaderId)
        {
            var temp = from p in _jobOrderLineRepository.Instance
                       join t in _unitOfWork.Repository<Dimension1>().Instance on p.Dimension1Id equals t.Dimension1Id into table
                       from Dim1 in table.DefaultIfEmpty()
                       join t1 in _unitOfWork.Repository<Dimension2>().Instance on p.Dimension2Id equals t1.Dimension2Id into table1
                       from Dim2 in table1.DefaultIfEmpty()
                       join t3 in _unitOfWork.Repository<JobOrderLineStatus>().Instance on p.JobOrderLineId equals t3.JobOrderLineId
                       into table3
                       from tab3 in table3.DefaultIfEmpty()
                       where p.JobOrderHeaderId == JobOrderHeaderId
                       orderby p.Sr
                       select new JobOrderLineViewModel
                       {
                           ProductUidName = p.ProductUid.ProductUidName,
                           ProdOrderDocNo = p.ProdOrderLine.ProdOrderHeader.DocNo,
                           LotNo = p.LotNo,
                           FromProcessName = p.FromProcess.ProcessName,
                           DealUnitId = p.DealUnitId,
                           DealUnitName = p.DealUnit.UnitName,
                           DealQty = p.DealQty,
                           NonCountedQty = p.NonCountedQty,
                           LossQty = p.LossQty,
                           Rate = p.Rate,
                           ProductUidHeaderId = p.ProductUidHeaderId,
                           Amount = p.Amount,
                           Dimension1Id = p.Dimension1Id,
                           Dimension2Id = p.Dimension2Id,
                           Dimension1Name = Dim1.Dimension1Name,
                           Dimension2Name = Dim2.Dimension2Name,
                           Specification = p.Specification,
                           UnitConversionMultiplier = p.UnitConversionMultiplier,
                           UnitId = p.UnitId,
                           UnitName = p.Unit.UnitName,
                           UnitDecimalPlaces = p.Unit.DecimalPlaces,
                           DealUnitDecimalPlaces = p.DealUnit.DecimalPlaces,
                           Remark = p.Remark,
                           DueDate = p.DueDate,
                           ProductId = p.ProductId,
                           ProductName = p.Product.ProductName,
                           Qty = p.Qty,
                           JobOrderHeaderId = p.JobOrderHeaderId,
                           JobOrderLineId = p.JobOrderLineId,
                           ProgressPerc = ((p.Qty == 0 || tab3 == null) ? 0 : (int)(((((tab3.CancelQty ?? 0) + (tab3.ReceiveQty ?? 0)
                           - (tab3.AmendmentQty ?? 0)) / p.Qty) * 100))),
                       };
            return temp;
        }

        public IEnumerable<JobOrderLineViewModel> GetJobOrderLineforDelete(int headerid)
        {
            return (from p in _jobOrderLineRepository.Instance
                    where p.JobOrderHeaderId == headerid
                    select new JobOrderLineViewModel
                    {
                        JobOrderLineId = p.JobOrderLineId,
                        StockId = p.StockId,
                        StockProcessId = p.StockProcessId,
                        ProductUidId = p.ProductUidId,
                        ProductUidHeaderId = p.ProductUidHeaderId,
                    }
                        );
        }

        public IEnumerable<ProcGetBomForWeavingViewModel> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int ProcessId, decimal Qty, int DocTypeId, string ProcName)
        {
            SqlParameter SQLDocTypeID = new SqlParameter("@DocTypeId", DocTypeId);
            SqlParameter SQLProductID = new SqlParameter("@ProductId", ProductId);
            SqlParameter SQLProcessId = new SqlParameter("@ProcessId", ProcessId);
            SqlParameter SQLQty = new SqlParameter("@Qty", Qty);
            SqlParameter SQLDime1 = new SqlParameter("@Dimension1Id", Dimension1Id);
            SqlParameter SQLDime2 = new SqlParameter("@Dimension2Id", Dimension2Id);

            List<ProcGetBomForWeavingViewModel> PendingOrderQtyForPacking = new List<ProcGetBomForWeavingViewModel>();

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //string ProcName = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId).SqlProcConsumption;

            //PendingOrderQtyForPacking = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetBomForWeaving @DocTypeId, @ProductId, @ProcessId,@Qty"+(Dimension1Id==null?"":",@Dimension1Id")+(Dimension2Id==null?"":",@Dimension2Id"), SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, (Dimension1Id==null? "" : Dimension1Id), (Dimension2Id)).ToList();


            if (Dimension1Id == null && Dimension2Id == null)
            {
                PendingOrderQtyForPacking = _unitOfWork.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty).ToList();
            }
            else if (Dimension1Id == null && Dimension2Id != null)
            {
                PendingOrderQtyForPacking = _unitOfWork.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension2Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, SQLDime2).ToList();
            }
            else if (Dimension1Id != null && Dimension2Id == null)
            {
                PendingOrderQtyForPacking = _unitOfWork.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension1Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, SQLDime1).ToList();
            }
            else
            {
                PendingOrderQtyForPacking = _unitOfWork.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension1Id, @Dimension2Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, SQLDime1, SQLDime2).ToList();
            }


            return PendingOrderQtyForPacking;

        }

        public JobOrderLineViewModel GetLineDetailForCancel(int id)
        {
            var temp = (from p in _unitOfWork.Repository<ViewJobOrderBalance>().Instance
                        join t1 in _jobOrderLineRepository.Instance on p.JobOrderLineId equals t1.JobOrderLineId
                        join t2 in _unitOfWork.Repository<Product>().Instance on p.ProductId equals t2.ProductId
                        join t3 in _unitOfWork.Repository<Dimension1>().Instance on t1.Dimension1Id equals t3.Dimension1Id into table3
                        from tab3 in table3.DefaultIfEmpty()
                        join t4 in _unitOfWork.Repository<Dimension2>().Instance on t1.Dimension2Id equals t4.Dimension2Id into table4
                        from tab4 in table4.DefaultIfEmpty()
                        where p.JobOrderLineId == id
                        select new JobOrderLineViewModel
                        {
                            Dimension1Name = tab3.Dimension1Name,
                            Dimension2Name = tab4.Dimension2Name,
                            LotNo = t1.LotNo,
                            Qty = p.BalanceQty,
                            Specification = t1.Specification,
                            UnitId = t1.UnitId,
                            DealUnitId = t1.DealUnitId,
                            JobOrderLineId = t1.JobOrderLineId,
                            DealQty = p.BalanceQty * t1.UnitConversionMultiplier,
                            UnitConversionMultiplier = t1.UnitConversionMultiplier,
                            UnitName = t1.Unit.UnitName,
                            DealUnitName = t1.DealUnit.UnitName,
                            ProductId = p.ProductId,
                            ProductName = t1.Product.ProductName,
                            UnitDecimalPlaces = t2.Unit.DecimalPlaces,
                            DealUnitDecimalPlaces = t1.DealUnit.DecimalPlaces,
                            Rate = p.Rate,
                            IsUidGenerated = (t1.ProductUidHeaderId == null ? false : true),
                            ProductUidHeaderId = t1.ProductUidHeaderId
                        }
                        ).FirstOrDefault();

            if (temp.IsUidGenerated)
            {
                List<ComboBoxList> Barcodes = new List<ComboBoxList>();
                var Temp = (from p in _unitOfWork.Repository<ProductUid>().Instance
                            where p.ProductUidHeaderId == temp.ProductUidHeaderId && ((p.LastTransactionDocNo == null || p.GenDocNo == p.LastTransactionDocNo) && (p.LastTransactionDocTypeId == null || p.GenDocTypeId == p.LastTransactionDocTypeId))
                            select new { Id = p.ProductUIDId, Name = p.ProductUidName }).ToList();
                foreach (var item in Temp)
                {
                    Barcodes.Add(new ComboBoxList
                    {
                        Id = item.Id,
                        PropFirst = item.Name,
                    });
                }
                temp.BarCodes = Barcodes;
            }


            return temp;

        }

        public JobOrderLineViewModel GetLineDetailFromUId(string UID)
        {
            return (from p in _unitOfWork.Repository<ViewJobOrderBalance>().Instance
                    join t1 in _jobOrderLineRepository.Instance on p.JobOrderLineId equals t1.JobOrderLineId
                    join t in _unitOfWork.Repository<ProductUid>().Instance on t1.ProductUidId equals t.ProductUIDId into uidtable
                    from uidtab in uidtable.DefaultIfEmpty()
                    join t2 in _unitOfWork.Repository<Product>().Instance on p.ProductId equals t2.ProductId
                    join t3 in _unitOfWork.Repository<Dimension1>().Instance on t1.Dimension1Id equals t3.Dimension1Id into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t4 in _unitOfWork.Repository<Dimension2>().Instance on t1.Dimension2Id equals t4.Dimension2Id into table4
                    from tab4 in table4.DefaultIfEmpty()
                    where uidtab.ProductUidName == UID
                    select new JobOrderLineViewModel
                    {
                        Dimension1Name = tab3.Dimension1Name,
                        Dimension2Name = tab4.Dimension2Name,
                        LotNo = t1.LotNo,
                        Qty = p.BalanceQty,
                        Specification = t1.Specification,
                        UnitId = t1.UnitId,
                        DealUnitId = t1.DealUnitId,
                        DealQty = p.BalanceQty * t1.UnitConversionMultiplier,
                        UnitConversionMultiplier = t1.UnitConversionMultiplier,
                        UnitName = t1.Unit.UnitName,
                        DealUnitName = t1.DealUnit.UnitName,
                        Rate = p.Rate,

                    }
                        ).FirstOrDefault();

        }


        public List<String> GetProcGenProductUids(int DocTypeId, decimal Qty, int DivisionId, int SiteId)
        {
            string ProcName = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId).SqlProcGenProductUID;

            List<string> CalculationLineList = new List<String>();


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

            //IEnumerable<string> CalculationLineList = db.Database.SqlQuery<string>("SELECT * FROM " + ProcName + " ("+ SqlParameterDocType+"," +SqlParameterQty+") ").ToList();

            return CalculationLineList.ToList();

        }

        public IEnumerable<JobOrderLineViewModel> GetProdOrdersForFilters(JobOrderLineFilterViewModel vm)
        {
            byte? UnitConvForId = _jobOrderHeaderRepository.Find(vm.JobOrderHeaderId).UnitConversionForId;

            var joborder = _jobOrderHeaderRepository.Find(vm.JobOrderHeaderId);

            var Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(joborder.DocTypeId, joborder.DivisionId, joborder.SiteId);


            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(Settings.filterContraSites)) { ContraSites = Settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDivisions)) { ContraDivisions = Settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }


            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProdOrderHeaderId)) { SaleOrderIdArr = vm.ProdOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] Dimension1 = null;
            if (!string.IsNullOrEmpty(vm.Dimension1Id)) { Dimension1 = vm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dimension1 = new string[] { "NA" }; }

            string[] Dimension2 = null;
            if (!string.IsNullOrEmpty(vm.Dimension2Id)) { Dimension2 = vm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dimension2 = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            if (!string.IsNullOrEmpty(vm.DealUnitId))
            {
                Unit Dealunit = _unitOfWork.Repository<Unit>().Find(vm.DealUnitId);

                var temp = (from p in _unitOfWork.Repository<ViewProdOrderBalance>().Instance
                            join t in _unitOfWork.Repository<ProdOrderHeader>().Instance on p.ProdOrderHeaderId equals t.ProdOrderHeaderId into table
                            from tab in table.DefaultIfEmpty()
                            join product in _unitOfWork.Repository<Product>().Instance on p.ProductId equals product.ProductId into table2
                            join t1 in _unitOfWork.Repository<ProdOrderLine>().Instance on p.ProdOrderLineId equals t1.ProdOrderLineId into table1
                            from tab1 in table1.DefaultIfEmpty()
                            from tab2 in table2.DefaultIfEmpty()
                            join t3 in _unitOfWork.Repository<UnitConversion>().Instance on new { p1 = p.ProductId, DU1 = vm.DealUnitId, U1 = UnitConvForId ?? 0 } equals new { p1 = t3.ProductId ?? 0, DU1 = t3.ToUnitId, U1 = t3.UnitConversionForId } into table3
                            from tab3 in table3.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.ProdOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.ProdOrderHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1.Contains(p.Dimension1Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2.Contains(p.Dimension2Id.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                            && (string.IsNullOrEmpty(Settings.filterContraSites) ? p.SiteId == joborder.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                            && (string.IsNullOrEmpty(Settings.filterContraDivisions) ? p.DivisionId == joborder.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                            && p.BalanceQty > 0
                            orderby tab.DocDate, tab.DocNo, tab1.Sr
                            select new JobOrderLineViewModel
                            {
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Dimension1Id = p.Dimension1Id,
                                Dimension2Id = p.Dimension2Id,
                                Specification = tab1.Specification,
                                ProdOrderBalanceQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                Rate = vm.Rate,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                JobOrderHeaderId = vm.JobOrderHeaderId,
                                ProdOrderLineId = p.ProdOrderLineId,
                                UnitId = tab2.UnitId,
                                LossQty = Settings.LossQty,
                                NonCountedQty = Settings.NonCountedQty,
                                ProdOrderDocNo = p.ProdOrderNo,
                                //DealUnitId = (tab3 == null ? tab2.UnitId : vm.DealUnitId),
                                DealUnitId = (vm.DealUnitId),
                                UnitConversionMultiplier = (tab3 == null ? 1 : tab3.ToQty / tab3.FromQty),
                                UnitConversionException = tab3 == null ? true : false,
                                UnitDecimalPlaces = tab2.Unit.DecimalPlaces,
                                DealUnitDecimalPlaces = (tab3 == null ? tab2.Unit.DecimalPlaces : Dealunit.DecimalPlaces)
                            }

                        );
                return temp;
            }
            else
            {
                var temp = (from p in _unitOfWork.Repository<ViewProdOrderBalance>().Instance
                            join t in _unitOfWork.Repository<ProdOrderHeader>().Instance on p.ProdOrderHeaderId equals t.ProdOrderHeaderId into table
                            from tab in table.DefaultIfEmpty()
                            join product in _unitOfWork.Repository<Product>().Instance on p.ProductId equals product.ProductId into table2
                            join t1 in _unitOfWork.Repository<ProdOrderLine>().Instance on p.ProdOrderLineId equals t1.ProdOrderLineId into table1
                            from tab1 in table1.DefaultIfEmpty()
                            from tab2 in table2.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.ProdOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.ProdOrderHeaderId.ToString()))
                             && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1.Contains(p.Dimension1Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2.Contains(p.Dimension2Id.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                            && p.BalanceQty > 0
                            select new JobOrderLineViewModel
                            {
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension1Id = p.Dimension1Id,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Dimension2Id = p.Dimension2Id,
                                Specification = tab1.Specification,
                                ProdOrderBalanceQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                Rate = vm.Rate,
                                LossQty = Settings.LossQty,
                                NonCountedQty = Settings.NonCountedQty,
                                ProdOrderDocNo = tab.DocNo,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                JobOrderHeaderId = vm.JobOrderHeaderId,
                                ProdOrderLineId = p.ProdOrderLineId,
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


        public IQueryable<ComboBoxResult> GetPendingProdOrderHelpList(int Id, string term)
        {

            var JobOrderHeader = _jobOrderHeaderRepository.Find(Id);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrderHeader.DocTypeId, JobOrderHeader.DivisionId, JobOrderHeader.SiteId);

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

            var list = (from p in _unitOfWork.Repository<ViewProdOrderBalance>().Instance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProdOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                         && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.ProdOrderHeaderId into g
                        orderby g.Max(m => m.p.IndentDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.ProdOrderNo) + " | " + g.Max(m => m.p.DocType.DocumentTypeShortName),
                            id = g.Key.ToString(),
                        }
                          );

            return list;
        }


        public IEnumerable<ProdOrderHeaderListViewModel> GetPendingProdOrdersWithPatternMatch(int Id, string term, int Limiter)
        {
            var JobOrderHeader = _jobOrderHeaderRepository.Find(Id);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrderHeader.DocTypeId, JobOrderHeader.DivisionId, JobOrderHeader.SiteId);

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


            var list = (from p in _unitOfWork.Repository<ViewProdOrderBalance>().Instance
                        where (
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.ProdOrderNo.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower())
                        ) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                         && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        orderby p.ProdOrderNo
                        select new ProdOrderHeaderListViewModel
                        {
                            DocNo = p.ProdOrderNo,
                            ProdOrderLineId = p.ProdOrderLineId,
                            ProductName = p.Product.ProductName,
                            Dimension1Name = p.Dimension1.Dimension1Name,
                            Dimension2Name = p.Dimension2.Dimension2Name,

                        }
                        ).Take(Limiter);

            return (list);
        }

        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var JobOrder = _jobOrderHeaderRepository.Find(Id);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrder.DocTypeId, JobOrder.DivisionId, JobOrder.SiteId);

            string settingProductTypes = "";
            string settingProductDivision = "";


            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { settingProductTypes = "|" + settings.filterProductTypes.Replace(",", "|,|") + "|"; }
            if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { settingProductDivision = "|" + settings.FilterProductDivision.Replace(",", "|,|") + "|"; }

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settingProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ProductDivision = null;
            if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { ProductDivision = settingProductDivision.Split(",".ToCharArray()); }
            else { ProductDivision = new string[] { "NA" }; }

            var list = (from p in _unitOfWork.Repository<Product>().Instance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains("|" + p.ProductGroup.ProductTypeId.ToString() + "|"))
                        && (string.IsNullOrEmpty(settings.FilterProductDivision) ? 1 == 1 : ProductDivision.Contains("|" + p.DivisionId.ToString() + "|"))
                        group new { p } by p.ProductId into g
                        select new ComboBoxList
                        {
                            PropFirst = g.Max(m => m.p.ProductName),
                            Id = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in _jobOrderLineRepository.Instance
                       where p.JobOrderHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }

        public List<ComboBoxResult> GetBarCodesForWeavingWizard(int id, string term)
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Site = _unitOfWork.Repository<Site>().Find(SiteId);
            var JobOrder = Find(id);

            //return (from p in db.ViewRequisitionBalance
            //        where p.PersonId == id && (string.IsNullOrEmpty(term) ? 1 == 1 : p.CostCenter.CostCenterName.ToLower().Contains(term.ToLower()))
            //        && p.SiteId == SiteId && p.DivisionId == DivisionId
            //        group p by p.CostCenterId into g
            //        orderby g.Max(m => m.CostCenter.CostCenterName)
            //        select new ComboBoxResult
            //        {
            //            text = g.Max(m => m.CostCenter.CostCenterName),
            //            id = g.Key.Value.ToString(),
            //        });

            var temp = from p in _unitOfWork.Repository<ProductUid>().Instance
                       where p.ProductUidHeaderId == JobOrder.ProductUidHeaderId && p.ProductId == JobOrder.ProductId && (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductUidName.ToLower().Contains(term.ToLower()))
                       && p.CurrenctGodownId == Site.DefaultGodownId
                       orderby p.ProductUidName
                       select new ComboBoxResult
                       {
                           text = p.ProductUidName,
                           id = p.ProductUIDId.ToString(),
                       };

            return temp.ToList();
        }

        public IEnumerable<JobRate> GetJobRate(int JobOrderHeaderId, int ProductId)
        {
            SqlParameter SQLJobOrderHeaderId = new SqlParameter("@JobOrderHeaderId", JobOrderHeaderId);
            SqlParameter SQLProductID = new SqlParameter("@ProductId", ProductId);

            IEnumerable<JobRate> RateList = _unitOfWork.SqlQuery<JobRate>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetJobOrderRate @JobOrderHeaderId, @ProductId ", SQLJobOrderHeaderId, SQLProductID).ToList();

            return RateList;
        }


        public decimal GetUnitConversionForProdOrderLine(int ProdLineId, byte UnitConvForId, string DealUnitId)
        {
            var Query = (from t1 in _unitOfWork.Repository<ProdOrderLine>().Instance
                         join t2 in _unitOfWork.Repository<Product>().Instance on t1.ProductId equals t2.ProductId
                         join t3 in _unitOfWork.Repository<UnitConversion>().Instance on new { p1 = t1.ProductId, DU1 = DealUnitId, U1 = UnitConvForId } equals new { p1 = t3.ProductId ?? 0, DU1 = t3.ToUnitId, U1 = t3.UnitConversionForId } into table3
                         from tab3 in table3.DefaultIfEmpty()
                         select tab3).FirstOrDefault();

            if (Query != null)
            {
                return (Query.ToQty / Query.FromQty);
            }
            else
                return 0;
        }

        public void SaveMultiple(JobOrderMasterDetailModel vm, string UserName)
        {
            int Cnt = 0;
            int Serial = GetMaxSr(vm.JobOrderLineViewModel.FirstOrDefault().JobOrderHeaderId);
            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            List<JobOrderLine> BarCodesPendingToUpdate = new List<JobOrderLine>();

            int pk = 0;
            bool HeaderChargeEdit = false;

            JobOrderHeader Header = _jobOrderHeaderRepository.Find(vm.JobOrderLineViewModel.FirstOrDefault().JobOrderHeaderId);

            JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new JobOrderLineChargeService(_unitOfWork).GetMaxProductCharge(Header.JobOrderHeaderId, "Web.JobOrderLines", "JobOrderHeaderId", "JobOrderLineId");

            int PersonCount = 0;

            decimal Qty = vm.JobOrderLineViewModel.Where(m => m.Rate > 0).Sum(m => m.Qty);

            int CalculationId = Settings.CalculationId ?? 0;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            foreach (var item in vm.JobOrderLineViewModel)
            {
                //if (item.Qty > 0 &&  ((Settings.isMandatoryRate.HasValue && Settings.isMandatoryRate == true )? item.Rate > 0 : 1 == 1))
                if (item.Qty > 0 && item.UnitConversionMultiplier > 0 && ((Settings.isVisibleRate == true && Settings.isMandatoryRate == true && item.Rate > 0) || (Settings.isVisibleRate == false || Settings.isVisibleRate == true && Settings.isMandatoryRate == false)))
                {
                    JobOrderLine line = new JobOrderLine();

                    if (Settings.isPostedInStock ?? false)
                    {
                        StockViewModel StockViewModel = new StockViewModel();

                        if (Cnt == 0)
                        {
                            StockViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
                        }
                        else
                        {
                            if (Header.StockHeaderId != null && Header.StockHeaderId != 0)
                            {
                                StockViewModel.StockHeaderId = (int)Header.StockHeaderId;
                            }
                            else
                            {
                                StockViewModel.StockHeaderId = -1;
                            }
                        }

                        StockViewModel.StockId = -Cnt;
                        StockViewModel.DocHeaderId = Header.JobOrderHeaderId;
                        StockViewModel.DocLineId = line.JobOrderLineId;
                        StockViewModel.DocTypeId = Header.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Header.DocDate;
                        StockViewModel.StockDocDate = DateTime.Now.Date;
                        StockViewModel.DocNo = Header.DocNo;
                        StockViewModel.DivisionId = Header.DivisionId;
                        StockViewModel.SiteId = Header.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = Header.JobWorkerId;
                        StockViewModel.ProductId = item.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = Header.GodownId;
                        StockViewModel.HeaderProcessId = Header.ProcessId;
                        StockViewModel.GodownId = (int)Header.GodownId;
                        StockViewModel.Remark = Header.Remark;
                        StockViewModel.Status = Header.Status;
                        StockViewModel.ProcessId = Header.ProcessId;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = Header.CostCenterId;
                        StockViewModel.Qty_Iss = item.Qty;
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = item.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = item.Specification;
                        StockViewModel.Dimension1Id = item.Dimension1Id;
                        StockViewModel.Dimension2Id = item.Dimension2Id;
                        StockViewModel.ProductUidId = item.ProductUidId;
                        StockViewModel.CreatedBy = UserName;
                        StockViewModel.CreatedDate = DateTime.Now;
                        StockViewModel.ModifiedBy = UserName;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = _stockService.StockPostDB(ref StockViewModel);

                        if (Cnt == 0)
                        {
                            Header.StockHeaderId = StockViewModel.StockHeaderId;
                        }
                        line.StockId = StockViewModel.StockId;
                    }

                    if (Settings.isPostedInStockProcess ?? false)
                    {
                        StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                        if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                        {
                            StockProcessViewModel.StockHeaderId = (int)Header.StockHeaderId;
                        }
                        else if (Settings.isPostedInStock ?? false)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                        {
                            StockProcessViewModel.StockHeaderId = -1;
                        }
                        else if (Cnt > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                        {
                            StockProcessViewModel.StockHeaderId = -1;
                        }
                        else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                        {
                            StockProcessViewModel.StockHeaderId = 0;
                        }
                        StockProcessViewModel.StockProcessId = -Cnt;
                        StockProcessViewModel.DocHeaderId = Header.JobOrderHeaderId;
                        StockProcessViewModel.DocLineId = line.JobOrderLineId;
                        StockProcessViewModel.DocTypeId = Header.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                        StockProcessViewModel.StockProcessDocDate = DateTime.Now.Date;
                        StockProcessViewModel.DocNo = Header.DocNo;
                        StockProcessViewModel.DivisionId = Header.DivisionId;
                        StockProcessViewModel.SiteId = Header.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.PersonId = Header.JobWorkerId;
                        StockProcessViewModel.ProductId = item.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = Header.GodownId;
                        StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                        StockProcessViewModel.GodownId = (int)Header.GodownId;
                        StockProcessViewModel.Remark = Header.Remark;
                        StockProcessViewModel.Status = Header.Status;
                        StockProcessViewModel.ProcessId = Header.ProcessId;
                        StockProcessViewModel.LotNo = null;
                        StockProcessViewModel.CostCenterId = Header.CostCenterId;
                        StockProcessViewModel.Qty_Iss = 0;
                        StockProcessViewModel.Qty_Rec = item.Qty;
                        StockProcessViewModel.Rate = item.Rate;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = item.Specification;
                        StockProcessViewModel.Dimension1Id = item.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = item.Dimension2Id;
                        StockProcessViewModel.ProductUidId = item.ProductUidId;
                        StockProcessViewModel.CreatedBy = UserName;
                        StockProcessViewModel.CreatedDate = DateTime.Now;
                        StockProcessViewModel.ModifiedBy = UserName;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = _stockProcessService.StockProcessPostDB(ref StockProcessViewModel);

                        if ((Settings.isPostedInStock ?? false) == false)
                        {
                            if (Cnt == 0)
                            {
                                Header.StockHeaderId = StockProcessViewModel.StockHeaderId;
                            }
                        }

                        line.StockProcessId = StockProcessViewModel.StockProcessId;
                    }


                    line.JobOrderHeaderId = item.JobOrderHeaderId;
                    line.ProdOrderLineId = item.ProdOrderLineId;
                    line.ProductId = item.ProductId;
                    line.Dimension1Id = item.Dimension1Id;
                    line.Dimension2Id = item.Dimension2Id;
                    line.Specification = item.Specification;
                    line.Qty = item.Qty;
                    line.UnitId = item.UnitId;
                    line.Sr = Serial++;
                    line.LossQty = item.LossQty;
                    line.NonCountedQty = item.NonCountedQty;
                    line.Rate = item.Rate;
                    line.DealQty = item.UnitConversionMultiplier * item.Qty;
                    line.DealUnitId = item.DealUnitId;
                    line.Amount = DecimalRoundOff.amountToFixed((line.DealQty * line.Rate), Settings.AmountRoundOff);
                    line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                    line.CreatedDate = DateTime.Now;
                    line.ModifiedDate = DateTime.Now;
                    line.CreatedBy = UserName;
                    line.ModifiedBy = UserName;
                    line.JobOrderLineId = pk;
                    line.ObjectState = Model.ObjectState.Added;
                    
                    Create(line);

                    if (line.ProdOrderLineId.HasValue)
                        LineStatus.Add(line.ProdOrderLineId.Value, line.Qty);


                    new JobOrderLineStatusService(_unitOfWork).CreateLineStatus(line.JobOrderLineId);

                    BarCodesPendingToUpdate.Add(line);


                    #region BOMPost

                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    if (!string.IsNullOrEmpty(Settings.SqlProcConsumption))
                    {
                        var BomPostList = GetBomPostingDataForWeaving(line.ProductId, line.Dimension1Id, line.Dimension2Id, Header.ProcessId, line.Qty, Header.DocTypeId, Settings.SqlProcConsumption).ToList();

                        foreach (var Bomitem in BomPostList)
                        {
                            JobOrderBom BomPost = new JobOrderBom();
                            BomPost.CreatedBy = UserName;
                            BomPost.CreatedDate = DateTime.Now;
                            BomPost.Dimension1Id = Bomitem.Dimension1Id;
                            BomPost.Dimension2Id = Bomitem.Dimension2Id;
                            BomPost.JobOrderHeaderId = line.JobOrderHeaderId;
                            BomPost.JobOrderLineId = line.JobOrderLineId;
                            BomPost.ModifiedBy = UserName;
                            BomPost.ModifiedDate = DateTime.Now;
                            BomPost.ProductId = Bomitem.ProductId;
                            BomPost.Qty = Convert.ToDecimal(Bomitem.Qty);
                            BomPost.ObjectState = Model.ObjectState.Added;

                            _unitOfWork.Repository<JobOrderBom>().Add(BomPost);
                        }
                    }

                    #endregion

                    if (Settings.CalculationId.HasValue)
                    {
                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.JobOrderLineId, HeaderTableId = item.JobOrderHeaderId, PersonID = Header.JobWorkerId, DealQty = line.DealQty });
                    }
                    pk++;
                    Cnt = Cnt + 1;
                }

            }
            if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
            {
                Header.Status = (int)StatusConstants.Modified;
                Header.ModifiedBy = UserName;
                Header.ModifiedDate = DateTime.Now;
            }

            Header.ObjectState = Model.ObjectState.Modified;
            _jobOrderHeaderRepository.Update(Header);

            UpdateProdQtyJobOrderMultiple(LineStatus, Header.DocDate);

            if (Settings.CalculationId.HasValue)
            {
                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, vm.JobOrderLineViewModel.FirstOrDefault().JobOrderHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.JobOrderHeaderCharges", "Web.JobOrderLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                //Saving Charges
                foreach (var item in LineCharges)
                {
                    JobOrderLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, JobOrderLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    _unitOfWork.Repository<JobOrderLineCharge>().Add(PoLineCharge);
                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {
                    if (!HeaderChargeEdit)
                    {
                        JobOrderHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, JobOrderHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.JobOrderLineViewModel.FirstOrDefault().JobOrderHeaderId;
                        POHeaderCharge.PersonID = Header.JobWorkerId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        _unitOfWork.Repository<JobOrderHeaderCharge>().Add(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new JobOrderHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        footercharge.ObjectState = Model.ObjectState.Modified;
                        _unitOfWork.Repository<JobOrderHeaderCharge>().Update(footercharge);
                    }
                }
            }

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = Header.DocTypeId,
                DocId = Header.JobOrderHeaderId,
                ActivityType = (int)ActivityTypeContants.MultipleCreate,
                DocNo = Header.DocNo,
                DocDate = Header.DocDate,
                DocStatus = Header.Status,
            }));
        }


        public JobOrderLineViewModel Create(JobOrderLineViewModel svm, string UserName)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            JobOrderHeader temp = _jobOrderHeaderRepository.Find(svm.JobOrderHeaderId);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            JobOrderLine s = Mapper.Map<JobOrderLineViewModel, JobOrderLine>(svm);

            StockViewModel StockViewModel = new StockViewModel();
            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
            //Posting in Stock
            if (settings.isPostedInStock.HasValue && settings.isPostedInStock == true)
            {
                StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                StockViewModel.DocHeaderId = temp.JobOrderHeaderId;
                StockViewModel.DocLineId = s.JobOrderLineId;
                StockViewModel.DocTypeId = temp.DocTypeId;
                StockViewModel.StockHeaderDocDate = temp.DocDate;
                StockViewModel.StockDocDate = DateTime.Now.Date;
                StockViewModel.DocNo = temp.DocNo;
                StockViewModel.DivisionId = temp.DivisionId;
                StockViewModel.SiteId = temp.SiteId;
                StockViewModel.CurrencyId = null;
                StockViewModel.HeaderProcessId = null;
                StockViewModel.PersonId = temp.JobWorkerId;
                StockViewModel.ProductId = s.ProductId;
                StockViewModel.HeaderFromGodownId = null;
                StockViewModel.HeaderGodownId = null;
                StockViewModel.GodownId = temp.GodownId ?? 0;
                StockViewModel.ProcessId = s.FromProcessId;
                StockViewModel.LotNo = s.LotNo;
                StockViewModel.CostCenterId = temp.CostCenterId;
                StockViewModel.Qty_Iss = s.Qty;
                StockViewModel.Qty_Rec = 0;
                StockViewModel.Rate = s.Rate;
                StockViewModel.ExpiryDate = null;
                StockViewModel.Specification = s.Specification;
                StockViewModel.Dimension1Id = s.Dimension1Id;
                StockViewModel.Dimension2Id = s.Dimension2Id;
                StockViewModel.Remark = s.Remark;
                StockViewModel.ProductUidId = s.ProductUidId;
                StockViewModel.Status = temp.Status;
                StockViewModel.CreatedBy = temp.CreatedBy;
                StockViewModel.CreatedDate = DateTime.Now;
                StockViewModel.ModifiedBy = temp.ModifiedBy;
                StockViewModel.ModifiedDate = DateTime.Now;

                string StockPostingError = "";
                StockPostingError = _stockService.StockPostDB(ref StockViewModel);

                s.StockId = StockViewModel.StockId;

                if (temp.StockHeaderId == null)
                {
                    temp.StockHeaderId = StockViewModel.StockHeaderId;
                }
            }



            if (settings.isPostedInStockProcess.HasValue && settings.isPostedInStockProcess == true)
            {
                if (temp.StockHeaderId != null && temp.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                {
                    StockProcessViewModel.StockHeaderId = (int)temp.StockHeaderId;
                }
                else if (settings.isPostedInStock.HasValue && settings.isPostedInStock == true)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                {
                    StockProcessViewModel.StockHeaderId = -1;
                }
                else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                {
                    StockProcessViewModel.StockHeaderId = 0;
                }


                StockProcessViewModel.DocHeaderId = temp.JobOrderHeaderId;
                StockProcessViewModel.DocLineId = s.JobOrderLineId;
                StockProcessViewModel.DocTypeId = temp.DocTypeId;
                StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                StockProcessViewModel.StockProcessDocDate = DateTime.Now.Date;
                StockProcessViewModel.DocNo = temp.DocNo;
                StockProcessViewModel.DivisionId = temp.DivisionId;
                StockProcessViewModel.SiteId = temp.SiteId;
                StockProcessViewModel.CurrencyId = null;
                StockProcessViewModel.HeaderProcessId = null;
                StockProcessViewModel.PersonId = temp.JobWorkerId;
                StockProcessViewModel.ProductId = s.ProductId;
                StockProcessViewModel.HeaderFromGodownId = null;
                StockProcessViewModel.HeaderGodownId = null;
                StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                StockProcessViewModel.ProcessId = temp.ProcessId;
                StockProcessViewModel.LotNo = s.LotNo;
                StockProcessViewModel.CostCenterId = temp.CostCenterId;
                StockProcessViewModel.Qty_Iss = 0;
                StockProcessViewModel.Qty_Rec = s.Qty;
                StockProcessViewModel.Rate = s.Rate;
                StockProcessViewModel.ExpiryDate = null;
                StockProcessViewModel.Specification = s.Specification;
                StockProcessViewModel.Dimension1Id = s.Dimension1Id;
                StockProcessViewModel.Dimension2Id = s.Dimension2Id;
                StockProcessViewModel.Remark = s.Remark;
                StockProcessViewModel.Status = temp.Status;
                StockProcessViewModel.ProductUidId = s.ProductUidId;
                StockProcessViewModel.CreatedBy = temp.CreatedBy;
                StockProcessViewModel.CreatedDate = DateTime.Now;
                StockProcessViewModel.ModifiedBy = temp.ModifiedBy;
                StockProcessViewModel.ModifiedDate = DateTime.Now;

                string StockProcessPostingError = "";
                StockProcessPostingError = _stockProcessService.StockProcessPostDB(ref StockProcessViewModel);

                s.StockProcessId = StockProcessViewModel.StockProcessId;


                if (settings.isPostedInStock == false)
                {
                    if (temp.StockHeaderId == null)
                    {
                        temp.StockHeaderId = StockProcessViewModel.StockHeaderId;
                    }
                }
            }

            s.CreatedDate = DateTime.Now;
            s.ModifiedDate = DateTime.Now;
            s.CreatedBy = UserName;
            s.Sr = GetMaxSr(s.JobOrderHeaderId);
            s.ModifiedBy = UserName;
            s.ObjectState = Model.ObjectState.Added;

            if (s.ProductUidId.HasValue && s.ProductUidId > 0)
            {
                ProductUid Uid = (from p in _unitOfWork.Repository<ProductUid>().Instance
                                  where p.ProductUIDId == s.ProductUidId
                                  select p).FirstOrDefault();


                s.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                s.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                s.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                s.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                s.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                s.ProductUidStatus = Uid.Status;
                s.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                s.ProductUidCurrentGodownId = Uid.CurrenctGodownId;


                Uid.LastTransactionDocId = s.JobOrderHeaderId;
                Uid.LastTransactionDocDate = temp.DocDate;
                Uid.LastTransactionDocNo = temp.DocNo;
                Uid.LastTransactionDocTypeId = temp.DocTypeId;
                Uid.LastTransactionLineId = s.JobOrderLineId;
                Uid.LastTransactionPersonId = temp.JobWorkerId;
                Uid.Status = (!string.IsNullOrEmpty(settings.BarcodeStatusUpdate) ? settings.BarcodeStatusUpdate : ProductUidStatusConstants.Issue);
                Uid.CurrenctProcessId = temp.ProcessId;
                Uid.CurrenctGodownId = null;
                Uid.ModifiedBy = UserName;
                Uid.ModifiedDate = DateTime.Now;
                Uid.ObjectState = Model.ObjectState.Modified;

                _unitOfWork.Repository<ProductUid>().Update(Uid);
            }

            Create(s);

            new JobOrderLineStatusService(_unitOfWork).CreateLineStatus(s.JobOrderLineId);

            if (s.ProdOrderLineId.HasValue)
                UpdateProdQtyOnJobOrder(s.ProdOrderLineId.Value, s.JobOrderLineId, temp.DocDate, s.Qty);


            if (svm.linecharges != null)
                foreach (var item in svm.linecharges)
                {
                    item.LineTableId = s.JobOrderLineId;
                    item.PersonID = temp.JobWorkerId;
                    item.HeaderTableId = temp.JobOrderHeaderId;
                    item.ObjectState = Model.ObjectState.Added;
                    _unitOfWork.Repository<JobOrderLineCharge>().Add(item);
                }

            if (svm.footercharges != null)
                foreach (var item in svm.footercharges)
                {

                    if (item.Id > 0)
                    {

                        var footercharge = new JobOrderHeaderChargeService(_unitOfWork).Find(item.Id);
                        footercharge.Rate = item.Rate;
                        footercharge.Amount = item.Amount;
                        footercharge.ObjectState = Model.ObjectState.Modified;
                        _unitOfWork.Repository<JobOrderHeaderCharge>().Update(footercharge);
                    }

                    else
                    {
                        item.HeaderTableId = s.JobOrderHeaderId;
                        item.PersonID = temp.JobWorkerId;
                        item.ObjectState = Model.ObjectState.Added;
                        _unitOfWork.Repository<JobOrderHeaderCharge>().Add(item);
                    }
                }


            //JobOrderHeader header = new JobOrderHeaderService(_unitOfWork).Find(s.JobOrderHeaderId);
            if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
            {
                temp.Status = (int)StatusConstants.Modified;
                temp.ModifiedDate = DateTime.Now;
                temp.ModifiedBy = UserName;
            }

            temp.ObjectState = Model.ObjectState.Modified;
            _jobOrderHeaderRepository.Update(temp);

            #region BOMPost

            //Saving BOMPOST Data
            //Saving BOMPOST Data
            //Saving BOMPOST Data
            //Saving BOMPOST Data
            if (!string.IsNullOrEmpty(svm.JobOrderSettings.SqlProcConsumption))
            {
                var BomPostList = GetBomPostingDataForWeaving(s.ProductId, s.Dimension1Id, s.Dimension2Id, temp.ProcessId, s.Qty, temp.DocTypeId, svm.JobOrderSettings.SqlProcConsumption).ToList();

                foreach (var item in BomPostList)
                {
                    JobOrderBom BomPost = new JobOrderBom();
                    BomPost.CreatedBy = UserName;
                    BomPost.CreatedDate = DateTime.Now;
                    BomPost.Dimension1Id = item.Dimension1Id;
                    BomPost.Dimension2Id = item.Dimension2Id;
                    BomPost.JobOrderHeaderId = s.JobOrderHeaderId;
                    BomPost.JobOrderLineId = s.JobOrderLineId;
                    BomPost.ModifiedBy = UserName;
                    BomPost.ModifiedDate = DateTime.Now;
                    BomPost.ProductId = item.ProductId;
                    BomPost.Qty = Convert.ToDecimal(item.Qty);
                    BomPost.ObjectState = Model.ObjectState.Added;
                    _unitOfWork.Repository<JobOrderBom>().Add(BomPost);
                }
            }

            #endregion

            _unitOfWork.Save();

            svm.JobOrderLineId = s.JobOrderLineId;

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = temp.DocTypeId,
                DocId = temp.JobOrderHeaderId,
                DocLineId = s.JobOrderLineId,
                ActivityType = (int)ActivityTypeContants.Added,
                DocNo = temp.DocNo,
                DocDate = temp.DocDate,
                DocStatus = temp.Status,
            }));

            return svm;

        }

        public void Update(JobOrderLineViewModel svm, string UserName)
        {
            JobOrderLine s = Mapper.Map<JobOrderLineViewModel, JobOrderLine>(svm);

            JobOrderHeader temp = _jobOrderHeaderRepository.Find(svm.JobOrderHeaderId);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            JobOrderLine templine = Find(s.JobOrderLineId);

            JobOrderLine ExTempLine = new JobOrderLine();
            ExTempLine = Mapper.Map<JobOrderLine>(templine);

            if (templine.StockId != null)
            {
                StockViewModel StockViewModel = new StockViewModel();
                StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                StockViewModel.StockId = templine.StockId ?? 0;
                StockViewModel.DocHeaderId = templine.JobOrderHeaderId;
                StockViewModel.DocLineId = templine.JobOrderLineId;
                StockViewModel.DocTypeId = temp.DocTypeId;
                StockViewModel.StockHeaderDocDate = temp.DocDate;
                StockViewModel.StockDocDate = templine.CreatedDate.Date;
                StockViewModel.DocNo = temp.DocNo;
                StockViewModel.DivisionId = temp.DivisionId;
                StockViewModel.SiteId = temp.SiteId;
                StockViewModel.CurrencyId = null;
                StockViewModel.HeaderProcessId = temp.ProcessId;
                StockViewModel.PersonId = temp.JobWorkerId;
                StockViewModel.ProductId = s.ProductId;
                StockViewModel.HeaderFromGodownId = null;
                StockViewModel.HeaderGodownId = temp.GodownId;
                StockViewModel.GodownId = temp.GodownId ?? 0;
                StockViewModel.ProcessId = templine.FromProcessId;
                StockViewModel.LotNo = templine.LotNo;
                StockViewModel.CostCenterId = temp.CostCenterId;
                StockViewModel.Qty_Iss = s.Qty;
                StockViewModel.Qty_Rec = 0;
                StockViewModel.Rate = templine.Rate;
                StockViewModel.ExpiryDate = null;
                StockViewModel.Specification = templine.Specification;
                StockViewModel.Dimension1Id = templine.Dimension1Id;
                StockViewModel.Dimension2Id = templine.Dimension2Id;
                StockViewModel.Remark = s.Remark;
                StockViewModel.ProductUidId = s.ProductUidId;
                StockViewModel.Status = temp.Status;
                StockViewModel.CreatedBy = templine.CreatedBy;
                StockViewModel.CreatedDate = templine.CreatedDate;
                StockViewModel.ModifiedBy = UserName;
                StockViewModel.ModifiedDate = DateTime.Now;

                string StockPostingError = "";
                StockPostingError = _stockService.StockPostDB(ref StockViewModel);
            }


            if (templine.StockProcessId != null)
            {
                StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                StockProcessViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                StockProcessViewModel.StockProcessId = templine.StockProcessId ?? 0;
                StockProcessViewModel.DocHeaderId = templine.JobOrderHeaderId;
                StockProcessViewModel.DocLineId = templine.JobOrderLineId;
                StockProcessViewModel.DocTypeId = temp.DocTypeId;
                StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                StockProcessViewModel.StockProcessDocDate = templine.CreatedDate.Date;
                StockProcessViewModel.DocNo = temp.DocNo;
                StockProcessViewModel.DivisionId = temp.DivisionId;
                StockProcessViewModel.SiteId = temp.SiteId;
                StockProcessViewModel.CurrencyId = null;
                StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                StockProcessViewModel.PersonId = temp.JobWorkerId;
                StockProcessViewModel.ProductId = s.ProductId;
                StockProcessViewModel.HeaderFromGodownId = null;
                StockProcessViewModel.HeaderGodownId = temp.GodownId;
                StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                StockProcessViewModel.ProcessId = temp.ProcessId;
                StockProcessViewModel.LotNo = templine.LotNo;
                StockProcessViewModel.CostCenterId = temp.CostCenterId;
                StockProcessViewModel.Qty_Iss = 0;
                StockProcessViewModel.Qty_Rec = s.Qty;
                StockProcessViewModel.Rate = templine.Rate;
                StockProcessViewModel.ExpiryDate = null;
                StockProcessViewModel.Specification = templine.Specification;
                StockProcessViewModel.Dimension1Id = templine.Dimension1Id;
                StockProcessViewModel.Dimension2Id = templine.Dimension2Id;
                StockProcessViewModel.Remark = s.Remark;
                StockProcessViewModel.ProductUidId = s.ProductUidId;
                StockProcessViewModel.Status = temp.Status;
                StockProcessViewModel.CreatedBy = templine.CreatedBy;
                StockProcessViewModel.CreatedDate = templine.CreatedDate;
                StockProcessViewModel.ModifiedBy = UserName;
                StockProcessViewModel.ModifiedDate = DateTime.Now;

                string StockProcessPostingError = "";
                StockProcessPostingError = _stockProcessService.StockProcessPostDB(ref StockProcessViewModel);

            }


            if (!string.IsNullOrEmpty(settings.SqlProcGenProductUID))
            {

                if (templine.ProductUidHeaderId != null)
                {
                    ProductUidHeader ProdUidHeader = (from p in _unitOfWork.Repository<ProductUidHeader>().Instance
                                                      where p.ProductUidHeaderId == templine.ProductUidHeaderId
                                                      select p).FirstOrDefault();

                    ProdUidHeader.ProductId = s.ProductId;
                    ProdUidHeader.Dimension1Id = s.Dimension1Id;
                    ProdUidHeader.Dimension2Id = s.Dimension2Id;
                    ProdUidHeader.GenDocId = s.JobOrderHeaderId;
                    ProdUidHeader.GenDocNo = temp.DocNo;
                    ProdUidHeader.GenDocTypeId = temp.DocTypeId;
                    ProdUidHeader.GenDocDate = temp.DocDate;
                    ProdUidHeader.GenPersonId = temp.JobWorkerId;
                    ProdUidHeader.ModifiedBy = UserName;
                    ProdUidHeader.ModifiedDate = DateTime.Now;
                    ProdUidHeader.ObjectState = Model.ObjectState.Modified;
                    _unitOfWork.Repository<ProductUidHeader>().Update(ProdUidHeader);

                    if (svm.Qty > templine.Qty)
                    {
                        List<string> uids = GetProcGenProductUids(temp.DocTypeId, svm.Qty - templine.Qty, temp.DivisionId, temp.SiteId);
                        int count = 0;
                        foreach (string item in uids)
                        {
                            ProductUid ProdUid = new ProductUid();

                            ProdUid.ProductUidName = item;
                            ProdUid.ProductId = s.ProductId;
                            ProdUid.IsActive = true;
                            ProdUid.CreatedBy = UserName;
                            ProdUid.CreatedDate = DateTime.Now;
                            ProdUid.ModifiedBy = UserName;
                            ProdUid.ModifiedDate = DateTime.Now;
                            ProdUid.GenLineId = s.JobOrderLineId;
                            ProdUid.GenDocId = s.JobOrderHeaderId;
                            ProdUid.GenDocNo = temp.DocNo;
                            ProdUid.GenDocTypeId = temp.DocTypeId;
                            ProdUid.GenDocDate = temp.DocDate;
                            ProdUid.GenPersonId = temp.JobWorkerId;
                            ProdUid.Dimension1Id = s.Dimension1Id;
                            ProdUid.Dimension2Id = s.Dimension2Id;
                            ProdUid.CurrenctProcessId = null;
                            ProdUid.Status = (!string.IsNullOrEmpty(settings.BarcodeStatusUpdate) ? settings.BarcodeStatusUpdate : ProductUidStatusConstants.Issue);
                            ProdUid.LastTransactionDocId = s.JobOrderHeaderId;
                            ProdUid.LastTransactionDocNo = temp.DocNo;
                            ProdUid.LastTransactionDocTypeId = temp.DocTypeId;
                            ProdUid.LastTransactionDocDate = temp.DocDate;
                            ProdUid.LastTransactionPersonId = temp.JobWorkerId;
                            ProdUid.LastTransactionLineId = s.JobOrderLineId;
                            ProdUid.ProductUIDId = count;
                            ProdUid.ObjectState = Model.ObjectState.Added;
                            _unitOfWork.Repository<ProductUid>().Add(ProdUid);

                            count++;
                        }
                    }
                    else if (svm.Qty < templine.Qty)
                    {
                        var ProductUidToDelete = (from L in _unitOfWork.Repository<ProductUid>().Instance
                                                  where L.ProductUidHeaderId == ProdUidHeader.ProductUidHeaderId
                                                  select L).Take((int)(templine.Qty - svm.Qty));

                        foreach (var item in ProductUidToDelete)
                        {
                            item.ObjectState = Model.ObjectState.Deleted;
                            _unitOfWork.Repository<ProductUid>().Delete(item);
                        }
                    }
                }
            }



            templine.DueDate = s.DueDate;
            templine.ProductId = s.ProductId;
            templine.ProductUidId = s.ProductUidId;
            templine.ProdOrderLineId = s.ProdOrderLineId;
            templine.DueDate = s.DueDate;
            templine.LotNo = s.LotNo;
            templine.FromProcessId = s.FromProcessId;
            templine.UnitId = s.UnitId;
            templine.DealUnitId = s.DealUnitId;
            templine.DealQty = s.DealQty;
            templine.NonCountedQty = s.NonCountedQty;
            templine.LossQty = s.LossQty;
            templine.Rate = s.Rate;
            templine.Amount = s.Amount;
            templine.Remark = s.Remark;
            templine.Qty = s.Qty;
            templine.Remark = s.Remark;
            templine.Dimension1Id = s.Dimension1Id;
            templine.Dimension2Id = s.Dimension2Id;
            templine.UnitConversionMultiplier = s.UnitConversionMultiplier;
            templine.Specification = s.Specification;

            templine.ModifiedDate = DateTime.Now;
            templine.ModifiedBy = UserName;
            templine.ObjectState = Model.ObjectState.Modified;
            Update(templine);

            int Status = 0;
            if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
            {
                Status = temp.Status;
                temp.Status = (int)StatusConstants.Modified;
                temp.ModifiedBy = UserName;
                temp.ModifiedDate = DateTime.Now;
            }

            temp.ObjectState = Model.ObjectState.Modified;
            _jobOrderHeaderRepository.Update(temp);

            if (templine.ProdOrderLineId.HasValue)
                UpdateProdQtyOnJobOrder(templine.ProdOrderLineId.Value, templine.JobOrderLineId, temp.DocDate, templine.Qty);

            var Boms = (from p in _unitOfWork.Repository<JobOrderBom>().Instance
                        where p.JobOrderLineId == templine.JobOrderLineId
                        select p).ToList();

            foreach (var item in Boms)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobOrderBom>().Delete(item);
            }


            #region BOMPost

            //Saving BOMPOST Data
            //Saving BOMPOST Data
            //Saving BOMPOST Data
            //Saving BOMPOST Data
            if (!string.IsNullOrEmpty(svm.JobOrderSettings.SqlProcConsumption))
            {
                var BomPostList = GetBomPostingDataForWeaving(s.ProductId, s.Dimension1Id, s.Dimension2Id, temp.ProcessId, s.Qty, temp.DocTypeId, svm.JobOrderSettings.SqlProcConsumption).ToList();

                foreach (var item in BomPostList)
                {
                    JobOrderBom BomPost = new JobOrderBom();
                    BomPost.CreatedBy = UserName;
                    BomPost.CreatedDate = DateTime.Now;
                    BomPost.Dimension1Id = item.Dimension1Id;
                    BomPost.Dimension2Id = item.Dimension2Id;
                    BomPost.JobOrderHeaderId = s.JobOrderHeaderId;
                    BomPost.JobOrderLineId = s.JobOrderLineId;
                    BomPost.ModifiedBy = UserName;
                    BomPost.ModifiedDate = DateTime.Now;
                    BomPost.ProductId = item.ProductId;
                    BomPost.Qty = Convert.ToDecimal(item.Qty);
                    BomPost.ObjectState = Model.ObjectState.Added;
                    _unitOfWork.Repository<JobOrderBom>().Add(BomPost);
                }
            }

            #endregion



            LogList.Add(new LogTypeViewModel
            {
                ExObj = ExTempLine,
                Obj = templine
            });

            if (svm.linecharges != null)
            {
                var ProductChargeList = (from p in _unitOfWork.Repository<JobOrderLineCharge>().Instance
                                         where p.LineTableId == templine.JobOrderLineId
                                         select p).ToList();

                foreach (var item in svm.linecharges)
                {
                    var productcharge = (ProductChargeList.Where(m => m.Id == item.Id)).FirstOrDefault();

                    var ExProdcharge = Mapper.Map<JobOrderLineCharge>(productcharge);
                    productcharge.Rate = item.Rate;
                    productcharge.Amount = item.Amount;
                    productcharge.DealQty = item.DealQty;
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExProdcharge,
                        Obj = productcharge
                    });
                    productcharge.ObjectState = Model.ObjectState.Modified;
                    _unitOfWork.Repository<JobOrderLineCharge>().Update(productcharge);
                }
            }

            if (svm.footercharges != null)
            {
                var footerChargerecords = (from p in _unitOfWork.Repository<JobOrderHeaderCharge>().Instance
                                           where p.HeaderTableId == temp.JobOrderHeaderId
                                           select p).ToList();

                foreach (var item in svm.footercharges)
                {
                    var footercharge = footerChargerecords.Where(m => m.Id == item.Id).FirstOrDefault();
                    var Exfootercharge = Mapper.Map<JobOrderHeaderCharge>(footercharge);
                    footercharge.Rate = item.Rate;
                    footercharge.Amount = item.Amount;
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Exfootercharge,
                        Obj = footercharge,
                    });
                    footercharge.ObjectState = Model.ObjectState.Modified;
                    _unitOfWork.Repository<JobOrderHeaderCharge>().Update(footercharge);
                }
            }

            XElement Modifications = _modificationCheck.CheckChanges(LogList);

            _unitOfWork.Save();

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = temp.DocTypeId,
                DocId = templine.JobOrderHeaderId,
                DocLineId = templine.JobOrderLineId,
                ActivityType = (int)ActivityTypeContants.Modified,
                DocNo = temp.DocNo,
                xEModifications = Modifications,
                DocDate = temp.DocDate,
                DocStatus = temp.Status,
            }));
        }

        public void Delete(JobOrderLineViewModel vm, string UserName)
        {

            int? StockId = 0;
            int? StockProcessId = 0;
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            JobOrderLine JobOrderLine = Find(vm.JobOrderLineId);
            JobOrderHeader header = _jobOrderHeaderRepository.Find(JobOrderLine.JobOrderHeaderId);

            JobOrderLineStatus LineStatus = (from p in _unitOfWork.Repository<JobOrderLineStatus>().Instance
                                             where p.JobOrderLineId == JobOrderLine.JobOrderLineId
                                             select p).FirstOrDefault();

            StockId = JobOrderLine.StockId;
            StockProcessId = JobOrderLine.StockProcessId;

            LogList.Add(new LogTypeViewModel
            {
                Obj = Mapper.Map<JobOrderLine>(JobOrderLine),
            });

            LineStatus.ObjectState = Model.ObjectState.Deleted;
            _unitOfWork.Repository<JobOrderLineStatus>().Delete(LineStatus);

            if (JobOrderLine.ProdOrderLineId.HasValue)
                UpdateProdQtyOnJobOrder(JobOrderLine.ProdOrderLineId.Value, JobOrderLine.JobOrderLineId, header.DocDate, 0);


            if ((JobOrderLine.ProductUidHeaderId.HasValue) && (JobOrderLine.ProductUidHeaderId.Value > 0))
            {

                var ProductUid = (from p in _unitOfWork.Repository<ProductUid>().Instance
                                  where p.ProductUidHeaderId == JobOrderLine.ProductUidHeaderId
                                  select p).ToList();

                foreach (var item in ProductUid)
                {
                    if (item.LastTransactionDocId == item.GenDocId && item.LastTransactionDocTypeId == item.GenDocTypeId)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        _unitOfWork.Repository<ProductUid>().Delete(item);
                    }
                    else
                    {
                        throw new Exception("Record Cannot be deleted as it is in use by other documents");
                    }
                }
            }
            else
            {

                if (JobOrderLine.ProductUidId.HasValue)
                {
                    ProductUid ProductUid = (from p in _unitOfWork.Repository<ProductUid>().Instance
                                             where p.ProductUIDId == JobOrderLine.ProductUidId
                                             select p).FirstOrDefault();

                    if (header.DocNo != ProductUid.LastTransactionDocNo || header.DocTypeId != ProductUid.LastTransactionDocTypeId)
                    {

                        throw new Exception("Bar Code Can't be deleted because this is already Proceed to another process.");
                        //ModelState.AddModelError("", "");
                        //PrepareViewBag(vm);
                        //ViewBag.LineMode = "Delete";
                        //return PartialView("_Create", vm);
                    }

                    ProductUid.LastTransactionDocDate = JobOrderLine.ProductUidLastTransactionDocDate;
                    ProductUid.LastTransactionDocId = JobOrderLine.ProductUidLastTransactionDocId;
                    ProductUid.LastTransactionDocNo = JobOrderLine.ProductUidLastTransactionDocNo;
                    ProductUid.LastTransactionDocTypeId = JobOrderLine.ProductUidLastTransactionDocTypeId;
                    ProductUid.LastTransactionPersonId = JobOrderLine.ProductUidLastTransactionPersonId;
                    ProductUid.CurrenctGodownId = JobOrderLine.ProductUidCurrentGodownId;
                    ProductUid.CurrenctProcessId = JobOrderLine.ProductUidCurrentProcessId;
                    ProductUid.Status = JobOrderLine.ProductUidStatus;
                    if (ProductUid.ProcessesDone != null)
                    {
                        ProductUid.ProcessesDone = ProductUid.ProcessesDone.Replace("|" + new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId.ToString() + "|", "");
                    }

                    ProductUid.ObjectState = Model.ObjectState.Modified;
                    _unitOfWork.Repository<ProductUid>().Update(ProductUid);
                }
            }



            //_JobOrderLineService.Delete(JobOrderLine);
            JobOrderLine.ObjectState = Model.ObjectState.Deleted;
            Delete(JobOrderLine);

            if (StockId != null)
            {
                _stockService.DeleteStock((int)StockId);
            }

            if (StockProcessId != null)
            {
                _stockProcessService.DeleteStockProcessDB((int)StockProcessId);
            }


            if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
            {
                header.Status = (int)StatusConstants.Modified;
                header.ModifiedDate = DateTime.Now;
                header.ModifiedBy = UserName;
                _jobOrderHeaderRepository.Update(header);
            }

            var chargeslist = (from p in _unitOfWork.Repository<JobOrderLineCharge>().Instance
                               where p.LineTableId == vm.JobOrderLineId
                               select p).ToList();

            if (chargeslist != null)
                foreach (var item in chargeslist)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    _unitOfWork.Repository<JobOrderLineCharge>().Delete(item);
                }

            if (vm.footercharges != null)
                foreach (var item in vm.footercharges)
                {
                    var footer = (from p in _unitOfWork.Repository<JobOrderHeaderCharge>().Instance
                                  where p.Id == item.Id
                                  select p).FirstOrDefault();

                    footer.Rate = item.Rate;
                    footer.Amount = item.Amount;
                    footer.ObjectState = Model.ObjectState.Modified;
                    _unitOfWork.Repository<JobOrderHeaderCharge>().Update(footer);
                }


            var Boms = (from p in _unitOfWork.Repository<JobOrderBom>().Instance
                        where p.JobOrderLineId == vm.JobOrderLineId
                        select p).ToList();

            foreach (var item in Boms)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                _unitOfWork.Repository<JobOrderBom>().Delete(item);
            }

            XElement Modifications = _modificationCheck.CheckChanges(LogList);

            _unitOfWork.Save();

            //Saving the Activity Log

            _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = header.DocTypeId,
                DocId = header.JobOrderHeaderId,
                DocLineId = JobOrderLine.JobOrderLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = header.DocNo,
                xEModifications = Modifications,
                DocDate = header.DocDate,
                DocStatus = header.Status,
            }));

        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        void UpdateProdQtyJobOrderMultiple(Dictionary<int, decimal> Qty, DateTime DocDate)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();

            var LineAndQty = (from p in _jobOrderLineRepository.Instance
                              where (IsdA).Contains(p.ProdOrderLineId.Value)
                              group p by p.ProdOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty),
                              }).ToList();

            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            var LineStatus = (from p in _unitOfWork.Repository<ProdOrderLineStatus>().Instance
                              where IsdA.Contains(p.ProdOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.JobOrderQty = Qty[item.ProdOrderLineId.Value] + (LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault().Qty);
                item.JobOrderDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                _unitOfWork.Repository<ProdOrderLineStatus>().Update(item);
            }
        }

        public void UpdateProdQtyOnJobOrder(int id, int JobOrderLineId, DateTime DocDate, decimal Qty)
        {

            var temp = (from p in _jobOrderLineRepository.Instance
                        where p.ProdOrderLineId == id && p.JobOrderLineId != JobOrderLineId
                        join t in _jobOrderHeaderRepository.Instance on p.JobOrderHeaderId equals t.JobOrderHeaderId
                        select new
                        {
                            Qty = p.Qty,
                            Date = t.DocDate,
                        }).ToList();
            decimal qty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                date = temp.Max(m => m.Date);
            }

            ProdOrderLineStatus Stat = (from p in _unitOfWork.Repository<ProdOrderLineStatus>().Instance
                                        where p.ProdOrderLineId == id
                                        select p).FirstOrDefault();

            Stat.JobOrderQty = Qty;
            Stat.JobOrderDate = date;

            Stat.ObjectState = Model.ObjectState.Modified;
            _unitOfWork.Repository<ProdOrderLineStatus>().Update(Stat);

        }

        public IEnumerable<JobOrderLine> GetJobOrderLineListForHeader(int HeaderId)
        {
            return (from p in _jobOrderLineRepository.Instance
                    where p.JobOrderHeaderId == HeaderId
                    select p);
        }


        #region Helper Methods

        public IEnumerable<Unit> GetUnitList()
        {
            return new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        public ProductViewModel GetProduct(int ProductId)
        {
            return new ProductService(_unitOfWork).GetProduct(ProductId);
        }

        public Unit FindUnit(string UnitId)
        {
            return new UnitService(_unitOfWork).Find(UnitId);
        }

        public UnitConversion GetUnitConversion(int Id, string UnitId, int conversionForId, string DealUnitId)
        {
            return new UnitConversionService(_unitOfWork).GetUnitConversion(Id, UnitId, conversionForId, DealUnitId);
        }

        public ProductUid FindProductUid(int Id)
        {
            return new ProductUidService(_unitOfWork).Find(Id);
        }

        public ProductUid FindProductUid(string Name)
        {
            return new ProductUidService(_unitOfWork).Find(Name);
        }

        public bool CheckProductUidProcessDone(string ProductUidName, int ProcessID)
        {
            return new ProductUidService(_unitOfWork).IsProcessDone(ProductUidName, ProcessID);
        }

        public IEnumerable<ProdOrderHeaderListViewModel> GetPendingProdOrders(int ProductId)
        {
            return (from p in _unitOfWork.Repository<ViewProdOrderBalance>().Instance
                    join t in _unitOfWork.Repository<ProdOrderHeader>().Instance on p.ProdOrderHeaderId equals t.ProdOrderHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in _unitOfWork.Repository<ProdOrderLine>().Instance on p.ProdOrderLineId equals t1.ProdOrderLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.ProductId == ProductId && p.BalanceQty > 0
                    select new ProdOrderHeaderListViewModel
                    {
                        ProdOrderLineId = p.ProdOrderLineId,
                        ProdOrderHeaderId = p.ProdOrderHeaderId,
                        DocNo = tab.DocNo,
                        Dimension1Name = tab1.Dimension1.Dimension1Name,
                        Dimension2Name = tab1.Dimension2.Dimension2Name,
                    }
                        );
        }

        public ProdOrderLineViewModel GetProdOrderDetailBalance(int id)
        {
            return (from b in _unitOfWork.Repository<ViewProdOrderBalance>().Instance
                    join p in _unitOfWork.Repository<ProdOrderLine>().Instance on b.ProdOrderLineId equals p.ProdOrderLineId
                    where b.ProdOrderLineId == id
                    select new ProdOrderLineViewModel
                    {
                        Qty = b.BalanceQty,
                        Specification = p.Specification,
                        Remark = p.Remark,
                        ProductId = p.ProductId,
                        UnitId = p.Product.UnitId,
                        UnitName = p.Product.Unit.UnitName,
                        ProductName = p.Product.ProductName,
                        Dimension1Id = p.Dimension1Id,
                        Dimension1Name = p.Dimension1.Dimension1Name,
                        Dimension2Id = p.Dimension2Id,
                        Dimension2Name = p.Dimension2.Dimension2Name,
                        unitDecimalPlaces = p.Product.Unit.DecimalPlaces,
                    }).FirstOrDefault();
        }

        public ProdOrderLineViewModel GetProdOrderForProdUid(int id)
        {

            return (from p in _unitOfWork.Repository<ProductUid>().Instance
                    where p.ProductUIDId == id
                    join t in _jobOrderLineRepository.Instance on p.ProductUidHeaderId equals t.ProductUidHeaderId
                    join t2 in _unitOfWork.Repository<ProdOrderLine>().Instance on t.JobOrderLineId equals t2.ReferenceDocLineId
                    select new ProdOrderLineViewModel
                    {
                        ProdOrderLineId = t2.ProdOrderLineId,
                        ProdOrderDocNo = t2.ProdOrderHeader.DocNo
                    }
                       ).FirstOrDefault();
        }

        #endregion



    }



    public class JobRate
    {
        public int ProductId { get; set; }
        public Decimal? Rate { get; set; }
        public Decimal? IncentiveRate { get; set; }
        public Decimal? DiscountRate { get; set; }
        public Decimal? LossRate { get; set; }
    }
}