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

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class StockTransferTextImportController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;

        public StockTransferTextImportController(IUnitOfWork unitOfWork)
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

        public ActionResult StockTransferTextImport(int id, int? GodownId, int? FromGodownId, int? PersonId)
        {
            
            string[] StrArr = new string[] {};

            string ErrorText = "";

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

            if (FromGodownId == 0 || FromGodownId == null)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select From Godown.");
                return View("Index");
            }

            if (FromGodownId == GodownId)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "From Godown and To Godown Can Not Be Same.");
                return View("Index");
            }

            if (PersonId == 0 || PersonId == null)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select Person.");
                return View("Index");
            }

            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);  
            file.SaveAs(filePath);

            StreamReader Sr;
            Sr = new StreamReader(filePath);

            string Line = "";


            //List<TypeImportStockTransferFromTextFile> ImportData = new List<TypeImportStockTransferFromTextFile>();

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DocTypeId");
            dataTable.Columns.Add("DocDate");
            dataTable.Columns["DocDate"].DataType = System.Type.GetType("System.DateTime");
            dataTable.Columns.Add("DocNo");
            dataTable.Columns.Add("DivisionId");
            dataTable.Columns.Add("SiteId");
            dataTable.Columns.Add("ProductUidName");
            dataTable.Columns.Add("CreatedBy");
            dataTable.Columns.Add("GodownId");
            dataTable.Columns.Add("FromGodownId");
            dataTable.Columns.Add("PersonId");



            int DocTypeId = (from Dt in db.DocumentType where Dt.DocumentTypeName == TransactionDoctypeConstants.CarpetTransfer select new { DocTypeId = Dt.DocumentTypeId }).FirstOrDefault().DocTypeId;
            string DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".StockHeaders", DocTypeId, DateTime.Now.Date, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

            int i = 0;
            do
            {
                i++;
                Line = Sr.ReadLine();

                if (Line != null)
                {
                    StrArr = Line.Split(new Char[] { ',' });


                    

                    var dr = dataTable.NewRow();
                    dr["DocTypeId"] = id;
                    dr["DocDate"] = DateTime.Now.Date;
                    dr["DivisionId"] = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    dr["SiteId"] = (int)System.Web.HttpContext.Current.Session["SiteId"];
                    dr["ProductUidName"] = StrArr[7];
                    dr["CreatedBy"] = User.Identity.Name;
                    dr["GodownId"] = GodownId;
                    dr["FromGodownId"] = FromGodownId;
                    dr["DocNo"] = DocNo;
                    dr["PersonId"] = PersonId;
                    
                    dataTable.Rows.Add(dr);
                }

            } while (Line != null);

            Sr.Close();

            //string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString();

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportStockTransferFromTextFile"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@TextFileData", dataTable);
                    cmd.CommandTimeout = 1000;
                    //cmd.Connection.Open();
                    //cmd.ExecuteNonQuery();
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(ds);
                    }
                    //cmd.Connection.Close();
                }
            }

            if (ds.Tables[0].Rows.Count == 0)
            {
                ViewBag.id = id;
                return View("Sucess");
            }
            else
            {
                for (int j = 0 ; j <= ds.Tables[0].Rows.Count -1 ; j ++)
                {
                    ErrorText = ErrorText + ds.Tables[0].Rows[j]["ErrorText"].ToString() + "." + Environment.NewLine ;
                }
                ViewBag.Error = ErrorText;
                ViewBag.id = id;
                return View("Error");
            }
        }

        public ActionResult ReturnToRoute(int id)//Document Type Id
        {
            //return RedirectToAction("Index", "StockTransferHeader", new { id = id });
            //return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/" + "StockTransferHeader" + "/" + "Index" + "/" + menuviewmodel.RouteId + "?MenuId=" + menuviewmodel.MenuId);
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["Storedomain"] + "/" + "MaterialTransferHeader" + "/" + "Index" + "/" + id);
        }
    }


}