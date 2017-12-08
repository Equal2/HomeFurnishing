using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjLib.DocumentConstants
{
    public class VoucherTypeConstants
    {
        public const string DebitNote = "Debit Note";
        public const string CreditNote = "Credit Note";
        public const string Payment = "Payment";
        public const string Receipt = "Receipt";
        public const string Journal = "Journal";
    }

    public class TransactionDocCategoryConstants
    {
        //---------------------  Sale 
        public const string SaleOrder = "Sale Order";
        public const string SaleOrderCancel = "Sale Order Cancel";
        public const string SaleDeliveryOrderCancel = "Sales Delivery Order Cancel";
        public const string SaleOrderAmendment = "Sale Order Amendment";

        public const string Packing = "Packing";
        public const string PackingReceive = "Packing Receive";

        public const string SaleInvoice = "Sale Invoice";

        public const string DispatchWaybill = "Dispatch Waybill";
        public const string CustomDetail = "Custom Detail";



        //---------------------  Store 
        public const string GatePass = "Gate Pass";
        public const string Requisition = "Store Requisition";
        public const string RequisitionCancel = "Store Requisition Cancel";
        public const string StoreIssue = "Store Issue";
        public const string StoreReceive = "Store Receive";


        public const string DyeingMaterialReceive = "Dyeing Material Receive";



        //---------------------  Planning 
        public const string MaterialPlan = "Material Plan";
        public const string ProdOrder = "Production Order";
        public const string ProdOrderCancel = "Production Order Cancel";

        public const string SaleOrderPlan = "Sale Order Plan";
        public const string FinishinedItemProductionPLan = "Finishined Item Production PLan";



        public const string DyedMaterialPlanForWeaving = "Dyed Material Plan For Weaving";
        public const string DyeingPlanning = "Dyeing Plan";
        public const string DyeingPlanningCancel = "Dyeing Plan Cancel";

        public const string MaterialPlanForWeaving = "Material Plan For Weaving";
        public const string SpinningPlanning = "Spinning Plan";
        public const string SpinningPlanningCancel = "Spinning Plan Cancel";




        //---------------------  Purchase 
        public const string PurchaseIndent = "Purchase Indent";
        public const string PurchaseOrder = "Purchase Order";
        public const string PurchaseOrderCancel = "Purchase Order Cancel";
        public const string PurchaseGoodsReceipt = "Purchase Challan";
        public const string PurchaseInvoice = "Purchase Invoice";
        public const string PurchaseOrderAmendment = "Purchase Order Amendment";


        public const string PurchaseIndentCancel = "Purchase Indent Cancel";
        public const string PurchaseWaybill = "Purchase Waybill";
        public const string PurchaseGoodsReturn = "Purchase Goods Return";
        public const string PurchaseInvoiceReturn = "Purchase Invoice Return";





        //---------------------  Job 
        public const string JobOrder = "Job Order";
        public const string JobOrderRateAmendment = "Job Order Rate Amendment";
        public const string JobReceive = "Job Receive";
        public const string JobInvoice = "Job Invoice";
        public const string JobOrderCancel = "Job Order Cancel";
        public const string JobOrderInspectionRequest = "Job Order Inspection Request";


        public const string DyeingOrder = "Dyeing Order";
        public const string DyeingOrderCancel = "Dyeing Cancel";
        public const string DyeingReceive = "Dyeing Receive";
        public const string DyedGoodsReturn = "Dyed Goods Return";
        public const string DyeingInvoice = "Dyeing Invoice";
        public const string DyeingCancel = "Dyeing Cancel";
        public const string DyeingConsumption = "Dyeing Consumption";
        public const string DyeingCreditNote = "Dyeing Credit Note";
        public const string DyeingDebitNote = "Dyeing Debit Note";
        public const string DyeingGoodsReturn = "Dyeing Goods Return";
        public const string DyeingIssue = "Dyeing Issue";
        public const string DyeingMaterialIssue = "Dyeing Material Issue";
        public const string DyeingOrderRateAmendment = "Dyeing Order Rate Amendment";
        public const string DyeingPayment = "Dyeing Payment";
        public const string DyeingPlan = "Dyeing Plan";
        public const string DyeingPlanCancel = "Dyeing Plan Cancel";
        public const string DyeingRateConversion = "Dyeing Rate Conversion";

        public const string WeavingBazar = "Weaving Bazar";
        public const string WeavingCancel = "Weaving Cancel";
        public const string WeavingChequeCancel = "Weaving Cheque Cancel";

        public const string WeavingConsumptionAdjustment = "Weaving Consumption Adjustment";
        public const string WeavingDebitNote = "Weaving Debit Note";
        public const string WeavingExchange = "Weaving Exchange";
        public const string WeavingIssue = "Weaving Issue";
        public const string WeavingMaterialNotNeeded = "Weaving Material Not Needed";
        public const string WeavingOrder = "Weaving Order";
        public const string WeavingPayment = "Weaving Payment";
        public const string WeavingPlan = "Weaving Plan";
        public const string WeavingPlanCancel = "Weaving Plan Cancel";
        public const string WeavingReceipt = "Weaving Receipt";
        public const string WeavingReturn = "Weaving Return";
        public const string WeavingTDS = "Weaving TDS";
        public const string WeavingTimeIncentive = "Weaving Time Incentive";

        public const string SpinningOrder = "Spinning Order";
        public const string SpinningOrderCancel = "Spinning Cancel";
        public const string SpinningReceive = "Spinning Receive";
        public const string SpinningInvoice = "Spinning Invoice";

        public const string CarpetFinishingOrder = "Carpet Finishing Order";
        public const string CarpetFinishingReceive = "Carpet Finishing Receive";
        public const string CarpetFinishingInvoice = "Carpet Finishing Invoice";




        //For Masters
        public const string ProcessSequence = "Process Sequence";
        public const string Size = "Size";
        public const string Quality = "Quality";
        public const string Payment = "Payment";
        public const string Receipt = "Receipt";
        public const string DebitNote = "Debit Note";
        public const string CreditNote = "Credit Note";
        public const string Report = "Report";
    }

    public class TransactionDoctypeConstants
    {
        //---------------------  Sale 
        public const string SaleOrder = "Sale Order";
        public const string SaleOrderCancel = "Sale Order Cancel";
        public const string SaleOrderAmendment = "Sale Order Amendment";

        public const string Packing = "Packing";
        public const string PackingReceive = "Packing Receive";

        public const string SaleInvoice = "Sale Invoice";
        public const string SaleChallan = "Sale Challan";
        public const string DispatchWaybill = "Dispatch Waybill";
        public const string CustomDetail = "Custom Detail";



        //---------------------  Store 
        public const string Requisition = "Store Requisition";
        public const string RequisitionCancel = "Store Requisition Cancel";
        public const string GatePass = "Gate Pass";

        public const string StoreIssue = "Store Issue";
        public const string StoreReceive = "Store Receive";
        public const string PurjaTransfer = "Purja Transfer";
        public const string MaterialReturnFromWeaving = "Material Return From Weaving";
        public const string PurjaAmtTransfer = "Purja Amt Transfer";

        public const string DyeingMaterialReceive = "Dyeing Material Receive";



        //---------------------  Planning 
        public const string MaterialPlan = "Material Plan";
        public const string ProdOrder = "Production Order";
        public const string ProdOrderCancel = "Production Order Cancel";

        public const string SaleOrderPlan = "Sale Order Plan";
        public const string FinishinedItemProductionPLan = "Finishined Item Production PLan";



        public const string DyedMaterialPlanForWeaving = "Dyed Material Plan For Weaving";
        public const string DyeingPlanning = "Dyeing Plan";
        public const string DyeingPlanningCancel = "Dyeing Plan Cancel";

        public const string MaterialPlanForWeaving = "Material Plan For Weaving";
        public const string YarnPlanForWeaving = "Yarn Plan For Weaving";
        public const string SpinningPlanning = "Spinning Plan";
        public const string SpinningPlanningCancel = "Spinning Plan Cancel";




        //---------------------  Purchase 
        public const string PurchaseIndent = "Purchase Indent";
        public const string PurchaseOrder = "Purchase Order";
        public const string PurchaseOrderCancel = "Purchase Order Cancel";
        public const string PurchaseGoodsReceipt = "Purchase Challan";
        public const string PurchaseInvoice = "Purchase Invoice";


        public const string PurchaseIndentCancel = "Purchase Indent Cancel";
        public const string PurchaseWaybill = "Purchase Waybill";
        public const string PurchaseGoodsReturn = "Purchase Goods Return";
        public const string PurchaseInvoiceReturn = "Purchase Invoice Return";


        public const string CarpetPurchaseGoodsReturn = "Carpet Purchase Goods Return";
        public const string CarpetPurchaseChallan = "Carpet Purchase Challan";











        //---------------------  Job 
        public const string JobOrder = "Job Order";
        public const string JobOrderRateAmendment = "Job Order Rate Amendment";
        public const string JobReceive = "Job Receive";
        public const string TraceMapReceive = "Trace Map Order Receive";
        public const string TraceMapInvoice = "Trace Map Invoice";
        public const string JobInvoice = "Job Invoice";
        public const string JobOrderCancel = "Job Order Cancel";




        public const string WeavingBazar = "Weaving Bazar";
        public const string WeavingCancel = "Weaving Cancel";
        public const string WeavingChequeCancel = "Weaving Cheque Cancel";
        public const string ChequeCancel = "Cheque Cancel";
        public const string WeavingClothIssue = "Weaving Cloth Issue";
        public const string WeavingConsumptionAdjustment = "Weaving Consumption Adjustment";
        public const string WeavingDebitNote = "Weaving Debit Note";
        public const string WeavingExchange = "Weaving Exchange";
        public const string WeavingFinishOrder = "Weaving Finish Order";
        public const string WeavingIssue = "Weaving Issue";
        public const string WeavingMaterialNotNeeded = "Weaving Material Not Needed";
        //public const string WeavingOrder = "Weaving Order";
        public const string WeavingPayment = "Weaving Payment";
        public const string WeavingPlan = "Weaving Plan";
        public const string WeavingPlanCancel = "Weaving Plan Cancel";
        public const string WeavingReceipt = "Weaving Receipt";
        public const string WeavingReturn = "Weaving Return";
        public const string WeavingTDS = "Weaving TDS";
        public const string WeavingTimeIncentive = "Weaving Time Incentive";



        public const string DyeingOrder = "Dyeing Order";
        public const string DyeingReceive = "Dyeing Receive";
        public const string DyeingReturn = "Dyeing Return";
        public const string DyeingInvoice = "Dyeing Invoice";


        public const string DyeingCancel = "Dyeing Cancel";
        public const string DyeingConsumption = "Dyeing Consumption";
        public const string DyeingCreditNote = "Dyeing Credit Note";
        public const string DyeingDebitNote = "Dyeing Debit Note";
        public const string DyeingGoodsReturn = "Dyeing Goods Return";
        public const string DyeingIssue = "Dyeing Issue";
        public const string DyeingMaterialIssue = "Dyeing Material Issue";
        public const string DyeingOrderRateAmendment = "Dyeing Order Rate Amendment";
        public const string DyeingPayment = "Dyeing Payment";
        public const string DyeingPlan = "Dyeing Plan";
        public const string DyeingPlanCancel = "Dyeing Plan Cancel";
        public const string DyeingRateConversion = "Dyeing Rate Conversion";
        public const string MaterialIssueForWeaving = "Material Issue For Weaving";

        public const string RateConversion = "Rate Conversion";




        public const string SpinningOrder = "Spinning Order";
        public const string SpinningReceive = "Spinning Receive";
        public const string SpinningInvoice = "Spinning Invoice";


        public const string WeavingOrder = "Weaving Order";
        public const string WeavingReceive = "Weaving Receive";


        //For Inventory
        public const string CarpetTransfer = "Carpet Transfer";




        //For Masters
        public const string ProcessSequence = "Process Sequence";
        public const string Quality = "Quality";
        public const string Payment = "Payment";
        public const string Receipt = "Receipt";
        public const string DebitNote = "Debit Note";
        public const string CreditNote = "Credit Note";
        

        public const string PreDispatchWaybill = "Pre-Dispatch Waybill";
        public const string FinalDispatchWaybill = "Final-Dispatch Waybill";

        public const string ProductGroup = "Product Group";
        public const string Product = "Product";
        public const string Report = "Report";



        //For Finance
        public const string SchemeIncentive = "Scheme Incentive";
        public const string WeavingTimePenalty = "Weaving Time Penalty";
        public const string WeavingCreditNote = "Weaving Credit Note";
        public const string SmallChunksBazarPenalty = "Small Chunks Bazar Penalty";
    }

    public class MasterDocCategoryConstants
    {
        public const string ProcessSequence = "Process Sequence";
        public const string City = "City";
        public const string Buyer = "Buyer";
        public const string Supplier = "Supplier";
        public const string JobWorker = "Job Worker";
        public const string Quality = "Quality";
        public const string ProductCategry = "Product Categry";
        public const string ProductGroup = "Product Group";
        public const string Design = "Design";
        public const string Size = "Size";
        public const string Process = "Process";
        public const string Product = "Product";
        public const string Godown = "Godown";
        public const string LedgerAccount = "Ledger Account";
        public const string LedgerAccountGroup = "Ledger Account Group";
        public const string TdsCategory = "TDS Category";
        public const string TdsGroup = "TDS Group";
        public const string ChargeGroupPerson = "Charge Group Person";
        public const string Site = "Site";
        public const string Division = "Division";
        public const string Person = "Person";

        //ForLogActivity Existing Master constants
        public const string Colour = "Colour";
        public const string CustomDetail = "Custom Detail";

        public const string DeliveryTerms = "DeliveryTerms";
        public const string DescriptionOfGoods = "Description Of Goods";
        public const string DesignConsumption = "Design Consumption";
        public const string DrawBackTariff = "Draw Back Tariff";
        public const string PersonRateGroup = "Person Rate Group";
        public const string ProductRateGroup = "Product Rate Group";
        public const string RateListHeader = "Rate List Header";
        public const string ProductInvoiceGroup = "Product Invoice Group";
        public const string Route = "Route";
        public const string SalesTaxGroupProduct = "Sales Tax Group Product";
        public const string SalesTaxGroupParty = "Sales Tax Group Party";
        public const string ServiceTaxCategory = "Service Tax Category";
        public const string ShipMethod = "Ship Method";
        public const string UnitConversion = "Unit Conversion";
        public const string CostCenter = "Cost Center";
        public const string Country = "Country";
        public const string Currency = "Currency";
        public const string DocumentCategory = "Document Category";
        public const string DocumentType = "Document Type";
        public const string Narration = "Narration";
        public const string PersonContactType = "Person Contact Type";
        public const string Reason = "Reason";
        public const string State = "State";
        public const string Department = "Department";
        public const string Designation = "Designation";
        public const string LeaveType = "Leave Type";
        public const string Dimension1 = "Dimension1";
        public const string Dimension2 = "Dimension2";
        public const string ProductCustomGroup = "Product Custom Group";
        public const string ProductTypeAttribute = "Product Type Attribute";
        public const string ProductType = "Product Type";
        public const string Unit = "Unit";
        

        //ForLogActivity Master Constants
        public const string Calculation = "Calculation";
        public const string CalculationLedgerAccount = "Calculation Ledger Account";
        public const string Charge = "Charge";
        public const string ChargeGroupProduct = "Charge Group Product";
        public const string ChargeType = "Charge Type";
        public const string ProductConsumption = "Product Consumption";
        public const string ProductBuyer = "Product Buyer";
        public const string ProductCollection = "Product Collection";
        public const string ProductConstruction = "Product Construction";
        public const string ProductContent = "Product Content";
        public const string ProductDesign = "Product Design";
        public const string ProductDesignPattern = "Product Design Pattern";
        public const string ProductManufacturingStyle = "Product Manufacturing Style";
        public const string ProductNature = "Product Nature";
        public const string ProductQuality = "Product Quality";
        public const string ProductShape = "Product Shape";
        public const string ProductSize = "Product Size";
        public const string ProductSizeType = "Product Size Type";
        public const string ProductStyle = "Product Style";
        public const string PersonAddress = "Person Address";
        public const string PersonBankAccount = "Person Bank Account";
        public const string PersonContact = "Person Contact";
        public const string PersonDocument = "Person Document";
        public const string RateList = "Rate List";
        public const string UserRoles = "User Roles";
        public const string DocumentTypeTimePlan = "DocumentType Time Plan";
        public const string Task = "Task";
        public const string DAR = "DAR";
        public const string UserTeam = "UserTeam";
        public const string Project = "Project";
        public const string Other = "Other";
    }


    public class MasterDocTypeConstants
    {

        public const string ProductUid = "Product Uid";
        public const string ProcessSequence = "Process Sequence";
        public const string City = "City";
        public const string Buyer = "Buyer";
        public const string Quality = "Quality";
        public const string ProductCategry = "Product Category";
        public const string ProductGroup = "Product Group";
        public const string Design = "Design";
        public const string Size = "Size";
        public const string Process = "Process";
        public const string Product = "Product";
        public const string Godown = "Godown";
        public const string Gate = "Gate";
        public const string LeaveType = "Leave Type";

        public const string Task = "Task";
        public const string DAR = "DAR";
        public const string UserTeam = "UserTeam";
        public const string Project = "Project";
        public const string Other = "Other";

        
        public const string CarpetDesign = "Carpet Design";
        public const string Style = "Style";

        public const string Shape = "Shape";

        public const string DesignPattern = "Design Pattern";
        public const string Content = "Content";
        public const string Construction = "Construction";
        public const string ColourWays = "Colour Ways";
        public const string Colour = "Colour";
        public const string Collection = "Collection";
        public const string CarpetSample = "Carpet Sample";
        public const string Carpet = "Carpet";
        public const string Unit = "Unit";
        public const string Transporter = "Transporter";
        public const string TdsGroup = "Tds Group";
        public const string TdsCategory = "Tds Category";
        public const string State = "State";
        public const string Site = "Site";
        public const string ShipMethod = "Ship Method";
        public const string ServiceTaxCategory = "Service Tax Category";
        public const string SalesTaxGroupProduct = "Sales Tax Group Product";
        public const string SalesTaxGroupParty = "Sales Tax Group Party";
        public const string Route = "Route";
        public const string ProductTypeAttribute = "Product Type Attribute";
        public const string ProductType = "Product Type";
        public const string ProductNature = "Product Nature";
        public const string ProductInvoiceGroup = "Product Invoice Group";
        public const string ProductCustomGroup = "Product Custom Group";
        public const string PersonRateGroup = "Person Rate Group";
        public const string ProductRateGroup = "Product Rate Group";
        public const string RateListHeader = "Rate List Header";
        public const string Manufacturer = "Manufacturer";
        public const string LedgerAccountGroup = "Ledger Account Group";
        public const string LedgerAccount = "Ledger Account";
        public const string Employee = "Employee";
        public const string DrawBackTariff = "Draw Back Tariff";
        public const string DocumentType = "Document Type";
        public const string DocumentCategory = "Document Category";
        public const string Division = "Division";
        public const string Designation = "Designation";
        public const string DesignConsumption = "Design Consumption";
        public const string DescriptionOfGoods = "Description Of Goods";
        public const string Department = "Department";
        public const string DeliveryTerms = "DeliveryTerms";
        public const string Currency = "Currency";
        public const string Courier = "Courier";
        public const string CostCenter = "Cost Center";
        public const string Agent = "Agent";

        public const string Dimension1 = "Dimension1";
        public const string Dimension2 = "Dimension2";
        public const string Country = "Country";
        public const string CustomDetail = "Custom Detail";
        public const string FinishedProduct = "Finished Product";
        public const string Narration = "Narration";
        public const string JobWorker = "Job Worker";

        public const string PersonContactType = "Person Contact Type";
        public const string Reason = "Reason";
        public const string RugStencil = "RugStencil";
        public const string Supplier = "Supplier";
        public const string GatePass = "Gate Pass";
        public const string UnitConversion = "Unit Conversion";
        public const string ChargeGroupPerson = "Charge Group Person";

        //ForLogActivity Master Constants
        public const string Calculation = "Calculation";
        public const string CalculationLedgerAccount = "Calculation Ledger Account";
        public const string Charge = "Charge";
        public const string ChargeGroupProduct = "Charge Group Product";
        public const string ChargeType = "Charge Type";
        public const string ProductConsumption = "Product Consumption";
        public const string ProductBuyer = "Product Buyer";
        public const string ProductCollection = "Product Collection";
        public const string ProductConstruction = "Product Construction";
        public const string ProductContent = "Product Content";
        public const string ProductAlias = "Product Alias";
        public const string ProductDesign = "Product Design";
        public const string ProductDesignPattern = "Product Design Pattern";
        public const string ProductManufacturingStyle = "Product Manufacturing Style";
        public const string ProductQuality = "Product Quality";
        public const string ProductShape = "Product Shape";
        public const string ProductSize = "Product Size";
        public const string ProductSizeType = "Product Size Type";
        public const string ProductStyle = "Product Style";
        public const string PersonAddress = "Person Address";
        public const string PersonBankAccount = "Person Bank Account";
        public const string PersonContact = "Person Contact";
        public const string PersonDocument = "Person Document";
        public const string RateList = "Rate List";
        public const string UserRoles = "User Roles";
        public const string DocumentTypeTimeExtension = "DocumentType Time Extension";
        public const string DocumentTypeTimePlan = "DocumentType Time Plan";
    }

    public class MaterialPlanConstants
    {
        public const string SaleOrder = "Sale Order";
        public const string ProdOrder = "Prod Order";
    }

    public class DocumentTypeConstants
    {
        public const string Packing = "Packing";
        public const string PurchaseOrder = "Purchase Order";
        public const string PurchaseOrderCancel = "Purchase Order Cancel";
        public const string PurchaseInvoice = "Purchase Invoice";
        public const string SaleOrder = "Sale Order";
        public const string SaleOrderCancel = "Sale Order Cancel";
        public const string SaleOrderQtyAmendment = "Sale Order Qty Amendment";
        public const string PurchaseIndent = "Purchase Indent";
    }
}
