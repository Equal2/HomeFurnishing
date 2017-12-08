using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Jobs.Helpers;
using Data.Infrastructure;
using System.Web.UI.WebControls;
using AutoMapper;
using Microsoft.AspNet.Identity;
using System.Configuration;
using Presentation;
using Model.ViewModel;
using Reports.Controllers;



namespace Jobs.Controllers
{
    [Authorize]
    public class SaleDeliveryWizardController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleDeliveryHeaderService _SaleDeliveryHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public SaleDeliveryWizardController(ISaleDeliveryHeaderService PurchaseOrderHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleDeliveryHeaderService = PurchaseOrderHeaderService;
            _exception = exec;
            _unitOfWork = unitOfWork;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult CreateSaleDelivery(int id)//DocTypeId
        {

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            ViewBag.id = id;

            int DocTypeId = id;

            //Getting Settings
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(DocTypeId, DivisionId, SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "SaleDeliverySettings", new { id = DocTypeId }).Warning("Please create settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            return View("CreateSaleDelivery");
        }

        public JsonResult PendingSaleInvoice(int id)//DocTypeId
        {

            var temp = _SaleDeliveryHeaderService.GetSaleInvoiceForSaleDeliveryWizard(id).ToList();

            return Json(new { data = temp }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ConfirmSaleInvoiceList(List<SaleDeliveryWizardViewModel> Selected, int id)
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(id, DivisionId, SiteId);

            System.Web.HttpContext.Current.Session["PendingInvoiceForSaleDelivery"] = Selected;

            return Json(new { Success = "URL", Data = "/SaleDeliveryWizard/Create/" + id.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [Serializable]
        public class SelectedSaleInvoice
        {
            public int id { get; set; }
            public decimal Qty { get; set; }
            public int? RefDocTypeId { get; set; }
            public int? RefDocLineId { get; set; }
        }

        private void PrepareViewBag()
        {
            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }


        // GET: /SaleDeliveryHeader/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            SaleDeliveryHeaderViewModel p = new SaleDeliveryHeaderViewModel();

            p.DocDate = DateTime.Now;
            p.CreatedDate = DateTime.Now;

            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            int DocTypeId = id;

            //Getting Settings
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(DocTypeId, p.DivisionId, p.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "SaleDeliverySettings", new { id = DocTypeId }).Warning("Please create job order settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            p.SaleDeliverySettings = Mapper.Map<SaleDeliverySetting, SaleDeliverySettingsViewModel>(settings);

            
            p.ProcessId = settings.ProcessId;
            PrepareViewBag();
            p.DocTypeId = DocTypeId;
            p.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".SaleDeliveryHeaders", p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);

            //RatesFetching from RateList Master
            var Lines = (List<SaleDeliveryWizardViewModel>)System.Web.HttpContext.Current.Session["PendingInvoiceForSaleDelivery"];
            if (Lines != null)
            {
                if (Lines.FirstOrDefault().SaleToBuyerId != null)
                {
                    p.SaleToBuyerId = (int)Lines.FirstOrDefault().SaleToBuyerId;
                    var PersonAddress = new PersonAddressService(_unitOfWork).GetShipAddressByPersonId(p.SaleToBuyerId);
                    if (PersonAddress != null)
                    {
                        p.ShipToPartyAddress = PersonAddress.Address;
                    }
                }
            }


            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(SaleDeliveryHeaderViewModel svm)
        {
            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            bool Continue = true;

            SaleDeliveryHeader s = Mapper.Map<SaleDeliveryHeaderViewModel, SaleDeliveryHeader>(svm);
            List<SaleDeliveryWizardViewModel> SaleInvoiceAndQtys = (List<SaleDeliveryWizardViewModel>)System.Web.HttpContext.Current.Session["PendingInvoiceForSaleDelivery"];
            if (svm.SaleDeliverySettings != null)
            {
                //if (svm.SaleDeliverySettings.isMandatoryCostCenter == true && (string.IsNullOrEmpty(svm.CostCenterName)))
                //{
                //    ModelState.AddModelError("CostCenterName", "The CostCenter field is required");
                //}
            }



            if (SaleInvoiceAndQtys.Count() <= 0)
                ModelState.AddModelError("", "No Records Selected");

            int SaleToBuyerCnt = (from l in SaleInvoiceAndQtys
                                group l by l.SaleToBuyerId into g
                                select new
                                {
                                    SaleToBuyerId = g.Key,
                                }).Distinct().Count();

            if (SaleToBuyerCnt > 1)
                ModelState.AddModelError("", "Select any one Buyer Orders.");

            List<SaleDeliveryLine> BarCodesToUpdate = new List<SaleDeliveryLine>();

            bool CostCenterGenerated = false;

            #region DocTypeTimeLineValidation

            try
            {

                if (svm.SaleDeliveryHeaderId <= 0)
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(svm), DocumentTimePlanTypeConstants.Create, User.Identity.Name, out ExceptionMsg, out Continue);
                else
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(svm), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXC"] += ExceptionMsg;

            #endregion

            if (ModelState.IsValid && (TimePlanValidation || Continue))
            {

                if (svm.SaleDeliveryHeaderId <= 0)
                {


                    if (SaleInvoiceAndQtys.Count() > 0)
                    {
                        s.CreatedDate = DateTime.Now;
                        s.ModifiedDate = DateTime.Now;
                        s.CreatedBy = User.Identity.Name;
                        s.ModifiedBy = User.Identity.Name;
                        s.Status = (int)StatusConstants.Drafted;
                        _SaleDeliveryHeaderService.Create(s);


                        int Cnt = 0;
                        int Sr = 0;

                        int pk = 0;

                        SaleDeliverySetting Settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(s.DocTypeId, s.DivisionId, s.SiteId);


                        var SaleInvoiceLineIds = SaleInvoiceAndQtys.Select(m => m.SaleInvoiceLineId).ToArray();

                        var BalQtyandUnits = (from p in db.ViewSaleInvoiceBalanceForDelivery
                                              join t in db.Product on p.ProductId equals t.ProductId
                                              where SaleInvoiceLineIds.Contains(p.SaleInvoiceLineId)
                                              select new
                                              {
                                                  BalQty = p.BalanceQty,
                                                  SaleInvoiceLineId = p.SaleInvoiceLineId,
                                                  UnitId = t.UnitId,
                                              }).ToList();

                        if (ModelState.IsValid)
                        {
                            foreach (var SelectedSaleInvoice in SaleInvoiceAndQtys)
                            {
                                if (SelectedSaleInvoice.SaleInvoiceLineId > 0)
                                {
                                    var SaleInvoiceLine = new SaleInvoiceLineService(_unitOfWork).Find((SelectedSaleInvoice.SaleInvoiceLineId));
                                    var Product = new ProductService(_unitOfWork).Find(SaleInvoiceLine.ProductId);


                                    var bal = BalQtyandUnits.Where(m => m.SaleInvoiceLineId == SelectedSaleInvoice.SaleInvoiceLineId).FirstOrDefault();

                                    if (SelectedSaleInvoice.Qty <= bal.BalQty)
                                    {
                                        SaleDeliveryLine line = new SaleDeliveryLine();


                                        line.SaleDeliveryHeaderId = s.SaleDeliveryHeaderId;
                                        line.SaleInvoiceLineId = SaleInvoiceLine.SaleInvoiceLineId;
                                        line.Qty = SelectedSaleInvoice.Qty;
                                        line.UnitConversionMultiplier = SaleInvoiceLine.UnitConversionMultiplier ?? 1;
                                        line.DealQty = SelectedSaleInvoice.Qty * line.UnitConversionMultiplier;
                                        line.DealUnitId = SaleInvoiceLine.DealUnitId;
                                        line.Sr = Sr++;
                                        line.CreatedDate = DateTime.Now;
                                        line.ModifiedDate = DateTime.Now;
                                        line.CreatedBy = User.Identity.Name;
                                        line.ModifiedBy = User.Identity.Name;
                                        line.SaleDeliveryLineId = pk;
                                        line.ObjectState = Model.ObjectState.Added;
                                        new SaleDeliveryLineService(_unitOfWork).Create(line);


                                        pk++;
                                        Cnt = Cnt + 1;
                                    }
                                }
                            }
                        }
                        string Errormessage = "";
                        try
                        {
                            _unitOfWork.Save();
                        }

                        catch (Exception ex)
                        {
                            Errormessage = _exception.HandleException(ex);
                            ModelState.AddModelError("", Errormessage);
                            PrepareViewBag();
                            ViewBag.Mode = "Add";
                            return View("Create", svm);

                        }


                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = s.DocTypeId,
                            DocId = s.SaleDeliveryHeaderId,
                            ActivityType = (int)ActivityTypeContants.WizardCreate,
                            DocNo = s.DocNo,                            
                            DocDate = s.DocDate,
                            DocStatus = s.Status,
                        }));

                        System.Web.HttpContext.Current.Session.Remove("PendingInvoiceForSaleDelivery");

                        return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/SaleDeliveryHeader/Submit/" + s.SaleDeliveryHeaderId);
                    }
                    else
                    {
                        return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/SaleDeliveryHeader/Index/" + s.DocTypeId);
                    }
                }
                else
                {

                }

            }
            PrepareViewBag();
            ViewBag.Mode = "Add";
            return View("Create", svm);
        }


        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleDeliveryHeaderService.GetCustomPerson(filter, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

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

        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
            {
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);
                TempData.Remove("CSEXC");
            }
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
