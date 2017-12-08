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
    public interface IJobOrderPerkService : IDisposable
    {
        JobOrderPerk Create(JobOrderPerk pt);
        void Delete(int id);
        void Delete(JobOrderPerk pt);
        JobOrderPerk Find(int id);
        IEnumerable<JobOrderPerk> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobOrderPerk pt);
        JobOrderPerk Add(JobOrderPerk pt);
        IEnumerable<JobOrderPerk> GetJobOrderPerkList(int JobOrderId);
        Task<IEquatable<JobOrderPerk>> GetAsync();
        Task<JobOrderPerk> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class JobOrderPerkService : IJobOrderPerkService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobOrderPerk> _JobOrderPerkRepository;
        RepositoryQuery<JobOrderPerk> JobOrderPerkRepository;
        public JobOrderPerkService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobOrderPerkRepository = new Repository<JobOrderPerk>(db);
            JobOrderPerkRepository = new RepositoryQuery<JobOrderPerk>(_JobOrderPerkRepository);
        }

        public JobOrderPerk Find(int id)
        {
            return _unitOfWork.Repository<JobOrderPerk>().Find(id);
        }

        public JobOrderPerk Create(JobOrderPerk pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderPerk>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderPerk>().Delete(id);
        }

        public void Delete(JobOrderPerk pt)
        {
            _unitOfWork.Repository<JobOrderPerk>().Delete(pt);
        }

        public void Update(JobOrderPerk pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderPerk>().Update(pt);
        }

        public IEnumerable<JobOrderPerk> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobOrderPerk>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.JobOrderPerkId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobOrderPerk> GetJobOrderPerkList(int jobOrderId)
        {
            return (from p in db.JobOrderPerk
                    where p.JobOrderHeaderId == jobOrderId
                    select p
                        );
        }

        public JobOrderPerk Add(JobOrderPerk pt)
        {
            _unitOfWork.Repository<JobOrderPerk>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobOrderPerk
                        orderby p.JobOrderPerkId
                        select p.JobOrderPerkId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderPerk
                        orderby p.JobOrderPerkId
                        select p.JobOrderPerkId).FirstOrDefault();
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

                temp = (from p in db.JobOrderPerk
                        orderby p.JobOrderPerkId
                        select p.JobOrderPerkId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobOrderPerk
                        orderby p.JobOrderPerkId
                        select p.JobOrderPerkId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<JobOrderPerk>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderPerk> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
