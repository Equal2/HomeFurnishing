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
    public interface IActivityLogService : IDisposable
    {
        ActivityLog Create(ActivityLog pt);
        void Delete(int id);
        void Delete(ActivityLog pt);
        ActivityLog GetActivityLog(int ptId);        
        void Update(ActivityLog pt);
        ActivityLog Add(ActivityLog pt);
        IEnumerable<ActivityLog> GetActivityLogList();

        // IEnumerable<ActivityLog> GetActivityLogList(int buyerId);
        Task<IEquatable<ActivityLog>> GetAsync();
        Task<ActivityLog> FindAsync(int id);
    }

    public class ActivityLogService : IActivityLogService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ActivityLog> _ActivityLogRepository;
        RepositoryQuery<ActivityLog> ActivityLogRepository;
        public ActivityLogService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ActivityLogRepository = new Repository<ActivityLog>(db);
            ActivityLogRepository = new RepositoryQuery<ActivityLog>(_ActivityLogRepository);
        }

        public ActivityLog GetActivityLog(int pt)
        {
            //return ActivityLogRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            return _unitOfWork.Repository<ActivityLog>().Find(pt);
        }

        public ActivityLog Create(ActivityLog pt)
        {
            pt.ObjectState = ObjectState.Added;            
            _unitOfWork.Repository<ActivityLog>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ActivityLog>().Delete(id);
        }

        public void Delete(ActivityLog pt)
        {
            _unitOfWork.Repository<ActivityLog>().Delete(pt);
        }

        public void Update(ActivityLog pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ActivityLog>().Update(pt);
        }

        

        public IEnumerable<ActivityLog> GetActivityLogList()
        {
            var pt = _unitOfWork.Repository<ActivityLog>().Query().Get();

            return pt;
        }

        public ActivityLog Add(ActivityLog pt)
        {
            _unitOfWork.Repository<ActivityLog>().Insert(pt);
            return pt;
        } 



        public void Dispose()
        {
        }        

        public Task<IEquatable<ActivityLog>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ActivityLog> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomStringOp
    {
        public static string CleanCode(string Value)
        {
            
            return Value.Replace("@", string.Empty).Replace(":", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);

        }
    }

}
