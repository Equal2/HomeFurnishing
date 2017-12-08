using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Jobs.Helpers;
using Data.Infrastructure;
using System.Net.Http;
using System.IO;
using System.Web.UI.WebControls;
using System.Web.UI;
using Presentation.ViewModels;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Configuration;
using Presentation;
using System.Text;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Model.ViewModel;
using System.Data.SqlClient;



namespace Jobs.Controllers
{
    [Authorize]
    public class UpdatePurchaseOrderExpiryController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        IUpdatePurchaseExpiryService _PurchaseExpiryService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public UpdatePurchaseOrderExpiryController(IUpdatePurchaseExpiryService PurchaseExpiryService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseExpiryService = PurchaseExpiryService;
            _exception = exec;
            _unitOfWork = unitOfWork;
        }


        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("UpdatePurchaseExpiry", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public ActionResult UpdatePurchaseExpiry(int id)
        {
            ViewBag.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.Id = id;
            return View("UpdatePurchaseExpiry");
        }

        public JsonResult AjaxGetJsonData(int draw, int start, int length, int DueDays, bool ShowExpired,int DocType)
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
            dataTableData.data = FilterData(ref recordsFiltered, ref TOTAL_ROWS, start, length, search, sortColumn, sortDirection, DueDays, ShowExpired, DocType);
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
            public List<PurchaseOrderForExpiryViewModel> data { get; set; }
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
        private List<PurchaseOrderForExpiryViewModel> FilterData(ref int recordFiltered, ref int recordTotal, int start, int length, string search, int sortColumn, string sortDirection, int DueDays,bool ShowExpired,int DocType)
        {

            //SqlParameter SqlParameterDocumentType = new SqlParameter("@DocumentType", Id);            
            //SqlParameter SearchString = new SqlParameter { ParameterName = "@SearchString", Value = search };
            //SqlParameter pagesize = new SqlParameter { ParameterName = "@PageSize", Value = length };
            //SqlParameter PageNo = new SqlParameter { ParameterName = "@PageNo", Value = (start/length) };
            //SqlParameter Site = new SqlParameter { ParameterName = "@Site", Value = (int)System.Web.HttpContext.Current.Session["SiteId"] };
            //SqlParameter division = new SqlParameter { ParameterName = "@Division", Value = (int)System.Web.HttpContext.Current.Session["DivisionId"] };
            //SqlParameter FilteredCount = new SqlParameter { ParameterName = "@FilterRecCount", Value = 0, Direction = ParameterDirection.Output };
            //SqlParameter TotalCount = new SqlParameter { ParameterName = "@TotalRecCount", Value = recordTotal, Direction = ParameterDirection.Output };

            //IEnumerable<ExcessPurchaseOrderReviewViewModel> PendingJOTReview = db.Database.SqlQuery<ExcessPurchaseOrderReviewViewModel>("[Web].[SpExcessQtyReviewProdOrder] @SearchString, @PageSize, @PageNo, @Site, @Division, @DocumentType, @FilterRecCount out, @TotalRecCount out", SearchString, pagesize, PageNo, Site, division, SqlParameterDocumentType, FilteredCount, TotalCount).ToList();

            //recordFiltered = (int)FilteredCount.Value;
            //recordTotal = (int)TotalCount.Value;
            //return PendingJOTReview.ToList();

            DateTime ExpiryDate = (DateTime.Now.AddDays(DueDays));

            IQueryable<PurchaseOrderHeader> _data = from p in db.PurchaseOrderHeader
                                                where p.Status != (int)StatusConstants.Complete && p.Status != (int)StatusConstants.Closed && p.DueDate <= ExpiryDate 
                                                && ( !ShowExpired ? p.DueDate >= DateTime.Now : 1==1  ) && p.DocTypeId==DocType
                                                select p;

            recordTotal = _data.Count();
            DateTime Date;
            bool Success = DateTime.TryParse(search, out Date);

            List<PurchaseOrderForExpiryViewModel> list = new List<PurchaseOrderForExpiryViewModel>();
            if (string.IsNullOrEmpty(search))
            {

            }
            else
            {
                // simulate search
                _data = _data.Where(m => !Success ? ((m.DocNo.ToLower().Contains(search.ToLower())) || (m.DocType.DocumentTypeName.ToLower().Contains(search.ToLower()))) : (((m.DocDate == Date)) || ((m.DueDate == Date))));
            }

            // simulate sort
            //if (sortColumn == 0)
            //{// sort Name
            _data = _data.OrderBy(m => m.DocDate).ThenBy(m => m.DocType.DocumentTypeName).ThenBy(m => m.DocNo);
            //}
            //else if (sortColumn == 1)
            //{// sort Age
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.DocNo) : _data.OrderByDescending(m => m.DocNo);
            //}
            //else if (sortColumn == 2)
            //{   // sort DoB
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.DocDate) : _data.OrderByDescending(m => m.DocDate);
            //}
            //else if (sortColumn == 3)
            //{   // sort DoB
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.DueDate) : _data.OrderByDescending(m => m.DueDate);
            //}

            recordFiltered = _data.Count();

            // get just one page of data
            list = _data.Select(m => new PurchaseOrderForExpiryViewModel { DocDate = m.DocDate, DocTypeName = m.DocType.DocumentTypeName, DueDate = m.DueDate, Reason = m.Remark, Revised = (m.DueDate != m.ActualDueDate ? "Yes" : "No"), PurchaseOrderHeaderId = m.PurchaseOrderHeaderId, PurchaseOrderNo = m.DocNo })
                .Skip(start).Take(length).ToList();

            return list.Select(m => new PurchaseOrderForExpiryViewModel { SDocDate = m.DocDate.ToString("dd/MMM/yyyy"), DocTypeName = m.DocTypeName, SDueDate = m.DueDate.Value.ToString("dd/MMM/yyyy"), Reason = m.Reason, Revised = m.Revised, PurchaseOrderHeaderId = m.PurchaseOrderHeaderId, PurchaseOrderNo = m.PurchaseOrderNo }).ToList();

        }

        //public JsonResult PendingPurchaseOrdersToReview(int id)//DocTypeId
        //{
        //    var temp = _PurchaseExpiryService.GetExcessPurchaseOrders(id).ToList();
        //    return Json(new { data = temp }, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult UpdateDueDate(int HeaderId, DateTime DueDate, string Reason)
        {

            bool Flag = _PurchaseExpiryService.UpdatePurchaseOrderExpiry(HeaderId, Reason, User.Identity.Name, DueDate);

            return Json(new { Success = Flag });
        }

        //public ActionResult DisApprove(int LineId)
        //{
        //    bool Flag = _PurchaseExpiryService.DisApproveExcessStock(LineId, User.Identity.Name);

        //    return Json(new { Success = Flag });
        //}

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
