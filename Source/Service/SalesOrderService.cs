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
    public interface ISalesOrderService : IDisposable
    {
        SalesOrder Create(SalesOrder so);
        void Delete(int id);
        void Delete(SalesOrder so);
        SalesOrder GetSalesOrder(int soId);
        IEnumerable<SalesOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SalesOrder so);
        SalesOrder Add(SalesOrder so);
        IEnumerable<SalesOrder> GetSalesOrderList();
        void Detach(SalesOrder salesOrder);
        IEnumerable<SalesOrder> GetSalesOrderList(int buyerId);
        Task<IEquatable<SalesOrder>> GetAsync();
        Task<SalesOrder> FindAsync(int id);



    }

    public class SalesOrderService : ISalesOrderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SalesOrder> _salesOrderRepository;
        RepositoryQuery<SalesOrder> salesOrderRepository;
        public SalesOrderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _salesOrderRepository = new Repository<SalesOrder>(db);
            salesOrderRepository = new RepositoryQuery<SalesOrder>(_salesOrderRepository);
        }

        public SalesOrder GetSalesOrder(int soId)
        {
            return salesOrderRepository.Include(r => r.Buyer).Get().Where(i => i.SalesOrderId == soId).FirstOrDefault();
           // return _unitOfWork.Repository<SalesOrder>().Find(soId);
        }

        public SalesOrder Create(SalesOrder so)
        {
            so.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SalesOrder>().Insert(so);
            return so;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SalesOrder>().Delete(id);
        }

        public void Delete(SalesOrder so)
        {
            _unitOfWork.Repository<SalesOrder>().Delete(so);
        }

        public void Update(SalesOrder so)
        {
            so.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SalesOrder>().Update(so);
        }

        public IEnumerable<SalesOrder> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SalesOrder>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.OrderDate))
                .Filter(q => !string.IsNullOrEmpty(q.OrderNumber))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SalesOrder> GetSalesOrderList()
        {
           // var so = _unitOfWork.Repository<SalesOrder>().Query().Get();
            var dd = salesOrderRepository.Include(b => b.Buyer).Include(sod=>sod.SalesOrderDetails).Get();
            return dd;
        }

        public IEnumerable<SalesOrder> GetSalesOrderList(int buyerId)
        {
            var so = _unitOfWork.Repository<SalesOrder>().Query().Get();
            return so;
        }

        public SalesOrder Add(SalesOrder so)
        {
            _unitOfWork.Repository<SalesOrder>().Insert(so);
            return so;
        }

        public void Dispose()
        {
        }

        public void Detach(SalesOrder salesOrder)
        {
            _unitOfWork.Repository<SalesOrder>().Detach(salesOrder);
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
}