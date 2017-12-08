using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class CostCenterStatusExtended : EntityBase
    {
        [Key]
        [ForeignKey("CostCenter")]
        [Display(Name = "CostCenter")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }

        public Decimal? MaterialIssueQty { get; set; }
        public DateTime? MaterialIssueDate { get; set; }
        public Decimal? MaterialReturnQty { get; set; }
        public DateTime? MaterialReturnDate { get; set; }

        public int? RequisitionProductCount { get; set; }
        public int? MaterialIssueProductCount { get; set; }
        public int? MaterialReturnProductCount { get; set; }

        public Decimal? BOMQty { get; set; }
        public Decimal? BOMCancelQty { get; set; }
        public Decimal? ConsumeQty { get; set; }

        public Decimal? RateSettlementQty { get; set; }
        public DateTime? RateSettlementDate { get; set; }
        public Decimal? RateSettlementAmount { get; set; }        

        public Decimal? TransferQty { get; set; }
        public DateTime? TransferDate { get; set; }
        public Decimal? TransferAmount { get; set; }

        public Decimal? ConsumptionAdjustmentQty { get; set; }
        public DateTime? ConsumptionAdjustmentDate { get; set; }

        public Decimal? OrderQty { get; set; }
        public Decimal? OrderDealQty { get; set; }
        public Decimal? OrderCancelQty { get; set; }
        public Decimal? OrderCancelDealQty { get; set; }
        public Decimal? ReceiveQty { get; set; }
        public Decimal? ReceiveDealQty { get; set; }
        public Decimal? InvoiceQty { get; set; }
        public Decimal? InvoiceDealQty { get; set; }
      
        public Decimal? PendingToInvoiceAmount { get; set; }
        public Decimal? InvoiceAmount { get; set; }
        public Decimal? ReceiveIncentiveAmount { get; set; }
        public Decimal? ReceivePenaltyAmount { get; set; }
        public Decimal? TimeIncentiveAmount { get; set; }
        public Decimal? TimePenaltyAmount { get; set; }
        public Decimal? FragmentationPenaltyAmount { get; set; }
        public Decimal? SchemeIncentiveAmount { get; set; }
        public Decimal? DebitAmount { get; set; }
        public Decimal? CreditAmount { get; set; }
        public Decimal? PaymentAmount { get; set; }
        public Decimal? TDSAmount { get; set; }
        public int? ProductId { get; set; }

        //public Decimal? MaterialBalance { get; set; }
        public Decimal? ReturnQty { get; set; }
        public Decimal? ReturnDealQty { get; set; }
        public Decimal? PaymentCancelAmount { get; set; }
        public decimal? Rate { get; set; }
        public Decimal? WeavingReceipt { get; set; }
        public DateTime? AmountTransferDate { get; set; }
        public Decimal? ReturnConsumeQty { get; set; }
        

    }
}
