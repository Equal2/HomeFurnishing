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

namespace Service
{
    public interface IProductCategoryProcessSettingsService : IDisposable
    {
        ProductCategoryProcessSettings Create(ProductCategoryProcessSettings pt);
        void Delete(int id);
        void Delete(ProductCategoryProcessSettings s);
        ProductCategoryProcessSettings Find(int Id);
        void Update(ProductCategoryProcessSettings s);
        IEnumerable<ProductCategoryProcessSettings> GetProductCategoryProcessSettingsList();
        ProductCategoryProcessSettings GetProductCategoryProcessSettings(int ProductCategoryId, int ProcessId);
        IQueryable<ProductCategoryProcessSettingsViewModel> GetProductCategoryProcessSettingList(int ProductCategoryId);


    }

    public class ProductCategoryProcessSettingsService : IProductCategoryProcessSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public ProductCategoryProcessSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProductCategoryProcessSettings Find(int id)
        {
            return _unitOfWork.Repository<ProductCategoryProcessSettings>().Find(id);
        }

        public ProductCategoryProcessSettings GetProductCategoryProcessSettings(int ProductCategoryId, int ProcessId)
        {
            return _unitOfWork.Repository<ProductCategoryProcessSettings>().Query().Get().Where(m=> m.ProductCategoryId == ProductCategoryId && m.ProcessId == ProcessId).FirstOrDefault();
        }

        public ProductCategoryProcessSettings Create(ProductCategoryProcessSettings s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductCategoryProcessSettings>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductCategoryProcessSettings>().Delete(id);
        }

        public void Delete(ProductCategoryProcessSettings s)
        {
            _unitOfWork.Repository<ProductCategoryProcessSettings>().Delete(s);
        }

        public void Update(ProductCategoryProcessSettings s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductCategoryProcessSettings>().Update(s);
        }


        public IEnumerable<ProductCategoryProcessSettings> GetProductCategoryProcessSettingsList()
        {
            var pt = _unitOfWork.Repository<ProductCategoryProcessSettings>().Query().Get();

            return pt;
        }

        public IQueryable<ProductCategoryProcessSettingsViewModel> GetProductCategoryProcessSettingList(int ProductCategoryId)
        {
            //var pt = _unitOfWork.Repository<ProductCategoryProcessSettings>().Query().Get().OrderBy(m => m.ProcessId).Where(m => m.ProductCategoryId == ProductCategoryId);

            var pt = from P in db.ProductCategoryProcessSettings
                     where P.ProductCategoryId == ProductCategoryId
                     orderby P.Process.ProcessSr
                     select new ProductCategoryProcessSettingsViewModel
                     {
                         ProductCategoryProcessSettingsId = P.ProductCategoryProcessSettingsId,
                         ProductCategoryId = P.ProductCategoryId,
                         ProductCategoryName = P.ProductCategory.ProductCategoryName,
                         ProcessId = P.ProcessId,
                         ProcessName = P.Process.ProcessName,
                         CreatedBy = P.CreatedBy,
                         CreatedDate = P.CreatedDate,
                         ModifiedBy = P.ModifiedBy,
                         ModifiedDate = P.ModifiedDate
                     };

            return pt;
        }

        public void Dispose()
        {
        }

    }
}
