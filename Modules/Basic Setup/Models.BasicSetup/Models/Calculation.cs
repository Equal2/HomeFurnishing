using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class Calculation : EntityBase, IHistoryLog
    {
        public Calculation()
        {            
        }

        [Key]
        [Display(Name = "Calculation Id")]
        public int CalculationId { get; set; }

        [MaxLength(50, ErrorMessage = "Calculation Name cannot exceed 50 characters"), Required]
        [Index("IX_Calculation_Calculation", IsUnique = true)]
        [Display(Name = "Calculation Name")]
        public string CalculationName { get; set; }
        
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
