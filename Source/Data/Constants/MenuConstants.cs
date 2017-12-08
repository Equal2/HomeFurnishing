using Jobs.Constants.ControllerAction;
using Jobs.Constants.DocumentCategory;
using Jobs.Constants.Module;
using Jobs.Constants.SubModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.Menu
{
    public static class MenuConstants
    {
        #region "Sales Transactions"
        public static class SaleEnquiry
        {
            public const int MenuId = 10;
            public const string MenuName = "Sale Enquiry";
            public const string Srl = "1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Enquiry";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleEnquiry.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleEnquiry.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleEnquiry.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleEnquiryCancel
        {
            public const int MenuId = 20;
            public const string MenuName = "Sale Enquiry Cancel";
            public const string Srl = "1.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Enquiry Cancel";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleEnquiryCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleEnquiryCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleEnquiryCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleQuotation
        {
            public const int MenuId = 30;
            public const string MenuName = "Sale Quotation";
            public const string Srl = "1.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Quotation";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleQuotation.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleQuotation.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleQuotation.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleQuotationCancel
        {
            public const int MenuId = 40;
            public const string MenuName = "Sale Quotation Cancel";
            public const string Srl = "1.15";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Quotation Cancel";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleQuotationCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleQuotationCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleQuotationCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleOrder
        {
            public const int MenuId = 50;
            public const string MenuName = "Sale Order";
            public const string Srl = "1.2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Order";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleOrder.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleOrder.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleOrder.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleOrderCancel
        {
            public const int MenuId = 60;
            public const string MenuName = "Sale Order Cancel";
            public const string Srl = "1.25";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Order Cancel";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleOrderCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleOrderCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleOrderCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleOrderAmendment
        {
            public const int MenuId = 70;
            public const string MenuName = "Sale Order Amendment";
            public const string Srl = "1.3";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Order Amendment";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleOrderAmendment.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleOrderAmendment.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleOrderAmendment.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleInspectionRequest
        {
            public const int MenuId = 80;
            public const string MenuName = "Sale Inspection Request";
            public const string Srl = "1.35";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Inspection Request";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleInspectionRequest.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleInspectionRequest.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleInspectionRequest.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleInspectionRequestCancel
        {
            public const int MenuId = 90;
            public const string MenuName = "Sale Inspection Request Cancel";
            public const string Srl = "1.4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Inspection Request Cancel";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleInspectionRequestCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleInspectionRequestCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleInspectionRequestCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleInspection
        {
            public const int MenuId = 100;
            public const string MenuName = "Sale Inspection";
            public const string Srl = "1.45";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Inspection";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleInspection.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleInspection.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleInspection.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleDeliveryOrder
        {
            public const int MenuId = 110;
            public const string MenuName = "Sale Delivery Order";
            public const string Srl = "1.5";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Delivery Order";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleDeliveryOrder.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleDeliveryOrder.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleDeliveryOrder.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleDeliveryOrderCancel
        {
            public const int MenuId = 120;
            public const string MenuName = "Sale Delivery Order Cancel";
            public const string Srl = "1.55";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Delivery Order Cancel";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleDeliveryOrderCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleDeliveryOrderCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleDeliveryOrderCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class QualityChecking
        {
            public const int MenuId = 130;
            public const string MenuName = "Quality Checking";
            public const string Srl = "1.6";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Quality Checking";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.QualityChecking.ControllerName;
            public const string ActionName = ControllerActionConstants.QualityChecking.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.QualityChecking.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class Packing
        {
            public const int MenuId = 140;
            public const string MenuName = "Packing";
            public const string Srl = "1.65";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Packing";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Packing.ControllerName;
            public const string ActionName = ControllerActionConstants.Packing.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.Packing.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PerformaInvoice
        {
            public const int MenuId = 150;
            public const string MenuName = "Performa Invoice";
            public const string Srl = "1.7";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Performa Invoice";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.PerformaInvoice.ControllerName;
            public const string ActionName = ControllerActionConstants.PerformaInvoice.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PerformaInvoice.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleGoodsDispatch
        {
            public const int MenuId = 160;
            public const string MenuName = "Sale Goods Dispatch";
            public const string Srl = "1.75";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Goods Dispatch";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleGoodsDispatch.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleGoodsDispatch.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleGoodsDispatch.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleInvoice
        {
            public const int MenuId = 170;
            public const string MenuName = "Sale Invoice";
            public const string Srl = "1.8";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Invoice";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleInvoice.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleInvoice.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleInvoice.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class DebitNoteSale
        {
            public const int MenuId = 180;
            public const string MenuName = "Debit Note (Sale)";
            public const string Srl = "1.85";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Debit Note (Sale)";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.DebitNoteSale.ControllerName;
            public const string ActionName = ControllerActionConstants.DebitNoteSale.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.DebitNoteSale.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class CreditNoteSale
        {
            public const int MenuId = 190;
            public const string MenuName = "Credit Note (Sale)";
            public const string Srl = "1.9";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Credit Note (Sale)";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.CreditNoteSale.ControllerName;
            public const string ActionName = ControllerActionConstants.CreditNoteSale.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.CreditNoteSale.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleInvoiceReturn
        {
            public const int MenuId = 200;
            public const string MenuName = "Sale Invoice Return";
            public const string Srl = "1.95";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Invoice Return";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleInvoiceReturn.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleInvoiceReturn.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleInvoiceReturn.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Sales Masters"
        public static class Customer
        {
            public const int MenuId = 210;
            public const string MenuName = "Customer";
            public const string Srl = "2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Customer";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Person.ControllerName;
            public const string ActionName = ControllerActionConstants.Person.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Sales Standard Reports"
        public static class SaleOrderReport
        {
            public const int MenuId = 220;
            public const string MenuName = "Sale Order Report";
            public const string Srl = "3";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Order Report";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.StandardReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleDispatchReport
        {
            public const int MenuId = 230;
            public const string MenuName = "Sale Dispatch Report";
            public const string Srl = "3.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Dispatch Report";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.StandardReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleInvoiceReport
        {
            public const int MenuId = 240;
            public const string MenuName = "Sale Invoice Report";
            public const string Srl = "3.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Invoice Report";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.StandardReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Sales Status Reports"
        public static class SaleEnquiryBalance
        {
            public const int MenuId = 250;
            public const string MenuName = "Sale Enquiry Balance";
            public const string Srl = "4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Enquiry Balance";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleOrderBalance
        {
            public const int MenuId = 260;
            public const string MenuName = "Sale Order Balance";
            public const string Srl = "4.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Order Balance";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SaleDispatchBalance
        {
            public const int MenuId = 270;
            public const string MenuName = "Sale Dispatch Balance";
            public const string Srl = "4.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Dispatch Balance";
            public const int ModuleId = ModuleConstants.Sales.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Purchase Transactions"
        public static class PurchaseIndent
        {
            public const int MenuId = 280;
            public const string MenuName = "Purchase Indent";
            public const string Srl = "1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Plan";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ProductionOrder.ControllerName;
            public const string ActionName = ControllerActionConstants.ProductionOrder.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseIndent.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseIndentCancel
        {
            public const int MenuId = 290;
            public const string MenuName = "Purchase Indent Cancel";
            public const string Srl = "1.03";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Plan Cancel";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ProductionOrderCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.ProductionOrderCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseIndentCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseEnquiry
        {
            public const int MenuId = 300;
            public const string MenuName = "Purchase Enquiry";
            public const string Srl = "1.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Enquiry";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobEnquiry.ControllerName;
            public const string ActionName = ControllerActionConstants.JobEnquiry.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseEnquiry.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseEnquiryCancel
        {
            public const int MenuId = 310;
            public const string MenuName = "Purchase Enquiry Cancel";
            public const string Srl = "1.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Enquiry Cancel";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobEnquiryCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.JobEnquiryCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseEnquiryCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseQuotation
        {
            public const int MenuId = 320;
            public const string MenuName = "Purchase Quotation";
            public const string Srl = "1.15";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Quotation";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobQuotation.ControllerName;
            public const string ActionName = ControllerActionConstants.JobQuotation.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseQuotation.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseQuotationCancel
        {
            public const int MenuId = 330;
            public const string MenuName = "Purchase Quotation Cancel";
            public const string Srl = "1.2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Quotation Cancel";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobQuotationCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.JobQuotationCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseQuotationCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseOrder
        {
            public const int MenuId = 340;
            public const string MenuName = "Purchase Order";
            public const string Srl = "1.25";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Order";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobOrder.ControllerName;
            public const string ActionName = ControllerActionConstants.JobOrder.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseOrder.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseOrderCancel
        {
            public const int MenuId = 350;
            public const string MenuName = "Purchase Order Cancel";
            public const string Srl = "1.3";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Order Cancel";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobOrderCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.JobOrderCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseOrderCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseOrderAmendment
        {
            public const int MenuId = 360;
            public const string MenuName = "Purchase Order Amendment";
            public const string Srl = "1.35";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Order Amendment";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobOrderAmendment.ControllerName;
            public const string ActionName = ControllerActionConstants.JobOrderAmendment.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseOrderAmendment.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseInspectionRequest
        {
            public const int MenuId = 370;
            public const string MenuName = "Purchase Inspection Request";
            public const string Srl = "1.4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Inspection Request";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobInspectionRequest.ControllerName;
            public const string ActionName = ControllerActionConstants.JobInspectionRequest.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseInspectionRequest.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseInspectionRequestCancel
        {
            public const int MenuId = 380;
            public const string MenuName = "Purchase Inspection Request Cancel";
            public const string Srl = "1.45";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Inspection Request Cancellation";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobInspectionRequestCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.JobInspectionRequestCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseInspectionRequestCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseInspection
        {
            public const int MenuId = 390;
            public const string MenuName = "Purchase Inspection";
            public const string Srl = "1.50";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Inspection";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobInspection.ControllerName;
            public const string ActionName = ControllerActionConstants.JobInspection.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseInspection.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseGoodsReceipt
        {
            public const int MenuId = 400;
            public const string MenuName = "Purchase Goods Receipt";
            public const string Srl = "1.55";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Goods Receipt";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobReceive.ControllerName;
            public const string ActionName = ControllerActionConstants.JobReceive.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseGoodsReceipt.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseGoodsReceiptQC
        {
            public const int MenuId = 410;
            public const string MenuName = "Purchase Goods Receipt QC";
            public const string Srl = "1.60";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Goods Receipt QC";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobReceiveQC.ControllerName;
            public const string ActionName = ControllerActionConstants.JobReceiveQC.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseGoodsReceiptQC.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseInvoice
        {
            public const int MenuId = 420;
            public const string MenuName = "Purchase Invoice";
            public const string Srl = "1.65";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Invoice";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobInvoice.ControllerName;
            public const string ActionName = ControllerActionConstants.JobInvoice.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PurchaseInvoice.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class DebitNoteOutward
        {
            public const int MenuId = 430;
            public const string MenuName = "Debit Note Outward";
            public const string Srl = "1.7";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Debit Note Outward";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.DebitNoteOutward.ControllerName;
            public const string ActionName = ControllerActionConstants.DebitNoteOutward.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.DebitNoteOutward.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class CreditNoteOutward
        {
            public const int MenuId = 440;
            public const string MenuName = "Credit Note Outward";
            public const string Srl = "1.75";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Credit Note Outward";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.CreditNoteOutward.ControllerName;
            public const string ActionName = ControllerActionConstants.CreditNoteOutward.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.CreditNoteOutward.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Purchase Masters"
        public static class Supplier
        {
            public const int MenuId = 450;
            public const string MenuName = "Supplier";
            public const string Srl = "2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Supplier";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Person.ControllerName;
            public const string ActionName = ControllerActionConstants.Person.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Purchase Standard Reports"
        public static class PurchaseOrderSummary
        {
            public const int MenuId = 460;
            public const string MenuName = "Purchase Order Summary";
            public const string Srl = "3";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Order Summary";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.StandardReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseGoodsReceiptSummary
        {
            public const int MenuId = 470;
            public const string MenuName = "Purchase Goods Receipt Summary";
            public const string Srl = "3.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Goods Receipt Summary";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.StandardReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class PurchaseInvoiceSummary
        {
            public const int MenuId = 480;
            public const string MenuName = "Purchase Invoice Summary";
            public const string Srl = "3.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Invoice Summary";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.StandardReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Purchase Status Reports"
        public static class PurchaseOrderBalance
        {
            public const int MenuId = 490;
            public const string MenuName = "Purchase Order Balance";
            public const string Srl = "4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Purchase Order Balance";
            public const int ModuleId = ModuleConstants.Purchase.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Production Transactions"
        public static class ProductionOrder
        {
            public const int MenuId = 500;
            public const string MenuName = "Production Order";
            public const string Srl = "1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Production Order";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ProductionOrder.ControllerName;
            public const string ActionName = ControllerActionConstants.ProductionOrder.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.ProductionOrder.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class ProductionOrderCancel
        {
            public const int MenuId = 510;
            public const string MenuName = "Production Order Cancel";
            public const string Srl = "1.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Production Order Cancel";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ProductionOrderCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.ProductionOrderCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.ProductionOrderCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobEnquiry
        {
            public const int MenuId = 520;
            public const string MenuName = "Job Enquiry";
            public const string Srl = "1.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Enquiry";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobEnquiry.ControllerName;
            public const string ActionName = ControllerActionConstants.JobEnquiry.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobEnquiry.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobEnquiryCancel
        {
            public const int MenuId = 530;
            public const string MenuName = "Job Enquiry Cancel";
            public const string Srl = "1.15";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Enquiry Cancel";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobEnquiryCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.JobEnquiryCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobEnquiryCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobQuotation
        {
            public const int MenuId = 540;
            public const string MenuName = "Job Quotation";
            public const string Srl = "1.2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Quotation";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobQuotation.ControllerName;
            public const string ActionName = ControllerActionConstants.JobQuotation.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobQuotation.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobQuotationCancel
        {
            public const int MenuId = 550;
            public const string MenuName = "Job Quotation Cancel";
            public const string Srl = "1.25";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Quotation Cancel";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobQuotationCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.JobQuotationCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobQuotationCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobOrder
        {
            public const int MenuId = 560;
            public const string MenuName = "Job Order";
            public const string Srl = "1.3";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Order";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobOrder.ControllerName;
            public const string ActionName = ControllerActionConstants.JobOrder.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobOrder.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobOrderCancel
        {
            public const int MenuId = 570;
            public const string MenuName = "Job Order Cancel";
            public const string Srl = "1.35";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Order Cancel";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobOrderCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.JobOrderCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobOrderCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobOrderAmendment
        {
            public const int MenuId = 580;
            public const string MenuName = "Job Order Amendment";
            public const string Srl = "1.4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Order Amendment";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobOrderAmendment.ControllerName;
            public const string ActionName = ControllerActionConstants.JobOrderAmendment.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobOrderAmendment.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobInspectionRequest
        {
            public const int MenuId = 590;
            public const string MenuName = "Job Inspection Request";
            public const string Srl = "1.45";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Inspection Request";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobInspectionRequest.ControllerName;
            public const string ActionName = ControllerActionConstants.JobInspectionRequest.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobInspectionRequest.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobInspectionRequestCancellation
        {
            public const int MenuId = 600;
            public const string MenuName = "Job Inspection Request Cancellation";
            public const string Srl = "1.5";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Inspection Request Cancellation";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobInspectionRequestCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.JobInspectionRequestCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobInspectionRequestCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobInspection
        {
            public const int MenuId = 610;
            public const string MenuName = "Job Inspection";
            public const string Srl = "1.55";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Inspection";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobInspection.ControllerName;
            public const string ActionName = ControllerActionConstants.JobInspection.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobInspection.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobGoodsReceipt
        {
            public const int MenuId = 620;
            public const string MenuName = "Job Goods Receipt";
            public const string Srl = "1.6";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Goods Receipt";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobReceive.ControllerName;
            public const string ActionName = ControllerActionConstants.JobReceive.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobReceive.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobGoodsReceiptQC
        {
            public const int MenuId = 630;
            public const string MenuName = "Job Goods Receipt QC";
            public const string Srl = "1.65";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Goods Receipt QC";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobReceiveQC.ControllerName;
            public const string ActionName = ControllerActionConstants.JobReceiveQC.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobReceiveQC.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobInvoice
        {
            public const int MenuId = 640;
            public const string MenuName = "Job Invoice";
            public const string Srl = "1.7";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Invoice";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobInvoice.ControllerName;
            public const string ActionName = ControllerActionConstants.JobInvoice.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobInvoice.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobInvoiceReturn
        {
            public const int MenuId = 650;
            public const string MenuName = "Job Invoice Return";
            public const string Srl = "1.9";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Invoice Return";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JobInvoiceReturn.ControllerName;
            public const string ActionName = ControllerActionConstants.JobInvoiceReturn.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JobInvoiceReturn.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Production Masters"
        public static class JobWorker
        {
            public const int MenuId = 690;
            public const string MenuName = "Job Worker";
            public const string Srl = "2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Worker";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Person.ControllerName;
            public const string ActionName = ControllerActionConstants.Person.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Production Standard Reports"
        public static class JobOrderSummary
        {
            public const int MenuId = 700;
            public const string MenuName = "Job Order Summary";
            public const string Srl = "3";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Order Summary";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.StandardReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobReceiveSummary
        {
            public const int MenuId = 710;
            public const string MenuName = "Job Receive Summary";
            public const string Srl = "3.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Receive Summary";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.StandardReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JobInvoiceSummary
        {
            public const int MenuId = 720;
            public const string MenuName = "Job Invoice Summary";
            public const string Srl = "3.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Invoice Summary";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.StandardReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Production Status Reports"
        public static class JobOrderBalance
        {
            public const int MenuId = 730;
            public const string MenuName = "Job Order Balance";
            public const string Srl = "4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Job Order Balance";
            public const int ModuleId = ModuleConstants.Production.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Inventory Transactions"
        public static class StockIssue
        {
            public const int MenuId = 740;
            public const string MenuName = "Stock Issue";
            public const string Srl = "1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Stock Issue";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.StockIssue.ControllerName;
            public const string ActionName = ControllerActionConstants.StockIssue.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.StockIssue.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class StockReceive
        {
            public const int MenuId = 750;
            public const string MenuName = "Stock Receive";
            public const string Srl = "1.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Stock Receive";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.StockReceive.ControllerName;
            public const string ActionName = ControllerActionConstants.StockReceive.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.StockReceive.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class StockTransfer
        {
            public const int MenuId = 760;
            public const string MenuName = "Stock Transfer";
            public const string Srl = "1.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Stock Transfer";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.StockTransfer.ControllerName;
            public const string ActionName = ControllerActionConstants.StockTransfer.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.StockTransfer.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class StockExchange
        {
            public const int MenuId = 770;
            public const string MenuName = "Stock Exchange";
            public const string Srl = "1.15";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Stock Exchange";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.StockExchange.ControllerName;
            public const string ActionName = ControllerActionConstants.StockExchange.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.StockExchange.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class GatePass
        {
            public const int MenuId = 780;
            public const string MenuName = "Gate Pass";
            public const string Srl = "1.2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Gate Pass";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.GatePass.ControllerName;
            public const string ActionName = ControllerActionConstants.GatePass.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.GatePass.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class StockReconciliation
        {
            public const int MenuId = 790;
            public const string MenuName = "Stock Reconciliation";
            public const string Srl = "1.25";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Stock Reconciliation";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.StockReconciliation.ControllerName;
            public const string ActionName = ControllerActionConstants.StockReconciliation.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Inventory Masters"
        public static class Product
        {
            public const int MenuId = 800;
            public const string MenuName = "Product";
            public const string Srl = "2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Product";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Product.ControllerName;
            public const string ActionName = ControllerActionConstants.Product.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class ProductGroup
        {
            public const int MenuId = 810;
            public const string MenuName = "Product Group";
            public const string Srl = "2.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Product Group";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ProductGroup.ControllerName;
            public const string ActionName = ControllerActionConstants.ProductGroup.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class ProductCategory
        {
            public const int MenuId = 820;
            public const string MenuName = "Product Category";
            public const string Srl = "2.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Product Category";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ProductCategory.ControllerName;
            public const string ActionName = ControllerActionConstants.ProductCategory.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class ProductType
        {
            public const int MenuId = 830;
            public const string MenuName = "Product Type";
            public const string Srl = "2.15";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Product Type";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ProductType.ControllerName;
            public const string ActionName = ControllerActionConstants.ProductType.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class ProductCustomGroup
        {
            public const int MenuId = 840;
            public const string MenuName = "Product Custom Group";
            public const string Srl = "2.2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Product Custom Group";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ProductCustomGroup.ControllerName;
            public const string ActionName = ControllerActionConstants.ProductCustomGroup.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class Godown
        {
            public const int MenuId = 850;
            public const string MenuName = "Godown";
            public const string Srl = "2.25";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Godown";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Godown.ControllerName;
            public const string ActionName = ControllerActionConstants.Godown.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class HSNCode
        {
            public const int MenuId = 860;
            public const string MenuName = "HSN Code";
            public const string Srl = "2.3";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "HSN Code";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SalesTaxProductCode.ControllerName;
            public const string ActionName = ControllerActionConstants.SalesTaxProductCode.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class Gate
        {
            public const int MenuId = 870;
            public const string MenuName = "Gate";
            public const string Srl = "2.35";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Gate";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Gate.ControllerName;
            public const string ActionName = ControllerActionConstants.Gate.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class City
        {
            public const int MenuId = 880;
            public const string MenuName = "City";
            public const string Srl = "2.4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "City";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.City.ControllerName;
            public const string ActionName = ControllerActionConstants.City.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class State
        {
            public const int MenuId = 890;
            public const string MenuName = "State";
            public const string Srl = "2.45";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "State";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.State.ControllerName;
            public const string ActionName = ControllerActionConstants.State.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class Country
        {
            public const int MenuId = 900;
            public const string MenuName = "Country";
            public const string Srl = "2.5";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Country";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Country.ControllerName;
            public const string ActionName = ControllerActionConstants.Country.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Inventory Standard Reports"
        #endregion
        #region "Inventory Status Reports"
        public static class StockInHand
        {
            public const int MenuId = 920;
            public const string MenuName = "Stock In Hand";
            public const string Srl = "4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Stock In Hand";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class StockInProcess
        {
            public const int MenuId = 930;
            public const string MenuName = "Stock In Process";
            public const string Srl = "4.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Stock In Process";
            public const int ModuleId = ModuleConstants.Inventory.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Human Resource Transactions"
        #endregion
        #region "Human Resource Masters"
        public static class Employee
        {
            public const int MenuId = 950;
            public const string MenuName = "Employee";
            public const string Srl = "2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Employee";
            public const int ModuleId = ModuleConstants.HumanResource.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Employee.ControllerName;
            public const string ActionName = ControllerActionConstants.Employee.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Human Resource Standard Reports"
        #endregion
        #region "Human Resource Status Reports"
        #endregion
        #region "Accounts Transactions"
        public static class PaymentVoucher
        {
            public const int MenuId = 980;
            public const string MenuName = "Payment Voucher";
            public const string Srl = "1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Payment Voucher";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.PaymentVoucher.ControllerName;
            public const string ActionName = ControllerActionConstants.PaymentVoucher.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.PaymentVoucher.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class ReceiptVoucher
        {
            public const int MenuId = 990;
            public const string MenuName = "Receipt Voucher";
            public const string Srl = "1.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Receipt Voucher";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ReceiptVoucher.ControllerName;
            public const string ActionName = ControllerActionConstants.ReceiptVoucher.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.ReceiptVoucher.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class JournalVoucher
        {
            public const int MenuId = 1000;
            public const string MenuName = "Journal Voucher";
            public const string Srl = "1.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Journal Voucher";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.JournalVoucher.ControllerName;
            public const string ActionName = ControllerActionConstants.JournalVoucher.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.JournalVoucher.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class ContraVoucher
        {
            public const int MenuId = 1010;
            public const string MenuName = "Contra Voucher";
            public const string Srl = "1.15";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Contra Voucher";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ContraVoucher.ControllerName;
            public const string ActionName = ControllerActionConstants.ContraVoucher.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.ContraVoucher.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class DebitNote
        {
            public const int MenuId = 1020;
            public const string MenuName = "Debit Note";
            public const string Srl = "1.2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Debit Note";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.DebitNote.ControllerName;
            public const string ActionName = ControllerActionConstants.DebitNote.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.DebitNote.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class ChequeCancel
        {
            public const int MenuId = 1030;
            public const string MenuName = "Cheque Cancel";
            public const string Srl = "1.25";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Cheque Cancel";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ChequeCancel.ControllerName;
            public const string ActionName = ControllerActionConstants.ChequeCancel.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.ChequeCancel.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class ExpenseVoucher
        {
            public const int MenuId = 1040;
            public const string MenuName = "Expense Voucher";
            public const string Srl = "1.3";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Expense Voucher";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ExpenseVoucher.ControllerName;
            public const string ActionName = ControllerActionConstants.ExpenseVoucher.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.ExpenseVoucher.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class CreditNote
        {
            public const int MenuId = 1050;
            public const string MenuName = "Credit Note";
            public const string Srl = "1.35";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Credit Note";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.CreditNote.ControllerName;
            public const string ActionName = ControllerActionConstants.CreditNote.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.CreditNote.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class BankReconciliation
        {
            public const int MenuId = 1060;
            public const string MenuName = "Bank Reconciliation";
            public const string Srl = "1.4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Bank Reconciliation";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.BankReconciliation.ControllerName;
            public const string ActionName = ControllerActionConstants.BankReconciliation.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Accounts Masters"
        public static class LedgerAccount
        {
            public const int MenuId = 1070;
            public const string MenuName = "Ledger Account";
            public const string Srl = "2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Ledger Account";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.LedgerAccount.ControllerName;
            public const string ActionName = ControllerActionConstants.LedgerAccount.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class LedgerAccountGroup
        {
            public const int MenuId = 1080;
            public const string MenuName = "Ledger Account Group";
            public const string Srl = "2.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Ledger Account Group";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.LedgerAccountGroup.ControllerName;
            public const string ActionName = ControllerActionConstants.LedgerAccountGroup.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class CostCenter
        {
            public const int MenuId = 1090;
            public const string MenuName = "Cost Center";
            public const string Srl = "2.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Cost Center";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.CostCenter.ControllerName;
            public const string ActionName = ControllerActionConstants.CostCenter.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class TdsCategory
        {
            public const int MenuId = 1100;
            public const string MenuName = "Tds Category";
            public const string Srl = "2.15";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Tds Category";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.TdsCategory.ControllerName;
            public const string ActionName = ControllerActionConstants.TdsCategory.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class TdsGroup
        {
            public const int MenuId = 1110;
            public const string MenuName = "Tds Group";
            public const string Srl = "2.2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Tds Group";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.TdsGroup.ControllerName;
            public const string ActionName = ControllerActionConstants.TdsGroup.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Accounts Standard Reports"
        #endregion
        #region "Accounts Status Reports"
        public static class TrialBalance
        {
            public const int MenuId = 1130;
            public const string MenuName = "Trial Balance";
            public const string Srl = "4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Trial Balance";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class SubTrialBalance
        {
            public const int MenuId = 1140;
            public const string MenuName = "Sub Trial Balance";
            public const string Srl = "4.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sub Trial Balance";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class BalanceSheet
        {
            public const int MenuId = 1150;
            public const string MenuName = "Balance Sheet";
            public const string Srl = "4.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Balance Sheet";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class ProfitAndLossAc
        {
            public const int MenuId = 1160;
            public const string MenuName = "Profit & Loss A/c";
            public const string Srl = "4.15";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Profit & Loss A/c";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class CashBook
        {
            public const int MenuId = 1170;
            public const string MenuName = "Cash Book";
            public const string Srl = "4.2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Cash Book";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class BankBook
        {
            public const int MenuId = 1180;
            public const string MenuName = "Bank Book";
            public const string Srl = "4.25";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Bank Book";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class DayBook
        {
            public const int MenuId = 1190;
            public const string MenuName = "Day Book";
            public const string Srl = "4.3";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Day Book";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class DebtorsAgeingAnalysisFIFO
        {
            public const int MenuId = 1200;
            public const string MenuName = "Debtors Ageing Analysis FIFO";
            public const string Srl = "4.35";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Debtors Ageing Analysis FIFO";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class DebtorsOutstandingFIFO
        {
            public const int MenuId = 1210;
            public const string MenuName = "Debtors Outstanding FIFO";
            public const string Srl = "4.4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Debtors Outstanding FIFO";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class CreditorsAgeingAnalysisFIFO
        {
            public const int MenuId = 1220;
            public const string MenuName = "Creditors Ageing Analysis FIFO";
            public const string Srl = "4.45";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Creditors Ageing Analysis FIFO";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class CreditorsOutstandingFIFO
        {
            public const int MenuId = 1230;
            public const string MenuName = "Creditors Outstanding FIFO";
            public const string Srl = "4.5";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Creditors Outstanding FIFO";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class TDSAdvise
        {
            public const int MenuId = 1240;
            public const string MenuName = "TDS Advise";
            public const string Srl = "4.55";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "TDS Advise";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class InputTaxReport
        {
            public const int MenuId = 1250;
            public const string MenuName = "Input Tax Report";
            public const string Srl = "4.6";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Input Tax Report";
            public const int ModuleId = ModuleConstants.Accounts.ModuleId;
            public const int SubModuleId = SubModuleConstants.StatusReports.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Planning Transactions"
        public static class SalesOrderPlan
        {
            public const int MenuId = 1260;
            public const string MenuName = "Sales Order Plan";
            public const string Srl = "1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sales Order Plan";
            public const int ModuleId = ModuleConstants.Planning.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.SaleOrderPlan.ControllerName;
            public const string ActionName = ControllerActionConstants.SaleOrderPlan.ActionName;
            public static readonly string RouteId = DocumentCategoryConstants.SaleOrderPlan.DocumentCategoryId.ToString();
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Planning Masters"
        #endregion
        #region "Planning Standard Reports"
        #endregion
        #region "Planning Status Reports"
        public static class SaleOrderPlanStatus
        {
            public const int MenuId = 1310;
            public const string MenuName = "Sale Order Plan Status";
            public const string Srl = "4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Sale Order Plan Status";
            public const int ModuleId = ModuleConstants.Planning.ModuleId;
            public const int SubModuleId = SubModuleConstants.Documents.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Report.ControllerName;
            public const string ActionName = ControllerActionConstants.Report.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Admin Setup Transactions"
        #endregion
        #region "Admin Setup Masters"
        public static class ChargeGroupSettings
        {
            public const int MenuId = 1330;
            public const string MenuName = "Charge Group Settings";
            public const string Srl = "2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Charge Group Settings";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.ChargeGroupSettings.ControllerName;
            public const string ActionName = ControllerActionConstants.ChargeGroupSettings.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class DocumentCategory
        {
            public const int MenuId = 1340;
            public const string MenuName = "Document Category";
            public const string Srl = "2.05";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Document Category";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.DocumentCategory.ControllerName;
            public const string ActionName = ControllerActionConstants.DocumentCategory.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class DocumentType
        {
            public const int MenuId = 1350;
            public const string MenuName = "Document Type";
            public const string Srl = "2.1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Document Type";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.DocumentType.ControllerName;
            public const string ActionName = ControllerActionConstants.DocumentType.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class Site
        {
            public const int MenuId = 1360;
            public const string MenuName = "Site";
            public const string Srl = "2.15";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Site";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Site.ControllerName;
            public const string ActionName = ControllerActionConstants.Site.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class Division
        {
            public const int MenuId = 1370;
            public const string MenuName = "Division";
            public const string Srl = "2.2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Division";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Division.ControllerName;
            public const string ActionName = ControllerActionConstants.Division.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class Calculation
        {
            public const int MenuId = 1380;
            public const string MenuName = "Calculation";
            public const string Srl = "2.25";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Calculation";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Calculation.ControllerName;
            public const string ActionName = ControllerActionConstants.Calculation.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class CalculationLedgerAccounts
        {
            public const int MenuId = 1390;
            public const string MenuName = "Calculation Ledger Accounts";
            public const string Srl = "2.3";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Calculation Ledger Accounts";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.CalculationLedgerAccounts.ControllerName;
            public const string ActionName = ControllerActionConstants.CalculationLedgerAccounts.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class Charge
        {
            public const int MenuId = 1400;
            public const string MenuName = "Charge";
            public const string Srl = "2.35";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Charge";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Charge.ControllerName;
            public const string ActionName = ControllerActionConstants.Charge.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class AssignPermissions
        {
            public const int MenuId = 1410;
            public const string MenuName = "Assign Permissions";
            public const string Srl = "2.4";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Assign Permissions";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.AssignPermissions.ControllerName;
            public const string ActionName = ControllerActionConstants.AssignPermissions.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class CreateRoles
        {
            public const int MenuId = 1420;
            public const string MenuName = "Create Roles";
            public const string Srl = "2.45";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Create Roles";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.CreateRoles.ControllerName;
            public const string ActionName = ControllerActionConstants.CreateRoles.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class Reason
        {
            public const int MenuId = 1430;
            public const string MenuName = "Reason";
            public const string Srl = "2.5";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Reason";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Reason.ControllerName;
            public const string ActionName = ControllerActionConstants.Reason.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class AssignNewRoles
        {
            public const int MenuId = 1440;
            public const string MenuName = "Assign New Roles";
            public const string Srl = "2.55";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Assign New Roles";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.AssignNewRoles.ControllerName;
            public const string ActionName = ControllerActionConstants.AssignNewRoles.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class AssignTemporaryRoles
        {
            public const int MenuId = 1450;
            public const string MenuName = "Assign Temporary Roles";
            public const string Srl = "2.6";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Assign Temporary Roles";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.AssignTemporaryRoles.ControllerName;
            public const string ActionName = ControllerActionConstants.AssignTemporaryRoles.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class UpdateTableStructure
        {
            public const int MenuId = 1460;
            public const string MenuName = "Update Table Structure";
            public const string Srl = "2.65";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Update Table Structure";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.UpdateTableStructure.ControllerName;
            public const string ActionName = ControllerActionConstants.UpdateTableStructure.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }

        public static class UserInvitation
        {
            public const int MenuId = 1470;
            public const string MenuName = "User Invitation";
            public const string Srl = "2.7";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "User Invitation";
            public const int ModuleId = ModuleConstants.AdminSetup.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.UserInvitation.ControllerName;
            public const string ActionName = ControllerActionConstants.UserInvitation.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Admin Setup Standard Reports"
        #endregion
        #region "Admin Setup Status Reports"
        #endregion
        #region "Task Management Transactions"
        public static class DAR
        {
            public const int MenuId = 1500;
            public const string MenuName = "DAR";
            public const string Srl = "1";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "DAR";
            public const int ModuleId = ModuleConstants.Task.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.DAR.ControllerName;
            public const string ActionName = ControllerActionConstants.DAR.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Task Management Masters"
        public static class Tasks
        {
            public const int MenuId = 1510;
            public const string MenuName = "Tasks";
            public const string Srl = "2";
            public const string IconName = "glyphicon glyphicon-book";
            public const string Description = "Tasks";
            public const int ModuleId = ModuleConstants.Task.ModuleId;
            public const int SubModuleId = SubModuleConstants.Setup.SubModuleId;
            public const string ControllerName = ControllerActionConstants.Tasks.ControllerName;
            public const string ActionName = ControllerActionConstants.Tasks.ActionName;
            public static readonly string RouteId = null;
            public const string URL = "JobsDomain";
            public const bool IsVisible = true;
            public const string AreaName = null;
        }
        #endregion
        #region "Task Management Standard Reports"
        #endregion
        #region "Task Management Status Reports"
        #endregion
    }
}