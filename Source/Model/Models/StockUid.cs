using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class StockUid : EntityBase
    {
        [Key]
        public int StockUidId { get; set; }

        public int DocHeaderId { get; set; }
        public int DocLineId { get; set; }

        [Display(Name = "Doc Type"), Required]
        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Doc Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocDate { get; set; }

        [Display(Name = "Division")]
        [ForeignKey("Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [Display(Name = "Site")]
        [ForeignKey("Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "Godown")]
        [ForeignKey("Godown")]
        public int ? GodownId { get; set; }
        public virtual Godown Godown { get; set; }

        [Display(Name = "Process")]
        [ForeignKey("Process")]
        public int ? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "ProductUid")]
        [ForeignKey("ProductUid")]
        public int ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [Display(Name = "Qty_Iss")]
        public int Qty_Iss { get; set; }

        [Display(Name = "Qty_Rec")]
        public int Qty_Rec { get; set; }

        public string Remark { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        
    }
}
