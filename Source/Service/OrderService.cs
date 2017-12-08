using System.Collections.Generic;
using System.Linq;
using Surya.India.Data;
using Surya.India.Data.Infrastructure;
using Surya.India.Model.Models;

using Surya.India.Core.Common;
using System;
using Surya.India.Model;
using System.Threading.Tasks;

namespace Surya.India.Service
{
    public interface IOrderService : IDisposable
    {
        SalesOrder Create(SalesOrder order);
        void Delete(int id);
        void Delete(SalesOrder order);
        SalesOrder GetOrder(int orderId);
        IEnumerable<SalesOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SalesOrder order);
        SalesOrder Add(SalesOrder order);
        IEnumerable<SalesOrder> GetOrderList();
        Task<IEquatable<SalesOrder>> GetAsync();
        Task<SalesOrder> FindAsync(int id);


        
    }

    public class OrderService : IOrderService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        
        public OrderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public SalesOrder GetOrder(int orderId)
        {
            return _unitOfWork.Repository<SalesOrder>().Find(orderId);
        }

        public SalesOrder Create(SalesOrder order)
        {
            order.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SalesOrder>().Insert(order);
            return order;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SalesOrder>().Delete(id);
        }

        public void Delete(SalesOrder order)
        {
            _unitOfWork.Repository<SalesOrder>().Delete(order);
        }

        public void Update(SalesOrder order)
        {
            order.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SalesOrder>().Update(order);
        }

        public IEnumerable<SalesOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var order = _unitOfWork.Repository<SalesOrder>()
                .Query()
                //.OrderBy(q => q
                //    .OrderBy(c => c.ContactName)
                //    .ThenBy(c => c.CompanyName))
                //.Filter(q => !string.IsNullOrEmpty(q.ContactName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return order;
        }

        public IEnumerable<SalesOrder> GetOrderList()
        {
            var order = _unitOfWork.Repository<SalesOrder>().Query().Get();              

            return order;
        }

        public SalesOrder Add(SalesOrder order)
        {
            _unitOfWork.Repository<SalesOrder>().Insert(order);
            return order;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SalesOrder>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SalesOrder> FindAsync(int id)
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
