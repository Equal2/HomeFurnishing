using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class StockAdj : EntityBase
    {
        public StockAdj()
        {
        }

        [Key]
        public int StockAdjId { get; set; }

        [Display(Name = "Stock In No."), Required]
        [ForeignKey("StockIn")]
        public int StockInId { get; set; }
        public virtual Stock StockIn { get; set; }

        [Display(Name = "Stock Out No."), Required]
        [ForeignKey("StockOut")]
        public int StockOutId { get; set; }
        public virtual Stock StockOut { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_Stock_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_Stock_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        public Decimal AdjustedQty { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
