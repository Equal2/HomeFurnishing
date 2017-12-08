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
    public interface ILedgerAccountGroupService : IDisposable
    {
        LedgerAccountGroup Create(LedgerAccountGroup pt);
        void Delete(int id);
        void Delete(LedgerAccountGroup pt);
        LedgerAccountGroup Find(string Name);
        LedgerAccountGroup Find(int id);      
        void Update(LedgerAccountGroup pt);
        LedgerAccountGroup Add(LedgerAccountGroup pt);
        IEnumerable<LedgerAccountGroup> GetLedgerAccountGroupList();
        LedgerAccountGroup GetLedgerAccountGroupByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class LedgerAccountGroupService : ILedgerAccountGroupService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<LedgerAccountGroup> _LedgerAccountGroupRepository;
        RepositoryQuery<LedgerAccountGroup> AccountGroupRepository;

        public LedgerAccountGroupService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LedgerAccountGroupRepository = new Repository<LedgerAccountGroup>(db);
            AccountGroupRepository = new RepositoryQuery<LedgerAccountGroup>(_LedgerAccountGroupRepository);
        }
        public LedgerAccountGroup GetLedgerAccountGroupByName(string terms)
        {
            return (from p in db.LedgerAccountGroup
                    where p.LedgerAccountGroupName == terms
                    select p).FirstOrDefault();
        }

        public LedgerAccountGroup Find(string Name)
        {
            return AccountGroupRepository.Get().Where(i => i.LedgerAccountGroupName == Name).FirstOrDefault();
        }


        public LedgerAccountGroup Find(int id)
        {
            return _unitOfWork.Repository<LedgerAccountGroup>().Find(id);
        }

        public LedgerAccountGroup Create(LedgerAccountGroup pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<LedgerAccountGroup>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<LedgerAccountGroup>().Delete(id);
        }

        public void Delete(LedgerAccountGroup pt)
        {
            _unitOfWork.Repository<LedgerAccountGroup>().Delete(pt);
        }

        public void Update(LedgerAccountGroup pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<LedgerAccountGroup>().Update(pt);
        }

        public IEnumerable<LedgerAccountGroup> GetLedgerAccountGroupList()
        {
            var pt = _unitOfWork.Repository<LedgerAccountGroup>().Query().Get().OrderBy(m=>m.LedgerAccountGroupName);

            return pt.ToList();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.LedgerAccountGroup
                        orderby p.LedgerAccountGroupName
                        select p.LedgerAccountGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.LedgerAccountGroup
                        orderby p.LedgerAccountGroupName
                        select p.LedgerAccountGroupId).FirstOrDefault();
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

                temp = (from p in db.LedgerAccountGroup
                        orderby p.LedgerAccountGroupName
                        select p.LedgerAccountGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.LedgerAccountGroup
                        orderby p.LedgerAccountGroupName
                        select p.LedgerAccountGroupId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public LedgerAccountGroup Add(LedgerAccountGroup pt)
        {
            _unitOfWork.Repository<LedgerAccountGroup>().Insert(pt);
            return pt;
        }

        public LedgerAccountGroup GetParentGroupDetail(int id)
        {
            var temp = (from b in db.LedgerAccountGroup
                        where b.LedgerAccountGroupId == id
                        select b).FirstOrDefault();

            return (temp);
        }

        public void Dispose()
        {
        }

    }
}
