using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class UrgentList : EntityBase, IHistoryLog
    {
        public UrgentList()
        {
        }

        [Key]
        [Display(Name = "Id")]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Decimal? Qty { get; set; }

        public Decimal? UnplannedQty { get; set; }

        public Decimal? PlannedQty { get; set; }

        public Decimal? InProcessQty { get; set; }
        public Decimal? StockQty { get; set; }

        public Decimal? PackedQty { get; set; }

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
