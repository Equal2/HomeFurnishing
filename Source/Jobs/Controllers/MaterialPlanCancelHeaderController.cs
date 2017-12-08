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
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Reports.Controllers;
using Reports.Reports;
using System.Configuration;
using Model.ViewModels;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class MaterialPlanCancelHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IMaterialPlanCancelHeaderService _MaterialPlanCancelHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public MaterialPlanCancelHeaderController(IMaterialPlanCancelHeaderService MaterialPlanCancelHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _MaterialPlanCancelHeaderService = MaterialPlanCancelHeaderService;
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
            //System.Web.HttpContext.Current.Session["MaterialPlanCancelType"] = MaterialPlanTypeConstants.ProdOrder;
            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }
        //public ActionResult DocumentTypeIndexSO(int id)//DocumentCategoryId
        //{
        //    var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();
        //    System.Web.HttpContext.Current.Session["MaterialPlanCancelType"] = MaterialPlanTypeConstants.SaleOrder;
        //    if (p != null)
        //    {
        //        if (p.Count == 1)
        //            return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
        //    }

        //    return View("DocumentTypeListSO", p);
        //}

        private void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId);
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();
            if (settings != null)
            {
                ViewBag.WizardId = settings.WizardMenuId;
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;                
                ViewBag.ExportMenuId = settings.ExportMenuId;
            }

            }

        // GET: /MaterialPlanCancelHeaderMaster/

        public ActionResult Index(int id, string IndexType)//DocumentTypeID
        {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }

            var MaterialPlanCancelHeader = _MaterialPlanCancelHeaderService.GetMaterialPlanCancelHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(MaterialPlanCancelHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _MaterialPlanCancelHeaderService.GetMaterialPlanCancelHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _MaterialPlanCancelHeaderService.GetMaterialPlanCancelHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }


        // GET: /MaterialPlanCancelHeaderMaster/Create

        public ActionResult Create(int id)//DocumentTypeID
        {
            MaterialPlanCancelHeaderViewModel vm = new MaterialPlanCancelHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.DocNo = _MaterialPlanCancelHeaderService.GetMaxDocNo();
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "MaterialPlanSettings", new { id = id }).Warning("Please create job order settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.MaterialPlanSettings = Mapper.Map<MaterialPlanSettings, MaterialPlanSettingsViewModel>(settings);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            if(string.IsNullOrEmpty(vm.MaterialPlanSettings.PlanType))
                TempData["CSEXC"] += "Please configure PlanType";

            vm.DocDate = DateTime.Now;
            vm.DueDate = DateTime.Now;
            vm.DocTypeId = id;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".MaterialPlanCancelHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(MaterialPlanCancelHeaderViewModel vm)
        {
            MaterialPlanCancelHeader pt = AutoMapper.Mapper.Map<MaterialPlanCancelHeaderViewModel, MaterialPlanCancelHeader>(vm);

            #region DocTypeTimeLineValidation

            try
            {

                if (vm.MaterialPlanCancelHeaderId <= 0)
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

            if (ModelState.IsValid && (TimePlanValidation || Continue))
            {

                #region CreateRecord
                if (vm.MaterialPlanCancelHeaderId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _MaterialPlanCancelHeaderService.Create(pt);
                  
                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        ViewBag.Mode = "Add";
                        ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(vm.DocTypeId).DocumentTypeName;
                        ViewBag.id = vm.DocTypeId;
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.MaterialPlanCancelHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    return RedirectToAction("Modify", new { id = pt.MaterialPlanCancelHeaderId }).Success("Data saved successfully");
                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    MaterialPlanCancelHeader temp = _MaterialPlanCancelHeaderService.Find(pt.MaterialPlanCancelHeaderId);
                   
                    MaterialPlanCancelHeader ExRec = Mapper.Map<MaterialPlanCancelHeader>(temp);

                    temp.DocNo = pt.DocNo;
                    temp.DocDate = pt.DocDate;
                    temp.Remark = pt.Remark;
                    temp.BuyerId = pt.BuyerId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _MaterialPlanCancelHeaderService.Update(temp);

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
                        ViewBag.Mode = "Edit";
                        ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(vm.DocTypeId).DocumentTypeName;
                        ViewBag.id = vm.DocTypeId;
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.MaterialPlanCancelHeaderId,
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
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(vm.DocTypeId).DocumentTypeName;
            ViewBag.id = vm.DocTypeId;
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            MaterialPlanCancelHeader header = _MaterialPlanCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            MaterialPlanCancelHeader header = _MaterialPlanCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }



        // GET: /ProductMaster/Edit/5

        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            MaterialPlanCancelHeader pt = _MaterialPlanCancelHeaderService.Find(id);
            MaterialPlanCancelHeaderViewModel vm = AutoMapper.Mapper.Map<MaterialPlanCancelHeader, MaterialPlanCancelHeaderViewModel>(pt);
            if (pt == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, pt.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
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

            //Getting Settings
            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(pt.DocTypeId, vm.DivisionId, vm.SiteId);
            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "MaterialPlanSettings", new { id = pt.DocTypeId }).Warning("Please create Material plan settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.MaterialPlanSettings = Mapper.Map<MaterialPlanSettings, MaterialPlanSettingsViewModel>(settings);
            ViewBag.Mode = "Edit";
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(pt.DocTypeId).DocumentTypeName;
            ViewBag.id = pt.DocTypeId;

            if (string.IsNullOrEmpty(vm.MaterialPlanSettings.PlanType))
                TempData["CSEXC"] += "Please configure PlanType";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.MaterialPlanCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", vm);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            MaterialPlanCancelHeader header = _MaterialPlanCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            MaterialPlanCancelHeader header = _MaterialPlanCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }


        // GET: /ProductMaster/Delete/5

        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaterialPlanCancelHeader MaterialPlanCancelHeader = db.MaterialPlanCancelHeader.Find(id);
            if (MaterialPlanCancelHeader == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, MaterialPlanCancelHeader.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(MaterialPlanCancelHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
            if (ModelState.IsValid)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                db.Configuration.AutoDetectChangesEnabled = false;
                var temp = _MaterialPlanCancelHeaderService.Find(vm.id);

                int status = temp.Status;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                var MaterialPlanCancelline = new MaterialPlanCancelLineService(_unitOfWork).GetMaterialPlanCancelForDelete(vm.id).ToList();

                List<ProdOrderCancelLine> ProdOrderCancelLines = new List<ProdOrderCancelLine>();
                List<PurchaseIndentCancelLine> PurchaseIndentCancelLines = new List<PurchaseIndentCancelLine>();


                foreach (var item in MaterialPlanCancelline)
                {

                    //Deleting MaterialPlanCancelforSaleOrder
                    var MaterialPlanCancelForSaleOrder = new MaterialPlanCancelForSaleOrderService(_unitOfWork).GetMaterialPlanCancelForSaleOrderForMaterialPlanCancelline(item.MaterialPlanCancelLineId).ToList();
                    foreach (var item2 in MaterialPlanCancelForSaleOrder)
                    {
                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = item2,
                        });

                        item2.ObjectState = Model.ObjectState.Deleted;
                        //new MaterialPlanCancelForSaleOrderService(_unitOfWork).Delete(item2);
                        db.MaterialPlanCancelForSaleOrder.Attach(item2);
                        db.MaterialPlanCancelForSaleOrder.Remove(item2);
                    }

                    //Deleting MaterialPlanCancelForProdOrderLine
                    var MaterialPlanCancelForProdOrderLine = new MaterialPlanCancelForProdOrderLineService(_unitOfWork).GetMaterialPlanCancelForProdORderForMaterialPlanCancel(item.MaterialPlanCancelLineId).ToList();
                    foreach (var item2 in MaterialPlanCancelForProdOrderLine)
                    {

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = item2,
                        });

                        item2.ObjectState = Model.ObjectState.Deleted;
                        //new MaterialPlanCancelForProdOrderLineService(_unitOfWork).Delete(item2.MaterialPlanCancelForProdOrderLineId);
                        db.MaterialPlanCancelForProdOrderLine.Attach(item2);
                        db.MaterialPlanCancelForProdOrderLine.Remove(item2);
                    }

                    //new MaterialPlanCancelLineService(_unitOfWork).Delete(item.MaterialPlanCancelLineId);

                    //MaterialPlanCancelLine Si = (MaterialPlanCancelLine) item;







                    ProdOrderCancelLines = new ProdOrderCancelLineService(_unitOfWork).GetProdOrderCancelLineForMaterialPlanCancel(item.MaterialPlanCancelLineId).ToList();

                    foreach (var item2 in ProdOrderCancelLines)
                    {
                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = item2,
                        });

                        item2.ObjectState = Model.ObjectState.Deleted;
                        db.ProdOrderCancelLine.Attach(item2);
                        db.ProdOrderCancelLine.Remove(item2);
                    }

                    PurchaseIndentCancelLines = new PurchaseIndentCancelLineService(_unitOfWork).GetPurchaseIndentCancelLineForMaterialPlanCancel(item.MaterialPlanCancelLineId).ToList();

                    foreach (var item2 in PurchaseIndentCancelLines)
                    {
                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = item2,
                        });

                        item2.ObjectState = Model.ObjectState.Deleted;
                        db.PurchaseIndentCancelLine.Attach(item2);
                        db.PurchaseIndentCancelLine.Remove(item2);
                    }





                    MaterialPlanCancelLine MPL = new MaterialPlanCancelLineService(_unitOfWork).Find(item.MaterialPlanCancelLineId);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = MPL,
                    });

                    MPL.ObjectState = Model.ObjectState.Deleted;

                    db.MaterialPlanCancelLine.Attach(MPL);
                    db.MaterialPlanCancelLine.Remove(MPL);
                }

                //Deleting MaterialPlanCancelForProdORder
                var MaterialPlanCancelForProdOrder = new MaterialPlanCancelForProdOrderService(_unitOfWork).GetMaterialPlanCancelForProdORderForMaterialPlanCancel(vm.id).ToList();
                foreach (var item2 in MaterialPlanCancelForProdOrder)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item2,
                    });

                    item2.ObjectState = Model.ObjectState.Deleted;
                    //new MaterialPlanCancelForProdOrderService(_unitOfWork).Delete(item2);
                    db.MaterialPlanCancelForProdOrder.Attach(item2);
                    db.MaterialPlanCancelForProdOrder.Remove(item2);
                }





                ProdOrderCancelHeader ProdOrderCancelHeaders = new ProdOrderCancelHeaderService(_unitOfWork).GetProdOrderCancelForMaterialPlan(vm.id);


                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ProdOrderCancelHeaders,
                });

                ProdOrderCancelHeaders.ObjectState = Model.ObjectState.Deleted;
                // new ProdOrderHeaderService(_unitOfWork).Delete(item2.ProdOrderHeaderId);
                db.ProdOrderCancelHeader.Attach(ProdOrderCancelHeaders);
                db.ProdOrderCancelHeader.Remove(ProdOrderCancelHeaders);








                temp.ObjectState = Model.ObjectState.Deleted;
                // _MaterialPlanCancelHeaderService.Delete(temp);
                db.MaterialPlanCancelHeader.Attach(temp);
                db.MaterialPlanCancelHeader.Remove(temp);

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    // _unitOfWork.Save();
                    db.SaveChanges();
                    db.Configuration.AutoDetectChangesEnabled = true;
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
                    DocId = temp.MaterialPlanCancelHeaderId,
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
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.MaterialPlanCancelHeaders", "MaterialPlanCancelHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.MaterialPlanCancelHeaders", "MaterialPlanCancelHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }


        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                var Settings = new MaterialPlanCancelLineService(_unitOfWork).GetMaterialPlanSettingsForDocument(DocTypeId, DivisionId, SiteId);

                if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "GeneratePrints") == false)
                {
                    return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
                }

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = db.MaterialPlanCancelHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.MaterialPlanCancelHeaderId,
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
                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

                            PdfStream.Add(Pdf);
                        }
                        else
                        {
                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);
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
        public ActionResult DetailInformation(int id, int? DocLineId, string IndexType)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail", IndexType = IndexType, DocLineId = DocLineId });
        }

        [Authorize]
        public ActionResult Detail(int id, string transactionType, string IndexType, int? DocLineId)
        {

            if (DocLineId.HasValue)
                ViewBag.DocLineId = DocLineId;

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            MaterialPlanCancelHeader pt = _MaterialPlanCancelHeaderService.Find(id);
            MaterialPlanCancelHeaderViewModel vm = AutoMapper.Mapper.Map<MaterialPlanCancelHeader, MaterialPlanCancelHeaderViewModel>(pt);
            if (pt == null)
            {
                return HttpNotFound();
            }
            //Getting Settings
            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(pt.DocTypeId, vm.DivisionId, vm.SiteId);
            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "MaterialPlanSettings", new { id = pt.DocTypeId }).Warning("Please create Material plan settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.MaterialPlanSettings = Mapper.Map<MaterialPlanSettings, MaterialPlanSettingsViewModel>(settings);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(pt.DocTypeId).DocumentTypeName;
            ViewBag.id = pt.DocTypeId;

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.MaterialPlanCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));


            return View("Create", vm);
        }



        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            MaterialPlanCancelHeader s = db.MaterialPlanCancelHeader.Find(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Submit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation

            

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

            MaterialPlanCancelHeader pd = new MaterialPlanCancelHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {

                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {


                    pd.Status = (int)StatusConstants.Submitted;
                    pd.ReviewBy = null;

                    _MaterialPlanCancelHeaderService.Update(pd);

                    _unitOfWork.Save();

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.MaterialPlanCancelHeaderId,
                        ActivityType = (int)ActivityTypeContants.Submitted,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));
                
                    //SendEmail_PODrafted(Id);
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
            MaterialPlanCancelHeader pd = new MaterialPlanCancelHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                _MaterialPlanCancelHeaderService.Update(pd);

                _unitOfWork.Save();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.MaterialPlanCancelHeaderId,
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
            return (_MaterialPlanCancelHeaderService.GetMaterialPlanCancelHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_MaterialPlanCancelHeaderService.GetMaterialPlanCancelHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _MaterialPlanCancelHeaderService.GetCustomPerson(filter, searchTerm);
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
