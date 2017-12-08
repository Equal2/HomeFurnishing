using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class BinLocation : EntityBase, IHistoryLog
    {
        [Key]
        [Display(Name = "BinLocation Id")]
        public int BinLocationId { get; set; }

        [ForeignKey("Godown")]
        public int GodownId { get; set; }
        public virtual Godown Godown { get; set; }

        public string BinLocationCode { get; set; }

        [MaxLength(50, ErrorMessage = "BinLocation Name cannot exceed 50 characters"), Required]
        [Index("IX_BinLocation_BinLocation", IsUnique = true)]
        [Display(Name = "BinLocation Name")]
        public string BinLocationName { get; set; }

        
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
