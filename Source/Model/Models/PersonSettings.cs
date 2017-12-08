using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PersonSettings : EntityBase, IHistoryLog
    {

        [Key]
        public int PersonSettingsId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }


        public bool? isVisibleAddress { get; set; }
        public bool? isVisibleCity { get; set; }
        public bool? isVisibleZipCode { get; set; }
        public bool? isVisiblePhone { get; set; }
        public bool? isVisibleMobile { get; set; }
        public bool? isVisibleEMail { get; set; }
        public bool? isVisibleGstNo { get; set; }
        public bool? isVisibleCstNo { get; set; }
        public bool? isVisibleTinNo { get; set; }
        public bool? isVisiblePanNo { get; set; }
        public bool? isVisibleAadharNo { get; set; }
        public bool? isVisibleSalesTaxGroup { get; set; }
        public bool? isVisibleGuarantor { get; set; }
        public bool? isVisibleParent { get; set; }
        public bool? isVisibleTdsCategory { get; set; }
        public bool? isVisibleTdsGroup { get; set; }
        public bool? isVisibleCreditDays { get; set; }
        public bool? isVisibleCreditLimit { get; set; }
        public bool? isVisibleWorkInDivision { get; set; }
        public bool? isVisibleWorkInBranch { get; set; }
        public bool? isVisibleTags { get; set; }
        public bool? isVisibleIsSisterConcern { get; set; }
        public bool? isVisibleContactPersonDetail { get; set; }
        public bool? isVisibleBankAccountDetail { get; set; }
        public bool? isVisiblePersonProcessDetail { get; set; }
        public bool? isVisiblePersonAddressDetail { get; set; }
        public bool? isVisiblePersonOpeningDetail { get; set; }
        public bool? isVisibleLedgerAccountGroup { get; set; }


        public bool? isMandatoryAddress { get; set; }
        public bool? isMandatoryCity { get; set; }
        public bool? isMandatoryZipCode { get; set; }
        public bool? isMandatoryMobile { get; set; }
        public bool? isMandatoryEmail { get; set; }
        public bool? isMandatoryPanNo { get; set; }
        public bool? isMandatoryGuarantor { get; set; }
        public bool? isMandatoryTdsCategory { get; set; }
        public bool? isMandatoryTdsGroup { get; set; }


        public bool? isMandatorySalesTaxGroup { get; set; }
        public bool? isMandatoryGstNo { get; set; }
        public bool? isMandatoryCstNo { get; set; }
        public bool? isMandatoryTinNo { get; set; }
        public bool? isMandatoryAadharNo { get; set; }
        public bool? isMandatoryCreditDays { get; set; }
        public bool? isMandatoryCreditLimit { get; set; }




        [ForeignKey("LedgerAccountGroup"), Display(Name = "Account Group"), Required]
        public int LedgerAccountGroupId { get; set; }
        public virtual LedgerAccountGroup LedgerAccountGroup { get; set; }

        [ForeignKey("DefaultProcess"), Display(Name = "Default Process")]
        public int? DefaultProcessId { get; set; }
        public virtual Process DefaultProcess { get; set; }


        [MaxLength(100)]
        public string SqlProcPersonCode { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }


    }
}
