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
    public class Display_SaleOrderBalanceController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        protected string connectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
        IDisplay_SaleOrderBalanceService _SaleOrderBalanceService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public Display_SaleOrderBalanceController(IDisplay_SaleOrderBalanceService Display_SaleOrderBalanceService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleOrderBalanceService = Display_SaleOrderBalanceService;
            _exception = exec;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult Display_SaleOrderBalance(int MenuId)
        {
            Menu Menu = new MenuService(_unitOfWork).Find(MenuId);

            Display_SaleOrderBalanceViewModel vm = new Display_SaleOrderBalanceViewModel();
            
            //vm.ReportType = Menu.MenuName;

            System.Web.HttpContext.Current.Session["SettingList"] = new List<SaleDisplayFilterSettings>();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = new SaleDisplayFilterSettings();
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            vm.FromDate = "01/Apr/" + DateTime.Now.Date.Year.ToString();
            vm.ToDate = DateTime.Now.Date.ToString("dd/MMM/yyyy");

            vm.Format = SaleReportFormat.BuyerWise;
            vm.SiteIds = SiteId.ToString();
            vm.DivisionIds = DivisionId.ToString();

            //DisplayFilterSettings SettingParameter = GetParameterSettings(vm);

            return View("Display_SaleOrderBalance", vm);
        }

        public JsonResult DisplaySaleOrderBalanceFill(Display_SaleOrderBalanceViewModel vm)
        {
            SaleDisplayFilterSettings SettingParameter = SetCurrentParameterSettings(vm);
            IEnumerable<SaleOrderBalancelOrderNoWiseViewModel> SaleOrderBalanceBuyerWise = _SaleOrderBalanceService.SaleOrderBalanceDetail(SettingParameter); 
            if (SaleOrderBalanceBuyerWise != null)
            {
                JsonResult json = Json(new { Success = true, Data = SaleOrderBalanceBuyerWise.ToList() }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }


        public SaleDisplayFilterSettings SetCurrentParameterSettings(Display_SaleOrderBalanceViewModel vm)
        {
            SaleDisplayFilterSettings SaleDisplayFilterSettings = new SaleDisplayFilterSettings();
            SaleDisplayFilterSettings.Format = vm.Format;

            SaleDisplayFilterParameters FromDateParameter = new SaleDisplayFilterParameters();
            FromDateParameter.ParameterName = "FromDate";
            FromDateParameter.Value = vm.FromDate;
            FromDateParameter.IsApplicable = true;

            SaleDisplayFilterParameters ToDateParameter = new SaleDisplayFilterParameters();
            ToDateParameter.ParameterName = "ToDate";
            ToDateParameter.Value = vm.ToDate;
            ToDateParameter.IsApplicable = true;

            SaleDisplayFilterParameters ProductNatureParameter = new SaleDisplayFilterParameters();
            ProductNatureParameter.ParameterName = "ProductNature";
            ProductNatureParameter.Value = vm.ProductNature;
            ProductNatureParameter.IsApplicable = true;

            SaleDisplayFilterParameters ProductTypeParameter = new SaleDisplayFilterParameters();
            ProductTypeParameter.ParameterName = "ProductType";
            ProductTypeParameter.Value = vm.ProductType;
            ProductTypeParameter.IsApplicable = true;

            SaleDisplayFilterParameters ProductCategoryParameter = new SaleDisplayFilterParameters();
            ProductCategoryParameter.ParameterName = "ProductCategory";
            ProductCategoryParameter.Value = vm.ProductCategory;
            ProductCategoryParameter.IsApplicable = true;

            SaleDisplayFilterParameters Buyer = new SaleDisplayFilterParameters();
            Buyer.ParameterName = "Buyer";
            Buyer.Value = vm.Buyer;
            Buyer.IsApplicable = true;

            SaleDisplayFilterParameters FormatParameter = new SaleDisplayFilterParameters();
            FormatParameter.ParameterName = "Format";
            FormatParameter.Value = vm.Format;
            FormatParameter.IsApplicable = true;

            SaleDisplayFilterParameters SiteParameter = new SaleDisplayFilterParameters();
            SiteParameter.ParameterName = "Site";
            SiteParameter.Value = vm.SiteIds;
            SiteParameter.IsApplicable = true;

            SaleDisplayFilterParameters DivisionParameter = new SaleDisplayFilterParameters();
            DivisionParameter.ParameterName = "Division";
            DivisionParameter.Value = vm.DivisionIds;
            DivisionParameter.IsApplicable = true;

      

            SaleDisplayFilterParameters TextHiddenParameter = new SaleDisplayFilterParameters();
            TextHiddenParameter.ParameterName = "TextHidden";
            TextHiddenParameter.Value = vm.TextHidden;
            TextHiddenParameter.IsApplicable = true;

            SaleDisplayFilterSettings.SaleDisplayFilterParameters = new List<SaleDisplayFilterParameters>();
            SaleDisplayFilterSettings.SaleDisplayFilterParameters.Add(FromDateParameter);
            SaleDisplayFilterSettings.SaleDisplayFilterParameters.Add(ToDateParameter);
            SaleDisplayFilterSettings.SaleDisplayFilterParameters.Add(ProductNatureParameter);
            SaleDisplayFilterSettings.SaleDisplayFilterParameters.Add(ProductTypeParameter);
            SaleDisplayFilterSettings.SaleDisplayFilterParameters.Add(ProductCategoryParameter);
            SaleDisplayFilterSettings.SaleDisplayFilterParameters.Add(Buyer);
            SaleDisplayFilterSettings.SaleDisplayFilterParameters.Add(FormatParameter);
            SaleDisplayFilterSettings.SaleDisplayFilterParameters.Add(SiteParameter);
            SaleDisplayFilterSettings.SaleDisplayFilterParameters.Add(DivisionParameter);            
            SaleDisplayFilterSettings.SaleDisplayFilterParameters.Add(TextHiddenParameter);
            System.Web.HttpContext.Current.Session["CurrentSetting"] = SaleDisplayFilterSettings;
            return SaleDisplayFilterSettings;
        }

        public void SaveCurrentSetting()
        {
            ((List<SaleDisplayFilterSettings>)System.Web.HttpContext.Current.Session["SettingList"]).Add((SaleDisplayFilterSettings)System.Web.HttpContext.Current.Session["CurrentSetting"]);
        }
        

        public JsonResult GetParameterSettingsForLastDisplay()
        {
            var SettingList = (List<SaleDisplayFilterSettings>)System.Web.HttpContext.Current.Session["SettingList"];

            var LastSetting = (from H in SettingList select H).LastOrDefault();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = LastSetting;

           // string Format = LastSetting.Format;
            var SiteSetting = (from H in LastSetting.SaleDisplayFilterParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in LastSetting.SaleDisplayFilterParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in LastSetting.SaleDisplayFilterParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in LastSetting.SaleDisplayFilterParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var BuyerSetting = (from H in LastSetting.SaleDisplayFilterParameters where H.ParameterName == "Buyer" select H).FirstOrDefault();
            var ProductNatureSetting = (from H in LastSetting.SaleDisplayFilterParameters where H.ParameterName == "ProductNature" select H).FirstOrDefault();
            var ProductTypeSetting = (from H in LastSetting.SaleDisplayFilterParameters where H.ParameterName == "ProductType" select H).FirstOrDefault();
            var ProductCategorySetting = (from H in LastSetting.SaleDisplayFilterParameters where H.ParameterName == "ProductCategory" select H).FirstOrDefault();
            var FormatSetting = (from H in LastSetting.SaleDisplayFilterParameters where H.ParameterName == "Format" select H).FirstOrDefault();
            var TextHiddenSetting = (from H in LastSetting.SaleDisplayFilterParameters where H.ParameterName == "TextHidden" select H).FirstOrDefault();
            SettingList.Remove(LastSetting);
            
            return Json(new
            {
                //Format = Format,
                SiteId = SiteSetting.Value,
                DivisionId = DivisionSetting.Value,
                FromDate = FromDateSetting.Value,
                ToDate = ToDateSetting.Value,
                Buyer = BuyerSetting.Value,
                ProductNature= ProductNatureSetting.Value,
                ProductType = ProductTypeSetting.Value,
                Format = FormatSetting.Value,
                TextHidden = TextHiddenSetting.Value,
                ProductCategory= ProductCategorySetting.Value,
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


        public JsonResult GetFilterFormat(string searchTerm, int pageSize, int pageNum, int? filter)
        {
            var Query = _SaleOrderBalanceService.GetFilterFormat(searchTerm, filter);
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
        public JsonResult SetFilterFormat(String Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();
            ProductJson.id = Ids;
            ProductJson.text = Ids;
            return Json(ProductJson);
        }

    }



}
