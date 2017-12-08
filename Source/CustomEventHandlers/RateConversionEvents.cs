using CustomEventArgs;
using Data.Models;
using System;
using RateConversionDocumentEvents;
using System.Linq;
using Core.Common;

namespace Jobs.Controllers
{
    public class RateConversionEvents : RateConversionDocEvents
    {
        //For Subscribing Events
        public RateConversionEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += RateConversionEvents__beforeHeaderSave;
            //_onHeaderSave += RateConversionEvents__onHeaderSave;
            //_afterHeaderSave += RateConversionEvents__afterHeaderSave;
            //_beforeHeaderDelete += RateConversionEvents__beforeHeaderDelete;
            _onLineSave += RateConversionEvents__onLineSave;
            //_beforeLineDelete += RateConversionEvents__beforeLineDelete;
            //_afterHeaderDelete += RateConversionEvents__afterHeaderDelete;
            _onLineSaveBulk += RateConversionEvents__onLineSaveBulk;
            _onLineDelete += RateConversionEvents__onLineDelete;
            _onHeaderDelete += RateConversionEvents__onHeaderDelete;
            _onHeaderSubmit += RateConversionEvents__onHeaderSubmit;
        }

        void RateConversionEvents__onHeaderSubmit(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var StockHeader = db.StockHeader.Local.Where(m => m.StockHeaderId == EventArgs.DocId).FirstOrDefault();
            var Temp = db.Ledger.Local.Where(m => m.LedgerHeaderId == StockHeader.LedgerHeaderId).ToList();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            //var LedgerHeader = (from p in DbContext.LedgerHeader
            //                   where p.LedgerHeaderId == EventArgs.DocId
            //                   select p
            //                ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.RateConversion
                           select p).FirstOrDefault();

            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();

            var LedgersForCostCenters = (from p in DbContext.Ledger
                                         join t in DbContext.LedgerHeader on p.LedgerHeaderId equals t.LedgerHeaderId
                                         where CostCenterIds.Contains(p.CostCenterId) && p.LedgerHeaderId != StockHeader.LedgerHeaderId
                                         && t.DocTypeId == DocType.DocumentTypeId && p.CostCenterId != null
                                         group p by p.CostCenterId into g
                                         select g).ToList();


            DbContext.Dispose();

            var GroupedTemp = (from p in Temp
                               where p.CostCenterId != null
                               group p by p.CostCenterId
                                   into g
                                   select g).ToList();


            if (DocType.DocumentTypeId == StockHeader.DocTypeId)
            {
                var CostCenterREcords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                foreach (var item in GroupedTemp)
                {
                    if (item.Max(m => m.CostCenterId).HasValue)
                    {
                        var CostCenterStatus = CostCenterREcords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

                        var ExistingLedgers = LedgersForCostCenters.Where(m => m.Key == item.Key).FirstOrDefault();

                        CostCenterStatus.RateSettlementAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Select(m => m.AmtDr - m.AmtCr).Sum())
                            + item.Select(m => m.AmtDr - m.AmtCr).Sum();

                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);

                    }

                }

            }
        }

        void RateConversionEvents__onHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.StockLine.Where(m => m.StockHeaderId == EventArgs.DocId).ToList();

            var StockHeader = DbContext.StockHeader.Find(EventArgs.DocId);

            var Ledgers = (from p in DbContext.Ledger
                           where p.LedgerHeaderId == StockHeader.LedgerHeaderId && p.CostCenterId != null
                           group p by p.CostCenterId into g
                           select g).ToList();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.RateConversion
                           select p).FirstOrDefault();

            DbContext.Dispose();


            if (StockHeader.DocTypeId == DocType.DocumentTypeId)
            {

                var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();

                var CostCenterRecords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                var GroupedTemp = (from p in Temp
                                   group p by new { p.CostCenterId } into g
                                   select g).ToList();

                foreach (var item in GroupedTemp)
                {

                    if (StockHeader.DocTypeId == DocType.DocumentTypeId && item.Sum(m => m.Qty) != 0)
                    {
                        if (item.Max(m => m.CostCenterId).HasValue)
                        {
                            var CostCenterStatus = (CostCenterRecords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId))).FirstOrDefault();
                            var TempLedgers = Ledgers.Where(m => m.Key == CostCenterStatus.CostCenterId).FirstOrDefault();
                            if (CostCenterStatus != null)
                            {
                                CostCenterStatus.RateSettlementQty = (CostCenterStatus.RateSettlementQty ?? 0) - item.Sum(m => m.Qty);
                                CostCenterStatus.RateSettlementDate = StockHeader.DocDate;
                                CostCenterStatus.RateSettlementAmount = (CostCenterStatus.RateSettlementAmount ?? 0) - ((TempLedgers == null) ? 0 : TempLedgers.Select(m => m.AmtDr - m.AmtCr).Sum());
                                CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                db.CostCenterStatusExtended.Add(CostCenterStatus);
                            }
                        }
                    }
                }
            }
        }

        void RateConversionEvents__onLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.StockLine.Where(m => m.StockLineId == EventArgs.DocLineId).FirstOrDefault();

            var StockHeader = DbContext.StockHeader.Find(Temp.StockHeaderId);
            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.RateConversion
                           select p).FirstOrDefault();

            DbContext.Dispose();


            if (StockHeader.DocTypeId == DocType.DocumentTypeId && Temp.Qty != 0)
            {
                if (Temp.CostCenterId.HasValue)
                {
                    var CostCenterStatus = db.CostCenterStatusExtended.Find(Temp.CostCenterId);

                    if (CostCenterStatus != null)
                    {
                        CostCenterStatus.RateSettlementQty = (CostCenterStatus.RateSettlementQty ?? 0) - Temp.Qty;
                        //CostCenterStatus.RateSettlementAmount = (CostCenterStatus.RateSettlementAmount ?? 0) - Temp.Amount;
                        CostCenterStatus.RateSettlementDate = StockHeader.DocDate;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }

        void RateConversionEvents__onLineSaveBulk(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.StockLine.Local.Where(m => m.StockHeaderId == EventArgs.DocId).ToList();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == EventArgs.DocId
                               select p
                               ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.RateConversion
                           select p).FirstOrDefault();

            DbContext.Dispose();

            if (StockHeader.DocTypeId == DocType.DocumentTypeId)
            {
                var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();
                var CostCenterRecords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                var GroupedTemp = (from p in Temp
                                   group p by p.CostCenterId into g
                                   select g).ToList();

                foreach (var item in GroupedTemp)
                {
                    if (StockHeader.DocTypeId == DocType.DocumentTypeId && item.Sum(m => m.Qty) != 0)
                    {
                        if (item.Max(m => m.CostCenterId).HasValue)
                        {
                            var CostCenterStatus = (CostCenterRecords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId))).FirstOrDefault();

                            if (CostCenterStatus != null)
                            {
                                CostCenterStatus.RateSettlementQty = (CostCenterStatus.RateSettlementQty ?? 0) + item.Sum(m => m.Qty);
                                //CostCenterStatus.RateSettlementAmount = (CostCenterStatus.RateSettlementAmount ?? 0) + item.Sum(m => m.Amount);
                                CostCenterStatus.RateSettlementDate = StockHeader.DocDate;
                                CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                db.CostCenterStatusExtended.Add(CostCenterStatus);
                            }
                        }
                    }
                }
            }
        }

        void RateConversionEvents__afterHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool RateConversionEvents__beforeLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void RateConversionEvents__onLineSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.StockLine.Local.Where(m => m.StockLineId == EventArgs.DocLineId).FirstOrDefault();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == Temp.StockHeaderId
                               select p
                            ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.RateConversion
                           select p).FirstOrDefault();

            DbContext.Dispose();


            if (StockHeader.DocTypeId == DocType.DocumentTypeId && Temp.Qty != 0)
            {
                if (Temp.CostCenterId.HasValue)
                {
                    var CostCenterStatus = db.CostCenterStatusExtended.Find(Temp.CostCenterId);

                    if (CostCenterStatus != null && EventArgs.DocLineId <= 0)
                    {
                        CostCenterStatus.RateSettlementQty = (CostCenterStatus.RateSettlementQty ?? 0) + Temp.Qty;
                        //CostCenterStatus.RateSettlementAmount = (CostCenterStatus.RateSettlementAmount ?? 0) + Temp.Amount;
                        CostCenterStatus.RateSettlementDate = StockHeader.DocDate;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                    else if (CostCenterStatus != null && EventArgs.DocLineId > 0)
                    {

                        var IssueLineCostCenterRecords = (from p in db.StockLine
                                                          join t in db.StockHeader on p.StockHeaderId equals t.StockHeaderId
                                                          where p.CostCenterId == Temp.CostCenterId && t.DocTypeId == DocType.DocumentTypeId
                                                          && p.StockLineId != EventArgs.DocLineId
                                                          select p).ToList();

                        CostCenterStatus.RateSettlementQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum() + Temp.Qty;
                        //CostCenterStatus.RateSettlementAmount = IssueLineCostCenterRecords.Select(m => m.Amount).Sum() + Temp.Amount;
                        CostCenterStatus.RateSettlementDate = StockHeader.DocDate;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }

        bool RateConversionEvents__beforeHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void RateConversionEvents__afterHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void RateConversionEvents__onHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            //CostCenterStatus Temp = new CostCenterStatus();
            //var RateConversion = db.RateConversionHeader.Local.Where(m => m.RateConversionHeaderId == EventArgs.DocId).FirstOrDefault();
            //Temp.CostCenterId = db.CostCenter.Local.Where(m => m.CostCenterId == RateConversion.CostCenterId).FirstOrDefault().CostCenterId;
            //Temp.ObjectState = Model.ObjectState.Added;
            //db.CostCenterStatus.Add(Temp);
        }

        bool RateConversionEvents__beforeHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
