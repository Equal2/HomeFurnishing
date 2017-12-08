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

namespace Service
{
    public interface IProductTypeAttributeService : IDisposable
    {
        ProductTypeAttribute Create(ProductTypeAttribute p);
        void Delete(int id);
        void Delete(ProductTypeAttribute p);
        ProductTypeAttribute GetProductTypeAttribute(int p);
        void Update(ProductTypeAttribute p);
        IEnumerable<ProductTypeAttributeViewModel> GetAttributeForProduct(int id);

        IEnumerable<ProductTypeAttribute> GetProductTypeAttributesList();

        IEnumerable<ProductTypeAttribute> GetProductTypeAttributesList(int prodyctTypeId);
        Task<IEquatable<ProductTypeAttribute>> GetAsync();
        Task<ProductTypeAttribute> FindAsync(int id);
        ProductTypeAttribute Find(int id);
        IEnumerable<ProductTypeAttribute> GetAttributesForProductType(int ProductTypeId);
        int NextId(int id, int ptypeid);
        int PrevId(int id, int ptypeid);
        IEnumerable<ProductTypeAttributeViewModel> GetAttributeVMForProductType(int ProductTypeId);
    }

    public class ProductTypeAttributeService : IProductTypeAttributeService
    {
        ApplicationDbContext db =new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductTypeAttribute> _productTypeAttributeRepository;
        RepositoryQuery<ProductTypeAttribute> productTypeAttributeRepositoryQry;
        public ProductTypeAttributeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productTypeAttributeRepository = new Repository<ProductTypeAttribute>(db);
            productTypeAttributeRepositoryQry = new RepositoryQuery<ProductTypeAttribute>(_productTypeAttributeRepository);
        }

        public ProductTypeAttribute GetProductTypeAttribute(int pId)
        {
           // return productTypeAttributeRepositoryQry.Include(r=>r.ProductType).Get().Where(i => i.ProductTypeAttributeId == pId).FirstOrDefault();
           //return _unitOfWork.Repository<SalesOrder>().Find(soId);

            return (from p in db.ProductTypeAttribute
                    where p.ProductTypeAttributeId == pId
                    select p
                        ).FirstOrDefault();


        }

        public IEnumerable<ProductTypeAttribute> GetAttributesForProductType(int ProductTypeId)
        {
            return _unitOfWork.Repository<ProductTypeAttribute>().Query().Get().Where(m => m.ProductType_ProductTypeId == ProductTypeId);
        }
        public ProductTypeAttribute Create(ProductTypeAttribute p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductTypeAttribute>().Insert(p);
            return p;
        }
        public IEnumerable<ProductTypeAttributeViewModel> GetAttributeForProduct(int id)
        {
            //int Typeid = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;
            int Typeid = (from P in db.Product
                          join Pg in db.ProductGroups on P.ProductGroupId equals Pg.ProductGroupId into ProductGroupTable
                          from ProductGroupTab in ProductGroupTable.DefaultIfEmpty()
                          where P.ProductId == id
                          select new
                          {
                              ProductTypeId = ProductGroupTab.ProductTypeId
                          }).FirstOrDefault().ProductTypeId;

            //var frn= from p in db.ProductTypeAttribute
            //      join t in db.ProductAttributes on p.ProductTypeAttributeId equals t.ProductTypeAttributeId into table
            //      from tab in table.DefaultIfEmpty()
            //      where (tab.ProductId == id || ((int?)tab.ProductId ?? 0) == 0) && (p.ProductType_ProductTypeId == Typeid)
            //      select new ProductTypeAttributeViewModel
            //      {
            //          ListItem=p.ListItem,
            //          DataType=p.DataType,
            //          DefaultValue = tab.ProductAttributeValue,
            //          Name = p.Name,
            //          ProductTypeAttributeId = p.ProductTypeAttributeId,
            //          ProductAttributeId = (int?)tab.ProductAttributeId ?? 0
            //      };


            var temp = from p in db.ProductTypeAttribute
                       join t in db.ProductAttributes on p.ProductTypeAttributeId equals  t.ProductTypeAttributeId  into table
                       from tab in table.Where(m=>m.ProductId==id).DefaultIfEmpty()
                       where (p.ProductType_ProductTypeId == Typeid)
                       select new ProductTypeAttributeViewModel
                       {
                           ListItem = p.ListItem,
                           DataType = p.DataType,
                           DefaultValue = tab.ProductAttributeValue,
                           Name = p.Name,
                           ProductTypeAttributeId = p.ProductTypeAttributeId,
                           ProductAttributeId = (int?)tab.ProductAttributeId ?? 0
                       };

            return temp;
        }
        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductTypeAttribute>().Delete(id);
        }

        public void Delete(ProductTypeAttribute p)
        {
            _unitOfWork.Repository<ProductTypeAttribute>().Delete(p);
        }

        public void Update(ProductTypeAttribute p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductTypeAttribute>().Update(p);
        }

        public IEnumerable<Product> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Product>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductCode))
                .Filter(q => !string.IsNullOrEmpty(q.ProductName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public IEnumerable<ProductTypeAttribute> GetProductTypeAttributesList()
        {
            var p = _unitOfWork.Repository<ProductTypeAttribute>().Query().Get();           
            
            return p;
        }



        public IEnumerable<ProductTypeAttribute> GetProductTypeAttributesList(int productTypeId)
        {
            var p = _unitOfWork.Repository<ProductTypeAttribute>().Query().Get();
            return p.Where(b => b.ProductType_ProductTypeId == productTypeId);
        }

        public IEnumerable<ProductTypeAttributeViewModel> GetAttributeVMForProductType(int ProductTypeId)
        {
            return (from p in db.ProductTypeAttribute
                    where p.ProductType_ProductTypeId == ProductTypeId
                    select new ProductTypeAttributeViewModel
                    {
                        DataType=p.DataType,
                        ListItem=p.ListItem,
                        DefaultValue=p.DefaultValue,
                        Name=p.Name,
                        ProductTypeAttributeId=p.ProductTypeAttributeId,
                        ProductType_ProductTypeId=p.ProductType_ProductTypeId,
                    }

                       );

        }


        public ProductTypeAttribute Find(int id)
        {
            return _unitOfWork.Repository<ProductTypeAttribute>().Find(id);
        }


        public int NextId(int id, int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductTypeAttribute
                        where p.ProductType_ProductTypeId == ptypeid
                        orderby p.Name
                        select p.ProductTypeAttributeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductTypeAttribute
                        where p.ProductType_ProductTypeId == ptypeid
                        orderby p.Name
                        select p.ProductTypeAttributeId).FirstOrDefault();
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

                temp = (from p in db.ProductTypeAttribute
                        where p.ProductType_ProductTypeId== ptypeid
                        orderby p.Name
                        select p.ProductTypeAttributeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductTypeAttribute
                        where p.ProductType_ProductTypeId == ptypeid
                        orderby p.Name
                        select p.ProductTypeAttributeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }



        public void Dispose()
        {
        }


        public Task<IEquatable<ProductTypeAttribute>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductTypeAttribute> FindAsync(int id)
        {
            throw new NotImplementedException();
        }


    }


}
