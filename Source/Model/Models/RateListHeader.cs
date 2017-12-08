using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class RateListHeader : EntityBase, IHistoryLog
    {

        [Key]        
        public int RateListHeaderId { get; set; }
                                        
        [Display(Name = "Effective Date"),Required ]        
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Index("IX_RateListHeader_DocID", IsUnique = true, Order = 1)]
        public DateTime  EffectiveDate { get; set; }


        [Display(Name = "Process"), Required]
        [ForeignKey("Process")]
        [Index("IX_RateListHeader_DocID", IsUnique = true, Order = 2)]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }


        [Display(Name = "Name"),Required,MaxLength(50) ]
        [Index("IX_RateListHeader_Name", IsUnique = true, Order = 1)]
        public string RateListName { get; set; }


        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_RateListHeader_DocID", IsUnique = true, Order = 3)]
        [Index("IX_RateListHeader_Name", IsUnique = true, Order = 2)]
        public int DivisionId { get; set; }
        public virtual  Division  Division {get; set;}


        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_RateListHeader_DocID", IsUnique = true, Order = 4)]
        [Index("IX_RateListHeader_Name", IsUnique = true, Order = 3)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }


        /// <summary>
        /// It is a list. (Qty, Deal Qty, Weight)
        /// </summary>
        [MaxLength(20),Required]
        public string CalculateWeightageOn { get; set; }


        [Index("IX_RateListHeader_DocID", IsUnique = true, Order = 5)]
        public Decimal WeightageGreaterOrEqual { get; set; }

        [ForeignKey("DealUnit"), Display(Name = "Deal Unit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }


        [Display(Name = "Close Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ? CloseDate { get; set; }


        [Display(Name = "Description")]
        public string Description { get; set; }


        [Display(Name = "Minimum Rate")]        
        public decimal MinRate { get; set; }


        [Display(Name = "Maximum Rate")]
        public decimal MaxRate { get; set; }       


        [Display(Name = "Status")]
        public int Status { get; set; }                

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }


        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
