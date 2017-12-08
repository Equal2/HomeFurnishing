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
    public interface ILedgerLineRefValueService : IDisposable
    {
        LedgerLineRefValue Create(LedgerLineRefValue pt);
        void Delete(int id);
        void Delete(LedgerLineRefValue pt);       
        LedgerLineRefValue Find(int id);
        IEnumerable<LedgerLineRefValue> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(LedgerLineRefValue pt);
        LedgerLineRefValue Add(LedgerLineRefValue pt);
        IQueryable<LedgerLineRefValue> GetLedgerLineRefValueList();
        Task<IEquatable<LedgerLineRefValue>> GetAsync();
        Task<LedgerLineRefValue> FindAsync(int id);
        IEnumerable<LedgerLineRefValue> GetRefValueForLedgerLine(int id);
    }

    public class LedgerLineRefValueService : ILedgerLineRefValueService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<LedgerLineRefValue> _LedgerLineRefValueRepository;
        RepositoryQuery<LedgerLineRefValue> LedgerLineRefValueRepository;
        public LedgerLineRefValueService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LedgerLineRefValueRepository = new Repository<LedgerLineRefValue>(db);
            LedgerLineRefValueRepository = new RepositoryQuery<LedgerLineRefValue>(_LedgerLineRefValueRepository);
        }        

        public LedgerLineRefValue Find(int id)
        {
            return _unitOfWork.Repository<LedgerLineRefValue>().Find(id);
        }

        public LedgerLineRefValue Create(LedgerLineRefValue pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<LedgerLineRefValue>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<LedgerLineRefValue>().Delete(id);
        }

        public void Delete(LedgerLineRefValue pt)
        {
            _unitOfWork.Repository<LedgerLineRefValue>().Delete(pt);
        }

        public void Update(LedgerLineRefValue pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<LedgerLineRefValue>().Update(pt);
        }

        public IEnumerable<LedgerLineRefValue> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<LedgerLineRefValue>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.LedgerLineRefValueId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<LedgerLineRefValue> GetLedgerLineRefValueList()
        {
            var pt = _unitOfWork.Repository<LedgerLineRefValue>().Query().Get().OrderBy(m=>m.LedgerLineRefValueId);

            return pt;
        }

        public LedgerLineRefValue Add(LedgerLineRefValue pt)
        {
            _unitOfWork.Repository<LedgerLineRefValue>().Insert(pt);
            return pt;
        }

        public IEnumerable<LedgerLineRefValue> GetRefValueForLedgerLine(int id)
        {
            return (from p in db.LedgerLineRefValue
                    where p.LedgerLineId == id
                    select p);
        }
    
        public void Dispose()
        {
        }


        public Task<IEquatable<LedgerLineRefValue>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<LedgerLineRefValue> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
