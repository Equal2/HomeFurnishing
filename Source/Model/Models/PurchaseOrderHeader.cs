using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseOrderHeader : EntityBase, IHistoryLog
    {
        public PurchaseOrderHeader()
        {
            PurchaseOrderLines = new List<PurchaseOrderLine>();
        }

        [Key]
        [Display(Name = "Purchase Order Id")]
        public int PurchaseOrderHeaderId { get; set; }
                        
        [Display(Name = "Order Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_PurchaseOrderHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
                
        [Display(Name = "Order Date"),Required ]        
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime  DocDate { get; set; }
        
        [Display(Name = "Order No"),Required,MaxLength(20) ]
        [Index("IX_PurchaseOrderHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_PurchaseOrderHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division {get; set;}

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_PurchaseOrderHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Supplier")]
        [Index("IX_PurchaseOrderHeader_DocID", IsUnique = true, Order = 5)]
        [Display(Name = "Supplier Name")]
        public int SupplierId { get; set; }
        public virtual Person Supplier { get; set; }

        [Display(Name = "Due Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ActualDueDate { get; set; }

        [Display(Name = "Ship Method")]
        [ForeignKey("ShipMethod")]
        public int ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }

        [Display(Name = "Delivery Terms")]
        [ForeignKey("DeliveryTerms")]
        public int? DeliveryTermsId { get; set; }
        public virtual DeliveryTerms DeliveryTerms { get; set; }
        public string TermsAndConditions { get; set; }

        [Display(Name = "Ship Address")]
        public string ShipAddress { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [ForeignKey("Currency"), Display(Name = "Currency")]
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        [ForeignKey("SalesTaxGroupPerson")]
        public int? SalesTaxGroupPersonId { get; set; }
        public virtual ChargeGroupPerson SalesTaxGroupPerson { get; set; }

        [Display(Name = "Supplier Ship Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? SupplierShipDate { get; set; }

        [Display(Name = "Supplier Remark")]
        public string SupplierRemark { get; set; }

        [Display(Name = "CreditDays")]
        public int? CreditDays { get; set; }
        [Display(Name = "Progress %")]
        [Range(0, 100)]
        public int? ProgressPer { get; set; }       

        public bool? isUninspected { get; set; }

        public bool CalculateDiscountOnRate { get; set; }


        [ForeignKey("UnitConversionFor")]
        [Display(Name = "Unit Conversion For Type")]
        public byte ? UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        public int Priority { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Approved By")]
        public string ApprovedBy { get; set; }

        [Display(Name = "Approved Date")]
        public DateTime? ApprovedDate { get; set; }


        //public Boolean IsCompleted 
        //{
        //    get 
        //    {
        //        if (PurchaseOrderLines == null)
        //            return false;
        //        else
        //            if (PurchaseOrderLines.Sum(i => i.BalanceQty) == 0)
        //                return true;
        //            else
        //                return false;
        //    }
        //}


        public ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
