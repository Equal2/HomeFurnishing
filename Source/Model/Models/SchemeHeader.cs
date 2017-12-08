using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SchemeHeader : EntityBase, IHistoryLog
    {
        [Key]
        public int SchemeId { get; set; }

        [Display(Name = "Scheme Name")]
        [MaxLength(100), Required]
        [Index("IX_Scheme_SchemeName", IsUnique = true)]
        public string SchemeName { get; set; }        
        
        [Display(Name = "Process")]
        [ForeignKey("Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "Order From Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime OrderFromDate { get; set; }

        [Display(Name = "Order To Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime OrderToDate { get; set; }

        [MaxLength(100), Required]
        public string ApplicableOn { get; set; }

        public string ApplicableValues { get; set; }

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
