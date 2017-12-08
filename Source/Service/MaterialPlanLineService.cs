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
    public interface IMaterialPlanLineService : IDisposable
    {
        MaterialPlanLine Create(MaterialPlanLine pt);
        void Delete(int id);
        void Delete(MaterialPlanLine pt);
        MaterialPlanLine Find(int id);
        IEnumerable<MaterialPlanLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanLine pt);
        MaterialPlanLine Add(MaterialPlanLine pt);
        IEnumerable<MaterialPlanLineViewModel> GetMaterialPlanLineList(int id);//Material Plan HeaderId        
        Task<IEquatable<MaterialPlanLine>> GetAsync();
        Task<MaterialPlanLine> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
        MaterialPlanSettings GetMaterialPlanSettingsForDocument(int DocTypeId, int DivisionId, int SiteId);
        IEnumerable<MaterialPlanLineViewModel> GetMaterialPlanForDelete(int HEaderId);//
        int GetMaxSr(int id);
        IEnumerable<MaterialPlanLineViewModel> GetSaleOrderDetail(int MaterialPlanLineId);
        IEnumerable<MaterialPlanLineViewModel> GetProdOrderDetail(int MaterialPlanLineId);

        IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term);
        IQueryable<ComboBoxResult> GetCustomProductGroups(int Id, string term);
    }

    public class MaterialPlanLineService : IMaterialPlanLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanLine> _MaterialPlanLineRepository;
        RepositoryQuery<MaterialPlanLine> MaterialPlanLineRepository;
        public MaterialPlanLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanLineRepository = new Repository<MaterialPlanLine>(db);
            MaterialPlanLineRepository = new RepositoryQuery<MaterialPlanLine>(_MaterialPlanLineRepository);
        }


        public MaterialPlanLine Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanLine>().Find(id);
        }

        public MaterialPlanLine Create(MaterialPlanLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanLine>().Delete(id);
        }

        public void Delete(MaterialPlanLine pt)
        {
            _unitOfWork.Repository<MaterialPlanLine>().Delete(pt);
        }
      
        public void Update(MaterialPlanLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanLine>().Update(pt);
        }

        public IEnumerable<MaterialPlanLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialPlanLineId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialPlanLineViewModel> GetMaterialPlanLineList(int id)//HEaderId
        {

            return (from p in db.MaterialPlanLine
                    join t in db.Product on p.ProductId equals t.ProductId
                    join u in db.Units on t.UnitId  equals u.UnitId
                    where p.MaterialPlanHeaderId == id
                    orderby p.Sr
                    select new MaterialPlanLineViewModel
                    {
                        RequiredQty = p.RequiredQty,
                        ProdPlanQty = p.ProdPlanQty,
                        ExcessStockQty = p.ExcessStockQty,
                        StockPlanQty=p.StockPlanQty,
                        PurchPlanQty=p.PurchPlanQty,
                        MaterialPlanHeaderDocNo = p.MaterialPlanHeader.DocNo,
                        MaterialPlanHeaderId = p.MaterialPlanHeaderId,
                        MaterialPlanLineId = p.MaterialPlanLineId,
                        ProductId = p.ProductId,
                        ProductName = t.ProductName,
                        Remark=p.Remark,
                        UnitId=u.UnitId,
                        UnitName=u.UnitName,
                        unitDecimalPlaces=u.DecimalPlaces,
                        Dimension1Id=p.Dimension1Id,
                        Dimension1Name=p.Dimension1.Dimension1Name,
                        Dimension2Id=p.Dimension2Id,
                        Dimension2Name=p.Dimension2.Dimension2Name,
                        Dimension3Id = p.Dimension3Id,
                        Dimension3Name = p.Dimension3.Dimension3Name,
                        Dimension4Id = p.Dimension4Id,
                        Dimension4Name = p.Dimension4.Dimension4Name,
                        ProcessId = p.ProcessId,
                        ProcessName = p.Process.ProcessName
                    }
                        );
            
        }

        public MaterialPlanSettings GetMaterialPlanSettingsForDocument(int DocTypeId, int DivisionId, int SiteId)
        {
            return (from p in db.MaterialPlanSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }

        public MaterialPlanLine Add(MaterialPlanLine pt)
        {
            _unitOfWork.Repository<MaterialPlanLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanLine
                        orderby p.MaterialPlanLineId
                        select p.MaterialPlanLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanLine
                        orderby p.MaterialPlanLineId
                        select p.MaterialPlanLineId).FirstOrDefault();
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

                temp = (from p in db.MaterialPlanLine
                        orderby p.MaterialPlanLineId
                        select p.MaterialPlanLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanLine
                        orderby p.MaterialPlanLineId
                        select p.MaterialPlanLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<MaterialPlanLineViewModel> GetMaterialPlanForDelete(int HEaderId)
        {
            return (from p in db.MaterialPlanLine
                    where p.MaterialPlanHeaderId == HEaderId
                    select new MaterialPlanLineViewModel
                    {
                        MaterialPlanHeaderId = p.MaterialPlanHeaderId,
                        MaterialPlanLineId = p.MaterialPlanLineId,
                    }
                        );
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.MaterialPlanLine
                       where p.MaterialPlanHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }

        public IEnumerable<MaterialPlanLineViewModel> GetSaleOrderDetail(int MaterialPlanLineId)
        {
            return (from p in db.MaterialPlanForSaleOrder
                    where p.MaterialPlanLineId == MaterialPlanLineId
                    select new MaterialPlanLineViewModel
                    {
                        ProductName=p.SaleOrderLine.Product.ProductName,
                        Dimension1Name=p.SaleOrderLine.Dimension1.Dimension1Name,
                        Dimension2Name=p.SaleOrderLine.Dimension2.Dimension2Name,
                        Specification=p.SaleOrderLine.Specification,
                        ProdPlanQty=p.Qty,
                    }).ToList();

        }

        public IEnumerable<MaterialPlanLineViewModel> GetProdOrderDetail(int MaterialPlanLineId)
        {
            return (from p in db.MaterialPlanForProdOrderLine
                    where p.MaterialPlanLineId == MaterialPlanLineId
                    join t in db.MaterialPlanForProdOrder on p.MaterialPlanForProdOrderId equals t.MaterialPlanForProdOrderId
                    select new MaterialPlanLineViewModel
                    {
                        ProductName = p.Product.ProductName,
                        Dimension1Name = p.Dimension1.Dimension1Name,
                        Dimension2Name = p.Dimension2.Dimension2Name,
                        Specification = t.ProdOrderLine.Specification,
                        ProdPlanQty=p.Qty,
                    }).ToList();

        }

        public IQueryable<ComboBoxResult> GetCustomProducts(int Id, string term)
        {

            var MaterialPlan = new MaterialPlanHeaderService(_unitOfWork).Find(Id);

            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(MaterialPlan.DocTypeId, MaterialPlan.DivisionId, MaterialPlan.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] Products = null;
            if (!string.IsNullOrEmpty(settings.filterProducts)) { Products = settings.filterProducts.Split(",".ToCharArray()); }
            else { Products = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }

            return (from p in db.Product
                    where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductGroup.ProductTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProducts) ? 1 == 1 : Products.Contains(p.ProductId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(p.ProductGroupId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductName.ToLower().Contains(term.ToLower()))
                    orderby p.ProductName
                    select new ComboBoxResult
                    {
                        id = p.ProductId.ToString(),
                        text = p.ProductName,
                    });
        }
        public IQueryable<ComboBoxResult> GetCustomProductGroups(int Id, string term)
        {

            var MaterialPlan = new MaterialPlanHeaderService(_unitOfWork).Find(Id);

            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(MaterialPlan.DocTypeId, MaterialPlan.DivisionId, MaterialPlan.SiteId);

            string[] ProductTypes = null;
            if (!string.IsNullOrEmpty(settings.filterProductTypes)) { ProductTypes = settings.filterProductTypes.Split(",".ToCharArray()); }
            else { ProductTypes = new string[] { "NA" }; }

            string[] ProductGroups = null;
            if (!string.IsNullOrEmpty(settings.filterProductGroups)) { ProductGroups = settings.filterProductGroups.Split(",".ToCharArray()); }
            else { ProductGroups = new string[] { "NA" }; }

            return (from p in db.ProductGroups
                    where (string.IsNullOrEmpty(settings.filterProductTypes) ? 1 == 1 : ProductTypes.Contains(p.ProductTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterProductGroups) ? 1 == 1 : ProductGroups.Contains(p.ProductGroupId.ToString()))
                    && (string.IsNullOrEmpty(term) ? 1 == 1 : p.ProductGroupName.ToLower().Contains(term.ToLower()))
                    orderby p.ProductGroupName
                    select new ComboBoxResult
                    {
                        id = p.ProductGroupId.ToString(),
                        text = p.ProductGroupName,
                    });
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<MaterialPlanLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
