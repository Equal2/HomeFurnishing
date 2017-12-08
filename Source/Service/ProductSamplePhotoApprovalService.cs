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
    public interface IProductSamplePhotoApprovalService : IDisposable
    {
        ProductSamplePhotoApproval Create(ProductSamplePhotoApproval pt);
        ProductSamplePhotoApproval GetProductSamplePhotoApproval(int ptId);

        IEnumerable<ProductSamplePhotoApproval> GetProductSamplePhotoApprovalList();

    }

    public class ProductSamplePhotoApprovalService : IProductSamplePhotoApprovalService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductSamplePhotoApproval> _ProductSamplePhotoApprovalRepository;
        RepositoryQuery<ProductSamplePhotoApproval> ProductSamplePhotoApprovalRepository;
        public ProductSamplePhotoApprovalService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductSamplePhotoApprovalRepository = new Repository<ProductSamplePhotoApproval>(db);
            ProductSamplePhotoApprovalRepository = new RepositoryQuery<ProductSamplePhotoApproval>(_ProductSamplePhotoApprovalRepository);
        }

        public ProductSamplePhotoApproval GetProductSamplePhotoApproval(int pt)
        {
            //return CurrencyRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            return _unitOfWork.Repository<ProductSamplePhotoApproval>().Query ()
                        .Include (m => m.ProductSample)
                        .Get().Where(m => m.ProductSamplePhotoApprovalId == pt).FirstOrDefault();
        }

        public ProductSamplePhotoApproval Create(ProductSamplePhotoApproval pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductSamplePhotoApproval>().Insert(pt);
            return pt;
        }

        public IEnumerable<ProductSamplePhotoApproval> GetProductSamplePhotoApprovalList()
        {
            var p = _unitOfWork.Repository<ProductSamplePhotoApproval>().Query().Include(m=>m.ProductSample).Get();
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductSamplePhotoApproval>().Delete(id);
        }

        public void Delete(ProductSamplePhotoApproval pt)
        {
            _unitOfWork.Repository<ProductSamplePhotoApproval>().Delete(pt);
        }

        public void Update(ProductSamplePhotoApproval pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductSamplePhotoApproval>().Update(pt);
        }

        public void Dispose()
        {
        }
        
    }
}
