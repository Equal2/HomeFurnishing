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
    public class Display_PaymentAdivceWithInvoiceController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        protected string connectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
        IDisplay_PaymentAdivceWithInvoiceService _PaymentAdivceService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public Display_PaymentAdivceWithInvoiceController(IDisplay_PaymentAdivceWithInvoiceService Display_PaymentAdivceService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PaymentAdivceService = Display_PaymentAdivceService;
            _exception = exec;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult Display_PaymentAdvice(int MenuId)
        {
            Menu Menu = new MenuService(_unitOfWork).Find(MenuId);

            Display_PaymentAdviceViewModel vm = new Display_PaymentAdviceViewModel();
            
            //vm.ReportType = Menu.MenuName;

            System.Web.HttpContext.Current.Session["SettingList"] = new List<DisplayFilterPaymentAdviceSettings>();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = new DisplayFilterPaymentAdviceSettings();
            int  SiteId= (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            DateTime now = DateTime.Now;
            vm.FromDate = new DateTime(now.Year, now.Month, 1).ToString("dd/MMM/yyyy");// "01/Apr/" + DateTime.Now.Date.Year.ToString();
            vm.ToDate = DateTime.Now.Date.ToString("dd/MMM/yyyy");

            vm.Format = ReportFormat.ProcessWise;
            vm.SiteIds = SiteId.ToString();
            vm.DivisionIds = DivisionId.ToString();

            //DisplayFilterPaymentAdviceSettings SettingParameter = GetParameterSettings(vm);

            return View("Display_PaymentAdvice", vm);
        }

        public JsonResult DisplayPaymentAdviceFill(Display_PaymentAdviceViewModel vm)
        {
                   DisplayFilterPaymentAdviceSettings SettingParameter = SetCurrentParameterSettings(vm);
                    IEnumerable<PaymentAdivceViewModel> FinishingPaymentAdvice = _PaymentAdivceService.FinishingPaymentAdvice(SettingParameter); 
                    if (FinishingPaymentAdvice != null)
                    {
                        JsonResult json = Json(new { Success = true, Data = FinishingPaymentAdvice.ToList() }, JsonRequestBehavior.AllowGet);
                        json.MaxJsonLength = int.MaxValue;
                        return json;
                    }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }


        public DisplayFilterPaymentAdviceSettings SetCurrentParameterSettings(Display_PaymentAdviceViewModel vm)
        {
            DisplayFilterPaymentAdviceSettings DisplayFilterPaymentAdviceSettings = new DisplayFilterPaymentAdviceSettings();
            DisplayFilterPaymentAdviceSettings.Format = vm.Format;

            DisplayFilterPaymentAdviceParameters FromDateParameter = new DisplayFilterPaymentAdviceParameters();
            FromDateParameter.ParameterName = "FromDate";
            FromDateParameter.Value = vm.FromDate;
            FromDateParameter.IsApplicable = true;

            DisplayFilterPaymentAdviceParameters ToDateParameter = new DisplayFilterPaymentAdviceParameters();
            ToDateParameter.ParameterName = "ToDate";
            ToDateParameter.Value = vm.ToDate;
            ToDateParameter.IsApplicable = true;
            
            DisplayFilterPaymentAdviceParameters JobWorkerParameter = new DisplayFilterPaymentAdviceParameters();
            JobWorkerParameter.ParameterName = "JobWorker";
            JobWorkerParameter.Value = vm.JobWorker;
            JobWorkerParameter.IsApplicable = true;

            DisplayFilterPaymentAdviceParameters FormatParameter = new DisplayFilterPaymentAdviceParameters();
            FormatParameter.ParameterName = "Format";
            FormatParameter.Value = vm.Format;
            FormatParameter.IsApplicable = true;

            DisplayFilterPaymentAdviceParameters SiteParameter = new DisplayFilterPaymentAdviceParameters();
            SiteParameter.ParameterName = "Site";
            SiteParameter.Value = vm.SiteIds;
            SiteParameter.IsApplicable = true;

            DisplayFilterPaymentAdviceParameters DivisionParameter = new DisplayFilterPaymentAdviceParameters();
            DivisionParameter.ParameterName = "Division";
            DivisionParameter.Value = vm.DivisionIds;
            DivisionParameter.IsApplicable = true;

            DisplayFilterPaymentAdviceParameters ProcessParameter = new DisplayFilterPaymentAdviceParameters();
            ProcessParameter.ParameterName = "Process";
            ProcessParameter.Value = vm.Process;
            ProcessParameter.IsApplicable = true;

            DisplayFilterPaymentAdviceParameters TextHiddenParameter = new DisplayFilterPaymentAdviceParameters();
            TextHiddenParameter.ParameterName = "TextHidden";
            TextHiddenParameter.Value = vm.TextHidden;
            TextHiddenParameter.IsApplicable = true;

            DisplayFilterPaymentAdviceSettings.DisplayFilterPaymentAdviceParameters = new List<DisplayFilterPaymentAdviceParameters>();
            DisplayFilterPaymentAdviceSettings.DisplayFilterPaymentAdviceParameters.Add(FromDateParameter);
            DisplayFilterPaymentAdviceSettings.DisplayFilterPaymentAdviceParameters.Add(ToDateParameter);
            DisplayFilterPaymentAdviceSettings.DisplayFilterPaymentAdviceParameters.Add(JobWorkerParameter);
            DisplayFilterPaymentAdviceSettings.DisplayFilterPaymentAdviceParameters.Add(FormatParameter);
            DisplayFilterPaymentAdviceSettings.DisplayFilterPaymentAdviceParameters.Add(SiteParameter);
            DisplayFilterPaymentAdviceSettings.DisplayFilterPaymentAdviceParameters.Add(DivisionParameter);
            DisplayFilterPaymentAdviceSettings.DisplayFilterPaymentAdviceParameters.Add(ProcessParameter);
            DisplayFilterPaymentAdviceSettings.DisplayFilterPaymentAdviceParameters.Add(TextHiddenParameter);
            System.Web.HttpContext.Current.Session["CurrentSetting"] = DisplayFilterPaymentAdviceSettings;
            return DisplayFilterPaymentAdviceSettings;
        }

        public void SaveCurrentSetting()
        {
            ((List<DisplayFilterPaymentAdviceSettings>)System.Web.HttpContext.Current.Session["SettingList"]).Add((DisplayFilterPaymentAdviceSettings)System.Web.HttpContext.Current.Session["CurrentSetting"]);
        }
        

        public JsonResult GetParameterSettingsForLastDisplay()
        {
            var SettingList = (List<DisplayFilterPaymentAdviceSettings>)System.Web.HttpContext.Current.Session["SettingList"];

            var LastSetting = (from H in SettingList select H).LastOrDefault();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = LastSetting;

           // string Format = LastSetting.Format;

            var SiteSetting = (from H in LastSetting.DisplayFilterPaymentAdviceParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in LastSetting.DisplayFilterPaymentAdviceParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in LastSetting.DisplayFilterPaymentAdviceParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in LastSetting.DisplayFilterPaymentAdviceParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var JobWorkerSetting = (from H in LastSetting.DisplayFilterPaymentAdviceParameters where H.ParameterName == "JobWorker" select H).FirstOrDefault();
            var FormatSetting = (from H in LastSetting.DisplayFilterPaymentAdviceParameters where H.ParameterName == "Format" select H).FirstOrDefault();
            var ProcessSetting = (from H in LastSetting.DisplayFilterPaymentAdviceParameters where H.ParameterName == "Process" select H).FirstOrDefault();
            var TextHiddenSetting = (from H in LastSetting.DisplayFilterPaymentAdviceParameters where H.ParameterName == "TextHidden" select H).FirstOrDefault();
            SettingList.Remove(LastSetting);
            
            return Json(new
            {
                //Format = Format,
                SiteId = SiteSetting.Value,
                DivisionId = DivisionSetting.Value,
                FromDate = FromDateSetting.Value,
                ToDate = ToDateSetting.Value,
                JobWorker= JobWorkerSetting.Value,
                Format = FormatSetting.Value,
                Process= ProcessSetting.Value,
                TextHidden=TextHiddenSetting.Value,
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
            var Query = _PaymentAdivceService.GetFilterFormat(searchTerm, filter);
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
