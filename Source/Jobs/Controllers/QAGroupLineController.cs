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
using Jobs.Helpers;
using Model.ViewModel;
using System.Xml.Linq;
using DocumentEvents;
using CustomEventArgs;
using JobOrderDocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class QAGroupLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IQAGroupLineService _QAGroupLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public QAGroupLineController(IQAGroupLineService QAGroup, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _QAGroupLineService = QAGroup;
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
            var p = _QAGroupLineService.GetQAGroupLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult _Index(int id, int Status)
        {
            ViewBag.Status = Status;
            ViewBag.QAGroupId = id;
            var p = _QAGroupLineService.GetQAGroupLineListForIndex(id).ToList();
            return PartialView(p);
        }


    
        private void PrepareViewBag(QAGroupLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();

            if (vm != null)
            {
                QAGroupViewModel H = new QAGroupService(_unitOfWork).GetQAGroup(vm.QAGroupId);
                ViewBag.DocNo = H.DocTypeName + "-" + H.QaGroupName;
            }
        }

        [HttpGet]
        public ActionResult CreateLine(int id, bool? IsProdBased)
        {
            return _Create(id, null, IsProdBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, bool? IsProdBased)
        {
            return _Create(id, null, IsProdBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int id, bool? IsProdBased)
        {
            return _Create(id, null, IsProdBased);
        }

        public ActionResult _Create(int Id, DateTime? date, bool? IsProdBased) 
        {
            QAGroup H = new QAGroupService(_unitOfWork).Find(Id);
            QAGroupLineViewModel s = new QAGroupLineViewModel();
            s.IsActive = true;
            s.QAGroupId = H.QAGroupId;
            ViewBag.Status = H.Status;            
            PrepareViewBag(s);
            ViewBag.LineMode = "Create";         
            return PartialView("_Create", s);
           

        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(QAGroupLineViewModel svm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            bool BeforeSave = true;
            QAGroup temp = new QAGroupService(_unitOfWork).Find(svm.QAGroupId);
            int i = 0;
            if (temp.QAGroupId <= 0)
            {
                i = _QAGroupLineService.GetQAGroupLineListForIndex(svm.QAGroupId).Where(x => x.Name == svm.Name).ToList().Count();
            }
            else
            {
                i = _QAGroupLineService.GetQAGroupLineListForIndex(svm.QAGroupId).Where(x => x.Name == svm.Name && x.QAGroupLineId != svm.QAGroupLineId).ToList().Count();
            }
            #region BeforeSave
            //try
            //{

            //    if (svm.GatePassLineId <= 0)
            //        BeforeSave = JobOrderDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.GatePassHeaderId, EventModeConstants.Add), ref db);
            //    else
            //        BeforeSave = JobOrderDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.GatePassHeaderId, EventModeConstants.Edit), ref db);

            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXCL"] += message;
            //    EventException = true;
            //}

            //if (!BeforeSave)
            //    ModelState.AddModelError("", "Validation failed before save.");
            #endregion


            if (svm.Name=="" || svm.Name==null)
            {
                
                ModelState.AddModelError("Name", "The Name is required");
               
            }

            if (i !=0)
            {
                ModelState.AddModelError("Name", svm.Name + " already exist");
            }


            QAGroupLine s = Mapper.Map<QAGroupLineViewModel, QAGroupLine>(svm);

            ViewBag.Status = temp.Status;



            if (svm.QAGroupLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                if (svm.QAGroupLineId <= 0)
                {
                    //Posting in Stock                    
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;                  
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    db.QAGroupLine.Add(s);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;

                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.QAGroup.Add(temp);
                    //try
                    //{
                    //    JobOrderDocEvents.onLineSaveEvent(this, new JobEventArgs(s.GatePassHeaderId, s.GatePassLineId, EventModeConstants.Add), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXCL"] += message;
                    //    EventException = true;
                    //}
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


                 

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.QAGroupId,
                        DocLineId = s.QAGroupLineId,
                        ActivityType = (int)ActivityTypeContants.Added,                       
                        DocNo = temp.QaGroupName,
                        DocDate = temp.CreatedDate,
                        DocStatus=temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = svm.QAGroupId, IsProdBased =false });
                    //return RedirectToAction("_Create", new { id = svm.GatePassHeaderId, IsProdBased = (s.ProdOrderLineId == null ? false : true) });

                }
                else
                {
                    QAGroupLine templine = (from p in db.QAGroupLine
                                             where p.QAGroupLineId == s.QAGroupLineId
                                             select p).FirstOrDefault();

                    QAGroupLine ExTempLine = new QAGroupLine();
                    ExTempLine = Mapper.Map<QAGroupLine>(templine);

                    templine.QAGroupId = s.QAGroupId;
                    /*templine.Name = s.Name;
                    templine.IsMandatory = s.IsMandatory;
                    templine.DataType = s.DataType;
                    templine.ListItem = s.ListItem;
                    templine.DefaultValue = s.DefaultValue;
                    templine.IsActive = s.IsActive;*/                 
                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.QAGroupLine.Add(templine);
                    int Status = 0;
                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        Status = temp.Status;
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                    }


                    temp.ObjectState = Model.ObjectState.Modified;
                    db.QAGroup.Add(temp);

             


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExTempLine,
                        Obj = templine
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
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(svm);                       
                            return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobOrderDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.QAGroupId, templine.QAGroupLineId, EventModeConstants.Edit), ref db);
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
                        DocId = templine.QAGroupId,
                        DocLineId = templine.QAGroupLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,                       
                        DocNo = temp.QaGroupName,
                        xEModifications = Modifications,
                        DocDate = temp.CreatedDate,
                        DocStatus=temp.Status,
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

        [HttpGet]
        public ActionResult _ModifyLineAfterApprove(int id)
        {
            return _Modify(id);
        }

        private ActionResult _Modify(int id)
        {
            QAGroupLineViewModel temp = _QAGroupLineService.GetQAGroupLine(id);

            QAGroup H = new QAGroupService(_unitOfWork).Find(temp.QAGroupId);

           
           
            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(temp);

    

            if ((TimePlanValidation || Continue))
                ViewBag.LineMode = "Edit";

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
            QAGroupLineViewModel temp = _QAGroupLineService.GetQAGroupLine(id);

            QAGroup H = new QAGroupService(_unitOfWork).Find(temp.QAGroupId);

            //Getting Settings
           
            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(temp);
            //ViewBag.LineMode = "Delete";

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason =null}, User.Identity.Name, out ExceptionMsg, out Continue);

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

         

          return PartialView("_Create", temp);
           
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(QAGroupLineViewModel vm)
        {
            bool BeforeSave = true;
            //try
            //{
            //    BeforeSave = JobOrderDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.QAGroupId, vm.QAGroupLineId), ref db);
            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXC"] += message;
            //    EventException = true;
            //}

            if (!BeforeSave)
                TempData["CSEXC"] += "Validation failed before delete.";

            if (BeforeSave && !EventException)
            {

                int? StockId = 0;
                int? StockProcessId = 0;
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                QAGroupLine QAGroupLine = (from p in db.QAGroupLine
                                           where p.QAGroupLineId == vm.QAGroupLineId
                                             select p).FirstOrDefault();
                QAGroup header = (from p in db.QAGroup
                                  where p.QAGroupId == QAGroupLine.QAGroupId
                                         select p).FirstOrDefault();
               
                LogList.Add(new LogTypeViewModel
                {
                    Obj = Mapper.Map<QAGroupLine>(QAGroupLine),
                });

                //_JobOrderLineService.Delete(JobOrderLine);
                QAGroupLine.ObjectState = Model.ObjectState.Deleted;
                db.QAGroupLine.Remove(QAGroupLine);




                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    db.QAGroup.Add(header);
                }

           

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

            

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
                    PrepareViewBag(vm);
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);

                }

                try
                {
                    JobOrderDocEvents.afterLineDeleteEvent(this, new JobEventArgs(QAGroupLine.QAGroupId, QAGroupLine.QAGroupLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                //Saving the Activity Log

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.QAGroupId,
                    DocLineId = QAGroupLine.QAGroupLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,                  
                    DocNo = header.QaGroupName,
                    xEModifications = Modifications,
                    DocDate = header.CreatedDate,
                    DocStatus=header.Status,
                }));
            }

            return Json(new { success = true });

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
