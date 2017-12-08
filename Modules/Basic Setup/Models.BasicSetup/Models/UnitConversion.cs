using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class UnitConversion : EntityBase, IHistoryLog
    {
        [Key]
        public int UnitConversionId { get; set; }

        [ForeignKey("Product")]
        [Display(Name = "Product")]
        [Index("IX_UnitConversion_UniqueKey", IsUnique = true, Order = 1)]
        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }       
        

        [ForeignKey("UnitConversionFor")]
        [Display(Name="Unit Conversion For")]
        [Index("IX_UnitConversion_UniqueKey", IsUnique = true, Order = 2)]
        public byte UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }

        [Display(Name = "From Qty")]        
        public decimal FromQty { get; set; }

        [ForeignKey("FromUnit")]
        [Display(Name = "From Unit")]
        [Index("IX_UnitConversion_UniqueKey", IsUnique = true, Order=3)]
        public string FromUnitId { get; set; }
        public virtual Unit FromUnit { get; set; }

        [Display(Name = "To Qty")]
        public decimal ToQty { get; set; }

        [ForeignKey("ToUnit")]
        [Display(Name = "To Unit")]
        [Index("IX_UnitConversion_UniqueKey", IsUnique = true, Order = 4)]
        public string ToUnitId { get; set; }
        public virtual Unit ToUnit { get; set; }

        [NotMapped]
        [Display(Name = "Multiplier")]
        public decimal Multiplier
        {
            get
            {
                return ToQty/FromQty;
            }
        }

        [Display(Name = "Description"),MaxLength(100)]
        public string Description { get; set; }

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
