using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class Perk : EntityBase, IHistoryLog
    {
        [Key]
        [Display(Name = "Perk Id")]
        public int PerkId { get; set; }

        [MaxLength(50, ErrorMessage = "Perk Name cannot exceed 50 characters"), Required]
        [Index("IX_Perk_Perk", IsUnique = true)]
        [Display(Name = "Perk Name")]
        public string PerkName { get; set; }

        [Display(Name = "Base Description")]               
        public string BaseDescription { get; set; }

        public decimal  Base { get; set; }

        [Display(Name = "Worth Description")]
        public string WorthDescription { get; set; }

        public decimal Worth { get; set; }

        public Decimal CostConversionMultiplier { get; set; }

        
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
