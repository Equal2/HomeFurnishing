using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseIndentHeader : EntityBase, IHistoryLog
    {
        public PurchaseIndentHeader()
        {
            PurchaseIndentLines = new List<PurchaseIndentLine>();
        }

        [Key]
        [Display(Name = "Purchase Indent Id")]
        public int PurchaseIndentHeaderId { get; set; }
                        
        [Display(Name = "Indent Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_PurchaseIndentHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
                
        [Display(Name = "Indent Date"),Required ]        
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime  DocDate { get; set; }
        
        [Display(Name = "Indent No"),Required,MaxLength(20) ]
        [Index("IX_PurchaseIndentHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_PurchaseIndentHeader_DocID", IsUnique = true, Order = 3)]       
        public int DivisionId { get; set; }
        public virtual  Division  Division {get; set;}

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_PurchaseIndentHeader_DocID", IsUnique = true, Order = 4)]       
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }


        [ForeignKey("Reason")]
        public int ? ReasonId { get; set; }
        public virtual Reason  Reason { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status") ]
        public int Status { get; set; }
        
        [ForeignKey("MaterialPlanHeader")]
        public int? MaterialPlanHeaderId { get; set; }
        public virtual MaterialPlanHeader MaterialPlanHeader { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        public ICollection<PurchaseIndentLine> PurchaseIndentLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
