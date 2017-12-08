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
    public interface IProductSampleShipmentService : IDisposable
    {
        ProductSampleShipment Create(ProductSampleShipment pt);
        ProductSampleShipment GetProductSampleShipment(int ptId);

        IEnumerable<ProductSampleShipment> GetProductSampleShipmentList();
        ProductSampleShipment CheckForShipementExist(int productsampleApprovalId);

    }

    public class ProductSampleShipmentService : IProductSampleShipmentService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductSampleShipment> _productSampleShipmentRepository;
        RepositoryQuery<ProductSampleShipment> productSampleShipment;
        public ProductSampleShipmentService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productSampleShipmentRepository = new Repository<ProductSampleShipment>(db);
            productSampleShipment = new RepositoryQuery<ProductSampleShipment>(_productSampleShipmentRepository);
        }

        public ProductSampleShipment GetProductSampleShipment(int pt) 
        {
            return _unitOfWork.Repository<ProductSampleShipment>().Query()
                    .Include(m => m.ProductSamplePhotoApproval)
                    .Include(m => m.ProductSamplePhotoApproval.ProductSample)
                    .Include(m => m.ProductSamplePhotoApproval.ProductSample.ProductSamplePhoto)
                     .Include(m => m.ProductSamplePhotoApproval.ProductSample.Supplier)
                    .Get().Where(m => m.ProductSampleShipmentId ==pt).FirstOrDefault();
        }

        public IQueryable < ProductSampleShipment> GetPhysicalSamplePendingToReview()
        {
            return _unitOfWork.Repository<ProductSampleShipment>().Query()
                    .Include(m => m.ProductSamplePhotoApproval)
                    .Include(m => m.ProductSamplePhotoApproval.ProductSample)
                    .Include(m => m.ProductSampleApprovals)
                    .Include(m=>m.ProductSamplePhotoApproval.ProductSample.Supplier)
                    .Get().Where(m => m.ProductSampleApprovals.Max(a => a.ProductSampleApprovalId) == null);
        }


        public ProductSampleShipment Create(ProductSampleShipment pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductSampleShipment>().Insert(pt);
            return pt;
        }

        public IEnumerable<ProductSampleShipment> GetProductSampleShipmentList()
        {
            var p = _unitOfWork.Repository<ProductSampleShipment>().Query()
                    .Include(m => m.ProductSamplePhotoApproval)
                    .Include(m=>m.ProductSamplePhotoApproval.ProductSample)
                    .Get();
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductSampleShipment>().Delete(id);
        }

        public void Delete(ProductSampleShipment pt)
        {
            _unitOfWork.Repository<ProductSampleShipment>().Delete(pt);
        }

        public void Update(ProductSampleShipment pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductSampleShipment>().Update(pt);
        }

        public ProductSampleShipment CheckForShipementExist(int ProductsampleApprovalId)
        {
            return _unitOfWork.Repository<ProductSampleShipment>().Query().Get().Where(m => m.ProductSamplePhotoApproval.ProductSamplePhotoApprovalId == ProductsampleApprovalId).FirstOrDefault();
        }

        public void Dispose()
        {
        }
        
    }
}
