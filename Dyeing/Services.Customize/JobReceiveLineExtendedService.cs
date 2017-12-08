using System.Collections.Generic;
using System;
using Model;
using System.Threading.Tasks;
using Models.Customize.Models;
using Infrastructure.IO;

namespace Services.Customize
{
    public interface IJobReceiveHeaderExtendedService : IDisposable
    {
        JobReceiveHeaderExtended Create(JobReceiveHeaderExtended pt);
        void Delete(int id);
        void Delete(JobReceiveHeaderExtended pt);
        JobReceiveHeaderExtended Find(int id);
        IEnumerable<JobReceiveHeaderExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobReceiveHeaderExtended pt);
        JobReceiveHeaderExtended Add(JobReceiveHeaderExtended pt);
        IEnumerable<JobReceiveHeaderExtended> GetJobReceiveHeaderExtendedList();
        Task<IEquatable<JobReceiveHeaderExtended>> GetAsync();
        Task<JobReceiveHeaderExtended> FindAsync(int id);
        void CreateLineStatus(int id);
    }

    public class JobReceiveHeaderExtendedService : IJobReceiveHeaderExtendedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobReceiveHeaderExtended> _JobReceiveHeaderExtendedRepository;
        public JobReceiveHeaderExtendedService(IUnitOfWork unitOfWork,IRepository<JobReceiveHeaderExtended> LineStatRepo)
        {
            _unitOfWork = unitOfWork;
            _JobReceiveHeaderExtendedRepository = LineStatRepo;
        }

        public JobReceiveHeaderExtendedService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobReceiveHeaderExtendedRepository = unitOfWork.Repository<JobReceiveHeaderExtended>();
        }


        public JobReceiveHeaderExtended Find(int id)
        {
            return _JobReceiveHeaderExtendedRepository.Find(id);
        }

        public JobReceiveHeaderExtended Create(JobReceiveHeaderExtended pt)
        {
            pt.ObjectState = ObjectState.Added;
            _JobReceiveHeaderExtendedRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _JobReceiveHeaderExtendedRepository.Delete(id);
        }

        public void Delete(JobReceiveHeaderExtended pt)
        {
            _JobReceiveHeaderExtendedRepository.Delete(pt);
        }

        public void Update(JobReceiveHeaderExtended pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _JobReceiveHeaderExtendedRepository.Update(pt);
        }

        public IEnumerable<JobReceiveHeaderExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _JobReceiveHeaderExtendedRepository
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobReceiveHeaderExtended> GetJobReceiveHeaderExtendedList()
        {
            var pt = _JobReceiveHeaderExtendedRepository.Query().Get();

            return pt;
        }

        public JobReceiveHeaderExtended Add(JobReceiveHeaderExtended pt)
        {
            _JobReceiveHeaderExtendedRepository.Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id)
        {
            JobReceiveHeaderExtended Stat = new JobReceiveHeaderExtended();
            Stat.JobReceiveHeaderId = id;
            Stat.ObjectState = Model.ObjectState.Added;
                Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            JobReceiveHeaderExtended Stat = Find(id);
            Delete(Stat);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<JobReceiveHeaderExtended>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveHeaderExtended> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
