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
    public interface IRateConversionSettingsService : IDisposable
    {
        RateConversionSettings Create(RateConversionSettings pt);
        void Delete(int id);
        void Delete(RateConversionSettings pt);
        RateConversionSettings Find(int id);
        IEnumerable<RateConversionSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(RateConversionSettings pt);
        RateConversionSettings Add(RateConversionSettings pt);
        RateConversionSettings GetRateConversionSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<RateConversionSettingsViewModel> GetRateConversionSettingsList();
        Task<IEquatable<RateConversionSettings>> GetAsync();
        Task<RateConversionSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class RateConversionSettingsService : IRateConversionSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<RateConversionSettings> _RateConversionSettingsRepository;
        RepositoryQuery<RateConversionSettings> RateConversionSettingsRepository;
        public RateConversionSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _RateConversionSettingsRepository = new Repository<RateConversionSettings>(db);
            RateConversionSettingsRepository = new RepositoryQuery<RateConversionSettings>(_RateConversionSettingsRepository);
        }

        public RateConversionSettings Find(int id)
        {
            return _unitOfWork.Repository<RateConversionSettings>().Find(id);
        }
        public RateConversionSettings GetRateConversionSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.RateConversionSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public RateConversionSettings Create(RateConversionSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RateConversionSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RateConversionSettings>().Delete(id);
        }

        public void Delete(RateConversionSettings pt)
        {
            _unitOfWork.Repository<RateConversionSettings>().Delete(pt);
        }

        public void Update(RateConversionSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RateConversionSettings>().Update(pt);
        }

        public IEnumerable<RateConversionSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<RateConversionSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.RateConversionSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<RateConversionSettingsViewModel> GetRateConversionSettingsList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.RateConversionSettings
                      orderby p.RateConversionSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new RateConversionSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          RateConversionSettingsId=p.RateConversionSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public RateConversionSettings Add(RateConversionSettings pt)
        {
            _unitOfWork.Repository<RateConversionSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.RateConversionSettings
                        orderby p.RateConversionSettingsId
                        select p.RateConversionSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.RateConversionSettings
                        orderby p.RateConversionSettingsId
                        select p.RateConversionSettingsId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.RateConversionSettings
                        orderby p.RateConversionSettingsId
                        select p.RateConversionSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.RateConversionSettings
                        orderby p.RateConversionSettingsId
                        select p.RateConversionSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<RateConversionSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RateConversionSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
