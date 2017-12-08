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
    public interface IProductGroupProcessSettingsService : IDisposable
    {
        ProductGroupProcessSettings Create(ProductGroupProcessSettings pt);
        void Delete(int id);
        void Delete(ProductGroupProcessSettings s);
        ProductGroupProcessSettings Find(int Id);
        void Update(ProductGroupProcessSettings s);
        IEnumerable<ProductGroupProcessSettings> GetProductGroupProcessSettingsList();
        ProductGroupProcessSettings GetProductGroupProcessSettings(int ProductGroupId, int ProcessId);
        IQueryable<ProductGroupProcessSettingsViewModel> GetProductGroupProcessSettingList(int ProductGroupId);


    }

    public class ProductGroupProcessSettingsService : IProductGroupProcessSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public ProductGroupProcessSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProductGroupProcessSettings Find(int id)
        {
            return _unitOfWork.Repository<ProductGroupProcessSettings>().Find(id);
        }

        public ProductGroupProcessSettings GetProductGroupProcessSettings(int ProductGroupId, int ProcessId)
        {
            return _unitOfWork.Repository<ProductGroupProcessSettings>().Query().Get().Where(m=> m.ProductGroupId == ProductGroupId && m.ProcessId == ProcessId).FirstOrDefault();
        }

        public ProductGroupProcessSettings Create(ProductGroupProcessSettings s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductGroupProcessSettings>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductGroupProcessSettings>().Delete(id);
        }

        public void Delete(ProductGroupProcessSettings s)
        {
            _unitOfWork.Repository<ProductGroupProcessSettings>().Delete(s);
        }

        public void Update(ProductGroupProcessSettings s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductGroupProcessSettings>().Update(s);
        }


        public IEnumerable<ProductGroupProcessSettings> GetProductGroupProcessSettingsList()
        {
            var pt = _unitOfWork.Repository<ProductGroupProcessSettings>().Query().Get();

            return pt;
        }

        public IQueryable<ProductGroupProcessSettingsViewModel> GetProductGroupProcessSettingList(int ProductGroupId)
        {
            //var pt = _unitOfWork.Repository<ProductGroupProcessSettings>().Query().Get().OrderBy(m => m.ProcessId).Where(m => m.ProductGroupId == ProductGroupId);

            var pt = from P in db.ProductGroupProcessSettings
                     where P.ProductGroupId == ProductGroupId
                     orderby P.Process.ProcessSr
                     select new ProductGroupProcessSettingsViewModel
                     {
                         ProductGroupProcessSettingsId = P.ProductGroupProcessSettingsId,
                         ProductGroupId = P.ProductGroupId,
                         ProductGroupName = P.ProductGroup.ProductGroupName,
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
