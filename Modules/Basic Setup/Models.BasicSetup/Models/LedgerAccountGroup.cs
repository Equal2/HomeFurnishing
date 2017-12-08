using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class LedgerAccountGroup : EntityBase, IHistoryLog
    {
        public LedgerAccountGroup()
        {
        }

        [Key]
        public int LedgerAccountGroupId { get; set; }

        [Display(Name="Account Group Name")]
        [MaxLength(50,ErrorMessage="Maximum characters 50"), Required]
        [Index("IX_LedgerAccountGroup_LedgerAccountGroupName", IsUnique = true)]
        public string LedgerAccountGroupName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }
        [MaxLength(10,ErrorMessage="Maximum 10 characters")]
        public string LedgerAccountType { get; set; }
        [MaxLength(10,ErrorMessage="Maximum 10 characters")]
        public string LedgerAccountNature{ get; set; }

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
