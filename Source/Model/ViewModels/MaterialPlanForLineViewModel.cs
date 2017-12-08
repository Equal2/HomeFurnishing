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
    public class MaterialPlanForSaleOrderViewModel
    {
        public int MaterialPlanForLineId { get; set; }
        public int MaterialPlanHeaderId { get; set; }
        public string MaterialPlanHeaderDocNo{ get; set; }
        public decimal BalanceQtyForPlan { get; set; }
        public decimal Qty { get; set; }
        public string SaleOrderDocNo { get; set; }
        public int SaleOrderLineId { get; set; }
        public string ProdOrderDocNo { get; set; }
        public int ProdOrderLineId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? BuyerId { get; set; }
        public DateTime ? DueDate { get; set; }        
        public string DeliveryUnitId { get; set; }
        public decimal DeliveryQty { get; set; }        
        public string Remark { get; set; }
        public int ? Dimension1Id { get; set; }
        public int ? Dimension2Id { get; set; }
        public int? Dimension3Id { get; set; }
        public int? Dimension4Id { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
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

    public class MaterialPlanForLineFilterViewModel
    {
        public int DocTypeId { get; set; }
        public int MaterialPlanHeaderId { get; set; }
        //public int SupplierId { get; set; }
        //[Display(Name = "Sale Order No")]
        public string SaleOrderHeaderId{ get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }


        [Display(Name = "Dimension1")]
        public string Dimension1Id { get; set; }
        
        [Display(Name = "Dimension2")]
        public string Dimension2Id { get; set; }

        [Display(Name = "Dimension3")]
        public string Dimension3Id { get; set; }
        
        [Display(Name = "Dimension4")]
        public string Dimension4Id { get; set; }


        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public string BuyerId { get; set; }
        public MaterialPlanSettingsViewModel MaterialPlanSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }
    public class MaterialPlanLineForProductionFilterViewModel
    {
        public int DocTypeId { get; set; }
        public DateTime? DocDate { get; set; }
        public int? ProcessId { get; set; }
        public int MaterialPlanHeaderId { get; set; }
        public string ProdOrderHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }
    
    public class MaterialPlanLineListViewModel
    {
        public MaterialPlanSettingsViewModel MaterialPlanSettings { get; set; }
        public List<MaterialPlanForSaleOrderViewModel> MaterialPlanLineViewModel { get; set; }
    }
    [Serializable]
    public class MaterialPlanForProcedureViewModel
    {
        public int ProductId { get; set; }
        public int ProdOrderLineId { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public int? Dimension3Id { get; set; }
        public int? Dimension4Id { get; set; }
        public int? ProcessId { get; set; }
        public decimal Qty { get; set; }
    }

    public class MaterialPlanSummaryViewModel
    {
        public List<MaterialPlanForProcedureViewModel> prodorderlinefromprocedure { get; set; }
        public List<MaterialPlanLineViewModel> PlanLine { get; set; }
        public MaterialPlanSettingsViewModel MaterialPlanSettings { get; set; }
    }

}
