using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using Model.ViewModel;
using Jobs.Helpers;
using AutoMapper;
using System.Configuration;
using System.Xml.Linq;
using PurchaseInvoiceReceiveDocumentEvents;
using DocumentEvents;
using CustomEventArgs;
using Reports.Controllers;
using Reports.Reports;
using Model.ViewModels;

namespace Jobs.Controllers
{
    [Authorize]
    public class DirectPurchaseInvoiceHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IPurchaseInvoiceHeaderService _PurchaseInvoiceHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public DirectPurchaseInvoiceHeaderController(IPurchaseInvoiceHeaderService PurchaseInvoiceHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseInvoiceHeaderService = PurchaseInvoiceHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!PurchaseInvoiceReceiveEvents.Initialized)
            {
                PurchaseInvoiceReceiveEvents Obj = new PurchaseInvoiceReceiveEvents();
            }

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

        // GET: /PurchaseInvoiceHeaderMaster/

        //public ActionResult Index(int id)//DocumentTypeId
        //{
        //    var PurchaseInvoiceHeader = _PurchaseInvoiceHeaderService.GetPurchaseInvoiceHeaderList(id, User.Identity.Name);
        //    PrepareViewBag(id);
        //    return View(PurchaseInvoiceHeader);
        //}

        public ActionResult Index(int id, string IndexType)//DocumentTypeId
        {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }
            var PurchaseInvoiceHeader = _PurchaseInvoiceHeaderService.GetPurchaseInvoiceHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(PurchaseInvoiceHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _PurchaseInvoiceHeaderService.GetPurchaseInvoicePendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _PurchaseInvoiceHeaderService.GetPurchaseInvoicePendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }


        void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();
            ViewBag.SalesTaxGroupList = new ChargeGroupPersonService(_unitOfWork).GetChargeGroupPersonList((int)(TaxTypeConstants.SalesTax)).ToList();
            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();
        }

        // GET: /PurchaseInvoiceHeaderMaster/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            PurchaseInvoiceHeaderViewModel vm = new PurchaseInvoiceHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateDirectPurchaseInvoice", "PurchaseInvoiceSetting", new { id = id }).Warning("Please create Purchase Invoice settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.PurchaseInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);
            vm.UnitConversionForId = settings.UnitConversionForId;
            vm.CalculateDiscountOnRate = settings.CalculateDiscountOnRate;
            vm.DocDate = DateTime.Now;
            vm.SupplierDocDate = DateTime.Now;
            vm.DocTypeId = id;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".PurchaseInvoiceHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);

            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(PurchaseInvoiceHeaderViewModel vm)
        {
            PurchaseInvoiceHeader pt = AutoMapper.Mapper.Map<PurchaseInvoiceHeaderViewModel, PurchaseInvoiceHeader>(vm);

            PurchaseGoodsReceiptHeader Receipt = Mapper.Map<PurchaseInvoiceHeaderViewModel, PurchaseGoodsReceiptHeader>(vm);

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(vm.DocTypeId, vm.DivisionId, vm.SiteId);
            if (!settings.PurchaseGoodsReceiptDocTypeId.HasValue)
                ModelState.AddModelError("", "Please create goodsreceipt document type in settings");

            if (vm.BillingAccountId <= 0)
            {
                ModelState.AddModelError("BillingAccountId", "The Billing Account field is required");
            }
            if (vm.GodownId <= 0)
            {
                ModelState.AddModelError("GodowId", "The Godown field is required");
            }

            #region BeforeSave
            bool BeforeSave = true;

            try
            {
                if (vm.PurchaseInvoiceHeaderId <= 0)
                    BeforeSave = PurchaseInvoiceReceiveDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseInvoiceReceiveDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceHeaderId, EventModeConstants.Edit), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }
            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before save";
            #endregion

            #region DocTypeTimeLineValidation

            try
            {

                if (vm.PurchaseInvoiceHeaderId <= 0)
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(vm), DocumentTimePlanTypeConstants.Create, User.Identity.Name, out ExceptionMsg, out Continue);
                else
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(vm), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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

            if (ModelState.IsValid && BeforeSave && !EventException && (TimePlanValidation || Continue))
            {
                #region CreateRecord
                if (vm.PurchaseInvoiceHeaderId <= 0)
                {
                    Receipt.DocTypeId = settings.PurchaseGoodsReceiptDocTypeId.Value;
                    Receipt.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".PurchaseGoodsReceiptHeaders", Receipt.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
                    Receipt.DivisionId = vm.DivisionId;
                    Receipt.SiteId = vm.SiteId;
                    Receipt.CreatedDate = DateTime.Now;
                    Receipt.ModifiedDate = DateTime.Now;
                    Receipt.CreatedBy = User.Identity.Name;
                    Receipt.ModifiedBy = User.Identity.Name;
                    Receipt.ObjectState = Model.ObjectState.Added;
                    db.PurchaseGoodsReceiptHeader.Add(Receipt);
                    //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Create(Receipt);

                    pt.PurchaseGoodsReceiptHeaderId = Receipt.PurchaseGoodsReceiptHeaderId;
                    pt.CalculateDiscountOnRate = vm.CalculateDiscountOnRate;
                    pt.DivisionId = vm.DivisionId;
                    pt.SiteId = vm.SiteId;
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    db.PurchaseInvoiceHeader.Add(pt);
                    //_PurchaseInvoiceHeaderService.Create(pt);

                    try
                    {
                        PurchaseInvoiceReceiveDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(pt.PurchaseInvoiceHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        EventException = true;
                    }


                    try
                    {
                        if (EventException)
                        { throw new Exception(); }

                        db.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", vm);
                    }

                    try
                    {
                        PurchaseInvoiceReceiveDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(pt.PurchaseInvoiceHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PurchaseInvoiceHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    //return Edit(pt.PurchaseInvoiceHeaderId).Success("Data saved successfully");
                    return RedirectToAction("Modify", new { id = pt.PurchaseInvoiceHeaderId }).Success("Data saved Successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseInvoiceHeader temp = _PurchaseInvoiceHeaderService.Find(pt.PurchaseInvoiceHeaderId);


                    PurchaseInvoiceHeader ExRec = new PurchaseInvoiceHeader();
                    ExRec = Mapper.Map<PurchaseInvoiceHeader>(temp);

                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted)
                        temp.Status = (int)StatusConstants.Modified;

                    temp.DocTypeId = pt.DocTypeId;
                    temp.CurrencyId = pt.CurrencyId;
                    temp.SupplierDocNo = pt.SupplierDocNo;
                    temp.SupplierDocDate = pt.SupplierDocDate;
                    temp.TermsAndConditions = pt.TermsAndConditions;
                    temp.SalesTaxGroupId = pt.SalesTaxGroupId;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.CreditDays = pt.CreditDays;
                    temp.Remark = pt.Remark;
                    temp.SupplierId = pt.SupplierId;
                    temp.BillingAccountId = pt.BillingAccountId;
                    temp.DocDate = pt.DocDate;
                    temp.DocNo = pt.DocNo;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceHeader.Add(temp);
                    //_PurchaseInvoiceHeaderService.Update(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    if (temp.PurchaseGoodsReceiptHeaderId.HasValue)
                    {

                        PurchaseGoodsReceiptHeader ReceiptHeader = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(temp.PurchaseGoodsReceiptHeaderId.Value);


                        PurchaseGoodsReceiptHeader ExRecLine = new PurchaseGoodsReceiptHeader();
                        ExRecLine = Mapper.Map<PurchaseGoodsReceiptHeader>(ReceiptHeader);

                        ReceiptHeader.DocDate = vm.DocDate;
                        //ReceiptHeader.DocNo = vm.DocNo;
                        ReceiptHeader.SupplierId = vm.SupplierId;
                        ReceiptHeader.GodownId = vm.GodownId;
                        ReceiptHeader.Status = temp.Status;
                        ReceiptHeader.SupplierDocNo = vm.SupplierDocNo;
                        ReceiptHeader.SupplierDocDate = vm.SupplierDocDate;
                        ReceiptHeader.ModifiedBy = User.Identity.Name;
                        ReceiptHeader.ModifiedDate = DateTime.Now;

                        ReceiptHeader.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseGoodsReceiptHeader.Add(ReceiptHeader);
                        //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(ReceiptHeader);

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = ExRecLine,
                            Obj = ReceiptHeader,
                        });

                        if (ReceiptHeader.StockHeaderId != null)
                        {
                            StockHeader S = db.StockHeader.Find(ReceiptHeader.StockHeaderId);
                            S.DocTypeId = ReceiptHeader.DocTypeId;
                            S.DocDate = ReceiptHeader.DocDate;
                            S.DocNo = ReceiptHeader.DocNo;
                            S.DivisionId = ReceiptHeader.DivisionId;
                            S.SiteId = ReceiptHeader.SiteId;
                            S.PersonId = ReceiptHeader.SupplierId;
                            S.GodownId = ReceiptHeader.GodownId;
                            S.Remark = ReceiptHeader.Remark;
                            S.Status = ReceiptHeader.Status;
                            S.ModifiedBy = ReceiptHeader.ModifiedBy;
                            S.ModifiedDate = ReceiptHeader.ModifiedDate;
                            S.ObjectState = Model.ObjectState.Modified;
                            db.StockHeader.Add(S);

                            //new StockHeaderService(_unitOfWork).UpdateStockHeader(S);
                        }


                    }


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseInvoiceReceiveDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseInvoiceHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        EventException = true;
                    }

                    try
                    {
                        if (EventException)
                        { throw new Exception(); }

                        db.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Edit";
                        return View("Create", pt);
                    }
                    try
                    {
                        PurchaseInvoiceReceiveDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseInvoiceHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseInvoiceHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));
                
                    return RedirectToAction("Index", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                #endregion
            }
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            PurchaseInvoiceHeaderViewModel pt = _PurchaseInvoiceHeaderService.GetPurchaseInvoiceHeader(id);
            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(pt), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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
            //Job Order Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateDirectPurchaseInvoice", "PurchaseInvoiceSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase Invoice settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.PurchaseInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);

            if (pt.PurchaseGoodsReceiptHeaderId.HasValue)
            {
                PurchaseGoodsReceiptHeader ReceiptHeader = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(pt.PurchaseGoodsReceiptHeaderId.Value);
                pt.GodownId = ReceiptHeader.GodownId;
            }
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.PurchaseInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }

        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                var Settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(DocTypeId, DivisionId, SiteId);

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = db.PurchaseInvoiceHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.PurchaseInvoiceHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

                        byte[] Pdf;

                        if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Import || pd.Status == (int)StatusConstants.Modified)
                        {
                            //LogAct(item.ToString());
                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);
                            PdfStream.Add(Pdf);
                        }
                        else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
                        {
                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterSubmit, User.Identity.Name, item);
                            PdfStream.Add(Pdf);
                        }
                        else
                        {
                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterApprove, User.Identity.Name, item);
                            PdfStream.Add(Pdf);
                        }

                    }

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

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            PurchaseInvoiceHeader header = _PurchaseInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            PurchaseInvoiceHeader header = _PurchaseInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            PurchaseInvoiceHeader header = _PurchaseInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            PurchaseInvoiceHeader header = _PurchaseInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            PurchaseInvoiceHeader header = _PurchaseInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DetailInformation(int id)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail" });
        }


        [Authorize]
        public ActionResult Detail(int id, string IndexType, string transactionType, int? DocLineId)
        {

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            PurchaseInvoiceHeaderViewModel pt = _PurchaseInvoiceHeaderService.GetPurchaseInvoiceHeader(id);
            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }

            //Job Order Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PurchaseInvoiceSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase Invoice settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.PurchaseInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.PurchaseInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }




        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            bool Continue = true;

            PurchaseInvoiceHeader s = db.PurchaseInvoiceHeader.Find(id);

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
            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseInvoiceReceiveDocEvents.beforeHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            PurchaseInvoiceHeader pd = new PurchaseInvoiceHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                int ActivityType;

                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {


                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    //_PurchaseInvoiceHeaderService.Update(pd);

                    if (pd.PurchaseGoodsReceiptHeaderId.HasValue)
                    {

                        PurchaseGoodsReceiptHeader ReceiptHeader = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(pd.PurchaseGoodsReceiptHeaderId.Value);
                        ReceiptHeader.Status = pd.Status;
                        ReceiptHeader.ReviewBy = null;
                        ReceiptHeader.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseGoodsReceiptHeader.Add(ReceiptHeader);
                        //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(ReceiptHeader);
                    }


                    #region "Ledger Posting"

                    LedgerHeaderViewModel LedgerHeaderViewModel = new LedgerHeaderViewModel();

                    LedgerHeaderViewModel.LedgerHeaderId = pd.LedgerHeaderId ?? 0;
                    LedgerHeaderViewModel.DocTypeId = pd.DocTypeId;
                    LedgerHeaderViewModel.DocDate = pd.DocDate;
                    LedgerHeaderViewModel.DocNo = pd.DocNo;
                    LedgerHeaderViewModel.DivisionId = pd.DivisionId;
                    LedgerHeaderViewModel.SiteId = pd.SiteId;
                    LedgerHeaderViewModel.Narration = "";
                    LedgerHeaderViewModel.Remark = pd.Remark;
                    LedgerHeaderViewModel.ExchangeRate = pd.ExchangeRate;
                    LedgerHeaderViewModel.CreatedBy = pd.CreatedBy;
                    LedgerHeaderViewModel.CreatedDate = DateTime.Now.Date;
                    LedgerHeaderViewModel.ModifiedBy = pd.ModifiedBy;
                    LedgerHeaderViewModel.ModifiedDate = DateTime.Now.Date;

                    IEnumerable<PurchaseInvoiceHeaderCharge> PurchaseInvoiceHeaderCharges = (from H in db.PurchaseInvoiceHeaderCharge where H.HeaderTableId == Id select H);
                    IEnumerable<PurchaseInvoiceLineCharge> PurchaseInvoiceLineCharges = (from L in db.PurchaseInvoiceLineCharge where L.HeaderTableId == Id select L);

                    try
                    {
                        new CalculationService(_unitOfWork).LedgerPostingDB(ref LedgerHeaderViewModel, PurchaseInvoiceHeaderCharges, PurchaseInvoiceLineCharges, ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        return RedirectToAction("Detail", new { id = Id, IndexType = IndexType, transactionType = "submit" });
                    }


                    if (pd.LedgerHeaderId == null)
                    {
                        pd.LedgerHeaderId = LedgerHeaderViewModel.LedgerHeaderId;
                        //_PurchaseInvoiceHeaderService.Update(pd);                  
                    }
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceHeader.Add(pd);

                    #endregion



                    try
                    {
                        PurchaseInvoiceReceiveDocEvents.onHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        EventException = true;
                    }


                    try
                    {
                        if (EventException)
                        { throw new Exception(); }

                        db.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
                    }

                    try
                    {
                        PurchaseInvoiceReceiveDocEvents.afterHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.PurchaseInvoiceHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.PurchaseInvoiceHeaders", "PurchaseInvoiceHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {
                            var PendingtoSubmitCount = _PurchaseInvoiceHeaderService.GetPurchaseInvoicePendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Invoice " + pd.DocNo + " submitted successfully.");
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Invoice " + pd.DocNo + " submitted successfully.");
                        }

                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("Purchase Invoice " + pd.DocNo + " submitted successfully.");

                    }

                    else
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Invoice " + pd.DocNo + " submitted successfully.");
                }
                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + pd.ModifiedBy + " only.");
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId });
        }

        public ActionResult Review(int id, string IndexType, string TransactionType)
        {
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "review" : TransactionType });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Review")]
        public ActionResult Reviewed(int Id, string IndexType, string UserRemark, string IsContinue)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseInvoiceReceiveDocEvents.beforeHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            PurchaseInvoiceHeader pd = new PurchaseInvoiceHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";
                pd.ObjectState = Model.ObjectState.Modified;
                db.PurchaseInvoiceHeader.Add(pd);


                if (pd.PurchaseGoodsReceiptHeaderId.HasValue)
                {
                    PurchaseGoodsReceiptHeader ReceiptHeader = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(pd.PurchaseGoodsReceiptHeaderId.Value);
                    ReceiptHeader.Status = pd.Status;
                    ReceiptHeader.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                    ReceiptHeader.ReviewBy += User.Identity.Name + ", ";
                    ReceiptHeader.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReceiptHeader.Add(ReceiptHeader);
                    //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(ReceiptHeader);
                }

                try
                {
                    PurchaseInvoiceReceiveDocEvents.onHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                try
                {
                    if (EventException)
                    { throw new Exception(); }

                    db.SaveChanges();
                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    return RedirectToAction("Index", new { id = pd.DocTypeId });
                }


                try
                {
                    PurchaseInvoiceReceiveDocEvents.afterHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.PurchaseInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));
            
                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    PurchaseInvoiceHeader HEader = _PurchaseInvoiceHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.PurchaseInvoiceHeaders", "PurchaseInvoiceHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _PurchaseInvoiceHeaderService.GetPurchaseInvoicePendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
                        if (PendingtoSubmitCount > 0)
                            return RedirectToAction("Index_PendingToReview", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");
                        else
                            return RedirectToAction("Index", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");

                    }

                    ViewBag.PendingToReview = PendingToReviewCount(Id);
                    return RedirectToAction("Detail", new { id = nextId, transactionType = "ReviewContinue", IndexType = IndexType }).Success("Record Reviewed Successfully.");
                }


                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId });
        }







        // GET: /ProductMaster/Delete/5

        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseInvoiceHeader PurchaseInvoiceHeader = db.PurchaseInvoiceHeader.Find(id);
            if (PurchaseInvoiceHeader == null)
            {
                return HttpNotFound();
            }
            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(PurchaseInvoiceHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseInvoiceReceiveDocEvents.beforeHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before delete";

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                var temp = db.PurchaseInvoiceHeader.Find(vm.id);

                try
                {
                    PurchaseInvoiceReceiveDocEvents.onHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseInvoiceHeader>(temp),
                });

                int PurchaseGoodsReceiptHeaderId = temp.PurchaseGoodsReceiptHeaderId.Value;

                var line = (from p in db.PurchaseInvoiceLine
                            where p.PurchaseInvoiceHeaderId == vm.id
                            select p).ToList();
                var InvLineIds = line.Select(m => m.PurchaseInvoiceLineId).ToArray();

                var InvLineCharges = (from p in db.PurchaseInvoiceLineCharge
                                      where InvLineIds.Contains(p.LineTableId)
                                      select p).ToList();

                foreach (var item in InvLineCharges)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseInvoiceLineCharge.Remove(item);
                }
                foreach (var item in line)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<PurchaseInvoiceLine>(item),
                    });

                    //var linecharges = new PurchaseInvoiceLineChargeService(_unitOfWork).GetCalculationProductList(item.PurchaseInvoiceLineId);

                    //foreach (var citem in linecharges)
                    //{
                    //    new PurchaseInvoiceLineChargeService(_unitOfWork).Delete(citem.Id);
                    //}

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseInvoiceLine.Remove(item);

                    //new PurchaseInvoiceLineService(_unitOfWork).Delete(item.PurchaseInvoiceLineId);

                }

                var headercharges = (from p in db.PurchaseInvoiceHeaderCharge
                                     where p.HeaderTableId == vm.id
                                     select p).ToList();

                foreach (var item in headercharges)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseInvoiceHeaderCharge.Remove(item);
                    //new PurchaseInvoiceHeaderChargeService(_unitOfWork).Delete(item.Id);
                }

                var LedgerHeadId = temp.LedgerHeaderId;

                //_PurchaseInvoiceHeaderService.Delete(vm.id);
                temp.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseInvoiceHeader.Remove(temp);


                if (LedgerHeadId.HasValue && LedgerHeadId.Value > 0)
                {
                    var LedgHead = db.LedgerHeader.Find(LedgerHeadId);

                    var Ledgers = (from p in db.Ledger
                                   where p.LedgerHeaderId == LedgerHeadId
                                   select p).ToList();

                    foreach (var item in Ledgers)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.Ledger.Remove(item);
                    }

                    LedgHead.ObjectState = Model.ObjectState.Deleted;
                    db.LedgerHeader.Remove(LedgHead);

                }



                int? StockHeaderId = 0;

                PurchaseGoodsReceiptHeader ReceiptHeader = db.PurchaseGoodsReceiptHeader.Find(PurchaseGoodsReceiptHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseGoodsReceiptHeader>(ReceiptHeader),
                });


                StockHeaderId = ReceiptHeader.StockHeaderId;

                var ReceiptLines = (from p in db.PurchaseGoodsReceiptLine
                                    where p.PurchaseGoodsReceiptHeaderId == ReceiptHeader.PurchaseGoodsReceiptHeaderId
                                    select p).ToList();

                List<int> StockIdList = new List<int>();

                foreach (var item in ReceiptLines)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<PurchaseGoodsReceiptLine>(item),
                    });

                    if (item.StockId != null)
                    {
                        StockIdList.Add((int)item.StockId);
                    }

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseGoodsReceiptLine.Remove(item);
                    //new PurchaseGoodsReceiptLineService(_unitOfWork).Delete(item.PurchaseGoodsReceiptLineId);
                }

                //foreach (var item in StockIdList)
                //{
                //    
                //    //new StockService(_unitOfWork).DeleteStockDB(item, ref db, true);
                //}

                //Changing to new Logic for GroupDelete
                new StockService(_unitOfWork).DeleteStockDBMultiple(StockIdList, ref db, true);

                ReceiptHeader.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseGoodsReceiptHeader.Remove(ReceiptHeader);
                //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Delete(ReceiptHeader.PurchaseGoodsReceiptHeaderId);


                if (StockHeaderId != null)
                {

                    var StockHead = db.StockHeader.Find(StockHeaderId);
                    StockHead.ObjectState = Model.ObjectState.Deleted;
                    db.StockHeader.Remove(StockHead);
                    //new StockHeaderService(_unitOfWork).Delete((int)StockHeaderId);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    if (EventException)
                    { throw new Exception(); }

                    db.SaveChanges();
                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    return PartialView("_Reason", vm);
                }

                try
                {
                    PurchaseInvoiceReceiveDocEvents.afterHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.PurchaseInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = temp.DocNo,
                    xEModifications = Modifications,
                    DocDate = temp.DocDate,
                    DocStatus = temp.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }

        [HttpGet]
        public ActionResult NextPage(int id)//CurrentHeaderId
        {
            var nextId = _PurchaseInvoiceHeaderService.NextId(id);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _PurchaseInvoiceHeaderService.PrevId(id);
            return Edit(nextId, "");
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

            Menu menu = new MenuService(_unitOfWork).Find(Dt.ReportMenuId ?? 0);

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

        public int PendingToSubmitCount(int id)
        {
            return (_PurchaseInvoiceHeaderService.GetPurchaseInvoicePendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_PurchaseInvoiceHeaderService.GetPurchaseInvoicePendingToReview(id, User.Identity.Name)).Count();
        }

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _PurchaseInvoiceHeaderService.GetCustomPerson(filter, searchTerm);
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
