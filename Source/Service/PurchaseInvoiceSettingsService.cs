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
    public interface IPurchaseInvoiceSettingService : IDisposable
    {
        PurchaseInvoiceSetting Create(PurchaseInvoiceSetting pt);
        void Delete(int id);
        void Delete(PurchaseInvoiceSetting pt);
        PurchaseInvoiceSetting Find(int id);
        IEnumerable<PurchaseInvoiceSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseInvoiceSetting pt);
        PurchaseInvoiceSetting Add(PurchaseInvoiceSetting pt);
        PurchaseInvoiceSetting GetPurchaseInvoiceSettingForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<PurchaseInvoiceSettingsViewModel> GetPurchaseInvoiceSettingList();
        Task<IEquatable<PurchaseInvoiceSetting>> GetAsync();
        Task<PurchaseInvoiceSetting> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class PurchaseInvoiceSettingService : IPurchaseInvoiceSettingService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseInvoiceSetting> _PurchaseInvoiceSettingRepository;
        RepositoryQuery<PurchaseInvoiceSetting> PurchaseInvoiceSettingRepository;
        public PurchaseInvoiceSettingService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseInvoiceSettingRepository = new Repository<PurchaseInvoiceSetting>(db);
            PurchaseInvoiceSettingRepository = new RepositoryQuery<PurchaseInvoiceSetting>(_PurchaseInvoiceSettingRepository);
        }

        public PurchaseInvoiceSetting Find(int id)
        {
            return _unitOfWork.Repository<PurchaseInvoiceSetting>().Find(id);
        }

        public PurchaseInvoiceSetting GetPurchaseInvoiceSettingForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.PurchaseInvoiceSetting
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public PurchaseInvoiceSetting Create(PurchaseInvoiceSetting pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseInvoiceSetting>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseInvoiceSetting>().Delete(id);
        }

        public void Delete(PurchaseInvoiceSetting pt)
        {
            _unitOfWork.Repository<PurchaseInvoiceSetting>().Delete(pt);
        }

        public void Update(PurchaseInvoiceSetting pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseInvoiceSetting>().Update(pt);
        }

        public IEnumerable<PurchaseInvoiceSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseInvoiceSetting>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PurchaseInvoiceSettingId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PurchaseInvoiceSettingsViewModel> GetPurchaseInvoiceSettingList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.PurchaseInvoiceSetting
                      orderby p.PurchaseInvoiceSettingId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new PurchaseInvoiceSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          PurchaseInvoiceSettingId=p.PurchaseInvoiceSettingId,
                      }
                          ).ToList();

            return pt;
        }

        public PurchaseInvoiceSetting Add(PurchaseInvoiceSetting pt)
        {
            _unitOfWork.Repository<PurchaseInvoiceSetting>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseInvoiceSetting
                        orderby p.PurchaseInvoiceSettingId
                        select p.PurchaseInvoiceSettingId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseInvoiceSetting
                        orderby p.PurchaseInvoiceSettingId
                        select p.PurchaseInvoiceSettingId).FirstOrDefault();
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

                temp = (from p in db.PurchaseInvoiceSetting
                        orderby p.PurchaseInvoiceSettingId
                        select p.PurchaseInvoiceSettingId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseInvoiceSetting
                        orderby p.PurchaseInvoiceSettingId
                        select p.PurchaseInvoiceSettingId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseInvoiceSetting>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseInvoiceSetting> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
