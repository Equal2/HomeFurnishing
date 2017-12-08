using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class LedgerAccount : EntityBase, IHistoryLog
    {
        public LedgerAccount()
        {
        }
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int LedgerAccountId { get; set; }

        [Display(Name="Account Name")]
        [MaxLength(100), Required]
        [Index("IX_LedgerAccount_LedgerAccountName", IsUnique = true, Order = 1)]
        public string LedgerAccountName { get; set; }

        [Display(Name = "Suffix")]
        [MaxLength(100), Required]
        [Index("IX_LedgerAccount_LedgerAccountName", IsUnique = true, Order = 2)]
        public string LedgerAccountSuffix { get; set; }

        [ForeignKey("Person"), Display(Name = "Person Name")]
        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }

        [ForeignKey("LedgerAccountGroup"), Display(Name = "Account Group"),Required]
        public int LedgerAccountGroupId { get; set; }
        public virtual LedgerAccountGroup LedgerAccountGroup { get; set; }

        [ForeignKey("Product"), Display(Name = "Product Name")]
        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; } 

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
