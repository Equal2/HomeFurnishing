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
using JobOrderCancelDocumentEvents;

namespace Jobs.Controllers
{
    public class JobOrderCancelEvents : JobOrderCancelDocEvents
    {
        //For Subscribing Events
        public JobOrderCancelEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += JobOrderEvents__beforeHeaderSave;
            //_onHeaderSave += JobOrderEvents__onHeaderSave;
            //_afterHeaderSave += JobOrderEvents__afterHeaderSave;
            //_beforeHeaderDelete += JobOrderEvents__beforeHeaderDelete;
            _onLineSave += JobOrderCancelEvents__onLineSave;
            _onLineSaveBulk += JobOrderCancelEvents__onLineSaveBulk;
            _onLineDelete += JobOrderCancelEvents__onLineDelete;
            _onHeaderDelete += JobOrderCancelEvents__onHeaderDelete;
            //_beforeLineDelete += JobOrderEvents__beforeLineDelete;
            //_afterHeaderDelete += JobOrderEvents__afterHeaderDelete;
            //_beforeHeaderApprove += JobOrderCancelEvents__beforeHeaderApprove;
            //_onHeaderApprove += JobOrderCancelEvents__onHeaderApprove;
        }

        void JobOrderCancelEvents__onLineSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.JobOrderCancelLine.Local.Where(m => m.JobOrderCancelLineId == EventArgs.DocLineId).FirstOrDefault();

            var Boms = db.JobOrderCancelBom.Local.Where(m => m.JobOrderCancelLineId == EventArgs.DocLineId).ToList();

            Decimal BomQty = 0;

            if (Boms != null && Boms.Count > 0)
                BomQty = Boms.Select(m => m.Qty).Sum();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var JobOrderLien = DbContext.JobOrderLine.Find(Temp.JobOrderLineId);

            var JobHeader = (from p in DbContext.JobOrderHeader
                             where p.JobOrderHeaderId == JobOrderLien.JobOrderHeaderId
                             select p
                            ).FirstOrDefault();

            DbContext.Dispose();

            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            if (DocType.Contains(JobHeader.DocTypeId) && Temp.Qty != 0)
            {
                if (JobHeader.CostCenterId.HasValue)
                {

                    var CostCenterStatus = (from p in db.CostCenterStatusExtended
                                            where p.CostCenterId == JobHeader.CostCenterId
                                            select p).FirstOrDefault();

                    if (CostCenterStatus != null && EventArgs.DocLineId <= 0)
                    {
                        CostCenterStatus.OrderCancelQty = (CostCenterStatus.OrderCancelQty ?? 0) + Temp.Qty;
                        CostCenterStatus.OrderCancelDealQty = (CostCenterStatus.OrderCancelDealQty ?? 0) + (Temp.Qty * JobOrderLien.UnitConversionMultiplier);
                        CostCenterStatus.BOMCancelQty = (CostCenterStatus.BOMCancelQty ?? 0) + BomQty;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                    else if (CostCenterStatus != null && EventArgs.DocLineId > 0)
                    {

                        var IssueLineCostCenterRecords = (from p in db.JobOrderCancelLine
                                                          join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                                          join t2 in db.JobOrderHeader on t.JobOrderHeaderId equals t2.JobOrderHeaderId
                                                          where t2.CostCenterId == JobHeader.CostCenterId
                                                          && p.JobOrderCancelLineId != EventArgs.DocLineId
                                                          select new
                                                          {
                                                              Qty = p.Qty,
                                                              UnitConversionMultiplier = t.UnitConversionMultiplier,
                                                          }).ToList();

                        var BomRecords = (from p in db.JobOrderCancelBom
                                          join jl in db.JobOrderCancelLine on p.JobOrderCancelLineId equals jl.JobOrderCancelLineId
                                          join t in db.JobOrderLine on jl.JobOrderLineId equals t.JobOrderLineId
                                          join t2 in db.JobOrderHeader on t.JobOrderHeaderId equals t2.JobOrderHeaderId
                                          where t2.CostCenterId == JobHeader.CostCenterId
                                          && p.JobOrderCancelLineId != EventArgs.DocLineId
                                          select new
                                          {
                                              Qty = p.Qty,
                                          }).ToList();

                        CostCenterStatus.OrderCancelQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum() + Temp.Qty;
                        CostCenterStatus.OrderCancelDealQty = (IssueLineCostCenterRecords.Select(m => m.Qty * m.UnitConversionMultiplier).Sum()) + (Temp.Qty * JobOrderLien.UnitConversionMultiplier);
                        CostCenterStatus.BOMCancelQty = BomRecords.Sum(m => m.Qty) + BomQty;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }

        void JobOrderCancelEvents__onHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = (from p in DbContext.JobOrderCancelLine
                        join t in DbContext.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                        join t2 in DbContext.JobOrderHeader on t.JobOrderHeaderId equals t2.JobOrderHeaderId
                        where p.JobOrderCancelHeaderId == EventArgs.DocId && t2.CostCenterId != null
                        select new
                        {
                            LineId = t.JobOrderLineId,
                            Qty = p.Qty,
                            UnitConvMul = t.UnitConversionMultiplier,
                            HeaderId = t.JobOrderHeaderId,
                            CostCenterId = t2.CostCenterId,
                            DocTypeId = t2.DocTypeId,
                        }).ToList();

            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();


            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            if (Temp.Select(m => m.DocTypeId).Intersect(DocType).Any())
            {

                var Boms = (from p in DbContext.JobOrderCancelBom
                            join t in DbContext.JobOrderCancelLine on p.JobOrderCancelLineId equals t.JobOrderCancelLineId
                            join t2 in DbContext.JobOrderLine on t.JobOrderLineId equals t2.JobOrderLineId
                            join t3 in DbContext.JobOrderHeader on t2.JobOrderHeaderId equals t3.JobOrderHeaderId
                            where p.JobOrderCancelHeaderId == EventArgs.DocId && t3.CostCenterId != null
                            group p by t3.CostCenterId into g
                            select new
                            {
                                BomQty = g.Sum(m => m.Qty),
                                CostCenterId = g.Key,
                            }).ToList();

                var CostCenterRecords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                foreach (var item in Temp.GroupBy(m => m.CostCenterId))
                {

                    var TempCostCenterREcord = CostCenterRecords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

                    var TempBoms = Boms.Where(m => m.CostCenterId == item.Key).ToList();

                    decimal BomQty = 0;

                    if (TempBoms != null && TempBoms.Count > 0)
                        BomQty = TempBoms.Sum(m => m.BomQty);


                    if (DocType.Contains(item.Max(x => x.DocTypeId)))
                    {

                        if (TempCostCenterREcord != null)
                        {
                            TempCostCenterREcord.OrderCancelQty = (TempCostCenterREcord.OrderCancelQty ?? 0) - item.Select(m => m.Qty).Sum();
                            TempCostCenterREcord.OrderCancelDealQty = (TempCostCenterREcord.OrderCancelDealQty ?? 0) - item.Select(m => m.Qty * m.UnitConvMul).Sum();
                            TempCostCenterREcord.BOMCancelQty = (TempCostCenterREcord.BOMCancelQty ?? 0) - BomQty;
                            TempCostCenterREcord.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatusExtended.Add(TempCostCenterREcord);
                        }

                    }

                }
            }

            DbContext.Dispose();
        }

        void JobOrderCancelEvents__onLineDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.JobOrderCancelLine.Where(m => m.JobOrderCancelLineId == EventArgs.DocLineId).FirstOrDefault();

            var Boms = DbContext.JobOrderCancelBom.Where(m => m.JobOrderCancelLineId == EventArgs.DocLineId).ToList();

            Decimal BomQty = 0;
            if (Boms != null && Boms.Count > 0)
                BomQty = Boms.Select(m => m.Qty).Sum();

            var JobORderLien = db.JobOrderLine.Find(Temp.JobOrderLineId);

            var JobHeader = DbContext.JobOrderHeader.Find(JobORderLien.JobOrderHeaderId);

            DbContext.Dispose();

            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();


            if (DocType.Contains(JobHeader.DocTypeId) && Temp.Qty != 0)
            {
                if (JobHeader.CostCenterId.HasValue)
                {
                    var CostCenterStatus = (from p in db.CostCenterStatusExtended
                                            where p.CostCenterId == JobHeader.CostCenterId
                                            select p).FirstOrDefault();

                    if (CostCenterStatus != null)
                    {
                        CostCenterStatus.OrderCancelQty = (CostCenterStatus.OrderCancelQty ?? 0) - Temp.Qty;
                        CostCenterStatus.OrderCancelDealQty = (CostCenterStatus.OrderCancelDealQty ?? 0) - Temp.Qty * JobORderLien.UnitConversionMultiplier;
                        CostCenterStatus.BOMCancelQty = (CostCenterStatus.BOMCancelQty ?? 0) - BomQty;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }

        void JobOrderCancelEvents__onLineSaveBulk(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.JobOrderCancelLine.Local.Where(m => m.JobOrderCancelHeaderId == EventArgs.DocId).ToList();

            var Boms = db.JobOrderCancelBom.Local.Where(m => m.JobOrderCancelHeaderId == EventArgs.DocId).ToList();

            var JobOrderLineIds = Temp.Select(m => m.JobOrderLineId).ToArray();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var JobHeader = (from p in DbContext.JobOrderHeader
                             join t in DbContext.JobOrderLine on p.JobOrderHeaderId equals t.JobOrderHeaderId
                             where JobOrderLineIds.Contains(t.JobOrderLineId) && p.CostCenterId != null
                             select new
                             {
                                 JLineId = t.JobOrderLineId,
                                 UniConvMul = t.UnitConversionMultiplier,
                                 JHeaderId = t.JobOrderHeaderId,
                                 CostCenterId = p.CostCenterId,
                                 DocTypeId = p.DocTypeId,
                             }
                               ).ToList();

            var Join = (from p in Temp
                        join t in JobHeader on p.JobOrderLineId equals t.JLineId
                        select new
                        {
                            Qty = p.Qty,
                            UnitConvMul = t.UniConvMul,
                            LineId = t.JLineId,
                            JHeaderid = t.JHeaderId,
                            CostCenterId = t.CostCenterId,
                        }).ToList();

            var BomJoin = (from p in Temp
                           join t in Boms on p.JobOrderCancelLineId equals t.JobOrderCancelLineId
                           join t2 in JobHeader on p.JobOrderLineId equals t2.JLineId
                           group t by t2.CostCenterId into g
                           select new
                           {
                               Boms = g.Sum(m => m.Qty),
                               CostCenterId = g.Key,
                           }).ToList();

            var CostCenterIds = JobHeader.Select(m => m.CostCenterId).ToArray();

            DbContext.Dispose();

            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            if (JobHeader.Select(m => m.DocTypeId).Intersect(DocType).Any())
            {
                var CostCenterRecords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                foreach (var item in JobHeader.GroupBy(m => m.CostCenterId))
                {
                    var TempCostCenterRecord = CostCenterRecords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

                    decimal BomsQty = 0;
                    var TEmpBoms = BomJoin.Where(m => m.CostCenterId == item.Key).ToList();
                    if (TEmpBoms != null && TEmpBoms.Count > 0)
                        BomsQty = TEmpBoms.Sum(m => m.Boms);

                    if (DocType.Contains(item.Max(m => m.DocTypeId)) && TempCostCenterRecord != null)
                    {
                        if (item.Max(m => m.CostCenterId).HasValue)
                        {

                            TempCostCenterRecord.OrderCancelQty = (TempCostCenterRecord.OrderCancelQty ?? 0) + Join.Where(m => m.CostCenterId == item.Key).Select(m => m.Qty).Sum();
                            TempCostCenterRecord.OrderCancelDealQty = (TempCostCenterRecord.OrderCancelDealQty ?? 0) + Join.Where(m => m.CostCenterId == item.Key).Select(m => m.Qty * m.UnitConvMul).Sum();
                            TempCostCenterRecord.BOMCancelQty = (TempCostCenterRecord.BOMCancelQty ?? 0) + BomsQty;

                        }
                        TempCostCenterRecord.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(TempCostCenterRecord);
                    }

                }
            }
        }

        void JobOrderCancelEvents__onHeaderApprove(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool JobOrderCancelEvents__beforeHeaderApprove(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            return false;
        }

        void JobOrderEvents__afterHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool JobOrderEvents__beforeLineDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }


        bool JobOrderEvents__beforeHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobOrderEvents__afterHeaderSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobOrderEvents__onHeaderSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool JobOrderEvents__beforeHeaderSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
