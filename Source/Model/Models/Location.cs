using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Location : EntityBase, IHistoryLog
    {
        public Location()
        {
            PurchaseGoodsReceiptHeaders = new List<PurchaseGoodsReceiptHeader>();
        }

        [Key]
        [Display(Name = "Location Id")]
        public int LocationId { get; set; }

        [MaxLength(50), Required]
        [Display(Name = "Location Name")]
        public string LocationName { get; set; }        

        public ICollection<PurchaseGoodsReceiptHeader> PurchaseGoodsReceiptHeaders { get; set; }

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
