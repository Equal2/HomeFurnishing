using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModel
{
    public class ProductBuyerSettingsViewModel : EntityBase, IHistoryLog
    {
        public ProductBuyerSettingsViewModel()
        {
        }

        [Key]
        public int ProductBuyerSettingsId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }

        public int ProductId { get; set; }

        public string BuyerSpecificationDisplayName { get; set; }
        public string BuyerSpecification1DisplayName { get; set; }
        public string BuyerSpecification2DisplayName { get; set; }
        public string BuyerSpecification3DisplayName { get; set; }
        public string BuyerSpecification4DisplayName { get; set; }
        public string BuyerSpecification5DisplayName { get; set; }
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
