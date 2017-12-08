using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Surya.India.Model.Models
{
    public class BuyerAccount : EntityBase, IHistoryLog
    {
        [Key]
        public int BuyerAccountID { get; set; }
        public string BankName { get; set; }
        public string Currency { get; set; }

        public string BankAccount { get; set; }
        public string BankAddress1 { get; set; }


        public string BankAddress2 { get; set; }
        public string BankAddress3 { get; set; }

        public virtual Buyer Buyer { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

    }
}