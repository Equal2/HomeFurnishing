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

        // IEnumerable<JobOrderLineExtended> GetJobOrderLineExtendedList(int buyerId);
        Task<IEquatable<JobOrderLineExtended>> GetAsync();
        Task<JobOrderLineExtended> FindAsync(int id);
        void CreateLineStatus(int id, ref ApplicationDbContext context, bool IsDBbased);        
    }

    public class JobOrderLineExtendedService : IJobOrderLineExtendedService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobOrderLineExtended> _JobOrderLineExtendedRepository;
        RepositoryQuery<JobOrderLineExtended> JobOrderLineExtendedRepository;
        public JobOrderLineExtendedService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobOrderLineExtendedRepository = new Repository<JobOrderLineExtended>(db);
            JobOrderLineExtendedRepository = new RepositoryQuery<JobOrderLineExtended>(_JobOrderLineExtendedRepository);
        }


        public JobOrderLineExtended Find(int id)
        {
            return _unitOfWork.Repository<JobOrderLineExtended>().Find(id);
        }

        public JobOrderLineExtended Create(JobOrderLineExtended pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderLineExtended>().Insert(pt);
            return pt;
        }

        public JobOrderLineExtended DBCreate(JobOrderLineExtended pt)
        {
            pt.ObjectState = ObjectState.Added;
            db.JobOrderLineExtended.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderLineExtended>().Delete(id);
        }

        public void Delete(JobOrderLineExtended pt)
        {
            _unitOfWork.Repository<JobOrderLineExtended>().Delete(pt);
        }

        public void Update(JobOrderLineExtended pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderLineExtended>().Update(pt);
        }

        public IEnumerable<JobOrderLineExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobOrderLineExtended>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobOrderLineExtended> GetJobOrderLineExtendedList()
        {
            var pt = _unitOfWork.Repository<JobOrderLineExtended>().Query().Get();

            return pt;
        }

        public JobOrderLineExtended Add(JobOrderLineExtended pt)
        {
            _unitOfWork.Repository<JobOrderLineExtended>().Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id, ref ApplicationDbContext context, bool IsDBbased)
        {
            JobOrderLineExtended Stat = new JobOrderLineExtended();
            Stat.JobOrderLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            if (IsDBbased)
                context.JobOrderLineExtended.Add(Stat);
            else
                Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            JobOrderLineExtended Stat = Find(id);
            Delete(Stat);
        }     

        public void Dispose()
        {
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
