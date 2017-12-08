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

namespace Service
{
    public interface IJobOrderHeaderStatusService : IDisposable
    {
        JobOrderHeaderStatus Create(JobOrderHeaderStatus pt);
        void Delete(int id);
        void Delete(JobOrderHeaderStatus pt);
        JobOrderHeaderStatus Find(int id);
        IEnumerable<JobOrderHeaderStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobOrderHeaderStatus pt);
        JobOrderHeaderStatus Add(JobOrderHeaderStatus pt);
        IEnumerable<JobOrderHeaderStatus> GetJobOrderHeaderStatusList();
        Task<IEquatable<JobOrderHeaderStatus>> GetAsync();
        Task<JobOrderHeaderStatus> FindAsync(int id);
        void CreateHeaderStatus(int id, ref ApplicationDbContext Context, bool DBbased);
    }

    public class JobOrderHeaderStatusService : IJobOrderHeaderStatusService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobOrderHeaderStatus> _JobOrderHeaderStatusRepository;
        RepositoryQuery<JobOrderHeaderStatus> JobOrderHeaderStatusRepository;
        public JobOrderHeaderStatusService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobOrderHeaderStatusRepository = new Repository<JobOrderHeaderStatus>(db);
            JobOrderHeaderStatusRepository = new RepositoryQuery<JobOrderHeaderStatus>(_JobOrderHeaderStatusRepository);
        }


        public JobOrderHeaderStatus Find(int id)
        {
            return _unitOfWork.Repository<JobOrderHeaderStatus>().Find(id);
        }

        public JobOrderHeaderStatus Create(JobOrderHeaderStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderHeaderStatus>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderHeaderStatus>().Delete(id);
        }

        public void Delete(JobOrderHeaderStatus pt)
        {
            _unitOfWork.Repository<JobOrderHeaderStatus>().Delete(pt);
        }

        public void Update(JobOrderHeaderStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderHeaderStatus>().Update(pt);
        }

        public IEnumerable<JobOrderHeaderStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobOrderHeaderStatus>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobOrderHeaderStatus> GetJobOrderHeaderStatusList()
        {
            var pt = _unitOfWork.Repository<JobOrderHeaderStatus>().Query().Get();

            return pt;
        }

        public JobOrderHeaderStatus Add(JobOrderHeaderStatus pt)
        {
            _unitOfWork.Repository<JobOrderHeaderStatus>().Insert(pt);
            return pt;
        }

        public void CreateHeaderStatus(int id, ref ApplicationDbContext Context, bool DBbased)
        {
            JobOrderHeaderStatus Stat = new JobOrderHeaderStatus();
            Stat.JobOrderHeaderId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            if (DBbased)
                Context.JobOrderHeaderStatus.Add(Stat);
            else
                Add(Stat);
        }
        public void DeleteHeaderStatus(int id)
        {
            JobOrderHeaderStatus Stat = Find(id);
            Delete(Stat);
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<JobOrderHeaderStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderHeaderStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
