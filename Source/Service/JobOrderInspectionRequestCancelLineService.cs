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
    public interface IJobOrderInspectionRequestCancelLineService : IDisposable
    {
        JobOrderInspectionRequestCancelLine Create(JobOrderInspectionRequestCancelLine pt, string UserName);
        void Delete(int id);
        void Delete(JobOrderInspectionRequestCancelLine pt);
        JobOrderInspectionRequestCancelLine Find(int id);
        void Update(JobOrderInspectionRequestCancelLine pt, string UserName);
        IEnumerable<JobOrderInspectionRequestCancelLineViewModel> GetJobOrderInspectionRequestCancelLineForHeader(int id);//Header Id
        Task<IEquatable<JobOrderInspectionRequestCancelLine>> GetAsync();
        Task<JobOrderInspectionRequestCancelLine> FindAsync(int id);
        IEnumerable<JobOrderInspectionRequestCancelLineViewModel> GetJobOrderInspectionRequestLineForMultiSelect(JobOrderInspectionRequestCancelFilterViewModel svm);
        JobOrderInspectionRequestCancelLineViewModel GetJobOrderInspectionRequestCancelLine(int id);//Line Id
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetPendingJobRequests(string term, int JobOrderInspectionRequestCancelHeaderId);
        IQueryable<ComboBoxResult> GetPendingProductsForJobOrderInspectionRequestCancel(string term, int JobOrderInspectionRequestCancelHeaderId);
        bool CheckForDuplicateJobOrder(int LineId, int RequestHeaderId);
        int GetMaxSr(int id);
        JobOrderInspectionRequestLineViewModel GetRequestLineForUidBranch(int Uid);
        List<ComboBoxList> GetPendingBarCodesList(int[] id);
        UIDValidationViewModel ValidateInspectionRequestCancelBarCode(string ProductUid, int HeaderId);
        JobOrderInspectionRequestLineViewModel GetInspectionRequestLineDetail(int id);
        IEnumerable<JobOrderInspectionRequestLineViewModel> GetPendingJobRequestsForAC(int HeaderId, string term, int Limiter);
    }

    public class JobOrderInspectionRequestCancelLineService : IJobOrderInspectionRequestCancelLineService
    {
        private ApplicationDbContext db;
        public JobOrderInspectionRequestCancelLineService(ApplicationDbContext db)
        {
            this.db = db;
        }


        public JobOrderInspectionRequestCancelLine Find(int id)
        {
            return db.JobOrderInspectionRequestCancelLine.Find(id);
        }

        public JobOrderInspectionRequestCancelLine Create(JobOrderInspectionRequestCancelLine pt, string UserName)
        {
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Added;
            db.JobOrderInspectionRequestCancelLine.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            var line = db.JobOrderInspectionRequestCancelLine.Find(id);
            line.ObjectState = Model.ObjectState.Deleted;

            db.JobOrderInspectionRequestCancelLine.Remove(line);
        }

        public void Delete(JobOrderInspectionRequestCancelLine pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionRequestCancelLine.Remove(pt);
        }

        public void Update(JobOrderInspectionRequestCancelLine pt, string UserName)
        {
            pt.ModifiedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Modified;
            db.JobOrderInspectionRequestCancelLine.Add(pt);
        }

        public IEnumerable<JobOrderInspectionRequestCancelLineViewModel> GetJobOrderInspectionRequestCancelLineForHeader(int id)
        {
            return (from p in db.JobOrderInspectionRequestCancelLine
                    join t in db.JobOrderInspectionRequestLine on p.JobOrderInspectionRequestLineId equals t.JobOrderInspectionRequestLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t5 in db.JobOrderInspectionRequestHeader on tab.JobOrderInspectionRequestHeaderId equals t5.JobOrderInspectionRequestHeaderId
                    join Jol in db.JobOrderLine on tab.JobOrderLineId equals Jol.JobOrderLineId
                    join D1 in db.Dimension1 on Jol.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                    from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                    join D2 in db.Dimension2 on Jol.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                    from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                    join D3 in db.Dimension3 on Jol.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                    from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                    join D4 in db.Dimension4 on Jol.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                    from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                    join t3 in db.Product on Jol.ProductId equals t3.ProductId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t4 in db.JobOrderInspectionRequestCancelHeader on p.JobOrderInspectionRequestCancelHeaderId equals t4.JobOrderInspectionRequestCancelHeaderId
                    join t6 in db.Persons on t4.JobWorkerId equals t6.PersonID into table6
                    from tab6 in table6.DefaultIfEmpty()
                    where p.JobOrderInspectionRequestCancelHeaderId == id
                    orderby p.Sr
                    select new JobOrderInspectionRequestCancelLineViewModel
                    {
                        Dimension1Name = Dimension1Tab.Dimension1Name,
                        Dimension2Name = Dimension2Tab.Dimension2Name,
                        Dimension3Name = Dimension3Tab.Dimension3Name,
                        Dimension4Name = Dimension4Tab.Dimension4Name,
                        //DueDate = tab.DueDate,
                        //LotNo = tab.LotNo,
                        ProductId = Jol.ProductId,
                        ProductName = tab3.ProductName,
                        JobOrderInspectionRequestCancelHeaderDocNo = t4.DocNo,
                        JobOrderInspectionRequestCancelHeaderId = p.JobOrderInspectionRequestCancelHeaderId,
                        JobOrderInspectionRequestCancelLineId = p.JobOrderInspectionRequestCancelLineId,
                        JobOrderInspectionRequestDocNo = t5.DocNo,
                        JobOrderInspectionRequestLineId = tab.JobOrderInspectionRequestLineId,
                        Qty = p.Qty,
                        Remark = p.Remark,
                        ProductUidId = p.ProductUidId,
                        Specification = Jol.Specification,
                        JobWorkerId = t4.JobWorkerId,
                        JobWorkerName = tab6.Name,
                        UnitId = tab3.UnitId,
                        unitDecimalPlaces = tab3.Unit.DecimalPlaces,
                        ProductUidName = p.ProductUid.ProductUidName,
                    }
                        );

        }

        public IEnumerable<JobOrderInspectionRequestCancelLineViewModel> GetJobOrderInspectionRequestLineForMultiSelect(JobOrderInspectionRequestCancelFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.JobOrderInspectionRequestId)) { SaleOrderIdArr = svm.JobOrderInspectionRequestId.Split(",".ToCharArray()); }
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

            var InsReqCanHeader = db.JobOrderInspectionRequestCancelHeader.Find(svm.JobOrderInspectionRequestCancelHeaderId);

            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(InsReqCanHeader.DocTypeId, InsReqCanHeader.DivisionId, InsReqCanHeader.SiteId);

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
                         where p.BalanceQty > 0 && p.JobWorkerId == svm.JobWorkerId && jir.ProcessId == InsReqCanHeader.ProcessId
                         orderby p.RequestDate, p.JobOrderInspectionRequestNo, tab.Sr
                         select new
                         {
                             BalanceQty = p.BalanceQty,
                             Qty = p.BalanceQty,
                             JobOrderInspectionRequestDocNo = p.JobOrderInspectionRequestNo,
                             ProductName = tab2.ProductName,
                             ProductId = p.ProductId,
                             JobOrderInspectionRequestCancelHeaderId = svm.JobOrderInspectionRequestCancelHeaderId,
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
                             unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                             DealunitDecimalPlaces = Jol.DealUnit.DecimalPlaces,
                             ProductUidName = (Jol.ProductUidHeaderId == null ? tab.ProductUid.ProductUidName : ""),
                             ProductUidId = tab.ProductUidId,
                             JobOrderInspectionRequestHeaderId = p.JobOrderInspectionRequestHeaderId,
                             ProdOrderLineId = Jol.ProdOrderLineId,
                             ProductGroupId = tab2.ProductGroupId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             DocTypeId = jir.DocTypeId,
                         });

            if (!string.IsNullOrEmpty(svm.ProductId))
                Query = Query.Where(m => ProductIdArr.Contains(m.ProductId.ToString()));

            if (!string.IsNullOrEmpty(svm.JobOrderInspectionRequestId))
                Query = Query.Where(m => SaleOrderIdArr.Contains(m.JobOrderInspectionRequestHeaderId.ToString()));

            if (!string.IsNullOrEmpty(svm.ProductGroupId))
                Query = Query.Where(m => ProductGroupIdArr.Contains(m.ProductGroupId.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension1Id))
                Query = Query.Where(m => Dime1IdArr.Contains(m.Dimension1Id.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension2Id))
                Query = Query.Where(m => Dime2IdArr.Contains(m.Dimension2Id.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension3Id))
                Query = Query.Where(m => Dime3IdArr.Contains(m.Dimension3Id.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension4Id))
                Query = Query.Where(m => Dime4IdArr.Contains(m.Dimension4Id.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == InsReqCanHeader.SiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == InsReqCanHeader.DivisionId);

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));

            return from p in Query
                   select new JobOrderInspectionRequestCancelLineViewModel
                   {

                       BalanceQty = p.BalanceQty,
                       Qty = p.BalanceQty,
                       JobOrderInspectionRequestDocNo = p.JobOrderInspectionRequestDocNo,
                       ProductName = p.ProductName,
                       ProductId = p.ProductId,
                       JobOrderInspectionRequestCancelHeaderId = svm.JobOrderInspectionRequestCancelHeaderId,
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
                       unitDecimalPlaces = p.unitDecimalPlaces,
                       DealunitDecimalPlaces = p.DealunitDecimalPlaces,
                       ProductUidName = (p.ProductUidName),
                       ProductUidId = p.ProductUidId,
                       JobOrderInspectionRequestHeaderId = p.JobOrderInspectionRequestHeaderId,
                       ProdOrderLineId = p.ProdOrderLineId,
                   };
        }
        public JobOrderInspectionRequestCancelLineViewModel GetJobOrderInspectionRequestCancelLine(int id)
        {
            var temp = (from p in db.JobOrderInspectionRequestCancelLine
                        join t in db.JobOrderInspectionRequestLine on p.JobOrderInspectionRequestLineId equals t.JobOrderInspectionRequestLineId into table
                        from tab in table.DefaultIfEmpty()
                        join Jol in db.JobOrderLine on tab.JobOrderLineId equals Jol.JobOrderLineId
                        join t5 in db.JobOrderInspectionRequestHeader on tab.JobOrderInspectionRequestHeaderId equals t5.JobOrderInspectionRequestHeaderId into table5
                        from tab5 in table5.DefaultIfEmpty()
                        join D1 in db.Dimension1 on Jol.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on Jol.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D3 in db.Dimension3 on Jol.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D4 in db.Dimension4 on Jol.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        join t3 in db.Product on Jol.ProductId equals t3.ProductId into table3
                        from tab3 in table3.DefaultIfEmpty()
                        join t4 in db.JobOrderInspectionRequestCancelHeader on p.JobOrderInspectionRequestCancelHeaderId equals t4.JobOrderInspectionRequestCancelHeaderId into table4
                        from tab4 in table4.DefaultIfEmpty()
                        join t6 in db.Persons on tab4.JobWorkerId equals t6.PersonID into table6
                        from tab6 in table6.DefaultIfEmpty()
                        join t7 in db.ViewJobOrderInspectionRequestBalance on p.JobOrderInspectionRequestLineId equals t7.JobOrderInspectionRequestLineId into table7
                        from tab7 in table7.DefaultIfEmpty()
                        orderby p.JobOrderInspectionRequestCancelLineId
                        where p.JobOrderInspectionRequestCancelLineId == id
                        select new JobOrderInspectionRequestCancelLineViewModel
                        {
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            DueDate = Jol.DueDate,
                            LotNo = Jol.LotNo,
                            ProductId = Jol.ProductId,
                            ProductName = tab3.ProductName,
                            JobOrderInspectionRequestCancelHeaderDocNo = tab4.DocNo,
                            JobOrderInspectionRequestCancelHeaderId = p.JobOrderInspectionRequestCancelHeaderId,
                            JobOrderInspectionRequestCancelLineId = p.JobOrderInspectionRequestCancelLineId,
                            JobOrderInspectionRequestDocNo = tab5.DocNo,
                            JobOrderInspectionRequestLineId = tab.JobOrderInspectionRequestLineId,
                            BalanceQty = p.Qty + tab7.BalanceQty,
                            Qty = p.Qty,
                            Remark = p.Remark,
                            Specification = Jol.Specification,
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

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobOrderInspectionRequestCancelLine
                        orderby p.JobOrderInspectionRequestCancelLineId
                        select p.JobOrderInspectionRequestCancelLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionRequestCancelLine
                        orderby p.JobOrderInspectionRequestCancelLineId
                        select p.JobOrderInspectionRequestCancelLineId).FirstOrDefault();
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

                temp = (from p in db.JobOrderInspectionRequestCancelLine
                        orderby p.JobOrderInspectionRequestCancelLineId
                        select p.JobOrderInspectionRequestCancelLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionRequestCancelLine
                        orderby p.JobOrderInspectionRequestCancelLineId
                        select p.JobOrderInspectionRequestCancelLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IQueryable<ComboBoxResult> GetPendingJobRequests(string term, int JobOrderInspectionRequestCancelHeaderId)//DocTypeId
        {

            JobOrderInspectionRequestCancelHeader header = new JobOrderInspectionRequestCancelHeaderService(db).Find(JobOrderInspectionRequestCancelHeaderId);

            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

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
                         where p.BalanceQty > 0
                         && p.SiteId == header.SiteId && p.DivisionId == header.DivisionId && p.JobWorkerId == header.JobWorkerId && ProTab.ProcessId == header.ProcessId
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


        public IQueryable<ComboBoxResult> GetPendingProductsForJobOrderInspectionRequestCancel(string term, int JobOrderInspectionRequestCancelHeaderId)//DocTypeId
        {
            JobOrderInspectionRequestCancelHeader header = new JobOrderInspectionRequestCancelHeaderService(db).Find(JobOrderInspectionRequestCancelHeaderId);

            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

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
                         where p.BalanceQty > 0
                         && p.SiteId == header.SiteId && p.DivisionId == header.DivisionId && p.JobWorkerId == header.JobWorkerId && jir.ProcessId == header.ProcessId
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

        public bool CheckForDuplicateJobOrder(int LineId, int RequestHeaderId)
        {

            return (from p in db.JobOrderInspectionRequestCancelLine
                    where p.JobOrderInspectionRequestCancelHeaderId == RequestHeaderId && p.JobOrderInspectionRequestLineId == LineId
                    select p).Any();

        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.JobOrderInspectionRequestCancelLine
                       where p.JobOrderInspectionRequestCancelHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }

        public JobOrderInspectionRequestLineViewModel GetRequestLineForUidBranch(int Uid)
        {
            var temp = (from p in db.ProductUid
                        where p.ProductUIDId == Uid
                        select new
                        {
                            Rec = (from t in db.JobOrderInspectionRequestLine
                                   join v in db.ViewJobOrderInspectionRequestBalance on t.JobOrderInspectionRequestLineId equals v.JobOrderInspectionRequestLineId
                                   where t.ProductUidId == p.ProductUIDId && v.BalanceQty > 0
                                   join Jol in db.JobOrderLine on t.JobOrderLineId equals Jol.JobOrderLineId
                                   select new JobOrderInspectionRequestLineViewModel
                                   {
                                       JobOrderInspectionRequestLineId = t.JobOrderInspectionRequestLineId,
                                       JobOrderInspectionRequestHeaderDocNo = t.JobOrderInspectionRequestHeader.DocNo,
                                       Specification = Jol.Specification,
                                       Dimension1Name = Jol.Dimension1.Dimension1Name,
                                       Dimension2Name = Jol.Dimension2.Dimension2Name,
                                       Dimension3Name = Jol.Dimension3.Dimension3Name,
                                       Dimension4Name = Jol.Dimension4.Dimension4Name,
                                       //ProdOrderBalanceQty = 1,
                                       LotNo = p.LotNo,
                                       UnitName = p.Product.Unit.UnitName,
                                   }).FirstOrDefault()
                        }
                        ).FirstOrDefault();

            return temp.Rec;
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

        public UIDValidationViewModel ValidateInspectionRequestCancelBarCode(string ProductUid, int HeaderId)
        {
            UIDValidationViewModel temp = new UIDValidationViewModel();

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var InspectionReqCancelHeader = db.JobOrderInspectionRequestCancelHeader.Find(HeaderId);

            temp = (from p in db.ViewJobOrderInspectionRequestBalance
                    join jir in db.JobOrderInspectionRequestHeader on p.JobOrderInspectionRequestHeaderId equals jir.JobOrderInspectionRequestHeaderId
                    join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                    where t.ProductUidName == ProductUid && p.BalanceQty > 0
                    && p.SiteId == SiteId && p.DivisionId == DivisionId && p.JobWorkerId == InspectionReqCancelHeader.JobWorkerId && jir.ProcessId == InspectionReqCancelHeader.ProcessId
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
                    var ProdUIdJobWoker = (from p in db.ViewJobOrderInspectionRequestBalance
                                           join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                                           where t.ProductUidName == ProductUid
                                           select p).FirstOrDefault();
                    if (ProdUIdJobWoker == null)
                    {
                        temp.ErrorType = "Error";
                        temp.ErrorMessage = "BarCode not pending.";
                    }
                    else if (ProdUIdJobWoker.JobWorkerId != InspectionReqCancelHeader.JobWorkerId)
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

        public JobOrderInspectionRequestLineViewModel GetInspectionRequestLineDetail(int id)
        {
            var temp = (from p in db.ViewJobOrderInspectionRequestBalance
                        join t1 in db.JobOrderInspectionRequestLine on p.JobOrderInspectionRequestLineId equals t1.JobOrderInspectionRequestLineId
                        join t2 in db.Product on p.ProductId equals t2.ProductId
                        join Jol in db.JobOrderLine on t1.JobOrderLineId equals Jol.JobOrderLineId
                        join D1 in db.Dimension1 on Jol.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on Jol.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D3 in db.Dimension3 on Jol.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D4 in db.Dimension4 on Jol.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        where p.JobOrderInspectionRequestLineId == id
                        select new JobOrderInspectionRequestLineViewModel
                        {
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            LotNo = Jol.LotNo,
                            Qty = p.BalanceQty,
                            Specification = Jol.Specification,
                            UnitId = t2.UnitId,
                            DealUnitId = Jol.DealUnitId,
                            JobOrderInspectionRequestLineId = t1.JobOrderInspectionRequestLineId,
                            DealQty = p.BalanceQty * Jol.UnitConversionMultiplier,
                            UnitConversionMultiplier = Jol.UnitConversionMultiplier,
                            UnitName = t2.Unit.UnitName,
                            DealUnitName = Jol.DealUnit.UnitName,
                            ProductId = p.ProductId,
                            ProductName = Jol.Product.ProductName,
                            unitDecimalPlaces = t2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces = Jol.DealUnit.DecimalPlaces,
                        }
                        ).FirstOrDefault();


            return temp;

        }

        public IEnumerable<JobOrderInspectionRequestLineViewModel> GetPendingJobRequestsForAC(int HeaderId, string term, int Limiter)//Product Id
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var InspectionReqCanHeader = db.JobOrderInspectionRequestCancelHeader.Find(HeaderId);

            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(InspectionReqCanHeader.DocTypeId, InspectionReqCanHeader.DivisionId, InspectionReqCanHeader.SiteId);

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
                         where p.BalanceQty > 0 && p.JobWorkerId == InspectionReqCanHeader.JobWorkerId && jir.ProcessId == jir.ProcessId
                         orderby p.JobOrderInspectionRequestNo
                         select new
                         {
                             JobOrderInspectionRequestHeaderDocNo = p.JobOrderInspectionRequestNo,
                             JobOrderInspectionRequestLineId = p.JobOrderInspectionRequestLineId,
                             Dimension1Name = p.Dimension1.Dimension1Name,
                             Dimension2Name = p.Dimension2.Dimension2Name,
                             Dimension3Name = p.Dimension3.Dimension3Name,
                             Dimension4Name = p.Dimension4.Dimension4Name,
                             ProductName = p.Product.ProductName,
                             BalanceQty = p.BalanceQty,
                             DocTypeId = jir.DocTypeId,
                             SiteId = jir.SiteId,
                             DivisionId = jir.DivisionId,
                         });

            if (!string.IsNullOrEmpty(term))
                Query = Query.Where(m => m.JobOrderInspectionRequestHeaderDocNo.ToLower().Contains(term.ToLower())
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
                Query = Query.Where(m => m.SiteId == InspectionReqCanHeader.SiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == InspectionReqCanHeader.DivisionId);

            return (from p in Query
                    select new JobOrderInspectionRequestLineViewModel
                    {
                        JobOrderInspectionRequestHeaderDocNo = p.JobOrderInspectionRequestHeaderDocNo,
                        JobOrderInspectionRequestLineId = p.JobOrderInspectionRequestLineId,
                        Dimension1Name = p.Dimension1Name,
                        Dimension2Name = p.Dimension2Name,
                        Dimension3Name = p.Dimension3Name,
                        Dimension4Name = p.Dimension4Name,
                        ProductName = p.ProductName,
                        BalanceQty = p.BalanceQty,
                    }).Take(Limiter);
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<JobOrderInspectionRequestCancelLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderInspectionRequestCancelLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
