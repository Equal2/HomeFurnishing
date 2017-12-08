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
    public interface IPurchaseQuotationSettingService : IDisposable
    {
        PurchaseQuotationSetting Create(PurchaseQuotationSetting pt);
        void Delete(int id);
        void Delete(PurchaseQuotationSetting pt);
        PurchaseQuotationSetting Find(int id);
        IEnumerable<PurchaseQuotationSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseQuotationSetting pt);
        PurchaseQuotationSetting Add(PurchaseQuotationSetting pt);
        PurchaseQuotationSetting GetPurchaseQuotationSettingForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<PurchaseQuotationSettingsViewModel> GetPurchaseQuotationSettingList();
        Task<IEquatable<PurchaseQuotationSetting>> GetAsync();
        Task<PurchaseQuotationSetting> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class PurchaseQuotationSettingService : IPurchaseQuotationSettingService
    {
        private ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseQuotationSetting> _PurchaseQuotationSettingRepository;
        RepositoryQuery<PurchaseQuotationSetting> PurchaseQuotationSettingRepository;
        public PurchaseQuotationSettingService(IUnitOfWorkForService unitOfWork)
        {
            this.db = new ApplicationDbContext();
            _unitOfWork = unitOfWork;
            _PurchaseQuotationSettingRepository = new Repository<PurchaseQuotationSetting>(db);
            PurchaseQuotationSettingRepository = new RepositoryQuery<PurchaseQuotationSetting>(_PurchaseQuotationSettingRepository);            
        }      

        public PurchaseQuotationSetting Find(int id)
        {
            return _unitOfWork.Repository<PurchaseQuotationSetting>().Find(id);
        }

        public PurchaseQuotationSetting GetPurchaseQuotationSettingForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.PurchaseQuotationSetting
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public PurchaseQuotationSetting Create(PurchaseQuotationSetting pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseQuotationSetting>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseQuotationSetting>().Delete(id);
        }

        public void Delete(PurchaseQuotationSetting pt)
        {
            _unitOfWork.Repository<PurchaseQuotationSetting>().Delete(pt);
        }

        public void Update(PurchaseQuotationSetting pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseQuotationSetting>().Update(pt);
        }

        public IEnumerable<PurchaseQuotationSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseQuotationSetting>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PurchaseQuotationSettingId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PurchaseQuotationSettingsViewModel> GetPurchaseQuotationSettingList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.PurchaseQuotationSetting
                      orderby p.PurchaseQuotationSettingId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new PurchaseQuotationSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          PurchaseQuotationSettingId=p.PurchaseQuotationSettingId,
                      }
                          ).ToList();

            return pt;
        }

        public PurchaseQuotationSetting Add(PurchaseQuotationSetting pt)
        {
            _unitOfWork.Repository<PurchaseQuotationSetting>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseQuotationSetting
                        orderby p.PurchaseQuotationSettingId
                        select p.PurchaseQuotationSettingId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseQuotationSetting
                        orderby p.PurchaseQuotationSettingId
                        select p.PurchaseQuotationSettingId).FirstOrDefault();
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

                temp = (from p in db.PurchaseQuotationSetting
                        orderby p.PurchaseQuotationSettingId
                        select p.PurchaseQuotationSettingId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseQuotationSetting
                        orderby p.PurchaseQuotationSettingId
                        select p.PurchaseQuotationSettingId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseQuotationSetting>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseQuotationSetting> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
