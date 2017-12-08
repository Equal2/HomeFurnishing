using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Presentation;
using System.Text;

namespace Jobs.Controllers
{
    
    [Authorize]
    public class PersonProcessController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IPersonProcessService _PersonProcessService;
        IPersonService _PersonService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public PersonProcessController(IPersonProcessService PersonProcess, IPersonService Person, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PersonProcessService = PersonProcess;
            _PersonService = Person;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _PersonProcessService.GetPersonProcessListForPerson(id).ToList();                               
            return Json(p, JsonRequestBehavior.AllowGet);
            
        }   

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            PersonProcessViewModel s = new PersonProcessViewModel();
            s.PersonId = Id;
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PersonProcessViewModel svm)
        {

            if (ModelState.IsValid)
            {        
                if(svm.PersonProcessId == 0)
                {

                    PersonProcess PersonProcess = new PersonProcess();
                   
                    PersonProcess.PersonId = svm.PersonId;
                    PersonProcess.PersonRateGroupId = svm.PersonRateGroupId;
                    PersonProcess.ProcessId = svm.ProcessId;
                    PersonProcess.CreatedDate = DateTime.Now;
                    PersonProcess.ModifiedDate = DateTime.Now;
                    PersonProcess.CreatedBy = User.Identity.Name;
                    PersonProcess.ModifiedBy = User.Identity.Name;
                    PersonProcess.ObjectState = Model.ObjectState.Added;
                    _PersonProcessService.Create(PersonProcess);


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
                    return RedirectToAction("_Create", new { id = PersonProcess.PersonId });
                }
                else
                {
                    PersonProcess PersonProcess = _PersonProcessService.Find(svm.PersonProcessId);

                    PersonProcess.ProcessId = svm.ProcessId;
                    PersonProcess.PersonRateGroupId = svm.PersonRateGroupId;

                    _PersonProcessService.Update(PersonProcess);

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


                    return Json(new { success = true });

                }
            }

            return PartialView("_Create",svm);
        }

        
        public ActionResult _Edit(int id)
        {
            PersonProcess temp = _PersonProcessService.GetPersonProcess(id);

            PersonProcessViewModel s = new PersonProcessViewModel();
            s.PersonId = temp.PersonId;
            s.PersonProcessId = temp.PersonProcessId;
            s.PersonRateGroupId = temp.PersonRateGroupId;
            s.ProcessId = temp.ProcessId;
            
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
            PersonProcess PersonProcess = _PersonProcessService.Find(id);
            if (PersonProcess == null)
            {
                return HttpNotFound();
            }
            return View(PersonProcess);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(PersonProcessViewModel vm)
        {
            PersonProcess PersonProcess = _PersonProcessService.Find(vm.PersonProcessId);

            PersonProcess PP = new PersonProcess() { PersonProcessId = vm.PersonProcessId };

            //_PersonProcessService.Delete(vm.PersonProcessId);
            db.PersonProcess.Attach(PP);
            PP.ObjectState = Model.ObjectState.Deleted;
            db.PersonProcess.Remove(PP);

            try
            {
                db.SaveChanges();
            }
         
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("EditSize", vm);

            }
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
