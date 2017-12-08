using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class ChargeGroupPersonCalculation : EntityBase, IHistoryLog
    {
        public ChargeGroupPersonCalculation()
        {
        }

        [Key]
        public int ChargeGroupPersonCalculationId { get; set; }

        [ForeignKey("DocType")]
        [Index("ChargeGroupPersonCalculation_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        [Index("ChargeGroupPersonCalculation_DocID", IsUnique = true, Order = 2)]
        public int? DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("ChargeGroupPersonCalculation_DocID", IsUnique = true, Order = 3)]
        public int? SiteId { get; set; }
        public virtual Site Site { get; set; }


        [Display(Name = "Charge Group Person")]
        [ForeignKey("ChargeGroupPerson")]
        [Index("ChargeGroupPersonCalculation_DocID", IsUnique = true, Order = 4)]
        public int ChargeGroupPersonId { get; set; }
        public virtual ChargeGroupPerson ChargeGroupPerson { get; set; }

        [ForeignKey("Calculation")]
        public int CalculationId { get; set; }
        public virtual Calculation Calculation { get; set; }


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
