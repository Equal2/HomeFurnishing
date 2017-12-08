using Microsoft.Owin.Security;
using Data.Infrastructure;
using Data.Models;
using Reports.Presentation.Helper;
using Model.ViewModels;
using Service;
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
using Model.Models;
using Reports.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Drawing;
using Reports.Common;
using Core.Common;
using Reports.Controllers;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class Report_BarcodePrintController : ReportController
    {

        public Report_BarcodePrintController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;           
        }

        [HttpGet]
        public ActionResult PrintBarCode(string  GenHeaderId )
        {

            DataTable DtTemp = new DataTable();
            DataTable Dt = new DataTable();

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            SqlConnection con = new SqlConnection(connectionString);
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            ApplicationDbContext Db = new ApplicationDbContext();
            String query = Db.strSchemaName + ".ProcBarcodePrint '" + GenHeaderId.ToString() + "' , " + CurrentSiteId.ToString() + ", " + CurrentDivisionId.ToString();


            SqlDataAdapter sqlDataAapter = new SqlDataAdapter(query.ToString(), con);
            dsRep.EnforceConstraints = false;
            sqlDataAapter.Fill(DtTemp);


            if (DtTemp.Rows.Count > 0)
            {


                RepName = "Barcode_Print";                
                Dt = FGetDataForBarcodePrint(DtTemp, con);

                reportdatasource = new ReportDataSource("DsMain", Dt);

                string path = System.AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["ReportsPathFromService"] + RepName + ".rdlc";
                reportViewer.LocalReport.ReportPath = path;
                ReportService reportservice = new ReportService();
                reportservice.SetReportAttibutes(reportViewer, reportdatasource, "Barcode Print", "");
                return PrintReport(reportViewer, "PDF");

            }
            else
            { return View("Close"); }


        }

        [HttpGet]
        public ActionResult PrintOnlyQRCode(string GenHeaderId)
        {

            DataTable DtTemp = new DataTable();
            DataTable Dt = new DataTable();

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            SqlConnection con = new SqlConnection(connectionString);
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            ApplicationDbContext Db = new ApplicationDbContext();
            String query = Db.strSchemaName + ".ProcBarcodePrint '" + GenHeaderId.ToString() + "' , " + CurrentSiteId.ToString() + ", " + CurrentDivisionId.ToString();


            SqlDataAdapter sqlDataAapter = new SqlDataAdapter(query.ToString(), con);
            dsRep.EnforceConstraints = false;
            sqlDataAapter.Fill(DtTemp);


            if (DtTemp.Rows.Count > 0)
            {


                RepName = "Barcode_Print";
                Dt = FGetDataForBarcodePrint(DtTemp, con);

                reportdatasource = new ReportDataSource("DsMain", Dt);

                string path = System.AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["ReportsPathFromService"] + RepName + ".rdlc";
                reportViewer.LocalReport.ReportPath = path;
                ReportService reportservice = new ReportService();
                reportservice.SetReportAttibutes(reportViewer, reportdatasource, "Barcode Print", "");
                return PrintReport(reportViewer, "PDF");

            }
            else
            { return View("Close"); }


        }

        [HttpGet]
        public ActionResult BarCodePrintFromReport()
        {

            return RedirectToAction("ReportLayout", "ReportLayout", new { name = "Barcode Print" });
        }


        public void BarCodePrintFromReport(FormCollection form)
        {

            DataTable Dt = new DataTable();


            string FromBarcode = (form["FromBarcode"].ToString());
            string ToBarcode = (form["ToBarcode"].ToString());
            string BarcodeGenerateNoId = (form["BarcodeGenerateNo"].ToString());
            string BarcodeGenerateNo = (form["BarcodeGenerateNoNames"].ToString());

            string Product = (form["Product"].ToString());
            string ProductName = (form["ProductNames"].ToString());
            string ReportFor = (form["ReportFor"].ToString());

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            DataTable DtTemp = new DataTable();



            SqlConnection con = new SqlConnection(connectionString);
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            ApplicationDbContext Db = new ApplicationDbContext();

            //String query = Db.strSchemaName + ".ProcBarcodePrint '" + BarcodeGenerateNoId.ToString() + "' , " + FromBarcode.ToString() + ", " + ToBarcode.ToString();
            String query = Db.strSchemaName + ".ProcBarcodePrint ";

            if (BarcodeGenerateNoId.ToString() != "")
            {
                query = query + "'" + BarcodeGenerateNoId.ToString() + "'";
            }

            else
            {
                query = query + " NULL ";
            }

            if (CurrentSiteId.ToString() != "")
            {
                query = query + ", '" + CurrentSiteId.ToString() + "'";
            }

            else
            {
                query = query + ", NULL ";
            }

            if (CurrentDivisionId.ToString() != "")
            {
                query = query + ", '" + CurrentDivisionId.ToString() + "'";
            }

            else
            {
                query = query + ", NULL ";
            }

            if (FromBarcode.ToString() != "")
            {
                query = query + ", " + FromBarcode.ToString() + " ";
            }

            else
            {
                query = query + ", NULL ";
            }

            if (ToBarcode.ToString() != "")
            {
                query = query + ", " + ToBarcode.ToString() + " ";
            }

            else
            {
                query = query + ", NULL ";
            }

            if (Product.ToString() != "")
            {
                query = query + ", '" + Product.ToString() + "'";
            }

            else
            {
                query = query + ", NULL ";
            }


            if (ReportFor.ToString() != "")
            {
                query = query + ", '" + ReportFor.ToString() + "'";
            }

            else
            {
                query = query + ", NULL ";
            }

            SqlDataAdapter sqlDataAapter = new SqlDataAdapter(query, con);
            dsRep.EnforceConstraints = false;
            sqlDataAapter.Fill(DtTemp);

            //RepName = "BarcodePrint";
            RepName = "Barcode_Print";
            Dt = FGetDataForBarcodePrint(DtTemp, con);


            reportdatasource = new ReportDataSource("DsMain", Dt);
            string path = System.AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["ReportsPathFromService"] + RepName + ".rdlc";
            reportViewer.LocalReport.ReportPath = path;


            ReportService reportservice = new ReportService();
            reportservice.SetReportAttibutes(reportViewer, reportdatasource, "Barcode Print", "");

        }


        [HttpPost]
        [MultipleButton(Name = "Print", Argument = "PDF")]
        public ActionResult PrintToPDF(FormCollection form)
        {
            BarCodePrintFromReport(form);
            return PrintReport(reportViewer, "PDF");
        }

        [HttpPost]
        [MultipleButton(Name = "Print", Argument = "Excel")]
        public ActionResult PrintToExcel(FormCollection form)
        {
            BarCodePrintFromReport(form);
            return PrintReport(reportViewer, "Excel");
        }




        public DataTable FGetDataForBarcodePrint(DataTable DtTemp, SqlConnection con)
        {

            DataTable Dt = new DataTable();
            Int16 I = 0;


            string bTempTable = "";
            string mQry = "";
            bTempTable = "TempUIdTable";

            mQry = "CREATE TABLE [#" + bTempTable + "] " +
                    " ( ProductUIDId INT,  ProductUidName nVarChar(100), ProductName nVarChar(100), ProductGroupName nVarChar(100), SizeName nVarChar(100),  ProductQualityName nVarChar(100),  ColourName nVarChar(100), ProductUidSpecial_Marks nVarChar(100) , ProductUidNameImg Image )";
            SqlCommand Cmd = new SqlCommand(mQry.ToString(), con);
            Cmd.ExecuteNonQuery();


            for (I = 0; I <= DtTemp.Rows.Count - 1; I++)
            {

                String sSQL = "Insert Into [#" + bTempTable + "] ( ProductUIDId,  ProductUidName,  ProductName,  ProductGroupName,  SizeName,  ProductQualityName,  ColourName,  ProductUidSpecial_Marks,  ProductUidNameImg) " +
                                                        " Values ( @ProductUIDId, @ProductUidName, @ProductName, @ProductGroupName, @SizeName, @ProductQualityName, @ColourName, @ProductUidSpecial_Marks, @ProductUidNameImg )";

                SqlCommand cmd = new SqlCommand(sSQL, con);

                SqlParameter ProductUIDId = new SqlParameter("@ProductUIDId", SqlDbType.Int);
                SqlParameter ProductUidName = new SqlParameter("@ProductUidName", SqlDbType.VarChar);
                SqlParameter ProductName = new SqlParameter("@ProductName", SqlDbType.VarChar);
                SqlParameter ProductGroupName = new SqlParameter("@ProductGroupName", SqlDbType.VarChar);
                SqlParameter SizeName = new SqlParameter("@SizeName", SqlDbType.VarChar);
                SqlParameter ProductQualityName = new SqlParameter("@ProductQualityName", SqlDbType.VarChar);
                SqlParameter ColourName = new SqlParameter("@ColourName", SqlDbType.VarChar);
                SqlParameter ProductUidSpecial_Marks = new SqlParameter("@ProductUidSpecial_Marks", SqlDbType.VarChar);
                SqlParameter ProductUidNameImg = new SqlParameter("ProductUidNameImg", SqlDbType.Image);


                ProductUIDId.Value = DtTemp.Rows[I]["ProductUIDId"];
                ProductUidName.Value = DtTemp.Rows[I]["ProductUidName"];
                ProductName.Value = DtTemp.Rows[I]["ProductName"];
                ProductGroupName.Value = DtTemp.Rows[I]["ProductGroupName"];
                SizeName.Value = DtTemp.Rows[I]["SizeName"];
                ProductQualityName.Value = DtTemp.Rows[I]["ProductQualityName"];
                ColourName.Value = DtTemp.Rows[I]["ColourName"];
                ProductUidSpecial_Marks.Value = DtTemp.Rows[I]["ProductUidSpecial_Marks"];

                if (DtTemp.Rows[I]["ProductUidName"].ToString() != "")
                    ProductUidNameImg.Value = PrintToBarCode(DtTemp.Rows[I]["ProductUidName"].ToString(), 600, 200);
                else
                    ProductUidNameImg.Value = PrintToBarCode("0", 400, 150);


                cmd.Parameters.Add(ProductUIDId);
                cmd.Parameters.Add(ProductUidName);
                cmd.Parameters.Add(ProductName);
                cmd.Parameters.Add(ProductGroupName);
                cmd.Parameters.Add(SizeName);
                cmd.Parameters.Add(ProductQualityName);
                cmd.Parameters.Add(ColourName);
                cmd.Parameters.Add(ProductUidSpecial_Marks);
                cmd.Parameters.Add(ProductUidNameImg);

                cmd.ExecuteNonQuery();

            }


            String strQry = "";

            strQry = " Select H.* From [#" + bTempTable + "] H ";


            SqlDataAdapter sqlDataAapter = new SqlDataAdapter(strQry.ToString(), con);
            dsRep.EnforceConstraints = false;
            sqlDataAapter.Fill(Dt);

            return Dt;
        }





        public Byte[] PrintToBarCode(String TextValue, int Width, int Hight)
        {
            Byte[] PrintToBarCode;
            BarcodeLib.Barcode b;
            b = new BarcodeLib.Barcode();

            Image Img;
            b.Alignment = BarcodeLib.AlignmentPositions.CENTER;
            b.IncludeLabel = false;
            b.RotateFlipType = RotateFlipType.RotateNoneFlipNone;
            b.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;

            if (TextValue == "0")
                Img = b.Encode(BarcodeLib.TYPE.CODE39Extended, TextValue, Color.White, Color.White, Width, Hight);
            else
                Img = b.Encode(BarcodeLib.TYPE.CODE39Extended, TextValue, Color.Black, Color.White, Width, Hight);

            PrintToBarCode = b.Encoded_Image_Bytes;
            return PrintToBarCode;
        }
    }
}