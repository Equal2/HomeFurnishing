using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;
using Services.BasicSetup;

namespace Services.BasicSetup
{
    public interface ILedgerService : IDisposable
    {
        Ledger Create(Ledger pt);
        void Delete(int id);
        void Delete(Ledger m);
        void Update(Ledger pt);
        Ledger Find(int id);
        IEnumerable<Ledger> FindForLedgerHeader(int LedgerHeaderId);
        void DeleteLedgerForLedgerHeader(int LedgerHeaderId);
        void DeleteLedgerForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId);
        Ledger GetContraLedgerAccount(int? contraledgeraccountid, int headerId);
    }


    public class LedgerService : ILedgerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Ledger> _LedgerRepository;

        public LedgerService(IUnitOfWork unitOfWork, IRepository<Ledger> ledgerRepo)
        {
            _unitOfWork = unitOfWork;
            _LedgerRepository = ledgerRepo;
        }

        public LedgerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LedgerRepository = unitOfWork.Repository<Ledger>();
        }

        public Ledger GetLedger(int pt)
        {
            return _LedgerRepository.Find(pt);
        }
        public Ledger Find(int id)
        {
            return _LedgerRepository.Find(id);
        }


        public IEnumerable<Ledger> FindForLedgerHeader(int LedgerHeaderId)
        {
            return _LedgerRepository.Query().Get().Where(i => i.LedgerHeaderId == LedgerHeaderId);
        }





        public Ledger Find(int LedgerHeaderId, int LedgerAccountId, int? ContraLedgerAccountId, int? CostCenterId)
        {
            Ledger ledger = (from L in _LedgerRepository.Instance
                           where L.LedgerHeaderId == LedgerHeaderId && L.LedgerAccountId == LedgerAccountId && L.ContraLedgerAccountId == ContraLedgerAccountId &&
                                 L.CostCenterId == CostCenterId
                           select L).FirstOrDefault();
            return ledger;
        }


        public Ledger GetContraLedgerAccount(int? id, int headerId)
        {

            return _LedgerRepository.Query().Get().Where(m => m.LedgerAccountId == id && m.LedgerHeaderId == headerId).FirstOrDefault();
            //return (from p in db.Ledger where p.LedgerAccountId == id && p.LedgerHeaderId == headerId select p).FirstOrDefault();
        }
        public Ledger Create(Ledger pt)
        {
            pt.ObjectState = ObjectState.Added;
            _LedgerRepository.Insert(pt);
            return pt;
        }   
        public void Delete(int id)
        {
            _LedgerRepository.Delete(id);
        }
        public void Delete(Ledger pt)
        {
            _LedgerRepository.Delete(pt);
        }

        public void Update(Ledger pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _LedgerRepository.Update(pt);
        }

        public IEnumerable<Ledger> GetLedgerList()
        {
            var pt = _LedgerRepository.Query().Get();

            return pt;
        }

        public Ledger Add(Ledger pt)
        {
            _LedgerRepository.Insert(pt);
            return pt;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<Ledger>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Ledger> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public void DeleteLedgerForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId)
        {
            IEnumerable<Ledger> LedgerList = (from L in _LedgerRepository.Instance
                                              join H in _unitOfWork.Repository<LedgerHeader>().Instance on L.LedgerHeaderId equals H.LedgerHeaderId into LedgerHeaderTable
                                              from LedgerHeaderTab in LedgerHeaderTable.DefaultIfEmpty()
                                              where LedgerHeaderTab.DocHeaderId == DocHeaderId && LedgerHeaderTab.DocTypeId == DocTypeId && LedgerHeaderTab.SiteId == SiteId && LedgerHeaderTab.DivisionId == DivisionId
                                              select L).ToList();

            if (LedgerList != null)
            {
                foreach (Ledger item in LedgerList)
                {
                    Delete(item);
                }
                new LedgerHeaderService(_unitOfWork).Delete(LedgerList.FirstOrDefault().LedgerHeaderId);
            }
        }


        public void DeleteLedgerForLedgerHeader(int LedgerHeaderId)
        {
            IEnumerable<Ledger> LedgerList = (from L in _LedgerRepository.Instance
                                              where L.LedgerHeaderId == LedgerHeaderId
                                              select L).ToList();

            foreach (Ledger item in LedgerList)
            {
                Delete(item);
            }
        }

     


    }
}
