using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ServiceTaxCategory : EntityBase, IHistoryLog
    {
        public ServiceTaxCategory()
        {
        }

        [Key]
        public int ServiceTaxCategoryId { get; set; }

        [Display(Name="ServiceTax Category")]
        [MaxLength(50), Required]
        [Index("IX_ServiceTaxCategory_ServiceTaxCategoryName", IsUnique = true)]
        public string ServiceTaxCategoryName { get; set; }

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
