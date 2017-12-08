using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using System.Configuration;
using Presentation.Helper;
using Model.ViewModels;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using Core.Common;



namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class ComboHelpListController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private IComboHelpListService cbl = new ComboHelpListService();

        int LoginSiteId = (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginSiteId];
        int LoginDivisionId = (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDivisionId];


        [HttpGet]
        // Reports Help 
        public ActionResult GetReportType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            //var productCacheKeyHint = "ReportTypeCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductGroupHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper();
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);
            //new AutoCompleteComboBoxRepositoryAndHelper()
            //if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            //List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Detail" });
            prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Product Wise Detail" });

            //int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, 2);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetReportType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductGroup> prod = from p in db.ProductGroups
                                                 where p.ProductGroupId == temp
                                                 select p;


                List<ComboBoxList> prodLst = new List<ComboBoxList>();
                prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Detail" });
                prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Product Wise Detail" });







                //List<SelectListItem> ReportTypeItems = new List<SelectListItem>();
                //ReportTypeItems.Add(new SelectListItem { Text = "Detail", Value = "Detail" });
                //ReportTypeItems.Add(new SelectListItem { Text = "Product Wise Detail", Value = "Product Wise Detail" });

                ProductJson.Add(new ComboBoxResult()
                {
                    id = prodLst.FirstOrDefault().Id.ToString(),
                    text = prodLst.FirstOrDefault().PropFirst.ToString()
                    //id = prod.FirstOrDefault().ProductGroupId.ToString(),
                    //text = prod.FirstOrDefault().ProductGroupName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleReportType(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            //IEnumerable<Product> prod = from p in db.Products
            //                            where p.ProductId == Ids
            //                            select p;

            string Value = "";

            if (Ids == 1)
                Value = "Detail";
            else if (Ids == 2)
                Value = "Product Wise Detail";
            else
                Value = "";

            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = Ids, PropFirst = Value });

            ProductJson.id = prodLst.FirstOrDefault().Id.ToString();
            ProductJson.text = prodLst.FirstOrDefault().PropFirst;

            return Json(ProductJson);
        }

        public ActionResult GetRegisterReportType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            //var productCacheKeyHint = "ReportTypeCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductGroupHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper();
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);
            //new AutoCompleteComboBoxRepositoryAndHelper()
            //if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            //List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Summary" });
            prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Detail" });

            //int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, 2);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetRegisterReportType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductGroup> prod = from p in db.ProductGroups
                                                 where p.ProductGroupId == temp
                                                 select p;


                List<ComboBoxList> prodLst = new List<ComboBoxList>();
                prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Summary" });
                prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Detail" });







                //List<SelectListItem> ReportTypeItems = new List<SelectListItem>();
                //ReportTypeItems.Add(new SelectListItem { Text = "Detail", Value = "Detail" });
                //ReportTypeItems.Add(new SelectListItem { Text = "Product Wise Detail", Value = "Product Wise Detail" });

                ProductJson.Add(new ComboBoxResult()
                {
                    id = prodLst.FirstOrDefault().Id.ToString(),
                    text = prodLst.FirstOrDefault().PropFirst.ToString()
                    //id = prod.FirstOrDefault().ProductGroupId.ToString(),
                    //text = prod.FirstOrDefault().ProductGroupName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleRegisterReportType(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            //IEnumerable<Product> prod = from p in db.Products
            //                            where p.ProductId == Ids
            //                            select p;

            string Value = "";

            if (Ids == 1)
                Value = "Summary";
            else if (Ids == 2)
                Value = "Detail";
            else
                Value = "";

            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = Ids, PropFirst = Value });

            ProductJson.id = prodLst.FirstOrDefault().Id.ToString();
            ProductJson.text = prodLst.FirstOrDefault().PropFirst;

            return Json(ProductJson);
        }

        public ActionResult GetCancelOrderDateFilterOn(string searchTerm, int pageSize, int pageNum)
        {
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper();
            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Order Date" });
            prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Cancel Date" });

            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, 2);

            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetCancelOrderDateFilterOn(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);

                IEnumerable<ProductGroup> prod = from p in db.ProductGroups
                                                 where p.ProductGroupId == temp
                                                 select p;

                List<ComboBoxList> prodLst = new List<ComboBoxList>();
                prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Order Date" });
                prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Cancel Date" });

                ProductJson.Add(new ComboBoxResult()
                {
                    id = prodLst.FirstOrDefault().Id.ToString(),
                    text = prodLst.FirstOrDefault().PropFirst.ToString()
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleCancelOrderDateFilterOn(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();
            string Value = "";

            if (Ids == 1)
                Value = "Order Date";
            else if (Ids == 2)
                Value = "Cance Date";
            else
                Value = "";

            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = Ids, PropFirst = Value });

            ProductJson.id = prodLst.FirstOrDefault().Id.ToString();
            ProductJson.text = prodLst.FirstOrDefault().PropFirst;

            return Json(ProductJson);
        }


        public ActionResult GetAmendmentOrderDateFilterOn(string searchTerm, int pageSize, int pageNum)
        {
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper();
            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Order Date" });
            prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Amendment Date" });

            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, 2);

            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetAmendmentOrderDateFilterOn(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);

                IEnumerable<ProductGroup> prod = from p in db.ProductGroups
                                                 where p.ProductGroupId == temp
                                                 select p;

                List<ComboBoxList> prodLst = new List<ComboBoxList>();
                prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Order Date" });
                prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Amendment Date" });

                ProductJson.Add(new ComboBoxResult()
                {
                    id = prodLst.FirstOrDefault().Id.ToString(),
                    text = prodLst.FirstOrDefault().PropFirst.ToString()
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleAmendmentOrderDateFilterOn(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();
            string Value = "";

            if (Ids == 1)
                Value = "Order Date";
            else if (Ids == 2)
                Value = "Amendment Date";
            else
                Value = "";

            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = Ids, PropFirst = Value });

            ProductJson.id = prodLst.FirstOrDefault().Id.ToString();
            ProductJson.text = prodLst.FirstOrDefault().PropFirst;

            return Json(ProductJson);
        }


        public ActionResult GetPackingRegisterDateFilterOn(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            //var productCacheKeyHint = "ReportTypeCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductGroupHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper();
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);
            //new AutoCompleteComboBoxRepositoryAndHelper()
            //if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            //List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Packing Date" });
            prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Entry Date" });

            //int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, 2);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetPackingRegisterDateFilterOn(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductGroup> prod = from p in db.ProductGroups
                                                 where p.ProductGroupId == temp
                                                 select p;


                List<ComboBoxList> prodLst = new List<ComboBoxList>();
                prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Packing Date" });
                prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Entry Date" });







                //List<SelectListItem> ReportTypeItems = new List<SelectListItem>();
                //ReportTypeItems.Add(new SelectListItem { Text = "Detail", Value = "Detail" });
                //ReportTypeItems.Add(new SelectListItem { Text = "Product Wise Detail", Value = "Product Wise Detail" });

                ProductJson.Add(new ComboBoxResult()
                {
                    id = prodLst.FirstOrDefault().Id.ToString(),
                    text = prodLst.FirstOrDefault().PropFirst.ToString()
                    //id = prod.FirstOrDefault().ProductGroupId.ToString(),
                    //text = prod.FirstOrDefault().ProductGroupName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSinglePackingRegisterDateFilterOn(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            //IEnumerable<Product> prod = from p in db.Products
            //                            where p.ProductId == Ids
            //                            select p;

            string Value = "";

            if (Ids == 1)
                Value = "Packing Date";
            else if (Ids == 2)
                Value = "Entry Date";
            else
                Value = "";

            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = Ids, PropFirst = Value });

            ProductJson.id = prodLst.FirstOrDefault().Id.ToString();
            ProductJson.text = prodLst.FirstOrDefault().PropFirst;

            return Json(ProductJson);
        }

        public ActionResult GetPackingQRBarCodePrintReportType(string searchTerm, int pageSize, int pageNum)
        {

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper();
            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Print For SCI" });
            prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Print For HDC" });
            prodLst.Add(new ComboBoxList() { Id = 3, PropFirst = "Print For Artistic Weavers" });

            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, 2);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetPackingQRBarCodePrintReportType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);

                List<ComboBoxList> prodLst = new List<ComboBoxList>();
                prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Print For SCI" });
                prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Print For HDC" });
                prodLst.Add(new ComboBoxList() { Id = 3, PropFirst = "Print For Artistic Weavers" });

                ProductJson.Add(new ComboBoxResult()
                {
                    id = prodLst.FirstOrDefault().Id.ToString(),
                    text = prodLst.FirstOrDefault().PropFirst.ToString()
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSinglePackingQRBarCodePrintReportType(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();
            string Value = "";

            if (Ids == 1)
                Value = "Print For SCI";
            else if (Ids == 2)
                Value = "Print For HDC";
            else if (Ids == 3)
                Value = "Print For Artistic Weavers";
            else
                Value = "";

            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = Ids, PropFirst = Value });

            ProductJson.id = prodLst.FirstOrDefault().Id.ToString();
            ProductJson.text = prodLst.FirstOrDefault().PropFirst;

            return Json(ProductJson);
        }


        public ActionResult GetTransactionStatusType(string searchTerm, int pageSize, int pageNum)
        {

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper();
            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = (int)StatusConstants.Drafted, PropFirst = "Drafted" });
            prodLst.Add(new ComboBoxList() { Id = (int)StatusConstants.Submitted, PropFirst = "Submitted" });
            prodLst.Add(new ComboBoxList() { Id = (int)StatusConstants.Approved, PropFirst = "Approved" });

            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, 2);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetTransactionStatusType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);

                List<ComboBoxList> prodLst = new List<ComboBoxList>();
                prodLst.Add(new ComboBoxList() { Id = (int)StatusConstants.Drafted, PropFirst = "Drafted" });
                prodLst.Add(new ComboBoxList() { Id = (int)StatusConstants.Submitted, PropFirst = "Submitted" });
                prodLst.Add(new ComboBoxList() { Id = (int)StatusConstants.Approved, PropFirst = "Approved" });

                ProductJson.Add(new ComboBoxResult()
                {
                    id = prodLst.FirstOrDefault().Id.ToString(),
                    text = prodLst.FirstOrDefault().PropFirst.ToString()
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleTransactionStatusType(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();
            string Value = "";

            if (Ids == (int)StatusConstants.Drafted)
                Value = "Drafted";
            else if (Ids == (int)StatusConstants.Submitted)
                Value = "Submitted";
            else if (Ids == (int)StatusConstants.Approved)
                Value = "Approved";
            else
                Value = "";

            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = Ids, PropFirst = Value });

            ProductJson.id = prodLst.FirstOrDefault().Id.ToString();
            ProductJson.text = prodLst.FirstOrDefault().PropFirst;

            return Json(ProductJson);
        }


        public ActionResult GetReportFor(string searchTerm, int pageSize, int pageNum)
        {

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper();

            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "All" });
            prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Pending" });

            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, 2);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetBarcodeGenerateDocId(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "BarcodeCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetBarcodeGenerateDocIdHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }



        public JsonResult SetSingleBarcodeGenerateDocId(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductUid> prod = from p in db.ProductUid
                                           where p.ProductUIDId == Ids
                                           select p;

            ProductJson.id = prod.FirstOrDefault().ProductUIDId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductUidName;

            return Json(ProductJson);
        }

        public JsonResult SetReportFor(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);

                List<ComboBoxList> prodLst = new List<ComboBoxList>();
                prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "All" });
                prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Pending" });

                ProductJson.Add(new ComboBoxResult()
                {
                    id = prodLst.FirstOrDefault().Id.ToString(),
                    text = prodLst.FirstOrDefault().PropFirst.ToString()
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleReportFor(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            string Value = "";

            if (Ids == 1)
                Value = "All";
            else if (Ids == 2)
                Value = "Pending";
            else
                Value = "";

            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = Ids, PropFirst = Value });

            ProductJson.id = prodLst.FirstOrDefault().Id.ToString();
            ProductJson.text = prodLst.FirstOrDefault().PropFirst;

            return Json(ProductJson);
        }

        public ActionResult GetReportSummaryType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            //var productCacheKeyHint = "ReportTypeCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 


            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper();

            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Month Wise Summary" });
            prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Party Wise Summary" });
            prodLst.Add(new ComboBoxList() { Id = 3, PropFirst = "Product Wise Summary" });
            prodLst.Add(new ComboBoxList() { Id = 4, PropFirst = "Product Group Wise Summary" });
            prodLst.Add(new ComboBoxList() { Id = 5, PropFirst = "Product Category Wise Summary" });
            prodLst.Add(new ComboBoxList() { Id = 6, PropFirst = "Product Type Wise Summary" });

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, 2);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetReportSummaryType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                List<ComboBoxList> prodLst = new List<ComboBoxList>();
                prodLst.Add(new ComboBoxList() { Id = 1, PropFirst = "Month Wise Summary" });
                prodLst.Add(new ComboBoxList() { Id = 2, PropFirst = "Party Wise Summary" });
                prodLst.Add(new ComboBoxList() { Id = 3, PropFirst = "Product Wise Summary" });
                prodLst.Add(new ComboBoxList() { Id = 4, PropFirst = "Product Group Wise Summary" });
                prodLst.Add(new ComboBoxList() { Id = 5, PropFirst = "Product Category Wise Summary" });
                prodLst.Add(new ComboBoxList() { Id = 6, PropFirst = "Product Type Wise Summary" });

                ProductJson.Add(new ComboBoxResult()
                {
                    id = prodLst.FirstOrDefault().Id.ToString(),
                    text = prodLst.FirstOrDefault().PropFirst.ToString()
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleReportSummaryType(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            string Value = "";

            if (Ids == 1)
                Value = "Month Wise Summary";
            else if (Ids == 2)
                Value = "Party Wise Summary";
            else if (Ids == 3)
                Value = "Product Wise Summary";
            else if (Ids == 4)
                Value = "Product Group Wise Summary";
            else if (Ids == 5)
                Value = "Product Category Wise Summary";
            else if (Ids == 6)
                Value = "Product Type Wise Summary";
            else
                Value = "";

            List<ComboBoxList> prodLst = new List<ComboBoxList>();
            prodLst.Add(new ComboBoxList() { Id = Ids, PropFirst = Value });

            ProductJson.id = prodLst.FirstOrDefault().Id.ToString();
            ProductJson.text = prodLst.FirstOrDefault().PropFirst;

            return Json(ProductJson);
        }



        // General Data Help 
        public ActionResult GetSite(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSite";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSiteHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSite(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Site> prod = from p in db.Site
                                         where p.SiteId == temp
                                         select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().SiteId.ToString(),
                    text = prod.FirstOrDefault().SiteCode
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleSite(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            string prod = (from p in db.Site
                           where p.SiteId == Ids
                           select p.SiteName).FirstOrDefault();

            ProductJson.id = Ids.ToString();
            ProductJson.text = prod;

            return Json(ProductJson);
        }

        public ActionResult GetDivision(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDivision";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDivisionHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetDivision(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Division> prod = from p in db.Divisions
                                             where p.DivisionId == temp
                                             select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DivisionId.ToString(),
                    text = prod.FirstOrDefault().DivisionName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleDivision(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            string prod = (from p in db.Divisions
                           where p.DivisionId == Ids
                           select p.DivisionName).FirstOrDefault();

            ProductJson.id = Ids.ToString();
            ProductJson.text = prod;

            return Json(ProductJson);
        }

        public ActionResult GetBuyers(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["BuyerCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetBuyerHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetBuyers(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);                        
                //IEnumerable<Person> prod = (from b in db.Buyer
                //                           join p in db.Persons on b.PersonID equals p.PersonID into PersonTable from PersonTab in PersonTable.DefaultIfEmpty()
                //                           where b.PersonID == temp
                //                           select new Person
                //                           {
                //                               PersonID = b.PersonID,
                //                               Name = PersonTab.Name
                //                           });
                IEnumerable<PersonViewModel> prod = (from b in db.Persons
                                                     where b.PersonID == temp
                                                     select new PersonViewModel
                                                     {
                                                         PersonID = b.PersonID,
                                                         Name = b.Name
                                                     });
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PersonID.ToString(),
                    text = prod.FirstOrDefault().Name
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleBuyer(int Ids)
        {
            ComboBoxResult BuyerJson = new ComboBoxResult();

            PersonViewModel person = (from p in db.Persons 
                                      where p.PersonID == Ids
                                      select new PersonViewModel
                                      {
                                          PersonID = p.PersonID,
                                          Name = p.Name
                                      }).FirstOrDefault();

            BuyerJson.id = person.PersonID.ToString();
            BuyerJson.text = person.Name;

            return Json(BuyerJson);
        }

        public ActionResult GetSuppliers(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["SupplierCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSupplierHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSuppliers(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);                        
                //IEnumerable<Person> prod = (from b in db.Supplier
                //                            join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                //                            from PersonTab in PersonTable.DefaultIfEmpty()
                //                            where b.PersonID == temp
                //                            select new Person
                //                            {
                //                                PersonID = b.PersonID,
                //                                Name = PersonTab.Name
                //                            });
                IEnumerable<PersonViewModel> prod = (from b in db.Persons
                                                     where b.PersonID == temp
                                                     select new PersonViewModel
                                                     {
                                                         PersonID = b.PersonID,
                                                         Name = b.Name
                                                     });
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PersonID.ToString(),
                    text = prod.FirstOrDefault().Name
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleSupplier(int Ids)
        {
            ComboBoxResult SupplierJson = new ComboBoxResult();

            PersonViewModel person = (from b in db.Supplier
                                      join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                                      from PersonTab in PersonTable.DefaultIfEmpty()
                                      where b.PersonID == Ids
                                      select new PersonViewModel
                                      {
                                          PersonID = b.PersonID,
                                          Name = PersonTab.Name
                                      }).FirstOrDefault();

            SupplierJson.id = person.PersonID.ToString();
            SupplierJson.text = person.Name;

            return Json(SupplierJson);
        }


        public ActionResult GetJobWorker_Packing(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["JobWorkerCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobWorkerHelpList_WithProcess(20), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }



        public ActionResult GetJobWorkers(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["JobWorkerCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobWorkerHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetJobWorkers(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);

                IEnumerable<PersonViewModel> prod = (from b in db.Persons
                                                     where b.PersonID == temp
                                                     select new PersonViewModel
                                                     {
                                                         PersonID = b.PersonID,
                                                         Name = b.Name
                                                     });
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PersonID.ToString(),
                    text = prod.FirstOrDefault().Name
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleJobWorker(int Ids)
        {
            ComboBoxResult JobWorkerJson = new ComboBoxResult();

            PersonViewModel person = (from b in db.JobWorker
                                      join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                                      from PersonTab in PersonTable.DefaultIfEmpty()
                                      where b.PersonID == Ids
                                      select new PersonViewModel
                                      {
                                          PersonID = b.PersonID,
                                          Name = PersonTab.Name
                                      }).FirstOrDefault();

            JobWorkerJson.id = person.PersonID.ToString();
            JobWorkerJson.text = person.Name;

            return Json(JobWorkerJson);
        }






        public ActionResult GetEmployees(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["EmployeeCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetEmployeeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetEmployees(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);

                IEnumerable<PersonViewModel> prod = (from b in db.Persons
                                                     where b.PersonID == temp
                                                     select new PersonViewModel
                                                     {
                                                         PersonID = b.PersonID,
                                                         Name = b.Name
                                                     });
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PersonID.ToString(),
                    text = prod.FirstOrDefault().Name
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleEmployee(int Ids)
        {
            ComboBoxResult EmployeeJson = new ComboBoxResult();

            PersonViewModel person = (from b in db.Persons
                                      where b.PersonID == Ids
                                      select new PersonViewModel
                                      {
                                          PersonID = b.PersonID,
                                          Name = b.Name
                                      }).FirstOrDefault();

            EmployeeJson.id = person.PersonID.ToString();
            EmployeeJson.text = person.Name;

            return Json(EmployeeJson);
        }








        public ActionResult GetCurrencys(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["CurrencyCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetCurrencyHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetCurrencys(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Currency> prod = from p in db.Currency
                                             where p.ID == temp
                                             select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ID.ToString(),
                    text = prod.FirstOrDefault().Name
                });
            }
            return Json(ProductJson);
        }



        public ActionResult GetCity(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["CityCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetCityHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleCity(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<City> prod = from p in db.City
                                     where p.CityId == Ids
                                     select p;

            ProductJson.id = prod.FirstOrDefault().CityId.ToString();
            ProductJson.text = prod.FirstOrDefault().CityName;

            return Json(ProductJson);
        }

        public ActionResult GetState(string searchTerm, int pageSize, int pageNum)
        {

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetStateHelpList(), "CacheState", RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleState(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<State> prod = from p in db.State
                                      where p.StateId == Ids
                                      select p;

            ProductJson.id = prod.FirstOrDefault().StateId.ToString();
            ProductJson.text = prod.FirstOrDefault().StateName;

            return Json(ProductJson);
        }


        public ActionResult GetCountry(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["CountryCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetCountryHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleCountry(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Country> prod = from p in db.Country
                                        where p.CountryId == Ids
                                        select p;

            ProductJson.id = prod.FirstOrDefault().CountryId.ToString();
            ProductJson.text = prod.FirstOrDefault().CountryName;

            return Json(ProductJson);
        }



        public ActionResult GetPerson(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["PersonCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPersonHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetPersonBE(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["PersonBECacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPersonHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePerson(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Person> prod = from p in db.Persons
                                       where p.PersonID == Ids
                                       select p;

            ProductJson.id = prod.FirstOrDefault().PersonID.ToString();
            ProductJson.text = prod.FirstOrDefault().Name;

            return Json(ProductJson);
        }

        public ActionResult GetPersonRateGroup(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["PersonRateGroupCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPersonRateGroupHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePersonRateGroup(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<PersonRateGroup> prod = from p in db.PersonRateGroup
                                                where p.PersonRateGroupId == Ids
                                                select p;

            ProductJson.id = prod.FirstOrDefault().PersonRateGroupId.ToString();
            ProductJson.text = prod.FirstOrDefault().PersonRateGroupName;

            return Json(ProductJson);
        }

        public ActionResult GetAccountGroup(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["AccountGroupCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetAccountGroupHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleAccountGroup(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<LedgerAccountGroup> prod = from p in db.LedgerAccountGroup
                                                   where p.LedgerAccountGroupId == Ids
                                                   select p;

            ProductJson.id = prod.FirstOrDefault().LedgerAccountGroupId.ToString();
            ProductJson.text = prod.FirstOrDefault().LedgerAccountGroupName;

            return Json(ProductJson);
        }

        public ActionResult GetAccount(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["AccountCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetAccountHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleAccount(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<LedgerAccount> prod = from p in db.LedgerAccount
                                              where p.LedgerAccountId == Ids
                                              select p;

            ProductJson.id = prod.FirstOrDefault().LedgerAccountId.ToString();
            ProductJson.text = prod.FirstOrDefault().LedgerAccountName;

            return Json(ProductJson);
        }

        public JsonResult SetAccount(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<LedgerAccount> prod = from p in db.LedgerAccount
                                                  where p.LedgerAccountId == temp
                                                  select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().LedgerAccountId.ToString(),
                    text = prod.FirstOrDefault().LedgerAccountName
                });
            }
            return Json(ProductJson);
        }
        public ActionResult GetCostCenter(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["CostCenterCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetCostCenterHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleCostCenter(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<CostCenter> prod = from p in db.CostCenter
                                           where p.CostCenterId == Ids
                                           select p;

            ProductJson.id = prod.FirstOrDefault().CostCenterId.ToString();
            ProductJson.text = prod.FirstOrDefault().CostCenterName;

            return Json(ProductJson);
        }

        public JsonResult SetCostCenter(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);

                IEnumerable<CostCenter> prod = from p in db.CostCenter
                                               where p.CostCenterId == temp
                                               select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().CostCenterId.ToString(),
                    text = prod.FirstOrDefault().CostCenterName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetLedgerAccountGroups(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "AccountGroupCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetAccountGroupsHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetLedgerAccountGroups(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<LedgerAccountGroup> prod = from p in db.LedgerAccountGroup
                                                       where p.LedgerAccountGroupId == temp
                                                       select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().LedgerAccountGroupId.ToString(),
                    text = prod.FirstOrDefault().LedgerAccountGroupName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleLedgerAccountGroup(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<LedgerAccountGroup> prod = from p in db.LedgerAccountGroup
                                                   where p.LedgerAccountGroupId == Ids
                                                   select p;

            ProductJson.id = prod.FirstOrDefault().LedgerAccountGroupId.ToString();
            ProductJson.text = prod.FirstOrDefault().LedgerAccountGroupName;

            return Json(ProductJson);
        }

        public ActionResult GetGodown(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "GodownCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetGodownHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetGodown(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Godown> prod = from p in db.Godown
                                           where p.GodownId == temp
                                           select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().GodownId.ToString(),
                    text = prod.FirstOrDefault().GodownName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleGodown(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Godown> prod = from p in db.Godown
                                       where p.GodownId == Ids
                                       select p;

            ProductJson.id = prod.FirstOrDefault().GodownId.ToString();
            ProductJson.text = prod.FirstOrDefault().GodownName;

            return Json(ProductJson);
        }

        public ActionResult GetCalculation(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CalculationCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetCalculationHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetCalculation(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Calculation> prod = from p in db.Calculation
                                                where p.CalculationId == temp
                                                select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().CalculationId.ToString(),
                    text = prod.FirstOrDefault().CalculationName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleCalculation(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Calculation> prod = from p in db.Calculation
                                            where p.CalculationId == Ids
                                            select p;

            ProductJson.id = prod.FirstOrDefault().CalculationId.ToString();
            ProductJson.text = prod.FirstOrDefault().CalculationName;

            return Json(ProductJson);
        }


        public ActionResult GetPerk(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PerkCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPerkHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetPerk(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Perk> prod = from p in db.Perk
                                         where p.PerkId == temp
                                         select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PerkId.ToString(),
                    text = prod.FirstOrDefault().PerkName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSinglePerk(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Perk> prod = from p in db.Perk
                                     where p.PerkId == Ids
                                     select p;

            ProductJson.id = prod.FirstOrDefault().PerkId.ToString();
            ProductJson.text = prod.FirstOrDefault().PerkName;

            return Json(ProductJson);
        }

        public ActionResult GetDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DocumentTypeCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetDocumentType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<DocumentType> prod = from p in db.DocumentType
                                                 where p.DocumentTypeId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentTypeId.ToString(),
                    text = prod.FirstOrDefault().DocumentTypeName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleDocumentType(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<DocumentType> prod = from p in db.DocumentType
                                             where p.DocumentTypeId == Ids
                                             select p;

            ProductJson.id = prod.FirstOrDefault().DocumentTypeId.ToString();
            ProductJson.text = prod.FirstOrDefault().DocumentTypeName;

            return Json(ProductJson);
        }



        public ActionResult GetDocumentCategory(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DocumentCategoryCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentCategoryHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetDocumentCategory(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<DocumentCategory> prod = from p in db.DocumentCategory
                                                     where p.DocumentCategoryId == temp
                                                     select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentCategoryId.ToString(),
                    text = prod.FirstOrDefault().DocumentCategoryName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleDocumentCategory(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<DocumentCategory> prod = from p in db.DocumentCategory
                                                 where p.DocumentCategoryId == Ids
                                                 select p;

            ProductJson.id = prod.FirstOrDefault().DocumentCategoryId.ToString();
            ProductJson.text = prod.FirstOrDefault().DocumentCategoryName;

            return Json(ProductJson);
        }


        public ActionResult GetTransporters(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["TransporterCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetTransporterHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetTransporters(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);                        
                IEnumerable<Person> prod = (from b in db.Transporter
                                            join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                                            from PersonTab in PersonTable.DefaultIfEmpty()
                                            where b.PersonID == temp
                                            select new Person
                                            {
                                                PersonID = b.PersonID,
                                                Name = PersonTab.Name
                                            });
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PersonID.ToString(),
                    text = prod.FirstOrDefault().Name
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleTransporter(int Ids)
        {
            ComboBoxResult TransporterJson = new ComboBoxResult();

            PersonViewModel person = (from b in db.Transporter
                                      join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                                      from PersonTab in PersonTable.DefaultIfEmpty()
                                      where b.PersonID == Ids
                                      select new PersonViewModel
                                      {
                                          PersonID = b.PersonID,
                                          Name = PersonTab.Name
                                      }).FirstOrDefault();

            TransporterJson.id = person.PersonID.ToString();
            TransporterJson.text = person.Name;

            return Json(TransporterJson);
        }

        public ActionResult GetRoutes(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["RouteCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetRouteHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetRoutes(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);                        
                IEnumerable<Route> prod = (from b in db.Route
                                           where b.RouteId == temp
                                           select new Route
                                           {
                                               RouteId = b.RouteId,
                                               RouteName = b.RouteName
                                           });
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().RouteId.ToString(),
                    text = prod.FirstOrDefault().RouteName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleRoute(int Ids)
        {
            ComboBoxResult RouteJson = new ComboBoxResult();

            var route = (from b in db.Route
                         where b.RouteId == Ids
                         select new
                         {
                             RouteId = b.RouteId,
                             RouteName = b.RouteName
                         }).FirstOrDefault();

            RouteJson.id = route.RouteId.ToString();
            RouteJson.text = route.RouteName;

            return Json(RouteJson);
        }

        public ActionResult GetDepartment(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDepartment";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDepartmentHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetDepartment(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Department> prod = from p in db.Department
                                               where p.DepartmentId == temp
                                               select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DepartmentId.ToString(),
                    text = prod.FirstOrDefault().DepartmentName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleDepartment(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Department> prod = from p in db.Department
                                           where p.DepartmentId == Ids
                                           select p;

            ProductJson.id = prod.FirstOrDefault().DepartmentId.ToString();
            ProductJson.text = prod.FirstOrDefault().DepartmentName;

            return Json(ProductJson);
        }




        // Products Help 
        public ActionResult GetTag(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheTag";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetTagHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetTag(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Tag> prod = from p in db.Tag
                                        where p.TagId == temp
                                        select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().TagId.ToString(),
                    text = prod.FirstOrDefault().TagName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetProducts(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 


            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public ActionResult GetBom(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["BOMCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 


            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetBomHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }



        public ActionResult GetFinishedProductDivisionWise(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["FinishedProductDivisionWiseCacheKeyHint"];

            //THis statement has been changed because GetFinishedProductDivisionWiseHelpList was calling again and again. 


            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetFinishedProductDivisionWiseHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProducts(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Product> prod = from p in db.Product
                                        where p.ProductId == Ids
                                        select p;

            ProductJson.id = prod.FirstOrDefault().ProductId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductName;

            return Json(ProductJson);
        }

        public JsonResult SetProducts(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Product> prod = from p in db.Product
                                            where p.ProductId == temp
                                            select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductId.ToString(),
                    text = prod.FirstOrDefault().ProductName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleProductCodes(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Product> prod = from p in db.Product
                                        where p.ProductId == Ids
                                        select p;

            ProductJson.id = prod.FirstOrDefault().ProductId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductCode;

            return Json(ProductJson);
        }

        public ActionResult GetProductUids(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductUidCacheKeyHint
            var ProductUidCacheKeyHint = ConfigurationManager.AppSettings["ProductUidCacheKeyHint"];

            //THis statement has been changed because GetProductUidHelpList was calling again and again. 


            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductUidHelpList(), ProductUidCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProductUids(int Ids)
        {
            ComboBoxResult ProductUidJson = new ComboBoxResult();

            IEnumerable<ProductUid> prod = from p in db.ProductUid
                                           where p.ProductUIDId == Ids
                                           select p;

            ProductUidJson.id = prod.FirstOrDefault().ProductUIDId.ToString();
            ProductUidJson.text = prod.FirstOrDefault().ProductUidName;

            return Json(ProductUidJson);
        }

        public JsonResult SetProductUids(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductUidJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<ProductUid> prod = from p in db.ProductUid
                                               where p.ProductUIDId == temp
                                               select p;
                ProductUidJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductUIDId.ToString(),
                    text = prod.FirstOrDefault().ProductUidName
                });
            }
            return Json(ProductUidJson);
        }

        public ActionResult GetProductTags(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductTagCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 


            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProductTags(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Product> prod = from p in db.Product
                                        where p.ProductId == Ids
                                        select p;

            ProductJson.id = prod.FirstOrDefault().ProductId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductName + " | " + prod.FirstOrDefault().UnitId + " | " + prod.FirstOrDefault().ProductCode;

            return Json(ProductJson);
        }

        public JsonResult SetProductTags(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Product> prod = from p in db.Product
                                            where p.ProductId == temp
                                            select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductId.ToString(),
                    text = prod.FirstOrDefault().ProductName + " | " + prod.FirstOrDefault().UnitId + " | " + prod.FirstOrDefault().ProductCode
                });
            }
            return Json(ProductJson);
        }



        public ActionResult GetProductGroup(string searchTerm, int pageSize, int pageNum, int? filter)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductGroupCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductGroupHelpList(filter), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetProductGroupForRug(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductGroupCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductGroupForRugHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductGroup(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductGroup> prod = from p in db.ProductGroups
                                                 where p.ProductGroupId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductGroupId.ToString(),
                    text = prod.FirstOrDefault().ProductGroupName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleProductGroup(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductGroup> prod = from p in db.ProductGroups
                                             where p.ProductGroupId == Ids
                                             select p;

            ProductJson.id = prod.FirstOrDefault().ProductGroupId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductGroupName;

            return Json(ProductJson);
        }

        public ActionResult GetProductType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductTypeCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductTypeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductType> prod = from p in db.ProductTypes
                                                where p.ProductTypeId == temp
                                                select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductTypeId.ToString(),
                    text = prod.FirstOrDefault().ProductTypeName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetProductCollection(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductCollectionCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductCollectionHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductCollection(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);                        
                IEnumerable<ProductCollection> prod = from p in db.ProductCollections
                                                      where p.ProductCollectionId == temp
                                                      select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductCollectionId.ToString(),
                    text = prod.FirstOrDefault().ProductCollectionName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleProductCollection(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductCollection> prod = from p in db.ProductCollections
                                                  where p.ProductCollectionId == Ids
                                                  select p;

            ProductJson.id = prod.FirstOrDefault().ProductCollectionId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductCollectionName;

            return Json(ProductJson);
        }

        public ActionResult GetProductNature(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductNature";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductNatureHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductNature(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductNature> prod = from p in db.ProductNature
                                                  where p.ProductNatureId == temp
                                                  select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductNatureId.ToString(),
                    text = prod.FirstOrDefault().ProductNatureName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetProductCategory(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductCategory";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductCategoryHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductCategory(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductCategory> prod = from p in db.ProductCategory
                                                    where p.ProductCategoryId == temp
                                                    select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductCategoryId.ToString(),
                    text = prod.FirstOrDefault().ProductCategoryName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleProductCategory(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductCategory> prod = from p in db.ProductCategory
                                                where p.ProductCategoryId == Ids
                                                select p;

            ProductJson.id = prod.FirstOrDefault().ProductCategoryId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductCategoryName;

            return Json(ProductJson);
        }

        public ActionResult GetProductQuality(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductQuality";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductQualityHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductQuality(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductType> prod = from p in db.ProductTypes
                                                where p.ProductTypeId == temp
                                                select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductTypeId.ToString(),
                    text = prod.FirstOrDefault().ProductTypeName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetProductDesign(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductDesign";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductDesignHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductDesign(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductDesign> prod = from p in db.ProductDesigns
                                                  where p.ProductDesignId == temp
                                                  select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductDesignId.ToString(),
                    text = prod.FirstOrDefault().ProductDesignName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetProductStyle(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductStyle";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductStyleHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductStyle(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductStyle> prod = from p in db.ProductStyle
                                                 where p.ProductStyleId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductStyleId.ToString(),
                    text = prod.FirstOrDefault().ProductStyleName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetProductShape(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductShape";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductShapeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductShape(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductShape> prod = from p in db.ProductShape
                                                 where p.ProductShapeId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductShapeId.ToString(),
                    text = prod.FirstOrDefault().ProductShapeName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetProductSize(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductSize";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductSizeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductSize(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                var prod = from p in db.Size
                           join ps in db.ProductShape on p.ProductShapeId equals ps.ProductShapeId into ProductShapeTable
                           from ProductShapeTab in ProductShapeTable.DefaultIfEmpty()
                           where p.SizeId == temp
                           select new
                           {
                               SizeId = p.SizeId,
                               SizeName = p.SizeName + ProductShapeTab.ProductShapeShortName ?? ""
                           };
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().SizeId.ToString(),
                    text = prod.FirstOrDefault().SizeName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetProductInvoiceGroup(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductInvoiceGroup";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductInvoiceGroupHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductInvoiceGroup(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductInvoiceGroup> prod = from p in db.ProductInvoiceGroup
                                                        where p.ProductInvoiceGroupId == temp
                                                        select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductInvoiceGroupId.ToString(),
                    text = prod.FirstOrDefault().ProductInvoiceGroupName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetProductCustomGroup(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductCustomGroup";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductCustomGroupHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductCustomGroup(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductCustomGroupHeader> prod = from p in db.ProductCustomGroupHeader
                                                             where p.ProductCustomGroupId == temp
                                                             select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductCustomGroupId.ToString(),
                    text = prod.FirstOrDefault().ProductCustomGroupName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleProductQuality(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductQuality> prod = from p in db.ProductQuality
                                               where p.ProductQualityId == Ids
                                               select p;

            ProductJson.id = prod.FirstOrDefault().ProductQualityId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductQualityName;

            return Json(ProductJson);
        }

        public JsonResult SetSingleProductDesign(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductDesign> prod = from p in db.ProductDesigns
                                              where p.ProductDesignId == Ids
                                              select p;

            ProductJson.id = prod.FirstOrDefault().ProductDesignId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductDesignName;

            return Json(ProductJson);
        }



        public JsonResult SetSingleProductStyle(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductStyle> prod = from p in db.ProductStyle
                                             where p.ProductStyleId == Ids
                                             select p;

            ProductJson.id = prod.FirstOrDefault().ProductStyleId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductStyleName;

            return Json(ProductJson);
        }

        public ActionResult GetProductManufacturer(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductManufacturerCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductManufacturerHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProductManufacturer(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Person> prod = from p in db.Persons
                                       where p.PersonID == Ids
                                       select p;

            ProductJson.id = prod.FirstOrDefault().PersonID.ToString();
            ProductJson.text = prod.FirstOrDefault().Name;

            return Json(ProductJson);
        }

        public ActionResult GetProductDrawBackTarrif(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductDrawBackTarrifCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductDrawBackTarrifHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProductDrawBackTarrif(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<DrawBackTariffHead> prod = from p in db.DrawBackTariffHead
                                                   where p.DrawBackTariffHeadId == Ids
                                                   select p;

            ProductJson.id = prod.FirstOrDefault().DrawBackTariffHeadId.ToString();
            ProductJson.text = prod.FirstOrDefault().DrawBackTariffHeadName;

            return Json(ProductJson);
        }

        public ActionResult GetProductProcessSequence(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductProcessSequenceCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductProcessSequenceHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProductProcessSequence(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProcessSequenceHeader> prod = from p in db.ProcessSequenceHeader
                                                      where p.ProcessSequenceHeaderId == Ids
                                                      select p;

            ProductJson.id = prod.FirstOrDefault().ProcessSequenceHeaderId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProcessSequenceHeaderName;

            return Json(ProductJson);
        }

        public ActionResult GetSize(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["SizeCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSizeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }



        public JsonResult SetSingleSize(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            //IEnumerable<Size> prod = from p in db.Size
            //                         where p.SizeId == Ids
            //                         select p;

            var prod = from p in db.Size
                       join ps in db.ProductShape on p.ProductShapeId equals ps.ProductShapeId into ProductShapeTable
                       from ProductShapeTab in ProductShapeTable.DefaultIfEmpty()
                       where p.SizeId == Ids
                       select new
                       {
                           SizeId = p.SizeId,
                           SizeName = p.SizeName + ProductShapeTab.ProductShapeShortName ?? ""
                       };

            ProductJson.id = prod.FirstOrDefault().SizeId.ToString();
            ProductJson.text = prod.FirstOrDefault().SizeName;

            return Json(ProductJson);
        }

        public ActionResult GetProductConstruction(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductCategory";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductConstructionHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductConstruction(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductCategory> prod = from p in db.ProductCategory
                                                    where p.ProductCategoryId == temp
                                                    select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductCategoryId.ToString(),
                    text = prod.FirstOrDefault().ProductCategoryName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleProductConstruction(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductCategory> prod = from p in db.ProductCategory
                                                where p.ProductCategoryId == Ids
                                                select p;

            ProductJson.id = prod.FirstOrDefault().ProductCategoryId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductCategoryName;

            return Json(ProductJson);
        }

        public ActionResult GetRugCollection(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductCollectionCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetRugCollectionHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetRugCollection(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);                        
                IEnumerable<ProductCollection> prod = from p in db.ProductCollections
                                                      where p.ProductCollectionId == temp
                                                      select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductCollectionId.ToString(),
                    text = prod.FirstOrDefault().ProductCollectionName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleRugCollection(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductCollection> prod = from p in db.ProductCollections
                                                  where p.ProductCollectionId == Ids
                                                  select p;

            ProductJson.id = prod.FirstOrDefault().ProductCollectionId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductCollectionName;

            return Json(ProductJson);
        }

        public ActionResult GetRugQuality(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductQuality";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetRugQualityHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetRugQuality(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductType> prod = from p in db.ProductTypes
                                                where p.ProductTypeId == temp
                                                select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductTypeId.ToString(),
                    text = prod.FirstOrDefault().ProductTypeName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleRugQuality(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductQuality> prod = from p in db.ProductQuality
                                               where p.ProductQualityId == Ids
                                               select p;

            ProductJson.id = prod.FirstOrDefault().ProductQualityId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductQualityName;

            return Json(ProductJson);
        }

        public ActionResult GetProcess(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProcessCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProcessHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProcess(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Process> prod = from p in db.Process
                                        where p.ProcessId == Ids
                                        select p;

            ProductJson.id = prod.FirstOrDefault().ProcessId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProcessName;

            return Json(ProductJson);
        }

        public JsonResult SetProcess(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Process> prod = from p in db.Process
                                            where p.ProcessId == temp
                                            select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProcessId.ToString(),
                    text = prod.FirstOrDefault().ProcessName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetMachine(string searchTerm, int pageSize, int pageNum)
        {
            var productCacheKeyHint = ConfigurationManager.AppSettings["MachineCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetMachineHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleMachine(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Product> prod = from p in db.Product
                                        where p.ProductId == Ids
                                        select p;

            ProductJson.id = prod.FirstOrDefault().ProductId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductName;

            return Json(ProductJson);
        }

        public JsonResult SetMachine(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Product> prod = from p in db.Product
                                            where p.ProductId == temp
                                            select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductId.ToString(),
                    text = prod.FirstOrDefault().ProductName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetProductDesignPatterns(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DesignPatternCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDesignPatternHelList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductDesignPatterns(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductDesignPattern> prod = from p in db.ProductDesignPattern
                                                         where p.ProductDesignPatternId == temp
                                                         select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductDesignPatternId.ToString(),
                    text = prod.FirstOrDefault().ProductDesignPatternName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleProductDesignPattern(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductDesignPattern> prod = from p in db.ProductDesignPattern
                                                     where p.ProductDesignPatternId == Ids
                                                     select p;

            ProductJson.id = prod.FirstOrDefault().ProductDesignPatternId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductDesignPatternName;

            return Json(ProductJson);
        }

        public ActionResult GetProductDescriptionOfGoods(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DescriptionOfGoodsCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDescriptionOfGoodsHelList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductDescriptionOfGoods(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<DescriptionOfGoods> prod = from p in db.DescriptionOfGoods
                                                       where p.DescriptionOfGoodsId == temp
                                                       select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DescriptionOfGoodsId.ToString(),
                    text = prod.FirstOrDefault().DescriptionOfGoodsName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleProductDescriptionOfGoods(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<DescriptionOfGoods> prod = from p in db.DescriptionOfGoods
                                                   where p.DescriptionOfGoodsId == Ids
                                                   select p;

            ProductJson.id = prod.FirstOrDefault().DescriptionOfGoodsId.ToString();
            ProductJson.text = prod.FirstOrDefault().DescriptionOfGoodsName;

            return Json(ProductJson);
        }

        public ActionResult GetColours(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "ColoursCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetColoursHelList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetColours(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Colour> prod = from p in db.Colour
                                           where p.ColourId == temp
                                           select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ColourId.ToString(),
                    text = prod.FirstOrDefault().ColourName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleColour(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Colour> prod = from p in db.Colour
                                       where p.ColourId == Ids
                                       select p;

            ProductJson.id = prod.FirstOrDefault().ColourId.ToString();
            ProductJson.text = prod.FirstOrDefault().ColourName;

            return Json(ProductJson);
        }

        public ActionResult GetProductContentHeader(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "ContentHeaderCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductContentHeaderHelList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProductContentHeaders(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductContentHeader> prod = from p in db.ProductContentHeader
                                                         where p.ProductContentHeaderId == temp
                                                         select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductContentHeaderId.ToString(),
                    text = prod.FirstOrDefault().ProductContentName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleProductContentHeaders(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductContentHeader> prod = from p in db.ProductContentHeader
                                                     where p.ProductContentHeaderId == Ids
                                                     select p;

            ProductJson.id = prod.FirstOrDefault().ProductContentHeaderId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductContentName;

            return Json(ProductJson);
        }

        public ActionResult GetRawMaterial(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["RawMaterialCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetRawMaterialHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetProductGroupWithTypeFilter(string searchTerm, int pageSize, int pageNum, int filter)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductGroupCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDimension1HelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }





        public ActionResult GetColourWaysForStencil(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "ProductDesignStencilCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetColourWaysForStencilHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDimension1(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "Dimension1CacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDimension1HelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetDimension1(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Dimension1> prod = from p in db.Dimension1
                                               where p.Dimension1Id == temp
                                               select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().Dimension1Id.ToString(),
                    text = prod.FirstOrDefault().Dimension1Name
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleDimension1(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Dimension1> prod = from p in db.Dimension1
                                           where p.Dimension1Id == Ids
                                           select p;

            ProductJson.id = prod.FirstOrDefault().Dimension1Id.ToString();
            ProductJson.text = prod.FirstOrDefault().Dimension1Name;

            return Json(ProductJson);
        }

        public ActionResult GetDimension2(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "Dimension2CacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDimension2HelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetDimension2(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Dimension2> prod = from p in db.Dimension2
                                               where p.Dimension2Id == temp
                                               select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().Dimension2Id.ToString(),
                    text = prod.FirstOrDefault().Dimension2Name
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleDimension2(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Dimension2> prod = from p in db.Dimension2
                                           where p.Dimension2Id == Ids
                                           select p;

            ProductJson.id = prod.FirstOrDefault().Dimension2Id.ToString();
            ProductJson.text = prod.FirstOrDefault().Dimension2Name;

            return Json(ProductJson);
        }

        public ActionResult GetDimension3(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "Dimension3CacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDimension3HelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public JsonResult SetDimension3(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Dimension3> prod = from p in db.Dimension3
                                               where p.Dimension3Id == temp
                                               select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().Dimension3Id.ToString(),
                    text = prod.FirstOrDefault().Dimension3Name
                });
            }
            return Json(ProductJson);
        }
        public JsonResult SetSingleDimension3(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Dimension3> prod = from p in db.Dimension3
                                           where p.Dimension3Id == Ids
                                           select p;

            ProductJson.id = prod.FirstOrDefault().Dimension3Id.ToString();
            ProductJson.text = prod.FirstOrDefault().Dimension3Name;

            return Json(ProductJson);
        }




        public ActionResult GetDimension4(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "Dimension4CacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDimension4HelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public JsonResult SetDimension4(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Dimension4> prod = from p in db.Dimension4
                                               where p.Dimension4Id == temp
                                               select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().Dimension4Id.ToString(),
                    text = prod.FirstOrDefault().Dimension4Name
                });
            }
            return Json(ProductJson);
        }
        public JsonResult SetSingleDimension4(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Dimension4> prod = from p in db.Dimension4
                                           where p.Dimension4Id == Ids
                                           select p;

            ProductJson.id = prod.FirstOrDefault().Dimension4Id.ToString();
            ProductJson.text = prod.FirstOrDefault().Dimension4Name;

            return Json(ProductJson);
        }

        public ActionResult GetFinishedMaterial(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["FinishedMaterialCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetFinishedMaterialHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetFinishedMaterialDivisionWise(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["FinishedMaterialDivisionWiseCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetFinishedMaterialDivisionWiseHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }




        // Sale Help 

        public ActionResult GetSaleOrderDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSaleOrderDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSaleOrderDocumentTypeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSaleOrderDocumentType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<DocumentType> prod = from p in db.DocumentType
                                                 where p.DocumentTypeId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentTypeId.ToString(),
                    text = prod.FirstOrDefault().DocumentTypeName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetSaleOrderPlanDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSaleOrderPlanDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSaleOrderPlanDocumentTypeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSaleOrderPlanDocumentType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<DocumentType> prod = from p in db.DocumentType
                                                 where p.DocumentTypeId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentTypeId.ToString(),
                    text = prod.FirstOrDefault().DocumentTypeName
                });
            }
            return Json(ProductJson);
        }


        public ActionResult GetSaleInvoiceDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSaleInvoiceDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSaleInvoiceDocumentTypeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSaleInvoiceDocumentType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<DocumentType> prod = from p in db.DocumentType
                                                 where p.DocumentTypeId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentTypeId.ToString(),
                    text = prod.FirstOrDefault().DocumentTypeName
                });
            }
            return Json(ProductJson);
        }


        public ActionResult GetSaleOrders(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["SaleOrderCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSaleOrderHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSaleOrderDivisionWise(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["SaleOrderDivisionWiseCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSaleOrderDivisionWistHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleSaleOrder(int Ids)
        {
            ComboBoxResult SaleOrderJson = new ComboBoxResult();

            IEnumerable<SaleOrderHeader> SaleOrderHeader = from H in db.SaleOrderHeader
                                                           where H.SaleOrderHeaderId == Ids
                                                           select H;

            SaleOrderJson.id = SaleOrderHeader.FirstOrDefault().SaleOrderHeaderId.ToString();
            SaleOrderJson.text = SaleOrderHeader.FirstOrDefault().DocNo;

            return Json(SaleOrderJson);
        }

        public JsonResult SetSaleOrders(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<SaleOrderHeader> prod = from p in db.SaleOrderHeader
                                                    where p.SaleOrderHeaderId == temp
                                                    select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().SaleOrderHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetSaleOrderAmendmentDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSaleOrderAmendmentDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSaleOrderAmendmentDocumentTypeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSaleOrderAmendmentDocumentType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<DocumentType> prod = from p in db.DocumentType
                                                 where p.DocumentTypeId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentTypeId.ToString(),
                    text = prod.FirstOrDefault().DocumentTypeName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetSaleOrderCancelDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSaleOrderCancelDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSaleOrderCancelDocumentTypeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSaleOrderCancelDocumentType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<DocumentType> prod = from p in db.DocumentType
                                                 where p.DocumentTypeId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentTypeId.ToString(),
                    text = prod.FirstOrDefault().DocumentTypeName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetSaleOrderAmendments(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["SaleOrderAmendmentCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSaleOrderAmendmentHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleSaleOrderAmendment(int Ids)
        {
            ComboBoxResult SaleOrderJson = new ComboBoxResult();

            IEnumerable<SaleOrderAmendmentHeader> SaleOrderHeaderAmendment = from H in db.SaleOrderAmendmentHeader
                                                                             where H.SaleOrderAmendmentHeaderId == Ids
                                                                             select H;

            SaleOrderJson.id = SaleOrderHeaderAmendment.FirstOrDefault().ToString();
            SaleOrderJson.text = SaleOrderHeaderAmendment.FirstOrDefault().DocNo;

            return Json(SaleOrderJson);
        }

        public JsonResult SetSaleOrderAmendments(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<SaleOrderAmendmentHeader> prod = from p in db.SaleOrderAmendmentHeader
                                                             where p.SaleOrderAmendmentHeaderId == temp
                                                             select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().SaleOrderAmendmentHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetSaleOrderCancels(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["SaleOrderCancelCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSaleOrderCancelHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleSaleOrderCancel(int Ids)
        {
            ComboBoxResult SaleOrderJson = new ComboBoxResult();

            IEnumerable<SaleOrderCancelHeader> SaleOrderHeaderCancel = from H in db.SaleOrderCancelHeader
                                                                       where H.SaleOrderCancelHeaderId == Ids
                                                                       select H;

            SaleOrderJson.id = SaleOrderHeaderCancel.FirstOrDefault().ToString();
            SaleOrderJson.text = SaleOrderHeaderCancel.FirstOrDefault().DocNo;

            return Json(SaleOrderJson);
        }

        public JsonResult SetSaleOrderCancels(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<SaleOrderCancelHeader> prod = from p in db.SaleOrderCancelHeader
                                                          where p.SaleOrderCancelHeaderId == temp
                                                          select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().SaleOrderCancelHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetSaleInvoices(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "SaleInvoiceCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSaleInvoiceHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleSaleInvoice(int Ids)
        {
            ComboBoxResult SaleOrderJson = new ComboBoxResult();

            IEnumerable<SaleInvoiceHeader> SaleOrderInvoice = from H in db.SaleInvoiceHeader
                                                              where H.SaleInvoiceHeaderId == Ids
                                                              select H;

            SaleOrderJson.id = SaleOrderInvoice.FirstOrDefault().SaleInvoiceHeaderId.ToString();
            SaleOrderJson.text = SaleOrderInvoice.FirstOrDefault().DocNo;

            return Json(SaleOrderJson);
        }

        public JsonResult SetSaleInvoices(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<SaleInvoiceHeader> prod = from p in db.SaleInvoiceHeader
                                                      where p.SaleInvoiceHeaderId == temp
                                                      select p;
                ProductJson.Add(new ComboBoxResult()
              {
                  id = prod.FirstOrDefault().SaleInvoiceHeaderId.ToString(),
                  text = prod.FirstOrDefault().DocNo
              });

            }
            return Json(ProductJson);
        }

        public ActionResult GetPackings(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PackingCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPackingHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePacking(int Ids)
        {
            ComboBoxResult PackingJson = new ComboBoxResult();

            IEnumerable<PackingHeader> Packing = from H in db.PackingHeader
                                                 where H.PackingHeaderId == Ids
                                                 select H;

            PackingJson.id = Packing.FirstOrDefault().PackingHeaderId.ToString();
            PackingJson.text = Packing.FirstOrDefault().DocNo;

            return Json(PackingJson);
        }

        public JsonResult SetPackings(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<PackingHeader> prod = from p in db.PackingHeader
                                                  where p.PackingHeaderId == temp
                                                  select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PackingHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });

            }
            return Json(ProductJson);
        }

        public ActionResult GetPackingNo(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["PackingNoCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPackingHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePackiongNo(int Ids)
        {
            ComboBoxResult PackingHeaderJson = new ComboBoxResult();

            IEnumerable<PackingHeader> PackingHeader = from H in db.PackingHeader
                                                       where H.PackingHeaderId == Ids
                                                       select H;

            PackingHeaderJson.id = PackingHeader.FirstOrDefault().PackingHeaderId.ToString();
            PackingHeaderJson.text = PackingHeader.FirstOrDefault().DocNo;

            return Json(PackingHeaderJson);
        }

        public JsonResult SetPackingNo(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<PackingHeader> prod = from p in db.PackingHeader
                                                  where p.PackingHeaderId == temp
                                                  select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PackingHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetColourWays(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheProductDesign";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetColourWaysHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetColourWays(string Ids)
        {
            int rugid = (from p in db.ProductTypes
                         where p.ProductTypeName == ProductTypeConstants.Rug
                         select p.ProductTypeId
                          ).FirstOrDefault();

            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductDesign> prod = from p in db.ProductDesigns
                                                  where p.ProductDesignId == temp && p.ProductTypeId == rugid
                                                  select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductDesignId.ToString(),
                    text = prod.FirstOrDefault().ProductDesignName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleColourWays(int Ids)
        {
            int rugid = (from p in db.ProductTypes
                         where p.ProductTypeName == ProductTypeConstants.Rug
                         select p.ProductTypeId
                         ).FirstOrDefault();

            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductDesign> prod = from p in db.ProductDesigns
                                              where p.ProductDesignId == Ids && p.ProductTypeId == rugid
                                              select p;

            ProductJson.id = prod.FirstOrDefault().ProductDesignId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductDesignName;

            return Json(ProductJson);
        }



        // Purchase Help 

        public JsonResult SetDocumentTypes(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<DocumentType> prod = from p in db.DocumentType
                                                 where p.DocumentTypeId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentTypeId.ToString(),
                    text = prod.FirstOrDefault().DocumentTypeName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetPurchaseIndentDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CachePurchaseIndentDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPurchaseIndentDocumentTypeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetPurchaseIndentDocumentType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<DocumentType> prod = from p in db.DocumentType
                                                 where p.DocumentTypeId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentTypeId.ToString(),
                    text = prod.FirstOrDefault().DocumentTypeName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetPurchaseIndentCancelDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CachePurchaseIndentCancelDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.PurchaseIndentCancel), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public ActionResult GetProdOrderDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CachePurchaseOrderDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.ProdOrder), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetProdOrderDocumentType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<DocumentType> prod = from p in db.DocumentType
                                                 where p.DocumentTypeId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentTypeId.ToString(),
                    text = prod.FirstOrDefault().DocumentTypeName
                });
            }
            return Json(ProductJson);
        }





        public ActionResult GetPurchaseOrderDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CachePurchaseOrderDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPurchaseOrderDocumentTypeHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetPurchaseOrderDocumentType(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<DocumentType> prod = from p in db.DocumentType
                                                 where p.DocumentTypeId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentTypeId.ToString(),
                    text = prod.FirstOrDefault().DocumentTypeName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetPurchaseOrderCancelDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CachePurchaseIndentCancelDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.PurchaseOrderCancel), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public ActionResult GetPurchaseGoodsReceiptDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CachePurchaseIndentCancelDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.PurchaseGoodsReceipt), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult GetPurchaseGoodsReturnDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CachePurchaseIndentCancelDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.PurchaseGoodsReturn), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult GetPurchaseInvoiceDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CachePurchaseIndentCancelDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.PurchaseInvoice), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult GetPurchaseInvoiceReturnDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CachePurchaseIndentCancelDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.PurchaseInvoiceReturn), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }



        public ActionResult GetReasons(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "ReasonCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetReasonHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public JsonResult SetReason(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Reason> prod = from H in db.Reason
                                           where H.ReasonId == temp
                                           select H;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ReasonId.ToString(),
                    text = prod.FirstOrDefault().ReasonName
                });
            }
            return Json(ProductJson);
        }




        public ActionResult GetPurchaseIndents(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PurchaseIndentCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPurchaseIndentHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePurchaseIndent(int Ids)
        {
            ComboBoxResult PurchaseIndentJson = new ComboBoxResult();

            IEnumerable<PurchaseIndentHeader> PurchaseIndentHeader = from H in db.PurchaseIndentHeader
                                                                     where H.PurchaseIndentHeaderId == Ids
                                                                     select H;

            PurchaseIndentJson.id = PurchaseIndentHeader.FirstOrDefault().PurchaseIndentHeaderId.ToString();
            PurchaseIndentJson.text = PurchaseIndentHeader.FirstOrDefault().DocNo;

            return Json(PurchaseIndentJson);
        }

        public JsonResult SetPurchaseIndents(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<PurchaseIndentHeader> prod = from H in db.PurchaseIndentHeader
                                                         join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                         from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                         where H.PurchaseIndentHeaderId == temp
                                                         select H;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PurchaseIndentHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo + "-" + prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }



        public ActionResult GetPurchaseIndentCancels(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PurchaseIndentCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPurchaseIndentCancelHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePurchaseIndentCancel(int Ids)
        {
            ComboBoxResult PurchaseIndentJson = new ComboBoxResult();

            IEnumerable<PurchaseIndentCancelHeader> PurchaseIndentHeader = from H in db.PurchaseIndentCancelHeader
                                                                           where H.PurchaseIndentCancelHeaderId == Ids
                                                                           select H;

            PurchaseIndentJson.id = PurchaseIndentHeader.FirstOrDefault().PurchaseIndentCancelHeaderId.ToString();
            PurchaseIndentJson.text = PurchaseIndentHeader.FirstOrDefault().DocNo;

            return Json(PurchaseIndentJson);
        }

        public JsonResult SetPurchaseIndentCancels(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<PurchaseIndentCancelHeader> prod = from H in db.PurchaseIndentCancelHeader
                                                               join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                               from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                               where H.PurchaseIndentCancelHeaderId == temp
                                                               select H;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PurchaseIndentCancelHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo + "-" + prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }



        public ActionResult GetPurchaseOrders(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PurchaseOrderCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPurchaseOrderHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePurchaseOrder(int Ids)
        {
            ComboBoxResult PurchaseOrderJson = new ComboBoxResult();

            IEnumerable<PurchaseOrderHeader> PurchaseOrderHeader = from H in db.PurchaseOrderHeader
                                                                   where H.PurchaseOrderHeaderId == Ids
                                                                   select H;

            PurchaseOrderJson.id = PurchaseOrderHeader.FirstOrDefault().PurchaseOrderHeaderId.ToString();
            PurchaseOrderJson.text = PurchaseOrderHeader.FirstOrDefault().DocNo;

            return Json(PurchaseOrderJson);
        }

        public JsonResult SetPurchaseOrders(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<PurchaseOrderHeader> prod = from p in db.PurchaseOrderHeader
                                                        where p.PurchaseOrderHeaderId == temp
                                                        select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PurchaseOrderHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }



        public ActionResult GetPurchaseOrderCancels(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PurchaseOrderCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPurchaseOrderCancelHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePurchaseOrderCancel(int Ids)
        {
            ComboBoxResult PurchaseOrderJson = new ComboBoxResult();

            IEnumerable<PurchaseOrderCancelHeader> PurchaseOrderHeader = from H in db.PurchaseOrderCancelHeader
                                                                         where H.PurchaseOrderCancelHeaderId == Ids
                                                                         select H;

            PurchaseOrderJson.id = PurchaseOrderHeader.FirstOrDefault().PurchaseOrderCancelHeaderId.ToString();
            PurchaseOrderJson.text = PurchaseOrderHeader.FirstOrDefault().DocNo;

            return Json(PurchaseOrderJson);
        }

        public JsonResult SetPurchaseOrderCancels(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<PurchaseOrderCancelHeader> prod = from H in db.PurchaseOrderCancelHeader
                                                              join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                              from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                              where H.PurchaseOrderCancelHeaderId == temp
                                                              select H;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PurchaseOrderCancelHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo + "-" + prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }


        public ActionResult GetPurchaseGoodsReceipt(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PurchaseGoodsReceiptCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPurchaseGoodsReceiptHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePurchaseGoodsReceipt(int Ids)
        {
            ComboBoxResult PurchaseIndentJson = new ComboBoxResult();

            IEnumerable<PurchaseGoodsReceiptHeader> PurchaseIndentHeader = from H in db.PurchaseGoodsReceiptHeader
                                                                           where H.PurchaseGoodsReceiptHeaderId == Ids
                                                                           select H;

            PurchaseIndentJson.id = PurchaseIndentHeader.FirstOrDefault().PurchaseGoodsReceiptHeaderId.ToString();
            PurchaseIndentJson.text = PurchaseIndentHeader.FirstOrDefault().DocNo;

            return Json(PurchaseIndentJson);
        }

        public JsonResult SetPurchaseGoodsReceipts(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<PurchaseGoodsReceiptHeader> prod = from p in db.PurchaseGoodsReceiptHeader
                                                               where p.PurchaseGoodsReceiptHeaderId == temp
                                                               select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PurchaseGoodsReceiptHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }



        public ActionResult GetDebitNoteDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDebitNoteDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.DebitNote), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetCreditNoteDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheCreditNoteDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.CreditNote), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }







        // **************************   Help of Planning        ************************************************

        public ActionResult GetWeavingPlanningDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheWeavingPlanningDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.WeavingPlan), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingPlanningDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDyeingPlanningDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.DyeingPlanning), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingPlanningCancelDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDyeingPlanningCancelDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.DyeingPlanningCancel), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSpinningPlanningDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSpinningPlanningDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.SpinningPlanning), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSpinningPlanningCancelDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSpinningPlanningCancelDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.SpinningPlanningCancel), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        //  MaterialPlanHeader

        public ActionResult GetSalesOrderMaterialPlannings(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "SalesOrderMaterialPlanningCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetMaterialPlanHelpList(TransactionDoctypeConstants.SaleOrderPlan, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingMaterialPlannings(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DyeingPlanningCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetMaterialPlanHelpList(TransactionDoctypeConstants.DyedMaterialPlanForWeaving, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetYarnPlanForWeavings(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "WeavingPlanningCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetMaterialPlanHelpList(TransactionDoctypeConstants.YarnPlanForWeaving, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetMaterialPlanForWeavings(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "WeavingPlanningCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetMaterialPlanHelpList(TransactionDoctypeConstants.MaterialPlanForWeaving, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetFinishedMaterialPlannings(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DyeingPlanningCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetMaterialPlanHelpList(TransactionDoctypeConstants.FinishinedItemProductionPLan, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSaleOrderPlannings(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DyeingPlanningCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProdOrderHelpList(TransactionDoctypeConstants.SaleOrderPlan, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleMaterialPlanning(int Ids)
        {
            ComboBoxResult ProdOrderJson = new ComboBoxResult();

            IEnumerable<MaterialPlanHeader> ProdOrderHeader = from H in db.MaterialPlanHeader
                                                              where H.MaterialPlanHeaderId == Ids
                                                              select H;

            ProdOrderJson.id = ProdOrderHeader.FirstOrDefault().MaterialPlanHeaderId.ToString();
            ProdOrderJson.text = ProdOrderHeader.FirstOrDefault().DocNo;

            return Json(ProdOrderJson);
        }

        public JsonResult SetMaterialPlannings(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<MaterialPlanHeader> prod = from p in db.MaterialPlanHeader
                                                       where p.MaterialPlanHeaderId == temp
                                                       select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().MaterialPlanHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }



        //  ProdOrderHeader        

        public ActionResult GetProdOrders(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "ProdOrdersCache";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProdOrderHelpList(LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProdOrders(int Ids)
        {
            ComboBoxResult ProdOrderJson = new ComboBoxResult();

            IEnumerable<ProdOrderHeader> ProdOrderHeader = from H in db.ProdOrderHeader
                                                           where H.ProdOrderHeaderId == Ids
                                                           select H;

            ProdOrderJson.id = ProdOrderHeader.FirstOrDefault().ProdOrderHeaderId.ToString();
            ProdOrderJson.text = ProdOrderHeader.FirstOrDefault().DocNo;

            return Json(ProdOrderJson);
        }

        public JsonResult SetProdOrders(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProdOrderHeader> prod = from p in db.ProdOrderHeader
                                                    where p.ProdOrderHeaderId == temp
                                                    select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProdOrderHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetDyeingPlannings(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DyeingPlanningCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProdOrderHelpList(TransactionDoctypeConstants.DyeingPlanning, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetWeavingPlannings(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "WeavingPlanningCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProdOrderHelpList(TransactionDoctypeConstants.WeavingPlan, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSpinningPlannings(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "SpinningPlanningCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProdOrderHelpList(TransactionDoctypeConstants.SpinningPlanning, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }






        //  ProdOrderCancelHeader   

        public ActionResult GetDyingPlanningCancels(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DyingPlanningCancelCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProdOrderCancelHelpList(TransactionDoctypeConstants.DyeingPlanningCancel, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSpinningPlanningCancels(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "SpinningPlanningCancelCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProdOrderCancelHelpList(TransactionDoctypeConstants.SpinningPlanningCancel, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePlanningCancel(int Ids)
        {
            ComboBoxResult ProdOrderJson = new ComboBoxResult();

            IEnumerable<ProdOrderCancelHeader> ProdOrderCancelHeader = from H in db.ProdOrderCancelHeader
                                                                       where H.ProdOrderCancelHeaderId == Ids
                                                                       select H;

            ProdOrderJson.id = ProdOrderCancelHeader.FirstOrDefault().ProdOrderCancelHeaderId.ToString();
            ProdOrderJson.text = ProdOrderCancelHeader.FirstOrDefault().DocNo;

            return Json(ProdOrderJson);
        }

        public JsonResult SetPlanningCancels(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProdOrderCancelHeader> prod = from p in db.ProdOrderCancelHeader
                                                          where p.ProdOrderCancelHeaderId == temp
                                                          select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProdOrderCancelHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }















        public ActionResult GetLedgerHeaders(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "LedgerHeaderCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetLedgerHeaderHelpList("", LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public JsonResult SetLedgerHeaders(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<LedgerHeader> prod = from H in db.LedgerHeader
                                                 join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                 from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                 where H.LedgerHeaderId == temp
                                                 select H;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().LedgerHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo + "-" + prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }






        // **************************    Help of Job        ************************************************

        public ActionResult GetDyeingOrderDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDyeingOrderDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.DyeingOrder), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingOrderCancelDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDyeingOrderCancelDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.DyeingOrderCancel), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingReceiveDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDyeingReceiveDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.DyeingReceive), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingInvoiceDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDyeingInvoiceDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.DyeingInvoice), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingReturnDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDyeingReturnDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.DyedGoodsReturn), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public ActionResult GetSpinningOrderDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSpinningOrderDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.SpinningOrder), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSpinningOrderCancelDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSpinningOrderCancelDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.SpinningOrderCancel), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSpinningReceiveDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSpinningReceiveDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.SpinningReceive), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSpinningInvoiceDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSpinningInvoiceDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.SpinningInvoice), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        //CostCenter

        public ActionResult GetWeavingCostcentrs(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "WeavingCostCenterCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetCostCenterHelpList(TransactionDoctypeConstants.WeavingOrder, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetCostcentrs(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<CostCenter> prod = from p in db.CostCenter
                                               where p.CostCenterId == temp
                                               select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().CostCenterId.ToString(),
                    text = prod.FirstOrDefault().CostCenterName
                });
            }
            return Json(ProductJson);
        }


        //JobOrderHeader
        public JsonResult SetSingleJobOrder(int Ids)
        {
            ComboBoxResult ProdOrderJson = new ComboBoxResult();

            IEnumerable<JobOrderHeader> JobOrderHeader = from H in db.JobOrderHeader
                                                         where H.JobOrderHeaderId == Ids
                                                         select H;

            ProdOrderJson.id = JobOrderHeader.FirstOrDefault().JobOrderHeaderId.ToString();
            ProdOrderJson.text = JobOrderHeader.FirstOrDefault().DocNo;

            return Json(ProdOrderJson);
        }

        public JsonResult SetJobOrders(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<JobOrderHeader> prod = from p in db.JobOrderHeader
                                                   where p.JobOrderHeaderId == temp
                                                   select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().JobOrderHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetJobOrders(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PurchaseOrderCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobOrderHelpList(LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSpinningOrders(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "SpinningOrderCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobOrderHelpList(TransactionDoctypeConstants.SpinningOrder, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingOrders(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DyeingOrderCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobOrderHelpList(TransactionDoctypeConstants.DyeingOrder, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetWeavingOrders(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "WeavingOrderCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobOrderHelpList(TransactionDoctypeConstants.WeavingOrder, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }



        /// JobReceiveHeader

        public JsonResult SetSingleJobReceive(int Ids)
        {
            ComboBoxResult ProdOrderJson = new ComboBoxResult();

            IEnumerable<JobReceiveHeader> JobReceiveHeader = from H in db.JobReceiveHeader
                                                             where H.JobReceiveHeaderId == Ids
                                                             select H;

            ProdOrderJson.id = JobReceiveHeader.FirstOrDefault().JobReceiveHeaderId.ToString();
            ProdOrderJson.text = JobReceiveHeader.FirstOrDefault().DocNo;

            return Json(ProdOrderJson);
        }

        public JsonResult SetJobReceives(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<JobReceiveHeader> prod = from p in db.JobReceiveHeader
                                                     where p.JobReceiveHeaderId == temp
                                                     select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().JobReceiveHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo,
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetJobReceives(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "JobReceiveCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobReceiveHelpList(LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingReceives(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DyeingReceiveCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobReceiveHelpList(TransactionDoctypeConstants.DyeingReceive, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetWeavingReceives(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "WeavingReceiveCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobReceiveHelpList(TransactionDoctypeConstants.WeavingReceive, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }




        /// JobInvoiceHeader
        public JsonResult SetSingleJobInvoice(int Ids)
        {
            ComboBoxResult ProdOrderJson = new ComboBoxResult();

            IEnumerable<JobInvoiceHeader> JobInvoiceHeader = from H in db.JobInvoiceHeader
                                                             where H.JobInvoiceHeaderId == Ids
                                                             select H;

            ProdOrderJson.id = JobInvoiceHeader.FirstOrDefault().JobInvoiceHeaderId.ToString();
            ProdOrderJson.text = JobInvoiceHeader.FirstOrDefault().DocNo;

            return Json(ProdOrderJson);
        }

        public JsonResult SetJobInvoices(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<JobInvoiceHeader> prod = from p in db.JobInvoiceHeader
                                                     where p.JobInvoiceHeaderId == temp
                                                     select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().JobInvoiceHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo,
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetJobInvoices(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "JobInvoiceCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobInvoiceHelpList(LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingInvoices(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DyeingInvoiceCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobInvoiceHelpList(TransactionDoctypeConstants.DyeingInvoice, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }








        public ActionResult GetDyeingReturns(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DyeingReturnCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobReceiveHelpList(TransactionDoctypeConstants.DyeingReturn, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }









        // JobOrderAmendmentHeader

        public ActionResult GetJobOrderAmendments(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PurchaseOrderAmendmentCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobOrderAmendmentHelpList(TransactionDocCategoryConstants.JobOrderRateAmendment, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetJobOrderAmendments(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<JobOrderAmendmentHeader> prod = from p in db.JobOrderAmendmentHeader
                                                            where p.JobOrderAmendmentHeaderId == temp
                                                            select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().JobOrderAmendmentHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo,
                });
            }
            return Json(ProductJson);
        }


        public ActionResult GetProductInvoiceGroups(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductInvoiceGroupCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductInvoiceGroupHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public ActionResult GetProductInvoiceGroupsDivisionWise(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductInvoiceGroupDivisionWiseCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductInvoiceGroupDivisionWiseHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public ActionResult GetProductInvoiceGroupsDivisionWiseExcludeSample(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["ProductInvoiceGroupDivisionWiseExcludeSampleCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductInvoiceGroupDivisionWiseExcludeSampleHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProductInvoiceGroup(int Ids)
        {
            ComboBoxResult ProductInvoiceGroupJson = new ComboBoxResult();

            IEnumerable<ProductInvoiceGroup> ProductInvoiceGroup = from H in db.ProductInvoiceGroup
                                                                   where H.ProductInvoiceGroupId == Ids
                                                                   select H;

            ProductInvoiceGroupJson.id = ProductInvoiceGroup.FirstOrDefault().ProductInvoiceGroupId.ToString();
            ProductInvoiceGroupJson.text = ProductInvoiceGroup.FirstOrDefault().ProductInvoiceGroupName;

            return Json(ProductInvoiceGroupJson);
        }

        public JsonResult SetProductInvoiceGroups(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<ProductInvoiceGroup> prod = from p in db.ProductInvoiceGroup
                                                        where p.ProductInvoiceGroupId == temp
                                                        select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductInvoiceGroupId.ToString(),
                    text = prod.FirstOrDefault().ProductInvoiceGroupName
                });
            }
            return Json(ProductJson);
        }


        public ActionResult GetPurchaseInvoices(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PurchaseInvoiceCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPurchaseInvoiceHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePurchaseInvoice(int Ids)
        {
            ComboBoxResult PurchaseInvoiceJson = new ComboBoxResult();

            IEnumerable<PurchaseInvoiceHeader> PurchaseInvoiceHeader = from H in db.PurchaseInvoiceHeader
                                                                       where H.PurchaseInvoiceHeaderId == Ids
                                                                       select H;

            PurchaseInvoiceJson.id = PurchaseInvoiceHeader.FirstOrDefault().PurchaseInvoiceHeaderId.ToString();
            PurchaseInvoiceJson.text = PurchaseInvoiceHeader.FirstOrDefault().DocNo;

            return Json(PurchaseInvoiceJson);
        }

        public JsonResult SetPurchaseInvoices(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<PurchaseInvoiceHeader> prod = from p in db.PurchaseInvoiceHeader
                                                          where p.PurchaseInvoiceHeaderId == temp
                                                          select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PurchaseInvoiceHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }



        public ActionResult GetPurchaseInvoiceReturns(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PurchaseInvoiceCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPurchaseInvoiceReturnHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePurchaseInvoiceReturn(int Ids)
        {
            ComboBoxResult PurchaseInvoiceJson = new ComboBoxResult();

            IEnumerable<PurchaseInvoiceReturnHeader> PurchaseInvoiceReturnHeader = from H in db.PurchaseInvoiceReturnHeader
                                                                                   where H.PurchaseInvoiceReturnHeaderId == Ids
                                                                                   select H;

            PurchaseInvoiceJson.id = PurchaseInvoiceReturnHeader.FirstOrDefault().PurchaseInvoiceReturnHeaderId.ToString();
            PurchaseInvoiceJson.text = PurchaseInvoiceReturnHeader.FirstOrDefault().DocNo;

            return Json(PurchaseInvoiceJson);
        }

        public JsonResult SetPurchaseInvoiceReturns(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<PurchaseInvoiceReturnHeader> prod = from p in db.PurchaseInvoiceReturnHeader
                                                                where p.PurchaseInvoiceReturnHeaderId == temp
                                                                select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PurchaseInvoiceReturnHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }



        public ActionResult GetPurchaseGoodsReturns(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "PurchaseGoodsCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPurchaseGoodsReturnHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSinglePurchaseGoodsReturn(int Ids)
        {
            ComboBoxResult PurchaseGoodsJson = new ComboBoxResult();

            IEnumerable<PurchaseGoodsReturnHeader> PurchaseGoodsReturnHeader = from H in db.PurchaseGoodsReturnHeader
                                                                               where H.PurchaseGoodsReturnHeaderId == Ids
                                                                               select H;

            PurchaseGoodsJson.id = PurchaseGoodsReturnHeader.FirstOrDefault().PurchaseGoodsReturnHeaderId.ToString();
            PurchaseGoodsJson.text = PurchaseGoodsReturnHeader.FirstOrDefault().DocNo;

            return Json(PurchaseGoodsJson);
        }

        public JsonResult SetPurchaseGoodsReturns(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<PurchaseGoodsReturnHeader> prod = from p in db.PurchaseGoodsReturnHeader
                                                              where p.PurchaseGoodsReturnHeaderId == temp
                                                              select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PurchaseGoodsReturnHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }


        public ActionResult GetStoreIssues(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "StoreIssueCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetStoreIssueReceiveHelpList(TransactionDocCategoryConstants.StoreIssue, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetStoreReceives(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "StoreReceiveCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetStoreIssueReceiveHelpList(TransactionDoctypeConstants.StoreReceive, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleStoreIssuesReceive(int Ids)
        {
            ComboBoxResult PurchaseInvoiceJson = new ComboBoxResult();

            IEnumerable<StockHeader> StockHeader = from H in db.StockHeader
                                                   where H.StockHeaderId == Ids
                                                   select H;

            PurchaseInvoiceJson.id = StockHeader.FirstOrDefault().StockHeaderId.ToString();
            PurchaseInvoiceJson.text = StockHeader.FirstOrDefault().DocNo;

            return Json(PurchaseInvoiceJson);
        }

        public JsonResult SetStoreIssuesReceives(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<StockHeader> prod = from p in db.StockHeader
                                                where p.StockHeaderId == temp
                                                select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().StockHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
                });
            }
            return Json(ProductJson);
        }




        public ActionResult GetDyeingMaterialIssues(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DyeingMaterialIssueCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetStoreIssueReceiveHelpList(TransactionDocCategoryConstants.DyeingMaterialIssue, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingMaterialReceives(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "DyeingMaterialReceiveCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetStoreIssueReceiveHelpList(TransactionDoctypeConstants.DyeingMaterialReceive, LoginSiteId, LoginDivisionId), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }





        public ActionResult GetStoreIssueDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheStoreIssueDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.StoreIssue), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetStoreReceiveDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheStoreReceiveDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.StoreReceive), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }



        public ActionResult GetDyeingMaterialIssueDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDyeingMaterialIssueDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.DyeingMaterialIssue), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetDyeingMaterialReceiveDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheDyeingMaterialReceiveDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.DyeingMaterialReceive), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }



        public ActionResult GetJobOrderDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheJobOrderDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.JobOrder), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public ActionResult GetWeavingOrderDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheWeavingOrderDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.WeavingOrder), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetJobOrderAmendmentDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheJobOrderAmendmentDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.JobOrderRateAmendment), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetJobReceiveDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheJobReceiveDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.JobReceive), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetJobInvoiceDocumentType(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheJobInvoiceDocumentType";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDocumentTypeHelpList(TransactionDocCategoryConstants.JobInvoice), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }























        public ActionResult GetBomMaterial(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["BomMaterialCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetBomMaterialHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSample(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "CacheSample";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSampleHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSample(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Product> prod = from p in db.Product
                                            where p.ProductId == temp
                                            select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductId.ToString(),
                    text = prod.FirstOrDefault().ProductName
                });
            }
            return Json(ProductJson);
        }

        public ActionResult GetPersonCustomGroup(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. PersonCacheKeyHint
            var PersonCacheKeyHint = "CachePersonCustomGroup";

            //THis statement has been changed because GetPersonHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPersonCustomGroupHelpList(), PersonCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, PersonCacheKeyHint);



            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetPersonCustomGroup(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> PersonJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                //IEnumerable<Person> Persons = db.Persons.Take(3);
                IEnumerable<PersonCustomGroupHeader> prod = from p in db.PersonCustomGroupHeader
                                                            where p.PersonCustomGroupId == temp
                                                            select p;
                PersonJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PersonCustomGroupId.ToString(),
                    text = prod.FirstOrDefault().PersonCustomGroupName
                });
            }
            return Json(PersonJson);
        }


        public JsonResult GetCustomProducts(string term)//SupplierID
        {
            return Json(cbl.GetProductsHelpList(term), JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetMenus(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = ConfigurationManager.AppSettings["MenuCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetMenuHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
            //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

            if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetMenus(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Menu> prod = (from b in db.Menu
                                          where b.MenuId == temp
                                          select new Menu
                                          {
                                              MenuId = b.MenuId,
                                              MenuName = b.MenuName
                                          });
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().MenuId.ToString(),
                    text = prod.FirstOrDefault().MenuName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleMenu(int Ids)
        {
            ComboBoxResult MenuJson = new ComboBoxResult();

            Menu person = (from b in db.Menu
                           where b.MenuId == Ids
                           select new Menu
                           {
                               MenuId = b.MenuId,
                               MenuName = b.MenuName
                           }).FirstOrDefault();

            MenuJson.id = person.MenuId.ToString();
            MenuJson.text = person.MenuName;

            return Json(MenuJson);
        }


        //public ActionResult GetSelect2Data(string searchTerm, int pageSize, int pageNum, string SqlProcGet)
        //{

        //    IEnumerable<CustomComboBoxResult> Select2List = cbl.GetSelect2HelpList(SqlProcGet, searchTerm, pageSize, pageNum);

        //    CustomComboBoxPagedResult pagedAttendees = new CustomComboBoxPagedResult();
        //    pagedAttendees.Results = Select2List.ToList();
        //    pagedAttendees.Total = Select2List.Count() > 0 ? Select2List.FirstOrDefault().RecCount : 0;
        //    //Return the data as a jsonp result
        //    return new JsonpResult
        //    {
        //        Data = pagedAttendees,
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };
        //}

        public ActionResult GetSelect2Data(string searchTerm, int pageSize, int pageNum, string SqlProcGet)
        {
            return new JsonpResult
            {
                Data = cbl.GetSelect2HelpList(SqlProcGet, searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSelct2Data(string Ids, string SqlProcSet)
        {

            //SqlParameter SqlParameterDocId = new SqlParameter("@Ids", Ids);
            if (SqlProcSet.Contains(" ") == true)
                SqlProcSet = SqlProcSet + " ,";

            List<ComboBoxResult> ProductJson = db.Database.SqlQuery<ComboBoxResult>(" " + SqlProcSet + " @Ids = \'" + Ids + "\'").ToList();

            return Json(ProductJson);
        }

        public JsonResult SetSingleSelect2Data(int Ids, string SqlProcSet)
        {

            SqlParameter SqlParameterDocId = new SqlParameter("@Ids", Ids);
            ComboBoxResult ProductJson = db.Database.SqlQuery<ComboBoxResult>(" " + SqlProcSet + " @Ids ", SqlParameterDocId).FirstOrDefault();

            return Json(ProductJson);
        }

        public JsonResult GetJobWorkersWithProcess(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = cbl.GetJobWorkerHelpListWithProcessFilter(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = cbl.GetJobWorkerHelpListWithProcessFilter(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult GetEmployeeWithProcess(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = cbl.GetEmployeeHelpListWithProcessFilter(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = cbl.GetEmployeeHelpListWithProcessFilter(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult SetDate(string Proc)
        {
            string Select2List = db.Database.SqlQuery<string>(Proc).FirstOrDefault();
            return Json(Select2List, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetProductRateGroup(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = cbl.GetProductRateGroupHelpList(searchTerm, filter).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = cbl.GetProductRateGroupHelpList(searchTerm, filter).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public JsonResult SetProductRateGroup(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<ComboBoxResult> prod = (from b in db.ProductRateGroup
                                                    where b.ProductRateGroupId == temp
                                                    select new ComboBoxResult
                                                    {
                                                        id = b.ProductRateGroupId.ToString(),
                                                        text = b.ProductRateGroupName
                                                    });
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().id,
                    text = prod.FirstOrDefault().text
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleProductRateGroup(int Ids)
        {
            ComboBoxResult MenuJson = new ComboBoxResult();

            ComboBoxResult person = (from b in db.ProductRateGroup
                                     where b.ProductRateGroupId == Ids
                                     select new ComboBoxResult
                                     {
                                         id = b.ProductRateGroupId.ToString(),
                                         text = b.ProductRateGroupName
                                     }).FirstOrDefault();

            MenuJson.id = person.id;
            MenuJson.text = person.text;

            return Json(MenuJson);
        }

        public ActionResult GetUsers(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetUsers(searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleUsers(string Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            string prod = (from p in db.Users
                           where p.UserName == Ids
                           select p.UserName).FirstOrDefault();

            ProductJson.id = prod;
            ProductJson.text = prod;

            return Json(ProductJson);
        }

        public JsonResult GetPersonWithProcess(string searchTerm, int pageSize, int pageNum, int? filter)//filter:PersonId
        {
            var temp = cbl.GetPersonHelpListWithProcessFilter(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = cbl.GetPersonHelpListWithProcessFilter(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetUnits(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetUnits(searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetUnits(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                string temp = subStr[i];
                IEnumerable<ComboBoxResult> prod = (from b in db.Units
                                                    where b.UnitId == temp
                                                    select new ComboBoxResult
                                                    {
                                                        id = b.UnitId.ToString(),
                                                        text = b.UnitName
                                                    });
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().id,
                    text = prod.FirstOrDefault().text
                });
            }
            return Json(ProductJson);
        }


        public ActionResult GetQAGroups(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetQAGroups(searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetQAGroups(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<ComboBoxResult> prod = (from b in db.QAGroup
                                                    where b.QAGroupId == temp
                                                    select new ComboBoxResult
                                                    {
                                                        id = b.QAGroupId.ToString(),
                                                        text = b.QaGroupName
                                                    });
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().id,
                    text = prod.FirstOrDefault().text
                });
            }
            return Json(ProductJson);
        }

        public JsonResult SetSingleQAGroup(int Ids)
        {
            ComboBoxResult MenuJson = new ComboBoxResult();

            ComboBoxResult person = (from b in db.QAGroup
                                     where b.QAGroupId == Ids
                                     select new ComboBoxResult
                                     {
                                         id = b.QAGroupId.ToString(),
                                         text = b.QaGroupName
                                     }).FirstOrDefault();

            MenuJson.id = person.id;
            MenuJson.text = person.text;

            return Json(MenuJson);
        }

        public JsonResult GetReason(string searchTerm, int pageSize, int pageNum, int filter)//filter:DocTypeId
        {
            var Query = cbl.GetReasonHelpListWithDocTypeFilter(filter, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleReason(int Ids)
        {
            ComboBoxResult ReasonJson = new ComboBoxResult();

            Reason reason = (from b in db.Reason
                             where b.ReasonId == Ids
                             select b).FirstOrDefault();

            ReasonJson.id = reason.ReasonId.ToString();
            ReasonJson.text = reason.ReasonName;

            return Json(ReasonJson);
        }



        public JsonResult GetSalesTaxProductCodes(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetSalesTaxProductCodes(searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public JsonResult SetSingleSalesTaxProductCode(int Ids)
        {
            ComboBoxResult SalesTaxProductCodeJson = new ComboBoxResult();

            SalesTaxProductCode SalesTaxProductCode = (from b in db.SalesTaxProductCode
                                                       where b.SalesTaxProductCodeId == Ids
                                                       select b).FirstOrDefault();

            SalesTaxProductCodeJson.id = SalesTaxProductCode.SalesTaxProductCodeId.ToString();
            SalesTaxProductCodeJson.text = SalesTaxProductCode.Code;

            return Json(SalesTaxProductCodeJson);
        }
        public JsonResult SetSalesTaxProductCode(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<SalesTaxProductCode> prod = from p in db.SalesTaxProductCode
                                                        where p.SalesTaxProductCodeId == temp
                                                        select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().SalesTaxProductCodeId.ToString(),
                    text = prod.FirstOrDefault().Code
                });
            }
            return Json(ProductJson);
        }
    }
}
