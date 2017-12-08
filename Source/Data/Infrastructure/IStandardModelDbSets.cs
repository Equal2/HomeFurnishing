using Microsoft.AspNet.Identity.EntityFramework;
using Model.DatabaseViews;
using Model.Models;
using Model.Tasks.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Model
{
    public interface IStandardModelDbSets
    {

        //Master Models        
        DbSet<CalculationHeaderLedgerAccount> CalculationHeaderLedgerAccount { get; set; }
        DbSet<CalculationLineLedgerAccount> CalculationLineLedgerAccount { get; set; }
        DbSet<DocumentTypeDivision> DocumentTypeDivision { get; set; }
        DbSet<DocumentTypeSite> DocumentTypeSite { get; set; }
        DbSet<AspNetRole> AspNetRole { get; set; }
        DbSet<AspNetUserRole> AspNetUserRole { get; set; }
        DbSet<Perk> Perk { get; set; }
        DbSet<PerkDocumentType> PerkDocumentType { get; set; }
        DbSet<JobOrderPerk> JobOrderPerk { get; set; }
        DbSet<JobOrderSettings> JobOrderSettings { get; set; }
        DbSet<LedgerSetting> LedgerSetting { get; set; }
        DbSet<PurchaseOrderSetting> PurchaseOrderSetting { get; set; }
        DbSet<SaleInvoiceSetting> SaleInvoiceSetting { get; set; }
        DbSet<PurchaseIndentSetting> PurchaseIndentSetting { get; set; }
        DbSet<StockHeaderSettings> MaterialIssueSettings { get; set; }
        DbSet<MaterialReceiveSettings> MaterialReceiveSettings { get; set; }
        DbSet<MaterialRequestSettings> MaterialRequestSettings { get; set; }
        DbSet<JobReceiveSettings> JobReceiveSettings { get; set; }
        DbSet<JobInvoiceSettings> JobInvoiceSettings { get; set; }
        DbSet<JobConsumptionSettings> JobConsumptionSettings { get; set; }
        DbSet<RateConversionSettings> RateConversionSettings { get; set; }
        DbSet<RequisitionSetting> RequisitionSetting { get; set; }
        DbSet<PurchaseInvoiceHeaderCharge> PurchaseInvoiceHeaderCharge { get; set; }
        DbSet<PurchaseInvoiceLineCharge> PurchaseInvoiceLineCharge { get; set; }
        DbSet<PurchaseInvoiceReturnHeader> PurchaseInvoiceReturnHeader { get; set; }
        DbSet<PurchaseInvoiceReturnHeaderCharge> PurchaseInvoiceReturnHeaderCharge { get; set; }
        DbSet<PurchaseInvoiceReturnLine> PurchaseInvoiceReturnLine { get; set; }
        DbSet<PurchaseInvoiceReturnLineCharge> PurchaseInvoiceReturnLineCharge { get; set; }
        DbSet<SaleInvoiceHeaderCharge> SaleInvoiceHeaderCharge { get; set; }
        DbSet<SaleInvoiceLineCharge> SaleInvoiceLineCharge { get; set; }
        DbSet<PurchaseOrderHeaderCharge> PurchaseOrderHeaderCharges { get; set; }
        DbSet<PurchaseOrderLineCharge> PurchaseOrderLineCharge { get; set; }
        DbSet<Route> Route { get; set; }
        DbSet<RouteLine> RouteLine { get; set; }
        DbSet<MenuModule> MenuModule { get; set; }
        DbSet<RugStencil> RugStencil { get; set; }
        DbSet<MenuSubModule> MenuSubModule { get; set; }
        DbSet<ControllerAction> ControllerAction { get; set; }
        DbSet<MvcController> MvcController { get; set; }
        DbSet<RolesControllerAction> RolesControllerAction { get; set; }
        DbSet<RolesDivision> RolesDivision { get; set; }
        DbSet<RolesMenu> RolesMenu { get; set; }
        DbSet<RolesSite> RolesSite { get; set; }
        DbSet<UserBookMark> UserBookMark { get; set; }
        DbSet<Menu> Menu { get; set; }
        DbSet<Agent> Agent { get; set; }
        DbSet<Manufacturer> Manufacturer { get; set; }
        DbSet<Buyer> Buyer { get; set; }
        DbSet<Supplier> Supplier { get; set; }
        DbSet<City> City { get; set; }
        DbSet<DocumentStatus> DocumentStatus { get; set; }
        DbSet<Company> Company { get; set; }
        DbSet<CostCenter> CostCenter { get; set; }
        DbSet<Country> Country { get; set; }
        DbSet<Currency> Currency { get; set; }
        DbSet<DeliveryTerms> DeliveryTerms { get; set; }
        DbSet<TrialBalanceSetting> TrialBalanceSetting { get; set; }
        DbSet<StockInHandSetting> StockInHandSetting { get; set; }
        DbSet<Dimension1> Dimension1 { get; set; }

        DbSet<Dimension2> Dimension2 { get; set; }

        DbSet<Division> Divisions { get; set; }
        DbSet<Colour> Colour { get; set; }
        DbSet<DocumentCategory> DocumentCategory { get; set; }
        DbSet<DocumentType> DocumentType { get; set; }
        DbSet<Employee> Employee { get; set; }
        DbSet<ProductSiteDetail> ProductSiteDetail { get; set; }
        DbSet<Transporter> Transporter { get; set; }
        DbSet<Courier> Courier { get; set; }
        DbSet<Godown> Godown { get; set; }
        DbSet<Gate> Gate { get; set; }
        DbSet<LeaveType> LeaveType { get; set; }
        DbSet<JobInstruction> JobInstruction { get; set; }
        DbSet<ProductDesignPattern> ProductDesignPattern { get; set; }
        DbSet<ChargeType> ChargeType { get; set; }
        DbSet<JobOrderLineStatus> JobOrderLineStatus { get; set; }
        DbSet<JobReceiveLineStatus> JobReceiveLineStatus { get; set; }
        DbSet<JobOrderHeaderStatus> JobOrderHeaderStatus { get; set; }
        DbSet<ProdOrderLineStatus> ProdOrderLineStatus { get; set; }
        DbSet<ProdOrderHeaderStatus> ProdOrderHeaderStatus { get; set; }
        DbSet<PurchaseOrderLineStatus> PurchaseOrderLineStatus { get; set; }
        DbSet<SaleOrderLineStatus> SaleOrderLineStatus { get; set; }
        DbSet<RequisitionLineStatus> RequisitionLineStatus { get; set; }
        DbSet<PurchaseOrderHeaderStatus> PurchaseOrderHeaderStatus { get; set; }
        DbSet<ChargeGroupProduct> ChargeGroupProduct { get; set; }
        DbSet<ChargeGroupPerson> ChargeGroupPerson { get; set; }
        DbSet<JobWorker> JobWorker { get; set; }
        //DbSet<Machine> Machine { get; set; }
        DbSet<Process> Process { get; set; }
        DbSet<Reason> Reason { get; set; }
        DbSet<ShipMethod> ShipMethod { get; set; }
        DbSet<Site> Site { get; set; }
        DbSet<State> State { get; set; }
        DbSet<Unit> Units { get; set; }
        DbSet<UnitConversion> UnitConversion { get; set; }
        DbSet<UnitConversionFor> UnitConversonFor { get; set; }
        DbSet<PersonRateGroup> PersonRateGroup { get; set; }
        DbSet<ProductRateGroup> ProductRateGroup { get; set; }
        DbSet<Counter> Counter { get; set; }
        DbSet<RateListHistory> RateListHistory { get; set; }
        DbSet<RateListHeader> RateListHeader { get; set; }
        DbSet<RateListPersonRateGroup> RateListPersonRateGroup { get; set; }
        DbSet<RateListProductRateGroup> RateListProductRateGroup { get; set; }
        DbSet<RateList> RateList { get; set; }
        DbSet<RateListLine> RateListLine { get; set; }
        DbSet<RateListLineHistory> RateListLineHistory { get; set; }
        DbSet<Department> Department { get; set; }
        DbSet<Designation> Designation { get; set; }
        DbSet<DrawBackTariffHead> DrawBackTariffHead { get; set; }
        DbSet<SalesTaxGroupProduct> SalesTaxGroupProduct { get; set; }
        DbSet<SalesTaxGroupParty> SalesTaxGroupParty { get; set; }
        DbSet<SalesTaxGroup> SalesTaxGroup { get; set; }
        DbSet<Tag> Tag { get; set; }
        DbSet<DocEmailContent> DocEmailContent { get; set; }
        DbSet<DocNotificationContent> DocNotificationContent { get; set; }
        DbSet<DocSmsContent> DocSmsContent { get; set; }
        

        //Person Models
        DbSet<LedgerAccountGroup> LedgerAccountGroup { get; set; }
        DbSet<LedgerAccount> LedgerAccount { get; set; }
        DbSet<BusinessEntity> BusinessEntity { get; set; }
        DbSet<Person> Persons { get; set; }
        DbSet<PersonContact> PersonContacts { get; set; }
        DbSet<PersonContactType> PersonContactType { get; set; }
        DbSet<PersonAddress> PersonAddress { get; set; }
        DbSet<PersonBankAccount> PersonBankAccount { get; set; }
        DbSet<PersonDocument> PersonDocument { get; set; }
        DbSet<PersonRegistration> PersonRegistration { get; set; }
        DbSet<ReportHeader> ReportHeader { get; set; }
        DbSet<ReportLine> ReportLine { get; set; }

        //Product Models
        DbSet<ProductContentHeader> ProductContentHeader { get; set; }
        DbSet<ProductContentLine> ProductContentLine { get; set; }
        DbSet<Product> Product { get; set; }
        DbSet<FinishedProduct> FinishedProduct { get; set; }
        DbSet<ProductAttributes> ProductAttributes { get; set; }
        DbSet<BomDetail> BomDetail { get; set; }
        DbSet<ProductCategory> ProductCategory { get; set; }
        DbSet<ProductCollection> ProductCollections { get; set; }
        // DbSet<ProductDesign> ProductDesign { get; set; }
        DbSet<ProductGroup> ProductGroups { get; set; }
        DbSet<ProductIncludedAccessories> ProductIncludedAccessories { get; set; }
        DbSet<ProductNature> ProductNature { get; set; }
        DbSet<ProductRelatedAccessories> ProductRelatedAccessories { get; set; }
        DbSet<ProductShape> ProductShape { get; set; }
        DbSet<ProductType> ProductTypes { get; set; }
        DbSet<ProductTypeAttribute> ProductTypeAttribute { get; set; }
        DbSet<ProductUidHeader> ProductUidHeader { get; set; }
        DbSet<ProductUid> ProductUid { get; set; }
        DbSet<ProductUidSiteDetail> ProductUidSiteDetail { get; set; }
        DbSet<ProductCustomGroupHeader> ProductCustomGroupHeader { get; set; }
        DbSet<ProductCustomGroupLine> ProductCustomGroupLine { get; set; }
        DbSet<PersonCustomGroupHeader> PersonCustomGroupHeader { get; set; }
        DbSet<PersonCustomGroupLine> PersonCustomGroupLine { get; set; }
        DbSet<ProductInvoiceGroup> ProductInvoiceGroup { get; set; }
        DbSet<ProductBuyer> ProductBuyer { get; set; }
        DbSet<ProductSizeType> ProductSizeType { get; set; }
        DbSet<ProductSize> ProductSize { get; set; }
        DbSet<ProductTag> ProductTag { get; set; }
        DbSet<ProductStyle> ProductStyle { get; set; }
        DbSet<ProductSupplier> ProductSupplier { get; set; }
        DbSet<ProductQuality> ProductQuality { get; set; }

        DbSet<Size> Size { get; set; }

        //Process Sequence
        DbSet<ProcessSequenceHeader> ProcessSequenceHeader { get; set; }
        DbSet<ProcessSequenceLine> ProcessSequenceLine { get; set; }

        //Inspection
        
        DbSet<InspectionQaAttributes> InspectionQaAttributes { get; set; }


        //User Models
        DbSet<IdentityUser> Users { get; set; }
        DbSet<UserInfo> UserInfo { get; set; }
        DbSet<TempUserStore> TempUserStore { get; set; }//temprory user password store /ToDo: need to remove after Identity 2.0 implimentation 

        //Log Models
        DbSet<ActivityLog> ActivityLog { get; set; }

        //Sales Models
        DbSet<SaleOrderHeader> SaleOrderHeader { get; set; }
        DbSet<SaleOrderLine> SaleOrderLine { get; set; }
        DbSet<SaleDeliveryOrderHeader> SaleDeliveryOrderHeader { get; set; }
        DbSet<SaleDeliveryOrderLine> SaleDeliveryOrderLine { get; set; }
        DbSet<SaleOrderSettings> SaleOrderSettings { get; set; }
        DbSet<SaleOrderCancelHeader> SaleOrderCancelHeader { get; set; }
        DbSet<SaleOrderCancelLine> SaleOrderCancelLine { get; set; }
        DbSet<SaleOrderAmendmentHeader> SaleOrderAmendmentHeader { get; set; }
        DbSet<SaleOrderQtyAmendmentLine> SaleOrderQtyAmendmentLine { get; set; }
        DbSet<SaleOrderRateAmendmentLine> SaleOrderRateAmendmentLine { get; set; }
        DbSet<SaleDispatchHeader> SaleDispatchHeader { get; set; }
        DbSet<SaleDispatchLine> SaleDispatchLine { get; set; }
        DbSet<SaleInvoiceHeader> SaleInvoiceHeader { get; set; }
        DbSet<SaleInvoiceHeaderDetail> SaleInvoiceHeaderDetail { get; set; }
        DbSet<SaleInvoiceLine> SaleInvoiceLine { get; set; }
        DbSet<DispatchWaybillHeader> DispatchWaybillHeader { get; set; }
        DbSet<DispatchWaybillLine> DispatchWaybillLine { get; set; }
        DbSet<CustomDetail> CustomDetail { get; set; }
        DbSet<SaleDispatchReturnHeader> SaleDispatchReturnHeader { get; set; }
        DbSet<SaleDispatchReturnLine> SaleDispatchReturnLine { get; set; }
        DbSet<SaleInvoiceReturnHeader> SaleInvoiceReturnHeader { get; set; }
        DbSet<SaleInvoiceReturnLine> SaleInvoiceReturnLine { get; set; }
        DbSet<ViewSaleDispatchBalance> ViewSaleDispatchBalance { get; set; }
        DbSet<ViewSaleInvoiceBalance> ViewSaleInvoiceBalance { get; set; }
        DbSet<SaleInvoiceReturnHeaderCharge> SaleInvoiceReturnHeaderCharge { get; set; }
        DbSet<SaleInvoiceReturnLineCharge> SaleInvoiceReturnLineCharge { get; set; }

        //Job Models
        DbSet<PersonProcess> PersonProcess { get; set; }
        DbSet<ProductProcess> ProductProcess { get; set; }
        DbSet<JobOrderHeader> JobOrderHeader { get; set; }
        DbSet<JobOrderHeaderCharge> JobOrderHeaderCharges { get; set; }

        DbSet<JobInvoiceHeaderCharge> JobInvoiceHeaderCharges { get; set; }
        DbSet<JobOrderLine> JobOrderLine { get; set; }
        DbSet<JobOrderLineCharge> JobOrderLineCharge { get; set; }
        DbSet<JobInvoiceLineCharge> JobInvoiceLineCharge { get; set; }
        DbSet<JobOrderBom> JobOrderBom { get; set; }
        DbSet<JobOrderCancelBom> JobOrderCancelBom { get; set; }
        DbSet<JobOrderByProduct> JobOrderByProduct { get; set; }
        DbSet<JobOrderJobOrder> JobOrderJobOrder { get; set; }
        DbSet<JobOrderCancelHeader> JobOrderCancelHeader { get; set; }
        DbSet<JobOrderCancelLine> JobOrderCancelLine { get; set; }
        DbSet<JobOrderAmendmentHeader> JobOrderAmendmentHeader { get; set; }
        DbSet<JobOrderQtyAmendmentLine> JobOrderQtyAmendmentLine { get; set; }
        DbSet<JobOrderRateAmendmentLine> JobOrderRateAmendmentLine { get; set; }
        DbSet<JobReceiveHeader> JobReceiveHeader { get; set; }
        DbSet<JobReceiveLine> JobReceiveLine { get; set; }
        DbSet<JobReceiveBom> JobReceiveBom { get; set; }
        DbSet<JobReceiveByProduct> JobReceiveByProduct { get; set; }
        DbSet<JobReturnHeader> JobReturnHeader { get; set; }
        DbSet<JobReturnLine> JobReturnLine { get; set; }

        DbSet<JobInvoiceHeader> JobInvoiceHeader { get; set; }
        DbSet<JobInvoiceLine> JobInvoiceLine { get; set; }

        //Material Plan Models
        DbSet<MaterialPlanHeader> MaterialPlanHeader { get; set; }
        DbSet<MaterialPlanForSaleOrderLine> MaterialPlanForSaleOrderLine { get; set; }
        DbSet<MaterialPlanForProdOrderLine> MaterialPlanForProdOrderLine { get; set; }
        DbSet<MaterialPlanForJobOrderLine> MaterialPlanForJobOrderLine { get; set; }
        DbSet<MaterialPlanForSaleOrder> MaterialPlanForSaleOrder { get; set; }
        DbSet<MaterialPlanForProdOrder> MaterialPlanForProdOrder { get; set; }
        DbSet<MaterialPlanForJobOrder> MaterialPlanForJobOrder { get; set; }
        DbSet<MaterialPlanLine> MaterialPlanLine { get; set; }
        DbSet<MaterialPlanSettings> MaterialPlanSettings { get; set; }
        DbSet<ProductionOrderSettings> ProductionOrderSettings { get; set; }
        DbSet<ProdOrderHeader> ProdOrderHeader { get; set; }
        DbSet<ProdOrderLine> ProdOrderLine { get; set; }
        DbSet<ProdOrderCancelHeader> ProdOrderCancelHeader { get; set; }
        DbSet<ProdOrderCancelLine> ProdOrderCancelLine { get; set; }

        //Purchase Models
        DbSet<RequisitionHeader> RequisitionHeader { get; set; }
        DbSet<RequisitionLine> RequisitionLine { get; set; }

        DbSet<RequisitionCancelHeader> RequisitionCancelHeader { get; set; }
        DbSet<RequisitionCancelLine> RequisitionCancelLine { get; set; }

        DbSet<PurchaseIndentHeader> PurchaseIndentHeader { get; set; }
        DbSet<PurchaseIndentLine> PurchaseIndentLine { get; set; }
        DbSet<PurchaseIndentCancelHeader> PurchaseIndentCancelHeader { get; set; }
        DbSet<PurchaseIndentCancelLine> PurchaseIndentCancelLine { get; set; }
        DbSet<PurchaseQuotationHeader> PurchaseQuotationHeader { get; set; }
        DbSet<PurchaseQuotationLine> PurchaseQuotationLine { get; set; }
        DbSet<PurchaseOrderHeader> PurchaseOrderHeader { get; set; }
        DbSet<PurchaseOrderLine> PurchaseOrderLine { get; set; }
        DbSet<PurchaseOrderCancelHeader> PurchaseOrderCancelHeader { get; set; }
        DbSet<PurchaseOrderCancelLine> PurchaseOrderCancelLine { get; set; }
        DbSet<PurchaseOrderAmendmentHeader> PurchaseOrderAmendmentHeader { get; set; }
        DbSet<PurchaseOrderQtyAmendmentLine> PurchaseOrderQtyAmendmentLine { get; set; }
        DbSet<PurchaseOrderRateAmendmentLine> PurchaseOrderRateAmendmentLine { get; set; }
        DbSet<PurchaseGoodsReceiptHeader> PurchaseGoodsReceiptHeader { get; set; }
        DbSet<PurchaseGoodsReturnHeader> PurchaseGoodsReturnHeader { get; set; }
        DbSet<PurchaseWaybill> PurchaseWaybill { get; set; }
        DbSet<PurchaseGoodsReceiptLine> PurchaseGoodsReceiptLine { get; set; }
        DbSet<PurchaseGoodsReturnLine> PurchaseGoodsReturnLine { get; set; }
        DbSet<PurchaseInvoiceHeader> PurchaseInvoiceHeader { get; set; }
        DbSet<PurchaseInvoiceLine> PurchaseInvoiceLine { get; set; }
        DbSet<ProductDesign> ProductDesigns { get; set; }
        DbSet<PurchaseGoodsReceiptSetting> PurchaseGoodsReceiptSetting { get; set; }
        DbSet<PurchaseInvoiceSetting> PurchaseInvoiceSetting { get; set; }

        //Packing Models
        DbSet<PackingSetting> PackingSettings { get; set; }
        DbSet<PackingHeader> PackingHeader { get; set; }
        DbSet<PackingLine> PackingLine { get; set; }

        //Stock Models
        DbSet<StockHeader> StockHeader { get; set; }
        DbSet<StockLine> StockLine { get; set; }
        DbSet<Stock> Stock { get; set; }
        //DbSet<StockBalance> StockBalance { get; set; }
        DbSet<StockAdj> StockAdj { get; set; }
        DbSet<StockProcess> StockProcess { get; set; }
        //DbSet<StockProcessBalance> StockProcessBalance { get; set; }

        //Ledger Models
        DbSet<LedgerHeader> LedgerHeader { get; set; }
        DbSet<LedgerLine> LedgerLine { get; set; }
        DbSet<LedgerLineRefValue> LedgerLineRefValue { get; set; }
        DbSet<Ledger> Ledger { get; set; }
        DbSet<LedgerAdj> LedgerAdj { get; set; }

        //Scheme Models
        DbSet<SchemeHeader> SchemeHeader { get; set; }
        DbSet<SchemeDateDetail> SchemeDateDetail { get; set; }
        DbSet<SchemeRateDetail> SchemeRateDetail { get; set; }

        //Gate Models
        DbSet<GatePassHeader> GatePassHeader { get; set; }
        DbSet<GatePassLine> GatePassLine { get; set; }
        DbSet<GateIn> GateIn { get; set; }

        //Tds Models
        DbSet<TdsCategory> TdsCategory { get; set; }
        DbSet<TdsGroup> TdsGroup { get; set; }
        DbSet<TdsRate> TdsRate { get; set; }
        DbSet<Calculation> Calculation { get; set; }
        DbSet<CalculationFooter> CalculationFooter { get; set; }
        DbSet<CalculationProduct> CalculationProduct { get; set; }
        DbSet<Charge> Charge { get; set; }

        DbSet<ServiceTaxCategory> ServiceTaxCategory { get; set; }
        DbSet<DescriptionOfGoods> DescriptionOfGoods { get; set; }

        DbSet<Narration> Narration { get; set; }

        //Customize Models
        DbSet<Rug_RetentionPercentage> Rug_RetentionPercentage { get; set; }

        //For Databse Views
        DbSet<StockUid> StockUid { get; set; }
        DbSet<StockVirtual> StockVirtual { get; set; }
        DbSet<CostCenterStatus> CostCenterStatus { get; set; }
        DbSet<ViewProdOrderHeader> ViewProdOrderHeader { get; set; }
        DbSet<ViewProdOrderLine> ViewProdOrderLine { get; set; }
        DbSet<ViewProdOrderBalance> ViewProdOrderBalance { get; set; }
        DbSet<ViewProdOrderBalanceForMPlan> ViewProdOrderBalanceForMPlan { get; set; }

        DbSet<ViewJobOrderHeader> ViewJobOrderHeader { get; set; }
        DbSet<ViewJobOrderLine> ViewJobOrderLine { get; set; }
        DbSet<ViewJobOrderBalance> ViewJobOrderBalance { get; set; }
        DbSet<ViewJobOrderBalanceForInvoice> ViewJobOrderBalanceForInvoice { get; set; }
        DbSet<ViewJobReceiveBalance> ViewJobReceiveBalance { get; set; }
        DbSet<ViewJobReceiveBalanceForInvoice> ViewJobReceiveBalanceForInvoice { get; set; }

        DbSet<ViewPurchaseIndentHeader> ViewPurchaseIndentHeader { get; set; }
        DbSet<ViewPurchaseIndentLine> ViewPurchaseIndentLine { get; set; }
        DbSet<ViewPurchaseIndentBalance> ViewPurchaseIndentBalance { get; set; }
        DbSet<ViewMaterialPlanBalance> ViewMaterialPlanBalance { get; set; }

        DbSet<ViewPurchaseOrderHeader> ViewPurchaseOrderHeader { get; set; }
        DbSet<ViewPurchaseOrderLine> ViewPurchaseOrderLine { get; set; }
        DbSet<ViewPurchaseOrderBalance> ViewPurchaseOrderBalance { get; set; }

        DbSet<ViewSaleOrderHeader> ViewSaleOrderHeader { get; set; }
        DbSet<ViewSaleOrderLine> ViewSaleOrderLine { get; set; }
        DbSet<ViewSaleOrderBalance> ViewSaleOrderBalance { get; set; }
        DbSet<ViewSaleOrderBalanceForCancellation> ViewSaleOrderBalanceForCancellation { get; set; }


        DbSet<ViewRugSize> ViewRugSize { get; set; }
        DbSet<ViewRugArea> ViewRugArea { get; set; }
        DbSet<ViewSaleInvoiceLine> ViewSaleInvoiceLine { get; set; }
        DbSet<ViewPurchaseGoodsReceiptLine> ViewPurchaseGoodsReceiptLine { get; set; }
        DbSet<ViewPurchaseGoodsReceiptBalance> ViewPurchaseGoodsReceiptBalance { get; set; }
        DbSet<ViewPurchaseInvoiceBalance> ViewPurchaseInvoiceBalance { get; set; }
        DbSet<ViewMaterialRequestBalance> ViewMaterialRequestBalance { get; set; }
        DbSet<ViewRequisitionBalance> ViewRequisitionBalance { get; set; }
        DbSet<ViewRoles> ViewRoles { get; set; }
        DbSet<_Users> _Users { get; set; }


    }
}