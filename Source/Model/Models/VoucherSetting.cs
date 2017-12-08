using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class VoucherSettings : EntityBase, IHistoryLog
    {

        [Key]
        public int VoucherSettingsId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }      

        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        public bool? isVisibleCostCenter { get; set; }
        public bool? isMandatoryCostCenter { get; set; }

        public string filterHeaderLedgerAccountGroups { get; set; }
        public string filterHeaderLedgerAccounts { get; set; }

        public string filterLineLedgerAccountGroups { get; set; }
        public string filterLineLedgerAccounts { get; set; }





        [MaxLength(100)]
        public string SqlProcDocumentPrint { get; set; }
        public string filterContraDocTypes { get; set; }



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
