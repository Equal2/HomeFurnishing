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
    public interface IJobOrderRateAmendmentLineService : IDisposable
    {
        JobOrderRateAmendmentLine Create(JobOrderRateAmendmentLine pt);
        void Delete(int id);
        void Delete(JobOrderRateAmendmentLine pt);
        JobOrderRateAmendmentLine Find(int id);
        IEnumerable<JobOrderRateAmendmentLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobOrderRateAmendmentLine pt);
        JobOrderRateAmendmentLine Add(JobOrderRateAmendmentLine pt);
        IEnumerable<JobOrderRateAmendmentLine> GetJobOrderRateAmendmentLineList();
        IEnumerable<JobOrderRateAmendmentLineViewModel> GetJobOrderRateAmendmentLineForHeader(int id);//Header Id
        Task<IEquatable<JobOrderRateAmendmentLine>> GetAsync();
        Task<JobOrderRateAmendmentLine> FindAsync(int id);
        IEnumerable<JobOrderRateAmendmentLineViewModel> GetJobOrderLineForMultiSelect(JobOrderAmendmentFilterViewModel svm);
        JobOrderRateAmendmentLineViewModel GetJobOrderRateAmendmentLine(int id);//Line Id
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetPendingJobOrdersForRateAmendment(int id, string term);
        IQueryable<ComboBoxResult> GetPendingProductsForRateAmndmt(int id, string term);
        IEnumerable<JobOrderHeaderListViewModel> GetPendingJobOrdersForRateAmndmt(int HeaderId, string term, int Limit);//ProductId
        int GetMaxSr(int id);
        IQueryable<ComboBoxResult> GetPendingJobOrderHelpList(int Id, string term);
    }

    public class JobOrderRateAmendmentLineService : IJobOrderRateAmendmentLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobOrderRateAmendmentLine> _JobOrderRateAmendmentLineRepository;
        RepositoryQuery<JobOrderRateAmendmentLine> JobOrderRateAmendmentLineRepository;
        public JobOrderRateAmendmentLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobOrderRateAmendmentLineRepository = new Repository<JobOrderRateAmendmentLine>(db);
            JobOrderRateAmendmentLineRepository = new RepositoryQuery<JobOrderRateAmendmentLine>(_JobOrderRateAmendmentLineRepository);
        }


        public JobOrderRateAmendmentLine Find(int id)
        {
            return _unitOfWork.Repository<JobOrderRateAmendmentLine>().Find(id);
        }

        public JobOrderRateAmendmentLine Create(JobOrderRateAmendmentLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderRateAmendmentLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderRateAmendmentLine>().Delete(id);
        }

        public void Delete(JobOrderRateAmendmentLine pt)
        {
            _unitOfWork.Repository<JobOrderRateAmendmentLine>().Delete(pt);
        }

        public void Update(JobOrderRateAmendmentLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderRateAmendmentLine>().Update(pt);
        }

        public IEnumerable<JobOrderRateAmendmentLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobOrderRateAmendmentLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.JobOrderRateAmendmentLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<JobOrderRateAmendmentLineViewModel> GetJobOrderRateAmendmentLineForHeader(int id)
        {
            return (from p in db.JobOrderRateAmendmentLine
                    join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    join dealunits in db.Units on tab.DealUnitId equals dealunits.UnitId
                    into dealunittable from dealunittab in dealunittable.DefaultIfEmpty()
                    join uid in db.ProductUid on tab.ProductUidId equals uid.ProductUIDId into uidtable
                    from uidtab in uidtable.DefaultIfEmpty()
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
                    join units in db.Units on tab3.UnitId equals units.UnitId
                    into unittable from unittab in unittable.DefaultIfEmpty()
                    join t4 in db.JobOrderAmendmentHeader on p.JobOrderAmendmentHeaderId equals t4.JobOrderAmendmentHeaderId
                    join t6 in db.Persons on t4.JobWorkerId equals t6.PersonID into table6
                    from tab6 in table6.DefaultIfEmpty()
                    where p.JobOrderAmendmentHeaderId == id
                    orderby p.Sr
                    select new JobOrderRateAmendmentLineViewModel
                    {
                        Dimension1Name = Dimension1Tab.Dimension1Name,
                        Dimension2Name = Dimension2Tab.Dimension2Name,
                        Dimension3Name = Dimension3Tab.Dimension3Name,
                        Dimension4Name = Dimension4Tab.Dimension4Name,
                        LotNo = tab.LotNo,
                        ProductId = tab.ProductId,
                        ProductName = tab3.ProductName,
                        JobOrderAmendmentHeaderDocNo = t4.DocNo,
                        JobOrderAmendmentHeaderId = p.JobOrderAmendmentHeaderId,
                        JobOrderRateAmendmentLineId = p.JobOrderRateAmendmentLineId,
                        JobOrderDocNo = t5.DocNo,
                        JobOrderLineId = tab.JobOrderLineId,
                        Qty = p.Qty,
                        DealQty = p.Qty * tab.UnitConversionMultiplier,
                        DealUnitName = dealunittab.UnitName,
                        Specification = tab.Specification,
                        JobWorkerId = p.JobWorkerId,                        
                        JobWorkerName = tab6.Name,
                        unitDecimalPlaces = unittab.DecimalPlaces,
                        UnitId = unittab.UnitId,
                        UnitName = unittab.UnitName,
                        DealunitDecimalPlaces = dealunittab.DecimalPlaces,
                        DealUnitId = dealunittab.UnitId,
                        Remark = p.Remark,
                        Rate = p.Rate,
                        Amount = p.Amount,
                        AmendedRate = p.AmendedRate,
                        JobOrderRate = p.JobOrderRate,
                        ProductUidName = uidtab.ProductUidName,
                    }
                        );

        }

        public IEnumerable<JobOrderRateAmendmentLineViewModel> GetJobOrderLineForMultiSelect(JobOrderAmendmentFilterViewModel svm)
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

            string[] Dim1Id = null;
            if (!string.IsNullOrEmpty(svm.Dimension1Id)) { Dim1Id = svm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dim1Id = new string[] { "NA" }; }

            string[] Dim2Id = null;
            if (!string.IsNullOrEmpty(svm.Dimension2Id)) { Dim2Id = svm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dim2Id = new string[] { "NA" }; }

            string[] Dim3Id = null;
            if (!string.IsNullOrEmpty(svm.Dimension3Id)) { Dim3Id = svm.Dimension3Id.Split(",".ToCharArray()); }
            else { Dim3Id = new string[] { "NA" }; }

            string[] Dim4Id = null;
            if (!string.IsNullOrEmpty(svm.Dimension4Id)) { Dim4Id = svm.Dimension4Id.Split(",".ToCharArray()); }
            else { Dim4Id = new string[] { "NA" }; }

            var temp = (from p in db.ViewJobOrderBalanceForInvoice
                        join t3 in db.JobOrderLine on p.JobOrderLineId equals t3.JobOrderLineId into table3
                        from tab3 in table3.DefaultIfEmpty()
                        join t2 in db.JobOrderHeader on p.JobOrderHeaderId equals t2.JobOrderHeaderId
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join AL in db.JobOrderRateAmendmentLine on p.JobOrderLineId equals AL.JobOrderLineId into ALTable
                        from AlTab in ALTable.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(svm.JobOrderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.JobOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && (string.IsNullOrEmpty(svm.Dimension1Id) ? 1 == 1 : Dim1Id.Contains(p.Dimension1Id.ToString()))
                        && (string.IsNullOrEmpty(svm.Dimension2Id) ? 1 == 1 : Dim2Id.Contains(p.Dimension2Id.ToString()))
                        && (string.IsNullOrEmpty(svm.Dimension3Id) ? 1 == 1 : Dim3Id.Contains(p.Dimension3Id.ToString()))
                        && (string.IsNullOrEmpty(svm.Dimension4Id) ? 1 == 1 : Dim4Id.Contains(p.Dimension4Id.ToString()))
                        && p.BalanceQty > 0 && ((svm.JobWorkerId.HasValue && svm.JobWorkerId.Value > 0) ? p.JobWorkerId == svm.JobWorkerId : 1 == 1)
                        && (svm.UpToDate.HasValue ? t2.DocDate <= svm.UpToDate : 1 == 1)
                        orderby t2.DocDate, t2.DocNo, tab3.Sr
                        select new JobOrderRateAmendmentLineViewModel
                        {
                            Dimension1Name = tab3.Dimension1.Dimension1Name,
                            Dimension2Name = tab3.Dimension2.Dimension2Name,
                            Dimension3Name = tab3.Dimension3.Dimension3Name,
                            Dimension4Name = tab3.Dimension4.Dimension4Name,
                            UnitName = tab2.Unit.UnitName,
                            DealUnitName = tab3.DealUnit.UnitName,
                            DealQty = p.BalanceQty * tab3.UnitConversionMultiplier,
                            UnitConversionMultiplier = tab3.UnitConversionMultiplier,
                            JobOrderRate = p.Rate,
                            AmendedRate = (svm.Rate == 0 ? p.Rate : svm.Rate),
                            Qty = p.BalanceQty,
                            JobOrderDocNo = p.JobOrderNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            JobOrderAmendmentHeaderId = svm.JobOrderAmendmentHeaderId,
                            JobOrderLineId = p.JobOrderLineId,
                            unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces = tab3.DealUnit.DecimalPlaces,
                            JobWorkerId = p.JobWorkerId,
                            AAmended = (AlTab == null ? false : true)
                        }
                        );
            return temp;
        }
        public JobOrderRateAmendmentLineViewModel GetJobOrderRateAmendmentLine(int id)
        {
            var temp = (from p in db.JobOrderRateAmendmentLine
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
                        join t4 in db.JobOrderAmendmentHeader on p.JobOrderAmendmentHeaderId equals t4.JobOrderAmendmentHeaderId into table4
                        from tab4 in table4.DefaultIfEmpty()
                        join t6 in db.Persons on tab4.JobWorkerId equals t6.PersonID into table6
                        from tab6 in table6.DefaultIfEmpty()
                        join t7 in db.ViewJobOrderBalanceForInvoice on p.JobOrderLineId equals t7.JobOrderLineId into table7
                        from tab7 in table7.DefaultIfEmpty()
                        orderby p.JobOrderRateAmendmentLineId
                        where p.JobOrderRateAmendmentLineId == id
                        select new JobOrderRateAmendmentLineViewModel
                        {
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            LotNo = tab.LotNo,
                            ProductId = tab.ProductId,
                            ProductName = tab3.ProductName,
                            JobOrderAmendmentHeaderDocNo = tab4.DocNo,
                            JobOrderAmendmentHeaderId = p.JobOrderAmendmentHeaderId,
                            JobOrderRateAmendmentLineId = p.JobOrderRateAmendmentLineId,
                            JobOrderDocNo = tab5.DocNo,
                            JobOrderLineId = tab.JobOrderLineId,
                            Qty = p.Qty,
                            Specification = tab.Specification,
                            JobWorkerId = p.JobWorkerId,
                            JobWorkerName = tab6.Name,
                            UnitId = tab3.UnitId,
                            UnitName = tab3.Unit.UnitName,
                            DealUnitName = tab.DealUnit.UnitName,
                            UnitConversionMultiplier = tab.UnitConversionMultiplier,
                            DealQty = (p.Qty * tab.UnitConversionMultiplier),
                            JobOrderRate = p.JobOrderRate,
                            LockReason=p.LockReason,
                            AmendedRate = p.AmendedRate,
                            Rate = p.Rate,
                            Amount = p.Amount,
                            Remark = p.Remark,
                        }

                      ).FirstOrDefault();

            return temp;

        }

        public IEnumerable<JobOrderRateAmendmentLine> GetJobOrderRateAmendmentLineList()
        {
            var pt = _unitOfWork.Repository<JobOrderRateAmendmentLine>().Query().Get().OrderBy(m => m.JobOrderRateAmendmentLineId);

            return pt;
        }

        public JobOrderRateAmendmentLine Add(JobOrderRateAmendmentLine pt)
        {
            _unitOfWork.Repository<JobOrderRateAmendmentLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobOrderRateAmendmentLine
                        orderby p.JobOrderRateAmendmentLineId
                        select p.JobOrderRateAmendmentLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderRateAmendmentLine
                        orderby p.JobOrderRateAmendmentLineId
                        select p.JobOrderRateAmendmentLineId).FirstOrDefault();
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

                temp = (from p in db.JobOrderRateAmendmentLine
                        orderby p.JobOrderRateAmendmentLineId
                        select p.JobOrderRateAmendmentLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderRateAmendmentLine
                        orderby p.JobOrderRateAmendmentLineId
                        select p.JobOrderRateAmendmentLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IQueryable<ComboBoxResult> GetPendingJobOrdersForRateAmendment(int id, string term)//DocTypeId
        {

            var JobOrderAmendmentHeader = new JobOrderAmendmentHeaderService(_unitOfWork).Find(id);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrderAmendmentHeader.DocTypeId, JobOrderAmendmentHeader.DivisionId, JobOrderAmendmentHeader.SiteId);

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

            var list = (from p in db.ViewJobOrderBalanceForInvoice
                        join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.JobOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        && (JobOrderAmendmentHeader.JobWorkerId.HasValue && JobOrderAmendmentHeader.JobWorkerId.Value > 0 ? t.JobWorkerId == JobOrderAmendmentHeader.JobWorkerId : 1 == 1)
                        group new { p } by p.JobOrderHeaderId into g
                        orderby g.Max(m => m.p.JobOrderNo)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.JobOrderNo),
                            id = g.Key.ToString(),
                        }
                        );
            return list;
        }

        public IQueryable<ComboBoxResult> GetPendingProductsForRateAmndmt(int id, string term)//DocTypeId
        {

            var JobOrderAmendmentHeader = new JobOrderAmendmentHeaderService(_unitOfWork).Find(id);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(JobOrderAmendmentHeader.DocTypeId, JobOrderAmendmentHeader.DivisionId, JobOrderAmendmentHeader.SiteId);

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

            var list = (from p in db.ViewJobOrderBalanceForInvoice
                        join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                        join t2 in db.Product on p.ProductId equals t2.ProductId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : t2.ProductName.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                         && (JobOrderAmendmentHeader.JobWorkerId.HasValue && JobOrderAmendmentHeader.JobWorkerId.Value > 0 ? t.JobWorkerId == JobOrderAmendmentHeader.JobWorkerId : 1 == 1)
                        group new { p, t2 } by p.ProductId into g
                        orderby g.Max(m => m.t2.ProductName)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t2.ProductName),
                            id = g.Key.ToString(),
                        }
                        );
            return list;
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.JobOrderRateAmendmentLine
                       where p.JobOrderAmendmentHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }


        public IEnumerable<JobOrderHeaderListViewModel> GetPendingJobOrdersForRateAmndmt(int HeaderId, string term, int Limit)//Product Id
        {

            var Header = new JobOrderAmendmentHeaderService(_unitOfWork).Find(HeaderId);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

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

            var tem = from p in db.ViewJobOrderBalanceForInvoice
                      join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                      join t2 in db.Product on p.ProductId equals t2.ProductId
                      join t3 in db.JobOrderHeader on p.JobOrderHeaderId equals t3.JobOrderHeaderId
                      where p.BalanceQty > 0 && (Header.JobWorkerId.HasValue && Header.JobWorkerId.Value > 0 ? p.JobWorkerId == Header.JobWorkerId : 1 == 1)
                      && (string.IsNullOrEmpty(term) ? 1 == 1 : (p.JobOrderNo.ToLower().Contains(term.ToLower()) || t2.ProductName.ToLower().Contains(term.ToLower())
                      || t.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower()) 
                      || t.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower())
                      || t.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower())
                      || t.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower())
                      ))
                      && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t3.DocTypeId.ToString()))
                      && (string.IsNullOrEmpty(settings.filterContraSites) ? t3.SiteId == CurrentSiteId : contraSites.Contains(t3.SiteId.ToString()))
                      && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t3.DivisionId == CurrentDivisionId : contraDivisions.Contains(t3.DivisionId.ToString()))
                      orderby p.JobOrderNo
                      select new JobOrderHeaderListViewModel
                      {
                          DocNo = p.JobOrderNo,
                          JobOrderLineId = p.JobOrderLineId,
                          Dimension1Name = t.Dimension1.Dimension1Name,
                          Dimension2Name = t.Dimension2.Dimension2Name,
                          Dimension3Name = t.Dimension3.Dimension3Name,
                          Dimension4Name = t.Dimension4.Dimension4Name,
                          ProductName = t2.ProductName,
                          BalanceQty = p.BalanceQty,

                      };

            return (tem.Take(Limit).ToList());
        }

        public IQueryable<ComboBoxResult> GetPendingJobOrderHelpList(int Id, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewJobOrderBalanceForInvoice
                        join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.JobOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && t.ProcessId == settings.ProcessId
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        group new { p, t } by p.JobOrderHeaderId into g
                        orderby g.Max(m => m.p.OrderDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.JobOrderNo) + " | " + g.Max(m => m.t.DocType.DocumentTypeShortName),
                            id = g.Key.ToString(),
                        }
                        );

            return list;
        }


        public IQueryable<ComboBoxResult> GetPendingJobWorkerHelpList(int Id, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewJobOrderBalanceForInvoice
                        join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : t.JobWorker.Person.Name.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && t.ProcessId == settings.ProcessId
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        group new { p, t } by p.JobWorkerId into g
                        orderby g.Max(m => m.p.OrderDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t.JobWorker.Person.Name),
                            id = g.Key.ToString(),
                        }
                        );

            return list;
        }

        public IQueryable<ComboBoxResult> GetPendingProductHelpList(int Id, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewJobOrderBalanceForInvoice
                        join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && t.ProcessId == settings.ProcessId
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        group new { p, t } by p.ProductId into g
                        orderby g.Max(m => m.p.OrderDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.Product.ProductName),
                            id = g.Key.ToString(),
                        }
                        );

            return list;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<JobOrderRateAmendmentLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderRateAmendmentLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
