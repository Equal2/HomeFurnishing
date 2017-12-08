using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModels;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModel;


namespace Service
{
    public interface IPurchaseIndentLineService : IDisposable
    {
        PurchaseIndentLine Create(PurchaseIndentLine s);
        void Delete(int id);
        void Delete(PurchaseIndentLine s);
        PurchaseIndentLineViewModel GetPurchaseIndentLine(int id);
        PurchaseIndentLine Find(int id);
        void Update(PurchaseIndentLine s);
        IQueryable<PurchaseIndentLineViewModel> GetPurchaseIndentLineListForIndex(int PurchaseIndentHeaderId);
        IEnumerable<PurchaseIndentLineViewModel> GetPurchaseIndentLineforDelete(int headerid);
        IEnumerable<PurchaseIndentLine> GetPurchaseIndentLineForMaterialPlan(int MaterialPlanLineId);
        IEnumerable<PurchaseIndentLineViewModel> GetPurchaseIndentForFilters(PurchaseIndentLineFilterViewModel vm);
        IEnumerable<MaterialPlanLineHelpListViewModel> GetPendingMaterialPlanHelpList(int Id, string term);//PurchaseOrderHeaderId
        IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term);
        int GetMaxSr(int id);
    }

    public class PurchaseIndentLineService : IPurchaseIndentLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public PurchaseIndentLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PurchaseIndentLine Create(PurchaseIndentLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseIndentLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseIndentLine>().Delete(id);
        }

        public void Delete(PurchaseIndentLine s)
        {
            _unitOfWork.Repository<PurchaseIndentLine>().Delete(s);
        }

        public void Update(PurchaseIndentLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseIndentLine>().Update(s);
        }

     
        public PurchaseIndentLineViewModel GetPurchaseIndentLine(int id)
        {
            return (from p in db.PurchaseIndentLine
                    join t in db.ViewMaterialPlanBalance on p.MaterialPlanLineId equals t.MaterialPlanLineId into table from tab in table.DefaultIfEmpty()
                    where p.PurchaseIndentLineId == id
                    select new PurchaseIndentLineViewModel
                    {
                        ProductId=p.ProductId,
                        DueDate = p.DueDate,                       
                        Qty = p.Qty,                       
                        Remark = p.Remark,
                        PurchaseIndentHeaderId = p.PurchaseIndentHeaderId,
                        PurchaseIndentLineId = p.PurchaseIndentLineId,                        
                        ProductName = p.Product.ProductName,
                       Dimension1Id=p.Dimension1Id,
                       Dimension1Name=p.Dimension1.Dimension1Name,
                       Dimension2Name=p.Dimension2.Dimension2Name,
                       Dimension2Id=p.Dimension2Id,
                       Specification=p.Specification,
                       UnitId=p.Product.UnitId,
                       MaterialPlanHeaderDocNo=p.MaterialPlanLine.MaterialPlanHeader.DocNo,
                       MaterialPlanLineId=p.MaterialPlanLineId,
                       PlanBalanceQty=tab==null?p.Qty:p.Qty+tab.BalanceQty,
                       LockReason=p.LockReason,
                    }

                        ).FirstOrDefault();
        }
        public PurchaseIndentLine Find(int id)
        {
            return _unitOfWork.Repository<PurchaseIndentLine>().Find(id);
        }
      

        public IQueryable<PurchaseIndentLineViewModel> GetPurchaseIndentLineListForIndex(int PurchaseIndentHeaderId)
        {
            var temp = from p in db.PurchaseIndentLine
                       join t in db.Dimension1 on p.Dimension1Id equals t.Dimension1Id into table from Dim1 in table.DefaultIfEmpty()
                       join t1 in db.Dimension2 on p.Dimension2Id equals t1.Dimension2Id into table1 from Dim2 in table1.DefaultIfEmpty()
                       join t3 in db.MaterialPlanLine on p.MaterialPlanLineId equals t3.MaterialPlanLineId into table3 from tab3 in table3.DefaultIfEmpty()
                       join t4 in db.MaterialPlanHeader on tab3.MaterialPlanHeaderId equals t4.MaterialPlanHeaderId into table4 from tab4 in table4.DefaultIfEmpty()
                       where p.PurchaseIndentHeaderId==PurchaseIndentHeaderId
                       orderby p.Sr
                       select new PurchaseIndentLineViewModel
                       {
                           Specification=p.Specification,
                           Dimension1Name=Dim2.Dimension2Name,
                           Dimension2Name=Dim1.Dimension1Name,
                           Remark=p.Remark,
                           DueDate = p.DueDate,
                           ProductId=p.ProductId,
                           ProductName = p.Product.ProductName,
                           Qty = p.Qty,
                           UnitId=p.Product.UnitId,
                           PurchaseIndentHeaderId = p.PurchaseIndentHeaderId,
                           PurchaseIndentLineId = p.PurchaseIndentLineId,
                           MaterialPlanLineId=p.MaterialPlanLineId,
                           MaterialPlanHeaderDocNo=tab4.DocNo,
                           PlanDocTypeId=tab4.DocTypeId,
                           PlanHeaderId=tab4.MaterialPlanHeaderId,
                       };
            return temp;
        }

        public IEnumerable<PurchaseIndentLineViewModel> GetPurchaseIndentLineforDelete(int headerid)
        {
            return (from p in db.PurchaseIndentLine
                    where p.PurchaseIndentHeaderId == headerid
                    select new PurchaseIndentLineViewModel
                    {
                        PurchaseIndentLineId = p.PurchaseIndentLineId
                    }

                        );


        }

        public IEnumerable<PurchaseIndentLine> GetPurchaseIndentLineForMaterialPlan(int MaterialPlanLineId)
        {
            return (from p in db.PurchaseIndentLine
                    where p.MaterialPlanLineId == MaterialPlanLineId
                    select p
                        );
        }


        public IEnumerable<PurchaseIndentLineViewModel> GetPurchaseIndentForFilters(PurchaseIndentLineFilterViewModel vm)
        {           

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.MaterialPlanHeaderId)) { SaleOrderIdArr = vm.MaterialPlanHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

                var temp = (from p in db.ViewMaterialPlanBalance
                            join t in db.MaterialPlanHeader on p.MaterialPlanHeaderId equals t.MaterialPlanHeaderId into table
                            from tab in table.DefaultIfEmpty()
                            join product in db.Product on p.ProductId equals product.ProductId into table2
                            join t1 in db.MaterialPlanLine on p.MaterialPlanLineId equals t1.MaterialPlanLineId into table1
                            from tab1 in table1.DefaultIfEmpty()
                            from tab2 in table2.DefaultIfEmpty()                            
                            where (string.IsNullOrEmpty(vm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                            && (string.IsNullOrEmpty(vm.MaterialPlanHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.MaterialPlanHeaderId.ToString()))
                            && (string.IsNullOrEmpty(vm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                            && p.BalanceQty > 0
                            orderby tab.DocDate,tab.DocNo,tab1.Sr
                            select new PurchaseIndentLineViewModel
                            {
                                Dimension1Name = tab1.Dimension1.Dimension1Name,
                                Dimension2Name = tab1.Dimension2.Dimension2Name,
                                Specification = tab1.Specification,
                                Dimension1Id=tab1.Dimension1Id,
                                Dimension2Id=tab1.Dimension2Id,
                                PlanBalanceQty = p.BalanceQty,
                                Qty = p.BalanceQty,
                                MaterialPlanHeaderDocNo = tab.DocNo,
                                ProductName = tab2.ProductName,
                                ProductId = p.ProductId,
                                PurchaseIndentHeaderId = vm.PurchaseIndentHeaderId,
                                MaterialPlanLineId = p.MaterialPlanLineId,
                                UnitId = tab2.UnitId,      
                                unitDecimalPlaces=tab2.Unit.DecimalPlaces,                                
                            }

                        );
                return temp;
        }


        public IEnumerable<MaterialPlanLineHelpListViewModel> GetPendingMaterialPlanHelpList(int Id, string term)
        {

            var PurchaseIndent = new PurchaseIndentHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(PurchaseIndent.DocTypeId, PurchaseIndent.DivisionId, PurchaseIndent.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            var list = (from p in db.ViewMaterialPlanBalance
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : p.MaterialPlanNo.ToLower().Contains(term.ToLower())) && p.BalanceQty > 0
                        && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                        group new { p } by p.MaterialPlanHeaderId into g
                        select new MaterialPlanLineHelpListViewModel
                        {
                            DocNo = g.Max(m => m.p.MaterialPlanNo),
                            MaterialPlanHeaderId = g.Key,
                            DocumentTypeName = g.Max(m => m.p.DocType.DocumentTypeName)
                            //    DocumentTypeName=g.Max(p=>p.p.DocumentTypeShortName)
                        }
                          ).Take(20);

            return list.ToList();

        }


        public IEnumerable<ComboBoxList> GetProductHelpList(int Id, string term)
        {
            var PurchaseIndent = new PurchaseIndentHeaderService(_unitOfWork).Find(Id);

            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(PurchaseIndent.DocTypeId, PurchaseIndent.DivisionId, PurchaseIndent.SiteId);

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
            var Max = (from p in db.PurchaseIndentLine
                       where p.PurchaseIndentHeaderId == id
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
    }
}
