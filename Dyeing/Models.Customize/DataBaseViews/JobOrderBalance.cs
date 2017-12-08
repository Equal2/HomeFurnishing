using Model;
using Models.BasicSetup.Models;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.DataBaseViews
{
    [Table("ViewJobOrderBalance")]
    public class ViewJobOrderBalance : EntityBase
    {
        [Key]
        public int JobOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string JobOrderNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int JobWorkerId { get; set; }
        public DateTime OrderDate { get; set; }


        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }
        public int? ProductUidId { get; set; }

    }
}
