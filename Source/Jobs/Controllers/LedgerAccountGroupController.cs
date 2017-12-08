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
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class LedgerAccountGroupController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ILedgerAccountGroupService _LedgerAccountGroupService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public LedgerAccountGroupController(ILedgerAccountGroupService LedgerAccountGroupService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _LedgerAccountGroupService = LedgerAccountGroupService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /ProductMaster/

        public ActionResult Index()
        {
            var LedgerAccountGroup = _LedgerAccountGroupService.GetLedgerAccountGroupList().ToList();
            return View(LedgerAccountGroup);
        }

        public void PrepareViewBag(LedgerAccountGroup vm)
        {

            List<SelectListItem> nature = new List<SelectListItem>();
            nature.Add(new SelectListItem { Text = LedgerAccountNatureConstants.Assets, Value = LedgerAccountNatureConstants.Assets });
            nature.Add(new SelectListItem { Text = LedgerAccountNatureConstants.Liabilities, Value = LedgerAccountNatureConstants.Liabilities });
            nature.Add(new SelectListItem { Text = LedgerAccountNatureConstants.Income, Value = LedgerAccountNatureConstants.Income });
            nature.Add(new SelectListItem { Text = LedgerAccountNatureConstants.Expenditure, Value = LedgerAccountNatureConstants.Expenditure });


            List<SelectListItem> type = new List<SelectListItem>();
            type.Add(new SelectListItem { Text = LedgerAccountTypeConstants.Real, Value = LedgerAccountTypeConstants.Real });
            type.Add(new SelectListItem { Text = LedgerAccountTypeConstants.Personal, Value = LedgerAccountTypeConstants.Personal });
            type.Add(new SelectListItem { Text = LedgerAccountTypeConstants.Nominal, Value = LedgerAccountTypeConstants.Nominal });
            type.Add(new SelectListItem { Text = LedgerAccountTypeConstants.Bank, Value = LedgerAccountTypeConstants.Bank });

            if (vm == null)
            {
                ViewBag.LedgerAccountNature = new SelectList(nature, "Value", "Text");
                ViewBag.LedgerAccountType = new SelectList(type, "Value", "Text");
            }
            else
            {
                ViewBag.LedgerAccountNature = new SelectList(nature, "Value", "Text", vm.LedgerAccountNature);
                ViewBag.LedgerAccountType = new SelectList(type, "Value", "Text", vm.LedgerAccountType);
            }
        }


        // GET: /ProductMaster/Create

        public ActionResult Create()
        {
            PrepareViewBag(null);
            LedgerAccountGroup vm = new LedgerAccountGroup();
            vm.IsActive = true;
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(LedgerAccountGroup vm)
        {
            LedgerAccountGroup pt = vm;

            if (vm.ParentLedgerAccountGroupId == null || vm.ParentLedgerAccountGroupId == 0)
            {
                string message = "Under Group is required.";
                ModelState.AddModelError("", message);
                PrepareViewBag(vm);
                return View("Create", vm);
            }

            if (ModelState.IsValid)
            {

                if (vm.LedgerAccountGroupId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _LedgerAccountGroupService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm);
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.LedgerAccountGroup).DocumentTypeId,
                        DocId = pt.LedgerAccountGroupId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("Create").Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    LedgerAccountGroup temp = _LedgerAccountGroupService.Find(pt.LedgerAccountGroupId);

                    LedgerAccountGroup ExRec = Mapper.Map<LedgerAccountGroup>(temp);

                    temp.LedgerAccountGroupName = pt.LedgerAccountGroupName;
                    temp.LedgerAccountNature = pt.LedgerAccountNature;
                    temp.LedgerAccountType = pt.LedgerAccountType;
                    temp.ParentLedgerAccountGroupId = pt.ParentLedgerAccountGroupId;
                    temp.IsActive = pt.IsActive;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _LedgerAccountGroupService.Update(temp);

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
                        PrepareViewBag(vm);
                        return View("Create", pt);
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.LedgerAccountGroup).DocumentTypeId,
                        DocId = temp.LedgerAccountGroupId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index").Success("Data saved successfully");
                }
            }
            PrepareViewBag(vm);
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            LedgerAccountGroup pt = _LedgerAccountGroupService.Find(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(pt);
            return View("Create", pt);
        }
        public ActionResult Modify(int id)
        {
            LedgerAccountGroup pt = _LedgerAccountGroupService.Find(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(pt);
            return View("Create", pt);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LedgerAccountGroup LedgerAccountGroup = _LedgerAccountGroupService.Find(id);

            if (LedgerAccountGroup == null)
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

                var temp = _LedgerAccountGroupService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                _LedgerAccountGroupService.Delete(vm.id);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.LedgerAccountGroup).DocumentTypeId,
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
            var nextId = _LedgerAccountGroupService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _LedgerAccountGroupService.PrevId(id);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.LedgerAccountGroup);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        public JsonResult GetParentGroupDetailJson(int ParentLedgerGroupId)
        {
            return Json(new LedgerAccountGroupService(_unitOfWork).GetParentGroupDetail(ParentLedgerGroupId));
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
