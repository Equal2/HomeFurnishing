using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Jobs.Helpers;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using System.Data.Entity;
using DocumentEvents;
using CustomEventArgs;
using JobReceiveQADocumentEvents;
using Reports.Controllers;

namespace Jobs.Areas.Rug.Controllers
{

    [Authorize]
    public class JobReceiveQAPenaltyController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;

        IJobReceiveQAPenaltyService _JobReceiveQAPenaltyService;
        IStockService _StockService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobReceiveQAPenaltyController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _StockService = new StockService(unitOfWork);
            _JobReceiveQAPenaltyService = new JobReceiveQAPenaltyService(db, unitOfWork);
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }



        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _JobReceiveQAPenaltyService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }




        private void PrepareViewBag()
        {
        }



        public ActionResult _Create(int Id) //Id ==> JobReceiveQALineId
        {
            JobReceiveQALine L = new JobReceiveQALineService(db, _unitOfWork).Find(Id);
            JobReceiveQAHeader H = new JobReceiveQAHeaderService(db).Find(L.JobReceiveQAHeaderId);
            DocumentType D = new DocumentTypeService(_unitOfWork).Find(H.DocTypeId);
            JobReceiveQAPenaltyViewModel s = new JobReceiveQAPenaltyViewModel();

            s.DocTypeId = H.DocTypeId;
            s.JobReceiveQALineId = Id;
            //Getting Settings
            PrepareViewBag();
            if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            {
                ViewBag.CSEXCL = TempData["CSEXCL"];
                TempData["CSEXCL"] = null;
            }
            ViewBag.LineMode = "Create";
            //ViewBag.DocNo = D.DocumentTypeName + "-" + H.DocNo;
            ViewBag.DocNo = D.DocumentTypeName + "-" + H.DocNo + " ( Deal Qty : " + L.DealQty.ToString() +" )";
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobReceiveQAPenaltyViewModel svm)
        {
            JobReceiveQALine L = new JobReceiveQALineService(db, _unitOfWork).Find(svm.JobReceiveQALineId);
            JobReceiveQAPenalty s = Mapper.Map<JobReceiveQAPenaltyViewModel, JobReceiveQAPenalty>(svm);
            JobReceiveQAHeader temp = new JobReceiveQAHeaderService(db).Find(L.JobReceiveQAHeaderId);

            #region BeforeSave
            bool BeforeSave = true;
            try
            {

                if (svm.JobReceiveQAPenaltyId <= 0)
                    BeforeSave = JobReceiveQADocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobReceiveQAPenaltyId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobReceiveQADocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobReceiveQAPenaltyId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");
            #endregion


            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            if (svm.JobReceiveQAPenaltyId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }



            //if (svm.JobReceiveLineId <= 0)
            //    ModelState.AddModelError("JobReceiveLineId", "The JobReceiveLine field is required");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (svm.JobReceiveQAPenaltyId <= 0)
                {
                    _JobReceiveQAPenaltyService.Create(s, User.Identity.Name);



                    try
                    {
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag();
                        return PartialView("_Create", svm);
                    }

                    

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobReceiveQAHeaderId,
                        DocLineId = s.JobReceiveQAPenaltyId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = svm.JobReceiveQALineId});
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    StringBuilder logstring = new StringBuilder();
                    JobReceiveQAPenalty temp1 = _JobReceiveQAPenaltyService.Find(svm.JobReceiveQAPenaltyId);


                    JobReceiveQAPenalty ExRec = new JobReceiveQAPenalty();
                    ExRec = Mapper.Map<JobReceiveQAPenalty>(temp1);

                    temp1.ReasonId = svm.ReasonId;
                    temp1.Amount = svm.Amount;
                    temp1.Remark = svm.Remark;

                    _JobReceiveQAPenaltyService.Update(temp1, User.Identity.Name);


                    //List<JobReceiveQAPenalty> PenaltyLines = (from Pl in db.JobReceiveQAPenalty where Pl.JobReceiveQALineId == L.JobReceiveQALineId select Pl).ToList();

                    

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
                    });

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                    }

                    List<JobReceiveQAPenalty> PenaltyLinesList = (from Pl in db.JobReceiveQAPenalty where Pl.JobReceiveQALineId == L.JobReceiveQALineId && Pl.JobReceiveQAPenaltyId != svm.JobReceiveQAPenaltyId select Pl).ToList();
                    Decimal TotalPenalty = 0;
                    if (PenaltyLinesList.Count() != 0)
                    {
                        TotalPenalty = PenaltyLinesList.Sum(i => i.Amount) + svm.Amount;
                    }
                    else
                    {
                        TotalPenalty = svm.Amount;
                    }

                    L.PenaltyAmt = TotalPenalty;
                    new JobReceiveQALineService(db, _unitOfWork).Update(L, User.Identity.Name);

                    new JobReceiveQAHeaderService(db).Update(temp, User.Identity.Name);

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobReceiveQADocEvents.onLineSaveEvent(this, new JobEventArgs(temp.JobReceiveQAHeaderId, temp1.JobReceiveQAPenaltyId, EventModeConstants.Edit), ref db);
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
                        PrepareViewBag();
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobReceiveQADocEvents.afterLineSaveEvent(this, new JobEventArgs(temp.JobReceiveQAHeaderId, temp1.JobReceiveQAPenaltyId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobReceiveQAHeaderId,
                        DocLineId = temp1.JobReceiveQAPenaltyId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));


                    return Json(new { success = true });

                }
            }
            PrepareViewBag();
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

        [HttpGet]
        public ActionResult _ModifyLineAfterApprove(int id)
        {
            return _Modify(id);
        }



        [HttpGet]
        private ActionResult _Modify(int id)
        {
            JobReceiveQAPenaltyViewModel temp = _JobReceiveQAPenaltyService.GetJobReceiveQAPenaltyForEdit(id);
            JobReceiveQALine L = new JobReceiveQALineService(db, _unitOfWork).Find(temp.JobReceiveQALineId);
            JobReceiveQAHeader Header = new JobReceiveQAHeaderService(db).Find(L.JobReceiveQAHeaderId);
            DocumentType D = new DocumentTypeService(_unitOfWork).Find(Header.DocTypeId);

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


            PrepareViewBag();
            //ViewBag.DocNo = D.DocumentTypeName + "-" + Header.DocNo;
            ViewBag.DocNo = D.DocumentTypeName + "-" + Header.DocNo + " ( Deal Qty : " + L.DealQty.ToString() + " )";
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

        [HttpGet]
        public ActionResult _DeleteLine_AfterApprove(int id)
        {
            return _Delete(id);
        }

        private ActionResult _Delete(int id)
        {
            JobReceiveQAPenaltyViewModel temp = _JobReceiveQAPenaltyService.GetJobReceiveQAPenaltyForEdit(id);

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

            
            PrepareViewBag();

            return PartialView("_Create", temp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobReceiveQAPenaltyViewModel vm)
        {
            JobReceiveQALine L = new JobReceiveQALineService(db, _unitOfWork).Find(vm.JobReceiveQALineId);

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReceiveQADocEvents.beforeLineDeleteEvent(this, new JobEventArgs(L.JobReceiveQAHeaderId, vm.JobReceiveQAPenaltyId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Validation failed before delete.";
            #endregion

            if (BeforeSave && !EventException)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                JobReceiveQAPenalty JobReceiveQAPenalty = (from p in db.JobReceiveQAPenalty
                                                     where p.JobReceiveQAPenaltyId == vm.JobReceiveQAPenaltyId
                                                     select p).FirstOrDefault();

                JobReceiveQAHeader header = new JobReceiveQAHeaderService(db).Find(L.JobReceiveQAHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobReceiveQAPenalty>(JobReceiveQAPenalty),
                });


                _JobReceiveQAPenaltyService.Delete(JobReceiveQAPenalty);



                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;

                    new JobReceiveQAHeaderService(db).Update(header, User.Identity.Name);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    JobReceiveQADocEvents.onLineDeleteEvent(this, new JobEventArgs(L.JobReceiveQAHeaderId, JobReceiveQAPenalty.JobReceiveQAPenaltyId), ref db);
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
                    PrepareViewBag();
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);
                }

                try
                {
                    JobReceiveQADocEvents.afterLineDeleteEvent(this, new JobEventArgs(L.JobReceiveQAHeaderId, JobReceiveQAPenalty.JobReceiveQAPenaltyId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.JobReceiveQAHeaderId,
                    DocLineId = JobReceiveQAPenalty.JobReceiveQAPenaltyId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));

            }

            return Json(new { success = true });
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
