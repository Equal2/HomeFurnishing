using Microsoft.Owin.Security;
using Data.Infrastructure;
using Data.Models;
using Reports.Presentation.Helper;
using Model.ViewModels;
using Reports.ViewModels;
using Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using ReportViewerForMvc.Example.Reports;
using Reports.Reports;
using Model.Models;
using Core.Common;

namespace Reports.Controllers
{

    [Authorize]
    public class ReportController : Controller
    {
        protected string RepName = "";
        protected string RepTitle = "";
        protected string RepSubtitle = "";
        protected string RepPath = "";
        protected ReportDataSource reportdatasource;
        protected dsLocalReport tds = new dsLocalReport();
        protected dsReport dsRep = new dsReport();
        protected ApplicationDbContext db = new ApplicationDbContext();
        protected string connectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
        protected ReportViewer reportViewer = new ReportViewer();
        protected IUnitOfWork _unitOfWork;



        protected void SetReportAttibutes()
        {  

            reportViewer.ProcessingMode = ProcessingMode.Local;
            reportViewer.SizeToReportContent = true;
            reportViewer.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            reportViewer.Height = System.Web.UI.WebControls.Unit.Percentage(100);
            reportViewer.LocalReport.ReportPath = RepPath;
            reportViewer.LocalReport.DataSources.Add(reportdatasource);
            reportViewer.LocalReport.SetParameters(new ReportParameter("ReportTitle", RepTitle));
            reportViewer.LocalReport.SetParameters(new ReportParameter("ReportSubtitle", RepSubtitle));
            reportViewer.LocalReport.SetParameters(new ReportParameter("SiteName", (string)System.Web.HttpContext.Current.Session[SessionNameConstants.SiteName]));
            reportViewer.LocalReport.SetParameters(new ReportParameter("DivisionName", (string)System.Web.HttpContext.Current.Session[SessionNameConstants.DivisionName ]));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyName", (string)System.Web.HttpContext.Current.Session[SessionNameConstants.CompanyName]));
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


            renderedBytes = reportViewer.LocalReport.Render(
                    reportformat,
                    deviceinfo,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings);

            ViewBag.File = File(renderedBytes, mimeType);
            return File(renderedBytes, mimeType);

        }
    }
}