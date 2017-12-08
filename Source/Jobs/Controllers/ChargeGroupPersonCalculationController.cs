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
using Core.Common;
using System.Net.Http;
using System.IO;
using System.Web.UI.WebControls;
using System.Web.UI;
using Presentation.ViewModels;
using Model.ViewModels;
using AutoMapper;
using System.Configuration;
using Jobs.Helpers;
using Presentation;
using System.Text;
using System.Web.Script.Serialization;
using System.Data.Entity.Validation;
using Model.ViewModel;

namespace Jobs.Controllers
{
    
    [Authorize]
    public class ChargeGroupPersonCalculationController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IChargeGroupPersonCalculationService _ChargeGroupPersonCalculationService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ChargeGroupPersonCalculationController(IChargeGroupPersonCalculationService ChargeGroupPersonCalculation, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ChargeGroupPersonCalculationService = ChargeGroupPersonCalculation;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _ChargeGroupPersonCalculationService.GetChargeGroupPersonCalculationList(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        private void PrepareViewBag(ChargeGroupPersonCalculationViewModel svm)
        {
        }

        public ActionResult _Create(int Id) //Id ==>Product Id
        {
            ChargeGroupPersonCalculationViewModel s = new ChargeGroupPersonCalculationViewModel();
            s.ChargeGroupPersonId = Id;
            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ChargeGroupPersonCalculationViewModel svm)
        {
            if (ModelState.IsValid)
            {        
                if(svm.ChargeGroupPersonCalculationId == 0)
                {
                    ChargeGroupPersonCalculation ChargeGroupPersonCalculation = new ChargeGroupPersonCalculation();

                    ChargeGroupPersonCalculation.ChargeGroupPersonId = svm.ChargeGroupPersonId;
                    ChargeGroupPersonCalculation.DocTypeId = svm.DocTypeId;
                    ChargeGroupPersonCalculation.DivisionId = svm.DivisionId;
                    ChargeGroupPersonCalculation.SiteId = svm.SiteId;
                    ChargeGroupPersonCalculation.CalculationId = svm.CalculationId;
                    ChargeGroupPersonCalculation.CreatedDate = DateTime.Now;
                    ChargeGroupPersonCalculation.ModifiedDate = DateTime.Now;
                    ChargeGroupPersonCalculation.CreatedBy = User.Identity.Name;
                    ChargeGroupPersonCalculation.ModifiedBy = User.Identity.Name;
                    ChargeGroupPersonCalculation.ObjectState = Model.ObjectState.Added;
                    _ChargeGroupPersonCalculationService.Create(ChargeGroupPersonCalculation);


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
                    return RedirectToAction("_Create", new { id = svm.ChargeGroupPersonId });
                }
                else
                {
                    ChargeGroupPersonCalculation ChargeGroupPersonCalculation = _ChargeGroupPersonCalculationService.Find(svm.ChargeGroupPersonCalculationId);
                    StringBuilder logstring = new StringBuilder();

                    ChargeGroupPersonCalculation.ChargeGroupPersonId = svm.ChargeGroupPersonId;
                    ChargeGroupPersonCalculation.DocTypeId = svm.DocTypeId;
                    ChargeGroupPersonCalculation.DivisionId = svm.DivisionId;
                    ChargeGroupPersonCalculation.SiteId = svm.SiteId;
                    ChargeGroupPersonCalculation.CalculationId = svm.CalculationId;
                    ChargeGroupPersonCalculation.ModifiedDate = DateTime.Now;
                    ChargeGroupPersonCalculation.ModifiedBy = User.Identity.Name;
                    ChargeGroupPersonCalculation.ObjectState = Model.ObjectState.Modified;
                    _ChargeGroupPersonCalculationService.Update(ChargeGroupPersonCalculation);


                    //Saving the Activity Log
                        ActivityLog al = new ActivityLog()
                        {
                            ActivityType = (int)ActivityTypeContants.Modified,
                            DocId = ChargeGroupPersonCalculation.ChargeGroupPersonCalculationId,
                            CreatedDate = DateTime.Now,
                            Narration = logstring.ToString(),
                            CreatedBy = User.Identity.Name,
                            //DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.ChargeGroupPersonCalculation).DocumentTypeId,
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
            ChargeGroupPersonCalculationViewModel Vm = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculationForEdit(id);
            return PartialView("_Create", Vm);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChargeGroupPersonCalculationViewModel Vm = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculationForEdit(id);

            if (Vm == null)
            {
                return HttpNotFound();
            }

            return View(Vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ChargeGroupPersonCalculationViewModel vm)
        {
            ChargeGroupPersonCalculation ChargeGroupPersonCalculation = _ChargeGroupPersonCalculationService.Find(vm.ChargeGroupPersonCalculationId);
            _ChargeGroupPersonCalculationService.Delete(vm.ChargeGroupPersonCalculationId);

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


        public JsonResult CheckForValidationinEdit(int DocTypeId, int? ChargeGroupPersonId, int? SiteId, int? DivisionId, int ChargeGroupPersonCalculationId)
        {
            var temp = (_ChargeGroupPersonCalculationService.CheckForChargeGroupPersonCalculationExists(DocTypeId, ChargeGroupPersonId, SiteId, DivisionId, ChargeGroupPersonCalculationId));
            return Json(new { returnvalue = temp });
        }

        public JsonResult CheckForValidation(int DocTypeId, int? ChargeGroupPersonId, int? SiteId, int? DivisionId)
        {
            var temp = (_ChargeGroupPersonCalculationService.CheckForChargeGroupPersonCalculationExists(DocTypeId, ChargeGroupPersonId, SiteId, DivisionId));
            return Json(new { returnvalue = temp });
        }
    }
}
