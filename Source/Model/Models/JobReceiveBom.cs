using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobReceiveBom : EntityBase, IHistoryLog
    {
        [Key]
        public int JobReceiveBomId { get; set; }

        [ForeignKey("JobReceiveHeader")]
        public int JobReceiveHeaderId { get; set; }
        public virtual JobReceiveHeader JobReceiveHeader { get; set; }

        [ForeignKey("JobReceiveLine")]
        public int ? JobReceiveLineId { get; set; }
        public virtual JobReceiveLine JobReceiveLine { get; set; }

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


        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }



        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }

        [MaxLength(50)]
        public string LotNo { get; set; }

        [Display(Name = "StockProcess")]
        [ForeignKey("StockProcess")]
        public int? StockProcessId { get; set; }
        public virtual StockProcess StockProcess { get; set; }


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
