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
using JobReceiveQADocumentEvents;
using Model.ViewModel;

namespace Jobs.Controllers
{  
    public class JobReceiveQAEvents : JobReceiveQADocEvents
    {
        //For Subscribing Events
        public JobReceiveQAEvents()
        {
            Initialized = true;
            _onHeaderSubmit += JobReceiveQAEvents_onSubmit;
        }

        private void JobReceiveQAEvents_onSubmit(object sender, JobEventArgs EventArgs, ref ApplicationDbContext db)
        {
            //Decimal FailedQty = (from L in db.JobReceiveQALine
            //                where L.JobReceiveQAHeaderId == EventArgs.DocId
            //                select new { FailQty = L.FailQty }).FirstOrDefault().FailQty;

            //var Header = (from H in db.JobReceiveQAHeader
            //              where H.JobReceiveQAHeaderId == EventArgs.DocId
            //              select new
            //              {
            //                  JobReceiveQAHeaderId = H.JobReceiveQAHeaderId,
            //                  DocTypeId = H.DocTypeId,
            //                  DocDate = H.DocDate,
            //                  DocNo = H.DocNo,
            //                  DivisionId = H.DivisionId,
            //                  SiteId = H.SiteId,
            //                  ProcessId = H.ProcessId,
            //                  Status = H.Status,
            //                  Remark = H.Remark,
            //                  CreatedBy = H.CreatedBy,
            //                  ModifiedBy = H.ModifiedBy
            //              }).FirstOrDefault();

            //int DocTypeId = (from D in db.DocumentType
            //                where D.DocumentTypeName == TransactionDoctypeConstants.JobQC
            //                select new { DocTypeId = D.DocumentTypeId }).FirstOrDefault().DocTypeId;

            //int DyeingProcessId = (from P in db.Process
            //                       where P.ProcessName == ProcessConstants.Dyeing
            //                       select new { ProcessId = P.ProcessId }).FirstOrDefault().ProcessId;

            //if (FailedQty > 0 && Header.DocTypeId == DocTypeId && Header.ProcessId == DyeingProcessId)
            //{
            //    int PersonId = (from L in db.JobReceiveQALine
            //                    join Rl in db.JobReceiveLine on L.JobReceiveLineId equals Rl.JobReceiveLineId
            //                    join Jol in db.JobOrderLine on Rl.JobOrderLineId equals Jol.JobOrderLineId
            //                    join Joh in db.JobOrderHeaderExtended on Jol.JobOrderHeaderId equals Joh.JobOrderHeaderId
            //                    where L.JobReceiveQAHeaderId == EventArgs.DocId
            //                    select new { PersonId = Joh.PersonId }).FirstOrDefault().PersonId;


            //    ProdOrderHeader ProdOrderHeader = new ProdOrderHeader();
            //    ProdOrderHeader.ProdOrderHeaderId = -1;
            //    ProdOrderHeader.DocTypeId = Header.DocTypeId;
            //    ProdOrderHeader.DocDate = Header.DocDate;
            //    ProdOrderHeader.DocNo = Header.DocNo;
            //    ProdOrderHeader.DivisionId = Header.DivisionId;
            //    ProdOrderHeader.SiteId = Header.SiteId;
            //    ProdOrderHeader.DueDate = Header.DocDate;
            //    ProdOrderHeader.ReferenceDocTypeId = Header.DocTypeId;
            //    ProdOrderHeader.ReferenceDocId = Header.JobReceiveQAHeaderId;
            //    ProdOrderHeader.Status = Header.Status;
            //    ProdOrderHeader.Remark = Header.Remark;
            //    ProdOrderHeader.BuyerId = PersonId;
            //    ProdOrderHeader.CreatedBy = Header.CreatedBy;
            //    ProdOrderHeader.CreatedDate = DateTime.Now;
            //    ProdOrderHeader.ModifiedBy = Header.ModifiedBy;
            //    ProdOrderHeader.ModifiedDate = DateTime.Now;
            //    ProdOrderHeader.LockReason = "Prod order automatically generated from Job QA.";
            //    ProdOrderHeader.ObjectState = Model.ObjectState.Added;
            //    db.ProdOrderHeader.Add(ProdOrderHeader);


            //    IEnumerable<JobReceiveQALineViewModel> Line = (from L in db.JobReceiveQALine
            //                                                   where L.JobReceiveQAHeaderId == EventArgs.DocId
            //                                                   select new JobReceiveQALineViewModel
            //                                                     {
            //                                                         ProductId = L.JobReceiveLine.JobOrderLine.ProductId,
            //                                                         Dimension1Id = L.JobReceiveLine.JobOrderLine.Dimension1Id,
            //                                                         Dimension2Id = L.JobReceiveLine.JobOrderLine.Dimension2Id,
            //                                                         JobReceiveQALineId = L.JobReceiveQALineId,
            //                                                         FailQty = L.FailQty
            //                                                     }).ToList();


            //    foreach (var item in Line)
            //    {
            //        ProdOrderLine ProdOrderLine = new ProdOrderLine();
            //        ProdOrderLine.ProdOrderLineId = -2;
            //        ProdOrderLine.ProdOrderHeaderId = ProdOrderHeader.ProdOrderHeaderId;
            //        ProdOrderLine.ProductId = item.ProductId;
            //        ProdOrderLine.Dimension1Id = item.Dimension1Id;
            //        ProdOrderLine.Dimension2Id = item.Dimension2Id;
            //        ProdOrderLine.Specification = null;
            //        ProdOrderLine.ProcessId = Header.ProcessId;
            //        ProdOrderLine.ReferenceDocTypeId = ProdOrderHeader.DocTypeId;
            //        ProdOrderLine.ReferenceDocLineId = item.JobReceiveLineId;
            //        ProdOrderLine.Sr = item.Sr;
            //        ProdOrderLine.Qty = item.FailQty;
            //        ProdOrderLine.Remark = item.Remark;
            //        ProdOrderLine.CreatedBy = ProdOrderHeader.CreatedBy;
            //        ProdOrderLine.ModifiedBy = ProdOrderHeader.ModifiedBy;
            //        ProdOrderLine.CreatedDate = DateTime.Now;
            //        ProdOrderLine.ModifiedDate = DateTime.Now;
            //        ProdOrderLine.LockReason = "Prod order automatically generated from recipe.";
            //        ProdOrderLine.ObjectState = Model.ObjectState.Added;
            //        db.ProdOrderLine.Add(ProdOrderLine);
            //    }
            //}
        }
    }
}
