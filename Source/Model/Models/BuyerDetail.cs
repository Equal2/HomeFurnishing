using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Surya.India.Model.Models
{
    public class BuyerDetail : EntityBase, IHistoryLog
    {
        [Key]
        public int BuyerDetailID { get; set; }
        public string Country { get; set; }
        public string City { get; set; }

        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Address1 { get; set; }

        public string Address2{ get; set; }
        public string Address3 { get; set; }

        public string ContectNumber1 { get; set; }
        public string AdditionalContectNo { get; set; }

        public string ContectPerson { get; set; }
        public string FaxNo { get; set; }
        public string EmailAddr1 { get; set; }

        public string EmailAddr2 { get; set; }
        public virtual Buyer Buyer { get; set; }

        [NotMapped]
        public int BuyerId { get; set; }

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