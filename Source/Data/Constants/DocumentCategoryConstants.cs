using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.DocumentCategory
{
    public static class DocumentCategoryConstants
    {
        #region "Sales"
        public static class SaleEnquiry
        {
            public const int DocumentCategoryId = 10;
            public const string DocumentCategoryName = "Sale Enquiry";
        }
        public static class SaleEnquiryCancel
        {
            public const int DocumentCategoryId = 20;
            public const string DocumentCategoryName = "Sale Enquiry Cancel";
        }
        public static class SaleQuotation
        {
            public const int DocumentCategoryId = 30;
            public const string DocumentCategoryName = "Sale Quotation";
        }
        public static class SaleQuotationCancel
        {
            public const int DocumentCategoryId = 40;
            public const string DocumentCategoryName = "Sale Quotation Cancel";
        }
        public static class SaleOrder
        {
            public const int DocumentCategoryId = 50;
            public const string DocumentCategoryName = "Sale Order";
        }
        public static class SaleOrderCancel
        {
            public const int DocumentCategoryId = 60;
            public const string DocumentCategoryName = "Sale Order Cancel";
        }
        public static class SaleOrderAmendment
        {
            public const int DocumentCategoryId = 70;
            public const string DocumentCategoryName = "Sale Order Amendment";
        }
        public static class SaleInspectionRequest
        {
            public const int DocumentCategoryId = 80;
            public const string DocumentCategoryName = "Sale Inspection Request";
        }
        public static class SaleInspectionRequestCancel
        {
            public const int DocumentCategoryId = 90;
            public const string DocumentCategoryName = "Sale Inspection Request Cancel";
        }
        public static class SaleInspection
        {
            public const int DocumentCategoryId = 100;
            public const string DocumentCategoryName = "Sale Inspection";
        }
        public static class Packing
        {
            public const int DocumentCategoryId = 110;
            public const string DocumentCategoryName = "Packing";
        }
        public static class PerformaInvoice
        {
            public const int DocumentCategoryId = 120;
            public const string DocumentCategoryName = "Performa Invoice";
        }
        public static class SaleGoodsDispatch
        {
            public const int DocumentCategoryId = 130;
            public const string DocumentCategoryName = "Sale Goods Dispatch";
        }
        public static class SaleGoodsDispatchReturn
        {
            public const int DocumentCategoryId = 140;
            public const string DocumentCategoryName = "Sale Goods Dispatch Return";
        }
        public static class SaleInvoice
        {
            public const int DocumentCategoryId = 150;
            public const string DocumentCategoryName = "Sale Invoice";
        }
        public static class SaleInvoiceReturn
        {
            public const int DocumentCategoryId = 160;
            public const string DocumentCategoryName = "Sale Invoice Return";
        }
        public static class SaleDeliveryOrder
        {
            public const int DocumentCategoryId = 170;
            public const string DocumentCategoryName = "Sale Delivery Order";
        }
        public static class SaleDeliveryOrderCancel
        {
            public const int DocumentCategoryId = 180;
            public const string DocumentCategoryName = "Sale Delivery Order Cancel";
        }
        public static class QualityChecking
        {
            public const int DocumentCategoryId = 190;
            public const string DocumentCategoryName = "Quality Checking";
        }
        public static class DebitNoteSale
        {
            public const int DocumentCategoryId = 200;
            public const string DocumentCategoryName = "Debit Note (Sale)";
        }
        public static class CreditNoteSale
        {
            public const int DocumentCategoryId = 210;
            public const string DocumentCategoryName = "Credit Note (Sale)";
        }
        #endregion
        #region "Purchase"
        public static class PurchaseIndent
        {
            public const int DocumentCategoryId = 220;
            public const string DocumentCategoryName = "Purchase Indent";
        }
        public static class PurchaseIndentCancel
        {
            public const int DocumentCategoryId = 230;
            public const string DocumentCategoryName = "Purchase Indent Cancel";
        }
        public static class PurchaseEnquiry
        {
            public const int DocumentCategoryId = 240;
            public const string DocumentCategoryName = "Purchase Enquiry";
        }
        public static class PurchaseEnquiryCancel
        {
            public const int DocumentCategoryId = 250;
            public const string DocumentCategoryName = "Purchase Enquiry Cancel";
        }
        public static class PurchaseQuotation
        {
            public const int DocumentCategoryId = 260;
            public const string DocumentCategoryName = "Purchase Quotation";
        }
        public static class PurchaseQuotationCancel
        {
            public const int DocumentCategoryId = 270;
            public const string DocumentCategoryName = "Purchase Quotation Cancel";
        }
        public static class PurchaseOrder
        {
            public const int DocumentCategoryId = 280;
            public const string DocumentCategoryName = "Purchase Order";
        }
        public static class PurchaseOrderCancel
        {
            public const int DocumentCategoryId = 290;
            public const string DocumentCategoryName = "Purchase Order Cancel";
        }
        public static class PurchaseOrderAmendment
        {
            public const int DocumentCategoryId = 300;
            public const string DocumentCategoryName = "Purchase Order Amendment";
        }
        public static class PurchaseInspectionRequest
        {
            public const int DocumentCategoryId = 320;
            public const string DocumentCategoryName = "Purchase Inspection Request";
        }
        public static class PurchaseInspectionRequestCancel
        {
            public const int DocumentCategoryId = 330;
            public const string DocumentCategoryName = "Purchase Inspection Request Cancel";
        }
        public static class PurchaseInspection
        {
            public const int DocumentCategoryId = 340;
            public const string DocumentCategoryName = "Purchase Inspection";
        }
        public static class PurchaseGoodsReceipt
        {
            public const int DocumentCategoryId = 350;
            public const string DocumentCategoryName = "Purchase Goods Receipt";
        }
        public static class PurchaseGoodsReceiptQC
        {
            public const int DocumentCategoryId = 360;
            public const string DocumentCategoryName = "Purchase Goods Receipt QC";
        }
        public static class PurchaseGoodsReturn
        {
            public const int DocumentCategoryId = 370;
            public const string DocumentCategoryName = "Purchase Goods Return";
        }
        public static class PurchaseInvoice
        {
            public const int DocumentCategoryId = 380;
            public const string DocumentCategoryName = "Purchase Invoice";
        }
        public static class PurchaseInvoiceReturn
        {
            public const int DocumentCategoryId = 390;
            public const string DocumentCategoryName = "Purchase Invoice Return";
        }
        #endregion
        #region "Job"
        public static class ProductionOrder
        {
            public const int DocumentCategoryId = 400;
            public const string DocumentCategoryName = "Production Order";
        }
        public static class ProductionOrderCancel
        {
            public const int DocumentCategoryId = 410;
            public const string DocumentCategoryName = "Production Order Cancel";
        }
        public static class JobEnquiry
        {
            public const int DocumentCategoryId = 420;
            public const string DocumentCategoryName = "Job Enquiry";
        }
        public static class JobEnquiryCancel
        {
            public const int DocumentCategoryId = 430;
            public const string DocumentCategoryName = "Job Enquiry Cancel";
        }
        public static class JobQuotation
        {
            public const int DocumentCategoryId = 440;
            public const string DocumentCategoryName = "Job Quotation";
        }
        public static class JobQuotationCancel
        {
            public const int DocumentCategoryId = 450;
            public const string DocumentCategoryName = "Job Quotation Cancel";
        }
        public static class JobOrder
        {
            public const int DocumentCategoryId = 460;
            public const string DocumentCategoryName = "Job Order";
        }
        public static class JobOrderCancel
        {
            public const int DocumentCategoryId = 470;
            public const string DocumentCategoryName = "Job Order Cancel";
        }
        public static class JobOrderAmendment
        {
            public const int DocumentCategoryId = 480;
            public const string DocumentCategoryName = "Job Order Amendment";
        }
        public static class JobReceive
        {
            public const int DocumentCategoryId = 490;
            public const string DocumentCategoryName = "Job Receive";
        }
        public static class JobInspectionRequest
        {
            public const int DocumentCategoryId = 500;
            public const string DocumentCategoryName = "Job Inspection Request";
        }
        public static class JobInspectionRequestCancel
        {
            public const int DocumentCategoryId = 510;
            public const string DocumentCategoryName = "Job Inspection Request Cancel";
        }
        public static class JobInspection
        {
            public const int DocumentCategoryId = 520;
            public const string DocumentCategoryName = "Job Inspection";
        }
        public static class JobReceiveQC
        {
            public const int DocumentCategoryId = 530;
            public const string DocumentCategoryName = "Job Receive QC";
        }
        public static class JobReturn
        {
            public const int DocumentCategoryId = 540;
            public const string DocumentCategoryName = "Job Return";
        }
        public static class JobInvoice
        {
            public const int DocumentCategoryId = 550;
            public const string DocumentCategoryName = "Job Invoice";
        }
        public static class JobInvoiceReturn
        {
            public const int DocumentCategoryId = 560;
            public const string DocumentCategoryName = "Job Invoice Return";
        }
        public static class DebitNoteOutward
        {
            public const int DocumentCategoryId = 570;
            public const string DocumentCategoryName = "Debit Note Outward";
        }
        public static class CreditNoteOutward
        {
            public const int DocumentCategoryId = 580;
            public const string DocumentCategoryName = "Credit Note Outward";
        }
        #endregion
        #region "Inventory"
        public static class StockRequisition
        {
            public const int DocumentCategoryId = 590;
            public const string DocumentCategoryName = "Stock Requisition";
        }
        public static class StockRequisitionCancel
        {
            public const int DocumentCategoryId = 600;
            public const string DocumentCategoryName = "Stock Requisition Cancel";
        }
        public static class StockIssue
        {
            public const int DocumentCategoryId = 610;
            public const string DocumentCategoryName = "Stock Issue";
        }
        public static class StockReceive
        {
            public const int DocumentCategoryId = 620;
            public const string DocumentCategoryName = "Stock Receive";
        }
        public static class StockTransfer
        {
            public const int DocumentCategoryId = 630;
            public const string DocumentCategoryName = "Stock Transfer";
        }
        public static class StockExchange
        {
            public const int DocumentCategoryId = 640;
            public const string DocumentCategoryName = "Stock Exchange";
        }
        public static class GatePass
        {
            public const int DocumentCategoryId = 650;
            public const string DocumentCategoryName = "Gate Pass";
        }
        #endregion
        #region "Planning"
        public static class SaleOrderPlan
        {
            public const int DocumentCategoryId = 660;
            public const string DocumentCategoryName = "Sale Order Plan";
        }
        #endregion
        #region "Accounts"
        public static class PaymentVoucher
        {
            public const int DocumentCategoryId = 670;
            public const string DocumentCategoryName = "Payment Voucher";
        }
        public static class ReceiptVoucher
        {
            public const int DocumentCategoryId = 680;
            public const string DocumentCategoryName = "Receipt Voucher";
        }
        public static class JournalVoucher
        {
            public const int DocumentCategoryId = 690;
            public const string DocumentCategoryName = "Journal Voucher";
        }
        public static class ContraVoucher
        {
            public const int DocumentCategoryId = 700;
            public const string DocumentCategoryName = "Contra Voucher";
        }
        public static class DebitNote
        {
            public const int DocumentCategoryId = 710;
            public const string DocumentCategoryName = "Debit Note";
        }
        public static class CreditNote
        {
            public const int DocumentCategoryId = 720;
            public const string DocumentCategoryName = "Credit Note";
        }
        public static class ChequeCancel
        {
            public const int DocumentCategoryId = 730;
            public const string DocumentCategoryName = "Cheque Cancel";
        }
        public static class ExpenseVoucher
        {
            public const int DocumentCategoryId = 740;
            public const string DocumentCategoryName = "Expense Voucher";
        }
        #endregion
        #region "Masters"
        public static class Person
        {
            public const int DocumentCategoryId = 750;
            public const string DocumentCategoryName = "Person";
        }
       #endregion
    }
}