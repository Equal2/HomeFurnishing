using Core.Common;
using CustomEventArgs;
using Data.Models;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentEvents;
using JobReturnDocumentEvents;

namespace Jobs.Controllers
{
    public class JobReturnEvents : JobReturnDocEvents
    {
        //For Subscribing Events
        public JobReturnEvents()
        {
            Initialized = true;
            _onLineSave += JobReturnEvents__onLineSave;
            _onLineSaveBulk += JobReturnEvents__onLineSaveBulk;
            _onLineDelete += JobReturnEvents__onLineDelete;
            _onHeaderDelete += JobReturnEvents__onHeaderDelete;
        }

        void JobReturnEvents__onHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = (from p in DbContext.JobReturnLine
                        join rl in DbContext.JobReceiveLine on p.JobReceiveLineId equals rl.JobReceiveLineId
                        join t in DbContext.JobOrderLine on rl.JobOrderLineId equals t.JobOrderLineId
                        join t2 in DbContext.JobOrderHeader on t.JobOrderHeaderId equals t2.JobOrderHeaderId
                        where p.JobReturnHeaderId == EventArgs.DocId && t2.CostCenterId != null
                        select new
                        {
                            LineId = t.JobOrderLineId,
                            Qty = p.Qty,
                            DealQty = p.DealQty,
                            UnitConvMul = t.UnitConversionMultiplier,
                            HeaderId = t.JobOrderHeaderId,
                            CostCenterId = t2.CostCenterId,
                            DocTypeId = t2.DocTypeId,
                        }).ToList();

            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            DbContext.Dispose();

            if (Temp.Select(m => m.DocTypeId).Intersect(DocType).Any())
            {

                var CostCenterRecords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                foreach (var item in Temp.GroupBy(m => m.CostCenterId))
                {
                    var TempCostCenterREcord = CostCenterRecords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

                    if (DocType.Contains(item.Max(x => x.DocTypeId)))
                    {
                        if (TempCostCenterREcord != null)
                        {
                            TempCostCenterREcord.ReturnQty = (TempCostCenterREcord.ReturnQty ?? 0) - item.Select(m => m.Qty).Sum();
                            TempCostCenterREcord.ReturnDealQty = (TempCostCenterREcord.ReturnDealQty ?? 0) - item.Select(m => m.DealQty).Sum();

                            TempCostCenterREcord.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatusExtended.Add(TempCostCenterREcord);
                        }
                    }
                }
            }
        }

        void JobReturnEvents__onLineDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = (DbContext.JobReturnLine.AsNoTracking().Where(m => m.JobReturnLineId == EventArgs.DocLineId).FirstOrDefault());

            var DocType = (from p in DbContext.DocumentType.AsNoTracking()
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            var JOH = (from p in DbContext.JobReceiveLine.AsNoTracking()
                       join t in DbContext.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                       into table
                       from tab in table.DefaultIfEmpty()
                       join t2 in DbContext.JobOrderHeader on tab.JobOrderHeaderId equals t2.JobOrderHeaderId
                       where p.JobReceiveLineId == Temp.JobReceiveLineId
                       select t2).FirstOrDefault();

            DbContext.Dispose();



            if (DocType.Contains(JOH.DocTypeId) && Temp.Qty != 0)
            {
                if (JOH.CostCenterId.HasValue)
                {
                    var CostCenterStatus = db.CostCenterStatusExtended.Find(JOH.CostCenterId);

                    if (CostCenterStatus != null)
                    {
                        CostCenterStatus.ReturnQty = (CostCenterStatus.ReturnQty ?? 0) - Temp.Qty;
                        CostCenterStatus.ReturnDealQty = (CostCenterStatus.ReturnDealQty ?? 0) - Temp.DealQty;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }

        void JobReturnEvents__onLineSaveBulk(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.JobReturnLine.Local.Where(m => m.JobReturnHeaderId == EventArgs.DocId).ToList();

            var JobRecLineIds = Temp.Select(m => m.JobReceiveLineId).ToArray();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var JobHeader = (from rl in DbContext.JobReceiveLine
                             join t in DbContext.JobOrderLine on rl.JobOrderLineId equals t.JobOrderLineId
                             join t2 in DbContext.JobOrderHeader on t.JobOrderHeaderId equals t2.JobOrderHeaderId
                             where JobRecLineIds.Contains(rl.JobReceiveLineId) && t2.CostCenterId != null
                             select new
                             {
                                 JRLineId = rl.JobReceiveLineId,
                                 UniConvMul = t.UnitConversionMultiplier,
                                 JHeaderId = t2.JobOrderHeaderId,
                                 CostCenterId = t2.CostCenterId,
                                 DocTypeId = t2.DocTypeId,
                             }
                               ).ToList();

            var Join = (from p in Temp
                        join t in JobHeader on p.JobReceiveLineId equals t.JRLineId
                        select new
                        {
                            Qty = p.Qty,
                            DealQty = p.DealQty,
                            UnitConvMul = t.UniConvMul,
                            LineId = t.JRLineId,
                            JHeaderid = t.JHeaderId,
                            CostCenterId = t.CostCenterId,
                        }).ToList();

            var CostCenterIds = JobHeader.Select(m => m.CostCenterId).ToArray();

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            DbContext.Dispose();

            if (JobHeader.Select(m => m.DocTypeId).Intersect(DocType).Any())
            {
                var CostCenterRecords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                foreach (var item in JobHeader.GroupBy(m => m.CostCenterId))
                {
                    var TempCostCenterRecord = CostCenterRecords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

                    if (DocType.Contains(item.Max(m => m.DocTypeId)) && TempCostCenterRecord != null)
                    {
                        if (item.Max(m => m.CostCenterId).HasValue)
                        {

                            TempCostCenterRecord.ReturnQty = (TempCostCenterRecord.ReturnQty ?? 0) + Join.Where(m => m.CostCenterId == item.Key).Select(m => m.Qty).Sum();
                            TempCostCenterRecord.ReturnDealQty = (TempCostCenterRecord.ReturnDealQty ?? 0) + Join.Where(m => m.CostCenterId == item.Key).Select(m => m.DealQty).Sum();
                        }
                        TempCostCenterRecord.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(TempCostCenterRecord);
                    }

                }
            }
        }

        void JobReturnEvents__onLineSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.JobReturnLine.Local.Where(m => m.JobReturnLineId == EventArgs.DocLineId).FirstOrDefault();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var JOH = (from p in db.JobReceiveLine
                       join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                       into table
                       from tab in table.DefaultIfEmpty()
                       join t in db.JobOrderHeader on tab.JobOrderHeaderId equals t.JobOrderHeaderId
                       where p.JobReceiveLineId == Temp.JobReceiveLineId
                       select t).FirstOrDefault();

            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            DbContext.Dispose();



            if (DocType.Contains(JOH.DocTypeId) && Temp.Qty != 0)
            {
                if (JOH.CostCenterId.HasValue)
                {

                    var CostCenterStatus = (from p in db.CostCenterStatusExtended
                                            where p.CostCenterId == JOH.CostCenterId
                                            select p).FirstOrDefault();

                    if (CostCenterStatus != null && EventArgs.DocLineId <= 0)
                    {
                        CostCenterStatus.ReturnQty = (CostCenterStatus.ReturnQty ?? 0) + Temp.Qty;
                        CostCenterStatus.ReturnDealQty = (CostCenterStatus.ReturnDealQty ?? 0) + Temp.DealQty;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                    else if (CostCenterStatus != null && EventArgs.DocLineId > 0)
                    {

                        var IssueLineCostCenterRecords = (from p in db.JobReturnLine
                                                          join rl in db.JobReceiveLine on p.JobReceiveLineId equals rl.JobReceiveLineId
                                                          join t in db.JobOrderLine on rl.JobOrderLineId equals t.JobOrderLineId
                                                          join t2 in db.JobOrderHeader on t.JobOrderHeaderId equals t2.JobOrderHeaderId
                                                          where t2.CostCenterId == JOH.CostCenterId
                                                          && p.JobReturnLineId != EventArgs.DocLineId
                                                          select new
                                                          {
                                                              Qty = p.Qty,
                                                              DealQty = p.DealQty,
                                                          }).ToList();

                        CostCenterStatus.ReturnQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum() + Temp.Qty;
                        CostCenterStatus.ReturnDealQty = (IssueLineCostCenterRecords.Select(m => m.DealQty).Sum()) + (Temp.DealQty);
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }
    }
}
