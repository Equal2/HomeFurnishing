using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
   
    public class LeaveType : EntityBase, IHistoryLog
    {
        public LeaveType()
        {
            //DocumentTypes = new List<DocumentType>();
        }

        [Key]
        public int LeaveTypeId { get; set; }

        [Display(Name = "Name")]
        [MaxLength(50, ErrorMessage = "Leave Type Name cannot exceed 50 characters"), Required]
        [Index("IX_LeaveType_LeaveTypeName", IsUnique = true)]
        public string LeaveTypeName { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
