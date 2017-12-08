using System;
using System.Collections.Generic;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModel
{
    public class StockHeaderViewModel
    {
        public int StockHeaderId { get; set; }

        public int? DocHeaderId { get; set; }

        [Display(Name = "Doc Type"), Required]        
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Doc Date"), Required]        
        public DateTime DocDate { get; set; }

        [Display(Name = "Doc No"), Required, MaxLength(20)]        
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]              
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Currency")]        
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }

        [Display(Name = "Person"),Required]        
        public int? PersonId { get; set; }
        public string PersonName { get; set; }

        [Display(Name = "Process")]        
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "From Godown")]        
        public int? FromGodownId { get; set; }
        public string FromGodownName { get; set; }
        public int? GatePassHeaderId { get; set; }
        public string GatePassDocNo { get; set; }
        public int GatePassStatus { get; set; }
        public DateTime ? GatePassDocDate { get; set; }

        public string ProductGroupName { get; set; }

        [Display(Name = "Godown")]        
        public int? GodownId { get; set; }
        public string GodownName { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

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

        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public StockHeaderSettingsViewModel StockHeaderSettings{ get; set; }
        public MaterialReceiveSettingsViewModel MaterialReceiveSettings { get; set; }
        public MaterialTransferSettingsViewModel MaterialTransferSettings { get; set; }        
        //public JobConsumptionSettingsViewModel JobConsumptionSettings { get; set; }
        //public RateConversionSettingsViewModel RateConversionSettings { get; set; }

        [Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }

        [Display(Name = "Machine")]
        public int? MachineId { get; set; }
        public string MachineName { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string LockReason { get; set; }

        public decimal? TotalQty { get; set; }
        public int? DecimalPlaces { get; set; }

        public List<DocumentTypeHeaderAttributeViewModel> DocumentTypeHeaderAttributes { get; set; }
    }


    public class StockLineViewModel
    {
        public int StockLineId { get; set; }

        public int? DocTypeId { get; set; }
        public int StockHeaderId { get; set; }
        public string StockHeaderDocNo { get; set; }

        [Display(Name = "ProductUid")]
        public int ? ProductUidId { get; set; }
        public string ProductUidIdName { get; set; }
        public string ProductCode { get; set; }

        [Display(Name = "Product"), Required]        
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "From Process")] 
        public int ? FromProcessId { get; set; }
        public string FromProcessName { get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }
        public Decimal ExcessStockQty { get; set; }
        public Decimal ? RequisitionBalanceQty { get; set; }
        public Decimal? StockProcessBalanceQty { get; set; }

        public Decimal? StockInBalanceQty { get; set; }

        public Decimal? IssueForQty { get; set; }



        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public string ProductGroupName { get; set; }

        [Display(Name = "Request No")]
        public int ? RequisitionLineId { get; set; }
        public string RequisitionHeaderDocNo { get; set; }

        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }

        [Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }

        [Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }


        [MaxLength(50)]
        public string Specification { get; set; }
        public decimal ? Rate { get; set; }

        public decimal? Weight { get; set; }
        public decimal ? Amount { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public byte ? UnitDecimalPlaces { get; set; }        
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public int? RequiredProductId { get; set; }
        public string RequiredProductName { get; set; }
        public StockHeaderSettingsViewModel StockHeaderSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public MaterialReceiveSettingsViewModel MaterialReceiveSettingsViewModel { get; set; }
        public MaterialTransferSettingsViewModel MaterialTransferSettings { get; set; }
        //public JobConsumptionSettingsViewModel JobConsumptionSettings { get; set; }
        //public RateConversionSettingsViewModel RateConversionSettings { get; set; }
        public int? StockProcessBalanceId { get; set; }

        public int? GodownId { get; set; }
        public int? FromGodownId { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }        
        public Decimal BalanceQty { get; set; }

        [Display(Name = "Person")]
        public int? PersonId { get; set; }
        public string PersonName { get; set; }
        public int ProductGroupId { get; set; }
        public int? StockInId { get; set; }
        public string StockInNo { get; set; }
        public bool Issue { get; set; }
        public string LockReason { get; set; }

        public int? ReferenceDocTypeId { get; set; }

        public int? ReferenceDocId { get; set; }

        public int? ReferenceDocLineId { get; set; }
    }



    public class ProductBalanceForProcessViewModel
    {

        public int ? ProcessId { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public string LotNo { get; set; }
        
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }


        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }

        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }


        public string Specification { get; set; }

        public decimal BalanceQty { get; set; }
        public int ProductGroupId { get; set; }

    }

    public class StockLineFilterViewModel
    {
        public int DocTypeId { get; set; }
        public int StockHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Purchase Order No")]
        public int ? CostCenterId { get; set; }
        public int ProcessId { get; set; }

        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public string CostCenterIds { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }

        public decimal Rate { get; set; }
        //public JobConsumptionSettingsViewModel JobConsumptionSettings { get; set; }
        public StockHeaderSettingsViewModel StockHeaderSettings { get; set; }
    }


    public class RequisitionFiltersForIssue
    {
        public int StockHeaderId { get; set; }
        public int PersonId { get; set; }
        public string ProductId { get; set; }
        public string RequisitionHeaderId { get; set; }
        public string ProductGroupId { get; set; }
        public string CostCenterId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }

    public class StockInFiltersForIssue
    {
        public int StockHeaderId { get; set; }
        public string ProductId { get; set; }
        public string StockInHeaderId { get; set; }
        public string ProductGroupId { get; set; }
        public string CostCenterId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }
        public string LotNo { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }

    public  class RequisitionFiltersForExchange
    {
        public int StockHeaderId { get; set; }
        public int PersonId { get; set; }
        public string ProductId { get; set; }
        public string ProductGroupId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }
        public string CostCenterId { get; set; }
        public string RequisitionHeaderId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }

    public class FiltersForProcessTransfer
    {
        public int StockHeaderId { get; set; }
        public int PersonId { get; set; }
        public string ProductId { get; set; }
        public string ProductGroupId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        [Required]
        public int FromCostCenterId { get; set; }
        [Required]
        public int ToCostCenterId { get; set; }        
        public int ProcessId { get; set; }
    }



    public class StockMasterDetailModel
    {
        public List<StockLineViewModel> StockLineViewModel { get; set; }
        //public JobConsumptionSettingsViewModel JobConsumptionSettings { get; set; }
        //public RateConversionSettingsViewModel RateConversionSettings { get; set; }
        public StockHeaderSettingsViewModel StockHeaderSettings { get; set; }
    }




    public class StockExchangeLineViewModel
    {
        public int StockLineId { get; set; }

        public int StockHeaderId { get; set; }
        public string StockHeaderDocNo { get; set; }

        [Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public string ProductUidIdName { get; set; }

        [Display(Name = "Product"), Required]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "From Process")]
        public int? FromProcessId { get; set; }
        public string FromProcessName { get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal QtyRec { get; set; }
        public decimal QtyIss { get; set; }
        public decimal Qty { get; set; }
        public Decimal? RequisitionBalanceQty { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Request No")]
        public int? RequisitionLineId { get; set; }
        public string RequisitionHeaderDocNo { get; set; }

        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }


        [Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }

        [Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }



        [MaxLength(50)]
        public string Specification { get; set; }
        public decimal? Rate { get; set; }

        public decimal? Amount { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public byte? UnitDecimalPlaces { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public int? ToCostCenterId { get; set; }
        public string ToCostCenterName { get; set; }
        public int? RequiredProductId { get; set; }
        public string RequiredProductName { get; set; }
        public StockHeaderSettingsViewModel StockHeaderSettings { get; set; }       
        public int? GodownId { get; set; }
        public int? FromGodownId { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public Decimal BalanceQty { get; set; }

        [Display(Name = "Person")]
        public int? PersonId { get; set; }
        public string PersonName { get; set; }
    }

    public class StockExchangeMasterDetailModel
    {
        public List<StockExchangeLineViewModel> StockLineViewModel { get; set; }        
        public StockHeaderSettingsViewModel StockHeaderSettings { get; set; }
    }

    public class ProductsFilterViewModel
    {
        public List<StockIssueForProductsFilterViewModel> StockIssueForProductsFilterViewModel { get; set; }
       
    }

    public class ProductsFiltersForIssue
    {
        public int StockHeaderId { get; set; }
        public int PersonId { get; set; }
        public string CostCenterId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }
    [Serializable]
    public class StockIssueForProductsFilterViewModel
    {
        public int StockHeaderId { get; set; }
        public int JobOrderLineId { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string JobHeaderDocNo { get; set; }
        public int CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public decimal? Qty { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public Boolean IsBomPosted { get; set; }

    }
}



