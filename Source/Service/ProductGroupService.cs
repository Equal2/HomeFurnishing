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
    public interface IProductGroupService : IDisposable
    {
        ProductGroup Create(ProductGroup pt);
        void Delete(int id);
        void Delete(ProductGroup pt);
        ProductGroup GetProductGroup(int ptId);
        ProductGroup Find(string Name);
        ProductGroup Find(int id);
        IEnumerable<ProductGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductGroup pt);
        ProductGroup Add(ProductGroup pt);
        IQueryable<ProductGroup> GetProductGroupList(int ProductTypeId);
        IEnumerable<ProductGroup> GetProductGroupListForItemType(int Id);

        // IEnumerable<ProductGroup> GetProductGroupList(int buyerId);
        Task<IEquatable<ProductGroup>> GetAsync();
        Task<ProductGroup> FindAsync(int id);
        IQueryable<CarpetIndexViewModel> GetCarpetListForIndex(bool sample);
        int NextIdForCarpet(int id);
        int PrevIdForCarpet(int id);
        int NextId(int id, int ptypeid);
        int PrevId(int id, int ptypeid);

    }

    public class ProductGroupService : IProductGroupService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductGroup> _ProductGroupRepository;
        RepositoryQuery<ProductGroup> ProductGroupRepository;
        public ProductGroupService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductGroupRepository = new Repository<ProductGroup>(db);
            ProductGroupRepository = new RepositoryQuery<ProductGroup>(_ProductGroupRepository);
        }

        public ProductGroup GetProductGroup(int pt)
        {
            return ProductGroupRepository.Include(r => r.ProductType).Get().Where(i => i.ProductGroupId == pt).FirstOrDefault();
            // return _unitOfWork.Repository<ProductGroup>().Find(pt);
        }

        public ProductGroup Find(string Name)
        {
            return ProductGroupRepository.Get().Where(i => i.ProductGroupName == Name).FirstOrDefault();
        }

        public IQueryable<CarpetIndexViewModel> GetCarpetListForIndex(bool sample)
        {
            //return (from p in db.Products
            //        join t in db.ProductCategory on p.ProductCategoryId equals t.ProductCategoryId into table
            //        from tab in table.DefaultIfEmpty()
            //        join t1 in db.ProductTypes on tab.ProductTypeId equals t1.ProductTypeId into table1
            //        from tab1 in table1.DefaultIfEmpty()
            //        where tab1.IsCustomUI != null
            //        orderby p.ProductName, p.ProductGroup.ProductGroupName
            //        select new CarpetIndexViewModel
            //        {
            //            ProductId = p.ProductId,
            //            ProductName = p.ProductName,
            //            ProductGroupName = p.ProductGroup.ProductGroupName,
            //            ProductCategoryId = tab.ProductCategoryId,
            //            ProductCategoryName = tab.ProductCategoryName,
            //            ProductGroupId = p.ProductGroupId,
            //            ProductDesignName=p.ProductDesign.ProductDesignName,
            //        }
            //            );



            //return (from p in db.ProductGroups
            //        join t in db.FinishedProduct on p.ProductGroupId equals t.ProductGroupId into FinishedProductTable
            //        from FinishedProductTab in FinishedProductTable
            //        join t1 in db.ProductCategory on FinishedProductTable.FirstOrDefault().ProductCategoryId equals t1.ProductCategoryId into table1
            //        from tab1 in table1
            //        join t2 in db.ProductTypes on tab1.ProductTypeId equals t2.ProductTypeId into table2
            //        from tab2 in table2
            //        where tab2.ProductTypeName==ProductTypeConstants.Rug
            //        orderby p.ProductGroupName
            //        select new CarpetIndexViewModel
            //        {
            //            ProductId = FinishedProductTab.ProductId,
            //            ProductName = FinishedProductTab.ProductName,
            //            ProductGroupName = p.ProductGroupName,
            //            ProductCategoryId = tab1.ProductCategoryId,
            //            ProductCategoryName = tab1.ProductCategoryName,
            //            ProductGroupId = p.ProductGroupId,
            //            ProductDesignName = FinishedProductTab.ProductDesign.ProductDesignName,
            //        }
            //          );







            int typeid = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;



            //return (from p in db.ProductGroups
            //        join t in db.FinishedProduct on p.ProductGroupId equals t.ProductGroupId
            //        where p.ProductTypeId == typeid
            //        group t by new { t.ProductGroupId, t.ProductGroup.ProductGroupName } into table
            //        orderby table.Key.ProductGroupName
            //        select new CarpetIndexViewModel
            //        {
            //            ProductGroupId = table.Key.ProductGroupId ?? 0,
            //            ProductGroupName = table.Key.ProductGroupName,
            //            ProductCategoryName = table.FirstOrDefault().ProductCategory.ProductCategoryName,
            //            ProductDesignName = table.FirstOrDefault().ProductDesign.ProductDesignName,
            //            ProductCollectionName = table.FirstOrDefault().ProductCollection.ProductCollectionName
            //        }

            //     );

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var temp = from Pg in db.ProductGroups
                       join Fp in db.FinishedProduct on Pg.ProductGroupId equals Fp.ProductGroupId into FinishedProductTable
                       from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
                       join Pc in db.ProductCategory on FinishedProductTab.ProductCategoryId equals Pc.ProductCategoryId into ProductCategoryTable
                       from ProductCategoryTab in ProductCategoryTable.DefaultIfEmpty()
                       join Pd in db.ProductDesigns on FinishedProductTab.ProductDesignId equals Pd.ProductDesignId into ProductDesignTable
                       from ProductDesignTab in ProductDesignTable.DefaultIfEmpty()
                       join PCol in db.ProductCollections on FinishedProductTab.ProductCollectionId equals PCol.ProductCollectionId into ProductCollectionTable
                       from ProductCollectionTab in ProductCollectionTable.DefaultIfEmpty()
                       where Pg.ProductTypeId == typeid && (FinishedProductTab.IsSample == sample || FinishedProductTab == null) && (FinishedProductTab==null || FinishedProductTab.DivisionId==DivisionId)
                       group new { Pg, ProductCategoryTab, ProductDesignTab, ProductCollectionTab } by new { Pg.ProductGroupId, Pg.ProductGroupName } into table
                       orderby table.Key.ProductGroupName
                       select new CarpetIndexViewModel
                       {
                           ProductGroupId = table.Key.ProductGroupId,
                           ProductGroupName = table.Key.ProductGroupName,
                           ProductCategoryName = table.Max(m => m.ProductCategoryTab.ProductCategoryName),
                           ProductDesignName = table.Max(m => m.ProductDesignTab.ProductDesignName),
                           ProductCollectionName = table.Max(m => m.ProductCollectionTab.ProductCollectionName)
                       };

            return temp;


        }

        public int NextIdForCarpet(int id)
        {
            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.ProductGroups
                        join t in db.Product on p.ProductGroupId equals t.ProductGroupId into tab
                        join t2 in db.ProductTypes on p.ProductTypeId equals t2.ProductTypeId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        orderby p.ProductGroupName
                        where tab2.ProductTypeName == "Rug"
                        select p.ProductGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();


            }
            else
            {
                temp = (from p in db.ProductGroups
                        join t in db.Product on p.ProductGroupId equals t.ProductGroupId into tab
                        join t2 in db.ProductTypes on p.ProductTypeId equals t2.ProductTypeId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        orderby p.ProductGroupName
                        where tab2.ProductTypeName == "Rug"
                        select p.ProductGroupId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevIdForCarpet(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductGroups
                        join t in db.Product on p.ProductGroupId equals t.ProductGroupId into tab
                        join t2 in db.ProductTypes on p.ProductTypeId equals t2.ProductTypeId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        orderby p.ProductGroupName
                        where tab2.ProductTypeName == "Rug"
                        select p.ProductGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductGroups
                        join t in db.Product on p.ProductGroupId equals t.ProductGroupId into tab
                        join t2 in db.ProductTypes on p.ProductTypeId equals t2.ProductTypeId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        orderby p.ProductGroupName
                        where tab2.ProductTypeName == "Rug"
                        select p.ProductGroupId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int NextId(int id, int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductGroups
                        where p.ProductTypeId == ptypeid
                        orderby p.ProductGroupName
                        select p.ProductGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductGroups
                        where p.ProductTypeId == ptypeid
                        orderby p.ProductGroupName
                        select p.ProductGroupId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id, int ptypeid)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.ProductGroups
                        where p.ProductTypeId == ptypeid
                        orderby p.ProductGroupName
                        select p.ProductGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductGroups
                        where p.ProductTypeId == ptypeid
                        orderby p.ProductGroupName
                        select p.ProductGroupId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public ProductGroup Find(int id)
        {
            return _unitOfWork.Repository<ProductGroup>().Find(id);
        }

        public string GetProductNatureName(int id)
        {
            return (from p in db.ProductGroups
                    join t in db.ProductTypes on p.ProductTypeId equals t.ProductTypeId into table
                    from tab in table.DefaultIfEmpty()
                    where p.ProductGroupId == id
                    select tab.ProductNature.ProductNatureName
                        ).FirstOrDefault();
        }

        public ProductGroup Create(ProductGroup pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductGroup>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductGroup>().Delete(id);
        }

        public void Delete(ProductGroup pt)
        {
            pt.ObjectState = ObjectState.Deleted;
            _unitOfWork.Repository<ProductGroup>().Delete(pt);
        }

        public void Update(ProductGroup pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductGroup>().Update(pt);
        }

        public IEnumerable<ProductGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductGroup>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductGroupName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<ProductGroup> GetProductGroupList(int ProductTypeId)
        {
            var pt = _unitOfWork.Repository<ProductGroup>().Query().Get().OrderBy(m => m.ProductGroupName).Where(m => m.ProductTypeId == ProductTypeId);

            return pt;
        }

        public IEnumerable<ProductGroup> GetProductGroupListForItemType(int Id)
        {
            var pt = _unitOfWork.Repository<ProductGroup>().Query().Get().Where(i => i.ProductTypeId == Id);

            return pt;
        }


        public ProductGroup Add(ProductGroup pt)
        {
            _unitOfWork.Repository<ProductGroup>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductGroup>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductGroup> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public ProductGroupQuality GetProductGroupQuality(int ProductGroupId)
        {
            ProductGroupQuality p = (from fp in db.FinishedProduct
                                     join pg in db.ProductGroups on fp.ProductGroupId equals pg.ProductGroupId into ProductGroupTable
                                     from ProductGroupTab in ProductGroupTable.DefaultIfEmpty()
                                     join pq in db.ProductQuality on fp.ProductQualityId equals pq.ProductQualityId into ProductQualityTable
                                     from ProductQualityTab in ProductQualityTable.DefaultIfEmpty()
                                     where fp.ProductGroupId == ProductGroupId
                                     select new ProductGroupQuality
                                     {
                                         ProductGroupName = ProductGroupTab.ProductGroupName,
                                         ProductQualityName = ProductQualityTab.ProductQualityName,
                                         GrossWeight = fp.GrossWeight
                                     }).FirstOrDefault();

            return p;

        }


    }

    public class ProductGroupQuality
    {
        public string ProductGroupName { get; set; }
        public string ProductQualityName { get; set; }

        public Decimal? GrossWeight { get; set; }
    }
}
