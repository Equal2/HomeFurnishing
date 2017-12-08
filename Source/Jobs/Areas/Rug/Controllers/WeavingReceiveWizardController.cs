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



namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class WeavingReceiveWizardController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobReceiveHeaderService _JobReceiveHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public WeavingReceiveWizardController(IJobReceiveHeaderService JobReceiveHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobReceiveHeaderService = JobReceiveHeaderService;
            _exception = exec;
            _unitOfWork = unitOfWork;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult CreateWeavingReceive(int id)//DocTypeId
        {

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            ViewBag.id = id;

            int DocTypeId = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeId;

            //Getting Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(DocTypeId, DivisionId, SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveSettings", new { id = DocTypeId }).Warning("Please create settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            return View("CreateWeavingReceive");
        }

        public JsonResult PendingJobOrders(int id)//DocTypeId
        {

            var temp = _JobReceiveHeaderService.GetJobOrdersForWeavingReceiveWizard(id).ToList();

            return Json(new { data = temp }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ConfirmJobOrderList(List<WeavingReceiveWizardViewModel> Selected)
        {
            System.Web.HttpContext.Current.Session["BarCodesWeavingWizardJobOrder"] = Selected;

            return Json(new { Success = "URL", Data = "/WeavingReceiveWizard/Create" }, JsonRequestBehavior.AllowGet);
        }

        [Serializable]
        public class SelectedJobOrders
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
        }




        // GET: /JobReceiveHeader/Create

        public ActionResult Create()//DocumentTypeId
        {
            JobReceiveHeaderViewModel p = new JobReceiveHeaderViewModel();

            p.DocDate = DateTime.Now;
            p.CreatedDate = DateTime.Now;

            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            int DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.WeavingBazar).DocumentTypeId;
            p.DocTypeId = DocTypeId;

            //Getting Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(DocTypeId, p.DivisionId, p.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveSettings", new { id = DocTypeId }).Warning("Please create job order settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            p.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);

            if (System.Web.HttpContext.Current.Session["BarCodesWeavingWizardJobOrder"] == null)
                return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/Index/" + DocTypeId);

            var JobOrderLin = ((List<WeavingReceiveWizardViewModel>)System.Web.HttpContext.Current.Session["BarCodesWeavingWizardJobOrder"]).FirstOrDefault();


            //p.JobReceiveById = new EmployeeService(_unitOfWork).GetEmloyeeForUser(User.Identity.GetUserId());
            p.ProcessId = settings.ProcessId;
            if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
            {
                p.GodownId = (int) System.Web.HttpContext.Current.Session["DefaultGodownId"];
            }

            PrepareViewBag();
            p.DocTypeId = DocTypeId;
            p.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveHeaders", p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);
            List<WeavingReceiveWizardViewModel> JobOrdersAndQtys = (List<WeavingReceiveWizardViewModel>)System.Web.HttpContext.Current.Session["BarCodesWeavingWizardJobOrder"];
            p.JobWorkerId = JobOrdersAndQtys.FirstOrDefault().JobWorkerId;
            

            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobReceiveHeaderViewModel svm)
        {
            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            bool Continue = true;

            JobReceiveHeader s = Mapper.Map<JobReceiveHeaderViewModel, JobReceiveHeader>(svm);
            List<WeavingReceiveWizardViewModel> JobOrdersAndQtys = (List<WeavingReceiveWizardViewModel>)System.Web.HttpContext.Current.Session["BarCodesWeavingWizardJobOrder"];
            

          

            if (JobOrdersAndQtys.Count() <= 0)
                ModelState.AddModelError("", "No Records Selected");

            int JobWorkerCnt = (from l in JobOrdersAndQtys
                                group l by l.JobWorkerId into g
                                select new
                                {
                                    JobWorkerId = g.Key,
                                }).Distinct().Count();

            if (JobWorkerCnt > 1)
                ModelState.AddModelError("", "Select any one Job Worker Orders.");




            s.JobWorkerId = JobOrdersAndQtys.FirstOrDefault().JobWorkerId;
            svm.JobWorkerId = JobOrdersAndQtys.FirstOrDefault().JobWorkerId;


            #region DocTypeTimeLineValidation

            try
            {

                if (svm.JobReceiveHeaderId <= 0)
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

                if (svm.JobReceiveHeaderId <= 0)
                {
                    if (JobOrdersAndQtys.Count() > 0)
                    {
                        s.CreatedDate = DateTime.Now;
                        s.ModifiedDate = DateTime.Now;
                        s.CreatedBy = User.Identity.Name;
                        s.ModifiedBy = User.Identity.Name;
                        s.Status = (int)StatusConstants.Drafted;
                        _JobReceiveHeaderService.Create(s);


                        int Cnt = 0;
                        int Sr = 0;


                        JobReceiveSettings Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

                        int ProductUidCountForJobOrderLine = 0;
                        int pk = 0;

                        var JobOrderLineIds = JobOrdersAndQtys.Select(m => m.JobOrderLineId).ToArray();

                        var BalQtyandUnits = (from p in db.ViewJobOrderBalance
                                              join t in db.Product on p.ProductId equals t.ProductId
                                              where JobOrderLineIds.Contains(p.JobOrderLineId)
                                              select new
                                              {
                                                  BalQty = p.BalanceQty,
                                                  JobOrderLineId = p.JobOrderLineId,
                                                  UnitId = t.UnitId,
                                              }).ToList();

                        if (ModelState.IsValid)
                        {
                            foreach (var SelectedJobOrderLine in JobOrdersAndQtys)
                            {
                                if (SelectedJobOrderLine.JobOrderLineId > 0)
                                {
                                    if (SelectedJobOrderLine.ToProductUidName != "" && SelectedJobOrderLine.ToProductUidName != null && SelectedJobOrderLine.FromProductUidName != "" && SelectedJobOrderLine.FromProductUidName != null)
                                    {
                                        if (SelectedJobOrderLine.Qty != (Convert.ToInt32(SelectedJobOrderLine.ToProductUidName) - Convert.ToInt32(SelectedJobOrderLine.FromProductUidName) + 1))
                                        {
                                            string Msg = "";
                                            Msg = "Qty and Barcode series does not match.";
                                            ModelState.AddModelError("", Msg);
                                            PrepareViewBag();
                                            ViewBag.Mode = "Add";
                                            return View("Create", svm);
                                        }
                                    }



                                    ProductUidCountForJobOrderLine = 0;
                                    var JobOrderLine = new JobOrderLineService(_unitOfWork).Find((SelectedJobOrderLine.JobOrderLineId));
                                    var Product = new ProductService(_unitOfWork).Find(JobOrderLine.ProductId);

                                    var bal = BalQtyandUnits.Where(m => m.JobOrderLineId == SelectedJobOrderLine.JobOrderLineId).FirstOrDefault();

                                    if (SelectedJobOrderLine.Qty <= bal.BalQty)
                                    {
                                        for (int i = 0; i <= SelectedJobOrderLine.Qty - 1;i ++ )
                                        {
                                            int? ProductUidHeaderId = null;
                                            int? ProductUidId = null;


                                            var SisterSite = (from S in db.Site where S.PersonId == s.JobWorkerId select S).FirstOrDefault();

                                            //if (!string.IsNullOrEmpty(Settings.SqlProcGenProductUID))
                                            if (SisterSite == null)
                                            {
                                                ProductUidHeader ProdUidHeader = new ProductUidHeader();

                                                ProdUidHeader.ProductUidHeaderId = Cnt;
                                                ProdUidHeader.ProductId = JobOrderLine.ProductId;
                                                ProdUidHeader.Dimension1Id = JobOrderLine.Dimension1Id;
                                                ProdUidHeader.Dimension2Id = JobOrderLine.Dimension2Id;
                                                ProdUidHeader.GenDocId = s.JobReceiveHeaderId;
                                                ProdUidHeader.GenDocNo = s.DocNo;
                                                ProdUidHeader.GenDocTypeId = s.DocTypeId;
                                                ProdUidHeader.GenDocDate = s.DocDate;
                                                ProdUidHeader.GenPersonId = s.JobWorkerId;
                                                ProdUidHeader.CreatedBy = User.Identity.Name;
                                                ProdUidHeader.CreatedDate = DateTime.Now;
                                                ProdUidHeader.ModifiedBy = User.Identity.Name;
                                                ProdUidHeader.ModifiedDate = DateTime.Now;
                                                ProdUidHeader.ObjectState = Model.ObjectState.Added;
                                                new ProductUidHeaderService(_unitOfWork).Create(ProdUidHeader);
                                                ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;


                                                string ProductUidName = (Convert.ToInt32(SelectedJobOrderLine.FromProductUidName) + ProductUidCountForJobOrderLine).ToString();

                                                ProductUid ProdUid = new ProductUid();
                                                ProdUid.ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;
                                                ProdUid.ProductUidName = ProductUidName;
                                                ProdUid.ProductId = JobOrderLine.ProductId;
                                                ProdUid.IsActive = true;
                                                ProdUid.CreatedBy = User.Identity.Name;
                                                ProdUid.CreatedDate = DateTime.Now;
                                                ProdUid.ModifiedBy = User.Identity.Name;
                                                ProdUid.ModifiedDate = DateTime.Now;
                                                ProdUid.GenLineId = null;
                                                ProdUid.GenDocId = s.JobReceiveHeaderId;
                                                ProdUid.GenDocNo = s.DocNo;
                                                ProdUid.GenDocTypeId = s.DocTypeId;
                                                ProdUid.GenDocDate = s.DocDate;
                                                ProdUid.GenPersonId = s.JobWorkerId;
                                                ProdUid.Dimension1Id = JobOrderLine.Dimension1Id;
                                                ProdUid.Dimension2Id = JobOrderLine.Dimension2Id;
                                                ProdUid.CurrenctProcessId = s.ProcessId;
                                                ProdUid.CurrenctGodownId = s.GodownId;
                                                ProdUid.Status = "Receive";
                                                ProdUid.LastTransactionDocId = s.JobReceiveHeaderId;
                                                ProdUid.LastTransactionDocNo = s.DocNo;
                                                ProdUid.LastTransactionDocTypeId = s.DocTypeId;
                                                ProdUid.LastTransactionDocDate = s.DocDate;
                                                ProdUid.LastTransactionPersonId = s.JobWorkerId;
                                                ProdUid.LastTransactionLineId = null;
                                                ProdUid.ProductUIDId = pk;
                                                new ProductUidService(_unitOfWork).Create(ProdUid);
                                                ProductUidId = ProdUid.ProductUIDId;
                                            }

                                            if (ProductUidId == null)
                                            {
                                                string ProductUidName = (Convert.ToInt32(SelectedJobOrderLine.FromProductUidName) + ProductUidCountForJobOrderLine).ToString();
                                                var temp = new ProductUidService(_unitOfWork).Find(ProductUidName);
                                                if (temp != null)
                                                {
                                                    ProductUidId = temp.ProductUIDId;
                                                }
                                                else
                                                {
                                                    string Msg = ProductUidName + " is not a valid barcode.";
                                                    ModelState.AddModelError("", Msg);
                                                    PrepareViewBag();
                                                    ViewBag.Mode = "Add";
                                                    return View("Create", svm);
                                                }

                                                if (temp.CurrenctGodownId != null)
                                                {
                                                    string Msg = ProductUidName + " is already in Stock at Godown " + new GodownService(_unitOfWork).Find(temp.CurrenctGodownId ?? 0).GodownName;
                                                    ModelState.AddModelError("", Msg);
                                                    PrepareViewBag();
                                                    ViewBag.Mode = "Add";
                                                    return View("Create", svm);
                                                    
                                                }

                                                if (temp.LastTransactionPersonId != s.JobWorkerId)
                                                {
                                                    string Msg = ProductUidName + ProductUidName + " does not belong to this Job Worker";
                                                    ModelState.AddModelError("", Msg);
                                                    PrepareViewBag();
                                                    ViewBag.Mode = "Add";
                                                    return View("Create", svm);

                                                }
                                            }


                                            StockViewModel StockViewModel = new StockViewModel();
                                            if (Cnt == 0)
                                            {
                                                StockViewModel.StockHeaderId = s.StockHeaderId ?? 0;
                                            }
                                            else
                                            {
                                                if (s.StockHeaderId != null && s.StockHeaderId != 0)
                                                {
                                                    StockViewModel.StockHeaderId = (int)s.StockHeaderId;
                                                }
                                                else
                                                {
                                                    StockViewModel.StockHeaderId = -1;
                                                }
                                            }

                                            StockViewModel.StockId = -Cnt;
                                            StockViewModel.DocHeaderId = s.JobReceiveHeaderId;
                                            StockViewModel.DocLineId = null;
                                            StockViewModel.DocTypeId = s.DocTypeId;
                                            StockViewModel.StockHeaderDocDate = s.DocDate;
                                            StockViewModel.StockDocDate = s.DocDate;
                                            StockViewModel.DocNo = s.DocNo;
                                            StockViewModel.DivisionId = s.DivisionId;
                                            StockViewModel.SiteId = s.SiteId;
                                            StockViewModel.CurrencyId = null;
                                            StockViewModel.PersonId = s.JobWorkerId;
                                            StockViewModel.ProductId = JobOrderLine.ProductId;
                                            //StockViewModel.ProductUidId = ProdUid.ProductUIDId;
                                            StockViewModel.ProductUidId = ProductUidId;
                                            StockViewModel.HeaderFromGodownId = null;
                                            StockViewModel.HeaderGodownId = s.GodownId;
                                            StockViewModel.HeaderProcessId = s.ProcessId;
                                            StockViewModel.GodownId = (int)s.GodownId;
                                            StockViewModel.Remark = s.Remark;
                                            StockViewModel.Status = s.Status;
                                            StockViewModel.ProcessId = s.ProcessId;
                                            StockViewModel.LotNo = null;
                                            StockViewModel.CostCenterId = SelectedJobOrderLine.CostCenterId;
                                            StockViewModel.Qty_Iss = 0;
                                            StockViewModel.Qty_Rec = 1;
                                            StockViewModel.Rate = SelectedJobOrderLine.Rate;
                                            StockViewModel.ExpiryDate = null;
                                            StockViewModel.Specification = JobOrderLine.Specification;
                                            StockViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                                            StockViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                                            StockViewModel.CreatedBy = User.Identity.Name;
                                            StockViewModel.CreatedDate = DateTime.Now;
                                            StockViewModel.ModifiedBy = User.Identity.Name;
                                            StockViewModel.ModifiedDate = DateTime.Now;

                                            string StockPostingError = "";
                                            StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel);

                                            if (StockPostingError != "")
                                            {
                                                string message = StockPostingError;
                                                ModelState.AddModelError("", message);
                                                return View("Create", svm);
                                            }

                                            if (Cnt == 0)
                                            {
                                                s.StockHeaderId = StockViewModel.StockHeaderId;
                                            }


                                            JobReceiveLine line = new JobReceiveLine();
                                            line.StockId = StockViewModel.StockId;
                                            //line.ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;
                                            line.ProductUidHeaderId = ProductUidHeaderId;
                                            //line.ProductUidId = ProdUid.ProductUIDId;
                                            line.ProductUidId = ProductUidId;
                                            line.JobReceiveHeaderId = s.JobReceiveHeaderId;
                                            line.JobOrderLineId = JobOrderLine.JobOrderLineId;
                                            line.Qty = 1;
                                            line.PassQty = 1;
                                            line.LossQty = 0;
                                            line.UnitConversionMultiplier = JobOrderLine.UnitConversionMultiplier;
                                            line.DealQty = 1 * JobOrderLine.UnitConversionMultiplier;
                                            line.DealUnitId = JobOrderLine.DealUnitId;
                                            line.Sr = Sr++;
                                            line.CreatedDate = DateTime.Now;
                                            line.ModifiedDate = DateTime.Now;
                                            line.CreatedBy = User.Identity.Name;
                                            line.ModifiedBy = User.Identity.Name;
                                            line.JobReceiveLineId = pk;
                                            line.ObjectState = Model.ObjectState.Added;
                                            new JobReceiveLineService(_unitOfWork).Create(line);

                                            new JobReceiveLineStatusService(_unitOfWork).CreateLineStatus(line.JobReceiveLineId, ref db, false);


                                            pk++;
                                            Cnt = Cnt + 1;
                                            ProductUidCountForJobOrderLine++;
                                        }
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



                        IEnumerable<JobReceiveLine> JobReceiveLineList = new JobReceiveLineService(_unitOfWork).GetJobReceiveLineList(s.JobReceiveHeaderId);

                        foreach(JobReceiveLine Line in JobReceiveLineList)
                        {
                            if (Line.ProductUidId != null)
                            {
                                ProductUid ProductUid = new ProductUidService(_unitOfWork).Find((int)Line.ProductUidId);
                                ProductUid.GenDocId = Line.JobReceiveHeaderId;
                                ProductUid.LastTransactionDocId = Line.JobReceiveHeaderId;
                                ProductUid.GenLineId = Line.JobReceiveLineId;
                                ProductUid.LastTransactionLineId = Line.JobReceiveLineId;
                                new ProductUidService(_unitOfWork).Update(ProductUid);
                            }
                        }

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
                            DocId = s.JobReceiveHeaderId,
                            ActivityType = (int)ActivityTypeContants.WizardCreate,
                            DocNo = s.DocNo,                            
                            DocDate = s.DocDate,
                            DocStatus = s.Status,
                        }));

                        System.Web.HttpContext.Current.Session.Remove("BarCodesWeavingWizardJobOrder");

                        return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/Modify/" + s.JobReceiveHeaderId);
                    }
                    else
                    {
                        return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/Index/" + s.DocTypeId);
                    }

                }
                else
                {

                }

            }
            PrepareViewBag();
            ViewBag.Mode = "Add";
            //return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/Submit/"+s.JobReceiveHeaderId);
            return View("Create", svm);
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
