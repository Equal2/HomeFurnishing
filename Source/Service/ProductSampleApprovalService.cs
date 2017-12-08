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
    public interface IProductSampleApprovalService : IDisposable
    {
        ProductSampleApproval Create(ProductSampleApproval pt);
        ProductSampleApproval GetProductSampleApproval(int ptId);

        IEnumerable<ProductSampleApproval> GetProductSampleApprovalList();
        IEnumerable<ProductSampleApproval> GetPendingProductSampleApprovalList();
        ProductSampleApproval Find(int id);
        void Update(ProductSampleApproval ps);
        ProductSampleApproval GetSampleApprovalWithSampleId(int SampleId);
        ProductSampleApproval CheckforAlreadyReview(int shipmentId);

    }

    public class ProductSampleApprovalService : IProductSampleApprovalService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductSampleApproval> _ProductSampleApprovalRepository;
        RepositoryQuery<ProductSampleApproval> ProductSampleApprovalRepository;
        public ProductSampleApprovalService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductSampleApprovalRepository = new Repository<ProductSampleApproval>(db);
            ProductSampleApprovalRepository = new RepositoryQuery<ProductSampleApproval>(_ProductSampleApprovalRepository);
        }

        public ProductSampleApproval GetProductSampleApproval(int pt)
        {
            return _unitOfWork.Repository<ProductSampleApproval>().Query().Include(m => m.ProductSampleShipment)
                .Include(m=>m.Product)
                .Include(m => m.ProductSampleShipment.ProductSamplePhotoApproval)
                .Include(m=>m.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample.ProductSampleAttributes)
                .Include(m => m.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample)
                .Include(m=>m.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample.ProductType)
                .Include(m => m.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample.Supplier)
                .Get().Where(m => m.ProductSampleApprovalId == pt).FirstOrDefault();
        }

        public ProductSampleApproval GetSampleApprovalWithSampleId(int id)
        {
            return _unitOfWork.Repository<ProductSampleApproval>().Query()
                .Include(m=>m.ProductSampleShipment)
                .Include(m=>m.ProductSampleShipment.ProductSamplePhotoApproval)
                .Include(m=>m.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample)
                .Include(m=>m.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample.Supplier)
                .Get().Where(m => m.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample.ProductSampleId == id).FirstOrDefault();
        }

        public ProductSampleApproval CheckforAlreadyReview(int shipmentId)
        {
            return _unitOfWork.Repository<ProductSampleApproval>().Query().Get().Where(m => m.ProductSampleShipment.ProductSampleShipmentId== shipmentId).FirstOrDefault();
            
        }

        public ProductSampleApproval Create(ProductSampleApproval pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductSampleApproval>().Insert(pt);
            return pt;
        }

        public IEnumerable<ProductSampleApproval> GetProductSampleApprovalList()
        {
            var p = _unitOfWork.Repository<ProductSampleApproval>().Query()
                .Include(m => m.ProductSampleShipment)
                .Include(m=>m.ProductSampleShipment.ProductSamplePhotoApproval)
                .Include(m => m.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample)
                .Get();
            return p;
        }

        public IEnumerable<ProductSampleApproval> GetPendingProductSampleApprovalList()
        {
            var p = _unitOfWork.Repository<ProductSampleApproval>().Query()
                .Include(m => m.ProductSampleShipment)
                .Include(m => m.ProductSampleShipment.ProductSamplePhotoApproval)
                .Include(m => m.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample)
                .Include(m => m.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample.Supplier)
                .Get().Where(m=>m.Product_ProductId==null&&m.IsApproved==true);

            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductSampleApproval>().Delete(id);
        }

        public void Delete(ProductSampleApproval pt)
        {
            _unitOfWork.Repository<ProductSampleApproval>().Delete(pt);
        }

        public void Update(ProductSampleApproval pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductSampleApproval>().Update(pt);
        }

        public ProductSampleApproval Find(int id)
        {
            return _unitOfWork.Repository<ProductSampleApproval>().Find(id);
        }

        public void Dispose()
        {
        }
        
    }
}
