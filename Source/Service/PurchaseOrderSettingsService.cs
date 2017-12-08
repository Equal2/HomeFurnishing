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
    public interface IPurchaseOrderSettingService : IDisposable
    {
        PurchaseOrderSetting Create(PurchaseOrderSetting pt);
        void Delete(int id);
        void Delete(PurchaseOrderSetting pt);
        PurchaseOrderSetting Find(int id);
        IEnumerable<PurchaseOrderSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseOrderSetting pt);
        PurchaseOrderSetting Add(PurchaseOrderSetting pt);
        PurchaseOrderSetting GetPurchaseOrderSettingForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<PurchaseOrderSettingsViewModel> GetPurchaseOrderSettingList();
        Task<IEquatable<PurchaseOrderSetting>> GetAsync();
        Task<PurchaseOrderSetting> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class PurchaseOrderSettingService : IPurchaseOrderSettingService
    {
        private ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseOrderSetting> _PurchaseOrderSettingRepository;
        RepositoryQuery<PurchaseOrderSetting> PurchaseOrderSettingRepository;
        public PurchaseOrderSettingService(IUnitOfWorkForService unitOfWork)
        {
            this.db = new ApplicationDbContext();
            _unitOfWork = unitOfWork;
            _PurchaseOrderSettingRepository = new Repository<PurchaseOrderSetting>(db);
            PurchaseOrderSettingRepository = new RepositoryQuery<PurchaseOrderSetting>(_PurchaseOrderSettingRepository);            
        }      

        public PurchaseOrderSetting Find(int id)
        {
            return _unitOfWork.Repository<PurchaseOrderSetting>().Find(id);
        }

        public PurchaseOrderSetting GetPurchaseOrderSettingForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.PurchaseOrderSetting
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public PurchaseOrderSetting Create(PurchaseOrderSetting pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseOrderSetting>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseOrderSetting>().Delete(id);
        }

        public void Delete(PurchaseOrderSetting pt)
        {
            _unitOfWork.Repository<PurchaseOrderSetting>().Delete(pt);
        }

        public void Update(PurchaseOrderSetting pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseOrderSetting>().Update(pt);
        }

        public IEnumerable<PurchaseOrderSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseOrderSetting>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PurchaseOrderSettingId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PurchaseOrderSettingsViewModel> GetPurchaseOrderSettingList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.PurchaseOrderSetting
                      orderby p.PurchaseOrderSettingId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new PurchaseOrderSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          PurchaseOrderSettingId=p.PurchaseOrderSettingId,
                      }
                          ).ToList();

            return pt;
        }

        public PurchaseOrderSetting Add(PurchaseOrderSetting pt)
        {
            _unitOfWork.Repository<PurchaseOrderSetting>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseOrderSetting
                        orderby p.PurchaseOrderSettingId
                        select p.PurchaseOrderSettingId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderSetting
                        orderby p.PurchaseOrderSettingId
                        select p.PurchaseOrderSettingId).FirstOrDefault();
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

                temp = (from p in db.PurchaseOrderSetting
                        orderby p.PurchaseOrderSettingId
                        select p.PurchaseOrderSettingId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderSetting
                        orderby p.PurchaseOrderSettingId
                        select p.PurchaseOrderSettingId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseOrderSetting>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrderSetting> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
