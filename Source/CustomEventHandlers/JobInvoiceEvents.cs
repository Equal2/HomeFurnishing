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
using JobInvoiceDocumentEvents;
using System.Data;

namespace Jobs.Controllers
{


    public class JobInvoiceEvents : JobInvoiceDocEvents
    {
        //For Subscribing Events
        public JobInvoiceEvents()
        {
            Initialized = true;
            //_beforeHeaderSave += JobInvoiceEvents__beforeHeaderSave;
            //_onHeaderSave += JobInvoiceEvents__onHeaderSave;
            //_afterHeaderSave += JobInvoiceEvents__afterHeaderSave;
            //_beforeHeaderDelete += JobInvoiceEvents__beforeHeaderDelete;
            _onLineSave += JobInvoiceEvents__onLineSave;
            _onLineDelete += JobInvoiceEvents__onLineDelete;
            _onHeaderDelete += JobInvoiceEvents__onHeaderDelete;
            //_beforeLineDelete += JobInvoiceEvents__beforeLineDelete;
            //_afterHeaderDelete += JobInvoiceEvents__afterHeaderDelete;
            _onLineSaveBulk += JobInvoiceEvents__onLineSaveBulk;
            _onHeaderSubmit += JobInvoiceEvents__onHeaderSubmit;
        }


        private void JobInvoiceEvents__onHeaderSubmit(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {

            int Id = EventArgs.DocId;

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];


            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();


                    using (SqlCommand cmd = new SqlCommand("Web.SpUpdate_JobOrderStatusUpdateFromInvoice"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@JobInvoiceHeaderId", Id);
                        cmd.CommandTimeout = 1000;
                        //cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        //cmd.Connection.Close();
                    }

                }
            }

            catch (Exception ex)
            {
                //Header.Status = (int)StatusConstants.Drafted;
                //new JobReceiveHeaderService(_unitOfWork).Update(Header);
                //_unitOfWork.Save();
                throw ex;
            }

            // return Redirect(ReturnUrl);
        }





        void JobInvoiceEvents__onHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = (from p in db.JobInvoiceLine
                        join p1 in db.JobReceiveLine on p.JobReceiveLineId equals p1.JobReceiveLineId
                        join t in db.JobOrderLine on p1.JobOrderLineId equals t.JobOrderLineId
                        join t2 in db.JobOrderHeader on t.JobOrderHeaderId equals t2.JobOrderHeaderId
                        where p.JobInvoiceHeaderId == EventArgs.DocId && t2.CostCenterId != null
                        select new
                        {
                            LineId = t.JobOrderLineId,
                            Qty = p1.Qty,
                            UnitConvMul = t.UnitConversionMultiplier,
                            HeaderId = t.JobOrderHeaderId,
                            CostCenterId = t2.CostCenterId,
                            DocTypeId = t2.DocTypeId,
                            Amount = p.Amount,
                            InvoiceLineId = p.JobInvoiceLineId,
                        }).ToList();

            var CostCenterIds = Temp.Select(m => m.CostCenterId).ToArray();

            var InvLineIds = Temp.Select(m => m.InvoiceLineId).ToArray();

            #region InvoicePenalty&Incentive

            var LineCharges = (from p in DbContext.JobInvoiceLineCharge.AsNoTracking()
                               where InvLineIds.Contains(p.LineTableId)
                               select p).ToList();
            var IncentiveChargeId = DbContext.Charge.Where(m => m.ChargeName == ChargeConstants.Incentive).FirstOrDefault().ChargeId;
            var PenaltyChargeId = DbContext.Charge.Where(m => m.ChargeName == ChargeConstants.Penalty).FirstOrDefault().ChargeId;

            #endregion


            DbContext.Dispose();



            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            var RecDocTypes = Temp.Select(m => m.DocTypeId).ToList();

            if (Temp.Select(m => m.DocTypeId).Intersect(DocType).Any())
            {

                var CostCenterRecords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                foreach (var item in Temp.GroupBy(m => m.CostCenterId))
                {
                    var TempCostCenterREcord = CostCenterRecords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

                    var Incentive = LineCharges.Where(m => item.Select(t => t.InvoiceLineId).ToArray().Contains(m.LineTableId) && m.ChargeId == IncentiveChargeId).Sum(m => m.Amount);
                    var Penanly = LineCharges.Where(m => item.Select(t => t.InvoiceLineId).ToArray().Contains(m.LineTableId) && m.ChargeId == PenaltyChargeId).Sum(m => m.Amount);

                    if (DocType.Contains(item.Max(x => x.DocTypeId)))
                    {
                        if (TempCostCenterREcord != null)
                        {
                            TempCostCenterREcord.InvoiceQty = (TempCostCenterREcord.InvoiceQty ?? 0) - item.Select(m => m.Qty).Sum();
                            TempCostCenterREcord.InvoiceDealQty = (TempCostCenterREcord.InvoiceDealQty ?? 0) - item.Select(m => m.Qty * m.UnitConvMul).Sum();
                            TempCostCenterREcord.InvoiceAmount = (TempCostCenterREcord.InvoiceAmount ?? 0) - item.Select(m => m.Amount).Sum();
                            TempCostCenterREcord.ObjectState = Model.ObjectState.Modified;
                            TempCostCenterREcord.ReceiveIncentiveAmount = (TempCostCenterREcord.ReceiveIncentiveAmount ?? 0) - Incentive;
                            TempCostCenterREcord.ReceivePenaltyAmount = (TempCostCenterREcord.ReceivePenaltyAmount ?? 0) - Penanly;
                            db.CostCenterStatusExtended.Add(TempCostCenterREcord);
                        }
                    }
                }

            }

        }

        void JobInvoiceEvents__onLineDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            ApplicationDbContext DbContext = new ApplicationDbContext();

            var Temp = DbContext.JobInvoiceLine.Where(m => m.JobInvoiceLineId == EventArgs.DocLineId).FirstOrDefault();

            var JobRecLien = (from p in db.JobReceiveLine
                              join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                              join t2 in db.JobOrderHeader on t.JobOrderHeaderId equals t2.JobOrderHeaderId
                              where p.JobReceiveLineId == Temp.JobReceiveLineId
                              select new
                              {
                                  JobOrderHeaderId = t2.JobOrderHeaderId,
                                  UnitConversionMultiplier = t.UnitConversionMultiplier,
                                  Qty = p.PassQty,
                                  JobOrderLineId = t.JobOrderLineId,
                                  CostCenterId = t2.CostCenterId,
                                  DocTypeId = t2.DocTypeId,
                              }).FirstOrDefault();

            #region InvoicePenalty&Incentive

            var LineCharges = (from p in DbContext.JobInvoiceLineCharge.AsNoTracking()
                               where p.LineTableId == EventArgs.DocLineId
                               select p).ToList();
            var IncentiveChargeId = DbContext.Charge.Where(m => m.ChargeName == ChargeConstants.Incentive).FirstOrDefault().ChargeId;
            var PenaltyChargeId = DbContext.Charge.Where(m => m.ChargeName == ChargeConstants.Penalty).FirstOrDefault().ChargeId;

            #endregion

            //var JobHeader = DbContext.JobOrderHeader.Find(JobORderLien.JobOrderHeaderId);

            DbContext.Dispose();

            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();


            if (DocType.Contains(JobRecLien.DocTypeId) && Temp.Qty != 0)
            {
                if (JobRecLien.CostCenterId.HasValue)
                {
                    var CostCenterStatus = (from p in db.CostCenterStatusExtended
                                            where p.CostCenterId == JobRecLien.CostCenterId
                                            select p).FirstOrDefault();

                    var Incentive = LineCharges.Where(m => m.LineTableId == Temp.JobInvoiceLineId && m.ChargeId == IncentiveChargeId).Sum(m => m.Amount);
                    var Penanly = LineCharges.Where(m => m.LineTableId == Temp.JobInvoiceLineId && m.ChargeId == PenaltyChargeId).Sum(m => m.Amount);


                    if (CostCenterStatus != null)
                    {
                        CostCenterStatus.InvoiceQty = (CostCenterStatus.InvoiceQty ?? 0) - JobRecLien.Qty;
                        CostCenterStatus.InvoiceDealQty = (CostCenterStatus.InvoiceDealQty ?? 0) - JobRecLien.Qty * JobRecLien.UnitConversionMultiplier;
                        CostCenterStatus.InvoiceAmount = (CostCenterStatus.InvoiceAmount ?? 0) - Temp.Amount;
                        CostCenterStatus.ReceiveIncentiveAmount = (CostCenterStatus.ReceiveIncentiveAmount ?? 0) - Incentive;
                        CostCenterStatus.ReceivePenaltyAmount = (CostCenterStatus.ReceivePenaltyAmount ?? 0) - Penanly;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }

        void JobInvoiceEvents__onLineSaveBulk(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.JobInvoiceLine.Local.Where(m => m.JobInvoiceHeaderId == EventArgs.DocId).ToList();

            //var JobOrderLineIds = Temp.Select(m => m.JobOrderLineId).ToArray();

            #region InvoicePenalty&Incentive

            var InvLineIds = Temp.Select(m => m.JobInvoiceLineId).ToArray();

            var LineCharges = (from p in db.JobInvoiceLineCharge.Local
                               where InvLineIds.Contains(p.LineTableId)
                               select p).ToList();
            #endregion


            var JobREceiveLineIds = Temp.Select(m => m.JobReceiveLineId).ToArray();

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var IncentiveChargeId = DbContext.Charge.Where(m => m.ChargeName == ChargeConstants.Incentive).FirstOrDefault().ChargeId;
            var PenaltyChargeId = DbContext.Charge.Where(m => m.ChargeName == ChargeConstants.Penalty).FirstOrDefault().ChargeId;


            var JobHeader = (from p in DbContext.JobOrderHeader
                             join t in DbContext.JobOrderLine on p.JobOrderHeaderId equals t.JobOrderHeaderId
                             join t2 in DbContext.JobReceiveLine on t.JobOrderLineId equals t2.JobOrderLineId
                             where JobREceiveLineIds.Contains(t2.JobReceiveLineId) && p.CostCenterId != null
                             select new
                             {
                                 JLineId = t.JobOrderLineId,
                                 UniConvMul = t.UnitConversionMultiplier,
                                 JHeaderId = t.JobOrderHeaderId,
                                 CostCenterId = p.CostCenterId,
                                 DocTypeId = p.DocTypeId,
                                 RecLineId = t2.JobReceiveLineId,
                                 Qty = t2.PassQty,
                             }
                               ).ToList();

            DbContext.Dispose();

            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();


            if (DocType.Intersect(JobHeader.Select(m => m.DocTypeId)).Any())
            {
                var Join = (from p in Temp
                            join t in JobHeader on p.JobReceiveLineId equals t.RecLineId
                            select new
                            {
                                Qty = t.Qty,
                                UnitConvMul = t.UniConvMul,
                                LineId = t.JLineId,
                                JHeaderid = t.JHeaderId,
                                RecLineId = p.JobReceiveLineId,
                                Amount = p.Amount,
                                CostCenterId = t.CostCenterId,
                                ILineId = p.JobInvoiceLineId,
                            }).ToList();

                var CostCenterIds = JobHeader.Select(m => m.CostCenterId).ToArray();

                var CostCenterRecords = (from p in db.CostCenterStatusExtended
                                         where CostCenterIds.Contains(p.CostCenterId)
                                         select p).ToList();

                foreach (var item in JobHeader.GroupBy(m => m.CostCenterId))
                {
                    var TempCostCenterRecord = CostCenterRecords.Where(m => m.CostCenterId == item.Max(x => x.CostCenterId)).FirstOrDefault();

                    var Incentive = LineCharges.Where(m => Join.Where(t => t.CostCenterId == item.Key).Select(t => t.ILineId).ToArray().Contains(m.LineTableId) && m.ChargeId == IncentiveChargeId).Sum(m => m.Amount);
                    var Penanly = LineCharges.Where(m => Join.Where(t => t.CostCenterId == item.Key).Select(t => t.ILineId).ToArray().Contains(m.LineTableId) && m.ChargeId == PenaltyChargeId).Sum(m => m.Amount);


                    if (DocType.Contains(item.Max(x => x.DocTypeId)) && TempCostCenterRecord != null)
                    {
                        if (item.Max(x => x.CostCenterId).HasValue)
                        {

                            TempCostCenterRecord.InvoiceQty = (TempCostCenterRecord.InvoiceQty ?? 0) + Join.Where(m => m.CostCenterId == item.Key).Select(m => m.Qty).Sum();
                            TempCostCenterRecord.InvoiceDealQty = (TempCostCenterRecord.InvoiceDealQty ?? 0) + Join.Where(m => m.CostCenterId == item.Key).Select(m => m.Qty * m.UnitConvMul).Sum();
                            TempCostCenterRecord.InvoiceAmount = (TempCostCenterRecord.InvoiceAmount ?? 0) + Join.Where(m => m.CostCenterId == item.Key).Select(m => m.Amount).Sum();
                            TempCostCenterRecord.ReceiveIncentiveAmount = (TempCostCenterRecord.ReceiveIncentiveAmount ?? 0) + Incentive;
                            TempCostCenterRecord.ReceivePenaltyAmount = (TempCostCenterRecord.ReceivePenaltyAmount ?? 0) + Penanly;
                        }
                        TempCostCenterRecord.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(TempCostCenterRecord);
                    }

                }
            }
        }

        void JobInvoiceEvents__afterHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        bool JobInvoiceEvents__beforeLineDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobInvoiceEvents__onLineSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            var Temp = db.JobInvoiceLine.Local.Where(m => m.JobInvoiceLineId == EventArgs.DocLineId).FirstOrDefault();

            #region InvoicePenalty&Incentive

            var LineCharges = (from p in db.JobInvoiceLineCharge.Local
                               where p.LineTableId == EventArgs.DocLineId
                               select p).ToList();
            #endregion

            ApplicationDbContext DbContext = new ApplicationDbContext();

            var JobRecLien = (from p in db.JobReceiveLine
                              join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                              where p.JobReceiveLineId == Temp.JobReceiveLineId
                              select new
                              {
                                  JobOrderHeaderId = t.JobOrderHeaderId,
                                  UnitConversionMultiplier = t.UnitConversionMultiplier,
                                  RecQty = p.PassQty,
                              }).FirstOrDefault();

            var JobHeader = DbContext.JobOrderHeader.Find(JobRecLien.JobOrderHeaderId);

            var IncentiveChargeId = DbContext.Charge.Where(m => m.ChargeName == ChargeConstants.Incentive).FirstOrDefault().ChargeId;
            var PenaltyChargeId = DbContext.Charge.Where(m => m.ChargeName == ChargeConstants.Penalty).FirstOrDefault().ChargeId;

            DbContext.Dispose();

            var DocType = (from p in db.DocumentType
                           where p.DocumentTypeName == TransactionDoctypeConstants.WeavingFinishOrder || p.DocumentTypeName == TransactionDoctypeConstants.WeavingOrder
                           select p.DocumentTypeId).ToList();

            if (DocType.Contains(JobHeader.DocTypeId) && JobRecLien.RecQty != 0)
            {
                if (JobHeader.CostCenterId.HasValue)
                {

                    var Incentive = LineCharges.Where(m => m.LineTableId == EventArgs.DocLineId && m.ChargeId == IncentiveChargeId).Sum(m => m.Amount);
                    var Penanly = LineCharges.Where(m => m.LineTableId == EventArgs.DocLineId && m.ChargeId == PenaltyChargeId).Sum(m => m.Amount);


                    var CostCenterStatus = (from p in db.CostCenterStatusExtended
                                            where p.CostCenterId == JobHeader.CostCenterId
                                            select p).FirstOrDefault();

                    if (CostCenterStatus != null && EventArgs.DocLineId <= 0)
                    {
                        CostCenterStatus.InvoiceQty = (CostCenterStatus.InvoiceQty ?? 0) + JobRecLien.RecQty;
                        CostCenterStatus.InvoiceDealQty = (CostCenterStatus.InvoiceDealQty ?? 0) + JobRecLien.RecQty * JobRecLien.UnitConversionMultiplier;
                        CostCenterStatus.InvoiceAmount = (CostCenterStatus.InvoiceAmount ?? 0) + Temp.Amount;
                        CostCenterStatus.ReceiveIncentiveAmount = (CostCenterStatus.ReceiveIncentiveAmount ?? 0) + Incentive;
                        CostCenterStatus.ReceivePenaltyAmount = (CostCenterStatus.ReceivePenaltyAmount ?? 0) + Penanly;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                    else if (CostCenterStatus != null && EventArgs.DocLineId > 0)
                    {

                        var IssueLineCostCenterRecords = (from p in db.JobInvoiceLine
                                                          join p1 in db.JobReceiveLine on p.JobReceiveLineId equals p1.JobReceiveLineId
                                                          join t in db.JobOrderLine on p1.JobOrderLineId equals t.JobOrderLineId
                                                          join t2 in db.JobOrderHeader on t.JobOrderHeaderId equals t2.JobOrderHeaderId
                                                          where t2.CostCenterId == JobHeader.CostCenterId
                                                          && p.JobInvoiceLineId != EventArgs.DocLineId
                                                          select new
                                                          {
                                                              Qty = p1.PassQty,
                                                              Amount = p.Amount,
                                                              UnitConversionMultiplier = t.UnitConversionMultiplier,
                                                          }).ToList();

                        var xLineCharges = (from p in db.JobInvoiceLineCharge.AsNoTracking()
                                            where (p.ChargeId == IncentiveChargeId || p.ChargeId == PenaltyChargeId)
                                            && p.LineTableId == EventArgs.DocLineId
                                            select p).ToList();

                        var xIncentive = xLineCharges.Where(m => m.LineTableId == EventArgs.DocLineId && m.ChargeId == IncentiveChargeId).Sum(m => m.Amount);
                        var xPenanly = xLineCharges.Where(m => m.LineTableId == EventArgs.DocLineId && m.ChargeId == PenaltyChargeId).Sum(m => m.Amount);

                        CostCenterStatus.InvoiceQty = IssueLineCostCenterRecords.Select(m => m.Qty).Sum() + JobRecLien.RecQty;
                        CostCenterStatus.InvoiceDealQty = (IssueLineCostCenterRecords.Select(m => m.Qty * m.UnitConversionMultiplier).Sum()) + (JobRecLien.RecQty * JobRecLien.UnitConversionMultiplier);
                        CostCenterStatus.InvoiceAmount = IssueLineCostCenterRecords.Select(m => m.Amount).Sum() + Temp.Amount;
                        CostCenterStatus.ReceiveIncentiveAmount = (CostCenterStatus.ReceiveIncentiveAmount ?? 0) - xIncentive + Incentive;
                        CostCenterStatus.ReceivePenaltyAmount = (CostCenterStatus.ReceivePenaltyAmount ?? 0) - xPenanly + Penanly;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CostCenterStatus);
                    }
                }
            }
        }

        bool JobInvoiceEvents__beforeHeaderDelete(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobInvoiceEvents__afterHeaderSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

        void JobInvoiceEvents__onHeaderSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            //CostCenterStatus Temp = new CostCenterStatus();
            //var JobInvoice = db.JobInvoiceHeader.Local.Where(m => m.JobInvoiceHeaderId == EventArgs.DocId).FirstOrDefault();
            //Temp.CostCenterId = db.CostCenter.Local.Where(m => m.CostCenterId == JobInvoice.CostCenterId).FirstOrDefault().CostCenterId;
            //Temp.ObjectState = Model.ObjectState.Added;
            //db.CostCenterStatus.Add(Temp);
        }

        bool JobInvoiceEvents__beforeHeaderSave(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            throw new NotImplementedException();
        }

    }
}
