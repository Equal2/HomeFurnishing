using Model.ViewModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class Dimension4 : EntityBase, IHistoryLog
    {
        public Dimension4()
        {
        }

        [Key]
        public int Dimension4Id { get; set; }

        [Display (Name="Name")]
        [MaxLength(50), Required]
        [Index("IX_Dimension4_Dimension4Name", IsUnique = true)]
        public string Dimension4Name { get; set; }

        [ForeignKey("ProductType")]
        [Display(Name = "ProductType")]
        public int? ProductTypeId { get; set; }
        public virtual ProductType ProductType { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [MaxLength(50)]
        public string Description { get; set; }


        [ForeignKey("ReferenceDocType"), Display(Name = "Order Type")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }
        public int? ReferenceDocId { get; set; }

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
