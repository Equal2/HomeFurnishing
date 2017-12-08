using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class JobRatePenaltyUpdateViewModel
    {

        public int JobReceiveLineId { get; set; }
        public int JobOrderLineId { get; set; }
        public int? JobReceiveQAPenaltyId { get; set; }
        public string ProductUidName { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string JobWorkerName { get; set; }
        public string OrderNo { get; set; }
        public string ProductName { get; set; }
        public Decimal? DealQty { get; set; }
        public string PenaltyResion { get; set; }
        public string PenaltyRemark { get; set; }
        public Decimal? Rate { get; set; }
        public Decimal? PenaltyAmount { get; set; }

    }

    public class JobRatePenaltyUpdateIndexViewModel
    {
        public int LedgerAccountId { get; set; }
        public string LedgerAccountName { get; set; }
    }
}
