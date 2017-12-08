using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Godown : EntityBase, IHistoryLog
    {
        public Godown()
        {
            //DocumentTypes = new List<DocumentType>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int GodownId { get; set; }

        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "Godown Name cannot exceed 50 characters"), Required]
        [Index("IX_Godown_GodownName", IsUnique = true)]
        public string GodownName { get; set; }

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

        [ForeignKey("Gate"), Display(Name = "Gate")]
        public int? GateId { get; set; }
        public virtual Gate Gate { get; set; }

    }
}
