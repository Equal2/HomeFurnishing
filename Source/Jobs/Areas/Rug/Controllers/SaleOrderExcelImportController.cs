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
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Model.ViewModel;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class SaleOrderExcelImportController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;

        public SaleOrderExcelImportController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        //  [Authorize]
        public ActionResult Index(int id)
        {
            ViewBag.id = id;
            ViewBag.GodownId = System.Web.HttpContext.Current.Session["DefaultGodownId"];
            return View();
        }

        public ActionResult SaleOrderExcelImport(int id)
        {
            int i = 0;
            string[] StrArr = new string[] {};

            string ErrorText = "";
            //string WarningText = "";

            if (Request.Files.Count == 0 || Request.Files[0].FileName == "")
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select file.");
                return View("Index");
            }

            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);
            file.SaveAs(filePath);

            var excel = new ExcelQueryFactory();
            excel.FileName = filePath;
            var SaleOrderRecordList = from c in excel.Worksheet<SaleOrderExcel>() select c;


            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DocTypeId");
            dataTable.Columns.Add("DocDate");
            dataTable.Columns["DocDate"].DataType = System.Type.GetType("System.DateTime");
            dataTable.Columns.Add("DocNo");
            dataTable.Columns.Add("DivisionId");
            dataTable.Columns.Add("SiteId");
            dataTable.Columns.Add("DueDate");
            dataTable.Columns["DueDate"].DataType = System.Type.GetType("System.DateTime");
            dataTable.Columns.Add("SaleToBuyerName");
            dataTable.Columns.Add("BuyerUpcCode");
            dataTable.Columns.Add("CreatedBy");
            dataTable.Columns.Add("ProductName");
            dataTable.Columns.Add("Qty");
            dataTable.Columns.Add("Rate");
            dataTable.Columns.Add("Sr");



            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];


            foreach (SaleOrderExcel item in SaleOrderRecordList)
            {
                i = i + 1;
                var dr = dataTable.NewRow();
                dr["DocTypeId"] = id;
                dr["DocDate"] = DateTime.Now.Date;
                dr["DocNo"] = item.BuyerOrderNo;
                dr["DivisionId"] = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                dr["SiteId"] = (int)System.Web.HttpContext.Current.Session["SiteId"];
                dr["DocDate"] = item.DueDate;
                dr["SaleToBuyerName"] = item.SaleToBuyer;
                dr["BuyerUpcCode"] = item.BuyerUpcCode;
                dr["CreatedBy"] = User.Identity.Name;
                dr["ProductName"] = item.Product;
                dr["Qty"] = item.Quantity;
                dr["Rate"] = item.Rate; 
                dr["Sr"] = i;

                dataTable.Rows.Add(dr);
            }


            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportSaleOrderFromExcelFile"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@TextFileData", dataTable);
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
                ViewBag.id = id;
                return View("Sucess");
            }
            else
            {
                for (int j = 0 ; j <= ds.Tables[0].Rows.Count -1 ; j ++)
                {
                    if (ds.Tables[0].Rows[j]["ErrorText"].ToString() != "")
                    {
                        ErrorText = ErrorText + ds.Tables[0].Rows[j]["ErrorText"].ToString() + "." + Environment.NewLine;
                    }

                    ImportErrors ImportError = new ImportErrors();
                    ImportError.ErrorText = ds.Tables[0].Rows[j]["ErrorText"].ToString();
                    ImportErrorList.Add(ImportError);
                }

                if (ErrorText != "" )
                {
                    ViewBag.Error = ErrorText; 
                    ViewBag.id = id;
                    string DataTableSessionVarName = "";
                    DataTableSessionVarName = User.Identity.Name.ToString() + "ImportData" + id.ToString();
                    Session[DataTableSessionVarName] = dataTable;
                    return View("Error", ImportErrorList);
                }


                return View("Sucess");

            }
        }

        public ActionResult ReturnToRoute(int id)//Document Type Id
        {
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/" + "JobOrderHeader" + "/" + "Index" + "/" + id);
        }

        public ActionResult Continue(int id, int ImportWarningRecords)//Document Type Id
        {
            string DataTableSessionVarName = "";
            DataTableSessionVarName = User.Identity.Name.ToString() + "ImportData" + id.ToString();

            DataTable dataTable = (DataTable)Session[DataTableSessionVarName];
            string ErrorText = "";
            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportSaleOrderFromExcelFile"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@TextFileData", dataTable);
                    cmd.Parameters.AddWithValue("@SkipValidation",1);
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
                ViewBag.id = id;
                return View("Sucess");
            }
            else
            {
                for (int j = 0 ; j <= ds.Tables[0].Rows.Count -1 ; j ++)
                {
                    if (ds.Tables[0].Rows[j]["ErrorText"].ToString() != "")
                    {
                        ErrorText = ErrorText + @"<br />" + ds.Tables[0].Rows[j]["ErrorText"].ToString() + ".";
                    }
                    
                    ImportErrors ImportError = new ImportErrors();
                    ImportError.ErrorText = ds.Tables[0].Rows[j]["ErrorText"].ToString();
                    ImportErrorList.Add(ImportError);
                }

                


                if (ErrorText != "")
                {
                    ViewBag.Error = ErrorText;
                    ViewBag.id = id;
                    return View("Error", ImportErrorList);
                }



                return View("Sucess");

            }
        }

    }
}

