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
    public interface IJobConsumptionSettingsService : IDisposable
    {
        JobConsumptionSettings Create(JobConsumptionSettings pt);
        void Delete(int id);
        void Delete(JobConsumptionSettings pt);
        JobConsumptionSettings Find(int id);
        IEnumerable<JobConsumptionSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobConsumptionSettings pt);
        JobConsumptionSettings Add(JobConsumptionSettings pt);
        JobConsumptionSettings GetJobConsumptionSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<JobConsumptionSettingsViewModel> GetJobConsumptionSettingsList();
        Task<IEquatable<JobConsumptionSettings>> GetAsync();
        Task<JobConsumptionSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class JobConsumptionSettingsService : IJobConsumptionSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobConsumptionSettings> _JobConsumptionSettingsRepository;
        RepositoryQuery<JobConsumptionSettings> JobConsumptionSettingsRepository;
        public JobConsumptionSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobConsumptionSettingsRepository = new Repository<JobConsumptionSettings>(db);
            JobConsumptionSettingsRepository = new RepositoryQuery<JobConsumptionSettings>(_JobConsumptionSettingsRepository);
        }

        public JobConsumptionSettings Find(int id)
        {
            return _unitOfWork.Repository<JobConsumptionSettings>().Find(id);
        }
        public JobConsumptionSettings GetJobConsumptionSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.JobConsumptionSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public JobConsumptionSettings Create(JobConsumptionSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobConsumptionSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobConsumptionSettings>().Delete(id);
        }

        public void Delete(JobConsumptionSettings pt)
        {
            _unitOfWork.Repository<JobConsumptionSettings>().Delete(pt);
        }

        public void Update(JobConsumptionSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobConsumptionSettings>().Update(pt);
        }

        public IEnumerable<JobConsumptionSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobConsumptionSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.JobConsumptionSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobConsumptionSettingsViewModel> GetJobConsumptionSettingsList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.JobConsumptionSettings
                      orderby p.JobConsumptionSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new JobConsumptionSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          JobConsumptionSettingsId=p.JobConsumptionSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public JobConsumptionSettings Add(JobConsumptionSettings pt)
        {
            _unitOfWork.Repository<JobConsumptionSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobConsumptionSettings
                        orderby p.JobConsumptionSettingsId
                        select p.JobConsumptionSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobConsumptionSettings
                        orderby p.JobConsumptionSettingsId
                        select p.JobConsumptionSettingsId).FirstOrDefault();
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

                temp = (from p in db.JobConsumptionSettings
                        orderby p.JobConsumptionSettingsId
                        select p.JobConsumptionSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobConsumptionSettings
                        orderby p.JobConsumptionSettingsId
                        select p.JobConsumptionSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<JobConsumptionSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobConsumptionSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
