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
    public class Display_JobOrderBalanceController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        protected string connectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
        IDisplay_JobOrderBalanceService _JobOrderBalanceService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public Display_JobOrderBalanceController(IDisplay_JobOrderBalanceService Display_JobOrderBalanceService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderBalanceService = Display_JobOrderBalanceService;
            _exception = exec;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult Display_JobOrderBalance(int MenuId)
        {
            Menu Menu = new MenuService(_unitOfWork).Find(MenuId);

            Display_JobOrderBalanceViewModel vm = new Display_JobOrderBalanceViewModel();
            
            //vm.ReportType = Menu.MenuName;

            System.Web.HttpContext.Current.Session["SettingList"] = new List<DisplayFilterSettings>();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = new DisplayFilterSettings();
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            vm.FromDate = "01/Apr/" + DateTime.Now.Date.Year.ToString();
            vm.ToDate = DateTime.Now.Date.ToString("dd/MMM/yyyy");

            vm.Format = ReportFormat.ProcessWise;
            vm.SiteIds = SiteId.ToString();
            vm.DivisionIds = DivisionId.ToString();

            vm.ReportHeaderCompanyDetail = new GridReportService().GetReportHeaderCompanyDetail();

            //DisplayFilterSettings SettingParameter = GetParameterSettings(vm);

            return View("Display_JobOrderBalance", vm);
        }

        public JsonResult DisplayJobOrderBalanceFill(Display_JobOrderBalanceViewModel vm)
        {
            DisplayFilterSettings SettingParameter = SetCurrentParameterSettings(vm);
            IEnumerable<JobOrderBalancelOrderNoWiseViewModel> JobOrderBalanceJobWorkerWise = _JobOrderBalanceService.JobOrderBalanceDetail(SettingParameter); 



            if (JobOrderBalanceJobWorkerWise != null)
            {
                JsonResult json = Json(new { Success = true, Data = JobOrderBalanceJobWorkerWise.ToList() }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }


        public DisplayFilterSettings SetCurrentParameterSettings(Display_JobOrderBalanceViewModel vm)
        {
            DisplayFilterSettings DisplayFilterSettings = new DisplayFilterSettings();
            DisplayFilterSettings.Format = vm.Format;

            DisplayFilterParameters FromDateParameter = new DisplayFilterParameters();
            FromDateParameter.ParameterName = "FromDate";
            FromDateParameter.Value = vm.FromDate;
            FromDateParameter.IsApplicable = true;

            DisplayFilterParameters ToDateParameter = new DisplayFilterParameters();
            ToDateParameter.ParameterName = "ToDate";
            ToDateParameter.Value = vm.ToDate;
            ToDateParameter.IsApplicable = true;

            DisplayFilterParameters ProductNatureParameter = new DisplayFilterParameters();
            ProductNatureParameter.ParameterName = "ProductNature";
            ProductNatureParameter.Value = vm.ProductNature;
            ProductNatureParameter.IsApplicable = true;

            DisplayFilterParameters ProductTypeParameter = new DisplayFilterParameters();
            ProductTypeParameter.ParameterName = "ProductType";
            ProductTypeParameter.Value = vm.ProductType;
            ProductTypeParameter.IsApplicable = true;

            DisplayFilterParameters JobWorkerParameter = new DisplayFilterParameters();
            JobWorkerParameter.ParameterName = "JobWorker";
            JobWorkerParameter.Value = vm.JobWorker;
            JobWorkerParameter.IsApplicable = true;

            DisplayFilterParameters FormatParameter = new DisplayFilterParameters();
            FormatParameter.ParameterName = "Format";
            FormatParameter.Value = vm.Format;
            FormatParameter.IsApplicable = true;

            DisplayFilterParameters SiteParameter = new DisplayFilterParameters();
            SiteParameter.ParameterName = "Site";
            SiteParameter.Value = vm.SiteIds;
            SiteParameter.IsApplicable = true;

            DisplayFilterParameters DivisionParameter = new DisplayFilterParameters();
            DivisionParameter.ParameterName = "Division";
            DivisionParameter.Value = vm.DivisionIds;
            DivisionParameter.IsApplicable = true;

            DisplayFilterParameters ProcessParameter = new DisplayFilterParameters();
            ProcessParameter.ParameterName = "Process";
            ProcessParameter.Value = vm.Process;
            ProcessParameter.IsApplicable = true;

            DisplayFilterParameters TextHiddenParameter = new DisplayFilterParameters();
            TextHiddenParameter.ParameterName = "TextHidden";
            TextHiddenParameter.Value = vm.TextHidden;
            TextHiddenParameter.IsApplicable = true;

            DisplayFilterSettings.DisplayFilterParameters = new List<DisplayFilterParameters>();
            DisplayFilterSettings.DisplayFilterParameters.Add(FromDateParameter);
            DisplayFilterSettings.DisplayFilterParameters.Add(ToDateParameter);
            DisplayFilterSettings.DisplayFilterParameters.Add(ProductNatureParameter);
            DisplayFilterSettings.DisplayFilterParameters.Add(ProductTypeParameter);
            DisplayFilterSettings.DisplayFilterParameters.Add(JobWorkerParameter);
            DisplayFilterSettings.DisplayFilterParameters.Add(FormatParameter);
            DisplayFilterSettings.DisplayFilterParameters.Add(SiteParameter);
            DisplayFilterSettings.DisplayFilterParameters.Add(DivisionParameter);
            DisplayFilterSettings.DisplayFilterParameters.Add(ProcessParameter);
            DisplayFilterSettings.DisplayFilterParameters.Add(TextHiddenParameter);
            System.Web.HttpContext.Current.Session["CurrentSetting"] = DisplayFilterSettings;
            return DisplayFilterSettings;
        }

        public void SaveCurrentSetting()
        {
            ((List<DisplayFilterSettings>)System.Web.HttpContext.Current.Session["SettingList"]).Add((DisplayFilterSettings)System.Web.HttpContext.Current.Session["CurrentSetting"]);
        }
        

        public JsonResult GetParameterSettingsForLastDisplay()
        {
            var SettingList = (List<DisplayFilterSettings>)System.Web.HttpContext.Current.Session["SettingList"];

            var LastSetting = (from H in SettingList select H).LastOrDefault();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = LastSetting;

           // string Format = LastSetting.Format;
            var SiteSetting = (from H in LastSetting.DisplayFilterParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in LastSetting.DisplayFilterParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in LastSetting.DisplayFilterParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in LastSetting.DisplayFilterParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var JobWorkerSetting = (from H in LastSetting.DisplayFilterParameters where H.ParameterName == "JobWorker" select H).FirstOrDefault();
            var ProductNatureSetting = (from H in LastSetting.DisplayFilterParameters where H.ParameterName == "ProductNature" select H).FirstOrDefault();
            var ProductTypeSetting = (from H in LastSetting.DisplayFilterParameters where H.ParameterName == "ProductType" select H).FirstOrDefault();
            var FormatSetting = (from H in LastSetting.DisplayFilterParameters where H.ParameterName == "Format" select H).FirstOrDefault();
            var ProcessSetting = (from H in LastSetting.DisplayFilterParameters where H.ParameterName == "Process" select H).FirstOrDefault();
            var TextHiddenSetting = (from H in LastSetting.DisplayFilterParameters where H.ParameterName == "TextHidden" select H).FirstOrDefault();
            SettingList.Remove(LastSetting);
            
            return Json(new
            {
                //Format = Format,
                SiteId = SiteSetting.Value,
                DivisionId = DivisionSetting.Value,
                FromDate = FromDateSetting.Value,
                ToDate = ToDateSetting.Value,
                JobWorker= JobWorkerSetting.Value,
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
            var Query = _JobOrderBalanceService.GetFilterFormat(searchTerm, filter);
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

        public JsonResult GetDivisionSettings()
        {
            GridReportService _GridReportService = new GridReportService();
            _GridReportService.GetDivisionSettings();

            return Json(new
            {
                Dimension1Caption = _GridReportService.Dimension1Caption,
                Dimension2Caption = _GridReportService.Dimension2Caption,
                Dimension3Caption = _GridReportService.Dimension3Caption,
                Dimension4Caption = _GridReportService.Dimension4Caption,
            });
        }
    }
}
