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
    public interface IStockService : IDisposable
    {
        Stock Create(Stock pt);
        void Delete(int id);
        void Update(Stock pt);
        Stock Find(int id);
        void DeleteStockForStockHeader(int StockHeaderId);
        void DeleteStockForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId, ApplicationDbContext Context);
        //string StockPost(IEnumerable<StockViewModel> StockViewModel);
        string StockPost(StockViewModel StockViewModel_New, StockViewModel StockViewModel_Old);

        string StockPostDB(ref StockViewModel StockViewModel, ref ApplicationDbContext Context);
        void DeleteStock(int StockId);
        void DeleteStockDB(int StockId, ref ApplicationDbContext Context, bool IsDBbased);

        IEnumerable<Stock> GetStockForStockHeaderId(int StockHeaderId);
    }

    public class StockService : IStockService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Stock> _StockRepository;
        RepositoryQuery<Stock> StockRepository;

        public StockService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StockRepository = new Repository<Stock>(db);
            StockRepository = new RepositoryQuery<Stock>(_StockRepository);
        }

        public Stock GetStock(int pt)
        {
            return _unitOfWork.Repository<Stock>().Find(pt);
        }

        public Stock Create(Stock pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Stock>().Insert(pt);
            return pt;
        }

        public Stock Find(int id)
        {
            return _unitOfWork.Repository<Stock>().Find(id);
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Stock>().Delete(id);
        }

        public void Delete(Stock pt)
        {
            //_unitOfWork.Repository<Stock>().Delete(pt);
            db.Stock.Remove(pt);
        }

        public void Delete(Stock pt, ApplicationDbContext Context)
        {
            //_unitOfWork.Repository<Stock>().Delete(pt);
            pt.ObjectState = Model.ObjectState.Deleted;
            Context.Stock.Attach(pt);
            Context.Stock.Remove(pt);
        }



        public void Update(Stock pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Stock>().Update(pt);
        }

        public IEnumerable<Stock> GetStockList()
        {
            var pt = _unitOfWork.Repository<Stock>().Query().Get();

            return pt;
        }

        public Stock Add(Stock pt)
        {
            _unitOfWork.Repository<Stock>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Stock>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Stock> FindAsync(int id)
        {
            throw new NotImplementedException();
        }



        public Stock Find(int StockHeaderId, int ProductId, DateTime DocDate, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId)
        {
            Stock stock = (from L in db.Stock
                           where L.StockHeaderId == StockHeaderId && L.ProductId == ProductId && L.DocDate == DocDate && L.Dimension1Id == Dimension1Id 
                                 && L.Dimension2Id == Dimension2Id
                                 && L.Dimension3Id == Dimension3Id
                                 && L.Dimension4Id == Dimension4Id 
                                 && L.ProcessId == ProcessId && L.LotNo == LotNo && L.GodownId == GodownId && L.CostCenterId == CostCenterId
                           select L).FirstOrDefault();

            return stock;
        }

        //public StockBalance FindStockBalance(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, string LotNo, int GodownId, int? CostCenterId)
        //{
        //    StockBalance stockbalance = (from L in db.StockBalance
        //                                 where L.ProductId == ProductId && L.Dimension1Id == Dimension1Id && L.Dimension2Id == Dimension2Id
        //                                 && L.Dimension3Id == Dimension3Id && L.Dimension4Id == Dimension4Id 
        //                                 && L.ProcessId == ProcessId && L.LotNo == LotNo && L.GodownId == GodownId && L.CostCenterId == CostCenterId
        //                                 select L).FirstOrDefault();
        //    return stockbalance;
        //}

        public void DeleteStockForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId, ApplicationDbContext Context)
        {
            IEnumerable<Stock> StockList = (from L in Context.Stock
                                            join H in Context.StockHeader on L.StockHeaderId equals H.StockHeaderId into StockHeaderTable
                                            from StockHeaderTab in StockHeaderTable.DefaultIfEmpty()
                                            where StockHeaderTab.DocHeaderId == DocHeaderId && StockHeaderTab.DocTypeId == DocTypeId && StockHeaderTab.SiteId == SiteId && StockHeaderTab.DivisionId == DivisionId
                                            select L).ToList();

            if (StockList != null && StockList.Count() > 0)
            {
                int i = 0;
                foreach (Stock item in StockList)
                {
                    try
                    {



                        //StockBalance stockbalance = (from L in Context.StockBalance
                        //                             where L.ProductId == item.ProductId && L.Dimension1Id == item.Dimension1Id && L.Dimension2Id == item.Dimension2Id
                        //                             && L.Dimension3Id == item.Dimension3Id && L.Dimension4Id == item.Dimension4Id 
                        //                             && L.ProcessId == item.ProcessId &&
                        //                             L.LotNo == item.LotNo && L.GodownId == item.GodownId && L.CostCenterId == item.CostCenterId
                        //                             select L).FirstOrDefault();

                        //if (stockbalance != null)
                        //{
                        //    stockbalance.Qty = stockbalance.Qty - item.Qty_Rec;
                        //    stockbalance.Qty = stockbalance.Qty + item.Qty_Iss;

                        //    if (stockbalance.Qty == 0)
                        //    {
                        //        item.ObjectState = Model.ObjectState.Deleted;
                        //        db.StockBalance.Attach(stockbalance);
                        //        Context.StockBalance.Remove(stockbalance);
                        //    }
                        //    else
                        //    {
                        //        i = i + 1;
                        //        stockbalance.ObjectState = Model.ObjectState.Modified;
                        //        Context.StockBalance.Add(stockbalance);

                        //    }
                        //}

                        Delete(item, Context);
                    }
                    catch (Exception e)
                    {
                        string str = e.Message;
                    }
                }
                new StockHeaderService(_unitOfWork).Delete(StockList.FirstOrDefault().StockHeaderId, Context);
            }
        }


        public void DeleteStockForStockHeader(int StockHeaderId)
        {
            IEnumerable<Stock> StockList = (from L in db.Stock
                                            where L.StockHeaderId == StockHeaderId
                                            select L).ToList();

            foreach (Stock item in StockList)
            {
                Delete(item);
            }
        }



        /// <summary>
        /// Create, update and delete record for whole entry.It deletes the stock posting for given docid and repost whole entry in stock.
        /// </summary>
        /// <param name="StockViewModel"></param>
        /// <returns></returns>
        //public string StockPost(IEnumerable<StockViewModel> StockViewModel)
        //{
        //    string ErrorText = "";

        //    //Get Data from Stock VIew Model and post in Stock header model for insertion or updation
        //    StockHeader StockHeader = (from L in StockViewModel
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


        //    //if record is found in Stock Header Table then it will edit it if data is not found in Stock header table than it will add a new record.
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
        //        DeleteStockForStockHeader(temp.StockHeaderId);


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

        //    IEnumerable<Stock> StockList = (from L in StockViewModel
        //                                    group new { L } by new  { L.ProductId, L.GodownId, L.CostCenterId, L.LotNo, L.ProcessId, L.Dimension1Id, L.Dimension2Id, DocDate = L.StockHeaderDocDate } into Result
        //                                    select new Stock
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


        //    foreach (Stock item in StockList)
        //    {
        //        if (temp != null)
        //        {
        //            item.StockHeaderId = temp.StockHeaderId;
        //        }

        //        Create(item);

        //        StockBalance StockBalance = new StockBalanceService(_unitOfWork).Find(item.ProductId, item.Dimension1Id, item.Dimension2Id, item.ProcessId, item.LotNo, item.GodownId, item.CostCenterId);

        //        if (StockBalance == null)
        //        {

        //            StockBalance st = new StockBalance();
        //            st.ProductId = item.ProductId;
        //            st.Dimension1Id = item.Dimension1Id;
        //            st.Dimension2Id = item.Dimension2Id;
        //            st.ProcessId = item.ProcessId;
        //            st.GodownId = item.GodownId;
        //            st.CostCenterId = item.CostCenterId;
        //            st.LotNo = item.LotNo;
        //            st.Qty = st.Qty + item.Qty_Rec;
        //            st.Qty = st.Qty - item.Qty_Iss;

        //            new StockBalanceService(_unitOfWork).Create(st);
        //        }
        //        else
        //        {
        //            StockBalance.Qty = StockBalance.Qty + item.Qty_Rec;
        //            StockBalance.Qty = StockBalance.Qty - item.Qty_Iss;
        //            new StockBalanceService(_unitOfWork).Update(StockBalance);
        //        }
        //    }

        //    return ErrorText;
        //}



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
                StockHeader = new StockHeaderService(_unitOfWork).FindByDocHeader(StockViewModel_New.DocHeaderId, StockViewModel_New.StockHeaderId, StockViewModel_New.DocTypeId, StockViewModel_New.SiteId, StockViewModel_New.DivisionId);

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

                        new StockHeaderService(_unitOfWork).Create(H);

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

                        new StockHeaderService(_unitOfWork).Update(StockHeader);
                    }
                }
            }
            else
            {
                StockHeader = new StockHeaderService(_unitOfWork).FindByDocHeader(StockViewModel_Old.DocHeaderId, StockViewModel_Old.StockHeaderId, StockViewModel_Old.DocTypeId, StockViewModel_Old.SiteId, StockViewModel_Old.DivisionId);
            }


            if (StockViewModel_Old != null)
            {
                Stock Stock_Old = Find(StockViewModel_Old.StockHeaderId, StockViewModel_Old.ProductId, StockViewModel_Old.StockDocDate, StockViewModel_Old.Dimension1Id, StockViewModel_Old.Dimension2Id, StockViewModel_Old.Dimension3Id, StockViewModel_Old.Dimension4Id, StockViewModel_Old.ProcessId, StockViewModel_Old.LotNo, StockViewModel_Old.GodownId, StockViewModel_Old.CostCenterId);

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

                    //StockBalance StockBalance_Old = FindStockBalance(StockViewModel_Old.ProductId, StockViewModel_Old.Dimension1Id, StockViewModel_Old.Dimension2Id, StockViewModel_Old.Dimension3Id, StockViewModel_Old.Dimension4Id, StockViewModel_Old.ProcessId, StockViewModel_Old.LotNo, StockViewModel_Old.GodownId, StockViewModel_Old.CostCenterId);

                    //if (StockBalance_Old != null)
                    //{
                    //    StockBalance_Old.Qty = StockBalance_Old.Qty - StockViewModel_Old.Qty_Rec;
                    //    StockBalance_Old.Qty = StockBalance_Old.Qty + StockViewModel_Old.Qty_Iss;

                    //    if (StockBalance_Old.Qty == 0) { new StockBalanceService(_unitOfWork).Delete(StockBalance_Old); }
                    //    else { new StockBalanceService(_unitOfWork).Update(StockBalance_Old); }
                    //}
                }


            }

            if (StockViewModel_New != null)
            {
                Stock Stock_New;

                if (StockHeader != null)
                {
                    Stock_New = Find(StockHeader.StockHeaderId, StockViewModel_New.ProductId, StockViewModel_New.StockDocDate, StockViewModel_New.Dimension1Id, StockViewModel_New.Dimension2Id, StockViewModel_New.Dimension3Id, StockViewModel_New.Dimension4Id, StockViewModel_New.ProcessId, StockViewModel_New.LotNo, StockViewModel_New.GodownId, StockViewModel_New.CostCenterId);
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
                    L.Dimension3Id = StockViewModel_New.Dimension3Id;
                    L.Dimension4Id = StockViewModel_New.Dimension4Id;
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

                //StockBalance StockBalance_New = FindStockBalance(StockViewModel_New.ProductId, StockViewModel_New.Dimension1Id, StockViewModel_New.Dimension2Id, StockViewModel_New.Dimension3Id, StockViewModel_New.Dimension4Id, StockViewModel_New.ProcessId, StockViewModel_New.LotNo, StockViewModel_New.GodownId, StockViewModel_New.CostCenterId);

                //if (StockBalance_New == null)
                //{

                //    StockBalance Sb = new StockBalance();

                //    Sb.ProductId = StockViewModel_New.ProductId;
                //    Sb.Dimension1Id = StockViewModel_New.Dimension1Id;
                //    Sb.Dimension2Id = StockViewModel_New.Dimension2Id;
                //    Sb.Dimension3Id = StockViewModel_New.Dimension3Id;
                //    Sb.Dimension4Id = StockViewModel_New.Dimension4Id;
                //    Sb.ProcessId = StockViewModel_New.ProcessId;
                //    Sb.GodownId = StockViewModel_New.GodownId;
                //    Sb.CostCenterId = StockViewModel_New.CostCenterId;
                //    Sb.LotNo = StockViewModel_New.LotNo;
                //    if (StockViewModel_New.Qty_Iss != 0) { Sb.Qty = StockViewModel_New.Qty_Iss; }
                //    if (StockViewModel_New.Qty_Rec != 0) { Sb.Qty = StockViewModel_New.Qty_Rec; }

                //    new StockBalanceService(_unitOfWork).Create(Sb);
                //}
                //else
                //{
                //    StockBalance_New.Qty = StockBalance_New.Qty + StockViewModel_New.Qty_Rec;
                //    StockBalance_New.Qty = StockBalance_New.Qty - StockViewModel_New.Qty_Iss;

                //    new StockBalanceService(_unitOfWork).Update(StockBalance_New);
                //}


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

                new StockHeaderService(_unitOfWork).Create(H);

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
                L.Dimension3Id = StockViewModel.Dimension3Id;
                L.Dimension4Id = StockViewModel.Dimension4Id;
                L.CreatedBy = StockViewModel.CreatedBy;
                L.CreatedDate = StockViewModel.CreatedDate;
                L.ModifiedBy = StockViewModel.ModifiedBy;
                L.ModifiedDate = StockViewModel.ModifiedDate;

                L.ObjectState = Model.ObjectState.Added;
                new StockService(_unitOfWork).Create(L);

                //new StockBalanceService(_unitOfWork).UpdateStockBalance(L);


                //StockBalance StockBalance = new StockBalanceService(_unitOfWork).Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.Dimension3Id, L.Dimension4Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                //if (StockBalance == null)
                //{
                //    StockBalance StockBalance_NewRecord = new StockBalance();

                //    StockBalance_NewRecord.ProductId = L.ProductId;
                //    StockBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                //    StockBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                //    StockBalance_NewRecord.Dimension3Id = L.Dimension3Id;
                //    StockBalance_NewRecord.Dimension4Id = L.Dimension4Id;
                //    StockBalance_NewRecord.ProcessId = L.ProcessId;
                //    StockBalance_NewRecord.GodownId = L.GodownId;
                //    StockBalance_NewRecord.CostCenterId = L.CostCenterId;
                //    StockBalance_NewRecord.LotNo = L.LotNo;
                //    if (L.Qty_Iss != 0) { StockBalance_NewRecord.Qty = -L.Qty_Iss; }
                //    if (L.Qty_Rec != 0) { StockBalance_NewRecord.Qty = L.Qty_Rec; }



                //    new StockBalanceService(_unitOfWork).Create(StockBalance_NewRecord);
                //}
                //else
                //{
                //    StockBalance.Qty = StockBalance.Qty - L.Qty_Iss;
                //    StockBalance.Qty = StockBalance.Qty + L.Qty_Rec;



                //    new StockBalanceService(_unitOfWork).Update(StockBalance);
                //}



                StockViewModel.StockId = L.StockId;


            }
            else
            {
                Stock L = new StockService(_unitOfWork).Find(StockViewModel.StockId);

                //To Rollback Chenges in Stock Balance done by old entry.
                Stock Temp = new Stock();
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
                //new StockBalanceService(_unitOfWork).UpdateStockBalance(Temp);
                ///////////////////////////////////
                //StockBalance StockBalance_Old = new StockBalanceService(_unitOfWork).Find(Temp.ProductId, Temp.Dimension1Id, Temp.Dimension2Id, Temp.Dimension3Id, Temp.Dimension4Id, Temp.ProcessId, Temp.LotNo, Temp.GodownId, Temp.CostCenterId);


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
                L.Dimension3Id = StockViewModel.Dimension3Id;
                L.Dimension4Id = StockViewModel.Dimension4Id;
                L.CreatedBy = StockViewModel.CreatedBy;
                L.CreatedDate = StockViewModel.CreatedDate;
                L.ModifiedBy = StockViewModel.ModifiedBy;
                L.ModifiedDate = StockViewModel.ModifiedDate;

                new StockService(_unitOfWork).Update(L);

                //new StockBalanceService(_unitOfWork).UpdateStockBalance(L);


                //StockBalance StockBalance_New = new StockBalanceService(_unitOfWork).Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.Dimension3Id, L.Dimension4Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                //if (StockBalance_New != null)
                //{
                //    if (StockBalance_Old.StockBalanceId != StockBalance_New.StockBalanceId)
                //    {
                //        if (StockBalance_Old != null)
                //        {
                //            StockBalance_Old.Qty = StockBalance_Old.Qty - Temp.Qty_Rec;
                //            StockBalance_Old.Qty = StockBalance_Old.Qty + Temp.Qty_Iss;

                //            new StockBalanceService(_unitOfWork).Update(StockBalance_New);
                //        }

                //        if (StockBalance_New == null)
                //        {
                //            StockBalance StockBalance_NewRecord = new StockBalance();

                //            StockBalance_NewRecord.ProductId = L.ProductId;
                //            StockBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                //            StockBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                //            StockBalance_NewRecord.Dimension3Id = L.Dimension3Id;
                //            StockBalance_NewRecord.Dimension4Id = L.Dimension4Id;
                //            StockBalance_NewRecord.ProcessId = L.ProcessId;
                //            StockBalance_NewRecord.GodownId = L.GodownId;
                //            StockBalance_NewRecord.CostCenterId = L.CostCenterId;
                //            StockBalance_NewRecord.LotNo = L.LotNo;
                //            if (L.Qty_Iss != 0) { StockBalance_NewRecord.Qty = -L.Qty_Iss; }
                //            if (L.Qty_Rec != 0) { StockBalance_NewRecord.Qty = L.Qty_Rec; }

                //            new StockBalanceService(_unitOfWork).Create(StockBalance_NewRecord);
                //        }
                //        else
                //        {
                //            StockBalance_New.Qty = StockBalance_New.Qty - L.Qty_Iss;
                //            StockBalance_New.Qty = StockBalance_New.Qty + L.Qty_Rec;

                //            new StockBalanceService(_unitOfWork).Update(StockBalance_New);
                //        }
                //    }
                //    else
                //    {
                //        StockBalance_New.Qty = StockBalance_New.Qty + Temp.Qty_Iss - L.Qty_Iss;
                //        StockBalance_New.Qty = StockBalance_New.Qty - Temp.Qty_Rec + L.Qty_Rec;

                //        new StockBalanceService(_unitOfWork).Update(StockBalance_New);
                //    }
                //}

                StockViewModel.StockId = L.StockId;

            }

            return ErrorText;
        }





        public string StockPostDB(ref StockViewModel StockViewModel, ref ApplicationDbContext Context)
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

                if (Context != null)
                    Context.StockHeader.Add(H);
                else
                    new StockHeaderService(_unitOfWork).Create(H);

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
                L.DocLineId = StockViewModel.DocLineId;
                L.DocDate = StockViewModel.StockDocDate;
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
                L.Dimension3Id = StockViewModel.Dimension3Id;
                L.Dimension4Id = StockViewModel.Dimension4Id;
                L.StockStatus = StockViewModel.StockStatus;
                L.Remark = StockViewModel.Remark;
                L.CreatedBy = StockViewModel.CreatedBy;
                L.CreatedDate = StockViewModel.CreatedDate;
                L.ModifiedBy = StockViewModel.ModifiedBy;
                L.ModifiedDate = StockViewModel.ModifiedDate;
                L.ReferenceDocId = StockViewModel.ReferenceDocId;
                L.ReferenceDocTypeId = StockViewModel.ReferenceDocTypeId;

                L.ObjectState = Model.ObjectState.Added;

                if (Context != null)
                    Context.Stock.Add(L);
                else
                    new StockService(_unitOfWork).Create(L);

                //new StockBalanceService(_unitOfWork).UpdateStockBalance(L);


                //StockBalance StockBalance = new StockBalanceService(_unitOfWork).Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.Dimension3Id, L.Dimension4Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);

                //if (StockBalance == null)
                //{
                //    StockBalance StockBalance_NewRecord = new StockBalance();

                //    StockBalance_NewRecord.ProductId = L.ProductId;
                //    StockBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                //    StockBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                //    StockBalance_NewRecord.Dimension3Id = L.Dimension3Id;
                //    StockBalance_NewRecord.Dimension4Id = L.Dimension4Id;
                //    StockBalance_NewRecord.ProcessId = L.ProcessId;
                //    StockBalance_NewRecord.GodownId = L.GodownId;
                //    StockBalance_NewRecord.CostCenterId = L.CostCenterId;
                //    StockBalance_NewRecord.LotNo = L.LotNo;
                //    if (L.Qty_Iss != 0) { StockBalance_NewRecord.Qty = -L.Qty_Iss; }
                //    if (L.Qty_Rec != 0) { StockBalance_NewRecord.Qty = L.Qty_Rec; }
                //    StockBalance_NewRecord.ObjectState = Model.ObjectState.Added;

                //    if (Context != null)
                //        Context.StockBalance.Add(StockBalance_NewRecord);
                //    else
                //        new StockBalanceService(_unitOfWork).Create(StockBalance_NewRecord);
                //}
                //else
                //{
                //    StockBalance.Qty = StockBalance.Qty - L.Qty_Iss;
                //    StockBalance.Qty = StockBalance.Qty + L.Qty_Rec;
                //    StockBalance.ObjectState = Model.ObjectState.Modified;

                //    if (Context != null)
                //        Context.StockBalance.Add(StockBalance);
                //    else
                //        new StockBalanceService(_unitOfWork).Update(StockBalance);
                //}

                StockViewModel.StockId = L.StockId;
            }
            else
            {
                Stock L = new StockService(_unitOfWork).Find(StockViewModel.StockId);

                //To Rollback Chenges in Stock Balance done by old entry.
                Stock Temp = new Stock();
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
                Temp.Weight_Iss = L.Weight_Iss;
                Temp.Weight_Rec = L.Weight_Rec;



                L.DocDate = StockViewModel.StockDocDate;
                L.ProductId = StockViewModel.ProductId;
                L.ProcessId = StockViewModel.ProcessId;
                L.GodownId = StockViewModel.GodownId;
                L.LotNo = StockViewModel.LotNo;
                L.CostCenterId = StockViewModel.CostCenterId;
                L.Qty_Iss = StockViewModel.Qty_Iss;
                L.Qty_Rec = StockViewModel.Qty_Rec;
                L.Weight_Iss = StockViewModel.Weight_Iss;
                L.Weight_Rec = StockViewModel.Weight_Rec;
                L.Rate = StockViewModel.Rate;
                L.ExpiryDate = StockViewModel.ExpiryDate;
                L.Specification = StockViewModel.Specification;
                L.Dimension1Id = StockViewModel.Dimension1Id;
                L.Dimension2Id = StockViewModel.Dimension2Id;
                L.Dimension3Id = StockViewModel.Dimension3Id;
                L.Dimension4Id = StockViewModel.Dimension4Id;
                L.StockStatus = StockViewModel.StockStatus;
                L.Remark = StockViewModel.Remark;
                L.CreatedBy = StockViewModel.CreatedBy;
                L.CreatedDate = StockViewModel.CreatedDate;
                L.ModifiedBy = StockViewModel.ModifiedBy;
                L.ModifiedDate = StockViewModel.ModifiedDate;
                L.ReferenceDocId = StockViewModel.ReferenceDocId;
                L.ReferenceDocTypeId = StockViewModel.ReferenceDocTypeId;
                L.ObjectState = Model.ObjectState.Modified;

                if (Context != null)
                    Context.Stock.Add(L);
                else
                    new StockService(_unitOfWork).Update(L);

                //new StockBalanceService(_unitOfWork).UpdateStockBalance(L);


                //StockBalance StockBalance_Old = new StockBalanceService(_unitOfWork).Find(Temp.ProductId, Temp.Dimension1Id, Temp.Dimension2Id, Temp.Dimension3Id, Temp.Dimension4Id, Temp.ProcessId, Temp.LotNo, Temp.GodownId, Temp.CostCenterId);

                //if (StockBalance_Old != null)
                //{
                //    StockBalance_Old.Qty = StockBalance_Old.Qty - Temp.Qty_Rec;
                //    StockBalance_Old.Qty = StockBalance_Old.Qty + Temp.Qty_Iss;
                //    StockBalance_Old.ObjectState = Model.ObjectState.Modified;

                //    if (Context != null)
                //        Context.StockBalance.Add(StockBalance_Old);
                //    else
                //        new StockBalanceService(_unitOfWork).Update(StockBalance_Old);
                //}


                //StockBalance StockBalance_New = new StockBalanceService(_unitOfWork).Find(L.ProductId, L.Dimension1Id, L.Dimension2Id, L.Dimension3Id, L.Dimension4Id, L.ProcessId, L.LotNo, L.GodownId, L.CostCenterId);


                //if (StockBalance_New == null)
                //{
                //    StockBalance StockBalance_NewRecord = new StockBalance();

                //    StockBalance_NewRecord.ProductId = L.ProductId;
                //    StockBalance_NewRecord.Dimension1Id = L.Dimension1Id;
                //    StockBalance_NewRecord.Dimension2Id = L.Dimension2Id;
                //    StockBalance_NewRecord.Dimension3Id = L.Dimension3Id;
                //    StockBalance_NewRecord.Dimension4Id = L.Dimension4Id;
                //    StockBalance_NewRecord.ProcessId = L.ProcessId;
                //    StockBalance_NewRecord.GodownId = L.GodownId;
                //    StockBalance_NewRecord.CostCenterId = L.CostCenterId;
                //    StockBalance_NewRecord.LotNo = L.LotNo;
                //    if (L.Qty_Iss != 0) { StockBalance_NewRecord.Qty = -L.Qty_Iss; }
                //    if (L.Qty_Rec != 0) { StockBalance_NewRecord.Qty = L.Qty_Rec; }
                //    StockBalance_NewRecord.ObjectState = Model.ObjectState.Added;

                //    if (Context != null)
                //        Context.StockBalance.Add(StockBalance_NewRecord);
                //    else
                //        new StockBalanceService(_unitOfWork).Create(StockBalance_NewRecord);
                //}
                //else
                //{
                //    StockBalance_New.Qty = StockBalance_New.Qty - L.Qty_Iss;
                //    StockBalance_New.Qty = StockBalance_New.Qty + L.Qty_Rec;
                //    StockBalance_New.ObjectState = Model.ObjectState.Modified;

                //    if (Context != null)
                //        Context.StockBalance.Add(StockBalance_New);
                //    else
                //        new StockBalanceService(_unitOfWork).Update(StockBalance_New);
                //}

                StockViewModel.StockId = L.StockId;

            }

            return ErrorText;
        }




        public void DeleteStock(int StockId)
        {
            Stock Stock = new StockService(_unitOfWork).Find(StockId);

            //StockBalance StockBalance = new StockBalanceService(_unitOfWork).Find(Stock.ProductId, Stock.Dimension1Id, Stock.Dimension2Id, Stock.Dimension3Id, Stock.Dimension4Id, Stock.ProcessId, Stock.LotNo, Stock.GodownId, Stock.CostCenterId);

            //if (StockBalance != null)
            //{
            //    StockBalance.Qty = StockBalance.Qty + Stock.Qty_Iss;
            //    StockBalance.Qty = StockBalance.Qty - Stock.Qty_Rec;

            //    new StockBalanceService(_unitOfWork).Update(StockBalance);
            //}

            new StockService(_unitOfWork).Delete(StockId);
        }

        public void DeleteStockDB(int StockId, ref ApplicationDbContext Context, bool IsDBbased)
        {
            Stock Stock = (from p in Context.Stock
                           where p.StockId == StockId
                           select p).FirstOrDefault();

            //StockBalance StockBalance = (from p in Context.StockBalance
            //                             where p.ProductId == Stock.ProductId &&
            //                           p.Dimension1Id == Stock.Dimension1Id &&
            //                           p.Dimension2Id == Stock.Dimension2Id &&
            //                           p.Dimension3Id == Stock.Dimension3Id &&
            //                           p.Dimension4Id == Stock.Dimension4Id &&
            //                           p.ProcessId == Stock.ProcessId &&
            //                           p.LotNo == Stock.LotNo &&
            //                           p.GodownId == Stock.GodownId &&
            //                           p.CostCenterId == Stock.CostCenterId
            //                             select p).FirstOrDefault();

            //if (StockBalance != null)
            //{
            //    StockBalance.Qty = StockBalance.Qty + Stock.Qty_Iss;
            //    StockBalance.Qty = StockBalance.Qty - Stock.Qty_Rec;
            //    StockBalance.ObjectState = Model.ObjectState.Modified;

            //    if (IsDBbased)
            //        Context.StockBalance.Add(StockBalance);
            //    else
            //        new StockBalanceService(_unitOfWork).Update(StockBalance);
            //}
            Stock.ObjectState = Model.ObjectState.Deleted;
            if (IsDBbased)
                Context.Stock.Remove(Stock);
            else
                new StockService(_unitOfWork).Delete(StockId);
        }

        public void DeleteStockDBMultiple(List<int> StockId, ref ApplicationDbContext Context, bool IsDBbased)
        {

            var StockIdArray = StockId.ToArray();

            var Stock = (from p in Context.Stock
                         where StockIdArray.Contains(p.StockId)
                         select p).ToList();


            var GroupedStock = (from p in Stock
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
                                    p.CostCenterId
                                } into g
                                select g).ToList();

            foreach (var item in GroupedStock)
            {
                //StockBalance StockBalance = (from p in Context.StockBalance
                //                             where p.ProductId == item.Key.ProductId &&
                //                           p.Dimension1Id == item.Key.Dimension1Id &&
                //                           p.Dimension2Id == item.Key.Dimension2Id &&
                //                           p.Dimension3Id == item.Key.Dimension3Id &&
                //                           p.Dimension4Id == item.Key.Dimension4Id &&
                //                           p.ProcessId == item.Key.ProcessId &&
                //                           p.LotNo == item.Key.LotNo &&
                //                           p.GodownId == item.Key.GodownId &&
                //                           p.CostCenterId == item.Key.CostCenterId
                //                             select p).FirstOrDefault();

                //if (StockBalance != null)
                //{
                //    StockBalance.Qty = StockBalance.Qty + item.Sum(m => m.Qty_Iss);
                //    StockBalance.Qty = StockBalance.Qty - item.Sum(m => m.Qty_Rec);
                //    StockBalance.ObjectState = Model.ObjectState.Modified;

                //    if (IsDBbased)
                //        Context.StockBalance.Add(StockBalance);
                //    else
                //        new StockBalanceService(_unitOfWork).Update(StockBalance);
                //}


                foreach (var IStock in item)
                {
                    IStock.ObjectState = Model.ObjectState.Deleted;
                    if (IsDBbased)
                        Context.Stock.Remove(IStock);
                    else
                        new StockService(_unitOfWork).Delete(IStock);
                }
            }


        }

        public IEnumerable<Stock> GetStockForStockHeaderId(int StockHeaderId)
        {
            var temp = (from L in db.Stock
                        where L.StockHeaderId == StockHeaderId
                        select L).ToList();

            return temp;
        }

        public void UpdateStockGodownId(int? StockHeaderId, int? NewGodownId, DateTime NewDocDate, ApplicationDbContext db)
        {
            if (StockHeaderId.HasValue)
            {
                var StockList = db.Stock.Where(m => m.StockHeaderId == StockHeaderId).ToList();

                foreach (var item in StockList)
                {
                    if (NewGodownId.HasValue)
                        item.GodownId = NewGodownId.Value;
                    item.DocDate = NewDocDate;
                    item.ObjectState = Model.ObjectState.Modified;
                    db.Stock.Add(item);
                }
            }
        }
    }
}
