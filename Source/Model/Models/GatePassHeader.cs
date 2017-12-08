using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class GatePassHeader : EntityBase, IHistoryLog
    {
        public GatePassHeader()
        {
            GatePassLines = new List<GatePassLine>();
        }

        [Key]
        [Display(Name = "GatePass Id")]
        public int GatePassHeaderId { get; set; }
                        
        [Display(Name = "Gate Pass Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_GatePassHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
                
        [Display(Name = "Gate Pass Date"),Required ]        
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime  DocDate { get; set; }
        
        [Display(Name = "GatePass No"),Required,MaxLength(20) ]
        [Index("IX_GatePassHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_GatePassHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division {get; set;}

        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Person")]
        [Display(Name = "Person Name")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }

        [ForeignKey("Godown")]
        public int GodownId { get; set; }
        public virtual Godown Godown { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status") ]
        public int Status { get; set; }                

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocId { get; set; }

        public string ReferenceDocNo { get; set; }
        public ICollection<GatePassLine> GatePassLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [ForeignKey("OrderBy"), Display(Name = "Order By")]
        public int? OrderById { get; set; }
        public virtual Employee OrderBy { get; set; }
    }
}
