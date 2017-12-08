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
using Model.ViewModels;
using AutoMapper;

namespace Service
{
    public interface IStockBalanceService : IDisposable
    {
        StockBalance Create(StockBalance pt);
        void Delete(int id);
        void Update(StockBalance pt);
        StockBalance Find(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId);

        void UpdateStockBalance(Stock Stock);
    }

    public class StockBalanceService : IStockBalanceService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<StockBalance> _StockBalanceRepository;
        RepositoryQuery<StockBalance> StockBalanceRepository;

        

        public StockBalanceService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StockBalanceRepository = new Repository<StockBalance>(db);
            StockBalanceRepository = new RepositoryQuery<StockBalance>(_StockBalanceRepository);
        }

        public StockBalance Create(StockBalance pt)
        {
            pt.ObjectState = ObjectState.Added;            
            _unitOfWork.Repository<StockBalance>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<StockBalance>().Delete(id);
        }

        public void Delete(StockBalance pt)
        {
            _unitOfWork.Repository<StockBalance>().Delete(pt);
        }


        public void Update(StockBalance pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<StockBalance>().Update(pt);
        }

        public StockBalance Add(StockBalance pt)
        {
            _unitOfWork.Repository<StockBalance>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<StockBalance>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StockBalance> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public StockBalance Find(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId)
        {
            //StockBalance stockbalance = (from L in db.StockBalance
            //                             where L.ProductId == ProductId && L.Dimension1Id == Dimension1Id && L.Dimension2Id == Dimension2Id && L.ProcessId == ProcessId && L.LotNo == LotNo && L.GodownId == GodownId && L.CostCenterId == CostCenterId
            //                             select L).FirstOrDefault();
            StockBalance stockbalance = _unitOfWork.Repository<StockBalance>().Query().Get().Where(i => i.ProductId == ProductId && 
                    i.Dimension1Id == Dimension1Id &&
                    i.Dimension2Id == Dimension2Id &&
                    i.Dimension3Id == Dimension3Id &&
                    i.Dimension4Id == Dimension4Id &&
                    i.ProcessId == ProcessId &&
                    i.LotNo == LotNo &&
                    i.GodownId == GodownId &&
                    i.CostCenterId == CostCenterId).FirstOrDefault();

            return stockbalance;
        }

        public void UpdateStockBalance(Stock Stock)
        {
            StockBalance StockBalance = new StockBalanceService(_unitOfWork).Find(Stock.ProductId, Stock.Dimension1Id, Stock.Dimension2Id, Stock.Dimension3Id, Stock.Dimension4Id, Stock.ProcessId, Stock.LotNo, Stock.GodownId, Stock.CostCenterId);

            if (StockBalance == null)
            {
                StockBalance StockBalance_NewRecord = new StockBalance();

                StockBalance_NewRecord.ProductId = Stock.ProductId;
                StockBalance_NewRecord.Dimension1Id = Stock.Dimension1Id;
                StockBalance_NewRecord.Dimension2Id = Stock.Dimension2Id;
                StockBalance_NewRecord.Dimension3Id = Stock.Dimension3Id;
                StockBalance_NewRecord.Dimension4Id = Stock.Dimension4Id;
                StockBalance_NewRecord.ProcessId = Stock.ProcessId;
                StockBalance_NewRecord.GodownId = Stock.GodownId;
                StockBalance_NewRecord.CostCenterId = Stock.CostCenterId;
                StockBalance_NewRecord.LotNo = Stock.LotNo;
                if (Stock.Qty_Iss != 0) { StockBalance_NewRecord.Qty = -Stock.Qty_Iss; }
                if (Stock.Qty_Rec != 0) { StockBalance_NewRecord.Qty = Stock.Qty_Rec; }

                new StockBalanceService(_unitOfWork).Create(StockBalance_NewRecord);
            }
            else
            {
                StockBalance.Qty = StockBalance.Qty - Stock.Qty_Iss;
                StockBalance.Qty = StockBalance.Qty + Stock.Qty_Rec;

                StockBalance.ObjectState = Model.ObjectState.Added;
                new StockBalanceService(_unitOfWork).Update(StockBalance);
            }

        }
    }
}
