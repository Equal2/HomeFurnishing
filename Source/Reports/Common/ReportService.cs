using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using Reports.ViewModels;
using Service;
using Data.Infrastructure;
using Data.Models;
using Model.Models;
using Core.Common;
using System.Configuration;
using System.Text;
using System.ComponentModel;
using Microsoft.Reporting.WebForms;
using ReportViewerForMvc.Example.Reports;

namespace Service
{
    public class ReportService  : Controller
    {
        public void SetReportAttibutes(ReportViewer reportViewer, ReportDataSource reportdatasource, string RepTitle, string RepSubtitle)
        {
            reportViewer.ProcessingMode = ProcessingMode.Local;
            reportViewer.SizeToReportContent = true;
            reportViewer.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            reportViewer.Height = System.Web.UI.WebControls.Unit.Percentage(100);
            reportViewer.LocalReport.DataSources.Add(reportdatasource);

            reportViewer.LocalReport.SetParameters(new ReportParameter("ReportTitle", RepTitle));
            reportViewer.LocalReport.SetParameters(new ReportParameter("ReportSubtitle", RepSubtitle));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyName", (string)System.Web.HttpContext.Current.Session[SessionNameConstants.CompanyName]));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyAddress", (string)System.Web.HttpContext.Current.Session[SessionNameConstants.SiteAddress]));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyCity", (string)System.Web.HttpContext.Current.Session[SessionNameConstants.SiteCityName]));
            reportViewer.LocalReport.SetParameters(new ReportParameter("SiteName", (string)System.Web.HttpContext.Current.Session[SessionNameConstants.SiteName]));
            reportViewer.LocalReport.SetParameters(new ReportParameter("DivisionName", (string)System.Web.HttpContext.Current.Session[SessionNameConstants.DivisionName]));

            reportViewer.HyperlinkTarget = "_blank";
            reportViewer.LocalReport.EnableHyperlinks = true;
       }


        public ActionResult PrintReport(ReportViewer reportViewer, string reportformat = "PDF")
        {
            string mimeType;
            string encoding;
            string fileNameExtension;

            string deviceinfo =
                "<DeviceInfo>" +
                "   <OutputFormat>" + reportformat + "</OutputFormat>" +
                "</DeviceInfo>";

            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;

            reportViewer.HyperlinkTarget = "_blank";
            reportViewer.LocalReport.EnableHyperlinks = true;
            renderedBytes = reportViewer.LocalReport.Render(
                    reportformat,
                    deviceinfo,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings);

            return File(renderedBytes, mimeType);
        }
    }    
}
