using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using System.Data;
using Model.ViewModels;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class Display_SaleOrderInventoryStatusController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        protected string connectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
        IDisplay_SaleOrderInventoryStatusService _SaleOrderInventoryStatusService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        private const string ReportType_Detail = "Detail";
        private const string ReportType_Summary = "Summary";

        private const string ReportFor_Pending = "Pending";
        private const string ReportFor_All = "All";

        public Display_SaleOrderInventoryStatusController(IDisplay_SaleOrderInventoryStatusService Display_SaleOrderInventoryStatusService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleOrderInventoryStatusService = Display_SaleOrderInventoryStatusService;
            _exception = exec;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult Display_SaleOrderInventoryStatus(int MenuId)
        {
            Menu Menu = new MenuService(_unitOfWork).Find(MenuId);

            Display_SaleOrderInventoryStatusViewModel vm = new Display_SaleOrderInventoryStatusViewModel();
            
            //vm.ReportType = Menu.MenuName;

            System.Web.HttpContext.Current.Session["SettingList"] = new List<SaleOrderInventoryStatusDisplayFilterSettings>();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = new SaleOrderInventoryStatusDisplayFilterSettings();
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            List<SelectListItem> ReportType = new List<SelectListItem>();
            ReportType.Add(new SelectListItem { Text = ReportType_Detail, Value = ReportType_Detail });
            ReportType.Add(new SelectListItem { Text = ReportType_Summary, Value = ReportType_Summary });
            ViewBag.ReportType = new SelectList(ReportType, "Value", "Text");

            List<SelectListItem> ReportFor = new List<SelectListItem>();
            ReportFor.Add(new SelectListItem { Text = ReportFor_All, Value = ReportFor_All });
            ReportFor.Add(new SelectListItem { Text = ReportFor_Pending, Value = ReportFor_Pending });
            ViewBag.ReportFor = new SelectList(ReportFor, "Value", "Text");


            vm.FromDate = "01/Apr/" + DateTime.Now.Date.Year.ToString();
            vm.ToDate = DateTime.Now.Date.ToString("dd/MMM/yyyy");
            vm.StatusOnDate = DateTime.Now.Date.ToString("dd/MMM/yyyy");

            vm.ReportType = "Detail";
            vm.Site = SiteId.ToString();
            vm.Division = DivisionId.ToString();

            return View("Display_SaleOrderInventoryStatus", vm);
        }

        public JsonResult DisplaySaleOrderInventoryStatusFill(Display_SaleOrderInventoryStatusViewModel vm)
        {
            SaleOrderInventoryStatusDisplayFilterSettings SettingParameter = SetCurrentParameterSettings(vm);

            if (vm.NextFormat ==  "Stock Detail")
            {
                IEnumerable<SaleOrderInventoryStatus_StockViewModel> SaleOrderInventoryStatus = _SaleOrderInventoryStatusService.StockDetail(SettingParameter);
                if (SaleOrderInventoryStatus != null)
                {
                    JsonResult json = Json(new { Success = true, Data = SaleOrderInventoryStatus.ToList() }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = int.MaxValue;
                    return json;
                }
            }
            else if (vm.NextFormat ==  "Loom Detail")
            {
                IEnumerable<SaleOrderInventoryStatus_LoomViewModel> SaleOrderInventoryStatus = _SaleOrderInventoryStatusService.LoomDetail(SettingParameter);
                if (SaleOrderInventoryStatus != null)
                {
                    JsonResult json = Json(new { Success = true, Data = SaleOrderInventoryStatus.ToList() }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = int.MaxValue;
                    return json;
                }
            }
            else
            {
                IEnumerable<SaleOrderInventoryStatusViewModel> SaleOrderInventoryStatus = _SaleOrderInventoryStatusService.SaleOrderInventoryStatusDetail(SettingParameter);
                if (SaleOrderInventoryStatus != null)
                {
                    JsonResult json = Json(new { Success = true, Data = SaleOrderInventoryStatus.ToList() }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = int.MaxValue;
                    return json;
                }
            }


            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }


        public SaleOrderInventoryStatusDisplayFilterSettings SetCurrentParameterSettings(Display_SaleOrderInventoryStatusViewModel vm)
        {
            SaleOrderInventoryStatusDisplayFilterSettings SaleOrderInventoryStatusDisplayFilterSettings = new SaleOrderInventoryStatusDisplayFilterSettings();

            SaleOrderInventoryStatusDisplayFilterParameters StatusOnDateParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            StatusOnDateParameter.ParameterName = "StatusOnDate";
            StatusOnDateParameter.Value = vm.StatusOnDate;
            StatusOnDateParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters DocTypeIdParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            DocTypeIdParameter.ParameterName = "DocTypeId";
            DocTypeIdParameter.Value = vm.DocTypeId;
            DocTypeIdParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters SiteParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            SiteParameter.ParameterName = "Site";
            SiteParameter.Value = vm.Site;
            SiteParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters DivisionParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            DivisionParameter.ParameterName = "Division";
            DivisionParameter.Value = vm.Division;
            DivisionParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters FromDateParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            FromDateParameter.ParameterName = "FromDate";
            FromDateParameter.Value = vm.FromDate;
            FromDateParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters ToDateParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            ToDateParameter.ParameterName = "ToDate";
            ToDateParameter.Value = vm.ToDate;
            ToDateParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters BuyerParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            BuyerParameter.ParameterName = "Buyer";
            BuyerParameter.Value = vm.Buyer;
            BuyerParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters SaleOrderHeaderIdParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            SaleOrderHeaderIdParameter.ParameterName = "SaleOrderHeaderId";
            SaleOrderHeaderIdParameter.Value = vm.SaleOrderHeaderId;
            SaleOrderHeaderIdParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters ProductParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            ProductParameter.ParameterName = "Product";
            ProductParameter.Value = vm.Product;
            ProductParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters ProductCategoryParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            ProductCategoryParameter.ParameterName = "ProductCategory";
            ProductCategoryParameter.Value = vm.ProductCategory;
            ProductCategoryParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters ProductQualityParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            ProductQualityParameter.ParameterName = "ProductQuality";
            ProductQualityParameter.Value = vm.ProductQuality;
            ProductQualityParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters ProductGroupParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            ProductGroupParameter.ParameterName = "ProductGroup";
            ProductGroupParameter.Value = vm.ProductGroup;
            ProductGroupParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters ProductSizeParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            ProductSizeParameter.ParameterName = "ProductSize";
            ProductSizeParameter.Value = vm.ProductSize;
            ProductSizeParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters ReportTypeParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            ReportTypeParameter.ParameterName = "ReportType";
            ReportTypeParameter.Value = vm.ReportType;
            ReportTypeParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters ReportForParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            ReportForParameter.ParameterName = "ReportFor";
            ReportForParameter.Value = vm.ReportFor;
            ReportForParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters NextFormatParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            NextFormatParameter.ParameterName = "NextFormat";
            NextFormatParameter.Value = vm.NextFormat;
            NextFormatParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters BuyerDesignParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            BuyerDesignParameter.ParameterName = "BuyerDesign";
            BuyerDesignParameter.Value = vm.BuyerDesign;
            BuyerDesignParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterParameters TextHiddenParameter = new SaleOrderInventoryStatusDisplayFilterParameters();
            TextHiddenParameter.ParameterName = "TextHidden";
            TextHiddenParameter.Value = vm.TextHidden;
            TextHiddenParameter.IsApplicable = true;

            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters = new List<SaleOrderInventoryStatusDisplayFilterParameters>();
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(StatusOnDateParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(DocTypeIdParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(SiteParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(DivisionParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(FromDateParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(ToDateParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(BuyerParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(SaleOrderHeaderIdParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(ProductParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(ProductCategoryParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(ProductQualityParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(ProductGroupParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(ProductSizeParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(ReportTypeParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(ReportForParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(NextFormatParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(BuyerDesignParameter);
            SaleOrderInventoryStatusDisplayFilterSettings.SaleOrderInventoryStatusDisplayFilterParameters.Add(TextHiddenParameter);


            System.Web.HttpContext.Current.Session["CurrentSetting"] = SaleOrderInventoryStatusDisplayFilterSettings;
            return SaleOrderInventoryStatusDisplayFilterSettings;
        }

        public void SaveCurrentSetting()
        {
            ((List<SaleOrderInventoryStatusDisplayFilterSettings>)System.Web.HttpContext.Current.Session["SettingList"]).Add((SaleOrderInventoryStatusDisplayFilterSettings)System.Web.HttpContext.Current.Session["CurrentSetting"]);
        }
        

        public JsonResult GetParameterSettingsForLastDisplay()
        {
            var SettingList = (List<SaleOrderInventoryStatusDisplayFilterSettings>)System.Web.HttpContext.Current.Session["SettingList"];

            var LastSetting = (from H in SettingList select H).LastOrDefault();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = LastSetting;

            var StatusOnDateSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "StatusOnDate" select H).FirstOrDefault();
            var DocTypeIdSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "DocTypeId" select H).FirstOrDefault();
            var SiteSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var BuyerSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "Buyer" select H).FirstOrDefault();
            var SaleOrderHeaderIdSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "SaleOrderHeaderId" select H).FirstOrDefault();
            var ProductSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "Product" select H).FirstOrDefault();
            var ProductCategorySetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "ProductCategory" select H).FirstOrDefault();
            var ProductQualitySetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "ProductQuality" select H).FirstOrDefault();
            var ProductGroupSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "ProductGroup" select H).FirstOrDefault();
            var ProductSizeSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "ProductSize" select H).FirstOrDefault();
            var ReportTypeSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "ReportType" select H).FirstOrDefault();
            var ReportForSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "ReportFor" select H).FirstOrDefault();
            var NextFormatSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "NextFormat" select H).FirstOrDefault();
            var BuyerDesignSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "BuyerDesign" select H).FirstOrDefault();
            var TextHiddenSetting = (from H in LastSetting.SaleOrderInventoryStatusDisplayFilterParameters where H.ParameterName == "TextHidden" select H).FirstOrDefault();
            SettingList.Remove(LastSetting);
            
            return Json(new
            {
                StatusOnDate = StatusOnDateSetting.Value,
                DocTypeId = DocTypeIdSetting.Value,
                Site = SiteSetting.Value,
                Division = DivisionSetting.Value,
                FromDate = FromDateSetting.Value,
                ToDate = ToDateSetting.Value,
                Buyer = BuyerSetting.Value,
                SaleOrderHeaderId = SaleOrderHeaderIdSetting.Value,
                Product = ProductSetting.Value,
                ProductCategory = ProductCategorySetting.Value,
                ProductQuality = ProductQualitySetting.Value,
                ProductGroup = ProductGroupSetting.Value,
                ProductSize = ProductSizeSetting.Value,
                ReportType = ReportTypeSetting.Value,
                ReportFor = ReportForSetting.Value,
                NextFormat = NextFormatSetting.Value,
                BuyerDesign = BuyerDesignSetting.Value,
                TextHidden = TextHiddenSetting.Value,
            });
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
                ControllerAction CA = db.ControllerAction.Find(DocumentType.ControllerActionId.Value);

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
            HandleErrorInfo Excp = new HandleErrorInfo(new Exception("Document Settings not Configured"), "TrialBalance", "DocumentMenu");
            return View("Error", Excp);


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
