using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleQuotationHeader : EntityBase, IHistoryLog
    {
        public SaleQuotationHeader()
        {
        }
        [Key]
        public int SaleQuotationHeaderId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Quotation Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name="Quotation Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Quotation No"), MaxLength(20)]
        public string DocNo { get; set; }

        [ForeignKey("Division"),Display(Name="Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"),Display(Name="Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("ParentSaleQuotationHeader"), Display(Name = "SaleQuotationHeader")]
        public int? ParentSaleQuotationHeaderId { get; set; }
        public virtual SaleQuotationHeader ParentSaleQuotationHeader { get; set; }

        [DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime DueDate { get; set; }


        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ExpiryDate { get; set; }

        [ForeignKey("Process"), Display(Name = "Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }


        [ForeignKey("SaleToBuyer"),Display(Name="Sale To Buyer")]
        public int SaleToBuyerId { get; set; }
        public virtual Person SaleToBuyer { get; set; }


        [ForeignKey("Currency"),Display(Name="Currency")]
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }





        public string TermsAndConditions { get; set; }








        public int Status { get; set; }

        [ForeignKey("UnitConversionFor")]
        [Display(Name = "Unit Conversion For")]        
        public byte UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }


        

        
        [ForeignKey("SalesTaxGroupPerson")]
        public int? SalesTaxGroupPersonId { get; set; }
        public virtual ChargeGroupPerson SalesTaxGroupPerson { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }


        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocId { get; set; }





        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}

