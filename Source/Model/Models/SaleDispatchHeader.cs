using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleDispatchHeader : EntityBase, IHistoryLog
    {
        public SaleDispatchHeader()
        {
            SaleDispatchLines = new List<SaleDispatchLine>();
        }

        [Key]
        public int SaleDispatchHeaderId { get; set; }
                        
        [Display(Name = "Dispatch Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_SaleDispatchHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]                
        [Display(Name = "Sale Dispatch Date"),Required ]
        public DateTime DocDate { get; set; }
        
        [Display(Name = "Sale Dispatch No"),Required,MaxLength(20) ]
        [Index("IX_SaleDispatchHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_SaleDispatchHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division { get; set; }



        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_SaleDispatchHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }


        [ForeignKey("SaleToBuyer"), Display(Name = "Sale To Buyer")]
        public int SaleToBuyerId { get; set; }
        public virtual Person SaleToBuyer { get; set; }


        [Display(Name = "Ship To Party Address"),  MaxLength(250)]
        public string ShipToPartyAddress { get; set; }


        [Display(Name = "Gate Entry No"), MaxLength(20)]
        public string GateEntryNo { get; set; }

        [Display(Name = "Form No"), MaxLength(20)]
        public string FormNo { get; set; }


        [Display(Name = "Transporter"), MaxLength(100)]
        public string Transporter { get; set; }

        [Display(Name = "Delivery Terms")]
        [ForeignKey("DeliveryTerms")]
        public int DeliveryTermsId { get; set; }
        public virtual DeliveryTerms DeliveryTerms { get; set; }

        [Display(Name = "Ship Method")]
        [ForeignKey("ShipMethod")]
        public int ? ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public int Status { get; set; }


        [ForeignKey("GatePassHeader")]
        [Display(Name = "Gatepass No.")]
        public int? GatePassHeaderId { get; set; }
        public virtual GatePassHeader GatePassHeader { get; set; }

        [ForeignKey("StockHeader")]
        [Display(Name = "Stock Header No.")]
        public int? StockHeaderId { get; set; }
        public virtual StockHeader StockHeader { get; set; }

        [ForeignKey("PackingHeader")]
        public int ? PackingHeaderId { get; set; }
        public virtual PackingHeader PackingHeader { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<SaleDispatchLine> SaleDispatchLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
