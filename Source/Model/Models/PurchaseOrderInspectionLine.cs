using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseOrderInspectionLine : EntityBase, IHistoryLog
    {

        [Key]
        public int PurchaseOrderInspectionLineId { get; set; }

        [Display(Name = "Inspection"), Required]
        [ForeignKey("PurchaseOrderInspectionHeader")]
        public int PurchaseOrderInspectionHeaderId { get; set; }
        public virtual PurchaseOrderInspectionHeader PurchaseOrderInspectionHeader { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }


        [Display(Name = "Inspection Request")]
        [ForeignKey("PurchaseOrderInspectionRequestLine")]
        public int ? PurchaseOrderInspectionRequestLineId { get; set; }
        public virtual PurchaseOrderInspectionRequestLine PurchaseOrderInspectionRequestLine { get; set; }


        [Display(Name = "Purchase Order"), Required]
        [ForeignKey("PurchaseOrderLine")]
        public int PurchaseOrderLineId { get; set; }
        public virtual PurchaseOrderLine PurchaseOrderLine { get; set; }


        [Display(Name = "Inspected Qty")]
        public decimal InspectedQty { get; set; }

        [Display(Name = "Pass Qty"), Required]
        public decimal Qty { get; set; }
        
        
        [Range(0,100)]
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
        public int? Sr { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }


    }
}
