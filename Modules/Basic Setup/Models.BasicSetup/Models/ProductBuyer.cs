using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class ProductBuyer : EntityBase, IHistoryLog
    {
     
        [Key]
        public int ProductBuyerId { get; set; }

        [ForeignKey("Product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }        
        public virtual Product Product { get; set; }

        [ForeignKey("Dimension1")]
        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }


        [ForeignKey("Dimension2")]
        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }





        [ForeignKey("Buyer")]
        [Display(Name = "Buyer")]
        public int BuyerId { get; set; }        
        public virtual Person Buyer { get; set; }

        [MaxLength(50)]
        public string BuyerSku { get; set; }

        [MaxLength(50)]
        public string BuyerProductCode { get; set; }

        [MaxLength(20)]
        public string BuyerUpcCode { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification1 { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification2 { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification3 { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification4 { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification5 { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification6 { get; set; }

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
