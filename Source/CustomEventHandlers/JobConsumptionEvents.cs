using CustomEventArgs;
using Data.Models;
using System;
using JobConsumptionDocumentEvents;
using System.Linq;
using Core.Common;

namespace Jobs.Controllers
{
    public class JobConsumptionEvents : JobConsumptionDocEvents
    {
        //For Subscribing Events
        public JobConsumptionEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += JobConsumptionEvents__beforeHeaderSave;
            //_onHeaderSave += JobConsumptionEvents__onHeaderSave;
            //_afterHeaderSave += JobConsumptionEvents__afterHeaderSave;
            //_beforeHeaderDelete += JobConsumptionEvents__beforeHeaderDelete;
            _onLineSave += JobConsumptionEvents__onLineSave;
            //_beforeLineDelete += JobConsumptionEvents__beforeLineDelete;
            //_afterHeaderDelete += JobConsumptionEvents__afterHeaderDelete;
            _onLineSaveBulk += JobConsumptionEvents__onLineSaveBulk;
            _onLineDelete += JobConsumptionEvents__onLineDelete;
            _onHeaderDelete += JobConsumptionEvents__onHeaderDelete;
        }

        void JobConsumptionEvents__onHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.StockLine.Where(m => m.StockHeaderId == EventArgs.DocId).ToList();

            var StockHeader = DbContext.StockHeader.Find(EventArgs.DocId);

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingConsumptionAdjustment
                           select p).FirstOrDefault();

            DbContext.Dispose();

            
            if (DocType != null)
            {
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

                                if (CostCenterStatus != null)
                                {
                                    CostCenterStatus.ConsumptionAdjustmentQty = (CostCenterStatus.ConsumptionAdjustmentQty ?? 0) - item.Sum(m => m.Qty);
                                    CostCenterStatus.ConsumptionAdjustmentDate = StockHeader.DocDate;
                                    CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                    db.CostCenterStatusExtended.Add(CostCenterStatus);
                                }
                            }
                        }
                    }
                }
            }
        }

        void JobConsumptionEvents__onLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.StockLine.Where(m => m.StockLineId == EventArgs.DocLineId).FirstOrDefault();

            var StockHeader = DbContext.StockHeader.Find(Temp.StockHeaderId);
            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingConsumptionAdjustment
                           select p).FirstOrDefault();

            DbContext.Dispose();

            if (DocType != null)
            {
                if (StockHeader.DocTypeId == DocType.DocumentTypeId && Temp.Qty != 0)
                {
                    if (Temp.CostCenterId.HasValue)
                    {
                        var CostCenterStatus = db.CostCenterStatusExtended.Find(Temp.CostCenterId);

                        if (CostCenterStatus != null)
                        {
                            CostCenterStatus.ConsumptionAdjustmentQty = (CostCenterStatus.ConsumptionAdjustmentQty ?? 0) - Temp.Qty;
                            CostCenterStatus.ConsumptionAdjustmentDate = StockHeader.DocDate;
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatusExtended.Add(CostCenterStatus);
                        }
                    }
                }
            }
        }

        void JobConsumptionEvents__onLineSaveBulk(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.StockLine.Local.Where(m => m.StockHeaderId == EventArgs.DocId).ToList();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == EventArgs.DocId
                               select p
                               ).FirstOrDefault();

            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingConsumptionAdjustment
                           select p).FirstOrDefault();

            DbContext.Dispose();

            if (DocType != null)
            {
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
                                    CostCenterStatus.ConsumptionAdjustmentQty = (CostCenterStatus.ConsumptionAdjustmentQty ?? 0) + item.Sum(m => m.Qty);
                                    CostCenterStatus.ConsumptionAdjustmentDate = StockHeader.DocDate;
                                    CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                    db.CostCenterStatusExtended.Add(CostCenterStatus);
                                }
                            }
                        }
                    }
                }
            }
        }

        void JobConsumptionEvents__afterHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool JobConsumptionEvents__beforeLineDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobConsumptionEvents__onLineSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.StockLine.Local.Where(m => m.StockLineId == EventArgs.DocLineId).FirstOrDefault();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var StockHeader = (from p in DbContext.StockHeader
                               where p.StockHeaderId == Temp.StockHeaderId
                               select p
                            ).FirstOrDefault();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingConsumptionAdjustment
                           select p).FirstOrDefault();

            DbContext.Dispose();

            if (DocType != null)
            {
                if (StockHeader.DocTypeId == DocType.DocumentTypeId && Temp.Qty != 0)
                {
                    if (Temp.CostCenterId.HasValue)
                    {
                        var CostCenterStatus = db.CostCenterStatusExtended.Find(Temp.CostCenterId);

                        if (CostCenterStatus != null && EventArgs.DocLineId <= 0)
                        {
                            CostCenterStatus.ConsumptionAdjustmentQty = (CostCenterStatus.ConsumptionAdjustmentQty ?? 0) + Temp.Qty;
                            CostCenterStatus.ConsumptionAdjustmentDate = StockHeader.DocDate;
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

                            CostCenterStatus.ConsumptionAdjustmentQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum() + Temp.Qty;
                            CostCenterStatus.ConsumptionAdjustmentDate = StockHeader.DocDate;
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatusExtended.Add(CostCenterStatus);
                        }
                    }
                }
            }
        }

        bool JobConsumptionEvents__beforeHeaderDelete(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobConsumptionEvents__afterHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobConsumptionEvents__onHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            //CostCenterStatus Temp = new CostCenterStatus();
            //var JobConsumption = db.JobConsumptionHeader.Local.Where(m => m.JobConsumptionHeaderId == EventArgs.DocId).FirstOrDefault();
            //Temp.CostCenterId = db.CostCenter.Local.Where(m => m.CostCenterId == JobConsumption.CostCenterId).FirstOrDefault().CostCenterId;
            //Temp.ObjectState = Model.ObjectState.Added;
            //db.CostCenterStatus.Add(Temp);
        }

        bool JobConsumptionEvents__beforeHeaderSave(object sender, StockEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
