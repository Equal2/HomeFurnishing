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
    public interface IProductBuyerSettingsService : IDisposable
    {
        ProductBuyerSettings Create(ProductBuyerSettings pt);
        void Delete(int id);
        void Delete(ProductBuyerSettings s);
        ProductBuyerSettings Find(int Id);
        void Update(ProductBuyerSettings s);
        IEnumerable<ProductBuyerSettings> GetProductBuyerSettingsList();
        ProductBuyerSettings GetProductBuyerSettings(int DivisionId, int SiteId);

        ProductBuyerSettings GetProductBuyerSettingsForExcelImport(int SiteId, int? DivisionId);

    }

    public class ProductBuyerSettingsService : IProductBuyerSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public ProductBuyerSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProductBuyerSettings Find(int id)
        {
            return _unitOfWork.Repository<ProductBuyerSettings>().Find(id);
        }

        public ProductBuyerSettings GetProductBuyerSettings(int DivisionId, int SiteId)
        {
            return _unitOfWork.Repository<ProductBuyerSettings>().Query().Get().Where(m=>m.DivisionId==DivisionId&&m.SiteId==SiteId).FirstOrDefault();
        }

        public ProductBuyerSettings GetProductBuyerSettingsForExcelImport(int SiteId, int? DivisionId)
        {
            return _unitOfWork.Repository<ProductBuyerSettings>().Query().Get().Where(m => m.DivisionId == DivisionId && m.SiteId == SiteId ).FirstOrDefault();
        }

        public ProductBuyerSettings Create(ProductBuyerSettings s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductBuyerSettings>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductBuyerSettings>().Delete(id);
        }

        public void Delete(ProductBuyerSettings s)
        {
            _unitOfWork.Repository<ProductBuyerSettings>().Delete(s);
        }

        public void Update(ProductBuyerSettings s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductBuyerSettings>().Update(s);
        }


        public IEnumerable<ProductBuyerSettings> GetProductBuyerSettingsList()
        {
            var pt = _unitOfWork.Repository<ProductBuyerSettings>().Query().Get();

            return pt;
        }

      

        public void Dispose()
        {
        }

    }
}
