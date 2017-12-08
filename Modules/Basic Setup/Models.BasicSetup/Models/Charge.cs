using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class Charge : EntityBase, IHistoryLog
    {


        [Key]
        [Display(Name = "Charge Id")]
        public int ChargeId { get; set; }

        [MaxLength(50, ErrorMessage = "Charge Name cannot exceed 50 characters"), Required]
        [Index("IX_Charge_Charge", IsUnique = true)]
        [Display(Name = "Charge Name")]
        public string ChargeName { get; set; }

        [MaxLength(50, ErrorMessage = "Charge Code cannot exceed 50 characters"), Required]
        [Index("IX_Charge_ChargeCode", IsUnique = true)]
        [Display(Name = "Charge Code")]
        public string ChargeCode { get; set; }

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

    }
}
