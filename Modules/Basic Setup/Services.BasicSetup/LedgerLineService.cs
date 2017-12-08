using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using Models.BasicSetup.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface ILedgerLineService : IDisposable
    {
        LedgerLine Create(LedgerLine pt);
        void Delete(int id);
        void Delete(LedgerLine pt);
        LedgerLine Find(int id);
        IEnumerable<LedgerLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(LedgerLine pt);
        LedgerLine Add(LedgerLine pt);
        IEnumerable<LedgerLine> GetLedgerLineList();
        Task<IEquatable<LedgerLine>> GetAsync();
        Task<LedgerLine> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<LedgerLine> FindByLedgerHeader(int id);
    }

    public class LedgerLineService : ILedgerLineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<LedgerLine> _LedgerLineRepository;
        public LedgerLineService(IUnitOfWork unitOfWork, IRepository<LedgerLine> ledgerLineRepo)
        {
            _unitOfWork = unitOfWork;
            _LedgerLineRepository = ledgerLineRepo;
        }
        public LedgerLineService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LedgerLineRepository = unitOfWork.Repository<LedgerLine>();
        }
        public IEnumerable<LedgerLine> FindByLedgerHeader(int id)
        {
            var pt = _LedgerLineRepository.Query().Get().Where(m => m.LedgerHeaderId == id).OrderBy(m => m.LedgerLineId);

            return pt;
        }

        public LedgerLine Find(int id)
        {
            return _LedgerLineRepository.Find(id);
        }

        public LedgerLine Create(LedgerLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _LedgerLineRepository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _LedgerLineRepository.Delete(id);
        }

        public void Delete(LedgerLine pt)
        {
            _LedgerLineRepository.Delete(pt);
        }
        public void Update(LedgerLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _LedgerLineRepository.Update(pt);
        }

        public IEnumerable<LedgerLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _LedgerLineRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.LedgerLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<LedgerLine> GetLedgerLineList()
        {
            var pt = _LedgerLineRepository.Query().Get().OrderBy(m => m.LedgerLineId);

            return pt;
        }

        public LedgerLine Add(LedgerLine pt)
        {
            _LedgerLineRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _LedgerLineRepository.Instance
                        orderby p.LedgerLineId
                        select p.LedgerLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _LedgerLineRepository.Instance
                        orderby p.LedgerLineId
                        select p.LedgerLineId).FirstOrDefault();
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

                temp = (from p in _LedgerLineRepository.Instance
                        orderby p.LedgerLineId
                        select p.LedgerLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _LedgerLineRepository.Instance
                        orderby p.LedgerLineId
                        select p.LedgerLineId).AsEnumerable().LastOrDefault();
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


        public Task<IEquatable<LedgerLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<LedgerLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}



