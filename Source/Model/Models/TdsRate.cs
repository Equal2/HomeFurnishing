using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class TdsRate : EntityBase, IHistoryLog
    {
        public TdsRate()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int TdsRateId { get; set; }

        [ForeignKey("TdsCategory"), Display(Name = "Tds Category")]
        public int TdsCategoryId { get; set; }
        public virtual TdsCategory TdsCategory { get; set; }

        [ForeignKey("TdsGroup"), Display(Name = "Tds Group")]
        public int TdsGroupId { get; set; }
        public virtual TdsGroup TdsGroup { get; set; }

        public Decimal Percentage { get; set; }

        [Display(Name = "LedgerAccount")]
        [ForeignKey("LedgerAccount")]
        public int? LedgerAccountId { get; set; }
        public virtual LedgerAccount LedgerAccount { get; set; }


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
