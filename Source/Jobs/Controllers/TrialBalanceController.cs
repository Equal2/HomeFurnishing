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
using Presentation;
using Presentation.ViewModels;
using Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Reports.Reports;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    // [Authorize]
    public class TrialBalanceController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ITrialBalanceService _TrialBalService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public TrialBalanceController(ITrialBalanceService TrialBalanceService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _TrialBalService = TrialBalanceService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }

        public ActionResult GeneratePrints(string ActionName,  int LedgerAccountGroupId  =0)
        {

            string PrintProcedure = "";


            if (!string.IsNullOrEmpty(ActionName))
            {

                var settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(User.Identity.Name);

                string SiteId = settings.SiteIds;
                string DivisionId = settings.DivisionIds;
                string FromDate = settings.FromDate.HasValue ? settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
                string ToDate = settings.ToDate.HasValue ? settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";

                if (ActionName == "BalanceSheet")
                    PrintProcedure = "Web.spBalanceSheet '" + SiteId.ToString() + "','" + DivisionId.ToString() + "','" + FromDate.ToString() + "','" + ToDate.ToString() + "'";
                else if (ActionName == "ProfitAndLoss")
                    PrintProcedure = "Web.spProfitAndLoss '" + SiteId.ToString() + "','" + DivisionId.ToString() + "','" + FromDate.ToString() + "','" + ToDate.ToString() + "'";
                else if (ActionName == "TrialBalanceSummary")
                    PrintProcedure = "Web.spTrialBalanceSummary '" + SiteId.ToString() + "','" + DivisionId.ToString() + "','" + FromDate.ToString() + "','" + ToDate.ToString() + "'";
                else if (ActionName == "TrialBalance")
                    PrintProcedure = "Web.spTrialBalance '" + SiteId.ToString() + "','" + DivisionId.ToString() + "','" + ToDate.ToString() + "'";
                else if (ActionName == "SubTrialBalanceSummary")
                    if (LedgerAccountGroupId ==0)
                    PrintProcedure = "Web.spSubTrialBalanceSummary '" + SiteId.ToString() + "','" + DivisionId.ToString() + "','" + FromDate.ToString() + "','" + ToDate.ToString() + "'";
                else
                    PrintProcedure = "Web.spSubTrialBalanceSummary '" + SiteId.ToString() + "','" + DivisionId.ToString() + "','" + FromDate.ToString() + "','" + ToDate.ToString() + "','" + LedgerAccountGroupId.ToString() + "'";
                else if (ActionName == "SubTrialBalance")
                    if (LedgerAccountGroupId == 0)
                        PrintProcedure = "Web.spSubTrialBalance '" + SiteId.ToString() + "','" + DivisionId.ToString() + "','" + ToDate.ToString() + "'";
                    else
                        PrintProcedure = "Web.spSubTrialBalance '" + SiteId.ToString() + "','" + DivisionId.ToString() + "','" + ToDate.ToString() + "','" + LedgerAccountGroupId.ToString() + "'";
                else if (ActionName == "LedgerBalance")
                    if (LedgerAccountGroupId == 0)
                        PrintProcedure = "Web.spLedger '" + SiteId.ToString() + "','" + DivisionId.ToString() + "','" + FromDate.ToString() + "','" + ToDate.ToString() + "'";
                    else
                        PrintProcedure = "Web.spLedger '" + SiteId.ToString() + "','" + DivisionId.ToString() + "','" + FromDate.ToString() + "','" + ToDate.ToString() + "','" + LedgerAccountGroupId.ToString() + "'";

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    DirectReportPrint drp = new DirectReportPrint();
                    byte[] Pdf;
                    Pdf = drp.DirectDocumentPrint(PrintProcedure, User.Identity.Name);
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

        public ActionResult GetTrialBalance()
        {
            var settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(User.Identity.Name);

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (settings == null)
            {
                return RedirectToAction("Create", "TrialBalanceSetting").Warning("Please create Trial Balance settings");
            }



            ViewBag.DrCr = settings.DrCr;

            if (settings.DisplayType == DisplayTypeConstants.Balance)
            {
                string FromDate = settings.ToDate.HasValue ? settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
                string ToDate = settings.ToDate.HasValue ? settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
                ViewBag.FilterRemark = "( As On Date : " + ToDate + " )";
                return View("TrialBalanceIndex");
            }
            else
            {
                string FromDate = settings.ToDate.HasValue ? settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
                string ToDate = settings.ToDate.HasValue ? settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
                ViewBag.FilterRemark = "( From Date : " + FromDate + " To Date : " + ToDate + " )";
                return View("TrialBalanceSummaryIndex");
            }
        }

        public JsonResult GetTrlBal()
        {
            return Json(new { data = _TrialBalService.GetTrialBalance(User.Identity.Name) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTrlBalSummary()
        {
            return Json(new { data = _TrialBalService.GetTrialBalanceSummary(User.Identity.Name) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProfitAndLoss()
        {
            var settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(User.Identity.Name);

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (settings == null)
            {
                return RedirectToAction("Create", "TrialBalanceSetting").Warning("Please create Trial Balance settings");
            }

            ViewBag.DrCr = settings.DrCr;
            string FromDate = settings.ToDate.HasValue ? settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
            string ToDate = settings.ToDate.HasValue ? settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
            ViewBag.FilterRemark = "( From Date : " + FromDate + " To Date : " + ToDate + " )";
            return View("ProfitAndLoss");
        }

        public JsonResult _ProfitAndLoss()
        {
            return Json(new { data = _TrialBalService.GetProfitAndLossSummary(User.Identity.Name) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBalanceSheet()
        {
            var settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(User.Identity.Name);

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (settings == null)
            {
                return RedirectToAction("Create", "TrialBalanceSetting").Warning("Please create Trial Balance settings");
            }

            string ToDate = settings.ToDate.HasValue ? settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";

            ViewBag.FilterRemark = "( As On Date : " + ToDate +" )";
            return View("BalanceSheet");
        }

        public JsonResult _BalanceSheet()
        {
            return Json(new { data = _TrialBalService.GetBalanceSheetSummary(User.Identity.Name) }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetSubTrialBalance(int? id)//LedgerAccountGroupId
        {
            ViewBag.id = id;
            if (id.HasValue && id.Value > 0)
                ViewBag.Name = new LedgerAccountGroupService(_unitOfWork).Find(id.Value).LedgerAccountGroupName;
            else
                ViewBag.Name = "Trial Balance";
            var settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(User.Identity.Name);

            ViewBag.DisplayType = settings.DisplayType;
            ViewBag.DrCr = settings.DrCr;

            if (settings.DisplayType == DisplayTypeConstants.Balance)
            {
                string FromDate = settings.ToDate.HasValue ? settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
                string ToDate = settings.ToDate.HasValue ? settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
                ViewBag.FilterRemark = "( As On Date : " + ToDate + " )";
                return View("SubTrialBalanceIndex");
            }
            else
            {
                string FromDate = settings.ToDate.HasValue ? settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
                string ToDate = settings.ToDate.HasValue ? settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
                ViewBag.FilterRemark = "( From Date : " + FromDate + " To Date : " + ToDate + " )";
                return View("SubTrialBalanceSummaryIndex");
            }

        }
        public JsonResult GetSubTrlBal(int? id)
        {
            return Json(new { data = _TrialBalService.GetSubTrialBalance(id, User.Identity.Name) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSubTrlBalSummary(int? id)
        {
            return Json(new { data = _TrialBalService.GetSubTrialBalanceSummary(id, User.Identity.Name) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLedgerBalance(int id)//LedgerAccountId
        {
            ViewBag.id = id;
            ViewBag.Name = new LedgerAccountService(_unitOfWork).Find(id).LedgerAccountName;

            var settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(User.Identity.Name);
            ViewBag.DrCr = settings.DrCr;

            return View("LedgerBalanceIndex");
        }

        public ActionResult GetLedgerGroupBalance(int id)//LedgerAccountId
        {
            var settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(User.Identity.Name);
            if (id != 0)
            {
                ViewBag.id = id;
                ViewBag.Name = new LedgerAccountGroupService(_unitOfWork).Find(id).LedgerAccountGroupName;
                ViewBag.DrCr = settings.DrCr;
            }

            string ToDate = settings.ToDate.HasValue ? settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
            ViewBag.FilterRemark = "( As On Date : " + ToDate + " )";
            return View("LedgerGroupBalance");
            
        }

        public JsonResult _GetLedgerGroupBalance(int id)
        {
            return Json(new { data = _TrialBalService.GetLedgerGroupBalance(id, User.Identity.Name) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLedgerBal(int id)
        {
            return Json(new { data = _TrialBalService.GetLedgerBalance(id, User.Identity.Name) }, JsonRequestBehavior.AllowGet);
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
