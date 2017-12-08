using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobOrderInspectionLine : EntityBase, IHistoryLog
    {

        [Key]
        public int JobOrderInspectionLineId { get; set; }

        [Display(Name = "Inspection"), Required]
        [ForeignKey("JobOrderInspectionHeader")]
        public int JobOrderInspectionHeaderId { get; set; }
        public virtual JobOrderInspectionHeader JobOrderInspectionHeader { get; set; }

        public int? Sr { get; set; }


        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }


        [Display(Name = "Inspection Request")]
        [ForeignKey("JobOrderInspectionRequestLine")]
        public int ? JobOrderInspectionRequestLineId { get; set; }
        public virtual JobOrderInspectionRequestLine JobOrderInspectionRequestLine { get; set; }


        [Display(Name = "Job Order"), Required]
        [ForeignKey("JobOrderLine")]
        public int JobOrderLineId { get; set; }
        public virtual JobOrderLine JobOrderLine { get; set; }


        [Display(Name = "Inspected Qty")]
        public decimal InspectedQty { get; set; }

        [Display(Name = "Pass Qty"), Required]
        public decimal Qty { get; set; }
        
        
        //[Range(0,100)]
        [Display(Name = "Marks")]
        public decimal Marks { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }


        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }


        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocLineId { get; set; }


        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }


    }
}
