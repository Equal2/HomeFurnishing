using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Presentation.ViewModels;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class DocumentTypeController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IDocumentTypeService _DocumentTypeService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public DocumentTypeController(IDocumentTypeService DocumentTypeService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _DocumentTypeService = DocumentTypeService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /DocumentTypeMaster/

        public ActionResult Index()
        {
            PrepareViewBag();
            var DocumentType = _DocumentTypeService.GetDocumentTypeList();
            return View(DocumentType);
        }


        private void PrepareViewBag()
        {
            List<SelectListItem> Nature = new List<SelectListItem>();

            Nature.Add(new SelectListItem { Text = "Cr", Value = "Cr" });
            Nature.Add(new SelectListItem { Text = "Dr", Value = "Dr" });

            ViewBag.Nature = new SelectList(Nature, "Value", "Text");

            List<SelectListItem> Domains = new List<SelectListItem>();
            Domains.Add(new SelectListItem { Text = "LoginDomain", Value = "LoginDomain" });
            Domains.Add(new SelectListItem { Text = "MenuDomain", Value = "MenuDomain" });
            Domains.Add(new SelectListItem { Text = "SaleDomain", Value = "SaleDomain" });
            Domains.Add(new SelectListItem { Text = "Planningdomain", Value = "Planningdomain" });
            Domains.Add(new SelectListItem { Text = "Purchasedomain", Value = "Purchasedomain" });
            Domains.Add(new SelectListItem { Text = "JobsDomain", Value = "JobsDomain" });
            Domains.Add(new SelectListItem { Text = "Storedomain", Value = "Storedomain" });
            Domains.Add(new SelectListItem { Text = "FinanceDomain", Value = "FinanceDomain" });
            Domains.Add(new SelectListItem { Text = "HumanResourceDomain", Value = "HumanResourceDomain" });
            Domains.Add(new SelectListItem { Text = "JobsDomain", Value = "JobsDomain" });
            Domains.Add(new SelectListItem { Text = "CalculationDomain", Value = "CalculationDomain" });

            ViewBag.Domains = new SelectList(Domains, "Value", "Text");

        }


        // GET: /DocumentTypeMaster/Create

        public ActionResult Create()
        {
            PrepareViewBag();
            DocumentType vm = new DocumentType();
            vm.IsActive = true;
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(DocumentType pt)
        {
            if (ModelState.IsValid)
            {
                if (pt.DocumentTypeId <= 0)
                {

                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _DocumentTypeService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.DocumentType).DocumentTypeId,
                        DocId = pt.DocumentTypeId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("Create").Success("Data saved successfully");

                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    DocumentType temp = _DocumentTypeService.Find(pt.DocumentTypeId);

                    DocumentType ExRec = Mapper.Map<DocumentType>(temp);

                    temp.DocumentTypeName = pt.DocumentTypeName;
                    temp.DocumentTypeShortName = pt.DocumentTypeShortName;
                    temp.DocumentCategoryId = pt.DocumentCategoryId;
                    temp.ControllerActionId = pt.ControllerActionId;
                    temp.VoucherType = pt.VoucherType;
                    temp.Nature = pt.Nature;
                    temp.IconDisplayName = pt.IconDisplayName;
                    temp.ImageFileName = pt.ImageFileName;
                    temp.DomainName = pt.DomainName;
                    temp.ReportMenuId = pt.ReportMenuId;
                    temp.IsActive = pt.IsActive;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _DocumentTypeService.Update(temp);

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
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.DocumentType).DocumentTypeId,
                        DocId = temp.DocumentTypeId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index").Success("Data saved successfully");
                }
            }
            PrepareViewBag();
            return View("Create", pt);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            PrepareViewBag();
            DocumentType pt = _DocumentTypeService.Find(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", pt);
        }


        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentType DocumentType = db.DocumentType.Find(id);
            if (DocumentType == null)
            {
                return HttpNotFound();
            }

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
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            if (ModelState.IsValid)
            {
                var temp = _DocumentTypeService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                _DocumentTypeService.Delete(vm.id);
                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.DocumentType).DocumentTypeId,
                    DocId = vm.id,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }


        [HttpGet]
        public ActionResult NextPage(int id)//CurrentHeaderId
        {
            var nextId = _DocumentTypeService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _DocumentTypeService.PrevId(id);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.DocumentType);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

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
