using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface IStockService : IDisposable
    {
        Stock Create(Stock pt);
        void Delete(int id);
        void Update(Stock pt);
        Stock Find(int id);
        string StockPost(StockViewModel StockViewModel_New, StockViewModel StockViewModel_Old);
        string StockPostDB(ref StockViewModel StockViewModel);
        void DeleteStock(int StockId);
        void DeleteStockDB(int StockId, bool IsDBbased);
        IEnumerable<Stock> GetStockForStockHeaderId(int StockHeaderId);
        void DeleteStockMultiple(List<int> StockId);
    }

    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Stock> _StockRepository;
        private readonly IStockHeaderService _stockHeaderService;
        private readonly IStockBalanceService _stockBalanceService;

        public StockService(IUnitOfWork unitOfWork, IRepository<Stock> StockRepo, IStockHeaderService stockHeaderService,
            IStockBalanceService stockBalService)
        {
            _unitOfWork = unitOfWork;
            _StockRepository = StockRepo;
            _stockHeaderService = stockHeaderService;
            _stockBalanceService = stockBalService;
        }

        public Stock GetStock(int pt)
        {
            return _StockRepository.Find(pt);
        }

        public Stock Create(Stock pt)
        {
            pt.ObjectState = ObjectState.Added;
            _StockRepository.Insert(pt);
            return pt;
        }

        public Stock Find(int id)
        {
            return _StockRepository.Find(id);
        }

        public void Delete(int id)
        {
            _StockRepository.Delete(id);
        }

        public void Delete(Stock pt)
        {
            _StockRepository.Delete(pt);
            //db.Stock.Remove(pt);
        }

        public void Update(Stock pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _StockRepository.Update(pt);
        }

        public IEnumerable<Stock> GetStockList()
        {
            var pt = _StockRepository.Query().Get();

            return pt;
        }

        public Stock Add(Stock pt)
        {
            _StockRepository.Insert(pt);
            return pt;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<Stock>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Stock> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Stock Find(int StockHeaderId, int ProductId, DateTime DocDate, int? Dimension1Id, int? Dimension2Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId)
        {
            Stock stock = (from L in _StockRepository.Instance
                           where L.StockHeaderId == StockHeaderId && L.ProductId == ProductId && L.DocDate == DocDate && L.Dimension1Id == Dimension1Id &&
                                 L.Dimension2Id == Dimension2Id && L.ProcessId == ProcessId && L.LotNo == LotNo && L.GodownId == GodownId && L.CostCenterId == CostCenterId
                           select L).FirstOrDefault();

            return stock;
        }

        public StockBalance FindStockBalance(int ProductId, int? Dimension1Id, int? Dimension2Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId)
        {
            StockBalance stockbalance = (from L in _unitOfWork.Repository<StockBalance>().Instance
                                         where L.ProductId == ProductId && L.Dimension1Id == Dimension1Id && L.Dimension2Id == Dimension2Id && L.ProcessId == ProcessId && L.LotNo == LotNo && L.GodownId == GodownId && L.CostCenterId == CostCenterId
                                         select L).FirstOrDefault();
            return stockbalance;
        }

        /// <summary>
        /// Create, update and delete Stock Posting for one record.
        /// </summary>
        /// <param name="StockViewModel_New"></param>
        /// <param name="StockViewModel_Old"></param>
        /// <returns></returns>
        public string StockPost(StockViewModel StockViewModel_New, StockViewModel StockViewModel_Old)
        {
            string ErrorText = "";
            StockHeader StockHeader;


            if (StockViewModel_New != null)
            {
                StockHeader = _stockHeaderService.FindByDocHeader(StockViewModel_New.DocHeaderId, StockViewModel_New.StockHeaderId, StockViewModel_New.DocTypeId, StockViewModel_New.SiteId, StockViewModel_New.DivisionId);

                if (StockViewModel_New.StockHeaderExist == 0 || StockViewModel_New.StockHeaderExist == null)
                {
                    if (StockHeader == null)
                    {
                        StockHeader H = new StockHeader();

                        H.DocHeaderId = StockViewModel_New.DocHeaderId;
                        H.DocTypeId = StockViewModel_New.DocTypeId;
                        H.DocDate = StockViewModel_New.StockHeaderDocDate;
                        H.DocNo = StockViewModel_New.DocNo;
                        H.DivisionId = StockViewModel_New.DivisionId;
                        H.SiteId = StockViewModel_New.SiteId;
                        H.CurrencyId = StockViewModel_New.CurrencyId;
                        H.PersonId = StockViewModel_New.PersonId;
                        H.ProcessId = StockViewModel_New.HeaderProcessId;
                        H.FromGodownId = StockViewModel_New.HeaderFromGodownId;
                        H.GodownId = StockViewModel_New.HeaderGodownId;
                        H.Remark = StockViewModel_New.Remark;
                        H.Status = StockViewModel_New.Status;
                        H.CreatedBy = StockViewModel_New.CreatedBy;
                        H.CreatedDate = StockViewModel_New.CreatedDate;
                        H.ModifiedBy = StockViewModel_New.ModifiedBy;
                        H.ModifiedDate = StockViewModel_New.ModifiedDate;

                        _stockHeaderService.Create(H);

                        StockHeader = H;
                    }
                    else
                    {
                        StockHeader.DocHeaderId = StockViewModel_New.DocHeaderId;
                        StockHeader.DocTypeId = StockViewModel_New.DocTypeId;
                        StockHeader.DocDate = StockViewModel_New.StockHeaderDocDate;
                        StockHeader.DocNo = StockViewModel_New.DocNo;
                        StockHeader.DivisionId = StockViewModel_New.DivisionId;
                        StockHeader.SiteId = StockViewModel_New.SiteId;
                        StockHeader.CurrencyId = StockViewModel_New.CurrencyId;
                        StockHeader.PersonId = StockViewModel_New.PersonId;
                        StockHeader.ProcessId = StockViewModel_New.HeaderProcessId;
                        StockHeader.FromGodownId = StockViewModel_New.HeaderFromGodownId;
                        StockHeader.GodownId = StockViewModel_New.HeaderGodownId;
                        StockHeader.Remark = StockViewModel_New.Remark;
                        StockHeader.Status = StockViewModel_New.Status;
                        StockHeader.CreatedBy = StockViewModel_New.CreatedBy;
                        StockHeader.CreatedDate = StockViewModel_New.CreatedDate;
                        StockHeader.ModifiedBy = StockViewModel_New.ModifiedBy;
                        StockHeader.ModifiedDate = StockViewModel_New.ModifiedDate;

                        _stockHeaderService.Update(StockHeader);
                    }
                }
            }
            else
            {
                StockHeader = _stockHeaderService.FindByDocHeader(StockViewModel_Old.DocHeaderId, StockViewModel_Old.StockHeaderId, StockViewModel_Old.DocTypeId, StockViewModel_Old.SiteId, StockViewModel_Old.DivisionId);
            }


            if (StockViewModel_Old != null)
            {
                Stock Stock_Old = Find(StockViewModel_Old.StockHeaderId, StockViewModel_Old.ProductId, StockViewModel_Old.StockDocDate, StockViewModel_Old.Dimension1Id, StockViewModel_Old.Dimension2Id, StockViewModel_Old.ProcessId, StockViewModel_Old.LotNo, StockViewModel_Old.GodownId, StockViewModel_Old.CostCenterId);

                if (Stock_Old != null)
                {
                    Stock_Old.Qty_Iss = Stock_Old.Qty_Iss - StockViewModel_Old.Qty_Iss;
                    Stock_Old.Qty_Rec = Stock_Old.Qty_Rec - StockViewModel_Old.Qty_Rec;
                    Stock_Old.Rate = StockViewModel_Old.Rate;
                    Stock_Old.ExpiryDate = StockViewModel_Old.ExpiryDate;
                    Stock_Old.Specification = StockViewModel_Old.Specification;

                    Update(Stock_Old);

                    //if (Stock_Old.Qty_Iss == 0 && Stock_Old.Qty_Rec == 0) { Delete(Stock_Old); }
                    //else { Update(Stock_Old); }

                    StockBalance StockBalance_Old = FindStockBalance(StockViewModel_Old.ProductId, StockViewModel_Old.Dimension1Id, StockViewModel_Old.Dimension2Id, StockViewModel_Old.ProcessId, StockViewModel_Old.LotNo, StockViewModel_Old.GodownId, StockViewModel_Old.CostCenterId);

                    if (StockBalance_Old != null)
                    {
                        StockBalance_Old.Qty = StockBalance_Old.Qty - StockViewModel_Old.Qty_Rec;
                        StockBalance_Old.Qty = StockBalance_Old.Qty + StockViewModel_Old.Qty_Iss;

                        if (StockBalance_Old.Qty == 0) { _stockBalanceService.Delete(StockBalance_Old); }
                        else { _stockBalanceService.Update(StockBalance_Old); }
                    }
                }


            }

            if (StockViewModel_New != null)
            {
                Stock Stock_New;

                if (StockHeader != null)
                {
                    Stock_New = Find(StockHeader.StockHeaderId, StockViewModel_New.ProductId, StockViewModel_New.StockDocDate, StockViewModel_New.Dimension1Id, StockViewModel_New.Dimension2Id, StockViewModel_New.ProcessId, StockViewModel_New.LotNo, StockViewModel_New.GodownId, StockViewModel_New.CostCenterId);
                }
                else
                {
                    Stock_New = null;
                }

                if (Stock_New == null)
                {
                    Stock L = new Stock();

                    L.DocDate = StockViewModel_New.StockDocDate;
                    L.ProductId = StockViewModel_New.ProductId;
                    L.ProcessId = StockViewModel_New.ProcessId;
                    L.GodownId = StockViewModel_New.GodownId;
                    L.LotNo = StockViewModel_New.LotNo;
                    L.CostCenterId = StockViewModel_New.CostCenterId;
                    L.Qty_Iss = StockViewModel_New.Qty_Iss;
                    L.Qty_Rec = StockViewModel_New.Qty_Rec;
                    L.Rate = StockViewModel_New.Rate;
                    L.ExpiryDate = StockViewModel_New.ExpiryDate;
                    L.Specification = StockViewModel_New.Specification;
                    L.Dimension1Id = StockViewModel_New.Dimension1Id;
                    L.Dimension2Id = StockViewModel_New.Dimension2Id;
                    L.CreatedBy = StockViewModel_New.CreatedBy;
                    L.CreatedDate = StockViewModel_New.CreatedDate;
                    L.ModifiedBy = StockViewModel_New.ModifiedBy;
                    L.ModifiedDate = StockViewModel_New.ModifiedDate;


                    if (StockHeader != null)
                    {
                        L.StockHeaderId = StockHeader.StockHeaderId;
                    }

                    Create(L);
                }
                else
                {
                    Stock_New.Qty_Iss = Stock_New.Qty_Iss + StockViewModel_New.Qty_Iss;
                    Stock_New.Qty_Rec = Stock_New.Qty_Rec + StockViewModel_New.Qty_Rec;
                    Stock_New.Rate = StockViewModel_New.Rate;
                    Stock_New.ExpiryDate = StockViewModel_New.ExpiryDate;
                    Stock_New.Specification = StockViewModel_New.Specification;
                    Stock_New.ModifiedBy = StockViewModel_New.ModifiedBy;
                    Stock_New.ModifiedDate = StockViewModel_New.ModifiedDate;

                    Update(Stock_New);
                }

                StockBalance StockBalance_New = FindStockBalance(StockViewModel_New.ProductId, StockViewModel_New.Dimension1Id, StockViewModel_New.Dimension2Id, StockViewModel_New.ProcessId, StockViewModel_New.LotNo, StockViewModel_New.GodownId, StockViewModel_New.CostCenterId);

                if (StockBalance_New == null)
                {

                    StockBalance Sb = new StockBalance();

                    Sb.ProductId = StockViewModel_New.ProductId;
                    Sb.Dimension1Id = StockViewModel_New.Dimension1Id;
                    Sb.Dimension2Id = StockViewModel_New.Dimension2Id;
                    Sb.ProcessId = StockViewModel_New.ProcessId;
                    Sb.GodownId = StockViewModel_New.GodownId;
                    Sb.CostCenterId = StockViewModel_New.CostCenterId;
                    Sb.LotNo = StockViewModel_New.LotNo;
                    if (StockViewModel_New.Qty_Iss != 0) { Sb.Qty = StockViewModel_New.Qty_Iss; }
                    if (StockViewModel_New.Qty_Rec != 0) { Sb.Qty = StockViewModel_New.Qty_Rec; }

                    _stockBalanceService.Create(Sb);
                }
                else
                {
                    StockBalance_New.Qty = StockBalance_New.Qty + StockViewModel_New.Qty_Rec;
                    StockBalance_New.Qty = StockBalance_New.Qty - StockViewModel_New.Qty_Iss;

                    _stockBalanceService.Update(StockBalance_New);
                }

            }

            return ErrorText;
        }

        public string StockPost(ref StockViewModel StockViewModel)
        {
            string ErrorText = "";

            if (StockViewModel.StockHeaderId == 0)
            {
                StockHeader H = new StockHeader();

                H.DocHeaderId = StockViewModel.DocHeaderId;
                H.DocTypeId = StockViewModel.DocTypeId;
                H.DocDate = StockViewModel.StockHeaderDocDate;
                H.DocNo = StockViewModel.DocNo;
                H.DivisionId = StockViewModel.DivisionId;
                H.SiteId = StockViewModel.SiteId;
                H.CurrencyId = StockViewModel.CurrencyId;
                H.PersonId = StockViewModel.PersonId;
                H.ProcessId = StockViewModel.HeaderProcessId;
                H.FromGodownId = StockViewModel.HeaderFromGodownId;
                H.GodownId = StockViewModel.HeaderGodownId;
                H.Remark = StockViewModel.Remark;
                H.Status = StockViewModel.Status;
                H.CreatedBy = StockViewModel.CreatedBy;
                H.CreatedDate = StockViewModel.CreatedDate;
                H.ModifiedBy = StockViewModel.ModifiedBy;
                H.ModifiedDate = StockViewModel.ModifiedDate;

                _stockHeaderService.Create(H);

                StockViewModel.StockHeaderId = H.StockHeaderId;
            }



            if (StockViewModel.StockId <= 0)
            {
                Stock L = new Stock();

                if (StockViewModel.StockHeaderId != 0 && StockViewModel.StockHeaderId != -1)
                {
                    L.StockHeaderId = StockViewModel.StockHeaderId;
                }

                L.StockId = StockViewModel.StockId;
                L.DocDate = StockViewModel.StockDocDate;
                L.ProductId = StockViewModel.ProductId;
                L.ProductUidId = StockViewModel.ProductUidId;
                L.ProcessId = StockViewModel.ProcessId;
                L.GodownId = StockViewModel.GodownId;
                L.LotNo = StockViewModel.LotNo;
                L.CostCenterId = StockViewModel.CostCenterId;
                L.Qty_Iss = StockViewModel.Qty_Iss;
                L.Qty_Rec = StockViewModel.Qty_Rec;
                L.Rate = StockViewModel.Rate;
                L.ExpiryDate = StockViewModel.ExpiryDate;
                L.Specification = StockViewModel.Specification;
                L.Dimension1Id = StockViewModel.Dimension1Id;
                L.Dimension2Id = StockViewModel.Dimension2Id;
                L.CreatedBy = StockViewModel.CreatedBy;
                L.CreatedDate = StockViewModel.CreatedDate;
                L.ModifiedBy = StockViewModel.ModifiedBy;
                L.ModifiedDate = StockViewModel.ModifiedDate;

                L.ObjectState = Model.ObjectState.Added;
                Create(L);

                //new StockBalanceService(_unitOfWork).UpdateStockBalance(L);


                StockBalance StockBalance = _stockBalanceService.Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                if (StockBalance == null)
                {
                    StockBalance StockBalance_NewRecord = new StockBalance();

                    StockBalance_NewRecord.ProductId = L.ProductId;
                    StockBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                    StockBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                    StockBalance_NewRecord.ProcessId = L.ProcessId;
                    StockBalance_NewRecord.GodownId = L.GodownId;
                    StockBalance_NewRecord.CostCenterId = L.CostCenterId;
                    StockBalance_NewRecord.LotNo = L.LotNo;
                    if (L.Qty_Iss != 0) { StockBalance_NewRecord.Qty = -L.Qty_Iss; }
                    if (L.Qty_Rec != 0) { StockBalance_NewRecord.Qty = L.Qty_Rec; }

                    _stockBalanceService.Create(StockBalance_NewRecord);
                }
                else
                {
                    StockBalance.Qty = StockBalance.Qty - L.Qty_Iss;
                    StockBalance.Qty = StockBalance.Qty + L.Qty_Rec;

                    _stockBalanceService.Update(StockBalance);
                }

                StockViewModel.StockId = L.StockId;

            }
            else
            {
                Stock L = Find(StockViewModel.StockId);

                //To Rollback Chenges in Stock Balance done by old entry.
                Stock Temp = new Stock();
                Temp.ProductId = L.ProductId;
                Temp.Dimension1Id = L.Dimension1Id;
                Temp.Dimension2Id = L.Dimension2Id;
                Temp.ProcessId = L.ProcessId;
                Temp.GodownId = L.GodownId;
                Temp.CostCenterId = L.CostCenterId;
                Temp.LotNo = L.LotNo;
                Temp.Qty_Iss = L.Qty_Iss;
                Temp.Qty_Rec = L.Qty_Rec;
                //new StockBalanceService(_unitOfWork).UpdateStockBalance(Temp);
                ///////////////////////////////////
                StockBalance StockBalance_Old = _stockBalanceService.Find(Temp.ProductId, Temp.Dimension1Id, Temp.Dimension2Id, Temp.ProcessId, Temp.LotNo, Temp.GodownId, Temp.CostCenterId);


                L.DocDate = StockViewModel.StockDocDate;
                L.ProductId = StockViewModel.ProductId;
                L.ProcessId = StockViewModel.ProcessId;
                L.GodownId = StockViewModel.GodownId;
                L.LotNo = StockViewModel.LotNo;
                L.CostCenterId = StockViewModel.CostCenterId;
                L.Qty_Iss = StockViewModel.Qty_Iss;
                L.Qty_Rec = StockViewModel.Qty_Rec;
                L.Rate = StockViewModel.Rate;
                L.ExpiryDate = StockViewModel.ExpiryDate;
                L.Specification = StockViewModel.Specification;
                L.Dimension1Id = StockViewModel.Dimension1Id;
                L.Dimension2Id = StockViewModel.Dimension2Id;
                L.CreatedBy = StockViewModel.CreatedBy;
                L.CreatedDate = StockViewModel.CreatedDate;
                L.ModifiedBy = StockViewModel.ModifiedBy;
                L.ModifiedDate = StockViewModel.ModifiedDate;

                Update(L);

                //new StockBalanceService(_unitOfWork).UpdateStockBalance(L);


                StockBalance StockBalance_New = _stockBalanceService.Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                if (StockBalance_New != null)
                {
                    if (StockBalance_Old.StockBalanceId != StockBalance_New.StockBalanceId)
                    {
                        if (StockBalance_Old != null)
                        {
                            StockBalance_Old.Qty = StockBalance_Old.Qty - Temp.Qty_Rec;
                            StockBalance_Old.Qty = StockBalance_Old.Qty + Temp.Qty_Iss;

                            _stockBalanceService.Update(StockBalance_New);
                        }

                        if (StockBalance_New == null)
                        {
                            StockBalance StockBalance_NewRecord = new StockBalance();

                            StockBalance_NewRecord.ProductId = L.ProductId;
                            StockBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                            StockBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                            StockBalance_NewRecord.ProcessId = L.ProcessId;
                            StockBalance_NewRecord.GodownId = L.GodownId;
                            StockBalance_NewRecord.CostCenterId = L.CostCenterId;
                            StockBalance_NewRecord.LotNo = L.LotNo;
                            if (L.Qty_Iss != 0) { StockBalance_NewRecord.Qty = -L.Qty_Iss; }
                            if (L.Qty_Rec != 0) { StockBalance_NewRecord.Qty = L.Qty_Rec; }

                            _stockBalanceService.Create(StockBalance_NewRecord);
                        }
                        else
                        {
                            StockBalance_New.Qty = StockBalance_New.Qty - L.Qty_Iss;
                            StockBalance_New.Qty = StockBalance_New.Qty + L.Qty_Rec;

                            _stockBalanceService.Update(StockBalance_New);
                        }
                    }
                    else
                    {
                        StockBalance_New.Qty = StockBalance_New.Qty + Temp.Qty_Iss - L.Qty_Iss;
                        StockBalance_New.Qty = StockBalance_New.Qty - Temp.Qty_Rec + L.Qty_Rec;

                        _stockBalanceService.Update(StockBalance_New);
                    }
                }

                StockViewModel.StockId = L.StockId;

            }

            return ErrorText;
        }





        public string StockPostDB(ref StockViewModel StockViewModel)
        {
            string ErrorText = "";

            if (StockViewModel.StockHeaderId == 0)
            {
                StockHeader H = new StockHeader();

                H.DocHeaderId = StockViewModel.DocHeaderId;
                H.DocTypeId = StockViewModel.DocTypeId;
                H.DocDate = StockViewModel.StockHeaderDocDate;
                H.DocNo = StockViewModel.DocNo;
                H.DivisionId = StockViewModel.DivisionId;
                H.SiteId = StockViewModel.SiteId;
                H.CurrencyId = StockViewModel.CurrencyId;
                H.PersonId = StockViewModel.PersonId;
                H.ProcessId = StockViewModel.HeaderProcessId;
                H.FromGodownId = StockViewModel.HeaderFromGodownId;
                H.GodownId = StockViewModel.HeaderGodownId;
                H.Remark = StockViewModel.HeaderRemark;
                H.Status = StockViewModel.Status;
                H.CreatedBy = StockViewModel.CreatedBy;
                H.CreatedDate = StockViewModel.CreatedDate;
                H.ModifiedBy = StockViewModel.ModifiedBy;
                H.ModifiedDate = StockViewModel.ModifiedDate;
                H.ObjectState = Model.ObjectState.Added;

                _stockHeaderService.Create(H);

                StockViewModel.StockHeaderId = H.StockHeaderId;
            }



            if (StockViewModel.StockId <= 0)
            {
                Stock L = new Stock();

                if (StockViewModel.StockHeaderId != 0 && StockViewModel.StockHeaderId != -1)
                {
                    L.StockHeaderId = StockViewModel.StockHeaderId;
                }

                L.StockId = StockViewModel.StockId;
                L.DocDate = StockViewModel.StockHeaderDocDate;
                L.ProductId = StockViewModel.ProductId;
                L.ProcessId = StockViewModel.ProcessId;
                L.GodownId = StockViewModel.GodownId;
                L.LotNo = StockViewModel.LotNo;
                L.ProductUidId = StockViewModel.ProductUidId;
                L.CostCenterId = StockViewModel.CostCenterId;
                L.Qty_Iss = StockViewModel.Qty_Iss;
                L.Qty_Rec = StockViewModel.Qty_Rec;
                L.Rate = StockViewModel.Rate;
                L.ExpiryDate = StockViewModel.ExpiryDate;
                L.Specification = StockViewModel.Specification;
                L.Dimension1Id = StockViewModel.Dimension1Id;
                L.Dimension2Id = StockViewModel.Dimension2Id;
                L.Remark = StockViewModel.Remark;
                L.CreatedBy = StockViewModel.CreatedBy;
                L.CreatedDate = StockViewModel.CreatedDate;
                L.ModifiedBy = StockViewModel.ModifiedBy;
                L.ModifiedDate = StockViewModel.ModifiedDate;

                L.ObjectState = Model.ObjectState.Added;

                Create(L);

                //new StockBalanceService(_unitOfWork).UpdateStockBalance(L);


                StockBalance StockBalance = _stockBalanceService.Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                if (StockBalance == null)
                {
                    StockBalance StockBalance_NewRecord = new StockBalance();

                    StockBalance_NewRecord.ProductId = L.ProductId;
                    StockBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                    StockBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                    StockBalance_NewRecord.ProcessId = L.ProcessId;
                    StockBalance_NewRecord.GodownId = L.GodownId;
                    StockBalance_NewRecord.CostCenterId = L.CostCenterId;
                    StockBalance_NewRecord.LotNo = L.LotNo;
                    if (L.Qty_Iss != 0) { StockBalance_NewRecord.Qty = -L.Qty_Iss; }
                    if (L.Qty_Rec != 0) { StockBalance_NewRecord.Qty = L.Qty_Rec; }
                    StockBalance_NewRecord.ObjectState = Model.ObjectState.Added;

                    _stockBalanceService.Create(StockBalance_NewRecord);
                }
                else
                {
                    StockBalance.Qty = StockBalance.Qty - L.Qty_Iss;
                    StockBalance.Qty = StockBalance.Qty + L.Qty_Rec;
                    StockBalance.ObjectState = Model.ObjectState.Modified;

                    _stockBalanceService.Update(StockBalance);
                }

                StockViewModel.StockId = L.StockId;
            }
            else
            {
                Stock L = Find(StockViewModel.StockId);

                //To Rollback Chenges in Stock Balance done by old entry.
                Stock Temp = new Stock();
                Temp.ProductId = L.ProductId;
                Temp.Dimension1Id = L.Dimension1Id;
                Temp.Dimension2Id = L.Dimension2Id;
                Temp.ProcessId = L.ProcessId;
                Temp.GodownId = L.GodownId;
                Temp.CostCenterId = L.CostCenterId;
                Temp.LotNo = L.LotNo;
                Temp.Qty_Iss = L.Qty_Iss;
                Temp.Qty_Rec = L.Qty_Rec;




                L.DocDate = StockViewModel.StockDocDate;
                L.ProductId = StockViewModel.ProductId;
                L.ProcessId = StockViewModel.ProcessId;
                L.GodownId = StockViewModel.GodownId;
                L.LotNo = StockViewModel.LotNo;
                L.CostCenterId = StockViewModel.CostCenterId;
                L.Qty_Iss = StockViewModel.Qty_Iss;
                L.Qty_Rec = StockViewModel.Qty_Rec;
                L.Rate = StockViewModel.Rate;
                L.ExpiryDate = StockViewModel.ExpiryDate;
                L.Specification = StockViewModel.Specification;
                L.Dimension1Id = StockViewModel.Dimension1Id;
                L.Dimension2Id = StockViewModel.Dimension2Id;
                L.Remark = StockViewModel.Remark;
                L.CreatedBy = StockViewModel.CreatedBy;
                L.CreatedDate = StockViewModel.CreatedDate;
                L.ModifiedBy = StockViewModel.ModifiedBy;
                L.ModifiedDate = StockViewModel.ModifiedDate;
                L.ObjectState = Model.ObjectState.Modified;

                Update(L);

                //new StockBalanceService(_unitOfWork).UpdateStockBalance(L);


                StockBalance StockBalance_Old = _stockBalanceService.Find(Temp.ProductId, Temp.Dimension1Id, Temp.Dimension2Id, Temp.ProcessId, Temp.LotNo, Temp.GodownId, Temp.CostCenterId);

                if (StockBalance_Old != null)
                {
                    StockBalance_Old.Qty = StockBalance_Old.Qty - Temp.Qty_Rec;
                    StockBalance_Old.Qty = StockBalance_Old.Qty + Temp.Qty_Iss;
                    StockBalance_Old.ObjectState = Model.ObjectState.Modified;

                    _stockBalanceService.Update(StockBalance_Old);
                }


                StockBalance StockBalance_New = _stockBalanceService.Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);


                if (StockBalance_New == null)
                {
                    StockBalance StockBalance_NewRecord = new StockBalance();

                    StockBalance_NewRecord.ProductId = L.ProductId;
                    StockBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                    StockBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                    StockBalance_NewRecord.ProcessId = L.ProcessId;
                    StockBalance_NewRecord.GodownId = L.GodownId;
                    StockBalance_NewRecord.CostCenterId = L.CostCenterId;
                    StockBalance_NewRecord.LotNo = L.LotNo;
                    if (L.Qty_Iss != 0) { StockBalance_NewRecord.Qty = -L.Qty_Iss; }
                    if (L.Qty_Rec != 0) { StockBalance_NewRecord.Qty = L.Qty_Rec; }
                    StockBalance_NewRecord.ObjectState = Model.ObjectState.Added;

                    _stockBalanceService.Create(StockBalance_NewRecord);
                }
                else
                {
                    StockBalance_New.Qty = StockBalance_New.Qty - L.Qty_Iss;
                    StockBalance_New.Qty = StockBalance_New.Qty + L.Qty_Rec;
                    StockBalance_New.ObjectState = Model.ObjectState.Modified;

                    _stockBalanceService.Update(StockBalance_New);
                }

                StockViewModel.StockId = L.StockId;

            }

            return ErrorText;
        }




        public void DeleteStock(int StockId)
        {
            Stock Stock = Find(StockId);

            StockBalance StockBalance = _stockBalanceService.Find(Stock.ProductId, Stock.Dimension1Id, Stock.Dimension2Id, Stock.ProcessId, Stock.LotNo, Stock.GodownId, Stock.CostCenterId);

            if (StockBalance != null)
            {
                StockBalance.Qty = StockBalance.Qty + Stock.Qty_Iss;
                StockBalance.Qty = StockBalance.Qty - Stock.Qty_Rec;

                _stockBalanceService.Update(StockBalance);
            }

            Delete(StockId);
        }

        public void DeleteStockDB(int StockId, bool IsDBbased)
        {
            Stock Stock = Find(StockId);

            StockBalance StockBalance = (from p in _unitOfWork.Repository<StockBalance>().Instance
                                         where p.ProductId == Stock.ProductId &&
                                       p.Dimension1Id == Stock.Dimension1Id &&
                                       p.Dimension2Id == Stock.Dimension2Id &&
                                       p.ProcessId == Stock.ProcessId &&
                                       p.LotNo == Stock.LotNo &&
                                       p.GodownId == Stock.GodownId &&
                                       p.CostCenterId == Stock.CostCenterId
                                         select p).FirstOrDefault();

            if (StockBalance != null)
            {
                StockBalance.Qty = StockBalance.Qty + Stock.Qty_Iss;
                StockBalance.Qty = StockBalance.Qty - Stock.Qty_Rec;
                StockBalance.ObjectState = Model.ObjectState.Modified;

                _stockBalanceService.Update(StockBalance);
            }
            Stock.ObjectState = Model.ObjectState.Deleted;

            Delete(StockId);
        }

        public void DeleteStockMultiple(List<int> StockId)
        {

            var StockIdArray = StockId.ToArray();

            var Stock = (from p in _StockRepository.Instance
                         where StockIdArray.Contains(p.StockId)
                         select p).ToList();


            var GroupedStock = (from p in Stock
                                group p by new
                                {
                                    p.ProductId,
                                    p.Dimension1Id,
                                    p.Dimension2Id,
                                    p.ProcessId,
                                    p.LotNo,
                                    p.GodownId,
                                    p.CostCenterId
                                } into g
                                select g).ToList();

            foreach (var item in GroupedStock)
            {
                StockBalance StockBalance = (from p in _unitOfWork.Repository<StockBalance>().Instance
                                             where p.ProductId == item.Key.ProductId &&
                                           p.Dimension1Id == item.Key.Dimension1Id &&
                                           p.Dimension2Id == item.Key.Dimension2Id &&
                                           p.ProcessId == item.Key.ProcessId &&
                                           p.LotNo == item.Key.LotNo &&
                                           p.GodownId == item.Key.GodownId &&
                                           p.CostCenterId == item.Key.CostCenterId
                                             select p).FirstOrDefault();

                if (StockBalance != null)
                {
                    StockBalance.Qty = StockBalance.Qty + item.Sum(m => m.Qty_Iss);
                    StockBalance.Qty = StockBalance.Qty - item.Sum(m => m.Qty_Rec);
                    StockBalance.ObjectState = Model.ObjectState.Modified;

                    _stockBalanceService.Update(StockBalance);
                }


                foreach (var IStock in item)
                {
                    Delete(IStock);
                }
            }


        }

        public IEnumerable<Stock> GetStockForStockHeaderId(int StockHeaderId)
        {
            var temp = (from L in _StockRepository.Instance
                        where L.StockHeaderId == StockHeaderId
                        select L).ToList();

            return temp;
        }
    }
}
