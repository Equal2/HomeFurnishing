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
    public class DyeingReceiveExcelImportController : System.Web.Mvc.Controller
    {
        IExceptionHandlingService _exception;

        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;

        public DyeingReceiveExcelImportController(IUnitOfWork unitOfWork)
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

        public ActionResult DyeingReceiveExcelImport(int id, int? GodownId, int? JobReceiveById, string WorkDispatchNo)
        {
            int i = 0;
            string ErrorText = "";



            if (GodownId == 0 || GodownId == null)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select Godown.");
                return View("Index");
            }

            if (JobReceiveById == 0 || JobReceiveById == null)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select Job Receive By.");
                return View("Index");
            }




            string DyeingConnectionString = "Persist Security Info=False;User ID='sa';pwd=P@ssw0rd!;Initial Catalog=DyeingHouse;Data Source=192.168.2.25";


            DataSet WorkDispatch = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(DyeingConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("Web.FGetWorkOrderDispatch"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@WorkDispatchNo", WorkDispatchNo);
                    cmd.CommandTimeout = 1000;
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(WorkDispatch);
                    }
                }
            }




            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DocTypeId");
            dataTable.Columns.Add("DocDate");
            dataTable.Columns["DocDate"].DataType = System.Type.GetType("System.DateTime");
            dataTable.Columns.Add("DivisionId");
            dataTable.Columns.Add("SiteId");
            dataTable.Columns.Add("WorkDispatchNo");
            dataTable.Columns.Add("JobOrderNo");
            dataTable.Columns.Add("JobWorkerDocNo");
            dataTable.Columns.Add("JobReceiveById");
            dataTable.Columns.Add("ProductName");
            dataTable.Columns.Add("Dimension1Name");
            dataTable.Columns.Add("Dimension2Name");
            dataTable.Columns.Add("Specification");
            dataTable.Columns.Add("CreatedBy");
            dataTable.Columns.Add("GodownId");
            dataTable.Columns.Add("ReceiveQty");
            dataTable.Columns.Add("PassQty");
            dataTable.Columns.Add("LossQty");
            dataTable.Columns.Add("Sr");


            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];


            for (i = 0; i <= WorkDispatch.Tables[0].Rows.Count - 1; i++)
            {
                var dr = dataTable.NewRow();
                dr["DocTypeId"] = id;
                dr["DocDate"] = WorkDispatch.Tables[0].Rows[i]["WorkDispatchDate"]; 
                dr["DivisionId"] = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                dr["SiteId"] = (int)System.Web.HttpContext.Current.Session["SiteId"];
                dr["WorkDispatchNo"] = WorkDispatchNo;
                dr["JobOrderNo"] = WorkDispatch.Tables[0].Rows[i]["JobOrderNo"]; 
                dr["JobWorkerDocNo"] = WorkDispatch.Tables[0].Rows[i]["JobWorkerDocNo"]; 
                dr["JobReceiveById"] = JobReceiveById;
                dr["ProductName"] = WorkDispatch.Tables[0].Rows[i]["Product"];
                dr["Dimension1Name"] = WorkDispatch.Tables[0].Rows[i]["Shade"];
                dr["Dimension2Name"] = WorkDispatch.Tables[0].Rows[i]["Design"];
                dr["Specification"] = WorkDispatch.Tables[0].Rows[i]["Specification"];
                dr["CreatedBy"] = User.Identity.Name;
                dr["GodownId"] = GodownId;
                dr["ReceiveQty"] = WorkDispatch.Tables[0].Rows[i]["ReceiveQty"];
                dr["PassQty"] = WorkDispatch.Tables[0].Rows[i]["PassQty"];
                dr["LossQty"] = WorkDispatch.Tables[0].Rows[i]["LossQty"];
                dr["Sr"] = i + 1;


                dataTable.Rows.Add(dr);

            }

            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportDyeingReceiveFromExcel"))
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
                ViewBag.id = id;
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
                    ImportError.BarCodes = "";
                    ImportErrorList.Add(ImportError);
                }

                if (ErrorText != "")
                {
                    ViewBag.Error = ErrorText;// +WarningText;
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
            //return RedirectToAction("Index", "JobReceiveHeader", new { id = id });
            //return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/" + "JobReceiveHeader" + "/" + "Index" + "/" + menuviewmodel.RouteId + "?MenuId=" + menuviewmodel.MenuId);
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/" + "JobReceiveHeader" + "/" + "Index" + "/" + id);
        }


        public ActionResult Continue(int id)//Document Type Id
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
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportDyeingReceiveFromExcel"))
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
                ViewBag.id = id;
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
                    ViewBag.id = id;
                    //return View("Error");
                    return View("Error", ImportErrorList);
                }

                return View("Sucess");

            }
        }
    }

    public class DyeingReceiveExcel 
    {
        public string JobOrderNo { get; set; }

        public string JobWorkerDocNo { get; set; }

        public string Product { get; set; }
        public string Shade { get; set; }
        public string Design { get; set; }

        public string Specification { get; set; }

        public Decimal ReceiveQty { get; set; }

        public Decimal LossQty { get; set; }

        public Decimal PassQty { get; set; }
    }
}