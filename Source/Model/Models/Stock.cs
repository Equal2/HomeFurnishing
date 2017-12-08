using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Stock : EntityBase
    {
        public Stock()
        {
        }

        [Key]        
        public int StockId { get; set; }

        [Display(Name = "Stock Header"), Required]
        [ForeignKey("StockHeader")]
        public int StockHeaderId { get; set; }
        public virtual StockHeader StockHeader { get; set; }

        public int? DocLineId { get; set; }

        [Display(Name = "Doc Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocDate { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }


        [Display(Name = "Product"), Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "Process")]
        [ForeignKey("Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "Godown")]
        [ForeignKey("Godown")]
        public int GodownId { get; set; }
        public virtual Godown Godown { get; set; }

        [Display(Name = "Lot No."), MaxLength(50)]
        public string LotNo { get; set; }

        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }

        [Display(Name = "Qty_Iss")]
        public Decimal Qty_Iss { get; set; }

        [Display(Name = "Qty_Rec")]
        public Decimal Qty_Rec { get; set; }

        [Display(Name = "Rate")]
        public Decimal? Rate { get; set; }

        [Display(Name = "Expiry Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Specification")]
        public string Specification { get; set; }

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


        [Display(Name = "Doc Qty_Iss")]
        public Decimal? DocQty_Iss { get; set; }

        [Display(Name = "Doc Qty_Rec")]
        public Decimal? DocQty_Rec { get; set; }

        [Display(Name = "Weight_Iss")]
        public Decimal? Weight_Iss { get; set; }

        [Display(Name = "Weight_Rec")]
        public Decimal? Weight_Rec { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocId { get; set; }

        [Display(Name = "StockStatus"), MaxLength(20)]
        public string StockStatus { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

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
