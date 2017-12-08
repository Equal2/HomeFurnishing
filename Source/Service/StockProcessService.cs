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
    public interface IStockProcessService : IDisposable
    {
        StockProcess Create(StockProcess pt);
        void Delete(int id);
        void Update(StockProcess pt);
        StockProcess Find(int id);
        void DeleteStockProcessForStockHeader(int StockHeaderId);
        void DeleteStockProcessForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId, ApplicationDbContext Context);
        //string StockProcessPost(IEnumerable<StockProcessViewModel> StockProcessViewModel);
        //string StockProcessPost(StockProcessViewModel StockProcessViewModel_New, StockProcessViewModel StockProcessViewModel_Old);

        void DeleteStockProcess(int StockProcessId);
        void DeleteStockProcessDB(int StockProcessId, ref ApplicationDbContext Context, bool IsDBbased);

        IEnumerable<StockProcess> GetStockProcessForStockHeaderId(int StockHeaderId);
    }

    public class StockProcessService : IStockProcessService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<StockProcess> _StockProcessRepository;
        RepositoryQuery<StockProcess> StockProcessRepository;

        public StockProcessService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StockProcessRepository = new Repository<StockProcess>(db);
            StockProcessRepository = new RepositoryQuery<StockProcess>(_StockProcessRepository);
        }

        public StockProcess GetStockProcess(int pt)
        {
            return _unitOfWork.Repository<StockProcess>().Find(pt);
        }

        public StockProcess Create(StockProcess pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<StockProcess>().Insert(pt);
            return pt;
        }

        public StockProcess Find(int id)
        {
            return _unitOfWork.Repository<StockProcess>().Find(id);
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<StockProcess>().Delete(id);
        }

        public void Delete(StockProcess pt)
        {
            //_unitOfWork.Repository<StockProcess>().Delete(pt);
            db.StockProcess.Remove(pt);
        }

        public void Delete(StockProcess pt, ApplicationDbContext Context)
        {
            //_unitOfWork.Repository<StockProcess>().Delete(pt);
            pt.ObjectState = Model.ObjectState.Deleted;
            Context.StockProcess.Attach(pt);
            Context.StockProcess.Remove(pt);
        }



        public void Update(StockProcess pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<StockProcess>().Update(pt);
        }

        public IEnumerable<StockProcess> GetStockProcessList()
        {
            var pt = _unitOfWork.Repository<StockProcess>().Query().Get();

            return pt;
        }

        public StockProcess Add(StockProcess pt)
        {
            _unitOfWork.Repository<StockProcess>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
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


        public StockProcess Find(int StockHeaderId, int ProductId, DateTime DocDate, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId)
        {
            StockProcess StockProcess = (from L in db.StockProcess
                                         where L.StockHeaderId == StockHeaderId && L.ProductId == ProductId && L.DocDate == DocDate && L.Dimension1Id == Dimension1Id &&
                                               L.Dimension2Id == Dimension2Id &&
                                               L.Dimension3Id == Dimension3Id &&
                                               L.Dimension4Id == Dimension4Id && 
                                               L.ProcessId == ProcessId && L.LotNo == LotNo && L.GodownId == GodownId && L.CostCenterId == CostCenterId
                                         select L).FirstOrDefault();

            return StockProcess;
        }

        //public StockProcessBalance FindStockProcessBalance(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId)
        //{
        //    StockProcessBalance StockProcessbalance = (from L in db.StockProcessBalance
        //                                               where L.ProductId == ProductId && L.Dimension1Id == Dimension1Id && L.Dimension2Id == Dimension2Id
        //                                               && L.Dimension3Id == Dimension3Id && L.Dimension4Id == Dimension4Id 
        //                                               && L.ProcessId == ProcessId && L.LotNo == LotNo && L.GodownId == GodownId && L.CostCenterId == CostCenterId
        //                                               select L).FirstOrDefault();
        //    return StockProcessbalance;
        //}

        public void DeleteStockProcessForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId, ApplicationDbContext Context)
        {
            IEnumerable<StockProcess> StockProcessList = (from L in Context.StockProcess
                                                          join H in Context.StockHeader on L.StockHeaderId equals H.StockHeaderId into StockHeaderTable
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



                        //StockProcessBalance StockProcessbalance = (from L in Context.StockProcessBalance
                        //                                           where L.ProductId == item.ProductId && L.Dimension1Id == item.Dimension1Id && L.Dimension2Id == item.Dimension2Id
                        //                                           && L.Dimension3Id == item.Dimension3Id && L.Dimension4Id == item.Dimension4Id 
                        //                                           && L.ProcessId == item.ProcessId &&
                        //                                           L.LotNo == item.LotNo && L.GodownId == item.GodownId && L.CostCenterId == item.CostCenterId
                        //                                           select L).FirstOrDefault();

                        //if (StockProcessbalance != null)
                        //{
                        //    StockProcessbalance.Qty = StockProcessbalance.Qty - item.Qty_Rec;
                        //    StockProcessbalance.Qty = StockProcessbalance.Qty + item.Qty_Iss;

                        //    if (StockProcessbalance.Qty == 0)
                        //    {
                        //        item.ObjectState = Model.ObjectState.Deleted;
                        //        db.StockProcessBalance.Attach(StockProcessbalance);
                        //        Context.StockProcessBalance.Remove(StockProcessbalance);
                        //    }
                        //    else
                        //    {
                        //        i = i + 1;
                        //        StockProcessbalance.ObjectState = Model.ObjectState.Modified;
                        //        Context.StockProcessBalance.Add(StockProcessbalance);

                        //    }
                        //}

                        Delete(item, Context);
                    }
                    catch (Exception e)
                    {
                        string str = e.Message;
                    }
                }
                new StockHeaderService(_unitOfWork).Delete(StockProcessList.FirstOrDefault().StockHeaderId, Context);
            }
        }


        public void DeleteStockProcessForStockHeader(int StockHeaderId)
        {
            IEnumerable<StockProcess> StockProcessList = (from L in db.StockProcess
                                                          where L.StockHeaderId == StockHeaderId
                                                          select L).ToList();

            foreach (StockProcess item in StockProcessList)
            {
                Delete(item);
            }
        }



        /// <summary>
        /// Create, update and delete record for whole entry.It deletes the StockProcess posting for given docid and repost whole entry in StockProcess.
        /// </summary>
        /// <param name="StockProcessViewModel"></param>
        /// <returns></returns>
        //public string StockProcessPost(IEnumerable<StockProcessViewModel> StockProcessViewModel)
        //{
        //    string ErrorText = "";

        //    //Get Data from StockProcess VIew Model and post in StockProcess header model for insertion or updation
        //    StockHeader StockHeader = (from L in StockProcessViewModel
        //                               select new StockHeader
        //                               {
        //                                   DocHeaderId = L.DocHeaderId,
        //                                   DocTypeId = L.DocTypeId,
        //                                   DocDate = L.StockHeaderDocDate,
        //                                   DocNo = L.DocNo,
        //                                   DivisionId = L.DivisionId,
        //                                   SiteId = L.SiteId,
        //                                   CurrencyId = L.CurrencyId,
        //                                   PersonId = L.PersonId,
        //                                   ProcessId = L.HeaderProcessId,
        //                                   FromGodownId = L.HeaderFromGodownId,
        //                                   GodownId = L.HeaderGodownId,
        //                                   Remark = L.Remark,
        //                                   Status = L.Status,
        //                                   CreatedBy = L.CreatedBy,
        //                                   CreatedDate = L.CreatedDate,
        //                                   ModifiedBy = L.ModifiedBy,
        //                                   ModifiedDate = L.ModifiedDate
        //                               }).FirstOrDefault();

        //    var temp = new StockHeaderService(_unitOfWork).FindByDocHeader(StockHeader.DocHeaderId,null, StockHeader.DocTypeId, StockHeader.SiteId, StockHeader.DivisionId);


        //    //if record is found in StockProcess Header Table then it will edit it if data is not found in StockProcess header table than it will add a new record.
        //    if (temp == null)
        //    {
        //        StockHeader H = new StockHeader();

        //        H.DocHeaderId = StockHeader.DocHeaderId;
        //        H.DocTypeId = StockHeader.DocTypeId;
        //        H.DocDate = StockHeader.DocDate;
        //        H.DocNo = StockHeader.DocNo;
        //        H.DivisionId = StockHeader.DivisionId;
        //        H.SiteId = StockHeader.SiteId;
        //        H.CurrencyId = StockHeader.CurrencyId;
        //        H.PersonId = StockHeader.PersonId;
        //        H.ProcessId = StockHeader.ProcessId;
        //        H.FromGodownId = StockHeader.FromGodownId;
        //        H.GodownId = StockHeader.GodownId;
        //        H.Remark = StockHeader.Remark;
        //        H.Status = StockHeader.Status;
        //        H.CreatedBy = StockHeader.CreatedBy;
        //        H.CreatedDate = StockHeader.CreatedDate;
        //        H.ModifiedBy = StockHeader.ModifiedBy;
        //        H.ModifiedDate = StockHeader.ModifiedDate;



        //        new StockHeaderService(_unitOfWork).Create(H);
        //    }
        //    else
        //    {
        //        DeleteStockProcessForStockHeader(temp.StockHeaderId);


        //        temp.DocHeaderId = StockHeader.DocHeaderId;
        //        temp.DocTypeId = StockHeader.DocTypeId;
        //        temp.DocDate = StockHeader.DocDate;
        //        temp.DocNo = StockHeader.DocNo;
        //        temp.DivisionId = StockHeader.DivisionId;
        //        temp.SiteId = StockHeader.SiteId;
        //        temp.CurrencyId = StockHeader.CurrencyId;
        //        temp.PersonId = StockHeader.PersonId;
        //        temp.ProcessId = StockHeader.ProcessId;
        //        temp.FromGodownId = StockHeader.FromGodownId;
        //        temp.GodownId = StockHeader.GodownId;
        //        temp.Remark = StockHeader.Remark;
        //        temp.Status = StockHeader.Status;
        //        temp.CreatedBy = StockHeader.CreatedBy;
        //        temp.CreatedDate = StockHeader.CreatedDate;
        //        temp.ModifiedBy = StockHeader.ModifiedBy;
        //        temp.ModifiedDate = StockHeader.ModifiedDate;


        //        new StockHeaderService(_unitOfWork).Update(temp);
        //    }

        //    IEnumerable<StockProcess> StockProcessList = (from L in StockProcessViewModel
        //                                    group new { L } by new  { L.ProductId, L.GodownId, L.CostCenterId, L.LotNo, L.ProcessId, L.Dimension1Id, L.Dimension2Id, DocDate = L.StockHeaderDocDate } into Result
        //                                    select new StockProcess
        //                                    {
        //                                        DocDate = Result.Key.DocDate,
        //                                        ProductId = Result.Key.ProductId,
        //                                        ProcessId = Result.Key.ProcessId,
        //                                        GodownId = Result.Key.GodownId,
        //                                        LotNo = Result.Key.LotNo,
        //                                        CostCenterId = Result.Key.CostCenterId,
        //                                        Qty_Iss = Result.Sum(i => i.L.Qty_Iss),
        //                                        Qty_Rec = Result.Sum(i => i.L.Qty_Rec),
        //                                        Rate = Result.Max(i => i.L.Rate),
        //                                        ExpiryDate = Result.Max(i => i.L.ExpiryDate),
        //                                        Specification = Result.Max(i => i.L.Specification),
        //                                        Dimension1Id = Result.Key.Dimension1Id,
        //                                        Dimension2Id = Result.Key.Dimension2Id,
        //                                        CreatedBy = Result.Max(i => i.L.CreatedBy),
        //                                        CreatedDate = Result.Max(i => i.L.CreatedDate),
        //                                        ModifiedBy = Result.Max(i => i.L.ModifiedBy),
        //                                        ModifiedDate = Result.Max(i => i.L.ModifiedDate),
        //                                    }).ToList();


        //    foreach (StockProcess item in StockProcessList)
        //    {
        //        if (temp != null)
        //        {
        //            item.StockHeaderId = temp.StockHeaderId;
        //        }

        //        Create(item);

        //        StockProcessBalance StockProcessBalance = new StockProcessBalanceService(_unitOfWork).Find(item.ProductId, item.Dimension1Id, item.Dimension2Id, item.ProcessId, item.LotNo, item.GodownId, item.CostCenterId);

        //        if (StockProcessBalance == null)
        //        {

        //            StockProcessBalance st = new StockProcessBalance();
        //            st.ProductId = item.ProductId;
        //            st.Dimension1Id = item.Dimension1Id;
        //            st.Dimension2Id = item.Dimension2Id;
        //            st.ProcessId = item.ProcessId;
        //            st.GodownId = item.GodownId;
        //            st.CostCenterId = item.CostCenterId;
        //            st.LotNo = item.LotNo;
        //            st.Qty = st.Qty + item.Qty_Rec;
        //            st.Qty = st.Qty - item.Qty_Iss;

        //            new StockProcessBalanceService(_unitOfWork).Create(st);
        //        }
        //        else
        //        {
        //            StockProcessBalance.Qty = StockProcessBalance.Qty + item.Qty_Rec;
        //            StockProcessBalance.Qty = StockProcessBalance.Qty - item.Qty_Iss;
        //            new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance);
        //        }
        //    }

        //    return ErrorText;
        //}



        ///// <summary>
        ///// Create, update and delete StockProcess Posting for one record.
        ///// </summary>
        ///// <param name="StockProcessViewModel_New"></param>
        ///// <param name="StockProcessViewModel_Old"></param>
        ///// <returns></returns>
        //public string StockProcessPost(StockProcessViewModel StockProcessViewModel_New, StockProcessViewModel StockProcessViewModel_Old)
        //{
        //    string ErrorText = "";
        //    StockHeader StockHeader;


        //    if (StockProcessViewModel_New != null)
        //    {
        //        StockHeader = new StockHeaderService(_unitOfWork).FindByDocHeader(StockProcessViewModel_New.DocHeaderId, StockProcessViewModel_New.StockHeaderId, StockProcessViewModel_New.DocTypeId, StockProcessViewModel_New.SiteId, StockProcessViewModel_New.DivisionId);

        //        if (StockProcessViewModel_New.StockHeaderExist == 0 || StockProcessViewModel_New.StockHeaderExist == null)
        //        {
        //            if (StockHeader == null)
        //            {
        //                StockHeader H = new StockHeader();

        //                H.DocHeaderId = StockProcessViewModel_New.DocHeaderId;
        //                H.DocTypeId = StockProcessViewModel_New.DocTypeId;
        //                H.DocDate = StockProcessViewModel_New.StockHeaderDocDate;
        //                H.DocNo = StockProcessViewModel_New.DocNo;
        //                H.DivisionId = StockProcessViewModel_New.DivisionId;
        //                H.SiteId = StockProcessViewModel_New.SiteId;
        //                H.CurrencyId = StockProcessViewModel_New.CurrencyId;
        //                H.PersonId = StockProcessViewModel_New.PersonId;
        //                H.ProcessId = StockProcessViewModel_New.HeaderProcessId;
        //                H.FromGodownId = StockProcessViewModel_New.HeaderFromGodownId;
        //                H.GodownId = StockProcessViewModel_New.HeaderGodownId;
        //                H.Remark = StockProcessViewModel_New.Remark;
        //                H.Status = StockProcessViewModel_New.Status;
        //                H.CreatedBy = StockProcessViewModel_New.CreatedBy;
        //                H.CreatedDate = StockProcessViewModel_New.CreatedDate;
        //                H.ModifiedBy = StockProcessViewModel_New.ModifiedBy;
        //                H.ModifiedDate = StockProcessViewModel_New.ModifiedDate;

        //                new StockHeaderService(_unitOfWork).Create(H);

        //                StockHeader = H;
        //            }
        //            else
        //            {
        //                StockHeader.DocHeaderId = StockProcessViewModel_New.DocHeaderId;
        //                StockHeader.DocTypeId = StockProcessViewModel_New.DocTypeId;
        //                StockHeader.DocDate = StockProcessViewModel_New.StockHeaderDocDate;
        //                StockHeader.DocNo = StockProcessViewModel_New.DocNo;
        //                StockHeader.DivisionId = StockProcessViewModel_New.DivisionId;
        //                StockHeader.SiteId = StockProcessViewModel_New.SiteId;
        //                StockHeader.CurrencyId = StockProcessViewModel_New.CurrencyId;
        //                StockHeader.PersonId = StockProcessViewModel_New.PersonId;
        //                StockHeader.ProcessId = StockProcessViewModel_New.HeaderProcessId;
        //                StockHeader.FromGodownId = StockProcessViewModel_New.HeaderFromGodownId;
        //                StockHeader.GodownId = StockProcessViewModel_New.HeaderGodownId;
        //                StockHeader.Remark = StockProcessViewModel_New.Remark;
        //                StockHeader.Status = StockProcessViewModel_New.Status;
        //                StockHeader.CreatedBy = StockProcessViewModel_New.CreatedBy;
        //                StockHeader.CreatedDate = StockProcessViewModel_New.CreatedDate;
        //                StockHeader.ModifiedBy = StockProcessViewModel_New.ModifiedBy;
        //                StockHeader.ModifiedDate = StockProcessViewModel_New.ModifiedDate;

        //                new StockHeaderService(_unitOfWork).Update(StockHeader);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        StockHeader = new StockHeaderService(_unitOfWork).FindByDocHeader(StockProcessViewModel_Old.DocHeaderId, StockProcessViewModel_Old.StockHeaderId, StockProcessViewModel_Old.DocTypeId, StockProcessViewModel_Old.SiteId, StockProcessViewModel_Old.DivisionId);
        //    }


        //    if (StockProcessViewModel_Old != null)
        //    {
        //        StockProcess StockProcess_Old = Find(StockProcessViewModel_Old.StockHeaderId, StockProcessViewModel_Old.ProductId, StockProcessViewModel_Old.StockProcessDocDate, StockProcessViewModel_Old.Dimension1Id, StockProcessViewModel_Old.Dimension2Id, StockProcessViewModel_Old.ProcessId, StockProcessViewModel_Old.LotNo, StockProcessViewModel_Old.GodownId, StockProcessViewModel_Old.CostCenterId);

        //        if (StockProcess_Old != null)
        //        {
        //            StockProcess_Old.Qty_Iss = StockProcess_Old.Qty_Iss - StockProcessViewModel_Old.Qty_Iss;
        //            StockProcess_Old.Qty_Rec = StockProcess_Old.Qty_Rec - StockProcessViewModel_Old.Qty_Rec;
        //            StockProcess_Old.Rate = StockProcessViewModel_Old.Rate;
        //            StockProcess_Old.ExpiryDate = StockProcessViewModel_Old.ExpiryDate;
        //            StockProcess_Old.Specification = StockProcessViewModel_Old.Specification;

        //            Update(StockProcess_Old);

        //            //if (StockProcess_Old.Qty_Iss == 0 && StockProcess_Old.Qty_Rec == 0) { Delete(StockProcess_Old); }
        //            //else { Update(StockProcess_Old); }

        //            StockProcessBalance StockProcessBalance_Old = FindStockProcessBalance(StockProcessViewModel_Old.ProductId, StockProcessViewModel_Old.Dimension1Id, StockProcessViewModel_Old.Dimension2Id, StockProcessViewModel_Old.ProcessId, StockProcessViewModel_Old.LotNo, StockProcessViewModel_Old.GodownId, StockProcessViewModel_Old.CostCenterId);

        //            if (StockProcessBalance_Old != null)
        //            {
        //                StockProcessBalance_Old.Qty = StockProcessBalance_Old.Qty - StockProcessViewModel_Old.Qty_Rec;
        //                StockProcessBalance_Old.Qty = StockProcessBalance_Old.Qty + StockProcessViewModel_Old.Qty_Iss;

        //                if (StockProcessBalance_Old.Qty == 0) { new StockProcessBalanceService(_unitOfWork).Delete(StockProcessBalance_Old); }
        //                else { new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance_Old); }
        //            }
        //        }


        //    }

        //    if (StockProcessViewModel_New != null)
        //    {
        //        StockProcess StockProcess_New;

        //        if (StockHeader != null)
        //        {
        //            StockProcess_New = Find(StockHeader.StockHeaderId, StockProcessViewModel_New.ProductId, StockProcessViewModel_New.StockProcessDocDate, StockProcessViewModel_New.Dimension1Id, StockProcessViewModel_New.Dimension2Id, StockProcessViewModel_New.ProcessId, StockProcessViewModel_New.LotNo, StockProcessViewModel_New.GodownId, StockProcessViewModel_New.CostCenterId);
        //        }
        //        else
        //        {
        //            StockProcess_New = null;
        //        }

        //        if (StockProcess_New == null)
        //        {
        //            StockProcess L = new StockProcess();

        //            L.DocDate = StockProcessViewModel_New.StockProcessDocDate;
        //            L.ProductId = StockProcessViewModel_New.ProductId;
        //            L.ProcessId = StockProcessViewModel_New.ProcessId;
        //            L.GodownId = StockProcessViewModel_New.GodownId;
        //            L.LotNo = StockProcessViewModel_New.LotNo;
        //            L.CostCenterId = StockProcessViewModel_New.CostCenterId;
        //            L.Qty_Iss = StockProcessViewModel_New.Qty_Iss;
        //            L.Qty_Rec = StockProcessViewModel_New.Qty_Rec;
        //            L.Rate = StockProcessViewModel_New.Rate;
        //            L.ExpiryDate = StockProcessViewModel_New.ExpiryDate;
        //            L.Specification = StockProcessViewModel_New.Specification;
        //            L.Dimension1Id = StockProcessViewModel_New.Dimension1Id;
        //            L.Dimension2Id = StockProcessViewModel_New.Dimension2Id;
        //            L.CreatedBy = StockProcessViewModel_New.CreatedBy;
        //            L.CreatedDate = StockProcessViewModel_New.CreatedDate;
        //            L.ModifiedBy = StockProcessViewModel_New.ModifiedBy;
        //            L.ModifiedDate = StockProcessViewModel_New.ModifiedDate;


        //            if (StockHeader != null)
        //            {
        //                L.StockHeaderId = StockHeader.StockHeaderId;
        //            }

        //            Create(L);
        //        }
        //        else
        //        {
        //            StockProcess_New.Qty_Iss = StockProcess_New.Qty_Iss + StockProcessViewModel_New.Qty_Iss;
        //            StockProcess_New.Qty_Rec = StockProcess_New.Qty_Rec + StockProcessViewModel_New.Qty_Rec;
        //            StockProcess_New.Rate = StockProcessViewModel_New.Rate;
        //            StockProcess_New.ExpiryDate = StockProcessViewModel_New.ExpiryDate;
        //            StockProcess_New.Specification = StockProcessViewModel_New.Specification;
        //            StockProcess_New.ModifiedBy = StockProcessViewModel_New.ModifiedBy;
        //            StockProcess_New.ModifiedDate = StockProcessViewModel_New.ModifiedDate;

        //            Update(StockProcess_New);
        //        }

        //        StockProcessBalance StockProcessBalance_New = FindStockProcessBalance(StockProcessViewModel_New.ProductId, StockProcessViewModel_New.Dimension1Id, StockProcessViewModel_New.Dimension2Id, StockProcessViewModel_New.ProcessId, StockProcessViewModel_New.LotNo, StockProcessViewModel_New.GodownId, StockProcessViewModel_New.CostCenterId);

        //        if (StockProcessBalance_New == null)
        //        {

        //            StockProcessBalance Sb = new StockProcessBalance();

        //            Sb.ProductId = StockProcessViewModel_New.ProductId;
        //            Sb.Dimension1Id = StockProcessViewModel_New.Dimension1Id;
        //            Sb.Dimension2Id = StockProcessViewModel_New.Dimension2Id;
        //            Sb.ProcessId = StockProcessViewModel_New.ProcessId;
        //            Sb.GodownId = StockProcessViewModel_New.GodownId;
        //            Sb.CostCenterId = StockProcessViewModel_New.CostCenterId;
        //            Sb.LotNo = StockProcessViewModel_New.LotNo;
        //            if (StockProcessViewModel_New.Qty_Iss != 0) { Sb.Qty = StockProcessViewModel_New.Qty_Iss; }
        //            if (StockProcessViewModel_New.Qty_Rec != 0) { Sb.Qty = StockProcessViewModel_New.Qty_Rec; }

        //            new StockProcessBalanceService(_unitOfWork).Create(Sb);
        //        }
        //        else
        //        {
        //            StockProcessBalance_New.Qty = StockProcessBalance_New.Qty + StockProcessViewModel_New.Qty_Rec;
        //            StockProcessBalance_New.Qty = StockProcessBalance_New.Qty - StockProcessViewModel_New.Qty_Iss;

        //            new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance_New);
        //        }


        //    }




        //    //Posting in StockProcesses
        //    if (StockProcessViewModel_New != null && StockProcessViewModel_Old == null && StockProcessViewModel_New.IsPostedInStockProcess)//Creating Record
        //    {
        //        StockProcess StockProcessProc = new StockProcess();

        //        StockProcessProc = new StockProcessService(_unitOfWork).Find(StockHeader.StockHeaderId, StockProcessViewModel_New.ProductId, StockProcessViewModel_New.Dimension1Id, StockProcessViewModel_New.Dimension2Id, StockProcessViewModel_New.ProcessId, StockProcessViewModel_New.LotNo, StockProcessViewModel_New.CostCenterId);

        //        if (StockProcessProc != null)
        //        {
        //            StockProcessProc.Qty_Iss += StockProcessViewModel_New.Qty_Iss;
        //            StockProcessProc.Qty_Rec += StockProcessViewModel_New.Qty_Rec;

        //            StockProcessProc.ObjectState = Model.ObjectState.Modified;
        //            new StockProcessService(_unitOfWork).Update(StockProcessProc);
        //        }
        //        else
        //        {
        //            StockProcessProc = new StockProcess();
        //            StockProcessProc.CostCenterId = StockProcessViewModel_New.CostCenterId;
        //            StockProcessProc.Dimension1Id = StockProcessViewModel_New.Dimension1Id;
        //            StockProcessProc.Dimension2Id = StockProcessViewModel_New.Dimension2Id;
        //            StockProcessProc.ExpiryDate = StockProcessViewModel_New.ExpiryDate;
        //            StockProcessProc.LotNo = StockProcessViewModel_New.LotNo;
        //            StockProcessProc.ProcessId = StockProcessViewModel_New.ProcessId;
        //            StockProcessProc.ProductId = StockProcessViewModel_New.ProductId;
        //            StockProcessProc.Qty_Iss = StockProcessViewModel_New.Qty_Iss;
        //            StockProcessProc.Qty_Rec = StockProcessViewModel_New.Qty_Rec;
        //            StockProcessProc.Rate = StockProcessViewModel_New.Rate;
        //            StockProcessProc.Specification = StockProcessViewModel_New.Specification;
        //            //StockProcessProc.StockHeaderId = StockHeader.StockHeaderId;

        //            StockProcessProc.ObjectState = Model.ObjectState.Added;
        //            new StockProcessService(_unitOfWork).Create(StockProcessProc);
        //        }


        //    }
        //    else if (StockProcessViewModel_New != null && StockProcessViewModel_Old != null && (StockProcessViewModel_New.IsPostedInStockProcess || StockProcessViewModel_Old.IsPostedInStockProcess))//Updating Record
        //    {

        //        StockProcess StockProcessProc_New = new StockProcess();

        //        StockProcessProc_New = new StockProcessService(_unitOfWork).Find(StockHeader.StockHeaderId, StockProcessViewModel_New.ProductId, StockProcessViewModel_New.Dimension1Id, StockProcessViewModel_New.Dimension2Id, StockProcessViewModel_New.ProcessId, StockProcessViewModel_New.LotNo, StockProcessViewModel_New.CostCenterId);




        //        //Changes made to only Qty
        //        if (StockProcessProc_New != null)
        //        {
        //            StockProcessProc_New.Qty_Rec = StockProcessProc_New.Qty_Rec + StockProcessViewModel_New.Qty_Rec - StockProcessViewModel_Old.Qty_Rec;
        //            StockProcessProc_New.Qty_Iss = StockProcessProc_New.Qty_Iss + StockProcessViewModel_New.Qty_Iss - StockProcessViewModel_Old.Qty_Iss;


        //            if (StockProcessProc_New.Qty_Rec == 0 && StockProcessProc_New.Qty_Iss == 0)
        //            {
        //                new StockProcessService(_unitOfWork).Delete(StockProcessProc_New);
        //            }
        //            else
        //            {
        //                StockProcessProc_New.ObjectState = Model.ObjectState.Modified;
        //                new StockProcessService(_unitOfWork).Update(StockProcessProc_New);
        //            }
        //        }
        //        else //Changes made to Other Parameters//Create New Record Updated Parameters and delete the Qty from old Record
        //        {
        //            StockProcess StockProcessProc_Old = new StockProcess();

        //            StockProcessProc_Old = new StockProcessService(_unitOfWork).Find(StockHeader.StockHeaderId, StockProcessViewModel_Old.ProductId, StockProcessViewModel_Old.Dimension1Id, StockProcessViewModel_Old.Dimension2Id, StockProcessViewModel_Old.ProcessId, StockProcessViewModel_Old.LotNo, StockProcessViewModel_Old.CostCenterId);


        //            StockProcessProc_Old.Qty_Iss = StockProcessProc_Old.Qty_Iss - StockProcessViewModel_Old.Qty_Iss;
        //            StockProcessProc_Old.Qty_Rec = StockProcessProc_Old.Qty_Rec - StockProcessViewModel_Old.Qty_Rec;

        //            if (StockProcessProc_Old.Qty_Rec == 0 && StockProcessProc_Old.Qty_Iss == 0)
        //            {
        //                new StockProcessService(_unitOfWork).Delete(StockProcessProc_Old);
        //            }
        //            else
        //            {
        //                StockProcessProc_Old.ObjectState = Model.ObjectState.Modified;
        //                new StockProcessService(_unitOfWork).Update(StockProcessProc_Old);
        //            }

        //            StockProcess StockProcessProc_Updat = new StockProcess();

        //            StockProcessProc_Updat = new StockProcess();
        //            StockProcessProc_Updat.CostCenterId = StockProcessViewModel_New.CostCenterId;
        //            StockProcessProc_Updat.Dimension1Id = StockProcessViewModel_New.Dimension1Id;
        //            StockProcessProc_Updat.Dimension2Id = StockProcessViewModel_New.Dimension2Id;
        //            StockProcessProc_Updat.ExpiryDate = StockProcessViewModel_New.ExpiryDate;
        //            StockProcessProc_Updat.LotNo = StockProcessViewModel_New.LotNo;
        //            StockProcessProc_Updat.ProcessId = StockProcessViewModel_New.ProcessId;
        //            StockProcessProc_Updat.ProductId = StockProcessViewModel_New.ProductId;
        //            StockProcessProc_Updat.Qty_Iss = StockProcessViewModel_New.Qty_Iss;
        //            StockProcessProc_Updat.Qty_Rec = StockProcessViewModel_New.Qty_Rec;
        //            StockProcessProc_Updat.Rate = StockProcessViewModel_New.Rate;
        //            StockProcessProc_Updat.Specification = StockProcessViewModel_New.Specification;
        //            //StockProcessProc_Updat.StockHeaderId = StockHeader.StockHeaderId;

        //            StockProcessProc_Updat.ObjectState = Model.ObjectState.Added;
        //            new StockProcessService(_unitOfWork).Create(StockProcessProc_Updat);



        //        }


        //    }
        //    else if (StockProcessViewModel_New == null && StockProcessViewModel_Old != null && StockProcessViewModel_Old.IsPostedInStockProcess)//Deleting Record
        //    {
        //        StockProcess StockProcessProc = new StockProcess();

        //        StockProcessProc = new StockProcessService(_unitOfWork).Find(StockHeader.StockHeaderId, StockProcessViewModel_Old.ProductId, StockProcessViewModel_Old.Dimension1Id, StockProcessViewModel_Old.Dimension2Id, StockProcessViewModel_Old.ProcessId, StockProcessViewModel_Old.LotNo, StockProcessViewModel_Old.CostCenterId);

        //        if (StockProcessProc != null)
        //        {
        //            StockProcessProc.Qty_Iss -= StockProcessViewModel_Old.Qty_Iss;
        //            StockProcessProc.Qty_Rec -= StockProcessViewModel_Old.Qty_Rec;

        //            if (StockProcessProc.Qty_Rec == 0 && StockProcessProc.Qty_Iss == 0)
        //            {
        //                new StockProcessService(_unitOfWork).Delete(StockProcessProc);
        //            }
        //            else
        //            {
        //                StockProcessProc.ObjectState = Model.ObjectState.Modified;
        //                new StockProcessService(_unitOfWork).Update(StockProcessProc);
        //            }
        //        }


        //    }




        //    return ErrorText;
        //}

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

                new StockHeaderService(_unitOfWork).Create(H);

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
                L.Dimension3Id = StockProcessViewModel.Dimension3Id;
                L.Dimension4Id = StockProcessViewModel.Dimension4Id;
                L.CreatedBy = StockProcessViewModel.CreatedBy;
                L.CreatedDate = StockProcessViewModel.CreatedDate;
                L.ModifiedBy = StockProcessViewModel.ModifiedBy;
                L.ModifiedDate = StockProcessViewModel.ModifiedDate;
                L.ObjectState = Model.ObjectState.Added;
                new StockProcessService(_unitOfWork).Create(L);


                //StockProcessBalance StockProcessBalance = new StockProcessBalanceService(_unitOfWork).Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.Dimension3Id, L.Dimension4Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                //if (StockProcessBalance == null)
                //{
                //    StockProcessBalance StockProcessBalance_NewRecord = new StockProcessBalance();

                //    StockProcessBalance_NewRecord.ProductId = L.ProductId;
                //    StockProcessBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                //    StockProcessBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                //    StockProcessBalance_NewRecord.Dimension3Id = L.Dimension3Id;
                //    StockProcessBalance_NewRecord.Dimension4Id = L.Dimension4Id;
                //    StockProcessBalance_NewRecord.ProcessId = L.ProcessId;
                //    StockProcessBalance_NewRecord.GodownId = L.GodownId;
                //    StockProcessBalance_NewRecord.CostCenterId = L.CostCenterId;
                //    StockProcessBalance_NewRecord.LotNo = L.LotNo;
                //    if (L.Qty_Iss != 0) { StockProcessBalance_NewRecord.Qty = -L.Qty_Iss; }
                //    if (L.Qty_Rec != 0) { StockProcessBalance_NewRecord.Qty = L.Qty_Rec; }

                //    new StockProcessBalanceService(_unitOfWork).Create(StockProcessBalance_NewRecord);
                //}
                //else
                //{
                //    StockProcessBalance.Qty = StockProcessBalance.Qty - L.Qty_Iss;
                //    StockProcessBalance.Qty = StockProcessBalance.Qty + L.Qty_Rec;

                //    new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance);
                //}

                StockProcessViewModel.StockProcessId = L.StockProcessId;
            }
            else
            {
                StockProcess L = new StockProcessService(_unitOfWork).Find(StockProcessViewModel.StockProcessId);

                //To Rollback Chenges in StockProcess Balance done by old entry.
                StockProcess Temp = new StockProcess();
                Temp.ProductId = L.ProductId;
                Temp.Dimension1Id = L.Dimension1Id;
                Temp.Dimension2Id = L.Dimension2Id;
                Temp.Dimension3Id = L.Dimension3Id;
                Temp.Dimension4Id = L.Dimension4Id;
                Temp.ProcessId = L.ProcessId;
                Temp.GodownId = L.GodownId;
                Temp.CostCenterId = L.CostCenterId;
                Temp.LotNo = L.LotNo;
                Temp.Qty_Iss = L.Qty_Iss;
                Temp.Qty_Rec = L.Qty_Rec;
                //new StockProcessBalanceService(_unitOfWork).UpdateStockProcessBalance(Temp);
                ///////////////////////////////////
                //StockProcessBalance StockProcessBalance_Old = new StockProcessBalanceService(_unitOfWork).Find(Temp.ProductId, Temp.Dimension1Id, Temp.Dimension2Id, Temp.Dimension3Id, Temp.Dimension4Id, Temp.ProcessId, Temp.LotNo, Temp.GodownId, Temp.CostCenterId);


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
                L.Dimension3Id = StockProcessViewModel.Dimension3Id;
                L.Dimension4Id = StockProcessViewModel.Dimension4Id;
                L.CreatedBy = StockProcessViewModel.CreatedBy;
                L.CreatedDate = StockProcessViewModel.CreatedDate;
                L.ModifiedBy = StockProcessViewModel.ModifiedBy;
                L.ModifiedDate = StockProcessViewModel.ModifiedDate;

                new StockProcessService(_unitOfWork).Update(L);

                //new StockProcessBalanceService(_unitOfWork).UpdateStockProcessBalance(L);


                //StockProcessBalance StockProcessBalance_New = new StockProcessBalanceService(_unitOfWork).Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.Dimension3Id, L.Dimension4Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                //if (StockProcessBalance_New != null)
                //{

                //    if (StockProcessBalance_Old.StockProcessBalanceId != StockProcessBalance_New.StockProcessBalanceId)
                //    {
                //        if (StockProcessBalance_Old != null)
                //        {
                //            StockProcessBalance_Old.Qty = StockProcessBalance_Old.Qty - Temp.Qty_Rec;
                //            StockProcessBalance_Old.Qty = StockProcessBalance_Old.Qty + Temp.Qty_Iss;

                //            new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance_New);
                //        }

                //        if (StockProcessBalance_New == null)
                //        {
                //            StockProcessBalance StockProcessBalance_NewRecord = new StockProcessBalance();

                //            StockProcessBalance_NewRecord.ProductId = L.ProductId;
                //            StockProcessBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                //            StockProcessBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                //            StockProcessBalance_NewRecord.Dimension3Id = L.Dimension3Id;
                //            StockProcessBalance_NewRecord.Dimension4Id = L.Dimension4Id;
                //            StockProcessBalance_NewRecord.ProcessId = L.ProcessId;
                //            StockProcessBalance_NewRecord.GodownId = L.GodownId;
                //            StockProcessBalance_NewRecord.CostCenterId = L.CostCenterId;
                //            StockProcessBalance_NewRecord.LotNo = L.LotNo;
                //            if (L.Qty_Iss != 0) { StockProcessBalance_NewRecord.Qty = -L.Qty_Iss; }
                //            if (L.Qty_Rec != 0) { StockProcessBalance_NewRecord.Qty = L.Qty_Rec; }

                //            new StockProcessBalanceService(_unitOfWork).Create(StockProcessBalance_NewRecord);
                //        }
                //        else
                //        {
                //            StockProcessBalance_New.Qty = StockProcessBalance_New.Qty - L.Qty_Iss;
                //            StockProcessBalance_New.Qty = StockProcessBalance_New.Qty + L.Qty_Rec;

                //            new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance_New);
                //        }
                //    }
                //    else
                //    {
                //        StockProcessBalance_New.Qty = StockProcessBalance_New.Qty + Temp.Qty_Iss - L.Qty_Iss;
                //        StockProcessBalance_New.Qty = StockProcessBalance_New.Qty - Temp.Qty_Rec + L.Qty_Rec;

                //        new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance_New);
                //    }
                //}

                StockProcessViewModel.StockProcessId = L.StockProcessId;

            }

            return ErrorText;
        }









        public string StockProcessPostDB(ref StockProcessViewModel StockProcessViewModel, ref ApplicationDbContext Context)
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

                if (Context != null)
                    Context.StockHeader.Add(H);
                else
                    new StockHeaderService(_unitOfWork).Create(H);

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
                L.Weight_Iss = StockProcessViewModel.Weight_Iss;
                L.Weight_Rec = StockProcessViewModel.Weight_Rec;
                L.Rate = StockProcessViewModel.Rate;
                L.ProductUidId = StockProcessViewModel.ProductUidId;
                L.Remark = StockProcessViewModel.Remark;
                L.ExpiryDate = StockProcessViewModel.ExpiryDate;
                L.Specification = StockProcessViewModel.Specification;
                L.Dimension1Id = StockProcessViewModel.Dimension1Id;
                L.Dimension2Id = StockProcessViewModel.Dimension2Id;
                L.Dimension3Id = StockProcessViewModel.Dimension3Id;
                L.Dimension4Id = StockProcessViewModel.Dimension4Id;
                L.CreatedBy = StockProcessViewModel.CreatedBy;
                L.CreatedDate = StockProcessViewModel.CreatedDate;
                L.ModifiedBy = StockProcessViewModel.ModifiedBy;
                L.ModifiedDate = StockProcessViewModel.ModifiedDate;
                L.ObjectState = Model.ObjectState.Added;

                if (Context != null)
                    Context.StockProcess.Add(L);
                else
                    new StockProcessService(_unitOfWork).Create(L);


                //StockProcessBalance StockProcessBalance = new StockProcessBalanceService(_unitOfWork).Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.Dimension3Id, L.Dimension4Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                //if (StockProcessBalance == null)
                //{
                //    StockProcessBalance StockProcessBalance_NewRecord = new StockProcessBalance();

                //    StockProcessBalance_NewRecord.ProductId = L.ProductId;
                //    StockProcessBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                //    StockProcessBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                //    StockProcessBalance_NewRecord.Dimension3Id = L.Dimension3Id;
                //    StockProcessBalance_NewRecord.Dimension4Id = L.Dimension4Id;
                //    StockProcessBalance_NewRecord.ProcessId = L.ProcessId;
                //    StockProcessBalance_NewRecord.GodownId = L.GodownId;
                //    StockProcessBalance_NewRecord.CostCenterId = L.CostCenterId;
                //    StockProcessBalance_NewRecord.LotNo = L.LotNo;
                //    if (L.Qty_Iss != 0) { StockProcessBalance_NewRecord.Qty = -L.Qty_Iss; }
                //    if (L.Qty_Rec != 0) { StockProcessBalance_NewRecord.Qty = L.Qty_Rec; }
                //    StockProcessBalance_NewRecord.ObjectState = Model.ObjectState.Added;

                //    if (Context != null)
                //        Context.StockProcessBalance.Add(StockProcessBalance_NewRecord);
                //    else
                //        new StockProcessBalanceService(_unitOfWork).Create(StockProcessBalance_NewRecord);
                //}
                //else
                //{
                //    StockProcessBalance.Qty = StockProcessBalance.Qty - L.Qty_Iss;
                //    StockProcessBalance.Qty = StockProcessBalance.Qty + L.Qty_Rec;
                //    StockProcessBalance.ObjectState = Model.ObjectState.Modified;

                //    if (Context != null)
                //        Context.StockProcessBalance.Add(StockProcessBalance);
                //    else
                //        new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance);
                //}

                StockProcessViewModel.StockProcessId = L.StockProcessId;
            }
            else
            {
                StockProcess L = new StockProcessService(_unitOfWork).Find(StockProcessViewModel.StockProcessId);

                //To Rollback Chenges in StockProcess Balance done by old entry.
                StockProcess Temp = new StockProcess();
                Temp.ProductId = L.ProductId;
                Temp.Dimension1Id = L.Dimension1Id;
                Temp.Dimension2Id = L.Dimension2Id;
                Temp.Dimension3Id = L.Dimension3Id;
                Temp.Dimension4Id = L.Dimension4Id;
                Temp.ProcessId = StockProcessViewModel.ProcessId;
                Temp.GodownId = L.GodownId;
                Temp.CostCenterId = L.CostCenterId;
                Temp.LotNo = L.LotNo;
                Temp.Qty_Iss = L.Qty_Iss;
                Temp.Qty_Rec = L.Qty_Rec;
                Temp.Weight_Iss = L.Weight_Iss;
                Temp.Weight_Rec = L.Weight_Rec;
                //new StockProcessBalanceService(_unitOfWork).UpdateStockProcessBalance(Temp);
                ///////////////////////////////////
                //StockProcessBalance StockProcessBalance_Old = new StockProcessBalanceService(_unitOfWork).Find(Temp.ProductId, Temp.Dimension1Id, Temp.Dimension2Id, Temp.Dimension3Id, Temp.Dimension4Id, Temp.ProcessId, Temp.LotNo, Temp.GodownId, Temp.CostCenterId);


                L.DocDate = StockProcessViewModel.StockProcessDocDate;
                L.ProductId = StockProcessViewModel.ProductId;
                L.ProcessId = StockProcessViewModel.ProcessId;
                L.GodownId = StockProcessViewModel.GodownId;
                L.LotNo = StockProcessViewModel.LotNo;
                L.CostCenterId = StockProcessViewModel.CostCenterId;
                L.Qty_Iss = StockProcessViewModel.Qty_Iss;
                L.Qty_Rec = StockProcessViewModel.Qty_Rec;

                L.Weight_Iss  = StockProcessViewModel.Weight_Iss;
                L.Weight_Rec = StockProcessViewModel.Weight_Rec;

                L.Rate = StockProcessViewModel.Rate;
                L.ExpiryDate = StockProcessViewModel.ExpiryDate;
                L.Specification = StockProcessViewModel.Specification;
                L.Dimension1Id = StockProcessViewModel.Dimension1Id;
                L.Dimension2Id = StockProcessViewModel.Dimension2Id;
                L.Dimension3Id = StockProcessViewModel.Dimension3Id;
                L.Dimension4Id = StockProcessViewModel.Dimension4Id;
                L.CreatedBy = StockProcessViewModel.CreatedBy;
                L.CreatedDate = StockProcessViewModel.CreatedDate;
                L.ModifiedBy = StockProcessViewModel.ModifiedBy;
                L.ModifiedDate = StockProcessViewModel.ModifiedDate;
                L.Remark = StockProcessViewModel.Remark;
                L.ObjectState = Model.ObjectState.Modified;

                if (Context != null)
                    Context.StockProcess.Add(L);
                else
                    new StockProcessService(_unitOfWork).Update(L);

                //new StockProcessBalanceService(_unitOfWork).UpdateStockProcessBalance(L);


                //StockProcessBalance StockProcessBalance_New = new StockProcessBalanceService(_unitOfWork).Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.Dimension3Id, L.Dimension4Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                //if (StockProcessBalance_New != null && StockProcessBalance_Old != null)
                //{

                //    if (StockProcessBalance_Old.StockProcessBalanceId != StockProcessBalance_New.StockProcessBalanceId)
                //    {
                //        if (StockProcessBalance_Old != null)
                //        {
                //            StockProcessBalance_Old.Qty = StockProcessBalance_Old.Qty - Temp.Qty_Rec;
                //            StockProcessBalance_Old.Qty = StockProcessBalance_Old.Qty + Temp.Qty_Iss;
                //            StockProcessBalance_Old.ObjectState = Model.ObjectState.Modified;

                //            if (Context != null)
                //                Context.StockProcessBalance.Add(StockProcessBalance_Old);
                //            else
                //                new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance_New);
                //        }

                //        if (StockProcessBalance_New == null)
                //        {
                //            StockProcessBalance StockProcessBalance_NewRecord = new StockProcessBalance();

                //            StockProcessBalance_NewRecord.ProductId = L.ProductId;
                //            StockProcessBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                //            StockProcessBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                //            StockProcessBalance_NewRecord.Dimension3Id = L.Dimension3Id;
                //            StockProcessBalance_NewRecord.Dimension4Id = L.Dimension4Id;
                //            StockProcessBalance_NewRecord.ProcessId = L.ProcessId;
                //            StockProcessBalance_NewRecord.GodownId = L.GodownId;
                //            StockProcessBalance_NewRecord.CostCenterId = L.CostCenterId;
                //            StockProcessBalance_NewRecord.LotNo = L.LotNo;
                //            if (L.Qty_Iss != 0) { StockProcessBalance_NewRecord.Qty = -L.Qty_Iss; }
                //            if (L.Qty_Rec != 0) { StockProcessBalance_NewRecord.Qty = L.Qty_Rec; }
                //            StockProcessBalance_NewRecord.ObjectState = Model.ObjectState.Added;

                //            if (Context != null)
                //                Context.StockProcessBalance.Add(StockProcessBalance_NewRecord);
                //            else
                //                new StockProcessBalanceService(_unitOfWork).Create(StockProcessBalance_NewRecord);
                //        }
                //        else
                //        {
                //            StockProcessBalance_New.Qty = StockProcessBalance_New.Qty - L.Qty_Iss;
                //            StockProcessBalance_New.Qty = StockProcessBalance_New.Qty + L.Qty_Rec;
                //            StockProcessBalance_New.ObjectState = Model.ObjectState.Modified;

                //            if (Context != null)
                //                Context.StockProcessBalance.Add(StockProcessBalance_New);
                //            else
                //                new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance_New);
                //        }
                //    }
                //    else
                //    {
                //        StockProcessBalance_New.Qty = StockProcessBalance_New.Qty + Temp.Qty_Iss - L.Qty_Iss;
                //        StockProcessBalance_New.Qty = StockProcessBalance_New.Qty - Temp.Qty_Rec + L.Qty_Rec;
                //        StockProcessBalance_New.ObjectState = Model.ObjectState.Modified;

                //        if (Context != null)
                //            Context.StockProcessBalance.Add(StockProcessBalance_New);
                //        else
                //            new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance_New);
                //    }
                //}

                StockProcessViewModel.StockProcessId = L.StockProcessId;

            }

            return ErrorText;
        }











        public void DeleteStockProcess(int StockProcessId)
        {
            StockProcess StockProcess = new StockProcessService(_unitOfWork).Find(StockProcessId);

            //StockProcessBalance StockProcessBalance = new StockProcessBalanceService(_unitOfWork).Find(StockProcess.ProductId, StockProcess.Dimension1Id, StockProcess.Dimension2Id, StockProcess.Dimension3Id, StockProcess.Dimension4Id, StockProcess.ProcessId, StockProcess.LotNo, StockProcess.GodownId, StockProcess.CostCenterId);

            //if (StockProcessBalance != null)
            //{
            //    StockProcessBalance.Qty = StockProcessBalance.Qty + StockProcess.Qty_Iss;
            //    StockProcessBalance.Qty = StockProcessBalance.Qty - StockProcess.Qty_Rec;

            //    new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance);
            //}
            new StockProcessService(_unitOfWork).Delete(StockProcessId);


        }


        public void DeleteStockProcessDB(int StockProcessId, ref ApplicationDbContext Context, bool IsDBbased)
        {

            StockProcess StockProcess = (from p in Context.StockProcess
                                         where p.StockProcessId == StockProcessId
                                         select p).FirstOrDefault();

            //StockProcessBalance StockProcessBalance = (from p in Context.StockProcessBalance
            //                                           where p.ProductId == StockProcess.ProductId &&
            //                                           p.Dimension1Id == StockProcess.Dimension1Id &&
            //                                           p.Dimension2Id == StockProcess.Dimension2Id &&
            //                                           p.Dimension3Id == StockProcess.Dimension3Id &&
            //                                           p.Dimension4Id == StockProcess.Dimension4Id &&
            //                                           p.ProcessId == StockProcess.ProcessId &&
            //                                           p.LotNo == StockProcess.LotNo &&
            //                                           p.GodownId == StockProcess.GodownId &&
            //                                           p.CostCenterId == StockProcess.CostCenterId
            //                                           select p).FirstOrDefault();

            //if (StockProcessBalance != null)
            //{
            //    StockProcessBalance.Qty = StockProcessBalance.Qty + StockProcess.Qty_Iss;
            //    StockProcessBalance.Qty = StockProcessBalance.Qty - StockProcess.Qty_Rec;
            //    StockProcessBalance.ObjectState = Model.ObjectState.Modified;

            //    if (IsDBbased)
            //        Context.StockProcessBalance.Add(StockProcessBalance);
            //    else
            //        new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance);
            //}
            StockProcess.ObjectState = Model.ObjectState.Deleted;
            if (IsDBbased)
                Context.StockProcess.Remove(StockProcess);
            else
                new StockProcessService(_unitOfWork).Delete(StockProcessId);


        }

        public IEnumerable<StockProcess> GetStockProcessForStockHeaderId(int StockHeaderId)
        {
            var temp = (from L in db.StockProcess
                        where L.StockHeaderId == StockHeaderId
                        select L).ToList();

            return temp;
        }

        public void DeleteStockProcessDBMultiple(List<int> StockProcessId, ref ApplicationDbContext Context, bool IsDBbased)
        {

            var StockProcIdArray = StockProcessId.ToArray();

            var StockProcess = (from p in Context.StockProcess
                                         where StockProcIdArray.Contains(p.StockProcessId)
                                         select p).ToList();

            var GroupedStockProc = (from p in StockProcess
                                    group p by new
                                    {
                                        p.ProductId,
                                        p.Dimension1Id,
                                        p.Dimension2Id,
                                        p.Dimension3Id,
                                        p.Dimension4Id,
                                        p.ProcessId,
                                        p.LotNo,
                                        p.GodownId,
                                        p.CostCenterId,
                                    } into g
                                    select g).ToList();

            foreach (var item in GroupedStockProc)
            {

                //StockProcessBalance StockProcessBalance = (from p in Context.StockProcessBalance
                //                                           where p.ProductId == item.Key.ProductId &&
                //                                           p.Dimension1Id == item.Key.Dimension1Id &&
                //                                           p.Dimension2Id == item.Key.Dimension2Id &&
                //                                           p.Dimension3Id == item.Key.Dimension3Id &&
                //                                           p.Dimension4Id == item.Key.Dimension4Id &&
                //                                           p.ProcessId == item.Key.ProcessId &&
                //                                           p.LotNo == item.Key.LotNo &&
                //                                           p.GodownId == item.Key.GodownId &&
                //                                           p.CostCenterId == item.Key.CostCenterId
                //                                           select p).FirstOrDefault();

                //if (StockProcessBalance != null)
                //{
                //    StockProcessBalance.Qty = StockProcessBalance.Qty + item.Sum(m => m.Qty_Iss);
                //    StockProcessBalance.Qty = StockProcessBalance.Qty - item.Sum(m => m.Qty_Rec);
                //    StockProcessBalance.ObjectState = Model.ObjectState.Modified;

                //    if (IsDBbased)
                //        Context.StockProcessBalance.Add(StockProcessBalance);
                //    else
                //        new StockProcessBalanceService(_unitOfWork).Update(StockProcessBalance);
                //}

                foreach(var IStockProc in item)
                {
                    IStockProc.ObjectState = Model.ObjectState.Deleted;
                if (IsDBbased)
                    Context.StockProcess.Remove(IStockProc);
                else
                    new StockProcessService(_unitOfWork).Delete(IStockProc);
                }
            }

        }


    }

}
