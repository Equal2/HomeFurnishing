using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class BusinessEntity : EntityBase
    {
        public BusinessEntity()
        {
        }

        [Key]
        [ForeignKey("Person"), Display(Name = "Person")]
        public int PersonID { get; set; }
        public virtual Person Person { get; set; }

        [ForeignKey("Parent"), Display(Name = "Parent")]
        public int? ParentId { get; set; }
        public virtual Person Parent { get; set; }

        [ForeignKey("TdsCategory"), Display(Name = "Tds Category")]
        public int? TdsCategoryId { get; set; }
        public virtual TdsCategory TdsCategory { get; set; }

        [ForeignKey("TdsGroup"), Display(Name = "Tds Group")]
        public int? TdsGroupId { get; set; }
        public virtual TdsGroup TdsGroup { get; set; }

        [ForeignKey("Guarantor"), Display(Name = "Guarantor")]
        public int? GuarantorId { get; set; }
        public virtual Person Guarantor { get; set; }

        [ForeignKey("SalesTaxGroupParty")]
        public int? SalesTaxGroupPartyId { get; set; }
        public virtual ChargeGroupPerson SalesTaxGroupParty { get; set; }

        [Display(Name = "Is Sister Concern ?")]
        public Boolean IsSisterConcern { get; set; }

        [ForeignKey("PersonRateGroup")]
        [Display(Name = "Person Rate Group")]
        public int? PersonRateGroupId { get; set; }
        public virtual PersonRateGroup PersonRateGroup { get; set; }

        [ForeignKey("ServiceTaxCategory"), Display(Name = "Service Tax Category")]
        public int? ServiceTaxCategoryId { get; set; }
        public virtual ServiceTaxCategory ServiceTaxCategory { get; set; }

        public int? CreaditDays { get; set; }

        public Decimal? CreaditLimit { get; set; }

        [MaxLength(100)]
        public string DivisionIds { get; set; }

        [MaxLength(100)]
        public string SiteIds { get; set; }

        public virtual Buyer Buyer { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
