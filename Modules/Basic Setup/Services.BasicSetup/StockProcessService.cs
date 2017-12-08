using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;
using Models.BasicSetup.ViewModels;

namespace Services.BasicSetup
{
    public interface IStockProcessService : IDisposable
    {
        StockProcess Create(StockProcess pt);
        void Delete(int id);
        void Update(StockProcess pt);
        StockProcess Find(int id);
        void DeleteStockProcessForStockHeader(int StockHeaderId);
        void DeleteStockProcessForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId);
        void DeleteStockProcess(int StockProcessId);
        void DeleteStockProcessDB(int StockProcessId);
        void DeleteStockProcessDBMultiple(List<int> StockProcessId);
        string StockProcessPostDB(ref StockProcessViewModel StockProcessViewModel);
    }

    public class StockProcessService : IStockProcessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<StockProcess> _StockProcessRepository;
        private readonly IStockHeaderService _stockHeaderService;
        private readonly IStockProcessBalanceService _stockProcessBalanceService;

        public StockProcessService(IUnitOfWork unitOfWork, IRepository<StockProcess> StockProcessRepo, IStockHeaderService stockHeaderService,
            IStockProcessBalanceService stockProcBalService)
        {
            _unitOfWork = unitOfWork;
            _StockProcessRepository = StockProcessRepo;
            _stockHeaderService = stockHeaderService;
            _stockProcessBalanceService = stockProcBalService;
        }

        public StockProcess GetStockProcess(int pt)
        {
            return _StockProcessRepository.Find(pt);
        }

        public StockProcess Create(StockProcess pt)
        {
            pt.ObjectState = ObjectState.Added;
            _StockProcessRepository.Add(pt);
            return pt;
        }

        public StockProcess Find(int id)
        {
            return _StockProcessRepository.Find(id);
        }

        public void Delete(int id)
        {
            _StockProcessRepository.Delete(id);
        }

        public void Delete(StockProcess pt)
        {
            _StockProcessRepository.Delete(pt);
        }

        public void Update(StockProcess pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _StockProcessRepository.Update(pt);
        }

        public IEnumerable<StockProcess> GetStockProcessList()
        {
            var pt = _StockProcessRepository.Query().Get();

            return pt;
        }

        public StockProcess Add(StockProcess pt)
        {
            _StockProcessRepository.Insert(pt);
            return pt;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<StockProcess>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StockProcess> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        //public StockProcess FindWithUnitOfWork(int StockHeaderId, int ProductId, DateTime DocDate, int? Dimension1Id, int? Dimension2Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId)
        //{
        //    return _unitOfWork.Repository<StockProcess>().Query().Get().Where(m => m.StockHeaderId == StockHeaderId && m.ProductId == ProductId && m.DocDate == DocDate
        //                && m.Dimension1Id == Dimension1Id && m.Dimension2Id == Dimension2Id && m.ProcessId == ProcessId && m.LotNo == LotNo && m.GodownId == GodownId
        //                 && m.CostCenterId == CostCenterId
        //                ).FirstOrDefault();
        //}


        public StockProcess Find(int StockHeaderId, int ProductId, DateTime DocDate, int? Dimension1Id, int? Dimension2Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId)
        {
            StockProcess StockProcess = (from L in _StockProcessRepository.Instance
                                         where L.StockHeaderId == StockHeaderId && L.ProductId == ProductId && L.DocDate == DocDate && L.Dimension1Id == Dimension1Id &&
                                               L.Dimension2Id == Dimension2Id && L.ProcessId == ProcessId && L.LotNo == LotNo && L.GodownId == GodownId && L.CostCenterId == CostCenterId
                                         select L).FirstOrDefault();

            return StockProcess;
        }

        public StockProcessBalance FindStockProcessBalance(int ProductId, int? Dimension1Id, int? Dimension2Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId)
        {
            StockProcessBalance StockProcessbalance = (from L in _unitOfWork.Repository<StockProcessBalance>().Instance
                                                       where L.ProductId == ProductId && L.Dimension1Id == Dimension1Id && L.Dimension2Id == Dimension2Id && L.ProcessId == ProcessId && L.LotNo == LotNo && L.GodownId == GodownId && L.CostCenterId == CostCenterId
                                                       select L).FirstOrDefault();
            return StockProcessbalance;
        }

        public void DeleteStockProcessForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId)
        {
            IEnumerable<StockProcess> StockProcessList = (from L in _StockProcessRepository.Instance
                                                          join H in _unitOfWork.Repository<StockHeader>().Instance on L.StockHeaderId equals H.StockHeaderId into StockHeaderTable
                                                          from StockHeaderTab in StockHeaderTable.DefaultIfEmpty()
                                                          where StockHeaderTab.DocHeaderId == DocHeaderId && StockHeaderTab.DocTypeId == DocTypeId && StockHeaderTab.SiteId == SiteId && StockHeaderTab.DivisionId == DivisionId
                                                          select L).ToList();

            if (StockProcessList != null && StockProcessList.Count() > 0)
            {
                int i = 0;
                foreach (StockProcess item in StockProcessList)
                {
                    try
                    {

                        StockProcessBalance StockProcessbalance = (from L in _unitOfWork.Repository<StockProcessBalance>().Instance
                                                                   where L.ProductId == item.ProductId && L.Dimension1Id == item.Dimension1Id && L.Dimension2Id == item.Dimension2Id && L.ProcessId == item.ProcessId &&
                                                                   L.LotNo == item.LotNo && L.GodownId == item.GodownId && L.CostCenterId == item.CostCenterId
                                                                   select L).FirstOrDefault();

                        if (StockProcessbalance != null)
                        {
                            StockProcessbalance.Qty = StockProcessbalance.Qty - item.Qty_Rec;
                            StockProcessbalance.Qty = StockProcessbalance.Qty + item.Qty_Iss;

                            if (StockProcessbalance.Qty == 0)
                            {
                                item.ObjectState = Model.ObjectState.Deleted;
                                _unitOfWork.Repository<StockProcessBalance>().Delete(StockProcessbalance);
                            }
                            else
                            {
                                i = i + 1;
                                StockProcessbalance.ObjectState = Model.ObjectState.Modified;
                                _unitOfWork.Repository<StockProcessBalance>().Add(StockProcessbalance);

                            }
                        }

                        Delete(item);
                    }
                    catch (Exception e)
                    {
                        string str = e.Message;
                    }
                }
                _stockHeaderService.Delete(StockProcessList.FirstOrDefault().StockHeaderId);
            }
        }


        public void DeleteStockProcessForStockHeader(int StockHeaderId)
        {
            IEnumerable<StockProcess> StockProcessList = (from L in _StockProcessRepository.Instance
                                                          where L.StockHeaderId == StockHeaderId
                                                          select L).ToList();

            foreach (StockProcess item in StockProcessList)
            {
                Delete(item);
            }
        }

        public string StockProcessPost(ref StockProcessViewModel StockProcessViewModel)
        {
            string ErrorText = "";

            if (StockProcessViewModel.StockHeaderId == 0)
            {
                StockHeader H = new StockHeader();

                H.DocHeaderId = StockProcessViewModel.DocHeaderId;
                H.DocTypeId = StockProcessViewModel.DocTypeId;
                H.DocDate = StockProcessViewModel.StockHeaderDocDate;
                H.DocNo = StockProcessViewModel.DocNo;
                H.DivisionId = StockProcessViewModel.DivisionId;
                H.SiteId = StockProcessViewModel.SiteId;
                H.CurrencyId = StockProcessViewModel.CurrencyId;
                H.PersonId = StockProcessViewModel.PersonId;
                H.ProcessId = StockProcessViewModel.HeaderProcessId;
                H.FromGodownId = StockProcessViewModel.HeaderFromGodownId;
                H.GodownId = StockProcessViewModel.HeaderGodownId;
                H.Remark = StockProcessViewModel.Remark;
                H.Status = StockProcessViewModel.Status;
                H.CreatedBy = StockProcessViewModel.CreatedBy;
                H.CreatedDate = StockProcessViewModel.CreatedDate;
                H.ModifiedBy = StockProcessViewModel.ModifiedBy;
                H.ModifiedDate = StockProcessViewModel.ModifiedDate;

                _stockHeaderService.Create(H);

                StockProcessViewModel.StockHeaderId = H.StockHeaderId;
            }

            if (StockProcessViewModel.StockProcessId <= 0)
            {
                StockProcess L = new StockProcess();

                if (StockProcessViewModel.StockHeaderId != 0 && StockProcessViewModel.StockHeaderId != -1)
                {
                    L.StockHeaderId = StockProcessViewModel.StockHeaderId;
                }

                L.StockProcessId = StockProcessViewModel.StockProcessId;
                L.DocDate = StockProcessViewModel.StockProcessDocDate;
                L.ProductId = StockProcessViewModel.ProductId;
                L.ProcessId = StockProcessViewModel.ProcessId;
                L.GodownId = StockProcessViewModel.GodownId;
                L.LotNo = StockProcessViewModel.LotNo;
                L.CostCenterId = StockProcessViewModel.CostCenterId;
                L.Qty_Iss = StockProcessViewModel.Qty_Iss;
                L.Qty_Rec = StockProcessViewModel.Qty_Rec;
                L.Rate = StockProcessViewModel.Rate;
                L.ExpiryDate = StockProcessViewModel.ExpiryDate;
                L.Specification = StockProcessViewModel.Specification;
                L.Dimension1Id = StockProcessViewModel.Dimension1Id;
                L.Dimension2Id = StockProcessViewModel.Dimension2Id;
                L.CreatedBy = StockProcessViewModel.CreatedBy;
                L.CreatedDate = StockProcessViewModel.CreatedDate;
                L.ModifiedBy = StockProcessViewModel.ModifiedBy;
                L.ModifiedDate = StockProcessViewModel.ModifiedDate;
                L.ObjectState = Model.ObjectState.Added;
                Create(L);


                StockProcessBalance StockProcessBalance = _stockProcessBalanceService.Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                if (StockProcessBalance == null)
                {
                    StockProcessBalance StockProcessBalance_NewRecord = new StockProcessBalance();

                    StockProcessBalance_NewRecord.ProductId = L.ProductId;
                    StockProcessBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                    StockProcessBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                    StockProcessBalance_NewRecord.ProcessId = L.ProcessId;
                    StockProcessBalance_NewRecord.GodownId = L.GodownId;
                    StockProcessBalance_NewRecord.CostCenterId = L.CostCenterId;
                    StockProcessBalance_NewRecord.LotNo = L.LotNo;
                    if (L.Qty_Iss != 0) { StockProcessBalance_NewRecord.Qty = -L.Qty_Iss; }
                    if (L.Qty_Rec != 0) { StockProcessBalance_NewRecord.Qty = L.Qty_Rec; }

                    _stockProcessBalanceService.Create(StockProcessBalance_NewRecord);
                }
                else
                {
                    StockProcessBalance.Qty = StockProcessBalance.Qty - L.Qty_Iss;
                    StockProcessBalance.Qty = StockProcessBalance.Qty + L.Qty_Rec;

                    _stockProcessBalanceService.Update(StockProcessBalance);
                }

                StockProcessViewModel.StockProcessId = L.StockProcessId;
            }
            else
            {
                StockProcess L = Find(StockProcessViewModel.StockProcessId);

                //To Rollback Chenges in StockProcess Balance done by old entry.
                StockProcess Temp = new StockProcess();
                Temp.ProductId = L.ProductId;
                Temp.Dimension1Id = L.Dimension1Id;
                Temp.Dimension2Id = L.Dimension2Id;
                Temp.ProcessId = L.ProcessId;
                Temp.GodownId = L.GodownId;
                Temp.CostCenterId = L.CostCenterId;
                Temp.LotNo = L.LotNo;
                Temp.Qty_Iss = L.Qty_Iss;
                Temp.Qty_Rec = L.Qty_Rec;
                //new StockProcessBalanceService(_unitOfWork).UpdateStockProcessBalance(Temp);
                ///////////////////////////////////
                StockProcessBalance StockProcessBalance_Old = _stockProcessBalanceService.Find(Temp.ProductId, Temp.Dimension1Id, Temp.Dimension2Id, Temp.ProcessId, Temp.LotNo, Temp.GodownId, Temp.CostCenterId);


                L.DocDate = StockProcessViewModel.StockProcessDocDate;
                L.ProductId = StockProcessViewModel.ProductId;
                L.ProcessId = StockProcessViewModel.ProcessId;
                L.GodownId = StockProcessViewModel.GodownId;
                L.LotNo = StockProcessViewModel.LotNo;
                L.CostCenterId = StockProcessViewModel.CostCenterId;
                L.Qty_Iss = StockProcessViewModel.Qty_Iss;
                L.Qty_Rec = StockProcessViewModel.Qty_Rec;
                L.Rate = StockProcessViewModel.Rate;
                L.ExpiryDate = StockProcessViewModel.ExpiryDate;
                L.Specification = StockProcessViewModel.Specification;
                L.Dimension1Id = StockProcessViewModel.Dimension1Id;
                L.Dimension2Id = StockProcessViewModel.Dimension2Id;
                L.CreatedBy = StockProcessViewModel.CreatedBy;
                L.CreatedDate = StockProcessViewModel.CreatedDate;
                L.ModifiedBy = StockProcessViewModel.ModifiedBy;
                L.ModifiedDate = StockProcessViewModel.ModifiedDate;

                Update(L);

                //new StockProcessBalanceService(_unitOfWork).UpdateStockProcessBalance(L);


                StockProcessBalance StockProcessBalance_New = _stockProcessBalanceService.Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                if (StockProcessBalance_New != null)
                {

                    if (StockProcessBalance_Old.StockProcessBalanceId != StockProcessBalance_New.StockProcessBalanceId)
                    {
                        if (StockProcessBalance_Old != null)
                        {
                            StockProcessBalance_Old.Qty = StockProcessBalance_Old.Qty - Temp.Qty_Rec;
                            StockProcessBalance_Old.Qty = StockProcessBalance_Old.Qty + Temp.Qty_Iss;

                            _stockProcessBalanceService.Update(StockProcessBalance_New);
                        }

                        if (StockProcessBalance_New == null)
                        {
                            StockProcessBalance StockProcessBalance_NewRecord = new StockProcessBalance();

                            StockProcessBalance_NewRecord.ProductId = L.ProductId;
                            StockProcessBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                            StockProcessBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                            StockProcessBalance_NewRecord.ProcessId = L.ProcessId;
                            StockProcessBalance_NewRecord.GodownId = L.GodownId;
                            StockProcessBalance_NewRecord.CostCenterId = L.CostCenterId;
                            StockProcessBalance_NewRecord.LotNo = L.LotNo;
                            if (L.Qty_Iss != 0) { StockProcessBalance_NewRecord.Qty = -L.Qty_Iss; }
                            if (L.Qty_Rec != 0) { StockProcessBalance_NewRecord.Qty = L.Qty_Rec; }

                            _stockProcessBalanceService.Create(StockProcessBalance_NewRecord);
                        }
                        else
                        {
                            StockProcessBalance_New.Qty = StockProcessBalance_New.Qty - L.Qty_Iss;
                            StockProcessBalance_New.Qty = StockProcessBalance_New.Qty + L.Qty_Rec;

                            _stockProcessBalanceService.Update(StockProcessBalance_New);
                        }
                    }
                    else
                    {
                        StockProcessBalance_New.Qty = StockProcessBalance_New.Qty + Temp.Qty_Iss - L.Qty_Iss;
                        StockProcessBalance_New.Qty = StockProcessBalance_New.Qty - Temp.Qty_Rec + L.Qty_Rec;

                        _stockProcessBalanceService.Update(StockProcessBalance_New);
                    }
                }

                StockProcessViewModel.StockProcessId = L.StockProcessId;

            }

            return ErrorText;
        }









        public string StockProcessPostDB(ref StockProcessViewModel StockProcessViewModel)
        {
            string ErrorText = "";


            if (StockProcessViewModel.StockHeaderId == 0)
            {
                StockHeader H = new StockHeader();

                H.DocHeaderId = StockProcessViewModel.DocHeaderId;
                H.DocTypeId = StockProcessViewModel.DocTypeId;
                H.DocDate = StockProcessViewModel.StockHeaderDocDate;
                H.DocNo = StockProcessViewModel.DocNo;
                H.DivisionId = StockProcessViewModel.DivisionId;
                H.SiteId = StockProcessViewModel.SiteId;
                H.CurrencyId = StockProcessViewModel.CurrencyId;
                H.PersonId = StockProcessViewModel.PersonId;
                H.ProcessId = StockProcessViewModel.HeaderProcessId;
                H.FromGodownId = StockProcessViewModel.HeaderFromGodownId;
                H.GodownId = StockProcessViewModel.HeaderGodownId;
                H.Remark = StockProcessViewModel.HeaderRemark;
                H.Status = StockProcessViewModel.Status;
                H.CreatedBy = StockProcessViewModel.CreatedBy;
                H.CreatedDate = StockProcessViewModel.CreatedDate;
                H.ModifiedBy = StockProcessViewModel.ModifiedBy;
                H.ModifiedDate = StockProcessViewModel.ModifiedDate;
                H.ObjectState = Model.ObjectState.Added;

                _stockHeaderService.Create(H);

                StockProcessViewModel.StockHeaderId = H.StockHeaderId;
            }

            if (StockProcessViewModel.StockProcessId <= 0)
            {
                StockProcess L = new StockProcess();

                if (StockProcessViewModel.StockHeaderId != 0 && StockProcessViewModel.StockHeaderId != -1)
                {
                    L.StockHeaderId = StockProcessViewModel.StockHeaderId;
                }

                L.StockProcessId = StockProcessViewModel.StockProcessId;
                L.DocDate = StockProcessViewModel.StockHeaderDocDate;
                L.ProductId = StockProcessViewModel.ProductId;
                L.ProcessId = StockProcessViewModel.ProcessId;
                L.GodownId = StockProcessViewModel.GodownId;
                L.LotNo = StockProcessViewModel.LotNo;
                L.CostCenterId = StockProcessViewModel.CostCenterId;
                L.Qty_Iss = StockProcessViewModel.Qty_Iss;
                L.Qty_Rec = StockProcessViewModel.Qty_Rec;
                L.Rate = StockProcessViewModel.Rate;
                L.ProductUidId = StockProcessViewModel.ProductUidId;
                L.Remark = StockProcessViewModel.Remark;
                L.ExpiryDate = StockProcessViewModel.ExpiryDate;
                L.Specification = StockProcessViewModel.Specification;
                L.Dimension1Id = StockProcessViewModel.Dimension1Id;
                L.Dimension2Id = StockProcessViewModel.Dimension2Id;
                L.CreatedBy = StockProcessViewModel.CreatedBy;
                L.CreatedDate = StockProcessViewModel.CreatedDate;
                L.ModifiedBy = StockProcessViewModel.ModifiedBy;
                L.ModifiedDate = StockProcessViewModel.ModifiedDate;
                L.ObjectState = Model.ObjectState.Added;

                Create(L);


                StockProcessBalance StockProcessBalance = _stockProcessBalanceService.Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                if (StockProcessBalance == null)
                {
                    StockProcessBalance StockProcessBalance_NewRecord = new StockProcessBalance();

                    StockProcessBalance_NewRecord.ProductId = L.ProductId;
                    StockProcessBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                    StockProcessBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                    StockProcessBalance_NewRecord.ProcessId = L.ProcessId;
                    StockProcessBalance_NewRecord.GodownId = L.GodownId;
                    StockProcessBalance_NewRecord.CostCenterId = L.CostCenterId;
                    StockProcessBalance_NewRecord.LotNo = L.LotNo;
                    if (L.Qty_Iss != 0) { StockProcessBalance_NewRecord.Qty = -L.Qty_Iss; }
                    if (L.Qty_Rec != 0) { StockProcessBalance_NewRecord.Qty = L.Qty_Rec; }
                    StockProcessBalance_NewRecord.ObjectState = Model.ObjectState.Added;

                    _stockProcessBalanceService.Create(StockProcessBalance_NewRecord);
                }
                else
                {
                    StockProcessBalance.Qty = StockProcessBalance.Qty - L.Qty_Iss;
                    StockProcessBalance.Qty = StockProcessBalance.Qty + L.Qty_Rec;
                    StockProcessBalance.ObjectState = Model.ObjectState.Modified;

                    _stockProcessBalanceService.Update(StockProcessBalance);
                }

                StockProcessViewModel.StockProcessId = L.StockProcessId;
            }
            else
            {
                StockProcess L = Find(StockProcessViewModel.StockProcessId);

                //To Rollback Chenges in StockProcess Balance done by old entry.
                StockProcess Temp = new StockProcess();
                Temp.ProductId = L.ProductId;
                Temp.Dimension1Id = L.Dimension1Id;
                Temp.Dimension2Id = L.Dimension2Id;
                Temp.ProcessId = StockProcessViewModel.ProcessId;
                Temp.GodownId = L.GodownId;
                Temp.CostCenterId = L.CostCenterId;
                Temp.LotNo = L.LotNo;
                Temp.Qty_Iss = L.Qty_Iss;
                Temp.Qty_Rec = L.Qty_Rec;
                //new StockProcessBalanceService(_unitOfWork).UpdateStockProcessBalance(Temp);
                ///////////////////////////////////
                StockProcessBalance StockProcessBalance_Old = _stockProcessBalanceService.Find(Temp.ProductId, Temp.Dimension1Id, Temp.Dimension2Id, Temp.ProcessId, Temp.LotNo, Temp.GodownId, Temp.CostCenterId);


                L.DocDate = StockProcessViewModel.StockProcessDocDate;
                L.ProductId = StockProcessViewModel.ProductId;
                L.ProcessId = StockProcessViewModel.ProcessId;
                L.GodownId = StockProcessViewModel.GodownId;
                L.LotNo = StockProcessViewModel.LotNo;
                L.CostCenterId = StockProcessViewModel.CostCenterId;
                L.Qty_Iss = StockProcessViewModel.Qty_Iss;
                L.Qty_Rec = StockProcessViewModel.Qty_Rec;
                L.Rate = StockProcessViewModel.Rate;
                L.ExpiryDate = StockProcessViewModel.ExpiryDate;
                L.Specification = StockProcessViewModel.Specification;
                L.Dimension1Id = StockProcessViewModel.Dimension1Id;
                L.Dimension2Id = StockProcessViewModel.Dimension2Id;
                L.CreatedBy = StockProcessViewModel.CreatedBy;
                L.CreatedDate = StockProcessViewModel.CreatedDate;
                L.ModifiedBy = StockProcessViewModel.ModifiedBy;
                L.ModifiedDate = StockProcessViewModel.ModifiedDate;
                L.Remark = StockProcessViewModel.Remark;
                L.ObjectState = Model.ObjectState.Modified;

                Update(L);

                //new StockProcessBalanceService(_unitOfWork).UpdateStockProcessBalance(L);


                StockProcessBalance StockProcessBalance_New = _stockProcessBalanceService.Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                if (StockProcessBalance_New != null && StockProcessBalance_Old != null)
                {

                    if (StockProcessBalance_Old.StockProcessBalanceId != StockProcessBalance_New.StockProcessBalanceId)
                    {
                        if (StockProcessBalance_Old != null)
                        {
                            StockProcessBalance_Old.Qty = StockProcessBalance_Old.Qty - Temp.Qty_Rec;
                            StockProcessBalance_Old.Qty = StockProcessBalance_Old.Qty + Temp.Qty_Iss;
                            StockProcessBalance_Old.ObjectState = Model.ObjectState.Modified;

                            _stockProcessBalanceService.Update(StockProcessBalance_New);
                        }

                        if (StockProcessBalance_New == null)
                        {
                            StockProcessBalance StockProcessBalance_NewRecord = new StockProcessBalance();

                            StockProcessBalance_NewRecord.ProductId = L.ProductId;
                            StockProcessBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                            StockProcessBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                            StockProcessBalance_NewRecord.ProcessId = L.ProcessId;
                            StockProcessBalance_NewRecord.GodownId = L.GodownId;
                            StockProcessBalance_NewRecord.CostCenterId = L.CostCenterId;
                            StockProcessBalance_NewRecord.LotNo = L.LotNo;
                            if (L.Qty_Iss != 0) { StockProcessBalance_NewRecord.Qty = -L.Qty_Iss; }
                            if (L.Qty_Rec != 0) { StockProcessBalance_NewRecord.Qty = L.Qty_Rec; }
                            StockProcessBalance_NewRecord.ObjectState = Model.ObjectState.Added;

                            _stockProcessBalanceService.Create(StockProcessBalance_NewRecord);
                        }
                        else
                        {
                            StockProcessBalance_New.Qty = StockProcessBalance_New.Qty - L.Qty_Iss;
                            StockProcessBalance_New.Qty = StockProcessBalance_New.Qty + L.Qty_Rec;
                            StockProcessBalance_New.ObjectState = Model.ObjectState.Modified;

                            _stockProcessBalanceService.Update(StockProcessBalance_New);
                        }
                    }
                    else
                    {
                        StockProcessBalance_New.Qty = StockProcessBalance_New.Qty + Temp.Qty_Iss - L.Qty_Iss;
                        StockProcessBalance_New.Qty = StockProcessBalance_New.Qty - Temp.Qty_Rec + L.Qty_Rec;
                        StockProcessBalance_New.ObjectState = Model.ObjectState.Modified;

                        _stockProcessBalanceService.Update(StockProcessBalance_New);
                    }
                }

                StockProcessViewModel.StockProcessId = L.StockProcessId;

            }

            return ErrorText;
        }











        public void DeleteStockProcess(int StockProcessId)
        {
            StockProcess StockProcess = Find(StockProcessId);

            StockProcessBalance StockProcessBalance = _stockProcessBalanceService.Find(StockProcess.ProductId, StockProcess.Dimension1Id, StockProcess.Dimension2Id, StockProcess.ProcessId, StockProcess.LotNo, StockProcess.GodownId, StockProcess.CostCenterId);

            if (StockProcessBalance != null)
            {
                StockProcessBalance.Qty = StockProcessBalance.Qty + StockProcess.Qty_Iss;
                StockProcessBalance.Qty = StockProcessBalance.Qty - StockProcess.Qty_Rec;

                _stockProcessBalanceService.Update(StockProcessBalance);
            }
            Delete(StockProcessId);
        }


        public void DeleteStockProcessDB(int StockProcessId)
        {

            StockProcess StockProcess = Find(StockProcessId);

            StockProcessBalance StockProcessBalance = (from p in _unitOfWork.Repository<StockProcessBalance>().Instance
                                                       where p.ProductId == StockProcess.ProductId &&
                                                       p.Dimension1Id == StockProcess.Dimension1Id &&
                                                       p.Dimension2Id == StockProcess.Dimension2Id &&
                                                       p.ProcessId == StockProcess.ProcessId &&
                                                       p.LotNo == StockProcess.LotNo &&
                                                       p.GodownId == StockProcess.GodownId &&
                                                       p.CostCenterId == StockProcess.CostCenterId
                                                       select p).FirstOrDefault();

            if (StockProcessBalance != null)
            {
                StockProcessBalance.Qty = StockProcessBalance.Qty + StockProcess.Qty_Iss;
                StockProcessBalance.Qty = StockProcessBalance.Qty - StockProcess.Qty_Rec;
                StockProcessBalance.ObjectState = Model.ObjectState.Modified;

                _stockProcessBalanceService.Update(StockProcessBalance);
            }
            StockProcess.ObjectState = Model.ObjectState.Deleted;

            Delete(StockProcessId);

        }

        public void DeleteStockProcessDBMultiple(List<int> StockProcessId)
        {

            var StockProcIdArray = StockProcessId.ToArray();

            var StockProcess = (from p in _StockProcessRepository.Instance
                                where StockProcIdArray.Contains(p.StockProcessId)
                                select p).ToList();

            var GroupedStockProc = (from p in StockProcess
                                    group p by new
                                    {
                                        p.ProductId,
                                        p.Dimension1Id,
                                        p.Dimension2Id,
                                        p.ProcessId,
                                        p.LotNo,
                                        p.GodownId,
                                        p.CostCenterId,
                                    } into g
                                    select g).ToList();

            foreach (var item in GroupedStockProc)
            {

                StockProcessBalance StockProcessBalance = (from p in _unitOfWork.Repository<StockProcessBalance>().Instance
                                                           where p.ProductId == item.Key.ProductId &&
                                                           p.Dimension1Id == item.Key.Dimension1Id &&
                                                           p.Dimension2Id == item.Key.Dimension2Id &&
                                                           p.ProcessId == item.Key.ProcessId &&
                                                           p.LotNo == item.Key.LotNo &&
                                                           p.GodownId == item.Key.GodownId &&
                                                           p.CostCenterId == item.Key.CostCenterId
                                                           select p).FirstOrDefault();

                if (StockProcessBalance != null)
                {
                    StockProcessBalance.Qty = StockProcessBalance.Qty + item.Sum(m => m.Qty_Iss);
                    StockProcessBalance.Qty = StockProcessBalance.Qty - item.Sum(m => m.Qty_Rec);
                    StockProcessBalance.ObjectState = Model.ObjectState.Modified;

                    _stockProcessBalanceService.Update(StockProcessBalance);
                }

                foreach (var IStockProc in item)
                {
                    IStockProc.ObjectState = Model.ObjectState.Deleted;

                    Delete(IStockProc);
                }
            }
        }
    }
}