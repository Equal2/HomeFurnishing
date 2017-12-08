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
    public interface ISaleDeliverySettingService : IDisposable
    {
        SaleDeliverySetting Create(SaleDeliverySetting pt);
        void Delete(int id);
        void Delete(SaleDeliverySetting pt);
        SaleDeliverySetting Find(int id);
        IEnumerable<SaleDeliverySetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleDeliverySetting pt);
        SaleDeliverySetting Add(SaleDeliverySetting pt);
        SaleDeliverySetting GetSaleDeliverySettingForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<SaleDeliverySettingsViewModel> GetSaleDeliverySettingList();
        Task<IEquatable<SaleDeliverySetting>> GetAsync();
        Task<SaleDeliverySetting> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class SaleDeliverySettingService : ISaleDeliverySettingService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleDeliverySetting> _SaleDeliverySettingRepository;
        RepositoryQuery<SaleDeliverySetting> SaleDeliverySettingRepository;
        public SaleDeliverySettingService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleDeliverySettingRepository = new Repository<SaleDeliverySetting>(db);
            SaleDeliverySettingRepository = new RepositoryQuery<SaleDeliverySetting>(_SaleDeliverySettingRepository);
        }

        public SaleDeliverySetting Find(int id)
        {
            return _unitOfWork.Repository<SaleDeliverySetting>().Find(id);
        }

        public SaleDeliverySetting GetSaleDeliverySettingForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.SaleDeliverySettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public SaleDeliverySetting Create(SaleDeliverySetting pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDeliverySetting>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleDeliverySetting>().Delete(id);
        }

        public void Delete(SaleDeliverySetting pt)
        {
            _unitOfWork.Repository<SaleDeliverySetting>().Delete(pt);
        }

        public void Update(SaleDeliverySetting pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDeliverySetting>().Update(pt);
        }

        public IEnumerable<SaleDeliverySetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleDeliverySetting>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SaleDeliverySettingId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SaleDeliverySettingsViewModel> GetSaleDeliverySettingList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.SaleDeliverySettings
                      orderby p.SaleDeliverySettingId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new SaleDeliverySettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          SaleDeliverySettingId=p.SaleDeliverySettingId,
                      }
                          ).ToList();

            return pt;
        }

        public SaleDeliverySetting Add(SaleDeliverySetting pt)
        {
            _unitOfWork.Repository<SaleDeliverySetting>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleDeliverySettings
                        orderby p.SaleDeliverySettingId
                        select p.SaleDeliverySettingId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDeliverySettings
                        orderby p.SaleDeliverySettingId
                        select p.SaleDeliverySettingId).FirstOrDefault();
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

                temp = (from p in db.SaleDeliverySettings
                        orderby p.SaleDeliverySettingId
                        select p.SaleDeliverySettingId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDeliverySettings
                        orderby p.SaleDeliverySettingId
                        select p.SaleDeliverySettingId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SaleDeliverySetting>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleDeliverySetting> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
