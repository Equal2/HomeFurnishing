using Model;
using Models.BasicSetup.Models;
using Models.Company.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobReceiveLineStatus : EntityBase
    {
        [Key]
        [ForeignKey("JobReceiveLine")]
        [Display(Name = "JobReceiveLine")]
        public int? JobReceiveLineId { get; set; }
        public virtual JobReceiveLine JobReceiveLine { get; set; }

        public Decimal? QaFailQty { get; set; }
        public Decimal? QaFailDealQty { get; set; }
        public Decimal? QaWeight { get; set; }
        public Decimal? QaPenalty { get; set; }
        public DateTime? QaDate { get; set; }


        public Decimal? ReturnQty { get; set; }
        public Decimal? ReturnDealQty { get; set; }
        public Decimal? ReturnWeight { get; set; }
        public DateTime? ReturnDate { get; set; }


        public Decimal? InvoiceQty { get; set; }
        public Decimal? InvoiceDealQty { get; set; }
        public Decimal? InvoiceWeight { get; set; }
        public DateTime? InvoiceDate { get; set; }
    }
}

