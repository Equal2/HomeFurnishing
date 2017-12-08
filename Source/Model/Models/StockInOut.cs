using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class StockInOut : EntityBase
    {
        [Key]
        public int StockInOutId { get; set; }

        [Display(Name = "Stock Out")]
        [ForeignKey("StockOut")]
        public int StockOutId { get; set; }
        public virtual Stock StockOut { get; set; }

        [Display(Name = "Stock In")]
        [ForeignKey("StockIn")]
        public int StockInId { get; set; }
        public virtual Stock StockIn { get; set; }

        public Decimal Qty_Adj { get; set; }

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
