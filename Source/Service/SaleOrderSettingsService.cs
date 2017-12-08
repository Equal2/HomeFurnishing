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
    public interface ISaleOrderSettingsService : IDisposable
    {
        SaleOrderSettings Create(SaleOrderSettings pt);
        void Delete(int id);
        void Delete(SaleOrderSettings s);
        SaleOrderSettings Find(int Id);
        void Update(SaleOrderSettings s);
        IEnumerable<SaleOrderSettings> GetSaleOrderSettingsList();
        SaleOrderSettings GetSaleOrderSettings(int DocTypeId, int? DivisionId, int SiteId);

        SaleOrderSettings GetSaleOrderSettingsForExcelImport(int SiteId, int? DivisionId);

    }

    public class SaleOrderSettingsService : ISaleOrderSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleOrderSettings> _SaleOrderSettingsRepository;
        RepositoryQuery<SaleOrderSettings> SaleOrderSettingsRepository;
        public SaleOrderSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleOrderSettingsRepository = new Repository<SaleOrderSettings>(db);
            SaleOrderSettingsRepository = new RepositoryQuery<SaleOrderSettings>(_SaleOrderSettingsRepository);
        }

        public SaleOrderSettings Find(int id)
        {
            return _unitOfWork.Repository<SaleOrderSettings>().Find(id);
        }

        public SaleOrderSettings GetSaleOrderSettings(int DocTypeId, int? DivisionId, int SiteId)
        {
            return _unitOfWork.Repository<SaleOrderSettings>().Query().Get().Where(m=>m.DivisionId==DivisionId&&m.SiteId==SiteId && m.DocTypeId==DocTypeId).FirstOrDefault();
        }

        public SaleOrderSettings GetSaleOrderSettingsForExcelImport(int SiteId, int? DivisionId)
        {
            return _unitOfWork.Repository<SaleOrderSettings>().Query().Get().Where(m => m.DivisionId == DivisionId && m.SiteId == SiteId ).FirstOrDefault();
        }

        public SaleOrderSettings Create(SaleOrderSettings s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleOrderSettings>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleOrderSettings>().Delete(id);
        }

        public void Delete(SaleOrderSettings s)
        {
            _unitOfWork.Repository<SaleOrderSettings>().Delete(s);
        }

        public void Update(SaleOrderSettings s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleOrderSettings>().Update(s);
        }


        public IEnumerable<SaleOrderSettings> GetSaleOrderSettingsList()
        {
            var pt = _unitOfWork.Repository<SaleOrderSettings>().Query().Get();

            return pt;
        }

        //new added
        public SaleOrderSettings GetSaleOrderSettingsForDocument(int DocTypeId, int DivisionId, int SiteId)
        {
            return (from p in db.SaleOrderSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();


        }

        public void Dispose()
        {
        }

    }
}
