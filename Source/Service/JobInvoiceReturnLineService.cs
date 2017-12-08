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
    public interface IJobInvoiceReturnLineService : IDisposable
    {
        JobInvoiceReturnLine Create(JobInvoiceReturnLine pt);
        void Delete(int id);
        void Delete(JobInvoiceReturnLine pt);
        JobInvoiceReturnLine Find(int id);
        void Update(JobInvoiceReturnLine pt);
        IEnumerable<JobInvoiceReturnLineIndexViewModel> GetLineListForIndex(int HeaderId);
        Task<IEquatable<JobInvoiceReturnLine>> GetAsync();
        Task<JobInvoiceReturnLine> FindAsync(int id);
        JobInvoiceReturnLineViewModel GetJobInvoiceReturnLine(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<JobInvoiceReturnLineViewModel> GetJobReceiveForFilters(JobInvoiceReturnLineFilterViewModel vm);
        IEnumerable<JobInvoiceReturnLineViewModel> GetJobInvoiceForFilters(JobInvoiceReturnLineFilterViewModel vm);
        IEnumerable<JobInvoiceListViewModel> GetPendingJobInvoiceHelpList(int Id, string term);
        IEnumerable<JobReceiveListViewModel> GetPendingJobReceiveHelpList(int Id, string term);
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);
        JobInvoiceLineViewModel GetJobInvoiceLineBalance(int id);
        object GetProductUidDetail(string ProductUidName, int filter);
        string GetFirstBarCodeForReturn(int JobReceiveLineId);
        string GetFirstBarCodeForReturn(int[] JobReceiveLineIds);
        List<ComboBoxList> GetPendingBarCodesList(string id, int GodownId);
        IEnumerable<ComboBoxResult> GetJobInvoiceHelpListForProduct(int Id, string term);
        IEnumerable<ComboBoxResult> GetCostCenterForPerson(int Id, string term);
    }

    public class JobInvoiceReturnLineService : IJobInvoiceReturnLineService
    {
        ApplicationDbContext db;
        public JobInvoiceReturnLineService(ApplicationDbContext db)
        {
            this.db = db;
        }


        public JobInvoiceReturnLine Find(int id)
        {
            return db.JobInvoiceReturnLine.Find(id);
        }

        public JobInvoiceReturnLine Create(JobInvoiceReturnLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            db.JobInvoiceReturnLine.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            var temp = db.JobInvoiceReturnLine.Find(id);
            temp.ObjectState = Model.ObjectState.Deleted;
            db.JobInvoiceReturnLine.Remove(temp);
        }

        public void Delete(JobInvoiceReturnLine pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobInvoiceReturnLine.Remove(pt);
        }
        public IEnumerable<JobInvoiceReturnLineViewModel> GetJobReceiveForFilters(JobInvoiceReturnLineFilterViewModel vm)
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

            var temp = (from VJIB in db.ViewJobInvoiceBalance
                        join L in db.JobInvoiceLine on VJIB.JobInvoiceLineId equals L.JobInvoiceLineId into JobInvoiceLineTable
                        from JobInvoiceLineTab in JobInvoiceLineTable.DefaultIfEmpty()
                        join H in db.JobInvoiceHeader on VJIB.JobInvoiceHeaderId equals H.JobInvoiceHeaderId into JobInvoiceHeaderTable
                        from JobInvoiceHeaderTab in JobInvoiceHeaderTable.DefaultIfEmpty()
                        join Jrl in db.JobReceiveLine on VJIB.JobReceiveLineId equals Jrl.JobReceiveLineId into JobReceiveLineTable
                        from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                        join Jrh in db.JobReceiveHeader on JobReceiveLineTab.JobReceiveHeaderId equals Jrh.JobReceiveHeaderId
                        join P in db.Product on VJIB.ProductId equals P.ProductId into table2
                        from ProductTab in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(VJIB.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.JobReceiveHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(VJIB.JobReceiveHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(ProductTab.ProductGroupId.ToString()))
                        && VJIB.BalanceQty > 0
                        orderby Jrh.DocDate, Jrh.DocNo, JobReceiveLineTab.Sr
                        select new JobInvoiceReturnLineViewModel
                        {
                            Dimension1Name = JobReceiveLineTab.Dimension1.Dimension1Name,
                            Dimension2Name = JobReceiveLineTab.Dimension2.Dimension2Name,
                            Dimension3Name = JobReceiveLineTab.Dimension3.Dimension3Name,
                            Dimension4Name = JobReceiveLineTab.Dimension4.Dimension4Name,
                            Specification = JobReceiveLineTab.Specification,
                            InvoiceBalQty = VJIB.BalanceQty,
                            Qty = VJIB.BalanceQty,
                            JobInvoiceHeaderDocNo = JobInvoiceHeaderTab.DocNo,
                            ProductName = ProductTab.ProductName,
                            ProductId = VJIB.ProductId,
                            JobInvoiceReturnHeaderId = vm.JobInvoiceReturnHeaderId,
                            JobInvoiceLineId = VJIB.JobInvoiceLineId,
                            UnitId = ProductTab.UnitId,
                            UnitConversionMultiplier = JobInvoiceLineTab.UnitConversionMultiplier,
                            DealUnitId = JobInvoiceLineTab.DealUnitId,
                            Rate = JobInvoiceLineTab.Rate,
                            //RateAfterDiscount = (linetab.Amount / linetab.DealQty),
                            unitDecimalPlaces = ProductTab.Unit.DecimalPlaces,
                            DealunitDecimalPlaces = JobInvoiceLineTab.DealUnit.DecimalPlaces,
                            ProductUidName = JobReceiveLineTab.ProductUid.ProductUidName,
                            ProductUidId = JobReceiveLineTab.ProductUidId,
                        }

                        );
            return temp;
        }
        public IEnumerable<JobInvoiceReturnLineViewModel> GetJobInvoiceForFilters(JobInvoiceReturnLineFilterViewModel vm)
        {
            var InvoiceReturnHeader = new JobInvoiceReturnHeaderService(db).Find(vm.JobInvoiceReturnHeaderId);

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.JobInvoiceHeaderId)) { SaleOrderIdArr = vm.JobInvoiceHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }
            //ToChange View to get Joborders instead of goodsreceipts
            var temp = (from p in db.ViewJobInvoiceBalance
                        join l in db.JobInvoiceLine on p.JobInvoiceLineId equals l.JobInvoiceLineId into linetable
                        from linetab in linetable.DefaultIfEmpty()
                        join h in db.JobInvoiceHeader on linetab.JobInvoiceHeaderId equals h.JobInvoiceHeaderId
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join t1 in db.JobReceiveLine on p.JobReceiveLineId equals t1.JobReceiveLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(vm.JobInvoiceHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.JobInvoiceHeaderId.ToString()))
                        && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        orderby h.DocDate, h.DocNo, linetab.Sr
                        select new JobInvoiceReturnLineViewModel
                        {
                            Dimension1Name = tab1.Dimension1.Dimension1Name,
                            Dimension2Name = tab1.Dimension2.Dimension2Name,
                            Dimension3Name = tab1.Dimension3.Dimension3Name,
                            Dimension4Name = tab1.Dimension4.Dimension4Name,
                            Specification = tab1.Specification,
                            InvoiceBalQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            JobInvoiceHeaderDocNo = p.JobInvoiceNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            JobInvoiceReturnHeaderId = vm.JobInvoiceReturnHeaderId,
                            JobInvoiceLineId = p.JobInvoiceLineId,
                            UnitId = tab2.UnitId,
                            UnitConversionMultiplier = linetab.UnitConversionMultiplier,
                            DealUnitId = linetab.DealUnitId,
                            Rate = linetab.Rate,
                            //RateAfterDiscount = (linetab.Amount / linetab.DealQty),
                            unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces = linetab.DealUnit.DecimalPlaces,
                            ProductUidName = tab1.ProductUid.ProductUidName,
                            ProductUidId = tab1.ProductUidId,
                            SalesTaxGroupProductId = linetab.SalesTaxGroupProductId,
                            SalesTaxGroupPersonId = InvoiceReturnHeader.SalesTaxGroupPersonId
                        });
            return temp;
        }
        public void Update(JobInvoiceReturnLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            db.JobInvoiceReturnLine.Add(pt);
        }
        public IEnumerable<JobInvoiceReturnLineIndexViewModel> GetLineListForIndex(int HeaderId)
        {
            return (from L in db.JobInvoiceReturnLine
                    join Jil in db.JobInvoiceLine on L.JobInvoiceLineId equals Jil.JobInvoiceLineId into JobInvoiceLineTable
                    from JobInvoiceLineTab in JobInvoiceLineTable.DefaultIfEmpty()
                    join Jrl in db.JobReceiveLine on JobInvoiceLineTab.JobReceiveLineId equals Jrl.JobReceiveLineId into JobReceiveLineTable
                    from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                    join Jrh in db.JobReceiveHeader on JobReceiveLineTab.JobReceiveHeaderId equals Jrh.JobReceiveHeaderId into JobReceiveHeaderTable
                    from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                    join P in db.Product on JobReceiveLineTab.ProductId equals P.ProductId into ProductTable
                    from ProductTab in ProductTable.DefaultIfEmpty()
                    where L.JobInvoiceReturnHeaderId == HeaderId
                    orderby L.Sr
                    select new JobInvoiceReturnLineIndexViewModel
                    {

                        ProductName = ProductTab.ProductName,
                        Qty = L.Qty,
                        JobInvoiceReturnLineId = L.JobInvoiceReturnLineId,
                        UnitId = ProductTab.UnitId,
                        Specification = JobReceiveLineTab.Specification,
                        Dimension1Name = JobReceiveLineTab.Dimension1.Dimension1Name,
                        Dimension2Name = JobReceiveLineTab.Dimension2.Dimension2Name,
                        Dimension3Name = JobReceiveLineTab.Dimension3.Dimension3Name,
                        Dimension4Name = JobReceiveLineTab.Dimension4.Dimension4Name,
                        LotNo = JobReceiveLineTab.LotNo,
                        JobGoodsRecieptHeaderDocNo = JobReceiveHeaderTab.DocNo,
                        JobInvoiceHeaderDocNo = JobInvoiceLineTab.JobInvoiceHeader.DocNo,
                        DealQty = L.DealQty,
                        DealUnitId = L.DealUnitId,
                        unitDecimalPlaces = ProductTab.Unit.DecimalPlaces,
                        DealunitDecimalPlaces = L.DealUnit.DecimalPlaces,
                        Rate = L.Rate,
                        Amount = L.Amount,
                        Remark = L.Remark,
                        ProductUidName = JobReceiveLineTab.ProductUid.ProductUidName,
                        UnitName = ProductTab.Unit.UnitName,
                        DealUnitName = L.DealUnit.UnitName,
                        JobInvoiceDocTypeId = L.JobInvoiceLine.JobInvoiceHeader.DocTypeId,
                        JobInvoiceHeaderId = L.JobInvoiceLine.JobInvoiceHeaderId,
                    });
        }
        public JobInvoiceReturnLineViewModel GetJobInvoiceReturnLine(int id)
        {
            return (from L in db.JobInvoiceReturnLine
                    join Jil in db.JobInvoiceLine on L.JobInvoiceLineId equals Jil.JobInvoiceLineId into JobInvoiceLineTable
                    from JobInvoiceLineTab in JobInvoiceLineTable.DefaultIfEmpty()
                    join VJil in db.ViewJobInvoiceBalance on L.JobInvoiceLineId equals VJil.JobInvoiceLineId into ViewJobInvoiceBalanceTable
                    from ViewJobInvoiceBalanceTab in ViewJobInvoiceBalanceTable.DefaultIfEmpty()
                    join Jrl in db.JobReceiveLine on JobInvoiceLineTab.JobReceiveLineId equals Jrl.JobReceiveLineId into JobReceiveLineTable
                    from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                    join Jirh in db.JobInvoiceReturnHeader on L.JobInvoiceReturnHeaderId equals Jirh.JobInvoiceReturnHeaderId into JobInvoiceReturnHeaderTable
                    from JobInvoiceReturnHeaderTab in JobInvoiceReturnHeaderTable.DefaultIfEmpty()
                    join Jih in db.JobInvoiceHeader on JobInvoiceLineTab.JobInvoiceHeaderId equals Jih.JobInvoiceHeaderId into JobInvoiceHeaderTable
                    from JobInvoiceHeaderTab in JobInvoiceHeaderTable.DefaultIfEmpty()
                    where L.JobInvoiceReturnLineId == id
                    select new JobInvoiceReturnLineViewModel
                    {
                        JobWorkerId = JobInvoiceReturnHeaderTab.JobWorkerId,
                        ProductId = JobReceiveLineTab.ProductId,
                        JobInvoiceLineId = L.JobInvoiceLineId,
                        JobInvoiceHeaderDocNo = JobInvoiceHeaderTab.DocNo,
                        JobInvoiceReturnHeaderId = L.JobInvoiceReturnHeaderId,
                        JobInvoiceReturnLineId = L.JobInvoiceReturnLineId,
                        Rate = L.Rate,
                        Amount = L.Amount,
                        UnitConversionMultiplier = L.UnitConversionMultiplier,
                        DealQty = L.DealQty,
                        DealUnitId = L.DealUnitId,
                        Qty = L.Qty,
                        InvoiceBalQty = ((L.JobInvoiceLineId == null || JobReceiveLineTab == null) ? L.Qty : L.Qty + (ViewJobInvoiceBalanceTab == null ? 0 : ViewJobInvoiceBalanceTab.BalanceQty)),
                        Remark = L.Remark,
                        UnitId = JobReceiveLineTab.Product.UnitId,
                        Dimension1Id = JobReceiveLineTab.Dimension1Id,
                        Dimension1Name = JobReceiveLineTab.Dimension1.Dimension1Name,
                        Dimension2Id = JobReceiveLineTab.Dimension2Id,
                        Dimension2Name = JobReceiveLineTab.Dimension2.Dimension2Name,
                        Dimension3Id = JobReceiveLineTab.Dimension3Id,
                        Dimension3Name = JobReceiveLineTab.Dimension3.Dimension3Name,
                        Dimension4Id = JobReceiveLineTab.Dimension4Id,
                        Dimension4Name = JobReceiveLineTab.Dimension4.Dimension4Name,
                        Specification = JobReceiveLineTab.Specification,
                        LotNo = JobReceiveLineTab.LotNo,
                        LockReason = L.LockReason,
                        Nature = JobInvoiceReturnHeaderTab.Nature,
                        ProductUidId = JobReceiveLineTab.ProductUidId,
                        ProductUidName = JobReceiveLineTab.ProductUid.ProductUidName,
                        SalesTaxGroupPersonId = JobInvoiceReturnHeaderTab.SalesTaxGroupPersonId,
                        SalesTaxGroupProductId = L.SalesTaxGroupProductId,
                        CostCenterId = L.CostCenterId,
                        CostCenterName = L.CostCenter.CostCenterName
                    }).FirstOrDefault();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobInvoiceReturnLine
                        orderby p.JobInvoiceReturnLineId
                        select p.JobInvoiceReturnLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceReturnLine
                        orderby p.JobInvoiceReturnLineId
                        select p.JobInvoiceReturnLineId).FirstOrDefault();
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

                temp = (from p in db.JobInvoiceReturnLine
                        orderby p.JobInvoiceReturnLineId
                        select p.JobInvoiceReturnLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceReturnLine
                        orderby p.JobInvoiceReturnLineId
                        select p.JobInvoiceReturnLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<JobInvoiceListViewModel> GetPendingJobInvoiceHelpList(int Id, string term)
        {

            var InvoiceReturnHeader = new JobInvoiceReturnHeaderService(db).Find(Id);

            var settings = db.JobInvoiceSettings
            .Where(m => m.DocTypeId == InvoiceReturnHeader.DocTypeId && m.DivisionId == InvoiceReturnHeader.DivisionId && m.SiteId == InvoiceReturnHeader.SiteId).FirstOrDefault();

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

            var list = (from p in db.ViewJobInvoiceBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.JobInvoiceNo.ToLower().Contains(term.ToLower())) && p.JobWorkerId == InvoiceReturnHeader.JobWorkerId && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.JobInvoiceDocTypeId.ToString()))
                          && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.JobInvoiceHeaderId into g
                        select new JobInvoiceListViewModel
                        {
                            DocNo = g.Max(m => m.p.JobInvoiceNo),
                            JobInvoiceHeaderId = g.Key,
                        }
                          ).Take(20);

            return list.ToList();
        }

        public IEnumerable<JobReceiveListViewModel> GetPendingJobReceiveHelpList(int Id, string term)
        {

            var InvoiceReturnHeader = new JobInvoiceReturnHeaderService(db).Find(Id);

            var settings = db.JobInvoiceSettings
            .Where(m => m.DocTypeId == InvoiceReturnHeader.DocTypeId && m.DivisionId == InvoiceReturnHeader.DivisionId && m.SiteId == InvoiceReturnHeader.SiteId).FirstOrDefault();

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

            var list = (from p in db.ViewJobInvoiceBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.JobReceiveNo.ToLower().Contains(term.ToLower())) && p.JobWorkerId == InvoiceReturnHeader.JobWorkerId && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.JobInvoiceDocTypeId.ToString()))
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


        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var JobInvoiceReturn = new JobInvoiceReturnHeaderService(db).Find(Id);

            var settings = db.JobInvoiceSettings
            .Where(m => m.DocTypeId == JobInvoiceReturn.DocTypeId && m.DivisionId == JobInvoiceReturn.DivisionId && m.SiteId == JobInvoiceReturn.SiteId).FirstOrDefault();

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
            var Max = (from p in db.JobInvoiceReturnLine
                       where p.JobInvoiceReturnHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }

        public JobInvoiceLineViewModel GetJobInvoiceLineBalance(int id)
        {
            var temp = (from VJIB in db.ViewJobInvoiceBalance
                    join Jil in db.JobInvoiceLine on VJIB.JobInvoiceLineId equals Jil.JobInvoiceLineId into JobInvoiceLineTable
                        from JobInvoiceLineTab in JobInvoiceLineTable.DefaultIfEmpty()
                        join Jrl in db.JobReceiveLine on JobInvoiceLineTab.JobReceiveLineId equals Jrl.JobReceiveLineId into JobReceiveLineTable
                    from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                        join Jih in db.JobInvoiceHeader on JobInvoiceLineTab.JobInvoiceHeaderId equals Jih.JobInvoiceHeaderId into JobInvoiceHeaderTable
                    from JobInvoiceHeaderTab in JobInvoiceHeaderTable.DefaultIfEmpty()
                    join Jrh in db.JobReceiveHeader on JobReceiveLineTab.JobReceiveHeaderId equals Jrh.JobReceiveHeaderId into JobReceiveHeaderTable
                    from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                        where JobInvoiceLineTab.JobInvoiceLineId == id
                    select new JobInvoiceLineViewModel
                    {

                        JobWorkerId = JobInvoiceHeaderTab.JobWorkerId.Value,
                        Amount = JobInvoiceLineTab.Amount,
                        ProductId = JobReceiveLineTab.ProductId,
                        ProductName = JobReceiveLineTab.Product.ProductName,
                        JobReceiveLineId = JobInvoiceLineTab.JobReceiveLineId,
                        JobReceiveDocNo = JobReceiveHeaderTab.DocNo,
                        JobInvoiceHeaderId = JobInvoiceLineTab.JobInvoiceHeaderId,
                        JobInvoiceLineId = JobInvoiceLineTab.JobInvoiceLineId,
                        InvoiceDocNo = JobInvoiceLineTab.JobInvoiceHeader.DocNo,
                        Qty = VJIB.BalanceQty,
                        Rate = JobInvoiceLineTab.Rate,
                        Remark = JobInvoiceLineTab.Remark,
                        UnitConversionMultiplier = JobInvoiceLineTab.UnitConversionMultiplier,
                        DealUnitId = JobInvoiceLineTab.DealUnitId,
                        DealQty = JobInvoiceLineTab.DealQty,
                        UnitId = JobReceiveLineTab.Product.UnitId,
                        Dimension1Id = JobReceiveLineTab.Dimension1Id,
                        Dimension1Name = JobReceiveLineTab.Dimension1.Dimension1Name,
                        Dimension2Id = JobReceiveLineTab.Dimension2Id,
                        Dimension2Name = JobReceiveLineTab.Dimension2.Dimension2Name,
                        Dimension3Id = JobReceiveLineTab.Dimension3Id,
                        Dimension3Name = JobReceiveLineTab.Dimension3.Dimension3Name,
                        Dimension4Id = JobReceiveLineTab.Dimension4Id,
                        Dimension4Name = JobReceiveLineTab.Dimension4.Dimension4Name,
                        Specification = JobReceiveLineTab.Specification,
                        LotNo = JobReceiveLineTab.LotNo,
                        SalesTaxGroupPersonId = JobInvoiceLineTab.JobInvoiceHeader.SalesTaxGroupPersonId,
                        SalesTaxGroupProductId = JobInvoiceLineTab.SalesTaxGroupProductId,
                        CostCenterId = JobInvoiceLineTab.CostCenterId,
                        CostCenterName = JobInvoiceLineTab.CostCenter.CostCenterName,
                        //DiscountPer = p.DiscountPer
                        Weight = JobReceiveLineTab.Qty == 0 ? 0 : (JobReceiveLineTab.Weight / JobReceiveLineTab.Qty) * VJIB.BalanceQty,
                    }).FirstOrDefault();

            var JobInvoiceLineId = (from p in db.JobInvoiceLine
                                  where p.JobInvoiceLineId == temp.JobInvoiceLineId
                                  select new { LineId = p.JobInvoiceLineId, HeaderId = p.JobInvoiceHeaderId }).FirstOrDefault();


            var Charges = (from p in db.JobInvoiceLineCharge
                           where p.LineTableId == JobInvoiceLineId.LineId
                           join t in db.Charge on p.ChargeId equals t.ChargeId
                           select new LineCharges
                           {
                               ChargeCode = t.ChargeCode,
                               Rate = p.Rate,
                               LedgerAccountCrId = p.LedgerAccountCrId,
                               LedgerAccountDrId = p.LedgerAccountDrId,
                           }).ToList();

            var HeaderCharges = (from p in db.JobInvoiceHeaderCharges
                                 where p.HeaderTableId == JobInvoiceLineId.HeaderId
                                 join t in db.Charge on p.ChargeId equals t.ChargeId
                                 select new HeaderCharges
                                 {
                                     ChargeCode = t.ChargeCode,
                                     Rate = p.Rate,
                                 }).ToList();

            temp.RHeaderCharges = HeaderCharges;
            temp.RLineCharges = Charges;

            return (temp);
        }


        public object GetProductUidDetail(string ProductUidName, int filter)
        {
            var Header = new JobInvoiceReturnHeaderService(db).Find(filter);

            var temp = (from i in db.JobInvoiceLine
                        join L in db.JobReceiveLine on i.JobReceiveLineId equals L.JobReceiveLineId
                        join H in db.JobReceiveHeader on L.JobReceiveHeaderId equals H.JobReceiveHeaderId into JobReceiveHeaderTable
                        from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                        join Jol in db.JobOrderLine on L.JobOrderLineId equals Jol.JobOrderLineId into JobOrderLineTable
                        from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                        join Pu in db.ProductUid on (JobOrderLineTab.ProductUidHeaderId == null ? JobOrderLineTab.ProductUidId : L.ProductUidId) equals Pu.ProductUIDId into ProductUidTable
                        from ProductUidTab in ProductUidTable.DefaultIfEmpty()
                        where ProductUidTab.ProductUidName == ProductUidName && JobReceiveHeaderTab.ProcessId == Header.ProcessId && JobReceiveHeaderTab.SiteId == Header.SiteId
                        && JobReceiveHeaderTab.DivisionId == Header.DivisionId && JobReceiveHeaderTab.JobWorkerId == Header.JobWorkerId
                        orderby JobReceiveHeaderTab.DocDate
                        select new
                        {
                            ProductUidId = ProductUidTab.ProductUIDId,
                            JobInvoiceLineId = i.JobInvoiceLineId,
                            JobInvoiceDocNo = i.JobInvoiceHeader.DocNo,
                            Success = (i.JobInvoiceHeader.JobWorkerId == Header.JobWorkerId ? true : false),
                            ProdUidHeaderId = JobOrderLineTab.ProductUidHeaderId,
                        }).ToList().Last();
            return temp;
        }

        public string GetFirstBarCodeForReturn(int JobInvoiceLineId)
        {
            return (from t2 in db.JobInvoiceLine
                    join p in db.JobReceiveLine on t2.JobReceiveLineId equals p.JobReceiveLineId
                    where t2.JobInvoiceLineId == JobInvoiceLineId
                    join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                    select p.ProductUidId).FirstOrDefault().ToString();
        }

        public string GetFirstBarCodeForReturn(int[] JobInvoiceLineIds)
        {
            return (from t2 in db.JobInvoiceLine
                    join p in db.JobReceiveLine on t2.JobReceiveLineId equals p.JobReceiveLineId
                    where JobInvoiceLineIds.Contains(t2.JobInvoiceLineId)
                    join t in db.ProductUid on p.ProductUidId equals t.ProductUIDId
                    orderby t.ProductUidName
                    select p.ProductUidId).FirstOrDefault().ToString();
        }

        public List<ComboBoxList> GetPendingBarCodesList(string id, int GodownId)
        {
            List<ComboBoxList> Barcodes = new List<ComboBoxList>();

            int[] JobInvoiceLine = id.Split(',').Select(Int32.Parse).ToArray();

            var InvoiceRecords = (from p in db.ViewJobInvoiceBalance
                                  join t in db.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                                  where JobInvoiceLine.Contains(p.JobInvoiceLineId)
                                  select t).ToList();

            int[] BalanceRecRecordsProdUIds = InvoiceRecords.Select(m => m.ProductUidId.Value).ToArray();

            using (ApplicationDbContext context = new ApplicationDbContext())
            {

                //context.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                //context.Database.CommandTimeout = 30000;

                var Temp = (from p in context.ProductUid
                            where BalanceRecRecordsProdUIds.Contains(p.ProductUIDId)
                            && p.CurrenctGodownId == GodownId && p.Status == ProductUidStatusConstants.Receive
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


        public IEnumerable<ComboBoxResult> GetJobInvoiceHelpListForProduct(int Id, string term)
        {
            var JobInvoiceHeader = new JobInvoiceReturnHeaderService(db).Find(Id);

            var settings = db.JobInvoiceSettings
            .Where(m => m.DocTypeId == JobInvoiceHeader.DocTypeId && m.DivisionId == JobInvoiceHeader.DivisionId && m.SiteId == JobInvoiceHeader.SiteId).FirstOrDefault();


            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            return (from VB in db.ViewJobInvoiceBalance
                    join L in db.JobInvoiceLine on VB.JobInvoiceLineId equals L.JobInvoiceLineId into JobInvoiceLineTable
                    from JobInvoiceLineTab in JobInvoiceLineTable.DefaultIfEmpty()
                    where JobInvoiceLineTab.JobInvoiceHeader.JobWorkerId == JobInvoiceHeader.JobWorkerId
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? VB.SiteId == CurrentSiteId : contraSites.Contains(VB.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? VB.DivisionId == CurrentDivisionId : contraDivisions.Contains(VB.DivisionId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : JobInvoiceLineTab.JobInvoiceHeader.DocNo.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobInvoiceLineTab.JobInvoiceHeader.DocType.DocumentTypeShortName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobInvoiceLineTab.JobReceiveLine.ProductUid.ProductUidName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobInvoiceLineTab.JobReceiveLine.Product.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobInvoiceLineTab.JobReceiveLine.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobInvoiceLineTab.JobReceiveLine.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobInvoiceLineTab.JobReceiveLine.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : JobInvoiceLineTab.JobReceiveLine.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower())
                        )
                    select new ComboBoxResult
                    {
                        id = VB.JobInvoiceLineId.ToString(),
                        text = JobInvoiceLineTab.JobReceiveLine.ProductUid != null ? (JobInvoiceLineTab.JobReceiveLine.ProductUid.ProductUidName + " : " + JobInvoiceLineTab.JobReceiveLine.Product.ProductName) : JobInvoiceLineTab.JobReceiveLine.Product.ProductName,
                        TextProp1 = "Balance :" + VB.BalanceQty,
                        TextProp2 = "Date :" + JobInvoiceLineTab.JobInvoiceHeader.DocDate,
                        AProp1 = JobInvoiceLineTab.JobInvoiceHeader.DocType.DocumentTypeShortName + "-" + JobInvoiceLineTab.JobInvoiceHeader.DocNo,
                        AProp2 = ((JobInvoiceLineTab.JobReceiveLine.Dimension1.Dimension1Name == null) ? "" : JobInvoiceLineTab.JobReceiveLine.Dimension1.Dimension1Name) +
                                    ((JobInvoiceLineTab.JobReceiveLine.Dimension2.Dimension2Name == null) ? "" : "," + JobInvoiceLineTab.JobReceiveLine.Dimension2.Dimension2Name) +
                                    ((JobInvoiceLineTab.JobReceiveLine.Dimension3.Dimension3Name == null) ? "" : "," + JobInvoiceLineTab.JobReceiveLine.Dimension3.Dimension3Name) +
                                    ((JobInvoiceLineTab.JobReceiveLine.Dimension4.Dimension4Name == null) ? "" : "," + JobInvoiceLineTab.JobReceiveLine.Dimension4.Dimension4Name)
                    });
        }



        public IEnumerable<ComboBoxResult> GetCostCenterForPerson(int Id, string term)
        {
            var JobInvoiceReturnHeader = new JobInvoiceReturnHeaderService(db).Find(Id);

            var settings = db.JobInvoiceSettings
            .Where(m => m.DocTypeId == JobInvoiceReturnHeader.DocTypeId && m.DivisionId == JobInvoiceReturnHeader.DivisionId && m.SiteId == JobInvoiceReturnHeader.SiteId).FirstOrDefault();

            int LedgerAccountId = 0;
            var LedgerAccount_Temp = (from L in db.LedgerAccount where L.PersonId == JobInvoiceReturnHeader.JobWorkerId select L).FirstOrDefault();
            if (LedgerAccount_Temp != null)
                LedgerAccountId = LedgerAccount_Temp.LedgerAccountId;

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var CostCenterList = (from C in db.CostCenter
                        where C.LedgerAccountId == LedgerAccountId
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? C.SiteId == CurrentSiteId : contraSites.Contains(C.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? C.DivisionId == CurrentDivisionId : contraDivisions.Contains(C.DivisionId.ToString()))
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : C.CostCenterName.ToLower().Contains(term.ToLower()))
                        && C.IsActive == true && C.Status != (int)StatusConstants.Closed
                        orderby C.CostCenterName
                        select new ComboBoxResult
                        {
                            text = C.CostCenterName + " | " + C.DocType.DocumentTypeShortName,
                            id = C.CostCenterId.ToString(),
                        });
         
            return CostCenterList;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<JobInvoiceReturnLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobInvoiceReturnLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
