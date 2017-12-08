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

namespace Service
{
    public interface IPurchaseIndentCancelLineService : IDisposable
    {
        PurchaseIndentCancelLine Create(PurchaseIndentCancelLine p);
        void Delete(int id);
        void Delete(PurchaseIndentCancelLine p);
        IEnumerable<PurchaseIndentCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseIndentCancelLine p);
        PurchaseIndentCancelLine Add(PurchaseIndentCancelLine p);
        IQueryable<PurchaseIndentCancelLineViewModel> GetPurchaseIndentCancelLineListForHeader(int PurchaseIndentCancelHeaderId);
        Task<IEquatable<PurchaseIndentCancelLine>> GetAsync();
        Task<PurchaseIndentCancelLine> FindAsync(int id);
        PurchaseIndentCancelLine Find(int id);
        IEnumerable<PurchaseIndentCancelLineViewModel> GetPurchaseIndentLineForMultiSelect(PurchaseIndentCancelFilterViewModel svm);
        PurchaseIndentCancelLineViewModel GetPurchaseIndentCancelLine(int id);//PurchaseIndentCancelLine Id
        IEnumerable<PurchaseIndentLineBalance> GetPurchaseIndentForProduct(int id, int PurchaseIndentCancelHeaderId);//ProductId
        PurchaseIndentCancelLineViewModel GetBalanceQuantity(int id);//PurchaseIndentLineId
        IEnumerable<PurchaseIndentCancelLine> GetPurchaseIndentCancelLineForMaterialPlanCancel(int id);
        int GetMaxSr(int id);
    }

    public class PurchaseIndentCancelLineService : IPurchaseIndentCancelLineService
    {
        ApplicationDbContext db =new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseIndentCancelLine> _PurchaseIndentCancelLineRepository;
        RepositoryQuery<PurchaseIndentCancelLine> PurchaseIndentCancelLineRepository;
        public PurchaseIndentCancelLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseIndentCancelLineRepository = new Repository<PurchaseIndentCancelLine>(db);
            PurchaseIndentCancelLineRepository = new RepositoryQuery<PurchaseIndentCancelLine>(_PurchaseIndentCancelLineRepository);
        }     

        public PurchaseIndentCancelLine Create(PurchaseIndentCancelLine p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseIndentCancelLine>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseIndentCancelLine>().Delete(id);
        }

        public void Delete(PurchaseIndentCancelLine p)
        {
            _unitOfWork.Repository<PurchaseIndentCancelLine>().Delete(p);
        }
        public IEnumerable<PurchaseIndentLineBalance> GetPurchaseIndentForProduct(int id, int PurchaseIndentCancelHeaderId)
        {

            var PurchaseIndentCancel = new PurchaseIndentCancelHeaderService(_unitOfWork).Find(PurchaseIndentCancelHeaderId);

            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(PurchaseIndentCancel.DocTypeId, PurchaseIndentCancel.DivisionId, PurchaseIndentCancel.SiteId);

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


            return (from p in db.ViewPurchaseIndentBalance
                    join t in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t.PurchaseIndentLineId into table from tab in table.DefaultIfEmpty()
                    join t1 in db.MaterialPlanLine on tab.MaterialPlanLineId equals t1.MaterialPlanLineId into table1 from tab1 in table1.DefaultIfEmpty()
                    join t2 in db.MaterialPlanHeader on tab1.MaterialPlanHeaderId equals t2.MaterialPlanHeaderId into table2 from tab2 in table2.DefaultIfEmpty()
                    where p.ProductId == id && p.BalanceQty > 0
                     && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    select new PurchaseIndentLineBalance
                    {
                        PurchaseIndentDocNo = p.PurchaseIndentNo,
                        PurchaseIndentLineId = p.PurchaseIndentLineId,                       
                        MaterialPlanDocNo=tab2.DocNo,                       
                    }
                        );
        }
        public PurchaseIndentCancelLineViewModel GetBalanceQuantity(int id)
        {
            return (from p in db.ViewPurchaseIndentBalance
                    join t in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t.PurchaseIndentLineId into table from tab in table.DefaultIfEmpty()
                    where p.PurchaseIndentLineId == id
                    select new PurchaseIndentCancelLineViewModel{
                        Specification=tab.Specification,
                        Qty=p.BalanceQty,
                        BalanceQty=p.BalanceQty,
                        Dimension1Name=tab.Dimension1.Dimension1Name,
                        Dimension2Name=tab.Dimension2.Dimension2Name,                        
                        }
                        ).FirstOrDefault();
        }
        public void Update(PurchaseIndentCancelLine p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseIndentCancelLine>().Update(p);
        }

        public IEnumerable<PurchaseIndentCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseIndentCancelLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PurchaseIndentCancelLineId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public PurchaseIndentCancelLine Find(int id)
        {
            return _unitOfWork.Repository<PurchaseIndentCancelLine>().Find(id);
        }


        public IQueryable<PurchaseIndentCancelLineViewModel> GetPurchaseIndentCancelLineListForHeader(int PurchaseIndentCancelHeaderId)
        {

            return (from p in db.PurchaseIndentCancelLine
                    join t in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t.PurchaseIndentLineId into table from tab in table.DefaultIfEmpty()
                    join t1 in db.PurchaseIndentHeader on tab.PurchaseIndentHeaderId equals t1.PurchaseIndentHeaderId into table1 from tab1 in table1.DefaultIfEmpty()
                    join t2 in db.Product on tab.ProductId equals t2.ProductId into table2 from tab2 in table2.DefaultIfEmpty()
                    join t3 in db.MaterialPlanLine on tab.MaterialPlanLineId equals t3.MaterialPlanLineId into table3 from tab3 in table3.DefaultIfEmpty()
                    join t4 in db.MaterialPlanHeader on tab3.MaterialPlanHeaderId equals t4.MaterialPlanHeaderId into table4 from tab4 in table4.DefaultIfEmpty()
                    where p.PurchaseIndentCancelHeaderId == PurchaseIndentCancelHeaderId
                    orderby p.Sr
                    select new PurchaseIndentCancelLineViewModel
                    {
                        DocNo=tab1.DocNo,
                        PurchaseIndentCancelHeaderId=p.PurchaseIndentCancelHeaderId,
                        PurchaseIndentCancelLineId=p.PurchaseIndentCancelLineId,
                        PurchaseIndentLineId=p.PurchaseIndentLineId,
                        Qty=p.Qty,
                        ProductId=tab2.ProductId,
                        ProductName=tab2.ProductName,
                        UnitId=tab2.UnitId,
                        MaterialPlanDocNo=tab4.DocNo
                    });

        }

        public PurchaseIndentCancelLine Add(PurchaseIndentCancelLine p)
        {
            _unitOfWork.Repository<PurchaseIndentCancelLine>().Insert(p);
            return p;
        }
        public PurchaseIndentCancelLineViewModel GetPurchaseIndentCancelLine(int id)
        {
            var temp = (from p in db.PurchaseIndentCancelLine
                        join t in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t.PurchaseIndentLineId into table
                        from tab in table.DefaultIfEmpty()
                        join t2 in db.PurchaseIndentHeader on tab.PurchaseIndentHeaderId equals t2.PurchaseIndentHeaderId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join t1 in db.Product on tab.ProductId equals t1.ProductId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join t2 in db.ViewPurchaseIndentBalance on p.PurchaseIndentLineId equals t2.PurchaseIndentLineId into table3 from tab3 in table3.DefaultIfEmpty()
                        where p.PurchaseIndentCancelLineId==id
                        select new PurchaseIndentCancelLineViewModel
                        {
                            DocNo = tab2.DocNo,
                            ProductId = tab1.ProductId,
                            ProductName = tab1.ProductName,
                            PurchaseIndentCancelHeaderId = p.PurchaseIndentCancelHeaderId,
                            PurchaseIndentCancelLineId = p.PurchaseIndentCancelLineId,
                            Qty = p.Qty,
                            PurchaseIndentLineId = p.PurchaseIndentLineId,
                            BalanceQty=(tab3==null ? p.Qty : tab3.BalanceQty+p.Qty),
                            Dimension1Name=tab.Dimension1.Dimension1Name,
                            Dimension2Name=tab.Dimension2.Dimension2Name,
                            Specification=tab.Specification,
                            LockReason=p.LockReason,
                        }
                          );
            return temp.FirstOrDefault();
        }
        public void Dispose()
        {
        }

        public IEnumerable<PurchaseIndentCancelLineViewModel> GetPurchaseIndentLineForMultiSelect(PurchaseIndentCancelFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.PurchaseIndentId)) { SaleOrderIdArr = svm.PurchaseIndentId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewPurchaseIndentBalance
                        join product in db.Product on p.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join t2 in db.PurchaseIndentLine on p.PurchaseIndentLineId equals t2.PurchaseIndentLineId
                        where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                        && (string.IsNullOrEmpty(svm.PurchaseIndentId) ? 1 == 1 : SaleOrderIdArr.Contains(p.PurchaseIndentHeaderId.ToString()))
                        && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0
                        orderby p.IndentDate,p.PurchaseIndentNo, t2.Sr
                        select new PurchaseIndentCancelLineViewModel
                        {
                            BalanceQty = p.BalanceQty,
                            Qty = p.BalanceQty,
                            DocNo = p.PurchaseIndentNo,
                            ProductName = tab2.ProductName,
                            ProductId = p.ProductId,
                            PurchaseIndentCancelHeaderId = svm.PurchaseIndentCancelHeaderId,
                            PurchaseIndentLineId = p.PurchaseIndentLineId,
                            unitDecimalPlaces=tab2.Unit.DecimalPlaces,
                        });
            return temp;
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.PurchaseIndentCancelLine
                       where p.PurchaseIndentCancelHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }

        public IEnumerable<PurchaseIndentCancelLine> GetPurchaseIndentCancelLineForMaterialPlanCancel(int id)
        {
            return (from p in db.PurchaseIndentCancelLine
                    where p.MaterialPlanCancelLineId == id
                    select p
                        );
        }

        public Task<IEquatable<PurchaseIndentCancelLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseIndentCancelLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }   


    }
}
