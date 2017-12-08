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
    public interface ISaleEnquirySettingsService : IDisposable
    {
        SaleEnquirySettings Create(SaleEnquirySettings pt);
        void Delete(int id);
        void Delete(SaleEnquirySettings s);
        SaleEnquirySettings Find(int Id);
        void Update(SaleEnquirySettings s);
        IEnumerable<SaleEnquirySettings> GetSaleEnquirySettingsList();
        SaleEnquirySettings GetSaleEnquirySettingsForDucument(int DocTypeId, int? DivisionId, int SiteId);

        SaleEnquirySettings GetSaleEnquirySettingsForExcelImport(int SiteId, int? DivisionId);

    }

    public class SaleEnquirySettingsService : ISaleEnquirySettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public SaleEnquirySettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public SaleEnquirySettings Find(int id)
        {
            return _unitOfWork.Repository<SaleEnquirySettings>().Find(id);
        }

        public SaleEnquirySettings GetSaleEnquirySettingsForDucument(int DocTypeId, int? DivisionId, int SiteId)
        {
            return _unitOfWork.Repository<SaleEnquirySettings>().Query().Get().Where(m=>m.DivisionId==DivisionId&&m.SiteId==SiteId && m.DocTypeId==DocTypeId).FirstOrDefault();
        }

        public SaleEnquirySettings GetSaleEnquirySettingsForExcelImport(int SiteId, int? DivisionId)
        {
            return _unitOfWork.Repository<SaleEnquirySettings>().Query().Get().Where(m => m.DivisionId == DivisionId && m.SiteId == SiteId ).FirstOrDefault();
        }

        public SaleEnquirySettings Create(SaleEnquirySettings s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleEnquirySettings>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleEnquirySettings>().Delete(id);
        }

        public void Delete(SaleEnquirySettings s)
        {
            _unitOfWork.Repository<SaleEnquirySettings>().Delete(s);
        }

        public void Update(SaleEnquirySettings s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleEnquirySettings>().Update(s);
        }


        public IEnumerable<SaleEnquirySettings> GetSaleEnquirySettingsList()
        {
            var pt = _unitOfWork.Repository<SaleEnquirySettings>().Query().Get();

            return pt;
        }

      

        public void Dispose()
        {
        }

    }
}
