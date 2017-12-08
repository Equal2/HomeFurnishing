using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class MaterialPlanCancelHeader : EntityBase, IHistoryLog
    {
        [Key]
        public int MaterialPlanCancelHeaderId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Plan Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name="Plan Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Plan No"), MaxLength(20)]
        public string DocNo { get; set; }

        [ForeignKey("Buyer"), Display(Name = "Buyer")]
        public int ? BuyerId { get; set; }
        public virtual Buyer Buyer { get; set; }

        [ForeignKey("Division"),Display(Name="Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"),Display(Name="Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }        

        [ForeignKey("Godown"), Display(Name = "Godown")]
        public int ? GodownId { get; set; }
        public virtual Godown Godown { get; set; }
        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        public string CreatedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }
       
        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
