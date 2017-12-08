using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Data.Models;
using Service;
using Data.Infrastructure;
using Model.ViewModels;
using System.Data.SqlClient;

namespace Jobs.Controllers
{
    [Authorize]
    public class BarCodeHistoryController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public BarCodeHistoryController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _unitOfWork = unitOfWork;
            _exception = exec;
        }
        // GET: /GateMaster/

        public ActionResult Index(string ProductUid)
        {

            if (!string.IsNullOrEmpty(ProductUid))
                ViewBag.ProductUid = ProductUid;

            return View();
        }



        public JsonResult AjaxGetJsonData(int draw, int start, int length, string ProductUid)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            if (length == -1)
            {
                length = TOTAL_ROWS;
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            int recordsFiltered = 0;
            dataTableData.data = FilterData(ref recordsFiltered, ref TOTAL_ROWS, start, length, search, sortColumn, sortDirection, ProductUid);
            dataTableData.recordsTotal = TOTAL_ROWS;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }


        private static int TOTAL_ROWS = 0;
        //private static readonly List<DataItem> _data = CreateData();    
        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<StockViewModel> data { get; set; }
        }


        public class DataTableLedgerData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<ProductUidLedgerInfo> data { get; set; }
        }


        // here we simulate SQL search, sorting and paging operations
        private List<StockViewModel> FilterData(ref int recordFiltered, ref int recordTotal, int start, int length, string search, int sortColumn, string sortDirection, string ProductUid)
        {

            //var _data = (from p in db.Stock
            //             join t in db.StockHeader on p.StockHeaderId equals t.StockHeaderId
            //             join t2 in db.ProductUid on p.ProductUidId equals t2.ProductUIDId
            //             where t2.ProductUidName == ProductUid
            //             orderby t.DocDate, p.CreatedDate
            //             select new
            //             {

            //                 t.DocType.DocumentTypeName,
            //                 t.DocNo,
            //                 t.DocDate,
            //                 t.Person.Name,
            //                 p.Godown.GodownName,
            //                 t.Process.ProcessName,
            //                 p.Remark,
            //                 p.StockId,
            //                 p.CreatedDate
            //             }).ToList();


            List<StockViewModel> _data = db.Database.SqlQuery<StockViewModel>("EXECUTE Web.GetBarCodeHistory @p0", new SqlParameter("@p0", ProductUid)).ToList();

            recordTotal = _data.Count();

            recordFiltered = _data.Count();

            return _data;

        }

        public JsonResult AjaxGetGeneration(string ProductUid)
        {



            //var Rec = (from p in db.ProductUid
            //           join t in db.DocumentType on p.GenDocTypeId equals t.DocumentTypeId
            //           into table
            //           from dt in table.DefaultIfEmpty()
            //           join t2 in db.Persons on p.GenPersonId equals t2.PersonID into table2
            //           from pt in table2.DefaultIfEmpty()
            //           join prod in db.Product on p.ProductId equals prod.ProductId
            //           where p.ProductUidName == ProductUid
            //           select new
            //           {

            //               p.GenDocNo,
            //               p.GenDocDate,
            //               dt.DocumentTypeName,
            //               pt.Name,
            //               prod.ProductName
            //           }).FirstOrDefault();

            BarCodeGenDetails Rec = db.Database.SqlQuery<BarCodeGenDetails>("EXECUTE Web.GetBarCodeGenData @p0", new SqlParameter("@p0", ProductUid)).FirstOrDefault();

            if (Rec != null)
                return Json(new { Success = true, DocNo = Rec.GenDocNo, DocDate = Rec.GenDocDate, DocType = Rec.DocumentTypeName, Person = Rec.Name, Product = Rec.ProductName, Godown = Rec.Godown, Status = Rec.Status }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);

        }

        class BarCodeGenDetails
        {
            public string GenDocNo { get; set; }
            public string GenDocDate { get; set; }
            public string DocumentTypeName { get; set; }
            public string Name { get; set; }
            public string ProductName { get; set; }
            public string Status { get; set; }
            public string Godown { get; set; }

        }

        public ActionResult RecordMenu(int id)
        {
            var StockRec = db.Stock.Find(id);

            var StockHeaderRec = db.StockHeader.Find(StockRec.StockHeaderId);

            string RedirectUrl = System.Configuration.ConfigurationManager.AppSettings["AdminSetupDomain"] + "/Redirect/RedirectToDocument?DocTypeId=" + StockHeaderRec.DocTypeId + "&DocId=" + (StockHeaderRec.DocHeaderId ?? StockHeaderRec.StockHeaderId);

            return Redirect(RedirectUrl);
        }

        public JsonResult AjaxGetLedgerData(int draw, int start, int length, string ProductUid)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            if (length == -1)
            {
                length = TOTAL_ROWS;
            }

            DataTableLedgerData dataTableData = new DataTableLedgerData();
            dataTableData.draw = draw;
            int recordsFiltered = 0;
            dataTableData.data = FilterLedgerData(ref recordsFiltered, ref TOTAL_ROWS, start, length, search, sortColumn, sortDirection, ProductUid);
            dataTableData.recordsTotal = TOTAL_ROWS;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }


        private List<ProductUidLedgerInfo> FilterLedgerData(ref int recordFiltered, ref int recordTotal, int start, int length, string search, int sortColumn, string sortDirection, string ProductUid)
        {
            List<ProductUidLedgerInfo> _data = db.Database.SqlQuery<ProductUidLedgerInfo>("EXECUTE Web.GetBarLedgerCodeHistory @p0", new SqlParameter("@p0", ProductUid)).ToList();

            recordTotal = _data.Count();

            recordFiltered = _data.Count();

            return _data;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }


    public class ProductUidLedgerInfo
    {
        public string DocTypeName { get; set; }
        public string DocNo { get; set; }
        public string sDocDate { get; set; }
        public string ProcessName { get; set; }
        public string PersonName { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public int LedgerId { get; set; }
    }
}
