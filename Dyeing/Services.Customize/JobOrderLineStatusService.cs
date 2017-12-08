using System.Collections.Generic;
using System;
using Model;
using System.Threading.Tasks;
using Models.Customize.Models;
using Infrastructure.IO;

namespace Services.Customize
{
    public interface IJobOrderLineStatusService : IDisposable
    {
        JobOrderLineStatus Create(JobOrderLineStatus pt);
        void Delete(int id);
        void Delete(JobOrderLineStatus pt);
        JobOrderLineStatus Find(int id);
        IEnumerable<JobOrderLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobOrderLineStatus pt);
        JobOrderLineStatus Add(JobOrderLineStatus pt);
        IEnumerable<JobOrderLineStatus> GetJobOrderLineStatusList();
        Task<IEquatable<JobOrderLineStatus>> GetAsync();
        Task<JobOrderLineStatus> FindAsync(int id);
        void CreateLineStatus(int id);
    }

    public class JobOrderLineStatusService : IJobOrderLineStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobOrderLineStatus> _JobOrderLineStatusRepository;
        public JobOrderLineStatusService(IUnitOfWork unitOfWork,IRepository<JobOrderLineStatus> LineStatRepo)
        {
            _unitOfWork = unitOfWork;
            _JobOrderLineStatusRepository = LineStatRepo;
        }

        public JobOrderLineStatusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobOrderLineStatusRepository = unitOfWork.Repository<JobOrderLineStatus>();
        }


        public JobOrderLineStatus Find(int id)
        {
            return _JobOrderLineStatusRepository.Find(id);
        }

        public JobOrderLineStatus Create(JobOrderLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _JobOrderLineStatusRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _JobOrderLineStatusRepository.Delete(id);
        }

        public void Delete(JobOrderLineStatus pt)
        {
            _JobOrderLineStatusRepository.Delete(pt);
        }

        public void Update(JobOrderLineStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _JobOrderLineStatusRepository.Update(pt);
        }

        public IEnumerable<JobOrderLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _JobOrderLineStatusRepository
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobOrderLineStatus> GetJobOrderLineStatusList()
        {
            var pt = _JobOrderLineStatusRepository.Query().Get();

            return pt;
        }

        public JobOrderLineStatus Add(JobOrderLineStatus pt)
        {
            _JobOrderLineStatusRepository.Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id)
        {
            JobOrderLineStatus Stat = new JobOrderLineStatus();
            Stat.JobOrderLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
                Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            JobOrderLineStatus Stat = Find(id);
            Delete(Stat);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<JobOrderLineStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderLineStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
