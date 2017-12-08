using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Model.ViewModel;
using System.Xml.Linq;
using JobConsumptionDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class JobConsumptionLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IStockLineService _StockLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobConsumptionLineController(IStockLineService Stock, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _StockLineService = Stock;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult _Multi(int id, int sid, int? CosCenteId, int ProcId)
        {
            StockLineFilterViewModel vm = new StockLineFilterViewModel();

            StockHeader Header = new StockHeaderService(_unitOfWork).Find(id);

            StockHeaderSettings settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            vm.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            vm.StockHeaderId = id;
            vm.JobWorkerId = sid;
            vm.CostCenterId = CosCenteId;
            vm.ProcessId = ProcId;
            return PartialView("_Filters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(StockLineFilterViewModel vm)
        {
            var stkConsumptionForFilter = _StockLineService.GetJobConsumptionForFilters(vm).ToList();
            List<StockLineViewModel> temp = stkConsumptionForFilter;
            StockMasterDetailModel svm = new StockMasterDetailModel();
            svm.StockLineViewModel = temp;
            StockHeader Header = new StockHeaderService(_unitOfWork).Find(vm.StockHeaderId);
            StockHeaderSettings Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            svm.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(Settings);


            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(StockMasterDetailModel vm)
        {
            StockHeader temp = new StockHeaderService(_unitOfWork).Find(vm.StockLineViewModel.FirstOrDefault().StockHeaderId);
            StockHeaderSettings Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobConsumptionDocEvents.beforeLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            int Cnt = 0;
            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                foreach (var item in vm.StockLineViewModel)
                {
                    if (item.Qty != 0 && (Settings.isMandatoryLineCostCenter == true ? item.CostCenterId.HasValue : 1 == 1))
                    {
                        StockLine line = new StockLine();
                        line.StockHeaderId = item.StockHeaderId;
                        line.Qty = item.Qty;
                        line.ProductId = item.ProductId;
                        line.LotNo = item.LotNo;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.Dimension3Id = item.Dimension3Id;
                        line.Dimension4Id = item.Dimension4Id;
                        line.CostCenterId = item.CostCenterId;
                        line.FromProcessId = item.FromProcessId;
                        line.Specification = item.Specification;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.DocNature = ( item.Qty < 0 ? StockNatureConstants.Receive : StockNatureConstants.Issue );


                        StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                        if (temp.StockHeaderId != null && temp.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                        {
                            StockProcessViewModel.StockHeaderId = (int)temp.StockHeaderId;
                        }
                        else if (Cnt > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                        {
                            StockProcessViewModel.StockHeaderId = -1;
                        }
                        else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                        {
                            StockProcessViewModel.StockHeaderId = 0;
                        }
                        StockProcessViewModel.StockProcessId = -Cnt;
                        StockProcessViewModel.DocHeaderId = temp.StockHeaderId;
                        StockProcessViewModel.DocLineId = line.StockLineId;
                        StockProcessViewModel.DocTypeId = temp.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                        StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                        StockProcessViewModel.DocNo = temp.DocNo;
                        StockProcessViewModel.DivisionId = temp.DivisionId;
                        StockProcessViewModel.SiteId = temp.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.PersonId = temp.PersonId;
                        StockProcessViewModel.ProductId = item.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = temp.GodownId;
                        StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                        StockProcessViewModel.GodownId = temp.GodownId;
                        StockProcessViewModel.Remark = temp.Remark;
                        StockProcessViewModel.Status = temp.Status;
                        StockProcessViewModel.ProcessId = temp.ProcessId;
                        StockProcessViewModel.LotNo = null;
                        StockProcessViewModel.CostCenterId = item.CostCenterId;
                        //StockProcessViewModel.Qty_Iss = item.Qty;
                        //StockProcessViewModel.Qty_Rec = 0;

                        if (item.Qty > 0)
                        {
                            StockProcessViewModel.Qty_Rec = item.Qty;
                            StockProcessViewModel.Qty_Iss = 0;
                        }
                        else if (item.Qty < 0)
                        {
                            StockProcessViewModel.Qty_Rec = 0;
                            StockProcessViewModel.Qty_Iss = Math.Abs(item.Qty); 
                        }

                        StockProcessViewModel.Rate = item.Rate;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = item.Specification;
                        StockProcessViewModel.Dimension1Id = item.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = item.Dimension2Id;
                        StockProcessViewModel.Dimension3Id = item.Dimension3Id;
                        StockProcessViewModel.Dimension4Id = item.Dimension4Id;
                        StockProcessViewModel.CreatedBy = User.Identity.Name;
                        StockProcessViewModel.CreatedDate = DateTime.Now;
                        StockProcessViewModel.ModifiedBy = User.Identity.Name;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            string message = StockProcessPostingError;
                            ModelState.AddModelError("", message);
                            return PartialView("_Results", vm);
                        }

                        line.StockProcessId = StockProcessViewModel.StockProcessId;

                        line.ObjectState = Model.ObjectState.Added;
                        db.StockLine.Add(line);
                        //_StockLineService.Create(line);
                        Cnt = Cnt + 1;
                    }
                }


                if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                {
                    temp.Status = (int)StatusConstants.Modified;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ModifiedDate = DateTime.Now;

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(temp);
                }

                try
                {
                    JobConsumptionDocEvents.onLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
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
                    TempData["CSEXCL"] += message;
                    return PartialView("_Results", vm);
                }

                try
                {
                    JobConsumptionDocEvents.afterLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.StockHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = temp.DocNo,
                    DocDate = temp.DocDate,
                    DocStatus = temp.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _StockLineService.GetStockLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }
        private void PrepareViewBag(StockLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            StockHeaderViewModel H = new StockHeaderService(_unitOfWork).GetStockHeader(vm.StockHeaderId);
            ViewBag.DocNo = H.DocTypeName + "-" + H.DocNo;
        }

        [HttpGet]
        public ActionResult CreateLine(int Id)
        {
            return _Create(Id);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int Id)
        {
            return _Create(Id);
        }


        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            StockHeader H = new StockHeaderService(_unitOfWork).Find(Id);
            StockLineViewModel s = new StockLineViewModel();

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            s.ProcessId = H.ProcessId;
            //s.CostCenterId = H.CostCenterId;
            s.StockHeaderId = H.StockHeaderId;
            s.GodownId = H.GodownId;
            ViewBag.Status = H.Status;
            PrepareViewBag(s);
            if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            {
                ViewBag.CSEXCL = TempData["CSEXCL"];
                TempData["CSEXCL"] = null;
            }
            return PartialView("_Create", s);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(StockLineViewModel svm)
        {
            StockHeader temp = new StockHeaderService(_unitOfWork).Find(svm.StockHeaderId);

            StockLine s = Mapper.Map<StockLineViewModel, StockLine>(svm);

            if (svm.StockHeaderSettings != null)
            {
                if (svm.StockHeaderSettings.isMandatoryProcessLine == true && (svm.FromProcessId <= 0 || svm.FromProcessId == null))
                {
                    ModelState.AddModelError("FromProcessId", "The Process field is required");
                }
                if (svm.StockHeaderSettings.isVisibleLineCostCenter == true && svm.StockHeaderSettings.isMandatoryLineCostCenter == true && (!svm.CostCenterId.HasValue))
                {
                    ModelState.AddModelError("CostCenterId", "The CostCenter field is required");
                }
            }

            bool BeforeSave = true;
            try
            {

                if (svm.StockLineId <= 0)
                    BeforeSave = JobConsumptionDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobConsumptionDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");

            if (svm.StockLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (svm.StockLineId <= 0)
                {
                    StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                    StockHeader StockHeader = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId);

                    //Posting in StockProcess
                    if (StockHeader.StockHeaderId != null)
                    {
                        StockProcessViewModel.StockHeaderId = StockHeader.StockHeaderId;
                    }
                    else
                    {
                        StockProcessViewModel.StockHeaderId = 0;
                    }

                    StockProcessViewModel.StockHeaderId = temp.StockHeaderId;
                    StockProcessViewModel.DocHeaderId = temp.StockHeaderId;
                    StockProcessViewModel.DocLineId = s.StockLineId;
                    StockProcessViewModel.DocTypeId = temp.DocTypeId;
                    StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                    StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                    StockProcessViewModel.DocNo = temp.DocNo;
                    StockProcessViewModel.DivisionId = temp.DivisionId;
                    StockProcessViewModel.SiteId = temp.SiteId;
                    StockProcessViewModel.CurrencyId = null;
                    StockProcessViewModel.HeaderProcessId = null;
                    StockProcessViewModel.PersonId = temp.PersonId;
                    StockProcessViewModel.ProductId = s.ProductId;
                    StockProcessViewModel.HeaderFromGodownId = temp.FromGodownId;
                    StockProcessViewModel.HeaderGodownId = temp.GodownId;
                    StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                    StockProcessViewModel.ProcessId = s.FromProcessId;
                    StockProcessViewModel.LotNo = s.LotNo;
                    StockProcessViewModel.CostCenterId = temp.CostCenterId;


                    if (s.Qty > 0)
                    {
                        StockProcessViewModel.Qty_Rec = s.Qty;
                        StockProcessViewModel.Qty_Iss = 0;
                    }
                    else if (s.Qty < 0)
                    {
                        StockProcessViewModel.Qty_Rec = 0;
                        StockProcessViewModel.Qty_Iss = Math.Abs(s.Qty); 
                    }

                    StockProcessViewModel.Rate = s.Rate;
                    StockProcessViewModel.ExpiryDate = null;
                    StockProcessViewModel.Specification = s.Specification;
                    StockProcessViewModel.Dimension1Id = s.Dimension1Id;
                    StockProcessViewModel.Dimension2Id = s.Dimension2Id;
                    StockProcessViewModel.Dimension3Id = s.Dimension3Id;
                    StockProcessViewModel.Dimension4Id = s.Dimension4Id;
                    StockProcessViewModel.Remark = s.Remark;
                    StockProcessViewModel.Status = temp.Status;
                    StockProcessViewModel.CreatedBy = temp.CreatedBy;
                    StockProcessViewModel.CreatedDate = DateTime.Now;
                    StockProcessViewModel.ModifiedBy = temp.ModifiedBy;
                    StockProcessViewModel.ModifiedDate = DateTime.Now;

                    string StockProcessPostingError = "";
                    StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                    if (StockProcessPostingError != "")
                    {
                        ModelState.AddModelError("", StockProcessPostingError);
                        return PartialView("_Create", svm);
                    }

                    s.StockProcessId = StockProcessViewModel.StockProcessId;


                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    db.StockLine.Add(s);
                    //_StockLineService.Create(s);

                    //StockHeader header = new StockHeaderService(_unitOfWork).Find(s.StockHeaderId);
                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ObjectState = Model.ObjectState.Modified;
                        
                        db.StockHeader.Add(temp);
                        //new StockHeaderService(_unitOfWork).Update(temp);
                    }

                    try
                    {
                        JobConsumptionDocEvents.onLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, s.StockLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        EventException = true;
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
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(svm);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobConsumptionDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, s.StockLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.StockHeaderId,
                        DocLineId = s.StockLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));


                    return RedirectToAction("_Create", new { id = svm.StockHeaderId });

                }


                else
                {
                    StockLine templine = _StockLineService.Find(s.StockLineId);
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    StockLine ExRec = new StockLine();
                    ExRec = Mapper.Map<StockLine>(templine);

                    templine.Qty = s.Qty;

                    if (templine.StockProcessId != null)
                    {
                        StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                        //Posting in StockProcess
                        if (temp.StockHeaderId != null)
                        {
                            StockProcessViewModel.StockHeaderId = temp.StockHeaderId;
                        }
                        else
                        {
                            StockProcessViewModel.StockHeaderId = 0;
                        }

                        StockProcessViewModel.StockProcessId = templine.StockProcessId ?? 0;
                        StockProcessViewModel.DocHeaderId = templine.StockHeaderId;
                        StockProcessViewModel.DocLineId = templine.StockLineId;
                        StockProcessViewModel.DocTypeId = temp.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                        StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                        StockProcessViewModel.DocNo = temp.DocNo;
                        StockProcessViewModel.DivisionId = temp.DivisionId;
                        StockProcessViewModel.SiteId = temp.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                        StockProcessViewModel.PersonId = temp.PersonId;
                        StockProcessViewModel.ProductId = s.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = temp.GodownId;
                        StockProcessViewModel.GodownId = temp.GodownId;
                        StockProcessViewModel.ProcessId = temp.ProcessId;
                        StockProcessViewModel.LotNo = templine.LotNo;
                        StockProcessViewModel.CostCenterId = templine.CostCenterId;

                        if (s.Qty > 0)
                        {
                            StockProcessViewModel.Qty_Rec = s.Qty;
                            StockProcessViewModel.Qty_Iss = 0;
                        }
                        else if (s.Qty < 0)
                        {
                            StockProcessViewModel.Qty_Rec = 0;
                            StockProcessViewModel.Qty_Iss = Math.Abs(s.Qty); 
                        }

                        StockProcessViewModel.Rate = templine.Rate;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = templine.Specification;
                        StockProcessViewModel.Dimension1Id = templine.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = templine.Dimension2Id;
                        StockProcessViewModel.Dimension3Id = templine.Dimension3Id;
                        StockProcessViewModel.Dimension4Id = templine.Dimension4Id;
                        StockProcessViewModel.Remark = s.Remark;
                        StockProcessViewModel.Status = temp.Status;
                        StockProcessViewModel.CreatedBy = templine.CreatedBy;
                        StockProcessViewModel.CreatedDate = templine.CreatedDate;
                        StockProcessViewModel.ModifiedBy = User.Identity.Name;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            ModelState.AddModelError("", StockProcessPostingError);
                            return PartialView("_Create", svm);
                        }
                    }


                    templine.Remark = s.Remark;
               

                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.StockLine.Add(templine);
                    //_StockLineService.Update(templine);

                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;

                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;

                        temp.ObjectState = Model.ObjectState.Modified;
                        db.StockHeader.Add(temp);
                        //new StockHeaderService(_unitOfWork).Update(temp);
                    }


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = templine,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobConsumptionDocEvents.onLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, templine.StockLineId, EventModeConstants.Edit), ref db);
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
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(svm);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobConsumptionDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, templine.StockLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = templine.StockHeaderId,
                        DocLineId = templine.StockLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,

                    }));
                    //End of Saving the Activity Log

                    return Json(new { success = true });
                }

            }
            PrepareViewBag(svm);
            return PartialView("_Create", svm);
        }

        [HttpGet]
        public ActionResult _ModifyLine(int id)
        {
            return _Modify(id);
        }

        [HttpGet]
        public ActionResult _ModifyLineAfterSubmit(int id)
        {
            return _Modify(id);
        }

        private ActionResult _Modify(int id)
        {
            StockLineViewModel temp = _StockLineService.GetJobConsumptionLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXCL"] += ExceptionMsg;
            #endregion

            if ((TimePlanValidation || Continue))
                ViewBag.LineMode = "Edit";

            StockHeader H = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId);

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
            temp.GodownId = H.GodownId;
            PrepareViewBag(temp);
            return PartialView("_Create", temp);
        }

        public ActionResult _Detail(int id)
        {
            StockLineViewModel temp = _StockLineService.GetJobConsumptionLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            StockHeader H = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId);

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            temp.GodownId = H.GodownId;            
            PrepareViewBag(temp);
            return PartialView("_Create", temp);
        }

        [HttpGet]
        public ActionResult _DeleteLine(int id)
        {
            return _Delete(id);
        }
        [HttpGet]
        public ActionResult _DeleteLine_AfterSubmit(int id)
        {
            return _Delete(id);
        }

        private ActionResult _Delete(int id)
        {
            StockLineViewModel temp = _StockLineService.GetJobConsumptionLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXCL"] += ExceptionMsg;
            #endregion

            if ((TimePlanValidation || Continue))
                ViewBag.LineMode = "Delete";

            StockHeader H = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId);

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            temp.GodownId = H.GodownId;
            PrepareViewBag(temp);
            return PartialView("_Create", temp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(StockLineViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobConsumptionDocEvents.beforeLineDeleteEvent(this, new StockEventArgs(vm.StockHeaderId, vm.StockLineId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Validation failed before delete.";

            if (BeforeSave && !EventException)
            {


                int? StockProcessId = 0;
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                StockLine StockLine = (from p in db.StockLine
                                       where p.StockLineId == vm.StockLineId
                                       select p).FirstOrDefault();

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<StockLine>(StockLine),
                });


                StockProcessId = StockLine.StockProcessId;

                //_StockLineService.Delete(StockLine);
                StockLine.ObjectState = Model.ObjectState.Deleted;
                db.StockLine.Remove(StockLine);


                if (StockProcessId != null)
                {
                    new StockProcessService(_unitOfWork).DeleteStockProcessDB((int)StockProcessId, ref db, true);
                }


                StockHeader header = new StockHeaderService(_unitOfWork).Find(StockLine.StockHeaderId);
                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(header);
                    //new StockHeaderService(_unitOfWork).Update(header);
                }


                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    JobConsumptionDocEvents.onLineDeleteEvent(this, new StockEventArgs(StockLine.StockHeaderId, StockLine.StockLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }


                try
                {
                    if (EventException)
                        throw new Exception();

                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    ViewBag.LineMode = "Delete";
                    return View("_Create", vm);
                }

                try
                {
                    JobConsumptionDocEvents.afterLineDeleteEvent(this, new StockEventArgs(StockLine.StockHeaderId, StockLine.StockLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.StockHeaderId,
                    DocLineId = StockLine.StockLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));                

            }

            return Json(new { success = true });

        }

        public JsonResult GetProductDetailJson(int ProductId, int StockId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);

            return Json(new { ProductId = product.ProductId, StandardCost = product.StandardCost, UnitId = product.UnitId });
        }

        public JsonResult GetPendingProdOrders(int ProductId)
        {
            return Json(new ProdOrderHeaderService(_unitOfWork).GetPendingProdOrders(ProductId).ToList());
        }

        public JsonResult GetProdOrderDetail(int ProdOrderLineId)
        {
            return Json(new ProdOrderLineService(_unitOfWork).GetProdOrderDetailBalance(ProdOrderLineId));
        }

        public JsonResult GetProcessBalanceProducts(int? CostCenterId, int ProcessId, int HeaderId)
        {
            return Json(_StockLineService.GetProcessBalanceProducts(ProcessId, CostCenterId, HeaderId).ToList());
        }

        public JsonResult GetPendingJobConsumptionProducts(int? CostCenterId, int ProcessId, int JobWorkerId, string term)
        {
            //var t = _StockLineService.GetPendingProductsForJobConsumption(CostCenterId, ProcessId, JobWorkerId, term);
            //t = t.ToList();
            return Json(_StockLineService.GetPendingProdFromStocProcBal(CostCenterId, ProcessId, JobWorkerId, term).ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingJobConsumptionCostCenters(int StockHeaderId, int? CostCenterId, int ProcessId, int JobWorkerId, string term)
        {
            //var t = _StockLineService.GetPendingProductsForJobConsumption(CostCenterId, ProcessId, JobWorkerId, term);
            //t = t.ToList();
            return Json(_StockLineService.GetPendingCCFromStockProcBal(StockHeaderId,CostCenterId, ProcessId, JobWorkerId, term).ToList(), JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);

            TempData.Remove("CSEXC");

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
