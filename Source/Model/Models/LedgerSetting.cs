using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class LedgerSetting : EntityBase,IHistoryLog
    {
        [Key]
        public int LedgerSettingId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }
        public bool? isVisibleLineCostCenter { get; set; }
        public bool? isMandatoryLineCostCenter { get; set; }
        public bool? isVisibleHeaderCostCenter { get; set; }
        public bool? isMandatoryHeaderCostCenter { get; set; }
        public bool? isVisibleChqNo { get; set; }        
        public bool? isMandatoryChqNo { get; set; }
        public bool? isVisibleRefNo { get; set; }
        public bool? isMandatoryRefNo { get; set; }
        public bool? isVisibleProcess { get; set; }
        public bool? isMandatoryProcess { get; set; }
        public bool? isVisibleGodown { get; set; }
        public bool? isMandatoryGodown { get; set; }
        public bool? isVisibleProductUid { get; set; }
        public bool? isVisibleDrCr { get; set; }
        public bool? isVisibleLineDrCr { get; set; }

        public bool? isVisibleAdjustmentType { get; set; }
        public bool? isVisiblePaymentFor { get; set; }
        public bool? isVisiblePartyDocNo { get; set; }
        public bool? isVisiblePartyDocDate { get; set; }
        public bool? isVisibleLedgerAdj { get; set; }


        public bool? isVisibleReferenceDocId { get; set; }
        public bool? isVisibleReferenceDocTypeId { get; set; }
        public string filterReferenceDocTypes { get; set; }
        public string filterLedgerAccountGroupHeaders { get; set; }
        public string filterExcludeLedgerAccountGroupHeaders { get; set; }
        public string filterPersonProcessHeaders { get; set; }
        public string filterPersonProcessLines { get; set; }
        public string filterLedgerAccountGroupLines { get; set; }
        public string filterExcludeLedgerAccountGroupLines { get; set; }
        public string filterDocTypeCostCenter { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }

        public bool IsAutoDocNo { get; set; }
        public string SqlProcReferenceNo { get; set; }

        [ForeignKey("Process"), Display(Name = "Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [MaxLength(100)]
        public string SqlProcDocumentPrint { get; set; }

        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterSubmit { get; set; }

        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterApprove { get; set; }

        [MaxLength(50,ErrorMessage="{0} Can not exceed {1} characters")]
        public string BaseValueText { get; set; }

        [MaxLength(50, ErrorMessage = "{0} Can not exceed {1} characters")]
        public string BaseRateText { get; set; }

        [MaxLength(50)]
        public string PartyDocNoCaption { get; set; }
        [MaxLength(50)]
        public string PartyDocDateCaption { get; set; }


        [ForeignKey("WizardMenu")]
        [Display(Name = "WizardMenu")]
        public int? WizardMenuId { get; set; }
        public virtual Menu WizardMenu { get; set; }



        [ForeignKey("CancelDocType"), Display(Name = "Cancel Type")]
        public int? CancelDocTypeId { get; set; }
        public virtual DocumentType CancelDocType { get; set; }



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
