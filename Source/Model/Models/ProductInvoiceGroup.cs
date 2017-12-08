using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductInvoiceGroup : EntityBase, IHistoryLog
    {
        public ProductInvoiceGroup()
        {
        }

        [Key]
        public int ProductInvoiceGroupId { get; set; }

        [Display(Name = "Product Invoice Group Name")]
        [MaxLength(100), Required]
        [Index("IX_ProductInvoiceGroup_ProductInvoiceGroupName", IsUnique = true)]
        public string ProductInvoiceGroupName { get; set; }

        [MaxLength(25)]
        public string ItcHsCode { get; set; }

        [MaxLength(50)]
        public string Code { get; set; }
        public Decimal Rate { get; set; }

        public Decimal Knots { get; set; }

        [ForeignKey("Division")]
        [Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        public bool IsSample { get; set; }

        public Decimal Weight { get; set; }

        public bool SeparateWeightInInvoice { get; set; }

        [ForeignKey("DescriptionOfGoods")]
        [Display(Name = "DescriptionOfGoods")]
        public int? DescriptionOfGoodsId { get; set; }
        public virtual DescriptionOfGoods DescriptionOfGoods { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
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
