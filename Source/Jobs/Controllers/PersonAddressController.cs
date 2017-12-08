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
using System.Collections.Generic;

namespace Jobs.Controllers
{
    
    [Authorize]
    public class PersonAddressController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IPersonAddressService _PersonAddressService;
        IPersonService _PersonService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public PersonAddressController(IPersonAddressService PersonAddress, IPersonService Person, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PersonAddressService = PersonAddress;
            _PersonService = Person;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _PersonAddressService.GetPersonAddressListForIndex(id).ToList();                               
            return Json(p, JsonRequestBehavior.AllowGet);
            
        }


        public void PrepareViewBag()
        {
            List<SelectListItem> AddressType = new List<SelectListItem>();
            AddressType.Add(new SelectListItem { Text = AddressTypeConstants.Permanent, Value = AddressTypeConstants.Permanent });
            AddressType.Add(new SelectListItem { Text = AddressTypeConstants.Temporary, Value = AddressTypeConstants.Temporary });
            AddressType.Add(new SelectListItem { Text = AddressTypeConstants.Work, Value = AddressTypeConstants.Work });
            AddressType.Add(new SelectListItem { Text = AddressTypeConstants.Office, Value = AddressTypeConstants.Office });
            AddressType.Add(new SelectListItem { Text = AddressTypeConstants.Godown, Value = AddressTypeConstants.Godown });

            ViewBag.AddressTypeList = new SelectList(AddressType, "Value", "Text");

        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            PersonAddressViewModel s = new PersonAddressViewModel();
            s.PersonId = Id;
            PrepareViewBag();
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PersonAddressViewModel svm)
        {
            if (svm.AddressType == null || svm.AddressType == "")
            {
                PrepareViewBag();
                string message = "Address Type is required.";
                ModelState.AddModelError("", message);
                return PartialView("_Create", svm);
            }

            if (ModelState.IsValid)
            {        
                if(svm.PersonAddressId == 0)
                {

                    PersonAddress PersonAddress = new PersonAddress();
                   
                    PersonAddress.PersonId = svm.PersonId;
                    PersonAddress.AddressType = svm.AddressType;
                    PersonAddress.Address = svm.Address;
                    PersonAddress.CityId = svm.CityId;
                    PersonAddress.Zipcode = svm.Zipcode;
                    PersonAddress.CreatedDate = DateTime.Now;
                    PersonAddress.ModifiedDate = DateTime.Now;
                    PersonAddress.CreatedBy = User.Identity.Name;
                    PersonAddress.ModifiedBy = User.Identity.Name;
                    PersonAddress.ObjectState = Model.ObjectState.Added;
                    _PersonAddressService.Create(PersonAddress);


                    try
                    {
                        _unitOfWork.Save();
                    }
                   
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        PrepareViewBag();
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", svm);
                    }
                    return RedirectToAction("_Create", new { id = PersonAddress.PersonId });
                }
                else
                {
                    PersonAddress PersonAddress = _PersonAddressService.Find(svm.PersonAddressId);

                    PersonAddress.AddressType = svm.AddressType;
                    PersonAddress.Address = svm.Address;
                    PersonAddress.CityId = svm.CityId;
                    PersonAddress.Zipcode = svm.Zipcode;

                    _PersonAddressService.Update(PersonAddress);

                    try
                    {
                        _unitOfWork.Save();
                    }
                 
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        PrepareViewBag();
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", svm);

                    }


                    return Json(new { success = true });

                }
            }

            PrepareViewBag();
            return PartialView("_Create",svm);
        }

        
        public ActionResult _Edit(int id)
        {
            PersonAddress temp = _PersonAddressService.GetPersonAddress(id);

            PersonAddressViewModel s = new PersonAddressViewModel();
            s.PersonId = temp.PersonId;
            s.PersonAddressId = temp.PersonAddressID;
            s.AddressType = temp.AddressType;
            s.Address = temp.Address;
            s.CityId = temp.CityId;
            s.Zipcode = temp.Zipcode;
            
            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag();
            return PartialView("_Create", s);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PersonAddress PersonAddress = _PersonAddressService.Find(id);
            if (PersonAddress == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag();
            return View(PersonAddress);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(PersonAddressViewModel vm)
        {
            PersonAddress PersonAddress = _PersonAddressService.Find(vm.PersonAddressId);


            PersonAddress PP = new PersonAddress() { PersonAddressID = vm.PersonAddressId };

            //_PersonProcessService.Delete(vm.PersonProcessId);
            db.PersonAddress.Attach(PP);
            PP.ObjectState = Model.ObjectState.Deleted;
            db.PersonAddress.Remove(PP);

            try
            {
                db.SaveChanges();
            }
         
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                PrepareViewBag();
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
