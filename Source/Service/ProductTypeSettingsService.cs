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
    public interface IProductTypeSettingsService : IDisposable
    {
        ProductTypeSettings Create(ProductTypeSettings pt);
        void Delete(int id);
        void Delete(ProductTypeSettings s);
        ProductTypeSettings Find(int Id);
        void Update(ProductTypeSettings s);
        IEnumerable<ProductTypeSettings> GetProductTypeSettingsList();
        ProductTypeSettings GetProductTypeSettings(int ProductTypeId);
    }

    public class ProductTypeSettingsService : IProductTypeSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductTypeSettings> _ProductTypeSettingsRepository;
        RepositoryQuery<ProductTypeSettings> ProductTypeSettingsRepository;
        public ProductTypeSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductTypeSettingsRepository = new Repository<ProductTypeSettings>(db);
            ProductTypeSettingsRepository = new RepositoryQuery<ProductTypeSettings>(_ProductTypeSettingsRepository);
        }

        public ProductTypeSettings Find(int id)
        {
            return _unitOfWork.Repository<ProductTypeSettings>().Find(id);
        }

        public ProductTypeSettings GetProductTypeSettings(int ProductTypeId)
        {
            return _unitOfWork.Repository<ProductTypeSettings>().Query().Get().Where(m => m.ProductTypeId == ProductTypeId).FirstOrDefault();
        }


        public ProductTypeSettings Create(ProductTypeSettings s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductTypeSettings>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductTypeSettings>().Delete(id);
        }

        public void Delete(ProductTypeSettings s)
        {
            _unitOfWork.Repository<ProductTypeSettings>().Delete(s);
        }

        public void Update(ProductTypeSettings s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductTypeSettings>().Update(s);
        }


        public IEnumerable<ProductTypeSettings> GetProductTypeSettingsList()
        {
            var pt = _unitOfWork.Repository<ProductTypeSettings>().Query().Get();

            return pt;
        }

        //new added
        public ProductTypeSettings GetProductTypeSettingsForDocument(int ProductTypeId)
        {
            return (from p in db.ProductTypeSettings
                    where p.ProductTypeId == ProductTypeId
                    select p
                        ).FirstOrDefault();


        }

        public void Dispose()
        {
        }

    }
}
