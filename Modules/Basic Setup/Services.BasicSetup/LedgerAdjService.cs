using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface ILedgerAdjService : IDisposable
    {
        LedgerAdj Create(LedgerAdj pt);
        void Delete(int id);
        void Delete(LedgerAdj pt);
        LedgerAdj Find(int id);
        void Update(LedgerAdj pt);
        LedgerAdj Add(LedgerAdj pt);
        Task<IEquatable<LedgerAdj>> GetAsync();
        Task<LedgerAdj> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<LedgerAdj> FindByDrLedgerId(int DrLedgerId);
        IEnumerable<LedgerAdj> FindByCrLedgerId(int CrLedgerId);
    }

    public class LedgerAdjService : ILedgerAdjService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<LedgerAdj> _LedgerAdjRepository;
        public LedgerAdjService(IUnitOfWork unitOfWork,IRepository<LedgerAdj> LedgerAdjRepo)
        {
            _unitOfWork = unitOfWork;
            _LedgerAdjRepository = LedgerAdjRepo;
        }
        public LedgerAdjService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LedgerAdjRepository = unitOfWork.Repository<LedgerAdj>();
        }
             
        public LedgerAdj Find(int id)
        {
            return _LedgerAdjRepository.Find(id);            
        }

        public IEnumerable<LedgerAdj> FindByDrLedgerId(int DrLedgerId)
        {
            return _LedgerAdjRepository.Query().Get().Where(i => i.DrLedgerId == DrLedgerId).ToList();
        }

        public IEnumerable<LedgerAdj> FindByCrLedgerId(int CrLedgerId)
        {
            return _LedgerAdjRepository.Query().Get().Where(i => i.CrLedgerId == CrLedgerId).ToList();
        }

        public LedgerAdj Create(LedgerAdj pt)
        {
            pt.ObjectState = ObjectState.Added;
            _LedgerAdjRepository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _LedgerAdjRepository.Delete(id);
        }

        public void Delete(LedgerAdj pt)
        {
            _LedgerAdjRepository.Delete(pt);
        }

        public void Update(LedgerAdj pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _LedgerAdjRepository.Update(pt);
        }

        public IEnumerable<LedgerAdj> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _LedgerAdjRepository
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public LedgerAdj Add(LedgerAdj pt)
        {
            _LedgerAdjRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _LedgerAdjRepository.Instance
                        orderby p.LedgerAdjId
                        select p.LedgerAdjId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _LedgerAdjRepository.Instance
                        orderby p.LedgerAdjId
                        select p.LedgerAdjId).FirstOrDefault();
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

                temp = (from p in _LedgerAdjRepository.Instance
                        orderby p.LedgerAdjId
                        select p.LedgerAdjId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _LedgerAdjRepository.Instance
                        orderby p.LedgerAdjId
                        select p.LedgerAdjId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<LedgerAdj>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<LedgerAdj> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
