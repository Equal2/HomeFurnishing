using System;
using System.Linq;
using System.Web.Mvc;
using Data.Models;
using Service;
using Core;
using Model.Models;
using System.Configuration;
using Data.Infrastructure;
using System.Data.SqlClient;
using System.Data;

namespace Jobs.Controllers
{
    [Authorize]
    public class SaleDeliveryOrderExcelImportController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;

        public SaleDeliveryOrderExcelImportController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        //  [Authorize]
        public ActionResult Index(int id)
        {
            ViewBag.id = id;
            return View();
        }

        public ActionResult SaleDeliveryOrderExcelImport(int id, int? BuyerId, DateTime? DueDate, int? ShipMethodId)
        {
            string[] StrArr = new string[] {};

            string ErrorText = "";

            if (Request.Files.Count == 0 || Request.Files[0].FileName == "")
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select file.");
                return View("Index");
            }

            if (BuyerId == 0 || BuyerId == null)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select Buyer.");
                return View("Index");
            }


            if (ShipMethodId == 0 || ShipMethodId == null)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select Ship Method.");
                return View("Index");
            }

            if (DueDate == null)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Please select Due Date.");
                return View("Index");
            }

            if (DueDate < DateTime.Now.Date)
            {
                ViewBag.id = id;
                ModelState.AddModelError("", "Due Date can not be less then current date.");
                return View("Index");
            }


            var file = Request.Files[0];
            string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] + file.FileName);  
            file.SaveAs(filePath);

            var excel = new ExcelQueryFactory();
            excel.FileName = filePath;
            var SaleDeliveryOrderRecordList = from c in excel.Worksheet<SaleDeliveryOrderExcel>() select c;


            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DocTypeId");
            dataTable.Columns.Add("DocDate");
            dataTable.Columns["DocDate"].DataType = System.Type.GetType("System.DateTime");
            dataTable.Columns.Add("DueDate");
            dataTable.Columns["DueDate"].DataType = System.Type.GetType("System.DateTime");
            dataTable.Columns.Add("DocNo");
            dataTable.Columns.Add("DivisionId");
            dataTable.Columns.Add("SiteId");
            dataTable.Columns.Add("BuyerId");
            dataTable.Columns.Add("ShipMethodId");
            dataTable.Columns.Add("CreatedBy");
            dataTable.Columns.Add("ProductName");
            dataTable.Columns.Add("Qty");


            string PrevSupplierCode = "";
            string PrevDoNo = "";


            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];


            string DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".SaleDeliveryOrderHeaders", id, DateTime.Now.Date, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

            foreach (var SaleOrderRecord in SaleDeliveryOrderRecordList)
            {
                var dr = dataTable.NewRow();
                dr["DocTypeId"] = id;
                dr["DocDate"] = DateTime.Now.Date;
                dr["DueDate"] = DueDate;
                dr["DocNo"] = DocNo;
                dr["DivisionId"] = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                dr["SiteId"] = (int)System.Web.HttpContext.Current.Session["SiteId"];
                dr["BuyerId"] = BuyerId;
                dr["ShipMethodId"] = ShipMethodId;
                dr["CreatedBy"] = User.Identity.Name;
                dr["ProductName"] = SaleOrderRecord.Product;
                dr["Qty"] = SaleOrderRecord.Qty;

                dataTable.Rows.Add(dr);
            }




            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcImportSaleDeliveryOrderFromExcelFile"))
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
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "SaleDeliveryOrderHeader" + "/" + "Index" + "/" + id);
        }
    }

  
}