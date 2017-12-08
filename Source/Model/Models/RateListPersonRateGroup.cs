using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class RateListPersonRateGroup : EntityBase, IHistoryLog
    {
        [Key]
        public int RateListPersonRateGroupId { get; set; }
        
        [Display(Name = "RateListHeader")]
        [ForeignKey("RateListHeader")]
        public int RateListHeaderId { get; set; }
        public virtual RateListHeader RateListHeader { get; set; }

        [Display(Name = "PersonRateGroup")]
        [ForeignKey("PersonRateGroup")]
        public int PersonRateGroupId { get; set; }
        public virtual PersonRateGroup PersonRateGroup { get; set; }

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
