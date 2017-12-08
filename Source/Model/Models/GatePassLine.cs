using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class GatePassLine : EntityBase, IHistoryLog
    {
        public GatePassLine()
        {
        }

        [Key]        
        public int GatePassLineId { get; set; }

        [Display(Name = "Purchase Indent")]
        [ForeignKey("GatePassHeader")]
        public int GatePassHeaderId { get; set; }
        public virtual GatePassHeader  GatePassHeader { get; set; }

        [Display(Name = "Product"), Required]        
        [MaxLength(255)]
        public string Product { get; set; }


        [MaxLength(255)]
        public string Specification { get; set; }
        
        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [ForeignKey("Unit"), Display(Name = "Unit")]
        public string UnitId { get; set; }
        public virtual Unit Unit { get; set; }

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
