using System;
using System.Collections.Generic;
using System.Data;
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
using System.Text;
using System.Data.SqlClient;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;


namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class PackingHeaderWithBuyerDetailController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IPackingHeaderService _PackingHeaderService;
        IActivityLogService _ActivityLogService;
        IStockService _StockService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public PackingHeaderWithBuyerDetailController(IPackingHeaderService PackingHeaderService, IActivityLogService ActivityLogService, IStockService StockService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PackingHeaderService = PackingHeaderService;
            _ActivityLogService = ActivityLogService;
            _StockService = StockService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }


        public ActionResult Index(string IndexType)
        {
            int PackingDocumenttType = 242;
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit");
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview");
            }
            IQueryable<PackingHeaderViewModel> temp = _PackingHeaderService.GetPackingHeaderList(User.Identity.Name);

            IQueryable<PackingHeaderViewModel> p = from L in temp
                                                   where L.DocTypeId == PackingDocumenttType
                                                   select L;
                                                   
            
            ViewBag.id = PackingDocumenttType;
            ViewBag.PendingToSubmit = PendingToSubmitCount();
            ViewBag.PendingToReview = PendingToReviewCount();
            ViewBag.IndexStatus = "All";
            return View(p);
        }


        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _PackingHeaderService.GetPackingHeaderListPendingToSubmit(User.Identity.Name);

            int PackingDocumenttType = 242;
            ViewBag.id = PackingDocumenttType;
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(PackingDocumenttType).DocumentTypeName;
            ViewBag.PendingToSubmit = PendingToSubmitCount();
            ViewBag.PendingToReview = PendingToReviewCount();
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _PackingHeaderService.GetPackingHeaderListPendingToReview(User.Identity.Name);
            int PackingDocumenttType = 242;
            ViewBag.id = PackingDocumenttType;
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(PackingDocumenttType).DocumentTypeName;
            ViewBag.PendingToSubmit = PendingToSubmitCount();
            ViewBag.PendingToReview = PendingToReviewCount();
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }


        [HttpGet]
        public ActionResult NextPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _PackingHeaderService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult PrevPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _PackingHeaderService.PrevId(id);
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


        private void PrepareViewBag(PackingHeaderViewModel s)
        {
            //ViewBag.DocTypeList = new DocumentTypeService(_unitOfWork).GetDocumentTypeList().ToList();
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            ViewBag.DocTypeList = new DocumentTypeService(_unitOfWork).GetDocumentTypeList(TransactionDocCategoryConstants.PackingReceive);
            ViewBag.BuyerList = new BuyerService(_unitOfWork).GetBuyerList().ToList();
            ViewBag.GodownList = new GodownService(_unitOfWork).GetGodownList(SiteId).ToList().Where(i => i.SiteId == (int)System.Web.HttpContext.Current.Session["SiteId"] && i.IsActive == true);
            ViewBag.DealUnitList = new UnitService(_unitOfWork).GetUnitList().Where(m => m.DimensionUnitId != null).ToList();
            ViewBag.ShipMethodList = new ShipMethodService(_unitOfWork).GetShipMethodList().ToList();

            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = "Product Invoice Group", Value = ((int)(BaleNoPatternConstants.ProductInvoiceGroup)).ToString() });
            temp.Add(new SelectListItem { Text = "Sale Order", Value = ((int)(BaleNoPatternConstants.SaleOrder)).ToString() });
            temp.Add(new SelectListItem { Text = "Size Wise", Value = ((int)(BaleNoPatternConstants.SmallSizes)).ToString() });
            temp.Add(new SelectListItem { Text = "Sale Order Size Wise", Value = ((int)(BaleNoPatternConstants.SaleOrderSize)).ToString() });

            if (s == null)
                ViewBag.BaleNoPattern = new SelectList(temp, "Value", "Text");
            else
                ViewBag.BaleNoPattern = new SelectList(temp, "Value", "Text", s.BaleNoPattern);


        }

        // GET: /PackingHeader/Create

        public ActionResult Create()
        {
            PackingHeaderViewModel p = new PackingHeaderViewModel();
            p.DocDate = DateTime.Now.Date;
            p.CreatedDate = DateTime.Now;
            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            p.DocNo = _PackingHeaderService.GetMaxDocNo();
            p.ShipMethodId = new ShipMethodService(_unitOfWork).Find("By Sea").ShipMethodId;

            DocumentType DT = new DocumentTypeService(_unitOfWork).FindByName("Packing Receive");
            PackingSetting temp = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(DT.DocumentTypeId, p.DivisionId, p.SiteId);

            p.PackingSettings = Mapper.Map<PackingSetting, PackingSettingsViewModel>(temp);

            if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
                p.GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];

            PrepareViewBag(p);
            ViewBag.Mode = "Add";
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PackingHeaderViewModel svm)
        {
            string DataValidationMsg = DataValidation(svm);
            if (DataValidationMsg != "")
            {
                PrepareViewBag(svm);
                return View(svm).Danger(DataValidationMsg);
            }


            #region DocTypeTimeLineValidation

            try
            {

                if (svm.PackingHeaderId <= 0)
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
                if (svm.PackingHeaderId == 0)
                {
                    PackingHeader packingheader = Mapper.Map<PackingHeaderViewModel, PackingHeader>(svm);
                    packingheader.CreatedDate = DateTime.Now;
                    packingheader.ModifiedDate = DateTime.Now;
                    packingheader.CreatedBy = User.Identity.Name;
                    packingheader.ModifiedBy = User.Identity.Name;
                    packingheader.Status = (int)StatusConstants.Drafted;
                    _PackingHeaderService.Create(packingheader);

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
                        DocTypeId = packingheader.DocTypeId,
                        DocId = packingheader.PackingHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = packingheader.DocNo,
                        DocDate = packingheader.DocDate,
                        DocStatus = packingheader.Status,
                    }));

                    return RedirectToAction("Modify", new { id = packingheader.PackingHeaderId }).Success("Data saved Successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PackingHeader packingheader = new PackingHeaderService(_unitOfWork).Find(svm.PackingHeaderId);

                    PackingHeader ExRec = new PackingHeader();
                    ExRec = Mapper.Map<PackingHeader>(packingheader);


                    StringBuilder logstring = new StringBuilder();

                    int status = packingheader.Status;

                    if (packingheader.Status != (int)StatusConstants.Drafted)
                    {
                        packingheader.Status = (int)StatusConstants.Modified;
                    }


                    packingheader.DocNo = svm.DocNo;
                    packingheader.DocDate = svm.DocDate;
                    packingheader.BuyerId = svm.BuyerId;
                    packingheader.GodownId = svm.GodownId;
                    packingheader.DealUnitId = svm.DealUnitId;
                    packingheader.Remark = svm.Remark;
                    packingheader.JobWorkerId = svm.JobWorkerId;
                    packingheader.BaleNoPattern = svm.BaleNoPattern;
                    packingheader.ShipMethodId = svm.ShipMethodId;
                    packingheader.ModifiedDate = DateTime.Now;
                    packingheader.ModifiedBy = User.Identity.Name;
                    _PackingHeaderService.Update(packingheader);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = packingheader,
                    });


                    if (packingheader.StockHeaderId != null)
                    {
                        StockHeaderViewModel S = new StockHeaderViewModel();

                        S.StockHeaderId = (int)packingheader.StockHeaderId;
                        S.DocTypeId = packingheader.DocTypeId;
                        S.DocDate = packingheader.DocDate;
                        S.DocNo = packingheader.DocNo;
                        S.DivisionId = packingheader.DivisionId;
                        S.SiteId = packingheader.SiteId;
                        S.PersonId = packingheader.JobWorkerId;
                        S.GodownId = packingheader.GodownId;
                        S.Remark = packingheader.Remark;
                        S.Status = packingheader.Status;
                        S.ModifiedBy = packingheader.ModifiedBy;
                        S.ModifiedDate = packingheader.ModifiedDate;

                        new StockHeaderService(_unitOfWork).UpdateStockHeader(S);
                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
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
                        DocTypeId = packingheader.DocTypeId,
                        DocId = packingheader.PackingHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = packingheader.DocNo,
                        xEModifications = Modifications,
                        DocDate = packingheader.DocDate,
                        DocStatus = packingheader.Status,
                    }));

                    return RedirectToAction("Index").Success("Data saved successfully");
                }
                #endregion
            }
            PrepareViewBag(svm);
            ViewBag.Mode = "Add";
            return View("Create", svm);
        }


        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;

            PackingHeaderViewModel svm = _PackingHeaderService.GetPackingHeaderViewModel(id);

            if (svm == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

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

            if ((!TimePlanValidation && !Continue))
            {
                return RedirectToAction("DetailInformation", new { id = id, IndexType = IndexType });
            }
            PrepareViewBag(svm);
            ViewBag.Mode = "Edit";


            PackingSetting temp = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(svm.DocTypeId, svm.DivisionId, svm.SiteId);

            svm.PackingSettings = Mapper.Map<PackingSetting, PackingSettingsViewModel>(temp);

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = svm.DocTypeId,
                    DocId = svm.PackingHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = svm.DocNo,
                    DocDate = svm.DocDate,
                    DocStatus = svm.Status,
                }));

            return View("Create", svm);
        }

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Approve(int id, string IndexType)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }


        public string DataValidation(PackingHeaderViewModel svm)
        {
            string ValidationMsg = "";

            if (svm.BuyerId == 0 || svm.BuyerId == null)
            {
                ValidationMsg = "Buyer is required.";
                return ValidationMsg;
            }

            if (svm.GodownId == 0 || svm.GodownId == null)
            {
                ValidationMsg = "Godown is required.";
                return ValidationMsg;
            }

            if (svm.DealUnitId == "" || svm.DealUnitId == null)
            {
                ValidationMsg = "Deal Unit is required.";
                return ValidationMsg;
            }

            //PackingHeader packingheader = _PackingHeaderService.FindByDocNo(svm.DocNo, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

            //int  DivisionId =  (int)System.Web.HttpContext.Current.Session["DivisionId"];
            //int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //var packingheader = (from H in db.PackingHeader
            //                               where H.DivisionId == DivisionId &&
            //                               H.SiteId == SiteId &&
            //                               H.DocNo == svm.DocNo &&
            //                               H.PackingHeaderId != svm.PackingHeaderId
            //                               select new 
            //                               {
            //                                   PackingHeaderId = H.PackingHeaderId
            //                               }).FirstOrDefault();

            //if (packingheader != null)
            //{
            //    ValidationMsg = "Doc no already exists.";
            //}

            return ValidationMsg;
        }



        //public ActionResult Detail(int id, string returl, string transactionType)
        //{
        //    PackingHeaderViewModel H = _PackingHeaderService.GetPackingHeaderViewModel(id);
        //    PackingHeaderViewModelWithLog Header = Mapper.Map<PackingHeaderViewModel, PackingHeaderViewModelWithLog>(H);
        //    List<PackingLineViewModel> L = new PackingLineService(_unitOfWork).GetPackingLineViewModelForHeaderId(id).ToList();

        //    PackingMasterDetailModel M = new PackingMasterDetailModel();
        //    M.PackingHeaderId = id;
        //    M.PackingHeaderViewModelWithLog = Header;
        //    M.PackingLineViewModel = L;


        //    var UManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        //    var UserRoles = UManager.GetRoles(User.Identity.GetUserId());
        //    if (UserRoles.Contains("manager"))
        //    {
        //        ViewBag.UserRole = "manager";
        //    }
        //    else if (UserRoles.Contains("supplier"))
        //    {
        //        ViewBag.UserRole = "supplier";
        //    }
        //    ViewBag.transactionType = transactionType;
        //    return View(M);
        //}

        [HttpGet]
        public ActionResult DetailInformation(int id, string IndexType)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail", IndexType = IndexType });
        }


        public ActionResult Detail(int id, string transactionType, string IndexType)
        {
            PackingHeaderViewModel svm = _PackingHeaderService.GetPackingHeaderViewModel(id);

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;
            PrepareViewBag(svm);
            if (svm == null)
            {
                return HttpNotFound();
            }

            PackingSetting temp = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(svm.DocTypeId, svm.DivisionId, svm.SiteId);

            svm.PackingSettings = Mapper.Map<PackingSetting, PackingSettingsViewModel>(temp);

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = svm.DocTypeId,
                    DocId = svm.PackingHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = svm.DocNo,
                    DocDate = svm.DocDate,
                    DocStatus = svm.Status,
                }));

            return View("Create", svm);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
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
            PackingHeader PackingHeader = new PackingHeaderService(_unitOfWork).Find(id);

            if (PackingHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(PackingHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReasonViewModel vm)
        {
            if (ModelState.IsValid)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                int? StockHeaderId = 0;
                var PackingHeader = _PackingHeaderService.GetPackingHeader(vm.id);
                int status = PackingHeader.Status;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = PackingHeader,
                });

                StockHeaderId = PackingHeader.StockHeaderId;

                new StockUidService(_unitOfWork).DeleteStockUidForDocHeader(PackingHeader.PackingHeaderId, PackingHeader.DocTypeId, PackingHeader.SiteId, PackingHeader.DivisionId);

                List<int> StockIssueIdList = new List<int>();
                List<int> StockReceiveIdList = new List<int>();


                var PackingLine = new PackingLineService(_unitOfWork).GetPackingLineForHeaderId(vm.id).ToList();

                foreach (var item in PackingLine)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });

                    if (item.StockIssueId != null)
                    {
                        StockIssueIdList.Add((int)item.StockIssueId);
                    }

                    if (item.StockReceiveId != null)
                    {
                        StockReceiveIdList.Add((int)item.StockReceiveId);
                    }


                    if (item.ProductUidId != null && item.ProductUidId != 0)
                    {
                        ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues((int)item.ProductUidId, "Packing-" + vm.id.ToString());

                        ProductUid ProductUid = new ProductUidService(_unitOfWork).Find((int)item.ProductUidId);


                        ProductUid.LastTransactionDocDate = item.ProductUidLastTransactionDocDate;
                        ProductUid.LastTransactionDocId = item.ProductUidLastTransactionDocId;
                        ProductUid.LastTransactionDocNo = item.ProductUidLastTransactionDocNo;
                        ProductUid.LastTransactionDocTypeId = item.ProductUidLastTransactionDocTypeId;
                        ProductUid.LastTransactionPersonId = item.ProductUidLastTransactionPersonId;
                        ProductUid.CurrenctGodownId = item.ProductUidCurrentGodownId;
                        ProductUid.CurrenctProcessId = item.ProductUidCurrentProcessId;
                        ProductUid.Status = item.ProductUidStatus;

                        //ProductUid.LastTransactionDocDate = ProductUidDetail.LastTransactionDocDate;
                        //ProductUid.LastTransactionDocId = ProductUidDetail.LastTransactionDocId;
                        //ProductUid.LastTransactionDocNo = ProductUidDetail.LastTransactionDocNo;
                        //ProductUid.LastTransactionDocTypeId = ProductUidDetail.LastTransactionDocTypeId;
                        //ProductUid.LastTransactionPersonId = ProductUidDetail.LastTransactionPersonId;
                        //ProductUid.CurrenctGodownId = ProductUidDetail.CurrenctGodownId;
                        //ProductUid.CurrenctProcessId = ProductUidDetail.CurrenctProcessId;

                        new ProductUidService(_unitOfWork).Update(ProductUid);
                    }

                    PackingLineExtended LineExtended = new PackingLineExtendedService(_unitOfWork).Find(item.PackingLineId);
                    if (LineExtended != null)
                    {
                        new PackingLineExtendedService(_unitOfWork).Delete(LineExtended);
                    }
                    new PackingLineService(_unitOfWork).Delete(item.PackingLineId);
                }


                foreach (var item in StockIssueIdList)
                {
                    StockAdj Adj = (from L in db.StockAdj
                                    where L.StockOutId == item
                                    select L).FirstOrDefault();

                    if (Adj != null)
                    {
                        //Adj.ObjectState = Model.ObjectState.Deleted;
                        //db.StockAdj.Remove(Adj);
                        new StockAdjService(_unitOfWork).Delete(Adj);
                    }

                    new StockService(_unitOfWork).DeleteStock(item);
                }

                foreach (var item in StockReceiveIdList)
                {
                    new StockService(_unitOfWork).DeleteStock(item);
                }

                new PackingHeaderService(_unitOfWork).Delete(vm.id);


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

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = PackingHeader.DocTypeId,
                    DocId = PackingHeader.PackingHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = PackingHeader.DocNo,
                    xEModifications = Modifications,
                    DocDate = PackingHeader.DocDate,
                    DocStatus = PackingHeader.Status,
                }));

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }








        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            #region DocTypeTimeLineValidation

            PackingHeader s = db.PackingHeader.Find(id);

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
            PackingHeader pd = new PackingHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {


                    #region Negatvie Sale Order Packing Validation

                    Decimal PendingOrderQtyForPacking = 0;
                    IEnumerable<PackingLineViewModel> PackingLine = new PackingLineService(_unitOfWork).GetPackingLineViewModelForHeaderId(Id);
                    string ValidationMsg = "";

                    foreach(PackingLineViewModel Line in PackingLine)
                    {
                        PendingOrderQtyForPacking = new PackingLineService(_unitOfWork).FGetPendingOrderQtyForPacking((int)Line.SaleOrderLineId, 0);
                        if (PendingOrderQtyForPacking < 0)
                        {
                            ValidationMsg = "Balance Qty For Product : " + Line.ProductName + " And Sale Order : " + Line.SaleOrderNo + " is going negative. Can't Submit Record.";
                            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning(ValidationMsg);
                        }

                        if (new PackingLineService(_unitOfWork).FSaleOrderProductMatchWithPacking((int)Line.SaleOrderLineId, Line.ProductId) == false)
                        {
                            ValidationMsg = "Product : " + Line.ProductName + " does not exist in Sale Order : " + Line.SaleOrderNo + " . Can't Submit Record.";
                            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning(ValidationMsg);
                        }
                    }

                    #endregion


                    pd.Status = (int)StatusConstants.Submitted;
                    
                    //pd.ReviewBy = User.Identity.Name;
                    //pd.ReviewCount = 1;

                    _PackingHeaderService.Update(pd);
                    _unitOfWork.Save();

                    //string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

                    //DataSet ds = new DataSet();
                    //using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                    //{
                    //    sqlConnection.Open();
                    //    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_UpdatePackingArea"))
                    //    {
                    //        cmd.CommandType = CommandType.StoredProcedure;
                    //        cmd.Connection = sqlConnection;
                    //        cmd.Parameters.AddWithValue("@PackingHeaderId", pd.PackingHeaderId);
                    //        cmd.Parameters.AddWithValue("@DealUnitId", pd.DealUnitId);
                    //        cmd.CommandTimeout = 1000;
                    //        cmd.ExecuteNonQuery();
                    //        cmd.Connection.Close();
                    //    }
                    //}

                    //SendEmail_PODrafted(Id);

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.PackingHeaderId,
                        ActivityType = (int)ActivityTypeContants.Submitted,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    Reviewed(Id, IndexType, UserRemark, IsContinue);

                    return RedirectToAction("Index", new { IndexType = IndexType }).Success("Record submitted successfully.");
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
            PackingHeader pd = new PackingHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {
                if (pd.Status != (int)StatusConstants.Approved)
                    pd.Status = (int)StatusConstants.Approved;

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                _PackingHeaderService.Update(pd);
                _unitOfWork.Save();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.PackingHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));

                return RedirectToAction("Index", new { IndexType = IndexType }).Success("Record reviewed successfully.");
            }

            return RedirectToAction("Index", new { IndexType = IndexType }).Warning("Error in reviewing.");
        }


        public int PendingToSubmitCount()
        {
            return (_PackingHeaderService.GetPackingHeaderListPendingToSubmit(User.Identity.Name)).Count();
        }

        public int PendingToReviewCount()
        {
            return (_PackingHeaderService.GetPackingHeaderListPendingToReview(User.Identity.Name)).Count();
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

        public JsonResult FIsDuplicateDocNo(string DocNo)
        {
            Boolean IsDuplicate;
            PackingHeader H = _PackingHeaderService.FindByDocNo(DocNo, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);
            if (H == null)
            {
                IsDuplicate = false;
            }
            else
            {
                IsDuplicate = true;
            }
            return Json(new { returnvalue = IsDuplicate });
        }




        public ActionResult PackingSegmentation()
        {
            ViewBag.PackingNoList = _PackingHeaderService.GetPackingHeaderList(User.Identity.Name);
            return View();
        }


        [HttpPost]
        public ActionResult PackingSegmentation(PackingSegmentationViewModel p)
        {
            int frombaleno = 0;
            int tobaleno = 0;
            string ValidationMsg = "";

            PackingHeader frompackingheader = new PackingHeaderService(_unitOfWork).Find(p.FromPackingHeaderId);
            PackingHeader topackingheader = new PackingHeaderService(_unitOfWork).Find(p.ToPackingHeaderId);

            if (frompackingheader.BuyerId != topackingheader.BuyerId)
            {
                ViewBag.PackingNoList = _PackingHeaderService.GetPackingHeaderList(User.Identity.Name);
                return View(p).Danger("Buyer mismatch found in selected packing nos.");
            }



            var packinglinelist = (from L in db.PackingLine where L.PackingHeaderId == p.ToPackingHeaderId select new { L }).ToList();


            var BaleNoCntInEntry = (from L in packinglinelist
                                    where L.L.PackingHeaderId == p.ToPackingHeaderId && (int.TryParse(L.L.BaleNo, out frombaleno) ? frombaleno : 0) >= p.FromBaleNo && (int.TryParse(L.L.BaleNo, out tobaleno) ? tobaleno : 0) <= p.ToBaleNo
                                    group new { L } by new { L.L.BaleNo } into Result
                                    where Result.Count() > 1
                                    select new
                                    {
                                        BaleNo = Result.Key,
                                        Cnt = Result.Count()
                                    }).ToList();

            if (BaleNoCntInEntry != null)
            {
                foreach (var item in BaleNoCntInEntry)
                {
                    ValidationMsg = ValidationMsg + "Bale No " + item.BaleNo + "already exists in " + p.ToPackingNo;
                }
                if (ValidationMsg != "")
                {
                    ViewBag.PackingNoList = _PackingHeaderService.GetPackingHeaderList(User.Identity.Name);
                    return View(p).Danger(ValidationMsg);
                }

            }

            List<PackingLine> packingline = new PackingLineService(_unitOfWork).GetPackingLineForHeaderId(p.FromPackingHeaderId).ToList().Where(i => (int.TryParse(i.BaleNo, out frombaleno) ? frombaleno : 0) >= p.FromBaleNo && (int.TryParse(i.BaleNo, out tobaleno) ? tobaleno : 0) <= p.ToBaleNo).ToList();

            foreach (PackingLine item in packingline)
            {
                //new PackingLineService(_unitOfWork).Delete(item);
                //new StockService(_unitOfWork).DeleteForDocLineId(item.PackingLineId);

                //item.PackingHeaderId = topackingheader.PackingHeaderId;

                //new PackingLineService(_unitOfWork).Create(item);
                //StockPost(topackingheader, item);

                item.PackingHeaderId = p.ToPackingHeaderId;
                new PackingLineService(_unitOfWork).Update(item);

            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Save();
            }

            return RedirectToAction("Detail", new { id = p.ToPackingHeaderId, transactionType = "submit" });
        }

        public ActionResult Print(int id, string ReportFileType)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return RedirectToAction("DocPrint", new { id = id, ReportFileType = ReportFileType });
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult PrintAfter_Submit(int id, string ReportFileType)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return RedirectToAction("DocPrint", new { id = id, ReportFileType = ReportFileType });
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult PrintAfter_Approve(int id, string ReportFileType)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return RedirectToAction("DocPrint", new { id = id, ReportFileType = ReportFileType });
            else
                return HttpNotFound();
        }

        public ActionResult DocPrint(int id, string ReportFileType)
        {
            ApplicationDbContext Db = new ApplicationDbContext();

            String query = "";
            if (ReportFileType == "Excel")
                //query = Db.strSchemaName + ".ProcPackingPrintToExcel " + id.ToString();
                query = Db.strSchemaName + ".ProcPackingPrintToExcel ";
            else
                // query = Db.strSchemaName + ".ProcPackingPrint " + id.ToString();
                query = Db.strSchemaName + ".ProcPackingPrint ";

            String ReportTitle = "Carpet Receive From Packing";
            return RedirectToAction("DocumentPrint", "Report_DocumentPrint", new { queryString = query, DocumentId = id, ReportTitle = ReportTitle, ReportFileType = ReportFileType });
        }


        public ActionResult Import(int id)//Document Type Id
        {
            int DivisionId = 0;
            int SiteId = 0;


            DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(id, DivisionId, SiteId);

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

    }
}