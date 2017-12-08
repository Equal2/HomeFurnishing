using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SchemeDateDetail : EntityBase, IHistoryLog
    {
        [Key]
        public int SchemeDateDetailId { get; set; }

        [Display(Name = "Scheme")]
        [ForeignKey("Scheme")]
        public int SchemeId { get; set; }
        public virtual SchemeHeader Scheme { get; set; }

        [Display(Name = "Receive From Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ReceiveFromDate { get; set; }

        [Display(Name = "Receive To Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ReceiveToDate { get; set; }

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
