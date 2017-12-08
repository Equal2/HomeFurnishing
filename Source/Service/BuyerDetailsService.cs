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
    public interface IBuyerDetailsService : IDisposable
    {
        BuyerDetail Create(BuyerDetail buyerDetail);
        void Delete(int id);
        void Delete(BuyerDetail buyerDetai);
        BuyerDetail GetBuyerDetail(int buyerDetailId);
        IEnumerable<BuyerDetail> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(BuyerDetail buyerDetail);
        BuyerDetail Add(BuyerDetail buyerDetail);
        IEnumerable<BuyerDetail> GetBuyerDetailList(int BuyerId);
        Task<IEquatable<BuyerDetail>> GetAsync();
        Task<BuyerDetail> FindAsync(int id);

        Buyer GetBuyer(int buyerDetailId);
    }

    public class BuyerDetailsService : IBuyerDetailsService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
       
        public BuyerDetailsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public BuyerDetail GetBuyerDetails(int buyerDetailId)
        {
            return _unitOfWork.Repository<BuyerDetail>().Find(buyerDetailId);
        }

        public BuyerDetail Create(BuyerDetail buyerDetail)
        {
            buyerDetail.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<BuyerDetail>().Insert(buyerDetail);
            return buyerDetail;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<BuyerDetail>().Delete(id);
        }

        public void Delete(BuyerDetail buyerDtl)
        {
            _unitOfWork.Repository<BuyerDetail>().Delete(buyerDtl);
        }

        public void Update(BuyerDetail buyerDetail)
        {
            buyerDetail.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<BuyerDetail>().Update(buyerDetail);
        }

        public IEnumerable<BuyerDetail> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var buyer = _unitOfWork.Repository<BuyerDetail>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.City))
                .Filter(q => !string.IsNullOrEmpty(q.Country))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return buyer;
        }

        public IEnumerable<BuyerDetail> GetBuyerDetailList()
        {
            var buyer = _unitOfWork.Repository<BuyerDetail>().Query().Get();              

            return buyer;
        }

        public BuyerDetail Add(BuyerDetail buyerDtl)
        {
            _unitOfWork.Repository<BuyerDetail>().Insert(buyerDtl);
            return buyerDtl;
        }

        public Buyer GetBuyer(int buyerDetailId)
        {
            var b = GetBuyerDetails(buyerDetailId);
           return b.Buyer;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<BuyerDetail>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BuyerDetail> FindAsync(int id)
        {
            throw new NotImplementedException();
        }




        public BuyerDetail GetBuyerDetail(int buyerDetailId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BuyerDetail> GetBuyerDetailList(int buyerId)
        {
            var buyerDetails = _unitOfWork.Repository<BuyerDetail>().Query().Get();
            return buyerDetails.Where(q=>q.Buyer.BuyerID==buyerId) ;
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
