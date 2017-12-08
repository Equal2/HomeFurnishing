using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Data.Models;
using Service;
using Data.Infrastructure;
using Model.ViewModels;
using System.Data.SqlClient;
using Model.ViewModel;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductPurchaseHistoryController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IExceptionHandlingService _exception;
        public ProductPurchaseHistoryController()
        {
            _exception = new ExceptionHandlingService();
        }
        // GET: /GateMaster/

        public ActionResult Index(int? ProductId)
        {
            ViewBag.ProductId = ProductId;
            return View();
        }



        public JsonResult AjaxGetJsonData(int ProductId)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";

            DataTableData dataTableData = new DataTableData();
            int recordsFiltered = 0;
            dataTableData.data = FilterData(ref recordsFiltered, ref TOTAL_ROWS, search, sortColumn, sortDirection, ProductId);
            dataTableData.recordsTotal = TOTAL_ROWS;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AjaxGetInvoiceJsonData(int ProductId)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";

            DataTableData dataTableData = new DataTableData();
            int recordsFiltered = 0;
            var fdata = InvoiceFilterData(ref recordsFiltered, ref TOTAL_ROWS, search, sortColumn, sortDirection, ProductId);
            dataTableData.recordsTotal = TOTAL_ROWS;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(new { data = fdata, recordsTotal = TOTAL_ROWS, recordsFiltered = recordsFiltered }, JsonRequestBehavior.AllowGet);
        }


        private static int TOTAL_ROWS = 0;
        //private static readonly List<DataItem> _data = CreateData();    
        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<PurchaseOrderHeaderViewModel> data { get; set; }
        }


        // here we simulate SQL search, sorting and paging operations
        private List<PurchaseOrderHeaderViewModel> FilterData(ref int recordFiltered, ref int recordTotal,
            string search, int sortColumn, string sortDirection, int ProductId)
        {

            //var _data = (from p in db.Stock
            //             join t in db.StockHeader on p.StockHeaderId equals t.StockHeaderId
            //             join t2 in db.ProductId on p.ProductIdId equals t2.ProductIdId
            //             where t2.ProductIdName == ProductId
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


            var PurchaseOrders = (from p in db.PurchaseOrderLine
                                  join t in db.PurchaseOrderHeader on p.PurchaseOrderHeaderId equals t.PurchaseOrderHeaderId
                                  where p.ProductId == ProductId
                                  join dt in db.DocumentType on t.DocTypeId equals dt.DocumentTypeId
                                  join per in db.Persons on t.SupplierId equals per.PersonID
                                  group new { t, dt, per, p } by p.PurchaseOrderHeaderId into g
                                  orderby g.Max(m => m.t.DocDate) descending, g.Max(m => m.t.DocNo) descending
                                  select new PurchaseOrderHeaderViewModel
                                  {
                                      DocDate = g.Max(m => m.t.DocDate),
                                      DocNo = g.Max(m => m.t.DocNo),
                                      DocTypeName = g.Max(m => m.dt.DocumentTypeName),
                                      PurchaseOrderHeaderId = g.Max(m => m.t.PurchaseOrderHeaderId),
                                      Remark = g.Max(m => m.t.Remark),
                                      SupplierName = g.Max(m => m.per.Name),
                                      Rate = g.Max(m => m.p.Rate),
                                      DiscountPer = g.Max(m => m.p.DiscountPer),
                                  }).Take(5).ToList();




            recordTotal = PurchaseOrders.Count();

            recordFiltered = PurchaseOrders.Count();

            return (from p in PurchaseOrders
                    select new PurchaseOrderHeaderViewModel
                    {
                        sDocDate = p.DocDate.ToString("dd/MMM/yyyy"),
                        DocNo = p.DocNo,
                        DocTypeName = p.DocTypeName,
                        PurchaseOrderHeaderId = p.PurchaseOrderHeaderId,
                        Remark = p.Remark,
                        SupplierName = p.SupplierName,
                        Rate = p.Rate,
                        DiscountPer = p.DiscountPer,
                    }).ToList();

        }

        // here we simulate SQL search, sorting and paging operations
        private List<PurchaseInvoiceHeaderViewModel> InvoiceFilterData(ref int recordFiltered, ref int recordTotal,
            string search, int sortColumn, string sortDirection, int ProductId)
        {

            var PurchaseOrders = (from p in db.PurchaseInvoiceLine
                                  join t in db.PurchaseInvoiceHeader on p.PurchaseInvoiceHeaderId equals t.PurchaseInvoiceHeaderId
                                  join pr in db.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals pr.PurchaseGoodsReceiptLineId
                                  join pl in db.PurchaseOrderLine on pr.PurchaseOrderLineId equals pl.PurchaseOrderLineId
                                  where pl.ProductId == ProductId
                                  join dt in db.DocumentType on t.DocTypeId equals dt.DocumentTypeId
                                  join per in db.Persons on t.SupplierId equals per.PersonID
                                  group new { t, dt, per, p } by p.PurchaseInvoiceHeaderId into g
                                  orderby g.Max(m => m.t.DocDate) descending, g.Max(m => m.t.DocNo) descending
                                  select new PurchaseInvoiceHeaderViewModel
                                  {
                                      DocDate = g.Max(m => m.t.DocDate),
                                      DocNo = g.Max(m => m.t.DocNo),
                                      DocTypeName = g.Max(m => m.dt.DocumentTypeName),
                                      PurchaseInvoiceHeaderId = g.Max(m => m.t.PurchaseInvoiceHeaderId),
                                      Remark = g.Max(m => m.t.Remark),
                                      SupplierName = g.Max(m => m.per.Name),
                                      Rate = g.Max(m => m.p.Rate),
                                      DiscountPer = g.Max(m => m.p.DiscountPer),
                                  }).Take(5).ToList();




            recordTotal = PurchaseOrders.Count();

            recordFiltered = PurchaseOrders.Count();

            return (from p in PurchaseOrders
                    select new PurchaseInvoiceHeaderViewModel
                    {
                        sDocDate = p.DocDate.ToString("dd/MMM/yyyy"),
                        DocNo = p.DocNo,
                        DocTypeName = p.DocTypeName,
                        PurchaseInvoiceHeaderId = p.PurchaseInvoiceHeaderId,
                        Remark = p.Remark,
                        SupplierName = p.SupplierName,
                        Rate = p.Rate,
                        DiscountPer = p.DiscountPer,
                    }).ToList();

        }

        public ActionResult RecordMenu(int id)
        {
            var PurchaseRec = db.PurchaseOrderHeader.Find(id);

            string RedirectUrl = System.Configuration.ConfigurationManager.AppSettings["AdminSetupDomain"] + "/Redirect/RedirectToDocument?DocTypeId=" + PurchaseRec.DocTypeId + "&DocId=" + (PurchaseRec.PurchaseOrderHeaderId);

            return Redirect(RedirectUrl);
        }

        public ActionResult GetProducts(string searchTerm, int pageSize, int pageNum)//DocTypeId
        {
            return new JsonpResult { Data = new dbProductService(db).GetProductHelpListWithNatureType(searchTerm, pageSize, pageNum), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
}
