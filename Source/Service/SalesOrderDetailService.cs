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
    public interface ISalesOrderDetailService : IDisposable
    {
        SalesOrderDetail Create(SalesOrderDetail sod);
        void Delete(int id);
        void Delete(SalesOrderDetail so);
        SalesOrderDetail GetSalesOrderDetail(int sodId);
        IEnumerable<SalesOrderDetail> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SalesOrderDetail so);
        SalesOrderDetail Add(SalesOrderDetail sod);
        IEnumerable<SalesOrderDetail> GetSalesOrderDetailList();

        IEnumerable<SalesOrderDetail> GetSalesOrderDetailList( int buyerId);
        void Detach(SalesOrderDetail salesOrderDetails);
        Task<IEquatable<SalesOrderDetail>> GetAsync();
        Task<SalesOrderDetail> FindAsync(int id);


        
    }

    public class SalesOrderDetailService : ISalesOrderDetailService
    {
        ApplicationDbContext db =new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SalesOrderDetail> _SalesOrderDetailRepository;
        RepositoryQuery<SalesOrderDetail> salesOrderDetailRepository;
        public SalesOrderDetailService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SalesOrderDetailRepository = new Repository<SalesOrderDetail>(db);
            salesOrderDetailRepository = new RepositoryQuery<SalesOrderDetail>(_SalesOrderDetailRepository);
        }

        public SalesOrderDetail GetSalesOrderDetail(int sodId)
        {
           // return salesOrderDetailRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == sodId).FirstOrDefault();
            return _unitOfWork.Repository<SalesOrderDetail>().Find(sodId);
        }

        public SalesOrderDetail Create(SalesOrderDetail sod)
        {
            sod.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SalesOrderDetail>().Insert(sod);
            return sod;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SalesOrderDetail>().Delete(id);
        }

        public void Delete(SalesOrderDetail sod)
        {
            _unitOfWork.Repository<SalesOrderDetail>().Delete(sod);
        }

        public void Update(SalesOrderDetail sod)
        {
            sod.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SalesOrderDetail>().Update(sod);
        }

        public IEnumerable<SalesOrderDetail> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SalesOrderDetail>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Saviourity))
                .Filter(q => !string.IsNullOrEmpty(q.Priority))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SalesOrderDetail> GetSalesOrderDetailList()
        {
            var sod = _unitOfWork.Repository<SalesOrderDetail>().Query().Get();           
            
            return sod;
        }

        public IEnumerable<SalesOrderDetail> GetSalesOrderDetailList(int soid)
        {
            var sodid = _unitOfWork.Repository<SalesOrderDetail>().Query().Get();
            return sodid.Where(b => b.SalesOrder.SalesOrderId == soid);
        }
       
        public SalesOrderDetail Add(SalesOrderDetail sod)
        {
            _unitOfWork.Repository<SalesOrderDetail>().Insert(sod);
            return sod;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SalesOrderDetail>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SalesOrderDetail> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public void Detach(SalesOrderDetail salesOrder)
        {
            _unitOfWork.Repository<SalesOrderDetail>().Detach(salesOrder);
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
