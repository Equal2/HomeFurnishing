using System.Collections.Generic;
using System;
using Model;
using System.Threading.Tasks;
using Models.Customize.Models;
using Infrastructure.IO;

namespace Services.Customize
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
        void CreateHeaderStatus(int id);
    }

    public class JobOrderHeaderStatusService : IJobOrderHeaderStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobOrderHeaderStatus> _JobOrderHeaderStatusRepository;
        public JobOrderHeaderStatusService(IUnitOfWork unitOfWork, IRepository<JobOrderHeaderStatus> JobOrderHeaderStatusRepo)
        {
            _unitOfWork = unitOfWork;
            _JobOrderHeaderStatusRepository = JobOrderHeaderStatusRepo;
        }
        public JobOrderHeaderStatusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobOrderHeaderStatusRepository = unitOfWork.Repository<JobOrderHeaderStatus>();
        }

        public JobOrderHeaderStatus Find(int id)
        {
            return _JobOrderHeaderStatusRepository.Find(id);
        }

        public JobOrderHeaderStatus Create(JobOrderHeaderStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _JobOrderHeaderStatusRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _JobOrderHeaderStatusRepository.Delete(id);
        }

        public void Delete(JobOrderHeaderStatus pt)
        {
            _JobOrderHeaderStatusRepository.Delete(pt);
        }

        public void Update(JobOrderHeaderStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _JobOrderHeaderStatusRepository.Update(pt);
        }

        public IEnumerable<JobOrderHeaderStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _JobOrderHeaderStatusRepository
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobOrderHeaderStatus> GetJobOrderHeaderStatusList()
        {
            var pt = _JobOrderHeaderStatusRepository.Query().Get();

            return pt;
        }

        public JobOrderHeaderStatus Add(JobOrderHeaderStatus pt)
        {
            _JobOrderHeaderStatusRepository.Insert(pt);
            return pt;
        }

        public void CreateHeaderStatus(int id)
        {
            JobOrderHeaderStatus Stat = new JobOrderHeaderStatus();
            Stat.JobOrderHeaderId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            Create(Stat);
        }
        public void DeleteHeaderStatus(int id)
        {
            JobOrderHeaderStatus Stat = Find(id);
            Delete(Stat);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
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
