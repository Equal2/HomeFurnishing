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
    public interface ISaleDeliveryOrderCancelLineService : IDisposable
    {
        SaleDeliveryOrderCancelLine Create(SaleDeliveryOrderCancelLine pt);
        void Delete(int id);
        void Delete(SaleDeliveryOrderCancelLine pt);
        SaleDeliveryOrderCancelLine Find(int id);
        IEnumerable<SaleDeliveryOrderCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleDeliveryOrderCancelLine pt);
        SaleDeliveryOrderCancelLine Add(SaleDeliveryOrderCancelLine pt);
        IEnumerable<SaleDeliveryOrderCancelLine> GetSaleDeliveryOrderCancelLineList();
        IEnumerable<SaleDeliveryOrderCancelLineViewModel> GetSaleDeliveryOrderCancelLineForHeader(int id);//Header Id
        Task<IEquatable<SaleDeliveryOrderCancelLine>> GetAsync();
        Task<SaleDeliveryOrderCancelLine> FindAsync(int id);
        IEnumerable<SaleDeliveryOrderCancelLineViewModel> GetSaleDeliveryOrderLineForMultiSelect(SaleDeliveryOrderCancelFilterViewModel svm);
        SaleDeliveryOrderCancelLineViewModel GetSaleDeliveryOrderCancelLine(int id);//Line Id
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetPendingSaleDeliveryOrderHelpList(int Id, string term);
        IQueryable<ComboBoxResult> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);
    }

    public class SaleDeliveryOrderCancelLineService : ISaleDeliveryOrderCancelLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleDeliveryOrderCancelLine> _SaleDeliveryOrderCancelLineRepository;
        RepositoryQuery<SaleDeliveryOrderCancelLine> SaleDeliveryOrderCancelLineRepository;
        public SaleDeliveryOrderCancelLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleDeliveryOrderCancelLineRepository = new Repository<SaleDeliveryOrderCancelLine>(db);
            SaleDeliveryOrderCancelLineRepository = new RepositoryQuery<SaleDeliveryOrderCancelLine>(_SaleDeliveryOrderCancelLineRepository);
        }


        public SaleDeliveryOrderCancelLine Find(int id)
        {
            return _unitOfWork.Repository<SaleDeliveryOrderCancelLine>().Find(id);
        }

        public SaleDeliveryOrderCancelLine Create(SaleDeliveryOrderCancelLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDeliveryOrderCancelLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleDeliveryOrderCancelLine>().Delete(id);
        }

        public void Delete(SaleDeliveryOrderCancelLine pt)
        {
            _unitOfWork.Repository<SaleDeliveryOrderCancelLine>().Delete(pt);
        }

        public void Update(SaleDeliveryOrderCancelLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDeliveryOrderCancelLine>().Update(pt);
        }

        public IEnumerable<SaleDeliveryOrderCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleDeliveryOrderCancelLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SaleDeliveryOrderCancelLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<SaleDeliveryOrderCancelLineViewModel> GetSaleDeliveryOrderCancelLineForHeader(int id)
        {
            return (from p in db.SaleDeliveryOrderCancelLine
                    join t in db.SaleDeliveryOrderLine on p.SaleDeliveryOrderLineId equals t.SaleDeliveryOrderLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t5 in db.SaleDeliveryOrderHeader on tab.SaleDeliveryOrderHeaderId equals t5.SaleDeliveryOrderHeaderId
                    join t6 in db.SaleOrderLine on tab.SaleOrderLineId equals t6.SaleOrderLineId
                    join t1 in db.Dimension1 on t6.Dimension1Id equals t1.Dimension1Id into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join t2 in db.Dimension2 on t6.Dimension2Id equals t2.Dimension2Id into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t3 in db.Product on t6.ProductId equals t3.ProductId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t4 in db.SaleDeliveryOrderCancelHeader on p.SaleDeliveryOrderCancelHeaderId equals t4.SaleDeliveryOrderCancelHeaderId
                    join t7 in db.Persons on t4.BuyerId equals t7.PersonID into table6
                    from tab6 in table6.DefaultIfEmpty()
                    where p.SaleDeliveryOrderCancelHeaderId == id
                    orderby p.Sr
                    select new SaleDeliveryOrderCancelLineViewModel
                    {
                        Dimension1Name = tab1.Dimension1Name,
                        Dimension2Name = tab2.Dimension2Name,
                        //DueDate = tab.DueDate,
                        //LotNo = tab.LotNo,
                        ProductId = tab3.ProductId,
                        ProductName = tab3.ProductName,
                        SaleDeliveryOrderCancelHeaderDocNo = t4.DocNo,
                        SaleDeliveryOrderCancelHeaderId = p.SaleDeliveryOrderCancelHeaderId,
                        SaleDeliveryOrderCancelLineId = p.SaleDeliveryOrderCancelLineId,
                        SaleDeliveryOrderDocNo = t5.DocNo,
                        SaleDeliveryOrderLineId = tab.SaleDeliveryOrderLineId,
                        //OrderDocTypeId = t5.DocTypeId,
                        //OrderHeaderId=t5.SaleDeliveryOrderHeaderId,
                        Qty = p.Qty,
                        Remark = p.Remark,
                        Specification = t6.Specification,
                        BuyerId = t4.BuyerId,
                        BuyerName = tab6.Name,
                        UnitId = tab3.UnitId,
                    }
                        );

        }

        public IEnumerable<SaleDeliveryOrderCancelLineViewModel> GetSaleDeliveryOrderLineForMultiSelect(SaleDeliveryOrderCancelFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.SaleDeliveryOrderId)) { SaleOrderIdArr = svm.SaleDeliveryOrderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.ViewSaleDeliveryOrderBalance
                        join sol in db.SaleOrderLine on p.SaleOrderLineId equals sol.SaleOrderLineId
                        join product in db.Product on sol.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join t in db.SaleDeliveryOrderLine on p.SaleDeliveryOrderLineId equals t.SaleDeliveryOrderLineId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join t2 in db.SaleDeliveryOrderHeader on tab1.SaleDeliveryOrderHeaderId equals t2.SaleDeliveryOrderHeaderId
                        where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(sol.ProductId.ToString()))
                        && (svm.BuyerId == 0 ? 1 == 1 : p.BuyerId == svm.BuyerId)
                        && (string.IsNullOrEmpty(svm.SaleDeliveryOrderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleDeliveryOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && p.BalanceQty > 0 && p.BuyerId == svm.BuyerId
                        orderby t2.DocDate, t2.DocNo, tab1.Sr
                        select new SaleDeliveryOrderCancelLineViewModel
                        {
                            BalanceQty = p.BalanceQty,
                            Qty = 0,
                            SaleDeliveryOrderDocNo = p.SaleDeliveryOrderNo,
                            ProductName = tab2.ProductName,
                            ProductId = sol.ProductId,
                            SaleDeliveryOrderCancelHeaderId = svm.SaleDeliveryOrderCancelHeaderId,
                            SaleDeliveryOrderLineId = p.SaleDeliveryOrderLineId,
                            unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                            DealunitDecimalPlaces = sol.DealUnit.DecimalPlaces,
                        }
                        );
            return temp;
        }
        public SaleDeliveryOrderCancelLineViewModel GetSaleDeliveryOrderCancelLine(int id)
        {
            var temp = (from p in db.SaleDeliveryOrderCancelLine
                        join t in db.SaleDeliveryOrderLine on p.SaleDeliveryOrderLineId equals t.SaleDeliveryOrderLineId into table
                        from tab in table.DefaultIfEmpty()
                        join t5 in db.SaleDeliveryOrderHeader on tab.SaleDeliveryOrderHeaderId equals t5.SaleDeliveryOrderHeaderId into table5
                        from tab5 in table5.DefaultIfEmpty()
                        join sol in db.SaleOrderLine on tab.SaleOrderLineId equals sol.SaleOrderLineId
                        join t1 in db.Dimension1 on sol.Dimension1Id equals t1.Dimension1Id into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join t2 in db.Dimension2 on sol.Dimension2Id equals t2.Dimension2Id into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join t3 in db.Product on sol.ProductId equals t3.ProductId into table3
                        from tab3 in table3.DefaultIfEmpty()
                        join t4 in db.SaleDeliveryOrderCancelHeader on p.SaleDeliveryOrderCancelHeaderId equals t4.SaleDeliveryOrderCancelHeaderId into table4
                        from tab4 in table4.DefaultIfEmpty()
                        join t6 in db.Persons on tab4.BuyerId equals t6.PersonID into table6
                        from tab6 in table6.DefaultIfEmpty()
                        join t7 in db.ViewSaleDeliveryOrderBalance on p.SaleDeliveryOrderLineId equals t7.SaleDeliveryOrderLineId into table7
                        from tab7 in table7.DefaultIfEmpty()
                        orderby p.SaleDeliveryOrderCancelLineId
                        where p.SaleDeliveryOrderCancelLineId == id
                        select new SaleDeliveryOrderCancelLineViewModel
                        {
                            Dimension1Name = tab1.Dimension1Name,
                            Dimension2Name = tab2.Dimension2Name,
                            //DueDate = tab.DueDate,
                            //LotNo = tab.LotNo,
                            ProductId = sol.ProductId,
                            ProductName = tab3.ProductName,
                            SaleDeliveryOrderCancelHeaderDocNo = tab4.DocNo,
                            SaleDeliveryOrderCancelHeaderId = p.SaleDeliveryOrderCancelHeaderId,
                            SaleDeliveryOrderCancelLineId = p.SaleDeliveryOrderCancelLineId,
                            SaleDeliveryOrderDocNo = tab5.DocNo,
                            SaleDeliveryOrderLineId = tab.SaleDeliveryOrderLineId,
                            BalanceQty = p.Qty + tab7.BalanceQty,
                            Qty = p.Qty,
                            Remark = p.Remark,
                            Specification = sol.Specification,
                            BuyerId = tab4.BuyerId,
                            BuyerName = tab6.Name,
                            UnitId = tab3.UnitId,
                            LockReason=p.LockReason,
                        }

                      ).FirstOrDefault();

            return temp;

        }

        public IEnumerable<SaleDeliveryOrderCancelLine> GetSaleDeliveryOrderCancelLineList()
        {
            var pt = _unitOfWork.Repository<SaleDeliveryOrderCancelLine>().Query().Get().OrderBy(m => m.SaleDeliveryOrderCancelLineId);

            return pt;
        }

        public SaleDeliveryOrderCancelLine Add(SaleDeliveryOrderCancelLine pt)
        {
            _unitOfWork.Repository<SaleDeliveryOrderCancelLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleDeliveryOrderCancelLine
                        orderby p.SaleDeliveryOrderCancelLineId
                        select p.SaleDeliveryOrderCancelLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDeliveryOrderCancelLine
                        orderby p.SaleDeliveryOrderCancelLineId
                        select p.SaleDeliveryOrderCancelLineId).FirstOrDefault();
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

                temp = (from p in db.SaleDeliveryOrderCancelLine
                        orderby p.SaleDeliveryOrderCancelLineId
                        select p.SaleDeliveryOrderCancelLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDeliveryOrderCancelLine
                        orderby p.SaleDeliveryOrderCancelLineId
                        select p.SaleDeliveryOrderCancelLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IQueryable<ComboBoxResult> GetPendingSaleDeliveryOrderHelpList(int Id, string term)
        {

            var SaleDeliveryOrderCancel = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(SaleDeliveryOrderCancel.DocTypeId, SaleDeliveryOrderCancel.DivisionId, SaleDeliveryOrderCancel.SiteId);

            //string[] contraDocTypes = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            //else { contraDocTypes = new string[] { "NA" }; }

            //string[] contraSites = null;
            //if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            //else { contraSites = new string[] { "NA" }; }

            //string[] contraDivisions = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            //else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var list = (from p in db.ViewSaleDeliveryOrderBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.SaleDeliveryOrderNo.ToLower().Contains(term.ToLower())) && p.BuyerId == SaleDeliveryOrderCancel.BuyerId && p.BalanceQty > 0
                            //&& (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        && p.SiteId == CurrentSiteId && p.DivisionId == CurrentDivisionId
                        group new { p } by p.SaleDeliveryOrderHeaderId into g
                        orderby g.Max(m=>m.p.SaleDeliveryOrderNo)
                        select new ComboBoxResult
                        {
                            text = g.Max(m => m.p.SaleDeliveryOrderNo),
                            id = g.Key.ToString(),
                        });

            return list;
        }

        public IQueryable<ComboBoxResult> GetProductHelpList(int Id, string term)
        {
            var SaleDeliveryOrderCancel = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(Id);

            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(SaleDeliveryOrderCancel.DocTypeId, SaleDeliveryOrderCancel.DivisionId, SaleDeliveryOrderCancel.SiteId);

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            //string[] ProductTypes = null;
            //if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            //else { ProductTypes = new string[] { "NA" }; }

            var list = (from p in db.ViewSaleDeliveryOrderBalance
                        join sol in db.SaleOrderLine on p.SaleOrderLineId equals sol.SaleOrderLineId
                        join prod in db.Product on sol.ProductId equals prod.ProductId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : prod.ProductName.ToLower().Contains(term.ToLower()))
                        && p.BuyerId == SaleDeliveryOrderCancel.BuyerId && p.SiteId == CurrentSiteId && p.DivisionId == CurrentDivisionId
                        //&& (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
                        group prod by prod.ProductId into g
                        orderby g.Max(m=>m.ProductName)
                        select new ComboBoxResult
                        {
                            text = g.Max(m=>m.ProductName),
                            id = g.Max(m=>m.ProductId.ToString()),
                        });

            return list;
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.SaleDeliveryOrderCancelLine
                       where p.SaleDeliveryOrderCancelHeaderId == id
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


        public Task<IEquatable<SaleDeliveryOrderCancelLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleDeliveryOrderCancelLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
