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
    public interface IPurchaseIndentSettingService : IDisposable
    {
        PurchaseIndentSetting Create(PurchaseIndentSetting pt);
        void Delete(int id);
        void Delete(PurchaseIndentSetting pt);
        PurchaseIndentSetting Find(int id);
        IEnumerable<PurchaseIndentSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseIndentSetting pt);
        PurchaseIndentSetting Add(PurchaseIndentSetting pt);
        PurchaseIndentSetting GetPurchaseIndentSettingForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<PurchaseIndentSettingsViewModel> GetPurchaseIndentSettingList();
        Task<IEquatable<PurchaseIndentSetting>> GetAsync();
        Task<PurchaseIndentSetting> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class PurchaseIndentSettingService : IPurchaseIndentSettingService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseIndentSetting> _PurchaseIndentSettingRepository;
        RepositoryQuery<PurchaseIndentSetting> PurchaseIndentSettingRepository;
        public PurchaseIndentSettingService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseIndentSettingRepository = new Repository<PurchaseIndentSetting>(db);
            PurchaseIndentSettingRepository = new RepositoryQuery<PurchaseIndentSetting>(_PurchaseIndentSettingRepository);
        }

        public PurchaseIndentSetting Find(int id)
        {
            return _unitOfWork.Repository<PurchaseIndentSetting>().Find(id);
        }

        public PurchaseIndentSetting GetPurchaseIndentSettingForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.PurchaseIndentSetting
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public PurchaseIndentSetting Create(PurchaseIndentSetting pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseIndentSetting>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseIndentSetting>().Delete(id);
        }

        public void Delete(PurchaseIndentSetting pt)
        {
            _unitOfWork.Repository<PurchaseIndentSetting>().Delete(pt);
        }

        public void Update(PurchaseIndentSetting pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseIndentSetting>().Update(pt);
        }

        public IEnumerable<PurchaseIndentSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseIndentSetting>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PurchaseIndentSettingId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PurchaseIndentSettingsViewModel> GetPurchaseIndentSettingList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.PurchaseIndentSetting
                      orderby p.PurchaseIndentSettingId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new PurchaseIndentSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          PurchaseIndentSettingId=p.PurchaseIndentSettingId,
                      }
                          ).ToList();

            return pt;
        }

        public PurchaseIndentSetting Add(PurchaseIndentSetting pt)
        {
            _unitOfWork.Repository<PurchaseIndentSetting>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseIndentSetting
                        orderby p.PurchaseIndentSettingId
                        select p.PurchaseIndentSettingId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseIndentSetting
                        orderby p.PurchaseIndentSettingId
                        select p.PurchaseIndentSettingId).FirstOrDefault();
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

                temp = (from p in db.PurchaseIndentSetting
                        orderby p.PurchaseIndentSettingId
                        select p.PurchaseIndentSettingId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseIndentSetting
                        orderby p.PurchaseIndentSettingId
                        select p.PurchaseIndentSettingId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseIndentSetting>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseIndentSetting> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
