using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Jobs.Helpers;
using Data.Infrastructure;
using Presentation.ViewModels;
using AutoMapper;
using System.Configuration;
using Presentation;
using Microsoft.Reporting.WebForms;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;


namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class SaleInvoiceHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        ISaleInvoiceHeaderService _SaleInvoiceHeaderService;
        ISaleDispatchHeaderService _SaleDispatchHeaderService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public SaleInvoiceHeaderController(ISaleInvoiceHeaderService SaleInvoiceHeaderService, ISaleDispatchHeaderService SaleDispatchHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleInvoiceHeaderService = SaleInvoiceHeaderService;
            _SaleDispatchHeaderService = SaleDispatchHeaderService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public ActionResult ChooseType(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            return PartialView("ChooseType");
        }
        [HttpGet]
        public ActionResult CopyFromExisting()
        {
            return PartialView("CopyFromExisting");
        }
        [HttpPost]
        public ActionResult CopyFromExisting(CopyFromExistingSaleInvoiceViewModel vm)
        {

            if (ModelState.IsValid)
            {

                SaleInvoiceHeaderDetail saleinvoiceheaderdetail = _SaleInvoiceHeaderService.Find(vm.SaleInvoiceHeaderId);
                SaleDispatchHeader saledispatchheader = _SaleDispatchHeaderService.Find(saleinvoiceheaderdetail.SaleDispatchHeaderId.Value);

                SaleInvoiceHeaderDetail newsaleinvoiceheaderdetail = new SaleInvoiceHeaderDetail();
                SaleDispatchHeader newsaledispatchheader = new SaleDispatchHeader();


                newsaledispatchheader.DeliveryTermsId = saledispatchheader.DeliveryTermsId;
                newsaledispatchheader.DivisionId = saledispatchheader.DivisionId;
                newsaledispatchheader.DocDate = saledispatchheader.DocDate;
                newsaledispatchheader.DocNo = vm.SaleInvoiceDocNo;
                newsaledispatchheader.DocTypeId = saledispatchheader.DocTypeId;
                newsaledispatchheader.FormNo = saledispatchheader.FormNo;
                newsaledispatchheader.GateEntryNo = saledispatchheader.GateEntryNo;
                newsaledispatchheader.Remark = saledispatchheader.Remark;
                newsaledispatchheader.SaleToBuyerId = saledispatchheader.SaleToBuyerId;
                newsaledispatchheader.ShipMethodId = saledispatchheader.ShipMethodId;
                newsaledispatchheader.ShipToPartyAddress = saledispatchheader.ShipToPartyAddress;
                newsaledispatchheader.SiteId = saledispatchheader.SiteId;
                newsaledispatchheader.Transporter = saledispatchheader.Transporter;

                newsaledispatchheader.CreatedDate = DateTime.Now;
                newsaledispatchheader.ModifiedDate = DateTime.Now;
                newsaledispatchheader.CreatedBy = User.Identity.Name;
                newsaledispatchheader.ModifiedBy = User.Identity.Name;
                newsaledispatchheader.Status = (int)StatusConstants.Drafted;
                _SaleDispatchHeaderService.Create(newsaledispatchheader);



                newsaleinvoiceheaderdetail.BaleNoSeries = saleinvoiceheaderdetail.BaleNoSeries;
                newsaleinvoiceheaderdetail.BillToBuyerId = saleinvoiceheaderdetail.BillToBuyerId;
                newsaleinvoiceheaderdetail.BLDate = saleinvoiceheaderdetail.BLDate;
                newsaleinvoiceheaderdetail.BLNo = saleinvoiceheaderdetail.BLNo;
                newsaleinvoiceheaderdetail.CircularDate = saleinvoiceheaderdetail.CircularDate;
                newsaleinvoiceheaderdetail.CircularNo = saleinvoiceheaderdetail.CircularNo;
                newsaleinvoiceheaderdetail.Compositions = saleinvoiceheaderdetail.Compositions;
                newsaleinvoiceheaderdetail.CurrencyId = saleinvoiceheaderdetail.CurrencyId;
                newsaleinvoiceheaderdetail.DescriptionOfGoods = saleinvoiceheaderdetail.DescriptionOfGoods;
                newsaleinvoiceheaderdetail.DestinationPort = saleinvoiceheaderdetail.DestinationPort;
                newsaleinvoiceheaderdetail.DivisionId = saleinvoiceheaderdetail.DivisionId;
                newsaleinvoiceheaderdetail.DocDate = saleinvoiceheaderdetail.DocDate;
                newsaleinvoiceheaderdetail.DocTypeId = saleinvoiceheaderdetail.DocTypeId;
                newsaleinvoiceheaderdetail.ExchangeRate = saleinvoiceheaderdetail.ExchangeRate;
                newsaleinvoiceheaderdetail.FinalPlaceOfDelivery = saleinvoiceheaderdetail.FinalPlaceOfDelivery;
                newsaleinvoiceheaderdetail.KindsOfackages = saleinvoiceheaderdetail.KindsOfackages;
                newsaleinvoiceheaderdetail.NotifyParty = saleinvoiceheaderdetail.NotifyParty;
                newsaleinvoiceheaderdetail.SaleToBuyerId = saleinvoiceheaderdetail.SaleToBuyerId;
                newsaleinvoiceheaderdetail.OrderDate = saleinvoiceheaderdetail.OrderDate;
                newsaleinvoiceheaderdetail.OrderNo = saleinvoiceheaderdetail.OrderNo;
                newsaleinvoiceheaderdetail.OtherRefrence = saleinvoiceheaderdetail.OtherRefrence;
                newsaleinvoiceheaderdetail.PackingMaterialDescription = saleinvoiceheaderdetail.PackingMaterialDescription;
                newsaleinvoiceheaderdetail.PlaceOfPreCarriage = saleinvoiceheaderdetail.PlaceOfPreCarriage;
                newsaleinvoiceheaderdetail.PortOfLoading = saleinvoiceheaderdetail.PortOfLoading;
                newsaleinvoiceheaderdetail.PreCarriageBy = saleinvoiceheaderdetail.PreCarriageBy;
                newsaleinvoiceheaderdetail.PrivateMark = saleinvoiceheaderdetail.PrivateMark;
                newsaleinvoiceheaderdetail.Remark = saleinvoiceheaderdetail.Remark;
                newsaleinvoiceheaderdetail.SaleDispatchHeaderId = newsaledispatchheader.SaleDispatchHeaderId;
                newsaleinvoiceheaderdetail.SiteId = saleinvoiceheaderdetail.SiteId;
                newsaleinvoiceheaderdetail.TermsOfSale = saleinvoiceheaderdetail.TermsOfSale;
                newsaleinvoiceheaderdetail.TransporterInformation = saleinvoiceheaderdetail.TransporterInformation;

                newsaleinvoiceheaderdetail.CreatedDate = DateTime.Now;
                newsaleinvoiceheaderdetail.ModifiedDate = DateTime.Now;
                newsaleinvoiceheaderdetail.DocNo = vm.SaleInvoiceDocNo;
                newsaleinvoiceheaderdetail.CreatedBy = User.Identity.Name;
                newsaleinvoiceheaderdetail.ModifiedBy = User.Identity.Name;
                newsaleinvoiceheaderdetail.Status = (int)StatusConstants.Drafted;
                _SaleInvoiceHeaderService.Create(newsaleinvoiceheaderdetail);



                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("CopyFromExisting", vm);

                }

                return Json(new { success = true });


            }

            return PartialView("CopyFromExisting", vm);

        }


        // GET: /SaleInvoiceHeader/

        public ActionResult Index(int id, string IndexType)//DocTypeId 
        {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }
            IQueryable<SaleInvoiceHeaderIndexViewModel> p = _SaleInvoiceHeaderService.GetSaleInvoiceHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(p);
        }


        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _SaleInvoiceHeaderService.GetSaleInvoiceHeaderListPendingToSubmit(id, User.Identity.Name);

            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _SaleInvoiceHeaderService.GetSaleInvoiceHeaderListPendingToReview(id, User.Identity.Name);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }


        [HttpGet]
        public ActionResult NextPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _SaleInvoiceHeaderService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult PrevPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _SaleInvoiceHeaderService.PrevId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Report(int id)
        {
            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).Find(id);

            Dictionary<int, string> DefaultValue = new Dictionary<int, string>();

            if (!Dt.ReportMenuId.HasValue)
                throw new Exception("Report Menu not configured in document types");

            Model.Models.Menu menu = new MenuService(_unitOfWork).Find(Dt.ReportMenuId ?? 0);

            if (menu != null)
            {
                ReportHeader header = new ReportHeaderService(_unitOfWork).GetReportHeaderByName(menu.MenuName);

                ReportLine Line = new ReportLineService(_unitOfWork).GetReportLineByName("DocumentType", header.ReportHeaderId);
                if (Line != null)
                    DefaultValue.Add(Line.ReportLineId, id.ToString());
                ReportLine Site = new ReportLineService(_unitOfWork).GetReportLineByName("Site", header.ReportHeaderId);
                if (Site != null)
                    DefaultValue.Add(Site.ReportLineId, ((int)System.Web.HttpContext.Current.Session["SiteId"]).ToString());
                ReportLine Division = new ReportLineService(_unitOfWork).GetReportLineByName("Division", header.ReportHeaderId);
                if (Division != null)
                    DefaultValue.Add(Division.ReportLineId, ((int)System.Web.HttpContext.Current.Session["DivisionId"]).ToString());

            }

            TempData["ReportLayoutDefaultValues"] = DefaultValue;

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        [HttpGet]
        public ActionResult Remove()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        private void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;

            ViewBag.ShipMethodList = new ShipMethodService(_unitOfWork).GetShipMethodList().ToList();
            ViewBag.DeliveryTermsList = new DeliveryTermsService(_unitOfWork).GetDeliveryTermsList().ToList();
            ViewBag.BuyerList = new BuyerService(_unitOfWork).GetBuyerList().ToList();
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();


        }

        // GET: /SaleInvoiceHeader/Create

        public ActionResult Create(int id)//DocTypeId
        {

            SaleInvoiceHeaderIndexViewModel p = new SaleInvoiceHeaderIndexViewModel();

            p.CreatedDate = DateTime.Now;
            p.DocDate = DateTime.Now.Date;
            p.BlDate = DateTime.Now.Date;
            p.CircularDate = DateTime.Now.Date;
            p.OrderDate = DateTime.Now.Date;
            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            p.DocTypeId = id;

            PrepareViewBag(id);
            ViewBag.Mode = "Add";
            p.DocNo = _SaleInvoiceHeaderService.GetMaxDocNo();
            return View("Create", p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HeaderPost(SaleInvoiceHeaderIndexViewModel svm)
        {

            #region DocTypeTimeLineValidation

            try
            {

                if (svm.SaleInvoiceHeaderId <= 0)
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

                #region CreateRecord
                if (svm.SaleInvoiceHeaderId == 0)
                {
                    SaleInvoiceHeaderDetail saleinvoiceheaderdetail = Mapper.Map<SaleInvoiceHeaderIndexViewModel, SaleInvoiceHeaderDetail>(svm);
                    SaleDispatchHeader saledispatchheader = Mapper.Map<SaleInvoiceHeaderIndexViewModel, SaleDispatchHeader>(svm);

                    saleinvoiceheaderdetail.CreatedDate = DateTime.Now;
                    saleinvoiceheaderdetail.ModifiedDate = DateTime.Now;
                    saleinvoiceheaderdetail.CreatedBy = User.Identity.Name;
                    saleinvoiceheaderdetail.ModifiedBy = User.Identity.Name;
                    saleinvoiceheaderdetail.Status = (int)StatusConstants.Drafted;
                    _SaleInvoiceHeaderService.Create(saleinvoiceheaderdetail);


                    saledispatchheader.DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDoctypeConstants.SaleChallan).DocumentTypeId;
                    saledispatchheader.CreatedDate = DateTime.Now;
                    saledispatchheader.ModifiedDate = DateTime.Now;
                    saledispatchheader.CreatedBy = User.Identity.Name;
                    saledispatchheader.ModifiedBy = User.Identity.Name;
                    saledispatchheader.Status = (int)StatusConstants.Drafted;
                    _SaleDispatchHeaderService.Create(saledispatchheader);


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(svm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = saleinvoiceheaderdetail.DocTypeId,
                        DocId = saleinvoiceheaderdetail.SaleInvoiceHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = saleinvoiceheaderdetail.DocNo,
                        DocDate = saleinvoiceheaderdetail.DocDate,
                        DocStatus = saleinvoiceheaderdetail.Status,
                    }));


                    return RedirectToAction("Modify", new { id = saleinvoiceheaderdetail.SaleInvoiceHeaderId }).Success("Data saved Successfully");
                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleInvoiceHeaderDetail saleinvoiceheaderdetail = _SaleInvoiceHeaderService.Find(svm.SaleInvoiceHeaderId);
                    SaleDispatchHeader saledispatchheader = _SaleDispatchHeaderService.Find(svm.SaleDispatchHeaderId);

                    SaleInvoiceHeaderDetail ExRec = new SaleInvoiceHeaderDetail();
                    ExRec = Mapper.Map<SaleInvoiceHeaderDetail>(saleinvoiceheaderdetail);


                    SaleDispatchHeader ExRecD = new SaleDispatchHeader();
                    ExRecD = Mapper.Map<SaleDispatchHeader>(saledispatchheader);


                    StringBuilder logstring = new StringBuilder();

                    int status = saleinvoiceheaderdetail.Status;

                    if (saleinvoiceheaderdetail.Status != (int)StatusConstants.Drafted)
                        saleinvoiceheaderdetail.Status = (int)StatusConstants.Modified;


                    saleinvoiceheaderdetail.BillToBuyerId = svm.BillToBuyerId;
                    saleinvoiceheaderdetail.BLDate = svm.BlDate;
                    saleinvoiceheaderdetail.BLNo = svm.BlNo;
                    saleinvoiceheaderdetail.CircularNo = svm.CircularNo;
                    saleinvoiceheaderdetail.CircularDate = svm.CircularDate;
                    saleinvoiceheaderdetail.Compositions = svm.Compositions;
                    saleinvoiceheaderdetail.CurrencyId = svm.CurrencyId;
                    saleinvoiceheaderdetail.ExchangeRate = svm.ExchangeRate;
                    saleinvoiceheaderdetail.DescriptionOfGoods = svm.DescriptionOfGoods;
                    saleinvoiceheaderdetail.PackingMaterialDescription = svm.PackingMaterialDescription;
                    saleinvoiceheaderdetail.DestinationPort = svm.DestinationPort;
                    saleinvoiceheaderdetail.DocDate = svm.DocDate;
                    saleinvoiceheaderdetail.DocNo = svm.DocNo;
                    saleinvoiceheaderdetail.SaleToBuyerId = svm.SaleToBuyerId;
                    saleinvoiceheaderdetail.DocTypeId = svm.DocTypeId;
                    saleinvoiceheaderdetail.FinalPlaceOfDelivery = svm.FinalPlaceOfDelivery;
                    saleinvoiceheaderdetail.KindsOfackages = svm.KindsOfackages;
                    saleinvoiceheaderdetail.InvoiceAmount = svm.InvoiceAmount;
                    saleinvoiceheaderdetail.NotifyParty = svm.NotifyParty;
                    saleinvoiceheaderdetail.OrderDate = svm.OrderDate;
                    saleinvoiceheaderdetail.OrderNo = svm.OrderNo;
                    saleinvoiceheaderdetail.OtherRefrence = svm.OtherRefrence;
                    saleinvoiceheaderdetail.PlaceOfPreCarriage = svm.PlaceOfPreCarriage;
                    saleinvoiceheaderdetail.PortOfLoading = svm.PortOfLoading;
                    saleinvoiceheaderdetail.PreCarriageBy = svm.PreCarriageBy;
                    saleinvoiceheaderdetail.PrivateMark = svm.PrivateMark;
                    saleinvoiceheaderdetail.Remark = svm.Remark;
                    saleinvoiceheaderdetail.BaleNoSeries = svm.BaleNoSeries;
                    saleinvoiceheaderdetail.TermsOfSale = svm.TermsOfSale;
                    saleinvoiceheaderdetail.Freight = svm.Freight;
                    saleinvoiceheaderdetail.Insurance = svm.Insurance;
                    saleinvoiceheaderdetail.FreightRemark = svm.FreightRemark;
                    saleinvoiceheaderdetail.InsuranceRemark = svm.InsuranceRemark;
                    saleinvoiceheaderdetail.VehicleNo = svm.VehicleNo;
                    saleinvoiceheaderdetail.Deduction = svm.Deduction;


                    saleinvoiceheaderdetail.TransporterInformation = svm.TransporterInformation;
                    saleinvoiceheaderdetail.ModifiedDate = DateTime.Now;
                    saleinvoiceheaderdetail.ModifiedBy = User.Identity.Name;
                    _SaleInvoiceHeaderService.Update(saleinvoiceheaderdetail);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = saleinvoiceheaderdetail,
                    });


                    saledispatchheader.DocDate = svm.DocDate;
                    saledispatchheader.SaleToBuyerId = svm.SaleToBuyerId;
                    saledispatchheader.ShipToPartyAddress = svm.ShipToPartyAddress;
                    saledispatchheader.Transporter = svm.Transporter;
                    saledispatchheader.DeliveryTermsId = svm.DeliveryTermsId;
                    saledispatchheader.ShipMethodId = svm.ShipMethodId;
                    saledispatchheader.Remark = svm.Remark;
                    saledispatchheader.ModifiedDate = DateTime.Now;
                    saledispatchheader.ModifiedBy = User.Identity.Name;
                    _SaleDispatchHeaderService.Update(saledispatchheader);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecD,
                        Obj = saledispatchheader,
                    });
                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(svm.DocTypeId);
                        ViewBag.Mode = "Edit";
                        return View("Create", svm);
                    }

                    //Saving Activity Log::
                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = saleinvoiceheaderdetail.DocTypeId,
                        DocId = saleinvoiceheaderdetail.SaleInvoiceHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = saleinvoiceheaderdetail.DocNo,
                        xEModifications = Modifications,
                        DocDate = saleinvoiceheaderdetail.DocDate,
                        DocStatus = saleinvoiceheaderdetail.Status,
                    }));
                    //End of Saving ActivityLog

                    return RedirectToAction("Index", new { id = svm.DocTypeId }).Success("Data saved successfully");
                } 
                #endregion

            }
            PrepareViewBag(svm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", svm);
        }

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            SaleInvoiceHeader header = _SaleInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            SaleInvoiceHeader header = _SaleInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }


        // GET: /SaleInvoiceHeader/Edit/5
        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;

            SaleInvoiceHeaderDetail s = _SaleInvoiceHeaderService.GetSaleInvoiceHeaderDetail(id);

            if (s == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(s), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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

           
            if ((!TimePlanValidation && !Continue))
            {
                return RedirectToAction("DetailInformation", new { id = id, IndexType = IndexType });
            }

            SaleDispatchHeader sd = _SaleDispatchHeaderService.GetSaleDispatchHeader((int)s.SaleDispatchHeaderId);

            SaleInvoiceHeaderIndexViewModel svm = Mapper.Map<SaleInvoiceHeaderDetail, SaleInvoiceHeaderIndexViewModel>(s);

            svm.SaleDispatchHeaderId = sd.SaleDispatchHeaderId;
            svm.SaleToBuyerId = sd.SaleToBuyerId;
            svm.ShipToPartyAddress = sd.ShipToPartyAddress;
            svm.Transporter = sd.Transporter;
            svm.DeliveryTermsId = sd.DeliveryTermsId;
            svm.ShipMethodId = sd.ShipMethodId.Value;
            svm.Remark = sd.Remark;
            ViewBag.Mode = "Edit";
            PrepareViewBag(s.DocTypeId);

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.SaleInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));

            return View("Create", svm);
        }

        [HttpGet]
        public ActionResult DetailInformation(int id, string IndexType)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail", IndexType = IndexType });
        }

        [Authorize]
        public ActionResult Detail(int id, string transactionType, string IndexType)
        {
            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            SaleInvoiceHeaderDetail s = _SaleInvoiceHeaderService.GetSaleInvoiceHeaderDetail(id);

            if (s == null)
            {
                return HttpNotFound();
            }

            SaleDispatchHeader sd = _SaleDispatchHeaderService.GetSaleDispatchHeader((int)s.SaleDispatchHeaderId);

            SaleInvoiceHeaderIndexViewModel svm = Mapper.Map<SaleInvoiceHeaderDetail, SaleInvoiceHeaderIndexViewModel>(s);

            svm.SaleDispatchHeaderId = sd.SaleDispatchHeaderId;
            svm.SaleToBuyerId = sd.SaleToBuyerId;
            svm.ShipToPartyAddress = sd.ShipToPartyAddress;
            svm.Transporter = sd.Transporter;
            svm.DeliveryTermsId = sd.DeliveryTermsId;
            svm.ShipMethodId = sd.ShipMethodId.Value;
            svm.Remark = sd.Remark;
            
            PrepareViewBag(s.DocTypeId);

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.SaleInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));

            return View("Create", svm);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            SaleInvoiceHeader header = _SaleInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            SaleInvoiceHeader header = _SaleInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }

        // GET: /PurchaseOrderHeader/Delete/5

        private ActionResult Remove(int id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SaleInvoiceHeaderIndexViewModel SaleInvoiceHeader = _SaleInvoiceHeaderService.GetSaleInvoiceHeaderVM(id);
            if (SaleInvoiceHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(SaleInvoiceHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
                TempData["CSEXC"] += ExceptionMsg;
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation && !Continue)
            {
                return PartialView("AjaxError");
            }
            #endregion

            ReasonViewModel rvm = new ReasonViewModel()
            {
                id = id,
            };
            return PartialView("_Reason", rvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReasonViewModel vm)
        {
            if (ModelState.IsValid)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                db.Configuration.AutoDetectChangesEnabled = false;

                //For Roll back Packing no Status

                var temp = (from L in db.SaleInvoiceLine
                            join dl in db.SaleDispatchLine on L.SaleDispatchLineId equals dl.SaleDispatchLineId into SaleDispatchLineTable
                            from SaleDispatchLineTab in SaleDispatchLineTable.DefaultIfEmpty()
                            join pl in db.PackingLine on SaleDispatchLineTab.PackingLineId equals pl.PackingLineId into PackingLineTable
                            from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                            where L.SaleInvoiceHeaderId == vm.id
                            group new { PackingLineTab } by new { PackingLineTab.PackingHeaderId } into Result
                            select new
                            {
                                PackingHeaderId = Result.Key.PackingHeaderId
                            }).ToList();


                foreach (var item in temp)
                {
                    PackingHeader PackingHeader = new PackingHeaderService(_unitOfWork).Find(item.PackingHeaderId);
                    PackingHeader.Status = (int)ActivityTypeContants.Approved;
                    PackingHeader.ObjectState = Model.ObjectState.Modified;
                    db.PackingHeader.Add(PackingHeader);
                }




                //string temp = (Request["Redirect"].ToString());
                //first find the Purchase Order Object based on the ID. (sience this object need to marked to be deleted IE. ObjectState.Deleted)
                var SaleInvoiceHeader = _SaleInvoiceHeaderService.GetSaleInvoiceHeaderDetail(vm.id);

                //_ActivityLogService.Create(al);
                
                
                //var SaleDispatchHeader = _SaleDispatchHeaderService.Find((int)SaleInvoiceHeader.SaleDispatchHeaderId);
                //new StockService(_unitOfWork).DeleteStockForDocHeader(SaleDispatchHeader.SaleDispatchHeaderId, SaleInvoiceHeader.DocTypeId, SaleInvoiceHeader.SiteId, SaleInvoiceHeader.DivisionId, db);

                int? StockHeaderId = (from H in db.SaleDispatchHeader
                                      where H.SaleDispatchHeaderId == SaleInvoiceHeader.SaleDispatchHeaderId
                                      select H).FirstOrDefault().StockHeaderId;

                int? LedgerHeaderId = (from H in db.SaleInvoiceHeader
                                      where H.SaleInvoiceHeaderId == vm.id
                                      select H).FirstOrDefault().LedgerHeaderId;


                var SaleInvoiceLine = (from L in db.SaleInvoiceLine where L.SaleInvoiceHeaderId == vm.id select L).ToList();

                //new SaleOrderLineStatusService(_unitOfWork).DeleteSaleQtyOnInvoiceMultiple(vm.id);


                DeleteSaleQtyOnInvoiceMultiple(SaleInvoiceHeader.SaleInvoiceHeaderId);

                int cnt = 0;
                foreach (var item in SaleInvoiceLine)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });

                    cnt = cnt + 1;
                    try
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.SaleInvoiceLine.Attach(item);
                        db.SaleInvoiceLine.Remove(item);
                    }
                    catch (Exception e)
                    {
                        string str = e.Message;
                    }

                }



                var SaleDispatchLine = (from L in db.SaleDispatchLine where L.SaleDispatchHeaderId == SaleInvoiceHeader.SaleDispatchHeaderId select L).ToList();

                foreach (var item in SaleDispatchLine)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.SaleDispatchLine.Attach(item);
                    db.SaleDispatchLine.Remove(item);
                }

                SaleInvoiceHeader Si = (from H in db.SaleInvoiceHeader where H.SaleInvoiceHeaderId == vm.id select H).FirstOrDefault();
                SaleDispatchHeader Sd = (from H in db.SaleDispatchHeader where H.SaleDispatchHeaderId == SaleInvoiceHeader.SaleDispatchHeaderId select H).FirstOrDefault();

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Si,
                });

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Sd,
                });


                Si.ObjectState = Model.ObjectState.Deleted;
                db.SaleInvoiceHeader.Attach(Si);
                db.SaleInvoiceHeader.Remove(Si);

                Sd.ObjectState = Model.ObjectState.Deleted;
                db.SaleDispatchHeader.Attach(Sd);
                db.SaleDispatchHeader.Remove(Sd);

                if (StockHeaderId != null)
                {
                    IEnumerable<Stock> StockList = new StockService(_unitOfWork).GetStockForStockHeaderId((int)StockHeaderId);
                    foreach(var item in StockList)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.Stock.Attach(item);
                        db.Stock.Remove(item);
                    }

                    StockHeader StockHeader = new StockHeaderService(_unitOfWork).Find((int)StockHeaderId);

                    StockHeader.ObjectState = Model.ObjectState.Deleted;
                    db.StockHeader.Attach(StockHeader);
                    db.StockHeader.Remove(StockHeader);
                }

                if (LedgerHeaderId != null)
                {
                    IEnumerable<Ledger> LedgerList = new LedgerService(_unitOfWork).FindForLedgerHeader((int)LedgerHeaderId);
                    foreach (var item in LedgerList)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.Ledger.Attach(item);
                        db.Ledger.Remove(item);
                    }

                    LedgerHeader LedgerHeader = new LedgerHeaderService(_unitOfWork).Find((int)LedgerHeaderId);

                    LedgerHeader.ObjectState = Model.ObjectState.Deleted;
                    db.LedgerHeader.Attach(LedgerHeader);
                    db.LedgerHeader.Remove(LedgerHeader);
                }



                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                //Commit the DB
                try
                {
                    db.SaveChanges();
                    db.Configuration.AutoDetectChangesEnabled = true;
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    db.Configuration.AutoDetectChangesEnabled = true;
                    TempData["CSEXC"] += message;
                    PrepareViewBag(SaleInvoiceHeader.DocTypeId);
                    return PartialView("_Reason", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Si.DocTypeId,
                    DocId = Si.SaleInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = Si.DocNo,
                    xEModifications = Modifications,
                    DocDate = Si.DocDate,
                    DocStatus = Si.Status,
                }));

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }



        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            #region DocTypeTimeLineValidation

            SaleInvoiceHeader s = db.SaleInvoiceHeader.Find(id);

            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(s), DocumentTimePlanTypeConstants.Submit, User.Identity.Name, out ExceptionMsg, out Continue);
                TempData["CSEXC"] += ExceptionMsg;
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation && !Continue)
            {
                return RedirectToAction("Index", new { id = s.DocTypeId, IndexType = IndexType });
            }
            #endregion
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "submit" : TransactionType });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Submit")]
        public ActionResult Submitted(int Id, string IndexType, string UserRemark, string IsContinue)
        {
            int SaleAc = 6650;

            SaleInvoiceHeader pd = new SaleInvoiceHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {


                    pd.Status = (int)StatusConstants.Submitted;
                    pd.ReviewBy = null;

                    _SaleInvoiceHeaderService.Update(pd);


                    int LedgerHeaderId = 0;

                    if (pd.LedgerHeaderId == 0 || pd.LedgerHeaderId == null)
                    {
                        LedgerHeader LedgerHeader = new LedgerHeader();

                        LedgerHeader.DocTypeId = pd.DocTypeId;
                        LedgerHeader.DocDate = pd.DocDate;
                        LedgerHeader.DocNo = pd.DocNo;
                        LedgerHeader.DivisionId = pd.DivisionId;
                        LedgerHeader.SiteId = pd.SiteId;
                        LedgerHeader.Narration = "";
                        LedgerHeader.Remark = pd.Remark;
                        LedgerHeader.CreatedBy = pd.CreatedBy;
                        LedgerHeader.CreatedDate = DateTime.Now.Date;
                        LedgerHeader.ModifiedBy = pd.ModifiedBy;
                        LedgerHeader.ModifiedDate = DateTime.Now.Date;

                        new LedgerHeaderService(_unitOfWork).Create(LedgerHeader);

                        pd.LedgerHeaderId = LedgerHeader.LedgerHeaderId;
                        _SaleInvoiceHeaderService.Update(pd);
                    }
                    else
                    {
                        LedgerHeader LedgerHeader = new LedgerHeaderService(_unitOfWork).Find((int)pd.LedgerHeaderId);

                        LedgerHeader.DocTypeId = pd.DocTypeId;
                        LedgerHeader.DocDate = pd.DocDate;
                        LedgerHeader.DocNo = pd.DocNo;
                        LedgerHeader.DivisionId = pd.DivisionId;
                        LedgerHeader.SiteId = pd.SiteId;
                        LedgerHeader.Narration = "";
                        LedgerHeader.Remark = pd.Remark;
                        LedgerHeader.ModifiedBy = pd.ModifiedBy;
                        LedgerHeader.ModifiedDate = DateTime.Now.Date;

                        new LedgerHeaderService(_unitOfWork).Update(LedgerHeader);

                        IEnumerable<Ledger> LedgerList = new LedgerService(_unitOfWork).FindForLedgerHeader(LedgerHeader.LedgerHeaderId);

                        foreach (Ledger item in LedgerList)
                        {
                            new LedgerService(_unitOfWork).Delete(item);
                        }

                        LedgerHeaderId = LedgerHeader.LedgerHeaderId;
                    }


                    Decimal TotalAmt = (from L in db.SaleInvoiceLine
                                        where L.SaleInvoiceHeaderId == pd.SaleInvoiceHeaderId
                                        select new
                                        {
                                            TotalAmt = L.Amount
                                        }).ToList().Sum(m => m.TotalAmt);

                    Ledger LedgerDr = new Ledger();

                    if (LedgerHeaderId != 0)
                    {
                        LedgerDr.LedgerHeaderId = LedgerHeaderId;
                    }
                    LedgerDr.LedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(pd.BillToBuyerId).LedgerAccountId;
                    LedgerDr.ContraLedgerAccountId = SaleAc;
                    LedgerDr.AmtDr = TotalAmt * (pd.ExchangeRate ?? 1);
                    LedgerDr.AmtCr = 0;
                    LedgerDr.Narration = "";

                    new LedgerService(_unitOfWork).Create(LedgerDr);

                    Ledger LedgerCr = new Ledger();

                    if (LedgerHeaderId != 0)
                    {
                        LedgerCr.LedgerHeaderId = LedgerHeaderId;
                    }
                    LedgerCr.LedgerAccountId = SaleAc;
                    LedgerCr.ContraLedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(pd.BillToBuyerId).LedgerAccountId;
                    LedgerCr.AmtDr = 0;
                    LedgerCr.AmtCr = TotalAmt * (pd.ExchangeRate ?? 1);
                    LedgerCr.Narration = "";

                    new LedgerService(_unitOfWork).Create(LedgerCr);


                    try
                    {

                        _unitOfWork.Save();

                    }
                    catch (Exception ex)
                    {                       
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        return RedirectToAction("Index", new { id = pd.DocTypeId });
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.SaleInvoiceHeaderId,
                        ActivityType = (int)ActivityTypeContants.Submitted,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));
                    
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record submitted successfully.");
                }
                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + pd.ModifiedBy + " only.");
            }

            return View();
        }



        public ActionResult Review(int id, string IndexType, string TransactionType)
        {
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "review" : TransactionType });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Review")]
        public ActionResult Reviewed(int Id, string IndexType, string UserRemark, string IsContinue)
        {
            SaleInvoiceHeader pd = new SaleInvoiceHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {
                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                _SaleInvoiceHeaderService.Update(pd);
                _unitOfWork.Save();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.SaleInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));

                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record reviewed successfully.");
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Error in reviewing.");
        }

        public int PendingToSubmitCount(int id)
        {
            return (_SaleInvoiceHeaderService.GetSaleInvoiceHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_SaleInvoiceHeaderService.GetSaleInvoiceHeaderListPendingToReview(id, User.Identity.Name)).Count();
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



        public ActionResult Print(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.id = id;

            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = "PDF", Value = "PDF" });
            temp.Add(new SelectListItem { Text = "Excel", Value = "Excel" });
            //temp.Add(new SelectListItem { Text = "Word", Value = "Word" });

            ViewBag.ReportFormat = new SelectList(temp, "Value", "Text", "PDF");

            return PartialView("Print");
        }


        public ActionResult PrintDocument(int id, string DocumentName, string ReportFormat)
        {

            ReportViewer reportViewer = new ReportViewer();
            IEnumerable<SaleInvoicePrintViewModel> SaleInvoiceprintviewmodel;

            switch (DocumentName)
            {
                case "Exporter Declaration":
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_ExporterDeclaration.rdlc");
                        break;
                    }

                case "Cargo Declaration":
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_CargoDeclaration.rdlc");
                        break;
                    }

                case "Order Sheet":
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintInvoiceData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_OrderSheet.rdlc");
                        break;
                    }

                case "Invoice":
                    {
                        //SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintInvoiceData(id);
                        //reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_Invoice.rdlc");
                        //break;

                        String query = "Web.ProcSaleInvoicePrint_ForInvoice";
                        return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_DocumentPrint/DocumentPrint/?DocumentId=" + id + "&queryString=" + query + "&ReportFileType=" + ReportFormat);

                    }

                case "Invoice With Collection":
                    {
                        String query = "Web.ProcSaleInvoicePrint_ForInvoice_WithCollection";
                        return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_DocumentPrint/DocumentPrint/?DocumentId=" + id + "&queryString=" + query + "&ReportFileType=" + ReportFormat);


                    }

                case "Single Country Declaration":
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_SingleCountryDeclaration.rdlc");
                        break;
                    }

                case "Post Shipment Covering Letter":
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_PostShipmentCoveringLetter.rdlc");
                        break;
                    }

                case "Document Of Exchange":
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_DocumentOfExchange.rdlc");
                        break;
                    }

                case "Shipping Agent Letter":
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_ShippingAgentLetter.rdlc");
                        break;
                    }

                case "Export Value Declaration":
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_ExportValueDeclaration.rdlc");
                        break;
                    }

                case "Measurement List":
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_MeasurementList.rdlc");
                        break;
                    }

                case "Packing List":
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintPackingListData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_PackingList.rdlc");
                        break;
                    }

                case "Packing List With Collection":
                    {
                        //SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintPackingListWithCollectionData(id);
                        //reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_PackingListWithCollection.rdlc");
                        //break;

                        String query = "Web.ProcSalePackingListPrint_WithCollection";
                        return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_DocumentPrint/DocumentPrint/?DocumentId=" + id + "&queryString=" + query + "&ReportFileType=" + ReportFormat);

                    }

                case "Special Custom Invoice":
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_SpecialCustomInvoice.rdlc");
                        break;
                    }

                case "Master Key":
                    {
                        IEnumerable<MasterKeyPrintViewModel> MasterKeyPrintViewModel;
                        MasterKeyPrintViewModel = _SaleInvoiceHeaderService.FGetPrintMasterKeyData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_MasterKey.rdlc");
                        ReportService reportservice_masterkey = new ReportService();
                        reportservice_masterkey.SetReportAttibutes(reportViewer, new ReportDataSource("DsMain", MasterKeyPrintViewModel), "Sale Invoice", "");
                        return reportservice_masterkey.PrintReport(reportViewer, ReportFormat);

                    }

                default:
                    {
                        SaleInvoiceprintviewmodel = _SaleInvoiceHeaderService.FGetPrintData(id);
                        reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + "SaleInvoice_Invoice.rdlc");
                        break;
                    }
            }

            ReportService reportservice = new ReportService();
            reportservice.SetReportAttibutes(reportViewer, new ReportDataSource("DsMain", SaleInvoiceprintviewmodel), "Sale Invoice", "");


            var Company = (from C in db.Company
                           join Ct in db.City on C.CityId equals Ct.CityId into CityTable
                           from CityTab in CityTable.DefaultIfEmpty()
                           join St in db.State on CityTab.StateId equals St.StateId into StateTable
                           from StateTab in StateTable.DefaultIfEmpty()
                           join Con in db.Country on StateTab.CountryId equals Con.CountryId into CountryTable
                           from CountryTab in CountryTable.DefaultIfEmpty()
                           select new
                           {
                               CompanyName = C.CompanyName,
                               CompanyCountry = CountryTab.CountryName,
                               CompanyExciseDivision = C.ExciseDivision,
                               CompanyDirector = C.DirectorName,
                               CompanyPhone = C.Phone,
                               CompanyFax = C.Fax,
                               CompanyBankName = C.BankName,
                               CompanyBankBranch = C.BankBranch,
                               CompanyIEC = C.IECNo,
                               CompanyTin = C.TinNo
                           }).FirstOrDefault();


            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyName", Company.CompanyName));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyCountry", Company.CompanyCountry));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyExciseDivision", Company.CompanyExciseDivision));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyDirector", Company.CompanyDirector));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyPhone", Company.CompanyPhone));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyFax", Company.CompanyFax));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyBankName", Company.CompanyBankName));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyBankBranch", Company.CompanyBankBranch));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyIEC", Company.CompanyIEC));
            reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyTin", Company.CompanyTin));


            //reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyName", "SURYA CARPET PVT. LTD."));
            //reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyCountry", "India"));
            //reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyExciseDivision", "Allahbad/Bhadohi"));
            //reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyDirector", "Surya Mani Tiwari"));
            //reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyPhone", "+91-5414-268253"));
            //reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyFax", "+91-5414-268571"));
            //reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyBankName", "State Bank Of India"));
            //reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyBankBranch", "Specialised Commercial Branch, Varanasi"));
            //reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyIEC", "1588000311"));
            //reportViewer.LocalReport.SetParameters(new ReportParameter("CompanyTin", "09715500172"));

            return reportservice.PrintReport(reportViewer, ReportFormat);
        }


        public void DeleteSaleQtyOnInvoiceMultiple(int id)
        {




            using (ApplicationDbContext con = new ApplicationDbContext())
            {
                con.Database.CommandTimeout = 30000;
                con.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");


                var LineAndQty = (from t in con.SaleInvoiceLine
                                  where t.SaleInvoiceHeaderId == id
                                  group t by t.SaleOrderLineId into g
                                  select new
                                  {
                                      LineId = g.Key,
                                      Qty = g.Sum(m => m.Qty)
                                  }).ToList();

                int[] IsdA2 = null;
                IsdA2 = LineAndQty.Select(m => m.LineId.Value).ToArray();

                var SaleOrderLineStatus = (from p in con.SaleOrderLineStatus
                                           where IsdA2.Contains(p.SaleOrderLineId.Value)
                                           select p
                                            ).ToList();

                foreach (var item in SaleOrderLineStatus)
                {
                    item.InvoiceQty = item.InvoiceQty - (LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault().Qty);
                    item.ShipQty = item.InvoiceQty;
                    item.ObjectState = Model.ObjectState.Modified;
                    db.SaleOrderLineStatus.Add(item);
                }

            }




        }



    }
}
