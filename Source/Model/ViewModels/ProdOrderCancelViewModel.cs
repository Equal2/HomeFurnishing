using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class ProdOrderCancelHeaderViewModel
    {
        public int ProdOrderCancelHeaderId { get; set; }

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
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

    }

    public class ProdOrderCancelLineViewModel
    {
        public int ProdOrderCancelLineId { get; set; }

        [Display(Name = "Production Order Cancel"), Required]
        public int ProdOrderCancelHeaderId { get; set; }
        public virtual ProdOrderCancelHeader ProdOrderCancelHeader { get; set; }

        [Display(Name = "Production Order"), Required]
        public int ProdOrderLineId { get; set; }
        public virtual ProdOrderLine ProdOrderLine { get; set; }        

        [Display(Name = "Qty"), Required]
        public decimal Qty { get; set; }
        public int? ProductId { get; set; }
        [Display(Name = "Product")]
        public string ProductName { get; set; }
        [Display(Name = "Prod Order No.")]
        public string ProdOrderNo { get; set; }
        [Display(Name = "Cancel No")]
        public string CancelNo { get; set; }
        public decimal BalanceQty { get; set; }
        public string UnitId { get; set; }
        public string MaterialPlanDocNo { get; set; }
        public int DivisionId { get; set; }
        public string Specification { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public string UnitName { get; set; }
        public ProdOrderSettingsViewModel ProdOrderSettings { get; set; }
        public string LockReason { get; set; }
        public int ProdOrderHeaderId { get; set; }
        public int ? ProductGroupId { get; set; }
        public DateTime OrderDocDate { get; set; }
    }

    public class ProdOrderCancelFilterViewModel
    {
        public int ProdOrderCancelHeaderId { get; set; }        
        [Display(Name = "Prod Order No")]
        public string ProdOrderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public DateTime ? UpToDate { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
    }
    public class ProdOrderCancelMasterDetailModel
    {
        public int ProdOrderCancelHeaderId { get; set; }     
        public ProdOrderCancelHeader ProdOrderCancelHeader { get; set; }
        public List<ProdOrderCancelLine> ProdOrderCancelLines { get; set; }
        public List<ProdOrderCancelLineViewModel> ProdOrderCancelViewModels { get; set; }
    }
    public class ProdOrderLineBalance
    {
        public int ProdOrderLineId { get; set; }
        public int ProductId { get; set; }
        public string ProdOrderDocNo { get; set; }
        public string MaterialPlanDocNo { get; set; }
        public string ProductName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public Decimal? BalanceQty { get; set; }

    }
}
