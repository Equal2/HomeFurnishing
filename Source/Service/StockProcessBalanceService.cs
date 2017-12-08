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
    public interface IStockProcessBalanceService : IDisposable
    {
        StockProcessBalance Create(StockProcessBalance pt);
        void Delete(int id);
        void Update(StockProcessBalance pt);
        StockProcessBalance Find(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, string LotNo, int? GodownId, int? CostCenterId);

        void UpdateStockProcessBalance(StockProcess StockProcess);
    }

    public class StockProcessBalanceService : IStockProcessBalanceService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<StockProcessBalance> _StockProcessBalanceRepository;
        RepositoryQuery<StockProcessBalance> StockProcessBalanceRepository;



        public StockProcessBalanceService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StockProcessBalanceRepository = new Repository<StockProcessBalance>(db);
            StockProcessBalanceRepository = new RepositoryQuery<StockProcessBalance>(_StockProcessBalanceRepository);
        }

        public StockProcessBalance Create(StockProcessBalance pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<StockProcessBalance>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<StockProcessBalance>().Delete(id);
        }

        public void Delete(StockProcessBalance pt)
        {
            _unitOfWork.Repository<StockProcessBalance>().Delete(pt);
        }


        public void Update(StockProcessBalance pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<StockProcessBalance>().Update(pt);
        }

        public StockProcessBalance Add(StockProcessBalance pt)
        {
            _unitOfWork.Repository<StockProcessBalance>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<StockProcessBalance>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StockProcessBalance> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public StockProcessBalance Find(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, string LotNo, int? GodownId, int? CostCenterId)
        {
            //var StockProcessbalance = (from L in db.StockProcessBalance
            //                             where L.ProductId == ProductId && L.Dimension1Id == Dimension1Id && L.Dimension2Id == Dimension2Id && L.ProcessId == ProcessId && L.LotNo == LotNo && L.GodownId == GodownId && L.CostCenterId == CostCenterId
            //                             select L);


            StockProcessBalance stockprocessbalance = _unitOfWork.Repository<StockProcessBalance>().Query().Get().Where(i => i.ProductId == ProductId &&
                i.Dimension1Id == Dimension1Id &&
                i.Dimension2Id == Dimension2Id &&
                i.Dimension3Id == Dimension3Id &&
                i.Dimension4Id == Dimension4Id &&
                i.ProcessId == ProcessId &&
                i.LotNo == LotNo &&
                i.GodownId == GodownId &&
                i.CostCenterId == CostCenterId).FirstOrDefault();

            return stockprocessbalance;
        }

        public void UpdateStockProcessBalance(StockProcess StockProcess)
        {
            StockProcessBalance StockProcessBalance = new StockProcessBalanceService(_unitOfWork).Find(StockProcess.ProductId, StockProcess.Dimension1Id, StockProcess.Dimension2Id, StockProcess.Dimension3Id, StockProcess.Dimension4Id, StockProcess.ProcessId, StockProcess.LotNo, StockProcess.GodownId, StockProcess.CostCenterId);

            if (StockProcessBalance == null)
            {
                StockProcessBalance StockProcessBalance_NewRecord = new StockProcessBalance();

                StockProcessBalance_NewRecord.ProductId = StockProcess.ProductId;
                StockProcessBalance_NewRecord.Dimension1Id = StockProcess.Dimension1Id;
                StockProcessBalance_NewRecord.Dimension2Id = StockProcess.Dimension2Id;
                StockProcessBalance_NewRecord.Dimension3Id = StockProcess.Dimension3Id;
                StockProcessBalance_NewRecord.Dimension4Id = StockProcess.Dimension4Id;
                StockProcessBalance_NewRecord.ProcessId = StockProcess.ProcessId;
                StockProcessBalance_NewRecord.GodownId = StockProcess.GodownId??0;
                StockProcessBalance_NewRecord.CostCenterId = StockProcess.CostCenterId;
                StockProcessBalance_NewRecord.LotNo = StockProcess.LotNo;
                if (StockProcess.Qty_Iss != 0) { StockProcessBalance_NewRecord.Qty = -StockProcess.Qty_Iss; }
                if (StockProcess.Qty_Rec != 0) { StockProcessBalance_NewRecord.Qty = StockProcess.Qty_Rec; }

                new StockProcessBalanceService(_unitOfWork).Create(StockProcessBalance_NewRecord);
            }
            else
            {
                StockProcessBalance.Qty = StockProcessBalance.Qty - StockProcess.Qty_Iss;
                StockProcessBalance.Qty = StockProcessBalance.Qty + StockProcess.Qty_Rec;

                StockProcessBalance.ObjectState = Model.ObjectState.Added;
                new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance);
            }

        }
    }
}
