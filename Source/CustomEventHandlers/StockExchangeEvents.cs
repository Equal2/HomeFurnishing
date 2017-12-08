using CustomEventArgs;
using Data.Models;
using System;
using System.Linq;
using StockExchangeDocumentEvents;
using Core.Common;

namespace Jobs.Controllers
{
    public class StockExchangeEvents : StockExchangeDocEvents
    {
        //For Subscribing Events
        public StockExchangeEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += StockExchangeEvents__beforeHeaderSave;
            //_onHeaderSave += StockExchangeEvents__onHeaderSave;
            //_afterHeaderSave += StockExchangeEvents__afterHeaderSave;
            //_beforeHeaderDelete += StockExchangeEvents__beforeHeaderDelete;
            _onLineSave += StockExchangeEvents__onLineSave;
            //_beforeLineDelete += StockExchangeEvents__beforeLineDelete;
            //_afterHeaderDelete += StockExchangeEvents__afterHeaderDelete;
            _onLineSaveBulk += StockExchangeEvents__onLineSaveBulk;
            _onHeaderDelete += StockExchangeEvents__onHeaderDelete;
        }

        void StockExchangeEvents__onHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.StockLine.Where(m => m.StockHeaderId == EventArgs.DocId).ToList();
            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == EventArgs.DocId
                               select p
                               ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingExchange
                           select p).FirstOrDefault();

            if (DocType != null)
            { 
                var IssueLineCostCenterRecords = (from p in DbContext.StockLine
                                                  join t in DbContext.StockHeader on p.StockHeaderId equals t.StockHeaderId
                                                  where CostCenterIds.Contains(p.CostCenterId) && t.DocTypeId == DocType.DocumentTypeId
                                                  && p.StockHeaderId != EventArgs.DocId
                                                  select p).ToList();


                var IssueProductCount = (from p in IssueLineCostCenterRecords
                                         group p by p.CostCenterId into g
                                         select g).ToList();

                DbContext.Dispose();

                if (StockHeader.DocTypeId == DocType.DocumentTypeId)
                {

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

                                if (CostCenterStatus != null)
                                {
                                    CostCenterStatus.MaterialIssueQty = (CostCenterStatus.MaterialIssueQty ?? 0) - item.Where(m => m.DocNature == StockNatureConstants.Issue).Sum(m => m.Qty);
                                    CostCenterStatus.MaterialReturnQty = (CostCenterStatus.MaterialReturnQty ?? 0) - item.Where(m => m.DocNature == StockNatureConstants.Receive).Sum(m => m.Qty);
                                    CostCenterStatus.MaterialIssueDate = StockHeader.DocDate;
                                    CostCenterStatus.MaterialIssueProductCount = (IssueProductCount.Count == 0) ? 0 : IssueProductCount.Where(m => m.Key == CostCenterStatus.CostCenterId).FirstOrDefault().Select(m => m.RequisitionLineId).Distinct().Count();
                                    CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                    db.CostCenterStatusExtended.Add(CostCenterStatus);
                                }
                            }
                        }
                    }
                }
            }
        }

        void StockExchangeEvents__onLineSaveBulk(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.StockLine.Local.Where(m => m.StockHeaderId == EventArgs.DocId).ToList();
            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == EventArgs.DocId
                               select p
                               ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingExchange
                           select p).FirstOrDefault();


            if (DocType != null)
            {
                var IssueLineCostCenterRecords = (from p in DbContext.StockLine
                                                  join t in DbContext.StockHeader on p.StockHeaderId equals t.StockHeaderId
                                                  where CostCenterIds.Contains(p.CostCenterId) && t.DocTypeId == DocType.DocumentTypeId
                                                  select p).ToList();

                IssueLineCostCenterRecords.AddRange(Temp);


                var IssueProductCount = (from p in IssueLineCostCenterRecords
                                         group p by p.CostCenterId into g
                                         select g).ToList();
                DbContext.Dispose();

                if (StockHeader.DocTypeId == DocType.DocumentTypeId)
                {

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

                                if (CostCenterStatus != null)
                                {
                                    CostCenterStatus.MaterialIssueQty = (CostCenterStatus.MaterialIssueQty ?? 0) + item.Where(m => m.DocNature == StockNatureConstants.Issue).Sum(m => m.Qty);
                                    CostCenterStatus.MaterialReturnQty = (CostCenterStatus.MaterialReturnQty ?? 0) + item.Where(m => m.DocNature == StockNatureConstants.Receive).Sum(m => m.Qty);
                                    CostCenterStatus.MaterialIssueDate = StockHeader.DocDate;
                                    CostCenterStatus.MaterialReturnDate = StockHeader.DocDate;
                                    CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                    db.CostCenterStatusExtended.Add(CostCenterStatus);
                                }
                            }
                        }
                    }
                }
            }
        }

        void StockExchangeEvents__afterHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool StockExchangeEvents__beforeLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void StockExchangeEvents__onLineSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.StockLine.Local.Where(m => m.StockLineId == EventArgs.DocLineId).FirstOrDefault();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == Temp.StockHeaderId
                               select p
                            ).FirstOrDefault();



            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingExchange
                           select p).FirstOrDefault();


            if (DocType != null)
            {
                var IssueLineCostCenterRecords = (from p in DbContext.StockLine
                                                  join t in DbContext.StockHeader on p.StockHeaderId equals t.StockHeaderId
                                                  where p.CostCenterId == Temp.CostCenterId && t.DocTypeId == DocType.DocumentTypeId
                                                  && p.StockLineId != EventArgs.DocLineId && p.DocNature == Temp.DocNature
                                                  select p).ToList();

                IssueLineCostCenterRecords.Add(Temp);


                var IssueProductCount = (from p in IssueLineCostCenterRecords
                                         select p.RequisitionLineId).Distinct().Count();

                DbContext.Dispose();

                if (StockHeader.DocTypeId == DocType.DocumentTypeId && Temp.Qty != 0)
                {
                    if (Temp.CostCenterId.HasValue)
                    {
                        var CostCenterStatus = db.CostCenterStatusExtended.Find(Temp.CostCenterId);

                        if (CostCenterStatus != null && EventArgs.DocLineId <= 0)
                        {
                            if (Temp.DocNature == StockNatureConstants.Issue)
                            {
                                CostCenterStatus.MaterialIssueQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum();
                                CostCenterStatus.MaterialIssueDate = StockHeader.DocDate;
                            }
                            else if (Temp.DocNature == StockNatureConstants.Receive)
                            {
                                CostCenterStatus.MaterialReturnQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum();
                                CostCenterStatus.MaterialReturnDate = StockHeader.DocDate;
                            }
                            CostCenterStatus.MaterialIssueProductCount = IssueProductCount;
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatusExtended.Add(CostCenterStatus);
                        }
                        else if (CostCenterStatus != null && EventArgs.DocLineId > 0)
                        {
                            if (Temp.DocNature == StockNatureConstants.Issue)
                            {
                                CostCenterStatus.MaterialIssueQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum();
                                CostCenterStatus.MaterialIssueDate = StockHeader.DocDate;
                            }
                            else if (Temp.DocNature == StockNatureConstants.Receive)
                            {
                                CostCenterStatus.MaterialReturnQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum();
                                CostCenterStatus.MaterialReturnDate = StockHeader.DocDate;
                            }
                            CostCenterStatus.MaterialIssueProductCount = IssueProductCount;
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatusExtended.Add(CostCenterStatus);
                        }
                    }
                }
            }
        }

        bool StockExchangeEvents__beforeHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void StockExchangeEvents__afterHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void StockExchangeEvents__onHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            //CostCenterStatus Temp = new CostCenterStatus();
            //var StockExchange = db.StockExchangeHeader.Local.Where(m => m.StockExchangeHeaderId == EventArgs.DocId).FirstOrDefault();
            //Temp.CostCenterId = db.CostCenter.Local.Where(m => m.CostCenterId == StockExchange.CostCenterId).FirstOrDefault().CostCenterId;
            //Temp.ObjectState = Model.ObjectState.Added;
            //db.CostCenterStatus.Add(Temp);
        }

        bool StockExchangeEvents__beforeHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
