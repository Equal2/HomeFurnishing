using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class ChargeType : EntityBase, IHistoryLog
    {
        public ChargeType()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int ChargeTypeId { get; set; }

        [Display (Name="Name")]
        [MaxLength(50), Required]
        [Index("IX_Charges_ChargesName", IsUnique = true)]
        public string ChargeTypeName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }
        public bool isCommentNeeded { get; set; }
        public bool isPersonBased { get; set; }
        public bool isProductBased { get; set; }
        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }
        
        [MaxLength(20)]
        public string Category { get; set; }

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
