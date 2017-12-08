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
    public class WeavingReceiveBranchTextImportController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;

        public WeavingReceiveBranchTextImportController(IUnitOfWork unitOfWork)
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

        public ActionResult WeavingReceiveBranchTextImport(int id, int? GodownId)
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



            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DocTypeId");
            dataTable.Columns.Add("DocDate");
            dataTable.Columns["DocDate"].DataType = System.Type.GetType("System.DateTime");
            dataTable.Columns.Add("DocNo");
            dataTable.Columns.Add("DivisionId");
            dataTable.Columns.Add("SiteId");
            dataTable.Columns.Add("ProcessCode");
            dataTable.Columns.Add("JobReceiveByCode");
            dataTable.Columns.Add("ProductUidName");
            dataTable.Columns.Add("SupplierCode");
            dataTable.Columns.Add("CreatedBy");
            dataTable.Columns.Add("GodownId");




            string PrevSupplierCode = "";
            string PrevDoNo = "";

            

            //int i = 0;
            //do
            //{
            //    i++;
            //    Line = Sr.ReadLine();

            //    if (Line != null)
            //    {
            //        StrArr = Line.Split(new Char[] { ',' });


                    

            //        var dr = dataTable.NewRow();
            //        dr["DocTypeId"] = id;
            //        dr["DocDate"] = DateTime.Now.Date;
            //        dr["DivisionId"] = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            //        dr["SiteId"] = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //        dr["ProductUidName"] = StrArr[11];
            //        dr["SupplierCode"] = StrArr[7];
            //        dr["CreatedBy"] = User.Identity.Name;
            //        dr["ProcessCode"] = StrArr[5];
            //        dr["JobReceiveByCode"] = StrArr[9];
            //        dr["GodownId"] = GodownId;

            //        if (PrevSupplierCode != dr["SupplierCode"].ToString())
            //        {
            //            if (PrevDoNo == "")
            //            {
            //                dr["DocNo"] = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveHeaders", Convert.ToInt32(dr["DocTypeId"]), Convert.ToDateTime(dr["DocDate"]), Convert.ToInt32(dr["DivisionId"]), Convert.ToInt32(dr["SiteId"]));
            //            }
            //            else
            //            {
            //                //PrevDoNo.Substring(0,2) + "-" +   
            //                //dr["DocNo"] =  (PrevDoNo.Substring(PrevDoNo.IndexOf("-")+1) + 1).PadLeft(4,new Char[] { '0' }));
            //                dr["DocNo"] = PrevDoNo.Substring(0, 2) + "-" + (Convert.ToInt32(PrevDoNo.Substring(PrevDoNo.IndexOf("-") + 1)) + 1).ToString().PadLeft(4, '0').ToString();
            //            }
            //            PrevSupplierCode = dr["SupplierCode"].ToString();
            //            PrevDoNo = dr["DocNo"].ToString();
            //        }
            //        else
            //        {
            //            dr["DocNo"] = PrevDoNo;
            //        }
                    
            //        dataTable.Rows.Add(dr);
            //    }

            //} while (Line != null);

            //Sr.Close();



            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            string StrQry = "  Declare @TmpTable as Table " +
                        " ( " +
                        " ProcessCode nVarchar(10), " +
                        " ProductUidName nVarchar(10), " +
                        " SupplierCode nVarchar(10), " +
                        " JobReceiveByCode nVarchar(10) " +
                        " ) ";

            int i = 0;
            do
            {
                i++;
                Line = Sr.ReadLine();

                if (Line != null)
                {
                    StrArr = Line.Split(new Char[] { ',' });

                    StrQry += " Insert Into @TmpTable (ProcessCode, ProductUidName, SupplierCode, JobReceiveByCode) ";
                    StrQry += " Values ('" + StrArr[5] + "', '" + StrArr[11] + "', '" + StrArr[7] + "', '" + StrArr[9] + "')";
                }

            } while (Line != null);

            Sr.Close();


            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


            StrQry = StrQry + " UPDATE T " +
                    " SET T.SupplierCode = V1.SupplierCode " +
                    " FROM @TmpTable T, (  	 " +
                    " 	SELECT T.ProductUIdName, P.Code AS SupplierCode         " +
                    " 	FROM @TmpTable T   " +
                    " 	LEFT JOIN Web.ProductUids Pu WITH (NOLock) ON T.ProductUidName = Pu.ProductUidName   " +
                    " 	LEFT JOIN (Select * From Web.ViewJobOrderBalance WITH (NOLock) Where BalanceQty > 0) AS V ON Pu.ProductUIDId = V.ProductUidId " +    
                    " 	LEFT JOIN Web.JobOrderHeaders H WITH (NOLock) ON V.JobOrderHeaderId = H.JobOrderHeaderId " +
                    " 	LEFT JOIN Web.People P WITH (NOLock) ON H.JobWorkerId = P.PersonID  " +
                    " 	WHERE H.SiteId = " + SiteId + " " +
                    " ) AS V1  " +
                    " WHERE T.ProductUIdName = V1.ProductUIdName ";


            string mQry = "";
            mQry = StrQry + " Select ProcessCode, ProductUidName, SupplierCode, JobReceiveByCode " +
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
                    cmd.CommandTimeout = 1000;
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
                dr["DocDate"] = DateTime.Now.Date;
                dr["DivisionId"] = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                dr["SiteId"] = (int)System.Web.HttpContext.Current.Session["SiteId"];
                dr["ProductUidName"] = TextData.Tables[0].Rows[i]["ProductUidName"];
                dr["SupplierCode"] = TextData.Tables[0].Rows[i]["SupplierCode"];
                
                

                //using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
                //{
                //    sqlConnection.Open();

                //    SqlCommand Totalf = new SqlCommand("SELECT P.Code FROM web.ViewJobOrderBalance L " + 
                //            " LEFT JOIN Web.ProductUids Pu ON L.ProductUidId = Pu.ProductUIDId " +
                //            " LEFT JOIN Web.People P ON L.JobWorkerId = P.PersonID " +
                //            " WHERE Pu.ProductUidName = '" + dr["ProductUidName"] + "'", sqlConnection);

                //    dr["SupplierCode"] = Totalf.ExecuteScalar();
                //}





                dr["CreatedBy"] = User.Identity.Name;
                dr["ProcessCode"] = TextData.Tables[0].Rows[i]["ProcessCode"];
                dr["JobReceiveByCode"] = TextData.Tables[0].Rows[i]["JobReceiveByCode"];
                dr["GodownId"] = GodownId;


                if (PrevSupplierCode != dr["SupplierCode"].ToString())
                {
                    if (PrevDoNo == "")
                    {
                        dr["DocNo"] = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveHeaders", Convert.ToInt32(dr["DocTypeId"]), Convert.ToDateTime(dr["DocDate"]), Convert.ToInt32(dr["DivisionId"]), Convert.ToInt32(dr["SiteId"]));
                    }
                    else
                    {
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




            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportWeavingReceiveBranchFromTextFile"))
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
            //return RedirectToAction("Index", "JobReceiveHeader", new { id = id });
            //return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/" + "JobReceiveHeader" + "/" + "Index" + "/" + menuviewmodel.RouteId + "?MenuId=" + menuviewmodel.MenuId);
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/" + "JobReceiveHeader" + "/" + "Index" + "/" + id);
        }
    }

  
}