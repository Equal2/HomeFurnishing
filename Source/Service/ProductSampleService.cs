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
using Model.ViewModels;

namespace Service
{
    public interface IProductSampleService : IDisposable
    {
        ProductSample Create(ProductSample p);
        void Delete(int id);
        void Delete(ProductSample p);
        ProductSample GetProductSample(int p);
        ProductSample Find(string Name);
        ProductSample Find(int id);
        IEnumerable<ProductSample> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductSample p);
      

      //  IEnumerable<ProductSample> GetProductSampleList(int prodyctTypeId);
        Task<IEquatable<ProductSample>> GetAsync();
        Task<ProductSample> FindAsync(int id);

        IEnumerable<ProductSample> GetProductSampleList(string supplyerLoginId);
        IEnumerable<ReviewSampleViewModel> GetProductSampleListForEmployee(string LoginId);
    }

    public class ProductSampleService : IProductSampleService
    {
        ApplicationDbContext db =new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductSample> _productSampleRepository;
        RepositoryQuery<ProductSample> productSampleRepository;
        public ProductSampleService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productSampleRepository = new Repository<ProductSample>(db);
            productSampleRepository = new RepositoryQuery<ProductSample>(_productSampleRepository);
        }

        public ProductSample GetProductSample(int pId)
        {
            return _unitOfWork.Repository<ProductSample>().Query() 
                .Include(m => m.Supplier)
                .Include(m => m.Employee)
                .Include(m=>m.ProductSampleAttributes)
                .Include(m=>m.ProductType)
                .Include(m=>m.ProductSamplePhoto)
                .Include(m=>m.ProductSamplePhotoApprovals)                
                .Get().Where(i => i.ProductSampleId == pId).FirstOrDefault();
           //return _unitOfWork.Repository<SalesOrder>().Find(soId);
        }

        public ProductSample Find(string Name)
        {
            return productSampleRepository.Get().Where(i => i.SampleName == Name).FirstOrDefault();
        }


        public ProductSample Find(int id)
        {
            return _unitOfWork.Repository<ProductSample>().Find(id);
        }

        public ProductSample Create(ProductSample p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductSample>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Product>().Delete(id);
        }

        public void Delete(ProductSample p)
        {
            _unitOfWork.Repository<ProductSample>().Delete(p);
        }

        public void Update(ProductSample p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductSample>().Update(p);
        }

        public IEnumerable<ProductSample> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductSample>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.EmailDate))
                .Filter(q => !string.IsNullOrEmpty(q.SampleName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }



        public IEnumerable<ProductSample> GetProductSampleList(string supplierLoginId)
        {
            var p = _unitOfWork.Repository<ProductSample>().Query().Include(m=>m.Supplier).Get().Where(s => s.Supplier.ApplicationUser.Id == supplierLoginId);
            return p;
        }

        public IEnumerable<ReviewSampleViewModel> GetProductSampleListForEmployee(string LoginId)
        {
            //var p = _unitOfWork.Repository<ProductSample>().Query()
            //            .Include(m => m.Employee)
            //            .Include (m => m.ProductSamplePhotoApproval)
            //            .Get().Where(s => s.Employee.ApplicationUser.Id == LoginId && s.ProductSamplePhotoApproval == null);


            //var p = _unitOfWork.Repository<ProductSample>().Query()
            //            .Include(m => m.Employee)
            //            .Include(m => m.Supplier)
            //            .Include(m => m.ProductSamplePhotoApprovals)
            //            .Include(m => m.ProductSamplePhoto)
            //            .Get().Where(s => s.ProductSamplePhotoApprovals.Max(a => a.ProductSamplePhotoApprovalId) == null);

           var pt = from p1 in db.Persons 
                     select new ReviewSampleViewModel
                     {

                         ProductSampleId = p1.PersonID,
                         SampleName = p1.Name,
                         SampleDescription = p1.Name,
                         EmailDate = p1.CreatedDate,
                         SupplierName = p1.Name,
                         ProductPicture = null
                     };

            //var p=from p in db.ProductSamples
            //      join p1 in db.Persons on p.PersonID equals p1.PersonID into table
            //{ p.ProductSampleId, p.SampleName, p.SampleDescription, p.EmailDate, p1.Name, p.ProductPicture };
                

            return pt;
        }
   

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductSample>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductSample> FindAsync(int id)
        {
            throw new NotImplementedException();
        }


    }
}
