using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PersonProductUid : EntityBase, IHistoryLog
    {
        [Key]
        public int PersonProductUidId { get; set; }
        
        public int? GenDocId { get; set; }
        public string GenDocNo { get; set; }

        [ForeignKey("GenDocType"), Display(Name = "Document Type")]
        public int? GenDocTypeId { get; set; }
        public virtual DocumentType GenDocType { get; set; }

        public int? GenLineId { get; set; }

        [Display(Name = "Product Uid Name")]
        [MaxLength(50), Required]
        public string ProductUidName { get; set; }

        [Display(Name = "Product Uid Specification")]
        public string ProductUidSpecification { get; set; }

        [Display(Name = "Sale Order")]
        [ForeignKey("SaleOrderLine")]
        public int? SaleOrderLineId { get; set; }
        public virtual SaleOrderLine SaleOrderLine { get; set; }

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
