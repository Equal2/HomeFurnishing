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
    public class Display_JobInvoiceSummaryController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        protected string connectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
        IDisplay_JobInvoiceSummaryService _JobInvoiceSummaryService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public Display_JobInvoiceSummaryController(IDisplay_JobInvoiceSummaryService Display_JobInvoiceSummaryService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobInvoiceSummaryService = Display_JobInvoiceSummaryService;
            _exception = exec;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult Display_JobInvoiceSummary(int MenuId)
        {
            Menu Menu = new MenuService(_unitOfWork).Find(MenuId);

            Display_JobInvoiceSummaryViewModel vm = new Display_JobInvoiceSummaryViewModel();
            
            //vm.ReportType = Menu.MenuName;

            System.Web.HttpContext.Current.Session["SettingList"] = new List<DisplayFilterJobInvoiceSummarySettings>();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = new DisplayFilterJobInvoiceSummarySettings();
            int  SiteId= (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            DateTime now = DateTime.Now;
            vm.FromDate = new DateTime(now.Year, now.Month, 1).ToString("dd/MMM/yyyy");// "01/Apr/" + DateTime.Now.Date.Year.ToString();
            vm.ToDate = DateTime.Now.Date.ToString("dd/MMM/yyyy");

            vm.Format = ReportFormatJobInvoiceSummary.JobWorkerWise;
            vm.ReportType = ReportTypeJobInvoiceSummary.JobInvoice;
            vm.SiteIds = SiteId.ToString();
            vm.DivisionIds = DivisionId.ToString();

            //DisplayFilterPaymentAdviceSettings SettingParameter = GetParameterSettings(vm);

            return View("Display_JobInvoiceSummary", vm);
        }

        public JsonResult DisplayJobInvoiceSummaryFill(Display_JobInvoiceSummaryViewModel vm)
        {
            DisplayFilterJobInvoiceSummarySettings SettingParameter = SetCurrentParameterSettings(vm);
                    IEnumerable<JobInvoiceSummaryViewModels> JobInvoiceSummary = _JobInvoiceSummaryService.JobInvoiceSummary(SettingParameter); 
                    if (JobInvoiceSummary != null)
                    {
                        JsonResult json = Json(new { Success = true, Data = JobInvoiceSummary.ToList() }, JsonRequestBehavior.AllowGet);
                        json.MaxJsonLength = int.MaxValue;
                        return json;
                    }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }


        public DisplayFilterJobInvoiceSummarySettings SetCurrentParameterSettings(Display_JobInvoiceSummaryViewModel vm)
        {
            DisplayFilterJobInvoiceSummarySettings DisplayFilterJobInvoiceSummarySettings = new DisplayFilterJobInvoiceSummarySettings();

            DisplayFilterJobInvoiceSummaryParameters FromDateParameter = new DisplayFilterJobInvoiceSummaryParameters();
            FromDateParameter.ParameterName = "FromDate";
            FromDateParameter.Value = vm.FromDate;
            FromDateParameter.IsApplicable = true;

            DisplayFilterJobInvoiceSummaryParameters ToDateParameter = new DisplayFilterJobInvoiceSummaryParameters();
            ToDateParameter.ParameterName = "ToDate";
            ToDateParameter.Value = vm.ToDate;
            ToDateParameter.IsApplicable = true;
            
            DisplayFilterJobInvoiceSummaryParameters JobWorkerParameter = new DisplayFilterJobInvoiceSummaryParameters();
            JobWorkerParameter.ParameterName = "JobWorker";
            JobWorkerParameter.Value = vm.JobWorker;
            JobWorkerParameter.IsApplicable = true;

            DisplayFilterJobInvoiceSummaryParameters FormatParameter = new DisplayFilterJobInvoiceSummaryParameters();
            FormatParameter.ParameterName = "Format";
            FormatParameter.Value = vm.Format;
            FormatParameter.IsApplicable = true;

            DisplayFilterJobInvoiceSummaryParameters ReportTypeParameter = new DisplayFilterJobInvoiceSummaryParameters();
            ReportTypeParameter.ParameterName = "ReportType";
            ReportTypeParameter.Value = vm.ReportType;
            ReportTypeParameter.IsApplicable = true;

            DisplayFilterJobInvoiceSummaryParameters SiteParameter = new DisplayFilterJobInvoiceSummaryParameters();
            SiteParameter.ParameterName = "Site";
            SiteParameter.Value = vm.SiteIds;
            SiteParameter.IsApplicable = true;

            DisplayFilterJobInvoiceSummaryParameters DivisionParameter = new DisplayFilterJobInvoiceSummaryParameters();
            DivisionParameter.ParameterName = "Division";
            DivisionParameter.Value = vm.DivisionIds;
            DivisionParameter.IsApplicable = true;

            DisplayFilterJobInvoiceSummaryParameters ProcessParameter = new DisplayFilterJobInvoiceSummaryParameters();
            ProcessParameter.ParameterName = "Process";
            ProcessParameter.Value = vm.Process;
            ProcessParameter.IsApplicable = true;

            DisplayFilterJobInvoiceSummaryParameters TextHiddenParameter = new DisplayFilterJobInvoiceSummaryParameters();
            TextHiddenParameter.ParameterName = "TextHidden";
            TextHiddenParameter.Value = vm.TextHidden;
            TextHiddenParameter.IsApplicable = true;

            DisplayFilterJobInvoiceSummarySettings.DisplayFilterJobInvoiceSummaryParameters = new List<DisplayFilterJobInvoiceSummaryParameters>();
            DisplayFilterJobInvoiceSummarySettings.DisplayFilterJobInvoiceSummaryParameters.Add(FromDateParameter);
            DisplayFilterJobInvoiceSummarySettings.DisplayFilterJobInvoiceSummaryParameters.Add(ToDateParameter);
            DisplayFilterJobInvoiceSummarySettings.DisplayFilterJobInvoiceSummaryParameters.Add(JobWorkerParameter);
            DisplayFilterJobInvoiceSummarySettings.DisplayFilterJobInvoiceSummaryParameters.Add(FormatParameter);
            DisplayFilterJobInvoiceSummarySettings.DisplayFilterJobInvoiceSummaryParameters.Add(ReportTypeParameter);
            DisplayFilterJobInvoiceSummarySettings.DisplayFilterJobInvoiceSummaryParameters.Add(SiteParameter);
            DisplayFilterJobInvoiceSummarySettings.DisplayFilterJobInvoiceSummaryParameters.Add(DivisionParameter);
            DisplayFilterJobInvoiceSummarySettings.DisplayFilterJobInvoiceSummaryParameters.Add(ProcessParameter);
            DisplayFilterJobInvoiceSummarySettings.DisplayFilterJobInvoiceSummaryParameters.Add(TextHiddenParameter);
            System.Web.HttpContext.Current.Session["CurrentSetting"] = DisplayFilterJobInvoiceSummarySettings;
            return DisplayFilterJobInvoiceSummarySettings;
        }

        public void SaveCurrentSetting()
        {
            ((List<DisplayFilterJobInvoiceSummarySettings>)System.Web.HttpContext.Current.Session["SettingList"]).Add((DisplayFilterJobInvoiceSummarySettings)System.Web.HttpContext.Current.Session["CurrentSetting"]);
        }
        

        public JsonResult GetParameterSettingsForLastDisplay()
        {
            var SettingList = (List<DisplayFilterJobInvoiceSummarySettings>)System.Web.HttpContext.Current.Session["SettingList"];

            var LastSetting = (from H in SettingList select H).LastOrDefault();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = LastSetting;

           // string Format = LastSetting.Format;

            var SiteSetting = (from H in LastSetting.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in LastSetting.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in LastSetting.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in LastSetting.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var JobWorkerSetting = (from H in LastSetting.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "JobWorker" select H).FirstOrDefault();
            var FormatSetting = (from H in LastSetting.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "Format" select H).FirstOrDefault();
            var ReportTypeSetting = (from H in LastSetting.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "ReportType" select H).FirstOrDefault();
            var ProcessSetting = (from H in LastSetting.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "Process" select H).FirstOrDefault();
            var TextHiddenSetting = (from H in LastSetting.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "TextHidden" select H).FirstOrDefault();
            SettingList.Remove(LastSetting);

            return Json(new
            {
                //Format = Format,
                SiteId = SiteSetting.Value,
                DivisionId = DivisionSetting.Value,
                FromDate = FromDateSetting.Value,
                ToDate = ToDateSetting.Value,
                JobWorker = JobWorkerSetting.Value,
                Format = FormatSetting.Value,
                ReportType = ReportTypeSetting.Value,
                Process = ProcessSetting.Value,
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
            var Query = _JobInvoiceSummaryService.GetFilterFormat(searchTerm, filter);
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
        public JsonResult GetFilterReportType(string searchTerm, int pageSize, int pageNum, int? filter)
        {
            var Query = _JobInvoiceSummaryService.GetFilterRepotType(searchTerm, filter);
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
