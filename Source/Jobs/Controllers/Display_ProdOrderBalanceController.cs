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
    public class Display_ProdOrderBalanceController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        protected string connectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
        IDisplay_ProdOrderBalanceService _ProdOrderBalanceService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public Display_ProdOrderBalanceController(IDisplay_ProdOrderBalanceService Display_ProdOrderBalanceService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProdOrderBalanceService = Display_ProdOrderBalanceService;
            _exception = exec;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult Display_ProdOrderBalance(int MenuId)
        {
            Menu Menu = new MenuService(_unitOfWork).Find(MenuId);

            Display_ProdOrderBalanceViewModel vm = new Display_ProdOrderBalanceViewModel();
            
            //vm.ReportType = Menu.MenuName;

            System.Web.HttpContext.Current.Session["SettingList"] = new List<ProdDisplayFilterSettings>();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = new ProdDisplayFilterSettings();
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            

            vm.FromDate = "01/Apr/" + DateTime.Now.Date.Year.ToString();
            vm.ToDate = DateTime.Now.Date.ToString("dd/MMM/yyyy");

            vm.Format = ProdReportFormat.ProcessWise;
            vm.SiteIds = SiteId.ToString();
            vm.DivisionIds = DivisionId.ToString();

            //ProdDisplayFilterSettings SettingParameter = GetParameterSettings(vm);

            return View("Display_ProdOrderBalance", vm);
        }

        public JsonResult DisplayProdOrderBalanceFill(Display_ProdOrderBalanceViewModel vm)
        {
            ProdDisplayFilterSettings SettingParameter = SetCurrentParameterSettings(vm);            
                    IEnumerable<ProdOrderBalancelOrderNoWiseViewModel> ProdOrderBalanceCustomerWise = _ProdOrderBalanceService.ProdOrderBalanceDetail(SettingParameter); 
                    if (ProdOrderBalanceCustomerWise != null)
                    {
                        JsonResult json = Json(new { Success = true, Data = ProdOrderBalanceCustomerWise.ToList() }, JsonRequestBehavior.AllowGet);
                        json.MaxJsonLength = int.MaxValue;
                        return json;
                    }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }


        public ProdDisplayFilterSettings SetCurrentParameterSettings(Display_ProdOrderBalanceViewModel vm)
        {
            ProdDisplayFilterSettings ProdDisplayFilterSettings = new ProdDisplayFilterSettings();
            ProdDisplayFilterSettings.Format = vm.Format;

            ProdDisplayFilterParameters FromDateParameter = new ProdDisplayFilterParameters();
            FromDateParameter.ParameterName = "FromDate";
            FromDateParameter.Value = vm.FromDate;
            FromDateParameter.IsApplicable = true;

            ProdDisplayFilterParameters ToDateParameter = new ProdDisplayFilterParameters();
            ToDateParameter.ParameterName = "ToDate";
            ToDateParameter.Value = vm.ToDate;
            ToDateParameter.IsApplicable = true;

            ProdDisplayFilterParameters ProductNatureParameter = new ProdDisplayFilterParameters();
            ProductNatureParameter.ParameterName = "ProductNature";
            ProductNatureParameter.Value = vm.ProductNature;
            ProductNatureParameter.IsApplicable = true;

            ProdDisplayFilterParameters ProductTypeParameter = new ProdDisplayFilterParameters();
            ProductTypeParameter.ParameterName = "ProductType";
            ProductTypeParameter.Value = vm.ProductType;
            ProductTypeParameter.IsApplicable = true;

            ProdDisplayFilterParameters CustomerParameter = new ProdDisplayFilterParameters();
            CustomerParameter.ParameterName = "Customer";
            CustomerParameter.Value = vm.Customer;
            CustomerParameter.IsApplicable = true;

            ProdDisplayFilterParameters FormatParameter = new ProdDisplayFilterParameters();
            FormatParameter.ParameterName = "Format";
            FormatParameter.Value = vm.Format;
            FormatParameter.IsApplicable = true;

            ProdDisplayFilterParameters SiteParameter = new ProdDisplayFilterParameters();
            SiteParameter.ParameterName = "Site";
            SiteParameter.Value = vm.SiteIds;
            SiteParameter.IsApplicable = true;

            ProdDisplayFilterParameters DivisionParameter = new ProdDisplayFilterParameters();
            DivisionParameter.ParameterName = "Division";
            DivisionParameter.Value = vm.DivisionIds;
            DivisionParameter.IsApplicable = true;

            ProdDisplayFilterParameters ProcessParameter = new ProdDisplayFilterParameters();
            ProcessParameter.ParameterName = "Process";
            ProcessParameter.Value = vm.Process;
            ProcessParameter.IsApplicable = true;

            ProdDisplayFilterParameters TextHiddenParameter = new ProdDisplayFilterParameters();
            TextHiddenParameter.ParameterName = "TextHidden";
            TextHiddenParameter.Value = vm.TextHidden;
            TextHiddenParameter.IsApplicable = true;

            ProdDisplayFilterSettings.ProdDisplayFilterParameters = new List<ProdDisplayFilterParameters>();
            ProdDisplayFilterSettings.ProdDisplayFilterParameters.Add(FromDateParameter);
            ProdDisplayFilterSettings.ProdDisplayFilterParameters.Add(ToDateParameter);
            ProdDisplayFilterSettings.ProdDisplayFilterParameters.Add(ProductNatureParameter);
            ProdDisplayFilterSettings.ProdDisplayFilterParameters.Add(ProductTypeParameter);
            ProdDisplayFilterSettings.ProdDisplayFilterParameters.Add(CustomerParameter);
            ProdDisplayFilterSettings.ProdDisplayFilterParameters.Add(FormatParameter);
            ProdDisplayFilterSettings.ProdDisplayFilterParameters.Add(SiteParameter);
            ProdDisplayFilterSettings.ProdDisplayFilterParameters.Add(DivisionParameter);
            ProdDisplayFilterSettings.ProdDisplayFilterParameters.Add(ProcessParameter);
            ProdDisplayFilterSettings.ProdDisplayFilterParameters.Add(TextHiddenParameter);
            System.Web.HttpContext.Current.Session["CurrentSetting"] = ProdDisplayFilterSettings;
            return ProdDisplayFilterSettings;
        }

        public void SaveCurrentSetting()
        {
            ((List<ProdDisplayFilterSettings>)System.Web.HttpContext.Current.Session["SettingList"]).Add((ProdDisplayFilterSettings)System.Web.HttpContext.Current.Session["CurrentSetting"]);
        }
        

        public JsonResult GetParameterSettingsForLastDisplay()
        {
            var SettingList = (List<ProdDisplayFilterSettings>)System.Web.HttpContext.Current.Session["SettingList"];

            var LastSetting = (from H in SettingList select H).LastOrDefault();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = LastSetting;

           // string Format = LastSetting.Format;
            var SiteSetting = (from H in LastSetting.ProdDisplayFilterParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in LastSetting.ProdDisplayFilterParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in LastSetting.ProdDisplayFilterParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in LastSetting.ProdDisplayFilterParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var CustomerSetting = (from H in LastSetting.ProdDisplayFilterParameters where H.ParameterName == "Customer" select H).FirstOrDefault();
            var ProductNatureSetting = (from H in LastSetting.ProdDisplayFilterParameters where H.ParameterName == "ProductNature" select H).FirstOrDefault();
            var ProductTypeSetting = (from H in LastSetting.ProdDisplayFilterParameters where H.ParameterName == "ProductType" select H).FirstOrDefault();
            var FormatSetting = (from H in LastSetting.ProdDisplayFilterParameters where H.ParameterName == "Format" select H).FirstOrDefault();
            var ProcessSetting = (from H in LastSetting.ProdDisplayFilterParameters where H.ParameterName == "Process" select H).FirstOrDefault();
            var TextHiddenSetting = (from H in LastSetting.ProdDisplayFilterParameters where H.ParameterName == "TextHidden" select H).FirstOrDefault();
            SettingList.Remove(LastSetting);
            
            return Json(new
            {
                //Format = Format,
                SiteId = SiteSetting.Value,
                DivisionId = DivisionSetting.Value,
                FromDate = FromDateSetting.Value,
                ToDate = ToDateSetting.Value,
                Customer= CustomerSetting.Value,
                ProductNature= ProductNatureSetting.Value,
                ProductType = ProductTypeSetting.Value,
                Format = FormatSetting.Value,
                Process= ProcessSetting.Value,
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


        public JsonResult GetFilterFormat(string searchTerm, int pageSize, int pageNum, int? filter)
        {
            var Query = _ProdOrderBalanceService.GetFilterFormat(searchTerm, filter);
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
