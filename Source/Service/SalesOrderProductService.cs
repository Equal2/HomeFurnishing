using System.Collections.Generic;
using System.Linq;
using Surya.India.Data;
using Surya.India.Data.Infrastructure;
using Surya.India.Model.Models;

using Surya.India.Core.Common;
using System;
using Surya.India.Model;
using System.Threading.Tasks;
using Surya.India.Data.Models;

namespace Surya.India.Service
{
    public interface ISalesOrderProductService : IDisposable
    {
        SalesOrderProduct Create(SalesOrderProduct sop);
        void Delete(int id);
        void Delete(SalesOrderProduct sop);
        SalesOrderProduct GetSalesOrderProduct(int sopId);
        IEnumerable<SalesOrderProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SalesOrderProduct sop);
        SalesOrderProduct Add(SalesOrderProduct sop);
        IEnumerable<SalesOrderProduct> GetSalesOrderProductList();

        IEnumerable<SalesOrderProduct> GetSalesOrderProductList(int salesOrderId);
        Task<IEquatable<SalesOrderProduct>> GetAsync();
        Task<SalesOrderProduct> FindAsync(int id);


        
    }

    public class SalesOrderProductService : ISalesOrderProductService
    {
        ApplicationDbContext db =new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SalesOrderProduct> _SalesOrderProductRepository;
        RepositoryQuery<SalesOrderProduct> salesOrderProductRepository;
        public SalesOrderProductService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SalesOrderProductRepository = new Repository<SalesOrderProduct>(db);
            salesOrderProductRepository = new RepositoryQuery<SalesOrderProduct>(_SalesOrderProductRepository);
        }

        public SalesOrderProduct GetSalesOrderProduct(int sopId)
        {
            return salesOrderProductRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderProductId == sopId).FirstOrDefault();
           //return _unitOfWork.Repository<SalesOrder>().Find(soId);
        }

        public SalesOrderProduct Create(SalesOrderProduct sop)
        {
            sop.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SalesOrderProduct>().Insert(sop);
            return sop;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SalesOrderProduct>().Delete(id);
        }

        public void Delete(SalesOrderProduct sop)
        {
            _unitOfWork.Repository<SalesOrderProduct>().Delete(sop);
        }

        public void Update(SalesOrderProduct sop)
        {
            sop.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SalesOrderProduct>().Update(sop);
        }

        public IEnumerable<SalesOrderProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SalesOrderProduct>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.BuyerSKU))
                .Filter(q => !string.IsNullOrEmpty(q.BuyerUPC))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SalesOrderProduct> GetSalesOrderProductList()
        {
            var sop = _unitOfWork.Repository<SalesOrderProduct>().Query().Get();    
            return sop;
        }

        public IEnumerable<SalesOrderProduct> GetSalesOrderProductList(int soid)
        {
            var sopid = _unitOfWork.Repository<SalesOrderProduct>().Query().Get();
            return sopid.Where(b => b.SalesOrder.SalesOrderId == soid);
        }

        public SalesOrderProduct Add(SalesOrderProduct sop)
        {
            _unitOfWork.Repository<SalesOrderProduct>().Insert(sop);
            return sop;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SalesOrderProduct>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SalesOrderProduct> FindAsync(int id)
        {
            throw new NotImplementedException();
        }      
    }
    //public interface IOrderService
    //{
    //    IEnumerable<Order> GetOrders();
    //    Order GetLastOrder(int userid);
    //    Order GetOrder(int id);

    //    void CreateOrder(Order order);
    //    void EditOrder(Order order);
    //    void DeleteOrder(int id);
    //    void SaveOrder();
    //    //IEnumerable<ValidationResult> CanAddUpdate(Update newUpdate);

    //}

    //public class OrderService : IOrderService
    //{

    //    private readonly IOrderRepository orderRepository;

    //    private readonly IUnitOfWork unitOfWork;
    //    public OrderService(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    //    {
    //        this.orderRepository = orderRepository;
    //        this.unitOfWork = unitOfWork;
    //    }
      
    //    public IEnumerable<Order> GetOrders()
    //    {
    //        var order = orderRepository.GetAll();
    //        return order;
    //    }
    //    public Order GetLastOrder(int orderId)
    //    {
    //        var order = orderRepository.GetMany(g => g.OrderID == orderId).Last();
    //        return order;
    //    }


    //    public Order GetOrder(int id)
    //    {
    //        var order = orderRepository.GetById(id);
    //        return order;
    //    }

    //    //public IEnumerable<ValidationResult> CanAddUpdate(Update newUpdate)
    //    //{
    //    //    if (newUpdate.status.GetType().Name != "Double")
    //    //        yield return new ValidationResult("Status", "Not a valid update");

    //    //}


    //    public void CreateOrder(Order order)
    //    {
    //        orderRepository.Add(order);
    //        SaveOrder();
    //    }

    //    public void EditOrder(Order order)
    //    {
    //        orderRepository.Update(order);
    //        SaveOrder();
    //    }

    //    public void DeleteOrder(int id)
    //    {
    //        var update = orderRepository.GetById(id);
    //        orderRepository.Delete(update);
    //        SaveOrder();
    //    }
    //    public void SaveOrder()
    //    {
    //        unitOfWork.Commit();
    //    }
    //}


}
