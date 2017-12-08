using Lib.Web.Mvc;
using Microsoft.Owin.Security;
using Surya.India.Data.Infrastructure;
using Surya.India.Data.Models;
using Surya.Reports.Presentation.Helper;
using Surya.India.Model.ViewModels;
using Surya.India.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Surya.India.Model.Models;
using Surya.India.Reports.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Text;

namespace Surya.India.Reports.Controllers
{
    [Authorize]
    public class Report_SaleOrderStatusSummaryController : ReportController
    {

        public Report_SaleOrderStatusSummaryController(IUnitOfWork unitOfWork)
        {              
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult SaleOrderStatusSummary()
        {
          
            return View();
        }

        public void SaleOrderStatusSummary(FormCollection form)
        {
            DataTable Dt = new DataTable();
            StringBuilder queryString = new StringBuilder();
            string ReportTitle = "";

            string ReportSummaryType = (form["ReportSummaryType"].ToString());
            string FromDate = (form["FromDate"].ToString());
            string ToDate = (form["ToDate"].ToString());
            string StatusOnDate = (form["StatusOnDate"].ToString());
            string SiteId = (form["Site"].ToString());
            string Site = (form["SiteNames"].ToString());
            string DivisionId = (form["Division"].ToString());
            string Division = (form["DivisionNames"].ToString());
            string DocumentTypeId = (form["DocumentType"].ToString());
            string DocumentType = (form["DocumentTypeNames"].ToString());
            string BuyerId = (form["Buyer"].ToString());
            string Buyer = (form["BuyerNames"].ToString());
            string CurrencyId = (form["Currency"].ToString());
            string Currency = (form["CurrencyNames"].ToString());
            string ProductNatureId = (form["ProductNature"].ToString());
            string ProductNature = (form["ProductNatureNames"].ToString());
            string ProductCategoryId = (form["ProductCategory"].ToString());
            string ProductCategory = (form["ProductCategoryNames"].ToString());
            string ProductTypeId = (form["ProductType"].ToString());
            string ProductType = (form["ProductTypeNames"].ToString());
            string ProductCollectionId = (form["ProductCollection"].ToString());
            string ProductCollection = (form["ProductCollectionNames"].ToString());
            string ProductQualityId = (form["ProductQuality"].ToString());
            string ProductQuality = (form["ProductQualityNames"].ToString());
            string ProductGroupId = (form["ProductGroup"].ToString());
            string ProductGroup = (form["ProductGroupNames"].ToString());
            string ProductStyleId = (form["ProductStyle"].ToString());
            string ProductStyle = (form["ProductStyleNames"].ToString());
            string ProductDesignId = (form["ProductDesign"].ToString());
            string ProductDesign = (form["ProductDesignNames"].ToString());
            string ProductShapeId = (form["ProductShape"].ToString());
            string ProductShape = (form["ProductShapeNames"].ToString());
            string ProductSizeId = (form["ProductSize"].ToString());
            string ProductSize = (form["ProductSizeNames"].ToString());
            string ProductInvoiceGroupId = (form["ProductInvoiceGroup"].ToString());
            string ProductInvoiceGroup = (form["ProductInvoiceGroupNames"].ToString());
            string ProductCustomGroupId = (form["ProductCustomGroup"].ToString());
            string ProductCustomGroup = (form["ProductCustomGroupNames"].ToString());
            string ProductTagId = (form["ProductTag"].ToString());
            string ProductTag = (form["ProductTagNames"].ToString());
            string ProductId = (form["Product"].ToString());
            string Product = (form["ProductNames"].ToString());
            string SaleOrderId = (form["SaleOrder"].ToString());
            string SaleOrder = (form["SaleOrderNames"].ToString());


            queryString.Append("ProcSaleOrderStatusSummary ");

            if (ReportSummaryType != null)
            {
                
                if (ReportSummaryType == "1")
                {
                    queryString.Append(" 'Month', "); 
                    ReportTitle = "Month Wise Sale Order Status Summary"; // SubTitle = "Month Wise Summary";
                }
                else if (ReportSummaryType == "2")
                {
                    queryString.Append(" 'Buyer', ");
                    ReportTitle = "Buyer Wise Sale Order Status Summary";// SubTitle = "Buyer Wise Summary"; 
                }
                else if (ReportSummaryType == "3")
                {
                    queryString.Append(" 'Product', ");
                    ReportTitle = "Product Wise Sale Order Status Summary";//SubTitle = "Buyer Wise Summary";
                }
                else if (ReportSummaryType == "4")
                {
                    queryString.Append(" 'ProductGroup', ");
                    ReportTitle = "Product Group Wise Sale Order Status Summary";//SubTitle = "Product Wise Summary";
                }
                else if (ReportSummaryType == "5")
                {
                    queryString.Append(" 'ProductCategory', ");
                    ReportTitle = "Product Category Wise Sale Order Status Summary";//SubTitle = "Product Group Wise Summary";
                }
                else if (ReportSummaryType == "6")
                {
                    queryString.Append(" 'ProductType', ");
                    ReportTitle = "Product Type Wise Sale Order Status Summary";// SubTitle = "Product Category Wise Summary";
                }
                
            }
            else queryString.Append("NULL, ");

            if (SiteId != "") queryString.Append(" '" + SiteId + "', "); else queryString.Append("NULL, ");
            if (DivisionId != "") queryString.Append(" '" + DivisionId + "', "); else queryString.Append("NULL, ");
            if (!string.IsNullOrEmpty(FromDate)) queryString.Append(" '" + String.Format("{0:MMMM dd yyyy}", FromDate) + "', "); else queryString.Append("NULL, ");
            if (!string.IsNullOrEmpty(ToDate)) queryString.Append(" '" + String.Format("{0:MMMM dd yyyy}", ToDate) + "', "); else queryString.Append("NULL, ");
            if (!string.IsNullOrEmpty(StatusOnDate)) queryString.Append(" '" + String.Format("{0:MMMM dd yyyy}", StatusOnDate) + "', "); else queryString.Append("NULL, ");
            if (DocumentTypeId != "") queryString.Append(" '" + DocumentTypeId + "', "); else queryString.Append("NULL, ");
            if (BuyerId != "") queryString.Append(" '" + BuyerId + "', "); else queryString.Append("NULL, ");
            if (CurrencyId != "") queryString.Append(" '" + CurrencyId + "', "); else queryString.Append("NULL, ");
            if (ProductNatureId != "") queryString.Append(" '" + ProductNatureId + "', "); else queryString.Append("NULL, ");
            if (ProductCategoryId != "") queryString.Append(" '" + ProductCategoryId + "', "); else queryString.Append("NULL, ");
            if (ProductTypeId != "") queryString.Append(" '" + ProductTypeId + "', "); else queryString.Append("NULL, ");
            if (ProductCollectionId != "") queryString.Append(" '" + ProductCollectionId + "', "); else queryString.Append("NULL, ");
            if (ProductQualityId != "") queryString.Append(" '" + ProductQualityId + "', "); else queryString.Append("NULL, ");
            if (ProductGroupId != "") queryString.Append(" '" + ProductGroupId + "', "); else queryString.Append("NULL, ");
            if (ProductStyleId != "") queryString.Append(" '" + ProductStyleId + "', "); else queryString.Append("NULL, ");
            if (ProductDesignId != "") queryString.Append(" '" + ProductDesignId + "', "); else queryString.Append("NULL, ");
            if (ProductShapeId != "") queryString.Append(" '" + ProductShapeId + "', "); else queryString.Append("NULL, ");
            if (ProductSizeId != "") queryString.Append(" '" + ProductSizeId + "', "); else queryString.Append("NULL, ");
            if (ProductInvoiceGroupId != "") queryString.Append(" '" + ProductInvoiceGroupId + "', "); else queryString.Append("NULL, ");
            if (ProductCustomGroupId != "") queryString.Append(" '" + ProductCustomGroupId + "', "); else queryString.Append("NULL, ");
            if (ProductTagId != "") queryString.Append(" '" + ProductTagId + "', "); else queryString.Append("NULL, ");
            if (ProductId != "") queryString.Append(" '" + ProductId + "', "); else queryString.Append("NULL, ");
            if (SaleOrderId != "") queryString.Append(" '" + SaleOrderId + "', "); else queryString.Append("NULL, ");


            if (form["ReportFor"].ToString() == "2")
            {
                queryString.Append(" '" + form["ReportFor"].ToString() + "' ");
            }
            else
            {
                queryString.Append("NULL ");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlDataAdapter sqlDataAapter = new SqlDataAdapter(queryString.ToString(), sqlConnection);
                dsRep.EnforceConstraints = false;
                sqlDataAapter.Fill(Dt);
            }
            reportdatasource = new ReportDataSource("DsMain", Dt);
            reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleOrderStatusSummary.rdlc");
            
            ReportService reportservice = new ReportService();
            reportservice.SetReportAttibutes(reportViewer, reportdatasource, ReportTitle, "");

            string FilterStr = "FilterStr";
            int i = 1;

            if (!string.IsNullOrEmpty(FromDate.ToString())) { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Date From : " + String.Format("{0:MMMM-dd-yyyy}", FromDate))); i++; }
            if (!string.IsNullOrEmpty(ToDate.ToString())) { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Date To : " + String.Format("{0:MMMM-dd-yyyy}", ToDate))); i++; }
            if (!string.IsNullOrEmpty(StatusOnDate.ToString())) { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Status On Date : " + String.Format("{0:MMMM-dd-yyyy}", StatusOnDate))); i++; }
            if (SiteId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Site : " + Site)); i++; }
            if (DivisionId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Division : " + Division)); i++; }
            if (DocumentTypeId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Document Type : " + DocumentType)); i++; }
            if (BuyerId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Buyer : " + Buyer)); i++; }
            if (CurrencyId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Currency : " + Currency)); i++; }
            if (ProductNatureId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Nature : " + ProductNature)); i++; }
            if (ProductCategoryId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Category : " + ProductCategory)); i++; }
            if (ProductTypeId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Type : " + ProductType)); i++; }
            if (ProductCollectionId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Collection : " + ProductCollection)); i++; }
            if (ProductQualityId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Quality : " + ProductQuality)); i++; }
            if (ProductGroupId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Group : " + ProductGroup)); i++; }
            if (ProductStyleId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Style : " + ProductStyle)); i++; }
            if (ProductDesignId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Design : " + ProductDesign)); i++; }
            if (ProductShapeId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Shape : " + ProductShape)); i++; }
            if (ProductSizeId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Size : " + ProductSize)); i++; }
            if (ProductInvoiceGroupId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Invoice Group : " + ProductInvoiceGroup)); i++; }
            if (ProductCustomGroupId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Invoice Group : " + ProductCustomGroup)); i++; }
            if (ProductTagId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product Tag : " + ProductTag)); i++; }
            if (ProductId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Product : " + Product)); i++; }
            if (SaleOrderId != "") { reportViewer.LocalReport.SetParameters(new ReportParameter(FilterStr + i.ToString(), "Sale Order : " + SaleOrder)); i++; }
        }
        


        [HttpPost]
        [MultipleButton(Name = "Print", Argument = "PDF")]
        public ActionResult PrintToPDF(FormCollection form)
        {
            SaleOrderStatusSummary(form);
            return PrintReport(reportViewer, "PDF");
        }

        [HttpPost]
        [MultipleButton(Name = "Print", Argument = "Excel")]
        public ActionResult PrintToExcel(FormCollection form)
        {
            SaleOrderStatusSummary(form);
            return PrintReport(reportViewer, "Excel");
        }
    }
}