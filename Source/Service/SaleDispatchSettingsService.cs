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
    public interface ISaleDispatchSettingService : IDisposable
    {
        SaleDispatchSetting Create(SaleDispatchSetting pt);
        void Delete(int id);
        void Delete(SaleDispatchSetting pt);
        SaleDispatchSetting Find(int id);
        IEnumerable<SaleDispatchSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleDispatchSetting pt);
        SaleDispatchSetting Add(SaleDispatchSetting pt);
        SaleDispatchSetting GetSaleDispatchSettingForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<SaleDispatchSettingsViewModel> GetSaleDispatchSettingList();
        Task<IEquatable<SaleDispatchSetting>> GetAsync();
        Task<SaleDispatchSetting> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class SaleDispatchSettingService : ISaleDispatchSettingService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleDispatchSetting> _SaleDispatchSettingRepository;
        RepositoryQuery<SaleDispatchSetting> SaleDispatchSettingRepository;
        public SaleDispatchSettingService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleDispatchSettingRepository = new Repository<SaleDispatchSetting>(db);
            SaleDispatchSettingRepository = new RepositoryQuery<SaleDispatchSetting>(_SaleDispatchSettingRepository);
        }

        public SaleDispatchSetting Find(int id)
        {
            return _unitOfWork.Repository<SaleDispatchSetting>().Find(id);
        }

        public SaleDispatchSetting GetSaleDispatchSettingForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.SaleDispatchSetting
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public SaleDispatchSetting Create(SaleDispatchSetting pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDispatchSetting>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleDispatchSetting>().Delete(id);
        }

        public void Delete(SaleDispatchSetting pt)
        {
            _unitOfWork.Repository<SaleDispatchSetting>().Delete(pt);
        }

        public void Update(SaleDispatchSetting pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDispatchSetting>().Update(pt);
        }

        public IEnumerable<SaleDispatchSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleDispatchSetting>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SaleDispatchSettingId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SaleDispatchSettingsViewModel> GetSaleDispatchSettingList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.SaleDispatchSetting
                      orderby p.SaleDispatchSettingId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new SaleDispatchSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          SaleDispatchSettingId=p.SaleDispatchSettingId,
                      }
                          ).ToList();

            return pt;
        }

        public SaleDispatchSetting Add(SaleDispatchSetting pt)
        {
            _unitOfWork.Repository<SaleDispatchSetting>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleDispatchSetting
                        orderby p.SaleDispatchSettingId
                        select p.SaleDispatchSettingId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDispatchSetting
                        orderby p.SaleDispatchSettingId
                        select p.SaleDispatchSettingId).FirstOrDefault();
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

                temp = (from p in db.SaleDispatchSetting
                        orderby p.SaleDispatchSettingId
                        select p.SaleDispatchSettingId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDispatchSetting
                        orderby p.SaleDispatchSettingId
                        select p.SaleDispatchSettingId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SaleDispatchSetting>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleDispatchSetting> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
