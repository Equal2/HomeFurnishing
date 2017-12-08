using Data.Infrastructure;
using Service;
using System.Configuration;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using System.Data.SqlClient;
using System.Data;
using Reports.Controllers;
using Jobs.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class Report_PackingQRBarCodePrintController : ReportController
    {

        public Report_PackingQRBarCodePrintController(IUnitOfWork unitOfWork)
        {              
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult PackingQRBarCodePrint()
        {

            return RedirectToAction("ReportLayout", "ReportLayout", new { name = "Packing QR Bar Code Print" });
        }
        
        public void PackingQRBarCodePrint(FormCollection form)
        {
            Report_PackingPrintController P = new Report_PackingPrintController(_unitOfWork);
            DataTable Dt = new DataTable();
            string PrintReportTypeId = (form["PackingReportType"].ToString());

            string PrintReportType = (form["ReportType1"].ToString());
            string PackingId = (form["Packing"].ToString());
            string Packing = (form["PackingNames"].ToString());
            string FromRollNo = (form["FromRollNo"].ToString());
            string ToRollNo = (form["ToRollNo"].ToString());


            string mQry, bConStr = "";
            DataTable DtTemp = new DataTable();

            //PackingId = "24";

            if (PackingId != "") { bConStr = " AND H.PackingHeaderId In ( " + PackingId + " )"; }
            if (FromRollNo != "" && ToRollNo != "")
            { bConStr = bConStr + " AND replace(L.BaleNo,'-','.')  Between   replace('" + FromRollNo + "','-','.') And  replace('" + ToRollNo + "','-','.') "+" "; }


            mQry = "SELECT P.ProductName AS CarpetSKU, SOH.DocNo AS SaleOrder , SOH.SaleOrderHeaderId, " +
                    " ISNULL(PB.BuyerUpcCode,'') AS UpcCode, isnumeric(PB.BuyerUpcCode) AS BuyerUpcCodeNumeric, L.ProductId, L.Qty, L.PartyProductUid,  " +
                    " PU.ProductUidName, L.BaleNo, SOH.SaleToBuyerId, PB.BuyerSku " +
                    " FROM Web.PackingHeaders H " +
                    " LEFT JOIN Web.PackingLines L ON L.PackingHeaderId = H.PackingHeaderId  " +
                    " LEFT JOIN Web.ProductUids PU ON PU.ProductUIDId = L.ProductUidId " +
                    " LEFT JOIN Web.Products P ON P.ProductId = L.ProductId  " +
                    " LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId  " +
                    " LEFT JOIN Web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId  " +
                    " LEFT JOIN Web.ProductBuyers PB ON PB.ProductId = L.ProductId AND PB.BuyerId = H.BuyerId  " +
                    " Where 1=1  " + bConStr +
                    " ORDER BY H.DocDate, H.PackingHeaderId, L.PackingLineId ";

            SqlConnection con = new SqlConnection(connectionString);
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            
            SqlDataAdapter sqlDataAapter = new SqlDataAdapter(mQry, con);
            dsRep.EnforceConstraints = false;
            sqlDataAapter.Fill(DtTemp);

            if (PrintReportType == "Print For HDC")
            {
                RepName = "Packing_HDCLabelPrint";
                Dt = P.FGetDataForHDCLabelPrint(DtTemp, con);

            }
            else if (PrintReportType == "Print For Artistic Weavers")               
            {
                //RepName = "Packing_AWLabelPrint";
                //RepName = "Packing_AWLabelPrintNew";
                RepName="Packing_AWLabelPrintNew11317";
                Dt = P.FGetDataForSCILabelPrint(DtTemp, con);               
            }
            else if (PrintReportType == "Print Only QR Code")
            {
                //RepName = "Packing_AWLabelPrint";
                RepName = "Packing_LabelPrint_OnlyQRCode";
                Dt = P.FGetDataForSCILabelPrint(DtTemp, con);
            }
            else if (PrintReportType == "Print For Pouf")
            {
                //RepName = "Packing_AWLabelPrint";
                RepName = "Packing_LabelPrint_ForPouf";
                Dt = P.FGetDataForSCILabelPrint(DtTemp, con);
            }
            else
            {
                //RepName = "Packing_LabelPrint";
                //RepName = "Packing_LabelPrintNew";
                RepName = "Packing_LabelPrintNew_1422017";
                RepName=P.ReportName(DtTemp);
                Dt = P.FGetDataForSCILabelPrint(DtTemp, con);
                

            }
            reportdatasource = new ReportDataSource("DsMain", Dt);
           // reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + RepName + ".rdlc");
            reportViewer.LocalReport.ReportPath = ConfigurationManager.AppSettings["PhysicalRDLCPath"] + ConfigurationManager.AppSettings["ReportsPathFromService"] + RepName + ".rdlc";
            ReportService reportservice = new ReportService();
            reportservice.SetReportAttibutes(reportViewer, reportdatasource, "Packing Label Print", "");

        }
        

        [HttpPost]
        [MultipleButton(Name = "Print", Argument = "PDF")]
        public ActionResult PrintToPDF(FormCollection form)
        {
            PackingQRBarCodePrint(form);
            return PrintReport(reportViewer, "PDF");
        }

        [HttpPost]
        [MultipleButton(Name = "Print", Argument = "Excel")]
        public ActionResult PrintToExcel(FormCollection form)
        {
            PackingQRBarCodePrint(form);
            return PrintReport(reportViewer, "Excel");
        }

       

    }
}