using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleQuotationHeaderDetail : EntityBase
    {
        [Key]

        [ForeignKey("SaleQuotationHeader")]
        [Display(Name = "SaleQuotationHeader")]
        public int SaleQuotationHeaderId { get; set; }
        public virtual SaleQuotationHeader SaleQuotationHeader { get; set; }

        public int Priority { get; set; }

        [ForeignKey("ShipMethod"),Display(Name="Ship Method"),Required]
        public int ? ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }

        public bool? IsDoorDelivery { get; set; }

        [ForeignKey("DeliveryTerms"),Display(Name="Delivery Terms"),Required]
        public int ? DeliveryTermsId { get; set; }
        public virtual DeliveryTerms DeliveryTerms { get; set; }
        public int CreditDays { get; set; }
        
        [ForeignKey("Financier")]
        public int? FinancierId { get; set; }
        public virtual Person Financier { get; set; }

        [ForeignKey("SalesExecutive")]
        public int? SalesExecutiveId { get; set; }
        public virtual Person SalesExecutive { get; set; }

        [ForeignKey("Agent")]
        public int? AgentId { get; set; }
        public virtual Person Agent { get; set; }
        public Decimal? PayTermAdvancePer { get; set; }
        public Decimal? PayTermOnDeliveryPer { get; set; }
        public Decimal? PayTermOnDueDatePer { get; set; }
        public Decimal? PayTermCashPer { get; set; }
        public Decimal? PayTermBankPer { get; set; }
        public string PayTermDescription { get; set; }
    }
}

