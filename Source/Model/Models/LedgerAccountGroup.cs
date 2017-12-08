using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class LedgerAccountGroup : EntityBase, IHistoryLog
    {
        public LedgerAccountGroup()
        {
        }
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int LedgerAccountGroupId { get; set; }

        [Display(Name="Account Group Name")]
        [MaxLength(50,ErrorMessage="Maximum characters 50"), Required]
        [Index("IX_LedgerAccountGroup_LedgerAccountGroupName", IsUnique = true)]
        public string LedgerAccountGroupName { get; set; }

        public Byte? Weightage { get; set; }
        

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

        public int ? ParentLedgerAccountGroupId { get; set; }
        //public string BSNature { get; set; }
        //public Int64 ? BSSr { get; set; }
        //public string TradingNature { get; set; }
        //public Int64? TradingSr { get; set; }
        //public string PLNature { get; set; }
        //public Int64? PLSr { get; set; }

        public string OMSId { get; set; }
    }
}
