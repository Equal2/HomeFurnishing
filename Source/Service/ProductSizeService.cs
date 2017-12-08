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

namespace Service
{
    public interface IProductSizeService : IDisposable
    {
        ProductSize Create(ProductSize p);
        void Delete(int id);
        void Delete(ProductSize p);
        ProductSize GetProductSize(int p);
        void Update(ProductSize p);
        ProductSize Add(ProductSize p);
        ProductSize Find(int id);
        IQueryable<ProductSize> GetProductSizeList();
        CarpetMasterViewModel GetProductSizeIndexForProduct(int id);

        IQueryable<ProductSizeViewModel> GetProductSizeForProduct(int id);
        IQueryable<ProductSizeIndexViewModel> GetProductSizeListForProductGroup(int id);
        ProductSizeIndexViewModel GetCommanSizes(int id, int productcategoryid);
        ProductSize FindProductSize(int sizetypeid, int productid);

    }



    public class ProductSizeService : IProductSizeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public ProductSizeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProductSize FindProductSize(int sizetypeid, int productid)
        {
            return (from p in db.ProductSize
                    where p.ProductId == productid && p.ProductSizeTypeId == sizetypeid
                    select p
                        ).FirstOrDefault();
        }
        public ProductSize Create(ProductSize p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductSize>().Insert(p);
            return p;
        }
        public ProductSize GetProductSize(int p)
        {
            return (_unitOfWork.Repository<ProductSize>().Query().Get().Where(m => m.ProductSizeId == p).FirstOrDefault());
        }
        public ProductSizeIndexViewModel GetCommanSizes(int id,int productcategoryId)
        {            
            //var temp1 = (from p in db.ViewRugSize
            //             join t in db.FinishedProduct on p.ProductId equals t.ProductId
            //             where p.StandardSizeID==id && t.ProductCategoryId==productcategoryId
            //             group p by new { p.ManufaturingSizeID,p.StandardSizeID,p.FinishingSizeID,p.StencilSizeId,p.MapSizeId } into ta
            //             orderby ta.Count() descending
            //             select new ProductSizeIndexViewModel
            //             {
            //                 ProductStandardSizeId = ta.Key.StandardSizeID ?? 0,
            //                 ProductManufacturingSizeId = ta.Key.ManufaturingSizeID ?? 0,
            //                 FinishingSizeId = ta.Key.FinishingSizeID ?? 0,
            //                 ProductStencilId = ta.Key.StencilSizeId?? 0,
            //                 ProductMapSizeId= ta.Key.MapSizeId?? 0,
            //                 ProductStandardSizeName = ta.FirstOrDefault().StandardSizeName,
            //                 ProductManufacturingSizeName=ta.FirstOrDefault().ManufaturingSizeName,
            //                 FinishingSizeName=ta.FirstOrDefault().FinishingSizeName,
            //                 ProductStencilSizeName= ta.FirstOrDefault().StencilSizeName,
            //                 ProductMapSizeName= ta.FirstOrDefault().MapSizeName,
            //             }).Take(1).FirstOrDefault();



            var temp1 = (from p in db.ViewRugSize
                         join t in db.FinishedProduct on p.ProductId equals t.ProductId
                         join SS in db.Size on p.StandardSizeID equals SS.SizeId into StandardSizeTable
                         from StandardSizeTab in StandardSizeTable.DefaultIfEmpty()
                         join MS in db.Size on p.ManufaturingSizeID equals MS.SizeId into ManufacturingSizeTable
                         from ManufacturingSizeTab in ManufacturingSizeTable.DefaultIfEmpty()
                         join FS in db.Size on p.FinishingSizeID equals FS.SizeId into FinishingSizeTable
                         from FinishingSizeTab in FinishingSizeTable.DefaultIfEmpty()
                         join Sts in db.Size on p.StencilSizeId equals Sts.SizeId into StencilSizeTable
                         from StencilSizeTab in StencilSizeTable.DefaultIfEmpty()
                         join Mps in db.Size on p.MapSizeId equals Mps.SizeId into MapSizeTable
                         from MapSizeTab in MapSizeTable.DefaultIfEmpty()
                         where p.StandardSizeID == id && t.ProductCategoryId == productcategoryId
                         group new { p, StandardSizeTab, ManufacturingSizeTab, FinishingSizeTab, StencilSizeTab, MapSizeTab } by new { p.ManufaturingSizeID, p.StandardSizeID, p.FinishingSizeID, p.StencilSizeId, p.MapSizeId } into ta
                         orderby ta.Count() descending
                         select new ProductSizeIndexViewModel
                         {
                             ProductStandardSizeId = ta.Key.StandardSizeID ?? 0,
                             ProductManufacturingSizeId = ta.Key.ManufaturingSizeID ?? 0,
                             FinishingSizeId = ta.Key.FinishingSizeID ?? 0,
                             ProductStencilId = ta.Key.StencilSizeId ?? 0,
                             ProductMapSizeId = ta.Key.MapSizeId ?? 0,
                             ProductStandardSizeName = ta.FirstOrDefault().StandardSizeTab.SizeName + ta.FirstOrDefault().StandardSizeTab.ProductShape.ProductShapeShortName,
                             ProductManufacturingSizeName = ta.FirstOrDefault().ManufacturingSizeTab.SizeName + ta.FirstOrDefault().ManufacturingSizeTab.ProductShape.ProductShapeShortName,
                             FinishingSizeName = ta.FirstOrDefault().FinishingSizeTab.SizeName + ta.FirstOrDefault().FinishingSizeTab.ProductShape.ProductShapeShortName,
                             ProductStencilSizeName = ta.FirstOrDefault().StencilSizeTab.SizeName + ta.FirstOrDefault().StencilSizeTab.ProductShape.ProductShapeShortName,
                             ProductMapSizeName = ta.FirstOrDefault().MapSizeTab.SizeName + ta.FirstOrDefault().MapSizeTab.ProductShape.ProductShapeShortName,
                         }).Take(1).FirstOrDefault();



            //if (temp1.FinishingSizeName == null)
            //    temp1.FinishingSizeName = "";
            //if (temp1.ProductManufacturingSizeName == null)
            //    temp1.ProductManufacturingSizeName = "";
            //if (temp1.FinishingSizeName == null)
            //    temp1.FinishingSizeName = "";

            return temp1;

        }
        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductSize>().Delete(id);
        }
        public IQueryable<ProductSizeIndexViewModel> GetProductSizeListForProductGroup(int id)
        {
            //return ( from p in db.ViewRugSize
            //         join t in db.Products on p.ProductId equals t.ProductId into table from tab in table.DefaultIfEmpty()
            //         where tab.ProductCategoryId==id
            //         select new ProductSizeIndexViewModel
            //         {
            //             ProductId=p.ProductId,
            //             ProductName=tab.ProductName,
            //             ProductStandardSizeId=p.StandardSizeID??0,
            //             ProductStandardSizeName=p.StandardSizeName,
            //             ProductManufacturingSizeId = p.ManufaturingSizeID??0,
            //             ProductManufacturingSizeName=p.ManufaturingSizeName,
            //             FinishingSizeId = p.FinishingSizeID??0 ,
            //             FinishingSizeName=p.FinishingSizeName
            //         }



            //             );


          

            return (from p in db.ViewRugSize
                    join t in db.Product on p.ProductId equals t.ProductId into tabl
                    from tab in tabl.DefaultIfEmpty()
                    join temp in db.Size on p.StandardSizeID equals temp.SizeId into temptable from temptab in temptable
                    join t2 in db.ProductShape on temptab.ProductShapeId equals t2.ProductShapeId into table2 from tab2 in table2
                    where tab.ProductGroupId == id
                    select new ProductSizeIndexViewModel
                    {
                        ProductId = p.ProductId,
                        ProductName = tab.ProductName,
                        ProductStandardSizeId = p.StandardSizeID ?? 0,
                        ProductStandardSizeName = p.StandardSizeName,
                        ProductManufacturingSizeId = p.ManufaturingSizeID ?? 0,
                        ProductManufacturingSizeName = p.ManufaturingSizeName,
                        FinishingSizeId = p.FinishingSizeID ?? 0,
                        FinishingSizeName = p.FinishingSizeName,
                        ShapeName = tab2.ProductShapeName 

                    }

);
        }

        public void Delete(ProductSize p)
        {
            _unitOfWork.Repository<ProductSize>().Delete(p);
        }

        public void Update(ProductSize p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductSize>().Update(p);
        }


        public IQueryable<ProductSize> GetProductSizeList()
        {
            var p = _unitOfWork.Repository<ProductSize>().Query().Get().OrderBy(M => M.ProductSizeId);

            return p;

        }

        public CarpetMasterViewModel GetProductSizeIndexForProduct(int id)
        {
            return (from p in db.ViewRugSize
                    where p.ProductId == id
                    select new CarpetMasterViewModel
                    {
                        StandardSizeId = p.StandardSizeID ?? 0,
                        ManufacturingSizeId = p.ManufaturingSizeID ?? 0,
                        FinishingSizeId = p.FinishingSizeID ?? 0,
                        StencilSizeId=p.StencilSizeId ??0,
                        MapSizeId=p.MapSizeId ?? 0,
                        ProductId = p.ProductId
                    }
                        ).FirstOrDefault();
        }

        public IQueryable<ProductSizeViewModel> GetProductSizeForProduct(int id)
        {
            return (from p in db.ProductSize
                    where p.ProductId == id
                    select new ProductSizeViewModel
                    {
                        ProductId = p.ProductId,
                        ProductName = p.Product.ProductName,
                        ProductSizeId = p.ProductSizeId,
                        ProductSizeTypeId = p.ProductSizeTypeId,
                        SizeId = p.SizeId,
                    }
                         );
        }

        public ProductSize Find(int id)
        {
            return _unitOfWork.Repository<ProductSize>().Find(id);
        }

        public ProductSize Add(ProductSize p)
        {
            _unitOfWork.Repository<ProductSize>().Add(p);
            return p;
        }

        public void Dispose()
        {
        }
    }

}
