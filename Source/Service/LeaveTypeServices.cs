using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

using Data;
using Data.Infrastructure;
using Model.Models;
using Core.Common;
using Model;
using Data.Models;


namespace Service
{

    public interface ILeaveTypeServices : IDisposable
    {
        LeaveType Create(LeaveType pt);
        void Delete(int id);
        void Delete(LeaveType pt);
        LeaveType Find(string Name);
        LeaveType Find(int id);
        IEnumerable<LeaveType> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(LeaveType pt);
        LeaveType Add(LeaveType pt);
        IEnumerable<LeaveType> GetLeavetypeList(int SiteId);

        // IEnumerable<Godown> GetGodownList(int buyerId);
        Task<IEquatable<LeaveType>> GetAsync();
        Task<LeaveType> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }


    public class LeaveTypeServices : ILeaveTypeServices
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<LeaveType> _LeaveTypeRepository;
        RepositoryQuery<LeaveType> LeaveTypeRepository;


        public LeaveTypeServices(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LeaveTypeRepository = new Repository<LeaveType>(db);
            LeaveTypeRepository = new RepositoryQuery<LeaveType>(_LeaveTypeRepository);
        }

        public LeaveType Find(string Name)
        {
            return LeaveTypeRepository.Get().Where(i => i.LeaveTypeName == Name).FirstOrDefault();
        }


        public LeaveType Find(int id)
        {
            return _unitOfWork.Repository<LeaveType>().Find(id);
        }

        public LeaveType Create(LeaveType pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<LeaveType>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<LeaveType>().Delete(id);
        }

        public void Delete(LeaveType pt)
        {
            _unitOfWork.Repository<LeaveType>().Delete(pt);
        }

        public void Update(LeaveType pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<LeaveType>().Update(pt);
        }

        public IEnumerable<LeaveType> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<LeaveType>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.LeaveTypeName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<LeaveType> GetLeavetypeList(int SiteId)
        {
            var pt = _unitOfWork.Repository<LeaveType>().Query().Get().OrderBy(m => m.LeaveTypeName).Where(m => m.SiteId == SiteId);

            return pt;
        }

        public LeaveType Add(LeaveType pt)
        {
            _unitOfWork.Repository<LeaveType>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.LeaveType
                        orderby p.LeaveTypeName
                        select p.LeaveTypeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.LeaveType
                        orderby p.LeaveTypeName
                        select p.LeaveTypeId).FirstOrDefault();
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

                temp = (from p in db.LeaveType
                        orderby p.LeaveTypeName
                        select p.LeaveTypeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.LeaveType
                        orderby p.LeaveTypeName
                        select p.LeaveTypeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<LeaveType>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<LeaveType> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

    }
}
