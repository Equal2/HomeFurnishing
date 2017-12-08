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
    public interface IJobOrderInspectionLineService : IDisposable
    {
        JobOrderInspectionLine Create(JobOrderInspectionLine pt, string UserName);
        void Delete(int id);
        void Delete(JobOrderInspectionLine pt);
        JobOrderInspectionLine Find(int id);
        void Update(JobOrderInspectionLine pt, string UserName);
        Task<IEquatable<JobOrderInspectionLine>> GetAsync();
        Task<JobOrderInspectionLine> FindAsync(int id);
        JobOrderInspectionLineViewModel GetJobOrderInspectionLine(int id);
        List<ComboBoxList> GetPendingBarCodesList(int[] id);
        List<ComboBoxList> GetPendingBarCodesListForInspectionRequest(int[] id);
        int GetMaxSr(int id);
        IEnumerable<JobOrderInspectionLineViewModel> GetLineListForIndex(int headerId);
        JobOrderInspectionLineViewModel GetLineDetailForInspection(int id, int ReceiveId, bool InsReq);
        UIDValidationViewModel ValidateInspectionBarCode(string ProductUid, int HeaderId);
        JobOrderInspectionLineViewModel GetOrderLineForUid(int Uid);
        IEnumerable<JobOrderInspectionLineViewModel> GetJobOrderLineForMultiSelect(JobOrderInspectionLineFilterViewModel svm);
        IEnumerable<JobOrderInspectionLineViewModel> GetJobRequestLineForMultiSelect(JobOrderInspectionLineFilterViewModel svm);
        IQueryable<ComboBoxResult> GetPendingProductsForJobOrderInspection(string term, int JobOrderInspectionHeaderId);
        IQueryable<ComboBoxResult> GetPendingProductsForInspection(string term, int JobOrderInspectionHeaderId);
        IQueryable<ComboBoxResult> GetPendingJobOrders(string term, int JobOrderInspectionHeaderId);
        IQueryable<ComboBoxResult> GetPendingJobRequests(string term, int JobOrderInspectionHeaderId);
        IEnumerable<JobOrderHeaderListViewModel> GetPendingJobOrdersForAC(int HeaderId, string term, int Limiter, bool InsReq);
    }

    public class JobOrderInspectionLineService : IJobOrderInspectionLineService
    {
        private ApplicationDbContext db;
        public JobOrderInspectionLineService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public JobOrderInspectionLine Find(int id)
        {
            return db.JobOrderInspectionLine.Find(id);
        }

        public JobOrderInspectionLine Create(JobOrderInspectionLine pt, string UserName)
        {
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Added;
            db.JobOrderInspectionLine.Add(pt);

            return pt;
        }

        public void Delete(int id)
        {
            var line = db.JobOrderInspectionLine.Find(id);
            line.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionLine.Remove(line);
        }

        public void Delete(JobOrderInspectionLine pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionLine.Remove(pt);
        }

        public void Update(JobOrderInspectionLine pt, string UserName)
        {
            pt.ModifiedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Modified;
            db.JobOrderInspectionLine.Add(pt);
        }

        public JobOrderInspectionLineViewModel GetJobOrderInspectionLine(int id)
        {
            return (from p in db.JobOrderInspectionLine
                    join t in db.ViewJobOrderBalanceForInspection on p.JobOrderLineId equals t.JobOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t2 in db.ViewJobOrderInspectionRequestBalance on p.JobOrderLineId equals t2.JobOrderLineId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    where p.JobOrderInspectionLineId == id
                    select new JobOrderInspectionLineViewModel
                    {
                        JobOrderDocNo = p.JobOrderLine.JobOrderHeader.DocNo,
                        JobOrderLineId = p.JobOrderLineId,
                        JobOrderInspectionDocNo = p.JobOrderInspectionHeader.DocNo,
                        JobOrderInspectionHeaderId = p.JobOrderInspectionHeaderId,
                        JobOrderInspectionLineId = p.JobOrderInspectionLineId,
                        ProductId = p.JobOrderLine.ProductId,
                        Dimension1Name = p.JobOrderLine.Dimension1.Dimension1Name,
                        Dimension2Name = p.JobOrderLine.Dimension2.Dimension2Name,
                        Dimension3Name = p.JobOrderLine.Dimension3.Dimension3Name,
                        Dimension4Name = p.JobOrderLine.Dimension4.Dimension4Name,
                        Specification = p.JobOrderLine.Specification,
                        BalanceQty = (tab2 == null ? tab.BalanceQty : tab2.BalanceQty) + p.Qty,
                        UnitDecimalPlaces = p.JobOrderLine.Product.Unit.DecimalPlaces,
                        DealUnitDecimalPlaces = p.JobOrderLine.DealUnit.DecimalPlaces,
                        UnitConversionMultiplier = p.JobOrderLine.UnitConversionMultiplier,
                        UnitId = p.JobOrderLine.Product.UnitId,
                        DealUnitId = p.JobOrderLine.DealUnitId,
                        DealQty = p.Qty * p.JobOrderLine.UnitConversionMultiplier,
                        ProductUidId = p.ProductUidId,
                        ProductUidName = p.ProductUid.ProductUidName,
                        InspectedQty = p.InspectedQty,
                        Qty = p.Qty,
                        Remark = p.Remark,
                        JobWorkerId = p.JobOrderLine.JobOrderHeader.JobWorkerId,
                        Marks = p.Marks,
                        JobOrderInspectionRequestDocNo = tab2.JobOrderInspectionRequestNo,
                        JobOrderInspectionRequestLineId = p.JobOrderInspectionRequestLineId,
                        LockReason = p.LockReason,
                    }
                        ).FirstOrDefault();
        }

        public IEnumerable<JobOrderInspectionLineViewModel> GetLineListForIndex(int headerId)
        {
            var pt = (from p in db.JobOrderInspectionLine
                      where p.JobOrderInspectionHeaderId == headerId
                      join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                      into table
                      from tab in table.DefaultIfEmpty()
                      orderby p.Sr
                      select new JobOrderInspectionLineViewModel
                      {
                          JobOrderInspectionLineId = p.JobOrderInspectionLineId,
                          Qty = p.Qty,
                          InspectedQty = p.InspectedQty,
                          Remark = p.Remark,
                          UnitConversionMultiplier = p.JobOrderLine.UnitConversionMultiplier,
                          JobOrderDocNo = p.JobOrderLine.JobOrderHeader.DocNo,
                          Dimension1Name = p.JobOrderLine.Dimension1.Dimension1Name,
                          Dimension2Name = p.JobOrderLine.Dimension2.Dimension2Name,
                          Dimension3Name = p.JobOrderLine.Dimension3.Dimension3Name,
                          Dimension4Name = p.JobOrderLine.Dimension4.Dimension4Name,
                          Specification = p.JobOrderLine.Specification,
                          JobOrderInspectionHeaderId = p.JobOrderInspectionHeaderId,
                          ProductId = p.JobOrderLine.ProductId,
                          ProductName = p.JobOrderLine.Product.ProductName,
                          UnitName = p.JobOrderLine.Product.Unit.UnitName,
                          ProductUidId = p.ProductUidId,
                          ProductUidName = p.ProductUid.ProductUidName,
                          UnitDecimalPlaces = p.JobOrderLine.Product.Unit.DecimalPlaces,
                          JobOrderLineId = p.JobOrderLineId,
                          Marks = p.Marks,
                          JobOrderInspectionRequestDocNo = p.JobOrderInspectionRequestLine.JobOrderInspectionRequestHeader.DocNo,
                      }
                        );

            return pt;


        }

        public IEnumerable<JobOrderInspectionLineViewModel> GetJobOrderLineForMultiSelect(JobOrderInspectionLineFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.JobOrderHeaderId)) { SaleOrderIdArr = svm.JobOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var InsHeader = db.JobOrderInspectionHeader.Find(svm.JobOrderInspectionHeaderId);

            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(InsHeader.DocTypeId, InsHeader.DivisionId, InsHeader.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderBalanceForInspection
                         join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId into table
                         from tab in table.DefaultIfEmpty()
                         join t2 in db.JobOrderHeader on tab.JobOrderHeaderId equals t2.JobOrderHeaderId
                         join product in db.Product on p.ProductId equals product.ProductId into table2
                         from tab2 in table2.DefaultIfEmpty()
                         where p.BalanceQty > 0 && p.JobWorkerId == svm.JobWorkerId && t2.ProcessId == InsHeader.ProcessId
                         orderby p.OrderDate, p.JobOrderNo, tab.Sr
                         select new
                         {
                             BalanceQty = p.BalanceQty,
                             Qty = p.BalanceQty,
                             JobOrderDocNo = p.JobOrderNo,
                             ProductName = tab2.ProductName,
                             ProductId = p.ProductId,
                             JobOrderInspectionHeaderId = svm.JobOrderInspectionHeaderId,
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
                             UnitDecimalPlaces = tab2.Unit.DecimalPlaces,
                             DealUnitDecimalPlaces = tab.DealUnit.DecimalPlaces,
                             ProductUidName = (tab.ProductUidHeaderId == null ? tab.ProductUid.ProductUidName : ""),
                             ProductUidId = tab.ProductUidId,
                             JobOrderHeaderId = p.JobOrderHeaderId,
                             DocTypeId = t2.DocTypeId,
                             SiteId = t2.SiteId,
                             DivisionId = t2.DivisionId,
                             ProductGroupId = tab2.ProductGroupId,
                         }
                        );

            if (!string.IsNullOrEmpty(svm.ProductId))
                Query = Query.Where(m => ProductIdArr.Contains(m.ProductId.ToString()));

            if (!string.IsNullOrEmpty(svm.JobOrderHeaderId))
                Query = Query.Where(m => SaleOrderIdArr.Contains(m.JobOrderHeaderId.ToString()));

            if (!string.IsNullOrEmpty(svm.ProductGroupId))
                Query = Query.Where(m => ProductGroupIdArr.Contains(m.ProductGroupId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == InsHeader.SiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == InsHeader.DivisionId);

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));


            return (from p in Query
                    select new JobOrderInspectionLineViewModel
                    {
                        BalanceQty = p.BalanceQty,
                        Qty = p.BalanceQty,
                        JobOrderDocNo = p.JobOrderDocNo,
                        ProductName = p.ProductName,
                        ProductId = p.ProductId,
                        JobOrderInspectionHeaderId = p.JobOrderInspectionHeaderId,
                        JobOrderLineId = p.JobOrderLineId,
                        Dimension1Id = p.Dimension1Id,
                        Dimension2Id = p.Dimension2Id,
                        Dimension3Id = p.Dimension3Id,
                        Dimension4Id = p.Dimension4Id,
                        Dimension1Name = p.Dimension1Name,
                        Dimension2Name = p.Dimension2Name,
                        Dimension3Name = p.Dimension3Name,
                        Dimension4Name = p.Dimension4Name,
                        Specification = p.Specification,
                        UnitId = p.UnitId,
                        UnitName = p.UnitName,
                        UnitDecimalPlaces = p.UnitDecimalPlaces,
                        DealUnitDecimalPlaces = p.DealUnitDecimalPlaces,
                        ProductUidName = p.ProductUidName,
                        ProductUidId = p.ProductUidId,
                        JobOrderHeaderId = p.JobOrderHeaderId,
                    });
        }

        public IEnumerable<JobOrderInspectionLineViewModel> GetJobRequestLineForMultiSelect(JobOrderInspectionLineFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.JobOrderInspectionRequestHeaderId)) { SaleOrderIdArr = svm.JobOrderInspectionRequestHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var InsHeader = db.JobOrderInspectionHeader.Find(svm.JobOrderInspectionHeaderId);

            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(InsHeader.DocTypeId, InsHeader.DivisionId, InsHeader.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderInspectionRequestBalance
                         join t in db.JobOrderInspectionRequestLine on p.JobOrderInspectionRequestLineId equals t.JobOrderInspectionRequestLineId into table
                         from tab in table.DefaultIfEmpty()
                         join jir in db.JobOrderInspectionRequestHeader on p.JobOrderInspectionRequestHeaderId equals jir.JobOrderInspectionRequestHeaderId
                         join Jol in db.JobOrderLine on tab.JobOrderLineId equals Jol.JobOrderLineId
                         join product in db.Product on p.ProductId equals product.ProductId into table2
                         from tab2 in table2.DefaultIfEmpty()
                         where p.BalanceQty > 0 && p.JobWorkerId == svm.JobWorkerId && jir.ProcessId == InsHeader.ProcessId
                         orderby p.RequestDate, p.JobOrderInspectionRequestNo, tab.Sr
                         select new
                         {
                             BalanceQty = p.BalanceQty,
                             Qty = p.BalanceQty,
                             JobOrderInspectionRequestDocNo = p.JobOrderInspectionRequestNo,
                             ProductName = tab2.ProductName,
                             ProductId = p.ProductId,
                             JobOrderInspectionHeaderId = svm.JobOrderInspectionHeaderId,
                             JobOrderInspectionRequestLineId = p.JobOrderInspectionRequestLineId,
                             Dimension1Id = p.Dimension1Id,
                             Dimension2Id = p.Dimension2Id,
                             Dimension3Id = p.Dimension3Id,
                             Dimension4Id = p.Dimension4Id,
                             Dimension1Name = p.Dimension1.Dimension1Name,
                             Dimension2Name = p.Dimension2.Dimension2Name,
                             Dimension3Name = p.Dimension3.Dimension3Name,
                             Dimension4Name = p.Dimension4.Dimension4Name,
                             Specification = Jol.Specification,
                             UnitId = tab2.UnitId,
                             UnitName = tab2.Unit.UnitName,
                             UnitDecimalPlaces = tab2.Unit.DecimalPlaces,
                             DealUnitDecimalPlaces = Jol.DealUnit.DecimalPlaces,
                             ProductUidName = (Jol.ProductUidHeaderId == null ? tab.ProductUid.ProductUidName : ""),
                             ProductUidId = tab.ProductUidId,
                             JobOrderInspectionRequestHeaderId = p.JobOrderInspectionRequestHeaderId,
                             ProductGroupId = tab2.ProductGroupId,
                             DocTypeId = jir.DocTypeId,
                             SiteId = jir.SiteId,
                             DivisionId = jir.DivisionId,
                         }
                        );

            if (!string.IsNullOrEmpty(svm.ProductId))
                Query = Query.Where(m => ProductIdArr.Contains(m.ProductId.ToString()));

            if (!string.IsNullOrEmpty(svm.JobOrderInspectionRequestHeaderId))
                Query = Query.Where(m => SaleOrderIdArr.Contains(m.JobOrderInspectionRequestHeaderId.ToString()));

            if (!string.IsNullOrEmpty(svm.ProductGroupId))
                Query = Query.Where(m => ProductGroupIdArr.Contains(m.ProductGroupId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == InsHeader.SiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == InsHeader.DivisionId);

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));


            return (from p in Query
                    select new JobOrderInspectionLineViewModel
                    {
                        BalanceQty = p.BalanceQty,
                        Qty = p.BalanceQty,
                        JobOrderInspectionRequestDocNo = p.JobOrderInspectionRequestDocNo,
                        ProductName = p.ProductName,
                        ProductId = p.ProductId,
                        JobOrderInspectionHeaderId = svm.JobOrderInspectionHeaderId,
                        JobOrderInspectionRequestLineId = p.JobOrderInspectionRequestLineId,
                        Dimension1Id = p.Dimension1Id,
                        Dimension2Id = p.Dimension2Id,
                        Dimension3Id = p.Dimension3Id,
                        Dimension4Id = p.Dimension4Id,
                        Dimension1Name = p.Dimension1Name,
                        Dimension2Name = p.Dimension2Name,
                        Dimension3Name = p.Dimension3Name,
                        Dimension4Name = p.Dimension4Name,
                        Specification = p.Specification,
                        UnitId = p.UnitId,
                        UnitName = p.UnitName,
                        UnitDecimalPlaces = p.UnitDecimalPlaces,
                        DealUnitDecimalPlaces = p.DealUnitDecimalPlaces,
                        ProductUidName = p.ProductUidName,
                        ProductUidId = p.ProductUidId,
                        JobOrderInspectionRequestHeaderId = p.JobOrderInspectionRequestHeaderId,
                    });
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.JobOrderInspectionLine
                       where p.JobOrderInspectionHeaderId == id
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

                var Temp = (from p in context.ViewJobOrderBalanceForInspection
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

        public List<ComboBoxList> GetPendingBarCodesListForInspectionRequest(int[] id)
        {
            List<ComboBoxList> Barcodes = new List<ComboBoxList>();

            //var LineIds = id.Split(',').Select(Int32.Parse).ToArray();

            using (ApplicationDbContext context = new ApplicationDbContext())
            {

                //context.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                //context.Database.CommandTimeout = 30000;

                var Temp = (from p in context.ViewJobOrderInspectionRequestBalance
                            join t in context.ProductUid on p.ProductUidId equals t.ProductUIDId
                            join t2 in context.JobOrderInspectionRequestLine on p.JobOrderInspectionRequestLineId equals t2.JobOrderInspectionRequestLineId
                            where id.Contains(p.JobOrderInspectionRequestLineId) && p.ProductUidId != null
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


        public JobOrderInspectionLineViewModel GetLineDetailForInspection(int id, int ReceiveId, bool InsReq)
        {

            var InspectionHeader = new JobOrderInspectionHeaderService(db).Find(ReceiveId);

            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(InspectionHeader.DocTypeId, InspectionHeader.DivisionId, InspectionHeader.SiteId);

            //string[] ContraSites = null;
            //if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            //else { ContraSites = new string[] { "NA" }; }

            //string[] ContraDivisions = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            //else { ContraDivisions = new string[] { "NA" }; }


            if (InsReq)
            {
                return (from p in db.ViewJobOrderInspectionRequestBalance
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
                        select new JobOrderInspectionLineViewModel
                        {
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            Qty = (p.BalanceQty),
                            Specification = t1.Specification,
                            UnitId = t2.UnitId,
                            DealUnitId = t1.DealUnitId,
                            DealQty = p.BalanceQty * t1.UnitConversionMultiplier,
                            UnitConversionMultiplier = t1.UnitConversionMultiplier,
                            UnitName = t2.Unit.UnitName,
                            DealUnitName = t1.DealUnit.UnitName,
                            ProductId = p.ProductId,
                            ProductName = t1.Product.ProductName,
                            UnitDecimalPlaces = t2.Unit.DecimalPlaces,
                            DealUnitDecimalPlaces = t1.DealUnit.DecimalPlaces,
                            JobOrderInspectionRequestDocNo = p.JobOrderInspectionRequestNo,
                            JobOrderInspectionRequestLineId = p.JobOrderInspectionRequestLineId,
                        }
                      ).FirstOrDefault();
            }
            else
            {
                return (from p in db.ViewJobOrderBalanceForInspection
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
                        select new JobOrderInspectionLineViewModel
                        {
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            Qty = p.BalanceQty,
                            Specification = t1.Specification,
                            UnitId = t2.UnitId,
                            DealUnitId = t1.DealUnitId,
                            DealQty = p.BalanceQty * t1.UnitConversionMultiplier,
                            UnitConversionMultiplier = t1.UnitConversionMultiplier,
                            UnitName = t2.Unit.UnitName,
                            DealUnitName = t1.DealUnit.UnitName,
                            ProductId = p.ProductId,
                            ProductName = t1.Product.ProductName,
                            UnitDecimalPlaces = t2.Unit.DecimalPlaces,
                            DealUnitDecimalPlaces = t1.DealUnit.DecimalPlaces,
                        }
                      ).FirstOrDefault();
            }



        }

        public UIDValidationViewModel ValidateInspectionBarCode(string ProductUid, int HeaderId)
        {
            UIDValidationViewModel temp = new UIDValidationViewModel();

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var InspectionHeader = db.JobOrderInspectionHeader.Find(HeaderId);
            temp = (from p in db.ViewJobOrderBalanceForInspection
                    join jo in db.JobOrderHeader on p.JobOrderHeaderId equals jo.JobOrderHeaderId
                    join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                    where t.ProductUidName == ProductUid && p.BalanceQty > 0
                    && p.SiteId == SiteId && p.DivisionId == DivisionId && jo.ProcessId == InspectionHeader.ProcessId
                    select new UIDValidationViewModel
                    {
                        ProductId = p.ProductId,
                        ProductName = p.Product.ProductName,
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
                    var ProdUIdJobWoker = (from p in db.ViewJobOrderBalanceForInspection
                                           join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                                           where t.ProductUidName == ProductUid
                                           select p).FirstOrDefault();
                    if (ProdUIdJobWoker == null)
                    {
                        temp.ErrorType = "Error";
                        temp.ErrorMessage = "BarCode not pending.";
                    }
                    else if (ProdUIdJobWoker.JobWorkerId != InspectionHeader.JobWorkerId)
                    {
                        temp.ErrorType = "Error";
                        temp.ErrorMessage = "Does not belong to JobWorker";
                    }
                    else if (ProdUIdJobWoker.BalanceQty <= 0)
                    {
                        temp.ErrorType = "Error";
                        temp.ErrorMessage = "ProductUid not pending for Inspection";
                    }
                }

            }
            else
            {
                temp.ErrorType = "Success";
            }

            return temp;

        }

        public JobOrderInspectionLineViewModel GetOrderLineForUid(int Uid)
        {

            //var Rec1 = (from t in db.JobOrderLine
            //            join v in db.ViewJobOrderBalanceForInspection on t.JobOrderLineId equals v.JobOrderLineId
            //            where t.ProductUidId == Uid && v.BalanceQty > 0
            //            join Jol in db.JobOrderLine on t.JobOrderLineId equals Jol.JobOrderLineId
            //            select new JobOrderInspectionLineViewModel
            //            {
            //                JobOrderLineId = t.JobOrderLineId,
            //                JobOrderDocNo = t.JobOrderHeader.DocNo,
            //            }).FirstOrDefault();

            var temp = (from p in db.ProductUid
                        where p.ProductUIDId == Uid
                        select new
                        {
                            Rec = (from t in db.JobOrderLine
                                   join v in db.ViewJobOrderBalanceForInspection on t.JobOrderLineId equals v.JobOrderLineId
                                   where v.ProductUidId == p.ProductUIDId && v.BalanceQty > 0
                                   join Jol in db.JobOrderLine on t.JobOrderLineId equals Jol.JobOrderLineId
                                   select new JobOrderInspectionLineViewModel
                                   {
                                       JobOrderLineId = t.JobOrderLineId,
                                       JobOrderDocNo = t.JobOrderHeader.DocNo,
                                   }).FirstOrDefault()
                        }
                        ).FirstOrDefault();

            return temp.Rec;
        }

        public IQueryable<ComboBoxResult> GetPendingProductsForJobOrderInspection(string term, int JobOrderInspectionHeaderId)//DocTypeId
        {
            JobOrderInspectionHeader header = new JobOrderInspectionHeaderService(db).Find(JobOrderInspectionHeaderId);

            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderBalanceForInspection
                         join jo in db.JobOrderHeader on p.JobOrderHeaderId equals jo.JobOrderHeaderId
                         join t in db.Product on p.ProductId equals t.ProductId into ProdTable
                         from ProTab in ProdTable.DefaultIfEmpty()
                         where p.BalanceQty > 0
                         && p.JobWorkerId == header.JobWorkerId && jo.ProcessId == header.ProcessId
                         select new
                         {
                             ProductId = p.ProductId,
                             ProductName = ProTab.ProductName,
                             DocTypeId = jo.DocTypeId,
                             SiteId = jo.SiteId,
                             DivisionId = jo.DivisionId,
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

        public IQueryable<ComboBoxResult> GetPendingProductsForInspection(string term, int JobOrderInspectionHeaderId)//DocTypeId
        {
            JobOrderInspectionHeader header = new JobOrderInspectionHeaderService(db).Find(JobOrderInspectionHeaderId);

            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderInspectionRequestBalance
                         join jir in db.JobOrderInspectionRequestHeader on p.JobOrderInspectionRequestHeaderId equals jir.JobOrderInspectionRequestHeaderId
                         join t in db.Product on p.ProductId equals t.ProductId into ProdTable
                         from ProTab in ProdTable.DefaultIfEmpty()
                         where p.BalanceQty > 0 && p.JobWorkerId == header.JobWorkerId && jir.ProcessId == header.ProcessId
                         select new
                         {
                             ProductId = p.ProductId,
                             ProductName = ProTab.ProductName,
                             DocTypeId = jir.DocTypeId,
                             SiteId = jir.SiteId,
                             DivisionId = jir.DivisionId,
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

        public IQueryable<ComboBoxResult> GetPendingJobOrders(string term, int JobOrderInspectionHeaderId)//DocTypeId
        {
            JobOrderInspectionHeader header = new JobOrderInspectionHeaderService(db).Find(JobOrderInspectionHeaderId);

            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }


            var Query = (from p in db.ViewJobOrderBalanceForInspection
                         join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId into ProdTable
                         from ProTab in ProdTable.DefaultIfEmpty()
                         where p.BalanceQty > 0 && p.JobWorkerId == header.JobWorkerId
                         && ProTab.ProcessId == header.ProcessId
                         select new
                         {
                             HeaderId = p.JobOrderHeaderId,
                             DocNo = ProTab.DocNo,
                             DocTypeId = ProTab.DocTypeId,
                             SiteId = ProTab.SiteId,
                             DivisionId = ProTab.DivisionId,
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
                    group p by p.HeaderId into g
                    orderby g.Max(m => m.DocNo)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.DocNo),
                        id = g.Key.ToString(),
                    });

        }

        public IQueryable<ComboBoxResult> GetPendingJobRequests(string term, int JobOrderInspectionHeaderId)//DocTypeId
        {

            JobOrderInspectionHeader header = new JobOrderInspectionHeaderService(db).Find(JobOrderInspectionHeaderId);

            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderInspectionRequestBalance
                         join t in db.JobOrderInspectionRequestHeader on p.JobOrderInspectionRequestHeaderId equals t.JobOrderInspectionRequestHeaderId into ProdTable
                         from ProTab in ProdTable.DefaultIfEmpty()
                         where p.BalanceQty > 0 && p.JobWorkerId == header.JobWorkerId && ProTab.ProcessId == header.ProcessId
                         select new
                         {
                             HeaderId = p.JobOrderInspectionRequestHeaderId,
                             DocNo = ProTab.DocNo,
                             DocTypeId = ProTab.DocTypeId,
                             SiteId = ProTab.SiteId,
                             DivisionId = ProTab.DivisionId,
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
                    group p by p.HeaderId into g
                    orderby g.Max(m => m.DocNo)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.DocNo),
                        id = g.Key.ToString(),
                    });

        }

        public IEnumerable<JobOrderHeaderListViewModel> GetPendingJobOrdersForAC(int HeaderId, string term, int Limiter, bool InsReq)//Product Id
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var InspectionHeader = db.JobOrderInspectionHeader.Find(HeaderId);

            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(InspectionHeader.DocTypeId, InspectionHeader.DivisionId, InspectionHeader.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            if (InsReq)
            {
                var Query = (from p in db.ViewJobOrderInspectionRequestBalance
                             join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                             from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                             join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                             from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                             join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                             from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                             join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                             from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                             join t3 in db.Product on p.ProductId equals t3.ProductId
                             join t4 in db.JobOrderInspectionRequestHeader on p.JobOrderInspectionRequestHeaderId equals t4.JobOrderInspectionRequestHeaderId
                             join t5 in db.JobOrderLine on p.JobOrderLineId equals t5.JobOrderLineId
                             join t6 in db.JobOrderHeader on t5.JobOrderHeaderId equals t6.JobOrderHeaderId
                             where p.BalanceQty > 0 && p.JobWorkerId == InspectionHeader.JobWorkerId && t4.ProcessId == InspectionHeader.ProcessId
                             orderby t4.DocNo
                             select new
                             {
                                 DocNo = t6.DocNo,
                                 RequestDocNo = t4.DocNo,
                                 JobOrderLineId = p.JobOrderLineId,
                                 Dimension1Name = Dimension1Tab.Dimension1Name,
                                 Dimension2Name = Dimension2Tab.Dimension2Name,
                                 Dimension3Name = Dimension3Tab.Dimension3Name,
                                 Dimension4Name = Dimension4Tab.Dimension4Name,
                                 ProductName = t3.ProductName,
                                 BalanceQty = (p.BalanceQty),
                                 DocTypeId = t4.DocTypeId,
                                 SiteId = t4.SiteId,
                                 DivisionId = t4.DivisionId,
                             });

                if (!string.IsNullOrEmpty(term))
                    Query = Query.Where(m => m.RequestDocNo.ToLower().Contains(term.ToLower())
                        || m.Dimension1Name.ToLower().Contains(term.ToLower())
                        || m.Dimension2Name.ToLower().Contains(term.ToLower())
                        || m.Dimension3Name.ToLower().Contains(term.ToLower())
                        || m.Dimension4Name.ToLower().Contains(term.ToLower())
                        || m.ProductName.ToLower().Contains(term.ToLower()));

                if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                    Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));
                if (!string.IsNullOrEmpty(settings.filterContraSites))
                    Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
                else
                    Query = Query.Where(m => m.SiteId == InspectionHeader.SiteId);
                if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                    Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
                else
                    Query = Query.Where(m => m.DivisionId == InspectionHeader.DivisionId);

                return (from p in Query
                        select new JobOrderHeaderListViewModel
                        {
                            DocNo = p.DocNo,
                            RequestDocNo = p.RequestDocNo,
                            JobOrderLineId = p.JobOrderLineId,
                            Dimension1Name = p.Dimension1Name,
                            Dimension2Name = p.Dimension2Name,
                            Dimension3Name = p.Dimension3Name,
                            Dimension4Name = p.Dimension4Name,
                            ProductName = p.ProductName,
                            BalanceQty = (p.BalanceQty),
                        }).Take(Limiter);
            }
            else
            {
                var Query = (from p in db.ViewJobOrderBalanceForInspection
                             join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                             from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                             join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                             from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                             join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                             from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                             join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                             from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                             join t3 in db.Product on p.ProductId equals t3.ProductId
                             join t4 in db.JobOrderHeader on p.JobOrderHeaderId equals t4.JobOrderHeaderId
                             where p.BalanceQty > 0 && p.JobWorkerId == InspectionHeader.JobWorkerId && t4.ProcessId == InspectionHeader.ProcessId
                             orderby p.JobOrderNo
                             select new
                             {
                                 DocNo = p.JobOrderNo,
                                 JobOrderLineId = p.JobOrderLineId,
                                 Dimension1Name = Dimension1Tab.Dimension1Name,
                                 Dimension2Name = Dimension2Tab.Dimension2Name,
                                 Dimension3Name = Dimension3Tab.Dimension3Name,
                                 Dimension4Name = Dimension4Tab.Dimension4Name,
                                 ProductName = t3.ProductName,
                                 BalanceQty = p.BalanceQty,
                                 DocTypeId = t4.DocTypeId,
                                 SiteId = t4.SiteId,
                                 DivisionId = t4.DivisionId,
                             });


                if (!string.IsNullOrEmpty(term))
                    Query = Query.Where(m => m.DocNo.ToLower().Contains(term.ToLower())
                        || m.Dimension1Name.ToLower().Contains(term.ToLower())
                        || m.Dimension2Name.ToLower().Contains(term.ToLower())
                        || m.Dimension3Name.ToLower().Contains(term.ToLower())
                        || m.Dimension4Name.ToLower().Contains(term.ToLower())
                        || m.ProductName.ToLower().Contains(term.ToLower()));

                if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                    Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));
                if (!string.IsNullOrEmpty(settings.filterContraSites))
                    Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
                else
                    Query = Query.Where(m => m.SiteId == InspectionHeader.SiteId);
                if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                    Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
                else
                    Query = Query.Where(m => m.DivisionId == InspectionHeader.DivisionId);

                return (from p in Query
                        select new JobOrderHeaderListViewModel
                        {
                            DocNo = p.DocNo,
                            JobOrderLineId = p.JobOrderLineId,
                            Dimension1Name = p.Dimension1Name,
                            Dimension2Name = p.Dimension2Name,
                            Dimension3Name = p.Dimension3Name,
                            Dimension4Name = p.Dimension4Name,
                            ProductName = p.ProductName,
                            BalanceQty = p.BalanceQty,
                        }).Take(Limiter);
            }

        }

        public Task<IEquatable<JobOrderInspectionLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderInspectionLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
