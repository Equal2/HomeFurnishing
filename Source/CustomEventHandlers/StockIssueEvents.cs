using CustomEventArgs;
using Data.Models;
using System;
using StockIssueDocumentEvents;
using System.Linq;
using Core.Common;
using Model.Models;

namespace Jobs.Controllers
{


    public class StockIssueEvents : StockIssueDocEvents
    {

        //For Subscribing Events
        public StockIssueEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += StockIssueEvents__beforeHeaderSave;
            //_onHeaderSave += StockIssueEvents__onHeaderSave;
            //_afterHeaderSave += StockIssueEvents__afterHeaderSave;
            //_beforeHeaderDelete += StockIssueEvents__beforeHeaderDelete;
            _onLineSave += StockIssueEvents__onLineSave;
            _onLineDelete += StockIssueEvents__onLineDelete;
            //_beforeLineDelete += StockIssueEvents__beforeLineDelete;
            //_afterHeaderDelete += StockIssueEvents__afterHeaderDelete;
            _onHeaderDelete += StockIssueEvents__onHeaderDelete;
            _onLineSaveBulk += StockIssueEvents__onLineSaveBulk;
        }

        void StockIssueEvents__onHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.StockLine.Where(m => m.StockHeaderId == EventArgs.DocId).ToList();
            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == EventArgs.DocId
                               select p
                               ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.MaterialIssueForWeaving
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
                                    CostCenterStatus.MaterialIssueQty = (CostCenterStatus.MaterialIssueQty ?? 0) - item.Sum(m => m.Qty);
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

        void StockIssueEvents__onLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.StockLine.Where(m => m.StockLineId == EventArgs.DocLineId).FirstOrDefault();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == Temp.StockHeaderId
                               select p
                           ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.MaterialIssueForWeaving
                           select p).FirstOrDefault();

            if (DocType != null)
            {
                var IssueLineCostCenterRecords = (from p in DbContext.StockLine
                                                  join t in DbContext.StockHeader on p.StockHeaderId equals t.StockHeaderId
                                                  where p.CostCenterId == Temp.CostCenterId && t.DocTypeId == DocType.DocumentTypeId
                                                  && p.StockLineId != EventArgs.DocLineId
                                                  select p).ToList();


                var IssueProductCount = (from p in IssueLineCostCenterRecords
                                         select p.RequisitionLineId).Distinct().Count();

                DbContext.Dispose();



                if (StockHeader.DocTypeId == DocType.DocumentTypeId && Temp.Qty != 0)
                {
                    if (Temp.CostCenterId.HasValue)
                    {
                        var CostCenterStatus = (from p in db.CostCenterStatusExtended
                                                where p.CostCenterId == Temp.CostCenterId
                                                select p).FirstOrDefault();

                        if (CostCenterStatus != null)
                        {
                            CostCenterStatus.MaterialIssueQty = (CostCenterStatus.MaterialIssueQty ?? 0) - Temp.Qty;
                            CostCenterStatus.MaterialIssueDate = StockHeader.DocDate;
                            CostCenterStatus.MaterialIssueProductCount = IssueProductCount;
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatusExtended.Add(CostCenterStatus);
                        }
                    }
                }
            }
        }

        void StockIssueEvents__onLineSaveBulk(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.StockLine.Local.Where(m => m.StockHeaderId == EventArgs.DocId).ToList();
            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == EventArgs.DocId
                               select p
                               ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.MaterialIssueForWeaving
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
                                    CostCenterStatus.MaterialIssueQty = (CostCenterStatus.MaterialIssueQty ?? 0) + item.Sum(m => m.Qty);
                                    CostCenterStatus.MaterialIssueDate = StockHeader.DocDate;
                                    CostCenterStatus.MaterialIssueProductCount = IssueProductCount.Where(m => m.Key == CostCenterStatus.CostCenterId).FirstOrDefault().Select(m => m.RequisitionLineId).Distinct().Count();
                                    CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                    db.CostCenterStatusExtended.Add(CostCenterStatus);
                                }
                            }
                        }
                    }
                }
            }
        }

        void StockIssueEvents__afterHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool StockIssueEvents__beforeLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void StockIssueEvents__onLineSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {

            var Temp = db.StockLine.Local.Where(m => m.StockLineId == EventArgs.DocLineId).FirstOrDefault();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == Temp.StockHeaderId
                               select p
                            ).FirstOrDefault();


            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.MaterialIssueForWeaving
                           select p).FirstOrDefault();

            if (DocType != null)
            {
                if (StockHeader.DocTypeId == DocType.DocumentTypeId && Temp.Qty != 0)
                {

                    var IssueLineCostCenterRecords = (from p in DbContext.StockLine
                                                      join t in DbContext.StockHeader on p.StockHeaderId equals t.StockHeaderId
                                                      where p.CostCenterId == Temp.CostCenterId && t.DocTypeId == DocType.DocumentTypeId
                                                      && p.StockLineId != EventArgs.DocLineId
                                                      select p).ToList();

                    var StockProcCostCenterRecords = (from p in DbContext.StockProcess
                                                      join sl in DbContext.StockLine on p.StockProcessId equals sl.StockProcessId
                                                      join t in DbContext.StockHeader on p.StockHeaderId equals t.StockHeaderId
                                                      where p.CostCenterId == Temp.CostCenterId && t.DocTypeId == DocType.DocumentTypeId
                                                      && sl.StockLineId != EventArgs.DocLineId
                                                      select p).ToList();

                    IssueLineCostCenterRecords.Add(Temp);




                    var IssueProductCount = (from p in IssueLineCostCenterRecords
                                             select p.RequisitionLineId).Distinct().Count();

                    if (Temp.CostCenterId.HasValue)
                    {
                        var CostCenterStatus = db.CostCenterStatusExtended.Find(Temp.CostCenterId);

                        if (CostCenterStatus != null && EventArgs.DocLineId <= 0)
                        {
                            CostCenterStatus.MaterialIssueQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum();
                            CostCenterStatus.MaterialIssueDate = StockHeader.DocDate;
                            CostCenterStatus.MaterialIssueProductCount = IssueProductCount;
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatusExtended.Add(CostCenterStatus);
                        }
                        else if (CostCenterStatus != null && EventArgs.DocLineId > 0)
                        {

                            CostCenterStatus.MaterialIssueQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum();
                            CostCenterStatus.MaterialIssueDate = StockHeader.DocDate;
                            //CostCenterStatus.MaterialIssueProductCount = (CostCenterStatus.MaterialIssueProductCount ?? 0) + 1;
                            CostCenterStatus.MaterialIssueProductCount = IssueProductCount;
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatusExtended.Add(CostCenterStatus);
                        }
                    }
                }


                DbContext.Dispose();
            }
        }

        bool StockIssueEvents__beforeHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void StockIssueEvents__afterHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void StockIssueEvents__onHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            //CostCenterStatus Temp = new CostCenterStatus();
            //var StockIssue = db.StockIssueHeader.Local.Where(m => m.StockIssueHeaderId == EventArgs.DocId).FirstOrDefault();
            //Temp.CostCenterId = db.CostCenter.Local.Where(m => m.CostCenterId == StockIssue.CostCenterId).FirstOrDefault().CostCenterId;
            //Temp.ObjectState = Model.ObjectState.Added;
            //db.CostCenterStatus.Add(Temp);
        }

        bool StockIssueEvents__beforeHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
