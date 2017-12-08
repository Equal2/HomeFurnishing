using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Model.ViewModel;

namespace Jobs.Controllers
{
    [Authorize]
    public class UpdateProductUIdStat
    {
        //public static ProductUid MapGodownIssue(StockHeader StockRecord,ref ProductUid UidOldRecord)
        //{            
        //    UidOldRecord.LastTransactionDocId = StockRecord.StockHeaderId;
        //    UidOldRecord.LastTransactionDocNo = StockRecord.DocNo;
        //    UidOldRecord.LastTransactionDocTypeId = StockRecord.DocTypeId;
        //    UidOldRecord.LastTransactionDocDate = StockRecord.DocDate;
        //    UidOldRecord.LastTransactionPersonId = StockRecord.PersonId;
        //    UidOldRecord.CurrenctGodownId = null;
        //    UidOldRecord.Status = ProductUidStatusConstants.Issue;

        //    return UidOldRecord;
            
        //}

        //public static ProductUid MapGodownReceive(StockHeader StockRecord,ref ProductUid UidOldRecord)
        //{

        //    UidOldRecord.LastTransactionDocId = StockRecord.StockHeaderId;
        //    UidOldRecord.LastTransactionDocNo = StockRecord.DocNo;
        //    UidOldRecord.LastTransactionDocTypeId = StockRecord.DocTypeId;
        //    UidOldRecord.LastTransactionDocDate = StockRecord.DocDate;
        //    UidOldRecord.LastTransactionPersonId = StockRecord.PersonId;
        //    UidOldRecord.CurrenctGodownId = StockRecord.GodownId;
        //    UidOldRecord.CurrenctProcessId = StockRecord.ProcessId;
        //    UidOldRecord.Status = ProductUidStatusConstants.Receive;

        //    return UidOldRecord;

            
        //}

        public static ProductUid MapGodownTransfer(StockHeader StockRecord, ProductUid UidOldRecord)
        {

            UidOldRecord.LastTransactionDocId = StockRecord.StockHeaderId;
            UidOldRecord.LastTransactionDocNo = StockRecord.DocNo;
            UidOldRecord.LastTransactionDocTypeId = StockRecord.DocTypeId;
            UidOldRecord.LastTransactionDocDate = StockRecord.DocDate;
            UidOldRecord.LastTransactionPersonId = StockRecord.PersonId;
            UidOldRecord.CurrenctGodownId = StockRecord.GodownId;
            UidOldRecord.CurrenctProcessId = StockRecord.ProcessId;
            UidOldRecord.Status = ProductUidStatusConstants.Transfer;

            return UidOldRecord;
        }   
      
    }

  
}
