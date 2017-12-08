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
    public interface IProductInvoiceGroupService : IDisposable
    {
        ProductInvoiceGroup Create(ProductInvoiceGroup pt);
        void Delete(int id);
        void Delete(ProductInvoiceGroup pt);
        IEnumerable<ProductInvoiceGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductInvoiceGroup pt);
        ProductInvoiceGroup Add(ProductInvoiceGroup pt);
        IEnumerable<ProductInvoiceGroup> GetProductInvoiceGroupList();

        // IEnumerable<ProductInvoiceGroup> GetProductInvoiceGroupList(int buyerId);
        Task<IEquatable<ProductInvoiceGroup>> GetAsync();
        Task<ProductInvoiceGroup> FindAsync(int id);
        ProductInvoiceGroup Find(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ProductInvoiceGroupService : IProductInvoiceGroupService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductInvoiceGroup> _ProductInvoiceGroupRepository;
        RepositoryQuery<ProductInvoiceGroup> ProductInvoiceGroupRepository;
        public ProductInvoiceGroupService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductInvoiceGroupRepository = new Repository<ProductInvoiceGroup>(db);
            ProductInvoiceGroupRepository = new RepositoryQuery<ProductInvoiceGroup>(_ProductInvoiceGroupRepository);
        }

        public ProductInvoiceGroup Find(int id)
        {
            return _unitOfWork.Repository<ProductInvoiceGroup>().Find(id);
        }

        public ProductInvoiceGroup Create(ProductInvoiceGroup pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductInvoiceGroup>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductInvoiceGroup>().Delete(id);
        }

        public void Delete(ProductInvoiceGroup pt)
        {
            _unitOfWork.Repository<ProductInvoiceGroup>().Delete(pt);
        }

        public void Update(ProductInvoiceGroup pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductInvoiceGroup>().Update(pt);
        }

        public IEnumerable<ProductInvoiceGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductInvoiceGroup>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductInvoiceGroupName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductInvoiceGroup> GetProductInvoiceGroupList()
        {
            var pt = (from p in db.ProductInvoiceGroup
                      orderby p.ProductInvoiceGroupName
                      select p
                          );
            return pt;
        }

        public ProductInvoiceGroup Add(ProductInvoiceGroup pt)
        {
            _unitOfWork.Repository<ProductInvoiceGroup>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductInvoiceGroup
                        orderby p.ProductInvoiceGroupName
                        select p.ProductInvoiceGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductInvoiceGroup
                        orderby p.ProductInvoiceGroupName
                        select p.ProductInvoiceGroupId).FirstOrDefault();
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

                temp = (from p in db.ProductInvoiceGroup
                        orderby p.ProductInvoiceGroupName
                        select p.ProductInvoiceGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductInvoiceGroup
                        orderby p.ProductInvoiceGroupName
                        select p.ProductInvoiceGroupId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductInvoiceGroup>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductInvoiceGroup> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
