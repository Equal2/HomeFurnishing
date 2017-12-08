using System;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Presentation;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    
    [Authorize]
    public class PersonBankAccountController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IPersonBankAccountService _PersonBankAccountService;
        IPersonService _PersonService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public PersonBankAccountController(IPersonBankAccountService PersonBankAccount, IPersonService Person, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PersonBankAccountService = PersonBankAccount;
            _PersonService = Person;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _PersonBankAccountService.GetPersonBankAccountListForIndex(id);                               
            return Json(p, JsonRequestBehavior.AllowGet);
            
        }

        private void PrepareViewBag(PersonBankAccount s)
        {
        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            PersonBankAccount s = new PersonBankAccount();
            s.PersonId = Id;
            PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PersonBankAccount svm)
        {

            if (ModelState.IsValid)
            {        
                if(svm.PersonBankAccountID == 0)
                {
                    svm.CreatedDate = DateTime.Now;
                    svm.ModifiedDate = DateTime.Now;
                    svm.CreatedBy = User.Identity.Name;
                    svm.ModifiedBy = User.Identity.Name;
                    svm.ObjectState = Model.ObjectState.Added;
                    _PersonBankAccountService.Create(svm);


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
                    return RedirectToAction("_Create", new { id = svm.PersonId });
                }
                else
                {
                    svm.ModifiedBy = User.Identity.Name;
                    svm.ModifiedDate = DateTime.Now;
                    _PersonBankAccountService.Update(svm);


                    //Saving the Activity Log
                        ActivityLog al = new ActivityLog()
                        {
                            ActivityType = (int)ActivityTypeContants.Modified,
                            DocId = svm.PersonBankAccountID,
                            CreatedDate = DateTime.Now,
                            //Narration = logstring.ToString(),
                            CreatedBy = User.Identity.Name,
                            //DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.PersonBankAccount).DocumentTypeId,
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
            PersonBankAccount temp = _PersonBankAccountService.GetPersonBankAccount(id);

            PrepareViewBag(temp);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", temp);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PersonBankAccount PersonBankAccount = _PersonBankAccountService.Find(id);
            if (PersonBankAccount == null)
            {
                return HttpNotFound();
            }
            return View(PersonBankAccount);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PersonBankAccount PersonBankAccount = _PersonBankAccountService.Find(id);
            _PersonBankAccountService.Delete(id);
            

            _unitOfWork.Save();

            return RedirectToAction("Index", new { Id = PersonBankAccount.PersonId }).Success("Data deleted successfully");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(PersonBankAccount vm)
        {
            _PersonBankAccountService.Delete(vm.PersonBankAccountID);

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
