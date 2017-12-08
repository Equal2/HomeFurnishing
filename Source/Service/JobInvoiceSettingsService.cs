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
    public interface IJobInvoiceSettingsService : IDisposable
    {
        JobInvoiceSettings Create(JobInvoiceSettings pt);
        void Delete(int id);
        void Delete(JobInvoiceSettings pt);
        JobInvoiceSettings Find(int id);
        IEnumerable<JobInvoiceSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobInvoiceSettings pt);
        JobInvoiceSettings Add(JobInvoiceSettings pt);
        JobInvoiceSettings GetJobInvoiceSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<JobInvoiceSettingsViewModel> GetJobInvoiceSettingsList();
        Task<IEquatable<JobInvoiceSettings>> GetAsync();
        Task<JobInvoiceSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
        string GetBomProcedureForDocType(int DocTypeId);
    }

    public class JobInvoiceSettingsService : IJobInvoiceSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobInvoiceSettings> _JobInvoiceSettingsRepository;
        RepositoryQuery<JobInvoiceSettings> JobInvoiceSettingsRepository;
        public JobInvoiceSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobInvoiceSettingsRepository = new Repository<JobInvoiceSettings>(db);
            JobInvoiceSettingsRepository = new RepositoryQuery<JobInvoiceSettings>(_JobInvoiceSettingsRepository);
        }

        public JobInvoiceSettings Find(int id)
        {
            return _unitOfWork.Repository<JobInvoiceSettings>().Find(id);
        }
        public JobInvoiceSettings GetJobInvoiceSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.JobInvoiceSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public JobInvoiceSettings Create(JobInvoiceSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobInvoiceSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobInvoiceSettings>().Delete(id);
        }

        public void Delete(JobInvoiceSettings pt)
        {
            _unitOfWork.Repository<JobInvoiceSettings>().Delete(pt);
        }

        public void Update(JobInvoiceSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobInvoiceSettings>().Update(pt);
        }

        public IEnumerable<JobInvoiceSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobInvoiceSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.JobInvoiceSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobInvoiceSettingsViewModel> GetJobInvoiceSettingsList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.JobInvoiceSettings
                      orderby p.JobInvoiceSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new JobInvoiceSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          JobInvoiceSettingsId=p.JobInvoiceSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public JobInvoiceSettings Add(JobInvoiceSettings pt)
        {
            _unitOfWork.Repository<JobInvoiceSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobInvoiceSettings
                        orderby p.JobInvoiceSettingsId
                        select p.JobInvoiceSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceSettings
                        orderby p.JobInvoiceSettingsId
                        select p.JobInvoiceSettingsId).FirstOrDefault();
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

                temp = (from p in db.JobInvoiceSettings
                        orderby p.JobInvoiceSettingsId
                        select p.JobInvoiceSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceSettings
                        orderby p.JobInvoiceSettingsId
                        select p.JobInvoiceSettingsId).AsEnumerable().LastOrDefault();
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

            return (from p in db.JobInvoiceSettings
                    where p.DivisionId == DivisionId && p.SiteId == SiteId && p.DocTypeId == DocTypeId
                    select p.SqlProcConsumption
                        ).FirstOrDefault();


        }

        public void Dispose()
        {
        }


        public Task<IEquatable<JobInvoiceSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobInvoiceSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
