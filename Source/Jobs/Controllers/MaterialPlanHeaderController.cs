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
using System.Data.SqlClient;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class MaterialPlanHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IMaterialPlanHeaderService _MaterialPlanHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public MaterialPlanHeaderController(IMaterialPlanHeaderService MaterialPlanHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _MaterialPlanHeaderService = MaterialPlanHeaderService;
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
            //System.Web.HttpContext.Current.Session["MaterialPlanType"] = MaterialPlanTypeConstants.ProdOrder;
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
        //    System.Web.HttpContext.Current.Session["MaterialPlanType"] = MaterialPlanTypeConstants.SaleOrder;
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

        // GET: /MaterialPlanHeaderMaster/

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

            var MaterialPlanHeader = _MaterialPlanHeaderService.GetMaterialPlanHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(MaterialPlanHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _MaterialPlanHeaderService.GetMaterialPlanHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _MaterialPlanHeaderService.GetMaterialPlanHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }


        // GET: /MaterialPlanHeaderMaster/Create

        public ActionResult Create(int id)//DocumentTypeID
        {
            MaterialPlanHeaderViewModel vm = new MaterialPlanHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.DocNo = _MaterialPlanHeaderService.GetMaxDocNo();
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

            if (string.IsNullOrEmpty(vm.MaterialPlanSettings.PlanType))
                TempData["CSEXC"] += "Please configure PlanType";

            vm.DocDate = DateTime.Now;
            vm.DueDate = DateTime.Now;
            vm.DocTypeId = id;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".MaterialPlanHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
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
        public ActionResult Post(MaterialPlanHeaderViewModel vm)
        {
            MaterialPlanHeader pt = AutoMapper.Mapper.Map<MaterialPlanHeaderViewModel, MaterialPlanHeader>(vm);

            #region DocTypeTimeLineValidation

            try
            {

                if (vm.MaterialPlanHeaderId <= 0)
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

            if (vm.MaterialPlanSettings.isMandatoryMachine == true && vm.MachineId == null)
            {
                ModelState.AddModelError("MachineId", "Machine field is required");
            }

            if (ModelState.IsValid && (TimePlanValidation || Continue))
            {

                #region CreateRecord
                if (vm.MaterialPlanHeaderId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _MaterialPlanHeaderService.Create(pt);
                  
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
                        DocId = pt.MaterialPlanHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    return RedirectToAction("Modify", new { id = pt.MaterialPlanHeaderId }).Success("Data saved successfully");
                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    MaterialPlanHeader temp = _MaterialPlanHeaderService.Find(pt.MaterialPlanHeaderId);
                   
                    MaterialPlanHeader ExRec = Mapper.Map<MaterialPlanHeader>(temp);

                    temp.DocNo = pt.DocNo;
                    temp.DocDate = pt.DocDate;
                    temp.DueDate = pt.DueDate;
                    temp.Remark = pt.Remark;
                    temp.BuyerId = pt.BuyerId;
                    temp.MachineId = pt.MachineId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _MaterialPlanHeaderService.Update(temp);

                    PurchaseIndentHeader ExistingIndent = new PurchaseIndentHeaderService(_unitOfWork).GetPurchaseIndentForMaterialPlan(temp.MaterialPlanHeaderId);

                    ProdOrderHeader ExistingProdOrder = new ProdOrderHeaderService(_unitOfWork).GetProdOrderForMaterialPlan(temp.MaterialPlanHeaderId);

                    if (ExistingIndent != null)
                    {
                        ExistingIndent.ModifiedBy = User.Identity.Name;
                        ExistingIndent.ModifiedDate = DateTime.Now;
                        ExistingIndent.Remark = temp.Remark;
                        ExistingIndent.ObjectState = Model.ObjectState.Added;
                        new PurchaseIndentHeaderService(_unitOfWork).Update(ExistingIndent);
                    }

                    if (ExistingProdOrder != null)
                    {
                        ExistingProdOrder.ModifiedBy = User.Identity.Name;
                        ExistingProdOrder.ModifiedDate = DateTime.Now;
                        ExistingProdOrder.Remark = temp.Remark;
                        ExistingProdOrder.ObjectState = Model.ObjectState.Added;
                        new ProdOrderHeaderService(_unitOfWork).Update(ExistingProdOrder);
                    }

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
                        DocId = temp.MaterialPlanHeaderId,
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
            MaterialPlanHeader header = _MaterialPlanHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            MaterialPlanHeader header = _MaterialPlanHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }



        // GET: /ProductMaster/Edit/5

        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            MaterialPlanHeader pt = _MaterialPlanHeaderService.Find(id);
            MaterialPlanHeaderViewModel vm = AutoMapper.Mapper.Map<MaterialPlanHeader, MaterialPlanHeaderViewModel>(pt);
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
                    DocId = pt.MaterialPlanHeaderId,
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
            MaterialPlanHeader header = _MaterialPlanHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            MaterialPlanHeader header = _MaterialPlanHeaderService.Find(id);
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
            MaterialPlanHeader MaterialPlanHeader = db.MaterialPlanHeader.Find(id);
            if (MaterialPlanHeader == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, MaterialPlanHeader.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(MaterialPlanHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
                var temp = _MaterialPlanHeaderService.Find(vm.id);

                int status = temp.Status;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });


                List<ProdOrderLine> ProdOrderLines = new List<ProdOrderLine>();
                List<PurchaseIndentLine> PurchaseIndentLines = new List<PurchaseIndentLine>();


                var materialplanline = new MaterialPlanLineService(_unitOfWork).GetMaterialPlanForDelete(vm.id).ToList();

                foreach (var item in materialplanline)
                {
                    //Deleting ProdOrderLines & HEaders
                    ProdOrderLines = new ProdOrderLineService(_unitOfWork).GetProdOrderLineForMaterialPlan(item.MaterialPlanLineId).ToList();

                    int[] LineIds = ProdOrderLines.Select(m => m.ProdOrderLineId).ToArray();

                    List<ProdOrderLineStatus> LineStatus = (from p in db.ProdOrderLineStatus
                                                            where LineIds.Contains(p.ProdOrderLineId.Value)
                                                            select p).ToList();

                    foreach (var LineStatitem in LineStatus)
                    {
                        LineStatitem.ObjectState = Model.ObjectState.Deleted;
                        db.ProdOrderLineStatus.Attach(LineStatitem);
                        db.ProdOrderLineStatus.Remove(LineStatitem);
                    }

                    foreach (var item2 in ProdOrderLines)
                    {

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = item2,
                        });

                        item2.ObjectState = Model.ObjectState.Deleted;
                        //new ProdOrderLineService(_unitOfWork).Delete(item2.ProdOrderLineId);
                        db.ProdOrderLine.Attach(item2);
                        db.ProdOrderLine.Remove(item2);
                    }


                    //Deleting PurchaseIndentLines & Headers
                    PurchaseIndentLines = new PurchaseIndentLineService(_unitOfWork).GetPurchaseIndentLineForMaterialPlan(item.MaterialPlanLineId).ToList();
                    foreach (var item2 in PurchaseIndentLines)
                    {

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = item2,
                        });

                        item2.ObjectState = Model.ObjectState.Deleted;
                        //new PurchaseIndentLineService(_unitOfWork).Delete(item2);
                        db.PurchaseIndentLine.Attach(item2);
                        db.PurchaseIndentLine.Remove(item2);
                    }


                    //Deleting MaterialplanforSaleOrder
                    var MAterialPlanForSaleOrder = new MaterialPlanForSaleOrderService(_unitOfWork).GetMaterialPlanForSaleOrderForMaterialPlanline(item.MaterialPlanLineId).ToList();
                    foreach (var item2 in MAterialPlanForSaleOrder)
                    {
                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = item2,
                        });

                        item2.ObjectState = Model.ObjectState.Deleted;
                        //new MaterialPlanForSaleOrderService(_unitOfWork).Delete(item2);
                        db.MaterialPlanForSaleOrder.Attach(item2);
                        db.MaterialPlanForSaleOrder.Remove(item2);
                    }

                    //Deleting MaterialPlanForProdOrderLine
                    var MAterialPlanForProdOrderLine = new MaterialPlanForProdOrderLineService(_unitOfWork).GetMAterialPlanForProdORderForMaterialPlan(item.MaterialPlanLineId).ToList();
                    foreach (var item2 in MAterialPlanForProdOrderLine)
                    {

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = item2,
                        });

                        item2.ObjectState = Model.ObjectState.Deleted;
                        //new MaterialPlanForProdOrderLineService(_unitOfWork).Delete(item2.MaterialPlanForProdOrderLineId);
                        db.MaterialPlanForProdOrderLine.Attach(item2);
                        db.MaterialPlanForProdOrderLine.Remove(item2);
                    }

                    //new MaterialPlanLineService(_unitOfWork).Delete(item.MaterialPlanLineId);

                    //MaterialPlanLine Si = (MaterialPlanLine) item;
                    MaterialPlanLine MPL = new MaterialPlanLineService(_unitOfWork).Find(item.MaterialPlanLineId);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = MPL,
                    });

                    MPL.ObjectState = Model.ObjectState.Deleted;

                    db.MaterialPlanLine.Attach(MPL);
                    db.MaterialPlanLine.Remove(MPL);
                }



                var ProdORderHeaders = new ProdOrderHeaderService(_unitOfWork).GetProdOrderListForMaterialPlan(vm.id).ToList();

                int[] ProdOrderIds = ProdORderHeaders.Select(m => m.ProdOrderHeaderId).ToArray();

                List<ProdOrderHeaderStatus> ProdOrderHeaderStatus = (from p in db.ProdOrderHeaderStatus
                                                                     where ProdOrderIds.Contains(p.ProdOrderHeaderId.Value)
                                                                     select p).ToList();

                foreach (var StatItem in ProdOrderHeaderStatus)
                {
                    StatItem.ObjectState = Model.ObjectState.Deleted;
                    db.ProdOrderHeaderStatus.Attach(StatItem);
                    db.ProdOrderHeaderStatus.Remove(StatItem);
                }
                foreach (var item2 in ProdORderHeaders)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item2,
                    });

                    item2.ObjectState = Model.ObjectState.Deleted;
                    // new ProdOrderHeaderService(_unitOfWork).Delete(item2.ProdOrderHeaderId);
                    db.ProdOrderHeader.Attach(item2);
                    db.ProdOrderHeader.Remove(item2);

                }
                var PurchaseIndentHeaders = new PurchaseIndentHeaderService(_unitOfWork).GetPurchaseIndentListForMAterialPlan(vm.id).ToList();
                foreach (var item2 in PurchaseIndentHeaders)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item2,
                    });

                    item2.ObjectState = Model.ObjectState.Deleted;
                    //new PurchaseIndentHeaderService(_unitOfWork).Delete(item2.PurchaseIndentHeaderId);
                    db.PurchaseIndentHeader.Attach(item2);
                    db.PurchaseIndentHeader.Remove(item2);
                }

                //Deleting MaterialPlanForProdORder
                var MaterialPlanForProdOrder = new MaterialPlanForProdOrderService(_unitOfWork).GetMAterialPlanForProdORderForMaterialPlan(vm.id).ToList();
                foreach (var item2 in MaterialPlanForProdOrder)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item2,
                    });

                    item2.ObjectState = Model.ObjectState.Deleted;
                    //new MaterialPlanForProdOrderService(_unitOfWork).Delete(item2);
                    db.MaterialPlanForProdOrder.Attach(item2);
                    db.MaterialPlanForProdOrder.Remove(item2);
                }

                temp.ObjectState = Model.ObjectState.Deleted;
                // _MaterialPlanHeaderService.Delete(temp);
                db.MaterialPlanHeader.Attach(temp);
                db.MaterialPlanHeader.Remove(temp);

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
                    DocId = temp.MaterialPlanHeaderId,
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
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.MaterialPlanHeaders", "MaterialPlanHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.MaterialPlanHeaders", "MaterialPlanHeaderId", PrevNextConstants.Prev);
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

                var Settings = new MaterialPlanLineService(_unitOfWork).GetMaterialPlanSettingsForDocument(DocTypeId, DivisionId, SiteId);

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

                        var pd = db.MaterialPlanHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.MaterialPlanHeaderId,
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

            MaterialPlanHeader pt = _MaterialPlanHeaderService.Find(id);
            MaterialPlanHeaderViewModel vm = AutoMapper.Mapper.Map<MaterialPlanHeader, MaterialPlanHeaderViewModel>(pt);
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
                    DocId = pt.MaterialPlanHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));


            return View("Create", vm);
        }



        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            MaterialPlanHeader s = db.MaterialPlanHeader.Find(id);

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

            MaterialPlanHeader pd = new MaterialPlanHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {

                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {
                    SqlParameter SqlMaterialPlanHeaderId = new SqlParameter("@MaterialPlanHeaderId", Id);
                    db.Database.SqlQuery<int>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_UpdateMaterialPlanForSaleOrder @MaterialPlanHeaderId", SqlMaterialPlanHeaderId).FirstOrDefault();


                    pd.Status = (int)StatusConstants.Submitted;
                    pd.ReviewBy = null;

                    _MaterialPlanHeaderService.Update(pd);

                    _unitOfWork.Save();


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.MaterialPlanHeaderId,
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
            MaterialPlanHeader pd = new MaterialPlanHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                _MaterialPlanHeaderService.Update(pd);

                _unitOfWork.Save();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.MaterialPlanHeaderId,
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
            return (_MaterialPlanHeaderService.GetMaterialPlanHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_MaterialPlanHeaderService.GetMaterialPlanHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }

        public ActionResult Wizard(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            MaterialPlanHeaderViewModel vm = new MaterialPlanHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings != null)
            {
                if (settings.WizardMenuId != null)
                {
                    
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.WizardMenuId);

                    if (menuviewmodel == null)
                    {
                        return View("~/Views/Shared/UnderImplementation.cshtml");
                    }
                    else if (!string.IsNullOrEmpty(menuviewmodel.URL))
                    {   
                        //return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + menuviewmodel.RouteId + "?MenuId=" + menuviewmodel.MenuId);
                        return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + "Index" + "/" + id + "?MenuId=" + menuviewmodel.MenuId);
                    }
                    else
                    {
                        return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = menuviewmodel.RouteId });
                        
                        
                    }
                }
            }
            return RedirectToAction("Index", new { id = id });
        }

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _MaterialPlanHeaderService.GetCustomPerson(filter, searchTerm);
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
