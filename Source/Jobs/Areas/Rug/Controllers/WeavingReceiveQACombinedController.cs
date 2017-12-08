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
using AutoMapper;
using Jobs.Helpers;
using System.Configuration;
using System.Xml.Linq;
using DocumentEvents;
using CustomEventArgs;
using Reports.Reports;
using Model.ViewModels;
using Reports.Controllers;
using System.Data.SqlClient;
using System.Data;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class WeavingReceiveQACombinedController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationDbContext db1 = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();
        int MainBranchId = 1;

        private bool EventException = false;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public WeavingReceiveQACombinedController(IExceptionHandlingService exec, IUnitOfWork uow)
        {
            _unitOfWork = uow;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }


        public void PrepareViewBag(int id)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();

            ViewBag.Name = db.DocumentType.Find(id).DocumentTypeName;
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
            var JobReceiveHeader = new WeavingReceiveQACombinedService(db).GetJobReceiveHeaderList(id, User.Identity.Name);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(JobReceiveHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            IQueryable<WeavingReceiveQaCombinedIndexViewModel> p = new WeavingReceiveQACombinedService(db).GetJobReceiveHeaderListPendingToSubmit(id, User.Identity.Name);
            //IQueryable<JobReceiveIndexViewModel> p = new JobReceiveHeaderService(_unitOfWork).GetJobReceiveHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            ViewBag.id = id;
            return View("Index", p);
        }
        public ActionResult Index_PendingToReview(int id)
        {
            IQueryable<WeavingReceiveQaCombinedIndexViewModel> p = new WeavingReceiveQACombinedService(db).GetJobReceiveHeaderListPendingToReview(id, User.Identity.Name);
            //IQueryable<JobReceiveIndexViewModel> p = new JobReceiveHeaderService(_unitOfWork).GetJobReceiveHeaderListPendingToSubmit(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            ViewBag.id = id;
            return View("Index", p);
        }
        public int PendingToSubmitCount(int id)
        {
            return (new JobReceiveHeaderService(_unitOfWork).GetJobReceiveHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (new JobReceiveHeaderService(_unitOfWork).GetJobReceiveHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }


        public ActionResult Create(int id)//DocTypeId
        {
            WeavingReceiveQACombinedViewModel vm = new WeavingReceiveQACombinedViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            //var temp = new JobReceiveQAAttributeService(_unitOfWork).GetJobReceiveQAAttribute(id);
            //vm.QAGroupLine = temp;

            var temp = GetQAGroupLine();
            vm.QAGroupLine = temp;


            LastValues LastValues = new WeavingReceiveQACombinedService(db).GetLastValues(id);

            if (LastValues != null)
            {
                if (LastValues.JobReceiveById != null)
                {
                    vm.JobReceiveById = (int)LastValues.JobReceiveById;
                    vm.JobWorkerId = (int)LastValues.JobWorkerId;
                    vm.DocDate = LastValues.DocDate;
                }
            }
            else
            {
                vm.DocDate = DateTime.Now;
            }

            //vm.ProductUidName = GetNewProductUid();

            //Getting Settings
            var jobreceivesettings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (jobreceivesettings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveSettings", new { id = id }).Warning("Please create job Inspection settings");
            }
            else if (jobreceivesettings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(jobreceivesettings);





            var jobreceiveqasettings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (jobreceiveqasettings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveQaSettings", new { id = id }).Warning("Please create job Inspection settings");
            }
            else if (jobreceiveqasettings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(jobreceiveqasettings);

            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(id);

            vm.ProcessId = jobreceivesettings.ProcessId;
            //Akash
            //vm.DocDate = DateTime.Now;
            vm.DocTypeId = id;

            if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
                vm.GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];

            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            PrepareViewBag(id);
            ViewBag.Mode = "Add"; 
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(WeavingReceiveQACombinedViewModel vm)
        {
            if (ModelState.IsValid)
            {
                #region CreateRecord
                if (vm.JobReceiveHeaderId <= 0)
                {
                    JobReceiveHeader JobReceiveHeader = new JobReceiveHeader();
                    JobReceiveHeader = new WeavingReceiveQACombinedService(db).Create(vm, User.Identity.Name);


                    try
                    {
                        db.SaveChanges();
                    }


                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", vm);
                    }

                    if (vm.ProductUidName != null || vm.LotNo  != null)
                    {
                        ProductUid ProductUid = new ProductUidService(_unitOfWork).Find(vm.LotNo);
                        ProductUid.ModifiedDate = DateTime.Now;
                        if (ProductUid.GenDocId == 0)
                        {
                            ProductUid.GenDocId = JobReceiveHeader.JobReceiveHeaderId;
                        }

                        ProductUid.CurrenctProcessId = JobReceiveHeader.ProcessId;
                        ProductUid.CurrenctGodownId = JobReceiveHeader.GodownId;
                        ProductUid.Status = ProductUidStatusConstants.Receive;
                        ProductUid.LastTransactionDocId = JobReceiveHeader.JobReceiveHeaderId;
                        ProductUid.LastTransactionDocNo = JobReceiveHeader.DocNo;
                        ProductUid.LastTransactionDocTypeId = JobReceiveHeader.DocTypeId;
                        ProductUid.LastTransactionDocDate = JobReceiveHeader.DocDate;
                        ProductUid.LastTransactionPersonId = JobReceiveHeader.JobWorkerId;

                        ProductUid.ObjectState = Model.ObjectState.Modified;
                        db1.ProductUid.Add(ProductUid);
                    }

                    try
                    {
                        db1.SaveChanges();
                    }


                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = vm.DocTypeId,
                        DocId = vm.JobReceiveQAAttributeId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocDate = vm.DocDate,
                        DocNo = vm.DocNo,
                        DocStatus = vm.Status,
                    }));


                    return RedirectToAction("Edit", new { id = JobReceiveHeader.JobReceiveHeaderId }).Success("Data saved successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobReceiveQAHeader temp = new JobReceiveQAHeaderService(db).Find(vm.JobReceiveQAHeaderId);

                    JobReceiveQAHeader ExRec = new JobReceiveQAHeader();
                    ExRec = Mapper.Map<JobReceiveQAHeader>(temp);


                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted)
                        temp.Status = (int)StatusConstants.Modified;


                    new WeavingReceiveQACombinedService(db).Update(vm, User.Identity.Name);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);



                    try
                    {
                        if (EventException)
                        { throw new Exception(); }
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(temp.DocTypeId);
                        ViewBag.Mode = "Edit";
                        return View("Create", vm);
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobReceiveQAHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        xEModifications = Modifications,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("Index", new { id = temp.DocTypeId }).Success("Data saved successfully");

                }
                #endregion

            }
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        public ActionResult Edit(int id, string IndexType)
        {
            WeavingReceiveQACombinedViewModel pt = new WeavingReceiveQACombinedService(db).GetJobReceiveDetailForEdit(id);

            if (pt != null)
            {
                ProductDimensions ProductDimensions = new ProductService(_unitOfWork).GetProductDimensions(pt.ProductId, pt.DealUnitId, pt.DocTypeId);
                if (ProductDimensions != null)
                {
                    pt.OrderLength = ProductDimensions.Length;
                    pt.OrderWidth = ProductDimensions.Width;
                    pt.DimensionUnitDecimalPlaces = ProductDimensions.DimensionUnitDecimalPlaces;
                }
            }

            if (pt == null)
            {
                return HttpNotFound();
            }

            var temp = new JobReceiveQAAttributeService(db).GetJobReceiveQAAttributeForEdit(pt.JobReceiveQALineId);
            pt.QAGroupLine = temp;
            
            
            //var temp = GetQAGroupLine();
            //pt.QAGroupLine = temp;

            //Getting Settings
            //Getting Settings
            var jobreceivesettings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (jobreceivesettings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveSettings", new { id = id }).Warning("Please create job Inspection settings");
            }
            else if (jobreceivesettings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(jobreceivesettings);





            var jobreceiveqasettings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (jobreceiveqasettings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveQaSettings", new { id = id }).Warning("Please create job Inspection settings");
            }
            else if (jobreceiveqasettings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(jobreceiveqasettings);


            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            PrepareViewBag(pt.DocTypeId);

            ViewBag.Mode = "Edit";
            if ((System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobReceiveHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }


        public JsonResult GetUnitConversionMultiplier(int ProductId, Decimal Length, Decimal Width, Decimal? Height, string ToUnitId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);

            Decimal UnitConversionMultiplier = 0;
            UnitConversionMultiplier = new ProductService(_unitOfWork).GetUnitConversionMultiplier(1, product.UnitId, Length, Width, Height, ToUnitId,db);

            return Json(UnitConversionMultiplier);
        }

        public JsonResult getunitconversiondetailjson(int productid, string unitid, string deliveryunitid, int JobOrderLineId)
        {
            var temp = (from L in db.JobOrderLine 
                        where L.JobOrderLineId == JobOrderLineId
                        select new
                        {
                            UnitConversionForId = L.JobOrderHeader.UnitConversionForId
                        }).FirstOrDefault();

            //UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(productid, unitid, deliveryunitid);
            UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(productid, unitid, (int)temp.UnitConversionForId, deliveryunitid);
            List<SelectListItem> unitconversionjson = new List<SelectListItem>();
            if (uc != null)
            {
                unitconversionjson.Add(new SelectListItem
                {
                    Text = uc.Multiplier.ToString(),
                    Value = uc.Multiplier.ToString()
                });
            }
            else
            {
                unitconversionjson.Add(new SelectListItem
                {
                    Text = "0",
                    Value = "0"
                });
            }


            return Json(unitconversionjson);
        }

        //public JsonResult GetProductDetailJson(int ProductId, string DealUnitId)
        //{
        //    SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
        //    SqlParameter SqlParameterDealUnitId = new SqlParameter("@DealUnitId", DealUnitId);

        //    ProductDimensions ProductDimensions = db.Database.SqlQuery<ProductDimensions>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetProductDimensions @ProductId, @DealUnitId", SqlParameterProductId, SqlParameterDealUnitId).FirstOrDefault();

        //    return Json(ProductDimensions);
        //}

        public ActionResult _CreatePenalty(int id) //Id ==> JobReceiveQALineId
        {
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveQAPenalty/_Create/" + id);

            //JobReceiveQALine L = new JobReceiveQALineService(db, _unitOfWork).Find(Id);
            //JobReceiveQAHeader H = new JobReceiveQAHeaderService(db).Find(L.JobReceiveQAHeaderId);
            //DocumentType D = new DocumentTypeService(_unitOfWork).Find(H.DocTypeId);
            //JobReceiveQAPenaltyViewModel s = new JobReceiveQAPenaltyViewModel();

            //s.DocTypeId = H.DocTypeId;
            //s.JobReceiveQALineId = Id;
            ////Getting Settings
            //PrepareViewBag();
            //if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            //{
            //    ViewBag.CSEXCL = TempData["CSEXCL"];
            //    TempData["CSEXCL"] = null;
            //}
            //ViewBag.LineMode = "Create";
            //ViewBag.DocNo = D.DocumentTypeName + "-" + H.DocNo;
            //return PartialView("_Create", s);
        }


        public ActionResult _IndexPenalty(int id) //Id ==> JobReceiveQALineId
        {
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveQAPenalty/Index/" + id);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            JobReceiveHeader header = new JobReceiveHeaderService(_unitOfWork).Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            JobReceiveHeader header = new JobReceiveHeaderService(_unitOfWork).Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }

        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobReceiveHeader JobReceiveHeader = new JobReceiveHeaderService(_unitOfWork).Find(id);

            if (JobReceiveHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(JobReceiveHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            bool BeforeSave = true;


            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                var temp = (from p in db.JobReceiveHeader
                            where p.JobReceiveHeaderId == vm.id
                            select p).FirstOrDefault();

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobReceiveHeader>(temp),
                });

                new WeavingReceiveQACombinedService(db).Delete(vm.id);


                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    if (EventException)
                    { throw new Exception(); }

                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    return PartialView("_Reason", vm);
                }



                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.JobReceiveHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = temp.DocNo,
                    DocDate = temp.DocDate,
                    xEModifications = Modifications,
                    DocStatus = temp.Status,
                }));


                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }

        [HttpGet]
        public JsonResult ConsumptionIndex(int id)
        {
            var p = new JobReceiveLineService(_unitOfWork).GetConsumptionLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Consumption(int id)//ReceiveHeaderId
        {
            JobReceiveBomViewModel vm = new JobReceiveBomViewModel();
            vm.JobReceiveHeaderId = id;
            JobReceiveHeader H = new JobReceiveHeaderService(_unitOfWork).Find(id);           
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
            ViewBag.LineMode = "Create";
            return PartialView("Consumption", vm);
        }

        public JsonResult GetProductUIDDetailJson_WithJobReceiveHeaderId(string ProductUIDNo, int JobReceiveHeaderId)
        {
            ProductUidDetail productuiddetail = new ProductUidService(_unitOfWork).FGetProductUidDetail(ProductUIDNo);

            List<ProductUidDetail> ProductUidDetailJson = new List<ProductUidDetail>();

            if (productuiddetail != null)
            {      
                ProductUidDetailJson.Add(new ProductUidDetail()
                {
                    ProductId = productuiddetail.ProductId,
                    ProductName = productuiddetail.ProductName,
                    ProductUidId = productuiddetail.ProductUidId,
                    PrevProcessId = productuiddetail.PrevProcessId,
                    PrevProcessName = productuiddetail.PrevProcessName,
                    DivisionId = productuiddetail.DivisionId,
                });
            }
            return Json(ProductUidDetailJson);
        }

        public JsonResult GetProductsForConsumption(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {

            var Query = new JobReceiveLineService(_unitOfWork).GetConsumptionProducts(searchTerm, filter);

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            return new JsonResult
            {
                Data = temp,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetProductDetailJson(int ProductId, int? HeaderId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);
            List<Product> ProductJson = new List<Product>();

            decimal BalanceQty = 0;

            if (HeaderId.HasValue)
            {
                BalanceQty = new JobReceiveLineService(_unitOfWork).GetConsumptionBalanceQty(HeaderId.Value, ProductId);
            }

            return Json(new
            {
                ProductId = product.ProductId,
                StandardCost = product.StandardCost,
                UnitId = product.UnitId,
                BalanceQty = BalanceQty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConsumptionPost(JobReceiveBomViewModel vm)
        {

            if (ModelState.IsValid)
            {
                //Create Logic
                if (vm.JobReceiveBomId <= 0)
                {
                    JobReceiveBom Consumption = Mapper.Map<JobReceiveBomViewModel, JobReceiveBom>(vm);
                    Consumption.CreatedBy = User.Identity.Name;
                    Consumption.CreatedDate = DateTime.Now;
                    Consumption.ModifiedBy = User.Identity.Name;
                    Consumption.ModifiedDate = DateTime.Now;

                    JobReceiveLine JobReceiveLine = new JobReceiveLineService(_unitOfWork).GetJobReceiveLineList(vm.JobReceiveHeaderId).FirstOrDefault();

                    CostCenter CostCenter = new JobReceiveLineService(_unitOfWork).GetCoscenterId((int)JobReceiveLine.JobReceiveLineId);

                    Consumption.CostCenterId = CostCenter.CostCenterId;
                    JobReceiveHeader JobReceiveHeader = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);
                    StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();


                    if (JobReceiveHeader.StockHeaderId == null)
                    {
                        StockProcessBomViewModel.StockHeaderId = 0;
                    }
                    else
                    {
                        StockProcessBomViewModel.StockHeaderId = (int)JobReceiveHeader.StockHeaderId;
                    }

                    StockProcessBomViewModel.DocHeaderId = JobReceiveHeader.JobReceiveHeaderId;
                    StockProcessBomViewModel.DocLineId = Consumption.JobReceiveBomId;
                    StockProcessBomViewModel.DocTypeId = JobReceiveHeader.DocTypeId;
                    StockProcessBomViewModel.StockHeaderDocDate = JobReceiveHeader.DocDate;
                    StockProcessBomViewModel.StockProcessDocDate = JobReceiveHeader.DocDate;
                    StockProcessBomViewModel.DocNo = JobReceiveHeader.DocNo;
                    StockProcessBomViewModel.DivisionId = JobReceiveHeader.DivisionId;
                    StockProcessBomViewModel.SiteId = JobReceiveHeader.SiteId;
                    StockProcessBomViewModel.CurrencyId = null;
                    StockProcessBomViewModel.HeaderProcessId = null;
                    StockProcessBomViewModel.PersonId = JobReceiveHeader.JobWorkerId;
                    StockProcessBomViewModel.ProductId = Consumption.ProductId;
                    StockProcessBomViewModel.HeaderFromGodownId = null;
                    StockProcessBomViewModel.HeaderGodownId = null;
                    StockProcessBomViewModel.GodownId = JobReceiveHeader.GodownId;
                    StockProcessBomViewModel.ProcessId = JobReceiveHeader.ProcessId;
                    StockProcessBomViewModel.LotNo = Consumption.LotNo;
                    StockProcessBomViewModel.CostCenterId = Consumption.CostCenterId;
                    StockProcessBomViewModel.Qty_Iss = Consumption.Qty;
                    StockProcessBomViewModel.Qty_Rec = 0;
                    StockProcessBomViewModel.Rate = 0;
                    StockProcessBomViewModel.ExpiryDate = null;
                    StockProcessBomViewModel.Specification = null;
                    StockProcessBomViewModel.Dimension1Id = null;
                    StockProcessBomViewModel.Dimension2Id = null;
                    StockProcessBomViewModel.Dimension3Id = null;
                    StockProcessBomViewModel.Dimension4Id = null;
                    StockProcessBomViewModel.Remark = null;
                    StockProcessBomViewModel.Status = JobReceiveHeader.Status;
                    StockProcessBomViewModel.CreatedBy = User.Identity.Name;
                    StockProcessBomViewModel.CreatedDate = DateTime.Now;
                    StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
                    StockProcessBomViewModel.ModifiedDate = DateTime.Now;

                    string StockProcessPostingError = "";
                    StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessBomViewModel, ref db);

                    if (StockProcessPostingError != "")
                    {
                        ModelState.AddModelError("", StockProcessPostingError);
                        return PartialView("_Create", vm);
                    }

                    Consumption.StockProcessId = StockProcessBomViewModel.StockProcessId;
                    Consumption.ObjectState = Model.ObjectState.Added;
                    db.JobReceiveBom.Add(Consumption);


                    if (!JobReceiveHeader.StockHeaderId.HasValue || JobReceiveHeader.StockHeaderId == 0)
                    {
                        JobReceiveHeader.StockHeaderId = StockProcessBomViewModel.StockHeaderId;

                        JobReceiveHeader.ObjectState = Model.ObjectState.Modified;
                        db.JobReceiveHeader.Add(JobReceiveHeader);
                    }


                    try
                    {
                        db.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("Consumption", vm);
                    }

                    return RedirectToAction("Consumption", new { id = vm.JobReceiveHeaderId });

                }
                else//Edit Logic
                {

                    JobReceiveBom temp = new JobReceiveBomService(_unitOfWork).Find(vm.JobReceiveBomId);
                    temp.ProductId = vm.ProductId;
                    temp.Dimension1Id = vm.Dimension1Id;
                    temp.Dimension2Id = vm.Dimension2Id;
                    temp.Dimension3Id = vm.Dimension3Id;
                    temp.Dimension4Id = vm.Dimension4Id;
                    temp.LotNo = vm.LotNo;
                    temp.Qty = vm.Qty;


                    JobReceiveHeader JobReceiveHeader = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);
                    StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();

                    StockProcessBomViewModel.StockHeaderId = JobReceiveHeader.StockHeaderId ?? 0;
                    StockProcessBomViewModel.StockProcessId = temp.StockProcessId ?? 0;
                    StockProcessBomViewModel.DocHeaderId = JobReceiveHeader.JobReceiveHeaderId;
                    StockProcessBomViewModel.DocLineId = temp.JobReceiveBomId;
                    StockProcessBomViewModel.DocTypeId = JobReceiveHeader.DocTypeId;
                    StockProcessBomViewModel.StockHeaderDocDate = JobReceiveHeader.DocDate;
                    StockProcessBomViewModel.StockProcessDocDate = JobReceiveHeader.DocDate;
                    StockProcessBomViewModel.DocNo = JobReceiveHeader.DocNo;
                    StockProcessBomViewModel.DivisionId = JobReceiveHeader.DivisionId;
                    StockProcessBomViewModel.SiteId = JobReceiveHeader.SiteId;
                    StockProcessBomViewModel.CurrencyId = null;
                    StockProcessBomViewModel.HeaderProcessId = null;
                    StockProcessBomViewModel.PersonId = JobReceiveHeader.JobWorkerId;
                    StockProcessBomViewModel.ProductId = temp.ProductId;
                    StockProcessBomViewModel.HeaderFromGodownId = null;
                    StockProcessBomViewModel.HeaderGodownId = null;
                    StockProcessBomViewModel.GodownId = JobReceiveHeader.GodownId;
                    StockProcessBomViewModel.ProcessId = JobReceiveHeader.ProcessId;
                    StockProcessBomViewModel.LotNo = temp.LotNo;
                    StockProcessBomViewModel.CostCenterId = temp.CostCenterId;
                    StockProcessBomViewModel.Qty_Iss = temp.Qty;
                    StockProcessBomViewModel.Qty_Rec = 0;
                    StockProcessBomViewModel.Rate = 0;
                    StockProcessBomViewModel.ExpiryDate = null;
                    StockProcessBomViewModel.Specification = null;
                    StockProcessBomViewModel.Dimension1Id = null;
                    StockProcessBomViewModel.Dimension2Id = null;
                    StockProcessBomViewModel.Dimension3Id = null;
                    StockProcessBomViewModel.Dimension4Id = null;
                    StockProcessBomViewModel.Remark = null;
                    StockProcessBomViewModel.Status = JobReceiveHeader.Status;
                    StockProcessBomViewModel.CreatedBy = User.Identity.Name;
                    StockProcessBomViewModel.CreatedDate = DateTime.Now;
                    StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
                    StockProcessBomViewModel.ModifiedDate = DateTime.Now;

                    string StockProcessPostingError = "";
                    StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessBomViewModel, ref db);

                    if (StockProcessPostingError != "")
                    {
                        ModelState.AddModelError("", StockProcessPostingError);
                        return PartialView("_Create", vm);
                    }



                    temp.StockProcessId = StockProcessBomViewModel.StockProcessId;
                    temp.ObjectState = Model.ObjectState.Modified;

                    db.JobReceiveBom.Add(temp);

                    //new JobReceiveBomService(_unitOfWork).Update(temp);

                    try
                    {
                        db.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("Consumption", vm);

                    }

                    return Json(new { success = true });

                }
            }

            return PartialView("Consumption", vm);

        }

        public ActionResult EditConsumption(int id)//ReceiveHeaderId
        {
            JobReceiveBomViewModel vm = new JobReceiveBomService(_unitOfWork).GetJobReceiveBom(id);

            JobReceiveHeader H = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);

            if (vm == null)
            {
                return HttpNotFound();
            }
            ViewBag.LineMode = "Edit";
            return PartialView("Consumption", vm);
        }

        public ActionResult DeleteConsumption(int id)//ReceiveHeaderId
        {
            JobReceiveBomViewModel vm = new JobReceiveBomService(_unitOfWork).GetJobReceiveBom(id);

            JobReceiveHeader H = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);

            if (vm == null)
            {
                return HttpNotFound();
            }
            ViewBag.LineMode = "Delete";
            return PartialView("Consumption", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConsumption(JobReceiveBomViewModel vm)
        {
            JobReceiveBom temp = (from p in db.JobReceiveBom
                                  where p.JobReceiveBomId == vm.JobReceiveBomId
                                  select p).FirstOrDefault();

            JobReceiveHeader header = new JobReceiveHeaderService(_unitOfWork).Find(vm.JobReceiveHeaderId);

            if (temp.StockProcessId != null)
            {
                var StockProcess = (from p in db.StockProcess
                                    where p.StockProcessId == temp.StockProcessId
                                    select p).FirstOrDefault();
                StockProcess.ObjectState = Model.ObjectState.Deleted;
                db.StockProcess.Remove(StockProcess);
            }

            temp.ObjectState = Model.ObjectState.Deleted;
            db.JobReceiveBom.Remove(temp);
            //new JobReceiveBomService(_unitOfWork).Delete(temp);

            if (header.Status != (int)StatusConstants.Drafted)
            {
                header.Status = (int)StatusConstants.Modified;
                header.ModifiedBy = User.Identity.Name;
                header.ModifiedDate = DateTime.Now;
            }


            header.ObjectState = Model.ObjectState.Modified;
            db.JobReceiveHeader.Add(header);

            try
            {
                db.SaveChanges();
                //_unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("Consumption", vm);
            }
            return Json(new { success = true });
        }

        [Authorize]
        public ActionResult Detail(int id, string IndexType, string transactionType, int? DocLineId)
        {
            if (DocLineId.HasValue)
                ViewBag.DocLineId = DocLineId;

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            WeavingReceiveQACombinedViewModel pt = new WeavingReceiveQACombinedService(db).GetJobReceiveDetailForEdit(id);


            //Job Receive Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveSettings", new { id = pt.DocTypeId }).Warning("Please create job receive settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            pt.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);


            var jobreceiveqasettings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (jobreceiveqasettings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveQaSettings", new { id = id }).Warning("Please create job Inspection settings");
            }
            else if (jobreceiveqasettings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(jobreceiveqasettings);

            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }
            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobReceiveHeaderId,
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

            JobReceiveHeader s = db.JobReceiveHeader.Find(id);

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
            if (ModelState.IsValid)
            {
                JobReceiveHeader pd = new JobReceiveHeaderService(_unitOfWork).Find(Id);
                int ActivityType;
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.JobReceiveHeader.Add(pd);

                    JobReceiveSettings Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);
                    try
                    {
                        string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
                        using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                        {
                            sqlConnection.Open();
                            using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + Settings.SqlProcConsumption))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = sqlConnection;
                                cmd.Parameters.AddWithValue("@JobReceiveHeaderId", Id);
                                cmd.CommandTimeout = 1000;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    try
                    {
                        if (EventException)
                        { throw new Exception(); }

                        db.SaveChanges();
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
                        DocId = pd.JobReceiveHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocDate = pd.DocDate,
                        DocNo = pd.DocNo,
                        DocStatus = pd.Status,
                    }));
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record submitted successfully.");
                }
                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + pd.ModifiedBy + " only.");
            }
            

            return View();
        }


        [HttpGet]
        public ActionResult DetailByProductUid(string ProductUidName)
        {

            if (ProductUidName != null && ProductUidName != "")
            {
                ProductUid PU = new ProductUidService(_unitOfWork).Find(ProductUidName);
                if (PU != null && PU.GenDocTypeId == 448)
                {
                    WeavingReceiveQACombinedViewModel_ByProductUid pt = new WeavingReceiveQACombinedService(db).GetProductUidDetail(PU.ProductUIDId);

                    int id = (int)PU.GenDocId;
                    

                    if (pt != null )
                    {
                        ProductDimensions ProductDimensions = new ProductService(_unitOfWork).GetProductDimensions(pt.ProductId, pt.DealUnitId, pt.DocTypeId);
                        if (ProductDimensions != null)
                        {
                            pt.OrderLength = ProductDimensions.Length;
                            pt.OrderWidth = ProductDimensions.Width;
                            pt.DimensionUnitDecimalPlaces = ProductDimensions.DimensionUnitDecimalPlaces;
                        }
                    }

                    if (pt == null)
                    {
                        return HttpNotFound();
                    }


                    var temp = new JobReceiveQAAttributeService(db).GetJobReceiveQAAttributeForEdit(pt.JobReceiveQALineId);
                    pt.QAGroupLine = temp;


                    var jobreceivesettings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

                    if (jobreceivesettings == null && UserRoles.Contains("SysAdmin"))
                    {
                        return RedirectToAction("Create", "JobReceiveSettings", new { id = id }).Warning("Please create job Inspection settings");
                    }
                    else if (jobreceivesettings == null && !UserRoles.Contains("SysAdmin"))
                    {
                        return View("~/Views/Shared/InValidSettings.cshtml");
                    }
                    pt.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(jobreceivesettings);





                    var jobreceiveqasettings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

                    if (jobreceiveqasettings == null && UserRoles.Contains("SysAdmin"))
                    {
                        return RedirectToAction("Create", "JobReceiveQaSettings", new { id = id }).Warning("Please create job Inspection settings");
                    }
                    else if (jobreceiveqasettings == null && !UserRoles.Contains("SysAdmin"))
                    {
                        return View("~/Views/Shared/InValidSettings.cshtml");
                    }
                    pt.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(jobreceiveqasettings);


                    pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

                    PrepareViewBag(pt.DocTypeId);
                    
                    ViewBag.Mode = "Edit";

                    return View("DetailByProductUid", pt);
                }
                else
                {
                    WeavingReceiveQACombinedViewModel_ByProductUid pt = new WeavingReceiveQACombinedViewModel_ByProductUid();
                    pt.OrderLength = 0;
                    pt.OrderWidth = 0;
                    pt.DimensionUnitDecimalPlaces = 0;

                    pt.DocTypeId = 448;
                    pt.DivisionId = 1;
                    pt.SiteId = 1;
                    pt.Message = "Invalid Barcode";
                    var temp = new JobReceiveQAAttributeService(db).GetJobReceiveQAAttributeForEdit(pt.JobReceiveQALineId);
                    pt.QAGroupLine = temp;


                    var jobreceivesettings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

                    if (jobreceivesettings == null && UserRoles.Contains("SysAdmin"))
                    {
                        //return RedirectToAction("Create", "JobReceiveSettings", new { id = id }).Warning("Please create job Inspection settings");
                    }
                    else if (jobreceivesettings == null && !UserRoles.Contains("SysAdmin"))
                    {
                        return View("~/Views/Shared/InValidSettings.cshtml");
                    }
                    pt.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(jobreceivesettings);





                    var jobreceiveqasettings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

                    if (jobreceiveqasettings == null && UserRoles.Contains("SysAdmin"))
                    {
                        //return RedirectToAction("Create", "JobReceiveQaSettings", new { id = id }).Warning("Please create job Inspection settings");
                    }
                    else if (jobreceiveqasettings == null && !UserRoles.Contains("SysAdmin"))
                    {
                        return View("~/Views/Shared/InValidSettings.cshtml");
                    }
                    pt.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(jobreceiveqasettings);


                    pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

                    PrepareViewBag(pt.DocTypeId);

                    pt.ProductUidName = "";
                    pt.JobReceiveHeaderId = 0;
                    pt.JobReceiveLineId = 0;
                    pt.JobReceiveQALineId = 0;
                    pt.JobReceiveQAHeaderId = 0;
                    pt.StockHeaderId = 0;
                    pt.StockId = 0;
                    pt.JobOrderLineId = 0;
                    pt.JobOrderHeaderDocNo = "";
                    pt.PaymentSlipNo = "";
                    pt.GodownId = 0;
                    pt.JobWorkerId = 0;
                    pt.ProductUidId = 0;
                    pt.ProductUidName = "";
                    pt.ProductId = 0;
                    pt.ProductCategoryName = "";
                    pt.ColourName = "";
                    pt.ProductGroupName = "";
                    pt.ProductName = "";
                    pt.LotNo = "";
                    pt.SiteName = "";
                    pt.PONo = "";
                    pt.InvoiceNo = "";
                    pt.InvoiceParty = "";
                    pt.RollNo = "";
                    pt.CostcenterName = "";
                    pt.Qty = 0;
                    pt.UnitId = "";
                    pt.DealUnitId = "";
                    pt.UnitConversionMultiplier = 0;
                    pt.DealQty = 0;
                    pt.Weight = 0;
                    pt.UnitDecimalPlaces = 0;
                    pt.DealUnitDecimalPlaces = 0;
                    pt.Rate = 0;
                    pt.XRate = 0;
                    pt.Amount = 0;
                    pt.PenaltyRate = 0;
                    pt.PenaltyAmt = 0;
                    pt.DivisionId = 0;
                    pt.SiteId = 0;
                    pt.ProcessId = 0;
                    pt.DocTypeId = 448;
                    pt.DocNo = "";
                    pt.ProductQualityName = "";
                    pt.JobReceiveById = 0;
                    pt.Remark = "";
                    pt.Length = 0;
                    pt.OrderLength = 0;
                    pt.Width = 0;
                    pt.OrderWidth = 0;
                    pt.Height = 0;
                    ViewBag.Mode = "Edit";

                    return View("DetailByProductUid", pt);
                }
            }
            else
            {

                WeavingReceiveQACombinedViewModel_ByProductUid pt = new WeavingReceiveQACombinedViewModel_ByProductUid();
                pt.OrderLength = 0;
                pt.OrderWidth = 0;
                pt.DimensionUnitDecimalPlaces = 0;

                pt.DocTypeId = 448;
                pt.DivisionId = 1;
                pt.SiteId = 1;
                var temp = new JobReceiveQAAttributeService(db).GetJobReceiveQAAttributeForEdit(pt.JobReceiveQALineId);
                pt.QAGroupLine = temp;


                var jobreceivesettings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

                if (jobreceivesettings == null && UserRoles.Contains("SysAdmin"))
                {
                    //return RedirectToAction("Create", "JobReceiveSettings", new { id = id }).Warning("Please create job Inspection settings");
                }
                else if (jobreceivesettings == null && !UserRoles.Contains("SysAdmin"))
                {
                    return View("~/Views/Shared/InValidSettings.cshtml");
                }
                pt.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(jobreceivesettings);





                var jobreceiveqasettings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

                if (jobreceiveqasettings == null && UserRoles.Contains("SysAdmin"))
                {
                    //return RedirectToAction("Create", "JobReceiveQaSettings", new { id = id }).Warning("Please create job Inspection settings");
                }
                else if (jobreceiveqasettings == null && !UserRoles.Contains("SysAdmin"))
                {
                    return View("~/Views/Shared/InValidSettings.cshtml");
                }
                pt.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(jobreceiveqasettings);


                pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

                PrepareViewBag(pt.DocTypeId);

                pt.ProductUidName = "";
                pt.JobReceiveHeaderId = 0;
                pt.JobReceiveLineId = 0;
                pt.JobReceiveQALineId = 0;
                pt.JobReceiveQAHeaderId = 0;
                pt.StockHeaderId = 0;
                pt.StockId = 0;
                pt.JobOrderLineId = 0;
                pt.JobOrderHeaderDocNo = "";
                pt.PaymentSlipNo = "";
                pt.GodownId = 0;
                pt.JobWorkerId = 0;
                pt.ProductUidId = 0;
                pt.ProductUidName = "";
                pt.ProductId = 0;
                pt.ProductCategoryName = "";
                pt.ColourName = "";
                pt.ProductGroupName = "";
                pt.ProductName = "";
                pt.LotNo = "";
                pt.SiteName = "";
                pt.PONo = "";
                pt.InvoiceNo = "";
                pt.InvoiceParty = "";
                pt.RollNo = "";
                pt.CostcenterName = "";
                pt.Qty = 0;
                pt.UnitId = "";
                pt.DealUnitId = "";
                pt.UnitConversionMultiplier = 0;
                pt.DealQty = 0;
                pt.Weight = 0;
                pt.UnitDecimalPlaces = 0;
                pt.DealUnitDecimalPlaces = 0;
                pt.Rate = 0;
                pt.XRate = 0;
                pt.Amount = 0;
                pt.PenaltyRate = 0;
                pt.PenaltyAmt = 0;
                pt.DivisionId = 0;
                pt.SiteId = 0;
                pt.ProcessId = 0;
                pt.DocTypeId = 448;
                pt.DocNo = "";
                pt.ProductQualityName = "";
                pt.JobReceiveById = 0;
                pt.Remark = "";
                pt.Length = 0;
                pt.OrderLength = 0;
                pt.Width = 0;
                pt.OrderWidth = 0;
                pt.Height = 0;
                ViewBag.Mode = "Edit";

                return View("DetailByProductUid", pt);
            }
        }



        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            JobReceiveHeader header =  new JobReceiveHeaderService(_unitOfWork).Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            JobReceiveHeader header = new JobReceiveHeaderService(_unitOfWork).Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        public List<QAGroupLineLineViewModel> GetQAGroupLine()
        {
            List<QAGroupLineLineViewModel> QAGroupLineList = (from L in db.QAGroupLine 
                                                                    select new QAGroupLineLineViewModel
                                                                    {
                                                                        QAGroupLineId = L.QAGroupLineId,
                                                                        DefaultValue = L.DefaultValue,
                                                                        Value = L.DefaultValue,
                                                                        Name = L.Name,
                                                                        DataType = L.DataType,
                                                                        ListItem = L.ListItem
                                                                    }).ToList();


            return QAGroupLineList;
        }

        public ActionResult GetCustomProduct(string searchTerm, int pageSize, int pageNum, int? filter, int ? PersonId)//DocTypeId
        {
            var Query = new WeavingReceiveQACombinedService(db).GetCustomProduct((int) filter, searchTerm, PersonId);
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

        public JsonResult GetJobOrderDetailJson(int JobOrderLineId)
        {
            JobOrderLine JOL = new JobOrderLineService(_unitOfWork).Find (JobOrderLineId);

            var ProductUIDName = GetNewProductUid(JOL.ProductId);

            var temp = (from L in db.ViewJobOrderBalance
                        join Dl in db.JobOrderLine on L.JobOrderLineId equals Dl.JobOrderLineId into JobOrderLineTable
                        from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                        join PU in db.ProductUid on L.ProductUidId equals PU.ProductUIDId into PUTable
                        from PUTab in PUTable.DefaultIfEmpty()
                        join P in db.Product on L.ProductId equals P.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join U in db.Units on ProductTab.UnitId equals U.UnitId into UnitTable
                        from UnitTab in UnitTable.DefaultIfEmpty()
                        join JW in db.Persons on L.JobWorkerId equals JW.PersonID into JWTable
                        from JWTab in JWTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on L.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on L.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where L.JobOrderLineId == JobOrderLineId
                        select new JobOrderDetail
                        {
                            JobOrderHeaderDocNo = L.JobOrderNo,
                            CostCenterNo = JobOrderLineTab.JobOrderHeader.CostCenter.CostCenterName,
                            DocTypeId = JobOrderLineTab.JobOrderHeader.DocTypeId,
                            JobWorkerId = JobOrderLineTab.JobOrderHeader.JobWorkerId,
                            JobWorkerName = JWTab.Name,
                            ProductUidId = PUTab.ProductUIDId ,
                            ProductUidName = PUTab.ProductUidName == null  ? ProductUIDName : PUTab.ProductUidName ,
                            UnitId = UnitTab.UnitId,
                            UnitName = UnitTab.UnitName,
                            DealUnitId = JobOrderLineTab.DealUnitId,
                            UnitConversionMultiplier = JobOrderLineTab.UnitConversionMultiplier,
                            ProductId = L.ProductId,
                            Rate = L.Rate,
                            BalanceQty = L.BalanceQty
                        }).FirstOrDefault();





            if (temp != null)
            {
                ProductDimensions ProductDimensions = new ProductService(_unitOfWork).GetProductDimensions(temp.ProductId, temp.DealUnitId, temp.DocTypeId);
                if (ProductDimensions != null)
                {
                    temp.Length = ProductDimensions.Length;
                    temp.Width = ProductDimensions.Width;
                    temp.Height = ProductDimensions.Height;
                    temp.DimensionUnitDecimalPlaces = ProductDimensions.DimensionUnitDecimalPlaces;
                }

                ProductViewModel PQ = new ProductService(_unitOfWork).GetFinishedProduct(temp.ProductId);
                if (PQ != null)
                {

                    temp.ProductQualityName = PQ.ProductQualityName;
                }

                Decimal UnitConversionMultiplier = 0;
                UnitConversionMultiplier = new ProductService(_unitOfWork).GetUnitConversionMultiplier(1, temp.UnitId, (decimal) temp.Length, (decimal) temp.Width, temp.Height, temp.DealUnitId, db);

                temp.UnitConversionMultiplier = UnitConversionMultiplier;
                return Json(temp);
            }
            else
            {
                return null;
            }
        }


        public JsonResult GetProductUidDetailJson(string ProductUidName)
        {
            ProductUid PUS = new ProductUidService(_unitOfWork).Find(ProductUidName);

            WeavingReceiveQACombinedViewModel_ByProductUid WeavingReceiveQADetail = (from H in db.JobReceiveHeader
                                                                                     join L in db.JobReceiveLine on H.JobReceiveHeaderId equals L.JobReceiveHeaderId into JobReceiveLineTable
                                                                                     from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                                                                                     join Jrql in db.JobReceiveQALine on JobReceiveLineTab.JobReceiveLineId equals Jrql.JobReceiveLineId into JobReceiveQaLineTable
                                                                                     from JobReceiveQALineTab in JobReceiveQaLineTable.DefaultIfEmpty()
                                                                                     join Jol in db.JobOrderLine on JobReceiveLineTab.JobOrderLineId equals Jol.JobOrderLineId into JobOrderLineTable
                                                                                     from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                                                                                     join PU in db.ProductUid on JobReceiveLineTab.ProductUidId equals PU.ProductUIDId into PUTable
                                                                                     from PUTab in PUTable.DefaultIfEmpty()
                                                                                     join SOL in db.SaleOrderLine on PUTab.SaleOrderLineId equals SOL.SaleOrderLineId into SOLTable
                                                                                     from SOLTab in SOLTable.DefaultIfEmpty()
                                                                                     join PL in db.PackingLine on JobReceiveLineTab.ProductUidId equals PL.ProductUidId into PLTable
                                                                                     from PLTab in PLTable.DefaultIfEmpty()
                                                                                     join SD in db.SaleDispatchLine on PLTab.PackingLineId equals SD.PackingLineId into SDTable
                                                                                     from SDTab in SDTable.DefaultIfEmpty()
                                                                                     join SI in db.SaleInvoiceLine on SDTab.SaleDispatchLineId equals SI.SaleDispatchLineId into SITable
                                                                                     from SITab in SITable.DefaultIfEmpty()
                                                                                     join SIH in db.SaleInvoiceHeader on SITab.SaleInvoiceHeaderId equals SIH.SaleInvoiceHeaderId into SIHTable
                                                                                     from SIHTab in SIHTable.DefaultIfEmpty()
                                                                                     join SIB in db.Persons on SIHTab.SaleToBuyerId equals SIB.PersonID into SIBTable
                                                                                     from SIBTab in SIBTable.DefaultIfEmpty()
                                                                                     join FP in db.FinishedProduct on JobOrderLineTab.ProductId equals FP.ProductId into FinishedProductTable
                                                                                     from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
                                                                                     join P in db.Product on JobOrderLineTab.ProductId equals P.ProductId into PTable
                                                                                     from PTab in PTable.DefaultIfEmpty()
                                                                                     join Ld in db.JobReceiveQALineExtended on JobReceiveQALineTab.JobReceiveQALineId equals Ld.JobReceiveQALineId into JobReceiveQALineExtendedTable
                                                                                     from JobReceiveQALineExtendedTab in JobReceiveQALineExtendedTable.DefaultIfEmpty()
                                                                                     where JobReceiveLineTab.JobReceiveHeaderId == PUS.GenDocId
                                                                                     select new WeavingReceiveQACombinedViewModel_ByProductUid
                                                                                     {
                                                                                         JobReceiveHeaderId = H.JobReceiveHeaderId,
                                                                                         JobReceiveLineId = JobReceiveLineTab.JobReceiveLineId,
                                                                                         JobReceiveQALineId = JobReceiveQALineTab.JobReceiveQALineId,
                                                                                         JobReceiveQAHeaderId = JobReceiveQALineTab.JobReceiveQAHeaderId,
                                                                                         StockHeaderId = H.StockHeaderId ?? 0,
                                                                                         StockId = JobReceiveLineTab.StockId ?? 0,
                                                                                         JobOrderLineId = (int)JobReceiveLineTab.JobOrderLineId,
                                                                                         JobOrderHeaderDocNo = JobOrderLineTab.JobOrderHeader.DocNo,
                                                                                         GodownId = H.GodownId,
                                                                                         JobWorkerId = H.JobWorkerId,
                                                                                         ProductUidId = JobReceiveLineTab.ProductUidId,
                                                                                         ProductUidName = JobReceiveLineTab.ProductUid.ProductUidName,
                                                                                         ProductId = JobOrderLineTab.ProductId,
                                                                                         ProductCategoryName = FinishedProductTab.ProductCategory.ProductCategoryName,
                                                                                         ColourName = FinishedProductTab.Colour.ColourName,
                                                                                         ProductGroupName = PTab.ProductGroup.ProductGroupName,
                                                                                         ProductName = JobOrderLineTab.Product.ProductName,
                                                                                         LotNo = JobReceiveLineTab.LotNo,
                                                                                         SiteName = H.Site.SiteName,
                                                                                         PONo = SOLTab.SaleOrderHeader.BuyerOrderNo + "{" + SOLTab.SaleOrderHeader.SaleToBuyer.Code + "}",
                                                                                         InvoiceNo = SITab.SaleInvoiceHeader.DocNo + "  " + SITab.SaleInvoiceHeader.DocDate,
                                                                                         InvoiceParty = PLTab.SaleOrderLine.SaleOrderHeader.BuyerOrderNo + "{" + SIBTab.Code + "}",
                                                                                         RollNo = PLTab.BaleNo,
                                                                                         CostcenterName = JobOrderLineTab.JobOrderHeader.CostCenter.CostCenterName,
                                                                                         Qty = JobReceiveLineTab.Qty,
                                                                                         UnitId = JobOrderLineTab.Product.UnitId,
                                                                                         DealUnitId = JobReceiveLineTab.DealUnitId,
                                                                                         UnitConversionMultiplier = JobReceiveQALineTab.UnitConversionMultiplier,
                                                                                         DealQty = JobReceiveQALineTab.DealQty,
                                                                                         Weight = JobReceiveLineTab.Weight,
                                                                                         UnitDecimalPlaces = JobOrderLineTab.Product.Unit.DecimalPlaces,
                                                                                         DealUnitDecimalPlaces = JobOrderLineTab.DealUnit.DecimalPlaces,
                                                                                         Rate = JobOrderLineTab.Rate,
                                                                                         XRate = JobOrderLineTab.Rate,
                                                                                         Amount = JobReceiveLineTab.DealQty * JobOrderLineTab.Rate,
                                                                                         PenaltyRate = JobReceiveLineTab.PenaltyRate,
                                                                                         PenaltyAmt = JobReceiveLineTab.PenaltyAmt,
                                                                                         DivisionId = H.DivisionId,
                                                                                         SiteId = H.SiteId,
                                                                                         ProcessId = H.ProcessId,
                                                                                         DocDate = H.DocDate,
                                                                                         DocTypeId = H.DocTypeId,
                                                                                         DocNo = H.DocNo,
                                                                                         ProductQualityName = FinishedProductTab.ProductQuality.ProductQualityName,
                                                                                         JobReceiveById = JobReceiveLineTab.JobReceiveHeader.JobReceiveById,
                                                                                         Remark = H.Remark,
                                                                                         Length = JobReceiveQALineExtendedTab.Length,
                                                                                         OrderLength = JobReceiveQALineExtendedTab.Length,
                                                                                         Width = JobReceiveQALineExtendedTab.Width,
                                                                                         OrderWidth = JobReceiveQALineExtendedTab.Width,
                                                                                         Height = JobReceiveQALineExtendedTab.Height,
                                                                                     }).FirstOrDefault();

            if (WeavingReceiveQADetail != null)
            {
                ProductDimensions ProductDimensions = new ProductService(_unitOfWork).GetProductDimensions(WeavingReceiveQADetail.ProductId, WeavingReceiveQADetail.DealUnitId, WeavingReceiveQADetail.DocTypeId);
                if (ProductDimensions != null)
                {
                    WeavingReceiveQADetail.DimensionUnitDecimalPlaces = ProductDimensions.DimensionUnitDecimalPlaces;
                }
                return Json(WeavingReceiveQADetail);
            }
            else
            {
                return null;
            }


        }



        public JsonResult GetJobOrderDetail_ByProductUidJson(string ProductUidName)
        {
            ProductUid PUS = new ProductUidService(_unitOfWork).Find(ProductUidName);

            JobOrderLine JOL = new JobOrderLineService(_unitOfWork).Find((int)PUS.GenLineId);

            var ProductUIDName = ProductUidName;

            var temp = (from L in db.ViewJobOrderBalance
                        join Dl in db.JobOrderLine on L.JobOrderLineId equals Dl.JobOrderLineId into JobOrderLineTable
                        from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                        join PU in db.ProductUid on L.ProductUidId equals PU.ProductUIDId into PUTable
                        from PUTab in PUTable.DefaultIfEmpty()
                        join P in db.Product on L.ProductId equals P.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join U in db.Units on ProductTab.UnitId equals U.UnitId into UnitTable
                        from UnitTab in UnitTable.DefaultIfEmpty()
                        join JW in db.Persons on L.JobWorkerId equals JW.PersonID into JWTable
                        from JWTab in JWTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on L.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on L.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where L.JobOrderLineId == (int)PUS.GenLineId
                        select new JobOrderDetail
                        {
                            JobOrderHeaderDocNo = L.JobOrderNo,
                            DocTypeId = JobOrderLineTab.JobOrderHeader.DocTypeId,
                            JobWorkerId = JobOrderLineTab.JobOrderHeader.JobWorkerId,
                            JobWorkerName = JWTab.Name,
                            CostCenterNo= JobOrderLineTab.JobOrderHeader.CostCenter.CostCenterName,
                            ProductUidId = PUTab.ProductUidName == null ? PUS.ProductUIDId : PUTab.ProductUIDId,
                            JobOrderLineId=L.JobOrderLineId,
                            ProductUidName = PUTab.ProductUidName == null ? ProductUIDName : PUTab.ProductUidName,
                            UnitId = UnitTab.UnitId,
                            UnitName = UnitTab.UnitName,
                            DealUnitId = JobOrderLineTab.DealUnitId,
                            UnitConversionMultiplier = JobOrderLineTab.UnitConversionMultiplier,
                            ProductId = L.ProductId,
                            ProductName = ProductTab.ProductName,
                            Rate = L.Rate,
                            BalanceQty = L.BalanceQty
                        }).FirstOrDefault();





            if (temp != null)
            {
                ProductDimensions ProductDimensions = new ProductService(_unitOfWork).GetProductDimensions(temp.ProductId, temp.DealUnitId, temp.DocTypeId);
                if (ProductDimensions != null)
                {
                    temp.Length = ProductDimensions.Length;
                    temp.Width = ProductDimensions.Width;
                    temp.Height = ProductDimensions.Height;
                    temp.DimensionUnitDecimalPlaces = ProductDimensions.DimensionUnitDecimalPlaces;
                }

                ProductViewModel PQ = new ProductService(_unitOfWork).GetFinishedProduct(temp.ProductId);
                if (PQ != null)
                {

                    temp.ProductQualityName = PQ.ProductQualityName;
                }

                Decimal UnitConversionMultiplier = 0;
                UnitConversionMultiplier = new ProductService(_unitOfWork).GetUnitConversionMultiplier(1, temp.UnitId, (decimal)temp.Length, (decimal)temp.Width, temp.Height, temp.DealUnitId, db);

                temp.UnitConversionMultiplier = UnitConversionMultiplier;
                return Json(temp);
            }
            else
            {
                return null;
            }


        }


        public string GetNewProductUid()
        {
            decimal Qty = 1;

            int DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.WeavingBazar).DocumentTypeId;
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


            JobReceiveSettings Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(DocTypeId, DivisionId, SiteId);
            List<string> uids = new List<string>();

            using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
            {
                sqlConnection.Open();

                int TypeId = DocTypeId;

                SqlCommand Totalf = new SqlCommand("SELECT * FROM " + Settings.SqlProcGenProductUID + "( " + TypeId + ", " + Qty + ",NULL)", sqlConnection);

                SqlDataReader ExcessStockQty = (Totalf.ExecuteReader());
                while (ExcessStockQty.Read())
                {
                    uids.Add((string)ExcessStockQty.GetValue(0));
                }
            }

            return uids.FirstOrDefault();
        }

        public string GetNewProductUid(int ProductId)
        {
            decimal Qty = 1;

            int DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.WeavingBazar).DocumentTypeId;
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


            JobReceiveSettings Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(DocTypeId, DivisionId, SiteId);
            List<string> uids = new List<string>();

            using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
            {
                sqlConnection.Open();

                int TypeId = DocTypeId;

                SqlCommand Totalf = new SqlCommand("SELECT * FROM " + Settings.SqlProcGenProductUID + "( " + TypeId + ", " + Qty + ", " + ProductId + ")", sqlConnection);

                SqlDataReader ExcessStockQty = (Totalf.ExecuteReader());
                while (ExcessStockQty.Read())
                {
                    uids.Add((string)ExcessStockQty.GetValue(0));
                }
            }

            return uids.FirstOrDefault();
        }

        public JsonResult SetSingleJobOrderLine(int Ids)
        {
            ComboBoxResult JobOrderJson = new ComboBoxResult();

            var JobOrderLine = from L in db.JobOrderLine
                                join H in db.JobOrderHeader on L.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
                                from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                                where L.JobOrderLineId == Ids
                                select new
                                {
                                    JobOrderLineId = L.JobOrderLineId,
                                    JobOrderNo = L.Product.ProductName
                                };

            JobOrderJson.id = JobOrderLine.FirstOrDefault().ToString();
            JobOrderJson.text = JobOrderLine.FirstOrDefault().JobOrderNo;

            return Json(JobOrderJson);
        }

        public ActionResult PendingRateUpdationIndex(int id)//DocumentTypeId
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            IQueryable<PendingWeavingReceives> PendingRateIndex = from H in db.JobReceiveHeader
                                                            join L in db.JobReceiveLine on H.JobReceiveHeaderId equals L.JobReceiveHeaderId into JobReceiveLineTable
                                                            from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                                                            join Jrql in db.JobReceiveQALine on JobReceiveLineTab.JobReceiveLineId equals Jrql.JobReceiveLineId into JobReceiveQALineTable
                                                            from JobReceiveQALineTab in JobReceiveQALineTable.DefaultIfEmpty()
                                                            join Jrqp in db.JobReceiveQAPenalty on JobReceiveQALineTab.JobReceiveQALineId equals Jrqp.JobReceiveQALineId into JobReceibeQAPenaltyTable
                                                            from JobReceiveQAPenaltyTab in JobReceibeQAPenaltyTable.DefaultIfEmpty()
                                                            where H.DocTypeId == id && JobReceiveQAPenaltyTab.ReasonId != null && JobReceiveQAPenaltyTab.Amount == 0 && H.SiteId == SiteId
                                                            group new { H, JobReceiveLineTab } by new { JobReceiveLineTab.ProductUidId } into Result
                                                            orderby Result.Key.ProductUidId
                                                            select new PendingWeavingReceives
                                                            {
                                                                JobReceiveHeaderId = Result.Max(m => m.H.JobReceiveHeaderId),
                                                                ProductUidName = Result.Max(m => m.JobReceiveLineTab.ProductUid.ProductUidName),
                                                                DocNo = Result.Max(m => m.H.DocNo),
                                                                DocDate = Result.Max(m => m.H.DocDate),
                                                                JobWorkerName = Result.Max(m => m.H.JobWorker.Name),
                                                                Status = Result.Max(m => m.H.Status)
                                                            };

            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.IndexStatus = "All";

            return View(PendingRateIndex);
        }


        public ActionResult PendingToImportFromBranch(int id)//DocumentTypeId
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            IQueryable<PendingWeavingReceives> PendingRateIndex = from H in db.JobReceiveHeader
                                                            join Hm in db.JobReceiveHeader on H.JobReceiveHeaderId equals Hm.ReferenceDocId into JobReceiveHeaderMainBranchTable
                                                            from JobReceiveHeaderMainBranchTab in JobReceiveHeaderMainBranchTable.DefaultIfEmpty()
                                                            join S in db.Site on H.SiteId equals S.SiteId into SiteTable from SiteTab in SiteTable.DefaultIfEmpty()
                                                            where H.DocTypeId == id && JobReceiveHeaderMainBranchTab.JobReceiveHeaderId == null && H.SiteId != MainBranchId 
                                                            orderby H.DocDate descending
                                                            select new PendingWeavingReceives
                                                            {
                                                                JobReceiveHeaderId = H.JobReceiveHeaderId,
                                                                DocNo = H.DocNo,
                                                                DocDate = H.DocDate,
                                                                JobWorkerName = SiteTab.SiteName,
                                                                Status = H.Status
                                                            };

            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.IndexStatus = "All";

            return View(PendingRateIndex);
        }

        public ActionResult ImportRecords(string Ids, int DocTypeId)
        {
            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


                int i = 0;
                int j = 0;
                int DocNoCnt = 0;
                foreach (var item in Ids.Split(',').Select(Int32.Parse))
                {
                    WeavingReceiveQACombinedViewModel vm = new WeavingReceiveQACombinedService(db).GetJobReceiveDetailForEdit(item);
                    int? PersonId = null;
                    PersonId = (from S in db.Site where S.SiteId == vm.SiteId select S).FirstOrDefault().PersonId;
                    if (PersonId != null)
                    {
                        vm.JobWorkerId = (int)PersonId;
                    }
                    if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
                    {
                        vm.GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];
                    }




                    vm.SiteId = MainBranchId;
                    
                    string DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
                    string[] tokens = DocNo.Split('-');
                    string Prefix = tokens[0];
                    int Counter = Convert.ToInt32(tokens[1]);
                    //vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
                    vm.DocNo = Prefix + "-" + (Counter + DocNoCnt).ToString().PadLeft(4).Replace(" ", "0");

                    int JobReceiveQALineId = vm.JobReceiveQALineId;

                    vm.ReferenceDocId = vm.JobReceiveHeaderId;
                    vm.ReferenceDocTypeId = vm.DocTypeId;
                    vm.JobReceiveHeaderId = i;
                    vm.JobReceiveLineId = i;
                    vm.JobReceiveQAHeaderId = i;
                    vm.JobReceiveQALineId = i;
                    vm.StockHeaderId = i;
                    vm.StockId = i;

                    JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(vm.JobOrderLineId);
                    if (JobOrderLine.ProdOrderLineId !=null)
                    {
                        ProdOrderLine ProdOrderLine = new ProdOrderLineService(_unitOfWork).Find((int)JobOrderLine.ProdOrderLineId);
                        vm.JobOrderLineId = (int)ProdOrderLine.ReferenceDocLineId;
                    }



                    JobReceiveHeader JobReceiveHeader = new JobReceiveHeader();
                    JobReceiveHeader = new WeavingReceiveQACombinedService(db).Create(vm, User.Identity.Name);


                    //var PenaltyList = new JobReceiveQAPenaltyService(db,_unitOfWork).GetLineListForIndex(vm.JobReceiveQALineId);
                    
                    var PenaltyList = (from L in db.JobReceiveQAPenalty where L.JobReceiveQALineId == JobReceiveQALineId select L).ToList();
                    foreach (var PenaltyItem in PenaltyList)
                    {
                        //JobReceiveQAPenaltyViewModel temp = new JobReceiveQAPenaltyService(db, _unitOfWork).GetJobReceiveQAPenaltyForEdit(PenaltyItem.JobReceiveQAPenaltyId);
                        JobReceiveQAPenaltyViewModel temp = (from p in db.JobReceiveQAPenalty
                                                             where p.JobReceiveQAPenaltyId == PenaltyItem.JobReceiveQAPenaltyId
                                                             select new JobReceiveQAPenaltyViewModel
                                                             {
                                                                 JobReceiveQAPenaltyId = p.JobReceiveQAPenaltyId,
                                                                 JobReceiveQALineId = p.JobReceiveQALineId,
                                                                 ReasonId = p.ReasonId,
                                                                 Amount = p.Amount,
                                                                 Remark = p.Remark,
                                                                 CreatedBy = p.CreatedBy,
                                                                 ModifiedBy = p.ModifiedBy,
                                                                 CreatedDate = p.CreatedDate,
                                                                 ModifiedDate = p.ModifiedDate,
                                                                 OMSId = p.OMSId
                                                             }).FirstOrDefault();
                        JobReceiveQAPenalty s = Mapper.Map<JobReceiveQAPenaltyViewModel, JobReceiveQAPenalty>(temp);
                        s.JobReceiveQALineId = vm.JobReceiveQALineId;
                        s.JobReceiveQAPenaltyId = j;
                        new JobReceiveQAPenaltyService(db, _unitOfWork).Create(s, User.Identity.Name);
                        j = j - 1;
                    }
                    i = i - 1;
                    DocNoCnt = DocNoCnt + 1;
                }

                try
                {
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                if (string.IsNullOrEmpty((string)TempData["CSEXC"]))
                    //return Json(new { redirectUrl = Url.Action("Index", new { id = DocTypeId }).Success("Data saved successfully");
                    return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet).Success("Imported successfully");
                else
                    return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Settings(int id)//Document Type Id
        {
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveSettings/Create/" + id);
        }


        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = new WeavingReceiveQACombinedService(db).GetCustomPerson(filter, searchTerm);
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

        public JsonResult GetStandardWeightson(int ProductId)
        {
            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);

            ProductWeight ProductWeight = db.Database.SqlQuery<ProductWeight>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetProductStandardWeight @ProductId", SqlParameterProductId).FirstOrDefault();

            return Json(ProductWeight);
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

    public class JobOrderDetail
    {
        public string JobOrderHeaderDocNo { get; set; }
        public string CostCenterNo { get; set; }
        public int ? JobOrderLineId { get; set; }
        public int DocTypeId { get; set; }
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string DealUnitId { get; set; }  
        public Decimal UnitConversionMultiplier { get; set; }  
        public int ProductId { get; set; }
        public Decimal Rate { get; set; }  
        public string ProductUidName { get; set; }
        public int? ProductUidId { get; set; }
        public Decimal BalanceQty { get; set; }
        public Decimal? Length { get; set; }
        public Decimal? Width { get; set; }
        public Decimal? Height { get; set; }
        public int? DimensionUnitDecimalPlaces { get; set; }
        public string ProductQualityName { get; set; }
        public string ProductName { get; set; }
    }

    public class PendingWeavingReceives
    {
        public int JobReceiveHeaderId { get; set; }
        public string ProductUidName { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string JobWorkerName { get; set; }
        public int Status { get; set; }

    }

    public class ProductWeight
    {
        public Decimal? StandardWeight { get; set; }
    }
}
