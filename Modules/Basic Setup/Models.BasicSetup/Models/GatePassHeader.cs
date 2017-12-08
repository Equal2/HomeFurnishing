using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class GatePassHeader : EntityBase, IHistoryLog
    {
        public GatePassHeader()
        {
            
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

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
