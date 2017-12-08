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
    public interface IProductGroupSettingsService : IDisposable
    {
        ProductGroupSettings Create(ProductGroupSettings pt);
        void Delete(int id);
        void Delete(ProductGroupSettings s);
        ProductGroupSettings Find(int Id);
        void Update(ProductGroupSettings s);
        IEnumerable<ProductGroupSettings> GetProductGroupSettingsList();
        ProductGroupSettings GetProductGroupSettings(int ProductGroupId);


    }

    public class ProductGroupSettingsService : IProductGroupSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public ProductGroupSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProductGroupSettings Find(int id)
        {
            return _unitOfWork.Repository<ProductGroupSettings>().Find(id);
        }

        public ProductGroupSettings GetProductGroupSettings(int ProductGroupId)
        {
            return _unitOfWork.Repository<ProductGroupSettings>().Query().Get().Where(m=> m.ProductGroupId == ProductGroupId).FirstOrDefault();
        }

        public ProductGroupSettings Create(ProductGroupSettings s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductGroupSettings>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductGroupSettings>().Delete(id);
        }

        public void Delete(ProductGroupSettings s)
        {
            _unitOfWork.Repository<ProductGroupSettings>().Delete(s);
        }

        public void Update(ProductGroupSettings s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductGroupSettings>().Update(s);
        }


        public IEnumerable<ProductGroupSettings> GetProductGroupSettingsList()
        {
            var pt = _unitOfWork.Repository<ProductGroupSettings>().Query().Get();

            return pt;
        }

      

        public void Dispose()
        {
        }

    }
}
