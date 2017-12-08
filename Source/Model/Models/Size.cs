using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model.Models
{
    
    public class Size : EntityBase, IHistoryLog
    {
        public Size()
        {
        }

        [Key]
        public int SizeId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Doc Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Size Name")]
        [MaxLength(50, ErrorMessage = "Size Name cannot exceed 50 characters"), Required]
        [Index("IX_Size_SizeName", IsUnique=true) ]
        public string SizeName { get; set; }

        [ForeignKey("ProductShape")]
        [Display(Name = "Product Shape")]
        public int? ProductShapeId { get; set; }
        public virtual ProductShape ProductShape { get; set; }

        [ForeignKey("Unit"), Display(Name = "Unit")]
        public string UnitId { get; set; }
        public virtual Unit Unit { get; set; }
        

        public decimal Length { get; set; }

        public decimal LengthFraction { get; set; }

        public decimal Width { get; set; }

        public decimal WidthFraction { get; set; }

        public decimal Height { get; set; }

        public decimal HeightFraction { get; set; }

        public Decimal Area { get; set; }

        public decimal Perimeter { get; set; }

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
