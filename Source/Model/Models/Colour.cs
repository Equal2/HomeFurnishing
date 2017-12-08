using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class Colour : EntityBase, IHistoryLog
    {
        [Key]
        public int ColourId { get; set; }

        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "Colour Name cannot exceed 50 characters"), Required]
        [Index("IX_Colour_ColourName", IsUnique = true)]
        public string ColourName { get; set; }
                     
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
