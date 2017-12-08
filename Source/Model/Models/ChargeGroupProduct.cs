using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class ChargeGroupProduct : EntityBase, IHistoryLog
    {
        public ChargeGroupProduct()
        {
        }

        [Key]
        public int ChargeGroupProductId { get; set; }

        [Display(Name="Charge Group Product")]
        [MaxLength(50, ErrorMessage = "{0} Name cannot exceed {1} characters"), Required]
        [Index("IX_ChargeGroupProduct_ChargeGroupProductName", IsUnique = true)]
        public string ChargeGroupProductName { get; set; }

        [MaxLength(50)]
        public string PrintingDescription { get; set; }


        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
