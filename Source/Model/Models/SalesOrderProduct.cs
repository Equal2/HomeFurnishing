using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SalesOrderProduct: EntityBase,IHistoryLog
    {
        public SalesOrderProduct()
        {
            //OrderDetails = new List<OrderDetail>();
        }
        [Key]
        public int SalesOrderProductId { get; set; }
       
        public string ProductSKU { get; set; }
        public string BuyerSKU { get; set; }
        public string BuyerUPC { get; set; }

        public string ProductDesign { get; set; }
        public int PCs { get; set; }
        public decimal Rate { get; set; }

        public decimal AreaPerPC { get; set; }

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
