using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using System.Configuration;
using Model.ViewModel;
using System.Data.SqlClient;
using System.Data;
using System.Xml.Linq;
using Model.DatabaseViews;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using Jobs.Helpers;
using System.Text;
using Model.ViewModels;
using System.Web.Script.Serialization;

namespace Jobs.Controllers
{
    [Authorize]
    public class FinancialDisplayController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        protected string connectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
        IFinancialDisplayService _FinancialDisplayService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        private const string ReportType_ProfitAndLoss = "Profit And Loss";
        private const string ReportType_BalanceSheet = "Balance Sheet";
        private const string ReportType_TrialBalance = "Trial Balance";
        private const string ReportType_SubTrialBalance = "Sub Trial Balance";
        private const string ReportType_Ledger = "Ledger";

        private const string DisplayType_Balance = "Balance";
        private const string DisplayType_Summary = "Summary";

        public FinancialDisplayController(IFinancialDisplayService FinancialDisplayService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _FinancialDisplayService = FinancialDisplayService;
            _exception = exec;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult FinancialDisplayLayout(int MenuId)
        {
            Menu Menu = new MenuService(_unitOfWork).Find(MenuId);

            FinancialDisplayViewModel vm = new FinancialDisplayViewModel();

            vm.ReportType = Menu.MenuName;
            vm.IsIncludeZeroBalance = true;
            vm.IsShowContraAccount = true;
            vm.IsIncludeOpening = true;

            System.Web.HttpContext.Current.Session["SettingList"] = new List<FinancialDisplaySettings>();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = new FinancialDisplaySettings();


            List<SelectListItem> DisplayType = new List<SelectListItem>();
            DisplayType.Add(new SelectListItem { Text = DisplayType_Balance, Value = DisplayType_Balance });
            DisplayType.Add(new SelectListItem { Text = DisplayType_Summary, Value = DisplayType_Summary });
            ViewBag.DisplayType = new SelectList(DisplayType, "Value", "Text");

            List<SelectListItem> DRCR = new List<SelectListItem>();
            DRCR.Add(new SelectListItem { Text = DrCrConstants.Both, Value = DrCrConstants.Both });
            DRCR.Add(new SelectListItem { Text = DrCrConstants.Debit, Value = DrCrConstants.Debit });
            DRCR.Add(new SelectListItem { Text = DrCrConstants.Credit, Value = DrCrConstants.Credit });

            ViewBag.DrCrList = new SelectList(DRCR, "Value", "Text");

            vm.FromDate = "01/Apr/" + DateTime.Now.Date.Year.ToString();
            vm.ToDate = DateTime.Now.Date.ToString("dd/MMM/yyyy");

            //FinancialDisplaySettings SettingParameter = GetParameterSettings(vm);

            ReportHeaderCompanyDetail ReportHeaderCompanyDetail = new ReportHeaderCompanyDetail();

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var CompanyId = (int)System.Web.HttpContext.Current.Session["CompanyId"];
            //ApplicationDbContext db = new ApplicationDbContext();

            Company Company = db.Company.Find(CompanyId);
            Division Division = db.Divisions.Find(DivisionId);


            ReportHeaderCompanyDetail.CompanyName = Company.CompanyName.Replace(System.Environment.NewLine, " ");
            ReportHeaderCompanyDetail.Address = Company.Address.Replace(System.Environment.NewLine, " ");
            if (Company.CityId != null)
                ReportHeaderCompanyDetail.CityName = db.City.Find(Company.CityId).CityName;
            else
                ReportHeaderCompanyDetail.CityName = "";
            ReportHeaderCompanyDetail.Phone = Company.Phone;
            ReportHeaderCompanyDetail.LogoBlob = Division.LogoBlob;

            vm.ReportHeaderCompanyDetail = ReportHeaderCompanyDetail;

            return View("FinancialDisplay",vm);
        }

        public JsonResult FinancialDisplayFill(FinancialDisplayViewModel vm)
        {
            FinancialDisplaySettings SettingParameter =  SetCurrentParameterSettings(vm);

            if (vm.ReportType == ReportType_TrialBalance)
            {
                if (vm.DisplayType == DisplayType_Balance)
                {
                    if (vm.IsShowDetail == true)
                    {
                        if (vm.IsFullHierarchy == true)
                        {
                            IEnumerable<TrialBalanceViewModel> TrialBalanceDetail = _FinancialDisplayService.GetTrialBalanceDetailWithFullHierarchy(SettingParameter);

                            if (TrialBalanceDetail != null)
                            {
                                JsonResult json = Json(new { Success = true, Data = TrialBalanceDetail.ToList() }, JsonRequestBehavior.AllowGet);
                                json.MaxJsonLength = int.MaxValue;
                                return json;
                            }
                        }
                        else
                        {
                            IEnumerable<TrialBalanceViewModel> TrialBalanceDetail = _FinancialDisplayService.GetTrialBalanceDetail(SettingParameter);

                            if (TrialBalanceDetail != null)
                            {
                                JsonResult json = Json(new { Success = true, Data = TrialBalanceDetail.ToList() }, JsonRequestBehavior.AllowGet);
                                json.MaxJsonLength = int.MaxValue;
                                return json;
                            }
                        }
                    }
                    else
                    {
                        IEnumerable<TrialBalanceViewModel> TrialBalance = _FinancialDisplayService.GetTrialBalance(SettingParameter);

                        if (TrialBalance != null)
                        {
                            JsonResult json = Json(new { Success = true, Data = TrialBalance.ToList() }, JsonRequestBehavior.AllowGet);
                            json.MaxJsonLength = int.MaxValue;
                            return json;
                        }
                    }
                }
                else if (vm.DisplayType == DisplayType_Summary)
                {
                    if (vm.IsShowDetail == true)
                    {
                        if (vm.IsFullHierarchy == true)
                        {
                            IEnumerable<TrialBalanceSummaryViewModel> TrialBalanceSummary = _FinancialDisplayService.GetTrialBalanceDetailSummaryWithFullHierarchy(SettingParameter);

                            if (TrialBalanceSummary != null)
                            {
                                JsonResult json = Json(new { Success = true, Data = TrialBalanceSummary.ToList() }, JsonRequestBehavior.AllowGet);
                                json.MaxJsonLength = int.MaxValue;
                                return json;
                            }
                        }
                        else
                        { 
                            IEnumerable<TrialBalanceSummaryViewModel> TrialBalanceSummary = _FinancialDisplayService.GetTrialBalanceDetailSummary(SettingParameter);

                            if (TrialBalanceSummary != null)
                            {
                                JsonResult json = Json(new { Success = true, Data = TrialBalanceSummary.ToList() }, JsonRequestBehavior.AllowGet);
                                json.MaxJsonLength = int.MaxValue;
                                return json;
                            }
                        }
                    }
                    else
                    {
                        IEnumerable<TrialBalanceSummaryViewModel> TrialBalanceSummary = _FinancialDisplayService.GetTrialBalanceSummary(SettingParameter);

                        if (TrialBalanceSummary != null)
                        {
                            JsonResult json = Json(new { Success = true, Data = TrialBalanceSummary.ToList() }, JsonRequestBehavior.AllowGet);
                            json.MaxJsonLength = int.MaxValue;
                            return json;
                        }
                    }
                }
            }
            else if (vm.ReportType == ReportType_SubTrialBalance)
            {
                if (vm.DisplayType == DisplayType_Balance)
                {
                    IEnumerable<SubTrialBalanceViewModel> SubTrialBalance = _FinancialDisplayService.GetSubTrialBalance(SettingParameter);

                    if (SubTrialBalance != null)
                    {
                        JsonResult json = Json(new { Success = true, Data = SubTrialBalance.ToList() }, JsonRequestBehavior.AllowGet);
                        json.MaxJsonLength = int.MaxValue;
                        return json;
                    }
                }
                else if (vm.DisplayType == DisplayType_Summary)
                {
                    IEnumerable<SubTrialBalanceSummaryViewModel> SubTrialBalanceSummary = _FinancialDisplayService.GetSubTrialBalanceSummary(SettingParameter);

                    if (SubTrialBalanceSummary != null)
                    {
                        JsonResult json = Json(new { Success = true, Data = SubTrialBalanceSummary.ToList() }, JsonRequestBehavior.AllowGet);
                        json.MaxJsonLength = int.MaxValue;
                        return json;
                    }
                }
            }
            else if (vm.ReportType == ReportType_Ledger)
            {
                if (vm.LedgerAccount != null)
                {
                    IEnumerable<LedgerBalanceViewModel> LedgerBalance = _FinancialDisplayService.GetLedgerBalance(SettingParameter);

                    if (LedgerBalance != null)
                    {
                        JsonResult json = Json(new { Success = true, Data = LedgerBalance.ToList() }, JsonRequestBehavior.AllowGet);
                        json.MaxJsonLength = int.MaxValue;
                        return json;
                    }
                }
            }
            

            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }


        public FinancialDisplaySettings SetCurrentParameterSettings(FinancialDisplayViewModel vm)
        {
            FinancialDisplaySettings FinancialDisplaySettings = new FinancialDisplaySettings();
            FinancialDisplaySettings.ReportType = vm.ReportType;

            FinancialDisplayParameters SiteParameter = new FinancialDisplayParameters();
            SiteParameter.ParameterName = "Site";
            SiteParameter.Value = vm.SiteIds;
            SiteParameter.IsApplicable = true;

            FinancialDisplayParameters DivisionParameter = new FinancialDisplayParameters();
            DivisionParameter.ParameterName = "Division";
            DivisionParameter.Value = vm.DivisionIds;
            DivisionParameter.IsApplicable = true;

            FinancialDisplayParameters FromDateParameter = new FinancialDisplayParameters();
            FromDateParameter.ParameterName = "FromDate";
            FromDateParameter.Value = vm.FromDate;
            FromDateParameter.IsApplicable = true;

            FinancialDisplayParameters ToDateParameter = new FinancialDisplayParameters();
            ToDateParameter.ParameterName = "ToDate";
            ToDateParameter.Value = vm.ToDate;
            ToDateParameter.IsApplicable = true;

            FinancialDisplayParameters CostCenterParameter = new FinancialDisplayParameters();
            CostCenterParameter.ParameterName = "CostCenter";
            CostCenterParameter.Value = vm.CostCenterIds;
            CostCenterParameter.IsApplicable = true;

            FinancialDisplayParameters DrCrParameter = new FinancialDisplayParameters();
            DrCrParameter.ParameterName = "DrCr";
            DrCrParameter.Value = vm.DrCr;
            DrCrParameter.IsApplicable = true;

            FinancialDisplayParameters IsIncludeZeroBalanceParameter = new FinancialDisplayParameters();
            IsIncludeZeroBalanceParameter.ParameterName = "IsIncludeZeroBalance";
            IsIncludeZeroBalanceParameter.Value = vm.IsIncludeZeroBalance.ToString();
            IsIncludeZeroBalanceParameter.IsApplicable = true;

            FinancialDisplayParameters IsShowContraAccountParameter = new FinancialDisplayParameters();
            IsShowContraAccountParameter.ParameterName = "IsShowContraAccount";
            IsShowContraAccountParameter.Value = vm.IsShowContraAccount.ToString();
            IsShowContraAccountParameter.IsApplicable = true;

            FinancialDisplayParameters IsIncludeOpeningParameter = new FinancialDisplayParameters();
            IsIncludeOpeningParameter.ParameterName = "IsIncludeOpening";
            IsIncludeOpeningParameter.Value = vm.IsIncludeOpening.ToString();
            IsIncludeOpeningParameter.IsApplicable = true;

            FinancialDisplayParameters IsShowDetailParameter = new FinancialDisplayParameters();
            IsShowDetailParameter.ParameterName = "IsShowDetail";
            IsShowDetailParameter.Value = vm.IsShowDetail.ToString();
            IsShowDetailParameter.IsApplicable = true;

            FinancialDisplayParameters IsFullHierarchyParameter = new FinancialDisplayParameters();
            IsFullHierarchyParameter.ParameterName = "IsFullHierarchy";
            IsFullHierarchyParameter.Value = vm.IsFullHierarchy.ToString();
            IsFullHierarchyParameter.IsApplicable = true;

            FinancialDisplayParameters LedgerAccountGroupParameter = new FinancialDisplayParameters();
            LedgerAccountGroupParameter.ParameterName = "LedgerAccountGroup";
            LedgerAccountGroupParameter.Value = vm.LedgerAccountGroup == null ? null : vm.LedgerAccountGroup.ToString();
            LedgerAccountGroupParameter.IsApplicable = true;

            FinancialDisplayParameters LedgerAccountParameter = new FinancialDisplayParameters();
            LedgerAccountParameter.ParameterName = "LedgerAccount";
            LedgerAccountParameter.Value = vm.LedgerAccount == null ? null : vm.LedgerAccount.ToString();
            LedgerAccountParameter.IsApplicable = true;


            FinancialDisplaySettings.FinancialDisplayParameters = new List<FinancialDisplayParameters>();
            FinancialDisplaySettings.FinancialDisplayParameters.Add(SiteParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(DivisionParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(FromDateParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(ToDateParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(CostCenterParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(DrCrParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(IsIncludeZeroBalanceParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(IsShowContraAccountParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(IsIncludeOpeningParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(IsShowDetailParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(IsFullHierarchyParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(LedgerAccountGroupParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(LedgerAccountParameter);

            System.Web.HttpContext.Current.Session["CurrentSetting"] = FinancialDisplaySettings;

            return FinancialDisplaySettings;
        }

        public void SaveCurrentSetting()
        {
            ((List<FinancialDisplaySettings>)System.Web.HttpContext.Current.Session["SettingList"]).Add((FinancialDisplaySettings)System.Web.HttpContext.Current.Session["CurrentSetting"]);
        }




        public JsonResult GetParameterSettingsForLastDisplay()
        {
            var SettingList = (List<FinancialDisplaySettings>)System.Web.HttpContext.Current.Session["SettingList"];

            var LastSetting = (from H in SettingList select H).LastOrDefault();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = LastSetting;

            if (LastSetting != null)
            {
                string ReportType = LastSetting.ReportType;
                var SiteSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "Site" select H).FirstOrDefault();
                var DivisionSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "Division" select H).FirstOrDefault();
                var FromDateSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
                var ToDateSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
                var CostCenterSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "CostCenter" select H).FirstOrDefault();
                var DrCrSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "DrCr" select H).FirstOrDefault();
                var IsIncludeZeroBalanceSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "IsIncludeZeroBalance" select H).FirstOrDefault();
                var IsShowContraAccountSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "IsShowContraAccount" select H).FirstOrDefault();
                var IsIncludeOpeningSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "IsIncludeOpening" select H).FirstOrDefault();
                var IsShowDetailSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "IsShowDetail" select H).FirstOrDefault();
                var IsFullHierarchySetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "IsFullHierarchy" select H).FirstOrDefault();

                var LedgerAccountGroupSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "LedgerAccountGroup" select H).FirstOrDefault();
                var LedgerAccountSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "LedgerAccount" select H).FirstOrDefault();
            
                SettingList.Remove(LastSetting);

                return Json(new
                {
                    ReportType = ReportType,
                    SiteId = SiteSetting.Value,
                    DivisionId = DivisionSetting.Value,
                    FromDate = FromDateSetting.Value,
                    ToDate = ToDateSetting.Value,
                    CostCenter = CostCenterSetting.Value,
                    DrCr = DrCrSetting.Value,
                    IsIncludeZeroBalance = IsIncludeZeroBalanceSetting.Value,
                    IsShowContraAccount = IsShowContraAccountSetting.Value,
                    IsIncludeOpening = IsIncludeOpeningSetting.Value,
                    IsShowDetail = IsShowDetailSetting.Value,
                    IsFullHierarchy = IsFullHierarchySetting.Value,
                    LedgerAccountGroup = LedgerAccountGroupSetting.Value,
                    LedgerAccount = LedgerAccountSetting.Value,
                });
            }
            else
            {
                return Json(new { Success = true, Data = LastSetting }, JsonRequestBehavior.AllowGet);
            }
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
