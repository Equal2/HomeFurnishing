using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleDeliveryHeader : EntityBase, IHistoryLog
    {
        public SaleDeliveryHeader()
        {
           
        }

        [Key]
        public int SaleDeliveryHeaderId { get; set; }
                        
        [Display(Name = "Dispatch Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_SaleDeliveryHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]                
        [Display(Name = "Sale Dispatch Date"),Required ]
        public DateTime DocDate { get; set; }
        
        [Display(Name = "Sale Dispatch No"),Required,MaxLength(20) ]
        [Index("IX_SaleDeliveryHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_SaleDeliveryHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division { get; set; }



        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_SaleDeliveryHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }


        [ForeignKey("SaleToBuyer"), Display(Name = "Sale To Buyer")]
        public int SaleToBuyerId { get; set; }
        public virtual Person SaleToBuyer { get; set; }

        [MaxLength(100)]
        public string DeliverToPerson { get; set; }
        [MaxLength(20)]
        public string DeliverToPersonReference { get; set; }


        [Display(Name = "Ship To Party Address"),  MaxLength(250)]
        public string ShipToPartyAddress { get; set; }

        [ForeignKey("GatePassHeader")]
        [Display(Name = "Gatepass No.")]
        public int? GatePassHeaderId { get; set; }
        public virtual GatePassHeader GatePassHeader { get; set; }

        
        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public int Status { get; set; }


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



        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
