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
using Model.ViewModels;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class PhysicalStockAdjustmentController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private IComboHelpListService cbl = new ComboHelpListService();

        IUnitOfWork _unitOfWork;

        public PhysicalStockAdjustmentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        //  [Authorize]
        public ActionResult Index(int id)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            ViewBag.id = id;
            ViewBag.GodownId = System.Web.HttpContext.Current.Session["DefaultGodownId"];
            var Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, DivisionId, SiteId);

            string ContraDocTypeId = "";
            if (Settings != null)
            {
                ContraDocTypeId = Settings.filterContraDocTypes;
            }
            ViewBag.ContraDocTypeIds = ContraDocTypeId;
            return View();
        }

        public ActionResult PhysicalStockAdjustment(int id, DateTime? DocDate, int? GodownId, string DateList, string Remark)
        {
            
            string[] StrArr = new string[] {};

            string ErrorText = "";
            //string WarningText = "";

            if (DocDate == null)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select Doc Date.");
                return View("Index");
            }

            if (GodownId == 0 || GodownId == null)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select Godown.");
                return View("Index");
            }

            if (DateList == "" || DateList == null)
            {
                //ViewBag.id = id;
                //ModelState.AddModelError("", "Please select Physical Dates.");
                //return View("Index");

                DateList = DocDate.ToString();
            }


            string CreatedBy = User.Identity.Name;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];


            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_PhysicalStockAdjustment"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@DocTypeId", id);
                    cmd.Parameters.AddWithValue("@DocDate", DocDate);
                    cmd.Parameters.AddWithValue("@GodownId", GodownId);
                    cmd.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                    cmd.Parameters.AddWithValue("@DivisionId", DivisionId);
                    cmd.Parameters.AddWithValue("@SiteId", SiteId);
                    cmd.Parameters.AddWithValue("@PhysicalDateList", DateList);
                    cmd.Parameters.AddWithValue("@Remark", Remark);
                    cmd.CommandTimeout = 1000;
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(ds);
                    }
                    //cmd.Connection.Close();
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

                    //if (ds.Tables[0].Rows[j]["WarningText"].ToString() != "")
                    //{
                    //    WarningText = WarningText + ds.Tables[0].Rows[j]["WarningText"].ToString() + "." + Environment.NewLine;
                    //}

                    ImportErrors ImportError = new ImportErrors();
                    ImportError.ErrorText = ds.Tables[0].Rows[j]["ErrorText"].ToString();
                    ImportError.BarCodes = "";
                    ImportErrorList.Add(ImportError);
                }

                if (ErrorText != "" )
                {
                    ViewBag.Error = ErrorText; // +WarningText;
                    ViewBag.id = id;
                    return View("Error", ImportErrorList);
                }


                return View("Sucess");

            }
        }

        public ActionResult ReturnToRoute(int id)//Document Type Id
        {
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["StoreDomain"] + "/" + "StockExchange" + "/" + "Index" + "/" + id);
        }





        public JsonResult GetDateList(string searchTerm, int pageSize, int pageNum, int filter, int ContraDocTypeId, int GodownId)//filter:PersonId
        {
            var temp = GetPhysicalStockDateHelpList(searchTerm, filter, ContraDocTypeId, GodownId).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = GetPhysicalStockDateHelpList(searchTerm, filter, ContraDocTypeId, GodownId).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public IQueryable<ComboBoxResult> GetPhysicalStockDateHelpList(string term, int filter, int ContraDocTypeId, int GodownId)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.StockHeader
                    where p.SiteId == CurrentSiteId && p.DocTypeId == ContraDocTypeId && p.GodownId == GodownId && p.ReferenceDocId == null
                    group p by p.DocDate into Result
                    orderby Result.Key
                    select new ComboBoxResult
                    {
                        id = Result.Key.ToString(),
                        text = Result.Key.ToString()
                    });
        }
    }
}

