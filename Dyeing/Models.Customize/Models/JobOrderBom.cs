using Model;
using Models.BasicSetup.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobOrderBom : EntityBase, IHistoryLog
    {
        [Key]
        public int JobOrderBomId { get; set; }

        [ForeignKey("JobOrderHeader")]
        public int JobOrderHeaderId { get; set; }
        public virtual JobOrderHeader JobOrderHeader { get; set; }

        [ForeignKey("JobOrderLine")]
        public int? JobOrderLineId { get; set; }
        public virtual JobOrderLine JobOrderLine { get; set; }

        [ForeignKey("Product"),Display(Name="Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public decimal Qty { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
