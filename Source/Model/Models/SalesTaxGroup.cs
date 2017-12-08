using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SalesTaxGroup : EntityBase, IHistoryLog
    {
        public SalesTaxGroup()
        {
        }

        [Key]
        public int SalesTaxGroupId { get; set; }

        [ForeignKey("SalesTaxGroupProduct"), Display(Name = "Sales Tax Group Product")]
        public int SalesTaxGroupProductId { get; set; }
        public virtual SalesTaxGroupProduct SalesTaxGroupProduct { get; set; }

        [ForeignKey("SalesTaxGroupParty"), Display(Name = "Sales Tax Group Party")]
        public int SalesTaxGroupPartyId { get; set; }
        public virtual SalesTaxGroupParty SalesTaxGroupParty { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        
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
