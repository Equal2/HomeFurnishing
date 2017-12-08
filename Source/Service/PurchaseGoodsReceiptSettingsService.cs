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
    public interface IPurchaseGoodsReceiptSettingService : IDisposable
    {
        PurchaseGoodsReceiptSetting Create(PurchaseGoodsReceiptSetting pt);
        void Delete(int id);
        void Delete(PurchaseGoodsReceiptSetting pt);
        PurchaseGoodsReceiptSetting Find(int id);
        IEnumerable<PurchaseGoodsReceiptSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseGoodsReceiptSetting pt);
        PurchaseGoodsReceiptSetting Add(PurchaseGoodsReceiptSetting pt);
        PurchaseGoodsReceiptSetting GetPurchaseGoodsReceiptSettingForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<PurchaseGoodsReceiptSettingsViewModel> GetPurchaseGoodsReceiptSettingList();
        Task<IEquatable<PurchaseGoodsReceiptSetting>> GetAsync();
        Task<PurchaseGoodsReceiptSetting> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class PurchaseGoodsReceiptSettingService : IPurchaseGoodsReceiptSettingService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseGoodsReceiptSetting> _PurchaseGoodsReceiptSettingRepository;
        RepositoryQuery<PurchaseGoodsReceiptSetting> PurchaseGoodsReceiptSettingRepository;
        public PurchaseGoodsReceiptSettingService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseGoodsReceiptSettingRepository = new Repository<PurchaseGoodsReceiptSetting>(db);
            PurchaseGoodsReceiptSettingRepository = new RepositoryQuery<PurchaseGoodsReceiptSetting>(_PurchaseGoodsReceiptSettingRepository);
        }

        public PurchaseGoodsReceiptSetting Find(int id)
        {
            return _unitOfWork.Repository<PurchaseGoodsReceiptSetting>().Find(id);
        }

        public PurchaseGoodsReceiptSetting GetPurchaseGoodsReceiptSettingForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.PurchaseGoodsReceiptSetting
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public PurchaseGoodsReceiptSetting Create(PurchaseGoodsReceiptSetting pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseGoodsReceiptSetting>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseGoodsReceiptSetting>().Delete(id);
        }

        public void Delete(PurchaseGoodsReceiptSetting pt)
        {
            _unitOfWork.Repository<PurchaseGoodsReceiptSetting>().Delete(pt);
        }

        public void Update(PurchaseGoodsReceiptSetting pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseGoodsReceiptSetting>().Update(pt);
        }

        public IEnumerable<PurchaseGoodsReceiptSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseGoodsReceiptSetting>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PurchaseGoodsReceiptSettingId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PurchaseGoodsReceiptSettingsViewModel> GetPurchaseGoodsReceiptSettingList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.PurchaseGoodsReceiptSetting
                      orderby p.PurchaseGoodsReceiptSettingId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new PurchaseGoodsReceiptSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          PurchaseGoodsReceiptSettingId=p.PurchaseGoodsReceiptSettingId,
                      }
                          ).ToList();

            return pt;
        }

        public PurchaseGoodsReceiptSetting Add(PurchaseGoodsReceiptSetting pt)
        {
            _unitOfWork.Repository<PurchaseGoodsReceiptSetting>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseGoodsReceiptSetting
                        orderby p.PurchaseGoodsReceiptSettingId
                        select p.PurchaseGoodsReceiptSettingId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseGoodsReceiptSetting
                        orderby p.PurchaseGoodsReceiptSettingId
                        select p.PurchaseGoodsReceiptSettingId).FirstOrDefault();
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

                temp = (from p in db.PurchaseGoodsReceiptSetting
                        orderby p.PurchaseGoodsReceiptSettingId
                        select p.PurchaseGoodsReceiptSettingId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseGoodsReceiptSetting
                        orderby p.PurchaseGoodsReceiptSettingId
                        select p.PurchaseGoodsReceiptSettingId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseGoodsReceiptSetting>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseGoodsReceiptSetting> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
