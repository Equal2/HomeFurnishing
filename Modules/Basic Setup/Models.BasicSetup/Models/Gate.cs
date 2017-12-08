using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Company.Models
{
    public class Gate : EntityBase, IHistoryLog
    {
        public Gate()
        {
            //DocumentTypes = new List<DocumentType>();
        }

        [Key]
        public int GateId { get; set; }

        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "Gate Name cannot exceed 50 characters"), Required]
        [Index("IX_Gate_GateName", IsUnique = true)]
        public string GateName { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
