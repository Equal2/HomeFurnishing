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
    public interface IRequisitionSettingService : IDisposable
    {
        RequisitionSetting Create(RequisitionSetting pt);
        void Delete(int id);
        void Delete(RequisitionSetting pt);
        RequisitionSetting Find(int id);
        IEnumerable<RequisitionSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(RequisitionSetting pt);
        RequisitionSetting Add(RequisitionSetting pt);
        RequisitionSetting GetRequisitionSettingForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<RequisitionSettingsViewModel> GetRequisitionSettingList();
        Task<IEquatable<RequisitionSetting>> GetAsync();
        Task<RequisitionSetting> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class RequisitionSettingService : IRequisitionSettingService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<RequisitionSetting> _RequisitionSettingRepository;
        RepositoryQuery<RequisitionSetting> RequisitionSettingRepository;
        public RequisitionSettingService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _RequisitionSettingRepository = new Repository<RequisitionSetting>(db);
            RequisitionSettingRepository = new RepositoryQuery<RequisitionSetting>(_RequisitionSettingRepository);
        }

        public RequisitionSetting Find(int id)
        {
            return _unitOfWork.Repository<RequisitionSetting>().Find(id);
        }

        public RequisitionSetting GetRequisitionSettingForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.RequisitionSetting
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public RequisitionSetting Create(RequisitionSetting pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RequisitionSetting>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RequisitionSetting>().Delete(id);
        }

        public void Delete(RequisitionSetting pt)
        {
            _unitOfWork.Repository<RequisitionSetting>().Delete(pt);
        }

        public void Update(RequisitionSetting pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RequisitionSetting>().Update(pt);
        }

        public IEnumerable<RequisitionSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<RequisitionSetting>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.RequisitionSettingId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<RequisitionSettingsViewModel> GetRequisitionSettingList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.RequisitionSetting
                      orderby p.RequisitionSettingId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new RequisitionSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          RequisitionSettingId=p.RequisitionSettingId,
                      }
                          ).ToList();

            return pt;
        }

        public RequisitionSetting Add(RequisitionSetting pt)
        {
            _unitOfWork.Repository<RequisitionSetting>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.RequisitionSetting
                        orderby p.RequisitionSettingId
                        select p.RequisitionSettingId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.RequisitionSetting
                        orderby p.RequisitionSettingId
                        select p.RequisitionSettingId).FirstOrDefault();
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

                temp = (from p in db.RequisitionSetting
                        orderby p.RequisitionSettingId
                        select p.RequisitionSettingId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.RequisitionSetting
                        orderby p.RequisitionSettingId
                        select p.RequisitionSettingId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<RequisitionSetting>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RequisitionSetting> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
