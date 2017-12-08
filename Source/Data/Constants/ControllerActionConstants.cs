using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.ControllerAction
{
    public static class ControllerActionConstants
    {
        #region "Sales"
        public static class SaleEnquiry
        {
            public const string ControllerName = "SaleEnquiryHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleEnquiryCancel
        {
            public const string ControllerName = "SaleEnquiryCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleQuotation
        {
            public const string ControllerName = "SaleQuotationHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleQuotationCancel
        {
            public const string ControllerName = "SaleQuotationCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleOrder
        {
            public const string ControllerName = "SaleOrderHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleOrderCancel
        {
            public const string ControllerName = "SaleOrderCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleOrderAmendment
        {
            public const string ControllerName = "SaleOrderAmendmentHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleInspectionRequest
        {
            public const string ControllerName = "SaleInspectionRequestHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleInspectionRequestCancel
        {
            public const string ControllerName = "SaleInspectionRequestCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleInspection
        {
            public const string ControllerName = "SaleInspectionHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleDeliveryOrder
        {
            public const string ControllerName = "SaleDeliveryOrderHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleDeliveryOrderCancel
        {
            public const string ControllerName = "SaleDeliveryOrderCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class QualityChecking
        {
            public const string ControllerName = "QualityCheckingHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class Packing
        {
            public const string ControllerName = "PackingHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class PerformaInvoice
        {
            public const string ControllerName = "SaleInvoiceHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleGoodsDispatch
        {
            public const string ControllerName = "SaleDispatchHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleDispatchReturn
        {
            public const string ControllerName = "SaleDispatchReturnHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleInvoice
        {
            public const string ControllerName = "SaleInvoiceHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class SaleInvoiceReturn
        {
            public const string ControllerName = "SaleInvoiceReturnHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class DebitNoteSale
        {
            public const string ControllerName = "SaleInvoiceReturnHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class CreditNoteSale
        {
            public const string ControllerName = "SaleInvoiceReturnHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        #endregion
        #region "Job"
        public static class JobEnquiry
        {
            public const string ControllerName = "JobEnquiryHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobEnquiryCancel
        {
            public const string ControllerName = "JobEnquiryCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobQuotation
        {
            public const string ControllerName = "JobQuotationHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobQuotationCancel
        {
            public const string ControllerName = "JobQuotationCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobOrder
        {
            public const string ControllerName = "JobOrderHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobOrderCancel
        {
            public const string ControllerName = "JobOrderCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobOrderAmendment
        {
            public const string ControllerName = "JobOrderAmendmentHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobInspectionRequest
        {
            public const string ControllerName = "JobInspectionRequestHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobInspectionRequestCancel
        {
            public const string ControllerName = "JobInspectionRequestCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobInspection
        {
            public const string ControllerName = "JobInspectionHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobReceive
        {
            public const string ControllerName = "JobReceiveHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobReceiveQC
        {
            public const string ControllerName = "JobReceiveQCHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobReturn
        {
            public const string ControllerName = "JobReturnHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobInvoice
        {
            public const string ControllerName = "JobInvoiceReceiveHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JobInvoiceReturn
        {
            public const string ControllerName = "JobInvoiceReturnHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class DebitNoteOutward
        {
            public const string ControllerName = "JobInvoiceReturnHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class CreditNoteOutward
        {
            public const string ControllerName = "JobInvoiceReturnHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        #endregion
        #region "Inventory"
        public static class StockRequisition
        {
            public const string ControllerName = "MaterialRequestHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class StockRequisitionCancel
        {
            public const string ControllerName = "MaterialRequestCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class StockIssue
        {
            public const string ControllerName = "StockIssueHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class StockReceive
        {
            public const string ControllerName = "StockReceiveHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class StockTransfer
        {
            public const string ControllerName = "MaterialTransferHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class StockExchange
        {
            public const string ControllerName = "StockExchange";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class StockReconciliation
        {
            public const string ControllerName = "StockReconciliation";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class GatePass
        {
            public const string ControllerName = "GatePass";
            public const string ActionName = "Index";
        }
        #endregion
        #region "Planning"
        public static class SaleOrderPlan
        {
            public const string ControllerName = "MaterialPlanHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class ProductionOrder
        {
            public const string ControllerName = "ProdOrderHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class ProductionOrderCancel
        {
            public const string ControllerName = "ProdOrderCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        #endregion
        #region "Accounts"
        public static class PaymentVoucher
        {
            public const string ControllerName = "LedgerHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class ReceiptVoucher
        {
            public const string ControllerName = "LedgerHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class JournalVoucher
        {
            public const string ControllerName = "LedgerHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class ContraVoucher
        {
            public const string ControllerName = "LedgerHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class ExpenseVoucher
        {
            public const string ControllerName = "JobInvoiceHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class ChequeCancel
        {
            public const string ControllerName = "ChequeCancelHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class DebitNote
        {
            public const string ControllerName = "LedgerHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class CreditNote
        {
            public const string ControllerName = "LedgerHeader";
            public const string ActionName = "DocumentTypeIndex";
        }
        public static class BankReconciliation
        {
            public const string ControllerName = "BankReconciliation";
            public const string ActionName = "Index";
        }
        #endregion
        #region "Report"
        public static class Report
        {
            public const string ControllerName = "Report_ReportPrint";
            public const string ActionName = "ReportPrint";
        }
#endregion
        #region "Master"
        public static class Person
        {
            public const string ControllerName = "Person";
            public const string ActionName = "Index";
        }
        public static class Product
        {
            public const string ControllerName = "Product";
            public const string ActionName = "MaterialIndex";
        }
        public static class ProductGroup
        {
            public const string ControllerName = "ProductGroup";
            public const string ActionName = "Index";
        }
        public static class ProductCategory
        {
            public const string ControllerName = "ProductCategory";
            public const string ActionName = "Index";
        }
        public static class ProductType
        {
            public const string ControllerName = "ProductType";
            public const string ActionName = "Index";
        }
        public static class ProductCustomGroup
        {
            public const string ControllerName = "ProductCustomGroup";
            public const string ActionName = "Index";
        }
        public static class Godown
        {
            public const string ControllerName = "Godown";
            public const string ActionName = "Index";
        }
        public static class SalesTaxProductCode
        {
            public const string ControllerName = "SalesTaxProductCode";
            public const string ActionName = "Index";
        }
        public static class Gate
        {
            public const string ControllerName = "Gate";
            public const string ActionName = "Index";
        }
        public static class City
        {
            public const string ControllerName = "City";
            public const string ActionName = "Index";
        }
        public static class State
        {
            public const string ControllerName = "State";
            public const string ActionName = "Index";
        }
        public static class Country
        {
            public const string ControllerName = "Country";
            public const string ActionName = "Index";
        }
        public static class Employee
        {
            public const string ControllerName = "Employee";
            public const string ActionName = "Index";
        }
        public static class LedgerAccount
        {
            public const string ControllerName = "LedgerAccount";
            public const string ActionName = "Index";
        }
        public static class LedgerAccountGroup
        {
            public const string ControllerName = "LedgerAccountGroup";
            public const string ActionName = "Index";
        }
        public static class CostCenter
        {
            public const string ControllerName = "CostCenter";
            public const string ActionName = "Index";
        }
        public static class TdsCategory
        {
            public const string ControllerName = "TdsCategory";
            public const string ActionName = "Index";
        }
        public static class TdsGroup
        {
            public const string ControllerName = "TdsGroup";
            public const string ActionName = "Index";
        }
        public static class ChargeGroupSettings
        {
            public const string ControllerName = "ChargeGroupSettings";
            public const string ActionName = "Index";
        }
        public static class DocumentCategory
        {
            public const string ControllerName = "DocumentCategory";
            public const string ActionName = "Index";
        }
        public static class DocumentType
        {
            public const string ControllerName = "DocumentType";
            public const string ActionName = "Index";
        }
        public static class Site
        {
            public const string ControllerName = "Site";
            public const string ActionName = "Index";
        }
        public static class Division
        {
            public const string ControllerName = "Division";
            public const string ActionName = "Index";
        }
        public static class Charge
        {
            public const string ControllerName = "Charge";
            public const string ActionName = "Index";
        }
        public static class Calculation
        {
            public const string ControllerName = "Calculation";
            public const string ActionName = "Index";
        }
        public static class CalculationLedgerAccounts
        {
            public const string ControllerName = "CalculationLedgerAccounts";
            public const string ActionName = "Index";
        }
        public static class AssignPermissions
        {
            public const string ControllerName = "CalculationLedgerAccounts";
            public const string ActionName = "Index";
        }
        public static class CreateRoles
        {
            public const string ControllerName = "CalculationLedgerAccounts";
            public const string ActionName = "Index";
        }
        public static class Reason
        {
            public const string ControllerName = "CalculationLedgerAccounts";
            public const string ActionName = "Index";
        }
        public static class AssignNewRoles
        {
            public const string ControllerName = "CalculationLedgerAccounts";
            public const string ActionName = "Index";
        }
        public static class AssignTemporaryRoles
        {
            public const string ControllerName = "CalculationLedgerAccounts";
            public const string ActionName = "Index";
        }
        public static class UpdateTableStructure
        {
            public const string ControllerName = "CalculationLedgerAccounts";
            public const string ActionName = "Index";
        }
        public static class UserInvitation
        {
            public const string ControllerName = "CalculationLedgerAccounts";
            public const string ActionName = "Index";
        }
        public static class DAR
        {
            public const string ControllerName = "CalculationLedgerAccounts";
            public const string ActionName = "Index";
        }
        public static class Tasks
        {
            public const string ControllerName = "CalculationLedgerAccounts";
            public const string ActionName = "Index";
        }
        #endregion
    }
}