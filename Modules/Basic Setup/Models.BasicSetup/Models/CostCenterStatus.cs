using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class CostCenterStatus : EntityBase
    {
        [Key]
        [ForeignKey("CostCenter")]
        [Display(Name = "CostCenter")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }

        [ForeignKey("Product")]
        [Display(Name = "Product")]
        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }
        public Decimal? AmountDr { get; set; }
        public Decimal? AmountCr { get; set; }
    }
}
