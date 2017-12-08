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
    public interface IPurchaseOrderCancelLineService : IDisposable
    {
        PurchaseOrderCancelLine Create(PurchaseOrderCancelLine pt);
        void Delete(int id);
        void Delete(PurchaseOrderCancelLine pt);
        PurchaseOrderCancelLine Find(int id);
        IEnumerable<PurchaseOrderCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseOrderCancelLine pt);
        PurchaseOrderCancelLine Add(PurchaseOrderCancelLine pt);
        IEnumerable<PurchaseOrderCancelLine> GetPurchaseOrderCancelLineList();
        IEnumerable<PurchaseOrderCancelLineViewModel> GetPurchaseOrderCancelLineForHeader(int id);//Header Id
        Task<IEquatable<PurchaseOrderCancelLine>> GetAsync();
        Task<PurchaseOrderCancelLine> FindAsync(int id);
        IEnumerable<PurchaseOrderCancelLineViewModel> GetPurchaseOrderLineForMultiSelect(PurchaseOrderCancelFilterViewModel svm);
        PurchaseOrderCancelLineViewModel GetPurchaseOrderCancelLine(int id);//Line Id
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetPendingPurchaseOrderHelpList(int Id, string term);
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);
    }

    public class PurchaseOrderCancelLineService : IPurchaseOrderCancelLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseOrderCancelLine> _PurchaseOrderCancelLineRepository;
        RepositoryQuery<PurchaseOrderCancelLine> PurchaseOrderCancelLineRepository;
        public PurchaseOrderCancelLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseOrderCancelLineRepository = new Repository<PurchaseOrderCancelLine>(db);
            PurchaseOrderCancelLineRepository = new RepositoryQuery<PurchaseOrderCancelLine>(_PurchaseOrderCancelLineRepository);
        }


        public PurchaseOrderCancelLine Find(int id)
        {
            return _unitOfWork.Repository<PurchaseOrderCancelLine>().Find(id);
        }

        public PurchaseOrderCancelLine Create(PurchaseOrderCancelLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseOrderCancelLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseOrderCancelLine>().Delete(id);
        }

        public void Delete(PurchaseOrderCancelLine pt)
        {
            _unitOfWork.Repository<PurchaseOrderCancelLine>().Delete(pt);
        }

        public void Update(PurchaseOrderCancelLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseOrderCancelLine>().Update(pt);
        }

        public IEnumerable<PurchaseOrderCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseOrderCancelLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PurchaseOrderCancelLineId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<PurchaseOrderCancelLineViewModel> GetPurchaseOrderCancelLineForHeader(int id)
        {
            return (from p in db.PurchaseOrderCancelLine
                    join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t5 in db.PurchaseOrderHeader on tab.PurchaseOrderHeaderId equals t5.PurchaseOrderHeaderId
                    join t1 in db.Dimension1 on tab.Dimension1Id equals t1.Dimension1Id into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join t2 in db.Dimension2 on tab.Dimension2Id equals t2.Dimension2Id into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t3 in db.Product on tab.ProductId equals t3.ProductId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t4 in db.PurchaseOrderCancelHeader on p.PurchaseOrderCancelHeaderId equals t4.PurchaseOrderCancelHeaderId 
                    join t6 in db.Persons on t4.SupplierId equals t6.PersonID into table6
                    from tab6 in table6.DefaultIfEmpty()                    
                    where p.PurchaseOrderCancelHeaderId == id
                    orderby p.Sr
                    select new PurchaseOrderCancelLineViewModel
                    {
                        Dimension1Name = tab1.Dimension1Name,
                        Dimension2Name = tab2.Dimension2Name,
                        DueDate = tab.DueDate,
                        LotNo = tab.LotNo,
                        ProductId = tab.ProductId,
                        ProductName = tab3.ProductName,
                        PurchaseOrderCancelHeaderDocNo = t4.DocNo,
                        PurchaseOrderCancelHeaderId = p.PurchaseOrderCancelHeaderId,
                        PurchaseOrderCancelLineId = p.PurchaseOrderCancelLineId,
                        PurchaseOrderDocNo = t5.DocNo,
                        PurchaseOrderLineId = tab.PurchaseOrderLineId,
                        OrderDocTypeId = t5.DocTypeId,
                        OrderHeaderId=t5.PurchaseOrderHeaderId,
                        Qty = p.Qty,
                        Remark = p.Remark,
                        Specification = tab.Specification,
                        SupplierId = t4.SupplierId,
                        SupplierName = tab6.Name,
                        UnitId = tab3.UnitId,
                    }
                        );

        }

        public IEnumerable<PurchaseOrderCancelLineViewModel> GetPurchaseOrderLineForMultiSelect(PurchaseOrderCancelFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.PurchaseOrderId)) { SaleOrderIdArr = svm.PurchaseOrderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewPurchaseOrderBalance
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table1 from tab1 in table1.DefaultIfEmpty()
                        join t2 in db.PurchaseOrderHeader on tab1.PurchaseOrderHeaderId equals t2.PurchaseOrderHeaderId
                        where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (svm.SupplierId == 0 ? 1 == 1 : p.SupplierId == svm.SupplierId)
                        && (string.IsNullOrEmpty(svm.PurchaseOrderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0 && p.SupplierId==svm.SupplierId
                        orderby t2.DocDate,t2.DocNo,tab1.Sr
                        select new PurchaseOrderCancelLineViewModel
                        {
                            BalanceQty = p.BalanceQty,
                            Qty = 0,
                            PurchaseOrderDocNo = p.PurchaseOrderNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            PurchaseOrderCancelHeaderId = svm.PurchaseOrderCancelHeaderId,
                            PurchaseOrderLineId = p.PurchaseOrderLineId,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces=tab1.DealUnit.DecimalPlaces,
                        }
                        );
            return temp;
        }
        public PurchaseOrderCancelLineViewModel GetPurchaseOrderCancelLine(int id)
        {
            var temp= (from p in db.PurchaseOrderCancelLine
                 join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId into table
                 from tab in table.DefaultIfEmpty()
                 join t5 in db.PurchaseOrderHeader on tab.PurchaseOrderHeaderId equals t5.PurchaseOrderHeaderId into table5 from tab5 in table5.DefaultIfEmpty()
                 join t1 in db.Dimension1 on tab.Dimension1Id equals t1.Dimension1Id into table1
                 from tab1 in table1.DefaultIfEmpty()
                 join t2 in db.Dimension2 on tab.Dimension2Id equals t2.Dimension2Id into table2
                 from tab2 in table2.DefaultIfEmpty()
                 join t3 in db.Product on tab.ProductId equals t3.ProductId into table3
                 from tab3 in table3.DefaultIfEmpty()
                 join t4 in db.PurchaseOrderCancelHeader on p.PurchaseOrderCancelHeaderId equals t4.PurchaseOrderCancelHeaderId into table4 from tab4 in table4.DefaultIfEmpty()
                 join t6 in db.Persons on tab4.SupplierId equals t6.PersonID into table6
                 from tab6 in table6.DefaultIfEmpty()        
                 join t7 in db.ViewPurchaseOrderBalance on p.PurchaseOrderLineId equals t7.PurchaseOrderLineId into table7 from tab7 in table7.DefaultIfEmpty()
                 orderby p.PurchaseOrderCancelLineId 
                 where p.PurchaseOrderCancelLineId == id
                 select new PurchaseOrderCancelLineViewModel
                 {
                     Dimension1Name = tab1.Dimension1Name,
                     Dimension2Name = tab2.Dimension2Name,
                     DueDate = tab.DueDate,
                     LotNo = tab.LotNo,
                     ProductId = tab.ProductId,
                     ProductName = tab3.ProductName,
                     PurchaseOrderCancelHeaderDocNo = tab4.DocNo,
                     PurchaseOrderCancelHeaderId = p.PurchaseOrderCancelHeaderId,
                     PurchaseOrderCancelLineId = p.PurchaseOrderCancelLineId,
                     PurchaseOrderDocNo = tab5.DocNo,
                     PurchaseOrderLineId = tab.PurchaseOrderLineId,
                     BalanceQty=p.Qty+tab7.BalanceQty,
                     Qty = p.Qty,
                     Remark = p.Remark,
                     Specification = tab.Specification,
                     SupplierId = tab4.SupplierId,
                     SupplierName = tab6.Name,
                     UnitId = tab3.UnitId,
                     LockReason=p.LockReason,
                 }

                      ).FirstOrDefault();

            return temp;
               
        }

        public IEnumerable<PurchaseOrderCancelLine> GetPurchaseOrderCancelLineList()
        {
            var pt = _unitOfWork.Repository<PurchaseOrderCancelLine>().Query().Get().OrderBy(m=>m.PurchaseOrderCancelLineId);

            return pt;
        }

        public PurchaseOrderCancelLine Add(PurchaseOrderCancelLine pt)
        {
            _unitOfWork.Repository<PurchaseOrderCancelLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseOrderCancelLine
                        orderby p.PurchaseOrderCancelLineId
                        select p.PurchaseOrderCancelLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderCancelLine
                        orderby p.PurchaseOrderCancelLineId
                        select p.PurchaseOrderCancelLineId).FirstOrDefault();
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

                temp = (from p in db.PurchaseOrderCancelLine
                        orderby p.PurchaseOrderCancelLineId
                        select p.PurchaseOrderCancelLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderCancelLine
                        orderby p.PurchaseOrderCancelLineId
                        select p.PurchaseOrderCancelLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IQueryable<ComboBoxResult> GetPendingPurchaseOrderHelpList(int Id, string term)
        {

            var PurchaseOrderCancel = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(PurchaseOrderCancel.DocTypeId, PurchaseOrderCancel.DivisionId, PurchaseOrderCancel.SiteId);

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

            var list = (from p in db.ViewPurchaseOrderBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseOrderNo.ToLower().Contains(term.ToLower())) && p.SupplierId == PurchaseOrderCancel.SupplierId && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.PurchaseOrderHeaderId into g
                        orderby g.Max(m=>m.p.PurchaseOrderNo)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.PurchaseOrderNo),
                            id = (g.Key).ToString(),
                            //    DocumentTypeName=g.Max(p=>p.p.DocumentTypeShortName)
                        }
                          );

            return list;
        }

        public IQueryable<ComboBoxResult> GetPendingOrderHelpList(int Id, string term)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }          

            var list = (from p in db.ViewPurchaseOrderBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { p } by p.PurchaseOrderHeaderId into g
                        orderby g.Max(m => m.p.PurchaseOrderNo)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.PurchaseOrderNo),
                            id = (g.Key).ToString(),
                            //    DocumentTypeName=g.Max(p=>p.p.DocumentTypeShortName)
                        }
                          );

            return list;
        }


        public IQueryable<ComboBoxResult> GetPendingSupplierHelpList(int Id, string term)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewPurchaseOrderBalance
                        join t in db.Persons on p.SupplierId equals t.PersonID
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { t } by t.PersonID into g
                        orderby g.Max(m => m.t.Name)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t.Name),
                            id = (g.Key).ToString(),
                            //    DocumentTypeName=g.Max(p=>p.p.DocumentTypeShortName)
                        }
                          );

            return list;
        }


        public IQueryable<ComboBoxResult> GetPendingProductHelpList(int Id, string term)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Id, CurrentDivisionId, CurrentSiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewPurchaseOrderBalance
                        join t in db.Product on p.ProductId equals t.ProductId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.PurchaseOrderNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                        group new { t } by p.ProductId into g
                        orderby g.Max(m => m.t.ProductName)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.t.ProductName),
                            id = (g.Key).ToString(),
                            //    DocumentTypeName=g.Max(p=>p.p.DocumentTypeShortName)
                        }
                          );

            return list;
        }

        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var PurchaseOrderCancel = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(PurchaseOrderCancel.DocTypeId, PurchaseOrderCancel.DivisionId, PurchaseOrderCancel.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            var list = (from p in db.Product
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))                       
                        select new ComboBoxList
                        {
                            PropFirst = p.ProductName,
                            Id =p.ProductId,                            
                        }
                          ).Take(20);

            return list.ToList();
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.PurchaseOrderCancelLine
                       where p.PurchaseOrderCancelHeaderId == id
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


        public Task<IEquatable<PurchaseOrderCancelLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrderCancelLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
