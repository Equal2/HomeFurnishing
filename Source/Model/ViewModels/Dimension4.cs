using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class Dimension4ViewModel : EntityBase
    {
        public Dimension4ViewModel()
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

        public ProductTypeSettingsViewModel ProductTypeSettings { get; set; }

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
