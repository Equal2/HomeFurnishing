using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using Models.BasicSetup.Models;
using Infrastructure.IO;

namespace Services.BasicSetup
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<LedgerAccountGroup> _LedgerAccountGroupRepository;

        public LedgerAccountGroupService(IUnitOfWork unitOfWork, IRepository<LedgerAccountGroup> _ledgerAccountGroupRepo)
        {
            _unitOfWork = unitOfWork;
            _LedgerAccountGroupRepository = _ledgerAccountGroupRepo;
        }
        public LedgerAccountGroupService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LedgerAccountGroupRepository = unitOfWork.Repository<LedgerAccountGroup>();
        }
        public LedgerAccountGroup GetLedgerAccountGroupByName(string terms)
        {
            return (from p in _LedgerAccountGroupRepository.Instance
                    where p.LedgerAccountGroupName == terms
                    select p).FirstOrDefault();
        }

        public LedgerAccountGroup Find(string Name)
        {
            return _LedgerAccountGroupRepository.Query().Get().Where(i => i.LedgerAccountGroupName == Name).FirstOrDefault();
        }


        public LedgerAccountGroup Find(int id)
        {
            return _LedgerAccountGroupRepository.Find(id);
        }

        public LedgerAccountGroup Create(LedgerAccountGroup pt)
        {
            pt.ObjectState = ObjectState.Added;
            _LedgerAccountGroupRepository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _LedgerAccountGroupRepository.Delete(id);
        }

        public void Delete(LedgerAccountGroup pt)
        {
            _LedgerAccountGroupRepository.Delete(pt);
        }

        public void Update(LedgerAccountGroup pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _LedgerAccountGroupRepository.Update(pt);
        }

        public IEnumerable<LedgerAccountGroup> GetLedgerAccountGroupList()
        {
            var pt = _LedgerAccountGroupRepository.Query().Get().OrderBy(m => m.LedgerAccountGroupName);

            return pt.ToList();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _LedgerAccountGroupRepository.Instance
                        orderby p.LedgerAccountGroupName
                        select p.LedgerAccountGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _LedgerAccountGroupRepository.Instance
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

                temp = (from p in _LedgerAccountGroupRepository.Instance
                        orderby p.LedgerAccountGroupName
                        select p.LedgerAccountGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _LedgerAccountGroupRepository.Instance
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

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

    }
}
