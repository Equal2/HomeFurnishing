using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;

namespace Services.BasicSetup
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
        Task<IEquatable<ProductGroup>> GetAsync();
        Task<ProductGroup> FindAsync(int id);
        int NextIdForCarpet(int id);
        int PrevIdForCarpet(int id);
        int NextId(int id, int ptypeid);
        int PrevId(int id, int ptypeid);

    }

    public class ProductGroupService : IProductGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ProductGroup> _ProductGroupRepository;
        public ProductGroupService(IUnitOfWork unitOfWork, IRepository<ProductGroup> productGroupRepo)
        {
            _unitOfWork = unitOfWork;
            _ProductGroupRepository = productGroupRepo;
        }
        public ProductGroupService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductGroupRepository = unitOfWork.Repository<ProductGroup>();
        }

        public ProductGroup GetProductGroup(int pt)
        {
            //return _ProductGroupRepository.Include(r => r.ProductType).Get().Where(i => i.ProductGroupId == pt).FirstOrDefault();

            return _ProductGroupRepository.Query().Get().Where(i => i.ProductGroupId == pt).FirstOrDefault();
            // return _unitOfWork.Repository<ProductGroup>().Find(pt);
        }

        public ProductGroup Find(string Name)
        {
            return _ProductGroupRepository.Query().Get().Where(i => i.ProductGroupName == Name).FirstOrDefault();
        }

        public int NextIdForCarpet(int id)
        {
            int temp = 0;
            if (id != 0)
            {

                temp = (from p in _unitOfWork.Repository<ProductGroup>().Instance
                        join t in _unitOfWork.Repository<Product>().Instance on p.ProductGroupId equals t.ProductGroupId into tab
                        join t2 in _unitOfWork.Repository<ProductType>().Instance on p.ProductTypeId equals t2.ProductTypeId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        orderby p.ProductGroupName
                        where tab2.ProductTypeName == "Rug"
                        select p.ProductGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();


            }
            else
            {
                temp = (from p in _unitOfWork.Repository<ProductGroup>().Instance
                        join t in _unitOfWork.Repository<Product>().Instance on p.ProductGroupId equals t.ProductGroupId into tab
                        join t2 in _unitOfWork.Repository<ProductType>().Instance on p.ProductTypeId equals t2.ProductTypeId into table2
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
                temp = (from p in _unitOfWork.Repository<ProductGroup>().Instance
                        join t in _unitOfWork.Repository<Product>().Instance on p.ProductGroupId equals t.ProductGroupId into tab
                        join t2 in _unitOfWork.Repository<ProductType>().Instance on p.ProductTypeId equals t2.ProductTypeId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        orderby p.ProductGroupName
                        where tab2.ProductTypeName == "Rug"
                        select p.ProductGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _unitOfWork.Repository<ProductGroup>().Instance
                        join t in _unitOfWork.Repository<Product>().Instance on p.ProductGroupId equals t.ProductGroupId into tab
                        join t2 in _unitOfWork.Repository<ProductType>().Instance on p.ProductTypeId equals t2.ProductTypeId into table2
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
                temp = (from p in _unitOfWork.Repository<ProductGroup>().Instance
                        where p.ProductTypeId == ptypeid
                        orderby p.ProductGroupName
                        select p.ProductGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _unitOfWork.Repository<ProductGroup>().Instance
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

                temp = (from p in _unitOfWork.Repository<ProductGroup>().Instance
                        where p.ProductTypeId == ptypeid
                        orderby p.ProductGroupName
                        select p.ProductGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _unitOfWork.Repository<ProductGroup>().Instance
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
            return (from p in _unitOfWork.Repository<ProductGroup>().Instance
                    join t in _unitOfWork.Repository<ProductType>().Instance on p.ProductTypeId equals t.ProductTypeId into table
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
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<ProductGroup>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductGroup> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

    }

    public class ProductGroupQuality
    {
        public string ProductGroupName { get; set; }
        public string ProductQualityName { get; set; }
    }
}