using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface IStockBalanceService : IDisposable
    {
        StockBalance Create(StockBalance pt);
        void Delete(int id);
        void Delete(StockBalance pt);
        void Update(StockBalance pt);
        StockBalance Find(int ProductId, int? Dimension1Id, int? Dimension2Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId);
        void UpdateStockBalance(Stock Stock);
        StockBalance Add(StockBalance pt);
    }

    public class StockBalanceService : IStockBalanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<StockBalance> _StockBalanceRepository;

        public StockBalanceService(IUnitOfWork unitOfWork, IRepository<StockBalance> stockBalRepo)
        {
            _unitOfWork = unitOfWork;
            _StockBalanceRepository = stockBalRepo;
        }
        public StockBalanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StockBalanceRepository = unitOfWork.Repository<StockBalance>();
        }
        public StockBalance Create(StockBalance pt)
        {
            pt.ObjectState = ObjectState.Added;
            _StockBalanceRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _StockBalanceRepository.Delete(id);
        }

        public void Delete(StockBalance pt)
        {
            _StockBalanceRepository.Delete(pt);
        }


        public void Update(StockBalance pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _StockBalanceRepository.Update(pt);
        }

        public StockBalance Add(StockBalance pt)
        {
            _StockBalanceRepository.Insert(pt);
            return pt;
        }

        public Task<IEquatable<StockBalance>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StockBalance> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public StockBalance Find(int ProductId, int? Dimension1Id, int? Dimension2Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId)
        {
            StockBalance stockbalance = _StockBalanceRepository.Query().Get().Where(i => i.ProductId == ProductId &&
                    i.Dimension1Id == Dimension1Id &&
                    i.Dimension2Id == Dimension2Id &&
                    i.ProcessId == ProcessId &&
                    i.LotNo == LotNo &&
                    i.GodownId == GodownId &&
                    i.CostCenterId == CostCenterId).FirstOrDefault();

            return stockbalance;
        }

        public void UpdateStockBalance(Stock Stock)
        {
            StockBalance StockBalance = Find(Stock.ProductId, Stock.Dimension1Id, Stock.Dimension2Id, Stock.ProcessId, Stock.LotNo, Stock.GodownId, Stock.CostCenterId);

            if (StockBalance == null)
            {
                StockBalance StockBalance_NewRecord = new StockBalance();

                StockBalance_NewRecord.ProductId = Stock.ProductId;
                StockBalance_NewRecord.Dimension1Id = Stock.Dimension1Id;
                StockBalance_NewRecord.Dimension2Id = Stock.Dimension2Id;
                StockBalance_NewRecord.ProcessId = Stock.ProcessId;
                StockBalance_NewRecord.GodownId = Stock.GodownId;
                StockBalance_NewRecord.CostCenterId = Stock.CostCenterId;
                StockBalance_NewRecord.LotNo = Stock.LotNo;
                if (Stock.Qty_Iss != 0) { StockBalance_NewRecord.Qty = -Stock.Qty_Iss; }
                if (Stock.Qty_Rec != 0) { StockBalance_NewRecord.Qty = Stock.Qty_Rec; }

                Create(StockBalance_NewRecord);
            }
            else
            {
                StockBalance.Qty = StockBalance.Qty - Stock.Qty_Iss;
                StockBalance.Qty = StockBalance.Qty + Stock.Qty_Rec;

                StockBalance.ObjectState = Model.ObjectState.Added;
                Update(StockBalance);
            }

        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
