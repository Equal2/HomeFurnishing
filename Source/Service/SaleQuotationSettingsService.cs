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
    public interface ISaleQuotationSettingsService : IDisposable
    {
        SaleQuotationSettings Create(SaleQuotationSettings pt);
        void Delete(int id);
        void Delete(SaleQuotationSettings pt);
        SaleQuotationSettings Find(int id);
        IEnumerable<SaleQuotationSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleQuotationSettings pt);
        SaleQuotationSettings Add(SaleQuotationSettings pt);
        SaleQuotationSettings GetSaleQuotationSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<SaleQuotationSettingsViewModel> GetSaleQuotationSettingsList();
        Task<IEquatable<SaleQuotationSettings>> GetAsync();
        Task<SaleQuotationSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
    }

    public class SaleQuotationSettingsService : ISaleQuotationSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleQuotationSettings> _SaleQuotationSettingsRepository;
        RepositoryQuery<SaleQuotationSettings> SaleQuotationSettingsRepository;
        public SaleQuotationSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleQuotationSettingsRepository = new Repository<SaleQuotationSettings>(db);
            SaleQuotationSettingsRepository = new RepositoryQuery<SaleQuotationSettings>(_SaleQuotationSettingsRepository);
        }

        public SaleQuotationSettings Find(int id)
        {
            return _unitOfWork.Repository<SaleQuotationSettings>().Find(id);
        }

        public SaleQuotationSettings GetSaleQuotationSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.SaleQuotationSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();


        }
        public SaleQuotationSettings Create(SaleQuotationSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleQuotationSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleQuotationSettings>().Delete(id);
        }

        public void Delete(SaleQuotationSettings pt)
        {
            _unitOfWork.Repository<SaleQuotationSettings>().Delete(pt);
        }

        public void Update(SaleQuotationSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleQuotationSettings>().Update(pt);
        }

        public IEnumerable<SaleQuotationSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleQuotationSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SaleQuotationSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SaleQuotationSettingsViewModel> GetSaleQuotationSettingsList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.SaleQuotationSettings
                      orderby p.SaleQuotationSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new SaleQuotationSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          SaleQuotationSettingsId=p.SaleQuotationSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public SaleQuotationSettings Add(SaleQuotationSettings pt)
        {
            _unitOfWork.Repository<SaleQuotationSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleQuotationSettings
                        orderby p.SaleQuotationSettingsId
                        select p.SaleQuotationSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleQuotationSettings
                        orderby p.SaleQuotationSettingsId
                        select p.SaleQuotationSettingsId).FirstOrDefault();
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

                temp = (from p in db.SaleQuotationSettings
                        orderby p.SaleQuotationSettingsId
                        select p.SaleQuotationSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleQuotationSettings
                        orderby p.SaleQuotationSettingsId
                        select p.SaleQuotationSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }



        public void Dispose()
        {
        }


        public Task<IEquatable<SaleQuotationSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleQuotationSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
