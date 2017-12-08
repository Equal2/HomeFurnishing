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
    public class StockIssRecTextImportController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;

        public StockIssRecTextImportController(IUnitOfWork unitOfWork)
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

        public ActionResult StockIssRecTextImport(int id, int? GodownId)
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

            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);  
            file.SaveAs(filePath);

            StreamReader Sr;
            Sr = new StreamReader(filePath);

            string Line = "";


            //List<TypeImportStockIssRecFromTextFile> ImportData = new List<TypeImportStockIssRecFromTextFile>();

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DocTypeId");
            dataTable.Columns.Add("DocDate");
            dataTable.Columns["DocDate"].DataType = System.Type.GetType("System.DateTime");
            dataTable.Columns.Add("DocNo");
            dataTable.Columns.Add("DivisionId");
            dataTable.Columns.Add("SiteId");
            dataTable.Columns.Add("ProcessCode");
            dataTable.Columns.Add("StockIssRecByCode");
            dataTable.Columns.Add("ProductUidName");
            dataTable.Columns.Add("SupplierCode");
            dataTable.Columns.Add("CreatedBy");
            dataTable.Columns.Add("GodownId");
            dataTable.Columns.Add("DocNature");




            string PrevSupplierCode = "";
            string PrevDoNo = "";

            int DocTypeId = (from Dt in db.DocumentType where Dt.DocumentTypeName == TransactionDoctypeConstants.WeavingBazar select new { DocTypeId = Dt.DocumentTypeId }).FirstOrDefault().DocTypeId;
            

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
                    dr["ProductUidName"] = StrArr[13];
                    dr["SupplierCode"] = StrArr[9];
                    dr["CreatedBy"] = User.Identity.Name;
                    dr["ProcessCode"] = StrArr[7];
                    dr["StockIssRecByCode"] = StrArr[11];
                    dr["GodownId"] = GodownId;
                    dr["DocNature"] = StrArr[5];

                    if (PrevSupplierCode != dr["SupplierCode"].ToString())
                    {
                        if (PrevDoNo == "")
                        {
                            dr["DocNo"] = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".StockHeaders", Convert.ToInt32(dr["DocTypeId"]), Convert.ToDateTime(dr["DocDate"]), Convert.ToInt32(dr["DivisionId"]), Convert.ToInt32(dr["SiteId"]));
                        }
                        else
                        {
                            //PrevDoNo.Substring(0,2) + "-" +   
                            //dr["DocNo"] =  (PrevDoNo.Substring(PrevDoNo.IndexOf("-")+1) + 1).PadLeft(4,new Char[] { '0' }));
                            dr["DocNo"] = PrevDoNo.Substring(0, 2) + "-" + (Convert.ToInt32(PrevDoNo.Substring(PrevDoNo.IndexOf("-") + 1)) + 1).ToString().PadLeft(4, '0').ToString();
                        }
                        PrevSupplierCode = dr["SupplierCode"].ToString();
                        PrevDoNo = dr["DocNo"].ToString();
                    }
                    else
                    {
                        dr["DocNo"] = PrevDoNo;
                    }
                    
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
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportStockIssRecFromTextFile"))
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
            //return RedirectToAction("Index", "StockIssRecHeader", new { id = id });
            //return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/" + "StockIssRecHeader" + "/" + "Index" + "/" + menuviewmodel.RouteId + "?MenuId=" + menuviewmodel.MenuId);
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/" + "StockIssRecHeader" + "/" + "Index" + "/" + id);
        }
    }


}