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
using System.Data.Entity.SqlServer;

namespace Service
{
    public interface IJobReceiveLineService : IDisposable
    {
        JobReceiveLine Create(JobReceiveLine pt);
        void Delete(int id);
        void Delete(JobReceiveLine pt);
        JobReceiveLine Find(int id);
        IEnumerable<JobReceiveLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobReceiveLine pt);
        JobReceiveLine Add(JobReceiveLine pt);
        IEnumerable<JobReceiveLine> GetJobReceiveLineList();
        IEnumerable<JobReceiveLine> GetJobReceiveLineList(int JobReceiveHeaderId);
        IEnumerable<JobReceiveLineViewModel> GetLineListForIndex(int headerId);//HeaderId
        Task<IEquatable<JobReceiveLine>> GetAsync();
        Task<JobReceiveLine> FindAsync(int id);
        JobReceiveLineViewModel GetJobReceiveLine(int id);
        IEnumerable<JobReceiveLineViewModel> GetJobOrdersForFilters(JobReceiveLineFilterViewModel vm);
        ComboBoxPagedResult GetPendingProductsForJobReceive(string searchTerm, int pageSize, int pageNum, int filter);
        ComboBoxPagedResult GetPendingProductGroupsForJobReceive(string searchTerm, int pageSize, int pageNum, int filter);
        ComboBoxPagedResult GetPendingJobOrders(string searchTerm, int pageSize, int pageNum, int filter);//JobReceive HeaderId,DocTypes,Search term
        //IEnumerable<ProcGetBomForWeavingViewModel> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int ProcessId, decimal Qty, int DocTypeId, string ProcName, int? JobOrderLineId);

        IEnumerable<ProcGetBomForWeavingViewModel> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int ProcessId, decimal Qty, int DocTypeId, string ProcName, int? JobOrderLineId, Decimal? Weight);
        IQueryable<JobReceiveBomViewModel> GetConsumptionLineListForIndex(int JobOrderHeaderId);
        IQueryable<JobReceiveByProductViewModel> GetByProductListForIndex(int JobOrderHeaderId);
        string GetFirstBarCodeForCancel(int JobOrderLineId);
        List<ComboBoxList> GetPendingBarCodesList(int id);
        List<ComboBoxList> GetPendingBarCodesList(int[] id);
        int GetMaxSr(int id);
        IQueryable<ComboBoxResult> GetPendingCostCenters(int id, string term);
        IEnumerable<ComboBoxList> GetConsumptionProducts(string term, int filter);
        decimal GetConsumptionBalanceQty(int filter, int ProductId);
        string GetReasons(int filter);
        List<ComboBoxResult> SetReason(string Ids);
        IEnumerable<JobReceiveLine> ProductUidsExist(int JobReceiveHeaderId, int? ProductUid);
        IEnumerable<ComboBoxResult> FGetProductUidHelpList(int Id, string term);
        IEnumerable<ComboBoxResult> GetJobOrderHelpListForProduct(int Id, string term);
    }

    public class JobReceiveLineService : IJobReceiveLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobReceiveLine> _JobReceiveLineRepository;
        RepositoryQuery<JobReceiveLine> JobReceiveLineRepository;
        public JobReceiveLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobReceiveLineRepository = new Repository<JobReceiveLine>(db);
            JobReceiveLineRepository = new RepositoryQuery<JobReceiveLine>(_JobReceiveLineRepository);
        }

        public JobReceiveLine Find(int id)
        {
            return _unitOfWork.Repository<JobReceiveLine>().Find(id);
        }

        public JobReceiveLine Create(JobReceiveLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobReceiveLine>().Insert(pt);
            return pt;
        }

        public IEnumerable<JobReceiveLineViewModel> GetJobOrdersForFilters(JobReceiveLineFilterViewModel vm)
        {

            var JobReceive = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(JobReceive.DocTypeId, JobReceive.DivisionId, JobReceive.SiteId);

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.JobOrderHeaderId)) { SaleOrderIdArr = vm.JobOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] CostCenterIdArr = null;
            if (!string.IsNullOrEmpty(vm.CostCenterId)) { CostCenterIdArr = vm.CostCenterId.Split(",".ToCharArray()); }
            else { CostCenterIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

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

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            string[] ContraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { ContraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { ContraDocTypes = new string[] { "NA" }; }

            //var TempProductSite = (from Vl in db.ViewJobOrderBalance
            //                       join L in db.JobOrderLine on Vl.JobOrderLineId equals L.JobOrderLineId into JobOrderLineTable
            //                       from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
            //                       join H in db.JobOrderHeader on JobOrderLineTab.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
            //                       from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
            //                       join Ps in db.ProductSiteDetail on new { A1 = JobOrderHeaderTab.SiteId, A2 = JobOrderHeaderTab.DivisionId, A3 = JobOrderHeaderTab.ProcessId, A4 = JobOrderLineTab.ProductId }
            //                                 equals new { A1 = Ps.SiteId, A2 = Ps.DivisionId, A3 = (Ps.ProcessId ?? 0), A4 = Ps.ProductId } into ProductSiteTable
            //                       from ProductSiteTab in ProductSiteTable.DefaultIfEmpty()
            //                       select new
            //                       {
            //                           JobOrderLineId = Vl.JobOrderLineId,
            //                           ExcessAllowedQty = JobOrderLineTab.Qty * (ProductSiteTab.ExcessReceiveAllowedAgainstOrderPer ?? 0) / 100 > (ProductSiteTab.ExcessReceiveAllowedAgainstOrderQty ?? 0) ? JobOrderLineTab.Qty * (ProductSiteTab.ExcessReceiveAllowedAgainstOrderPer ?? 0) / 100 : (ProductSiteTab.ExcessReceiveAllowedAgainstOrderQty ?? 0)
            //                       }).ToList();

            var temp = (from Vl in db.ViewJobOrderBalance
                        join H in db.JobOrderHeader on Vl.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
                        from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                        join P in db.Product on Vl.ProductId equals P.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join L in db.JobOrderLine on Vl.JobOrderLineId equals L.JobOrderLineId into JobOrderLineTable
                        from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                        join Ps in db.ProductSiteDetail on new { A1 = JobOrderHeaderTab.SiteId, A2 = JobOrderHeaderTab.DivisionId, A3 = JobOrderHeaderTab.ProcessId, A4 = JobOrderLineTab.ProductId }
                            equals new { A1 = Ps.SiteId, A2 = Ps.DivisionId, A3 = (Ps.ProcessId ?? 0), A4 = Ps.ProductId } into ProductSiteTable
                            from ProductSiteTab in ProductSiteTable.DefaultIfEmpty()
                        where JobOrderHeaderTab.ProcessId == JobReceive.ProcessId
                        && (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(Vl.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.JobOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(Vl.JobOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.CostCenterId) ? 1 == 1 : CostCenterIdArr.Contains(JobOrderHeaderTab.CostCenterId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(ProductTab.ProductGroupId.ToString()))
                        && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1.Contains(Vl.Dimension1Id.ToString()))
                        && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2.Contains(Vl.Dimension2Id.ToString()))
                        && (string.IsNullOrEmpty(vm.Dimension3Id) ? 1 == 1 : Dimension3.Contains(Vl.Dimension3Id.ToString()))
                        && (string.IsNullOrEmpty(vm.Dimension4Id) ? 1 == 1 : Dimension4.Contains(Vl.Dimension4Id.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : ContraDocTypes.Contains(JobOrderHeaderTab.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? Vl.SiteId == JobReceive.SiteId : ContraSites.Contains(Vl.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? Vl.DivisionId == JobReceive.DivisionId : ContraDivisions.Contains(Vl.DivisionId.ToString()))
                        && Vl.BalanceQty > 0 && Vl.JobWorkerId == vm.JobWorkerId && JobOrderHeaderTab.DocDate <= JobReceive.DocDate
                        orderby JobOrderHeaderTab.DocDate, JobOrderHeaderTab.DocNo, JobOrderLineTab.Sr
                        select new JobReceiveLineViewModel
                        {
                            Dimension1Name = JobOrderLineTab.Dimension1.Dimension1Name,
                            Dimension2Name = JobOrderLineTab.Dimension2.Dimension2Name,
                            Dimension3Name = JobOrderLineTab.Dimension3.Dimension3Name,
                            Dimension4Name = JobOrderLineTab.Dimension4.Dimension4Name,
                            Specification = JobOrderLineTab.Specification,
                            OrderBalanceQty = Vl.BalanceQty,
                            Qty = Vl.BalanceQty,
                            DocQty = Vl.BalanceQty,
                            ReceiveQty = Vl.BalanceQty,
                            PassQty = Vl.BalanceQty,
                            LotNo = JobOrderLineTab.LotNo,
                            //OrderQty = JobOrderLineTab.Qty,
                            //OrderDealQty = JobOrderLineTab.DealQty,
                            ProductName = ProductTab.ProductName,
                            ProductId = Vl.ProductId,
                            JobReceiveHeaderId = vm.JobReceiveHeaderId,
                            JobOrderLineId = Vl.JobOrderLineId,
                            UnitId = ProductTab.UnitId,
                            JobOrderHeaderDocNo = Vl.JobOrderNo,
                            DealUnitId = ProductTab.UnitId,
                            UnitDecimalPlaces = ProductTab.Unit.DecimalPlaces,
                            UnitConversionMultiplier = JobOrderLineTab.UnitConversionMultiplier,
                            DealUnitDecimalPlaces = ProductTab.Unit.DecimalPlaces,
                            JobOrderUidHeaderId = JobOrderLineTab.ProductUidHeaderId,
                            ProductUidId = JobOrderLineTab.ProductUidId,
                            CostCenterName = JobOrderHeaderTab.CostCenter.CostCenterName,
                            ProdOrderLineId = JobOrderLineTab.ProdOrderLineId,
                            JobOrderHeaderId = JobOrderLineTab.JobOrderHeaderId,
                            ExcessReceiveAllowedAgainstOrderQty = JobOrderLineTab.Qty * (ProductSiteTab.ExcessReceiveAllowedAgainstOrderPer ?? 0) / 100 > (ProductSiteTab.ExcessReceiveAllowedAgainstOrderQty ?? 0) ? JobOrderLineTab.Qty * (ProductSiteTab.ExcessReceiveAllowedAgainstOrderPer ?? 0) / 100 : (ProductSiteTab.ExcessReceiveAllowedAgainstOrderQty ?? 0)
                        }
                        );
            return temp;
        }
        public void Delete(int id)
        {
            _unitOfWork.Repository<JobReceiveLine>().Delete(id);
        }

        public void Delete(JobReceiveLine pt)
        {
            _unitOfWork.Repository<JobReceiveLine>().Delete(pt);
        }

        public void Update(JobReceiveLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobReceiveLine>().Update(pt);
        }
        public IEnumerable<JobReceiveLineViewModel> GetLineListForIndex(int headerId)
        {
            JobReceiveHeader Header = new JobReceiveHeaderService(_unitOfWork).Find(headerId);

            int DocTypeId = 0;
            int JobReceiveQADocTypeId = 0;
            if (Header != null)
            {
                DocTypeId = Header.DocTypeId;
            }

            if (DocTypeId != 0 && DocTypeId != null)
            {
                JobReceiveQASettings JobReceiveQASettings = (from S in db.JobReceiveQASettings
                                                             where S.DivisionId == Header.DivisionId && S.SiteId == Header.SiteId
                                                             && SqlFunctions.CharIndex(DocTypeId.ToString(), S.filterContraDocTypes) > 0
                                                             select S).FirstOrDefault();
                
                if (JobReceiveQASettings != null)
                {
                    JobReceiveQADocTypeId = JobReceiveQASettings.DocTypeId;
                }

            }



            var JobReceiveQALine = from L in db.JobReceiveLine
                                   join Jql in db.JobReceiveQALine on L.JobReceiveLineId equals Jql.JobReceiveLineId into JobReceiveQALineTable
                                   from JobReceiveQALineTab in JobReceiveQALineTable.DefaultIfEmpty()
                                   where L.JobReceiveHeaderId == headerId
                                   group new { L, JobReceiveQALineTab } by new { L.JobReceiveLineId } into Result
                                   select new
                                   {
                                       JobReceiveLineId = Result.Key.JobReceiveLineId,
                                       JobReceiveQALineId = Result.Max(i => i.JobReceiveQALineTab.JobReceiveQALineId)
                                   };

            var pt = (from p in db.JobReceiveLine
                      where p.JobReceiveHeaderId == headerId
                      join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                      into table from tab in table.DefaultIfEmpty()
                      join H in db.JobReceiveHeader on p.JobReceiveHeaderId equals H.JobReceiveHeaderId into JobReceiveHeaderTable from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                      join Jql in JobReceiveQALine on p.JobReceiveLineId equals Jql.JobReceiveLineId into JobReceiveQALineTable
                      from JobReceiveQALineTab in JobReceiveQALineTable.DefaultIfEmpty()
                      join Pp in db.ProductProcess on new { X1 = tab.ProductId, X2 = tab.Dimension1Id, X3 = tab.Dimension2Id, X4 = tab.Dimension3Id, X5 = tab.Dimension4Id, X6 = JobReceiveHeaderTab.ProcessId } equals new { X1 = Pp.ProductId, X2 = Pp.Dimension1Id, X3 = Pp.Dimension2Id, X4 = Pp.Dimension3Id, X5 = Pp.Dimension4Id, X6 = (Pp.ProcessId ?? 0) } into ProductProcessTable
                      from ProductProcessTab in ProductProcessTable.DefaultIfEmpty()
                      orderby p.Sr
                      select new JobReceiveLineViewModel
                      {
                          JobReceiveLineId = p.JobReceiveLineId,
                          Qty = p.Qty + p.LossQty,
                          Remark = p.Remark,
                          UnitConversionMultiplier = p.UnitConversionMultiplier,
                          JobOrderHeaderDocNo = tab.JobOrderHeader.DocNo,
                          Dimension1Name = tab.Dimension1.Dimension1Name,
                          Dimension2Name = tab.Dimension2.Dimension2Name,
                          Dimension3Name = tab.Dimension3.Dimension3Name,
                          Dimension4Name = tab.Dimension4.Dimension4Name,
                          Specification = tab.Specification,
                          JobReceiveHeaderId = p.JobReceiveHeaderId,
                          LossQty = p.LossQty,
                          LotNo = p.LotNo,
                          PassQty = p.PassQty,
                          PenaltyAmt = p.PenaltyAmt,
                          ProductId = tab.ProductId,
                          ProductName = tab.Product.ProductName,
                          UnitName = tab.Unit.UnitName,
                          ProductUidId = p.ProductUidId,
                          ProductUidName = p.ProductUid.ProductUidName,
                          ReceiveQty = p.Qty,
                          StockId = p.StockId,
                          DealQty = p.DealQty,
                          StockProcessId = p.StockProcessId,
                          UnitDecimalPlaces = tab.Unit.DecimalPlaces,
                          JobOrderLineId = p.JobOrderLineId,
                          OrderDocTypeId = tab.JobOrderHeader.DocTypeId,
                          OrderHeaderId = tab.JobOrderHeaderId,
                          JobReceiveQALineId = JobReceiveQALineTab.JobReceiveQALineId,
                          JobReceiveQADocTypeId = JobReceiveQADocTypeId,
                          QAGroupId = ProductProcessTab.QAGroupId,
                         DealUnitName=tab.DealUnit.UnitName
                      });

            return pt;


        }
        public JobReceiveLineViewModel GetJobReceiveLine(int id)
        {
            return (from L in db.JobReceiveLine
                    join VJob in db.ViewJobOrderBalance on L.JobOrderLineId equals VJob.JobOrderLineId into ViewJobOrderBalanceTable
                    from ViewJobOrderBalanceTab in ViewJobOrderBalanceTable.DefaultIfEmpty()
                    //join jl in db.JobOrderLine on L.JobOrderLineId equals jl.JobOrderLineId
                    //from tab in table.DefaultIfEmpty()
                    join Ps in db.ProductSiteDetail on new { A1 = L.JobReceiveHeader.SiteId, A2 = L.JobReceiveHeader.DivisionId, A3 = L.JobReceiveHeader.ProcessId, A4 = L.JobOrderLine.ProductId }
                            equals new { A1 = Ps.SiteId, A2 = Ps.DivisionId, A3 = (Ps.ProcessId ?? 0), A4 = Ps.ProductId } into ProductSiteTable
                    from ProductSiteTab in ProductSiteTable.DefaultIfEmpty()
                    where L.JobReceiveLineId == id
                    select new JobReceiveLineViewModel
                    {
                        JobOrderHeaderDocNo = L.JobOrderLine.JobOrderHeader.DocNo,
                        JobOrderLineId = L.JobOrderLineId,
                        JobReceiveHeaderDocNo = L.JobReceiveHeader.DocNo,
                        JobReceiveHeaderId = L.JobReceiveHeaderId,
                        JobReceiveLineId = L.JobReceiveLineId,
                        ProductId = L.ProductId,
                        Dimension1Id = L.Dimension1Id,
                        Dimension2Id = L.Dimension2Id,
                        Dimension3Id = L.Dimension3Id,
                        Dimension4Id = L.Dimension4Id,
                        Dimension1Name = L.Dimension1.Dimension1Name,
                        Dimension2Name = L.Dimension2.Dimension2Name,
                        Dimension3Name = L.Dimension3.Dimension3Name,
                        Dimension4Name = L.Dimension4.Dimension4Name,
                        Specification = L.Specification,
                        OrderBalanceQty = (ViewJobOrderBalanceTab == null ? 0 : ViewJobOrderBalanceTab.BalanceQty) + L.Qty + L.LossQty,
                        UnitDecimalPlaces = L.Product.Unit.DecimalPlaces,
                        DealUnitDecimalPlaces = L.DealUnit.DecimalPlaces,
                        UnitConversionMultiplier = L.UnitConversionMultiplier,
                        UnitId = L.JobOrderLine.UnitId ?? L.Product.UnitId,
                        MachineId = L.MachineId,
                        DealUnitId = L.DealUnitId,
                        DealQty = L.DealQty,
                        ProductUidId = L.ProductUidId,
                        ProductUidName = L.ProductUid.ProductUidName,
                        IncentiveAmt = L.IncentiveAmt,
                        IncentiveRate = L.IncentiveRate,
                        LossQty = L.LossQty,
                        LotNo = L.LotNo,
                        PassQty = L.PassQty,
                        PenaltyAmt = L.PenaltyAmt,
                        PenaltyRate = L.PenaltyRate,
                        DocQty = L.Qty + L.LossQty,
                        ReceiveQty = L.Qty,
                        Weight = L.Weight,
                        Qty = L.Qty + L.LossQty,
                        Remark = L.Remark,
                        JobWorkerId = L.JobReceiveHeader.JobWorkerId,
                        LockReason = L.LockReason,
                        ExcessReceiveAllowedAgainstOrderQty = L.JobOrderLine.Qty * (ProductSiteTab.ExcessReceiveAllowedAgainstOrderPer ?? 0) / 100 > (ProductSiteTab.ExcessReceiveAllowedAgainstOrderQty ?? 0) ? L.JobOrderLine.Qty * (ProductSiteTab.ExcessReceiveAllowedAgainstOrderPer ?? 0) / 100 : (ProductSiteTab.ExcessReceiveAllowedAgainstOrderQty ?? 0)
                    }).FirstOrDefault();

        }

        public CostCenter GetCoscenterId(int id)
        {
            return (from JRL in db.JobReceiveLine                    
                    join JOL in db.JobOrderLine on JRL.JobOrderLineId equals JOL.JobOrderLineId into tableJOL
                    from tabJOL in tableJOL.DefaultIfEmpty()
                    join JOH in db.JobOrderHeader on tabJOL.JobOrderHeaderId equals JOH.JobOrderHeaderId into tableJOH
                    from tabJOH in tableJOH.DefaultIfEmpty()
                    join CC in db.CostCenter on tabJOH.CostCenterId equals CC.CostCenterId into tableCC
                    from tabCC in tableCC.DefaultIfEmpty()
                    where JRL.JobReceiveLineId == id
                    select tabCC
                      ).FirstOrDefault();


        }

        public IEnumerable<JobReceiveLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobReceiveLine>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobReceiveLine> GetJobReceiveLineList()
        {
            var pt = _unitOfWork.Repository<JobReceiveLine>().Query().Get();

            return pt;
        }

        public IEnumerable<JobReceiveLine> GetJobReceiveLineList(int JobReceiveHeaderId)
        {
            var pt = _unitOfWork.Repository<JobReceiveLine>().Query().Get().Where(i => i.JobReceiveHeaderId == JobReceiveHeaderId);

            return pt;
        }

        public IEnumerable<JobReceiveLine> ProductUidsExist(int JobReceiveHeaderId,int? ProductUid)
        {
            var pt = _unitOfWork.Repository<JobReceiveLine>().Query().Get().Where(i => i.JobReceiveHeaderId == JobReceiveHeaderId && i.ProductUidId== ProductUid);

            return pt;
        }
     
        public JobReceiveLine Add(JobReceiveLine pt)
        {
            _unitOfWork.Repository<JobReceiveLine>().Insert(pt);
            return pt;
        }


        public JobReceiveLineViewModel GetJobReceiveDetailBalance(int id)
        {
            var temp = (from b in db.ViewJobReceiveBalance
                        join p in db.JobReceiveLine on b.JobReceiveLineId equals p.JobReceiveLineId
                        join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId into table
                        from tab in table.DefaultIfEmpty()
                        where b.JobReceiveLineId == id
                        select new JobReceiveLineViewModel
                        {
                            JobReceiveHeaderDocNo = b.JobReceiveNo,
                            JobReceiveLineId = b.JobReceiveLineId,
                            Qty = b.BalanceQty,
                            Remark = p.Remark,
                            Rate = tab.Rate,
                            Amount = tab.Rate * (b.BalanceQty * tab.UnitConversionMultiplier),
                            Dimension1Id = tab.Dimension1Id,
                            Dimension1Name = tab.Dimension1.Dimension1Name,
                            Dimension2Id = tab.Dimension2Id,
                            Dimension2Name = tab.Dimension2.Dimension2Name,
                            Dimension3Id = tab.Dimension3Id,
                            Dimension3Name = tab.Dimension3.Dimension3Name,
                            Dimension4Id = tab.Dimension4Id,
                            Dimension4Name = tab.Dimension4.Dimension4Name,
                            ProductId = tab.ProductId,
                            ProductName = tab.Product.ProductName,
                            Specification = tab.Specification,
                            LotNo = p.LotNo,
                            UnitConversionMultiplier = tab.UnitConversionMultiplier,
                            UnitId = tab.UnitId,
                            UnitName = tab.Unit.UnitName,
                            DealUnitId = tab.DealUnitId,
                            DealUnitName = tab.DealUnit.UnitName,
                            DealQty = b.BalanceQty * tab.UnitConversionMultiplier,
                            Weight = (p.Weight / p.Qty) * b.BalanceQty,
                        }).FirstOrDefault();
            return temp;

        }

        public JobReceiveLineViewModel GetJobReceiveDetailBalanceForInvoice(int id)
        {

            var temp = (from b in db.ViewJobReceiveBalanceForInvoice
                        join p in db.JobReceiveLine on b.JobReceiveLineId equals p.JobReceiveLineId
                        join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId into table
                        from tab in table.DefaultIfEmpty()
                        join rqa in db.JobReceiveQALine on p.JobReceiveLineId equals rqa.JobReceiveLineId into qatable
                        from qatab in qatable.DefaultIfEmpty()
                        where b.JobReceiveLineId == id
                        select new JobReceiveLineViewModel
                        {
                            JobReceiveHeaderDocNo = b.JobReceiveNo,
                            JobReceiveLineId = b.JobReceiveLineId,
                            Qty = b.BalanceQty,
                            Remark = p.Remark,
                            Rate = tab.Rate,
                            Amount = tab.Rate * (b.BalanceQty * tab.UnitConversionMultiplier),
                            Dimension1Id = tab.Dimension1Id,
                            Dimension1Name = tab.Dimension1.Dimension1Name,
                            Dimension2Id = tab.Dimension2Id,
                            Dimension2Name = tab.Dimension2.Dimension2Name,
                            Dimension3Id = tab.Dimension3Id,
                            Dimension3Name = tab.Dimension3.Dimension3Name,
                            Dimension4Id = tab.Dimension4Id,
                            Dimension4Name = tab.Dimension4.Dimension4Name,
                            ProductId = tab.ProductId,
                            ProductName = tab.Product.ProductName,
                            SalesTaxGroupProductId = tab.Product.SalesTaxGroupProductId ?? tab.Product.ProductGroup.DefaultSalesTaxGroupProductId,
                            SalesTaxGroupProductName = tab.Product.SalesTaxGroupProduct.ChargeGroupProductName ?? tab.Product.ProductGroup.DefaultSalesTaxGroupProduct.ChargeGroupProductName,
                            Specification = tab.Specification,
                            LotNo = p.LotNo,
                            UnitConversionMultiplier = tab.UnitConversionMultiplier,
                            UnitId = tab.UnitId,
                            UnitName = tab.Unit.UnitName,
                            DealUnitId = tab.DealUnitId,
                            DealUnitName = tab.DealUnit.UnitName,
                            DealQty = b.BalanceQty * tab.UnitConversionMultiplier,
                            JobWorkerId = b.JobWorkerId,
                            CostCenterId = tab.JobOrderHeader.CostCenterId != null ? tab.JobOrderHeader.CostCenterId : null,
                            CostCenterName = tab.JobOrderHeader.CostCenterId != null ? tab.JobOrderHeader.CostCenter.CostCenterName : null,
                            PenaltyAmt = p.PenaltyAmt - (p.IncentiveAmt ?? 0) + ((qatab == null) ? 0 : qatab.PenaltyAmt),
                        }).FirstOrDefault();

            var JobOrderLineId = (from p in db.JobReceiveLine
                                  where p.JobReceiveLineId == temp.JobReceiveLineId
                                  join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                  select new { LineId = p.JobOrderLineId, HeaderId = t.JobOrderHeaderId }).FirstOrDefault();


            var Charges = (from p in db.JobOrderLineCharge
                           where p.LineTableId == JobOrderLineId.LineId
                           join t in db.Charge on p.ChargeId equals t.ChargeId
                           select new LineCharges
                           {
                               ChargeCode = t.ChargeCode,
                               Rate = p.Rate,
                           }).ToList();

            var HeaderCharges = (from p in db.JobOrderHeaderCharges
                                 where p.HeaderTableId == JobOrderLineId.HeaderId
                                 join t in db.Charge on p.ChargeId equals t.ChargeId
                                 select new HeaderCharges
                                 {
                                     ChargeCode = t.ChargeCode,
                                     Rate = p.Rate,
                                 }).ToList();

            temp.RHeaderCharges = HeaderCharges;
            temp.RLineCharges = Charges;

            return (temp);

        }


        public ComboBoxPagedResult GetPendingProductsForJobReceive(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var JobReceive = new JobReceiveHeaderService(_unitOfWork).Find(filter);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(JobReceive.DocTypeId, JobReceive.DivisionId, JobReceive.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }


            var Query = (from p in db.Product
                         join t in db.ProductGroups on p.ProductGroupId equals t.ProductGroupId
                         select new
                         {
                             ProductId = p.ProductId,
                             ProductName = p.ProductName,
                             ProductTypeId = t.ProductTypeId,
                         });

            if (!string.IsNullOrEmpty(settings.filterProductTypes))
                Query = Query.Where(m => ProductTypes.Contains(m.ProductTypeId.ToString()));

            if (!string.IsNullOrEmpty(searchTerm))
                Query = Query.Where(m => m.ProductName.ToLower().Contains(searchTerm.ToLower()));

            var Recods = Query.OrderBy(m => m.ProductName).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();
            var Count = Query.Count();

            return (new ComboBoxPagedResult
            {
                Results = Recods.Select(m => new ComboBoxResult { id = m.ProductId.ToString(), text = m.ProductName }).ToList(),
                Total = Count,
            });

        }


        public ComboBoxPagedResult GetPendingProductGroupsForJobReceive(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var JobReceive = new JobReceiveHeaderService(_unitOfWork).Find(filter);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(JobReceive.DocTypeId, JobReceive.DivisionId, JobReceive.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }


            var Query = (from p in db.ProductGroups
                         select new
                         {
                             ProductGroupId = p.ProductGroupId,
                             ProductGroupName = p.ProductGroupName,
                             ProductTypeId = p.ProductTypeId,
                         });

            if (!string.IsNullOrEmpty(settings.filterProductTypes))
                Query = Query.Where(m => ProductTypes.Contains(m.ProductTypeId.ToString()));

            if (!string.IsNullOrEmpty(searchTerm))
                Query = Query.Where(m => m.ProductGroupName.ToLower().Contains(searchTerm.ToLower()));

            var Recods = Query.OrderBy(m => m.ProductGroupName).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();
            var Count = Query.Count();

            return (new ComboBoxPagedResult
            {
                Results = Recods.Select(m => new ComboBoxResult { id = m.ProductGroupId.ToString(), text = m.ProductGroupName }).ToList(),
                Total = Count,
            });

        }

        public ComboBoxPagedResult GetPendingJobOrders(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var header = new JobReceiveHeaderService(_unitOfWork).Find(filter);

            var Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(header.DocTypeId, DivisionId, SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(Settings.filterContraSites)) { ContraSites = Settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            // var ConSites = settings.filterContraSites.Split(',').Select(Int32.Parse).ToList().ToArray();

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDivisions)) { ContraDivisions = Settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            string[] ContraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { ContraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { ContraDocTypes = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderBalance
                         join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId into ProdTable
                         from ProTab in ProdTable.DefaultIfEmpty()
                         where p.JobWorkerId == header.JobWorkerId && p.BalanceQty > 0
                         select new
                         {
                             Id = ProTab.JobOrderHeaderId,
                             DocNo = ProTab.DocNo,
                             BalanceQty = p.BalanceQty,
                             Date = ProTab.DocDate,
                             DocTypeId = ProTab.DocTypeId,
                             SiteId = ProTab.SiteId,
                             DivisionId = ProTab.DivisionId,
                         }
                        );

            //Filters
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes))
                Query = Query.Where(m => ContraDocTypes.Contains(m.DocTypeId.ToString()));

            if (!string.IsNullOrEmpty(Settings.filterContraSites))
                Query = Query.Where(m => ContraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == header
                    .SiteId);

            if (!string.IsNullOrEmpty(Settings.filterContraDivisions))
                Query = Query.Where(m => ContraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == header.DivisionId);

            DateTime Temp;

            if (searchTerm != null && searchTerm != "")
            {
                if (DateTime.TryParse(searchTerm, out Temp))
                {
                    Query = Query.Where(m => m.Date == Temp);
                }
                else
                {
                    Query = Query.Where(m => m.DocNo.ToLower().Contains(searchTerm.ToLower()));
                }
            }
            var GQuery = (from p in Query
                          group p by p.Id into g
                          select new
                          {
                              Id = g.Key,
                              DocNo = g.Max(m => m.DocNo),
                              Date = g.Max(m => m.Date),
                              BalanceQty = g.Sum(m => m.BalanceQty),
                          });
            var Recods = GQuery.OrderBy(m => m.DocNo).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();
            var Count = GQuery.Count();

            return (new ComboBoxPagedResult
            {
                Results = Recods.Select(m => new ComboBoxResult { id = m.Id.ToString(), text = m.DocNo, TextProp1 = "Dated : " + m.Date.ToString("dd/MMM/yyyy"), TextProp2 = "Balance : " + m.BalanceQty.ToString() }).ToList(),
                Total = Count,
            });
        }

        public IQueryable<ComboBoxResult> GetPendingCostCenters(int id, string term)//DocTypeId
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var header = new JobReceiveHeaderService(_unitOfWork).Find(id);

            var Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(header.DocTypeId, DivisionId, SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(Settings.filterContraSites)) { ContraSites = Settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            // var ConSites = settings.filterContraSites.Split(',').Select(Int32.Parse).ToList().ToArray();

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDivisions)) { ContraDivisions = Settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            string[] ContraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { ContraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { ContraDocTypes = new string[] { "NA" }; }

            return (from p in db.ViewJobOrderBalance
                    join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where (string.IsNullOrEmpty(Settings.filterContraDocTypes) ? 1 == 1 : ContraDocTypes.Contains(ProTab.DocTypeId.ToString())) && p.BalanceQty > 0 && ProTab.CostCenter.CostCenterName.ToLower().Contains(term.ToLower()) && p.JobWorkerId == header.JobWorkerId
                     && (string.IsNullOrEmpty(Settings.filterContraSites) ? p.SiteId == header.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                     && (string.IsNullOrEmpty(Settings.filterContraDivisions) ? p.DivisionId == header.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                    && ProTab.CostCenterId != null
                    group new { p, ProTab } by ProTab.CostCenterId into g
                    orderby g.Key
                    select new ComboBoxResult
                    {
                        id = g.Key.ToString(),
                        text = g.Max(m => m.ProTab.CostCenter.CostCenterName),
                    }
                        );
        }

        public IEnumerable<ProcGetBomForWeavingViewModel> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int ProcessId, decimal Qty, int DocTypeId, string ProcName, int? JobOrderLineId, Decimal? Weight)
        {
            SqlParameter SQLDocTypeID = new SqlParameter("@DocTypeId", DocTypeId);
            SqlParameter SQLProductID = new SqlParameter("@ProductId", ProductId);
            SqlParameter SQLProcessId = new SqlParameter("@ProcessId", ProcessId);
            SqlParameter SQLQty = new SqlParameter("@Qty", Qty);
            SqlParameter SQLDime1 = new SqlParameter("@Dimension1Id", Dimension1Id == null ? DBNull.Value : (object)Dimension1Id);
            SqlParameter SQLDime2 = new SqlParameter("@Dimension2Id", Dimension2Id == null ? DBNull.Value : (object)Dimension2Id);
            SqlParameter SQLJobOrderLineId = new SqlParameter("@JobOrderLineId", JobOrderLineId);
            SqlParameter SQLWeight = new SqlParameter("@Weight", Weight);

            List<ProcGetBomForWeavingViewModel> BomForWeaving = new List<ProcGetBomForWeavingViewModel>();

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //string ProcName = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId).SqlProcConsumption;

            //PendingOrderQtyForPacking = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetBomForWeaving @DocTypeId, @ProductId, @ProcessId,@Qty"+(Dimension1Id==null?"":",@Dimension1Id")+(Dimension2Id==null?"":",@Dimension2Id"), SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, (Dimension1Id==null? "" : Dimension1Id), (Dimension2Id)).ToList();


            if (JobOrderLineId != null && Weight != null && Weight != 0)
            {
                BomForWeaving = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension1Id, @Dimension2Id, @JobOrderLineId, @Weight", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, SQLDime1, SQLDime2, SQLJobOrderLineId, SQLWeight).ToList();
            }
            else if (Dimension1Id == null && Dimension2Id == null)
            {
                BomForWeaving = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty).ToList();
            }
            else if (Dimension1Id == null && Dimension2Id != null)
            {
                BomForWeaving = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension2Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, SQLDime2).ToList();
            }
            else if (Dimension1Id != null && Dimension2Id == null)
            {
                BomForWeaving = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension1Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, SQLDime1).ToList();
            }
            else
            {
                BomForWeaving = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension1Id, @Dimension2Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, SQLDime1, SQLDime2).ToList();
            }


            return BomForWeaving;

        }


        public IQueryable<JobReceiveBomViewModel> GetConsumptionLineListForIndex(int JobReceiveHeaderId)
        {
            var temp = from p in db.JobReceiveBom
                       join t2 in db.Product on p.ProductId equals t2.ProductId into table2
                       from ProdTab in table2.DefaultIfEmpty()
                       orderby p.JobReceiveLineId
                       where p.JobReceiveHeaderId == JobReceiveHeaderId
                       select new JobReceiveBomViewModel
                       {
                           UnitName = ProdTab.Unit.UnitName,
                           UnitDecimalPlaces = ProdTab.Unit.DecimalPlaces,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Name = p.Dimension2.Dimension2Name,
                           Dimension3Name = p.Dimension3.Dimension3Name,
                           Dimension4Name = p.Dimension4.Dimension4Name,
                           JobReceiveBomId = p.JobReceiveBomId,
                           JobReceiveHeaderId = p.JobReceiveHeaderId,
                           JobReceiveLineId = p.JobReceiveLineId,
                           ProductName = ProdTab.ProductName,
                           Qty = p.Qty,
                           CostCenterName = p.CostCenter.CostCenterName,
                       };
            return temp;
        }

        public IQueryable<JobReceiveByProductViewModel> GetByProductListForIndex(int JobReceiveHeaderId)
        {
            var temp = from p in db.JobReceiveByProduct
                       join t2 in db.Product on p.ProductId equals t2.ProductId into table2
                       from ProdTab in table2.DefaultIfEmpty()
                       orderby p.JobReceiveByProductId
                       where p.JobReceiveHeaderId == JobReceiveHeaderId
                       select new JobReceiveByProductViewModel
                       {
                           UnitName = ProdTab.Unit.UnitName,
                           UnitDecimalPlaces = ProdTab.Unit.DecimalPlaces,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Name = p.Dimension2.Dimension2Name,
                           Dimension3Name = p.Dimension3.Dimension3Name,
                           Dimension4Name = p.Dimension4.Dimension4Name,
                           JobReceiveByProductId = p.JobReceiveByProductId,
                           JobReceiveHeaderId = p.JobReceiveHeaderId,
                           ProductName = ProdTab.ProductName,
                           Qty = p.Qty,
                       };
            return temp;
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.JobReceiveLine
                       where p.JobReceiveHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }


        public string GetFirstBarCodeForCancel(int JobOrderLineId)
        {
            return (from p in db.JobOrderLine
                    where p.JobOrderLineId == JobOrderLineId
                    join t in db.ProductUid on p.ProductUidHeaderId equals t.ProductUidHeaderId
                    join t2 in db.JobOrderCancelLine on t.ProductUIDId equals t2.ProductUidId into table
                    from tab in table.DefaultIfEmpty()
                    where tab == null
                    select p.ProductUidId).FirstOrDefault().ToString();
        }

        public void Dispose()
        {
        }

        public List<ComboBoxList> GetPendingBarCodesList(int id)
        {
            List<ComboBoxList> Barcodes = new List<ComboBoxList>();

            var JobOrderline = new JobOrderLineService(_unitOfWork).Find(id);
            var JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderline.JobOrderHeaderId);

            using (ApplicationDbContext context = new ApplicationDbContext())
            {

                //context.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                //context.Database.CommandTimeout = 30000;

                //var Temp = (from p in context.ProductUid
                //            join t in context.JobReceiveLine on p.ProductUIDId equals t.ProductUidId into table
                //            from tab in table.DefaultIfEmpty()
                //            join t3 in context.JobReceiveHeader.Where(m => m.SiteId == JobOrderHeader.SiteId && m.DivisionId == JobOrderHeader.DivisionId) on tab.JobReceiveHeaderId equals t3.JobReceiveHeaderId into table5
                //            from JRH in table5.DefaultIfEmpty()
                //            //where p.ProductUidHeaderId == JobOrderline.ProductUidHeaderId && JRH == null
                //            //&& p.Status != ProductUidStatusConstants.Return && p.Status != ProductUidStatusConstants.Cancel && ((p.GenDocId == p.LastTransactionDocId && p.GenDocNo == p.LastTransactionDocNo && p.GenPersonId == p.LastTransactionPersonId) || p.CurrenctGodownId != null)
                //            //orderby p.ProductUIDId
                //            join t2 in context.JobOrderLine on p.ProductUidHeaderId equals t2.ProductUidHeaderId
                //            join JOH in context.JobOrderHeader.Where(m => m.SiteId == JobOrderHeader.SiteId && m.DivisionId == JobOrderHeader.DivisionId) on t2.JobOrderHeaderId equals JOH.JobOrderHeaderId
                //            join RecLineStatus in context.JobReceiveLineStatus on tab.JobReceiveLineId equals RecLineStatus.JobReceiveLineId into RecLineStatTab
                //            from RecLineStat in RecLineStatTab.DefaultIfEmpty()
                //            where p.ProductUidHeaderId == JobOrderline.ProductUidHeaderId && (JRH == null || ((tab.Qty - (RecLineStat.ReturnQty ?? 0)) == 0)) && p.Status != ProductUidStatusConstants.Return &&
                //            p.Status != ProductUidStatusConstants.Cancel && ((p.GenPersonId == p.LastTransactionPersonId) || p.CurrenctGodownId != null)
                //            && JOH.ProcessId == JobOrderHeader.ProcessId
                //            orderby p.ProductUIDId
                //            select new { Id = p.ProductUIDId, Name = p.ProductUidName }).ToList();


                SqlParameter SQLProductUidHeaderId = new SqlParameter("@ProductUidHeaderId", JobOrderline.ProductUidHeaderId);
                SqlParameter SQLSiteId = new SqlParameter("@SiteId", JobOrderHeader.SiteId);
                SqlParameter SQLDivisionId = new SqlParameter("@DivisionId", JobOrderHeader.DivisionId);
                SqlParameter SQLProcessId = new SqlParameter("@ProcessId", JobOrderHeader.ProcessId);


                var Temp = db.Database.SqlQuery<ProductUidList>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetProductUidListForWeavingOrderCancel @ProductUidHeaderId, @SiteId,	@DivisionId, @ProcessId", SQLProductUidHeaderId, SQLSiteId, SQLDivisionId, SQLProcessId).ToList();

                foreach (var item in Temp)
                {
                    Barcodes.Add(new ComboBoxList
                    {
                        Id = item.ProductUidId,
                        PropFirst = item.ProductUidName,
                    });
                }
            }



            return Barcodes;
        }

        public List<ComboBoxList> GetPendingBarCodesList(int[] id)
        {
            List<ComboBoxList> Barcodes = new List<ComboBoxList>();

            //var LineIds = id.Split(',').Select(Int32.Parse).ToArray();

            using (ApplicationDbContext context = new ApplicationDbContext())
            {

                //context.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                //context.Database.CommandTimeout = 30000;

                var Temp = (from p in context.ViewJobOrderBalance
                            join t in context.ProductUid on p.ProductUidId equals t.ProductUIDId
                            join t2 in context.JobOrderLine on p.JobOrderLineId equals t2.JobOrderLineId
                            where id.Contains(p.JobOrderLineId) && p.ProductUidId != null
                            orderby t2.Sr
                            select new { Id = t.ProductUIDId, Name = t.ProductUidName }).ToList();
                foreach (var item in Temp)
                {
                    Barcodes.Add(new ComboBoxList
                    {
                        Id = item.Id,
                        PropFirst = item.Name,
                    });
                }
            }



            return Barcodes;
        }


        public IEnumerable<ComboBoxList> GetConsumptionProducts(string term, int filter)
        {
            var Receive = db.JobReceiveHeader.Find(filter);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(Receive.DocTypeId, Receive.DivisionId, Receive.SiteId);

            SqlParameter SqlParameterProcessId = new SqlParameter("@ProcessId", Receive.ProcessId);
            SqlParameter SqlParameterPersonId = new SqlParameter("@PersonId", Receive.JobWorkerId);
            SqlParameter SqlParameterUptoDate = new SqlParameter("@UptoDate", DateTime.Now.Date);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", System.Web.HttpContext.Current.Session["DivisionId"]);
            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", System.Web.HttpContext.Current.Session["SiteId"]);

            IEnumerable<StockLineViewModel> StockProcessBalance = db.Database.SqlQuery<StockLineViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spStockProcessBalance  @UptoDate,@SiteId,@DivisionId,@PersonId,@ProcessId", SqlParameterUptoDate, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterPersonId, SqlParameterProcessId).ToList();

            var temp = from H in StockProcessBalance
                       where string.IsNullOrEmpty(term) ? 1 == 1 : H.ProductName.ToLower().Contains(term.ToLower())
                       select new ComboBoxList
                       {
                           Id = H.ProductId,
                           PropFirst = H.ProductName,
                           PropSecond = H.BalanceQty.ToString(),
                       };

            return temp;
        }

        public decimal GetConsumptionBalanceQty(int filter, int ProductId)
        {
            var Receive = db.JobReceiveHeader.Find(filter);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(Receive.DocTypeId, Receive.DivisionId, Receive.SiteId);

            SqlParameter SqlParameterProcessId = new SqlParameter("@ProcessId", Receive.ProcessId);
            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterCostCenterId = new SqlParameter("@CostCenterId", DBNull.Value);
            SqlParameter SqlParameterPersonId = new SqlParameter("@PersonId", Receive.JobWorkerId);
            SqlParameter SqlParameterUptoDate = new SqlParameter("@UptoDate", DateTime.Now.Date);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", System.Web.HttpContext.Current.Session["DivisionId"]);
            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", System.Web.HttpContext.Current.Session["SiteId"]);

            StockLineViewModel StockProcessBalance = db.Database.SqlQuery<StockLineViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spStockProcessBalance  @UptoDate,@SiteId,@DivisionId,@PersonId,@ProcessId,@CostCenterId,@ProductId", SqlParameterUptoDate, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterPersonId, SqlParameterProcessId, SqlParameterCostCenterId, SqlParameterProductId).FirstOrDefault();

            return StockProcessBalance.BalanceQty;
        }

        
        

        public string GetReasons(int filter)//DocTypeId
        {

            var Query = (from p in db.Reason
                         join t in db.DocumentType on p.DocumentCategoryId equals t.DocumentCategoryId
                         where t.DocumentTypeId == filter && p.IsActive == true
                         select new
                         {
                             p.ReasonName,
                         }
                        );

            return string.Join(",", Query.Select(m => m.ReasonName).ToList());

        }

        public List<ComboBoxResult> SetReason(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                string temp = subStr[i];
                IEnumerable<Reason> prod = from H in db.Reason
                                           where H.ReasonName == temp
                                           select H;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ReasonName,
                    text = prod.FirstOrDefault().ReasonName
                });
            }
            return ProductJson;
        }


        public IEnumerable<ComboBoxResult> FGetProductUidHelpList(int Id, string term)
        {
            var JobReceiveHeader = new JobReceiveHeaderService(_unitOfWork).Find(Id);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(JobReceiveHeader.DocTypeId, JobReceiveHeader.DivisionId, JobReceiveHeader.SiteId);

            SqlParameter SQLJobReceiveHeaderId = new SqlParameter("@JobReceiveHeaderId", Id);
            IEnumerable<ComboBoxResult> ProductUidList = db.Database.SqlQuery<ComboBoxResult>(settings.SqlProcProductUidHelpList + " @JobReceiveHeaderId", SQLJobReceiveHeaderId).ToList();


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


        public IEnumerable<ComboBoxResult> GetJobOrderHelpListForProduct(int Id, string term)
        {
            var JobReceiveHeader = new JobReceiveHeaderService(_unitOfWork).Find(Id);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(JobReceiveHeader.DocTypeId, JobReceiveHeader.DivisionId, JobReceiveHeader.SiteId);

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var OrderBalance = (from VB in db.ViewJobOrderBalance
                                where VB.JobWorkerId == JobReceiveHeader.JobWorkerId
                                select new
                                {
                                    JobOrderLineId = VB.JobOrderLineId,
                                    BalanceQty = VB.BalanceQty
                                });

            return (from VB in OrderBalance
                    join L in db.JobOrderLine on VB.JobOrderLineId equals L.JobOrderLineId into JobOrderLineTable
                    from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                    where JobOrderLineTab.JobOrderHeader.JobWorkerId == JobReceiveHeader.JobWorkerId
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? JobOrderLineTab.JobOrderHeader.Site.SiteId == CurrentSiteId : contraSites.Contains(JobOrderLineTab.JobOrderHeader.Site.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? JobOrderLineTab.JobOrderHeader.Division.DivisionId == CurrentDivisionId : contraDivisions.Contains(JobOrderLineTab.JobOrderHeader.Division.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.JobOrderHeader.DocNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.JobOrderHeader.DocType.DocumentTypeShortName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Product.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.LotNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.ProductUid.ProductUidName.ToLower().Contains(term.ToLower())
                        )
                    select new ComboBoxResult
                    {
                        id = VB.JobOrderLineId.ToString(),
                        text = JobOrderLineTab.JobOrderHeader.DocType.DocumentTypeShortName + "-" + JobOrderLineTab.JobOrderHeader.DocNo,
                        TextProp1 = "Balance :" + VB.BalanceQty,
                        TextProp2 = "Date :" + JobOrderLineTab.JobOrderHeader.DocDate, 
                        AProp1 = JobOrderLineTab.Product.ProductName + ((JobOrderLineTab.ProductUid.ProductUidName == null) ? "" : "," + JobOrderLineTab.ProductUid.ProductUidName),
                        AProp2 = ((JobOrderLineTab.Dimension1.Dimension1Name == null) ? "" : JobOrderLineTab.Dimension1.Dimension1Name) +
                                    ((JobOrderLineTab.Dimension2.Dimension2Name == null) ? "" : "," + JobOrderLineTab.Dimension2.Dimension2Name) +
                                    ((JobOrderLineTab.Dimension3.Dimension3Name == null) ? "" : "," + JobOrderLineTab.Dimension3.Dimension3Name) +
                                    ((JobOrderLineTab.Dimension4.Dimension4Name == null) ? "" : "," + JobOrderLineTab.Dimension4.Dimension4Name) + 
                                    ((JobOrderLineTab.LotNo == null) ? "" : "," + JobOrderLineTab.LotNo)
                    });

        }




        public Task<IEquatable<JobReceiveLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
