using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class Dimension1Types : EntityBase, IHistoryLog
    {
        public Dimension1Types()
        {
        }

        [Key]
        public int Dimension1TypeId { get; set; }

        [Display(Name="Dimension1 Type")]
        [MaxLength(50, ErrorMessage = "Dimension1Type Name cannot exceed 50 characters"), Required]
        [Index("IX_Dimension1Type_Dimension1TypeName", IsUnique = true)]
        public string Dimension1TypeName { get; set; }

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
