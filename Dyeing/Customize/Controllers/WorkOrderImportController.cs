using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using System.Configuration;
using Presentation;
using Components.ExceptionHandlers;
using Services.BasicSetup;
using Models.Customize.ViewModels;
using Models.Customize.Models;
using Services.Customize;
using TimePlanValidator;
using TimePlanValidator.ViewModels;
using TimePlanValidator.Common;
using ProjLib.Constants;
using Models.BasicSetup.ViewModels;
using CookieNotifier;
using Presentation.Helper;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using Infrastructure.IO;

namespace Customize.Controllers
{
    [Authorize]
    public class WorkOrderImportController : System.Web.Mvc.Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public WorkOrderImportController(IUnitOfWork unitOfWork)
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

        public ActionResult WorkOrderImport(int id, string DyeingOrderNo)
        {
            int i = 0;
            string ErrorText = "";


            //string DyeingConnectionString = "Persist Security Info=False;User ID='sa'pwd=P@ssw0rd!;Initial Catalog=Rug;Data Source=192.168.2.25";
            string DyeingConnectionString = "Data Source=192.168.2.25;Initial Catalog=Rug;Integrated Security=false; User Id=sa; pwd=P@ssw0rd!";

            DataSet DyeingOrder = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(DyeingConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("Web.FGetDyeingOrderForImport"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@DyeingOrderNo", DyeingOrderNo);
                    cmd.CommandTimeout = 1000;
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(DyeingOrder);
                    }
                }
            }




            DataTable dataTable = new DataTable();


            dataTable.Columns.Add("DocTypeId");
            dataTable.Columns.Add("DocDate");
            dataTable.Columns["DocDate"].DataType = System.Type.GetType("System.DateTime");
            dataTable.Columns.Add("DivisionId");
            dataTable.Columns.Add("SiteId");
            dataTable.Columns.Add("JobWorkerDocNo");
            dataTable.Columns.Add("JobWorkerDocDate");
            dataTable.Columns.Add("OMSPersonId");
            dataTable.Columns.Add("OMSDivisionId");
            dataTable.Columns.Add("DueDate");
            dataTable.Columns.Add("CurrencyName");
            dataTable.Columns.Add("TermsAndConditions");
            dataTable.Columns.Add("ProductName");
            dataTable.Columns.Add("Dimension1Name");
            dataTable.Columns.Add("Dimension2Name");
            dataTable.Columns.Add("Specification");
            dataTable.Columns.Add("CreatedBy");
            dataTable.Columns.Add("GodownId");
            dataTable.Columns.Add("Qty");
            dataTable.Columns.Add("Rate");
            dataTable.Columns.Add("Amount");
            dataTable.Columns.Add("Sr");


            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];


            for (i = 0; i <= DyeingOrder.Tables[0].Rows.Count - 1; i++)
            {
                var dr = dataTable.NewRow();


                dr["DocTypeId"] = id;
                dr["DocDate"] = DyeingOrder.Tables[0].Rows[i]["DocDate"];
                dr["DivisionId"] = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                dr["SiteId"] = (int)System.Web.HttpContext.Current.Session["SiteId"];
                dr["JobWorkerDocNo"] = DyeingOrder.Tables[0].Rows[i]["JobWorkerDocNo"];
                dr["JobWorkerDocDate"] = DyeingOrder.Tables[0].Rows[i]["JobWorkerDocDate"];
                dr["OMSPersonId"] = DyeingOrder.Tables[0].Rows[i]["OMSPersonId"];
                dr["OMSDivisionId"] = DyeingOrder.Tables[0].Rows[i]["OMSDivisionId"];
                dr["DueDate"] = DyeingOrder.Tables[0].Rows[i]["DueDate"];
                dr["CurrencyName"] = DyeingOrder.Tables[0].Rows[i]["CurrencyName"];
                dr["TermsAndConditions"] = DyeingOrder.Tables[0].Rows[i]["TermsAndConditions"];
                dr["ProductName"] = DyeingOrder.Tables[0].Rows[i]["ProductName"];
                dr["Dimension1Name"] = DyeingOrder.Tables[0].Rows[i]["Dimension1Name"];
                dr["Dimension2Name"] = DyeingOrder.Tables[0].Rows[i]["Dimension2Name"];
                dr["Specification"] = DyeingOrder.Tables[0].Rows[i]["Specification"];
                dr["CreatedBy"] = User.Identity.Name;
                dr["GodownId"] = null;
                dr["Qty"] = DyeingOrder.Tables[0].Rows[i]["Qty"];
                dr["Rate"] = DyeingOrder.Tables[0].Rows[i]["Rate"];
                dr["Amount"] = DyeingOrder.Tables[0].Rows[i]["Amount"];
                dr["Sr"] = DyeingOrder.Tables[0].Rows[i]["Sr"];


                dataTable.Rows.Add(dr);

            }

            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportWorkOrderFromExcel"))
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
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["SaleDomain"] + "/" + "SaleOrderHeader" + "/" + "Index" + "/" + id);
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
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportDyeingInvoiceFromExcel"))
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

        public ActionResult GetJobOrder(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            List<ComboBoxResult> temp = GetJobOrderHelpListForProduct(filter, searchTerm);

            var count = temp.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;
            

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public List<ComboBoxResult> GetJobOrderHelpListForProduct(int filter, string term)
        {
            SqlParameter SqlSearchTerm = new SqlParameter("@SearchTerm", term);

            List<ComboBoxResult> list = _unitOfWork.SqlQuery<ComboBoxResult>("Web.GetPendingDyeingOrderToImport @SearchTerm", SqlSearchTerm).ToList();

            return list;
        }

        public JsonResult SetJobOrders(string Ids)
        {
            return Json(GetJobOrderListCsv(Ids));
        }

        public List<ComboBoxResult> GetJobOrderListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();

            

            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);

                SqlParameter SqlJobOrderHeaderId = new SqlParameter("@JobOrderHeaderId", temp);

                List<ComboBoxResult> JobOrderIdList = _unitOfWork.SqlQuery<ComboBoxResult>("Web.GetRugDyeingOrderList @JobOrderHeaderId", SqlJobOrderHeaderId).ToList();


                ProductJson.Add(new ComboBoxResult()
                {
                    id = JobOrderIdList.FirstOrDefault().id.ToString(),
                    text = JobOrderIdList.FirstOrDefault().text
                });
            }
            return ProductJson;
        }
    }

    public class ImportErrors
    {
        public string ErrorText { get; set; }
        public string BarCodes { get; set; }
    }

    
}