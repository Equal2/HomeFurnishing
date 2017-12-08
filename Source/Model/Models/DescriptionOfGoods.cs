using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class DescriptionOfGoods : EntityBase, IHistoryLog
    {
        public DescriptionOfGoods()
        {
        }

        [Key]
        public int DescriptionOfGoodsId { get; set; }

        [Display(Name="Description Of Goods")]
        [MaxLength(50), Required]
        [Index("IX_DescriptionOfGoods_DescriptionOfGoodsName", IsUnique = true)]
        public string DescriptionOfGoodsName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

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
