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

namespace Service
{
    public interface IJobReturnLineService : IDisposable
    {
        JobReturnLine Create(JobReturnLine pt);
        void Delete(int id);
        void Delete(JobReturnLine pt);
        JobReturnLine Find(int id);
        IEnumerable<JobReturnLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobReturnLine pt);
        JobReturnLine Add(JobReturnLine pt);
        IEnumerable<JobReturnLine> GetJobReturnLineList();
        IEnumerable<JobReturnLine> GetJobReturnLineList(int id);
        IEnumerable<JobReturnLineIndexViewModel> GetLineListForIndex(int HeaderId);
        Task<IEquatable<JobReturnLine>> GetAsync();
        Task<JobReturnLine> FindAsync(int id);
        JobReturnLineViewModel GetJobReturnLine(int id);
        int NextId(int id);
        int PrevId(int id);

        IEnumerable<JobReturnLineViewModel> GetJobOrderForFilters(JobReturnLineFilterViewModel vm);
        IEnumerable<JobReturnLineViewModel> GetJobReceivesForFilters(JobReturnLineFilterViewModel vm);
        IEnumerable<JobReceiveListViewModel> GetPendingJobReceiptHelpList(int Id, string term);//JobOrderHeaderId
        IEnumerable<JobOrderLineListViewModel> GetPendingJobOrderHelpList(int Id, string term);//JobOrderHeaderId
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);
        string GetFirstBarCodeForReturn(int JobReceiveLineId);
        string GetFirstBarCodeForReturn(int[] JobReceiveLineIds);
        List<ComboBoxList> GetPendingBarCodesList(string id);
    }

    public class JobReturnLineService : IJobReturnLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobReturnLine> _JobReturnLineRepository;
        RepositoryQuery<JobReturnLine> JobReturnLineRepository;
        public JobReturnLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobReturnLineRepository = new Repository<JobReturnLine>(db);
            JobReturnLineRepository = new RepositoryQuery<JobReturnLine>(_JobReturnLineRepository);
        }


        public JobReturnLine Find(int id)
        {
            return _unitOfWork.Repository<JobReturnLine>().Find(id);
        }

        public JobReturnLine Create(JobReturnLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobReturnLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobReturnLine>().Delete(id);
        }

        public void Delete(JobReturnLine pt)
        {
            _unitOfWork.Repository<JobReturnLine>().Delete(pt);
        }

        public IEnumerable<JobReturnLineViewModel> GetJobOrderForFilters(JobReturnLineFilterViewModel vm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.JobOrderHeaderId)) { SaleOrderIdArr = vm.JobOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewJobReceiveBalance
                        join l in db.JobReceiveLine on p.JobReceiveLineId equals l.JobReceiveLineId into linetable
                        from linetab in linetable.DefaultIfEmpty()
                        join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join t1 in db.JobReceiveLine on p.JobReceiveLineId equals t1.JobReceiveLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join line in db.JobOrderLine on p.JobOrderLineId equals line.JobOrderLineId into JOlinetable from JOlinetab in JOlinetable.DefaultIfEmpty()
                        join header in db.JobOrderHeader on JOlinetab.JobOrderHeaderId equals header.JobOrderHeaderId
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.JobOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.JobOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        orderby header.DocDate,header.DocNo, JOlinetab.Sr
                        select new JobReturnLineViewModel
                        {
                            Dimension1Name = tab1.JobOrderLine.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.JobOrderLine.Dimension2.Dimension2Name,
                            Dimension3Name = tab1.JobOrderLine.Dimension3.Dimension3Name,
                            Dimension4Name = tab1.JobOrderLine.Dimension4.Dimension4Name,
                            Dimension1Id = tab1.JobOrderLine.Dimension1Id,
                            Dimension2Id = tab1.JobOrderLine.Dimension2Id,
                            Dimension3Id = tab1.JobOrderLine.Dimension3Id,
                            Dimension4Id = tab1.JobOrderLine.Dimension4Id,
                            Specification = tab1.JobOrderLine.Specification,
                            GoodsReceiptBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            JobReceiveHeaderDocNo = tab.DocNo,
                            JobOrderDocNo = tab1.JobOrderLine == null ? "" : tab1.JobOrderLine.JobOrderHeader.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            JobReturnHeaderId = vm.JobReturnHeaderId,
                            JobReceiveLineId = p.JobReceiveLineId,
                            UnitId = tab2.UnitId,
                            UnitConversionMultiplier = linetab.JobOrderLine.UnitConversionMultiplier,
                            DealUnitId = linetab.JobOrderLine.DealUnitId,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces=linetab.JobOrderLine.DealUnit.DecimalPlaces,
                        }

                        );
            return temp;
        }

        public IEnumerable<JobReturnLineViewModel> GetJobReceivesForFilters(JobReturnLineFilterViewModel vm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.JobReceiveHeaderId)) { SaleOrderIdArr = vm.JobReceiveHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }
            //ToChange View to get Joborders instead of goodsreceipts
            var temp = (from p in db.ViewJobReceiveBalance
                        join l in db.JobReceiveLine on p.JobReceiveLineId equals l.JobReceiveLineId into linetable
                        from linetab in linetable.DefaultIfEmpty()
                        join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId into table
                        from tab in table.DefaultIfEmpty()
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        join t1 in db.JobReceiveLine on p.JobReceiveLineId equals t1.JobReceiveLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.JobReceiveHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.JobReceiveHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        orderby tab.DocDate,tab.DocNo, linetab.Sr
                        select new JobReturnLineViewModel
                        {
                            Dimension1Id = tab1.JobOrderLine.Dimension1Id,
                            Dimension1Name = tab1.JobOrderLine.Dimension1.Dimension1Name,
                            Dimension2Id = tab1.JobOrderLine.Dimension2Id,
                            Dimension2Name = tab1.JobOrderLine.Dimension2.Dimension2Name,
                            Dimension3Id = tab1.JobOrderLine.Dimension3Id,
                            Dimension3Name = tab1.JobOrderLine.Dimension3.Dimension3Name,
                            Dimension4Id = tab1.JobOrderLine.Dimension4Id,
                            Dimension4Name = tab1.JobOrderLine.Dimension4.Dimension4Name,
                            Specification = tab1.JobOrderLine.Specification,
                            GoodsReceiptBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            JobReceiveHeaderDocNo = tab.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            JobReturnHeaderId = vm.JobReturnHeaderId,
                            JobReceiveLineId = p.JobReceiveLineId,
                            UnitId = tab2.UnitId,
                            UnitName = tab2.Unit.UnitName,
                            UnitConversionMultiplier = linetab.JobOrderLine.UnitConversionMultiplier,
                            DealUnitId = linetab.JobOrderLine.DealUnitId,
                            DealUnitName = linetab.JobOrderLine.DealUnit.UnitName,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces=linetab.JobOrderLine.DealUnit.DecimalPlaces,
                        });
            return temp;
        }
        public void Update(JobReturnLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobReturnLine>().Update(pt);
        }

        public IEnumerable<JobReturnLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobReturnLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.JobReturnLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<JobReturnLineIndexViewModel> GetLineListForIndex(int HeaderId)
        {
            return (from p in db.JobReturnLine
                    join t in db.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t in db.JobReceiveLine on tab.JobReceiveLineId equals t.JobReceiveLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join t in db.JobReceiveHeader on tab1.JobReceiveHeaderId equals t.JobReceiveHeaderId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    where p.JobReturnHeaderId == HeaderId
                    orderby p.Sr
                    select new JobReturnLineIndexViewModel
                    {

                        ProductName = tab1.JobOrderLine.Product.ProductName,
                        Qty = p.Qty,
                        JobReturnLineId = p.JobReturnLineId,
                        UnitId = tab1.JobOrderLine.Product.UnitId,
                        Specification = tab1.JobOrderLine.Specification,
                        Dimension1Name = tab1.JobOrderLine.Dimension1.Dimension1Name,
                        Dimension2Name = tab1.JobOrderLine.Dimension2.Dimension2Name,
                        Dimension3Name = tab1.JobOrderLine.Dimension3.Dimension3Name,
                        Dimension4Name = tab1.JobOrderLine.Dimension4.Dimension4Name,
                        LotNo = tab1.LotNo,
                        JobReceiveHeaderDocNo = tab3.DocNo,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        unitDecimalPlaces = tab1.JobOrderLine.Product.Unit.DecimalPlaces,
                        DealunitDecimalPlaces=p.DealUnit.DecimalPlaces,
                        Remark = p.Remark,
                        StockId = p.StockId ?? 0,
                        ProductUidName=tab.ProductUid.ProductUidName

                    }
                        );
        }
        public JobReturnLineViewModel GetJobReturnLine(int id)
        {
            return (from p in db.JobReturnLine
                    join t in db.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t4 in db.ViewJobReceiveBalance on p.JobReceiveLineId equals t4.JobReceiveLineId into table4
                    from tab4 in table4.DefaultIfEmpty()
                    join t3 in db.JobReceiveLine on tab.JobReceiveLineId equals t3.JobReceiveLineId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t2 in db.JobReturnHeader on p.JobReturnHeaderId equals t2.JobReturnHeaderId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t in db.JobReceiveHeader on tab.JobReceiveHeaderId equals t.JobReceiveHeaderId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.JobReturnLineId == id
                    select new JobReturnLineViewModel
                    {
                        JobWorkerId = tab2.JobWorkerId,
                        ProductId = tab3.JobOrderLine.ProductId,
                        JobReceiveLineId = p.JobReceiveLineId,
                        JobReceiveHeaderDocNo = tab1.DocNo,
                        JobReturnHeaderId = p.JobReturnHeaderId,
                        JobReturnLineId = p.JobReturnLineId,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        DealQty = p.DealQty,
                        DealUnitId = p.DealUnitId,
                        DealUnitName=p.DealUnit.UnitName,
                        Qty = p.Qty,
                        GoodsReceiptBalQty = ((p.JobReceiveLineId == null || tab4 == null) ? p.Qty : p.Qty + tab4.BalanceQty),
                        Remark = p.Remark,
                        UnitId = tab3.JobOrderLine.Product.UnitId,
                        UnitName = tab3.JobOrderLine.Product.Unit.UnitName,
                        Dimension1Id = tab3.JobOrderLine.Dimension1Id,
                        Dimension1Name = tab3.JobOrderLine.Dimension1.Dimension1Name,
                        Dimension2Id = tab3.JobOrderLine.Dimension2Id,
                        Dimension2Name = tab3.JobOrderLine.Dimension2.Dimension2Name,
                        Dimension3Id = tab3.JobOrderLine.Dimension3Id,
                        Dimension3Name = tab3.JobOrderLine.Dimension3.Dimension3Name,
                        Dimension4Id = tab3.JobOrderLine.Dimension4Id,
                        Dimension4Name = tab3.JobOrderLine.Dimension4.Dimension4Name,
                        Specification = tab3.JobOrderLine.Specification,
                        ProductUidId=tab.ProductUidId,
                        ProductUidName=tab.ProductUid.ProductUidName,
                        Weight=p.Weight,
                        LockReason=p.LockReason,
                    }
                        ).FirstOrDefault();
        }

        public IEnumerable<JobReturnLine> GetJobReturnLineList()
        {
            var pt = _unitOfWork.Repository<JobReturnLine>().Query().Get().OrderBy(m => m.JobReturnLineId);

            return pt;
        }

        public IEnumerable<JobReturnLine> GetJobReturnLineList(int id)
        {
            return (from p in db.JobReturnLine
                    where p.JobReturnHeaderId==id
                    select p).ToList();
        }

        public JobReturnLine Add(JobReturnLine pt)
        {
            _unitOfWork.Repository<JobReturnLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobReturnLine
                        orderby p.JobReturnLineId
                        select p.JobReturnLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobReturnLine
                        orderby p.JobReturnLineId
                        select p.JobReturnLineId).FirstOrDefault();
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

                temp = (from p in db.JobReturnLine
                        orderby p.JobReturnLineId
                        select p.JobReturnLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobReturnLine
                        orderby p.JobReturnLineId
                        select p.JobReturnLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public IEnumerable<JobReceiveListViewModel> GetPendingJobReceiptHelpList(int Id, string term)
        {

            var JobInvoice = new JobReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

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

            var list = (from p in db.ViewJobReceiveBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.JobReceiveNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && p.JobWorkerId == JobInvoice.JobWorkerId
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                         && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.JobReceiveHeaderId into g
                        select new JobReceiveListViewModel
                        {
                            DocNo = g.Max(m => m.p.JobReceiveNo),
                            JobReceiveHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }

        public IEnumerable<JobOrderLineListViewModel> GetPendingJobOrderHelpList(int Id, string term)//JobOrderHeaderId
        {
            var JobInvoice = new JobReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(JobInvoice.DocTypeId, JobInvoice.DivisionId, JobInvoice.SiteId);

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

            var list = (from p in db.ViewJobReceiveBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.JobOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && p.JobWorkerId == JobInvoice.JobWorkerId && p.JobOrderLineId != null
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                         && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.JobOrderHeaderId == null ? -1 : p.JobOrderHeaderId into g
                        select new JobOrderLineListViewModel
                        {
                            DocNo = g.Max(m => m.p.JobOrderNo),
                            JobOrderHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }



        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var JobReturn = new JobReturnHeaderService(_unitOfWork).Find(Id);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(JobReturn.DocTypeId, JobReturn.DivisionId, JobReturn.SiteId);

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

                            //    DocumentTypeName=g.Max(p=>p.p.DocumentTypeShortName)
                        }
                          ).Take(20);

            return list.ToList();
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.JobReturnLine
                       where p.JobReturnHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }

        public string GetFirstBarCodeForReturn(int JobReceiveLineId)
        {
            return (from p in db.JobReceiveLine
                    where p.JobReceiveLineId == JobReceiveLineId
                    join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                    select p.ProductUidId).FirstOrDefault().ToString();
        }

        public string GetFirstBarCodeForReturn(int[] JobReceiveLineIds)
        {
            return (from p in db.JobReceiveLine
                    where JobReceiveLineIds.Contains(p.JobReceiveLineId)
                    join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                    orderby t.ProductUidName
                    select p.ProductUidId).FirstOrDefault().ToString();
        }

        public List<ComboBoxList> GetPendingBarCodesList(string id)
        {
            List<ComboBoxList> Barcodes = new List<ComboBoxList>();

            int[] JobReceiveLine = id.Split(',').Select(Int32.Parse).ToArray();

            var ReceiveRecords = (from p in db.ViewJobReceiveBalance
                                  join t in db.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                                  where JobReceiveLine.Contains(p.JobReceiveLineId)
                                  select t).ToList();

            int[] BalanceRecRecordsProdUIds = ReceiveRecords.Select(m => m.ProductUidId.Value).ToArray();

            using (ApplicationDbContext context = new ApplicationDbContext())
            {

                //context.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                //context.Database.CommandTimeout = 30000;

                var Temp = (from p in context.ProductUid
                            where BalanceRecRecordsProdUIds.Contains(p.ProductUIDId)
                            && p.Status == ProductUidStatusConstants.Issue 
                            orderby p.ProductUidName
                            select new { Id = p.ProductUIDId, Name = p.ProductUidName }).ToList();
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

        public void Dispose()
        {
        }


        public Task<IEquatable<JobReturnLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReturnLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
