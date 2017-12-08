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
    public interface IRequisitionCancelLineService : IDisposable
    {
        RequisitionCancelLine Create(RequisitionCancelLine pt);
        void Delete(int id);
        void Delete(RequisitionCancelLine pt);
        RequisitionCancelLine Find(int id);
        void Update(RequisitionCancelLine pt);
        RequisitionCancelLine Add(RequisitionCancelLine pt);
        IEnumerable<RequisitionCancelLineViewModel> GetRequisitionCancelLineForHeader(int id);//Header Id
        Task<IEquatable<RequisitionCancelLine>> GetAsync();
        Task<RequisitionCancelLine> FindAsync(int id);
        RequisitionCancelLineViewModel GetRequisitionCancelLine(int id);//Line Id
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<RequisitionCancelLineViewModel> GetRequisitionLineForOrders(RequisitionCancelFilterViewModel svm);
        RequisitionCancelLineViewModel GetLineDetail(int id);
        IEnumerable<RequisitionCancelProductHelpList> GetPendingProductsForOrder(int id, string term, int Limit);
        IQueryable<ComboBoxResult> GetPendingProductsForFilters(int id, string term);
        IQueryable<ComboBoxResult> GetPendingCostCentersForFilters(int id, string term);
        IEnumerable<ComboBoxList> GetPendingRequisitionsForFilters(int id, string term, int Limit);
    }

    public class RequisitionCancelLineService : IRequisitionCancelLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<RequisitionCancelLine> _RequisitionCancelLineRepository;
        RepositoryQuery<RequisitionCancelLine> RequisitionCancelLineRepository;
        public RequisitionCancelLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _RequisitionCancelLineRepository = new Repository<RequisitionCancelLine>(db);
            RequisitionCancelLineRepository = new RepositoryQuery<RequisitionCancelLine>(_RequisitionCancelLineRepository);
        }


        public RequisitionCancelLine Find(int id)
        {
            return _unitOfWork.Repository<RequisitionCancelLine>().Find(id);
        }

        public RequisitionCancelLine Create(RequisitionCancelLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RequisitionCancelLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RequisitionCancelLine>().Delete(id);
        }

        public void Delete(RequisitionCancelLine pt)
        {
            _unitOfWork.Repository<RequisitionCancelLine>().Delete(pt);
        }

        public void Update(RequisitionCancelLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RequisitionCancelLine>().Update(pt);
        }

        public IEnumerable<RequisitionCancelLineViewModel> GetRequisitionCancelLineForHeader(int id)
        {
            return (from p in db.RequisitionCancelLine
                    join t in db.RequisitionLine on p.RequisitionLineId equals t.RequisitionLineId into table
                    from tab in table.DefaultIfEmpty()
                    join t5 in db.RequisitionHeader on tab.RequisitionHeaderId equals t5.RequisitionHeaderId
                    join t1 in db.Dimension1 on tab.Dimension1Id equals t1.Dimension1Id into table1
                    from tab1 in table1.DefaultIfEmpty()
                    join t2 in db.Dimension2 on tab.Dimension2Id equals t2.Dimension2Id into table2
                    from tab2 in table2.DefaultIfEmpty()
                    join t3 in db.Product on tab.ProductId equals t3.ProductId into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t4 in db.RequisitionCancelHeader on p.RequisitionCancelHeaderId equals t4.RequisitionCancelHeaderId
                    join t6 in db.Persons on t4.PersonId equals t6.PersonID into table6
                    from tab6 in table6.DefaultIfEmpty()
                    orderby p.RequisitionCancelLineId
                    where p.RequisitionCancelHeaderId == id
                    select new RequisitionCancelLineViewModel
                    {
                        Dimension1Name = tab1.Dimension1Name,
                        Dimension2Name = tab2.Dimension2Name,
                        ProductId = tab.ProductId,
                        ProductName = tab3.ProductName,
                        RequisitionCancelHeaderDocNo = t4.DocNo,
                        RequisitionCancelHeaderId = p.RequisitionCancelHeaderId,
                        RequisitionCancelLineId = p.RequisitionCancelLineId,
                        RequisitionDocNo = t5.DocNo,
                        RequisitionLineId = tab.RequisitionLineId,
                        Qty = p.Qty,
                        Remark = p.Remark,
                        Specification = tab.Specification,
                        UnitId = tab3.UnitId,
                        unitDecimalPlaces = tab3.Unit.DecimalPlaces,
                    }
                        );

        }


        public RequisitionCancelLineViewModel GetRequisitionCancelLine(int id)
        {
            var temp = (from p in db.RequisitionCancelLine
                        join t in db.RequisitionLine on p.RequisitionLineId equals t.RequisitionLineId into table
                        from tab in table.DefaultIfEmpty()
                        join t5 in db.RequisitionHeader on tab.RequisitionHeaderId equals t5.RequisitionHeaderId into table5
                        from tab5 in table5.DefaultIfEmpty()
                        join t1 in db.Dimension1 on tab.Dimension1Id equals t1.Dimension1Id into table1
                        from tab1 in table1.DefaultIfEmpty()
                        join t2 in db.Dimension2 on tab.Dimension2Id equals t2.Dimension2Id into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join t3 in db.Product on tab.ProductId equals t3.ProductId into table3
                        from tab3 in table3.DefaultIfEmpty()
                        join t4 in db.RequisitionCancelHeader on p.RequisitionCancelHeaderId equals t4.RequisitionCancelHeaderId into table4
                        from tab4 in table4.DefaultIfEmpty()
                        join t6 in db.Persons on tab4.PersonId equals t6.PersonID into table6
                        from tab6 in table6.DefaultIfEmpty()
                        join t7 in db.ViewRequisitionBalance on p.RequisitionLineId equals t7.RequisitionLineId into table7
                        from tab7 in table7.DefaultIfEmpty()
                        orderby p.RequisitionCancelLineId
                        where p.RequisitionCancelLineId == id
                        select new RequisitionCancelLineViewModel
                        {
                            Dimension1Name = tab1.Dimension1Name,
                            Dimension2Name = tab2.Dimension2Name,
                            ProductId = tab.ProductId,
                            ProductName = tab3.ProductName,
                            RequisitionCancelHeaderDocNo = tab4.DocNo,
                            RequisitionCancelHeaderId = p.RequisitionCancelHeaderId,
                            RequisitionCancelLineId = p.RequisitionCancelLineId,
                            RequisitionDocNo = tab5.DocNo,
                            RequisitionLineId = tab.RequisitionLineId,
                            BalanceQty = p.Qty + tab7.BalanceQty,
                            Qty = p.Qty,
                            Remark = p.Remark,
                            Specification = tab.Specification,
                            UnitId = tab3.UnitId,
                            UnitName = tab3.Unit.UnitName,
                            LockReason = p.LockReason,
                        }

                      ).FirstOrDefault();

            return temp;

        }

        public RequisitionCancelLine Add(RequisitionCancelLine pt)
        {
            _unitOfWork.Repository<RequisitionCancelLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.RequisitionCancelLine
                        orderby p.RequisitionCancelLineId
                        select p.RequisitionCancelLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.RequisitionCancelLine
                        orderby p.RequisitionCancelLineId
                        select p.RequisitionCancelLineId).FirstOrDefault();
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

                temp = (from p in db.RequisitionCancelLine
                        orderby p.RequisitionCancelLineId
                        select p.RequisitionCancelLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.RequisitionCancelLine
                        orderby p.RequisitionCancelLineId
                        select p.RequisitionCancelLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public IEnumerable<RequisitionCancelLineViewModel> GetRequisitionLineForOrders(RequisitionCancelFilterViewModel svm)
        {


            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.RequisitionId)) { SaleOrderIdArr = svm.RequisitionId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] CostCenterIdArr = null;
            if (!string.IsNullOrEmpty(svm.CostCenterId)) { CostCenterIdArr = svm.CostCenterId.Split(",".ToCharArray()); }
            else { CostCenterIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            string[] Dimension1IdArr = null;
            if (!string.IsNullOrEmpty(svm.Dimension1Id)) { Dimension1IdArr = svm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dimension1IdArr = new string[] { "NA" }; }

            string[] Dimension2IdArr = null;
            if (!string.IsNullOrEmpty(svm.Dimension2Id)) { Dimension2IdArr = svm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dimension2IdArr = new string[] { "NA" }; }


            var Query = (from p in db.ViewRequisitionBalance
                         join product in db.Product on p.ProductId equals product.ProductId into table2
                         from tab2 in table2.DefaultIfEmpty()
                         join t in db.RequisitionLine on p.RequisitionLineId equals t.RequisitionLineId into table1
                         from tab1 in table1.DefaultIfEmpty()
                         join t2 in db.RequisitionHeader on tab1.RequisitionHeaderId equals t2.RequisitionHeaderId
                         where p.BalanceQty > 0 && p.PersonId == svm.PersonId
                         orderby t2.DocDate, t2.DocNo
                         select new
                         {
                             BalanceQty = p.BalanceQty,
                             Qty = 0,
                             RequisitionDocNo = p.RequisitionNo,
                             ProductName = tab2.ProductName,
                             ProductId = p.ProductId,
                             RequisitionCancelHeaderId = svm.RequisitionCancelHeaderId,
                             RequisitionLineId = p.RequisitionLineId,
                             unitDecimalPlaces = tab2.Unit.DecimalPlaces,
                             UnitId = tab2.UnitId,
                             UnitName = tab2.Unit.UnitName,
                             Dimension1Name = tab1.Dimension1.Dimension1Name,
                             Dimension2Name = tab1.Dimension2.Dimension2Name,
                             Specification = tab1.Specification,

                             //ForFilters
                             PersonId = p.PersonId,
                             RequisitionHeaderId = p.RequisitionHeaderId,
                             CostCenterId = p.CostCenterId,
                             ProductGorupId = tab2.ProductGroupId,
                             Dimension1Id = tab1.Dimension1Id,
                             Dimension2Id = tab1.Dimension2Id,
                         }
                        );

            if (!string.IsNullOrEmpty(svm.ProductId))
                Query = Query.Where(m => ProductIdArr.Contains(m.ProductId.ToString()));

            if (svm.PersonId != 0)
                Query = Query.Where(m => m.PersonId == svm.PersonId);

            if (!string.IsNullOrEmpty(svm.RequisitionId))
                Query = Query.Where(m => SaleOrderIdArr.Contains(m.RequisitionHeaderId.ToString()));

            if (!string.IsNullOrEmpty(svm.CostCenterId))
                Query = Query.Where(m => CostCenterIdArr.Contains(m.CostCenterId.ToString()));

            if (!string.IsNullOrEmpty(svm.ProductGroupId))
                Query = Query.Where(m => ProductGroupIdArr.Contains(m.ProductGorupId.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension1Id))
                Query = Query.Where(m => Dimension1IdArr.Contains(m.Dimension1Id.ToString()));

            if (!string.IsNullOrEmpty(svm.Dimension2Id))
                Query = Query.Where(m => Dimension2IdArr.Contains(m.Dimension2Id.ToString()));

            return Query.Select(m => new RequisitionCancelLineViewModel
            {
                BalanceQty = m.BalanceQty,
                Qty = 0,
                RequisitionDocNo = m.RequisitionDocNo,
                ProductName = m.ProductName,
                ProductId = m.ProductId,
                RequisitionCancelHeaderId = m.RequisitionCancelHeaderId,
                RequisitionLineId = m.RequisitionLineId,
                unitDecimalPlaces = m.unitDecimalPlaces,
                UnitId = m.UnitId,
                UnitName = m.UnitName,
                Dimension1Name = m.Dimension1Name,
                Dimension2Name = m.Dimension2Name,
                Specification = m.Specification,
            });
        }


        public RequisitionCancelLineViewModel GetLineDetail(int id)
        {
            return (from p in db.ViewRequisitionBalance
                    join t1 in db.RequisitionLine on p.RequisitionLineId equals t1.RequisitionLineId
                    join t2 in db.Product on p.ProductId equals t2.ProductId
                    join t3 in db.Dimension1 on t1.Dimension1Id equals t3.Dimension1Id into table3
                    from tab3 in table3.DefaultIfEmpty()
                    join t4 in db.Dimension2 on t1.Dimension2Id equals t4.Dimension2Id into table4
                    from tab4 in table4.DefaultIfEmpty()
                    where p.RequisitionLineId == id
                    select new RequisitionCancelLineViewModel
                    {
                        Dimension1Name = tab3.Dimension1Name,
                        Dimension2Name = tab4.Dimension2Name,
                        Qty = p.BalanceQty,
                        BalanceQty = p.BalanceQty,
                        Specification = t1.Specification,
                        UnitId = t2.UnitId,
                        UnitName = t2.Unit.UnitName,
                        ProductId = p.ProductId,
                        ProductName = t1.Product.ProductName,
                        unitDecimalPlaces = t2.Unit.DecimalPlaces,
                        RequisitionDocNo = p.RequisitionNo,
                    }
                        ).FirstOrDefault();

        }




        public IEnumerable<RequisitionCancelProductHelpList> GetPendingProductsForOrder(int id, string term, int Limit)
        {

            var RequisitionCancel = new RequisitionCancelHeaderService(_unitOfWork).Find(id);

            var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(RequisitionCancel.DocTypeId, RequisitionCancel.DivisionId, RequisitionCancel.SiteId);


            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewRequisitionBalance
                        join t in db.RequisitionHeader on p.RequisitionHeaderId equals t.RequisitionHeaderId
                        join t2 in db.RequisitionLine on p.RequisitionLineId equals t2.RequisitionLineId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : t2.Specification.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower())
                        || string.IsNullOrEmpty(term) ? 1 == 1 : p.RequisitionNo.ToLower().Contains(term.ToLower())
                        )
                        && (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.Product.ProductGroup.ProductTypeId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == RequisitionCancel.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == RequisitionCancel.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                        orderby t.DocDate, t.DocNo
                        select new RequisitionCancelProductHelpList
                        {
                            ProductName = p.Product.ProductName,
                            ProductId = p.ProductId,
                            Specification = t2.Specification,
                            Dimension1Name = p.Dimension1.Dimension1Name,
                            Dimension2Name = p.Dimension2.Dimension2Name,
                            RequisitionDocNo = p.RequisitionNo,
                            RequisitionLineId = p.RequisitionLineId,
                            BalanceQty = p.BalanceQty,
                        }
                          ).Take(Limit);

            return list.ToList();

        }

        public IQueryable<ComboBoxResult> GetPendingProductsForFilters(int id, string term)
        {

            var RequisitionCancel = new RequisitionCancelHeaderService(_unitOfWork).Find(id);

            var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(RequisitionCancel.DocTypeId, RequisitionCancel.DivisionId, RequisitionCancel.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewRequisitionBalance
                         join t in db.RequisitionHeader on p.RequisitionHeaderId equals t.RequisitionHeaderId
                         join prod in db.Product on p.ProductId equals prod.ProductId
                         join pg in db.ProductGroups on prod.ProductGroupId equals pg.ProductGroupId
                         into table
                         from pgtable in table.DefaultIfEmpty()
                         where p.BalanceQty > 0
                         //where (string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower()))
                         //&& (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.Product.ProductGroup.ProductTypeId.ToString()))
                         //&& (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == RequisitionCancel.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                         //&& (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == RequisitionCancel.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                         group new { p, pgtable, prod } by p.ProductId into g
                         orderby g.Max(m => m.prod.ProductName)
                         select new
                         {
                             ProductName = g.Max(m => m.prod.ProductName),
                             ProductId = g.Key,
                             ProductTypeId = g.Max(m => m.pgtable.ProductTypeId),
                             SiteId = g.Max(m => m.p.SiteId),
                             DivisionId = g.Max(m => m.p.DivisionId),
                         });

            if (!string.IsNullOrEmpty(term))
                Query = Query.Where(m => m.ProductName.ToLower().Contains(term.ToLower()));

            if (!string.IsNullOrEmpty(settings.filterProductTypes))
                Query = Query.Where(m => ProductTypes.Contains(m.ProductTypeId.ToString()));

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => ContraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == RequisitionCancel.SiteId);

            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => ContraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == RequisitionCancel.DivisionId);

            return Query.Select(m => new ComboBoxResult
            {
                id = m.ProductId.ToString(),
                text = m.ProductName,
            });

        }

        public IQueryable<ComboBoxResult> GetPendingCostCentersForFilters(int id, string term)
        {

            var RequisitionCancel = new RequisitionCancelHeaderService(_unitOfWork).Find(id);

            var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(RequisitionCancel.DocTypeId, RequisitionCancel.DivisionId, RequisitionCancel.SiteId);

            string[] ContraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { ContraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { ContraDocTypes = new string[] { "NA" }; }

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            var Query = (from p in db.ViewRequisitionBalance
                         join cs in db.CostCenter on p.CostCenterId equals cs.CostCenterId
                         join head in db.RequisitionHeader on p.RequisitionHeaderId equals head.RequisitionHeaderId
                         where p.BalanceQty > 0
                         group new { p,cs,head } by p.CostCenterId into g
                         orderby g.Max(m => m.cs.CostCenterName)
                         select new
                         {
                             CostCenterName = g.Max(m => m.cs.CostCenterName),
                             CostCenterId = g.Max(m => m.p.CostCenterId),
                             ReferenceDocType = g.Max(m => m.head.DocTypeId),
                             SiteId = g.Max(m => m.p.SiteId),
                             DivisionId = g.Max(m => m.p.DivisionId),
                             PersonId = g.Max(m => m.p.PersonId),
                         });

            if (!string.IsNullOrEmpty(term))
                Query = Query.Where(m => m.CostCenterName.ToLower().Contains(term.ToLower()));

            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
                Query = Query.Where(m => ContraDocTypes.Contains(m.ReferenceDocType.ToString()));

            if (RequisitionCancel.PersonId != 0)
                Query = Query.Where(m => m.PersonId == RequisitionCancel.PersonId);

            if (!string.IsNullOrEmpty(settings.filterContraSites))
                Query = Query.Where(m => ContraSites.Contains(m.SiteId.ToString()));
            else
                Query = Query.Where(m => m.SiteId == RequisitionCancel.SiteId);

            if (!string.IsNullOrEmpty(settings.filterContraDivisions))
                Query = Query.Where(m => ContraDivisions.Contains(m.DivisionId.ToString()));
            else
                Query = Query.Where(m => m.DivisionId == RequisitionCancel.DivisionId);

            return Query.Select(m => new ComboBoxResult
            {
                id = m.CostCenterId.ToString(),
                text = m.CostCenterName,
            });

        }

        public IEnumerable<ComboBoxList> GetPendingRequisitionsForFilters(int id, string term, int Limit)
        {

            var RequisitionCancel = new RequisitionCancelHeaderService(_unitOfWork).Find(id);

            var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(RequisitionCancel.DocTypeId, RequisitionCancel.DivisionId, RequisitionCancel.SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            string[] ContraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { ContraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { ContraDivisions = new string[] { "NA" }; }

            var list = (from p in db.ViewRequisitionBalance
                        join t in db.RequisitionHeader on p.RequisitionHeaderId equals t.RequisitionHeaderId
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.RequisitionNo.ToLower().Contains(term.ToLower()))
                        && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == RequisitionCancel.SiteId : ContraSites.Contains(p.SiteId.ToString()))
                        && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == RequisitionCancel.DivisionId : ContraDivisions.Contains(p.DivisionId.ToString()))
                        group p by p.RequisitionHeaderId into g
                        orderby g.Max(m => m.RequisitionNo)
                        select new ComboBoxList
                        {
                            PropFirst = g.Max(m => m.RequisitionNo),
                            Id = g.Key,
                        }
                          ).Take(Limit);

            return list.ToList();

        }




        public void Dispose()
        {
        }


        public Task<IEquatable<RequisitionCancelLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RequisitionCancelLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
