using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductBuyerSettings : EntityBase, IHistoryLog
    {
        public ProductBuyerSettings()
        {
        }

        [Key]
        public int ProductBuyerSettingsId { get; set; }
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [Display(Name = "Buyer Specification Display Name"), MaxLength(50)]
        public string BuyerSpecificationDisplayName { get; set; }

        [Display(Name = "Buyer Specification1 Display Name"), MaxLength(50)]
        public string BuyerSpecification1DisplayName { get; set; }

        [Display(Name = "Buyer Specification2 Display Name"), MaxLength(50)]
        public string BuyerSpecification2DisplayName { get; set; }

        [Display(Name = "Buyer Specification3 Display Name"), MaxLength(50)]
        public string BuyerSpecification3DisplayName { get; set; }

        [Display(Name = "Buyer Specification4 Display Name"), MaxLength(50)]
        public string BuyerSpecification4DisplayName { get; set; }

        [Display(Name = "Buyer Specification5 Display Name"), MaxLength(50)]
        public string BuyerSpecification5DisplayName { get; set; }

        [Display(Name = "Buyer Specification6 Display Name"), MaxLength(50)]
        public string BuyerSpecification6DisplayName { get; set; }

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
