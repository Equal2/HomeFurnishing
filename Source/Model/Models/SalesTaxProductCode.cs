using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class SalesTaxProductCode : EntityBase, IHistoryLog
    {
        [Key]
        public int SalesTaxProductCodeId { get; set; }

        [Display(Name = "Code")]
        [MaxLength(50, ErrorMessage = "SalesTaxProduct Code cannot exceed 50 characters"), Required]
        [Index("IX_SalesTaxProduct_SalesTaxProductCode", IsUnique = true)]
        public string Code { get; set; }

        [Display (Name="Description")]
        public string Description { get; set; }
                     
        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

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
