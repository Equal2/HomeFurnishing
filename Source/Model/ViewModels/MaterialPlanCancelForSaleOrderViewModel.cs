using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModel
{
    [Serializable]
    public class MaterialPlanCancelForSaleOrderViewModel
    {
        public int MaterialPlanCancelForLineId { get; set; }
        public int MaterialPlanCancelHeaderId { get; set; }
        public string MaterialPlanCancelHeaderDocNo{ get; set; }
        public decimal BalanceQtyForPlan { get; set; }
        public decimal Qty { get; set; }
        public string SaleOrderDocNo { get; set; }
        public int SaleOrderLineId { get; set; }
        public string ProdOrderDocNo { get; set; }
        public int ProdOrderLineId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime ? DueDate { get; set; }        
        public string DeliveryUnitId { get; set; }
        public decimal DeliveryQty { get; set; }        
        public string Remark { get; set; }
        public int ? Dimension1Id { get; set; }
        public int ? Dimension2Id { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public int ? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public bool BomDetailExists { get; set; }
        public string Specification { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public DateTime SaleOrderDocDate { get; set; }
    }

    public class MaterialPlanCancelFilterViewModel
    {
        public int DocTypeId { get; set; }
        public int MaterialPlanCancelHeaderId { get; set; }
        //public int SupplierId { get; set; }
        //[Display(Name = "Sale Order No")]
        public string MaterialPlanHeaderId{ get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }

        [Display(Name = "Dimension1")]
        public string Dimension1Id { get; set; }

        [Display(Name = "Dimension2")]
        public string Dimension2Id { get; set; }
        
        [Display(Name = "Dimension3")]
        public string Dimension3Id { get; set; }

        [Display(Name = "Dimension4")]
        public string Dimension4Id { get; set; }
        public string ProcessId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public MaterialPlanSettingsViewModel MaterialPlanSettings { get; set; }

    }
    public class MaterialPlanCancelLineListViewModel
    {
        public MaterialPlanSettings MaterialPlanSettings { get; set; }
        public List<MaterialPlanCancelLineViewModel> MaterialPlanCancelLineViewModel { get; set; }
    }
    [Serializable]
    public class MaterialPlanCancelForProcedureViewModel
    {
        public int ProductId { get; set; }
        public int ProdOrderLineId { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public int? ProcessId { get; set; }
        public decimal Qty { get; set; }
    }

    public class MaterialPlanCancelSummaryViewModel
    {
        public List<MaterialPlanCancelForProcedureViewModel> prodorderlinefromprocedure { get; set; }
        public List<MaterialPlanCancelLineViewModel> PlanLine { get; set; }
        public MaterialPlanSettings MaterialPlanSettings { get; set; }
    }

}
