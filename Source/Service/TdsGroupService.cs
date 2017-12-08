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
    public interface ITdsGroupService : IDisposable
    {
        TdsGroup Create(TdsGroup pt);
        void Delete(int id);
        void Delete(TdsGroup pt);
        TdsGroup GetTdsGroup(int ptId);
        TdsGroup Find(string Name);
        TdsGroup Find(int id);
        IEnumerable<TdsGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(TdsGroup pt);
        TdsGroup Add(TdsGroup pt);
        IEnumerable<TdsGroup> GetTdsGroupList();

        // IEnumerable<TdsGroup> GetTdsGroupList(int buyerId);
        Task<IEquatable<TdsGroup>> GetAsync();
        Task<TdsGroup> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class TdsGroupService : ITdsGroupService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<TdsGroup> _TdsGroupRepository;
        RepositoryQuery<TdsGroup> TdsGroupRepository;
        public TdsGroupService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _TdsGroupRepository = new Repository<TdsGroup>(db);
            TdsGroupRepository = new RepositoryQuery<TdsGroup>(_TdsGroupRepository);
        }

        public TdsGroup GetTdsGroup(int pt)
        {            
            //return _unitOfWork.Repository<TdsGroup>().Find(pt);
            return TdsGroupRepository.Get().Where(i => i.TdsGroupId == pt).FirstOrDefault();
        }


        public TdsGroup Find(string Name)
        {            
            return TdsGroupRepository.Get().Where(i => i.TdsGroupName == Name).FirstOrDefault();
        }


        public TdsGroup Find(int id)
        {
            return _unitOfWork.Repository<TdsGroup>().Find(id);            
        }

        public TdsGroup Create(TdsGroup pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<TdsGroup>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<TdsGroup>().Delete(id);
        }

        public void Delete(TdsGroup pt)
        {
            _unitOfWork.Repository<TdsGroup>().Delete(pt);
        }

        public void Update(TdsGroup pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<TdsGroup>().Update(pt);
        }

        public IEnumerable<TdsGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<TdsGroup>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.TdsGroupName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<TdsGroup> GetTdsGroupList()
        {
            var pt = _unitOfWork.Repository<TdsGroup>().Query().Get().OrderBy(m=>m.TdsGroupName);

            return pt;
        }

        public TdsGroup Add(TdsGroup pt)
        {
            _unitOfWork.Repository<TdsGroup>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.TdsGroup                        
                        orderby p.TdsGroupName
                        select p.TdsGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.TdsGroup                        
                        orderby p.TdsGroupName
                        select p.TdsGroupId).FirstOrDefault();
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

                temp = (from p in db.TdsGroup
                        orderby p.TdsGroupName
                        select p.TdsGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.TdsGroup
                        orderby p.TdsGroupName
                        select p.TdsGroupId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<TdsGroup>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TdsGroup> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
