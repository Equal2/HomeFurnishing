using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class LedgerLineRefValue : EntityBase
    {

        [Key]        
        public int LedgerLineRefValueId { get; set; }

        [Display(Name = "Ledger Line")]
        [ForeignKey("LedgerLine")]
        public int LedgerLineId { get; set; }
        public virtual LedgerLine  LedgerLine { get; set; }


        [MaxLength(50)]
        public string Head { get; set; }

        [MaxLength(50)]
        public string Value { get; set; }

        public string OMSId { get; set; }
    }
}
