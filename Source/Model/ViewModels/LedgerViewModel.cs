using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class LedgerViewModel 
    {
        [Key]
        public int LedgerId { get; set; }

        public int? DocHeaderId { get; set; }

        [Display(Name = "Doc Type"), Required]
        [ForeignKey("DocType")]
        [Index("IX_Ledger_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Doc Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocDate { get; set; }

        [Display(Name = "Doc No"), Required, MaxLength(20)]
        [Index("IX_Ledger_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_Ledger_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_Ledger_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }


        [Display(Name = "HeaderLedgerAccount"), Required]
        [ForeignKey("HeaderLedgerAccount")]
        public int? HeaderLedgerAccountId { get; set; }
        public virtual LedgerAccount HeaderLedgerAccount { get; set; }


        [Display(Name = "LedgerAccount"), Required]
        [ForeignKey("LedgerAccount")]
        public int LedgerAccountId { get; set; }
        public virtual LedgerAccount LedgerAccount { get; set; }

        [Display(Name = "Contra Ledger Account")]
        [ForeignKey("ContraLedgerAccount")]
        public int? ContraLedgerAccountId { get; set; }
        public virtual LedgerAccount ContraLedgerAccount { get; set; }

        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }

        [Display(Name = "AmtDr")]
        public Decimal AmtDr { get; set; }

        [Display(Name = "AmtCr")]
        public Decimal AmtCr { get; set; }

        [MaxLength(250)]
        public string HeaderNarration { get; set; }

        [MaxLength(250)]
        public string Narration { get; set; }

        public string ContraText { get; set; }

        public int CreditDays { get; set; }

        public string Remark { get; set; }

        public int Status { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }

    public class LedgersViewModel
    {
        public int LedgerId { get; set; }

        [Display(Name = "Ledger Header"), Required]        
        public int LedgerHeaderId { get; set; }
        public string LedgerHeaderDocNo { get; set; }

        public int LedgerLineId { get; set; }

        [Display(Name = "LedgerAccount"), Required,Range(1,int.MaxValue,ErrorMessage="The Ledger Account field is required.")]
        
        public int LedgerAccountId { get; set; }
        public string LedgerAccountName{ get; set; }

        [Display(Name = "Contra Ledger Account")]        
        public int? ContraLedgerAccountId { get; set; }
        public string ContraLedgerAccountName{ get; set; }

        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public string CostCenterName{ get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCr { get; set; }
        public decimal AmountDr { get; set; }
        public string DrCr { get; set; }

        [MaxLength(250)]
        public string Narration { get; set; }

        public int? ReferenceId { get; set; }
        public string ReferenceDocNo { get; set; }

        public int? ReferenceDocId { get; set; }
        public int? ReferenceDocTypeId { get; set; }

        public string ContraText { get; set; }
        [Display(Name = "Chq No"), MaxLength(10)]
        public string ChqNo { get; set; }

        [Display(Name = "Chq Date")]
        public DateTime ? DueDate { get; set; }
        public int DocumentCategoryId { get; set; }
        public string Remark { get; set; }
        public LedgerSettingViewModel LedgerSetting { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        public int? ProductUidId { get; set; }

        public string ProductUidName { get; set; }

        public decimal BaseValue { get; set; }
        public decimal BaseRate { get; set; }
        public string LockReason { get; set; }
    }


    public class PaymentCancelWizardViewModel
    {

        public string DocNo { get; set; }
        public DateTime ? DocDate { get; set; }
        public string SDocDate { get; set; }
        public string LedgerAcCr { get; set; }
        public string LedgerAcDr { get; set; }
        public string ReferenceDocNo { get; set; }
        public string ChqNo { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public int LedgerLineId { get; set; }
        public int LedgerHeaderId { get; set; }
        public string CostCenterName { get; set; }
        public string DocTypeName { get; set; }
        public int ? LedgerAcCrId { get; set; }
        public int ? LedgerAcDrId { get; set; }

    }

    public class PaymentCancelWizardFilterViewModel
    {
        public int DocTypeId { get; set; }
        public DateTime ? FromDate { get; set; }
        public DateTime ? ToDate { get; set; }
        public int ? ProcessId { get; set; }
        public string LedgerHeaderId { get; set; }
        public string LedgerAcCr { get; set; }
        public string LedgerAcDr { get; set; }
        public string ChqNo { get; set; }
        public decimal ? Amount { get; set; }
        public string CostCenter { get; set; }
    }


    public class LedgerToAdjustViewModel
    {
        public int LedgerId { get; set; }
        public Decimal Amount { get; set; }
        public string DrCr { get; set; }
        public List<PendingLedgerViewModel> LedgerViewModel { get; set; }
        public LedgerSettingViewModel LedgerSetting { get; set; }
    }

    public class PendingLedgerViewModel
    {
        public int? LedgerAdjId { get; set; }
        public int LedgerId { get; set; }
        public int LedgerAccountId { get; set; }
        public string LedgerAccountName { get; set; }
        public string LedgerHeaderDocNo { get; set; }
        public DateTime LedgerHeaderDocDate { get; set; }
        public string PartyDocNo { get; set; }
        public DateTime PartyDocDate { get; set; }
        public Decimal BillAmount { get; set; }
        public Decimal BalanceAmount { get; set; }
        public Decimal? AdjustedAmount { get; set; }
        public bool IsSelected { get; set; }
    }

    public class LedgerToAdjustViewModel_Single
    {
        public int LedgerLineId { get; set; }
        public int LedgerHeaderId { get; set; }
        public int LedgerAccountId { get; set; }
        public int LedgerId { get; set; }
        public Decimal Amount { get; set; }
        public Decimal BalanceAmount { get; set; }
        public string DrCr { get; set; }
        public int LedgerId_Adjusted { get; set; }
        public string LedgerHeaderDocNo_Adjusted { get; set; }
        public DateTime LedgerHeaderDocDate_Adjusted { get; set; }
        public string PartyDocNo_Adjusted { get; set; }
        public DateTime? PartyDocDate_Adjusted { get; set; }
        public Decimal BillAmount_Adjusted { get; set; }
        public Decimal BalanceAmount_Adjusted { get; set; }
        public Decimal? AdjustedAmount { get; set; }
        public LedgerSettingViewModel LedgerSetting { get; set; }
        public List<PendingLedgerViewModel> LedgerViewModel { get; set; }
    }
}
