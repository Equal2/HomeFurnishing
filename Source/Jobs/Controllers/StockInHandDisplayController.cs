using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Presentation.ViewModels;
using Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Model.ViewModel;
using Reports.Reports;
using Microsoft.Reporting.WebForms;
using System.Web.Script.Serialization;
using System.Text;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    // [Authorize]
    public class StockInHandDisplayController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IStockInHandService _StockInHandService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public StockInHandDisplayController(IStockInHandService StockInHandService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
           
            _StockInHandService = StockInHandService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }


        public ActionResult ProductTypeIndex(int Id,string Routeid)
        {
            
            //var pt = new ProductTypeService(_unitOfWork).GetRawAndOtherMaterialProductTypes().Where(m => m.IsActive != false).ToList();
            var pt = new ProductTypeService(_unitOfWork).GetProductTypes(Id).Where(m => m.IsActive != false).ToList();
            var ProductNatureName = new ProductNatureService(_unitOfWork).Find(Id);
            ViewBag.RouteId = Routeid.ToString();
            if (ProductNatureName != null)
            {
                ViewBag.productNatureName = ProductNatureName.ProductNatureName;               
            }
            else
            {
                ViewBag.productNatureName = "Product Nature";
            }
            return View("ProductTypeIndex", pt);
        }

        public ActionResult ProductNatureIndex()
        {
            var  Route = Url.RequestContext.RouteData.Values["id"];
            ViewBag.RouteId = Route.ToString();
            var pt = new ProductNatureService(_unitOfWork).GetProductNatureList().ToList();
            return View("ProductNatureIndex", pt);
        }


        [HandleError()]
        public ActionResult GetStockInHand(int id,string Routeid)
        {

            ProductTypeSettings ProductTypeSetting = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettings(id);
            if (ProductTypeSetting != null)
            {
                System.Web.HttpContext.Current.Session["Route"] = Routeid;
                System.Web.HttpContext.Current.Session["ProductTypeId"] = id;
                var settings = new StockInHandSettingService(_unitOfWork).GetTrailBalanceSetting(User.Identity.Name, id, Routeid);
               List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            //if (settings == null)
            //{
            //    return RedirectToAction("Create", "StockInHandSetting", new { ProductTypeId = id, ControllerName = "StockInHandDisplay" }).Warning("Please create Stock In Hand settings");
            //}

            if (settings == null)
            {
                settings = new StockInHandSettingService(_unitOfWork).GetTrailBalanceSetting(id, Routeid);
                if (settings == null)
                {
                    return RedirectToAction("Create", "StockInHandSetting", new { ProductTypeId = id, ControllerName = "StockInHandDisplay",Route=Routeid }).Warning("Please create Stock In Hand settings");
                }
            }

            string FromDate = settings.ToDate.HasValue ? settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
            string ToDate = settings.ToDate.HasValue ? settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";


            Dimension1Types Dimension1Type = new ProductTypeService(_unitOfWork).GetProductTypeDimension1Types(id);
            Dimension2Types Dimension2Type = new ProductTypeService(_unitOfWork).GetProductTypeDimension2Types(id);

            
            ProductType ProductType = new ProductTypeService(_unitOfWork).Find(id);

                if (ProductTypeSetting.Dimension1Caption == null)
                {
                    ViewBag.Dimension1Caption = "Dimension1";
                }
                else
                {
                    ViewBag.Dimension1Caption = ProductTypeSetting.Dimension1Caption;
                }

                if (ProductTypeSetting.Dimension2Caption == null)
                {
                    ViewBag.Dimension2Caption = "Dimension2";
                }
                else
                {
                    ViewBag.Dimension2Caption = ProductTypeSetting.Dimension2Caption;
                }

                if (ProductTypeSetting.Dimension3Caption == null)
                {
                    ViewBag.Dimension3Caption = "Dimension3";
                }
                else
                {
                    ViewBag.Dimension3Caption = ProductTypeSetting.Dimension3Caption;
                }

                if (ProductTypeSetting.Dimension4Caption == null)
                {
                    ViewBag.Dimension4Caption = "Dimension4";
                }
                else
                {
                    ViewBag.Dimension4Caption = ProductTypeSetting.Dimension4Caption;
                }

                if (ProductTypeSetting.ProductNameCaption == null)
                {
                    ViewBag.ProductNameCaption = "Product";
                }
                else
                {
                    ViewBag.ProductNameCaption = ProductTypeSetting.ProductNameCaption;
                }

                ViewBag.FilterRemark = "( From Date : " + FromDate + " To Date : " + ToDate + " Product Type : " + ProductType.ProductTypeName + " )";
                ViewBag.id = id;
                ViewBag.ControllerName = "StockInHandDisplay";
                ViewBag.GroupOn = settings.GroupOn;
                int ShowOpening = (settings.ShowOpening == true ? 1 : 0);
                ViewBag.ShowOpening = ShowOpening;
                if(Routeid== "Stock")
                {
                    ViewBag.TitleName = "Stock In Hand Display";
                }
                else
                {
                    ViewBag.TitleName = "Stock Process Display";
                }
                ViewBag.Routeid = Routeid;
                //if(settings.DisplayType==DisplayTypeConstants.Balance)
                return View("StockInHandIndex");
            }
            else
            {
                throw new ApplicationException("Product Type settings Is not Define");
            }

            //else
            //    return View("StockInHandSummaryIndex");
        }

        public JsonResult GetStockInHandJson(int id,string Routeid)
        {
            JsonResult json = Json(new { data = _StockInHandService.GetStockInHandDisplay(id, User.Identity.Name, Routeid) }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
           
        }



        public ActionResult GetStockLedger(int? ProductId, int? Dim1, int? Dim2, int? Dim3, int? Dim4, int? Process, string LotNo, int? Godown,int? PersonId)//LedgerAccountId
        {
            var Name = "";
            var TempName = "";
            ViewBag.ProductId = ProductId;
            Name = (from p in db.Product
                    where p.ProductId == ProductId
                    select p).AsNoTracking().FirstOrDefault().ProductName;

            if (Dim1 != null)
            {
                TempName = "";
                TempName = (from P in db.Dimension1
                            join PT in db.ProductTypes on P.ProductTypeId equals PT.ProductTypeId into tablePT
                            from tabPT in tablePT.DefaultIfEmpty()
                            join DT in db.Dimension1Types on tabPT.Dimension1TypeId equals DT.Dimension1TypeId into tableDT
                            from tabDT in tableDT.DefaultIfEmpty()
                            where P.Dimension1Id == Dim1
                            select ", " + tabDT.Dimension1TypeName + " : " + P.Dimension1Name
                           ).AsNoTracking().FirstOrDefault();

                if (TempName != "")
                {
                    Name = Name + TempName;
                }


            }

            if (Dim2 != null)
            {
                TempName = "";
                TempName = (from P in db.Dimension2
                            join PT in db.ProductTypes on P.ProductTypeId equals PT.ProductTypeId into tablePT
                            from tabPT in tablePT.DefaultIfEmpty()
                            join DT in db.Dimension2Types on tabPT.Dimension1TypeId equals DT.Dimension2TypeId into tableDT
                            from tabDT in tableDT.DefaultIfEmpty()
                            where P.Dimension2Id == Dim2
                            select ", " + tabDT.Dimension2TypeName + " : " + P.Dimension2Name
                           ).AsNoTracking().FirstOrDefault();

                if (TempName != "")
                {
                    Name = Name + TempName;
                }
            }

            if (LotNo != null && LotNo != "")
            {
                Name = Name + ", LotNo : " + LotNo;
            }
            ViewBag.Name = Name;
            ViewBag.Dim1 = Dim1;
            ViewBag.Dim2 = Dim2;
            ViewBag.Dim3 = Dim3;
            ViewBag.Dim4 = Dim4;
            ViewBag.Process = Process;
            ViewBag.LotNo = LotNo;
            ViewBag.Godown = Godown;
            ViewBag.PersonId = PersonId;

            string Routeid = (string)System.Web.HttpContext.Current.Session["Route"];
            int ProductTypeid=(int)System.Web.HttpContext.Current.Session["ProductTypeId"];           

            var settings = new StockInHandSettingService(_unitOfWork).GetTrailBalanceSetting(User.Identity.Name, ProductTypeid, Routeid);
            if (settings == null)
            {
                settings = new StockInHandSettingService(_unitOfWork).GetTrailBalanceSetting(ProductTypeid, Routeid);
            }

            ViewBag.GroupOn = settings.GroupOn;
            string FromDate = settings.ToDate.HasValue ? settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
            string ToDate = settings.ToDate.HasValue ? settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
            ViewBag.FilterRemark = "( From Date : " + FromDate + " To Date : " + ToDate + " )";
            return View("StockLedgerIndex");
        }

        public JsonResult GetStockLedgerJson(int? ProductId, int? Dim1, int? Dim2, int? Dim3, int? Dim4, int? Process, string LotNo, int? Godown,int? PersonId)
        {
            string Routeid = (string)System.Web.HttpContext.Current.Session["Route"];
            int ProductTypeid = (int)System.Web.HttpContext.Current.Session["ProductTypeId"];

            //return Json(new { data = _StockInHandService.GetStockLedger(ProductId, Dim1, Dim2, Process, LotNo, Godown, User.Identity.Name) }, JsonRequestBehavior.AllowGet);
            var T = Json(new { data = _StockInHandService.GetStockLedger(ProductId, Dim1, Dim2, Dim3, Dim4, Process, LotNo, Godown, PersonId, User.Identity.Name, ProductTypeid, Routeid).ToList() }, JsonRequestBehavior.AllowGet);
            T.MaxJsonLength = int.MaxValue;
            return T;

        }


        public ActionResult DocumentMenu(int DocTypeId, int DocId)
        {
            if (DocTypeId == 0 || DocId == 0)
            {
                return View("Error");
            }

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var DocumentType = new DocumentTypeService(_unitOfWork).Find(DocTypeId);


            if (DocumentType.ControllerActionId.HasValue && DocumentType.ControllerActionId.Value > 0)
            {
                ControllerAction CA = new ControllerActionService(_unitOfWork).Find(DocumentType.ControllerActionId.Value);

                if (CA == null)
                {
                    return View("~/Views/Shared/UnderImplementation.cshtml");
                }
                else if (!string.IsNullOrEmpty(DocumentType.DomainName))
                {
                    return Redirect(System.Configuration.ConfigurationManager.AppSettings[DocumentType.DomainName] + "/" + CA.ControllerName + "/" + CA.ActionName + "/" + DocId);
                }
                else
                {
                    return RedirectToAction(CA.ActionName, CA.ControllerName, new { id = DocId });
                }
            }
            ViewBag.RetUrl = System.Web.HttpContext.Current.Request.UrlReferrer;
            HandleErrorInfo Excp = new HandleErrorInfo(new Exception("Document Settings not Configured"), "StockInHand", "DocumentMenu");
            return View("Error", Excp);
        }


        public ActionResult GeneratePrints(string Route, string ActionName, int Id = 0)
        {

            string PrintProcedure = "";


            if (!string.IsNullOrEmpty(ActionName))
            {

                var Settings = new StockInHandSettingService(_unitOfWork).GetTrailBalanceSetting(User.Identity.Name, Id,Route);



                string SqlParameterGroupOn;
                string SqlParameterShowBalance;
                string SqlParameterProdType;



                string SqlParameterSiteId = Settings.SiteIds;
                string DivisionId = Settings.DivisionIds;
                string SqlParameterFromDate = Settings.FromDate.HasValue ? Settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
                string SqlParameterToDate = Settings.ToDate.HasValue ? Settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";


                SqlParameterProdType = Id.ToString();

                if (string.IsNullOrEmpty(Settings.GroupOn))
                    SqlParameterGroupOn = "NULL";
                else
                    SqlParameterGroupOn = Settings.GroupOn.ToString();

                if (string.IsNullOrEmpty(Settings.ShowBalance) || Settings.ShowBalance == StockInHandShowBalanceConstants.All)
                    SqlParameterShowBalance = "NULL";
                else
                    SqlParameterShowBalance = Settings.ShowBalance.ToString();


                string[] s = Settings.GroupOn.ToString().Split(new Char[] { ',' });
                int i = s.Length;
                string ReportName = "SELECT 'StockInDisplay" + i.ToString() + "'";

                //Web.spStockInHand
                if (ActionName == "StockInHand") 
                     //PrintProcedure = "Web.spStockInHand_ForMultipleProductTest  @ProductType ='" + SqlParameterProdType.ToString() + "', @Site='" + SqlParameterSiteId.ToString() + "', @FromDate='" + SqlParameterFromDate.ToString() + "', @ToDate='" + SqlParameterToDate.ToString() + "', @GroupOn='" + SqlParameterGroupOn.ToString() + "', @ShowOpening='" + (Settings.ShowOpening == true ? 1 : 0) + "'";

               

                try
                {
                    DataTable Dt = new DataTable();
                    List<byte[]> PdfStream = new List<byte[]>();
                    DirectReportPrint drp = new DirectReportPrint();
                    byte[] Pdf;
                        //Pdf = drp.DirectDocumentPrint(PrintProcedure, User.Identity.Name, 0); //drp.DirectPrint(Dt, User.Identity.Name);
                        //PdfStream.Add(Pdf);      
                        Pdf = drp.rsDirectDocumentPrintDisplay(ReportName, Route, User.Identity.Name, Id);
                        PdfStream.Add(Pdf);

                    PdfMerger pm = new PdfMerger();

                    byte[] Merge = pm.MergeFiles(PdfStream);

                    if (Merge != null)
                        return File(Merge, "application/pdf");

                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
                }


                return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

        }

        //[HttpPost]
        //[ValidateAntiForgeryToken] Prints(List<StockInHandViewModel> things)
        //public ActionResult Prints(List<Thing> things)
        //{
        //    var t = things;

        //    List<Thing> query = (from c in t.AsEnumerable() select c).ToList();

        //    DataTable Dt = query.ToDataTable();

        //    try
        //    {
        //        // DataTable Dt = new DataTable();

        //        List<byte[]> PdfStream = new List<byte[]>();
        //        DirectReportPrint drp = new DirectReportPrint();
        //        byte[] Pdf;
        //        Pdf = drp.DirectPrint(Dt, User.Identity.Name);
        //        PdfStream.Add(Pdf);


        //        PdfMerger pm = new PdfMerger();

        //        byte[] Merge = pm.MergeFiles(PdfStream);

        //        if (Merge != null)
        //            return File(Merge, "application/pdf");

        //    }

        //    catch (Exception ex)
        //    {
        //        string message = _exception.HandleException(ex);
        //       return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
        //    }


        //    return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet);


        //}



        //public ActionResult ReportPrint(string ActionName, int id = 0)
        //{
        //    var SubReportDataList = new List<DataTable>();
        //    var SubReportNameList = new List<string>();
        //    DataTable ReportData = new DataTable();
        //    Dictionary<string, string> ReportFilters = new Dictionary<string, string>();
        //    StringBuilder queryString = new StringBuilder();





        //    var Settings = new StockInHandSettingService(_unitOfWork).GetTrailBalanceSetting(User.Identity.Name, id);
        //    if (Settings == null)
        //    {
        //        Settings = new StockInHandSettingService(_unitOfWork).GetTrailBalanceSetting(id);
        //    }
        //    List<ReportParameter> Params = new List<ReportParameter>();

        //    ReportParameter Param = new ReportParameter();
        //    Param.Name = CustomStringOp.CleanCode("@GroupOn");
        //    Param.Values.Add(Settings.GroupOn);

        //    Param.Name = CustomStringOp.CleanCode("@Site");
        //    Param.Values.Add(Settings.SiteIds);

        //    Param.Name = CustomStringOp.CleanCode("@ToDate");
        //    Param.Values.Add((Settings.ToDate.ToString() != "" ? String.Format("{0:MMMM dd yyyy}", Settings.ToDate.ToString()) : "Null"));

        //    Param.Name = CustomStringOp.CleanCode("@FromDate");
        //    Param.Values.Add((Settings.FromDate.ToString() != "" ? String.Format("{0:MMMM dd yyyy}", Settings.FromDate.ToString()) : "Null"));

        //    Param.Name = CustomStringOp.CleanCode("@ProductType");
        //    Param.Values.Add(id.ToString());

        //    Param.Name = CustomStringOp.CleanCode("@ShowBalance");
        //    Param.Values.Add(Settings.ShowBalance.ToString());

        //    Param.Name = CustomStringOp.CleanCode("@ShowOpening");
        //    Param.Values.Add((Settings.ShowOpening == true ? 1 : 0).ToString());

        //    Params.Add(Param);


        //    string ReportName = "";

        //    ApplicationDbContext Db = new ApplicationDbContext();






        //    var uid = Guid.NewGuid();
        //    using (ApplicationDbContext context = new ApplicationDbContext())
        //    {
        //        foreach (var item in Params)
        //        {
        //            ReportUIDValues rid = new ReportUIDValues();
        //            rid.UID = uid;
        //            rid.Type = item.Name;
        //            rid.Value = item.Values[0];

        //            rid.ObjectState = Model.ObjectState.Added;
        //            context.ReportUIDValues.Add(rid);
        //        }
        //        context.SaveChanges();
        //    }




        //    ReportName = "StockInDisplay1"; //Db.Database.SqlQuery<string>(header.ReportSQL.Replace("REPORTUID", uid.ToString())).FirstOrDefault();

        //    using (ApplicationDbContext context = new ApplicationDbContext())
        //    {
        //        var Items = context.ReportUIDValues.Where(m => m.UID == uid).ToList();

        //        foreach (var item in Items)
        //        {
        //            item.ObjectState = Model.ObjectState.Deleted;
        //            context.ReportUIDValues.Remove(item);
        //        }
        //        context.SaveChanges();
        //    }

        //    ReportParameter UserName = new ReportParameter();
        //    UserName.Name = "PrintedBy";
        //    UserName.Values.Add(User.Identity.Name);
        //    Params.Add(UserName);

        //    ReportParameter ConString = new ReportParameter();
        //    ConString.Name = "DatabaseConnectionString";
        //    ConString.Values.Add((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]);
        //    Params.Add(ConString);

        //    string mimtype;
        //    ReportGenerateService c = new ReportGenerateService();
        //    byte[] BAR;

        //    BAR = c.ReportGenerateCustom(out mimtype, ReportFileTypeConstants.PDF, User.Identity.Name, Params, ReportName);

        //    XElement s = new XElement(CustomStringOp.CleanCode(ReportName));
        //    XElement Name = new XElement("Filters");
        //    foreach (var Rec in ReportFilters)
        //    {
        //        Name.Add(new XElement(CustomStringOp.CleanCode(Rec.Key), Rec.Value));
        //    }
        //    s.Add(Name);


        //    //LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
        //    //{
        //    //    DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.Report).DocumentTypeId,
        //    //    DocId = header.ReportHeaderId,
        //    //    ActivityType = (int)ActivityTypeContants.Report,
        //    //    xEModifications = s,
        //    //}));

        //    if (BAR.Length == 1)
        //    {
        //        ViewBag.Message = "Report Name is not define.";
        //        return View("Close");
        //    }
        //    else if (BAR.Length == 2)
        //    {
        //        ViewBag.Message = "Report Title is not define.";
        //        return View("Close");
        //    }
        //    else
        //    {
        //        if (mimtype == "application/vnd.ms-excel")
        //            return File(BAR, mimtype, ReportName + ".xls");
        //        else
        //            return File(BAR, mimtype);
        //    }


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
