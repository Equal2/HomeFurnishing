using System.Collections.Generic;
using System;
using Model;
using System.Threading.Tasks;
using Models.Customize.Models;
using Infrastructure.IO;

namespace Services.Customize
{
    public interface IJobReceiveLineStatusService : IDisposable
    {
        JobReceiveLineStatus Create(JobReceiveLineStatus pt);
        void Delete(int id);
        void Delete(JobReceiveLineStatus pt);
        JobReceiveLineStatus Find(int id);
        IEnumerable<JobReceiveLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobReceiveLineStatus pt);
        JobReceiveLineStatus Add(JobReceiveLineStatus pt);
        IEnumerable<JobReceiveLineStatus> GetJobReceiveLineStatusList();
        Task<IEquatable<JobReceiveLineStatus>> GetAsync();
        Task<JobReceiveLineStatus> FindAsync(int id);
        void CreateLineStatus(int id);
    }

    public class JobReceiveLineStatusService : IJobReceiveLineStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobReceiveLineStatus> _JobReceiveLineStatusRepository;
        public JobReceiveLineStatusService(IUnitOfWork unitOfWork,IRepository<JobReceiveLineStatus> LineStatRepo)
        {
            _unitOfWork = unitOfWork;
            _JobReceiveLineStatusRepository = LineStatRepo;
        }

        public JobReceiveLineStatusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobReceiveLineStatusRepository = unitOfWork.Repository<JobReceiveLineStatus>();
        }


        public JobReceiveLineStatus Find(int id)
        {
            return _JobReceiveLineStatusRepository.Find(id);
        }

        public JobReceiveLineStatus Create(JobReceiveLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _JobReceiveLineStatusRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _JobReceiveLineStatusRepository.Delete(id);
        }

        public void Delete(JobReceiveLineStatus pt)
        {
            _JobReceiveLineStatusRepository.Delete(pt);
        }

        public void Update(JobReceiveLineStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _JobReceiveLineStatusRepository.Update(pt);
        }

        public IEnumerable<JobReceiveLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _JobReceiveLineStatusRepository
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobReceiveLineStatus> GetJobReceiveLineStatusList()
        {
            var pt = _JobReceiveLineStatusRepository.Query().Get();

            return pt;
        }

        public JobReceiveLineStatus Add(JobReceiveLineStatus pt)
        {
            _JobReceiveLineStatusRepository.Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id)
        {
            JobReceiveLineStatus Stat = new JobReceiveLineStatus();
            Stat.JobReceiveLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
                Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            JobReceiveLineStatus Stat = Find(id);
            Delete(Stat);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<JobReceiveLineStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveLineStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
