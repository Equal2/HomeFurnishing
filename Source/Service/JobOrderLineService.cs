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
    public interface IJobOrderLineService : IDisposable
    {
        JobOrderLine Create(JobOrderLine s);
        void Delete(int id);
        void Delete(JobOrderLine s);
        JobOrderLineViewModel GetJobOrderLine(int id);
        JobOrderLine Find(int id);
        void Update(JobOrderLine s);
        IQueryable<JobOrderLineViewModel> GetJobOrderLineListForIndex(int JobOrderHeaderId);
        IQueryable<JobOrderBomViewModel> GetConsumptionLineListForIndex(int JobOrderHeaderId);
        IEnumerable<JobOrderLineViewModel> GetJobOrderLineforDelete(int headerid);
        IEnumerable<ProcGetBomForWeavingViewModel> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int ProcessId, decimal Qty, int DocTypeId, string ProcName);
        List<String> GetProcGenProductUids(int DocTypeId, decimal Qty, int DivisionId, int SiteId);
        JobOrderLineViewModel GetLineDetailFromUId(string UID);
        IEnumerable<JobOrderLineViewModel> GetProdOrdersForFilters(JobOrderLineFilterViewModel vm);
        IEnumerable<JobOrderLineViewModel> GetJobOrderForStockInFilters(JobOrderLineFilterForStockInViewModel vm);
        IQueryable<ComboBoxResult> GetPendingProdOrderHelpList(int Id, string term);//PurchaseOrderHeaderId

        IEnumerable<ProdOrderHeaderListViewModel> GetPendingProdOrdersWithPatternMatch(int Id, string term, int Limiter);
        //IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);
        List<ComboBoxResult> GetBarCodesForWeavingWizard(int id, string term);

        IEnumerable<JobRate> GetJobRate(int JobOrderHeaderId, int ProductId);
        decimal GetUnitConversionForProdOrderLine(int ProdLineId, byte UnitConvForId, string DealUnitId);
        IQueryable<ComboBoxResult> GetCustomProductGroups(int Id, string term);
        IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term);

        IQueryable<ComboBoxResult> GetProdOrderHelpListForProduct(int Id, string term);

        IEnumerable<ComboBoxResult> GetPendingProdOrderForProductUid(int JobOrderHeaderId, int ProductUidId, string term);
        IEnumerable<ComboBoxResult> FGetProductUidHelpList(int Id, string term);

        IEnumerable<ComboBoxResult> GetPendingStockInForIssue(int id, int? ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, string term);
        IEnumerable<ComboBoxResult> GetPendingStockInHeaderForIssue(int StockHeaderId, string term);
        Decimal? GetExcessReceiveAllowedAgainstOrderQty(int JobOrderLineId);
    }

    public class JobOrderLineService : IJobOrderLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public JobOrderLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public JobOrderLine Create(JobOrderLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderLine>().Delete(id);
        }

        public void Delete(JobOrderLine s)
        {
            _unitOfWork.Repository<JobOrderLine>().Delete(s);
        }

        public void Update(JobOrderLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderLine>().Update(s);
        }


        public JobOrderLineViewModel GetJobOrderLine(int id)
        {
            var temp = (from p in db.JobOrderLine
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
                            Dimension2Id = p.Dimension2Id,
                            Dimension3Id = p.Dimension3Id,
                            Dimension4Id = p.Dimension4Id,
                            Dimension1Name = p.Dimension1.Dimension1Name,
                            Dimension2Name = p.Dimension2.Dimension2Name,
                            Dimension3Name = p.Dimension3.Dimension3Name,
                            Dimension4Name = p.Dimension4.Dimension4Name,
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
                            DiscountPer = p.DiscountPer,
                            DiscountAmount = p.DiscountAmount,
                            Amount = p.Amount,
                            NonCountedQty = p.NonCountedQty,
                            UnitId = p.UnitId,
                            UnitName = p.Unit.UnitName,
                            UnitConversionMultiplier = p.UnitConversionMultiplier,
                            UnitDecimalPlaces = p.Unit.DecimalPlaces,
                            DealUnitDecimalPlaces = p.DealUnit.DecimalPlaces,
                            Specification = p.Specification,
                            StockInId = p.StockInId,
                            StockInNo = p.StockIn.StockHeader.DocNo,
                            SalesTaxGroupProductId = p.SalesTaxGroupProductId,
                            SalesTaxGroupPersonId = p.JobOrderHeader.SalesTaxGroupPersonId,
                        }).FirstOrDefault();

            temp.ProdOrderBalanceQty = (new ProdOrderLineService(_unitOfWork).GetProdOrderBalance(temp.ProdOrderLineId)) + temp.Qty;


            return temp;
        }

        public JobOrderLine GetMainJobOrderLine(int id)
        {
            var temp = (from p in db.JobOrderLine
                        join POL in db.ProdOrderLine on p.ProdOrderLineId equals POL.ProdOrderLineId into tablePOL
                        from TabPOL in tablePOL.DefaultIfEmpty()
                        join JOL in db.JobOrderLine on TabPOL.ReferenceDocLineId equals JOL.JobOrderLineId into tableJOL
                        from TabJOL in tableJOL.DefaultIfEmpty()
                        where p.JobOrderLineId == id
                        select new JobOrderLine
                        {
                            ProductId = TabJOL.ProductId,
                            ProductUidHeaderId = TabJOL.ProductUidHeaderId,
                            DueDate = TabJOL.DueDate,
                            Qty = TabJOL.Qty,
                            Remark = TabJOL.Remark,
                            JobOrderHeaderId = TabJOL.JobOrderHeaderId,
                            JobOrderLineId = TabJOL.JobOrderLineId,
                            Dimension1Id = TabJOL.Dimension1Id,
                            Dimension2Id = TabJOL.Dimension2Id,
                            Dimension3Id = TabJOL.Dimension3Id,
                            Dimension4Id = TabJOL.Dimension4Id,
                            ProductUidId = TabJOL.ProductUidId,
                            ProdOrderLineId = TabJOL.ProdOrderLineId,
                            LotNo = TabJOL.LotNo,
                            FromProcessId = TabJOL.FromProcessId,
                            DealUnitId = TabJOL.DealUnitId,
                            DealQty = TabJOL.DealQty,
                            LockReason = TabJOL.LockReason,
                            LossQty = TabJOL.LossQty,
                            Rate = TabJOL.Rate,
                            Amount = TabJOL.Amount,
                            NonCountedQty = TabJOL.NonCountedQty,
                            UnitId = TabJOL.UnitId,
                            UnitConversionMultiplier = TabJOL.UnitConversionMultiplier,
                            Specification = TabJOL.Specification,
                        }).FirstOrDefault();


            return temp;
        }

        public JobOrderLine Find(int id)
        {
            return _unitOfWork.Repository<JobOrderLine>().Find(id);
        }


        public IQueryable<JobOrderBomViewModel> GetConsumptionLineListForIndex(int JobOrderHeaderId)
        {
            var temp = from p in db.JobOrderBom
                       join t2 in db.Product on p.ProductId equals t2.ProductId into table2
                       from ProdTab in table2.DefaultIfEmpty()
                       orderby p.JobOrderLineId
                       where p.JobOrderHeaderId == JobOrderHeaderId
                       select new JobOrderBomViewModel
                       {
                           UnitName = ProdTab.Unit.UnitName,
                           UnitDecimalPlaces = ProdTab.Unit.DecimalPlaces,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Name = p.Dimension2.Dimension2Name,
                           Dimension3Name = p.Dimension3.Dimension3Name,
                           Dimension4Name = p.Dimension4.Dimension4Name,
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
            var temp = from p in db.JobOrderLine
                       join t3 in db.JobOrderLineStatus on p.JobOrderLineId equals t3.JobOrderLineId
                       into table3
                       from LineStatus in table3.DefaultIfEmpty()
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
                           Dimension3Id = p.Dimension3Id,
                           Dimension4Id = p.Dimension4Id,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Name = p.Dimension2.Dimension2Name,
                           Dimension3Name = p.Dimension3.Dimension3Name,
                           Dimension4Name = p.Dimension4.Dimension4Name,
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
                           OrderDocTypeId = p.ProdOrderLine.ProdOrderHeader.DocTypeId,
                           OrderHeaderId = p.ProdOrderLine.ProdOrderHeaderId,
                           ProgressPerc = ((p.Qty == 0 || LineStatus == null) ? 0 : (int)(((((LineStatus.ReceiveQty ?? 0)) / (p.Qty + (LineStatus.AmendmentQty ?? 0))) * 100))),
                           ProgressPercCancelled = ((p.Qty == 0 || LineStatus == null) ? 0 : (int)(((((LineStatus.CancelQty ?? 0) / (p.Qty + (LineStatus.AmendmentQty ?? 0)))) * 100))),
                       };
            return temp;
        }

        public IEnumerable<JobOrderLineViewModel> GetJobOrderLineforDelete(int headerid)
        {
            return (from p in db.JobOrderLine
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

        public IEnumerable<ProcGetBomForWeavingViewModel> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int ProcessId, decimal Qty, int DocTypeId, string ProcName)
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
                PendingOrderQtyForPacking = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty).ToList();
            }
            else if (Dimension1Id == null && Dimension2Id != null)
            {
                PendingOrderQtyForPacking = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension2Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, SQLDime2).ToList();
            }
            else if (Dimension1Id != null && Dimension2Id == null)
            {
                PendingOrderQtyForPacking = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension1Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, SQLDime1).ToList();
            }
            else
            {
                PendingOrderQtyForPacking = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension1Id, @Dimension2Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, SQLDime1, SQLDime2).ToList();
            }


            return PendingOrderQtyForPacking;

        }

        public JobOrderRateAmendmentLineViewModel GetLineDetail(int id)
        {
            return (from p in db.ViewJobOrderBalanceForInvoice
                    join t1 in db.JobOrderLine on p.JobOrderLineId equals t1.JobOrderLineId
                    join t2 in db.Product on p.ProductId equals t2.ProductId
                    join D1 in db.Dimension1 on t1.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                    from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                    join D2 in db.Dimension2 on t1.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                    from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                    join D3 in db.Dimension3 on t1.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                    from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                    join D4 in db.Dimension4 on t1.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                    from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                    join t5 in db.JobWorker on p.JobWorkerId equals t5.PersonID
                    where p.JobOrderLineId == id
                    select new JobOrderRateAmendmentLineViewModel
                    {
                        Dimension1Name = Dimension1Tab.Dimension1Name,
                        Dimension2Name = Dimension2Tab.Dimension2Name,
                        Dimension3Name = Dimension3Tab.Dimension3Name,
                        Dimension4Name = Dimension4Tab.Dimension4Name,
                        LotNo = t1.LotNo,
                        Qty = p.BalanceQty,
                        Specification = t1.Specification,
                        UnitId = t1.UnitId,
                        DealUnitId = t1.DealUnitId,
                        DealQty = p.BalanceQty * t1.UnitConversionMultiplier,
                        UnitConversionMultiplier = t1.UnitConversionMultiplier,
                        UnitName = t1.Unit.UnitName,
                        DealUnitName = t1.DealUnit.UnitName,
                        ProductId = p.ProductId,
                        ProductName = t1.Product.ProductName,
                        unitDecimalPlaces = t2.Unit.DecimalPlaces,
                        DealunitDecimalPlaces = t1.DealUnit.DecimalPlaces,
                        JobWorkerId = p.JobWorkerId,
                        JobWorkerName = t5.Person.Name,
                        Rate = p.Rate,
                    }).FirstOrDefault();

        }

        public JobOrderLineViewModel GetLineDetailForCancel(int id)
        {
            var temp = (from p in db.ViewJobOrderBalance
                        join t1 in db.JobOrderLine on p.JobOrderLineId equals t1.JobOrderLineId
                        join t2 in db.Product on p.ProductId equals t2.ProductId
                        join D1 in db.Dimension1 on t1.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on t1.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D1 in db.Dimension3 on t1.Dimension3Id equals D1.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D2 in db.Dimension4 on t1.Dimension4Id equals D2.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        where p.JobOrderLineId == id
                        select new JobOrderLineViewModel
                        {
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
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
                var Temp = (from p in db.ProductUid
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

        public JobOrderLineViewModel GetLineDetailForReceive(int id, int ReceiveId)
        {

            var Receipt = new JobReceiveHeaderService(_unitOfWork).Find(ReceiveId);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(Receipt.DocTypeId, Receipt.DivisionId, Receipt.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }


            Decimal? ExcessReceiveAllowedAgainstOrderQty = GetExcessReceiveAllowedAgainstOrderQty(id);


            return (from p in db.ViewJobOrderBalance
                    join t1 in db.JobOrderLine on p.JobOrderLineId equals t1.JobOrderLineId
                    join t2 in db.Product on p.ProductId equals t2.ProductId
                    join D1 in db.Dimension1 on t1.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                    from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                    join D2 in db.Dimension2 on t1.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                    from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                    join D3 in db.Dimension3 on t1.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                    from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                    join D4 in db.Dimension4 on t1.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                    from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                    join L in db.JobOrderLine on p.JobOrderLineId equals L.JobOrderLineId into JobOrderLineTable from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                    where p.JobOrderLineId == id
                      && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == Receipt.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == Receipt.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                    select new JobOrderLineViewModel
                    {
                        Dimension1Id = Dimension1Tab.Dimension1Id,
                        Dimension1Name = Dimension1Tab.Dimension1Name,
                        Dimension2Id = Dimension2Tab.Dimension2Id,
                        Dimension2Name = Dimension2Tab.Dimension2Name,
                        Dimension3Id = Dimension3Tab.Dimension3Id,
                        Dimension3Name = Dimension3Tab.Dimension3Name,
                        Dimension4Id = Dimension4Tab.Dimension4Id,
                        Dimension4Name = Dimension4Tab.Dimension4Name,
                        JobOrderHeaderDocNo = p.JobOrderNo,
                        ProductUidId = JobOrderLineTab.ProductUidId,
                        ProductUidName = JobOrderLineTab.ProductUid.ProductUidName,
                        LotNo = t1.LotNo,
                        Qty = p.BalanceQty,
                        Specification = t1.Specification,
                        UnitId = t1.UnitId,
                        DealUnitId = t1.DealUnitId,
                        //DealQty = p.BalanceQty * t1.UnitConversionMultiplier,
                        DealQty = (t1.UnitConversionMultiplier == null || t1.UnitConversionMultiplier == 0) ? p.BalanceQty : p.BalanceQty * t1.UnitConversionMultiplier,
                        UnitConversionMultiplier = t1.UnitConversionMultiplier,
                        UnitName = t1.Unit.UnitName,
                        DealUnitName = t1.DealUnit.UnitName,
                        ProductId = p.ProductId,
                        ProductName = t1.Product.ProductName,
                        UnitDecimalPlaces = t2.Unit.DecimalPlaces,
                        DealUnitDecimalPlaces = t1.DealUnit.DecimalPlaces,
                        Rate = p.Rate,
                        ExcessReceiveAllowedAgainstOrderQty = ExcessReceiveAllowedAgainstOrderQty
                    }).FirstOrDefault();

        }


        public JobOrderLineViewModel GetLineDetailForInvoice(int id, int InvoiceId)
        {

            var Invoice = new JobInvoiceHeaderService(_unitOfWork).Find(InvoiceId);

            var OrderLine = new JobOrderLineService(_unitOfWork).Find(id);


            var temp = (from p in db.ViewJobOrderBalance
                        join t1 in db.JobOrderLine on p.JobOrderLineId equals t1.JobOrderLineId
                        join t2 in db.Product on p.ProductId equals t2.ProductId
                        join D1 in db.Dimension1 on t1.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on t1.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D3 in db.Dimension3 on t1.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D4 in db.Dimension4 on t1.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        where p.JobOrderLineId == id
                        select new JobOrderLineViewModel
                        {
                            Dimension1Id = t1.Dimension1Id,
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Id = t1.Dimension2Id,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Id = t1.Dimension3Id,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Id = t1.Dimension4Id,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            LotNo = t1.LotNo,
                            Qty = p.BalanceQty,
                            Specification = t1.Specification,
                            UnitId = t1.UnitId,
                            DealUnitId = t1.DealUnitId,
                            Amount = t1.Amount,
                            DealQty = p.BalanceQty * t1.UnitConversionMultiplier,
                            UnitConversionMultiplier = t1.UnitConversionMultiplier,
                            UnitName = t1.Unit.UnitName,
                            DealUnitName = t1.DealUnit.UnitName,
                            ProductId = p.ProductId,
                            ProductName = t1.Product.ProductName,
                            UnitDecimalPlaces = t2.Unit.DecimalPlaces,
                            DealUnitDecimalPlaces = t1.DealUnit.DecimalPlaces,
                            Rate = p.Rate,
                            JobOrderHeaderDocNo = p.JobOrderNo,
                            SalesTaxGroupProductId = t2.SalesTaxGroupProductId ?? t2.ProductGroup.DefaultSalesTaxGroupProductId,
                            SalesTaxGroupProductName = t2.SalesTaxGroupProduct.ChargeGroupProductName ?? t2.ProductGroup.DefaultSalesTaxGroupProduct.ChargeGroupProductName,
                            CostCenterId = t1.JobOrderHeader.CostCenterId != null ? t1.JobOrderHeader.CostCenterId : null,
                            CostCenterName = t1.JobOrderHeader.CostCenterId != null ? t1.JobOrderHeader.CostCenter.CostCenterName : null,

                        }
                        ).FirstOrDefault();

            var Charges = (from p in db.JobOrderLineCharge
                           where p.LineTableId == OrderLine.JobOrderLineId
                           join t in db.Charge on p.ChargeId equals t.ChargeId
                           select new LineCharges
                           {
                               ChargeCode = t.ChargeCode,
                               Rate = p.Rate,
                           }).ToList();

            var HeaderCharges = (from p in db.JobOrderHeaderCharges
                                 where p.HeaderTableId == OrderLine.JobOrderHeaderId
                                 join t in db.Charge on p.ChargeId equals t.ChargeId
                                 select new HeaderCharges
                                 {
                                     ChargeCode = t.ChargeCode,
                                     Rate = p.Rate,
                                 }).ToList();

            temp.RHeaderCharges = HeaderCharges;
            temp.RLineCharges = Charges;


            return temp;

        }

        public bool ValidateJobOrder(int lineid, int headerid)
        {
            var temp = (from p in db.JobOrderRateAmendmentLine
                        where p.JobOrderLineId == lineid && p.JobOrderAmendmentHeaderId == headerid
                        select p).FirstOrDefault();
            if (temp != null)
                return false;
            else
                return true;

        }

        public JobOrderLineViewModel GetLineDetailFromUId(string UID)
        {
            return (from p in db.ViewJobOrderBalance
                    join t1 in db.JobOrderLine on p.JobOrderLineId equals t1.JobOrderLineId
                    join t in db.ProductUid on t1.ProductUidId equals t.ProductUIDId into uidtable
                    from uidtab in uidtable.DefaultIfEmpty()
                    join t2 in db.Product on p.ProductId equals t2.ProductId
                    join D1 in db.Dimension1 on t1.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                    from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                    join D2 in db.Dimension2 on t1.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                    from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                    join D3 in db.Dimension3 on t1.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                    from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                    join D4 in db.Dimension4 on t1.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                    from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                    where uidtab.ProductUidName == UID
                    select new JobOrderLineViewModel
                    {
                        Dimension1Name = Dimension1Tab.Dimension1Name,
                        Dimension2Name = Dimension2Tab.Dimension2Name,
                        Dimension3Name = Dimension3Tab.Dimension3Name,
                        Dimension4Name = Dimension4Tab.Dimension4Name,
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
            byte? UnitConvForId = new JobOrderHeaderService(_unitOfWork).Find(vm.JobOrderHeaderId).UnitConversionForId;

            var joborder = new JobOrderHeaderService(_unitOfWork).Find(vm.JobOrderHeaderId);

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


            string[] Dimension3 = null;
            if (!string.IsNullOrEmpty(vm.Dimension3Id)) { Dimension3 = vm.Dimension3Id.Split(",".ToCharArray()); }
            else { Dimension3 = new string[] { "NA" }; }

            string[] Dimension4 = null;
            if (!string.IsNullOrEmpty(vm.Dimension4Id)) { Dimension4 = vm.Dimension4Id.Split(",".ToCharArray()); }
            else { Dimension4 = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }


            string[] ProductCategoryIdArr = null;
            if (!string.IsNullOrEmpty(Settings.filterProductCategories)) { ProductCategoryIdArr = Settings.filterProductCategories.Split(",".ToCharArray()); }
            else { ProductCategoryIdArr = new string[] { "NA" }; }


            if (!string.IsNullOrEmpty(vm.DealUnitId))
            {
                Unit Dealunit = new UnitService(_unitOfWork).Find(vm.DealUnitId);

                var temp = (from p in db.ViewProdOrderBalance
                            join t in db.ProdOrderHeader on p.ProdOrderHeaderId equals t.ProdOrderHeaderId into table
                            from tab in table.DefaultIfEmpty()
                            join product in db.Product on p.ProductId equals product.ProductId into table2
                            from tab2 in table2.DefaultIfEmpty()
                            join t1 in db.ProdOrderLine on p.ProdOrderLineId equals t1.ProdOrderLineId into table1
                            from tab1 in table1.DefaultIfEmpty()
                            join t3 in db.UnitConversion on new { p1 = p.ProductId, DU1 = vm.DealUnitId, U1 = UnitConvForId ?? 0 } equals new { p1 = t3.ProductId ?? 0, DU1 = t3.ToUnitId, U1 = t3.UnitConversionForId } into table3
                            from tab3 in table3.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.ProdOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.ProdOrderHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1.Contains(p.Dimension1Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2.Contains(p.Dimension2Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension3Id) ? 1 == 1 : Dimension3.Contains(p.Dimension3Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension4Id) ? 1 == 1 : Dimension4.Contains(p.Dimension4Id.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                            && tab1.ProcessId == joborder.ProcessId
                            && tab.DocDate <= joborder.DocDate
                            && (string.IsNullOrEmpty(Settings.filterContraSites) ? p.SiteId == joborder.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                            && (string.IsNullOrEmpty(Settings.filterProductCategories) ? 1 == 1 : ProductCategoryIdArr.Contains(tab2.ProductCategoryId.ToString()))
                            && (string.IsNullOrEmpty(Settings.filterContraDivisions) ? p.DivisionId == joborder.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                            && p.BalanceQty > 0
                            orderby tab.DocDate, tab.DocNo, tab1.Sr
                            select new JobOrderLineViewModel
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
                                //UnitConversionMultiplier =  (tab3 == null ? 1 : tab3.ToQty / tab3.FromQty),
                                UnitConversionMultiplier = Math.Round( (tab3 == null ? 1 : tab3.ToQty / tab3.FromQty), (tab3 == null ? tab2.Unit.DecimalPlaces : Dealunit.DecimalPlaces)),
                                UnitConversionException = tab3 == null ? true : false,
                                UnitDecimalPlaces = tab2.Unit.DecimalPlaces,
                                DealUnitDecimalPlaces = (tab3 == null ? tab2.Unit.DecimalPlaces : Dealunit.DecimalPlaces),
                                SalesTaxGroupProductId = tab2.SalesTaxGroupProductId ?? tab2.ProductGroup.DefaultSalesTaxGroupProductId,
                                SalesTaxGroupPersonId = joborder.SalesTaxGroupPersonId,
                            }

                        );
                return temp;
            }
            else
            {
                var temp = (from p in db.ViewProdOrderBalance
                            join t in db.ProdOrderHeader on p.ProdOrderHeaderId equals t.ProdOrderHeaderId into table
                            from tab in table.DefaultIfEmpty()
                            join product in db.Product on p.ProductId equals product.ProductId into table2
                            from tab2 in table2.DefaultIfEmpty()
                            join t1 in db.ProdOrderLine on p.ProdOrderLineId equals t1.ProdOrderLineId into table1
                            from tab1 in table1.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.ProdOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.ProdOrderHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1.Contains(p.Dimension1Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2.Contains(p.Dimension2Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension3Id) ? 1 == 1 : Dimension3.Contains(p.Dimension3Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension4Id) ? 1 == 1 : Dimension4.Contains(p.Dimension4Id.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                            && tab1.ProcessId == joborder.ProcessId
                            && (string.IsNullOrEmpty(Settings.filterProductCategories) ? 1 == 1 : ProductCategoryIdArr.Contains(tab2.ProductCategoryId.ToString()))
                            && p.BalanceQty > 0
                            select new JobOrderLineViewModel
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
                                SalesTaxGroupProductId = tab2.SalesTaxGroupProductId ?? tab2.ProductGroup.DefaultSalesTaxGroupProductId,
                                SalesTaxGroupPersonId = joborder.SalesTaxGroupPersonId,
                            }

                        );
                return temp;
            }

        }



        public IEnumerable<JobOrderLineViewModel> GetJobOrderForStockInFilters(JobOrderLineFilterForStockInViewModel vm)
        {
            byte? UnitConvForId = new JobOrderHeaderService(_unitOfWork).Find(vm.JobOrderHeaderId).UnitConversionForId;

            var joborder = new JobOrderHeaderService(_unitOfWork).Find(vm.JobOrderHeaderId);

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
            if (!string.IsNullOrEmpty(vm.StockInHeaderId)) { SaleOrderIdArr = vm.StockInHeaderId.Split(",".ToCharArray()); }
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


            string[] ProductCategoryIdArr = null;
            if (!string.IsNullOrEmpty(Settings.filterProductCategories)) { ProductCategoryIdArr = Settings.filterProductCategories.Split(",".ToCharArray()); }
            else { ProductCategoryIdArr = new string[] { "NA" }; }


            if (!string.IsNullOrEmpty(vm.DealUnitId))
            {
                Unit Dealunit = new UnitService(_unitOfWork).Find(vm.DealUnitId);

                var temp = (from L in db.ViewStockInBalance
                            join S in db.Stock on L.StockInId equals S.StockId into StockTable
                            from StockTab in StockTable.DefaultIfEmpty()
                            join Uc in db.UnitConversion on new { p1 = L.ProductId, DU1 = vm.DealUnitId, U1 = UnitConvForId ?? 0 } equals new { p1 = Uc.ProductId ?? 0, DU1 = Uc.ToUnitId, U1 = Uc.UnitConversionForId } into UnitConversionTable
                            from UnitConversionTab in UnitConversionTable.DefaultIfEmpty()
                            where StockTab.GodownId == joborder.GodownId
                            && (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(L.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.StockInHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(StockTab.StockHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1.Contains(L.Dimension1Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2.Contains(L.Dimension2Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension3Id) ? 1 == 1 : Dimension3.Contains(L.Dimension3Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension4Id) ? 1 == 1 : Dimension4.Contains(L.Dimension4Id.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(StockTab.Product.ProductGroupId.ToString()))
                            && StockTab.DocDate <= joborder.DocDate
                            && (string.IsNullOrEmpty(Settings.filterContraSites) ? L.SiteId == joborder.SiteId : ContraSites.Contains(L.SiteId.ToString()))
                            && (string.IsNullOrEmpty(Settings.filterProductCategories) ? 1 == 1 : ProductCategoryIdArr.Contains(StockTab.Product.ProductCategoryId.ToString()))
                            && (string.IsNullOrEmpty(Settings.filterContraDivisions) ? L.DivisionId == joborder.DivisionId : ContraDivisions.Contains(L.DivisionId.ToString()))
                            && L.BalanceQty > 0
                            orderby StockTab.DocDate, StockTab.StockHeader.DocNo
                            select new JobOrderLineViewModel
                            {
                                Dimension1Name = StockTab.Dimension1.Dimension1Name,
                                Dimension2Name = StockTab.Dimension2.Dimension2Name,
                                Dimension3Name = StockTab.Dimension3.Dimension3Name,
                                Dimension4Name = StockTab.Dimension4.Dimension4Name,
                                Dimension1Id = L.Dimension1Id,
                                Dimension2Id = L.Dimension2Id,
                                Dimension3Id = L.Dimension3Id,
                                Dimension4Id = L.Dimension4Id,
                                Specification = StockTab.Specification,
                                ProdOrderBalanceQty = L.BalanceQty,
                                Qty = L.BalanceQty,
                                Rate = vm.Rate,
                                ProductName = StockTab.Product.ProductName,
                                ProductId = L.ProductId,
                                JobOrderHeaderId = vm.JobOrderHeaderId,
                                StockInId = L.StockInId,
                                StockInNo = StockTab.StockHeader.DocNo,
                                FromProcessId = StockTab.ProcessId,
                                UnitId = StockTab.Product.UnitId,
                                LossQty = Settings.LossQty,
                                NonCountedQty = Settings.NonCountedQty,
                                DealUnitId = (vm.DealUnitId),
                                UnitConversionMultiplier = Math.Round((UnitConversionTab == null ? 1 : UnitConversionTab.ToQty / UnitConversionTab.FromQty), (UnitConversionTab == null ? StockTab.Product.Unit.DecimalPlaces : Dealunit.DecimalPlaces)),
                                UnitConversionException = UnitConversionTab == null ? true : false,
                                UnitDecimalPlaces = StockTab.Product.Unit.DecimalPlaces,
                                DealUnitDecimalPlaces = (UnitConversionTab == null ? StockTab.Product.Unit.DecimalPlaces : Dealunit.DecimalPlaces)
                            }

                        );
                return temp;
            }
            else
            {
                var temp = (from L in db.ViewStockInBalance
                            join S in db.Stock on L.StockInId equals S.StockId into StockTable
                            from StockTab in StockTable.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(L.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.StockInHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(StockTab.StockHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension1Id) ? 1 == 1 : Dimension1.Contains(L.Dimension1Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension2Id) ? 1 == 1 : Dimension2.Contains(L.Dimension2Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension3Id) ? 1 == 1 : Dimension3.Contains(L.Dimension3Id.ToString()))
                            && (string.IsNullOrEmpty(vm.Dimension4Id) ? 1 == 1 : Dimension4.Contains(L.Dimension4Id.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(StockTab.Product.ProductGroupId.ToString()))
                            && StockTab.DocDate <= joborder.DocDate
                            && (string.IsNullOrEmpty(Settings.filterContraSites) ? L.SiteId == joborder.SiteId : ContraSites.Contains(L.SiteId.ToString()))
                            && (string.IsNullOrEmpty(Settings.filterProductCategories) ? 1 == 1 : ProductCategoryIdArr.Contains(StockTab.Product.ProductCategoryId.ToString()))
                            && (string.IsNullOrEmpty(Settings.filterContraDivisions) ? L.DivisionId == joborder.DivisionId : ContraDivisions.Contains(L.DivisionId.ToString()))
                            && L.BalanceQty > 0
                            orderby StockTab.DocDate, StockTab.StockHeader.DocNo
                            select new JobOrderLineViewModel
                            {
                                Dimension1Name = StockTab.Dimension1.Dimension1Name,
                                Dimension2Name = StockTab.Dimension2.Dimension2Name,
                                Dimension3Name = StockTab.Dimension3.Dimension3Name,
                                Dimension4Name = StockTab.Dimension4.Dimension4Name,
                                Dimension1Id = L.Dimension1Id,
                                Dimension2Id = L.Dimension2Id,
                                Dimension3Id = L.Dimension3Id,
                                Dimension4Id = L.Dimension4Id,
                                Specification = StockTab.Specification,
                                ProdOrderBalanceQty = L.BalanceQty,
                                Qty = L.BalanceQty,
                                Rate = vm.Rate,
                                ProductName = StockTab.Product.ProductName,
                                ProductId = L.ProductId,
                                JobOrderHeaderId = vm.JobOrderHeaderId,
                                StockInId = L.StockInId,
                                StockInNo = StockTab.StockHeader.DocNo,
                                FromProcessId = StockTab.ProcessId,
                                UnitId = StockTab.Product.UnitId,
                                DealUnitId = StockTab.Product.UnitId,
                                UnitConversionMultiplier = 1,
                                UnitDecimalPlaces = StockTab.Product.Unit.DecimalPlaces,
                                DealUnitDecimalPlaces = StockTab.Product.Unit.DecimalPlaces,
                            }

                        );
                return temp;
            }

        }


        public IQueryable<ComboBoxResult> GetPendingProdOrderHelpList(int Id, string term)
        {
            var JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(Id);

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

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ProductDivision = null;
            if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { ProductDivision = settings.FilterProductDivision.Split(",".ToCharArray()); }
            else { ProductDivision = new string[] { "NA" }; }

            string[] ProductCategory = null;
            if (!string.IsNullOrEmpty(settings.filterProductCategories)) { ProductCategory = settings.filterProductCategories.Split(",".ToCharArray()); }
            else { ProductCategory = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var list = (from p in db.ViewProdOrderBalance
                        join t in db.Persons on p.BuyerId equals t.PersonID into table
                        from tab in table.DefaultIfEmpty()
                        join Pt in db.Product on p.ProductId equals Pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join Fp in db.FinishedProduct on p.ProductId equals Fp.ProductId into FinishedProductTable
                        from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProdOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(ProductTab.ProductGroup.ProductTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.FilterProductDivision) ? 1 == 1 : ProductDivision.Contains(ProductTab.DivisionId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProductCategories) ? 1 == 1 : ProductCategory.Contains(FinishedProductTab.ProductCategoryId.ToString()))
                        group new { p, tab.Code } by p.ProdOrderHeaderId into g
                        orderby g.Max(m => m.p.IndentDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.DocType.DocumentTypeShortName) + "-" + g.Max(m => m.p.ProdOrderNo) + " {" + g.Max(m => m.Code) + "}",
                            id = g.Key.ToString(),
                        });

            
             
            return list;
        }


        public IEnumerable<ProdOrderHeaderListViewModel> GetPendingProdOrdersWithPatternMatch(int Id, string term, int Limiter)
        {
            var JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(Id);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrderHeader.DocTypeId, JobOrderHeader.DivisionId, JobOrderHeader.SiteId);

            string settingProductTypes = "";
            string settingProductDivision = "";
            string settingProductCategory = "";

            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { settingProductTypes = "|" + settings.filterProductTypes.Replace(",", "|,|") + "|"; }
            if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { settingProductDivision = "|" + settings.FilterProductDivision.Replace(",", "|,|") + "|"; }
            if (!string.IsNullOrEmpty(settings.filterProductCategories)) { settingProductCategory = "|" + settings.filterProductCategories.Replace(",", "|,|") + "|"; }


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

            string[] ProductCategory = null;
            if (!string.IsNullOrEmpty(settings.filterProductCategories)) { ProductCategory = settingProductCategory.Split(",".ToCharArray()); }
            else { ProductCategory = new string[] { "NA" }; }


            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var list = (from p in db.ViewProdOrderBalance
                        join Pt in db.Product on p.ProductId equals Pt.ProductId into ProductTable from ProductTab in ProductTable.DefaultIfEmpty()
                        join Fp in db.FinishedProduct on p.ProductId equals Fp.ProductId into FinishedProductTable
                        from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
                        where (
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.ProdOrderNo.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower())
                        ) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains("|" + ProductTab.ProductGroup.ProductTypeId.ToString() + "|"))
                        && (string.IsNullOrEmpty(settings.FilterProductDivision) ? 1 == 1 : ProductDivision.Contains("|" + ProductTab.DivisionId.ToString() + "|"))
                        && (string.IsNullOrEmpty(settings.filterProductCategories) ? 1 == 1 : ProductCategory.Contains("|" + FinishedProductTab.ProductCategoryId.ToString() + "|"))
                        orderby p.ProdOrderNo
                        select new ProdOrderHeaderListViewModel
                        {
                            DocNo = p.ProdOrderNo,
                            ProdOrderLineId = p.ProdOrderLineId,
                            ProductName = p.Product.ProductName,
                            Dimension1Name = p.Dimension1.Dimension1Name,
                            Dimension2Name = p.Dimension2.Dimension2Name,
                            Dimension3Name = p.Dimension3.Dimension3Name,
                            Dimension4Name = p.Dimension4.Dimension4Name,
                        }).Take(Limiter);

            return (list);
        }

        //public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        //{
        //    var JobOrder = new JobOrderHeaderService(_unitOfWork).Find(Id);

        //    var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrder.DocTypeId, JobOrder.DivisionId, JobOrder.SiteId);

        //    string settingProductTypes = "";
        //    string settingProductDivision = "";
        //    string settingProductCategory = "";


        //    if (!string.IsNullOrEmpty(settings.filterProductTypes)) { settingProductTypes = "|" + settings.filterProductTypes.Replace(",", "|,|") + "|"; }
        //    if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { settingProductDivision = "|" + settings.FilterProductDivision.Replace(",", "|,|") + "|"; }
        //    if (!string.IsNullOrEmpty(settings.filterProductCategories)) { settingProductCategory = "|" + settings.filterProductCategories.Replace(",", "|,|") + "|"; }


        //    string[] ProductTypes = null;
        //    if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settingProductTypes.Split(",".ToCharArray()); }
        //    else { ProductTypes = new string[] { "NA" }; }

        //    string[] ProductDivision = null;
        //    if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { ProductDivision = settingProductDivision.Split(",".ToCharArray()); }
        //    else { ProductDivision = new string[] { "NA" }; }

        //    string[] ProductCategory = null;
        //    if (!string.IsNullOrEmpty(settings.filterProductCategories)) { ProductCategory = settingProductCategory.Split(",".ToCharArray()); }
        //    else { ProductCategory = new string[] { "NA" }; }


        //    var list = (from p in db.Product
        //                join Fp in db.FinishedProduct on p.ProductId equals Fp.ProductId into FinishedProductTable from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
        //                where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
        //                && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains("|" + p.ProductGroup.ProductTypeId.ToString() + "|"))
        //                && (string.IsNullOrEmpty(settings.FilterProductDivision) ? 1 == 1 : ProductDivision.Contains("|" + p.DivisionId.ToString() + "|"))
        //                && (string.IsNullOrEmpty(settings.filterProductCategories) ? 1 == 1 : ProductCategory.Contains("|" + FinishedProductTab.ProductCategoryId.ToString() + "|"))
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
            var JobOrder = new JobOrderHeaderService(_unitOfWork).Find(Id);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrder.DocTypeId, JobOrder.DivisionId, JobOrder.SiteId);

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
            var Max = (from p in db.JobOrderLine
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

            var Site = new SiteService(_unitOfWork).Find(SiteId);
            var JobOrder = Find(id);

            var JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrder.JobOrderHeaderId);

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

            var temp = from p in db.ProductUid
                       where p.ProductUidHeaderId == JobOrder.ProductUidHeaderId && p.ProductId == JobOrder.ProductId && (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductUidName.ToLower().Contains(term.ToLower()))
                       && p.CurrenctGodownId == Site.DefaultGodownId && (!(p.ProcessesDone ?? "").Contains("|" + JobOrderHeader.ProcessId + "|"))
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

            IEnumerable<JobRate> RateList = db.Database.SqlQuery<JobRate>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetJobOrderRate @JobOrderHeaderId, @ProductId ", SQLJobOrderHeaderId, SQLProductID).ToList();

            return RateList;
        }


        public decimal GetUnitConversionForProdOrderLine(int ProdLineId, byte UnitConvForId, string DealUnitId)
        {
            var Query = (from t1 in db.ProdOrderLine
                         join t2 in db.Product on t1.ProductId equals t2.ProductId
                         join t3 in db.UnitConversion on new { p1 = t1.ProductId, DU1 = DealUnitId, U1 = UnitConvForId } equals new { p1 = t3.ProductId ?? 0, DU1 = t3.ToUnitId, U1 = t3.UnitConversionForId } into table3
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
            var JobOrder = new JobOrderHeaderService(_unitOfWork).Find(Id);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrder.DocTypeId, JobOrder.DivisionId, JobOrder.SiteId);

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

        public IQueryable<ComboBoxResult> GetProdOrderHelpListForProduct(int Id, string term)
        {
            var JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(Id);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrderHeader.DocTypeId, JobOrderHeader.DivisionId, JobOrderHeader.SiteId);

            string settingProductTypes = "";
            string settingProductDivision = "";
            string settingProductCategory = "";

            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { settingProductTypes = "|" + settings.filterProductTypes.Replace(",", "|,|") + "|"; }
            if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { settingProductDivision = "|" + settings.FilterProductDivision.Replace(",", "|,|") + "|"; }
            if (!string.IsNullOrEmpty(settings.filterProductCategories)) { settingProductCategory = "|" + settings.filterProductCategories.Replace(",", "|,|") + "|"; }


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

            string[] ProductCategory = null;
            if (!string.IsNullOrEmpty(settings.filterProductCategories)) { ProductCategory = settingProductCategory.Split(",".ToCharArray()); }
            else { ProductCategory = new string[] { "NA" }; }


            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];



            var list = (from p in db.ViewProdOrderBalance
                        join Pol in db.ProdOrderLine on p.ProdOrderLineId equals Pol.ProdOrderLineId into ProdOrderLineTable from ProdOrderLineTab in ProdOrderLineTable.DefaultIfEmpty()
                        join Pt in db.Product on p.ProductId equals Pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join Fp in db.FinishedProduct on p.ProductId equals Fp.ProductId into FinishedProductTable
                        from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
                        where (
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.ProdOrderNo.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower())
                        ) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains("|" + ProductTab.ProductGroup.ProductTypeId.ToString() + "|"))
                        && (string.IsNullOrEmpty(settings.FilterProductDivision) ? 1 == 1 : ProductDivision.Contains("|" + ProductTab.DivisionId.ToString() + "|"))
                        && (string.IsNullOrEmpty(settings.filterProductCategories) ? 1 == 1 : ProductCategory.Contains("|" + FinishedProductTab.ProductCategoryId.ToString() + "|"))
                        //&& ProdOrderLineTab.ProcessId == JobOrderHeader.ProcessId
                        orderby p.ProdOrderNo
                        select new ComboBoxResult
                        {
                            text = ProductTab.ProductName,
                            id = p.ProdOrderLineId.ToString(),
                            TextProp1 = "Prod Order No: " + p.ProdOrderNo.ToString(),
                            TextProp2 = "BalQty: " + p.BalanceQty.ToString(),
                            AProp1 = p.Dimension1.Dimension1Name + (string.IsNullOrEmpty(p.Dimension1.Dimension1Name) ? "" : ",") + p.Dimension2.Dimension2Name,
                            AProp2 = p.Dimension3.Dimension3Name + (string.IsNullOrEmpty(p.Dimension3.Dimension3Name) ? "" : ",") + p.Dimension4.Dimension4Name,
                        });

            return list;
        }

        public IEnumerable<ComboBoxResult> GetPendingProdOrderForProductUid(int JobOrderHeaderId, int ProductUidId, string term)
        {
            var JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderHeaderId);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrderHeader.DocTypeId, JobOrderHeader.DivisionId, JobOrderHeader.SiteId);

            var ProductUid = new ProductUidService(_unitOfWork).Find(ProductUidId);

            string settingProductTypes = "";
            string settingProductDivision = "";
            string settingProductCategory = "";

            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { settingProductTypes = "|" + settings.filterProductTypes.Replace(",", "|,|") + "|"; }
            if (!string.IsNullOrEmpty(settings.FilterProductDivision)) { settingProductDivision = "|" + settings.FilterProductDivision.Replace(",", "|,|") + "|"; }
            if (!string.IsNullOrEmpty(settings.filterProductCategories)) { settingProductCategory = "|" + settings.filterProductCategories.Replace(",", "|,|") + "|"; }


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

            string[] ProductCategory = null;
            if (!string.IsNullOrEmpty(settings.filterProductCategories)) { ProductCategory = settingProductCategory.Split(",".ToCharArray()); }
            else { ProductCategory = new string[] { "NA" }; }


            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];



            var list = (from p in db.ViewProdOrderBalance
                        join Pt in db.Product on p.ProductId equals Pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join Fp in db.FinishedProduct on p.ProductId equals Fp.ProductId into FinishedProductTable
                        from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
                        where (
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.ProdOrderNo.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower()) ||
                        string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower())
                        ) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains("|" + ProductTab.ProductGroup.ProductTypeId.ToString() + "|"))
                        && (string.IsNullOrEmpty(settings.FilterProductDivision) ? 1 == 1 : ProductDivision.Contains("|" + ProductTab.DivisionId.ToString() + "|"))
                        && (string.IsNullOrEmpty(settings.filterProductCategories) ? 1 == 1 : ProductCategory.Contains("|" + FinishedProductTab.ProductCategoryId.ToString() + "|"))
                        && p.ProductId == ProductUid.ProductId
                        && p.Dimension1Id == ProductUid.Dimension1Id
                        && p.Dimension2Id == ProductUid.Dimension2Id
                        && p.Dimension3Id == ProductUid.Dimension3Id
                        && p.Dimension4Id == ProductUid.Dimension4Id
                        orderby p.ProdOrderNo
                        select new ComboBoxResult
                        {
                            text = ProductTab.ProductName,
                            id = p.ProdOrderLineId.ToString(),
                            TextProp1 = "Prod Order No: " + p.ProdOrderNo.ToString(),
                            TextProp2 = "BalQty: " + p.BalanceQty.ToString(),
                            AProp1 = p.Dimension1.Dimension1Name + (string.IsNullOrEmpty(p.Dimension1.Dimension1Name) ? "" : ",") + p.Dimension2.Dimension2Name,
                            AProp2 = p.Dimension3.Dimension3Name + (string.IsNullOrEmpty(p.Dimension3.Dimension3Name) ? "" : ",") + p.Dimension4.Dimension4Name,
                        });
            return list;
        }

        public IEnumerable<ComboBoxResult> FGetProductUidHelpList(int Id, string term)
        {
            var JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(Id);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrderHeader.DocTypeId, JobOrderHeader.DivisionId, JobOrderHeader.SiteId);

            SqlParameter SQLJobOrderHeaderId = new SqlParameter("@JobOrderHeaderId", Id);
            IEnumerable<ComboBoxResult> ProductUidList = db.Database.SqlQuery<ComboBoxResult>(settings.SqlProcProductUidHelpList + " @JobOrderHeaderId", SQLJobOrderHeaderId).ToList();


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

        public IEnumerable<ComboBoxResult> GetPendingStockInForIssue(int JobOrderHeaderId, int? ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, string term)
        {

            var JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderHeaderId);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrderHeader.DocTypeId, JobOrderHeader.DivisionId, JobOrderHeader.SiteId);


            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            return (from p in db.ViewStockInBalance
                    join L in db.Stock on p.StockInId equals L.StockId into StockTable
                    from StockTab in StockTable.DefaultIfEmpty()
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
                    where p.BalanceQty > 0 && StockTab.GodownId == JobOrderHeader.GodownId
                    && (ProductId == null || ProductId == 0 ? 1 == 1 : p.ProductId == ProductId)
                    && (Dimension1Id == null ? 1 == 1 : p.Dimension1Id == Dimension1Id)
                    && (Dimension2Id == null ? 1 == 1 : p.Dimension2Id == Dimension2Id)
                    && (Dimension3Id == null ? 1 == 1 : p.Dimension3Id == Dimension3Id)
                    && (Dimension4Id == null ? 1 == 1 : p.Dimension4Id == Dimension4Id)
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.StockInNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : StockTab.StockHeader.DocType.DocumentTypeShortName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : ProductTab.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension1Tab.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension2Tab.Dimension2Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension3Tab.Dimension3Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : Dimension4Tab.Dimension4Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : StockTab.LotNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : StockTab.ProductUid.ProductUidName.ToLower().Contains(term.ToLower())
                        )
                    select new ComboBoxResult
                    {
                        id = p.StockInId.ToString(),
                        text = StockTab.StockHeader.DocType.DocumentTypeShortName + "-" + p.StockInNo,
                        TextProp1 = "Balance :" + p.BalanceQty,
                        TextProp2 = "Date :" + p.StockInDate,
                        AProp1 = ProductTab.ProductName + ((StockTab.ProductUid.ProductUidName == null) ? "" : "," + StockTab.ProductUid.ProductUidName),
                        AProp2 = ((Dimension1Tab.Dimension1Name == null) ? "" : Dimension1Tab.Dimension1Name) +
                                    ((Dimension2Tab.Dimension2Name == null) ? "" : "," + Dimension2Tab.Dimension2Name) +
                                    ((Dimension3Tab.Dimension3Name == null) ? "" : "," + Dimension3Tab.Dimension3Name) +
                                    ((Dimension4Tab.Dimension4Name == null) ? "" : "," + Dimension4Tab.Dimension4Name) + 
                                    ((p.LotNo == null) ? "" : "," + p.LotNo)
                    });
        }


        public IEnumerable<ComboBoxResult> GetPendingStockInHeaderForIssue(int JobOrderHeaderId, string term)
        {
            var JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderHeaderId);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrderHeader.DocTypeId, JobOrderHeader.DivisionId, JobOrderHeader.SiteId);

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            return (from p in db.ViewStockInBalance
                    join L in db.Stock on p.StockInId equals L.StockId into StockTable
                    from StockTab in StockTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && StockTab.GodownId == JobOrderHeader.GodownId
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.StockInNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : StockTab.StockHeader.DocType.DocumentTypeShortName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : StockTab.StockHeader.Process.ProcessName.ToLower().Contains(term.ToLower())
                    )
                    group new { p, StockTab } by new { StockTab.StockHeaderId } into Result
                    select new ComboBoxResult
                    {
                        id = Result.Key.StockHeaderId.ToString(),
                        text = Result.Max(i => i.StockTab.StockHeader.DocType.DocumentTypeShortName + "-" + i.StockTab.StockHeader.DocNo),
                        TextProp1 = "Date :" + Result.Max(i => i.StockTab.StockHeader.DocDate),
                        TextProp2 = "Balance :" + Result.Sum(i => i.p.BalanceQty),
                        AProp1 = "Process :" + Result.Max(i => i.StockTab.StockHeader.Process.ProcessName),
                    });
        }

        public Decimal? GetExcessReceiveAllowedAgainstOrderQty(int JobOrderLineId)
        {
            Decimal? ExcessAllowedQty = null;


            var JobOrder = (from L in db.JobOrderLine
                            where L.JobOrderLineId == JobOrderLineId
                            select new
                            {
                                SiteId = L.JobOrderHeader.SiteId,
                                DivisionId = L.JobOrderHeader.DivisionId,
                                ProcessId = L.JobOrderHeader.ProcessId,
                                ProductId = L.ProductId,
                                OrderQty = L.Qty
                            }).FirstOrDefault();

            if (JobOrder != null)
            {
                var ProductSiteDetail = (from Ps in db.ProductSiteDetail where Ps.ProductId == JobOrder.ProductId && Ps.SiteId == JobOrder.SiteId && Ps.DivisionId == JobOrder.DivisionId && Ps.ProcessId == JobOrder.ProcessId select Ps).FirstOrDefault();
                if (ProductSiteDetail != null)
                {
                    if (ProductSiteDetail.ExcessReceiveAllowedAgainstOrderPer != null || ProductSiteDetail.ExcessReceiveAllowedAgainstOrderQty != null)
                    {
                        Decimal ExcessAllowedWithPer = JobOrder.OrderQty * (ProductSiteDetail.ExcessReceiveAllowedAgainstOrderPer ?? 0) / 100;
                        Decimal ExcessAllowedWithQty = ProductSiteDetail.ExcessReceiveAllowedAgainstOrderQty ?? 0;

                        if (ExcessAllowedWithPer > ExcessAllowedWithQty)
                            ExcessAllowedQty = ExcessAllowedWithPer;
                        else
                            ExcessAllowedQty = ExcessAllowedWithQty;
                    }
                }
            }

            return ExcessAllowedQty;
        }

        public void Dispose()
        {
        }
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
