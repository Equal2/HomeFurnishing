using CustomEventArgs;
using Data.Models;
using System;
using JobReceiveDocumentEvents;
using System.Linq;
using Core.Common;

namespace Jobs.Controllers
{


    public class JobReceiveEvents : JobReceiveDocEvents
    {
        int MainSiteId = 17;
        //For Subscribing Events
        public JobReceiveEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += JobReceiveEvents__beforeHeaderSave;
            //_onHeaderSave += JobReceiveEvents__onHeaderSave;
            //_afterHeaderSave += JobReceiveEvents__afterHeaderSave;
            //_beforeHeaderDelete += JobReceiveEvents__beforeHeaderDelete;
            _onLineSaveBulk += JobReceiveEvents__onLineSaveBulk;
            _onLineSaveBulk += JobReceiveEvents_BranchValidation;
            _onLineSave += JobReceiveEvents__onLineSave;
            _onLineSave += JobReceiveEvents_BranchValidation;
            _onLineDelete += JobReceiveEvents__onLineDelete;
            _onHeaderDelete += JobReceiveEvents__onHeaderDelete;
            //_beforeLineDelete += JobReceiveEvents__beforeLineDelete;
            //_afterHeaderDelete += JobReceiveEvents__afterHeaderDelete;
            //_beforeLineSave += JobReceiveEvents__beforeLineSave;            
        }

        void JobReceiveEvents__onHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = (from p in DbContext.JobReceiveLine
                        join t in DbContext.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                        join t2 in DbContext.JobOrderHeader on t.JobOrderHeaderId equals t2.JobOrderHeaderId
                        where p.JobReceiveHeaderId == EventArgs.DocId && t2.CostCenterId != null
                        select new
                        {
                            LineId = t.JobOrderLineId,
                            Qty = p.Qty,
                            UnitConvMul = t.UnitConversionMultiplier,
                            HeaderId = t.JobOrderHeaderId,
                            CostCenterId = t2.CostCenterId,
                            DocTypeId = t2.DocTypeId,
                            IncAmt = p.IncentiveAmt,
                            PenAmt = p.PenaltyAmt,
                        }).ToList();

            var Boms = (from p in DbContext.StockProcess
                        let StockHeaderId = DbContext.JobReceiveHeader.Where(m => m.JobReceiveHeaderId == EventArgs.DocId).FirstOrDefault().StockHeaderId
                        where p.StockHeaderId == StockHeaderId && p.CostCenterId != null
                        group p by p.CostCenterId into g
                        select new
                        {
                            BomQty = g.Sum(m => m.Qty_Iss),
                            HeaderId = g.Key,
                        }).ToList();

            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();


            DbContext.Dispose();

            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            if (Temp.Select(m => m.DocTypeId).Intersect(DocType).Any())
            {

                var CostCenterRecords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                foreach (var item in Temp.GroupBy(m => m.CostCenterId))
                {

                    var TempCostCenterREcord = CostCenterRecords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

                    var TempBoms = Boms.Where(m => m.HeaderId == item.Max(x => x.CostCenterId)).ToList();

                    decimal BomQty = 0;

                    if (TempBoms != null && TempBoms.Count > 0)
                        BomQty = TempBoms.Sum(m => m.BomQty);

                    if (DocType.Contains(item.Max(x => x.DocTypeId)))
                    {
                        if (TempCostCenterREcord != null)
                        {
                            TempCostCenterREcord.ReceiveQty = (TempCostCenterREcord.ReceiveQty ?? 0) - item.Select(m => m.Qty).Sum();
                            TempCostCenterREcord.ReceiveDealQty = (TempCostCenterREcord.ReceiveDealQty ?? 0) - item.Select(m => m.Qty * m.UnitConvMul).Sum();
                            //TempCostCenterREcord.ReceiveIncentiveAmount = (TempCostCenterREcord.ReceiveIncentiveAmount ?? 0) - item.Select(m => m.IncAmt).Sum();
                            //TempCostCenterREcord.ReceivePenaltyAmount = (TempCostCenterREcord.ReceivePenaltyAmount ?? 0) - item.Select(m => m.PenAmt).Sum();
                            TempCostCenterREcord.ConsumeQty = (TempCostCenterREcord.ConsumeQty ?? 0) - BomQty;

                            TempCostCenterREcord.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatusExtended.Add(TempCostCenterREcord);
                        }
                    }
                }
            }
        }

        void JobReceiveEvents__onLineDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.JobReceiveLine.Where(m => m.JobReceiveLineId == EventArgs.DocLineId).FirstOrDefault();

            var Boms = (from p in DbContext.StockProcess.AsNoTracking()
                        let StockHeaderId = DbContext.JobReceiveHeader.Where(m => m.JobReceiveHeaderId == EventArgs.DocId).FirstOrDefault().StockHeaderId
                        where p.StockHeaderId == StockHeaderId
                        select p).ToList();

            DbContext.JobReceiveBom.Where(m => m.JobReceiveLineId == EventArgs.DocLineId).ToList();

            //Decimal BomQty = 0;
            //if (Boms != null && Boms.Count > 0)
            //    BomQty = Boms.Select(m => m.Qty_Iss).Sum();

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
                    var CostCenterStatus = db.CostCenterStatusExtended.Find(JobHeader.CostCenterId);

                    if (CostCenterStatus != null)
                    {
                        CostCenterStatus.ReceiveQty = (CostCenterStatus.ReceiveQty ?? 0) - Temp.Qty;
                        CostCenterStatus.ReceiveDealQty = (CostCenterStatus.ReceiveDealQty ?? 0) - Temp.Qty * JobORderLien.UnitConversionMultiplier;
                        //CostCenterStatus.ReceiveIncentiveAmount = (CostCenterStatus.ReceiveIncentiveAmount ?? 0) - (Temp.IncentiveAmt ?? 0);
                        //CostCenterStatus.ReceivePenaltyAmount = (CostCenterStatus.ReceivePenaltyAmount ?? 0) - Temp.PenaltyAmt;
                        CostCenterStatus.ConsumeQty = (CostCenterStatus.ConsumeQty ?? 0) - Boms.Where(m => m.CostCenterId == CostCenterStatus.CostCenterId).Sum(m => m.Qty_Iss);
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }

        void JobReceiveEvents__onLineSaveBulk(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.JobReceiveLine.Local.Where(m => m.JobReceiveHeaderId == EventArgs.DocId).ToList();

            var JobOrderLineIds = Temp.Select(m => m.JobOrderLineId).ToArray();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var JobRecHeader = (from p in db.JobReceiveHeader.AsNoTracking()
                                where p.JobReceiveHeaderId == EventArgs.DocId
                                select p).FirstOrDefault();

            var Boms = db.StockProcess.Local.Where(m => m.StockHeaderId == JobRecHeader.StockHeaderId).ToList();

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

            var DocType = (from p in DbContext.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            DbContext.Dispose();

            if (JobHeader.Select(m => m.DocTypeId).Intersect(DocType).Any())
            {


                var Join = (from p in Temp
                            join t in JobHeader on p.JobOrderLineId equals t.JLineId
                            select new
                            {
                                Qty = p.Qty,
                                UnitConvMul = t.UniConvMul,
                                LineId = t.JLineId,
                                JHeaderid = t.JHeaderId,
                                IncAmt = p.IncentiveAmt,
                                PenAmt = p.PenaltyAmt,
                                CostCenterId = t.CostCenterId,
                            }).ToList();

                var BomJoin = (from p in Boms
                               group p by p.CostCenterId into g
                               select new
                               {
                                   Boms = g.Sum(m => m.Qty_Iss),
                                   CostCenterId = g.Key,
                               }).ToList();

                var CostCenterIds = JobHeader.Select(m => m.CostCenterId).ToArray();

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

                            TempCostCenterRecord.ReceiveQty = (TempCostCenterRecord.ReceiveQty ?? 0) + Join.Where(m => m.CostCenterId == item.Key).Select(m => m.Qty).Sum();
                            TempCostCenterRecord.ReceiveDealQty = (TempCostCenterRecord.ReceiveDealQty ?? 0) + Join.Where(m => m.CostCenterId == item.Key).Select(m => m.Qty * m.UnitConvMul).Sum();
                            //TempCostCenterRecord.ReceiveIncentiveAmount = (TempCostCenterRecord.ReceiveIncentiveAmount ?? 0) + Join.Where(m => m.CostCenterId == item.Key).Select(m => m.IncAmt).Sum();
                            //TempCostCenterRecord.ReceivePenaltyAmount = (TempCostCenterRecord.ReceivePenaltyAmount ?? 0) + Join.Where(m => m.CostCenterId == item.Key).Select(m => m.PenAmt).Sum();
                            TempCostCenterRecord.ConsumeQty = (TempCostCenterRecord.ConsumeQty ?? 0) + BomsQty;

                        }
                        TempCostCenterRecord.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(TempCostCenterRecord);
                    }
                }
            }
        }

        void JobReceiveEvents_BranchValidation(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var JobReceiveHeaders = db.JobReceiveHeader.AsNoTracking().Where(m => m.JobReceiveHeaderId == EventArgs.DocId).FirstOrDefault();
            int WeavingReceiveDocTypeId = 0;
            var temp = db.DocumentType.AsNoTracking().Where(m => m.DocumentTypeName == TransactionDoctypeConstants.WeavingBazar).FirstOrDefault();
            if (temp != null)
            {
                WeavingReceiveDocTypeId = temp.DocumentTypeId;
            }

            

            if (JobReceiveHeaders.SiteId == MainSiteId && JobReceiveHeaders.DocTypeId == WeavingReceiveDocTypeId)
            {
                var Temp = db.JobReceiveLine.Local.Where(m => m.JobReceiveHeaderId == EventArgs.DocId).Select(m => new { ProductUidId = m.ProductUidId });
                var ProductUids = Temp.Select(m => m.ProductUidId).ToArray();

                ApplicationDbContext DbContext = new ApplicationDbContext();

                var BalanceRecord = (from p in DbContext.ViewJobOrderBalance.AsNoTracking()
                                     where p.ProductUidId != null && ProductUids.Contains(p.ProductUidId) && p.SiteId != MainSiteId
                                     && p.BalanceQty > 0
                                     select p).FirstOrDefault();
                
                if (BalanceRecord != null)
                {
                    if (BalanceRecord.BalanceQty > 0)
                    {
                        throw new Exception("Record must be received in branch before Main");
                    }
                }
            }
        }

        bool JobReceiveEvents__beforeLineSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobReceiveEvents__afterHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool JobReceiveEvents__beforeLineDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobReceiveEvents__onLineSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.JobReceiveLine.Local.Where(m => m.JobReceiveLineId == EventArgs.DocLineId).FirstOrDefault();

            var Boms = db.JobReceiveBom.Local.Where(m => m.JobReceiveLineId == EventArgs.DocLineId).ToList();



            Decimal BomQty = 0;

            if (Boms != null && Boms.Count > 0)
                BomQty = Boms.Select(m => m.Qty).Sum();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var JobOrderLien = DbContext.JobOrderLine.Find(Temp.JobOrderLineId);

            var JobHeader = DbContext.JobOrderHeader.Find(JobOrderLien.JobOrderHeaderId);

            var JobReceiveHeader = (from p in db.JobReceiveHeader.AsNoTracking()
                                    where p.JobReceiveHeaderId == Temp.JobReceiveHeaderId
                                    select p).FirstOrDefault();

            DbContext.Dispose();           

            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            if (DocType.Contains(JobHeader.DocTypeId) && Temp.Qty != 0)
            {
                var ConsumeQty = db.StockProcess.Local.Where(m => m.StockHeaderId == JobReceiveHeader.StockHeaderId).ToList();

                if (JobHeader.CostCenterId.HasValue)
                {

                    var CostCenterStatus = (from p in db.CostCenterStatusExtended
                                            where p.CostCenterId == JobHeader.CostCenterId
                                            select p).FirstOrDefault();

                    if (CostCenterStatus != null && EventArgs.DocLineId <= 0)
                    {
                        CostCenterStatus.ReceiveQty = (CostCenterStatus.ReceiveQty ?? 0) + Temp.Qty;
                        CostCenterStatus.ReceiveDealQty = (CostCenterStatus.ReceiveDealQty ?? 0) + Temp.Qty * JobOrderLien.UnitConversionMultiplier;
                        //CostCenterStatus.ReceiveIncentiveAmount = (CostCenterStatus.ReceiveIncentiveAmount ?? 0) + Temp.IncentiveAmt;
                        //CostCenterStatus.ReceivePenaltyAmount = (CostCenterStatus.ReceivePenaltyAmount ?? 0) + Temp.PenaltyAmt;
                        CostCenterStatus.ConsumeQty = (CostCenterStatus.ConsumeQty ?? 0) + ConsumeQty.Sum(m => m.Qty_Iss);
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                    else if (CostCenterStatus != null && EventArgs.DocLineId > 0)
                    {

                        var IssueLineCostCenterRecords = (from p in db.JobReceiveLine
                                                          join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                                          join t2 in db.JobOrderHeader on t.JobOrderHeaderId equals t2.JobOrderHeaderId
                                                          where t2.CostCenterId == JobHeader.CostCenterId
                                                          && p.JobReceiveLineId != EventArgs.DocLineId
                                                          select new
                                                          {
                                                              Qty = p.PassQty,
                                                              IncentiveAmt = p.IncentiveAmt,
                                                              PenaltyAmt = p.PenaltyAmt,
                                                              UnitConversionMultiplier = t.UnitConversionMultiplier,
                                                          }).ToList();

                        CostCenterStatus.ReceiveQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum() + Temp.Qty;
                        CostCenterStatus.ReceiveDealQty = (IssueLineCostCenterRecords.Select(m => m.Qty * m.UnitConversionMultiplier).Sum()) + (Temp.Qty * JobOrderLien.UnitConversionMultiplier);
                       // CostCenterStatus.ReceiveIncentiveAmount = IssueLineCostCenterRecords.Select(m => m.IncentiveAmt).Sum() + Temp.IncentiveAmt;
                       // CostCenterStatus.ReceivePenaltyAmount = IssueLineCostCenterRecords.Select(m => m.PenaltyAmt).Sum() + Temp.PenaltyAmt;
                        CostCenterStatus.ConsumeQty = (CostCenterStatus.ConsumeQty ?? 0) + ConsumeQty.Sum(m => m.Qty_Iss);
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }

        bool JobReceiveEvents__beforeHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobReceiveEvents__afterHeaderSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobReceiveEvents__onHeaderSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool JobReceiveEvents__beforeHeaderSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
