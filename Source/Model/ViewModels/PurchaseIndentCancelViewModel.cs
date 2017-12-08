using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class PurchaseIndentCancelHeaderViewModel
    {
        public int PurchaseIndentCancelHeaderId { get; set; }

        [Display(Name = "Cancel Type"), Required]
        public int DocTypeId { get; set; }
        public string DocTypeName{ get; set; }

        [DataType(DataType.Date)]        
        [Display(Name = "Cancel Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Cancel No"), Required, MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters")]        
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]                
        public int DivisionId { get; set; }
        public string DivisionName{ get; set; }

        [Display(Name = "Site"), Required]              
        public int SiteId { get; set; }
        public string SiteName{ get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }
        public PurchaseIndentSettingsViewModel PurchaseIndentSettings { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FirstName { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public string LockReason { get; set; }

    }

    public class PurchaseIndentCancelLineViewModel
    {
        public int PurchaseIndentCancelLineId { get; set; }

        [Display(Name = "Production Order Cancel"), Required]
        public int PurchaseIndentCancelHeaderId { get; set; }
        public virtual PurchaseIndentCancelHeader PurchaseIndentCancelHeader { get; set; }

        [Display(Name = "Production Order"), Required]
        public int PurchaseIndentLineId { get; set; }
        public virtual PurchaseIndentLine PurchaseIndentLine { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }
        public int? ProductId { get; set; }
        [Display(Name = "Product")]
        public string ProductName { get; set; }
        [Display(Name = "Prod Order No.")]
        public string DocNo { get; set; }
        [Display(Name = "Cancel No")]
        public string CancelNo { get; set; }
        public decimal BalanceQty { get; set; }
        public string UnitId { get; set; }
        public string MaterialPlanDocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public PurchaseIndentSettingsViewModel PurchIndentSettings { get; set; }
        public int unitDecimalPlaces { get; set; }
        public string LockReason { get; set; }
    }

    public class PurchaseIndentCancelFilterViewModel
    {
        public int PurchaseIndentCancelHeaderId { get; set; }        
        [Display(Name = "Prod Order No")]
        public string PurchaseIndentId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
    }
    public class PurchaseIndentCancelMasterDetailModel
    {
        public int PurchaseIndentCancelHeaderId { get; set; }     
        public PurchaseIndentCancelHeader PurchaseIndentCancelHeader { get; set; }
        public List<PurchaseIndentCancelLine> PurchaseIndentCancelLines { get; set; }
        public List<PurchaseIndentCancelLineViewModel> PurchaseIndentCancelViewModels { get; set; }
    }
    public class PurchaseIndentLineBalance
    {
        public int PurchaseIndentLineId { get; set; }
        public string PurchaseIndentDocNo { get; set; }
        public string MaterialPlanDocNo { get; set; }

    }
}
