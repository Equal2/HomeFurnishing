using System.Collections.Generic;
using System;
using Model;
using System.Threading.Tasks;
using Models.Customize.Models;
using Infrastructure.IO;

namespace Services.Customize
{
    public interface IJobOrderLineExtendedService : IDisposable
    {
        JobOrderLineExtended Create(JobOrderLineExtended pt);
        void Delete(int id);
        void Delete(JobOrderLineExtended pt);
        JobOrderLineExtended Find(int id);
        IEnumerable<JobOrderLineExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobOrderLineExtended pt);
        JobOrderLineExtended Add(JobOrderLineExtended pt);
        IEnumerable<JobOrderLineExtended> GetJobOrderLineExtendedList();
        Task<IEquatable<JobOrderLineExtended>> GetAsync();
        Task<JobOrderLineExtended> FindAsync(int id);
        void CreateLineStatus(int id);
    }

    public class JobOrderLineExtendedService : IJobOrderLineExtendedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobOrderLineExtended> _JobOrderLineExtendedRepository;
        public JobOrderLineExtendedService(IUnitOfWork unitOfWork,IRepository<JobOrderLineExtended> LineStatRepo)
        {
            _unitOfWork = unitOfWork;
            _JobOrderLineExtendedRepository = LineStatRepo;
        }

        public JobOrderLineExtendedService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobOrderLineExtendedRepository = unitOfWork.Repository<JobOrderLineExtended>();
        }


        public JobOrderLineExtended Find(int id)
        {
            return _JobOrderLineExtendedRepository.Find(id);
        }

        public JobOrderLineExtended Create(JobOrderLineExtended pt)
        {
            pt.ObjectState = ObjectState.Added;
            _JobOrderLineExtendedRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _JobOrderLineExtendedRepository.Delete(id);
        }

        public void Delete(JobOrderLineExtended pt)
        {
            _JobOrderLineExtendedRepository.Delete(pt);
        }

        public void Update(JobOrderLineExtended pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _JobOrderLineExtendedRepository.Update(pt);
        }

        public IEnumerable<JobOrderLineExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _JobOrderLineExtendedRepository
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobOrderLineExtended> GetJobOrderLineExtendedList()
        {
            var pt = _JobOrderLineExtendedRepository.Query().Get();

            return pt;
        }

        public JobOrderLineExtended Add(JobOrderLineExtended pt)
        {
            _JobOrderLineExtendedRepository.Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id)
        {
            JobOrderLineExtended Stat = new JobOrderLineExtended();
            Stat.JobOrderLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
                Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            JobOrderLineExtended Stat = Find(id);
            Delete(Stat);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<JobOrderLineExtended>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderLineExtended> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
