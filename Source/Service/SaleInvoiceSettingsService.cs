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
    public interface ISaleInvoiceSettingService : IDisposable
    {
        SaleInvoiceSetting Create(SaleInvoiceSetting pt);
        void Delete(int id);
        void Delete(SaleInvoiceSetting pt);
        SaleInvoiceSetting Find(int id);
        IEnumerable<SaleInvoiceSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleInvoiceSetting pt);
        SaleInvoiceSetting Add(SaleInvoiceSetting pt);
        SaleInvoiceSetting GetSaleInvoiceSettingForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<SaleInvoiceSettingsViewModel> GetSaleInvoiceSettingList();
        Task<IEquatable<SaleInvoiceSetting>> GetAsync();
        Task<SaleInvoiceSetting> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class SaleInvoiceSettingService : ISaleInvoiceSettingService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleInvoiceSetting> _SaleInvoiceSettingRepository;
        RepositoryQuery<SaleInvoiceSetting> SaleInvoiceSettingRepository;
        public SaleInvoiceSettingService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleInvoiceSettingRepository = new Repository<SaleInvoiceSetting>(db);
            SaleInvoiceSettingRepository = new RepositoryQuery<SaleInvoiceSetting>(_SaleInvoiceSettingRepository);
        }

        public SaleInvoiceSetting Find(int id)
        {
            return _unitOfWork.Repository<SaleInvoiceSetting>().Find(id);
        }

        public SaleInvoiceSetting GetSaleInvoiceSettingForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.SaleInvoiceSetting
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public SaleInvoiceSetting Create(SaleInvoiceSetting pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleInvoiceSetting>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleInvoiceSetting>().Delete(id);
        }

        public void Delete(SaleInvoiceSetting pt)
        {
            _unitOfWork.Repository<SaleInvoiceSetting>().Delete(pt);
        }

        public void Update(SaleInvoiceSetting pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleInvoiceSetting>().Update(pt);
        }

        public IEnumerable<SaleInvoiceSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleInvoiceSetting>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SaleInvoiceSettingId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SaleInvoiceSettingsViewModel> GetSaleInvoiceSettingList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.SaleInvoiceSetting
                      orderby p.SaleInvoiceSettingId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new SaleInvoiceSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          SaleInvoiceSettingId=p.SaleInvoiceSettingId,
                      }
                          ).ToList();

            return pt;
        }

        public SaleInvoiceSetting Add(SaleInvoiceSetting pt)
        {
            _unitOfWork.Repository<SaleInvoiceSetting>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleInvoiceSetting
                        orderby p.SaleInvoiceSettingId
                        select p.SaleInvoiceSettingId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleInvoiceSetting
                        orderby p.SaleInvoiceSettingId
                        select p.SaleInvoiceSettingId).FirstOrDefault();
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

                temp = (from p in db.SaleInvoiceSetting
                        orderby p.SaleInvoiceSettingId
                        select p.SaleInvoiceSettingId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleInvoiceSetting
                        orderby p.SaleInvoiceSettingId
                        select p.SaleInvoiceSettingId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SaleInvoiceSetting>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleInvoiceSetting> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
