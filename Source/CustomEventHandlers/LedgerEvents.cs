using CustomEventArgs;
using Data.Models;
using System;
using LedgerDocumentEvents;
using System.Linq;
using Core.Common;

namespace Jobs.Controllers
{
    public class LedgerEvents : LedgerDocEvents
    {
        //For Subscribing Events
        public LedgerEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += LedgerEvents__beforeHeaderSave;
            //_onHeaderSave += LedgerEvents__onHeaderSave;
            //_afterHeaderSave += LedgerEvents__afterHeaderSave;
            //_beforeHeaderDelete += LedgerEvents__beforeHeaderDelete;
            _onLineSave += LedgerEvents__onLineSave;
            //_beforeLineDelete += LedgerEvents__beforeLineDelete;
            //_afterHeaderDelete += LedgerEvents__afterHeaderDelete;
            //_onLineSaveBulk += LedgerEvents__onLineSaveBulk;
            _onLineDelete += LedgerEvents__onLineDelete;
            //_onHeaderSubmit += LedgerEvents__onHeaderSubmit;
            _onHeaderDelete += LedgerEvents__onHeaderDelete;
            _onWizardSave += LedgerEvents__onWizardSave;
        }

        void LedgerEvents__onWizardSave(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        {

            var Header = db.LedgerHeader.Local.Where(m => m.LedgerHeaderId == EventArgs.DocId).FirstOrDefault();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var DocType = (from p in DbContext.DocumentType.AsNoTracking()
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingChequeCancel
                           select p).FirstOrDefault();

            DbContext.Dispose();

            //ForPosting in CostcenterStatusExtended
            if (Header.DocTypeId == DocType.DocumentTypeId)
            {
                var sLedgers = db.Ledger.Local.Where(m => m.LedgerHeaderId == Header.LedgerHeaderId).ToList();

                var Group = (from p in sLedgers
                             group p by p.CostCenterId into g
                             select g).ToList();

                var CostCenterIds = Group.Select(m => m.Key).ToArray();

                var CostCenterStatusExtended = (from p in db.CostCenterStatusExtended
                                                where CostCenterIds.Contains(p.CostCenterId)
                                                select p).ToList();

                foreach (var item in CostCenterStatusExtended)
                {

                    item.PaymentCancelAmount = (item.PaymentCancelAmount ?? 0) + Group.Where(m => m.Key == item.CostCenterId).FirstOrDefault().Sum(m => m.AmtCr);
                    item.ObjectState = Model.ObjectState.Modified;
                    db.CostCenterStatusExtended.Add(item);
                }

            }
        }

        #region Submit
        //void LedgerEvents__onHeaderSubmit(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        //{
        //    var Temp = db.Ledger.Local.Where(m => m.LedgerHeaderId == EventArgs.DocId).ToList();

        //    ApplicationDbContext DbContext = new ApplicationDbContext();

        //    var LedgerHeader = (from p in DbContext.LedgerHeader
        //                        where p.LedgerHeaderId == EventArgs.DocId
        //                        select p
        //                    ).FirstOrDefault();

        //    var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();

        //    var LedgersForCostCenters = (from p in DbContext.Ledger
        //                                 join t in DbContext.LedgerHeader on p.LedgerHeaderId equals t.LedgerHeaderId
        //                                 where CostCenterIds.Contains(p.CostCenterId) && p.LedgerHeaderId != LedgerHeader.LedgerHeaderId
        //                                 && t.DocTypeId == LedgerHeader.DocTypeId && p.CostCenterId != null
        //                                 group p by p.CostCenterId into g
        //                                 select g).ToList();

        //    var DocType = (from p in DbContext.DocumentType
        //                   where p.DocumentTypeName == TransactionDoctypeConstants.SchemeIncentive
        //                   || p.DocumentTypeName == TransactionDoctypeConstants.WeavingDebitNote
        //                   || p.DocumentTypeName == TransactionDoctypeConstants.WeavingPayment
        //                   || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTDS
        //                   || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTimeIncentive
        //                   || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTimePenalty
        //                   || p.DocumentTypeName == TransactionDoctypeConstants.WeavingCreditNote
        //                   || p.DocumentTypeName == TransactionDoctypeConstants.SmallChunksBazarPenalty
        //                   || p.DocumentTypeName == TransactionDoctypeConstants.PurjaAmtTransfer
        //                   select p).ToList();


        //    DbContext.Dispose();

        //    var GroupedTemp = (from p in Temp
        //                       where p.CostCenterId != null
        //                       group p by p.CostCenterId into g
        //                       select g).ToList();

        //    var CostCenterREcords = (from p in db.CostCenterStatusExtended
        //                             where CostCenterIds.Contains(p.CostCenterId)
        //                             select p).ToList();


        //    if (DocType.Select(m => m.DocumentTypeId).ToArray().Contains(LedgerHeader.DocTypeId))
        //    {

        //        foreach (var item in GroupedTemp)
        //        {
        //            if (item.Max(m => m.CostCenterId).HasValue)
        //            {
        //                var CostCenterStatus = CostCenterREcords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

        //                var TempDocType = DocType.Where(m => m.DocumentTypeId == LedgerHeader.DocTypeId).FirstOrDefault();

        //                var ExistingLedgers = LedgersForCostCenters.Where(m => m.Key == item.Max(x => x.CostCenterId)).FirstOrDefault();

        //                switch (TempDocType.DocumentTypeName)
        //                {
        //                    case TransactionDoctypeConstants.PurjaAmtTransfer:
        //                        {
        //                            CostCenterStatus.TransferAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr) - ExistingLedgers.Sum(m => m.AmtCr)) + ((item.Sum(m => m.AmtDr) - item.Sum(m => m.AmtCr)));
        //                            break;
        //                        }
        //                    case TransactionDoctypeConstants.SchemeIncentive:
        //                        {
        //                            CostCenterStatus.SchemeIncentiveAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtCr)) + item.Sum(m => m.AmtCr);
        //                            break;
        //                        }
        //                    case TransactionDoctypeConstants.WeavingDebitNote:
        //                        {
        //                            CostCenterStatus.DebitAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr)) + item.Sum(m => m.AmtDr);
        //                            break;
        //                        }
        //                    case TransactionDoctypeConstants.WeavingPayment:
        //                        {
        //                            CostCenterStatus.PaymentAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr)) + item.Sum(m => m.AmtDr);
        //                            break;
        //                        }
        //                    case TransactionDoctypeConstants.WeavingTDS:
        //                        {
        //                            CostCenterStatus.TDSAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr)) + item.Sum(m => m.AmtDr);
        //                            break;
        //                        }
        //                    case TransactionDoctypeConstants.WeavingTimeIncentive:
        //                        {
        //                            CostCenterStatus.TimeIncentiveAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtCr)) + item.Sum(m => m.AmtCr);
        //                            break;
        //                        }
        //                    case TransactionDoctypeConstants.WeavingTimePenalty:
        //                        {
        //                            CostCenterStatus.TimePenaltyAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr)) + item.Sum(m => m.AmtDr);
        //                            break;
        //                        }
        //                    case TransactionDoctypeConstants.WeavingCreditNote:
        //                        {
        //                            CostCenterStatus.CreditAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtCr)) + item.Sum(m => m.AmtCr);
        //                            break;
        //                        }
        //                    case TransactionDoctypeConstants.SmallChunksBazarPenalty:
        //                        {
        //                            CostCenterStatus.FragmentationPenaltyAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr)) + item.Sum(m => m.AmtDr);
        //                            break;
        //                        }

        //                }

        //                CostCenterStatus.ObjectState = Model.ObjectState.Modified;
        //                db.CostCenterStatusExtended.Add(CostCenterStatus);

        //            }

        //        }

        //    }
        //} 
        #endregion

        void LedgerEvents__onHeaderDelete(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.Ledger.AsNoTracking().Where(m => m.LedgerHeaderId == EventArgs.DocId).ToList();

            var StockHeader = (from p in DbContext.LedgerHeader.AsNoTracking()
                               where p.LedgerHeaderId == EventArgs.DocId
                               select p
                            ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType.AsNoTracking()
                           where p.DocumentTypeName == TransactionDoctypeConstants.SchemeIncentive
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingDebitNote
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingPayment
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTDS
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTimeIncentive
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTimePenalty
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingCreditNote
                           || p.DocumentTypeName == TransactionDoctypeConstants.SmallChunksBazarPenalty
                           || p.DocumentTypeName == TransactionDoctypeConstants.PurjaAmtTransfer
                           || p.DocumentTypeName == TransactionDoctypeConstants.DebitNote
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingChequeCancel
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingReceipt
                           select p).ToList();

            DbContext.Dispose();



            var GroupedTemp = (from p in Temp
                               where p.CostCenterId != null
                               group p by p.CostCenterId
                                   into g
                                   select g).ToList();

            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();          

            if (DocType.Select(m => m.DocumentTypeId).ToArray().Contains(StockHeader.DocTypeId))
            {
                var CostCenterREcords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                foreach (var item in GroupedTemp)
                {
                    if (item.Max(m => m.CostCenterId).HasValue)
                    {
                        var CostCenterStatus = CostCenterREcords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

                        var TempDocType = DocType.Where(m => m.DocumentTypeId == StockHeader.DocTypeId).FirstOrDefault();


                        switch (TempDocType.DocumentTypeName)
                        {
                            case TransactionDoctypeConstants.PurjaAmtTransfer:
                                {
                                    CostCenterStatus.TransferAmount = (CostCenterStatus.TransferAmount ?? 0) - ((item.Sum(m => m.AmtDr) - item.Sum(m => m.AmtCr)));
                                    break;
                                }
                            case TransactionDoctypeConstants.SchemeIncentive:
                                {
                                    CostCenterStatus.SchemeIncentiveAmount = (CostCenterStatus.SchemeIncentiveAmount ?? 0) - item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingDebitNote:
                            case TransactionDoctypeConstants.DebitNote:
                                {
                                    CostCenterStatus.DebitAmount = (CostCenterStatus.DebitAmount ?? 0) - item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingPayment:
                                {
                                    CostCenterStatus.PaymentAmount = (CostCenterStatus.PaymentAmount ?? 0) - item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingTDS:
                                {
                                    CostCenterStatus.TDSAmount = (CostCenterStatus.TDSAmount ?? 0) - item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingTimeIncentive:
                                {
                                    CostCenterStatus.TimeIncentiveAmount = (CostCenterStatus.TimeIncentiveAmount ?? 0) - item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingTimePenalty:
                                {
                                    CostCenterStatus.TimePenaltyAmount = (CostCenterStatus.TimePenaltyAmount ?? 0) - item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingCreditNote:
                                {
                                    CostCenterStatus.CreditAmount = (CostCenterStatus.CreditAmount ?? 0) - item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.SmallChunksBazarPenalty:
                                {
                                    CostCenterStatus.FragmentationPenaltyAmount = (CostCenterStatus.FragmentationPenaltyAmount ?? 0) - item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingChequeCancel:
                                {
                                    CostCenterStatus.PaymentCancelAmount = (CostCenterStatus.PaymentCancelAmount ?? 0) - item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingReceipt:
                                {
                                    CostCenterStatus.WeavingReceipt = (CostCenterStatus.WeavingReceipt ?? 0) - item.Sum(m => m.AmtCr);
                                    break;
                                }

                        }




                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);

                    }

                }

            }
        }

        void LedgerEvents__onLineDelete(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        {

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.Ledger.AsNoTracking().Where(m => m.LedgerLineId == EventArgs.DocLineId).ToList();

            var StockHeader = (from p in DbContext.LedgerHeader.AsNoTracking()
                               where p.LedgerHeaderId == EventArgs.DocId
                               select p
                            ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType.AsNoTracking()
                           where p.DocumentTypeName == TransactionDoctypeConstants.SchemeIncentive
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingDebitNote
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingPayment
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTDS
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTimeIncentive
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTimePenalty
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingCreditNote
                           || p.DocumentTypeName == TransactionDoctypeConstants.SmallChunksBazarPenalty
                           || p.DocumentTypeName == TransactionDoctypeConstants.PurjaAmtTransfer
                            || p.DocumentTypeName == TransactionDoctypeConstants.DebitNote
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingChequeCancel
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingReceipt
                           select p).ToList();

            DbContext.Dispose();



            var GroupedTemp = (from p in Temp
                               where p.CostCenterId != null
                               group p by p.CostCenterId
                                   into g
                                   select g).ToList();

            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();            

            if (DocType.Select(m => m.DocumentTypeId).ToArray().Contains(StockHeader.DocTypeId))
            {
                var CostCenterREcords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                foreach (var item in GroupedTemp)
                {
                    if (item.Max(m => m.CostCenterId).HasValue)
                    {
                        var CostCenterStatus = CostCenterREcords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

                        var TempDocType = DocType.Where(m => m.DocumentTypeId == StockHeader.DocTypeId).FirstOrDefault();


                        switch (TempDocType.DocumentTypeName)
                        {
                            case TransactionDoctypeConstants.PurjaAmtTransfer:
                                {
                                    CostCenterStatus.TransferAmount = (CostCenterStatus.TransferAmount ?? 0) - ((item.Sum(m => m.AmtDr) - item.Sum(m => m.AmtCr)));
                                    break;
                                }
                            case TransactionDoctypeConstants.SchemeIncentive:
                                {
                                    CostCenterStatus.SchemeIncentiveAmount = (CostCenterStatus.SchemeIncentiveAmount ?? 0) - item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingDebitNote:
                            case TransactionDoctypeConstants.DebitNote:
                                {
                                    CostCenterStatus.DebitAmount = (CostCenterStatus.DebitAmount ?? 0) - item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingPayment:
                                {
                                    CostCenterStatus.PaymentAmount = (CostCenterStatus.PaymentAmount ?? 0) - item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingTDS:
                                {
                                    CostCenterStatus.TDSAmount = (CostCenterStatus.TDSAmount ?? 0) - item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingTimeIncentive:
                                {
                                    CostCenterStatus.TimeIncentiveAmount = (CostCenterStatus.TimeIncentiveAmount ?? 0) - item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingTimePenalty:
                                {
                                    CostCenterStatus.TimePenaltyAmount = (CostCenterStatus.TimePenaltyAmount ?? 0) - item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingCreditNote:
                                {
                                    CostCenterStatus.CreditAmount = (CostCenterStatus.CreditAmount ?? 0) - item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.SmallChunksBazarPenalty:
                                {
                                    CostCenterStatus.FragmentationPenaltyAmount = (CostCenterStatus.FragmentationPenaltyAmount ?? 0) - item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingChequeCancel:
                                {
                                    CostCenterStatus.PaymentCancelAmount = (CostCenterStatus.PaymentCancelAmount ?? 0) - item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingReceipt:
                                {
                                    CostCenterStatus.WeavingReceipt = (CostCenterStatus.WeavingReceipt ?? 0) - item.Sum(m => m.AmtCr);
                                    break;
                                }
                        }

                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);

                    }

                }

            }
        }

        void LedgerEvents__onLineSaveBulk(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void LedgerEvents__afterHeaderDelete(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool LedgerEvents__beforeLineDelete(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void LedgerEvents__onLineSave(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.Ledger.Local.Where(m => m.LedgerHeaderId == EventArgs.DocId).ToList();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var LedgerHeader = (from p in DbContext.LedgerHeader.AsNoTracking()
                                where p.LedgerHeaderId == EventArgs.DocId
                                select p
                            ).FirstOrDefault();

            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();

            var LedgersForCostCenters = (from p in DbContext.Ledger.AsNoTracking()
                                         join t in DbContext.LedgerHeader on p.LedgerHeaderId equals t.LedgerHeaderId
                                         where CostCenterIds.Contains(p.CostCenterId) && p.LedgerLineId != EventArgs.DocLineId
                                         && t.DocTypeId == LedgerHeader.DocTypeId && p.CostCenterId != null
                                         group p by p.CostCenterId into g
                                         select g).ToList();

            var DocType = (from p in DbContext.DocumentType.AsNoTracking()
                           where p.DocumentTypeName == TransactionDoctypeConstants.SchemeIncentive
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingDebitNote
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingPayment
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTDS
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTimeIncentive
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingTimePenalty
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingCreditNote
                           || p.DocumentTypeName == TransactionDoctypeConstants.SmallChunksBazarPenalty
                           || p.DocumentTypeName == TransactionDoctypeConstants.PurjaAmtTransfer
                           || p.DocumentTypeName == TransactionDoctypeConstants.DebitNote
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingChequeCancel
                           || p.DocumentTypeName == TransactionDoctypeConstants.WeavingReceipt
                           select p).ToList();


            DbContext.Dispose();

            var GroupedTemp = (from p in Temp
                               where p.CostCenterId != null
                               group p by p.CostCenterId into g
                               select g).ToList();           


            if (DocType.Select(m => m.DocumentTypeId).ToArray().Contains(LedgerHeader.DocTypeId))
            {
                var CostCenterREcords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                foreach (var item in GroupedTemp)
                {
                    if (item.Max(m => m.CostCenterId).HasValue)
                    {
                        var CostCenterStatus = CostCenterREcords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

                        var TempDocType = DocType.Where(m => m.DocumentTypeId == LedgerHeader.DocTypeId).FirstOrDefault();

                        var ExistingLedgers = LedgersForCostCenters.Where(m => m.Key == item.Max(x => x.CostCenterId)).FirstOrDefault();

                        switch (TempDocType.DocumentTypeName)
                        {
                            case TransactionDoctypeConstants.PurjaAmtTransfer:
                                {
                                    CostCenterStatus.TransferAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr) - ExistingLedgers.Sum(m => m.AmtCr)) + ((item.Sum(m => m.AmtDr) - item.Sum(m => m.AmtCr)));
                                    CostCenterStatus.AmountTransferDate = LedgerHeader.DocDate;
                                    break;
                                }
                            case TransactionDoctypeConstants.SchemeIncentive:
                                {
                                    CostCenterStatus.SchemeIncentiveAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtCr)) + item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingDebitNote:
                            case TransactionDoctypeConstants.DebitNote:
                                {
                                    CostCenterStatus.DebitAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr)) + item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingPayment:
                                {
                                    CostCenterStatus.PaymentAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr)) + item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingTDS:
                                {
                                    CostCenterStatus.TDSAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr)) + item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingTimeIncentive:
                                {
                                    CostCenterStatus.TimeIncentiveAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtCr)) + item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingTimePenalty:
                                {
                                    CostCenterStatus.TimePenaltyAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr)) + item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingCreditNote:
                                {
                                    CostCenterStatus.CreditAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtCr)) + item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.SmallChunksBazarPenalty:
                                {
                                    CostCenterStatus.FragmentationPenaltyAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtDr)) + item.Sum(m => m.AmtDr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingChequeCancel:
                                {
                                    CostCenterStatus.PaymentCancelAmount = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtCr)) + item.Sum(m => m.AmtCr);
                                    break;
                                }
                            case TransactionDoctypeConstants.WeavingReceipt:
                                {
                                    CostCenterStatus.WeavingReceipt = ((ExistingLedgers == null) ? 0 : ExistingLedgers.Sum(m => m.AmtCr)) + item.Sum(m => m.AmtCr);
                                    break;
                                }

                        }

                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);

                    }

                }

            }
        }

        bool LedgerEvents__beforeHeaderDelete(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void LedgerEvents__afterHeaderSave(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void LedgerEvents__onHeaderSave(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        {
            //CostCenterStatus Temp = new CostCenterStatus();
            //var Ledger = db.LedgerHeader.Local.Where(m => m.LedgerHeaderId == EventArgs.DocId).FirstOrDefault();
            //Temp.CostCenterId = db.CostCenter.Local.Where(m => m.CostCenterId == Ledger.CostCenterId).FirstOrDefault().CostCenterId;
            //Temp.ObjectState = Model.ObjectState.Added;
            //db.CostCenterStatus.Add(Temp);
        }

        bool LedgerEvents__beforeHeaderSave(object sender, LedgerEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
