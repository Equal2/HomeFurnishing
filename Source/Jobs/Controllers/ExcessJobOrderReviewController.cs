using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Data.Models;
using Service;
using Data.Infrastructure;
using Model.ViewModel;
using System.Data.SqlClient;



namespace Jobs.Controllers
{
    [Authorize]
    public class ExcessJobOrderReviewController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        IExcessJobReviewService _JobReviewService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ExcessJobOrderReviewController(IExcessJobReviewService PurchaseOrderHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobReviewService = PurchaseOrderHeaderService;
            _exception = exec;
            _unitOfWork = unitOfWork;
        }


        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("ReviewExcessJobOrders", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public ActionResult ReviewExcessJobOrders(int id)
        {            
            
            ViewBag.id = id;
            return View("ReviewExcessJobOrders");
        }

        public JsonResult AjaxGetJsonData(int draw, int start, int length,int id)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            string SortColName = "";
            if (length == -1)
            {
                length = TOTAL_ROWS;
            }

            // note: we only sort one column at a time
            if (Request.Form["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.Form["order[0][column]"]);
                SortColName = Request.Form["columns[" + sortColumn + "][data]"];
            }
            if (Request.Form["order[0][dir]"] != null)
            {
                sortDirection = Request.Form["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;            
            int recordsFiltered = 0;
            dataTableData.data = FilterData(ref recordsFiltered, ref TOTAL_ROWS, start, length, search, sortColumn, sortDirection, id);
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
            public List<ExcessJobOrderReviewViewModel> data { get; set; }
        }

        // here we simulate data from a database table.
        // !!!!DO NOT DO THIS IN REAL APPLICATION !!!!
        //private static List<DataItem> CreateData()
        //{
        //    Random rnd = new Random();
        //    List<DataItem> list = new List<DataItem>();
        //    for (int i = 1; i <= TOTAL_ROWS; i++)
        //    {
        //        DataItem item = new DataItem();
        //        item.Name = "Name_" + i.ToString().PadLeft(5, '0');
        //        DateTime dob = new DateTime(1900 + rnd.Next(1, 100), rnd.Next(1, 13), rnd.Next(1, 28));
        //        item.Age = ((DateTime.Now - dob).Days / 365).ToString();
        //        item.DoB = dob.ToShortDateString();
        //        list.Add(item);
        //    }
        //    return list;
        //}

        private int SortString(string s1, string s2, string sortDirection)
        {
            return sortDirection == "asc" ? s1.CompareTo(s2) : s2.CompareTo(s1);
        }

        private int SortInteger(string s1, string s2, string sortDirection)
        {
            int i1 = int.Parse(s1);
            int i2 = int.Parse(s2);
            return sortDirection == "asc" ? i1.CompareTo(i2) : i2.CompareTo(i1);
        }

        private int SortDateTime(string s1, string s2, string sortDirection)
        {
            DateTime d1 = DateTime.Parse(s1);
            DateTime d2 = DateTime.Parse(s2);
            return sortDirection == "asc" ? d1.CompareTo(d2) : d2.CompareTo(d1);
        }

        // here we simulate SQL search, sorting and paging operations
        // !!!! DO NOT DO THIS IN REAL APPLICATION !!!!
        private List<ExcessJobOrderReviewViewModel> FilterData(ref int recordFiltered, ref int recordTotal, int start, int length, string search, int sortColumn, string sortDirection, int Id)
        {
            
            SqlParameter SqlParameterDocumentType = new SqlParameter("@DocumentType", Id);            
            SqlParameter SearchString = new SqlParameter { ParameterName = "@SearchString", Value = search };
            SqlParameter pagesize = new SqlParameter { ParameterName = "@PageSize", Value = length };
            SqlParameter PageNo = new SqlParameter { ParameterName = "@PageNo", Value = (start/length) };
            SqlParameter Site = new SqlParameter { ParameterName = "@Site", Value = (int)System.Web.HttpContext.Current.Session["SiteId"] };
            SqlParameter division = new SqlParameter { ParameterName = "@Division", Value = (int)System.Web.HttpContext.Current.Session["DivisionId"] };
            SqlParameter FilteredCount = new SqlParameter { ParameterName = "@FilterRecCount", Value = 0, Direction = ParameterDirection.Output };
            SqlParameter TotalCount = new SqlParameter { ParameterName = "@TotalRecCount", Value = recordTotal, Direction = ParameterDirection.Output };

            IEnumerable<ExcessJobOrderReviewViewModel> PendingJOTReview = db.Database.SqlQuery<ExcessJobOrderReviewViewModel>("[Web].[SpExcessQtyReviewProdOrder] @SearchString, @PageSize, @PageNo, @Site, @Division, @DocumentType, @FilterRecCount out, @TotalRecCount out", SearchString, pagesize, PageNo, Site, division, SqlParameterDocumentType, FilteredCount, TotalCount).ToList();

            recordFiltered = (int)FilteredCount.Value;
            recordTotal = (int)TotalCount.Value;
            return PendingJOTReview.ToList();


            //IQueryable<Product> _data = db.Product;
            //List<ProductViewModel> list = new List<ProductViewModel>();
            //if (string.IsNullOrEmpty(search))
            //{
                
            //}
            //else
            //{
            //    // simulate search
            //    _data = _data.Where(m => (m.ProductName.ToLower().Contains(search.ToLower())) || (m.UnitId.ToLower().Contains(search.ToLower())) || (m.ProductSpecification.ToLower().Contains(search.ToLower())));
            //}

            //// simulate sort
            //if (sortColumn == 0)
            //{// sort Name
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.ProductName) : _data.OrderByDescending(m => m.ProductName);
            //}
            //else if (sortColumn == 1)
            //{// sort Age
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.ProductSpecification) : _data.OrderByDescending(m => m.ProductSpecification);
            //}
            //else if (sortColumn == 2)
            //{   // sort DoB
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.UnitId) : _data.OrderByDescending(m => m.UnitId);
            //}

            //recordFiltered = _data.Count();

            // get just one page of data
            //list = _data.Skip(start).Take(length).Select(m=>new ProductViewModel { DT_RowId=m.ProductId, UnitId=m.UnitId,ProductSpecification=m.ProductSpecification,ProductName=m.ProductName,StandardCost=m.StandardCost}).ToList();

            //return list;

            //var idParam = new SqlParameter { ParameterName = "@CountMatchedRec", Value = 0, Direction = ParameterDirection.Output  };
            //var ids= new SqlParameter { ParameterName = "@Ids", Value = DBNull.Value};
            //var SearchString = new SqlParameter { ParameterName = "@SearchString", Value = search};
            //var pagesize = new SqlParameter { ParameterName = "@PageSize", Value = length};
            //var PageNo = new SqlParameter { ParameterName = "@PageNo", Value = start};
            //var Site = new SqlParameter { ParameterName = "@SiteId", Value = DBNull.Value};
            //var division = new SqlParameter { ParameterName = "@DivisionId", Value = DBNull.Value};
            //string mQry;

            //mQry = " Web.spWizardProducts @SearchString = '" + search + "', @PageSize =" + length.ToString() + ", @PageNo =" + start + ", @SiteId= NULL, @DivisionId= NULL,@CountMatchedRec out";
            //IEnumerable<CustomComboBoxResult> Select2List = db.Database.SqlQuery<CustomComboBoxResult>(" Web.spWizardProducts @Ids, @SearchString, @PageSize, @PageNo, @SiteId, @DivisionId,@CountMatchedRec out",ids,SearchString,pagesize,PageNo,Site,division, idParam).ToList();

            //var tes= Select2List.AsEnumerable().Select(m => new ProductViewModel
            //{
            //    DT_RowId = Convert.ToInt32(m.id),
            //    ProductName = m.text,
            //}).ToList();


            //return tes;

        }

        //public ActionResult ReviewExcessJobOrders(int id)//DocTypeId
        //{

        //    int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
        //    int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
        //    List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
        //    ViewBag.id = id;

        //    //int DocTypeId = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeId;

        //    //Getting Settings
        //    var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId);

        //    if (settings != null)
        //    {
        //        ViewBag.Dim1Visible = settings.isVisibleDimension1;
        //        ViewBag.Dim2Visible = settings.isVisibleDimension2;
        //    }
        //    return View("ReviewExcessJobOrders");
        //}

        public JsonResult PendingJobOrdersToReview(int id)//DocTypeId
        {
            var temp = _JobReviewService.GetExcessJobOrders(id).ToList();
            return Json(new { data = temp }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Approve(int LineId)
        {

            bool Flag = _JobReviewService.ApproveExcessStock(LineId, User.Identity.Name);

            return Json(new { Success = Flag });
        }

        public ActionResult DisApprove(int LineId)
        {
            bool Flag = _JobReviewService.DisApproveExcessStock(LineId, User.Identity.Name);

            return Json(new { Success = Flag });
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
