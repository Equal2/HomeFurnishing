using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Presentation.ViewModels;
using Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ChargeGroupPersonController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();


        IChargeGroupPersonService _ChargeGroupPersonService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ChargeGroupPersonController(IChargeGroupPersonService ChargeGroupPersonService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ChargeGroupPersonService = ChargeGroupPersonService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /ChargeGroupPersonMaster/

        public ActionResult Index()
        {
            var chargeGroupPerson = _ChargeGroupPersonService.GetChargeGroupPersonList(0);
            return View(chargeGroupPerson);
        }

        private void PrepareViewBag()
        {
        }


        // GET: /ChargeGroupPersonMaster/Create

        public ActionResult Create()
        {
            ChargeGroupPerson vm = new ChargeGroupPerson();
            vm.IsActive = true;
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ChargeGroupPerson pt)
        {
            if (ModelState.IsValid)
            {

                if (pt.ChargeGroupPersonId <= 0)
                {

                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ChargeGroupPersonService.Create(pt);

                    ActivityLog log = new ActivityLog()
                    {
                        ActivityType = (int)(ActivityTypeContants.Added),
                        CreatedBy = User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        DocId = pt.ChargeGroupPersonId,
                        Narration = "A new ChargeGroupPerson is created with the Id " + pt.ChargeGroupPersonId,
                    };

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

                    return RedirectToAction("Create").Success("Data saved successfully");

                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ChargeGroupPerson temp = _ChargeGroupPersonService.Find(pt.ChargeGroupPersonId);

                    ChargeGroupPerson ExRec = Mapper.Map<ChargeGroupPerson>(temp);

                    temp.ChargeGroupPersonName = pt.ChargeGroupPersonName;
                    temp.IsActive = pt.IsActive;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _ChargeGroupPersonService.Update(temp);

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

                    //LogActivity.LogActivityDetail(new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ChargeGroupPerson).DocumentTypeId,
                    //temp.ChargeGroupPersonId,
                    //null,
                    //(int)ActivityTypeContants.Modified,
                    //"",
                    //User.Identity.Name, temp.ChargeGroupPersonName, Modifications);

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ChargeGroupPerson).DocumentTypeId,
                        DocId = temp.ChargeGroupPersonId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index").Success("Data saved successfully");


                }

            }
            return View("Create", pt);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            ChargeGroupPerson pt = _ChargeGroupPersonService.Find(id);
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
            ChargeGroupPerson ChargeGroupPerson = db.ChargeGroupPerson.Find(id);
            if (ChargeGroupPerson == null)
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
                var temp = _ChargeGroupPersonService.Find(vm.id);
                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });
                _ChargeGroupPersonService.Delete(vm.id);

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

                //LogActivity.LogActivityDetail(new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ChargeGroupPerson).DocumentTypeId,
                //temp.ChargeGroupPersonId,
                //null,
                //(int)ActivityTypeContants.Deleted,
                //vm.Reason,
                //User.Identity.Name,
                //temp.ChargeGroupPersonName, Modifications);

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ChargeGroupPerson).DocumentTypeId,
                    DocId = temp.ChargeGroupPersonId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                }));  


                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }


        [HttpGet]
        public ActionResult NextPage(int id, int ctypeid)//CurrentHeaderId
        {
            var nextId = _ChargeGroupPersonService.NextId(id, ctypeid);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, int ctypeid)//CurrentHeaderId
        {
            var nextId = _ChargeGroupPersonService.PrevId(id, ctypeid);
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
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
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
