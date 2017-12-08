using Jobs.Constants.DocumentCategory;
using Jobs.Constants.DocumentNature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.DocumentType
{
    public static class DocumentTypeConstants
    {
        #region "Sales"
        public static class SaleEnquiry
        {
            public const int DocumentTypeId = 10;
            public const string DocumentTypeShortName = "SENQR";
            public const string DocumentTypeName = "Sale Enquiry";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleEnquiry.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleEnquiry.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleEnquiryCancel
        {
            public const int DocumentTypeId = 20;
            public const string DocumentTypeShortName = "SENCN";
            public const string DocumentTypeName = "Sale Enquiry Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleEnquiryCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleEnquiryCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleQuotation
        {
            public const int DocumentTypeId = 30;
            public const string DocumentTypeShortName = "SOUOT";
            public const string DocumentTypeName = "Sale Quotation";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleQuotation.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleQuotation.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleQuotationCancel
        {
            public const int DocumentTypeId = 40;
            public const string DocumentTypeShortName = "SOTCN";
            public const string DocumentTypeName = "Sale Quotation Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleQuotationCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleQuotationCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleOrder 
        {
            public const int DocumentTypeId = 50;
            public const string DocumentTypeShortName = "SORDR";
            public const string DocumentTypeName = "Sale Order";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleOrder.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleOrder.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleOrderCancel
        {
            public const int DocumentTypeId = 60;
            public const string DocumentTypeShortName = "SOCNL";
            public const string DocumentTypeName = "Sale Order Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleOrderCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleOrderCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleOrderAmendment
        {
            public const int DocumentTypeId = 70;
            public const string DocumentTypeShortName = "SOAMD";
            public const string DocumentTypeName = "Sale Order Amendment";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleOrderAmendment.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleOrderAmendment.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleInspectionRequest
        {
            public const int DocumentTypeId = 80;
            public const string DocumentTypeShortName = "SINRQ";
            public const string DocumentTypeName = "Sale Inspection Request";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleInspectionRequest.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleInspectionRequest.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleInspectionRequestCancel
        {
            public const int DocumentTypeId = 90;
            public const string DocumentTypeShortName = "SINRC";
            public const string DocumentTypeName = "Sale Inspection Request Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleInspectionRequestCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleInspectionRequestCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleInspection
        {
            public const int DocumentTypeId = 100;
            public const string DocumentTypeShortName = "SINSP";
            public const string DocumentTypeName = "Sale Inspection";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleInspection.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleInspection.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class Packing
        {
            public const int DocumentTypeId = 110;
            public const string DocumentTypeShortName = "PCKNG";
            public const string DocumentTypeName = "Packing";
            public const int DocumentCategoryId = DocumentCategoryConstants.Packing.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.Packing.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleGoodsDispatch
        {
            public const int DocumentTypeId = 120;
            public const string DocumentTypeShortName = "SGDSP";
            public const string DocumentTypeName = "Sale Goods Dispatch";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleGoodsDispatch.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleGoodsDispatch.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleGoodsDispatchReturn
        {
            public const int DocumentTypeId = 130;
            public const string DocumentTypeShortName = "SGDRT";
            public const string DocumentTypeName = "Sale Goods Dispatch Return";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleGoodsDispatchReturn.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleGoodsDispatchReturn.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleInvoice
        {
            public const int DocumentTypeId = 140;
            public const string DocumentTypeShortName = "SINVC";
            public const string DocumentTypeName = "Sale Invoice";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleInvoice.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleInvoice.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleInvoiceReturn
        {
            public const int DocumentTypeId = 150;
            public const string DocumentTypeShortName = "SIRET";
            public const string DocumentTypeName = "Sale Invoice Return";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleInvoiceReturn.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleInvoiceReturn.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleDeliveryOrder
        {
            public const int DocumentTypeId = 160;
            public const string DocumentTypeShortName = "SDORD";
            public const string DocumentTypeName = "Sale Delivery Order";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleInvoiceReturn.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleInvoiceReturn.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class SaleDeliveryOrderCancel
        {
            public const int DocumentTypeId = 170;
            public const string DocumentTypeShortName = "SDOCN";
            public const string DocumentTypeName = "Sale Delivery Order Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleInvoiceReturn.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleInvoiceReturn.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class QualityChecking
        {
            public const int DocumentTypeId = 180;
            public const string DocumentTypeShortName = "QCHCK";
            public const string DocumentTypeName = "Quality Checking";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleInvoiceReturn.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleInvoiceReturn.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class DebitNoteSale
        {
            public const int DocumentTypeId = 190;
            public const string DocumentTypeShortName = "DNtSL";
            public const string DocumentTypeName = "Debit Note Sale";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleInvoiceReturn.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleInvoiceReturn.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class CreditNoteSale
        {
            public const int DocumentTypeId = 200;
            public const string DocumentTypeShortName = "CNTSL";
            public const string DocumentTypeName = "Credit Note Sale";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleInvoiceReturn.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleInvoiceReturn.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        #endregion
        #region "Purchase"
        public static class PurchaseIndent
        {
            public const int DocumentTypeId = 210;
            public const string DocumentTypeShortName = "PINDT";
            public const string DocumentTypeName = "Purchase Indent";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseIndent.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.ProductionOrder.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseIndentCancel
        {
            public const int DocumentTypeId = 220;
            public const string DocumentTypeShortName = "PINCL";
            public const string DocumentTypeName = "Purchase Indent Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseIndentCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.ProductionOrderCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseEnquiry
        {
            public const int DocumentTypeId = 230;
            public const string DocumentTypeShortName = "PENQR";
            public const string DocumentTypeName = "Purchase Enquiry";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseEnquiry.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobEnquiry.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseEnquiryCancel
        {
            public const int DocumentTypeId = 240;
            public const string DocumentTypeShortName = "PENCN";
            public const string DocumentTypeName = "Purchase Enquiry Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseEnquiry.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobEnquiryCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseQuotation
        {
            public const int DocumentTypeId = 250;
            public const string DocumentTypeShortName = "POUOT";
            public const string DocumentTypeName = "Purchase Quotation";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseQuotation.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobQuotation.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseQuotationCancel
        {
            public const int DocumentTypeId = 260;
            public const string DocumentTypeShortName = "PQCNL";
            public const string DocumentTypeName = "Purchase Quotation Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseQuotationCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobQuotationCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseOrder
        {
            public const int DocumentTypeId = 270;
            public const string DocumentTypeShortName = "PORDR";
            public const string DocumentTypeName = "Purchase Order";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseOrder.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobOrder.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }

        public static class PurchaseOrderCancel
        {
            public const int DocumentTypeId = 280;
            public const string DocumentTypeShortName = "POCNL";
            public const string DocumentTypeName = "Purchase Order Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseOrderCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobOrderCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseOrderAmendment
        {
            public const int DocumentTypeId = 290;
            public const string DocumentTypeShortName = "POAMD";
            public const string DocumentTypeName = "Purchase Order Amendment";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseOrderAmendment.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobOrderAmendment.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseInspectionRequest
        {
            public const int DocumentTypeId = 300;
            public const string DocumentTypeShortName = "PIREQ";
            public const string DocumentTypeName = "Purchase Inspection Request";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseInspectionRequest.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobInspectionRequest.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseInspectionRequestCancel
        {
            public const int DocumentTypeId = 310;
            public const string DocumentTypeShortName = "PIRQC";
            public const string DocumentTypeName = "Purchase Inspection Request Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseInspectionRequestCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobInspectionRequestCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseInspection
        {
            public const int DocumentTypeId = 320;
            public const string DocumentTypeShortName = "PINSP";
            public const string DocumentTypeName = "Purchase Inspection";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseInspection.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobInspection.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }

        public static class PurchaseGoodsReceipt
        {
            public const int DocumentTypeId = 330;
            public const string DocumentTypeShortName = "PGRCT";
            public const string DocumentTypeName = "Purchase Goods Receipt";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseGoodsReceipt.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobReceive.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseGoodsReceiptQC
        {
            public const int DocumentTypeId = 340;
            public const string DocumentTypeShortName = "PGRQC";
            public const string DocumentTypeName = "Purchase Goods Receipt QC";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseGoodsReceipt.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobReceiveQC.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseGoodsReturn
        {
            public const int DocumentTypeId = 350;
            public const string DocumentTypeShortName = "PGRET";
            public const string DocumentTypeName = "Purchase Goods Return";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseGoodsReturn.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobReturn.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseInvoice
        {
            public const int DocumentTypeId = 360;
            public const string DocumentTypeShortName = "PINVC";
            public const string DocumentTypeName = "Purchase Invoice";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseInvoice.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobInvoice.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class PurchaseInvoiceReturn
        {
            public const int DocumentTypeId = 370;
            public const string DocumentTypeShortName = "PIRET";
            public const string DocumentTypeName = "Purchase Invoice Return";
            public const int DocumentCategoryId = DocumentCategoryConstants.PurchaseInvoiceReturn.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobInvoiceReturn.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        #endregion
        #region "Job"
        public static class ProductionOrder
        {
            public const int DocumentTypeId = 380;
            public const string DocumentTypeShortName = "PRORD";
            public const string DocumentTypeName = "Production Order";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobQuotation.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobQuotation.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class ProductionOrderCancel
        {
            public const int DocumentTypeId = 390;
            public const string DocumentTypeShortName = "PROCN";
            public const string DocumentTypeName = "Production Order Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobQuotation.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobQuotation.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobEnquiry
        {
            public const int DocumentTypeId = 400;
            public const string DocumentTypeShortName = "JENQR";
            public const string DocumentTypeName = "Job Enquiry";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobEnquiry.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobEnquiry.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobEnquiryCancel
        {
            public const int DocumentTypeId = 410;
            public const string DocumentTypeShortName = "JENCL";
            public const string DocumentTypeName = "Job Enquiry Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobEnquiryCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobEnquiryCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobQuotation
        {
            public const int DocumentTypeId = 420;
            public const string DocumentTypeShortName = "JOUOT";
            public const string DocumentTypeName = "Job Quotation";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobQuotation.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobQuotation.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobQuotationCancel
        {
            public const int DocumentTypeId = 430;
            public const string DocumentTypeShortName = "JQTCL";
            public const string DocumentTypeName = "Job Quotation Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobQuotation.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobQuotation.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobOrder
        {
            public const int DocumentTypeId = 440;
            public const string DocumentTypeShortName = "JORDR";
            public const string DocumentTypeName = "Job Order";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobOrder.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobOrder.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobOrderCancel
        {
            public const int DocumentTypeId = 450;
            public const string DocumentTypeShortName = "JOCNL";
            public const string DocumentTypeName = "Job Order Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobOrderCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobOrderCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobOrderAmendment
        {
            public const int DocumentTypeId = 460;
            public const string DocumentTypeShortName = "JOAMD";
            public const string DocumentTypeName = "Job Order Amendment";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobOrderAmendment.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobOrderAmendment.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobReceive
        {
            public const int DocumentTypeId = 470;
            public const string DocumentTypeShortName = "JBREC";
            public const string DocumentTypeName = "Job Receive";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobReceive.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobReceive.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobInspectionRequest
        {
            public const int DocumentTypeId = 480;
            public const string DocumentTypeShortName = "JIREQ";
            public const string DocumentTypeName = "Job Inspection Request";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobReceive.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobReceive.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobInspectionRequestCancel
        {
            public const int DocumentTypeId = 490;
            public const string DocumentTypeShortName = "JIRCL";
            public const string DocumentTypeName = "Job Inspection Request Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobReceive.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobReceive.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobInspection
        {
            public const int DocumentTypeId = 500;
            public const string DocumentTypeShortName = "JBISP";
            public const string DocumentTypeName = "Job Inspection";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobReceive.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobReceive.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobReceiveQC
        {
            public const int DocumentTypeId = 510;
            public const string DocumentTypeShortName = "JRCQC";
            public const string DocumentTypeName = "Job Receive QC";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobReceive.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobReceive.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobReturn
        {
            public const int DocumentTypeId = 520;
            public const string DocumentTypeShortName = "JBRET";
            public const string DocumentTypeName = "Job Return";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobReturn.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobReturn.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobInvoice
        {
            public const int DocumentTypeId = 530;
            public const string DocumentTypeShortName = "JINVC";
            public const string DocumentTypeName = "Job Invoice";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobInvoice.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobInvoice.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JobInvoiceReturn
        {
            public const int DocumentTypeId = 540;
            public const string DocumentTypeShortName = "JIRET";
            public const string DocumentTypeName = "Job Invoice Return";
            public const int DocumentCategoryId = DocumentCategoryConstants.JobInvoiceReturn.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JobInvoiceReturn.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        #endregion
        #region "Inventory"
        public static class StockRequisition
        {
            public const int DocumentTypeId = 550;
            public const string DocumentTypeShortName = "STREQ";
            public const string DocumentTypeName = "Store Requisition";
            public const int DocumentCategoryId = DocumentCategoryConstants.StockRequisition.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.StockRequisition.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class StockRequisitionCancel
        {
            public const int DocumentTypeId = 560;
            public const string DocumentTypeShortName = "STRCN";
            public const string DocumentTypeName = "Store Requisition Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.StockRequisitionCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.StockRequisitionCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class StockIssue
        {
            public const int DocumentTypeId = 570;
            public const string DocumentTypeShortName = "STISS";
            public const string DocumentTypeName = "Store Issue";
            public const int DocumentCategoryId = DocumentCategoryConstants.StockIssue.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.StockIssue.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class StockReceive
        {
            public const int DocumentTypeId = 580;
            public const string DocumentTypeShortName = "STREC";
            public const string DocumentTypeName = "Store Receive";
            public const int DocumentCategoryId = DocumentCategoryConstants.StockReceive.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.StockReceive.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class StockTransfer
        {
            public const int DocumentTypeId = 590;
            public const string DocumentTypeShortName = "STTRF";
            public const string DocumentTypeName = "Store Transfer";
            public const int DocumentCategoryId = DocumentCategoryConstants.StockTransfer.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.StockTransfer.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }

        #endregion
        #region "Planning"
        public static class SaleOrderPlan
        {
            public const int DocumentTypeId = 600;
            public const string DocumentTypeShortName = "SOPLN";
            public const string DocumentTypeName = "Sales Order Plan";
            public const int DocumentCategoryId = DocumentCategoryConstants.SaleOrderPlan.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.SaleOrderPlan.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        #endregion
        #region "Accounts"
        public static class PaymentVoucher
        {
            public const int DocumentTypeId = 610;
            public const string DocumentTypeShortName = "PMTVC";
            public const string DocumentTypeName = "Payment Voucher";
            public const int DocumentCategoryId = DocumentCategoryConstants.PaymentVoucher.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.PaymentVoucher.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class ReceiptVoucher
        {
            public const int DocumentTypeId = 620;
            public const string DocumentTypeShortName = "RCTVC";
            public const string DocumentTypeName = "Receipt Voucher";
            public const int DocumentCategoryId = DocumentCategoryConstants.ReceiptVoucher.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.ReceiptVoucher.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class JournalVoucher
        {
            public const int DocumentTypeId = 630;
            public const string DocumentTypeShortName = "JNRVC";
            public const string DocumentTypeName = "Journal Voucher";
            public const int DocumentCategoryId = DocumentCategoryConstants.JournalVoucher.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.JournalVoucher.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class ContraVoucher
        {
            public const int DocumentTypeId = 640;
            public const string DocumentTypeShortName = "CNTVC";
            public const string DocumentTypeName = "Contra Voucher";
            public const int DocumentCategoryId = DocumentCategoryConstants.ContraVoucher.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.ContraVoucher.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class DebitNote
        {
            public const int DocumentTypeId = 650;
            public const string DocumentTypeShortName = "DNOTE";
            public const string DocumentTypeName = "Debit Note";
            public const int DocumentCategoryId = DocumentCategoryConstants.DebitNote.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.DebitNote.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class CreditNote
        {
            public const int DocumentTypeId = 660;
            public const string DocumentTypeShortName = "CNOTE";
            public const string DocumentTypeName = "Credit Note";
            public const int DocumentCategoryId = DocumentCategoryConstants.CreditNote.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.CreditNote.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class ChequeCancel
        {
            public const int DocumentTypeId = 670;
            public const string DocumentTypeShortName = "CHQCN";
            public const string DocumentTypeName = "Cheque Cancel";
            public const int DocumentCategoryId = DocumentCategoryConstants.ChequeCancel.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.ChequeCancel.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class ExpenseVoucher
        {
            public const int DocumentTypeId = 680;
            public const string DocumentTypeShortName = "EXPVC";
            public const string DocumentTypeName = "Expense Voucher";
            public const int DocumentCategoryId = DocumentCategoryConstants.ExpenseVoucher.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.ExpenseVoucher.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        #endregion
        #region "Masters"
        public static class JobWorker
        {
            public const int DocumentTypeId = 690;
            public const string DocumentTypeShortName = "JWORK";
            public const string DocumentTypeName = "Job Worker";
            public const int DocumentCategoryId = DocumentCategoryConstants.Person.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.Person.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class Customer
        {
            public const int DocumentTypeId = 700;
            public const string DocumentTypeShortName = "CSTMR";
            public const string DocumentTypeName = "Customer";
            public const int DocumentCategoryId = DocumentCategoryConstants.Person.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.Person.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        public static class Supplier
        {
            public const int DocumentTypeId = 710;
            public const string DocumentTypeShortName = "SUPPR";
            public const string DocumentTypeName = "Supplier";
            public const int DocumentCategoryId = DocumentCategoryConstants.Person.DocumentCategoryId;
            public const int DocumentNatureId = DocumentNatureConstants.Person.DocumentNatureId;
            public const string Nature = null;
            public const string PrintTitle = null;
        }
        #endregion
    }
}