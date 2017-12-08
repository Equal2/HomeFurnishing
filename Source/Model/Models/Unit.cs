using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Unit : EntityBase, IHistoryLog
    {
        public Unit()
        {
            Products = new List<Product>();
        }

        [Key]
        [MaxLength(3, ErrorMessage = "UnitId cannot exceed 3 characters")]
        [Display (Name="Unit Id"),Required]
        public string UnitId { get; set; }

        [MaxLength(20, ErrorMessage = "Unit Name cannot exceed 20 characters"), Required]
        [Index("IX_Unit_UnitName", IsUnique = true)]
        [Display(Name = "Unit Name")]
        public string UnitName { get; set; }

        [MaxLength(1, ErrorMessage = "Symbol cannot exceed 1 characters")]
        public string Symbol { get; set; }

        [MaxLength(20, ErrorMessage = "Fraction Name cannot exceed 20 characters")]
        public string FractionName { get; set; }

        public int? FractionUnits { get; set; }

        [MaxLength(1, ErrorMessage = "FractionSymbol cannot exceed 1 characters")]
        public string FractionSymbol { get; set; }
        
        
        [Display(Name = "Decimal Places")]
        public byte DecimalPlaces { get; set; }

        [MaxLength(3)]
        public string DimensionUnitId { get; set; }

        [MaxLength(50)]
        public string GSTUnit { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }
        
        public ICollection<Product> Products { get; set; }
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
