using CustomEventArgs;
using Data.Models;
using System;
using StockProcessTransferDocumentEvents;
using System.Linq;
using Core.Common;

namespace Jobs.Controllers
{
    public class StockProcessTransferEvents : StockProcessTransferDocEvents
    {
        //For Subscribing Events
        public StockProcessTransferEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += StockProcessTransferEvents__beforeHeaderSave;
            //_onHeaderSave += StockProcessTransferEvents__onHeaderSave;
            //_afterHeaderSave += StockProcessTransferEvents__afterHeaderSave;
            //_beforeHeaderDelete += StockProcessTransferEvents__beforeHeaderDelete;
            _onLineSave += StockProcessTransferEvents__onLineSave;
            //_beforeLineDelete += StockProcessTransferEvents__beforeLineDelete;
            //_afterHeaderDelete += StockProcessTransferEvents__afterHeaderDelete;
            _onLineSaveBulk += StockProcessTransferEvents__onLineSaveBulk;
            _onLineDelete += StockProcessTransferEvents__onLineDelete;
            _onHeaderDelete += StockProcessTransferEvents__onHeaderDelete;
        }

        void StockProcessTransferEvents__onHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.StockLine.Where(m => m.StockHeaderId == EventArgs.DocId).ToList();

            var StockHeader = DbContext.StockHeader.Find(EventArgs.DocId);

            //var Ledgers = (from p in DbContext.Ledger
            //               where p.LedgerHeaderId == StockHeader.LedgerHeaderId && p.CostCenterId != null
            //               group p by p.CostCenterId into g
            //               select g).ToList();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.PurjaTransfer
                           select p).FirstOrDefault();

            DbContext.Dispose();

            if (StockHeader.DocTypeId == DocType.DocumentTypeId)
            {

                var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();

                var CostCenterRecords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                var GroupedTemp = (from p in Temp
                                   where p.CostCenterId != null
                                   group p by new { p.CostCenterId } into g
                                   select g).ToList();

                foreach (var item in GroupedTemp)
                {

                    if (StockHeader.DocTypeId == DocType.DocumentTypeId && item.Sum(m => m.Qty) != 0)
                    {
                        if (item.Max(m => m.CostCenterId).HasValue)
                        {
                            var CostCenterStatus = (CostCenterRecords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId))).FirstOrDefault();
                            //var TempLedgers = Ledgers.Where(m => m.Key == CostCenterStatus.CostCenterId).FirstOrDefault();
                            if (CostCenterStatus != null)
                            {
                                CostCenterStatus.TransferQty = (CostCenterStatus.TransferQty ?? 0) - item.Sum(m => m.Qty);
                                CostCenterStatus.TransferDate = StockHeader.DocDate;
                                //CostCenterStatus.TransferAmount = (CostCenterStatus.TransferAmount ?? 0) - ((TempLedgers == null) ? 0 : TempLedgers.Select(m => m.AmtDr - m.AmtCr).Sum());
                                CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                db.CostCenterStatusExtended.Add(CostCenterStatus);
                            }
                        }
                    }
                }
            }
        }

        void StockProcessTransferEvents__onLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.StockLine.Where(m => m.StockLineId == EventArgs.DocLineId).FirstOrDefault();

            var StockHeader = DbContext.StockHeader.Find(Temp.StockHeaderId);
            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.PurjaTransfer
                           select p).FirstOrDefault();

            DbContext.Dispose();


            if (StockHeader.DocTypeId == DocType.DocumentTypeId && Temp.Qty != 0)
            {
                if (Temp.CostCenterId.HasValue)
                {
                    var CostCenterStatus = db.CostCenterStatusExtended.Find(Temp.CostCenterId);

                    if (CostCenterStatus != null)
                    {
                        CostCenterStatus.TransferQty = (CostCenterStatus.TransferQty ?? 0) - Temp.Qty;
                        CostCenterStatus.TransferDate = StockHeader.DocDate;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }

        void StockProcessTransferEvents__onLineSaveBulk(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.StockLine.Local.Where(m => m.StockHeaderId == EventArgs.DocId).ToList();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == EventArgs.DocId
                               select p
                               ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.PurjaTransfer
                           select p).FirstOrDefault();

            DbContext.Dispose();


            if (StockHeader.DocTypeId == DocType.DocumentTypeId)
            {

                var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();
                var CostCenterRecords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                var GroupedTemp = (from p in Temp
                                   where p.CostCenterId != null
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
                                CostCenterStatus.TransferQty = (CostCenterStatus.TransferQty ?? 0) + item.Sum(m => m.Qty);
                                CostCenterStatus.TransferDate = StockHeader.DocDate;
                                CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                db.CostCenterStatusExtended.Add(CostCenterStatus);
                            }
                        }
                    }
                }
            }
        }

        void StockProcessTransferEvents__afterHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool StockProcessTransferEvents__beforeLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void StockProcessTransferEvents__onLineSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.StockLine.Local.Where(m => m.StockLineId == EventArgs.DocLineId).FirstOrDefault();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == Temp.StockHeaderId
                               select p
                            ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.PurjaTransfer
                           select p).FirstOrDefault();

            DbContext.Dispose();


            if (StockHeader.DocTypeId == DocType.DocumentTypeId && Temp.Qty != 0)
            {
                if (Temp.CostCenterId.HasValue)
                {
                    var CostCenterStatus = db.CostCenterStatusExtended.Find(Temp.CostCenterId);

                    if (CostCenterStatus != null && EventArgs.DocLineId <= 0)
                    {
                        CostCenterStatus.TransferQty = (CostCenterStatus.TransferQty ?? 0) + Temp.Qty;
                        CostCenterStatus.TransferDate = StockHeader.DocDate;
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

                        CostCenterStatus.TransferQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum() + Temp.Qty;
                        CostCenterStatus.TransferDate = StockHeader.DocDate;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }

        bool StockProcessTransferEvents__beforeHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void StockProcessTransferEvents__afterHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void StockProcessTransferEvents__onHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            //CostCenterStatus Temp = new CostCenterStatus();
            //var StockProcessTransfer = db.StockProcessTransferHeader.Local.Where(m => m.StockProcessTransferHeaderId == EventArgs.DocId).FirstOrDefault();
            //Temp.CostCenterId = db.CostCenter.Local.Where(m => m.CostCenterId == StockProcessTransfer.CostCenterId).FirstOrDefault().CostCenterId;
            //Temp.ObjectState = Model.ObjectState.Added;
            //db.CostCenterStatus.Add(Temp);
        }

        bool StockProcessTransferEvents__beforeHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
