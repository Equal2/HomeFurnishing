using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class DeliveryTerms : EntityBase, IHistoryLog
    {
        public DeliveryTerms()
        {
        }

        [Key]
        public int DeliveryTermsId { get; set; }
        [Display (Name="Delivery Terms")]
        [MaxLength(50, ErrorMessage = "Deliveryterms Name cannot exceed 50 characters"), Required]
        [Index("IX_DeliveryTerms_DeliveryTermsName", IsUnique = true)]
        public string DeliveryTermsName { get; set; }

        [MaxLength(50), Required]
        public string PrintingDescription { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
