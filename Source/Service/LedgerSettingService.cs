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
    public interface ILedgerSettingService : IDisposable
    {
        LedgerSetting Create(LedgerSetting pt);
        void Delete(int id);
        void Delete(LedgerSetting pt);
        LedgerSetting Find(int id);
        IEnumerable<LedgerSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(LedgerSetting pt);
        LedgerSetting Add(LedgerSetting pt);
        LedgerSetting GetLedgerSettingForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<LedgerSettingViewModel> GetLedgerSettingList();
        Task<IEquatable<LedgerSetting>> GetAsync();
        Task<LedgerSetting> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class LedgerSettingService : ILedgerSettingService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<LedgerSetting> _LedgerSettingRepository;
        RepositoryQuery<LedgerSetting> LedgerSettingRepository;
        public LedgerSettingService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LedgerSettingRepository = new Repository<LedgerSetting>(db);
            LedgerSettingRepository = new RepositoryQuery<LedgerSetting>(_LedgerSettingRepository);
        }

        public LedgerSetting Find(int id)
        {
            return _unitOfWork.Repository<LedgerSetting>().Find(id);
        }

        public LedgerSetting GetLedgerSettingForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.LedgerSetting
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();


        }
        public LedgerSetting Create(LedgerSetting pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<LedgerSetting>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<LedgerSetting>().Delete(id);
        }

        public void Delete(LedgerSetting pt)
        {
            _unitOfWork.Repository<LedgerSetting>().Delete(pt);
        }

        public void Update(LedgerSetting pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<LedgerSetting>().Update(pt);
        }

        public IEnumerable<LedgerSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<LedgerSetting>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.LedgerSettingId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<LedgerSettingViewModel> GetLedgerSettingList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.LedgerSetting
                      orderby p.LedgerSettingId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new LedgerSettingViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          LedgerSettingId=p.LedgerSettingId,
                      }
                          ).ToList();

            return pt;
        }

        public LedgerSetting Add(LedgerSetting pt)
        {
            _unitOfWork.Repository<LedgerSetting>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.LedgerSetting
                        orderby p.LedgerSettingId
                        select p.LedgerSettingId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.LedgerSetting
                        orderby p.LedgerSettingId
                        select p.LedgerSettingId).FirstOrDefault();
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

                temp = (from p in db.LedgerSetting
                        orderby p.LedgerSettingId
                        select p.LedgerSettingId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.LedgerSetting
                        orderby p.LedgerSettingId
                        select p.LedgerSettingId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<LedgerSetting>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<LedgerSetting> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
