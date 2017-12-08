using System.Collections.Generic;
using System.Linq;
using Data;
using System;
using Model;
using System.Threading.Tasks;
//using AdminSetup.Models.Models;
//using AdminSetup.Models.ViewModels;
//using Infrastructure.IO;
//using Models.Company.Models;
//using Models.BasicSetup.ViewModels;
using Model.Models;
using Data.Infrastructure;
using Model.ViewModel;


namespace Service
{
    public interface ICompanySettingsService : IDisposable
    {
        CompanySettings Create(CompanySettings pt);
        void Delete(int id);
        void Delete(CompanySettings pt);
        CompanySettings Find(int id);      
        void Update(CompanySettings pt);
        CompanySettings Add(CompanySettings pt);
        CompanySettingsViewModel GetCompanySettingsForCompany(int id);
    }

    public class CompanySettingsService : ICompanySettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CompanySettings> _CompanySettingsRepository;
        private readonly IRepository<ControllerAction> _ControllerActionRepository;
        public CompanySettingsService(IUnitOfWork unitOfWork, IRepository<CompanySettings> CompanySettingsRepo,IRepository<ControllerAction> ControllerActionRepo)
        {
            _unitOfWork = unitOfWork;
            _CompanySettingsRepository = CompanySettingsRepo;
            _ControllerActionRepository = ControllerActionRepo;
        }


   


        public CompanySettings Find(int id)
        {
            return _unitOfWork.Repository<CompanySettings>().Find(id);
        }

        public CompanySettings Create(CompanySettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<CompanySettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<CompanySettings>().Delete(id);
        }

        public void Delete(CompanySettings pt)
        {
            _unitOfWork.Repository<CompanySettings>().Delete(pt);
        }

        public void Update(CompanySettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<CompanySettings>().Update(pt);
        }

        public IEnumerable<CompanySettings> GetCompanySettingsList()
        {
            var pt = (from p in _CompanySettingsRepository.Instance
                      orderby p.CompanySettingsId
                      select p
                          );

            return pt;
        }

        public CompanySettingsViewModel GetCompanySettingsForCompany(int id)
        {
            var pt = (from p in _CompanySettingsRepository.Instance
                      where p.CompanyId == id
                      select new CompanySettingsViewModel
                      {
                          CompanySettingsId = p.CompanySettingsId,
                          CompanyId = p.CompanyId,
                          isVisibleMessage = p.isVisibleMessage,
                          isVisibleNotification = p.isVisibleNotification,
                          isVisibleTask = p.isVisibleTask,
                          isVisibleGodownSelection = p.isVisibleGodownSelection,
                          isVisibleCompanyName = p.isVisibleCompanyName,
                          GodownCaption = p.GodownCaption,
                          SiteCaption = p.SiteCaption,
                          DivisionCaption = p.DivisionCaption,
                          CreatedBy = p.CreatedBy,
                          CreatedDate = p.CreatedDate,
                          ModifiedBy = p.ModifiedBy,
                          ModifiedDate = p.ModifiedDate
                      }).FirstOrDefault();

            return pt;
        }

        public CompanySettings Add(CompanySettings pt)
        {
            _unitOfWork.Repository<CompanySettings>().Insert(pt);
            return pt;
        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

    }
}
