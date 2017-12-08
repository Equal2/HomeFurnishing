using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.Customize.ViewModels
{
    public class ProdOrderHeaderViewModel
    {
        public int ProdOrderHeaderId { get; set; }

        [Display(Name = "Plan Type")]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Plan Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Plan No"), MaxLength(20)]
        public string DocNo { get; set; }

        [Display(Name = "Division")]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site")]
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DueDate { get; set; }

        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int? MaterialPlanHeaderId { get; set; }
        public string MaterialPlanDocNo { get; set; }
        public int? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LockReason { get; set; }

    }

    public class ProdOrderLineViewModel
    {
        public int ProdOrderLineId { get; set; }
        public int ProdOrderHeaderId { get; set; }
        public string ProdOrderDocNo { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }

        public string Specification { get; set; }
        public int? MaterialPlanLineId { get; set; }
        public string MaterialPlanName { get; set; }
        public decimal PlanBalanceQty { get; set; }
        public string MaterialPlanHeaderDocNo { get; set; }

        public decimal Qty { get; set; }

        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string DealUnitId { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public string LockReason { get; set; }
    }
    [Serializable]
    public class ProdOrderHeaderListViewModel
    {
        public int ProdOrderHeaderId { get; set; }
        public int ProdOrderLineId { get; set; }
        public string DocNo { get; set; }
        public decimal Qty { get; set; }
        public string ProductName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
    }
    public class ProdOrderBalanceViewModel
    {
        [Key]
        public int ProdOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int DocTypeId { get; set; }
        public int ProdOrderHeaderId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; } 
        public string ProdOrderNo { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime ProdOrderDate { get; set; }
        public int ? Dimension1Id { get; set; }
        public int ? Dimension2Id { get; set; }
        public int ?  ProcessId { get; set; }
        public int ProductGroupId { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string ProcessName { get; set; }
        public string UnitName { get; set; }
        public bool IsBomExist { get; set; }
        public string DocTypeName { get; set; }
    }

    public class ProdOrderLineFilterViewModel
    {
        public int ProdOrderHeaderId { get; set; }

        [Display(Name = "Material Plan No")]
        public string MaterialPlanHeaderId { get; set; }

        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }

    }
    public class ProdOrderMasterDetailModel
    {
        public List<ProdOrderLineViewModel> ProdOrderLineViewModel { get; set; }
    }



}
