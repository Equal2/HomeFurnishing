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

namespace Service
{
    public interface IJobReceiveQALineService : IDisposable
    {
        JobReceiveQALine Create(JobReceiveQALine pt, string UserName);
        void Delete(int id);
        void Delete(JobReceiveQALine pt);
        JobReceiveQALine Find(int id);
        void Update(JobReceiveQALine pt, string UserName);
        Task<IEquatable<JobReceiveQALine>> GetAsync();
        Task<JobReceiveQALine> FindAsync(int id);
        JobReceiveQALineViewModel GetJobReceiveQALine(int id);
        List<ComboBoxList> GetPendingBarCodesList(int[] id);
        int GetMaxSr(int id);
        IEnumerable<JobReceiveQALineViewModel> GetLineListForIndex(int headerId);
        JobReceiveQALineViewModel GetLineDetailForQA(int id, int ReceiveId);
        UIDValidationViewModel ValidateQABarCode(string ProductUid, int HeaderId);
        JobReceiveQALineViewModel GetReceiveLineForUid(int Uid, int HeaderId);
        IEnumerable<JobReceiveQALineViewModel> GetJobReceiveLineForMultiSelect(JobReceiveQALineFilterViewModel svm);
        IEnumerable<ComboBoxResult> GetPendingProductsForJobReceiveQA(string term, int JobReceiveQAHeaderId);
        IQueryable<ComboBoxResult> GetPendingJobReceives(string term, int JobReceiveQAHeaderId);
        IEnumerable<JobReceiveHeaderListViewModel> GetPendingJobReceivesForAC(int HeaderId, string term, int Limiter);
    }

    public class JobReceiveQALineService : IJobReceiveQALineService
    {
        ApplicationDbContext db;
        private readonly IStockService _stockService;
        public JobReceiveQALineService(ApplicationDbContext db, IUnitOfWork _unitOfWork)
        {
            this.db = db;
            _stockService = new StockService(_unitOfWork);
        }

        public JobReceiveQALine Find(int id)
        {
            return db.JobReceiveQALine.Find(id);
        }

        public JobReceiveQALine Create(JobReceiveQALine pt, string UserName)
        {
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;

            pt.ObjectState = ObjectState.Added;
            db.JobReceiveQALine.Add(pt);





            return pt;
        }

        public void Delete(int id)
        {
            JobReceiveQALine Temp = db.JobReceiveQALine.Find(id);
            Temp.ObjectState = Model.ObjectState.Deleted;

            db.JobReceiveQALine.Remove(Temp);
        }

        public void Delete(JobReceiveQALine pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobReceiveQALine.Remove(pt);
        }

        public void Update(JobReceiveQALine pt, string UserName)
        {
            pt.ModifiedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Modified;
            db.JobReceiveQALine.Add(pt);
        }

        public JobReceiveQALineViewModel GetJobReceiveQALine(int id)
        {
            return (from p in db.JobReceiveQALine
                    join t in db.ViewJobReceiveBalanceForQA on p.JobReceiveLineId equals t.JobReceiveLineId into table
                    from tab in table.DefaultIfEmpty()
                    join jrl in db.JobReceiveLine on p.JobReceiveLineId equals jrl.JobReceiveLineId
                    join jrh in db.JobReceiveHeader on jrl.JobReceiveHeaderId equals jrh.JobReceiveHeaderId
                    join jol in db.JobOrderLine on jrl.JobOrderLineId equals jol.JobOrderLineId
                    where p.JobReceiveQALineId == id
                    select new JobReceiveQALineViewModel
                    {
                        JobReceiveDocNo = jrh.DocNo,
                        JobReceiveLineId = p.JobReceiveLineId,
                        JobReceiveQADocNo = p.JobReceiveQAHeader.DocNo,
                        JobReceiveQAHeaderId = p.JobReceiveQAHeaderId,
                        JobReceiveQALineId = p.JobReceiveQALineId,
                        ProductId = jol.ProductId,
                        Dimension1Name = jol.Dimension1.Dimension1Name,
                        Dimension2Name = jol.Dimension2.Dimension2Name,
                        Dimension3Name = jol.Dimension3.Dimension3Name,
                        Dimension4Name = jol.Dimension4.Dimension4Name,
                        Specification = jol.Specification,
                        BalanceQty = (tab == null) ? (p.Qty) : (tab.BalanceQty + p.Qty),
                        UnitDecimalPlaces = jol.Product.Unit.DecimalPlaces,
                        DealUnitDecimalPlaces = jol.DealUnit.DecimalPlaces,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        UnitId = jol.Product.UnitId,
                        DealUnitId = jol.DealUnitId,
                        DealQty = p.DealQty,
                        FailDealQty = p.FailDealQty,
                        ProductUidId = p.ProductUidId,
                        ProductUidName = p.ProductUid.ProductUidName,
                        InspectedQty = p.InspectedQty,
                        QAQty = p.QAQty,
                        FailQty = p.FailQty,
                        Qty = p.Qty,
                        Remark = p.Remark,
                        JobWorkerId = jrh.JobWorkerId,
                        Marks = p.Marks,
                        LockReason = p.LockReason,
                        Weight = p.Weight,
                        PenaltyAmt = p.PenaltyAmt,
                        PenaltyRate = p.PenaltyRate,
                    }
                        ).FirstOrDefault();
        }

        public IEnumerable<JobReceiveQALineViewModel> GetLineListForIndex(int headerId)
        {
            var pt = (from p in db.JobReceiveQALine
                      where p.JobReceiveQAHeaderId == headerId
                      join t in db.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                      into table
                      from tab in table.DefaultIfEmpty()
                      join jrh in db.JobReceiveHeader on tab.JobReceiveHeaderId equals jrh.JobReceiveHeaderId
                      join jol in db.JobOrderLine on tab.JobOrderLineId equals jol.JobOrderLineId
                      join prod in db.Product on jol.ProductId equals prod.ProductId
                      join unit in db.Units on prod.UnitId equals unit.UnitId
                      join dunit in db.Units on jol.DealUnitId equals dunit.UnitId
                      orderby p.Sr
                      select new JobReceiveQALineViewModel
                      {
                          JobReceiveQALineId = p.JobReceiveQALineId,
                          Qty = p.Qty,
                          DealQty = p.DealQty,
                          InspectedQty = p.InspectedQty,
                          Remark = p.Remark,
                          UnitConversionMultiplier = p.UnitConversionMultiplier,
                          JobReceiveDocNo = jrh.DocNo,
                          Dimension1Name = jol.Dimension1.Dimension1Name,
                          Dimension2Name = jol.Dimension2.Dimension2Name,
                          Dimension3Name = jol.Dimension3.Dimension3Name,
                          Dimension4Name = jol.Dimension4.Dimension4Name,
                          Specification = jol.Specification,
                          JobReceiveQAHeaderId = p.JobReceiveQAHeaderId,
                          ProductId = jol.ProductId,
                          ProductName = prod.ProductName,
                          UnitName = unit.UnitName,
                          ProductUidId = p.ProductUidId,
                          ProductUidName = p.ProductUid.ProductUidName,
                          UnitDecimalPlaces = unit.DecimalPlaces,
                          JobReceiveLineId = p.JobReceiveLineId,
                          Marks = p.Marks,
                          DealUnitName = dunit.UnitName,
                          DealUnitDecimalPlaces = dunit.DecimalPlaces,
                          Weight = p.Weight,
                          PenaltyRate = p.PenaltyRate,
                          PenaltyAmt = p.PenaltyAmt,
                          ReceiveHeaderId = tab.JobReceiveHeaderId,
                          ReceiveDocTypeId = jrh.DocTypeId,
                      }
                        );

            return pt;


        }

        public IEnumerable<JobReceiveQALineViewModel> GetJobReceiveLineForMultiSelect(JobReceiveQALineFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.JobReceiveHeaderId)) { SaleOrderIdArr = svm.JobReceiveHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }



            var temp = (from p in db.ViewJobReceiveBalanceForQA
                        join t in db.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId into table
                        from tab in table.DefaultIfEmpty()
                        join Jol in db.JobOrderLine on tab.JobOrderLineId equals Jol.JobOrderLineId
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join unit in db.Units on tab2.UnitId equals unit.UnitId
                        join dunit in db.Units on Jol.DealUnitId equals dunit.UnitId
                        join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (svm.JobWorkerId == 0 ? 1 == 1 : p.JobWorkerId == svm.JobWorkerId)
                        && (string.IsNullOrEmpty(svm.JobReceiveHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.JobReceiveHeaderId.ToString()))
                        && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0 && p.JobWorkerId == svm.JobWorkerId
                        orderby p.OrderDate, p.JobReceiveNo, tab.Sr
                        select new JobReceiveQALineViewModel
                        {
                            BalanceQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            JobReceiveDocNo = p.JobReceiveNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            JobReceiveQAHeaderId = svm.JobReceiveQAHeaderId,
                            JobReceiveLineId = p.JobReceiveLineId,
                            Dimension1Id = p.Dimension1Id,
                            Dimension2Id = p.Dimension2Id,
                            Dimension3Id = p.Dimension3Id,
                            Dimension4Id = p.Dimension4Id,
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            Specification = Jol.Specification,
                            UnitId = unit.UnitId,
                            UnitName = unit.UnitName,
                            DealUnitId = dunit.UnitId,
                            DealUnitName = dunit.UnitName,
                            UnitDecimalPlaces = unit.DecimalPlaces,
                            DealUnitDecimalPlaces = dunit.DecimalPlaces,
                            ProductUidName = (Jol.ProductUidHeaderId == null ? tab.ProductUid.ProductUidName : ""),
                            ProductUidId = tab.ProductUidId,
                            JobReceiveHeaderId = p.JobReceiveHeaderId,
                        }
                        );
            return temp;
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.JobReceiveQALine
                       where p.JobReceiveQAHeaderId == id
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

                var Temp = (from p in context.ViewJobReceiveBalanceForQA
                            join t in context.ProductUid on p.ProductUidId equals t.ProductUIDId
                            join t2 in context.JobReceiveLine on p.JobReceiveLineId equals t2.JobReceiveLineId
                            where id.Contains(p.JobReceiveLineId) && p.ProductUidId != null
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


        public JobReceiveQALineViewModel GetLineDetailForQA(int RecId, int ReceiveQAId)
        {

            var temp = from p in db.ViewJobReceiveBalanceForQA
                       join t1 in db.JobReceiveLine on p.JobReceiveLineId equals t1.JobReceiveLineId into JobReceiveLineTable
                       from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                       join t2 in db.Product on p.ProductId equals t2.ProductId into ProductTable
                       from ProductTab in ProductTable.DefaultIfEmpty()
                       join jol in db.JobOrderLine on JobReceiveLineTab.JobOrderLineId equals jol.JobOrderLineId into JobOrderLineTable
                       from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                       join D1 in db.Dimension1 on JobOrderLineTab.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                       from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                       join D2 in db.Dimension2 on JobOrderLineTab.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                       from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                       join D3 in db.Dimension3 on JobOrderLineTab.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                       from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                       join D4 in db.Dimension4 on JobOrderLineTab.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                       from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                       where p.JobReceiveLineId == RecId
                       select new JobReceiveQALineViewModel
                       {
                           Dimension1Name = Dimension1Tab.Dimension1Name,
                           Dimension2Name = Dimension2Tab.Dimension2Name,
                           Dimension3Name = Dimension3Tab.Dimension3Name,
                           Dimension4Name = Dimension4Tab.Dimension4Name,
                           Qty = p.BalanceQty,
                           Specification = JobOrderLineTab.Specification,
                           UnitId = ProductTab.UnitId,
                           DealUnitId = JobOrderLineTab.DealUnitId,
                           DealQty = p.BalanceQty * JobOrderLineTab.UnitConversionMultiplier,
                           UnitConversionMultiplier = JobOrderLineTab.UnitConversionMultiplier,
                           UnitName = ProductTab.Unit.UnitName,
                           DealUnitName = JobOrderLineTab.DealUnit.UnitName,
                           ProductId = p.ProductId,
                           ProductName = JobOrderLineTab.Product.ProductName,
                           UnitDecimalPlaces = ProductTab.Unit.DecimalPlaces,
                           DealUnitDecimalPlaces = JobOrderLineTab.DealUnit.DecimalPlaces,
                       };

            return (from p in db.ViewJobReceiveBalanceForQA
                    join t1 in db.JobReceiveLine on p.JobReceiveLineId equals t1.JobReceiveLineId into JobReceiveLineTable
                    from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                    join t2 in db.Product on p.ProductId equals t2.ProductId into ProductTable
                    from ProductTab in ProductTable.DefaultIfEmpty()
                    join jol in db.JobOrderLine on JobReceiveLineTab.JobOrderLineId equals jol.JobOrderLineId into JobOrderLineTable
                    from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                    join D1 in db.Dimension1 on JobOrderLineTab.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                    from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                    join D2 in db.Dimension2 on JobOrderLineTab.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                    from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                    join D3 in db.Dimension3 on JobOrderLineTab.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                    from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                    join D4 in db.Dimension4 on JobOrderLineTab.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                    from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                    where p.JobReceiveLineId == RecId
                    select new JobReceiveQALineViewModel
                    {
                        Dimension1Name = Dimension1Tab.Dimension1Name,
                        Dimension2Name = Dimension2Tab.Dimension2Name,
                        Dimension3Name = Dimension3Tab.Dimension3Name,
                        Dimension4Name = Dimension4Tab.Dimension4Name,
                        Qty = p.BalanceQty,
                        Specification = JobOrderLineTab.Specification,
                        UnitId = ProductTab.UnitId,
                        DealUnitId = JobOrderLineTab.DealUnitId,
                        DealQty = p.BalanceQty * JobOrderLineTab.UnitConversionMultiplier,
                        UnitConversionMultiplier = JobOrderLineTab.UnitConversionMultiplier,
                        UnitName = ProductTab.Unit.UnitName,
                        DealUnitName = JobOrderLineTab.DealUnit.UnitName,
                        ProductId = p.ProductId,
                        ProductName = JobOrderLineTab.Product.ProductName,
                        UnitDecimalPlaces = ProductTab.Unit.DecimalPlaces,
                        DealUnitDecimalPlaces = JobOrderLineTab.DealUnit.DecimalPlaces,
                    }
                  ).FirstOrDefault();



        }

        public UIDValidationViewModel ValidateQABarCode(string ProductUid, int HeaderId)
        {
            UIDValidationViewModel temp = new UIDValidationViewModel();

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var QAHeader = db.JobReceiveQAHeader.Find(HeaderId);
            temp = (from p in db.ViewJobReceiveBalanceForQA
                    join jo in db.JobReceiveHeader on p.JobReceiveHeaderId equals jo.JobReceiveHeaderId
                    join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                    join prod in db.Product on p.ProductId equals prod.ProductId
                    where t.ProductUidName == ProductUid && p.BalanceQty > 0
                    && p.SiteId == SiteId && p.DivisionId == DivisionId && jo.ProcessId == QAHeader.ProcessId
                    && p.JobWorkerId == QAHeader.JobWorkerId
                    select new UIDValidationViewModel
                    {
                        ProductId = p.ProductId,
                        ProductName = prod.ProductName,
                        ProductUIDId = t.ProductUIDId,
                        ProductUidName = t.ProductUidName,
                    }).FirstOrDefault();

            if (temp == null)
            {
                temp = new UIDValidationViewModel();
                var ProdUidExist = (from p in db.ProductUid
                                    where p.ProductUidName == ProductUid
                                    select p).FirstOrDefault();

                if (ProdUidExist == null)
                {
                    temp.ErrorType = "Error";
                    temp.ErrorMessage = "Invalid ProductUID";
                }
                else
                {
                    var ProdUIdJobWoker = (from p in db.ViewJobReceiveBalanceForQA
                                           join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                                           where t.ProductUidName == ProductUid
                                           select p).FirstOrDefault();
                    if (ProdUIdJobWoker == null)
                    {
                        temp.ErrorType = "Error";
                        temp.ErrorMessage = "BarCode not pending.";
                    }
                    else if (ProdUIdJobWoker.JobWorkerId != QAHeader.JobWorkerId)
                    {
                        temp.ErrorType = "Error";
                        temp.ErrorMessage = "Does not belong to JobWorker";
                    }
                    else if (ProdUIdJobWoker.BalanceQty <= 0)
                    {
                        temp.ErrorType = "Error";
                        temp.ErrorMessage = "ProductUid not pending for QA";
                    }
                }

            }
            else
            {
                temp.ErrorType = "Success";
            }

            return temp;

        }

        public JobReceiveQALineViewModel GetReceiveLineForUid(int Uid, int HeaderId)
        {

            var QaHeader = new JobReceiveQAHeaderService(db).Find(HeaderId);

            var Settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(QaHeader.DocTypeId, QaHeader.DivisionId, QaHeader.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { contraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(Settings.filterContraSites)) { contraSites = Settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDivisions)) { contraDivisions = Settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from t in db.JobReceiveLine
                         join v in db.ViewJobReceiveBalanceForQA on t.JobReceiveLineId equals v.JobReceiveLineId
                         join jrh in db.JobReceiveHeader on t.JobReceiveHeaderId equals jrh.JobReceiveHeaderId
                         where t.ProductUidId == Uid && v.BalanceQty > 0
                         && jrh.ProcessId == QaHeader.ProcessId
                         join Jol in db.JobReceiveLine on t.JobReceiveLineId equals Jol.JobReceiveLineId
                         select new
                         {
                             JobReceiveLineId = t.JobReceiveLineId,
                             JobReceiveDocNo = t.JobReceiveHeader.DocNo,
                             DocTypeId = jrh.DocTypeId,
                             SiteId = v.SiteId,
                             DivisionId = v.DivisionId,
                         });

            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));
            if (!string.IsNullOrEmpty(Settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == QaHeader.SiteId);
            if (!string.IsNullOrEmpty(Settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == QaHeader.DivisionId);


            return (from p in Query
                    select new JobReceiveQALineViewModel
                    {
                        JobReceiveLineId = p.JobReceiveLineId,
                        JobReceiveDocNo = p.JobReceiveDocNo,
                    }).FirstOrDefault();
        }

        public IEnumerable<ComboBoxResult> GetPendingProductsForJobReceiveQA(string term, int JobReceiveQAHeaderId)//DocTypeId
        {
            JobReceiveQAHeader header = new JobReceiveQAHeaderService(db).Find(JobReceiveQAHeaderId);

            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }


            var Query = (from p in db.ViewJobReceiveBalanceForQA
                         join prod in db.Product on p.ProductId equals prod.ProductId
                         join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
                         where p.BalanceQty > 0 && t.ProcessId == settings.ProcessId && p.JobWorkerId == header.JobWorkerId
                         select new
                         {
                             ProductName = prod.ProductName,
                             DocTypeId = p.DocTypeId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             ProductId = p.ProductId,

                         }
                           );

            if (!string.IsNullOrEmpty(term))
                Query = Query.Where(m => m.ProductName.ToLower().Contains(term.ToLower()));
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));
            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == header.SiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == header.DivisionId);

            return (from p in Query
                    group p by p.ProductId into g
                    orderby g.Max(m => m.ProductName)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.ProductName),
                        id = g.Key.ToString(),
                    });
        }

        public IQueryable<ComboBoxResult> GetPendingJobReceives(string term, int JobReceiveQAHeaderId)//DocTypeId
        {

            JobReceiveQAHeader header = new JobReceiveQAHeaderService(db).Find(JobReceiveQAHeaderId);

            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }


            var Query = (from p in db.ViewJobReceiveBalanceForQA
                         join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
                         where p.BalanceQty > 0 && t.ProcessId == settings.ProcessId && p.JobWorkerId == header.JobWorkerId
                         select new
                         {
                             DocNo = p.JobReceiveNo,
                             DocTypeId = p.DocTypeId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             JobReceiveHeaderId = p.JobReceiveHeaderId,

                         }
                           );

            if (!string.IsNullOrEmpty(term))
                Query = Query.Where(m => m.DocNo.ToLower().Contains(term.ToLower()));
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));
            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == header.SiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == header.DivisionId);

            return (from p in Query
                    group p by p.JobReceiveHeaderId into g
                    orderby g.Max(m => m.DocNo)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.DocNo),
                        id = g.Key.ToString(),
                    });
        }

        public IEnumerable<JobReceiveHeaderListViewModel> GetPendingJobReceivesForAC(int HeaderId, string term, int Limiter)//Product Id
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


            var QAHeader = db.JobReceiveQAHeader.Find(HeaderId);

            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(QAHeader.DocTypeId, QAHeader.DivisionId, QAHeader.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }


            var tem = (from p in db.ViewJobReceiveBalanceForQA
                       join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                       from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                       join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                       from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                       join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                       from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                       join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                       from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                       join t3 in db.Product on p.ProductId equals t3.ProductId
                       join t4 in db.JobReceiveHeader on p.JobReceiveHeaderId equals t4.JobReceiveHeaderId
                       where p.BalanceQty > 0 && p.JobWorkerId == QAHeader.JobWorkerId && t4.ProcessId == QAHeader.ProcessId
                       select new
                       {
                           DocTypeId = p.DocTypeId,
                           SiteId = p.SiteId,
                           DivisionId = p.DivisionId,
                           DocNo = p.JobReceiveNo,
                           JobReceiveLineId = p.JobReceiveLineId,
                           Dimension1Name = Dimension1Tab.Dimension1Name,
                           Dimension2Name = Dimension2Tab.Dimension2Name,
                           Dimension3Name = Dimension3Tab.Dimension3Name,
                           Dimension4Name = Dimension4Tab.Dimension4Name,
                           ProductName = t3.ProductName,
                           BalanceQty = p.BalanceQty,

                       });

            if (!string.IsNullOrEmpty(term))
            {
                tem = tem.Where(m => m.DocNo.ToLower().Contains(term.ToLower()) || m.Dimension1Name.ToLower().Contains(term.ToLower())
                    || m.Dimension2Name.ToLower().Contains(term.ToLower()) 
                    || m.Dimension2Name.ToLower().Contains(term.ToLower())
                    || m.Dimension3Name.ToLower().Contains(term.ToLower())
                    || m.Dimension4Name.ToLower().Contains(term.ToLower()) 
                    || m.ProductName.ToLower().Contains(term.ToLower()));
            }

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                tem = tem.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));
            if (!string.IsNullOrEmpty(settings.filterContraSites))
                tem = tem.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                tem = tem.Where(m => m.SiteId == SiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                tem = tem.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                tem = tem.Where(m => m.DivisionId == DivisionId);

            return (from p in tem
                    orderby p.DocNo
                    select new JobReceiveHeaderListViewModel
                        {
                            DocNo = p.DocNo,
                            JobReceiveLineId = p.JobReceiveLineId,
                            Dimension1Name = p.Dimension1Name,
                            Dimension2Name = p.Dimension2Name,
                            Dimension3Name = p.Dimension3Name,
                            Dimension4Name = p.Dimension4Name,
                            ProductName = p.ProductName,
                            BalanceQty = p.BalanceQty,
                        }).Take(Limiter);

        }


        public IQueryable<ComboBoxResult> GetPendingReceiveHelpList(int Id, string term)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobReceiveBalanceForQA
                         join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
                         where p.BalanceQty > 0 && t.ProcessId == settings.ProcessId
                         select new
                         {
                             DocNo = p.JobReceiveNo,
                             DocTypeId = p.DocTypeId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             JobReceiveHeaderId = p.JobReceiveHeaderId,

                         }
                          );

            if (!string.IsNullOrEmpty(term))
                Query = Query.Where(m => m.DocNo.ToLower().Contains(term.ToLower()));
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));
            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == CurrentSiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == CurrentDivisionId);

            return (from p in Query
                    group p by p.JobReceiveHeaderId into g
                    orderby g.Max(m => m.DocNo)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.DocNo),
                        id = g.Key.ToString(),
                    });
        }


        public IQueryable<ComboBoxResult> GetPendingJobWorkerHelpList(int Id, string term)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewJobReceiveBalanceForQA
                        join jrh in db.JobReceiveHeader on p.JobReceiveHeaderId equals jrh.JobReceiveHeaderId
                        join t in db.Persons on p.JobWorkerId equals t.PersonID
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.JobReceiveNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        && jrh.ProcessId == settings.ProcessId
                        group new { t } by t.PersonID into g
                        orderby g.Max(m => m.t.Name)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t.Name),
                            id = (g.Key).ToString(),
                        }
                          );

            return list;
        }


        public IQueryable<ComboBoxResult> GetPendingProductHelpList(int Id, string term)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewJobReceiveBalanceForQA
                        join jrh in db.JobReceiveHeader on p.JobReceiveHeaderId equals jrh.JobReceiveHeaderId
                        join t in db.Product on p.ProductId equals t.ProductId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.JobReceiveNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        && p.DivisionId == CurrentDivisionId && p.SiteId == CurrentSiteId
                        && jrh.ProcessId == settings.ProcessId
                        group new { t } by p.ProductId into g
                        orderby g.Max(m => m.t.ProductName)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t.ProductName),
                            id = (g.Key).ToString(),
                        }
                          );

            return list;
        }

        //public int? StockPost(int JobReceiveQAHeaderId)
        //{
        //    int? StockHeaderId = null;

        //    var Header = (from H in db.JobReceiveQAHeader
        //                  where H.JobReceiveQAHeaderId == JobReceiveQAHeaderId
        //                  select new
        //                  {
        //                      StockHeaderId = H.StockHeaderId,
        //                      JobReceiveQAHeaderId = H.JobReceiveQAHeaderId,
        //                      DocTypeId = H.DocTypeId,
        //                      DocDate = H.DocDate,
        //                      DocNo = H.DocNo,
        //                      DivisionId = H.DivisionId,
        //                      SiteId = H.SiteId,
        //                      ProcessId = H.ProcessId,
        //                      JobWorkerId = H.JobWorkerId,
        //                      Status = H.Status,
        //                      CreatedBy = H.CreatedBy,
        //                      ModifiedBy = H.ModifiedBy
        //                  }).FirstOrDefault();

        //    JobReceiveQASettings Settings = (from H in db.JobReceiveQASettings
        //                                     where H.DocTypeId == Header.DocTypeId &&
        //                                     H.SiteId == Header.SiteId &&
        //                                     H.DivisionId == Header.DivisionId
        //                                     select H).FirstOrDefault();

        //    IEnumerable<JobReceiveQALine> JobReceiveQALineList = (from L in db.JobReceiveQALine
        //                                                          where L.JobReceiveQAHeaderId == JobReceiveQAHeaderId
        //                                                          select L).ToList();



        //    foreach (JobReceiveQALine item in JobReceiveQALineList)
        //    {


        //        if (Settings.isPostedInStock ?? false && item.FailQty > 0)
        //        {
        //            JobOrderLine JobOrderLine = (from L in db.JobReceiveLine
        //                                         join Jol in db.JobOrderLine on L.JobOrderLineId equals Jol.JobOrderLineId
        //                                         where L.JobReceiveLineId == item.JobReceiveLineId
        //                                         select Jol).FirstOrDefault();

        //            int? Personid = (from H in db.JobOrderHeaderExtended
        //                            where H.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId
        //                            select H).FirstOrDefault().PersonId;

        //            JobReceiveLine JobReceiveLine = (from L in db.JobReceiveLine
        //                                             where L.JobReceiveLineId == item.JobReceiveLineId
        //                                             select L).FirstOrDefault();


        //            int GodownId = (from L in db.JobReceiveLine
        //                            join H in db.JobReceiveHeader on L.JobReceiveHeaderId equals H.JobReceiveHeaderId
        //                            select H.GodownId).FirstOrDefault();

        //            StockViewModel StockViewModel_IssQty = new StockViewModel();

        //            StockViewModel_IssQty.StockId = -1;
        //            StockViewModel_IssQty.StockHeaderId = Header.StockHeaderId ?? 0;
        //            StockViewModel_IssQty.DocHeaderId = Header.JobReceiveQAHeaderId;
        //            StockViewModel_IssQty.DocLineId = item.JobReceiveLineId;
        //            StockViewModel_IssQty.DocTypeId = Header.DocTypeId;
        //            StockViewModel_IssQty.StockHeaderDocDate = Header.DocDate;
        //            StockViewModel_IssQty.StockDocDate = DateTime.Now.Date;
        //            StockViewModel_IssQty.DocNo = Header.DocNo;
        //            StockViewModel_IssQty.DivisionId = Header.DivisionId;
        //            StockViewModel_IssQty.SiteId = Header.SiteId;
        //            StockViewModel_IssQty.CurrencyId = null;
        //            StockViewModel_IssQty.HeaderProcessId = Header.ProcessId;
        //            StockViewModel_IssQty.PersonId = Personid ?? Header.JobWorkerId;
        //            StockViewModel_IssQty.ProductId = JobOrderLine.ProductId;
        //            StockViewModel_IssQty.HeaderFromGodownId = null;
        //            StockViewModel_IssQty.HeaderGodownId = null;
        //            StockViewModel_IssQty.GodownId = GodownId;
        //            StockViewModel_IssQty.ProcessId = Header.ProcessId;
        //            StockViewModel_IssQty.LotNo = JobReceiveLine.LotNo;
        //            StockViewModel_IssQty.CostCenterId = null;
        //            StockViewModel_IssQty.Qty_Iss = item.QAQty;
        //            StockViewModel_IssQty.Qty_Rec = 0;
        //            StockViewModel_IssQty.Rate = 0;
        //            StockViewModel_IssQty.StockStatus = null;
        //            StockViewModel_IssQty.ExpiryDate = null;
        //            StockViewModel_IssQty.Specification = null;
        //            StockViewModel_IssQty.Dimension1Id = JobOrderLine.Dimension1Id;
        //            StockViewModel_IssQty.Dimension2Id = JobOrderLine.Dimension2Id;
        //            StockViewModel_IssQty.ReferenceDocId = item.JobReceiveQALineId;
        //            StockViewModel_IssQty.Remark = null;
        //            StockViewModel_IssQty.Status = Header.Status;
        //            StockViewModel_IssQty.ProductUidId = JobOrderLine.ProductUidId;
        //            StockViewModel_IssQty.CreatedBy = Header.CreatedBy;
        //            StockViewModel_IssQty.CreatedDate = DateTime.Now;
        //            StockViewModel_IssQty.ModifiedBy = Header.ModifiedBy;
        //            StockViewModel_IssQty.ModifiedDate = DateTime.Now;
                    

        //            string StockPostingError = "";
        //            StockPostingError = _stockService.StockPostDB(ref StockViewModel_IssQty, ref db);

        //            StockHeaderId = StockViewModel_IssQty.StockHeaderId;


        //            //For Posting Entry In Stock Adjustment Table Which Was received from Job Receive.
        //            if (JobReceiveLine.StockId != null)
        //            {
        //                StockAdj Adj_IssQty = new StockAdj();
        //                Adj_IssQty.StockInId = (int)JobReceiveLine.StockId;
        //                Adj_IssQty.StockOutId = (int)StockViewModel_IssQty.StockId;
        //                Adj_IssQty.DivisionId = StockViewModel_IssQty.DivisionId;
        //                Adj_IssQty.SiteId = StockViewModel_IssQty.SiteId;
        //                Adj_IssQty.AdjustedQty = StockViewModel_IssQty.Qty_Iss;
        //                Adj_IssQty.ObjectState = ObjectState.Added;
        //                db.StockAdj.Add(Adj_IssQty);
        //            }
                    

        //            //End Stock Adj Code

        //            StockViewModel StockViewModel_RecFailedQty = new StockViewModel();

        //            StockViewModel_RecFailedQty.StockId = -2;
        //            StockViewModel_RecFailedQty.StockHeaderId = Header.StockHeaderId ?? -1;
        //            StockViewModel_RecFailedQty.DocHeaderId = Header.JobReceiveQAHeaderId;
        //            StockViewModel_RecFailedQty.DocLineId = item.JobReceiveLineId;
        //            StockViewModel_RecFailedQty.DocTypeId = Header.DocTypeId;
        //            StockViewModel_RecFailedQty.StockHeaderDocDate = Header.DocDate;
        //            StockViewModel_RecFailedQty.StockDocDate = DateTime.Now.Date;
        //            StockViewModel_RecFailedQty.DocNo = Header.DocNo;
        //            StockViewModel_RecFailedQty.DivisionId = Header.DivisionId;
        //            StockViewModel_RecFailedQty.SiteId = Header.SiteId;
        //            StockViewModel_RecFailedQty.CurrencyId = null;
        //            StockViewModel_RecFailedQty.HeaderProcessId = Header.ProcessId;
        //            StockViewModel_RecFailedQty.PersonId = Header.JobWorkerId;
        //            StockViewModel_RecFailedQty.ProductId = JobOrderLine.ProductId;
        //            StockViewModel_RecFailedQty.HeaderFromGodownId = null;
        //            StockViewModel_RecFailedQty.HeaderGodownId = null;
        //            StockViewModel_RecFailedQty.GodownId = GodownId;
        //            StockViewModel_RecFailedQty.ProcessId = Header.ProcessId;
        //            StockViewModel_RecFailedQty.LotNo = JobReceiveLine.LotNo;
        //            StockViewModel_RecFailedQty.CostCenterId = null;
        //            StockViewModel_RecFailedQty.Qty_Iss = 0;
        //            StockViewModel_RecFailedQty.Qty_Rec = item.FailQty;
        //            StockViewModel_RecFailedQty.Rate = 0;
        //            StockViewModel_RecFailedQty.StockStatus = StockStatusConstants.Failed;
        //            StockViewModel_RecFailedQty.ExpiryDate = null;
        //            StockViewModel_RecFailedQty.Specification = null;
        //            StockViewModel_RecFailedQty.Dimension1Id = JobOrderLine.Dimension1Id;
        //            StockViewModel_RecFailedQty.Dimension2Id = JobOrderLine.Dimension2Id;
        //            StockViewModel_RecFailedQty.ReferenceDocId = item.JobReceiveQALineId;
        //            StockViewModel_RecFailedQty.Remark = null;
        //            StockViewModel_RecFailedQty.Status = Header.Status;
        //            StockViewModel_RecFailedQty.ProductUidId = JobOrderLine.ProductUidId;
        //            StockViewModel_RecFailedQty.CreatedBy = Header.CreatedBy;
        //            StockViewModel_RecFailedQty.CreatedDate = DateTime.Now;
        //            StockViewModel_RecFailedQty.ModifiedBy = Header.ModifiedBy;
        //            StockViewModel_RecFailedQty.ModifiedDate = DateTime.Now;

        //            StockPostingError = StockPostingError + _stockService.StockPostDB(ref StockViewModel_RecFailedQty, ref db);


        //            StockViewModel StockViewModel_RecPassedQty = new StockViewModel();

        //            StockViewModel_RecPassedQty.StockId = -3;
        //            StockViewModel_RecPassedQty.StockHeaderId = Header.StockHeaderId ?? -1;
        //            StockViewModel_RecPassedQty.DocHeaderId = Header.JobReceiveQAHeaderId;
        //            StockViewModel_RecPassedQty.DocLineId = item.JobReceiveLineId;
        //            StockViewModel_RecPassedQty.DocTypeId = Header.DocTypeId;
        //            StockViewModel_RecPassedQty.StockHeaderDocDate = Header.DocDate;
        //            StockViewModel_RecPassedQty.StockDocDate = DateTime.Now.Date;
        //            StockViewModel_RecPassedQty.DocNo = Header.DocNo;
        //            StockViewModel_RecPassedQty.DivisionId = Header.DivisionId;
        //            StockViewModel_RecPassedQty.SiteId = Header.SiteId;
        //            StockViewModel_RecPassedQty.CurrencyId = null;
        //            StockViewModel_RecPassedQty.HeaderProcessId = Header.ProcessId;
        //            StockViewModel_RecPassedQty.PersonId = Header.JobWorkerId;
        //            StockViewModel_RecPassedQty.ProductId = JobOrderLine.ProductId;
        //            StockViewModel_RecPassedQty.HeaderFromGodownId = null;
        //            StockViewModel_RecPassedQty.HeaderGodownId = null;
        //            StockViewModel_RecPassedQty.GodownId = GodownId;
        //            StockViewModel_RecPassedQty.ProcessId = Header.ProcessId;
        //            StockViewModel_RecPassedQty.LotNo = JobReceiveLine.LotNo;
        //            StockViewModel_RecPassedQty.CostCenterId = null;
        //            StockViewModel_RecPassedQty.Qty_Iss = 0;
        //            StockViewModel_RecPassedQty.Qty_Rec = item.QAQty - item.FailQty;
        //            StockViewModel_RecPassedQty.Rate = 0;
        //            StockViewModel_RecPassedQty.StockStatus = null;
        //            StockViewModel_RecPassedQty.ExpiryDate = null;
        //            StockViewModel_RecPassedQty.Specification = null;
        //            StockViewModel_RecPassedQty.Dimension1Id = JobOrderLine.Dimension1Id;
        //            StockViewModel_RecPassedQty.Dimension2Id = JobOrderLine.Dimension2Id;
        //            StockViewModel_RecPassedQty.ReferenceDocId = item.JobReceiveQALineId;
        //            StockViewModel_RecPassedQty.Remark = null;
        //            StockViewModel_RecPassedQty.Status = Header.Status;
        //            StockViewModel_RecPassedQty.ProductUidId = JobOrderLine.ProductUidId;
        //            StockViewModel_RecPassedQty.CreatedBy = Header.CreatedBy;
        //            StockViewModel_RecPassedQty.CreatedDate = DateTime.Now;
        //            StockViewModel_RecPassedQty.ModifiedBy = Header.ModifiedBy;
        //            StockViewModel_RecPassedQty.ModifiedDate = DateTime.Now;

        //            StockPostingError = StockPostingError + _stockService.StockPostDB(ref StockViewModel_RecPassedQty, ref db);


                    


        //        }
        //    }
        //    return StockHeaderId;
        //}

        public int? StockPost(int JobReceiveQAHeaderId)
        {
            int? StockHeaderId = null;

            var Header = (from H in db.JobReceiveQAHeader
                          where H.JobReceiveQAHeaderId == JobReceiveQAHeaderId
                          select new
                          {
                              StockHeaderId = H.StockHeaderId,
                              JobReceiveQAHeaderId = H.JobReceiveQAHeaderId,
                              DocTypeId = H.DocTypeId,
                              DocDate = H.DocDate,
                              DocNo = H.DocNo,
                              DivisionId = H.DivisionId,
                              SiteId = H.SiteId,
                              ProcessId = H.ProcessId,
                              JobWorkerId = H.JobWorkerId,
                              Status = H.Status,
                              CreatedBy = H.CreatedBy,
                              ModifiedBy = H.ModifiedBy
                          }).FirstOrDefault();

            JobReceiveQASettings Settings = (from H in db.JobReceiveQASettings
                                             where H.DocTypeId == Header.DocTypeId &&
                                             H.SiteId == Header.SiteId &&
                                             H.DivisionId == Header.DivisionId
                                             select H).FirstOrDefault();

            IEnumerable<JobReceiveQALine> JobReceiveQALineList = (from L in db.JobReceiveQALine
                                                                  where L.JobReceiveQAHeaderId == JobReceiveQAHeaderId
                                                                  select L).ToList();



            foreach (JobReceiveQALine item in JobReceiveQALineList)
            {


                if (Settings.isPostedInStock ?? false && item.FailQty > 0)
                {
                    JobOrderLine JobOrderLine = (from L in db.JobReceiveLine
                                                 join Jol in db.JobOrderLine on L.JobOrderLineId equals Jol.JobOrderLineId
                                                 where L.JobReceiveLineId == item.JobReceiveLineId
                                                 select Jol).FirstOrDefault();

                    int? Personid = (from H in db.JobOrderHeaderExtended
                                     where H.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId
                                     select H).FirstOrDefault().PersonId;

                    JobReceiveLine JobReceiveLine = (from L in db.JobReceiveLine
                                                     where L.JobReceiveLineId == item.JobReceiveLineId
                                                     select L).FirstOrDefault();


                    int GodownId = (from L in db.JobReceiveLine
                                    join H in db.JobReceiveHeader on L.JobReceiveHeaderId equals H.JobReceiveHeaderId
                                    select H.GodownId).FirstOrDefault();

                    StockViewModel StockViewModel_IssQty = new StockViewModel();

                    StockViewModel_IssQty.StockId = -1;
                    StockViewModel_IssQty.StockHeaderId = Header.StockHeaderId ?? 0;
                    StockViewModel_IssQty.DocHeaderId = Header.JobReceiveQAHeaderId;
                    StockViewModel_IssQty.DocLineId = item.JobReceiveQALineId;
                    StockViewModel_IssQty.DocTypeId = Header.DocTypeId;
                    StockViewModel_IssQty.StockHeaderDocDate = Header.DocDate;
                    StockViewModel_IssQty.StockDocDate = Header.DocDate;
                    StockViewModel_IssQty.DocNo = Header.DocNo;
                    StockViewModel_IssQty.DivisionId = Header.DivisionId;
                    StockViewModel_IssQty.SiteId = Header.SiteId;
                    StockViewModel_IssQty.CurrencyId = null;
                    StockViewModel_IssQty.HeaderProcessId = Header.ProcessId;
                    StockViewModel_IssQty.PersonId = Personid ?? Header.JobWorkerId;
                    StockViewModel_IssQty.ProductId = JobOrderLine.ProductId;
                    StockViewModel_IssQty.HeaderFromGodownId = null;
                    StockViewModel_IssQty.HeaderGodownId = null;
                    StockViewModel_IssQty.GodownId = GodownId;
                    StockViewModel_IssQty.ProcessId = Header.ProcessId;
                    StockViewModel_IssQty.LotNo = JobReceiveLine.LotNo;
                    StockViewModel_IssQty.CostCenterId = null;
                    StockViewModel_IssQty.Qty_Iss = item.FailQty;
                    StockViewModel_IssQty.Qty_Rec = 0;
                    StockViewModel_IssQty.Rate = 0;
                    StockViewModel_IssQty.StockStatus = null;
                    StockViewModel_IssQty.ExpiryDate = null;
                    StockViewModel_IssQty.Specification = null;
                    StockViewModel_IssQty.Dimension1Id = JobOrderLine.Dimension1Id;
                    StockViewModel_IssQty.Dimension2Id = JobOrderLine.Dimension2Id;
                    StockViewModel_IssQty.Dimension3Id = JobOrderLine.Dimension3Id;
                    StockViewModel_IssQty.Dimension4Id = JobOrderLine.Dimension4Id;
                    StockViewModel_IssQty.ReferenceDocId = item.JobReceiveQALineId;
                    StockViewModel_IssQty.Remark = null;
                    StockViewModel_IssQty.Status = Header.Status;
                    StockViewModel_IssQty.ProductUidId = JobOrderLine.ProductUidId;
                    StockViewModel_IssQty.CreatedBy = Header.CreatedBy;
                    StockViewModel_IssQty.CreatedDate = DateTime.Now;
                    StockViewModel_IssQty.ModifiedBy = Header.ModifiedBy;
                    StockViewModel_IssQty.ModifiedDate = DateTime.Now;


                    string StockPostingError = "";
                    StockPostingError = _stockService.StockPostDB(ref StockViewModel_IssQty, ref db);

                    StockHeaderId = StockViewModel_IssQty.StockHeaderId;


                    //For Posting Entry In Stock Adjustment Table Which Was received from Job Receive.
                    if (JobReceiveLine.StockId != null)
                    {
                        StockAdj Adj_IssQty = new StockAdj();
                        Adj_IssQty.StockInId = (int)JobReceiveLine.StockId;
                        Adj_IssQty.StockOutId = (int)StockViewModel_IssQty.StockId;
                        Adj_IssQty.DivisionId = StockViewModel_IssQty.DivisionId;
                        Adj_IssQty.SiteId = StockViewModel_IssQty.SiteId;
                        Adj_IssQty.AdjustedQty = StockViewModel_IssQty.Qty_Iss;
                        Adj_IssQty.ObjectState = ObjectState.Added;
                        db.StockAdj.Add(Adj_IssQty);
                    }


                    //End Stock Adj Code

                    StockViewModel StockViewModel_RecFailedQty = new StockViewModel();

                    StockViewModel_RecFailedQty.StockId = -2;
                    StockViewModel_RecFailedQty.StockHeaderId = Header.StockHeaderId ?? -1;
                    StockViewModel_RecFailedQty.DocHeaderId = Header.JobReceiveQAHeaderId;
                    StockViewModel_RecFailedQty.DocLineId = item.JobReceiveQALineId;
                    StockViewModel_RecFailedQty.DocTypeId = Header.DocTypeId;
                    StockViewModel_RecFailedQty.StockHeaderDocDate = Header.DocDate;
                    StockViewModel_RecFailedQty.StockDocDate = Header.DocDate;
                    StockViewModel_RecFailedQty.DocNo = Header.DocNo;
                    StockViewModel_RecFailedQty.DivisionId = Header.DivisionId;
                    StockViewModel_RecFailedQty.SiteId = Header.SiteId;
                    StockViewModel_RecFailedQty.CurrencyId = null;
                    StockViewModel_RecFailedQty.HeaderProcessId = Header.ProcessId;
                    StockViewModel_RecFailedQty.PersonId = Header.JobWorkerId;
                    StockViewModel_RecFailedQty.ProductId = JobOrderLine.ProductId;
                    StockViewModel_RecFailedQty.HeaderFromGodownId = null;
                    StockViewModel_RecFailedQty.HeaderGodownId = null;
                    StockViewModel_RecFailedQty.GodownId = GodownId;
                    StockViewModel_RecFailedQty.ProcessId = Header.ProcessId;
                    StockViewModel_RecFailedQty.LotNo = JobReceiveLine.LotNo;
                    StockViewModel_RecFailedQty.CostCenterId = null;
                    StockViewModel_RecFailedQty.Qty_Iss = 0;
                    StockViewModel_RecFailedQty.Qty_Rec = item.FailQty;
                    StockViewModel_RecFailedQty.Rate = 0;
                    StockViewModel_RecFailedQty.StockStatus = StockStatusConstants.Failed;
                    StockViewModel_RecFailedQty.ExpiryDate = null;
                    StockViewModel_RecFailedQty.Specification = null;
                    StockViewModel_RecFailedQty.Dimension1Id = JobOrderLine.Dimension1Id;
                    StockViewModel_RecFailedQty.Dimension2Id = JobOrderLine.Dimension2Id;
                    StockViewModel_RecFailedQty.Dimension3Id = JobOrderLine.Dimension3Id;
                    StockViewModel_RecFailedQty.Dimension4Id = JobOrderLine.Dimension4Id;
                    StockViewModel_RecFailedQty.ReferenceDocId = item.JobReceiveQALineId;
                    StockViewModel_RecFailedQty.Remark = null;
                    StockViewModel_RecFailedQty.Status = Header.Status;
                    StockViewModel_RecFailedQty.ProductUidId = JobOrderLine.ProductUidId;
                    StockViewModel_RecFailedQty.CreatedBy = Header.CreatedBy;
                    StockViewModel_RecFailedQty.CreatedDate = DateTime.Now;
                    StockViewModel_RecFailedQty.ModifiedBy = Header.ModifiedBy;
                    StockViewModel_RecFailedQty.ModifiedDate = DateTime.Now;

                    StockPostingError = StockPostingError + _stockService.StockPostDB(ref StockViewModel_RecFailedQty, ref db);
                }
            }
            return StockHeaderId;
        }

        public Task<IEquatable<JobReceiveQALine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveQALine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
