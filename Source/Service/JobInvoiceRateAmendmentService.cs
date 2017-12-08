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
    public interface IJobInvoiceRateAmendmentLineService : IDisposable
    {
        JobInvoiceRateAmendmentLine Create(JobInvoiceRateAmendmentLine pt);
        void Delete(int id);
        void Delete(JobInvoiceRateAmendmentLine pt);
        JobInvoiceRateAmendmentLine Find(int id);
        IEnumerable<JobInvoiceRateAmendmentLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobInvoiceRateAmendmentLine pt);
        JobInvoiceRateAmendmentLine Add(JobInvoiceRateAmendmentLine pt);
        IEnumerable<JobInvoiceRateAmendmentLine> GetJobInvoiceRateAmendmentLineList();
        IEnumerable<JobInvoiceRateAmendmentLineViewModel> GetJobInvoiceRateAmendmentLineForHeader(int id);//Header Id
        Task<IEquatable<JobInvoiceRateAmendmentLine>> GetAsync();
        Task<JobInvoiceRateAmendmentLine> FindAsync(int id);
        IEnumerable<JobInvoiceRateAmendmentLineViewModel> GetJobInvoiceLineForMultiSelect(JobInvoiceAmendmentFilterViewModel svm);
        JobInvoiceRateAmendmentLineViewModel GetJobInvoiceRateAmendmentLine(int id);//Line Id
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetPendingJobInvoicesForRateAmendment(int id, string term);
        IQueryable<ComboBoxResult> GetPendingProductsForRateAmndmt(int id, string term);
        IQueryable<ComboBoxResult> GetPendingJobWorkersForRateAmndmt(int id, string term);
        IEnumerable<JobInvoiceHeaderListViewModel> GetPendingJobInvoicesForRateAmndmt(int HeaderId, string term, int Limit);//ProductId
        int GetMaxSr(int id);
        IQueryable<ComboBoxResult> GetPendingJobInvoiceHelpList(int Id, string term);
        JobInvoiceLineViewModel GetLineDetailFromUId(string UID, int HeaderId);
    }

    public class JobInvoiceRateAmendmentLineService : IJobInvoiceRateAmendmentLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobInvoiceRateAmendmentLine> _JobInvoiceRateAmendmentLineRepository;
        RepositoryQuery<JobInvoiceRateAmendmentLine> JobInvoiceRateAmendmentLineRepository;
        public JobInvoiceRateAmendmentLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobInvoiceRateAmendmentLineRepository = new Repository<JobInvoiceRateAmendmentLine>(db);
            JobInvoiceRateAmendmentLineRepository = new RepositoryQuery<JobInvoiceRateAmendmentLine>(_JobInvoiceRateAmendmentLineRepository);
        }


        public JobInvoiceRateAmendmentLine Find(int id)
        {
            return _unitOfWork.Repository<JobInvoiceRateAmendmentLine>().Find(id);
        }

        public JobInvoiceRateAmendmentLine Create(JobInvoiceRateAmendmentLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobInvoiceRateAmendmentLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobInvoiceRateAmendmentLine>().Delete(id);
        }

        public void Delete(JobInvoiceRateAmendmentLine pt)
        {
            _unitOfWork.Repository<JobInvoiceRateAmendmentLine>().Delete(pt);
        }

        public void Update(JobInvoiceRateAmendmentLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobInvoiceRateAmendmentLine>().Update(pt);
        }

        public IEnumerable<JobInvoiceRateAmendmentLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobInvoiceRateAmendmentLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.JobInvoiceRateAmendmentLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<JobInvoiceRateAmendmentLineViewModel> GetJobInvoiceRateAmendmentLineForHeader(int id)
        {
            return (from p in db.JobInvoiceRateAmendmentLine
                    join t in db.JobInvoiceLine on p.JobInvoiceLineId equals t.JobInvoiceLineId into table
                    from tab in table.DefaultIfEmpty()
                    join JR in db.JobReceiveLine on tab.JobReceiveLineId equals JR.JobReceiveLineId
                    join JO in db.JobOrderLine on JR.JobOrderLineId equals JO.JobOrderLineId
                    join uid in db.ProductUid on JR.ProductUidId equals uid.ProductUIDId into uidtable
                    from uidtab in uidtable.DefaultIfEmpty()
                    join t5 in db.JobInvoiceHeader on tab.JobInvoiceHeaderId equals t5.JobInvoiceHeaderId
                    join D1 in db.Dimension1 on JO.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                    from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                    join D2 in db.Dimension2 on JO.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                    from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                    join D3 in db.Dimension3 on JO.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                    from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                    join D4 in db.Dimension4 on JO.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                    from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                    join t3 in db.Product on JO.ProductId equals t3.ProductId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t4 in db.JobInvoiceAmendmentHeader on p.JobInvoiceAmendmentHeaderId equals t4.JobInvoiceAmendmentHeaderId
                    join t6 in db.Persons on tab.JobWorkerId equals t6.PersonID into table6
                    from tab6 in table6.DefaultIfEmpty()
                    where p.JobInvoiceAmendmentHeaderId == id
                    orderby p.Sr
                    select new JobInvoiceRateAmendmentLineViewModel
                    {
                        Dimension1Name = Dimension1Tab.Dimension1Name,
                        Dimension2Name = Dimension2Tab.Dimension2Name,
                        Dimension3Name = Dimension3Tab.Dimension3Name,
                        Dimension4Name = Dimension4Tab.Dimension4Name,
                        LotNo = JO.LotNo,
                        ProductId = JO.ProductId,
                        ProductName = tab3.ProductName,
                        JobInvoiceAmendmentHeaderDocNo = t4.DocNo,
                        JobInvoiceAmendmentHeaderId = p.JobInvoiceAmendmentHeaderId,
                        JobInvoiceRateAmendmentLineId = p.JobInvoiceRateAmendmentLineId,
                        JobInvoiceDocNo = t5.DocNo,
                        JobInvoiceDocDate=t5.DocDate,
                        JobInvoiceLineId = tab.JobInvoiceLineId,
                        Qty = p.Qty,
                        DealQty = p.Qty * tab.UnitConversionMultiplier,
                        DealUnitName = tab.DealUnit.UnitName,
                        Specification = JO.Specification,
                        JobWorkerId = p.JobWorkerId,
                        JobWorkerName = tab6.Name,
                        UnitId = tab3.UnitId,
                        UnitName = tab3.Unit.UnitName,
                        unitDecimalPlaces = tab3.Unit.DecimalPlaces,
                        DealunitDecimalPlaces = tab.DealUnit.DecimalPlaces,
                        Remark = p.Remark,
                        Rate = p.Rate,
                        Amount = p.Amount,
                        AmendedRate = p.AmendedRate,
                        JobInvoiceRate = p.JobInvoiceRate,
                        ProductUidName = uidtab.ProductUidName,
                        InvoiceDocTypeId=t5.DocTypeId,
                        InvoiceHeaderId=t5.JobInvoiceHeaderId,
                    }
                        );

        }

        public IEnumerable<JobInvoiceRateAmendmentLineViewModel> GetJobInvoiceLineForMultiSelect(JobInvoiceAmendmentFilterViewModel svm)
        {

            var Header = db.JobInvoiceAmendmentHeader.Find(svm.JobInvoiceAmendmentHeaderId);

            var Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { contraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(Settings.filterContraSites)) { contraSites = Settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDivisions)) { contraDivisions = Settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }


            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.JobInvoiceId)) { SaleOrderIdArr = svm.JobInvoiceId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            string[] Dim1Id = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { Dim1Id = svm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dim1Id = new string[] { "NA" }; }

            string[] Dim2Id = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { Dim2Id = svm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dim2Id = new string[] { "NA" }; }

            string[] Dim3Id = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { Dim3Id = svm.Dimension3Id.Split(",".ToCharArray()); }
            else { Dim3Id = new string[] { "NA" }; }

            string[] Dim4Id = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { Dim4Id = svm.Dimension4Id.Split(",".ToCharArray()); }
            else { Dim4Id = new string[] { "NA" }; }

            var Query = (from VJ in db.ViewJobInvoiceBalanceForRateAmendment
                         join p in db.JobInvoiceLine on VJ.JobInvoiceLineId equals p.JobInvoiceLineId
                         join t2 in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals t2.JobInvoiceHeaderId
                         join JR in db.JobReceiveLine on p.JobReceiveLineId equals JR.JobReceiveLineId
                         join JO in db.JobOrderLine on JR.JobOrderLineId equals JO.JobOrderLineId
                         join product in db.Product on JO.ProductId equals product.ProductId into table2
                         from tab2 in table2.DefaultIfEmpty()
                         join AL in db.JobInvoiceRateAmendmentLine on p.JobInvoiceLineId equals AL.JobInvoiceLineId into ALTable
                         from AlTab in ALTable.DefaultIfEmpty()
                         where
                             //(string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(JO.ProductId.ToString()))
                             //&& (string.IsNullOrEmpty(svm.JobInvoiceId) ? 1 == 1 : SaleOrderIdArr.Contains(p.JobInvoiceHeaderId.ToString()))
                             //&& (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                             //&& (string.IsNullOrEmpty(svm.Dimension1Id) ? 1 == 1 : Dim1Id.Contains(JO.Dimension1Id.ToString()))
                             //&& (string.IsNullOrEmpty(svm.Dimension2Id) ? 1 == 1 : Dim2Id.Contains(JO.Dimension2Id.ToString()))
                             //&& (string.IsNullOrEmpty(Settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t2.DocTypeId.ToString()))
                             //&& (string.IsNullOrEmpty(Settings.filterContraDivisions) ? t2.DivisionId == Header.DivisionId : contraDivisions.Contains(t2.DivisionId.ToString()))
                             //&& (string.IsNullOrEmpty(Settings.filterContraSites) ? t2.SiteId == Header.SiteId : contraSites.Contains(t2.SiteId.ToString()))
                             //&& ((svm.JobWorkerId.HasValue && svm.JobWorkerId.Value > 0) ? p.JobWorkerId == svm.JobWorkerId : 1 == 1)
                             //&& ((svm.OldRate > 0) ? p.Rate == svm.OldRate : 1 == 1)
                             //&& (svm.UpToDate.HasValue ? t2.DocDate <= svm.UpToDate : 1 == 1)
                             //&& 
                         p.Qty > 0 && t2.ProcessId == Header.ProcessId
                         orderby t2.DocDate, t2.DocNo, p.Sr
                         select new
                         {
                             Dimension1Name = JO.Dimension1.Dimension1Name,
                             Dimension2Name = JO.Dimension2.Dimension2Name,
                             Dimension3Name = JO.Dimension3.Dimension3Name,
                             Dimension4Name = JO.Dimension4.Dimension4Name,
                             UnitName = tab2.Unit.UnitName,
                             DealUnitName = p.DealUnit.UnitName,
                             DealQty = (p.Qty) * p.UnitConversionMultiplier,
                             UnitConversionMultiplier = p.UnitConversionMultiplier,
                             JobInvoiceRate = p.Rate,
                             AmendedRate = (svm.Rate == 0 ? p.Rate : svm.Rate),
                             Qty = (p.Qty),
                             JobInvoiceDocNo = t2.DocNo,
                             ProductName = tab2.ProductName,
                             ProductId = tab2.ProductId,
                             JobInvoiceAmendmentHeaderId = svm.JobInvoiceAmendmentHeaderId,
                             JobInvoiceLineId = p.JobInvoiceLineId,
                             unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                             DealunitDecimalPlaces = p.DealUnit.DecimalPlaces,
                             JobWorkerId = p.JobWorkerId,
                             AAmended = (AlTab == null ? false : true),
                             JobInvoiceHeaderId = p.JobInvoiceHeaderId,
                             ProductGroupId = tab2.ProductGroupId,
                             Dimension1Id = JO.Dimension1Id,
                             Dimension2Id = JO.Dimension2Id,
                             Dimension3Id = JO.Dimension3Id,
                             Dimension4Id = JO.Dimension4Id,
                             DocTypeId = t2.DocTypeId,
                             DivisionId = t2.DivisionId,
                             SiteId = t2.SiteId,
                             DocDate = t2.DocDate,
                         });

            if (!string.IsNullOrEmpty(svm.ProductId))
                Query = Query.Where(m => ProductIdArr.Contains(m.ProductId.ToString()));

            if (!string.IsNullOrEmpty(svm.JobInvoiceId))
                Query = Query.Where(m => SaleOrderIdArr.Contains(m.JobInvoiceHeaderId.ToString()));

            if (!string.IsNullOrEmpty(svm.ProductGroupId))
                Query = Query.Where(m => ProductGroupIdArr.Contains(m.ProductGroupId.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension1Id))
                Query = Query.Where(m => Dim1Id.Contains(m.Dimension1Id.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension2Id))
                Query = Query.Where(m => Dim2Id.Contains(m.Dimension2Id.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension3Id))
                Query = Query.Where(m => Dim3Id.Contains(m.Dimension3Id.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension4Id))
                Query = Query.Where(m => Dim4Id.Contains(m.Dimension4Id.ToString()));

            if (!string.IsNullOrEmpty(Settings.filterContraSites))
                Query = Query.Where(m => contraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == Header.SiteId);
            if (!string.IsNullOrEmpty(Settings.filterContraDivisions))
                Query = Query.Where(m => contraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == Header.DivisionId);

            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));

            if (svm.OldRate > 0)
                Query = Query.Where(m => m.JobInvoiceRate == svm.OldRate);

            if (svm.JobWorkerId.HasValue && svm.JobWorkerId.Value > 0)
                Query = Query.Where(m => m.JobWorkerId == svm.JobWorkerId);

            if (svm.UpToDate.HasValue)
                Query = Query.Where(m => m.DocDate <= svm.UpToDate);


            return (from p in Query
                    select new JobInvoiceRateAmendmentLineViewModel
                    {
                        Dimension1Name = p.Dimension1Name,
                        Dimension2Name = p.Dimension2Name,
                        Dimension3Name = p.Dimension3Name,
                        Dimension4Name = p.Dimension4Name,
                        UnitName = p.UnitName,
                        DealUnitName = p.UnitName,
                        DealQty = p.DealQty,
                        UnitConversionMultiplier = p.UnitConversionMultiplier,
                        JobInvoiceRate = p.JobInvoiceRate,
                        AmendedRate = p.AmendedRate,
                        Qty = (p.Qty),
                        JobInvoiceDocNo = p.JobInvoiceDocNo,
                        ProductName = p.ProductName,
                        ProductId = p.ProductId,
                        JobInvoiceAmendmentHeaderId = p.JobInvoiceAmendmentHeaderId,
                        JobInvoiceLineId = p.JobInvoiceLineId,
                        unitDecimalPlaces = p.unitDecimalPlaces,
                        DealunitDecimalPlaces = p.DealunitDecimalPlaces,
                        JobWorkerId = p.JobWorkerId,
                        AAmended = p.AAmended,
                    });
        }
        public JobInvoiceRateAmendmentLineViewModel GetJobInvoiceRateAmendmentLine(int id)
        {
            var temp = (from p in db.JobInvoiceRateAmendmentLine
                        join t in db.JobInvoiceLine on p.JobInvoiceLineId equals t.JobInvoiceLineId into table
                        from tab in table.DefaultIfEmpty()
                        join t5 in db.JobInvoiceHeader on tab.JobInvoiceHeaderId equals t5.JobInvoiceHeaderId into table5
                        from tab5 in table5.DefaultIfEmpty()
                        join JR in db.JobReceiveLine on tab.JobReceiveLineId equals JR.JobReceiveLineId
                        join JO in db.JobOrderLine on JR.JobOrderLineId equals JO.JobOrderLineId
                        join D1 in db.Dimension1 on JO.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on JO.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D3 in db.Dimension3 on JO.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D4 in db.Dimension4 on JO.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        join t3 in db.Product on JO.ProductId equals t3.ProductId into table3
                        from tab3 in table3.DefaultIfEmpty()
                        join t4 in db.JobInvoiceAmendmentHeader on p.JobInvoiceAmendmentHeaderId equals t4.JobInvoiceAmendmentHeaderId into table4
                        from tab4 in table4.DefaultIfEmpty()
                        join t6 in db.Persons on tab4.JobWorkerId equals t6.PersonID into table6
                        from tab6 in table6.DefaultIfEmpty()
                        orderby p.JobInvoiceRateAmendmentLineId
                        where p.JobInvoiceRateAmendmentLineId == id
                        select new JobInvoiceRateAmendmentLineViewModel
                        {
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            LotNo = JR.LotNo,
                            ProductId = JO.ProductId,
                            ProductName = tab3.ProductName,
                            JobInvoiceAmendmentHeaderDocNo = tab4.DocNo,
                            JobInvoiceAmendmentHeaderId = p.JobInvoiceAmendmentHeaderId,
                            JobInvoiceRateAmendmentLineId = p.JobInvoiceRateAmendmentLineId,
                            JobInvoiceDocNo = tab5.DocNo,
                            JobInvoiceLineId = tab.JobInvoiceLineId,
                            Qty = p.Qty,
                            Specification = JO.Specification,
                            JobWorkerId = p.JobWorkerId,
                            JobWorkerName = tab6.Name,
                            UnitId = tab3.UnitId,
                            UnitName = tab3.Unit.UnitName,
                            DealUnitName = tab.DealUnit.UnitName,
                            UnitConversionMultiplier = tab.UnitConversionMultiplier,
                            DealQty = (p.Qty * tab.UnitConversionMultiplier),
                            JobInvoiceRate = p.JobInvoiceRate,
                            AmendedRate = p.AmendedRate,
                            Rate = p.Rate,
                            Amount = p.Amount,
                            Remark = p.Remark,
                            LockReason = p.LockReason,
                            ProductUidName = JR.ProductUid.ProductUidName,
                        }

                      ).FirstOrDefault();

            return temp;

        }

        public IEnumerable<JobInvoiceRateAmendmentLine> GetJobInvoiceRateAmendmentLineList()
        {
            var pt = _unitOfWork.Repository<JobInvoiceRateAmendmentLine>().Query().Get().OrderBy(m => m.JobInvoiceRateAmendmentLineId);

            return pt;
        }

        public JobInvoiceRateAmendmentLine Add(JobInvoiceRateAmendmentLine pt)
        {
            _unitOfWork.Repository<JobInvoiceRateAmendmentLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobInvoiceRateAmendmentLine
                        orderby p.JobInvoiceRateAmendmentLineId
                        select p.JobInvoiceRateAmendmentLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceRateAmendmentLine
                        orderby p.JobInvoiceRateAmendmentLineId
                        select p.JobInvoiceRateAmendmentLineId).FirstOrDefault();
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

                temp = (from p in db.JobInvoiceRateAmendmentLine
                        orderby p.JobInvoiceRateAmendmentLineId
                        select p.JobInvoiceRateAmendmentLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceRateAmendmentLine
                        orderby p.JobInvoiceRateAmendmentLineId
                        select p.JobInvoiceRateAmendmentLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IQueryable<ComboBoxResult> GetPendingJobInvoicesForRateAmendment(int id, string term)//DocTypeId
        {

            var JobInvoiceAmendmentHeader = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(id);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoiceAmendmentHeader.DocTypeId, JobInvoiceAmendmentHeader.DivisionId, JobInvoiceAmendmentHeader.SiteId);

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

            var list = (from VJ in db.ViewJobInvoiceBalanceForRateAmendment
                        join p in db.JobInvoiceLine on VJ.JobInvoiceLineId equals p.JobInvoiceLineId
                        join t in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals t.JobInvoiceHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : t.DocNo.ToLower().Contains(term.ToLower())) && p.Qty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        && (JobInvoiceAmendmentHeader.JobWorkerId.HasValue && JobInvoiceAmendmentHeader.JobWorkerId.Value > 0 ? t.JobWorkerId == JobInvoiceAmendmentHeader.JobWorkerId : 1 == 1)
                        group new { p, t } by p.JobInvoiceHeaderId into g
                        orderby g.Max(m => m.t.DocNo)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t.DocNo),
                            id = g.Key.ToString(),
                        }
                        );
            return list;
        }

        public IQueryable<ComboBoxResult> GetPendingProductsForRateAmndmt(int id, string term)//DocTypeId
        {

            var JobInvoiceAmendmentHeader = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(id);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoiceAmendmentHeader.DocTypeId, JobInvoiceAmendmentHeader.DivisionId, JobInvoiceAmendmentHeader.SiteId);

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

            var list = (from VJ in db.ViewJobInvoiceBalanceForRateAmendment
                        join p in db.JobInvoiceLine on VJ.JobInvoiceLineId equals p.JobInvoiceLineId
                        join t in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals t.JobInvoiceHeaderId
                        join JR in db.JobReceiveLine on p.JobReceiveLineId equals JR.JobReceiveLineId
                        join JO in db.JobOrderLine on JR.JobOrderLineId equals JO.JobOrderLineId
                        join t2 in db.Product on JO.ProductId equals t2.ProductId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : t2.ProductName.ToLower().Contains(term.ToLower())) && p.Qty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                         && (JobInvoiceAmendmentHeader.JobWorkerId.HasValue && JobInvoiceAmendmentHeader.JobWorkerId.Value > 0 ? t.JobWorkerId == JobInvoiceAmendmentHeader.JobWorkerId : 1 == 1)
                        group new { p, t2 } by JO.ProductId into g
                        orderby g.Max(m => m.t2.ProductName)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t2.ProductName),
                            id = g.Key.ToString(),
                        }
                        );
            return list;
        }

        public IQueryable<ComboBoxResult> GetPendingJobWorkersForRateAmndmt(int id, string term)//DocTypeId
        {

            var JobInvoiceAmendmentHeader = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(id);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoiceAmendmentHeader.DocTypeId, JobInvoiceAmendmentHeader.DivisionId, JobInvoiceAmendmentHeader.SiteId);

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

            var list = (from VJ in db.ViewJobInvoiceBalanceForRateAmendment
                        join p in db.JobInvoiceLine on VJ.JobInvoiceLineId equals p.JobInvoiceLineId
                        join H in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals H.JobInvoiceHeaderId
                        join t in db.Persons on p.JobWorkerId equals t.PersonID
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : t.Name.ToLower().Contains(term.ToLower())) && p.Qty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(H.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? H.SiteId == CurrentSiteId : contraSites.Contains(H.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? H.DivisionId == CurrentDivisionId : contraDivisions.Contains(H.DivisionId.ToString()))
                        && (JobInvoiceAmendmentHeader.JobWorkerId.HasValue && JobInvoiceAmendmentHeader.JobWorkerId.Value > 0 ? p.JobWorkerId == JobInvoiceAmendmentHeader.JobWorkerId : 1 == 1)
                        group new { t } by t.PersonID into g
                        orderby g.Max(m => m.t.Name)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t.Name),
                            id = g.Key.ToString(),
                        }
                        );
            return list;
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.JobInvoiceRateAmendmentLine
                       where p.JobInvoiceAmendmentHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }


        public IEnumerable<JobInvoiceHeaderListViewModel> GetPendingJobInvoicesForRateAmndmt(int HeaderId, string term, int Limit)//Product Id
        {

            var Header = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(HeaderId);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

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

            var tem = from VJB in db.ViewJobInvoiceBalanceForRateAmendment
                      join p in db.JobInvoiceLine on VJB.JobInvoiceLineId equals p.JobInvoiceLineId
                      join JR in db.JobReceiveLine on p.JobReceiveLineId equals JR.JobReceiveLineId
                      join JO in db.JobOrderLine on JR.JobOrderLineId equals JO.JobOrderLineId
                      join t2 in db.Product on JO.ProductId equals t2.ProductId
                      join t3 in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals t3.JobInvoiceHeaderId
                      where p.Qty > 0 && (Header.JobWorkerId.HasValue && Header.JobWorkerId.Value > 0 ? p.JobWorkerId == Header.JobWorkerId : 1 == 1)
                      && (string.IsNullOrEmpty(term) ? 1 == 1 : (t3.DocNo.ToLower().Contains(term.ToLower()) || t2.ProductName.ToLower().Contains(term.ToLower())
                      || JO.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower()) || JO.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower()) || JO.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower()) || JO.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower()) || VJB.ProductUidName.ToLower().Contains(term.ToLower())))
                      && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t3.DocTypeId.ToString()))
                      && (string.IsNullOrEmpty(settings.filterContraSites) ? t3.SiteId == CurrentSiteId : contraSites.Contains(t3.SiteId.ToString()))
                      && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t3.DivisionId == CurrentDivisionId : contraDivisions.Contains(t3.DivisionId.ToString()))
                      orderby t3.DocNo
                      select new JobInvoiceHeaderListViewModel
                      {
                          DocNo = t3.DocNo,
                          JobInvoiceLineId = p.JobInvoiceLineId,
                          Dimension1Name = JO.Dimension1.Dimension1Name,
                          Dimension2Name = JO.Dimension2.Dimension2Name,
                          Dimension3Name = JO.Dimension3.Dimension3Name,
                          Dimension4Name = JO.Dimension4.Dimension4Name,
                          ProductName = t2.ProductName,
                          BalanceQty = (p.Qty),
                          ProductUidName = VJB.ProductUidName,
                      };

            return (tem.Take(Limit).ToList());
        }


        public IQueryable<ComboBoxResult> GetPendingJobInvoiceHelpList(int Id, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewJobInvoiceBalanceForRateAmendment
                        join t in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals t.JobInvoiceHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.JobInvoiceNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && t.ProcessId == settings.ProcessId
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        group new { p, t } by p.JobInvoiceHeaderId into g
                        orderby g.Max(m => m.p.InvoiceDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.JobInvoiceNo) + " | " + g.Max(m => m.t.DocType.DocumentTypeShortName),
                            id = g.Key.ToString(),
                        }
                        );

            return list;
        }

        public IQueryable<ComboBoxResult> GetPendingJobWorkerHelpList(int Id, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewJobInvoiceBalanceForRateAmendment
                        join t in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals t.JobInvoiceHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : t.JobWorker.Name.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && t.ProcessId == settings.ProcessId
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        group new { p, t } by p.JobWorkerId into g
                        orderby g.Max(m => m.p.InvoiceDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t.JobWorker.Name),
                            id = g.Key.ToString(),
                        }
                        );

            return list;
        }

        public IQueryable<ComboBoxResult> GetPendingProductHelpList(int Id, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewJobInvoiceBalanceForRateAmendment
                        join t in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals t.JobInvoiceHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0 && t.ProcessId == settings.ProcessId
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? t.SiteId == CurrentSiteId : contraSites.Contains(t.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? t.DivisionId == CurrentDivisionId : contraDivisions.Contains(t.DivisionId.ToString()))
                        group new { p, t } by p.ProductId into g
                        orderby g.Max(m => m.p.InvoiceDate)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.Product.ProductName),
                            id = g.Key.ToString(),
                        }
                        );

            return list;
        }

        public JobInvoiceLineViewModel GetLineDetailFromUId(string UID, int HeaderId)
        {
            var Header = db.JobInvoiceAmendmentHeader.Find(HeaderId);

            return (from p in db.ViewJobInvoiceBalanceForRateAmendment
                    join ji in db.JobInvoiceLine on p.JobInvoiceLineId equals ji.JobInvoiceLineId
                    join jih in db.JobInvoiceHeader on ji.JobInvoiceHeaderId equals jih.JobInvoiceHeaderId
                    join JR in db.JobReceiveLine on ji.JobReceiveLineId equals JR.JobReceiveLineId
                    join JO in db.JobOrderLine on JR.JobOrderLineId equals JO.JobOrderLineId
                    join t in db.ProductUid on JR.ProductUidId equals t.ProductUIDId into uidtable
                    from uidtab in uidtable.DefaultIfEmpty()
                    join t2 in db.Product on JO.ProductId equals t2.ProductId
                    join D1 in db.Dimension1 on JO.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                    from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                    join D2 in db.Dimension2 on JO.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                    from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                    join D3 in db.Dimension3 on JO.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                    from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                    join D4 in db.Dimension4 on JO.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                    from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                    where uidtab.ProductUidName == UID && p.ProcessId == Header.ProcessId && jih.SiteId == Header.SiteId && jih.DivisionId == Header.DivisionId
                    select new JobInvoiceLineViewModel
                    {
                        Dimension1Name = Dimension1Tab.Dimension1Name,
                        Dimension2Name = Dimension2Tab.Dimension2Name,
                        Dimension3Name = Dimension3Tab.Dimension3Name,
                        Dimension4Name = Dimension4Tab.Dimension4Name,
                        LotNo = JR.LotNo,
                        Qty = (ji.Qty),
                        Specification = JO.Specification,
                        UnitId = t2.UnitId,
                        DealUnitId = ji.DealUnitId,
                        DealQty = ji.DealQty,
                        UnitConversionMultiplier = ji.UnitConversionMultiplier,
                        UnitName = t2.Unit.UnitName,
                        DealUnitName = ji.DealUnit.UnitName,
                        JobWorkerId = ji.JobWorkerId,
                        JobWorkerName = ji.JobWorker.Person.Name,
                        Rate = p.Rate,
                        ProductId = JO.ProductId,
                        ProductName = JO.Product.ProductName,
                        InvoiceDocNo = jih.DocNo,
                        JobInvoiceLineId=ji.JobInvoiceLineId,
                    }
                        ).FirstOrDefault();

        }

        public void Dispose()
        {
        }


        public Task<IEquatable<JobInvoiceRateAmendmentLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobInvoiceRateAmendmentLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
