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
using Model.ViewModel;
using Model.ViewModels;
using System.Data.SqlClient;

namespace Service
{
    public interface IJobInvoiceLineService : IDisposable
    {
        JobInvoiceLine Create(JobInvoiceLine pt);
        void Delete(int id);
        void Delete(JobInvoiceLine pt);
        JobInvoiceLine Find(int id);
        IEnumerable<JobInvoiceLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobInvoiceLine pt);
        JobInvoiceLine Add(JobInvoiceLine pt);
        IEnumerable<JobInvoiceLine> GetJobInvoiceLineList();
        IEnumerable<JobInvoiceLineIndexViewModel> GetLineListForIndex(int HeaderId);
        Task<IEquatable<JobInvoiceLine>> GetAsync();
        Task<JobInvoiceLine> FindAsync(int id);
        JobInvoiceLineViewModel GetJobInvoiceLine(int id);
        JobInvoiceLineViewModel GetJobInvoiceReceiveLine(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<JobInvoiceLineViewModel> GetJobReceiptForFilters(JobInvoiceLineFilterViewModel vm);
        IEnumerable<JobInvoiceLineViewModel> GetJobOrderForFilters(JobInvoiceLineFilterViewModel vm);
        IEnumerable<JobInvoiceLineViewModel> GetJobOrderForFiltersForInvoiceReceive(JobInvoiceLineFilterViewModel vm);
        JobInvoiceLine FindByJobInvoiceHeader(int id);
        IEnumerable<ComboBoxList> GetPendingProductsForJobInvoice(int Jid, string term);
        IQueryable<ComboBoxResult> GetPendingJobWorkersForJobInvoice(int Id, string term);
        IEnumerable<ComboBoxList> GetPendingJobReceive(int Jid, string term);
        IEnumerable<ComboBoxList> GetPendingJobOrders(int Jid, string term);
        IEnumerable<JobReceiveProductHelpList> GetProductHelpList(int Id, int? JobWorkerId, string term, int Limit);
        IEnumerable<JobReceiveProductHelpList> GetProductHelpListForPendingJobOrders(int Id, int JobWorkerId, string term, int Limit);
        IEnumerable<JobReceiveProductHelpList> GetProductHelpListForPendingTraceMapJobOrders(int Id, int JobWorkerId, string term, int Limit);
        ComboBoxPagedResult GetPendingProductsForJobInvoice(string searchTerm, int pageSize, int pageNum, int filter);
        ComboBoxPagedResult GetPendingJobOrdersForInvoice(string searchTerm, int pageSize, int pageNum, int filter);
        ComboBoxPagedResult GetPendingJobReceivesForInvoice(string searchTerm, int pageSize, int pageNum, int filter);
        int GetMaxSr(int id);
        IEnumerable<ComboBoxResult> FGetProductUidHelpList(int Id, string term);
        IEnumerable<ComboBoxResult> GetJobOrderHelpListForProduct(int Id, string term);
        IEnumerable<ComboBoxResult> GetJobReceiveHelpListForProduct(int Id, string term);
        IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term);
        JobReceiveLineViewModel GetReceiveLineDetailForInvoice(int id, int InvoiceId);
    }

    public class JobInvoiceLineService : IJobInvoiceLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobInvoiceLine> _JobInvoiceLineRepository;
        RepositoryQuery<JobInvoiceLine> JobInvoiceLineRepository;
        public JobInvoiceLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobInvoiceLineRepository = new Repository<JobInvoiceLine>(db);
            JobInvoiceLineRepository = new RepositoryQuery<JobInvoiceLine>(_JobInvoiceLineRepository);
        }

        public JobInvoiceLine FindByJobInvoiceHeader(int id)
        {
            return (from p in db.JobInvoiceLine
                    where p.JobInvoiceHeaderId == id
                    select p).FirstOrDefault();
        }
        public JobInvoiceLine Find(int id)
        {
            return _unitOfWork.Repository<JobInvoiceLine>().Find(id);
        }

        public JobInvoiceLine Create(JobInvoiceLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobInvoiceLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobInvoiceLine>().Delete(id);
        }

        public void Delete(JobInvoiceLine pt)
        {
            _unitOfWork.Repository<JobInvoiceLine>().Delete(pt);
        }
        public IEnumerable<JobInvoiceLineViewModel> GetJobReceiptForFilters(JobInvoiceLineFilterViewModel vm)
        {

            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(vm.JobInvoiceHeaderId);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { ContraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { ContraDocTypes = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            string[] ContraProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ContraProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ContraProductTypes = new string[] { "NA" }; }

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.JobReceiveHeaderId)) { SaleOrderIdArr = vm.JobReceiveHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] JobWorkerIdArr = null;
            if (!string.IsNullOrEmpty(vm.JobWorkerIds)) { JobWorkerIdArr = vm.JobWorkerIds.Split(",".ToCharArray()); }
            else { JobWorkerIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var query = (from p in db.ViewJobReceiveBalanceForInvoice
                         join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId into table
                         from tab in table.DefaultIfEmpty()
                         join jw in db.Persons on tab.JobWorkerId equals jw.PersonID
                         join t1 in db.JobReceiveLine on p.JobReceiveLineId equals t1.JobReceiveLineId into table1
                         from tab1 in table1.DefaultIfEmpty()
                         join Qa in db.JobReceiveQALine on tab1.JobReceiveLineId equals Qa.JobReceiveLineId into JobReceiveQALineTable from JobReceiveQALineTab in JobReceiveQALineTable.DefaultIfEmpty()
                         join t4 in db.JobOrderLine on tab1.JobOrderLineId equals t4.JobOrderLineId into table4
                         from tab4 in table4.DefaultIfEmpty()
                         join product in db.Product on p.ProductId equals product.ProductId into table2
                         from tab2 in table2.DefaultIfEmpty()
                         where
                             //(string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                             //&& (string.IsNullOrEmpty(vm.JobReceiveHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.JobReceiveHeaderId.ToString()))
                             //&& (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                             //&& (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == JobInvoice.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                             //&& (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : ContraDocTypes.Contains(p.DocTypeId.ToString()))
                             //&& (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == JobInvoice.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                             //&& (vm.AsOnDate.HasValue ? tab.DocDate <= vm.AsOnDate.Value : 1 == 1)
                             //&& (JobInvoice.JobWorkerId.HasValue ? p.JobWorkerId == JobInvoice.JobWorkerId : 1 == 1)
                          p.BalanceQty > 0 && tab.ProcessId == settings.ProcessId && tab.Status == (int)StatusConstants.Submitted
                         orderby tab1.JobReceiveHeader.DocDate, tab1.JobReceiveHeader.DocNo, tab1.Sr
                         select new
                         {
                             //Filter Projections
                             JobReceiveHeaderId = p.JobReceiveHeaderId,
                             ProductGroupId = tab2.ProductGroupId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             DocTypeId = p.DocTypeId,
                             DocDate = tab.DocDate,
                             ProductTypeId = tab2.ProductGroup.ProductTypeId,

                             //Data Projections
                             Dimension1Name = tab4.Dimension1.Dimension1Name,
                             Dimension2Name = tab4.Dimension2.Dimension2Name,
                             Dimension3Name = tab4.Dimension3.Dimension3Name,
                             Dimension4Name = tab4.Dimension4.Dimension4Name,
                             Specification = tab4.Specification,
                             ReceiptBalQty = p.BalanceQty,
                             Qty = p.BalanceQty,
                             JobReceiveDocNo = tab.DocNo,
                             ProductName = tab2.ProductName,
                             ProductId = p.ProductId,
                             JobReceiveLineId = p.JobReceiveLineId,
                             UnitId = tab2.UnitId,
                             UnitName = tab2.Unit.UnitName,
                             DealUnitId = tab4.DealUnitId,
                             DealUnitName = tab4.DealUnit.UnitName,
                             JobWorkerId = tab.JobWorkerId,
                             JobWorkerName = jw.Name,
                             //DealQty = p.BalanceQty * tab4.UnitConversionMultiplier,
                             DealQty = JobReceiveQALineTab.JobReceiveQALineId != null ? JobReceiveQALineTab.DealQty :  tab4.UnitConversionMultiplier * p.BalanceQty,
                             Rate = tab4.Rate,
                             UnitConversionMultiplier = tab4.UnitConversionMultiplier,
                             UnitDecimalPlaces = tab2.Unit.DecimalPlaces,
                             DealUnitDecimalPlaces = tab4.DealUnit.DecimalPlaces,
                             CostCenterId = tab4.JobOrderHeader.CostCenterId,
                         });

            if (!string.IsNullOrEmpty(vm.ProductId))
                query = query.Where(m => ProductIdArr.Contains(m.ProductId.ToString()));

            if (!string.IsNullOrEmpty(vm.JobReceiveHeaderId))
                query = query.Where(m => SaleOrderIdArr.Contains(m.JobReceiveHeaderId.ToString()));

            if (JobInvoice.JobWorkerId.HasValue && settings.isVisibleHeaderJobWorker == true)
                query = query.Where(m => m.JobWorkerId == JobInvoice.JobWorkerId);
            else if (!string.IsNullOrEmpty(vm.JobWorkerIds))
                query = query.Where(m => JobWorkerIdArr.Contains(m.JobWorkerId.ToString()));

            if (!string.IsNullOrEmpty(vm.ProductGroupId))
                query = query.Where(m => ProductGroupIdArr.Contains(m.ProductGroupId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterProductTypes))
                query = query.Where(m => ContraProductTypes.Contains(m.ProductTypeId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                query = query.Where(m => ContraSites.Contains(m.SiteId.ToString()));
            else
                query = query.Where(m => m.SiteId == JobInvoice.SiteId);

            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                query = query.Where(m => ContraDivisions.Contains(m.DivisionId.ToString()));
            else
                query = query.Where(m => m.DivisionId == JobInvoice.DivisionId);

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                query = query.Where(m => ContraDocTypes.Contains(m.DocTypeId.ToString()));

            if (vm.ReceiveAsOnDate.HasValue)
                query = query.Where(m => m.DocDate <= vm.ReceiveAsOnDate.Value);


            return (from p in query
                    select new JobInvoiceLineViewModel
                         {
                             Dimension1Name = p.Dimension1Name,
                             Dimension2Name = p.Dimension2Name,
                             Dimension3Name = p.Dimension3Name,
                             Dimension4Name = p.Dimension4Name,
                             Specification = p.Specification,
                             ReceiptBalQty = p.ReceiptBalQty,
                             Qty = p.Qty,
                             JobReceiveDocNo = p.JobReceiveDocNo,
                             ProductName = p.ProductName,
                             ProductId = p.ProductId,
                             JobInvoiceHeaderId = vm.JobInvoiceHeaderId,
                             JobReceiveLineId = p.JobReceiveLineId,
                             UnitId = p.UnitId,
                             UnitName = p.UnitName,
                             DealUnitId = p.DealUnitId,
                             DealUnitName = p.DealUnitName,
                             JobWorkerId = p.JobWorkerId,
                             JobWorkerName = p.JobWorkerName,
                             DealQty = p.DealQty,
                             Rate = p.Rate,
                             UnitConversionMultiplier = p.UnitConversionMultiplier,
                             UnitDecimalPlaces = p.UnitDecimalPlaces,
                             DealUnitDecimalPlaces = p.DealUnitDecimalPlaces,
                             CostCenterId = p.CostCenterId,
                         }).ToList();


            //return temp;
        }
        public IEnumerable<JobInvoiceLineViewModel> GetJobOrderForFilters(JobInvoiceLineFilterViewModel vm)
        {

            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(vm.JobInvoiceHeaderId);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            string[] ContraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { ContraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { ContraDocTypes = new string[] { "NA" }; }

            string[] ContraProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ContraProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ContraProductTypes = new string[] { "NA" }; }

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.JobOrderHeaderId)) { SaleOrderIdArr = vm.JobOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] JobWorkerIdArr = null;
            if (!string.IsNullOrEmpty(vm.JobWorkerIds)) { JobWorkerIdArr = vm.JobWorkerIds.Split(",".ToCharArray()); }
            else { JobWorkerIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            //ToChange View to get Joborders instead of goodsreceipts
            var query = (from p in db.ViewJobReceiveBalanceForInvoice
                         join t2 in db.JobOrderHeader on p.JobOrderHeaderId equals t2.JobOrderHeaderId into table5
                         from tabl2 in table5.DefaultIfEmpty()
                         join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId into table
                         from tab in table.DefaultIfEmpty()
                         join product in db.Product on p.ProductId equals product.ProductId into table2
                         from tab2 in table2.DefaultIfEmpty()
                         join t1 in db.JobReceiveLine on p.JobReceiveLineId equals t1.JobReceiveLineId into table1
                         from tab1 in table1.DefaultIfEmpty()
                         join Qa in db.JobReceiveQALine on tab1.JobReceiveLineId equals Qa.JobReceiveLineId into JobReceiveQALineTable
                         from JobReceiveQALineTab in JobReceiveQALineTable.DefaultIfEmpty()
                         join t3 in db.JobOrderLine on tab1.JobOrderLineId equals t3.JobOrderLineId into table3
                         from tab3 in table3.DefaultIfEmpty()

                         where
                             //(string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                             //&& (string.IsNullOrEmpty(vm.JobOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.JobOrderHeaderId.ToString()))
                             //&& (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                             // && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == JobInvoice.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                             //&& (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == JobInvoice.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                             //&& (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : ContraDocTypes.Contains(p.DocTypeId.ToString()))
                             //&& (vm.AsOnDate.HasValue ? tab.DocDate <= vm.AsOnDate.Value : 1 == 1)
                             //&& (JobInvoice.JobWorkerId.HasValue ? p.JobWorkerId == JobInvoice.JobWorkerId : 1 == 1)
                             //&&
                         p.BalanceQty > 0 && tab.ProcessId == settings.ProcessId && tab.Status == (int)StatusConstants.Submitted
                         orderby tabl2.DocDate, tabl2.DocNo, tab.DocDate, tab.DocNo, tab3.Sr
                         select new
                         {

                             //Filter Projections
                             JobReceiveHeaderId = p.JobReceiveHeaderId,
                             ProductGroupId = tab2.ProductGroupId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             DocTypeId = p.DocTypeId,
                             DocDate = tab.DocDate,
                             ProductTypeId = tab2.ProductGroup.ProductTypeId,

                             //Data Projections
                             Dimension1Name = tab3.Dimension1.Dimension1Name,
                             Dimension2Name = tab3.Dimension2.Dimension2Name,
                             Dimension3Name = tab3.Dimension3.Dimension3Name,
                             Dimension4Name = tab3.Dimension4.Dimension4Name,
                             Specification = tab3.Specification,
                             ReceiptBalQty = p.BalanceQty,
                             Qty = p.BalanceQty,
                             CostCenterId = tabl2.CostCenterId,
                             JobReceiveDocNo = tab.DocNo,
                             ProductName = tab2.ProductName,
                             ProductId = p.ProductId,
                             JobInvoiceHeaderId = vm.JobInvoiceHeaderId,
                             JobReceiveLineId = p.JobReceiveLineId,
                             UnitId = tab2.UnitId,
                             UnitName = tab2.Unit.UnitName,
                             DealUnitId = tab3.DealUnitId,
                             DealUnitName = tab3.DealUnit.UnitName,
                             JobWorkerId = tab.JobWorkerId,
                             //DealQty = p.BalanceQty * tab3.UnitConversionMultiplier,
                             DealQty = JobReceiveQALineTab.JobReceiveQALineId != null ? JobReceiveQALineTab.DealQty : tab3.UnitConversionMultiplier * p.BalanceQty,
                             Rate = tab3.Rate,
                             UnitConversionMultiplier = tab3.UnitConversionMultiplier,
                             JobOrderDocNo = tabl2.DocNo,
                             UnitDecimalPlaces = tab2.Unit.DecimalPlaces,
                             DealUnitDecimalPlaces = tab3.DealUnit.DecimalPlaces,
                         }
                        );


            if (!string.IsNullOrEmpty(vm.ProductId))
                query = query.Where(m => ProductIdArr.Contains(m.ProductId.ToString()));

            if (!string.IsNullOrEmpty(vm.JobReceiveHeaderId))
                query = query.Where(m => SaleOrderIdArr.Contains(m.JobReceiveHeaderId.ToString()));

            if (JobInvoice.JobWorkerId.HasValue && settings.isVisibleHeaderJobWorker == true)
                query = query.Where(m => m.JobWorkerId == JobInvoice.JobWorkerId);
            else if (!string.IsNullOrEmpty(vm.JobWorkerIds))
                query = query.Where(m => JobWorkerIdArr.Contains(m.JobWorkerId.ToString()));

            if (!string.IsNullOrEmpty(vm.ProductGroupId))
                query = query.Where(m => ProductGroupIdArr.Contains(m.ProductGroupId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterProductTypes))
                query = query.Where(m => ContraProductTypes.Contains(m.ProductTypeId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                query = query.Where(m => ContraSites.Contains(m.SiteId.ToString()));
            else
                query = query.Where(m => m.SiteId == JobInvoice.SiteId);

            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                query = query.Where(m => ContraDivisions.Contains(m.DivisionId.ToString()));
            else
                query = query.Where(m => m.DivisionId == JobInvoice.DivisionId);

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                query = query.Where(m => ContraDocTypes.Contains(m.DocTypeId.ToString()));

            if (vm.ReceiveAsOnDate.HasValue)
                query = query.Where(m => m.DocDate <= vm.ReceiveAsOnDate.Value);


            return (from p in query
                    select new JobInvoiceLineViewModel
                    {
                        Dimension1Name = p.Dimension1Name,
                        Dimension2Name = p.Dimension2Name,
                        Dimension3Name = p.Dimension3Name,
                        Dimension4Name = p.Dimension4Name,
                        Specification = p.Specification,
                        ReceiptBalQty = p.ReceiptBalQty,
                        Qty = p.Qty,
                        CostCenterId = p.CostCenterId,
                        JobReceiveDocNo = p.JobReceiveDocNo,
                        ProductName = p.ProductName,
                        ProductId = p.ProductId,
                        JobInvoiceHeaderId = vm.JobInvoiceHeaderId,
                        JobReceiveLineId = p.JobReceiveLineId,
                        UnitId = p.UnitId,
                        UnitName = p.UnitName,
                        DealUnitId = p.DealUnitId,
                        DealUnitName = p.UnitName,
                        JobWorkerId = p.JobWorkerId,
                        DealQty = p.DealQty,
                        Rate = p.Rate,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        JobOrderDocNo = p.JobOrderDocNo,
                        UnitDecimalPlaces = p.UnitDecimalPlaces,
                        DealUnitDecimalPlaces = p.DealUnitDecimalPlaces,
                    }).ToList();
        }

        public IEnumerable<JobInvoiceLineViewModel> GetJobOrderForFiltersForInvoiceReceive(JobInvoiceLineFilterViewModel vm)
        {

            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(vm.JobInvoiceHeaderId);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            string[] ContraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { ContraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { ContraDocTypes = new string[] { "NA" }; }

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] JobOrderHeaderIdArr = null;
            if (!string.IsNullOrEmpty(vm.JobOrderHeaderId)) { JobOrderHeaderIdArr = vm.JobOrderHeaderId.Split(",".ToCharArray()); }
            else { JobOrderHeaderIdArr = new string[] { "NA" }; }

            string[] JobReceiveHeaderIdArr = null;
            if (!string.IsNullOrEmpty(vm.JobReceiveHeaderId)) { JobReceiveHeaderIdArr = vm.JobReceiveHeaderId.Split(",".ToCharArray()); }
            else { JobReceiveHeaderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }


            if ((settings.isVisibleJobReceive ?? false) == true)
            {
                var temp = (from VJRBal in db.ViewJobReceiveBalance
                            join H in db.JobReceiveHeader on VJRBal.JobReceiveHeaderId equals H.JobReceiveHeaderId into JobReceiveHeaderTable
                            from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                            join P in db.Product on VJRBal.ProductId equals P.ProductId into ProductTable
                            from ProductTab in ProductTable.DefaultIfEmpty()
                            join L in db.JobReceiveLine on VJRBal.JobReceiveLineId equals L.JobReceiveLineId into JobReceiveLineTable
                            from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(VJRBal.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.JobReceiveHeaderId) ? 1 == 1 : JobReceiveHeaderIdArr.Contains(VJRBal.JobReceiveHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(ProductTab.ProductGroupId.ToString()))
                            && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : ContraDocTypes.Contains(JobReceiveHeaderTab.DocTypeId.ToString()))
                            && (string.IsNullOrEmpty(settings.filterContraSites) ? VJRBal.SiteId == JobInvoice.SiteId : ContraSites.Contains(VJRBal.SiteId.ToString()))
                            && (string.IsNullOrEmpty(settings.filterContraDivisions) ? VJRBal.DivisionId == JobInvoice.DivisionId : ContraDivisions.Contains(VJRBal.DivisionId.ToString()))
                            && (JobInvoice.JobWorkerId == null ? 1 == 1 : VJRBal.JobWorkerId == vm.JobWorkerId)
                            && (vm.ReceiveAsOnDate == null ? 1 == 1 : JobReceiveHeaderTab.DocDate <= vm.ReceiveAsOnDate)
                            && JobReceiveLineTab.ProductUidHeaderId == null
                            && VJRBal.BalanceQty > 0
                            orderby JobReceiveHeaderTab.DocDate, JobReceiveHeaderTab.DocNo, JobReceiveLineTab.Sr
                            select new JobInvoiceLineViewModel
                            {
                                Dimension1Name = JobReceiveLineTab.Dimension1.Dimension1Name,
                                Dimension2Name = JobReceiveLineTab.Dimension2.Dimension2Name,
                                Dimension3Name = JobReceiveLineTab.Dimension3.Dimension3Name,
                                Dimension4Name = JobReceiveLineTab.Dimension4.Dimension4Name,
                                Specification = JobReceiveLineTab.Specification,
                                ReceiptBalQty = VJRBal.BalanceQty,
                                OrderBalanceQty = VJRBal.BalanceQty,
                                Qty = VJRBal.BalanceQty,
                                JobQty = VJRBal.BalanceQty,
                                ReceiveQty = VJRBal.BalanceQty,
                                CostCenterId = JobReceiveLineTab.JobOrderLine.JobOrderHeader.CostCenterId,
                                PassQty = VJRBal.BalanceQty,
                                ProductName = ProductTab.ProductName,
                                ProductId = VJRBal.ProductId,
                                JobInvoiceHeaderId = vm.JobInvoiceHeaderId,
                                JobOrderLineId = JobReceiveLineTab.JobOrderLineId,
                                JobOrderDocNo = JobReceiveLineTab.JobOrderLine.JobOrderHeader.DocNo,
                                JobReceiveLineId = JobReceiveLineTab.JobReceiveLineId,
                                JobReceiveDocNo = JobReceiveLineTab.JobReceiveHeader.DocNo,
                                ProductUidId = JobReceiveLineTab.ProductUidId,
                                ProductUidName = JobReceiveLineTab.ProductUid.ProductUidName,
                                UnitId = ProductTab.UnitId,
                                UnitName = ProductTab.Unit.UnitName,
                                DealUnitId = JobReceiveLineTab.DealUnitId,
                                DealUnitName = JobReceiveLineTab.DealUnit.UnitName,
                                JobWorkerId = VJRBal.JobWorkerId,
                                DealQty = VJRBal.BalanceQty * JobReceiveLineTab.UnitConversionMultiplier,
                                Rate = JobReceiveLineTab.JobOrderLine.Rate,
                                UnitConversionMultiplier = JobReceiveLineTab.UnitConversionMultiplier,
                                OrderDocDate = JobReceiveLineTab.JobOrderLine.JobOrderHeader.DocDate,
                                UnitDecimalPlaces = ProductTab.Unit.DecimalPlaces,
                                DealUnitDecimalPlaces = JobReceiveLineTab.DealUnit.DecimalPlaces,
                                SalesTaxGroupProductId = JobReceiveLineTab.Product.SalesTaxGroupProductId ?? JobReceiveLineTab.Product.ProductGroup.DefaultSalesTaxGroupProductId,
                                SalesTaxGroupPersonId = JobInvoice.SalesTaxGroupPersonId,
                            });
                return temp;
            }
            else
            {
                var temp = (from VJOBal in db.ViewJobOrderBalance
                            join H in db.JobOrderHeader on VJOBal.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
                            from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                            join P in db.Product on VJOBal.ProductId equals P.ProductId into ProductTable
                            from ProductTab in ProductTable.DefaultIfEmpty()
                            join L in db.JobOrderLine on VJOBal.JobOrderLineId equals L.JobOrderLineId into JobOrderLineTable
                            from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(VJOBal.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.JobOrderHeaderId) ? 1 == 1 : JobOrderHeaderIdArr.Contains(VJOBal.JobOrderHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(ProductTab.ProductGroupId.ToString()))
                            && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : ContraDocTypes.Contains(JobOrderHeaderTab.DocTypeId.ToString()))
                            && (string.IsNullOrEmpty(settings.filterContraSites) ? VJOBal.SiteId == JobInvoice.SiteId : ContraSites.Contains(VJOBal.SiteId.ToString()))
                            && (string.IsNullOrEmpty(settings.filterContraDivisions) ? VJOBal.DivisionId == JobInvoice.DivisionId : ContraDivisions.Contains(VJOBal.DivisionId.ToString()))
                            && (JobInvoice.JobWorkerId == null ? 1 == 1 : VJOBal.JobWorkerId == vm.JobWorkerId)
                            && JobOrderLineTab.ProductUidHeaderId == null
                            && VJOBal.BalanceQty > 0
                            orderby JobOrderHeaderTab.DocDate, JobOrderHeaderTab.DocNo, JobOrderLineTab.Sr
                            select new JobInvoiceLineViewModel
                            {
                                Dimension1Name = JobOrderLineTab.Dimension1.Dimension1Name,
                                Dimension2Name = JobOrderLineTab.Dimension2.Dimension2Name,
                                Dimension3Name = JobOrderLineTab.Dimension3.Dimension3Name,
                                Dimension4Name = JobOrderLineTab.Dimension4.Dimension4Name,
                                Specification = JobOrderLineTab.Specification,
                                ReceiptBalQty = VJOBal.BalanceQty,
                                OrderBalanceQty = VJOBal.BalanceQty,
                                Qty = VJOBal.BalanceQty,
                                JobQty = VJOBal.BalanceQty,
                                ReceiveQty = VJOBal.BalanceQty,
                                CostCenterId = JobOrderHeaderTab.CostCenterId,
                                PassQty = VJOBal.BalanceQty,
                                ProductName = ProductTab.ProductName,
                                ProductId = VJOBal.ProductId,
                                JobInvoiceHeaderId = vm.JobInvoiceHeaderId,
                                JobOrderLineId = JobOrderLineTab.JobOrderLineId,
                                ProductUidId = JobOrderLineTab.ProductUidId,
                                ProductUidName = JobOrderLineTab.ProductUid.ProductUidName,
                                UnitId = ProductTab.UnitId,
                                UnitName = ProductTab.Unit.UnitName,
                                DealUnitId = JobOrderLineTab.DealUnitId,
                                DealUnitName = JobOrderLineTab.DealUnit.UnitName,
                                JobWorkerId = VJOBal.JobWorkerId,
                                DealQty = VJOBal.BalanceQty * JobOrderLineTab.UnitConversionMultiplier,
                                Rate = JobOrderLineTab.Rate,
                                UnitConversionMultiplier = JobOrderLineTab.UnitConversionMultiplier,
                                JobOrderDocNo = JobOrderHeaderTab.DocNo,
                                OrderDocDate = JobOrderHeaderTab.DocDate,
                                UnitDecimalPlaces = ProductTab.Unit.DecimalPlaces,
                                DealUnitDecimalPlaces = JobOrderLineTab.DealUnit.DecimalPlaces,
                                SalesTaxGroupProductId = JobOrderLineTab.Product.SalesTaxGroupProductId ?? JobOrderLineTab.Product.ProductGroup.DefaultSalesTaxGroupProductId,
                                SalesTaxGroupPersonId = JobInvoice.SalesTaxGroupPersonId,
                            });
                return temp;
            }
            
        }


        public void Update(JobInvoiceLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobInvoiceLine>().Update(pt);
        }

        public IEnumerable<JobInvoiceLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobInvoiceLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.JobInvoiceLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<JobInvoiceLineIndexViewModel> GetLineListForIndex(int HeaderId)
        {
            return (from L in db.JobInvoiceLine
                    join La in db.LedgerAccount on L.JobReceiveLine.ProductId equals La.ProductId into LedgerAccountTable
                    from LedgerAccountTab in LedgerAccountTable.DefaultIfEmpty()
                    //join Jrl in db.JobReceiveLine on L.JobReceiveLineId equals Jrl.JobReceiveLineId into table
                    //from tab in table.DefaultIfEmpty()
                    //join Jol in db.JobOrderLine on tab.JobOrderLineId equals Jol.JobOrderLineId into table1
                    //from tab1 in table1.DefaultIfEmpty()
                    //join t in db.JobOrderHeader on tab1.JobOrderHeaderId equals t.JobOrderHeaderId into table3
                    //from tab3 in table3.DefaultIfEmpty()
                    //join t in db.Product on tab1.ProductId equals t.ProductId into table2
                    //from tab2 in table2.DefaultIfEmpty()
                    where L.JobInvoiceHeaderId == HeaderId
                    orderby L.Sr
                    select new JobInvoiceLineIndexViewModel
                    {
                        ProductName = L.JobReceiveLine.Product.ProductName,
                        ProductGroupName = LedgerAccountTab.LedgerAccountGroup.LedgerAccountGroupName ?? L.JobReceiveLine.Product.ProductGroup.ProductGroupName,
                        Amount = L.Amount,
                        Rate = L.Rate,
                        Qty = L.Qty,
                        JobOrderDocNo = L.JobReceiveLine.JobOrderLine.JobOrderHeader.DocNo,
                        JobInvoiceLineId = L.JobInvoiceLineId,
                        UnitId = L.JobReceiveLine.Product.UnitId,
                        UnitName = L.JobReceiveLine.Product.Unit.UnitName,
                        UnitDecimalPlaces = L.JobReceiveLine.Product.Unit.DecimalPlaces,
                        ProductUidName = L.JobReceiveLine.ProductUid.ProductUidName,
                        Specification = L.JobReceiveLine.JobOrderLine.Specification,
                        Dimension1Name = L.JobReceiveLine.Dimension1.Dimension1Name,
                        Dimension2Name = L.JobReceiveLine.Dimension2.Dimension2Name,
                        Dimension3Name = L.JobReceiveLine.Dimension3.Dimension3Name,
                        Dimension4Name = L.JobReceiveLine.Dimension4.Dimension4Name,
                        LotNo = L.JobReceiveLine.LotNo,
                        JobReceiveHeaderDocNo = L.JobReceiveLine.JobReceiveHeader.DocNo,
                        JobOrderHeaderDocNo = L.JobReceiveLine.JobOrderLine.JobOrderHeader.DocNo,
                        DealQty = L.DealQty,
                        DealUnitId = L.DealUnitId,
                        DealUnitName = L.DealUnit.UnitName,
                        DealUnitDecimalPlaces = L.DealUnit.DecimalPlaces,
                        Remark = L.Remark,
                        OrderDocTypeId = L.JobReceiveLine.JobOrderLine.JobOrderHeader.DocTypeId,
                        ReceiptDocTypeId = L.JobReceiveLine.JobReceiveHeader.DocTypeId,
                        OrderHeaderId = L.JobReceiveLine.JobOrderLine.JobOrderHeaderId,
                        ReceiptHeaderId = L.JobReceiveLine.JobReceiveHeaderId,
                        OrderLineId = L.JobReceiveLine.JobOrderLineId,
                        ReceiptLineId = L.JobReceiveLineId,
                        IncentiveAmt = L.IncentiveAmt,
                        IncentiveRate = L.IncentiveRate,
                    });
        }
        public JobInvoiceLineViewModel GetJobInvoiceLine(int id)
        {
            return (from p in db.JobInvoiceLine
                    join t in db.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t3 in db.JobOrderLine on tab.JobOrderLineId equals t3.JobOrderLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t2 in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals t2.JobInvoiceHeaderId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t in db.JobReceiveHeader on tab.JobReceiveHeaderId equals t.JobReceiveHeaderId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.JobInvoiceLineId == id
                    select new JobInvoiceLineViewModel
                    {
                        Amount = p.Amount,
                        ProductId = tab3.ProductId,
                        ProductName = tab3.Product.ProductName,
                        JobReceiveLineId = p.JobReceiveLineId,
                        JobReceiveDocNo = tab1.DocNo,
                        JobInvoiceHeaderId = p.JobInvoiceHeaderId,
                        JobInvoiceLineId = p.JobInvoiceLineId,
                        Qty = p.Qty,
                        UnitId = tab3.Product.UnitId,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        Rate = p.Rate,
                        JobWorkerId = p.JobWorkerId,
                        DealUnitId = p.DealUnitId,
                        DealQty = p.DealQty,
                        Dimension1Id = tab3.Dimension1Id,
                        Dimension1Name = tab3.Dimension1.Dimension1Name,
                        Dimension2Id = tab3.Dimension2Id,
                        Dimension2Name = tab3.Dimension2.Dimension2Name,
                        Dimension3Id = tab3.Dimension3Id,
                        Dimension3Name = tab3.Dimension3.Dimension3Name,
                        Dimension4Id = tab3.Dimension4Id,
                        Dimension4Name = tab3.Dimension4.Dimension4Name,
                        Specification = tab3.Specification,
                        SalesTaxGroupProductId = p.SalesTaxGroupProductId,
                        Remark = p.Remark,
                        LockReason = p.LockReason,
                    }
                        ).FirstOrDefault();
        }

        public JobInvoiceLineViewModel GetJobInvoiceReceiveLine(int id)
        {
            return (from L in db.JobInvoiceLine
                    //join t in db.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId into table
                    //from tab in table.DefaultIfEmpty()
                    join VJoBal in db.ViewJobOrderBalance on L.JobReceiveLine.JobOrderLineId equals VJoBal.JobOrderLineId into ViewJobOrderBalanceTable
                    from ViewJobOrderBalanceTab in ViewJobOrderBalanceTable.DefaultIfEmpty()
                    //join t3 in db.JobOrderLine on tab.JobOrderLineId equals t3.JobOrderLineId into table3
                    //from tab3 in table3.DefaultIfEmpty()
                    //join t2 in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals t2.JobInvoiceHeaderId into table2
                    //from tab2 in table2.DefaultIfEmpty()
                    //join t in db.JobReceiveHeader on tab.JobReceiveHeaderId equals t.JobReceiveHeaderId into table1
                    //from tab1 in table1.DefaultIfEmpty()
                    where L.JobInvoiceLineId == id
                    select new JobInvoiceLineViewModel
                    {
                        Amount = L.Amount,
                        ProductId = L.JobReceiveLine.ProductId,
                        ProductName = L.JobReceiveLine.Product.ProductName,
                        ProductUidId = L.JobReceiveLine.ProductUidId,
                        ProductUidName = L.JobReceiveLine.ProductUid.ProductUidName,
                        JobReceiveDocNo = L.JobReceiveLine.JobReceiveHeader.DocNo,
                        JobReceiveLineId = L.JobReceiveLineId,
                        JobReceiveHeaderId = L.JobReceiveLine.JobReceiveHeaderId,
                        JobOrderDocNo = L.JobReceiveLine.JobOrderLine.JobOrderHeader.DocNo,
                        JobOrderLineId = L.JobReceiveLine.JobOrderLineId,
                        OrderBalanceQty = (ViewJobOrderBalanceTab.BalanceQty == null ? 0 : ViewJobOrderBalanceTab.BalanceQty) + L.JobReceiveLine.Qty + L.JobReceiveLine.LossQty,
                        UnitDecimalPlaces = L.JobReceiveLine.Product.Unit.DecimalPlaces,
                        DealUnitDecimalPlaces = L.DealUnit.DecimalPlaces,
                        LossQty = L.JobReceiveLine.LossQty,
                        LotNo = L.JobReceiveLine.LotNo,
                        PassQty = L.Qty,
                        JobQty = L.JobReceiveLine.Qty + L.JobReceiveLine.LossQty,
                        ReceiveQty = L.JobReceiveLine.Qty,
                        Remark = L.Remark,
                        JobInvoiceHeaderId = L.JobInvoiceHeaderId,
                        JobInvoiceLineId = L.JobInvoiceLineId,
                        Qty = L.Qty,
                        UnitId = L.JobReceiveLine.Product.UnitId,
                        UnitConversionMultiplier = L.UnitConversionMultiplier,
                        Rate = L.Rate,
                        JobWorkerId = L.JobWorkerId,
                        DealUnitId = L.DealUnitId,
                        DealQty = L.DealQty,
                        Dimension1Id = L.JobReceiveLine.Dimension1Id,
                        Dimension1Name = L.JobReceiveLine.Dimension1.Dimension1Name,
                        Dimension2Id = L.JobReceiveLine.Dimension2Id,
                        Dimension2Name = L.JobReceiveLine.Dimension2.Dimension2Name,
                        Dimension3Id = L.JobReceiveLine.Dimension3Id,
                        Dimension3Name = L.JobReceiveLine.Dimension3.Dimension3Name,
                        Dimension4Id = L.JobReceiveLine.Dimension4Id,
                        Dimension4Name = L.JobReceiveLine.Dimension4.Dimension4Name,
                        Specification = L.JobReceiveLine.ProductUid.ProductUidSpecification ?? L.JobReceiveLine.JobOrderLine.Specification,
                        Weight = L.JobReceiveLine.Weight,
                        PenaltyAmt = L.JobReceiveLine.PenaltyAmt,
                        IncentiveAmt = L.JobReceiveLine.IncentiveAmt ?? 0,
                        IncentiveRate = L.JobReceiveLine.IncentiveRate,
                        PenaltyRate = L.JobReceiveLine.PenaltyRate,
                        RateDiscountPer = L.RateDiscountPer,
                        RateDiscountAmt = L.RateDiscountAmt,
                        MfgDate = L.JobReceiveLine.MfgDate,
                        LockReason = L.LockReason,
                        SalesTaxGroupPersonId = L.JobInvoiceHeader.SalesTaxGroupPersonId,
                        SalesTaxGroupProductId = L.SalesTaxGroupProductId,
                        ProductNatureName = L.JobReceiveLine.Product.ProductGroup.ProductType.ProductNature.ProductNatureName
                    }).FirstOrDefault();
        }

        //public JobInvoiceLineViewModel GetJobInvoiceLineBalance(int id)
        //{
        //    return (from b in db.ViewJobInvoiceBalance
        //            join p in db.JobInvoiceLine on b.JobInvoiceLineId equals p.JobInvoiceLineId 
        //            join t in db.JobGoodsReceiptLine on p.JobGoodsReceiptLineId equals t.JobGoodsReceiptLineId into table
        //            from tab in table.DefaultIfEmpty()
        //            join t2 in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals t2.JobInvoiceHeaderId into table2
        //            from tab2 in table2.DefaultIfEmpty()
        //            join t in db.JobGoodsReceiptHeader on tab.JobGoodsReceiptHeaderId equals t.JobGoodsReceiptHeaderId into table1
        //            from tab1 in table1.DefaultIfEmpty()

        //            where p.JobInvoiceLineId == id
        //            select new JobInvoiceLineViewModel
        //            {

        //                SupplierId = tab2.SupplierId,
        //                Amount = p.Amount,
        //                ProductId = tab.ProductId,
        //                JobGoodsReceiptLineId = p.JobGoodsReceiptLineId,
        //                JobGoodsRecieptHeaderDocNo = tab1.DocNo,
        //                JobInvoiceHeaderId = p.JobInvoiceHeaderId,
        //                JobInvoiceLineId = p.JobInvoiceLineId,
        //                Qty = b.BalanceQty,
        //                Rate = p.Rate,
        //                Remark = p.Remark,
        //                UnitConversionMultiplier = p.UnitConversionMultiplier,
        //                DealUnitId = p.DealUnitId,
        //                DealQty = p.DealQty,
        //                UnitId = tab.Product.UnitId,
        //                Dimension1Id = tab.Dimension1Id,
        //                Dimension1Name = tab.Dimension1.Dimension1Name,
        //                Dimension2Id = tab.Dimension2Id,
        //                Dimension2Name = tab.Dimension2.Dimension2Name,
        //                Specification = tab.Specification,
        //                LotNo = tab.LotNo,

        //            }
        //                ).FirstOrDefault();
        //}

        public IEnumerable<JobInvoiceLine> GetJobInvoiceLineList()
        {
            var pt = _unitOfWork.Repository<JobInvoiceLine>().Query().Get().OrderBy(m => m.JobInvoiceLineId);

            return pt;
        }

        public JobInvoiceLine Add(JobInvoiceLine pt)
        {
            _unitOfWork.Repository<JobInvoiceLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobInvoiceLine
                        orderby p.JobInvoiceLineId
                        select p.JobInvoiceLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceLine
                        orderby p.JobInvoiceLineId
                        select p.JobInvoiceLineId).FirstOrDefault();
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

                temp = (from p in db.JobInvoiceLine
                        orderby p.JobInvoiceLineId
                        select p.JobInvoiceLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceLine
                        orderby p.JobInvoiceLineId
                        select p.JobInvoiceLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<ComboBoxList> GetPendingProductsForJobInvoice(int Jid, string term)//DocTypeId
        {

            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(Jid);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            return (from p in db.ViewJobReceiveBalanceForInvoice
                    join t in db.Product on p.ProductId equals t.ProductId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && ProTab.ProductName.ToLower().Contains(term.ToLower()) && (JobInvoice.JobWorkerId.HasValue ? p.JobWorkerId == JobInvoice.JobWorkerId : 1 == 1)
                     && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == JobInvoice.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                     && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == JobInvoice.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                    group new { p, ProTab } by p.ProductId into g
                    orderby g.Key descending
                    select new ComboBoxList
                    {
                        Id = g.Key,
                        PropFirst = g.Max(m => m.ProTab.ProductName)
                    }
                        ).Take(20);
        }

        public IEnumerable<ComboBoxList> GetPendingJobReceive(int Jid, string term)//DocTypeId
        {

            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(Jid);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            return (from p in db.ViewJobReceiveBalanceForInvoice
                    join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && (ProTab.DocNo.ToLower().Contains(term.ToLower()) || ProTab.JobWorkerDocNo.ToLower().Contains(term.ToLower())) && (JobInvoice.JobWorkerId.HasValue ? p.JobWorkerId == JobInvoice.JobWorkerId : 1 == 1)
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == JobInvoice.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == JobInvoice.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                    && ProTab.Status == (int)StatusConstants.Submitted
                    group new { p, ProTab } by p.JobReceiveHeaderId into g
                    select new ComboBoxList
                    {
                        Id = g.Key,
                        PropFirst = g.Max(m => m.ProTab.DocNo + " | " + m.ProTab.JobWorkerDocNo),
                    }
                        ).Take(20);
        }

        public IEnumerable<ComboBoxList> GetPendingJobOrders(int Jid, string term)//DocTypeId
        {

            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(Jid);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }


            return (from p in db.ViewJobReceiveBalanceForInvoice
                    join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && ProTab.DocNo.ToLower().Contains(term.ToLower()) && p.JobWorkerId == JobInvoice.JobWorkerId
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == JobInvoice.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == JobInvoice.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                    group new { p, ProTab } by p.JobOrderHeaderId into g
                    select new ComboBoxList
                    {
                        Id = g.Key,
                        PropFirst = g.Max(m => m.ProTab.DocNo),
                    }
                        ).Take(20);
        }

        public IQueryable<ComboBoxResult> GetPendingJobWorkersForJobInvoice(int Id, string term)//DocTypeId
        {

            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            string[] ContraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { ContraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { ContraDocTypes = new string[] { "NA" }; }

            var query = (from p in db.ViewJobReceiveBalanceForInvoice
                         join t in db.JobWorker on p.JobWorkerId equals t.PersonID into table
                         from tab in table.DefaultIfEmpty()
                         join per in db.Persons on tab.PersonID equals per.PersonID
                         where p.BalanceQty > 0
                         select new
                         {
                             JobWorkerId = p.JobWorkerId,
                             JobWorkerName = per.Name,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             DocTypeId = p.DocTypeId,
                         }
                        );

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                query = query.Where(m => ContraSites.Contains(m.SiteId.ToString()));
            else
                query = query.Where(m => m.SiteId == JobInvoice.SiteId);

            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                query = query.Where(m => ContraDivisions.Contains(m.DivisionId.ToString()));
            else
                query = query.Where(m => m.DivisionId == JobInvoice.DivisionId);

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                query = query.Where(m => ContraDocTypes.Contains(m.DocTypeId.ToString()));

            return (from p in query
                    group p by p.JobWorkerId into g
                    orderby g.Max(m => m.JobWorkerId)
                    select new ComboBoxResult
                    {
                        id = g.Key.ToString(),
                        text = g.Max(m => m.JobWorkerName),
                    });


        }

        public IEnumerable<JobReceiveProductHelpList> GetProductHelpList(int Id, int? JobWorkerId, string term, int Limit)
        {
            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);


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

            var query = (from p in db.ViewJobReceiveBalanceForInvoice
                         join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
                         join rl in db.JobReceiveLine on p.JobReceiveLineId equals rl.JobReceiveLineId
                         join prod in db.Product on p.ProductId equals prod.ProductId
                         join pg in db.ProductGroups on prod.ProductGroupId equals pg.ProductGroupId
                         join uid in db.ProductUid on rl.ProductUidId equals uid.ProductUIDId into table
                         from uidtab in table.DefaultIfEmpty()
                         where t.Status == (int)StatusConstants.Submitted
                         orderby t.DocDate, t.DocNo
                         select new
                         {
                             ProductName = p.Product.ProductName,
                             ProductId = p.ProductId,
                             Specification = p.JobOrderLine.Specification,
                             Dimension1Name = p.Dimension1.Dimension1Name,
                             Dimension2Name = p.Dimension2.Dimension2Name,
                             Dimension3Name = p.Dimension3.Dimension3Name,
                             Dimension4Name = p.Dimension4.Dimension4Name,
                             JobOrderNo = p.JobOrderNo,
                             JobReceiveNo = t.DocNo,
                             JobReceiveDocNo =  t.JobWorkerDocNo,
                             JobReceiveLineId = p.JobReceiveLineId,
                             Qty = p.BalanceQty,
                             ProductType = pg.ProductTypeId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             JobWorkerId = p.JobWorkerId,
                             ProductUidName = uidtab.ProductUidName,
                             DocTypeId = p.DocTypeId,
                         });

            if (!string.IsNullOrEmpty(settings.filterProductTypes))
                query = query.Where(m => ProductTypes.Contains(m.ProductType.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                query = query.Where(m => ContraSites.Contains(m.SiteId.ToString()));
            else
                query = query.Where(m => m.SiteId == JobInvoice.SiteId);

            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                query = query.Where(m => ContraDivisions.Contains(m.DivisionId.ToString()));
            else
                query = query.Where(m => m.DivisionId == JobInvoice.DivisionId);

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                query = query.Where(m => ContraDocTypes.Contains(m.DocTypeId.ToString()));

            if (JobWorkerId.HasValue && JobWorkerId.Value > 0)
                query = query.Where(m => m.JobWorkerId == JobWorkerId);

            if (!string.IsNullOrEmpty(term))
                query = query.Where(m => m.ProductName.ToLower().Contains(term.ToLower())
                 || m.Specification.ToLower().Contains(term.ToLower())
                 || m.Dimension1Name.ToLower().Contains(term.ToLower())
                 || m.Dimension2Name.ToLower().Contains(term.ToLower())
                 || m.Dimension3Name.ToLower().Contains(term.ToLower())
                 || m.Dimension4Name.ToLower().Contains(term.ToLower())
                 || m.JobOrderNo.ToLower().Contains(term.ToLower())
                 || m.JobReceiveDocNo.ToLower().Contains(term.ToLower())
                 || m.JobReceiveNo.ToLower().Contains(term.ToLower())
                 || m.ProductUidName.ToLower().Contains(term.ToLower())
                 );



            var list = query.Take(Limit);

            return (from p in query
                    select new JobReceiveProductHelpList
                    {
                        ProductName = p.ProductName,
                        ProductId = p.ProductId,
                        Specification = p.Specification,
                        Dimension1Name = p.Dimension1Name,
                        Dimension2Name = p.Dimension2Name,
                        Dimension3Name = p.Dimension3Name,
                        Dimension4Name = p.Dimension4Name,
                        JobOrderNo = p.JobOrderNo,
                        JobReceiveNo = p.JobReceiveNo,
                        JobReceiveDocNo = p.JobReceiveDocNo,
                        JobReceiveLineId = p.JobReceiveLineId,
                        Qty = p.Qty,
                        ProductType = p.ProductType,
                        SiteId = p.SiteId,
                        DivisionId = p.DivisionId,
                        JobWorkerId = p.JobWorkerId,
                        ProductUidName = p.ProductUidName
                    }).Take(Limit).ToList();
        }

        public IEnumerable<JobReceiveProductHelpList> GetProductHelpListForPendingJobOrders(int Id, int JobWorkerId, string term, int Limit)
        {
            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);


            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewJobOrderBalance
                        join t2 in db.JobOrderLine on p.JobOrderLineId equals t2.JobOrderLineId
                        join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : t2.Specification.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.JobOrderNo.ToLower().Contains(term.ToLower())
                        )
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.Product.ProductGroup.ProductTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == JobInvoice.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == JobInvoice.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                        && p.JobWorkerId == JobWorkerId && p.BalanceQty > 0
                        orderby t.DocDate, t.DocNo
                        select new JobReceiveProductHelpList
                        {
                            ProductName = p.Product.ProductName,
                            ProductId = p.ProductId,
                            Specification = t2.Specification,
                            Dimension1Name = p.Dimension1.Dimension1Name,
                            Dimension2Name = p.Dimension2.Dimension2Name,
                            Dimension3Name = p.Dimension3.Dimension3Name,
                            Dimension4Name = p.Dimension4.Dimension4Name,
                            JobOrderNo = p.JobOrderNo,
                            JobOrderLineId = p.JobOrderLineId,
                            Qty = p.BalanceQty,
                        }
                          ).Take(Limit);

            return list.ToList();
        }

        public IEnumerable<JobReceiveProductHelpList> GetProductHelpListForPendingTraceMapJobOrders(int Id, int JobWorkerId, string term, int Limit)//Product Id
        {

            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);


            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewJobOrderBalanceFromStatus
                        join t2 in db.JobOrderLine on p.JobOrderLineId equals t2.JobOrderLineId
                        join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                        join prod in db.Product on p.ProductId equals prod.ProductId
                        join d1 in db.Dimension1 on p.Dimension1Id equals d1.Dimension1Id into dim1table
                        from dim1tab in dim1table.DefaultIfEmpty()
                        join d2 in db.Dimension2 on p.Dimension2Id equals d2.Dimension2Id into dim2table
                        from dim2tab in dim2table.DefaultIfEmpty()
                        join d3 in db.Dimension3 on p.Dimension3Id equals d3.Dimension3Id into dim3table
                        from dim3tab in dim3table.DefaultIfEmpty()
                        join d4 in db.Dimension4 on p.Dimension4Id equals d4.Dimension4Id into dim4table
                        from dim4tab in dim4table.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : prod.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : t2.Specification.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : dim1tab.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : dim2tab.Dimension2Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : dim3tab.Dimension3Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : dim4tab.Dimension4Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.JobOrderNo.ToLower().Contains(term.ToLower())
                        )
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.Product.ProductGroup.ProductTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == JobInvoice.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == JobInvoice.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                        && p.JobWorkerId == JobWorkerId && p.BalanceQty > 0
                        orderby t.DocDate, t.DocNo
                        select new JobReceiveProductHelpList
                        {
                            ProductName = prod.ProductName,
                            ProductId = p.ProductId,
                            Specification = t2.Specification,
                            Dimension1Name = dim1tab.Dimension1Name,
                            Dimension2Name = dim2tab.Dimension2Name,
                            Dimension3Name = dim3tab.Dimension3Name,
                            Dimension4Name = dim4tab.Dimension4Name,
                            JobOrderNo = p.JobOrderNo,
                            JobOrderLineId = p.JobOrderLineId,
                            Qty = p.BalanceQty,
                        }
                          ).Take(Limit);

            return list.ToList();
        }

        public ComboBoxPagedResult GetPendingProductsForJobInvoice(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(filter);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

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

            var Query = (from p in db.ViewJobOrderBalance
                         join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                         join t2 in db.JobOrderLine on p.JobOrderLineId equals t2.JobOrderLineId
                         join prod in db.Product on p.ProductId equals prod.ProductId
                         join pg in db.ProductGroups on prod.ProductGroupId equals pg.ProductGroupId into pgtable
                         from pgtab in pgtable.DefaultIfEmpty()
                         where p.BalanceQty > 0 &&
                         t2.ProductUidHeaderId == null &&
                         p.JobWorkerId == JobInvoice.JobWorkerId
                         select new
                         {
                             ProductName = prod.ProductName,
                             Id = p.ProductId,
                             ProductTypeId = pgtab.ProductTypeId,
                             SiteId = t.SiteId,
                             DivisionId = t.DivisionId,
                             DocType = t.DocTypeId,
                         });

            //Filters
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => ContraDocTypes.Contains(m.DocType.ToString()));

            if (!string.IsNullOrEmpty(settings.filterProductTypes))
                Query = Query.Where(m => ProductTypes.Contains(m.ProductTypeId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => ContraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == JobInvoice
                    .SiteId);

            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => ContraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == JobInvoice.DivisionId);

            if (!string.IsNullOrEmpty(searchTerm))
                Query = Query.Where(m => m.ProductName.ToLower().Contains(searchTerm.ToLower()));

            var GQuery = (from p in Query
                          group p by p.Id into g
                          select new
                          {
                              Id = g.Key,
                              DocNo = g.Max(m => m.ProductName),
                          });
            var Recods = GQuery.OrderBy(m => m.DocNo).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();
            var Count = GQuery.Count();

            return (new ComboBoxPagedResult
            {
                Results = Recods.Select(m => new ComboBoxResult { id = m.Id.ToString(), text = m.DocNo }).ToList(),
                Total = Count,
            });

        }

        public ComboBoxPagedResult GetPendingJobOrdersForInvoice(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(filter);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

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

            var Query = (from p in db.ViewJobOrderBalance
                         join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                         join t2 in db.JobOrderLine on p.JobOrderLineId equals t2.JobOrderLineId
                         where
                         p.BalanceQty > 0 &&
                         t2.ProductUidHeaderId == null
                         && (JobInvoice.JobWorkerId == null ? 1 == 1 : p.JobWorkerId == JobInvoice.JobWorkerId)
                         orderby t.DocDate, t.DocNo
                         select new
                         {
                             Id = p.JobOrderHeaderId,
                             DocNo = t.DocNo,
                             BalanceQty = p.BalanceQty,
                             Date = p.OrderDate,
                             DocTypeId = t.DocTypeId,
                             SiteId = t.SiteId,
                             DivisionId = t.DivisionId,
                         }
                          );

            //Filters
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => ContraDocTypes.Contains(m.DocTypeId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => ContraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == JobInvoice
                    .SiteId);

            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => ContraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == JobInvoice.DivisionId);

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


        public ComboBoxPagedResult GetPendingJobReceivesForInvoice(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(filter);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

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

            var Query = (from p in db.ViewJobReceiveBalance
                         join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
                         join t2 in db.JobReceiveLine on p.JobReceiveLineId equals t2.JobReceiveLineId
                         where
                         p.BalanceQty > 0 &&
                         t2.ProductUidHeaderId == null
                         && (JobInvoice.JobWorkerId == null ? 1 == 1 : p.JobWorkerId == JobInvoice.JobWorkerId)
                         orderby t.DocDate, t.DocNo
                         select new
                         {
                             Id = p.JobReceiveHeaderId,
                             DocNo = t.DocNo,
                             BalanceQty = p.BalanceQty,
                             Date = p.OrderDate,
                             DocTypeId = t.DocTypeId,
                             SiteId = t.SiteId,
                             DivisionId = t.DivisionId,
                         }
                          );

            //Filters
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => ContraDocTypes.Contains(m.DocTypeId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => ContraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == JobInvoice
                    .SiteId);

            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => ContraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == JobInvoice.DivisionId);

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

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.JobInvoiceLine
                       where p.JobInvoiceHeaderId == id
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

        public JobInvoiceRateAmendmentLineViewModel GetLineDetail(int id)
        {
            return (from p in db.JobInvoiceLine
                    join JR in db.JobReceiveLine on p.JobReceiveLineId equals JR.JobReceiveLineId
                    join JO in db.JobOrderLine on JR.JobOrderLineId equals JO.JobOrderLineId
                    join t2 in db.Product on JO.ProductId equals t2.ProductId
                    join D1 in db.Dimension1 on JO.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                    from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                    join D2 in db.Dimension2 on JO.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                    from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                    join D3 in db.Dimension3 on JO.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                    from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                    join D4 in db.Dimension4 on JO.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                    from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                    join t5 in db.JobWorker on p.JobWorkerId equals t5.PersonID
                    where p.JobInvoiceLineId == id
                    select new JobInvoiceRateAmendmentLineViewModel
                    {
                        Dimension1Name = Dimension1Tab.Dimension1Name,
                        Dimension2Name = Dimension2Tab.Dimension2Name,
                        Dimension3Name = Dimension3Tab.Dimension3Name,
                        Dimension4Name = Dimension4Tab.Dimension4Name,
                        LotNo = JR.LotNo,
                        Qty = p.Qty,
                        Specification = JO.Specification,
                        UnitId = t2.UnitId,
                        DealUnitId = p.DealUnitId,
                        DealQty = p.DealQty,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        UnitName = t2.Unit.UnitName,
                        DealUnitName = p.DealUnit.UnitName,
                        ProductId = JO.ProductId,
                        ProductName = t2.ProductName,
                        unitDecimalPlaces = t2.Unit.DecimalPlaces,
                        DealunitDecimalPlaces = p.DealUnit.DecimalPlaces,
                        JobWorkerId = p.JobWorkerId,
                        JobWorkerName = t5.Person.Name,
                        Rate = p.Rate,
                    }).FirstOrDefault();

        }

        public bool ValidateJobInvoice(int lineid, int headerid)
        {
            var temp = (from p in db.JobInvoiceRateAmendmentLine
                        where p.JobInvoiceLineId == lineid && p.JobInvoiceAmendmentHeaderId == headerid
                        select p).FirstOrDefault();
            if (temp != null)
                return false;
            else
                return true;

        }

        public IEnumerable<ComboBoxResult> FGetProductUidHelpList(int Id, string term)
        {
            var JobInvoiceHeader = new JobInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoiceHeader.DocTypeId, JobInvoiceHeader.DivisionId, JobInvoiceHeader.SiteId);

            SqlParameter SQLJobInvoiceHeaderId = new SqlParameter("@JobInvoiceHeaderId", Id);
            IEnumerable<ComboBoxResult> ProductUidList = db.Database.SqlQuery<ComboBoxResult>(settings.SqlProcProductUidHelpList + " @JobInvoiceHeaderId", SQLJobInvoiceHeaderId).ToList();


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

        public IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term)
        {
            var JobInvoice = new JobInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }


            return (from p in db.Product
                    join La in db.LedgerAccount on p.ProductId equals La.ProductId into LedgerAccountTable
                    from LedgerAccountTab in LedgerAccountTable.DefaultIfEmpty()
                    where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(p.ProductGroupId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductGroup.ProductGroupName.ToLower().Contains(term.ToLower()))
                    orderby p.ProductName
                    select new ComboBoxResult
                    {
                        id = p.ProductId.ToString(),
                        text = p.ProductName,
                        AProp1 = LedgerAccountTab.LedgerAccountGroup.LedgerAccountGroupName ??  p.ProductGroup.ProductGroupName,
                    });
        }
        public IEnumerable<ComboBoxResult> GetJobOrderHelpListForProduct(int Id, string term)
        {
            var JobInvoiceHeader = new JobInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoiceHeader.DocTypeId, JobInvoiceHeader.DivisionId, JobInvoiceHeader.SiteId);

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var OrderBalance = (from VB in db.ViewJobOrderBalance
                                where VB.JobWorkerId == JobInvoiceHeader.JobWorkerId
                                select new
                                {
                                    JobOrderLineId = VB.JobOrderLineId,
                                    BalanceQty = VB.BalanceQty
                                });

            return (from VB in OrderBalance
                    join L in db.JobOrderLine on VB.JobOrderLineId equals L.JobOrderLineId into JobOrderLineTable
                    from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                    where JobOrderLineTab.JobOrderHeader.JobWorkerId == JobInvoiceHeader.JobWorkerId
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? JobOrderLineTab.JobOrderHeader.Site.SiteId == CurrentSiteId : contraSites.Contains(JobOrderLineTab.JobOrderHeader.Site.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? JobOrderLineTab.JobOrderHeader.Division.DivisionId == CurrentDivisionId : contraDivisions.Contains(JobOrderLineTab.JobOrderHeader.Division.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.JobOrderHeader.DocNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.JobOrderHeader.DocType.DocumentTypeShortName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Product.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower())
                        )
                    select new ComboBoxResult
                    {
                        id = VB.JobOrderLineId.ToString(),
                        text = JobOrderLineTab.JobOrderHeader.DocType.DocumentTypeShortName + "-" + JobOrderLineTab.JobOrderHeader.DocNo,
                        TextProp1 = "Balance :" + VB.BalanceQty,
                        TextProp2 = "Date :" + JobOrderLineTab.JobOrderHeader.DocDate,
                        AProp1 = JobOrderLineTab.Product.ProductName,
                        AProp2 = ((JobOrderLineTab.Dimension1.Dimension1Name == null) ? "" : JobOrderLineTab.Dimension1.Dimension1Name) +
                                    ((JobOrderLineTab.Dimension2.Dimension2Name == null) ? "" : "," + JobOrderLineTab.Dimension2.Dimension2Name) +
                                    ((JobOrderLineTab.Dimension3.Dimension3Name == null) ? "" : "," + JobOrderLineTab.Dimension3.Dimension3Name) +
                                    ((JobOrderLineTab.Dimension4.Dimension4Name == null) ? "" : "," + JobOrderLineTab.Dimension4.Dimension4Name)
                    });

            //return (from VB in db.ViewJobOrderBalance
            //        join L in db.JobOrderLine on VB.JobOrderLineId equals L.JobOrderLineId into JobOrderLineTable
            //        from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
            //        where VB.BalanceQty > 0 && JobOrderLineTab.JobOrderHeader.JobWorkerId == JobInvoiceHeader.JobWorkerId
            //        && (string.IsNullOrEmpty(settings.filterContraSites) ? VB.SiteId == CurrentSiteId : contraSites.Contains(VB.SiteId.ToString()))
            //        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? VB.DivisionId == CurrentDivisionId : contraDivisions.Contains(VB.DivisionId.ToString()))
            //        && (string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.JobOrderHeader.DocNo.ToLower().Contains(term.ToLower())
            //            || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.JobOrderHeader.DocType.DocumentTypeShortName.ToLower().Contains(term.ToLower())
            //            || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Product.ProductName.ToLower().Contains(term.ToLower())
            //            || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower())
            //            || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower())
            //            || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower())
            //            || string.IsNullOrEmpty(term) ? 1 == 1 : JobOrderLineTab.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower())
            //            )
            //        select new ComboBoxResult
            //        {
            //            id = VB.JobOrderLineId.ToString(),
            //            text = JobOrderLineTab.JobOrderHeader.DocType.DocumentTypeShortName + "-" + JobOrderLineTab.JobOrderHeader.DocNo,
            //            TextProp1 = "Balance :" + VB.BalanceQty,
            //            TextProp2 = "Date :" + JobOrderLineTab.JobOrderHeader.DocDate,
            //            AProp1 = JobOrderLineTab.Product.ProductName,
            //            AProp2 = ((JobOrderLineTab.Dimension1.Dimension1Name == null) ? "" : JobOrderLineTab.Dimension1.Dimension1Name) +
            //                        ((JobOrderLineTab.Dimension2.Dimension2Name == null) ? "" : "," + JobOrderLineTab.Dimension2.Dimension2Name) +
            //                        ((JobOrderLineTab.Dimension3.Dimension3Name == null) ? "" : "," + JobOrderLineTab.Dimension3.Dimension3Name) +
            //                        ((JobOrderLineTab.Dimension4.Dimension4Name == null) ? "" : "," + JobOrderLineTab.Dimension4.Dimension4Name)
            //        });
        }


        public IEnumerable<ComboBoxResult> GetJobReceiveHelpListForProduct(int Id, string term)
        {
            var JobInvoiceHeader = new JobInvoiceHeaderService(_unitOfWork).Find(Id);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoiceHeader.DocTypeId, JobInvoiceHeader.DivisionId, JobInvoiceHeader.SiteId);

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            return (from VB in db.ViewJobReceiveBalance
                    join L in db.JobReceiveLine on VB.JobReceiveLineId equals L.JobReceiveLineId into JobReceiveLineTable
                    from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                    where VB.BalanceQty > 0 && JobReceiveLineTab.JobReceiveHeader.JobWorkerId == JobInvoiceHeader.JobWorkerId
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? VB.SiteId == CurrentSiteId : contraSites.Contains(VB.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? VB.DivisionId == CurrentDivisionId : contraDivisions.Contains(VB.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : JobReceiveLineTab.JobReceiveHeader.DocNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobReceiveLineTab.JobReceiveHeader.DocType.DocumentTypeShortName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobReceiveLineTab.Product.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobReceiveLineTab.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobReceiveLineTab.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobReceiveLineTab.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobReceiveLineTab.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower())
                        )
                    select new ComboBoxResult
                    {
                        id = VB.JobReceiveLineId.ToString(),
                        text = JobReceiveLineTab.Product.ProductName,
                        TextProp1 = "Balance :" + VB.BalanceQty,
                        TextProp2 = "Date :" + JobReceiveLineTab.JobReceiveHeader.DocDate,
                        AProp1 = JobReceiveLineTab.JobReceiveHeader.DocType.DocumentTypeShortName + "-" + JobReceiveLineTab.JobReceiveHeader.DocNo,
                        AProp2 = ((JobReceiveLineTab.Dimension1.Dimension1Name == null) ? "" : JobReceiveLineTab.Dimension1.Dimension1Name) +
                                    ((JobReceiveLineTab.Dimension2.Dimension2Name == null) ? "" : "," + JobReceiveLineTab.Dimension2.Dimension2Name) +
                                    ((JobReceiveLineTab.Dimension3.Dimension3Name == null) ? "" : "," + JobReceiveLineTab.Dimension3.Dimension3Name) +
                                    ((JobReceiveLineTab.Dimension4.Dimension4Name == null) ? "" : "," + JobReceiveLineTab.Dimension4.Dimension4Name)
                    });
        }


        public JobReceiveLineViewModel GetReceiveLineDetailForInvoice(int id, int InvoiceId)
        {
            var Invoice = new JobInvoiceHeaderService(_unitOfWork).Find(InvoiceId);

            var ReceiveLine = new JobReceiveLineService(_unitOfWork).Find(id);
            var OrderLine = new JobOrderLineService(_unitOfWork).Find(ReceiveLine.JobOrderLineId ?? 0);


            var temp = (from VJRBal in db.ViewJobReceiveBalance
                        join L in db.JobReceiveLine on VJRBal.JobReceiveLineId equals L.JobReceiveLineId
                        join P in db.Product on VJRBal.ProductId equals P.ProductId
                        join D1 in db.Dimension1 on L.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on L.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D3 in db.Dimension3 on L.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D4 in db.Dimension4 on L.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        where VJRBal.JobReceiveLineId == id
                        select new JobReceiveLineViewModel
                        {
                            Dimension1Id = L.Dimension1Id,
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Id = L.Dimension2Id,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Id = L.Dimension3Id,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Id = L.Dimension4Id,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            LotNo = L.LotNo,
                            Qty = VJRBal.BalanceQty,
                            Specification = L.Specification,
                            UnitId = L.JobOrderLine.UnitId,
                            DealUnitId = L.DealUnitId,
                            Amount = L.DealQty * L.JobOrderLine.Rate,
                            DealQty = VJRBal.BalanceQty * L.UnitConversionMultiplier,
                            UnitConversionMultiplier = L.UnitConversionMultiplier,
                            UnitName = L.JobOrderLine.Unit.UnitName,
                            DealUnitName = L.DealUnit.UnitName,
                            ProductId = VJRBal.ProductId,
                            ProductName = L.Product.ProductName,
                            UnitDecimalPlaces = P.Unit.DecimalPlaces,
                            DealUnitDecimalPlaces = L.DealUnit.DecimalPlaces,
                            Rate = L.JobOrderLine.Rate,
                            JobReceiveHeaderDocNo = VJRBal.JobReceiveNo,
                            JobOrderHeaderDocNo = L.JobOrderLine.JobOrderHeader.DocNo,
                            SalesTaxGroupProductId = P.SalesTaxGroupProductId ?? P.ProductGroup.DefaultSalesTaxGroupProductId,
                            SalesTaxGroupProductName = P.SalesTaxGroupProduct.ChargeGroupProductName ?? P.ProductGroup.DefaultSalesTaxGroupProduct.ChargeGroupProductName,
                            CostCenterId = L.JobOrderLine.JobOrderHeader.CostCenterId != null ? L.JobOrderLine.JobOrderHeader.CostCenterId : null,
                            CostCenterName = L.JobOrderLine.JobOrderHeader.CostCenterId != null ? L.JobOrderLine.JobOrderHeader.CostCenter.CostCenterName : null,
                        }).FirstOrDefault();

            if (OrderLine != null)
            {
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
            }

            return temp;

        }


        public Task<IEquatable<JobInvoiceLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobInvoiceLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
