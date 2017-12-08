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
using Reports.Controllers;
using Model.ViewModel;
using Reports.Reports;



namespace Jobs.Controllers
{
    [Authorize]
    public class DispatchWaybillHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IDispatchWaybillHeaderService _DispatchWaybillHeaderService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public DispatchWaybillHeaderController(IDispatchWaybillHeaderService DispatchWaybillHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _DispatchWaybillHeaderService = DispatchWaybillHeaderService;
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

        private void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
        }

        public ActionResult Index(int id, string IndexType)
        {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }
            IQueryable<DispatchWaybillHeaderViewModel> p = _DispatchWaybillHeaderService.GetDispatchWaybillHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(p);
        }


        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _DispatchWaybillHeaderService.GetDispatchWaybillHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _DispatchWaybillHeaderService.GetDispatchWaybillHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }

        [HttpGet]
        public ActionResult NextPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _DispatchWaybillHeaderService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult PrevPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _DispatchWaybillHeaderService.PrevId(id);
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

        private void PrepareViewBag(DispatchWaybillHeaderViewModel s)
        {
            ViewBag.ShipMethodList = new ShipMethodService(_unitOfWork).GetShipMethodList().ToList();          

            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = PaymentTypeConstants.ToPay, Value = PaymentTypeConstants.ToPay });
            temp.Add(new SelectListItem { Text = PaymentTypeConstants.Paid, Value = PaymentTypeConstants.Paid });
            temp.Add(new SelectListItem { Text = PaymentTypeConstants.TobeBilled, Value = PaymentTypeConstants.TobeBilled });

            if (s == null)
                ViewBag.PaymentType = new SelectList(temp, "Value", "Text");
            else
                ViewBag.PaymentType = new SelectList(temp, "Value", "Text", s.PaymentType);
        }

        // GET: /DispatchWaybillHeader/Create

        public ActionResult Create(int DocTypeId)
        {
            DispatchWaybillHeaderViewModel p = new DispatchWaybillHeaderViewModel();
            p.CreatedDate = DateTime.Now;
            p.DocDate = DateTime.Now.Date;
            p.WaybillDate = DateTime.Now.Date;
            p.EstimatedDeliveryDate = DateTime.Now.Date;
            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            p.DocNo = _DispatchWaybillHeaderService.GetMaxDocNo();
            p.DocTypeId = DocTypeId;
            PrepareViewBag(p);
            ViewBag.Mode = "Add";
            PrepareViewBag(DocTypeId);
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DispatchWaybillHeaderViewModel svm)
        {
            string DataValidationMsg = DataValidation(svm);

            if (DataValidationMsg != "")
            {
                PrepareViewBag(svm);
                PrepareViewBag(svm.DocTypeId);
                return View(svm).Danger(DataValidationMsg);
            }

            #region DocTypeTimeLineValidation

            try
            {

                if (svm.DispatchWaybillHeaderId <= 0)
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
                if (svm.DispatchWaybillHeaderId == 0)
                {
                    DispatchWaybillHeader DispatchWaybillHeader = Mapper.Map<DispatchWaybillHeaderViewModel, DispatchWaybillHeader>(svm);
                    DispatchWaybillHeader.CreatedDate = DateTime.Now;
                    DispatchWaybillHeader.ModifiedDate = DateTime.Now;
                    DispatchWaybillHeader.CreatedBy = User.Identity.Name;
                    DispatchWaybillHeader.ModifiedBy = User.Identity.Name;
                    DispatchWaybillHeader.Status = (int)StatusConstants.Drafted;

                    if (svm.DocTypeId == new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.PreDispatchWaybill).DocumentTypeId)
                    {
                        DispatchWaybillHeader.IsPreCarriage = true;
                    }
                    else
                    {
                        DispatchWaybillHeader.IsPreCarriage = false;
                    }


                    _DispatchWaybillHeaderService.Create(DispatchWaybillHeader);


                    var routecities = new RouteLineService(_unitOfWork).GetRouteLineListForIndex(svm.RouteId).ToList();

                    foreach (var item in routecities)
                    {
                        DispatchWaybillLine DispatchWaybillLine = new DispatchWaybillLine();
                        DispatchWaybillLine.CityId = item.CityId;
                        DispatchWaybillLine.CreatedDate = DateTime.Now;
                        DispatchWaybillLine.ModifiedDate = DateTime.Now;
                        DispatchWaybillLine.CreatedBy = User.Identity.Name;
                        DispatchWaybillLine.ModifiedBy = User.Identity.Name;
                        new DispatchWaybillLineService(_unitOfWork).Create(DispatchWaybillLine);
                    }

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
                        DocTypeId = DispatchWaybillHeader.DocTypeId,
                        DocId = DispatchWaybillHeader.DispatchWaybillHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = DispatchWaybillHeader.DocNo,
                        DocDate = DispatchWaybillHeader.DocDate,
                        DocStatus = DispatchWaybillHeader.Status,
                    }));

                    return RedirectToAction("Modify", new { id = DispatchWaybillHeader.DispatchWaybillHeaderId }).Success("Data saved Successfully");
                } 
                #endregion

                #region EditRecord
                else
                {
                    int routeid = _DispatchWaybillHeaderService.GetDispatchWaybillHeaderViewModel(svm.DispatchWaybillHeaderId).RouteId;

                    DispatchWaybillHeader DispatchWaybillHeader = Mapper.Map<DispatchWaybillHeaderViewModel, DispatchWaybillHeader>(svm);

                    int status = DispatchWaybillHeader.Status;

                    if (DispatchWaybillHeader.Status != (int)StatusConstants.Drafted && DispatchWaybillHeader.Status!=(int)StatusConstants.Import)
                        DispatchWaybillHeader.Status = (int)StatusConstants.Modified;

                    DispatchWaybillHeader.Status = (int)StatusConstants.Modified;
                    DispatchWaybillHeader.ModifiedDate = DateTime.Now;
                    DispatchWaybillHeader.ModifiedBy = User.Identity.Name;
                    _DispatchWaybillHeaderService.Update(DispatchWaybillHeader);




                    if (routeid != svm.RouteId)
                    {
                        var dispatchwaybillline = new DispatchWaybillLineService(_unitOfWork).GetDispatchWaybillLineForHeaderId(svm.DispatchWaybillHeaderId);

                        foreach (var item in dispatchwaybillline)
                        {
                            new DispatchWaybillLineService(_unitOfWork).Delete(item.DispatchWaybillLineId);
                        }


                        var routecities = new RouteLineService(_unitOfWork).GetRouteLineListForIndex(svm.RouteId).ToList();

                        foreach (var item in routecities)
                        {
                            DispatchWaybillLine DispatchWaybillLine = new DispatchWaybillLine();
                            DispatchWaybillLine.DispatchWaybillHeaderId = DispatchWaybillHeader.DispatchWaybillHeaderId;
                            DispatchWaybillLine.CityId = item.CityId;
                            DispatchWaybillLine.CreatedDate = DateTime.Now;
                            DispatchWaybillLine.ModifiedDate = DateTime.Now;
                            DispatchWaybillLine.CreatedBy = User.Identity.Name;
                            DispatchWaybillLine.ModifiedBy = User.Identity.Name;
                            new DispatchWaybillLineService(_unitOfWork).Create(DispatchWaybillLine);
                        }


                    }

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        ViewBag.Mode = "Edit";
                        return View("Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = DispatchWaybillHeader.DocTypeId,
                        DocId = DispatchWaybillHeader.DispatchWaybillHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = DispatchWaybillHeader.DocNo,                        
                        DocDate = DispatchWaybillHeader.DocDate,
                        DocStatus = DispatchWaybillHeader.Status,
                    }));

                    return RedirectToAction("Index", new { id=svm.DocTypeId,IndexType=""}).Success("Data saved successfully");
                } 
                #endregion
            }
            PrepareViewBag(svm);
            PrepareViewBag(svm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", svm);
        }

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            DispatchWaybillHeader header = _DispatchWaybillHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            DispatchWaybillHeader header = _DispatchWaybillHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;            

            DispatchWaybillHeader s = _DispatchWaybillHeaderService.Find(id);

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

            DispatchWaybillHeaderViewModel svm = new DispatchWaybillHeaderViewModel();
            svm = Mapper.Map<DispatchWaybillHeader, DispatchWaybillHeaderViewModel>(s);

            if ((!TimePlanValidation && !Continue))
            {
                return RedirectToAction("DetailInformation", new { id = id, IndexType = IndexType });
            }

            PrepareViewBag(svm);
            PrepareViewBag(s.DocTypeId);
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.DispatchWaybillHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));

            return View("Create", svm);
        }


        public string DataValidation(DispatchWaybillHeaderViewModel svm)
        {
            string ValidationMsg = "";

            if (svm.ConsigneeId == 0 || svm.ConsigneeId == null)
            {
                ValidationMsg = "Consignee is required.";
                return ValidationMsg;
            }

            if (svm.SaleInvoiceHeaderId == 0 || svm.SaleInvoiceHeaderId == null)
            {
                ValidationMsg = "Invoice No is required.";
                return ValidationMsg;
            }

            if (svm.TransporterId == 0 || svm.TransporterId == null)
            {
                ValidationMsg = "Transporter is required.";
                return ValidationMsg;
            }

            if (svm.RouteId == 0 || svm.RouteId == null)
            {
                ValidationMsg = "Route is required.";
                return ValidationMsg;
            }

            return ValidationMsg;
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            DispatchWaybillHeader DispatchWaybillHeader = _DispatchWaybillHeaderService.Find(id);
            if (DispatchWaybillHeader.Status == (int)StatusConstants.Drafted || DispatchWaybillHeader.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            DispatchWaybillHeader DispatchWaybillHeader = _DispatchWaybillHeaderService.Find(id);
            if (DispatchWaybillHeader.Status == (int)StatusConstants.Submitted || DispatchWaybillHeader.Status == (int)StatusConstants.Modified)
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

            DispatchWaybillHeader DispatchWaybillHeader = _DispatchWaybillHeaderService.Find(id);
            if (DispatchWaybillHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(DispatchWaybillHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
                var DispatchWaybillHeader = _DispatchWaybillHeaderService.GetDispatchWaybillHeader(vm.id);

                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = DispatchWaybillHeader.DispatchWaybillHeaderId,
                    UserRemark = vm.Reason,
                    Narration = "Sale Order is deleted with DocNo:" + DispatchWaybillHeader.DocNo,
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.Packing).DocumentTypeId,
                    UploadDate = DateTime.Now,

                };
                _ActivityLogService.Create(al);

                var dispatchwaybillline = new DispatchWaybillLineService(_unitOfWork).GetDispatchWaybillLineForHeaderId(vm.id);

                foreach (var item in dispatchwaybillline)
                {
                    new DispatchWaybillLineService(_unitOfWork).Delete(item.DispatchWaybillLineId);
                }

                new DispatchWaybillHeaderService(_unitOfWork).Delete(vm.id);

                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vm);

                }

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }




        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            #region DocTypeTimeLineValidation

            DispatchWaybillHeader s = db.DispatchWaybillHeader.Find(id);

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
            DispatchWaybillHeader pd = new DispatchWaybillHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    pd.Status = (int)StatusConstants.Submitted;
                    pd.ReviewBy = null;
                    _DispatchWaybillHeaderService.Update(pd);

                    _unitOfWork.Save();

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.DispatchWaybillHeaderId,
                        ActivityType = pd.Status,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));                
                    //SendEmail_PODrafted(Id);

                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record Submitted successfully.");
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
            DispatchWaybillHeader pd = new DispatchWaybillHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                _DispatchWaybillHeaderService.Update(pd);
                _unitOfWork.Save();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.DispatchWaybillHeaderId,
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

        [HttpGet]
        public ActionResult DetailInformation(int id, string IndexType)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail", IndexType = IndexType });
        }


        public ActionResult Detail(int id, string transactionType, string IndexType)
        {
            DispatchWaybillHeaderViewModel H = _DispatchWaybillHeaderService.GetDispatchWaybillHeaderViewModel(id);
            DispatchWaybillHeaderViewModelWithLog Header = Mapper.Map<DispatchWaybillHeaderViewModel, DispatchWaybillHeaderViewModelWithLog>(H);

            DispatchWaybillHeaderMasterDetailModel M = new DispatchWaybillHeaderMasterDetailModel();
            M.DispatchWaybillHeaderId = id;
            M.DispatchWaybillHeaderViewModelWithLog = Header;

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = H.DocTypeId,
                    DocId = H.DispatchWaybillHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = H.DocNo,
                    DocDate = H.DocDate,
                    DocStatus = H.Status,
                }));


            return View(M);
        }


        [HttpGet]
        public ActionResult Print(int id)
        {
            String query = "Web.ProcDispatchWayBillPrint ";
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_DocumentPrint/DocumentPrint/?DocumentId=" + id + "&queryString=" + query);

        }


        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                //var Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = db.DispatchWaybillHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.DispatchWaybillHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

                        byte[] Pdf;

                        if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Import || pd.Status == (int)StatusConstants.Modified)
                        {
                            //LogAct(item.ToString());
                            Pdf = drp.DirectDocumentPrint("Web.ProcDispatchWayBillPrint ", User.Identity.Name, item);

                            PdfStream.Add(Pdf);
                        }
                        else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
                        {
                            Pdf = drp.DirectDocumentPrint("Web.ProcDispatchWayBillPrint ", User.Identity.Name, item);

                            PdfStream.Add(Pdf);
                        }
                        else
                        {
                            Pdf = drp.DirectDocumentPrint("Web.ProcDispatchWayBillPrint ", User.Identity.Name, item);
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



        public int PendingToSubmitCount(int id)
        {
            return (_DispatchWaybillHeaderService.GetDispatchWaybillHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_DispatchWaybillHeaderService.GetDispatchWaybillHeaderListPendingToReview(id, User.Identity.Name)).Count();
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
