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
    public interface IStockHeaderSettingsService : IDisposable
    {
        StockHeaderSettings Create(StockHeaderSettings pt);
        void Delete(int id);
        void Delete(StockHeaderSettings pt);
        StockHeaderSettings Find(int id);
        IEnumerable<StockHeaderSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(StockHeaderSettings pt);
        StockHeaderSettings Add(StockHeaderSettings pt);
        StockHeaderSettings GetStockHeaderSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<StockHeaderSettingsViewModel> GetStockHeaderSettingsList();
        Task<IEquatable<StockHeaderSettings>> GetAsync();
        Task<StockHeaderSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class StockHeaderSettingsService : IStockHeaderSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<StockHeaderSettings> _MaterialIssueSettingsRepository;
        RepositoryQuery<StockHeaderSettings> MaterialIssueSettingsRepository;
        public StockHeaderSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialIssueSettingsRepository = new Repository<StockHeaderSettings>(db);
            MaterialIssueSettingsRepository = new RepositoryQuery<StockHeaderSettings>(_MaterialIssueSettingsRepository);
        }

        public StockHeaderSettings Find(int id)
        {
            return _unitOfWork.Repository<StockHeaderSettings>().Find(id);
        }
        public StockHeaderSettings GetStockHeaderSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.MaterialIssueSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public StockHeaderSettings Create(StockHeaderSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<StockHeaderSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<StockHeaderSettings>().Delete(id);
        }

        public void Delete(StockHeaderSettings pt)
        {
            _unitOfWork.Repository<StockHeaderSettings>().Delete(pt);
        }

        public void Update(StockHeaderSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<StockHeaderSettings>().Update(pt);
        }

        public IEnumerable<StockHeaderSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<StockHeaderSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.StockHeaderSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<StockHeaderSettingsViewModel> GetStockHeaderSettingsList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.MaterialIssueSettings
                      orderby p.StockHeaderSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new StockHeaderSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          StockHeaderSettingsId=p.StockHeaderSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public StockHeaderSettings Add(StockHeaderSettings pt)
        {
            _unitOfWork.Repository<StockHeaderSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialIssueSettings
                        orderby p.StockHeaderSettingsId
                        select p.StockHeaderSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialIssueSettings
                        orderby p.StockHeaderSettingsId
                        select p.StockHeaderSettingsId).FirstOrDefault();
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

                temp = (from p in db.MaterialIssueSettings
                        orderby p.StockHeaderSettingsId
                        select p.StockHeaderSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialIssueSettings
                        orderby p.StockHeaderSettingsId
                        select p.StockHeaderSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<StockHeaderSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StockHeaderSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
