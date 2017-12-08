using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data;
using Data.Models;
using Service;
using Core;
using Model.Models;
using System.Configuration;
using System.Text;
using Data.Infrastructure;
using Core.Common;
using Core.Attributes;
using System.Data;
using System.Data.SqlClient;
using Model.ViewModel;

namespace Jobs.Areas.Rug.Controllers
{

    [Authorize]
    public class PackingBarcodeImportController : System.Web.Mvc.Controller
    {
        IExceptionHandlingService _exception;

        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;

        public PackingBarcodeImportController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        //  [Authorize]

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PackingBarcodeImport(int? PackingHeaderId)
        {
            string ErrorText = "";

            if (PackingHeaderId == 0 || PackingHeaderId == null)
            {
                ModelState.AddModelError("", "Please select Packing No.");
                return View("Index");
            }

            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);
            file.SaveAs(filePath);
            var excel = new ExcelQueryFactory();
            excel.FileName = filePath;
            var PackingBarcodeList = from c in excel.Worksheet<PackingBarcode>() select c;



            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("BarCode");
            dataTable.Columns.Add("PackingHeaderId");
            dataTable.Columns.Add("CreatedBy");
            dataTable.Columns.Add("Sr");


            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            int i = 0;
            foreach (var temp in PackingBarcodeList)
            {
                i++;
                var dr = dataTable.NewRow();
                dr["BarCode"] = temp.BarCode;
                dr["PackingHeaderId"] = PackingHeaderId;
                dr["CreatedBy"] = User.Identity.Name;
                dr["Sr"] = i; 

                dataTable.Rows.Add(dr);
            }

            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportPackingBarcode"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@ExcelFileData", dataTable);
                    cmd.CommandTimeout = 1000;
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(ds);
                    }
                }
            }

            List<ImportErrors> ImportErrorList = new List<ImportErrors>();

            if (ds.Tables[0].Rows.Count == 0)
            {
                return View("Sucess");
            }
            else
            {
                for (int j = 0; j <= ds.Tables[0].Rows.Count - 1; j++)
                {
                    if (ds.Tables[0].Rows[j]["ErrorText"].ToString() != "")
                    {
                        ErrorText = ErrorText + ds.Tables[0].Rows[j]["ErrorText"].ToString() + "." + Environment.NewLine;
                    }

                    ImportErrors ImportError = new ImportErrors();
                    ImportError.ErrorText = ds.Tables[0].Rows[j]["ErrorText"].ToString();
                    ImportError.BarCodes = ds.Tables[0].Rows[j]["BarCodes"].ToString();
                    ImportErrorList.Add(ImportError);
                }

                if (ErrorText != "")
                {
                    ViewBag.Error = ErrorText;// +WarningText;
                    string DataTableSessionVarName = "";
                    DataTableSessionVarName = User.Identity.Name.ToString() + "ImportDataPacking" ;
                    Session[DataTableSessionVarName] = dataTable;
                    return View("Error", ImportErrorList);
                }

                return View("Sucess");
            }
        }

        public ActionResult ReturnToRoute()
        {
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "PackingHeader" + "/" + "Index");
        }


        public ActionResult Continue()
        {
            string DataTableSessionVarName = "";
            DataTableSessionVarName = User.Identity.Name.ToString() + "ImportDataPacking";

            DataTable dataTable = (DataTable)Session[DataTableSessionVarName];
            string ErrorText = "";
            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportPackingBarcode"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@ExcelFileData", dataTable);
                    cmd.Parameters.AddWithValue("@SkipValidation", 1);
                    cmd.CommandTimeout = 1000;
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(ds);
                    }
                }
            }

            List<ImportErrors> ImportErrorList = new List<ImportErrors>();
            if (ds.Tables[0].Rows.Count == 0)
            {
                return View("Sucess");
            }
            else
            {
                for (int j = 0; j <= ds.Tables[0].Rows.Count - 1; j++)
                {
                    if (ds.Tables[0].Rows[j]["ErrorText"].ToString() != "")
                    {
                        ErrorText = ErrorText + ds.Tables[0].Rows[j]["ErrorText"].ToString() + "." + Environment.NewLine;
                    }

                    ImportErrors ImportError = new ImportErrors();
                    ImportError.ErrorText = ds.Tables[0].Rows[j]["ErrorText"].ToString();
                    ImportErrorList.Add(ImportError);
                }

                if (ErrorText != "")
                {
                    ViewBag.Error = ErrorText;
                    //return View("Error");
                    return View("Error", ImportErrorList);
                }

                return View("Sucess");

            }
        }
    }

    public class PackingBarcode 
    {
        public string BarCode { get; set; }
        public string PackingNo { get; set; }
    }
}