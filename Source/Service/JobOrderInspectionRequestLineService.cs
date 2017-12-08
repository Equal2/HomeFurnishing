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
    public interface IJobOrderInspectionRequestLineService : IDisposable
    {
        JobOrderInspectionRequestLine Create(JobOrderInspectionRequestLine pt, string UserName);
        void Delete(int id);
        void Delete(JobOrderInspectionRequestLine pt);
        JobOrderInspectionRequestLine Find(int id);
        void Update(JobOrderInspectionRequestLine pt, string UserName);
        IEnumerable<JobOrderInspectionRequestLineViewModel> GetJobOrderInspectionRequestLineForHeader(int id);//Header Id
        Task<IEquatable<JobOrderInspectionRequestLine>> GetAsync();
        Task<JobOrderInspectionRequestLine> FindAsync(int id);
        IEnumerable<JobOrderInspectionRequestLineViewModel> GetJobOrderLineForMultiSelect(JobOrderInspectionRequestFilterViewModel svm);
        JobOrderInspectionRequestLineViewModel GetJobOrderInspectionRequestLine(int id);//Line Id
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetPendingJobOrders(string term, int filter);
        IQueryable<ComboBoxResult> GetPendingProductHelpList(string term, int filter);
        bool CheckForDuplicateJobOrder(int LineId, int RequestHeaderId);
        int GetMaxSr(int id);
        JobOrderLineViewModel GetOrderLineForUidBranch(int Uid);
        List<ComboBoxList> GetPendingBarCodesList(int[] id);
        UIDValidationViewModel ValidateInspectionRequestBarCode(string ProductUid, int HeaderId);
        IEnumerable<JobOrderHeaderListViewModel> GetPendingJobOrdersForAC(int HeaderId, string term, int Limiter);
        JobOrderLineViewModel GetLineDetailForInsReq(int id);
    }

    public class JobOrderInspectionRequestLineService : IJobOrderInspectionRequestLineService
    {
        private ApplicationDbContext db;
        public JobOrderInspectionRequestLineService(ApplicationDbContext db)
        {
            this.db = db;
        }


        public JobOrderInspectionRequestLine Find(int id)
        {
            return db.JobOrderInspectionRequestLine.Find(id);
        }

        public JobOrderInspectionRequestLine Create(JobOrderInspectionRequestLine pt, string UserName)
        {
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Added;
            db.JobOrderInspectionRequestLine.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            var line = db.JobOrderInspectionRequestLine.Find(id);
            line.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionRequestLine.Remove(line);
        }

        public void Delete(JobOrderInspectionRequestLine pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobOrderInspectionRequestLine.Remove(pt);
        }

        public void Update(JobOrderInspectionRequestLine pt, string UserName)
        {
            pt.ModifiedDate = DateTime.Now; pt.ModifiedBy = UserName;
            pt.ObjectState = ObjectState.Modified;
            db.JobOrderInspectionRequestLine.Add(pt);
        }

        public IEnumerable<JobOrderInspectionRequestLineViewModel> GetJobOrderInspectionRequestLineForHeader(int id)
        {
            return (from p in db.JobOrderInspectionRequestLine
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
                    join t4 in db.JobOrderInspectionRequestHeader on p.JobOrderInspectionRequestHeaderId equals t4.JobOrderInspectionRequestHeaderId
                    join t6 in db.Persons on t4.JobWorkerId equals t6.PersonID into table6
                    from tab6 in table6.DefaultIfEmpty()
                    where p.JobOrderInspectionRequestHeaderId == id
                    orderby p.Sr
                    select new JobOrderInspectionRequestLineViewModel
                    {
                        Dimension1Name = Dimension1Tab.Dimension1Name,
                        Dimension2Name = Dimension2Tab.Dimension2Name,
                        Dimension3Name = Dimension3Tab.Dimension3Name,
                        Dimension4Name = Dimension4Tab.Dimension4Name,
                        DueDate = tab.DueDate,
                        LotNo = tab.LotNo,
                        ProductId = tab.ProductId,
                        ProductName = tab3.ProductName,
                        JobOrderInspectionRequestHeaderDocNo = t4.DocNo,
                        JobOrderInspectionRequestHeaderId = p.JobOrderInspectionRequestHeaderId,
                        JobOrderInspectionRequestLineId = p.JobOrderInspectionRequestLineId,
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
                        ProductUidName = p.ProductUid.ProductUidName,
                    }
                        );

        }

        public IEnumerable<JobOrderInspectionRequestLineViewModel> GetJobOrderLineForMultiSelect(JobOrderInspectionRequestFilterViewModel svm)
        {
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

            var Header = db.JobOrderInspectionRequestHeader.Find(svm.JobOrderInspectionRequestHeaderId);

            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderBalanceForInspectionRequest
                         join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId into table
                         from tab in table.DefaultIfEmpty()
                         join product in db.Product on p.ProductId equals product.ProductId into table2
                         from tab2 in table2.DefaultIfEmpty()
                         join jo in db.JobOrderHeader on p.JobOrderHeaderId equals jo.JobOrderHeaderId
                         join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                         from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                         join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                         from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                         join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                         from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                         join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                         from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                         join uni in db.Units on tab2.UnitId equals uni.UnitId into unittable
                         from unittab in unittable.DefaultIfEmpty()
                         join dealunit in db.Units on tab.DealUnitId equals dealunit.UnitId into dealunittable
                         from dealunittab in dealunittable.DefaultIfEmpty()
                         where p.BalanceQty > 0 && p.JobWorkerId == svm.JobWorkerId && jo.ProcessId == Header.ProcessId
                         orderby p.OrderDate, p.JobOrderNo, tab.Sr
                         select new
                         {
                             BalanceQty = p.BalanceQty,
                             Qty = p.BalanceQty,
                             JobOrderDocNo = p.JobOrderNo,
                             ProductName = tab2.ProductName,
                             ProductId = p.ProductId,
                             JobOrderInspectionRequestHeaderId = svm.JobOrderInspectionRequestHeaderId,
                             JobOrderLineId = p.JobOrderLineId,
                             Dimension1Id = Dimension1Tab.Dimension1Id,
                             Dimension2Id = Dimension2Tab.Dimension2Id,
                             Dimension3Id = Dimension3Tab.Dimension3Id,
                             Dimension4Id = Dimension4Tab.Dimension4Id,
                             Dimension1Name = Dimension1Tab.Dimension1Name,
                             Dimension2Name = Dimension2Tab.Dimension2Name,
                             Dimension3Name = Dimension3Tab.Dimension3Name,
                             Dimension4Name = Dimension4Tab.Dimension4Name,
                             Specification = tab.Specification,
                             UnitId = unittab.UnitId,
                             UnitName = unittab.UnitName,
                             unitDecimalPlaces = unittab.DecimalPlaces,
                             DealunitDecimalPlaces = dealunittab.DecimalPlaces,
                             ProductUidName = (tab.ProductUidHeaderId == null ? tab.ProductUid.ProductUidName : ""),
                             ProductUidId = tab.ProductUidId,
                             JobOrderHeaderId = p.JobOrderHeaderId,
                             ProdOrderLineId = tab.ProdOrderLineId,
                             ProductGroupId = tab2.ProductGroupId,
                             SiteId = jo.SiteId,
                             DivisionId = jo.DivisionId,
                             DocTypeId = jo.DocTypeId,
                         }
                        );

            if (!string.IsNullOrEmpty(svm.ProductId))
                Query = Query.Where(m => ProductIdArr.Contains(m.ProductId.ToString()));

            if (!string.IsNullOrEmpty(svm.JobOrderId))
                Query = Query.Where(m => SaleOrderIdArr.Contains(m.JobOrderHeaderId.ToString()));

            if (!string.IsNullOrEmpty(svm.ProductGroupId))
                Query = Query.Where(m => ProductGroupIdArr.Contains(m.ProductGroupId.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension1Id))
                Query = Query.Where(m => Dime1IdArr.Contains(m.Dimension1Id.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension2Id))
                Query = Query.Where(m => Dime2IdArr.Contains(m.Dimension2Id.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension3Id))
                Query = Query.Where(m => Dime1IdArr.Contains(m.Dimension3Id.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension4Id))
                Query = Query.Where(m => Dime2IdArr.Contains(m.Dimension4Id.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == Header.SiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == Header.DivisionId);

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));


            return (from p in Query
                    select new JobOrderInspectionRequestLineViewModel
                    {
                        BalanceQty = p.BalanceQty,
                        Qty = p.Qty,
                        JobOrderDocNo = p.JobOrderDocNo,
                        ProductName = p.ProductName,
                        ProductId = p.ProductId,
                        JobOrderInspectionRequestHeaderId = p.JobOrderInspectionRequestHeaderId,
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
                        unitDecimalPlaces = p.unitDecimalPlaces,
                        DealunitDecimalPlaces = p.DealunitDecimalPlaces,
                        ProductUidName = p.ProductUidName,
                        ProductUidId = p.ProductUidId,
                        JobOrderHeaderId = p.JobOrderHeaderId,
                        ProdOrderLineId = p.ProdOrderLineId,
                    }).ToList();
        }
        public JobOrderInspectionRequestLineViewModel GetJobOrderInspectionRequestLine(int id)
        {
            var temp = (from p in db.JobOrderInspectionRequestLine
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
                        join t4 in db.JobOrderInspectionRequestHeader on p.JobOrderInspectionRequestHeaderId equals t4.JobOrderInspectionRequestHeaderId into table4
                        from tab4 in table4.DefaultIfEmpty()
                        join t6 in db.Persons on tab4.JobWorkerId equals t6.PersonID into table6
                        from tab6 in table6.DefaultIfEmpty()
                        join t7 in db.ViewJobOrderBalanceForInspectionRequest on p.JobOrderLineId equals t7.JobOrderLineId into table7
                        from tab7 in table7.DefaultIfEmpty()
                        orderby p.JobOrderInspectionRequestLineId
                        where p.JobOrderInspectionRequestLineId == id
                        select new JobOrderInspectionRequestLineViewModel
                        {
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            DueDate = tab.DueDate,
                            LotNo = tab.LotNo,
                            ProductId = tab.ProductId,
                            ProductName = tab3.ProductName,
                            JobOrderInspectionRequestHeaderDocNo = tab4.DocNo,
                            JobOrderInspectionRequestHeaderId = p.JobOrderInspectionRequestHeaderId,
                            JobOrderInspectionRequestLineId = p.JobOrderInspectionRequestLineId,
                            JobOrderDocNo = tab5.DocNo,
                            JobOrderLineId = tab.JobOrderLineId,
                            BalanceQty = p.Qty + tab7.BalanceQty,
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

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobOrderInspectionRequestLine
                        orderby p.JobOrderInspectionRequestLineId
                        select p.JobOrderInspectionRequestLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionRequestLine
                        orderby p.JobOrderInspectionRequestLineId
                        select p.JobOrderInspectionRequestLineId).FirstOrDefault();
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

                temp = (from p in db.JobOrderInspectionRequestLine
                        orderby p.JobOrderInspectionRequestLineId
                        select p.JobOrderInspectionRequestLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderInspectionRequestLine
                        orderby p.JobOrderInspectionRequestLineId
                        select p.JobOrderInspectionRequestLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public IQueryable<ComboBoxResult> GetPendingJobOrders(string term, int Id)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Header = new JobOrderInspectionRequestHeaderService(db).Find(Id);

            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(Header.DocTypeId, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderBalanceForInspectionRequest
                         join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                         where p.BalanceQty > 0 && t.ProcessId == settings.ProcessId && p.JobWorkerId==Header.JobWorkerId
                         select new
                         {
                             DocNo = p.JobOrderNo,
                             DocTypeId = t.DocTypeId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             JobOrderHeaderId = p.JobOrderHeaderId,

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
                    group p by p.JobOrderHeaderId into g
                    orderby g.Max(m => m.DocNo)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.DocNo),
                        id = g.Key.ToString(),
                    });
        }

        public IQueryable<ComboBoxResult> GetPendingJobOrdersForWizardFilters(string term, int Id)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];          

            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderBalanceForInspectionRequest
                         join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                         where p.BalanceQty > 0 && t.ProcessId == settings.ProcessId
                         select new
                         {
                             DocNo = p.JobOrderNo,
                             DocTypeId = t.DocTypeId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             JobOrderHeaderId = p.JobOrderHeaderId,

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
                    group p by p.JobOrderHeaderId into g
                    orderby g.Max(m => m.DocNo)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.DocNo),
                        id = g.Key.ToString(),
                    });
        }


        public IQueryable<ComboBoxResult> GetPendingJobWorkerHelpList(string term, int Id)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(Id, CurrentSiteId, CurrentDivisionId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderBalanceForInspectionRequest
                         join jrh in db.JobOrderHeader on p.JobOrderHeaderId equals jrh.JobOrderHeaderId
                         join t in db.Persons on p.JobWorkerId equals t.PersonID
                         where jrh.ProcessId == settings.ProcessId
                         orderby t.Name
                         select new
                         {
                             DocTypeId = jrh.DocTypeId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                             JobWorker = t.Name,
                             Id = t.PersonID,
                         }
                          );

            if (!string.IsNullOrEmpty(term))
                Query = Query.Where(m => m.JobWorker.ToLower().Contains(term.ToLower()));
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
                    group p by p.Id into g
                    orderby g.Max(m => m.JobWorker)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.JobWorker),
                        id = g.Key.ToString(),
                    });

        }


        public IQueryable<ComboBoxResult> GetPendingProductHelpList(string term, int Id)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Header = new JobOrderInspectionRequestHeaderService(db).Find(Id);

            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(Header.DocTypeId, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderBalanceForInspectionRequest
                         join jrh in db.JobOrderHeader on p.JobOrderHeaderId equals jrh.JobOrderHeaderId
                         join t in db.Product on p.ProductId equals t.ProductId
                         where jrh.ProcessId == settings.ProcessId && jrh.JobWorkerId == Header.JobWorkerId && p.BalanceQty > 0
                         select new
                         {
                             Product = t.ProductName,
                             Id = t.ProductId,
                             DocTypeId = jrh.DocTypeId,
                             SiteId = p.SiteId,
                             DivisionId = p.DivisionId,
                         }
                          );

            if (!string.IsNullOrEmpty(term))
                Query = Query.Where(m => m.Product.ToLower().Contains(term.ToLower()));
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
                    group p by p.Id into g
                    orderby g.Max(m => m.Product)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.Product),
                        id = g.Key.ToString(),
                    });
        }






        public bool CheckForDuplicateJobOrder(int LineId, int RequestHeaderId)
        {

            return (from p in db.JobOrderInspectionRequestLine
                    where p.JobOrderInspectionRequestHeaderId == RequestHeaderId && p.JobOrderLineId == LineId
                    select p).Any();

        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.JobOrderInspectionRequestLine
                       where p.JobOrderInspectionRequestHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
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

                var Temp = (from p in context.ViewJobOrderBalanceForInspectionRequest
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


        public UIDValidationViewModel ValidateInspectionRequestBarCode(string ProductUid, int HeaderId)
        {
            UIDValidationViewModel temp = new UIDValidationViewModel();

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var InspectionReqHeader = db.JobOrderInspectionRequestHeader.Find(HeaderId);
            temp = (from p in db.ViewJobOrderBalanceForInspectionRequest
                    join jo in db.JobOrderHeader on p.JobOrderHeaderId equals jo.JobOrderHeaderId
                    join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                    where t.ProductUidName == ProductUid && p.BalanceQty > 0
                    && p.SiteId == SiteId && p.DivisionId == DivisionId && jo.ProcessId == InspectionReqHeader.ProcessId
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
                    var ProdUIdJobWoker = (from p in db.ViewJobOrderBalanceForInspectionRequest
                                           join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                                           where t.ProductUidName == ProductUid
                                           select p).FirstOrDefault();

                    if (ProdUIdJobWoker.JobWorkerId != InspectionReqHeader.JobWorkerId)
                    {
                        temp.ErrorType = "Error";
                        temp.ErrorMessage = "Does not belong to JobWorker";
                    }
                    else if (ProdUIdJobWoker.BalanceQty <= 0)
                    {
                        temp.ErrorType = "Error";
                        temp.ErrorMessage = "ProductUid not pending for Inspection";
                    }
                    else
                    {
                        temp.ErrorType = "Error";
                        temp.ErrorMessage = "Unknown Error";
                    }
                }


            }
            else
            {
                temp.ErrorType = "Success";
            }

            return temp;

        }

        public IEnumerable<JobOrderHeaderListViewModel> GetPendingJobOrdersForAC(int HeaderId, string term, int Limiter)//Product Id
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var InspectionRequestHeader = db.JobOrderInspectionRequestHeader.Find(HeaderId);

            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(InspectionRequestHeader.DocTypeId, InspectionRequestHeader.DivisionId, InspectionRequestHeader.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewJobOrderBalanceForInspectionRequest
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
                         where p.BalanceQty > 0 && p.JobWorkerId == InspectionRequestHeader.JobWorkerId && t4.ProcessId == InspectionRequestHeader.ProcessId
                         //&& ((string.IsNullOrEmpty(term) ? 1 == 1 : p.JobOrderNo.ToLower().Contains(term.ToLower()))
                         //|| (string.IsNullOrEmpty(term) ? 1 == 1 : dim1.Dimension1Name.ToLower().Contains(term.ToLower()))
                         //|| (string.IsNullOrEmpty(term) ? 1 == 1 : dim2.Dimension2Name.ToLower().Contains(term.ToLower()))
                         //|| (string.IsNullOrEmpty(term) ? 1 == 1 : t3.ProductName.ToLower().Contains(term.ToLower())))
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
                             SiteId = t4.SiteId,
                             DivisionId = t4.DivisionId,
                             DocTypeId = t4.DocTypeId,
                         });

            if (!string.IsNullOrEmpty(term))
                Query = Query.Where(m => m.DocNo.Contains(term.ToLower()) || m.Dimension1Name.Contains(term.ToLower()) 
                    || m.Dimension2Name.Contains(term.ToLower())
                    || m.Dimension3Name.Contains(term.ToLower())
                    || m.Dimension4Name.Contains(term.ToLower())
                    || m.ProductName.ToLower().Contains(term.ToLower()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == InspectionRequestHeader.SiteId);
            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == InspectionRequestHeader.DivisionId);

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));


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

        public JobOrderLineViewModel GetLineDetailForInsReq(int id)
        {
            var temp = (from p in db.ViewJobOrderBalanceForInspectionRequest
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
                            Rate = t1.Rate,
                            ProductUidHeaderId = t1.ProductUidHeaderId
                        }
                        ).FirstOrDefault();
            return temp;

        }

        public void Dispose()
        {
        }


        public Task<IEquatable<JobOrderInspectionRequestLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderInspectionRequestLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
