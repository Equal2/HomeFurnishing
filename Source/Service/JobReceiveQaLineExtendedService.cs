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
    public interface IJobReceiveQALineExtendedService : IDisposable
    {
        JobReceiveQALineExtended Create(JobReceiveQALineExtended pt);
        void Delete(int id);
        void Delete(JobReceiveQALineExtended pt);
        JobReceiveQALineExtended Find(int id);
        IEnumerable<JobReceiveQALineExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobReceiveQALineExtended pt);
        JobReceiveQALineExtended Add(JobReceiveQALineExtended pt);
        IEnumerable<JobReceiveQALineExtended> GetJobReceiveQALineExtendedList();

        // IEnumerable<JobReceiveQALineExtended> GetJobReceiveQALineExtendedList(int buyerId);
        Task<IEquatable<JobReceiveQALineExtended>> GetAsync();
        Task<JobReceiveQALineExtended> FindAsync(int id);
    }

    public class JobReceiveQALineExtendedService : IJobReceiveQALineExtendedService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobReceiveQALineExtended> _JobReceiveQALineExtendedRepository;
        RepositoryQuery<JobReceiveQALineExtended> JobReceiveQALineExtendedRepository;
        public JobReceiveQALineExtendedService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobReceiveQALineExtendedRepository = new Repository<JobReceiveQALineExtended>(db);
            JobReceiveQALineExtendedRepository = new RepositoryQuery<JobReceiveQALineExtended>(_JobReceiveQALineExtendedRepository);
        }


        public JobReceiveQALineExtended Find(int id)
        {
            return _unitOfWork.Repository<JobReceiveQALineExtended>().Find(id);
        }

        public JobReceiveQALineExtended Create(JobReceiveQALineExtended pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobReceiveQALineExtended>().Insert(pt);
            return pt;
        }

        public JobReceiveQALineExtended DBCreate(JobReceiveQALineExtended pt)
        {
            pt.ObjectState = ObjectState.Added;
            db.JobReceiveQALineExtended.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobReceiveQALineExtended>().Delete(id);
        }

        public void Delete(JobReceiveQALineExtended pt)
        {
            _unitOfWork.Repository<JobReceiveQALineExtended>().Delete(pt);
        }

        public void Update(JobReceiveQALineExtended pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobReceiveQALineExtended>().Update(pt);
        }

        public IEnumerable<JobReceiveQALineExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobReceiveQALineExtended>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobReceiveQALineExtended> GetJobReceiveQALineExtendedList()
        {
            var pt = _unitOfWork.Repository<JobReceiveQALineExtended>().Query().Get();

            return pt;
        }

        public JobReceiveQALineExtended Add(JobReceiveQALineExtended pt)
        {
            _unitOfWork.Repository<JobReceiveQALineExtended>().Insert(pt);
            return pt;
        }

        
        public void Dispose()
        {
        }


        public Task<IEquatable<JobReceiveQALineExtended>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveQALineExtended> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
