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
    public interface ICarpetSkuSettingsService : IDisposable
    {
        CarpetSkuSettings Create(CarpetSkuSettings pt);
        void Delete(int id);
        void Delete(CarpetSkuSettings s);
        CarpetSkuSettings Find(int Id);
        void Update(CarpetSkuSettings s);
        IEnumerable<CarpetSkuSettings> GetCarpetSkuSettingsList();
        CarpetSkuSettings GetCarpetSkuSettings(int DivisionId, int SiteId);

        CarpetSkuSettings GetCarpetSkuSettingsForExcelImport(int SiteId, int? DivisionId);

    }

    public class CarpetSkuSettingsService : ICarpetSkuSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public CarpetSkuSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public CarpetSkuSettings Find(int id)
        {
            return _unitOfWork.Repository<CarpetSkuSettings>().Find(id);
        }

        public CarpetSkuSettings GetCarpetSkuSettings(int DivisionId, int SiteId)
        {
            return _unitOfWork.Repository<CarpetSkuSettings>().Query().Get().Where(m=>m.DivisionId==DivisionId&&m.SiteId==SiteId).FirstOrDefault();
        }

        public CarpetSkuSettings GetCarpetSkuSettingsForExcelImport(int SiteId, int? DivisionId)
        {
            return _unitOfWork.Repository<CarpetSkuSettings>().Query().Get().Where(m => m.DivisionId == DivisionId && m.SiteId == SiteId ).FirstOrDefault();
        }

        public CarpetSkuSettings Create(CarpetSkuSettings s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<CarpetSkuSettings>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<CarpetSkuSettings>().Delete(id);
        }

        public void Delete(CarpetSkuSettings s)
        {
            _unitOfWork.Repository<CarpetSkuSettings>().Delete(s);
        }

        public void Update(CarpetSkuSettings s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<CarpetSkuSettings>().Update(s);
        }


        public IEnumerable<CarpetSkuSettings> GetCarpetSkuSettingsList()
        {
            var pt = _unitOfWork.Repository<CarpetSkuSettings>().Query().Get();

            return pt;
        }

      

        public void Dispose()
        {
        }

    }
}
