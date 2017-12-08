using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class Dimension1 : EntityBase, IHistoryLog
    {
        public Dimension1()
        {
        }

        [Key]
        public int Dimension1Id { get; set; }

        [ForeignKey("DocType")]
        public int? DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display (Name="Name")]
        [MaxLength(50), Required]
        [Index("IX_Dimension1_Dimension1Name", IsUnique = true)]
        public string Dimension1Name { get; set; }

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
