using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class LedgerAdj : EntityBase
    {


        [Key]
        public int LedgerAdjId { get; set; }

        //[Display(Name = "Ledger Id"), Required]
        //[ForeignKey("Ledger")]
        //public int LedgerId { get; set; }
        //public virtual Ledger Ledger { get; set; }

        [Display(Name = "Debit Ledger Id")]
        [ForeignKey("DrLedger")]
        public int? DrLedgerId { get; set; }
        public virtual Ledger DrLedger { get; set; }

        [Display(Name = "Credit Ledger Id")]
        [ForeignKey("CrLedger")]
        public int? CrLedgerId { get; set; }
        public virtual Ledger CrLedger { get; set; }
        
        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [MaxLength(20)]
        public string Adj_Type { get; set; }

        public Decimal Amount { get; set; }




        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }


    }
}
