using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class ChargeGroupSettings : EntityBase, IHistoryLog
    {
        public ChargeGroupSettings()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int ChargeGroupSettingsId { get; set; }


        [Display(Name = "Charge Type")]
        [ForeignKey("ChargeType")]
        public int ChargeTypeId { get; set; }
        public virtual ChargeType ChargeType { get; set; }


        [Display(Name = "Charge Group Person")]
        [ForeignKey("ChargeGroupPerson")]
        public int ChargeGroupPersonId { get; set; }
        public virtual ChargeGroupPerson ChargeGroupPerson { get; set; }


        [ForeignKey("Process"), Display(Name = "Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }


        [Display(Name = "Charge Group Product")]
        [ForeignKey("ChargeGroupProduct")]
        public int ChargeGroupProductId { get; set; }
        public virtual ChargeGroupProduct ChargeGroupProduct { get; set; }

        public Decimal ChargePer { get; set; }

        [ForeignKey("ChargeLedgerAccount")]
        public int? ChargeLedgerAccountId { get; set; }
        public virtual LedgerAccount ChargeLedgerAccount { get; set; }


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
