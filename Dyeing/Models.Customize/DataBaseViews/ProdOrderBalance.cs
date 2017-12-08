using Model;
using Models.BasicSetup.Models;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.DataBaseViews
{
    [Table("ViewProdOrderBalance")]
    public class ViewProdOrderBalance : EntityBase
    {
        [Key]
        public int ProdOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProdOrderHeaderId { get; set; }
        public string ProdOrderNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public DateTime IndentDate { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public int? ReferenceDocLineId { get; set; }
        public int? ReferenceDocTypeId { get; set; }
    }
}
