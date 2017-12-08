using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Customize.Models
{
    public class JobOrderPerk : EntityBase, IHistoryLog
    {
        [Key]
        public int JobOrderPerkId { get; set; }

        [ForeignKey("JobOrderHeader")]
        public int JobOrderHeaderId { get; set; }
        public virtual JobOrderHeader JobOrderHeader { get; set; }


        [ForeignKey("Perk"),Display(Name="Perk")]
        public int PerkId { get; set; }
        public virtual Perk Perk { get; set; }

        public decimal Base { get; set; }

        public decimal Worth { get; set; }

        public decimal CostConversionMultiplier { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
