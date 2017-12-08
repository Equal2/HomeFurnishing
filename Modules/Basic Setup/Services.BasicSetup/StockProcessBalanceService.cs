using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface IStockProcessBalanceService : IDisposable
    {
        StockProcessBalance Create(StockProcessBalance pt);
        void Delete(int id);
        void Update(StockProcessBalance pt);
        StockProcessBalance Find(int ProductId, int? Dimension1Id, int? Dimension2Id, int? ProcessId, string LotNo, int? GodownId, int? CostCenterId);
        void UpdateStockProcessBalance(StockProcess StockProcess);
    }

    public class StockProcessBalanceService : IStockProcessBalanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<StockProcessBalance> _StockProcessBalanceRepository;

        public StockProcessBalanceService(IUnitOfWork unitOfWork, IRepository<StockProcessBalance> stockProceBalanceRepo)
        {
            _unitOfWork = unitOfWork;
            _StockProcessBalanceRepository = stockProceBalanceRepo;
        }
        public StockProcessBalanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StockProcessBalanceRepository = unitOfWork.Repository<StockProcessBalance>();
        }

        public StockProcessBalance Create(StockProcessBalance pt)
        {
            pt.ObjectState = ObjectState.Added;
            _StockProcessBalanceRepository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _StockProcessBalanceRepository.Delete(id);
        }

        public void Delete(StockProcessBalance pt)
        {
            _StockProcessBalanceRepository.Delete(pt);
        }


        public void Update(StockProcessBalance pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _StockProcessBalanceRepository.Update(pt);
        }

        public StockProcessBalance Add(StockProcessBalance pt)
        {
            _StockProcessBalanceRepository.Insert(pt);
            return pt;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<StockProcessBalance>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StockProcessBalance> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public StockProcessBalance Find(int ProductId, int? Dimension1Id, int? Dimension2Id, int? ProcessId, string LotNo, int? GodownId, int? CostCenterId)
        {
            //var StockProcessbalance = (from L in db.StockProcessBalance
            //                             where L.ProductId == ProductId && L.Dimension1Id == Dimension1Id && L.Dimension2Id == Dimension2Id && L.ProcessId == ProcessId && L.LotNo == LotNo && L.GodownId == GodownId && L.CostCenterId == CostCenterId
            //                             select L);


            StockProcessBalance stockprocessbalance = _StockProcessBalanceRepository.Query().Get().Where(i => i.ProductId == ProductId &&
                i.Dimension1Id == Dimension1Id &&
                i.Dimension2Id == Dimension2Id &&
                i.ProcessId == ProcessId &&
                i.LotNo == LotNo &&
                i.GodownId == GodownId &&
                i.CostCenterId == CostCenterId).FirstOrDefault();

            return stockprocessbalance;
        }

        public void UpdateStockProcessBalance(StockProcess StockProcess)
        {
            StockProcessBalance StockProcessBalance = Find(StockProcess.ProductId, StockProcess.Dimension1Id, StockProcess.Dimension2Id, StockProcess.ProcessId, StockProcess.LotNo, StockProcess.GodownId, StockProcess.CostCenterId);

            if (StockProcessBalance == null)
            {
                StockProcessBalance StockProcessBalance_NewRecord = new StockProcessBalance();

                StockProcessBalance_NewRecord.ProductId = StockProcess.ProductId;
                StockProcessBalance_NewRecord.Dimension1Id = StockProcess.Dimension1Id;
                StockProcessBalance_NewRecord.Dimension2Id = StockProcess.Dimension2Id;
                StockProcessBalance_NewRecord.ProcessId = StockProcess.ProcessId;
                StockProcessBalance_NewRecord.GodownId = StockProcess.GodownId ?? 0;
                StockProcessBalance_NewRecord.CostCenterId = StockProcess.CostCenterId;
                StockProcessBalance_NewRecord.LotNo = StockProcess.LotNo;
                if (StockProcess.Qty_Iss != 0) { StockProcessBalance_NewRecord.Qty = -StockProcess.Qty_Iss; }
                if (StockProcess.Qty_Rec != 0) { StockProcessBalance_NewRecord.Qty = StockProcess.Qty_Rec; }

                Create(StockProcessBalance_NewRecord);
            }
            else
            {
                StockProcessBalance.Qty = StockProcessBalance.Qty - StockProcess.Qty_Iss;
                StockProcessBalance.Qty = StockProcessBalance.Qty + StockProcess.Qty_Rec;

                StockProcessBalance.ObjectState = Model.ObjectState.Added;
                Update(StockProcessBalance);
            }

        }
    }
}
