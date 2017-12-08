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
using Presentation.Helper;
using System.Text;
using Model.ViewModels;
using System.Web.Script.Serialization;

namespace Web
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
        private const string ReportType_TrialBalanceAsPerDetail = "Trial Balance As Per Detail";
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

            System.Web.HttpContext.Current.Session["SettingList"] = new List<FinancialDisplaySettings>();
            System.Web.HttpContext.Current.Session["CurrentSetting"] = new FinancialDisplaySettings();


            List<SelectListItem> DisplayType = new List<SelectListItem>();
            DisplayType.Add(new SelectListItem { Text = DisplayType_Balance, Value = DisplayType_Balance });
            DisplayType.Add(new SelectListItem { Text = DisplayType_Summary, Value = DisplayType_Summary });
            ViewBag.DisplayType = new SelectList(DisplayType, "Value", "Text");

            vm.FromDate = "01/Apr/" + DateTime.Now.Date.Year.ToString();
            vm.ToDate = DateTime.Now.Date.ToString("dd/MMM/yyyy");

            //FinancialDisplaySettings SettingParameter = GetParameterSettings(vm);

            return View("FinancialDisplay",vm);
        }

        public JsonResult FinancialDisplayFill(FinancialDisplayViewModel vm)
        {
            FinancialDisplaySettings SettingParameter =  SetCurrentParameterSettings(vm);


            



            if (vm.ReportType == ReportType_TrialBalance)
            {
                if (vm.DisplayType == DisplayType_Balance)
                {
                    IEnumerable<TrialBalanceViewModel> TrialBalance = _FinancialDisplayService.GetTrialBalance(SettingParameter);

                    if (TrialBalance != null)
                    {
                        JsonResult json = Json(new { Success = true, Data = TrialBalance.ToList() }, JsonRequestBehavior.AllowGet);
                        json.MaxJsonLength = int.MaxValue;
                        return json;
                    }
                }
                else if (vm.DisplayType == DisplayType_Summary)
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
            else if (vm.ReportType == ReportType_TrialBalanceAsPerDetail)
            {
                if (vm.DisplayType == DisplayType_Balance)
                {
                    List<TrialBalanceAsPerDetailViewModel> TrialBalanceAsPerDetail = _FinancialDisplayService.GetTrialBalanceAsPerDetail(SettingParameter);

                    if (TrialBalanceAsPerDetail != null)
                    {
                        JsonResult json = Json(new { Success = true, Data = TrialBalanceAsPerDetail.ToList() }, JsonRequestBehavior.AllowGet);
                        json.MaxJsonLength = int.MaxValue;
                        return json;
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


        //public FinancialDisplaySettings GetParameterSettings(FinancialDisplayViewModel vm)
        //{
        //    var SettingList = (List<FinancialDisplaySettings>)System.Web.HttpContext.Current.Session["SettingList"];

        //    var SettingForReportType = (from H in SettingList where H.ReportType == vm.ReportType select H).FirstOrDefault();
            
        //    FinancialDisplaySettings FinancialDisplaySettings = new FinancialDisplaySettings();

        //    if (SettingForReportType == null)
        //    {
        //        FinancialDisplaySettings.ReportType = vm.ReportType;

        //        FinancialDisplayParameters SiteParameter = new FinancialDisplayParameters();
        //        SiteParameter.ParameterName = "Site";
        //        SiteParameter.Value = vm.SiteIds;
        //        SiteParameter.IsApplicable = true;

        //        FinancialDisplayParameters DivisionParameter = new FinancialDisplayParameters();
        //        DivisionParameter.ParameterName = "Division";
        //        DivisionParameter.Value = vm.DivisionIds;
        //        DivisionParameter.IsApplicable = true;

        //        FinancialDisplayParameters FromDateParameter = new FinancialDisplayParameters();
        //        FromDateParameter.ParameterName = "FromDate";
        //        FromDateParameter.Value = vm.FromDate;
        //        FromDateParameter.IsApplicable = true;

        //        FinancialDisplayParameters ToDateParameter = new FinancialDisplayParameters();
        //        ToDateParameter.ParameterName = "ToDate";
        //        ToDateParameter.Value = vm.ToDate;
        //        ToDateParameter.IsApplicable = true;


        //        FinancialDisplaySettings.FinancialDisplayParameters = new List<FinancialDisplayParameters>();
        //        FinancialDisplaySettings.FinancialDisplayParameters.Add(SiteParameter);
        //        FinancialDisplaySettings.FinancialDisplayParameters.Add(DivisionParameter);
        //        FinancialDisplaySettings.FinancialDisplayParameters.Add(FromDateParameter);
        //        FinancialDisplaySettings.FinancialDisplayParameters.Add(ToDateParameter);

        //        System.Web.HttpContext.Current.Session["CurrentSetting"] = FinancialDisplaySettings;

        //        //((List<FinancialDisplaySettings>)System.Web.HttpContext.Current.Session["SettingList"]).Add(FinancialDisplaySettings);
        //    }
        //    else
        //    {
        //        //var SiteSetting = (from H in SettingForReportType.FinancialDisplayParameters where H.ParameterName == "Site" select H).FirstOrDefault();
        //        //SiteSetting.Value = vm.SiteIds;
                
        //        //var DivisionSetting = (from H in SettingForReportType.FinancialDisplayParameters where H.ParameterName == "Division" select H).FirstOrDefault();
        //        //DivisionSetting.Value = vm.DivisionIds;

        //        //var FromDateSetting = (from H in SettingForReportType.FinancialDisplayParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
        //        //FromDateSetting.Value = vm.FromDate;

        //        //var ToDateSetting = (from H in SettingForReportType.FinancialDisplayParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
        //        //ToDateSetting.Value = vm.ToDate;

        //        FinancialDisplaySettings = SettingForReportType;
        //    }


        //    return FinancialDisplaySettings;
        //}


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


            FinancialDisplayParameters LedgerAccountGroupParameter = new FinancialDisplayParameters();
            LedgerAccountGroupParameter.ParameterName = "LedgerAccountGroup";
            LedgerAccountGroupParameter.Value = vm.LedgerAccountGroup.ToString();
            LedgerAccountGroupParameter.IsApplicable = true;

            FinancialDisplayParameters LedgerAccountParameter = new FinancialDisplayParameters();
            LedgerAccountParameter.ParameterName = "LedgerAccount";
            LedgerAccountParameter.Value = vm.LedgerAccount.ToString();
            LedgerAccountParameter.IsApplicable = true;


            FinancialDisplaySettings.FinancialDisplayParameters = new List<FinancialDisplayParameters>();
            FinancialDisplaySettings.FinancialDisplayParameters.Add(SiteParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(DivisionParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(FromDateParameter);
            FinancialDisplaySettings.FinancialDisplayParameters.Add(ToDateParameter);
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

            string ReportType = LastSetting.ReportType;
            var SiteSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in LastSetting.FinancialDisplayParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
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
                LedgerAccountGroup = LedgerAccountGroupSetting.Value,
                LedgerAccount = LedgerAccountSetting.Value,
            });
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
