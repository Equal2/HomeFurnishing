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
    public interface IProductBuyerService : IDisposable
    {
        ProductBuyer Create(ProductBuyer pt);
        void Delete(int id);
        void Delete(ProductBuyer pt);
        ProductBuyer Find(string Name);

        ProductBuyer Find(int id);

        ProductBuyer Find(int BuyerId, int ProductId);
        ProductBuyer Find(int BuyerId, string BuyerSpecification, string BuyerSpecification1, string BuyerSpecification2, string BuyerSpecification3);
        IEnumerable<ProductBuyer> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductBuyer pt);
        ProductBuyer Add(ProductBuyer pt);
        IEnumerable<ProductBuyerViewModel> GetProductBuyerList(int id);//BuyerId
        Task<IEquatable<ProductBuyer>> GetAsync();
        Task<ProductBuyer> FindAsync(int id);
        int NextId(int id,int ProductId);
        int PrevId(int id,int ProductId);
    }

    public class ProductBuyerService : IProductBuyerService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductBuyer> _ProductBuyerRepository;
        RepositoryQuery<ProductBuyer> ProductBuyerRepository;
        public ProductBuyerService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductBuyerRepository = new Repository<ProductBuyer>(db);
            ProductBuyerRepository = new RepositoryQuery<ProductBuyer>(_ProductBuyerRepository);
        }

        public ProductBuyer Find(string Name)
        {
            return ProductBuyerRepository.Get().Where(i => i.Buyer.Person.Name == Name).FirstOrDefault();
        }


        public ProductBuyer Find(int id)
        {
            return _unitOfWork.Repository<ProductBuyer>().Find(id);
        }

        public ProductBuyer Find(int BuyerId, int ProductId)
        {
            return ProductBuyerRepository.Get().Where(i => i.BuyerId == BuyerId && i.ProductId == ProductId).FirstOrDefault();
        }

        public ProductBuyer Find(int BuyerId, string BuyerSpecification, string BuyerSpecification1, string BuyerSpecification2, string BuyerSpecification3)
        {
            return ProductBuyerRepository.Get().Where(i => i.BuyerId == BuyerId && i.BuyerSpecification == BuyerSpecification && i.BuyerSpecification1 == BuyerSpecification1 && i.BuyerSpecification2 == BuyerSpecification2 && i.BuyerSpecification3 == BuyerSpecification3).FirstOrDefault();
        }

        public ProductBuyer Create(ProductBuyer pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductBuyer>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductBuyer>().Delete(id);
        }

        public void Delete(ProductBuyer pt)
        {
            _unitOfWork.Repository<ProductBuyer>().Delete(pt);
        }

        public void Update(ProductBuyer pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductBuyer>().Update(pt);
        }

        public IEnumerable<ProductBuyer> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductBuyer>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductBuyerId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductBuyerViewModel> GetProductBuyerList(int id)
        {
            var pt = (from p in db.ProductBuyer
                      where p.ProductId == id
                      orderby p.ProductBuyerId
                      select new ProductBuyerViewModel
                      {
                          ProductBuyerId=p.ProductBuyerId,
                          ProductName=p.Product.ProductName,
                          ProductId=p.ProductId,
                          BuyerName = p.Buyer.Person.Name,
                          BuyerSku = p.BuyerSku,
                          BuyerSpecification = p.BuyerSpecification,
                          BuyerUpcCode = p.BuyerUpcCode,
                      }
                          );

            return pt;
        }

        public ProductBuyer Add(ProductBuyer pt)
        {
            _unitOfWork.Repository<ProductBuyer>().Insert(pt);
            return pt;
        }

        public int NextId(int id,int ProductId)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductBuyer
                        where p.ProductId==ProductId
                        orderby p.ProductBuyerId
                        select p.ProductBuyerId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductBuyer
                        where p.ProductId==ProductId
                        orderby p.ProductBuyerId
                        select p.ProductBuyerId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id,int ProductId)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.ProductBuyer
                        where p.ProductId==ProductId
                        orderby p.ProductBuyerId
                        select p.ProductBuyerId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductBuyer
                        where p.ProductId==ProductId
                        orderby p.ProductBuyerId
                        select p.ProductBuyerId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }



        public Task<IEquatable<ProductBuyer>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductBuyer> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
