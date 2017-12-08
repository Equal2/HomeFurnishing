using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductUidHeader : EntityBase, IHistoryLog
    {

        [Key]
        public int ProductUidHeaderId { get; set; }

        [Display(Name = "Product"), Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }




        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }


        [Display(Name = "Lot No"), MaxLength(50)]
        public string LotNo { get; set; }

        //Update Fields
        public int? GenDocId { get; set; }        

        public string GenDocNo { get; set; }

        [ForeignKey("GenDocType"), Display(Name = "Document Type")]
        public int? GenDocTypeId { get; set; }
        public virtual DocumentType GenDocType { get; set; }

        public DateTime? GenDocDate { get; set; }

        [ForeignKey("GenPerson"), Display(Name = "Gen Person")]
        public int? GenPersonId { get; set; }
        public virtual Person GenPerson { get; set; }

        [Display(Name = "Remark")]
        public string GenRemark { get; set; }

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


