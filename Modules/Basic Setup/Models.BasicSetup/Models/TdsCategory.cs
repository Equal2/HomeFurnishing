using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class TdsCategory : EntityBase, IHistoryLog
    {
        public TdsCategory()
        {
        }

        [Key]
        public int TdsCategoryId { get; set; }

        [Display(Name="Tds Category")]
        [MaxLength(50), Required]
        [Index("IX_TdsCategory_TdsCategoryName", IsUnique = true)]
        public string TdsCategoryName { get; set; }

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
