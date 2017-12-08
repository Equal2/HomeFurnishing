using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Presentation;
using Model.ViewModel;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{

    [Authorize]
    public class ProcessSequenceLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProcessSequenceLineService _ProcessSequenceLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ProcessSequenceLineController(IProcessSequenceLineService ProcessSequence, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProcessSequenceLineService = ProcessSequence;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _ProcessSequenceLineService.GetProcessSequenceLineListForProductCollectionIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        private void PrepareViewBag(ProcessSequenceLineViewModel s)
        {
            if (s == null)
            {
                ViewBag.ProcessId = new SelectList(new ProcessService(_unitOfWork).GetProcessList(), "ProcessId", "ProcessName");
            }
            else
            {
                ViewBag.ProcessId = new SelectList(new ProcessService(_unitOfWork).GetProcessList(), "ProcessId", "ProcessName", s.ProcessId);
            }
        }

        public ActionResult _CreateForProductCollection(int Id) //Id ==>ProductCalculationId
        {
            ProcessSequenceHeader H = new ProcessSequenceHeaderService(_unitOfWork).GetProcessSequenceHeaderForProductCollection(Id);
            ProcessSequenceLineViewModel s = new ProcessSequenceLineViewModel();
            s.RefDocId = Id;
            s.RefDocTypeId = H.ReferenceDocTypeId;
            s.ProcessSequenceHeaderId = H.ProcessSequenceHeaderId;
            PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ProcessSequenceLineViewModel svm)
        {
            ProcessSequenceLine s = Mapper.Map<ProcessSequenceLineViewModel, ProcessSequenceLine>(svm);
            ProcessSequenceHeader temp = (from p in db.ProcessSequenceHeader
                                          where p.ProcessSequenceHeaderId == svm.ProcessSequenceHeaderId
                                          select p).FirstOrDefault();

            ProductCollection PColl = new ProductCollectionService(_unitOfWork).Find(svm.RefDocId.Value);


            if (ModelState.IsValid)
            {
                if (svm.ProcessSequenceLineId <= 0)
                {

                    if (temp == null)
                    {
                        int RefDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ProductCollection).DocumentTypeId;

                        ProcessSequenceHeader Header = new ProcessSequenceHeader();
                        Header.ProcessSequenceHeaderName = PColl.ProductCollectionName + "-" + new DivisionService(_unitOfWork).Find((int)HttpContext.Session["DivisionId"]).DivisionName;
                        Header.ReferenceDocTypeId = RefDocTypeId;
                        Header.ReferenceDocId = PColl.ProductCollectionId;
                        Header.CreatedBy = User.Identity.Name;
                        Header.CreatedDate = DateTime.Now;
                        Header.ModifiedBy = User.Identity.Name;
                        Header.ModifiedDate = DateTime.Now;
                        Header.ObjectState = Model.ObjectState.Added;

                        new ProcessSequenceHeaderService(_unitOfWork).Create(Header);

                        s.ProcessSequenceHeaderId = Header.ProcessSequenceHeaderId;
                    }
                    else
                    {
                        s.ProcessSequenceHeaderId = temp.ProcessSequenceHeaderId;
                    }

                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    _ProcessSequenceLineService.Create(s);

                    if (temp != null && temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        new ProcessSequenceHeaderService(_unitOfWork).Update(temp);
                    }

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProcessSequence).DocumentTypeId,
                        DocId = s.ProcessSequenceHeaderId,
                        DocLineId = s.ProcessSequenceLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("_CreateForProductCollection", new { id = svm.RefDocId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProcessSequenceHeader header = new ProcessSequenceHeaderService(_unitOfWork).Find(svm.ProcessSequenceHeaderId);
                    int status = header.Status;
                    ProcessSequenceLine temp1 = _ProcessSequenceLineService.Find(svm.ProcessSequenceLineId);

                    ProcessSequenceLine ExRec = Mapper.Map<ProcessSequenceLine>(temp1);

                    temp1.ProcessId = svm.ProcessId;
                    temp1.Sequence = svm.Sequence;
                    temp1.ProductRateGroupId = svm.ProductRateGroupId;
                    temp1.Days = svm.Days;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    _ProcessSequenceLineService.Update(temp1);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
                    });
                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProcessSequence).DocumentTypeId,
                        DocId = temp.ProcessSequenceHeaderId,
                        DocLineId = temp1.ProcessSequenceLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return Json(new { success = true });

                }
            }
            PrepareViewBag(svm);
            return PartialView("_Create", svm);
        }

        [HttpGet]
        public ActionResult _Edit(int id)
        {
            ProcessSequenceLine temp = _ProcessSequenceLineService.GetProcessSequenceLine(id);

            ProcessSequenceHeader H = new ProcessSequenceHeaderService(_unitOfWork).GetProcessSequenceHeader(temp.ProcessSequenceHeaderId);
            ViewBag.DocNo = H.ProcessSequenceHeaderName;
            ProcessSequenceLineViewModel s = Mapper.Map<ProcessSequenceLine, ProcessSequenceLineViewModel>(temp);

            s.RefDocId = H.ReferenceDocId;
            s.RefDocTypeId = H.ReferenceDocTypeId;

            PrepareViewBag(s);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", s);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProcessSequenceLineViewModel ProcessSequenceLine = _ProcessSequenceLineService.GetProcessSequenceLineModel(id);
            if (ProcessSequenceLine == null)
            {
                return HttpNotFound();
            }
            return View(ProcessSequenceLine);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProcessSequenceLine ProcessSequenceLine = _ProcessSequenceLineService.GetProcessSequenceLine(id);
            _ProcessSequenceLineService.Delete(id);
            ProcessSequenceHeader header = new ProcessSequenceHeaderService(_unitOfWork).Find(ProcessSequenceLine.ProcessSequenceHeaderId);
            if (header.Status != (int)StatusConstants.Drafted)
            {
                header.Status = (int)StatusConstants.Modified;
                new ProcessSequenceHeaderService(_unitOfWork).Update(header);
            }


            _unitOfWork.Save();

            return RedirectToAction("Index", new { Id = ProcessSequenceLine.ProcessSequenceHeaderId }).Success("Data deleted successfully");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ProcessSequenceLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            ProcessSequenceLine ProcessSequenceLine = _ProcessSequenceLineService.GetProcessSequenceLine(vm.ProcessSequenceLineId);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = vm,
            });


            _ProcessSequenceLineService.Delete(vm.ProcessSequenceLineId);
            ProcessSequenceHeader header = new ProcessSequenceHeaderService(_unitOfWork).Find(ProcessSequenceLine.ProcessSequenceHeaderId);
            if (header.Status != (int)StatusConstants.Drafted)
            {
                header.Status = (int)StatusConstants.Modified;
                new ProcessSequenceHeaderService(_unitOfWork).Update(header);
            }

            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("EditSize", vm);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProcessSequence).DocumentTypeId,
                DocId = vm.ProcessSequenceHeaderId,
                DocLineId = vm.ProcessSequenceLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                xEModifications = Modifications,
            }));

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
