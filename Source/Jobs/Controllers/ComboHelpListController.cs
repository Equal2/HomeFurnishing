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
using Jobs.Helpers;
using Model.ViewModels;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using Core.Common;
using Jobs;


namespace Jobs.Controllers
{
    [Authorize]
    public class ComboHelpListController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private IComboHelpListService cbl = new ComboHelpListService();

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
              prodLst.Add(new ComboBoxList() { Id = (int) StatusConstants.Drafted , PropFirst = "Drafted" });
              prodLst.Add(new ComboBoxList() { Id = (int)StatusConstants.Submitted  , PropFirst = "Submitted" });
              prodLst.Add(new ComboBoxList() { Id = (int)StatusConstants.Approved , PropFirst = "Approved" });

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

              if (Ids == (int)StatusConstants.Drafted )
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
              //var productCacheKeyHint = "CacheSite";

              //THis statement has been changed because GetProductHelpList was calling again and again. 

              AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSiteHelpList());
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
                                                  where p.SiteId  == temp
                                                  select p;
                  ProductJson.Add(new ComboBoxResult()
                  {
                      id = prod.FirstOrDefault().SiteId.ToString(),
                      text = prod.FirstOrDefault().SiteCode
                  });
              }
              return Json(ProductJson);
          }

          public ActionResult GetDivision(string searchTerm, int pageSize, int pageNum)
          {
              //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
              //var productCacheKeyHint = "CacheDivision";

              //THis statement has been changed because GetProductHelpList was calling again and again. 

              AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetDivisionHelpList());
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
                                           where p.DivisionId  == temp
                                           select p;
                  ProductJson.Add(new ComboBoxResult()
                  {
                      id = prod.FirstOrDefault().DivisionId.ToString(),
                      text = prod.FirstOrDefault().DivisionName
                  });
              }
              return Json(ProductJson);
          }

        public ActionResult GetBuyers(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            //var productCacheKeyHint = ConfigurationManager.AppSettings["SupplierCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetBuyerHelpList());
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
            //var productCacheKeyHint = ConfigurationManager.AppSettings["SupplierCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSupplierHelpList());
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

        public ActionResult GetMenus(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            //var productCacheKeyHint = ConfigurationManager.AppSettings["MenuCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetMenuHelpList());
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
                                                     where b.MenuId== temp
                                                     select new Menu
                                                     {
                                                         MenuId = b.MenuId,
                                                         MenuName= b.MenuName
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

            var person = (from b in db.Menu                                      
                                      where b.MenuId == Ids
                                      select new
                                      {
                                          MenuId = b.MenuId,
                                          MenuName = b.MenuName
                                      }).FirstOrDefault();

            MenuJson.id = person.MenuId.ToString();
            MenuJson.text = person.MenuName;

            return Json(MenuJson);
        }


        public ActionResult GetJobWorker_Packing(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            //var productCacheKeyHint = ConfigurationManager.AppSettings["JobWorkerCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobWorkerHelpList_WithProcess(20));
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
            //var productCacheKeyHint = ConfigurationManager.AppSettings["JobWorkerCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetJobWorkerHelpList());
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

            PersonViewModel person = (from p in db.Persons 
                                      where p.PersonID == Ids
                                      select new PersonViewModel
                                      {
                                          PersonID = p.PersonID,
                                          Name = p.Name
                                      }).FirstOrDefault();

            JobWorkerJson.id = person.PersonID.ToString();
            JobWorkerJson.text = person.Name;

            return Json(JobWorkerJson);
        }






        public ActionResult GetEmployees(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            //var productCacheKeyHint = ConfigurationManager.AppSettings["EmployeeCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetEmployeeHelpList());
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
            //var productCacheKeyHint = ConfigurationManager.AppSettings["CurrencyCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetCurrencyHelpList());
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
                                                   where p.ID  == temp
                                             select p;
                ProductJson.Add(new ComboBoxResult()
                {
                      id = prod.FirstOrDefault().ID .ToString(),
                    text = prod.FirstOrDefault().Name
                });
            }
            return Json(ProductJson);
        }

        //public ActionResult GetCity(string searchTerm, int pageSize, int pageNum)
        //{
        //    var productCacheKeyHint = ConfigurationManager.AppSettings["CityCacheKeyHint"];
        //    AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetCityHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
        //    if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

        //    List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
        //    int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
        //    ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
        //    return new JsonpResult
        //    {
        //        Data = pagedAttendees,
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };
        //}

        public JsonResult GetCity(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetCityHelpList(searchTerm);
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

        public ActionResult GetCountry(string searchTerm, int pageSize, int pageNum)
        {
            //var productCacheKeyHint = ConfigurationManager.AppSettings["CountryCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetCountryHelpList());
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

        //public ActionResult GetPerson(string searchTerm, int pageSize, int pageNum)
        //{
        //    var productCacheKeyHint = ConfigurationManager.AppSettings["PersonCacheKeyHint"];
        //    AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPersonHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
        //    if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

        //    List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
        //    int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
        //    ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
        //    return new JsonpResult
        //    {
        //        Data = pagedAttendees,
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };
        //}

        public ActionResult GetPerson(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetPersonHelpList(searchTerm);
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

        //public ActionResult GetPersonBE(string searchTerm, int pageSize, int pageNum)
        //{
        //    var productCacheKeyHint = ConfigurationManager.AppSettings["PersonBECacheKeyHint"];
        //    AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPersonHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
        //    if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

        //    List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
        //    int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);
        //    ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);
        //    return new JsonpResult
        //    {
        //        Data = pagedAttendees,
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };
        //}

        public ActionResult GetPersonBE(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetPersonHelpList(searchTerm);
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

        public JsonResult SetSinglePerson(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Person> prod = from p in db.Persons
                                       where p.PersonID == Ids
                                       select p;

            ProductJson.id = prod.FirstOrDefault().PersonID.ToString();
            ProductJson.text = prod.FirstOrDefault().Name + ", " + prod.FirstOrDefault().Suffix + " [" + prod.FirstOrDefault().Code + "]";

            return Json(ProductJson);
        }

        public ActionResult GetPersonRateGroup(string searchTerm, int pageSize, int pageNum)
        {
            //var productCacheKeyHint = ConfigurationManager.AppSettings["PersonRateGroupCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetPersonRateGroupHelpList());
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
            //var productCacheKeyHint = ConfigurationManager.AppSettings["AccountGroupCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetAccountGroupHelpList());
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
            //var productCacheKeyHint = ConfigurationManager.AppSettings["AccountCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetAccountHelpList());
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
            //var productCacheKeyHint = ConfigurationManager.AppSettings["CostCenterCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetCostCenterHelpList());
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
                                              where p.CostCenterId== Ids
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
                //IEnumerable<Product> products = db.Products.Take(3);
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
            //var productCacheKeyHint = "AccountGroupCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetAccountGroupsHelpList());
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
            //var productCacheKeyHint = "GodownCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetGodownHelpList());
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
            //var productCacheKeyHint = "CalculationCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetCalculationHelpList());
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

        public ActionResult GetMenu(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            //var productCacheKeyHint = "MenuCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetMenuHelpList());
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

        public JsonResult SetMenu(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);                
                IEnumerable<Menu> prod = from p in db.Menu
                                                where p.MenuId == temp
                                                select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().MenuId.ToString(),
                    text = prod.FirstOrDefault().MenuName
                });
            }
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




        //public ActionResult GetTransporters(string searchTerm, int pageSize, int pageNum)
        //{
        //    //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
        //    var productCacheKeyHint = ConfigurationManager.AppSettings["TransporterCacheKeyHint"];

        //    //THis statement has been changed because GetProductHelpList was calling again and again. 

        //    AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetTransporterHelpList(), productCacheKeyHint, RefreshData.RefreshProductData);
        //    //AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(null, productCacheKeyHint);

        //    if (RefreshData.RefreshProductData == true) { RefreshData.RefreshProductData = false; }

        //    List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
        //    int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

        //    //Translate the attendees into a format the select2 dropdown expects
        //    ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

        //    //Return the data as a jsonp result
        //    return new JsonpResult
        //    {
        //        Data = pagedAttendees,
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };
        //}

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
            //var productCacheKeyHint = ConfigurationManager.AppSettings["ProductCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 


            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductHelpList());
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



        public ActionResult GetProductGroup(string searchTerm, int pageSize, int pageNum,int ? filter)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            //var productCacheKeyHint = ConfigurationManager.AppSettings["ProductGroupCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductGroupHelpList(filter));
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
            //var productCacheKeyHint = ConfigurationManager.AppSettings["ProductTypeCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductTypeHelpList());
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
              //var productCacheKeyHint = "CacheProductQuality";

              //THis statement has been changed because GetProductHelpList was calling again and again. 

              AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductQualityHelpList());
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
                  IEnumerable<Size> prod = from p in db.Size
                                                      where p.SizeId == temp
                                                      select p;
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
              //var productCacheKeyHint = ConfigurationManager.AppSettings["ProductManufacturerCacheKeyHint"];
              AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductManufacturerHelpList());
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
              //var productCacheKeyHint = ConfigurationManager.AppSettings["ProductDrawBackTarrifCacheKeyHint"];
              AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductDrawBackTarrifHelpList());
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
              //var productCacheKeyHint = ConfigurationManager.AppSettings["ProductProcessSequenceCacheKeyHint"];
              AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProductProcessSequenceHelpList());
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
              AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSizeHelpList());
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

              IEnumerable<Size> prod = from p in db.Size
                                       where p.SizeId == Ids
                                       select p;

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
              //var productCacheKeyHint = ConfigurationManager.AppSettings["ProductCollectionCacheKeyHint"];

              //THis statement has been changed because GetProductHelpList was calling again and again. 

              AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetRugCollectionHelpList());
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
              //var productCacheKeyHint = ConfigurationManager.AppSettings["ProcessCacheKeyHint"];
              AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProcessHelpList());
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

        //public JsonResult SetSingleMachine(int Ids)
        //{
        //    ComboBoxResult ProductJson = new ComboBoxResult();

        //    IEnumerable<Product> prod = from p in db.Product
        //                                where p.ProductId == Ids
        //                                select p;

        //    ProductJson.id = prod.FirstOrDefault().ProductId.ToString();
        //    ProductJson.text = prod.FirstOrDefault().ProductName;

        //    return Json(ProductJson);
        //}

        //public JsonResult SetMachine(string Ids)
        //{
        //    string[] subStr = Ids.Split(',');
        //    List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
        //    for (int i = 0; i < subStr.Length; i++)
        //    {
        //        int temp = Convert.ToInt32(subStr[i]);
        //        //IEnumerable<Product> products = db.Products.Take(3);
        //        IEnumerable<Product> prod = from p in db.Product
        //                                    where p.ProductId == temp
        //                                    select p;
        //        ProductJson.Add(new ComboBoxResult()
        //        {
        //            id = prod.FirstOrDefault().ProductId.ToString(),
        //            text = prod.FirstOrDefault().ProductName
        //        });
        //    }
        //    return Json(ProductJson);
        //}


        public JsonResult SetSingleMachine(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductUid> prod = from p in db.ProductUid
                                        where p.ProductUIDId == Ids
                                        select p;

            ProductJson.id = prod.FirstOrDefault().ProductUIDId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductUidName;

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
                IEnumerable<ProductUid> prod = from p in db.ProductUid
                                               where p.ProductUIDId == temp
                                            select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ProductUIDId.ToString(),
                    text = prod.FirstOrDefault().ProductUidName
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
                                                          where p.DocumentTypeId  == temp
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
            //var productCacheKeyHint = ConfigurationManager.AppSettings["SaleOrderCacheKeyHint"];

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSaleOrderHelpList());
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
                                                                      where H.SaleOrderAmendmentHeaderId  == Ids
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
                                                      where p.SaleOrderAmendmentHeaderId  == temp
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
                                                                where H.PackingHeaderId  == Ids
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
                                                        where p.PackingHeaderId  == temp
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
                                                  where p.ProductDesignId == temp  && p.ProductTypeId==rugid
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
                                              where p.ProductDesignId == Ids && p.ProductTypeId==rugid
                                              select p;

            ProductJson.id = prod.FirstOrDefault().ProductDesignId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductDesignName;

            return Json(ProductJson);
        }



        // Purchase Help 
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
                IEnumerable<PurchaseIndentHeader> prod = from p in db.PurchaseIndentHeader
                                                    where p.PurchaseIndentHeaderId == temp
                                                    select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PurchaseIndentHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
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
                                                                     where H.PurchaseGoodsReceiptHeaderId== Ids
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
                                                         where p.PurchaseGoodsReceiptHeaderId== temp
                                                         select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().PurchaseGoodsReceiptHeaderId.ToString(),
                    text = prod.FirstOrDefault().DocNo
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

        public JsonResult SetSingleJobReceive(int Ids)
        {
            ComboBoxResult JobReceiveJson = new ComboBoxResult();

            IEnumerable<JobReceiveHeader> JobReceive = from H in db.JobReceiveHeader
                                                                   where H.JobReceiveHeaderId == Ids
                                                                   select H;

            JobReceiveJson.id = JobReceive.FirstOrDefault().JobReceiveHeaderId.ToString();
            JobReceiveJson.text = JobReceive.FirstOrDefault().DocNo;

            return Json(JobReceiveJson);
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
                    text = prod.FirstOrDefault().DocNo
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

        public ActionResult GetSiteGodown(string searchTerm, int pageSize, int pageNum)
        {
            //Get the paged results and the total count of the results for this query. ProductCacheKeyHint
            var productCacheKeyHint = "SiteGodownCacheKeyHint";

            //THis statement has been changed because GetProductHelpList was calling again and again. 

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetSiteGodownHelpList(SiteId), productCacheKeyHint, RefreshData.RefreshProductData);
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
            var Query = cbl.GetEmployeeHelpListWithProcessFilter(filter, searchTerm);
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

        public JsonResult GetEmployeeWithDepartMent(string searchTerm, int pageSize, int pageNum, int filter)//filter:DepartmentId
        {
            var Query=cbl.GetEmployeeHelpListWithDepartMentFilter(filter, searchTerm);
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

        public JsonResult GetPersonRoles(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetPersonRoles(searchTerm);
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
        public JsonResult SetSinglePersonRole(int Ids)
        {
            ComboBoxResult PersonRoleJson = new ComboBoxResult();

            DocumentType PersonRole = (from b in db.DocumentType
                             where b.DocumentTypeId == Ids
                             select b).FirstOrDefault();

            PersonRoleJson.id = PersonRole.DocumentTypeId.ToString();
            PersonRoleJson.text = PersonRole.DocumentTypeName;

            return Json(PersonRoleJson);
        }
        public JsonResult SetPersonRole(string Ids)
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

        public JsonResult SetSingleUnit(string Ids)
        {
            ComboBoxResult MenuJson = new ComboBoxResult();

            ComboBoxResult person = (from b in db.Units
                                     where b.UnitId == Ids
                                     select new ComboBoxResult
                                     {
                                         id = b.UnitId,
                                         text = b.UnitName
                                     }).FirstOrDefault();

            MenuJson.id = person.id;
            MenuJson.text = person.text;

            return Json(MenuJson);
        }











        public JsonResult GetDeliveryTerms(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetDeliveryTerms(searchTerm);
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
        public JsonResult SetSingleDeliveryTerm(int Ids)
        {
            ComboBoxResult DeliveryTermJson = new ComboBoxResult();

            DeliveryTerms DeliveryTerm = (from b in db.DeliveryTerms
                                       where b.DeliveryTermsId == Ids
                                       select b).FirstOrDefault();

            DeliveryTermJson.id = DeliveryTerm.DeliveryTermsId .ToString();
            DeliveryTermJson.text = DeliveryTerm.DeliveryTermsName;

            return Json(DeliveryTermJson);
        }
        public JsonResult SetDeliveryTerm(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<DeliveryTerms> prod = from p in db.DeliveryTerms
                                                  where p.DeliveryTermsId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DeliveryTermsId.ToString(),
                    text = prod.FirstOrDefault().DeliveryTermsName
                });
            }
            return Json(ProductJson);
        }







        public JsonResult GetAddresses(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetAddresses(searchTerm);
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
        public JsonResult SetSingleAddress(int Ids)
        {
            ComboBoxResult AddressJson = new ComboBoxResult();

            var Address = (from b in db.PersonAddress
                           where b.PersonAddressID == Ids
                           select new
                           {
                               PersonAddressID = b.PersonAddressID,
                               PersonName = b.Person.Name
                           }).FirstOrDefault();

            AddressJson.id = Address.PersonAddressID.ToString();
            AddressJson.text = Address.PersonName;

            return Json(AddressJson);
        }
        public JsonResult SetAddress(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                var prod = from p in db.PersonAddress
                                                  where p.PersonAddressID == temp
                           select new ComboBoxResult
                                                 {
                                                     id = p.PersonAddressID.ToString(),
                                                     text = p.Person.Name
                                                 };

                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().id.ToString(),
                    text = prod.FirstOrDefault().text
                });
            }
            return Json(ProductJson);
        }








        public JsonResult GetCurrencies(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetCurrencies(searchTerm);
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
        public JsonResult SetSingleCurrency(int Ids)
        {
            ComboBoxResult CurrencyJson = new ComboBoxResult();

            Currency Currency = (from b in db.Currency
                                 where b.ID == Ids
                                       select b).FirstOrDefault();

            CurrencyJson.id = Currency.ID.ToString();
            CurrencyJson.text = Currency.Name;

            return Json(CurrencyJson);
        }
        public JsonResult SetCurrency(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
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




        public JsonResult GetSalesTaxGroupPerson(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetSalesTaxGroupPerson(searchTerm);
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
        public JsonResult SetSingleSalesTaxGroupPerson(int Ids)
        {
            ComboBoxResult SalesTaxGroupPersonJson = new ComboBoxResult();

            ChargeGroupPerson SalesTaxGroupPerson = (from b in db.ChargeGroupPerson
                                                     where b.ChargeGroupPersonId == Ids
                                       select b).FirstOrDefault();

            SalesTaxGroupPersonJson.id = SalesTaxGroupPerson.ChargeGroupPersonId.ToString();
            SalesTaxGroupPersonJson.text = SalesTaxGroupPerson.ChargeGroupPersonName;

            return Json(SalesTaxGroupPersonJson);
        }
        public JsonResult SetSalesTaxGroupPerson(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<ChargeGroupPerson> prod = from p in db.ChargeGroupPerson
                                                      where p.ChargeGroupPersonId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ChargeGroupPersonId.ToString(),
                    text = prod.FirstOrDefault().ChargeGroupPersonName
                });
            }
            return Json(ProductJson);
        }





        public JsonResult GetSalesTaxGroupProduct(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetSalesTaxGroupProduct(searchTerm);
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
        public JsonResult SetSingleSalesTaxGroupProduct(int Ids)
        {
            ComboBoxResult SalesTaxGroupProductJson = new ComboBoxResult();

            ChargeGroupProduct SalesTaxGroupProduct = (from b in db.ChargeGroupProduct
                                                     where b.ChargeGroupProductId == Ids
                                                     select b).FirstOrDefault();

            SalesTaxGroupProductJson.id = SalesTaxGroupProduct.ChargeGroupProductId.ToString();
            SalesTaxGroupProductJson.text = SalesTaxGroupProduct.ChargeGroupProductName;

            return Json(SalesTaxGroupProductJson);
        }
        public JsonResult SetSalesTaxGroupProduct(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<ChargeGroupProduct> prod = from p in db.ChargeGroupProduct
                                                      where p.ChargeGroupProductId == temp
                                                      select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ChargeGroupProductId.ToString(),
                    text = prod.FirstOrDefault().ChargeGroupProductName
                });
            }
            return Json(ProductJson);
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


        public JsonResult GetShipMethods(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetShipMethods(searchTerm);
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
        public JsonResult SetSingleShipMethod(int Ids)
        {
            ComboBoxResult ShipMethodJson = new ComboBoxResult();

            ShipMethod ShipMethod = (from b in db.ShipMethod
                                     where b.ShipMethodId == Ids
                                       select b).FirstOrDefault();

            ShipMethodJson.id = ShipMethod.ShipMethodId.ToString();
            ShipMethodJson.text = ShipMethod.ShipMethodName;

            return Json(ShipMethodJson);
        }
        public JsonResult SetShipMethod(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<ShipMethod> prod = from p in db.ShipMethod
                                               where p.ShipMethodId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ShipMethodId.ToString(),
                    text = prod.FirstOrDefault().ShipMethodName
                });
            }
            return Json(ProductJson);
        }



        public JsonResult GetDocumentShipMethods(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetDocumentShipMethods(searchTerm);
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
        public JsonResult SetSingleDocumentShipMethod(int Ids)
        {
            ComboBoxResult DocumentShipMethodJson = new ComboBoxResult();

            DocumentShipMethod DocumentShipMethod = (from b in db.DocumentShipMethod
                                                     where b.DocumentShipMethodId == Ids
                                       select b).FirstOrDefault();

            DocumentShipMethodJson.id = DocumentShipMethod.DocumentShipMethodId.ToString();
            DocumentShipMethodJson.text = DocumentShipMethod.DocumentShipMethodName;

            return Json(DocumentShipMethodJson);
        }
        public JsonResult SetDocumentShipMethod(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<DocumentShipMethod> prod = from p in db.DocumentShipMethod
                                                       where p.DocumentShipMethodId == temp
                                                 select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().DocumentShipMethodId.ToString(),
                    text = prod.FirstOrDefault().DocumentShipMethodName
                });
            }
            return Json(ProductJson);
        }












        public JsonResult GetTransporters(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetTransporters(searchTerm);
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

        public JsonResult GetAgents(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetAgents(searchTerm);
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


        public JsonResult GetFinanciers(string searchTerm, int pageSize, int pageNum, int? filter)
        {
            var Query = cbl.GetFinanciers(searchTerm, filter);
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


        public JsonResult GetSalesExecutives(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetSalesExecutives(searchTerm);
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


        public ActionResult GetProcessWithChildProcess(string searchTerm, int pageSize, int pageNum, int? filter)
        {
            //var productCacheKeyHint = ConfigurationManager.AppSettings["ProcessCacheKeyHint"];
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(cbl.GetProcessWithChildProcessHelpList(filter));
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

        public JsonResult SetSingleStockIn(int Ids)
        {
            ComboBoxResult StockInJson = new ComboBoxResult();

            var StockInLine = from L in db.Stock
                              join H in db.StockHeader on L.StockHeaderId equals H.StockHeaderId into StockHeaderTable
                              from StockHeaderTab in StockHeaderTable.DefaultIfEmpty()
                              where L.StockId == Ids
                              select new
                              {
                                  StockInId = L.StockId,
                                  StockInNo = StockHeaderTab.DocNo
                              };

            StockInJson.id = StockInLine.FirstOrDefault().ToString();
            StockInJson.text = StockInLine.FirstOrDefault().StockInNo;

            return Json(StockInJson);
        }


        public JsonResult GetLedgerAccountForGroup(string searchTerm, int pageSize, int pageNum, int? filter)
        {
            var Query = cbl.GetLedgerAccountForGroup(searchTerm, filter);
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
        public JsonResult SetSingleLedgerAccount(int Ids)
        {
            ComboBoxResult LedgerAccountJson = new ComboBoxResult();

            LedgerAccount LedgerAccount = (from b in db.LedgerAccount
                                                       where b.LedgerAccountId == Ids
                                                       select b).FirstOrDefault();

            LedgerAccountJson.id = LedgerAccount.LedgerAccountId.ToString();
            LedgerAccountJson.text = LedgerAccount.LedgerAccountName;

            return Json(LedgerAccountJson);
        }
        public JsonResult SetLedgerAccount(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
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

        public JsonResult SetSingleJobOrderLine(int Ids)
        {
            ComboBoxResult JobOrderJson = new ComboBoxResult();

            var JobOrder = from L in db.JobOrderLine
                              where L.JobOrderLineId == Ids
                              select new
                              {
                                  JobOrderLineId = L.JobOrderLineId,
                                  JobOrderNo = L.JobOrderHeader.DocNo
                              };

            JobOrderJson.id = JobOrder.FirstOrDefault().ToString();
            JobOrderJson.text = JobOrder.FirstOrDefault().JobOrderNo;

            return Json(JobOrderJson);
        }

        public JsonResult SetSingleSaleInvoiceLine(int Ids)
        {
            ComboBoxResult SaleInvoiceJson = new ComboBoxResult();

            var SaleInvoice = from L in db.SaleInvoiceLine
                           where L.SaleInvoiceLineId == Ids
                           select new
                           {
                               SaleInvoiceLineId = L.SaleInvoiceLineId,
                               SaleInvoiceNo = L.SaleInvoiceHeader.DocNo
                           };

            SaleInvoiceJson.id = SaleInvoice.FirstOrDefault().ToString();
            SaleInvoiceJson.text = SaleInvoice.FirstOrDefault().SaleInvoiceNo;

            return Json(SaleInvoiceJson);
        }

        public JsonResult GetAdditionalCharges(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetAdditionalCharges(searchTerm);
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

        /// <summary>
        /// /////////////////////////////////////////////////Finiance Helps///////////////////////////////////////
        /// </summary>
        /// <param name="disposing"></param>
        /// 

        public JsonResult GetSites(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetSites(searchTerm);
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
        public JsonResult SetSingleSite(int Ids)
        {
            ComboBoxResult SiteJson = new ComboBoxResult();

            Site Site = (from b in db.Site
                         where b.SiteId == Ids
                         select b).FirstOrDefault();

            SiteJson.id = Site.SiteId.ToString();
            SiteJson.text = Site.SiteName;

            return Json(SiteJson);
        }


        public JsonResult GetDivisions(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetDivisions(searchTerm);
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
        public JsonResult SetSingleDivision(int Ids)
        {
            ComboBoxResult DivisionJson = new ComboBoxResult();

            Division Division = (from b in db.Divisions
                                 where b.DivisionId == Ids
                                 select b).FirstOrDefault();

            DivisionJson.id = Division.DivisionId.ToString();
            DivisionJson.text = Division.DivisionName;

            return Json(DivisionJson);
        }

        public JsonResult GetTdsGroup(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetTdsGroups(searchTerm);
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

        public JsonResult SetSingleTdsGroup(int Ids)
        {
            ComboBoxResult TdsGroupJson = new ComboBoxResult();

            TdsGroup TdsGroup = (from b in db.TdsGroup
                                 where b.TdsGroupId == Ids
                                 select b).FirstOrDefault();

            TdsGroupJson.id = TdsGroup.TdsGroupId.ToString();
            TdsGroupJson.text = TdsGroup.TdsGroupName;

            return Json(TdsGroupJson);
        }


        public JsonResult GetTdsCategory(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetTdsCategory(searchTerm);
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

        public JsonResult SetSingleTdsCategory(int Ids)
        {
            ComboBoxResult TdsCategoryJson = new ComboBoxResult();

            TdsCategory TdsCategory = (from b in db.TdsCategory
                                       where b.TdsCategoryId == Ids
                                       select b).FirstOrDefault();

            TdsCategoryJson.id = TdsCategory.TdsCategoryId.ToString();
            TdsCategoryJson.text = TdsCategory.TdsCategoryName;

            return Json(TdsCategoryJson);
        }


        public JsonResult GetSalesTaxGroupParty(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetSalesTaxGroupPerson(searchTerm);
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

        public JsonResult SetSingleSalesTaxGroupParty(int Ids)
        {
            ComboBoxResult SalesTaxGroupPartyJson = new ComboBoxResult();

            ChargeGroupPerson SalesTaxGroupParty = (from b in db.ChargeGroupPerson
                                                    where b.ChargeGroupPersonId == Ids
                                                    select b).FirstOrDefault();

            SalesTaxGroupPartyJson.id = SalesTaxGroupParty.ChargeGroupPersonId.ToString();
            SalesTaxGroupPartyJson.text = SalesTaxGroupParty.ChargeGroupPersonName;

            return Json(SalesTaxGroupPartyJson);
        }


        /// /////////////////////////////////////////////////End Finiance Helps///////////////////////////////////////
        /// 
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

        public ActionResult SetDate(string Proc)
        {
            string Select2List = db.Database.SqlQuery<string>(Proc).FirstOrDefault();
            return Json(Select2List, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetChargeTypeList(string searchTerm, int pageSize, int pageNum)
        {

            var temp = cbl.GetChargeTypeList(searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = cbl.GetChargeTypeList(searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleChargeType(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ChargeType> prod = from p in db.ChargeType
                                           where p.ChargeTypeId == Ids
                                           select p;

            ProductJson.id = prod.FirstOrDefault().ChargeTypeId.ToString();
            ProductJson.text = prod.FirstOrDefault().ChargeTypeName;

            return Json(ProductJson);
        }


        public ActionResult GetChargeList(string searchTerm, int pageSize, int pageNum)
        {

            var temp = cbl.GetChargeList(searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = cbl.GetChargeList(searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleCharge(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Charge> prod = from p in db.Charge
                                       where p.ChargeId == Ids
                                       select p;

            ProductJson.id = prod.FirstOrDefault().ChargeId.ToString();
            ProductJson.text = prod.FirstOrDefault().ChargeName;

            return Json(ProductJson);
        }

        public ActionResult GetChargeListWithCode(string searchTerm, int pageSize, int pageNum)
        {

            var temp = cbl.GetChargeListWithCode(searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = cbl.GetChargeListWithCode(searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public JsonResult SetChargeWithCode(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                string temp = subStr[i];
                //IEnumerable<Product> products = db.Products.Take(3);
                IEnumerable<Charge> prod = from p in db.Charge
                                           where p.ChargeCode == temp
                                           select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ChargeCode.ToString(),
                    text = prod.FirstOrDefault().ChargeName
                });
            }
            return Json(ProductJson);
        }
        public JsonResult SetSingleChargeWithCode(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Charge> prod = from p in db.Charge
                                       where p.ChargeId == Ids
                                       select p;

            ProductJson.id = prod.FirstOrDefault().ChargeCode;
            ProductJson.text = prod.FirstOrDefault().ChargeName;

            return Json(ProductJson);
        }

        public ActionResult GetCostCenterWithDocType(string searchTerm, int pageSize, int pageNum)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = cbl.GetCostCenterHelpListWithDocTypes(searchTerm, SiteId, DivisionId).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = cbl.GetCostCenterHelpListWithDocTypes(searchTerm, SiteId, DivisionId).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
            {
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);
                TempData.Remove("CSEXC");
            }
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



        public JsonResult GetProductHelpListWithProductNatureFilter(string searchTerm, int pageSize, int pageNum, int filter)//filter:ProductNature
        {

            var temp = cbl.GetProductHelpListWithProductNatureFilter(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();
            var count = cbl.GetProductHelpListWithProductNatureFilter(filter, searchTerm).Count();
            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public JsonResult GetChargeGroupProducts(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetChargeGroupProducts(searchTerm);
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
        public JsonResult SetSingleChargeGroupProduct(int Ids)
        {
            ComboBoxResult ChargeGroupProductJson = new ComboBoxResult();

            ChargeGroupProduct ChargeGroupProduct = (from b in db.ChargeGroupProduct
                                                     where b.ChargeGroupProductId == Ids
                                                     select b).FirstOrDefault();

            ChargeGroupProductJson.id = ChargeGroupProduct.ChargeGroupProductId.ToString();
            ChargeGroupProductJson.text = ChargeGroupProduct.ChargeGroupProductName;

            return Json(ChargeGroupProductJson);
        }
        public JsonResult SetChargeGroupProduct(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<ChargeGroupProduct> prod = from p in db.ChargeGroupProduct
                                                       where p.ChargeGroupProductId == temp
                                                       select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = prod.FirstOrDefault().ChargeGroupProductId.ToString(),
                    text = prod.FirstOrDefault().ChargeGroupProductName
                });
            }
            return Json(ProductJson);
        }

        public JsonResult GetBinLocations(string searchTerm, int pageSize, int pageNum, int filter)
        {
            var Query = cbl.GetBinLocations(searchTerm, filter);
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
        public JsonResult SetSingleBinLocation(int Ids)
        {
            ComboBoxResult BinLocationJson = new ComboBoxResult();

            BinLocation BinLocation = (from b in db.BinLocation
                                       where b.BinLocationId == Ids
                                       select b).FirstOrDefault();

            BinLocationJson.id = BinLocation.BinLocationId.ToString();
            BinLocationJson.text = BinLocation.BinLocationName;

            return Json(BinLocationJson);
        }

        public JsonResult GetProductIndexFilterParameter(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetProductIndexFilterParameter(searchTerm);
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


        public JsonResult SetSingleIndexFilterParameter(string Ids)
        {
            ComboBoxResult IndexFilterParameterJson = new ComboBoxResult();

            IndexFilterParameterJson.id = Ids;
            IndexFilterParameterJson.text = Ids;

            return Json(IndexFilterParameterJson);
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

        public JsonResult SetSingleCalculationFooter(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            var prod = (from p in db.CalculationFooter
                        join t in db.Charge on p.ChargeId equals t.ChargeId
                        where p.CalculationFooterLineId == Ids
                        select new
                        {
                            ChargeName = t.ChargeName,
                            Id = p.CalculationFooterLineId,
                        });

            ProductJson.id = prod.FirstOrDefault().Id.ToString();
            ProductJson.text = prod.FirstOrDefault().ChargeName;

            return Json(ProductJson);
        
        
        }

        public ActionResult GetRoles(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = cbl.GetRoles(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleRoles(string Ids)
        {
            return Json(cbl.GetRole(Ids));
        }

        public JsonResult SetRoles(string Ids)
        {
            return Json(cbl.GetMultipleRoles(Ids));
        }

        
        public JsonResult SetSingleProductType(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductType> prod = from p in db.ProductTypes
                                        where p.ProductTypeId == Ids
                                        select p;

            ProductJson.id = prod.FirstOrDefault().ProductTypeId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductTypeName;

            return Json(ProductJson);
        }	

        public JsonResult SetSingleProductNature(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ProductNature> prod = from p in db.ProductNature
                                        where p.ProductNatureId == Ids
                                        select p;

            ProductJson.id = prod.FirstOrDefault().ProductNatureId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductNatureName;

            return Json(ProductJson);
        }

        public JsonResult GetLedgerAccounts(string searchTerm, int pageSize, int pageNum)
        {
            var Query = cbl.GetLedgerAccount(searchTerm);
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


    }
}

