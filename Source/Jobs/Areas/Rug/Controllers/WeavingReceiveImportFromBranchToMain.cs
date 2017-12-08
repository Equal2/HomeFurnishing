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
using Model.ViewModels;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class WeavingReceiveImportFromBranchToMainController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;

        public WeavingReceiveImportFromBranchToMainController(IUnitOfWork unitOfWork)
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

        public ActionResult WeavingReceiveImportFromBranchToMain(int id, int? GodownId, int? JobReceiveById, string JobReceiveHeaderIds)
        {
            
            string[] StrArr = new string[] {};

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

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            DataSet TextData = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("Web.sp_GetWeavingReceiveForImport"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@JobReceiveHeaderIds", JobReceiveHeaderIds);
                    cmd.CommandTimeout = 1000;
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(TextData);
                    }
                }
            }



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

            string JobReceiveByCode = (from P in db.Persons where P.PersonID == JobReceiveById select P).FirstOrDefault().Code;


            for (int i = 0; i <= TextData.Tables[0].Rows.Count - 1; i++)
            {
                var dr = dataTable.NewRow();
                dr["DocTypeId"] = id;
                dr["DocDate"] = DateTime.Now.Date;
                dr["DivisionId"] = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                dr["SiteId"] = (int)System.Web.HttpContext.Current.Session["SiteId"];
                dr["ProductUidName"] = TextData.Tables[0].Rows[i]["ProductUidName"];
                dr["SupplierCode"] = TextData.Tables[0].Rows[i]["SupplierCode"];
                dr["CreatedBy"] = User.Identity.Name;
                dr["ProcessCode"] = TextData.Tables[0].Rows[i]["ProcessCode"];
                dr["JobReceiveByCode"] = JobReceiveByCode;
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
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportWeavingReceiveFromBarnchToMainFromTextFile"))
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
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/" + "JobReceiveHeader" + "/" + "Index" + "/" + id);
        }

        public ActionResult GetJobReceive(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            List<ComboBoxResult> temp = GetJobReceiveHelpListForProduct(filter, searchTerm);

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

        public List<ComboBoxResult> GetJobReceiveHelpListForProduct(int filter, string term)
        {
            SqlParameter SqlSearchTerm = new SqlParameter("@SearchTerm", term);

            List<ComboBoxResult> list = _unitOfWork.SqlQuery<ComboBoxResult>("Web.sp_GetPendingWeavingReceiveFromBarnchToImport @SearchTerm", SqlSearchTerm).ToList();

            return list;
        }

        public JsonResult SetJobReceive(string Ids)
        {
            return Json(GetJobReceiveListCsv(Ids));
        }

        public List<ComboBoxResult> GetJobReceiveListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();



            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);

                SqlParameter SqlJobReceiveHeaderId = new SqlParameter("@JobReceiveHeaderId", temp);

                List<ComboBoxResult> JobReceiveIdList = _unitOfWork.SqlQuery<ComboBoxResult>("Web.sp_GetJobReceiveList @JobReceiveHeaderId", SqlJobReceiveHeaderId).ToList();


                ProductJson.Add(new ComboBoxResult()
                {
                    id = JobReceiveIdList.FirstOrDefault().id.ToString(),
                    text = JobReceiveIdList.FirstOrDefault().text
                });
            }
            return ProductJson;
        }
    }


}