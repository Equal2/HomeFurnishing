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
    public class FinishingInvoiceReturnTextImportController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;

        public FinishingInvoiceReturnTextImportController(IUnitOfWork unitOfWork)
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

        public ActionResult FinishingInvoiceReturnTextImport(int id, int? GodownId)
        {
            
            string[] StrArr = new string[] {};

            string ErrorText = "";
            //string WarningText = "";

            if (Request.Files.Count == 0 || Request.Files[0].FileName == "")
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select file.");
                return View("Index");
            }

            if (GodownId == 0 || GodownId == null)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select Godown.");
                return View("Index");
            }

            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);  
            file.SaveAs(filePath);

            StreamReader Sr;
            Sr = new StreamReader(filePath);

            string Line = "";


            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DocTypeId");
            dataTable.Columns.Add("JobReturnDocTypeId");
            dataTable.Columns.Add("DocDate");
            dataTable.Columns["DocDate"].DataType = System.Type.GetType("System.DateTime");
            dataTable.Columns.Add("DocNo");
            dataTable.Columns.Add("JobReturnDocNo");
            dataTable.Columns.Add("DivisionId");
            dataTable.Columns.Add("SiteId");
            dataTable.Columns.Add("ProcessCode");
            dataTable.Columns.Add("OrderByCode");
            dataTable.Columns.Add("ProductUidName");
            dataTable.Columns.Add("SupplierCode");
            dataTable.Columns.Add("CreatedBy");
            dataTable.Columns.Add("GodownId");


            int JobReturnDocTypeId = 0;
            var JobInvoiceSettings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

            if (JobInvoiceSettings != null)
            {
                if (JobInvoiceSettings.JobReturnDocTypeId != null)
                {
                    JobReturnDocTypeId = (int)JobInvoiceSettings.JobReturnDocTypeId;
                }
            }



            string PrevSupplierCode = "";
            string PrevDocNo = "";
            string PrevJobReturnDocNo = "";

            

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            string StrQry = "  Declare @TmpTable as Table " +
                        " ( " +
                        " ProcessCode nVarchar(10), " +
                        " ProductUidName nVarchar(10), " +
                        " SupplierCode nVarchar(10), " +
                        " OrderByCode nVarchar(10) " +
                        " ) ";

            int i = 0;
            do
            {
                i++;
                Line = Sr.ReadLine();

                if (Line != null)
                {
                    StrArr = Line.Split(new Char[] { ',' });

                    StrQry += " Insert Into @TmpTable (ProcessCode, ProductUidName, SupplierCode, OrderByCode) ";
                    StrQry += " Values ('" + StrArr[7] + "', '" + StrArr[13] + "', '" + StrArr[9] + "', '" + StrArr[11] + "')";
                }

            } while (Line != null);

            Sr.Close();

            string mQry = "";
            mQry = StrQry + " Select ProcessCode, ProductUidName, SupplierCode, OrderByCode " +
                " From @TmpTable " +
                " Order by SupplierCode ";


            DataSet TextData = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand(mQry))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = sqlConnection;
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(TextData);
                    }
                }
            }



            for (i = 0; i <= TextData.Tables[0].Rows.Count - 1; i++)
            {
                var dr = dataTable.NewRow();
                dr["DocTypeId"] = id;
                dr["JobReturnDocTypeId"] = JobReturnDocTypeId;
                dr["DocDate"] = DateTime.Now.Date;
                dr["DivisionId"] = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                dr["SiteId"] = (int)System.Web.HttpContext.Current.Session["SiteId"];
                dr["ProductUidName"] = TextData.Tables[0].Rows[i]["ProductUidName"];
                dr["SupplierCode"] = TextData.Tables[0].Rows[i]["SupplierCode"];
                dr["CreatedBy"] = User.Identity.Name;
                dr["ProcessCode"] = TextData.Tables[0].Rows[i]["ProcessCode"];
                dr["OrderByCode"] = TextData.Tables[0].Rows[i]["OrderByCode"];
                dr["GodownId"] = GodownId;

                if (PrevSupplierCode != dr["SupplierCode"].ToString())
                {
                    if (PrevDocNo == "")
                    {
                        dr["DocNo"] = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobInvoiceHeaders", Convert.ToInt32(dr["DocTypeId"]), Convert.ToDateTime(dr["DocDate"]), Convert.ToInt32(dr["DivisionId"]), Convert.ToInt32(dr["SiteId"]));
                        dr["JobReturnDocNo"] = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReturnHeaders", Convert.ToInt32(dr["JobReturnDocTypeId"]), Convert.ToDateTime(dr["DocDate"]), Convert.ToInt32(dr["DivisionId"]), Convert.ToInt32(dr["SiteId"]));
                    }
                    else
                    {
                        dr["DocNo"] = PrevDocNo.Substring(0, 2) + "-" + (Convert.ToInt32(PrevDocNo.Substring(PrevDocNo.IndexOf("-") + 1)) + 1).ToString().PadLeft(4, '0').ToString();
                        dr["JobReturnDocNo"] = PrevJobReturnDocNo.Substring(0, 2) + "-" + (Convert.ToInt32(PrevJobReturnDocNo.Substring(PrevJobReturnDocNo.IndexOf("-") + 1)) + 1).ToString().PadLeft(4, '0').ToString();
                    }
                    PrevSupplierCode = dr["SupplierCode"].ToString();
                    PrevDocNo = dr["DocNo"].ToString();
                    PrevJobReturnDocNo = dr["JobReturnDocNo"].ToString();
                }
                else
                {
                    dr["DocNo"] = PrevDocNo;
                    dr["JobReturnDocNo"] = PrevJobReturnDocNo;
                }
                dataTable.Rows.Add(dr);
            }




            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportFinishingInvoiceReturnFromTextFile"))
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

                if (ErrorText != "" )
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
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/" + "JobInvoiceReturnHeader" + "/" + "Index" + "/" + id);
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
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportFinishingInvoiceReturnFromTextFile"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@TextFileData", dataTable);
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
                    return View("Error", ImportErrorList);
                }

                return View("Sucess");

            }
        }
    }
}