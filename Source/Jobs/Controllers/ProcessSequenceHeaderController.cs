using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using AutoMapper;
using Presentation;
using Model.ViewModel;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProcessSequenceHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext context = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProcessSequenceHeaderService _ProcessSequenceHeaderService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ProcessSequenceHeaderController(IProcessSequenceHeaderService PurchaseOrderHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProcessSequenceHeaderService = PurchaseOrderHeaderService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /ProcessSequenceHeader/

        public ActionResult Index()
        {
            IQueryable<ProcessSequenceHeaderIndexViewModel> p = _ProcessSequenceHeaderService.GetProcessSequenceHeaderList();
            return View(p);
        }

        [HttpGet]
        public ActionResult NextPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _ProcessSequenceHeaderService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _ProcessSequenceHeaderService.PrevId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult Print()
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
        public ActionResult Report()
        {

            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProcessSequence);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        [HttpGet]
        public ActionResult Remove()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        private void PrepareViewBag(ProcessSequenceHeaderIndexViewModel s)
        {

        }

        // GET: /ProcessSequenceHeader/Create

        public ActionResult Create()
        {
            ProcessSequenceHeaderIndexViewModel p = new ProcessSequenceHeaderIndexViewModel();

            return View("Create", p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HeaderPost(ProcessSequenceHeaderIndexViewModel svm)
        {

            if (ModelState.IsValid)
            {

                if (svm.ProcessSequenceHeaderId == 0)
                {
                    ProcessSequenceHeader s = Mapper.Map<ProcessSequenceHeaderIndexViewModel, ProcessSequenceHeader>(svm);
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    _ProcessSequenceHeaderService.Create(s);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProcessSequence).DocumentTypeId,
                        DocId = s.ProcessSequenceHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("Edit", new { id = s.ProcessSequenceHeaderId }).Success("Data saved Successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProcessSequenceHeader s = Mapper.Map<ProcessSequenceHeaderIndexViewModel, ProcessSequenceHeader>(svm);
                    ProcessSequenceHeader temp = _ProcessSequenceHeaderService.Find(s.ProcessSequenceHeaderId);

                    ProcessSequenceHeader ExRec = Mapper.Map<ProcessSequenceHeader>(temp);

                    temp.ProcessSequenceHeaderName = s.ProcessSequenceHeaderName;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    _ProcessSequenceHeaderService.Update(temp);

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
                        ModelState.AddModelError("", message);
                        return View("Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProcessSequence).DocumentTypeId,
                        DocId = temp.ProcessSequenceHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index").Success("Data saved successfully");
                }

            }
            PrepareViewBag(svm);
            return View("Create", svm);
        }


        // GET: /ProcessSequenceHeader/Edit/5
        public ActionResult Edit(int id, string PrevAction, string PrevController)
        {
            if (PrevAction != null)
            {
                ViewBag.Redirect = PrevAction;
            }

            ProcessSequenceHeader s = _ProcessSequenceHeaderService.GetProcessSequenceHeader(id);
            ProcessSequenceHeaderIndexViewModel svm = Mapper.Map<ProcessSequenceHeader, ProcessSequenceHeaderIndexViewModel>(s);
            PrepareViewBag(svm);
            if (s == null)
            {
                return HttpNotFound();
            }

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocId = s.ProcessSequenceHeaderId,
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProcessSequence).DocumentTypeId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.ProcessSequenceHeaderName.Length >20 ?s.ProcessSequenceHeaderName.Substring(0,20) : s.ProcessSequenceHeaderName,
                    DocStatus = s.Status,
                }));

            return View("Create", svm);
        }

        [Authorize]
        public ActionResult Detail(int id, string returl, string transactionType)
        {
            ProcessSequenceHeaderIndexViewModel h = (ProcessSequenceHeaderIndexViewModel)_ProcessSequenceHeaderService.GetProcessSequenceHeaderVM(id);
            ProcessSequenceHeaderIndexViewModelForEdit Header = Mapper.Map<ProcessSequenceHeaderIndexViewModel, ProcessSequenceHeaderIndexViewModelForEdit>(h);

            List<ProcessSequenceLineIndexViewModel> l = new ProcessSequenceLineService(_unitOfWork).GetProcessSequenceLineList(id).ToList();

            ProcessSequenceMasterDetailModel vm = new ProcessSequenceMasterDetailModel();
            vm.ProcessSequenceHeaderIndexViewModelForEdit = Header;
            vm.ProcessSequenceLineIndexViewModel = l;


            ViewBag.transactionType = transactionType;
            vm.ProcessSequenceHeaderId = vm.ProcessSequenceHeaderIndexViewModelForEdit.ProcessSequenceHeaderId;

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocId = h.ProcessSequenceHeaderId,
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProcessSequence).DocumentTypeId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = h.ProcessSequenceHeaderName.Substring(0,20),
                }));

            return View(vm);
        }


        // GET: /PurchaseOrderHeader/Delete/5

        public ActionResult Delete(int id, string PrevAction, string PrevController)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProcessSequenceHeaderIndexViewModel ProcessSequenceHeader = _ProcessSequenceHeaderService.GetProcessSequenceHeaderVM(id);
            if (ProcessSequenceHeader == null)
            {
                return HttpNotFound();
            }
            ReasonViewModel rvm = new ReasonViewModel()
            {
                id = id,
            };
            return PartialView("_Reason", rvm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {
                //string temp = (Request["Redirect"].ToString());
                //first find the Purchase Order Object based on the ID. (sience this object need to marked to be deleted IE. ObjectState.Deleted)
                var ProcessSequenceHeader = _ProcessSequenceHeaderService.GetProcessSequenceHeader(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ProcessSequenceHeader,
                });

                //Then find all the Purchase Order Header Line associated with the above ProductType.
                var ProcessSequenceLine = new ProcessSequenceLineService(_unitOfWork).GetProcessSequenceLineList(vm.id);

                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in ProcessSequenceLine)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });
                    new ProcessSequenceLineService(_unitOfWork).Delete(item.ProcessSequenceLineId);
                }

                // Now delete the Purhcase Order Header
                new ProcessSequenceHeaderService(_unitOfWork).Delete(vm.id);
                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                //Commit the DB
                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProcessSequence).DocumentTypeId,
                    DocId = vm.id,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                }));

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
