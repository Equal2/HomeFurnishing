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
using RateConversionDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class RateConversionLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;
        IStockLineService _StockLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public RateConversionLineController(IStockLineService Stock, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
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

            StockHeaderSettings Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            vm.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(Settings);

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
            List<StockLineViewModel> temp = _StockLineService.GetJobConsumptionForFilters(vm).ToList();
            StockMasterDetailModel svm = new StockMasterDetailModel();
            svm.StockLineViewModel = temp;
            StockHeader Header = new StockHeaderService(_unitOfWork).Find(vm.StockHeaderId);
            StockHeaderSettings settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(StockMasterDetailModel vm)
        {
            int Cnt = 0;
            StockHeader Header = new StockHeaderService(_unitOfWork).Find(vm.StockLineViewModel.FirstOrDefault().StockHeaderId);

            StockHeaderSettings Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            bool BeforeSave = true;
            try
            {
                BeforeSave = RateConversionDocEvents.beforeLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                var CostCenterRecords = (from p in vm.StockLineViewModel
                                         where p.CostCenterId != null
                                         group p by p.CostCenterId into g
                                         select g).ToList();

                var CostCenterIds = CostCenterRecords.Select(m => m.Key).ToArray();

                var DBCostCenterStatus = (from p in db.CostCenterStatus
                                          where CostCenterIds.Contains(p.CostCenterId)
                                          select p).ToArray();


                foreach (var item in vm.StockLineViewModel)
                {
                    if (item.Qty != 0 && item.Rate > 0 && (Settings.isMandatoryLineCostCenter == true ? item.CostCenterId.HasValue : 1 == 1))
                    {
                        StockLine line = new StockLine();

                        StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                        if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                        {
                            StockProcessViewModel.StockHeaderId = (int)Header.StockHeaderId;
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
                        StockProcessViewModel.DocHeaderId = Header.StockHeaderId;
                        StockProcessViewModel.DocLineId = line.StockLineId;
                        StockProcessViewModel.DocTypeId = Header.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                        StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                        StockProcessViewModel.DocNo = Header.DocNo;
                        StockProcessViewModel.DivisionId = Header.DivisionId;
                        StockProcessViewModel.SiteId = Header.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.PersonId = Header.PersonId;
                        StockProcessViewModel.ProductId = item.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = Header.GodownId;
                        StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                        StockProcessViewModel.GodownId = Header.GodownId;
                        StockProcessViewModel.Remark = Header.Remark;
                        StockProcessViewModel.Status = Header.Status;
                        StockProcessViewModel.ProcessId = Header.ProcessId;
                        StockProcessViewModel.LotNo = null;
                        StockProcessViewModel.CostCenterId = (item.CostCenterId == null ? Header.CostCenterId : item.CostCenterId);


                        if (item.Qty < 0)
                        {
                            StockProcessViewModel.Qty_Rec = Math.Abs(item.Qty);
                            StockProcessViewModel.Qty_Iss = 0;
                        }
                        else if (item.Qty > 0)
                        {
                            StockProcessViewModel.Qty_Iss = Math.Abs (item.Qty);
                            StockProcessViewModel.Qty_Rec = 0;
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






                        line.StockHeaderId = item.StockHeaderId;
                        line.Qty = item.Qty;
                        line.ProductId = item.ProductId;
                        line.LotNo = item.LotNo;
                        line.Rate = (decimal)item.Rate;
                        line.Amount = Math.Round((decimal)item.Rate * item.Qty, Settings.LineRoundOff??0);
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.Dimension3Id = item.Dimension3Id;
                        line.Dimension4Id = item.Dimension4Id;
                        line.DocNature = StockNatureConstants.Receive;
                        line.CostCenterId = item.CostCenterId;
                        line.FromProcessId = item.FromProcessId;
                        line.Specification = item.Specification;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;

                        line.ObjectState = Model.ObjectState.Added;
                        //_StockLineService.Create(line);
                        db.StockLine.Add(line);

                        Cnt = Cnt + 1;


                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;

                    Header.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(Header);
                }

                //ForUpdating CostCenterStatus Values//

                foreach (var item in DBCostCenterStatus)
                {
                    var CostCenterAmounts = db.StockLine.Local.Where(m => m.CostCenterId == item.CostCenterId).ToList();

                   

                    if (CostCenterAmounts != null)
                    {
                        if (CostCenterAmounts.Sum(m => m.Amount) > 0)
                            item.AmountDr = (item.AmountDr ?? 0) + CostCenterAmounts.Sum(m => m.Amount);
                        else if (CostCenterAmounts.Sum(m => m.Amount) < 0)
                            item.AmountDr = (item.AmountDr ?? 0) + CostCenterAmounts.Sum(m => m.Amount);

                        item.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatus.Add(item);

                    }

                }


                try
                {
                    RateConversionDocEvents.onLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
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
                    RateConversionDocEvents.afterLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.StockHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
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
        public ActionResult CreateLine(int id)
        {
            return _Create(id);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, bool? IsProdBased)
        {
            return _Create(id);
        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            StockHeader H = new StockHeaderService(_unitOfWork).Find(Id);
            StockLineViewModel s = new StockLineViewModel();

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            s.StockHeaderId = H.StockHeaderId;
            s.GodownId = H.GodownId;
            ViewBag.Status = H.Status;
            PrepareViewBag(s);
            if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            {
                ViewBag.CSEXCL = TempData["CSEXCL"];
                TempData["CSEXCL"] = null;
            }

            ViewBag.LineMode = "Create";

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
                if (svm.Rate <= 0)
                {
                    ModelState.AddModelError("Rate", "The Rate field is required");
                }
                if (svm.StockHeaderSettings.isMandatoryLineCostCenter == true && !(svm.CostCenterId.HasValue))
                {
                    ModelState.AddModelError("CostCenterId", "The CostCenter field is required");
                }
            }

            bool BeforeSave = true;
            try
            {

                if (svm.StockLineId <= 0)
                    BeforeSave = RateConversionDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = RateConversionDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Edit), ref db);

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


                    if (s.Qty < 0)
                    {
                        StockProcessViewModel.Qty_Rec = Math.Abs(s.Qty);
                        StockProcessViewModel.Qty_Iss = 0;
                    }
                    else if (s.Qty > 0)
                    {
                        StockProcessViewModel.Qty_Iss = Math.Abs(s.Qty);
                        StockProcessViewModel.Qty_Rec = 0;
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
                    s.DocNature = StockNatureConstants.Receive;
                    s.ObjectState = Model.ObjectState.Added;
                    //_StockLineService.Create(s);
                    db.StockLine.Add(s);

                    //StockHeader header = new StockHeaderService(_unitOfWork).Find(s.StockHeaderId);
                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
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
                        RateConversionDocEvents.onLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, s.StockLineId, EventModeConstants.Add), ref db);
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
                        RateConversionDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, s.StockLineId, EventModeConstants.Add), ref db);
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

                    if (templine.StockProcessId != null)
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

                        if (s.Qty < 0)
                        {
                            StockProcessViewModel.Qty_Rec = Math.Abs(s.Qty);
                            StockProcessViewModel.Qty_Iss = 0;
                        }
                        else if (s.Qty > 0)
                        {
                            StockProcessViewModel.Qty_Iss = Math.Abs(s.Qty);
                            StockProcessViewModel.Qty_Rec = 0;
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



                    templine.Qty = s.Qty;
                    templine.Remark = s.Remark;
                    templine.Amount = s.Qty * s.Rate;
                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    //_StockLineService.Update(templine);
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.StockLine.Add(templine);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;

                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;

                        temp.ObjectState = Model.ObjectState.Modified;
                        db.StockHeader.Add(temp);
                        //new StockHeaderService(_unitOfWork).Update(temp);
                    }

                    if (templine.CostCenterId.HasValue && templine.CostCenterId > 0)
                    {
                        var CostCenterStatus = db.CostCenterStatus.Find(templine.CostCenterId);

                        if (CostCenterStatus != null)
                        {
                            //For Reducing Old Amount
                            if (ExRec.Amount > 0)
                            {
                                CostCenterStatus.AmountDr = CostCenterStatus.AmountDr - ExRec.Amount;
                            }
                            else
                            {
                                CostCenterStatus.AmountCr = CostCenterStatus.AmountCr - ExRec.Amount;
                            }
                            //For Adding New Amount
                            if (templine.Amount > 0)
                            {
                                CostCenterStatus.AmountDr = CostCenterStatus.AmountDr + templine.Amount;
                            }
                            else
                            {
                                CostCenterStatus.AmountCr = CostCenterStatus.AmountCr + templine.Amount;
                            }
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatus.Add(CostCenterStatus);
                        }

                    }

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = templine,
                    });


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        RateConversionDocEvents.onLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, templine.StockLineId, EventModeConstants.Edit), ref db);
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
                        RateConversionDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, templine.StockLineId, EventModeConstants.Edit), ref db);
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
            StockLineViewModel temp = _StockLineService.GetRateConversionLine(id);

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
            StockLineViewModel temp = _StockLineService.GetRateConversionLine(id);

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
                BeforeSave = RateConversionDocEvents.beforeLineDeleteEvent(this, new StockEventArgs(vm.StockHeaderId, vm.StockLineId), ref db);
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
                int? CostCenterId = null;
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                StockLine StockLine = (from p in db.StockLine
                                       where p.StockLineId == vm.StockLineId
                                       select p).FirstOrDefault();

                CostCenterId = StockLine.CostCenterId;

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

                
                if(CostCenterId.HasValue && CostCenterId.Value > 0)
                {
                    var CostCenterStatus = db.CostCenterStatus.Find(CostCenterId);
                    if(CostCenterStatus!=null && StockLine.Amount > 0)
                    {
                        if(StockLine.Amount>0)
                        {
                            CostCenterStatus.AmountDr = CostCenterStatus.AmountDr - StockLine.Amount;
                        }
                        else
                        {
                            CostCenterStatus.AmountCr = CostCenterStatus.AmountCr - StockLine.Amount;
                        }
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatus.Add(CostCenterStatus);
                    }
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    RateConversionDocEvents.onLineDeleteEvent(this, new StockEventArgs(StockLine.StockHeaderId, StockLine.StockLineId), ref db);
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
                        throw new Exception();
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);
                }

                try
                {
                    RateConversionDocEvents.afterLineDeleteEvent(this, new StockEventArgs(StockLine.StockHeaderId, StockLine.StockLineId), ref db);
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
        public JsonResult GetPendingRateConversionProducts(int? CostCenterId, int ProcessId, int JobWorkerId, string term)
        {
            return Json(_StockLineService.GetPendingProdFromStocProcBal(CostCenterId, ProcessId, JobWorkerId, term).ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingRateConversionCostCenters(int StockHeaderId, int? CostCenterId, int ProcessId, int JobWorkerId, string term)
        {
            return Json(_StockLineService.GetPendingCCFromStockProcBal(StockHeaderId, CostCenterId, ProcessId, JobWorkerId, term).ToList(), JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);

            TempData["CSEXC"] = null;

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
