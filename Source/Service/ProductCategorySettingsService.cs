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
    public interface IProductCategorySettingsService : IDisposable
    {
        ProductCategorySettings Create(ProductCategorySettings pt);
        void Delete(int id);
        void Delete(ProductCategorySettings s);
        ProductCategorySettings Find(int Id);
        void Update(ProductCategorySettings s);
        IEnumerable<ProductCategorySettings> GetProductCategorySettingsList();
        ProductCategorySettings GetProductCategorySettings(int ProductCategoryId);


    }

    public class ProductCategorySettingsService : IProductCategorySettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public ProductCategorySettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProductCategorySettings Find(int id)
        {
            return _unitOfWork.Repository<ProductCategorySettings>().Find(id);
        }

        public ProductCategorySettings GetProductCategorySettings(int ProductCategoryId)
        {
            return _unitOfWork.Repository<ProductCategorySettings>().Query().Get().Where(m=> m.ProductCategoryId == ProductCategoryId).FirstOrDefault();
        }

        public ProductCategorySettings Create(ProductCategorySettings s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductCategorySettings>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductCategorySettings>().Delete(id);
        }

        public void Delete(ProductCategorySettings s)
        {
            _unitOfWork.Repository<ProductCategorySettings>().Delete(s);
        }

        public void Update(ProductCategorySettings s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductCategorySettings>().Update(s);
        }


        public IEnumerable<ProductCategorySettings> GetProductCategorySettingsList()
        {
            var pt = _unitOfWork.Repository<ProductCategorySettings>().Query().Get();

            return pt;
        }

      

        public void Dispose()
        {
        }

    }
}
