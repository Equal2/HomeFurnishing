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
    public interface IJobOrderSettingsService : IDisposable
    {
        JobOrderSettings Create(JobOrderSettings pt);
        void Delete(int id);
        void Delete(JobOrderSettings pt);
        JobOrderSettings Find(int id);
        IEnumerable<JobOrderSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobOrderSettings pt);
        JobOrderSettings Add(JobOrderSettings pt);
        JobOrderSettings GetJobOrderSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<JobOrderSettingsViewModel> GetJobOrderSettingsList();
        Task<IEquatable<JobOrderSettings>> GetAsync();
        Task<JobOrderSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
        string GetBomProcedureForDocType(int DocTypeId);
    }

    public class JobOrderSettingsService : IJobOrderSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobOrderSettings> _JobOrderSettingsRepository;
        RepositoryQuery<JobOrderSettings> JobOrderSettingsRepository;
        public JobOrderSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobOrderSettingsRepository = new Repository<JobOrderSettings>(db);
            JobOrderSettingsRepository = new RepositoryQuery<JobOrderSettings>(_JobOrderSettingsRepository);
        }

        public JobOrderSettings Find(int id)
        {
            return _unitOfWork.Repository<JobOrderSettings>().Find(id);
        }

        public JobOrderSettings GetJobOrderSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.JobOrderSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }

        
        public JobOrderSettings Create(JobOrderSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderSettings>().Delete(id);
        }

        public void Delete(JobOrderSettings pt)
        {
            _unitOfWork.Repository<JobOrderSettings>().Delete(pt);
        }

        public void Update(JobOrderSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderSettings>().Update(pt);
        }

        public IEnumerable<JobOrderSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobOrderSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.JobOrderSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobOrderSettingsViewModel> GetJobOrderSettingsList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.JobOrderSettings
                      orderby p.JobOrderSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new JobOrderSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          JobOrderSettingsId=p.JobOrderSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public JobOrderSettings Add(JobOrderSettings pt)
        {
            _unitOfWork.Repository<JobOrderSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobOrderSettings
                        orderby p.JobOrderSettingsId
                        select p.JobOrderSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderSettings
                        orderby p.JobOrderSettingsId
                        select p.JobOrderSettingsId).FirstOrDefault();
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

                temp = (from p in db.JobOrderSettings
                        orderby p.JobOrderSettingsId
                        select p.JobOrderSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderSettings
                        orderby p.JobOrderSettingsId
                        select p.JobOrderSettingsId).AsEnumerable().LastOrDefault();
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

            return (from p in db.JobOrderSettings
                    where p.DivisionId == DivisionId && p.SiteId == SiteId && p.DocTypeId == DocTypeId
                    select p.SqlProcConsumption
                        ).FirstOrDefault();


        }

        public void Dispose()
        {
        }


        public Task<IEquatable<JobOrderSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
