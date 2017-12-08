using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseOrderHeaderStatus : EntityBase
    {
        [Key]
        [ForeignKey("PurchaseOrderHeader")]
        [Display(Name = "PurchaseOrderHeader")]
        public int? PurchaseOrderHeaderId { get; set; }
        public virtual PurchaseOrderHeader PurchaseOrderHeader { get; set; }

        //public Decimal? MaterialIssueQty { get; set; }

        //public DateTime MaterialIssueDate { get; set; }

        //public Decimal? MaterialReturnQty { get; set; }

        //public DateTime MaterialReturnDate { get; set; }

        public Decimal? BOMQty { get; set; }

        public bool IsTimeIncentiveProcessed { get; set; }

    }
}
