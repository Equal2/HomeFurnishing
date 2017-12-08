using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Infrastructure.IO;
using Models.Customize.Models;
using Models.Customize.ViewModels;

namespace Services.Customize
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
        JobReceiveSettings GetJobReceiveSettingsForDocument(int DocTypeId, int DivisionId, int SiteId);
        Task<IEquatable<JobReceiveSettings>> GetAsync();
        Task<JobReceiveSettings> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        string GetBomProcedureForDocType(int DocTypeId);
    }

    public class JobReceiveSettingsService : IJobReceiveSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobReceiveSettings> _JobReceiveSettingsRepository;
        public JobReceiveSettingsService(IUnitOfWork unitOfWork, IRepository<JobReceiveSettings> JobReceivesettingsRepo)
        {
            _unitOfWork = unitOfWork;
            _JobReceiveSettingsRepository = JobReceivesettingsRepo;
        }
        public JobReceiveSettingsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobReceiveSettingsRepository = unitOfWork.Repository<JobReceiveSettings>();
        }

        public JobReceiveSettings Find(int id)
        {
            return _unitOfWork.Repository<JobReceiveSettings>().Find(id);
        }

        public JobReceiveSettings GetJobReceiveSettingsForDocument(int DocTypeId, int DivisionId, int SiteId)
        {
            return (from p in _JobReceiveSettingsRepository.Instance
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
                temp = (from p in _JobReceiveSettingsRepository.Instance
                        orderby p.JobReceiveSettingsId
                        select p.JobReceiveSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _JobReceiveSettingsRepository.Instance
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

                temp = (from p in _JobReceiveSettingsRepository.Instance
                        orderby p.JobReceiveSettingsId
                        select p.JobReceiveSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _JobReceiveSettingsRepository.Instance
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

            return (from p in _JobReceiveSettingsRepository.Instance
                    where p.DivisionId == DivisionId && p.SiteId == SiteId && p.DocTypeId == DocTypeId
                    select p.SqlProcConsumption
                        ).FirstOrDefault();


        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
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
