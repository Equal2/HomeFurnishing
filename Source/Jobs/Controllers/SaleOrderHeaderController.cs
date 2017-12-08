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
using Presentation;
using System.Text;
using EmailContents;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;
using Reports.Reports;
using System.Configuration;

namespace Jobs.Controllers
{
    [Authorize]
    public class SaleOrderHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext context = new ApplicationDbContext();        

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        ISaleOrderHeaderService _SaleOrderHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public SaleOrderHeaderController(ISaleOrderHeaderService PurchaseOrderHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleOrderHeaderService = PurchaseOrderHeaderService;
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

        // GET: /SaleOrderHeader/

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
            IQueryable<SaleOrderHeaderIndexViewModel> p = _SaleOrderHeaderService.GetSaleOrderHeaderList(id, User.Identity.Name);
            PrepareViewBagSettings(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(p);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _SaleOrderHeaderService.GetSaleOrderHeaderListPendingToSubmit(id, User.Identity.Name);
            PrepareViewBagSettings(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _SaleOrderHeaderService.GetSaleOrderHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBagSettings(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }


        private void PrepareViewBagSettings(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(id, DivisionId, SiteId);
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();
            if (settings != null)
            {
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.ExportMenuId = settings.ExportMenuId;
            }

        }


        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleOrderHeaders", "SaleOrderHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleOrderHeaders", "SaleOrderHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            //return View("~/Views/Shared/UnderImplementation.cshtml");
            MultipleDocumentPrint rvm = new MultipleDocumentPrint()
            {
                // id = 5,
            };
            return PartialView("MultipleDocumentPrint", rvm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult History(MultipleDocumentPrint vm)
        {
            ApplicationDbContext Db = new ApplicationDbContext();
            String query = Db.strSchemaName + ".ProcSaleOrderPrint " + 3.ToString();
            string PackingId = (vm.SaleOrderHeaderId.ToString());
            //String query = Db.strSchemaName + ".ProcSaleOrderPrintToMultiple '" + PackingId.ToString() + "'";
            String ReportTitle = "Sale Order";
            return RedirectToAction("DocumentPrint", "Report_DocumentPrint", new { queryString = query, ReportTitle = ReportTitle });
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

            //if (!Dt.ReportMenuId.HasValue)
            //    throw new Exception("Report Menu not configured in document types");

            if (!Dt.ReportMenuId.HasValue)
                return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/GridReport/GridReportLayout/?MenuName=Sale Order Report&DocTypeId=" + id.ToString());


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

        private void PrepareViewBag(SaleOrderHeaderIndexViewModel s)
        {


            ViewBag.UnitConvForList = (from p in context.UnitConversonFor
                                       select p).ToList();
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(s.DocTypeId).DocumentTypeName;
            ViewBag.id = s.DocTypeId;
            //ViewBag.DivisionList = new DivisionService(_unitOfWork).GetDivisionList().ToList();            
            ViewBag.ShipMethodList = new ShipMethodService(_unitOfWork).GetShipMethodList().ToList();
            ViewBag.DeliveryTermsList = new DeliveryTermsService(_unitOfWork).GetDeliveryTermsList().ToList();
            ViewBag.BuyerList = new BuyerService(_unitOfWork).GetBuyerList().ToList();
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();
            //ViewBag.SiteList = new SiteService(_unitOfWork).GetSiteList().ToList();
            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(SaleOrderPriority), -10), Value = ((int)(SaleOrderPriority.Low)).ToString() });
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(SaleOrderPriority), 0), Value = ((int)(SaleOrderPriority.Normal)).ToString() });
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(SaleOrderPriority), 10), Value = ((int)(SaleOrderPriority.High)).ToString() });

            if (s == null)
                ViewBag.Priority = new SelectList(temp, "Value", "Text");
            else
                ViewBag.Priority = new SelectList(temp, "Value", "Text", s.Priority);

        }

        // GET: /SaleOrderHeader/Create

        public ActionResult Create(int id)
        {

            SaleOrderHeaderIndexViewModel p = new SaleOrderHeaderIndexViewModel();

            p.DocDate = DateTime.Now.Date;
            p.DueDate = DateTime.Now.Date;
            p.CreatedDate = DateTime.Now;
            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            SaleOrderSettings settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(id, p.DivisionId, p.SiteId);
            if (settings != null)
            {
                p.DocTypeId = id;
                p.ShipMethodId = settings.ShipMethodId;
                p.DeliveryTermsId = settings.DeliveryTermsId;
                p.Priority = settings.Priority;
                p.CurrencyId = settings.CurrencyId;
                p.UnitConversionForId = settings.UnitConversionForId;
                p.ProcessId = settings.ProcessId;
                PrepareViewBag(p);
            }
            else
            {
                return RedirectToAction("Edit", "SaleOrderSettings", new { id = id }).Warning("Please create sale order settings");
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            p.SaleOrderSettings = Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(settings);
            ViewBag.Mode = "Add";
            p.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".SaleOrderHeaders", p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);
            return View("Create", p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HeaderPost(SaleOrderHeaderIndexViewModel svm)
        {
            if (svm.DocDate > svm.DueDate)
            {
                ModelState.AddModelError("DueDate", "DueDate cannot be greater than DocDate");
            }

            #region DocTypeTimeLineValidation

            try
            {

                if (svm.SaleOrderHeaderId <= 0)
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
                if (svm.SaleOrderHeaderId == 0)
                {
                    SaleOrderHeader s = Mapper.Map<SaleOrderHeaderIndexViewModel, SaleOrderHeader>(svm);

                    s.ActualDueDate = s.DueDate;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.Status = (int)StatusConstants.Drafted;
                    _SaleOrderHeaderService.Create(s);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(svm);
                        ViewBag.Mode = "Add";
                        return View("Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = s.DocTypeId,
                        DocId = s.SaleOrderHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = s.DocNo,
                        DocDate = s.DocDate,
                        DocStatus = s.Status,
                    }));

                    return RedirectToAction("Modify", new { id = s.SaleOrderHeaderId }).Success("Data saved Successfully");
                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    //string tempredirect = (Request["Redirect"].ToString());
                    SaleOrderHeader s = Mapper.Map<SaleOrderHeaderIndexViewModel, SaleOrderHeader>(svm);
                    StringBuilder logstring = new StringBuilder();
                    SaleOrderHeader temp = _SaleOrderHeaderService.Find(s.SaleOrderHeaderId);

                    SaleOrderHeader ExRec = new SaleOrderHeader();
                    ExRec = Mapper.Map<SaleOrderHeader>(temp);

                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;

                    temp.DocTypeId = s.DocTypeId;
                    temp.DocDate = s.DocDate;
                    temp.DocNo = s.DocNo;
                    temp.BuyerOrderNo = s.BuyerOrderNo;
                    temp.SaleToBuyerId = s.SaleToBuyerId;
                    temp.BillToBuyerId = s.BillToBuyerId;
                    temp.CurrencyId = s.CurrencyId;
                    temp.Priority = s.Priority;
                    temp.UnitConversionForId = s.UnitConversionForId;
                    temp.ShipMethodId = s.ShipMethodId;
                    temp.ShipAddress = s.ShipAddress;
                    temp.DeliveryTermsId = s.DeliveryTermsId;
                    temp.Remark = s.Remark;
                    temp.DueDate = s.DueDate;
                    temp.Advance = s.Advance;
                    temp.FinancierId = s.FinancierId;
                    temp.SalesExecutiveId = s.SalesExecutiveId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    _SaleOrderHeaderService.Update(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
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
                        PrepareViewBag(svm);
                        ViewBag.Mode = "Edit";
                        return View("Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleOrderHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("Index", new { id = svm.DocTypeId }).Success("Data saved successfully");
                } 
                #endregion

            }
            PrepareViewBag(svm);
            ViewBag.Mode = "Add";
            return View("Create", svm);
        }

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            SaleOrderHeader header = _SaleOrderHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            SaleOrderHeader header = _SaleOrderHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }


        // GET: /SaleOrderHeader/Edit/5
        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;

            SaleOrderHeader s = _SaleOrderHeaderService.GetSaleOrderHeader(id);
            if (s == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
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


            SaleOrderHeaderIndexViewModel svm = Mapper.Map<SaleOrderHeader, SaleOrderHeaderIndexViewModel>(s);
            PrepareViewBag(svm);
            ViewBag.Mode = "Edit";

            SaleOrderSettings temp = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(s.DocTypeId, s.DivisionId, s.SiteId);
            svm.SaleOrderSettings = Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(temp);
            svm.ProcessId = temp.ProcessId;

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.SaleOrderHeaderId,
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

            SaleOrderHeader s = _SaleOrderHeaderService.GetSaleOrderHeader(id);
            SaleOrderHeaderIndexViewModel svm = Mapper.Map<SaleOrderHeader, SaleOrderHeaderIndexViewModel>(s);


            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(s.DocTypeId, s.DivisionId , s.SiteId);
            svm.SaleOrderSettings = Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(settings);


            PrepareViewBag(svm);
            if (s == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.SaleOrderHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));


            return View("Create", svm);
        }


        public ActionResult Delete(int id)
        {
            SaleOrderHeader header = _SaleOrderHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            SaleOrderHeader header = _SaleOrderHeaderService.Find(id);
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
            SaleOrderHeader SaleOrderHeader = _SaleOrderHeaderService.GetSaleOrderHeader(id);
            if (SaleOrderHeader == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, SaleOrderHeader.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(SaleOrderHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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



                


                //string temp = (Request["Redirect"].ToString());
                //first find the Purchase Order Object based on the ID. (sience this object need to marked to be deleted IE. ObjectState.Deleted)
                var SaleOrderHeader = _SaleOrderHeaderService.GetSaleOrderHeader(vm.id);


                //For Updating Enquiry Header and Lines so that it can be edited and deleted as needed.
                if (SaleOrderHeader.ReferenceDocId != null && SaleOrderHeader.ReferenceDocId != 0)
                {
                    var SaleEnquiryHeader = (from H in context.SaleEnquiryHeader where H.SaleEnquiryHeaderId == SaleOrderHeader.ReferenceDocId && H.DocTypeId == SaleOrderHeader.ReferenceDocTypeId select H).FirstOrDefault();
                    if (SaleEnquiryHeader != null)
                    {
                        SaleEnquiryHeader Header = new SaleEnquiryHeaderService(_unitOfWork).Find(SaleEnquiryHeader.SaleEnquiryHeaderId);
                        Header.LockReason = null;
                        new SaleEnquiryHeaderService(_unitOfWork).Update(Header);

                        IEnumerable<SaleEnquiryLine> LineList = new SaleEnquiryLineService(_unitOfWork).GetSaleEnquiryLineListForHeader(SaleEnquiryHeader.SaleEnquiryHeaderId);
                        foreach (SaleEnquiryLine Line in LineList)
                        {
                            Line.LockReason = null;
                            new SaleEnquiryLineService(_unitOfWork).Update(Line);
                        }
                    }
                }

                

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = SaleOrderHeader,
                });

                //Then find all the Purchase Order Header Line associated with the above ProductType.
                var SaleOrderLine = new SaleOrderLineService(_unitOfWork).GetSaleOrderLineList(vm.id);



                List<int> StockIdList = new List<int>();

                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in SaleOrderLine)
                {
                    if (item.StockId != null)
                    {
                        StockIdList.Add((int)item.StockId);
                    }

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });

                    new SaleOrderLineStatusService(_unitOfWork).Delete(item.SaleOrderLineId);

                    new SaleOrderLineService(_unitOfWork).Delete(item.SaleOrderLineId);
                }

                foreach (var item in StockIdList)
                {
                    if (item != null)
                    {
                        new StockService(_unitOfWork).DeleteStock((int)item);
                    }
                }

                int? StockHeaderId = null;
                StockHeaderId = SaleOrderHeader.StockHeaderId;

                int LedgerHeaderId = SaleOrderHeader.LedgerHeaderId ?? 0;
                

                // Now delete the Sale Order Header
                new SaleOrderHeaderService(_unitOfWork).Delete(vm.id);

                // Now delete the Ledger & Ledger Header
                if (LedgerHeaderId != 0)
                {
                    var LedgerList = new LedgerService(_unitOfWork).FindForLedgerHeader(LedgerHeaderId).ToList();                    
                    foreach (var item in LedgerList)
                    {
                        new LedgerService(_unitOfWork).Delete(item.LedgerId);
                    }
                    new LedgerHeaderService(_unitOfWork).Delete(LedgerHeaderId);
                }



                if (StockHeaderId != null)
                {
                    new StockHeaderService(_unitOfWork).Delete((int)StockHeaderId);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                //Commit the DB
                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    return PartialView("_Reason", vm);
                }

                //Logging Activity

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = SaleOrderHeader.DocTypeId,
                    DocId = SaleOrderHeader.SaleOrderHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = SaleOrderHeader.DocNo,
                    xEModifications = Modifications,
                    DocDate = SaleOrderHeader.DocDate,
                    DocStatus = SaleOrderHeader.Status,
                }));

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }



        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            SaleOrderHeader s = context.SaleOrderHeader.Find(id);
            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Submit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation

            
            try
            {
                TimePlanValidation = Submitvalidation(id, out ExceptionMsg);
                TempData["CSEXC"] += ExceptionMsg;
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }
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
            SaleOrderHeader pd = new SaleOrderHeaderService(_unitOfWork).Find(Id);
            if (ModelState.IsValid)
            {
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {
                    int ActivityType;

                    pd.ReviewBy = null;
                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;


                    int LedgerHeaderId = 0;
                    LedgerHeaderId = LedgerPost(pd);
                    pd.LedgerHeaderId = LedgerHeaderId;

                    _SaleOrderHeaderService.Update(pd);


                    _unitOfWork.Save();

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.SaleOrderHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    //SendEmail_PODrafted(Id);
                    if (pd.Status == (int)StatusConstants.Submitted)
                        SaleOrderEmailContents.SendSaleOrderSubmitEmail(Id);
                    //else if (pd.Status == (int)StatusConstants.ModificationSubmitted)
                    //    SaleOrderEmailContents.SendSaleOrderModifiedEmail(Id, UserRemark);

                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record Submitted successfully.");
                }
                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + pd.ModifiedBy + " only.");
            }

            return View();
        }

        public int LedgerPost(SaleOrderHeader pd)
        {
            int LedgerHeaderId = 0;

            if (pd.LedgerHeaderId == 0 || pd.LedgerHeaderId == null)
            {
                LedgerHeader LedgerHeader = new LedgerHeader();

                LedgerHeader.DocTypeId = pd.DocTypeId;
                LedgerHeader.DocDate = pd.DocDate;
                LedgerHeader.DocNo = pd.DocNo;
                LedgerHeader.DivisionId = pd.DivisionId;
                LedgerHeader.SiteId = pd.SiteId;
                LedgerHeader.Remark = pd.Remark;
                LedgerHeader.CreatedBy = pd.CreatedBy;
                LedgerHeader.CreatedDate = DateTime.Now.Date;
                LedgerHeader.ModifiedBy = pd.ModifiedBy;
                LedgerHeader.ModifiedDate = DateTime.Now.Date;

                new LedgerHeaderService(_unitOfWork).Create(LedgerHeader);
            }
            else
            {
                LedgerHeader LedgerHeader = new LedgerHeaderService(_unitOfWork).Find((int)pd.LedgerHeaderId);

                LedgerHeader.DocTypeId = pd.DocTypeId;
                LedgerHeader.DocDate = pd.DocDate;
                LedgerHeader.DocNo = pd.DocNo;
                LedgerHeader.DivisionId = pd.DivisionId;
                LedgerHeader.SiteId = pd.SiteId;
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


            int SalesAc = new LedgerAccountService(_unitOfWork).Find(LedgerAccountConstants.Sale).LedgerAccountId;

            if (pd.Advance > 0)
            {
                Ledger LedgerDr = new Ledger();
                if (LedgerHeaderId != 0) { LedgerDr.LedgerHeaderId = LedgerHeaderId; }
                LedgerDr.LedgerAccountId = SalesAc;
                LedgerDr.ContraLedgerAccountId = pd.BillToBuyerId;
                LedgerDr.AmtDr = pd.Advance ?? 0;
                LedgerDr.AmtCr = 0;
                LedgerDr.Narration = "";
                new LedgerService(_unitOfWork).Create(LedgerDr);

                Ledger LedgerCr = new Ledger();
                if (LedgerHeaderId != 0) { LedgerCr.LedgerHeaderId = LedgerHeaderId; }
                LedgerCr.LedgerAccountId = pd.BillToBuyerId;
                LedgerCr.ContraLedgerAccountId = SalesAc;
                LedgerCr.AmtDr = 0;
                LedgerCr.AmtCr = pd.Advance ?? 0;
                LedgerCr.Narration = "";
                new LedgerService(_unitOfWork).Create(LedgerCr);
            }

            return LedgerHeaderId;
        }



        public ActionResult Review(int id, string IndexType, string TransactionType)
        {
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "review" : TransactionType });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Review")]
        public ActionResult Reviewed(int Id, string IndexType, string UserRemark, string IsContinue)
        {
            SaleOrderHeader pd = new SaleOrderHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {


                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                _SaleOrderHeaderService.Update(pd);

                _unitOfWork.Save();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.SaleOrderHeaderId,
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
            return (_SaleOrderHeaderService.GetSaleOrderHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_SaleOrderHeaderService.GetSaleOrderHeaderListPendingToReview(id, User.Identity.Name)).Count();
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
                context.Dispose();
            }
            base.Dispose(disposing);
        }


     public ActionResult GeneratePrints(string Ids, int DocTypeId)
         {

             if (!string.IsNullOrEmpty(Ids))
             {
                 int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                 int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                
                var Settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

                if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, DocTypeId, Settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "GeneratePrints") == false)
                {
                    return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
                }

                try
                 {

                     List<byte[]> PdfStream = new List<byte[]>();
                     foreach (var item in Ids.Split(',').Select(Int32.Parse))
                     {

                         DirectReportPrint drp = new DirectReportPrint();

                         var pd = context.SaleOrderHeader.Find(item);

                         LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                         {
                             DocTypeId = pd.DocTypeId,
                             DocId = pd.SaleOrderHeaderId,
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
        
         
        public ActionResult Import(int id)//Document Type Id
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(id, DivisionId, SiteId);

            if (settings != null)
            {
                if (settings.ImportMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.ImportMenuId);

                    if (menuviewmodel == null)
                    {
                        return View("~/Views/Shared/UnderImplementation.cshtml");
                    }
                    else if (!string.IsNullOrEmpty(menuviewmodel.URL))
                    {
                        return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + id + "?MenuId=" + menuviewmodel.MenuId);
                    }
                    else
                    {
                        return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = id });
                    }
                }
            }
            return RedirectToAction("Index", new { id = id });
        }

        #region submitValidation
        public bool Submitvalidation(int id, out string Msg)
        {
            Msg = "";
            int Stockline = (new SaleOrderLineService(_unitOfWork).GetSaleOrderLineListForIndex(id)).Count();
            if (Stockline == 0)
            {
                Msg = "Add Line Record. <br />";
            }
            else
            {
                Msg = "";
            }
            return (string.IsNullOrEmpty(Msg));
        }

        #endregion submitValidation
        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleOrderHeaderService.GetCustomPerson(filter, searchTerm);
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
    }
}
