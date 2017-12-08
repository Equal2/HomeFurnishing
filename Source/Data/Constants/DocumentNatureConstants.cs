using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.DocumentNature
{
    public static class DocumentNatureConstants
    {
        #region "Sales"
        public static class SaleEnquiry
        {
            public const int DocumentNatureId = 10;
            public const string DocumentNatureName = "Sale Enquiry";
        }
        public static class SaleEnquiryCancel
        {
            public const int DocumentNatureId = 20;
            public const string DocumentNatureName = "Sale Enquiry Cancel";
        }
        public static class SaleQuotation
        {
            public const int DocumentNatureId = 30;
            public const string DocumentNatureName = "Sale Quotation";
        }
        public static class SaleQuotationCancel
        {
            public const int DocumentNatureId = 40;
            public const string DocumentNatureName = "Sale Quotation Cancel";
        }
        public static class SaleOrder
        {
            public const int DocumentNatureId = 50;
            public const string DocumentNatureName = "Sale Order";
        }
        public static class SaleOrderCancel
        {
            public const int DocumentNatureId = 60;
            public const string DocumentNatureName = "Sale Order Cancel";
        }
        public static class SaleOrderAmendment
        {
            public const int DocumentNatureId = 70;
            public const string DocumentNatureName = "Sale Order Amendment";
        }
        public static class SaleInspectionRequest
        {
            public const int DocumentNatureId = 80;
            public const string DocumentNatureName = "Sale Inspection Request";
        }
        public static class SaleInspectionRequestCancel
        {
            public const int DocumentNatureId = 90;
            public const string DocumentNatureName = "Sale Inspection Request Cancel";
        }
        public static class SaleInspection
        {
            public const int DocumentNatureId = 100;
            public const string DocumentNatureName = "Sale Inspection";
        }
        public static class Packing
        {
            public const int DocumentNatureId = 110;
            public const string DocumentNatureName = "Packing";
        }
        public static class PerformaInvoice
        {
            public const int DocumentNatureId = 120;
            public const string DocumentNatureName = "Performa Invoice";
        }
        public static class SaleGoodsDispatch
        {
            public const int DocumentNatureId = 130;
            public const string DocumentNatureName = "Sale Goods Dispatch";
        }
        public static class SaleGoodsDispatchReturn
        {
            public const int DocumentNatureId = 140;
            public const string DocumentNatureName = "Sale Goods Dispatch Return";
        }
        public static class SaleInvoice
        {
            public const int DocumentNatureId = 150;
            public const string DocumentNatureName = "Sale Invoice";
        }
        public static class SaleInvoiceReturn
        {
            public const int DocumentNatureId = 160;
            public const string DocumentNatureName = "Sale Invoice Return";
        }
        public static class SaleDeliveryOrder
        {
            public const int DocumentNatureId = 170;
            public const string DocumentNatureName = "Sale Delivery Order";
        }
        public static class SaleDeliveryOrderCancel
        {
            public const int DocumentNatureId = 180;
            public const string DocumentNatureName = "Sale Delivery Order Cancel";
        }
        public static class QualityChecking
        {
            public const int DocumentNatureId = 190;
            public const string DocumentNatureName = "Quality Checking";
        }
        public static class DebitNoteSale
        {
            public const int DocumentNatureId = 200;
            public const string DocumentNatureName = "Debit Note (Sale)";
        }
        public static class CreditNoteSale
        {
            public const int DocumentNatureId = 210;
            public const string DocumentNatureName = "Credit Note (Sale)";
        }
        #endregion
        #region "Job"
        public static class ProductionOrder
        {
            public const int DocumentNatureId = 220;
            public const string DocumentNatureName = "Production Order";
        }
        public static class ProductionOrderCancel
        {
            public const int DocumentNatureId = 230;
            public const string DocumentNatureName = "Production Order Cancel";
        }
        public static class JobEnquiry
        {
            public const int DocumentNatureId = 240;
            public const string DocumentNatureName = "Job Enquiry";
        }
        public static class JobEnquiryCancel
        {
            public const int DocumentNatureId = 250;
            public const string DocumentNatureName = "Job Enquiry Cancel";
        }
        public static class JobQuotation
        {
            public const int DocumentNatureId = 260;
            public const string DocumentNatureName = "Job Quotation";
        }
        public static class JobQuotationCancel
        {
            public const int DocumentNatureId = 270;
            public const string DocumentNatureName = "Job Quotation Cancel";
        }
        public static class JobOrder
        {
            public const int DocumentNatureId = 280;
            public const string DocumentNatureName = "Job Order";
        }
        public static class JobOrderCancel
        {
            public const int DocumentNatureId = 290;
            public const string DocumentNatureName = "Job Order Cancel";
        }
        public static class JobOrderAmendment
        {
            public const int DocumentNatureId = 300;
            public const string DocumentNatureName = "Job Order Amendment";
        }
        public static class JobReceive
        {
            public const int DocumentNatureId = 310;
            public const string DocumentNatureName = "Job Receive";
        }
        public static class JobInspectionRequest
        {
            public const int DocumentNatureId = 320;
            public const string DocumentNatureName = "Job Inspection Request";
        }
        public static class JobInspectionRequestCancel
        {
            public const int DocumentNatureId = 330;
            public const string DocumentNatureName = "Job Inspection Request Cancel";
        }
        public static class JobInspection
        {
            public const int DocumentNatureId = 340;
            public const string DocumentNatureName = "Job Inspection";
        }
        public static class JobReceiveQC
        {
            public const int DocumentNatureId = 350;
            public const string DocumentNatureName = "Job Receive QC";
        }
        public static class JobReturn
        {
            public const int DocumentNatureId = 360;
            public const string DocumentNatureName = "Job Return";
        }
        public static class JobInvoice
        {
            public const int DocumentNatureId = 370;
            public const string DocumentNatureName = "Job Invoice";
        }
        public static class JobInvoiceReturn
        {
            public const int DocumentNatureId = 380;
            public const string DocumentNatureName = "Job Invoice Return";
        }
        public static class DebitNoteOutward
        {
            public const int DocumentNatureId = 390;
            public const string DocumentNatureName = "Debit Note Outward";
        }
        public static class CreditNoteOutward
        {
            public const int DocumentNatureId = 400;
            public const string DocumentNatureName = "Credit Note Outward";
        }
        #endregion
        #region "Inventory"
        public static class StockRequisition
        {
            public const int DocumentNatureId = 410;
            public const string DocumentNatureName = "Stock Requisition";
        }
        public static class StockRequisitionCancel
        {
            public const int DocumentNatureId = 420;
            public const string DocumentNatureName = "Stock Requisition Cancel";
        }
        public static class StockIssue
        {
            public const int DocumentNatureId = 430;
            public const string DocumentNatureName = "Stock Issue";
        }
        public static class StockReceive
        {
            public const int DocumentNatureId = 440;
            public const string DocumentNatureName = "Stock Receive";
        }
        public static class StockTransfer
        {
            public const int DocumentNatureId = 450;
            public const string DocumentNatureName = "Stock Transfer";
        }
        public static class StockExchange
        {
            public const int DocumentNatureId = 460;
            public const string DocumentNatureName = "Stock Exchange";
        }
        public static class GatePass
        {
            public const int DocumentNatureId = 470;
            public const string DocumentNatureName = "Gate Pass";
        }
        #endregion
        #region "Planning"
        public static class SaleOrderPlan
        {
            public const int DocumentNatureId = 480;
            public const string DocumentNatureName = "Sale Order Plan";
        }

        #endregion
        #region "Accounts"
        public static class PaymentVoucher
        {
            public const int DocumentNatureId = 490;
            public const string DocumentNatureName = "Payment Voucher";
        }
        public static class ReceiptVoucher
        {
            public const int DocumentNatureId = 500;
            public const string DocumentNatureName = "Receipt Voucher";
        }
        public static class JournalVoucher
        {
            public const int DocumentNatureId = 510;
            public const string DocumentNatureName = "Journal Voucher";
        }
        public static class ContraVoucher
        {
            public const int DocumentNatureId = 520;
            public const string DocumentNatureName = "Contra Voucher";
        }
        public static class DebitNote
        {
            public const int DocumentNatureId = 530;
            public const string DocumentNatureName = "Debit Note";
        }
        public static class CreditNote
        {
            public const int DocumentNatureId = 540;
            public const string DocumentNatureName = "Credit Note";
        }
        public static class ChequeCancel
        {
            public const int DocumentNatureId = 550;
            public const string DocumentNatureName = "Cheque Cancel";
        }
        public static class ExpenseVoucher
        {
            public const int DocumentNatureId = 560;
            public const string DocumentNatureName = "Expense Voucher";
        }
        #endregion
        #region "Person"
        public static class Person
        {
            public const int DocumentNatureId = 570;
            public const string DocumentNatureName = "Person";
        }
        #endregion
    }
}