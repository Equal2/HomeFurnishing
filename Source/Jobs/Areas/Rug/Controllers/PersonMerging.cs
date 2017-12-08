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
    public class PersonMergingController : System.Web.Mvc.Controller
    {
        IExceptionHandlingService _exception;

        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;

        public PersonMergingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        //  [Authorize]

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PersonMerging()
        {
            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);
            file.SaveAs(filePath);
            var excel = new ExcelQueryFactory();
            excel.FileName = filePath;
            IList<MergingExcel> MergingRecordList = (from c in excel.Worksheet<MergingExcel>() select c).ToList();

            return DatabaseOperation(MergingRecordList);
        }

        public ActionResult DatabaseOperation(IList<MergingExcel>  MergingRecordList)
        {
            string ErrorText = "";

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("MergingItem");
            dataTable.Columns.Add("MainItem");


            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            string UserName = User.Identity.Name;

            foreach (var item in MergingRecordList)
            {
                var dr = dataTable.NewRow();
                dr["MergingItem"] = item.MergingItem;
                dr["MainItem"] = item.MainItem;

                dataTable.Rows.Add(dr);
            }

            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_MergePerson"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@ExcelFileData", dataTable);
                    cmd.Parameters.AddWithValue("@UserName", UserName);
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
                    ImportError.BarCodes = "";
                    ImportErrorList.Add(ImportError);
                }

                if (ErrorText != "")
                {
                    ViewBag.Error = ErrorText;// +WarningText;
                    string DataTableSessionVarName = "";
                    DataTableSessionVarName = User.Identity.Name.ToString() + "ImportDataPersonMerging";
                    Session[DataTableSessionVarName] = dataTable;
                    return View("Error", ImportErrorList);
                }

                return View("Sucess");
            }
        }

        public ActionResult Import()
        {
            return RedirectToAction("Index");
        }

        public ActionResult SingleMerging()
        {
            return View();
        }

        public ActionResult SingleMergingPost(int? MergingPersonId, int? MainPersonId)
        {
            if (MergingPersonId == 0 || MergingPersonId == null)
            {
                ModelState.AddModelError("", "Please select Merging Person.");
                return View("SingleMerging");
            }

            if (MainPersonId == 0 || MainPersonId == null)
            {
                ModelState.AddModelError("", "Please select Main Person.");
                return View("SingleMerging");
            }

            if (MergingPersonId == MainPersonId)
            {
                ModelState.AddModelError("", "Merging person and Main person should not be same.");
                return View("SingleMerging");
            }


            
            MergingExcel M = new MergingExcel();
            M.MergingItem = new PersonService(_unitOfWork).Find((int)MergingPersonId).Code;
            M.MainItem = new PersonService(_unitOfWork).Find((int)MainPersonId).Code;
            //M.MergingItem = new PersonService(_unitOfWork).Find((int)MergingPersonId).Name;
            //M.MainItem = new PersonService(_unitOfWork).Find((int)MainPersonId).Name;
            
            IList<MergingExcel> MergingRecordList = new List<MergingExcel>();
            MergingRecordList.Add(M);

            return DatabaseOperation(MergingRecordList);
        }
    }
}