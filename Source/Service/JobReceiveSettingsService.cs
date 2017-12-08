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
    public interface IJobReceiveSettingsService : IDisposable
    {
        JobReceiveSettings Create(JobReceiveSettings pt);
        void Delete(int id);
        void Delete(JobReceiveSettings pt);
        JobReceiveSettings Find(int id);
        IEnumerable<JobReceiveSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobReceiveSettings pt);
        JobReceiveSettings Add(JobReceiveSettings pt);
        JobReceiveSettings GetJobReceiveSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<JobReceiveSettingsViewModel> GetJobReceiveSettingsList();
        Task<IEquatable<JobReceiveSettings>> GetAsync();
        Task<JobReceiveSettings> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        string GetBomProcedureForDocType(int DocTypeId);
    }

    public class JobReceiveSettingsService : IJobReceiveSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobReceiveSettings> _JobReceiveSettingsRepository;
        RepositoryQuery<JobReceiveSettings> JobReceiveSettingsRepository;
        public JobReceiveSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobReceiveSettingsRepository = new Repository<JobReceiveSettings>(db);
            JobReceiveSettingsRepository = new RepositoryQuery<JobReceiveSettings>(_JobReceiveSettingsRepository);
        }

        public JobReceiveSettings Find(int id)
        {
            return _unitOfWork.Repository<JobReceiveSettings>().Find(id);
        }
        public JobReceiveSettings GetJobReceiveSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.JobReceiveSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public JobReceiveSettings Create(JobReceiveSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobReceiveSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobReceiveSettings>().Delete(id);
        }

        public void Delete(JobReceiveSettings pt)
        {
            _unitOfWork.Repository<JobReceiveSettings>().Delete(pt);
        }

        public void Update(JobReceiveSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobReceiveSettings>().Update(pt);
        }

        public IEnumerable<JobReceiveSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobReceiveSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.JobReceiveSettingsId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobReceiveSettingsViewModel> GetJobReceiveSettingsList()
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.JobReceiveSettings
                      orderby p.JobReceiveSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new JobReceiveSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          JobReceiveSettingsId=p.JobReceiveSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public JobReceiveSettings Add(JobReceiveSettings pt)
        {
            _unitOfWork.Repository<JobReceiveSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobReceiveSettings
                        orderby p.JobReceiveSettingsId
                        select p.JobReceiveSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobReceiveSettings
                        orderby p.JobReceiveSettingsId
                        select p.JobReceiveSettingsId).FirstOrDefault();
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

                temp = (from p in db.JobReceiveSettings
                        orderby p.JobReceiveSettingsId
                        select p.JobReceiveSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobReceiveSettings
                        orderby p.JobReceiveSettingsId
                        select p.JobReceiveSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public string GetBomProcedureForDocType(int DocTypeId)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.JobReceiveSettings
                    where p.DivisionId == DivisionId && p.SiteId == SiteId && p.DocTypeId == DocTypeId
                    select p.SqlProcConsumption
                        ).FirstOrDefault();


        }

        public void Dispose()
        {
        }


        public Task<IEquatable<JobReceiveSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
