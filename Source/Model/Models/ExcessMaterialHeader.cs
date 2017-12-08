using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ExcessMaterialHeader : EntityBase, IHistoryLog
    {
        public ExcessMaterialHeader()
        {
            ExcessMaterialLines = new List<ExcessMaterialLine>();
            SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            DocDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            CreatedDate = DateTime.Now;
        }

        [Key]
        [Display(Name = "ExcessMaterial Header Id")]
        public int ExcessMaterialHeaderId { get; set; }
                        
        [Display(Name = "Doc Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_ExcessMaterialHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
                
        [Display(Name = "Doc Date"),Required ]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime  DocDate { get; set; }
        
        [Display(Name = "Doc No"),Required,MaxLength(20) ]
        [Index("IX_ExcessMaterialHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_ExcessMaterialHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division {get; set;}

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_ExcessMaterialHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "Currency")]
        [ForeignKey("Currency")]
        public int? CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        [Display(Name = "Person")]
        [ForeignKey("Person")]
        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }

        [Display(Name = "Process")]
        [ForeignKey("Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "Godown")]
        [ForeignKey("Godown")]
        public int? GodownId { get; set; }
        public virtual Godown Godown { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
       
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")] 
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<ExcessMaterialLine> ExcessMaterialLines { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
