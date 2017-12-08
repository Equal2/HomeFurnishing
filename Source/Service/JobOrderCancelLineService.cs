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
using System.Configuration;

namespace Service
{
    public interface IJobOrderCancelLineService : IDisposable
    {
        JobOrderCancelLine Create(JobOrderCancelLine pt);
        void Delete(int id);
        void Delete(JobOrderCancelLine pt);
        JobOrderCancelLine Find(int id);
        IEnumerable<JobOrderCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobOrderCancelLine pt);
        JobOrderCancelLine Add(JobOrderCancelLine pt);
        IEnumerable<JobOrderCancelLine> GetJobOrderCancelLineList();
        IEnumerable<JobOrderCancelLineViewModel> GetJobOrderCancelLineForHeader(int id);//Header Id
        Task<IEquatable<JobOrderCancelLine>> GetAsync();
        Task<JobOrderCancelLine> FindAsync(int id);
        IEnumerable<JobOrderCancelLineViewModel> GetJobOrderLineForMultiSelect(JobOrderCancelFilterViewModel svm);
        JobOrderCancelLineViewModel GetJobOrderCancelLine(int id);//Line Id
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<ComboBoxList> GetPendingJobOrders(string DocTypes, string term, int JobOrderCancelHeaderId);
        IEnumerable<ComboBoxList> GetPendingProductsForJobOrderCancel(string DocTypes, string term, int JobOrderCancelHeaderId);
        IEnumerable<ProcGetBomForWeavingViewModel> GetBomPostingDataForCancel(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int ProcessId, decimal Qty, int DocTypeId);
        List<ComboBoxList> GetPendingBarCodesList(int id);
        bool CheckForDuplicateJobOrder(int LineId, int CancelHeaderId);
        int GetMaxSr(int id);
        string GetFirstBarCodeForCancel(int JobOrderLineId);
        JobOrderLineViewModel GetOrderLineForUidMain(int Uid, int CancelHeaderId);
        JobOrderLineViewModel GetOrderLineForUidBranch(int Uid);
    }

    public class JobOrderCancelLineService : IJobOrderCancelLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobOrderCancelLine> _JobOrderCancelLineRepository;
        RepositoryQuery<JobOrderCancelLine> JobOrderCancelLineRepository;
        public JobOrderCancelLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobOrderCancelLineRepository = new Repository<JobOrderCancelLine>(db);
            JobOrderCancelLineRepository = new RepositoryQuery<JobOrderCancelLine>(_JobOrderCancelLineRepository);
        }


        public JobOrderCancelLine Find(int id)
        {
            return _unitOfWork.Repository<JobOrderCancelLine>().Find(id);
        }

        public JobOrderCancelLine Create(JobOrderCancelLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderCancelLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderCancelLine>().Delete(id);
        }

        public void Delete(JobOrderCancelLine pt)
        {
            _unitOfWork.Repository<JobOrderCancelLine>().Delete(pt);
        }

        public void Update(JobOrderCancelLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderCancelLine>().Update(pt);
        }

        public IEnumerable<JobOrderCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobOrderCancelLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.JobOrderCancelLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<JobOrderCancelLineViewModel> GetJobOrderCancelLineForHeader(int id)
        {
            return (from p in db.JobOrderCancelLine
                    join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t5 in db.JobOrderHeader on tab.JobOrderHeaderId equals t5.JobOrderHeaderId
                    join D1 in db.Dimension1 on tab.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                    from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                    join D2 in db.Dimension2 on tab.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                    from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                    join D3 in db.Dimension3 on tab.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                    from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                    join D4 in db.Dimension4 on tab.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                    from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                    join t3 in db.Product on tab.ProductId equals t3.ProductId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t4 in db.JobOrderCancelHeader on p.JobOrderCancelHeaderId equals t4.JobOrderCancelHeaderId
                    join t6 in db.Persons on t4.JobWorkerId equals t6.PersonID into table6
                    from tab6 in table6.DefaultIfEmpty()
                    where p.JobOrderCancelHeaderId == id
                    orderby p.Sr
                    select new JobOrderCancelLineViewModel
                    {
                        Dimension1Name = Dimension1Tab.Dimension1Name,
                        Dimension2Name = Dimension2Tab.Dimension2Name,
                        Dimension3Name = Dimension3Tab.Dimension3Name,
                        Dimension4Name = Dimension4Tab.Dimension4Name,
                        DueDate = tab.DueDate,
                        LotNo = tab.LotNo,
                        ProductId = tab.ProductId,
                        ProductName = tab3.ProductName,
                        JobOrderCancelHeaderDocNo = t4.DocNo,
                        JobOrderCancelHeaderId = p.JobOrderCancelHeaderId,
                        JobOrderCancelLineId = p.JobOrderCancelLineId,
                        JobOrderDocNo = t5.DocNo,
                        JobOrderLineId = tab.JobOrderLineId,
                        Qty = p.Qty,
                        Remark = p.Remark,
                        ProductUidId = p.ProductUidId,
                        Specification = tab.Specification,
                        JobWorkerId = t4.JobWorkerId,
                        JobWorkerName = tab6.Name,
                        UnitId = tab3.UnitId,
                        unitDecimalPlaces = tab3.Unit.DecimalPlaces,
                        StockId = p.StockId,
                        StockProcessId = p.StockProcessId,
                        ProductUidName = p.ProductUid.ProductUidName,
                    }
                        );

        }

        public IEnumerable<JobOrderCancelLineViewModel> GetJobOrderLineForMultiSelect(JobOrderCancelFilterViewModel svm)
        {

            JobOrderCancelHeader Header = db.JobOrderCancelHeader.Find(svm.JobOrderCancelHeaderId);

            var Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);


            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.JobOrderId)) { SaleOrderIdArr = svm.JobOrderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            string[] Dime1IdArr = null;
            if (!string.IsNullOrEmpty(svm.Dimension1Id)) { Dime1IdArr = svm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dime1IdArr = new string[] { "NA" }; }

            string[] Dime2IdArr = null;
            if (!string.IsNullOrEmpty(svm.Dimension2Id)) { Dime2IdArr = svm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dime2IdArr = new string[] { "NA" }; }

            string[] Dime3IdArr = null;
            if (!string.IsNullOrEmpty(svm.Dimension3Id)) { Dime3IdArr = svm.Dimension3Id.Split(",".ToCharArray()); }
            else { Dime3IdArr = new string[] { "NA" }; }

            string[] Dime4IdArr = null;
            if (!string.IsNullOrEmpty(svm.Dimension4Id)) { Dime4IdArr = svm.Dimension4Id.Split(",".ToCharArray()); }
            else { Dime4IdArr = new string[] { "NA" }; }

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(Settings.filterContraSites)) { ContraSites = Settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDivisions)) { ContraDivisions = Settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }


            var temp = (from p in db.ViewJobOrderBalance
                        join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId into table
                        from tab in table.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (svm.JobWorkerId == 0 ? 1 == 1 : p.JobWorkerId == svm.JobWorkerId)
                        && (string.IsNullOrEmpty(svm.JobOrderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.JobOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && (string.IsNullOrEmpty(svm.Dimension1Id) ? 1 == 1 : Dime1IdArr.Contains(p.Dimension1Id.ToString()))
                        && (string.IsNullOrEmpty(svm.Dimension2Id) ? 1 == 1 : Dime2IdArr.Contains(p.Dimension2Id.ToString()))
                        && (string.IsNullOrEmpty(svm.Dimension3Id) ? 3 == 3 : Dime3IdArr.Contains(p.Dimension3Id.ToString()))
                        && (string.IsNullOrEmpty(svm.Dimension4Id) ? 3 == 3 : Dime4IdArr.Contains(p.Dimension4Id.ToString()))
                        && (string.IsNullOrEmpty(Settings.filterContraSites) ? p.SiteId == Header.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(Settings.filterContraDivisions) ? p.DivisionId == Header.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                        && p.BalanceQty > 0 && p.JobWorkerId == svm.JobWorkerId
                        orderby p.OrderDate, p.JobOrderNo, tab.Sr
                        select new JobOrderCancelLineViewModel
                        {
                            BalanceQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            JobOrderDocNo = p.JobOrderNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            JobOrderCancelHeaderId = svm.JobOrderCancelHeaderId,
                            JobOrderLineId = p.JobOrderLineId,
                            Dimension1Id = p.Dimension1Id,
                            Dimension2Id = p.Dimension2Id,
                            Dimension3Id = p.Dimension3Id,
                            Dimension4Id = p.Dimension4Id,
                            Dimension1Name = p.Dimension1.Dimension1Name,
                            Dimension2Name = p.Dimension2.Dimension2Name,
                            Dimension3Name = p.Dimension3.Dimension3Name,
                            Dimension4Name = p.Dimension4.Dimension4Name,
                            Specification = tab.Specification,
                            UnitId = tab2.UnitId,
                            UnitName = tab2.Unit.UnitName,
                            unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces = tab.DealUnit.DecimalPlaces,
                            ProductUidName = (tab.ProductUidHeaderId == null ? tab.ProductUid.ProductUidName : "")
                        });

            return temp;
        }
        public JobOrderCancelLineViewModel GetJobOrderCancelLine(int id)
        {
            var temp = (from p in db.JobOrderCancelLine
                        join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId into table
                        from tab in table.DefaultIfEmpty()
                        join t5 in db.JobOrderHeader on tab.JobOrderHeaderId equals t5.JobOrderHeaderId into table5
                        from tab5 in table5.DefaultIfEmpty()
                        join D1 in db.Dimension1 on tab.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on tab.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D3 in db.Dimension3 on tab.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D4 in db.Dimension4 on tab.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        join t3 in db.Product on tab.ProductId equals t3.ProductId into table3
                        from tab3 in table3.DefaultIfEmpty()
                        join t4 in db.JobOrderCancelHeader on p.JobOrderCancelHeaderId equals t4.JobOrderCancelHeaderId into table4
                        from tab4 in table4.DefaultIfEmpty()
                        join t6 in db.Persons on tab4.JobWorkerId equals t6.PersonID into table6
                        from tab6 in table6.DefaultIfEmpty()
                        join t7 in db.ViewJobOrderBalance on p.JobOrderLineId equals t7.JobOrderLineId into table7
                        from tab7 in table7.DefaultIfEmpty()
                        orderby p.JobOrderCancelLineId
                        where p.JobOrderCancelLineId == id
                        select new JobOrderCancelLineViewModel
                        {
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            DueDate = tab.DueDate,
                            LotNo = tab.LotNo,
                            ProductId = tab.ProductId,
                            ProductName = tab3.ProductName,
                            JobOrderCancelHeaderDocNo = tab4.DocNo,
                            JobOrderCancelHeaderId = p.JobOrderCancelHeaderId,
                            JobOrderCancelLineId = p.JobOrderCancelLineId,
                            JobOrderDocNo = tab5.DocNo,
                            JobOrderLineId = tab.JobOrderLineId,
                            BalanceQty = p.Qty + (tab7.BalanceQty == null ? 0 : tab7.BalanceQty),
                            Qty = p.Qty,
                            Remark = p.Remark,
                            Specification = tab.Specification,
                            JobWorkerId = tab4.JobWorkerId,
                            JobWorkerName = tab6.Name,
                            UnitId = tab3.UnitId,
                            ProductUidId = p.ProductUidId,
                            ProductUidName = p.ProductUid.ProductUidName,
                            LockReason = p.LockReason,
                        }

                      ).FirstOrDefault();

            return temp;

        }

        public IEnumerable<JobOrderCancelLine> GetJobOrderCancelLineList()
        {
            var pt = _unitOfWork.Repository<JobOrderCancelLine>().Query().Get().OrderBy(m => m.JobOrderCancelLineId);

            return pt;
        }

        public JobOrderCancelLine Add(JobOrderCancelLine pt)
        {
            _unitOfWork.Repository<JobOrderCancelLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobOrderCancelLine
                        orderby p.JobOrderCancelLineId
                        select p.JobOrderCancelLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderCancelLine
                        orderby p.JobOrderCancelLineId
                        select p.JobOrderCancelLineId).FirstOrDefault();
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

                temp = (from p in db.JobOrderCancelLine
                        orderby p.JobOrderCancelLineId
                        select p.JobOrderCancelLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderCancelLine
                        orderby p.JobOrderCancelLineId
                        select p.JobOrderCancelLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<ComboBoxList> GetPendingJobOrders(string DocTypes, string term, int JobOrderCancelHeaderId)//DocTypeId
        {

            JobOrderCancelHeader header = new JobOrderCancelHeaderService(_unitOfWork).Find(JobOrderCancelHeaderId);

            return (from p in db.ViewJobOrderBalance
                    join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && ProTab.DocNo.ToLower().Contains(term.ToLower())
                    && p.SiteId == header.SiteId && p.DivisionId == header.DivisionId && p.JobWorkerId == header.JobWorkerId
                    group new { p, ProTab } by p.JobOrderHeaderId into g
                    select new ComboBoxList
                    {
                        Id = g.Key,
                        PropFirst = g.Max(m => m.ProTab.DocNo),
                    }
                        );
        }


        public IEnumerable<ComboBoxList> GetPendingProductsForJobOrderCancel(string DocTypes, string term, int JobOrderCancelHeaderId)//DocTypeId
        {
            JobOrderCancelHeader header = new JobOrderCancelHeaderService(_unitOfWork).Find(JobOrderCancelHeaderId);

            return (from p in db.ViewJobOrderBalance
                    join t in db.Product on p.ProductId equals t.ProductId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.BalanceQty > 0 && ProTab.ProductName.ToLower().Contains(term.ToLower())
                    && p.SiteId == header.SiteId && p.DivisionId == header.DivisionId
                    group new { p, ProTab } by p.ProductId into g
                    orderby g.Key descending
                    select new ComboBoxList
                    {
                        Id = g.Key,
                        PropFirst = g.Max(m => m.ProTab.ProductName)
                    }
                        );
        }


        public IEnumerable<ProcGetBomForWeavingViewModel> GetBomPostingDataForCancel(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int ProcessId, decimal Qty, int DocTypeId)
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

            string ProcName = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId).SqlProcConsumption;

            //PendingOrderQtyForPacking = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetBomForWeaving @DocTypeId, @ProductId, @ProcessId,@Qty"+(Dimension1Id==null?"":",@Dimension1Id")+(Dimension2Id==null?"":",@Dimension2Id"), SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, (Dimension1Id==null? "" : Dimension1Id), (Dimension2Id)).ToList();

            if (ProcName != null && ProcName != "")
            {
                if (Dimension1Id == null && Dimension2Id == null)
                {
                    PendingOrderQtyForPacking = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty).ToList();
                }
                else if (Dimension1Id == null && Dimension2Id != null)
                {
                    PendingOrderQtyForPacking = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension2Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, Dimension2Id).ToList();
                }
                else if (Dimension1Id != null && Dimension2Id == null)
                {
                    PendingOrderQtyForPacking = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension1Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, Dimension1Id).ToList();
                }
                else
                {
                    PendingOrderQtyForPacking = db.Database.SqlQuery<ProcGetBomForWeavingViewModel>("" + ProcName + " @DocTypeId, @ProductId, @ProcessId,@Qty,@Dimension1Id, @Dimension2Id", SQLDocTypeID, SQLProductID, SQLProcessId, SQLQty, SQLDime1, SQLDime2).ToList();
                }
            }

            return PendingOrderQtyForPacking;

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
                //            where p.ProductUidHeaderId == JobOrderline.ProductUidHeaderId
                //            join t in context.JobReceiveLine on p.ProductUIDId equals t.ProductUidId into table
                //            from tab in table.DefaultIfEmpty()
                //            join RT in context.JobReturnLine  on tab.JobReceiveLineId  equals RT.JobReceiveLineId into tableRT
                //            from tabRT in tableRT.DefaultIfEmpty()
                //            where ( tab == null && p.Status != ProductUidStatusConstants.Cancel && ((p.GenDocId == p.LastTransactionDocId && p.GenDocNo == p.LastTransactionDocNo && p.GenPersonId == p.LastTransactionPersonId) || p.CurrenctGodownId != null) )
                //            || (tab != null && tabRT != null )                           
                //            orderby p.ProductUidName
                //            select new { Id = p.ProductUIDId, Name = p.ProductUidName }).ToList();


                //p.Status != ProductUidStatusConstants.Return &&


                SqlParameter SQLProductUidHeaderId = new SqlParameter("@ProductUidHeaderId", JobOrderline.ProductUidHeaderId);
                SqlParameter SQLSiteId = new SqlParameter("@SiteId", JobOrderHeader.SiteId);
                SqlParameter SQLDivisionId = new SqlParameter("@DivisionId", JobOrderHeader.DivisionId);
                SqlParameter SQLProcessId = new SqlParameter("@ProcessId", JobOrderHeader.ProcessId);


                var Temp = db.Database.SqlQuery<ProductUidList>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetProductUidListForWeavingOrderCancel @ProductUidHeaderId, @SiteId,	@DivisionId, @ProcessId", SQLProductUidHeaderId, SQLSiteId, SQLDivisionId, SQLProcessId).ToList();


                //var Temp = (from p in context.ProductUid
                //            join t in context.JobReceiveLine on p.ProductUIDId equals t.ProductUidId into table
                //            from tab in table.DefaultIfEmpty()
                //            join t3 in context.JobReceiveHeader.Where(m => m.SiteId == JobOrderHeader.SiteId && m.DivisionId == JobOrderHeader.DivisionId) on tab.JobReceiveHeaderId equals t3.JobReceiveHeaderId into table5
                //            from JRH in table5.DefaultIfEmpty()
                //            join t2 in context.JobOrderLine on p.ProductUidHeaderId equals t2.ProductUidHeaderId
                //            join JOH in context.JobOrderHeader.Where(m => m.SiteId == JobOrderHeader.SiteId && m.DivisionId == JobOrderHeader.DivisionId) on t2.JobOrderHeaderId equals JOH.JobOrderHeaderId
                //            join RecLineStatus in context.JobReceiveLineStatus on tab.JobReceiveLineId equals RecLineStatus.JobReceiveLineId into RecLineStatTab
                //            from RecLineStat in RecLineStatTab.DefaultIfEmpty()
                //            where p.ProductUidHeaderId == JobOrderline.ProductUidHeaderId && (JRH == null || ((tab.Qty - (RecLineStat.ReturnQty ?? 0)) == 0)) &&
                //            p.Status != ProductUidStatusConstants.Cancel && ((p.GenPersonId == p.LastTransactionPersonId) || p.CurrenctGodownId != null)
                //            && JOH.ProcessId == JobOrderHeader.ProcessId
                //            orderby p.ProductUIDId
                //            select new { Id = p.ProductUIDId, Name = p.ProductUidName }).ToList();

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

        public bool CheckForDuplicateJobOrder(int LineId, int CancelHeaderId)
        {

            return (from p in db.JobOrderCancelLine
                    where p.JobOrderCancelHeaderId == CancelHeaderId && p.JobOrderLineId == LineId
                    select p).Any();

        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.JobOrderCancelLine
                       where p.JobOrderCancelHeaderId == id
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
                    join t2 in db.JobReceiveLine on t.ProductUIDId equals t2.ProductUidId into table
                    from tab in table.DefaultIfEmpty()
                    where tab == null
                    select p.ProductUidId).FirstOrDefault().ToString();
        }

        public JobOrderLineViewModel GetOrderLineForUidMain(int Uid, int JobOrderCancelHeaderId)
        {
            JobOrderCancelHeader header = new JobOrderCancelHeaderService(_unitOfWork).Find(JobOrderCancelHeaderId);

            var Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            string[] ContraDocTypeIdArr = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { ContraDocTypeIdArr = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { ContraDocTypeIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ProductUid
                        where p.ProductUIDId == Uid
                        select new
                        {
                            Rec = (from t in db.JobOrderLine                                   
                                   join R in db.JobReceiveLine on new { A = t.JobOrderLineId, B = (int ?) Uid  } equals new { A = R.JobOrderLineId ?? 0, B = R.ProductUidId } into Rtable
                                   from Rtab in Rtable.DefaultIfEmpty()
                                   join RT in db.JobReturnLine on Rtab.JobReceiveLineId equals RT.JobReceiveLineId into RTtable
                                   from RTtab in RTtable.DefaultIfEmpty()
                                   join JOH in db.JobOrderHeader on t.JobOrderHeaderId equals JOH.JobOrderHeaderId into JOHtable
                                   from JOHtab in JOHtable.DefaultIfEmpty()
                                   where t.JobOrderHeaderId == p.GenDocId && t.JobOrderHeader.DocTypeId == p.GenDocTypeId && t.ProductUidHeaderId == p.ProductUidHeaderId
                                   && (string.IsNullOrEmpty(Settings.filterContraDocTypes) ? 1 == 1 : ContraDocTypeIdArr.Contains(JOHtab.DocTypeId.ToString()))                              
                                   && Rtab == null
                                   select new JobOrderLineViewModel
                                   {
                                       JobOrderLineId = t.JobOrderLineId,
                                       JobOrderHeaderDocNo = t.JobOrderHeader.DocNo,
                                       Specification = t.Specification,
                                       Dimension1Name = t.Dimension1.Dimension1Name,
                                       Dimension2Name = t.Dimension2.Dimension2Name,
                                       Dimension3Name = t.Dimension3.Dimension3Name,
                                       Dimension4Name = t.Dimension4.Dimension4Name,
                                       ProdOrderBalanceQty = 1,
                                       LotNo = p.LotNo,
                                       UnitName = p.Product.Unit.UnitName,
                                   }).FirstOrDefault()
                        }
                    ).FirstOrDefault();
            return temp.Rec;
        }

        public JobOrderLineViewModel GetOrderLineForUidBranch(int Uid)
        {
            var temp = (from p in db.ProductUid
                        where p.ProductUIDId == Uid
                        select new
                        {
                            Rec = (from t in db.JobOrderLine
                                   where t.JobOrderHeaderId == p.LastTransactionDocId && t.JobOrderHeader.DocTypeId == p.LastTransactionDocTypeId && t.ProductUidId == p.ProductUIDId
                                   select new JobOrderLineViewModel
                                   {
                                       JobOrderLineId = t.JobOrderLineId,
                                       JobOrderHeaderDocNo = t.JobOrderHeader.DocNo,
                                       Specification = t.Specification,
                                       Dimension1Name = t.Dimension1.Dimension1Name,
                                       Dimension2Name = t.Dimension2.Dimension2Name,
                                       Dimension3Name = t.Dimension3.Dimension3Name,
                                       Dimension4Name = t.Dimension4.Dimension4Name,
                                       ProdOrderBalanceQty = 1,
                                       LotNo = p.LotNo,
                                       UnitName = p.Product.Unit.UnitName,
                                   }).FirstOrDefault()
                        }
                        ).FirstOrDefault();

            return temp.Rec;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<JobOrderCancelLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderCancelLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }

    public class ProductUidList
    {
        public int ProductUidId { get; set; }

        public string ProductUidName { get; set; }
    }
}
