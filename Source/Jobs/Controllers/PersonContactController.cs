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
using Jobs.Helpers;

namespace Jobs.Controllers
{
    
    [Authorize]
    public class PersonContactController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IPersonContactService _PersonContactService;
        IPersonService _PersonService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public PersonContactController(IPersonContactService PersonContact, IPersonService Person, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PersonContactService = PersonContact;
            _PersonService = Person;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _PersonContactService.GetPersonContactListForIndex(id);
            
            return Json(p, JsonRequestBehavior.AllowGet);
            
        }

        private void PrepareViewBag(PersonContactViewModel s)
        {
            ViewBag.PersonContactTypeList = new PersonContactTypeService(_unitOfWork).GetPersonContactTypeList().ToList();
        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            PersonContactViewModel s = new PersonContactViewModel();
            s.PersonID = Id;
            PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PersonContactViewModel svm)
        {

            if (ModelState.IsValid)
            {        
                if(svm.PersonContactId == 0)
                {

                    Person person = new Person();
                    PersonContact personcontact = new PersonContact();


                    person.Name = svm.Name;
                    person.Suffix = svm.Suffix;
                    person.Code = svm.Code;
                    person.Phone = svm.Phone;
                    person.Mobile = svm.Mobile;
                    person.Email = svm.Email;
                    person.CreatedDate = DateTime.Now;
                    person.ModifiedDate = DateTime.Now;
                    person.CreatedBy = User.Identity.Name;
                    person.ModifiedBy = User.Identity.Name;
                    person.ObjectState = Model.ObjectState.Added;
                    _PersonService.Create(person);





                    personcontact.PersonId = svm.PersonID;
                    personcontact.ContactId = person.PersonID;
                    personcontact.PersonContactTypeId = svm.PersonContactTypeId;
                    personcontact.CreatedDate = DateTime.Now;
                    personcontact.ModifiedDate = DateTime.Now;
                    personcontact.CreatedBy = User.Identity.Name;
                    personcontact.ModifiedBy = User.Identity.Name;
                    personcontact.ObjectState = Model.ObjectState.Added;
                    _PersonContactService.Create(personcontact);


                    try
                    {
                        _unitOfWork.Save();
                    }
                   
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(null);
                        return PartialView("_Create", svm);
                    }
                    return RedirectToAction("_Create", new { id = person.PersonID });
                }
                else
                {
                    PersonContact personcontact = _PersonContactService.Find(svm.PersonContactId);
                    Person person = _PersonService.Find(personcontact.ContactId);
                    StringBuilder logstring = new StringBuilder();

                    person.Name = svm.Name;
                    person.Suffix = svm.Suffix;
                    person.Code = svm.Code;
                    person.Phone = svm.Phone;
                    person.Mobile = svm.Mobile;
                    person.Email = svm.Email;
                    person.ModifiedBy = User.Identity.Name;
                    person.ModifiedDate = DateTime.Now;
                    _PersonService.Update(person);

                    personcontact.PersonContactTypeId = svm.PersonContactTypeId;
                    _PersonContactService.Update(personcontact);


                    //Saving the Activity Log
                        ActivityLog al = new ActivityLog()
                        {
                            ActivityType = (int)ActivityTypeContants.Modified,
                            DocId = personcontact.PersonContactID,
                            CreatedDate = DateTime.Now,
                            Narration = logstring.ToString(),
                            CreatedBy = User.Identity.Name,
                            //DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.PersonContact).DocumentTypeId,
                        };
                        new ActivityLogService(_unitOfWork).Create(al);
                    //End of Saving the Activity Log


                    try
                    {
                        _unitOfWork.Save();
                    }
                 
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(null);
                        return PartialView("_Create", svm);

                    }


                    return Json(new { success = true });

                }
            }

            PrepareViewBag(svm);
            return PartialView("_Create",svm);
        }

        
        public ActionResult _Edit(int id)
        {
            PersonContact temp = _PersonContactService.GetPersonContact(id);
            Person person = _PersonService.Find(temp.ContactId);
            PersonContactViewModel s = Mapper.Map<PersonContact, PersonContactViewModel>(temp);

            s.Name = person.Name;
            s.Mobile = person.Mobile;
            s.Phone = person.Phone;
            s.Email = person.Email;
            s.Code = person.Code;
            s.Suffix = person.Suffix;
            
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
            PersonContact PersonContact = _PersonContactService.Find(id);
            if (PersonContact == null)
            {
                return HttpNotFound();
            }
            return View(PersonContact);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PersonContact PersonContact = _PersonContactService.Find(id);
            _PersonContactService.Delete(id);
            

            _unitOfWork.Save();

            return RedirectToAction("Index", new { Id = PersonContact.PersonId }).Success("Data deleted successfully");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(PersonContactViewModel vm)
        {
            PersonContact PersonContact = _PersonContactService.Find(vm.PersonContactId);
            Person person = _PersonService.Find(PersonContact.ContactId);
            _PersonContactService.Delete(vm.PersonContactId);
            _PersonService.Delete(person.PersonID);

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
