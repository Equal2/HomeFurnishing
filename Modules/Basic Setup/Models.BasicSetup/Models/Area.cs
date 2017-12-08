using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class Area : EntityBase, IHistoryLog
    {
        [Key]
        public int AreaId { get; set; }


        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }


        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "Area Name cannot exceed 50 characters"), Required]
        [Index("IX_Area_AreaName", IsUnique = true)]
        public string AreaName { get; set; }
                     
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
