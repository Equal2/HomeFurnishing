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
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class ServiceTaxCategoryController : System.Web.Mvc.Controller
    {
          private ApplicationDbContext db = new ApplicationDbContext();

          IServiceTaxCategoryService _ServiceTaxCategoryService;
          IUnitOfWork _unitOfWork;
          IExceptionHandlingService _exception;
          public ServiceTaxCategoryController(IServiceTaxCategoryService ServiceTaxCategoryService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
          {
              _ServiceTaxCategoryService = ServiceTaxCategoryService;
              _unitOfWork = unitOfWork;
              _exception = exec;
          }
        // GET: /ProductMaster/
        
          public ActionResult Index()
          { 
              var ServiceTaxCategory = _ServiceTaxCategoryService.GetServiceTaxCategoryList().ToList();
              return View(ServiceTaxCategory);
              //return RedirectToAction("Create");
          }

        // GET: /ProductMaster/Create
        
          public ActionResult Create()
          {              
              ServiceTaxCategory vm = new ServiceTaxCategory();
              return View("Create",vm);
          }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
          public ActionResult Create(ServiceTaxCategory vm)
          {
              ServiceTaxCategory pt = vm;              
                
              if (ModelState.IsValid)
              {
                  pt.CreatedDate = DateTime.Now;
                  pt.ModifiedDate = DateTime.Now;
                  pt.CreatedBy = User.Identity.Name;
                  pt.ModifiedBy = User.Identity.Name;
                  pt.ObjectState = Model.ObjectState.Added;
                  _ServiceTaxCategoryService.Create(pt);

                  try
                  {
                      _unitOfWork.Save();
                  }
                
                  catch (Exception ex)
                  {
                      string message = _exception.HandleException(ex);
                      ModelState.AddModelError("", message);
                      return View("Create", vm);

                  }


                  return RedirectToAction("Create").Success("Data saved successfully");
              }
              return View("Create", vm);
          }


        // GET: /ProductMaster/Edit/5
        
        public ActionResult Edit(int id)
        {
            ServiceTaxCategory pt = _ServiceTaxCategoryService.GetServiceTaxCategory(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", pt);
        }

        // POST: /ProductMaster/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ServiceTaxCategory pt)
        {

            if (ModelState.IsValid)
            {
                ServiceTaxCategory temp = _ServiceTaxCategoryService.Find(pt.ServiceTaxCategoryId);

                temp.ServiceTaxCategoryName = pt.ServiceTaxCategoryName;
                temp.ModifiedDate = DateTime.Now;
                temp.ModifiedBy = User.Identity.Name;
                temp.ObjectState = Model.ObjectState.Modified;
                _ServiceTaxCategoryService.Update(temp);

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


                return RedirectToAction("Index").Success("Data saved successfully");
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
            ServiceTaxCategory ServiceTaxCategory = _ServiceTaxCategoryService.GetServiceTaxCategory(id);            
            
            if (ServiceTaxCategory == null)
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

            if(ModelState.IsValid)
            {
                var temp = _ServiceTaxCategoryService.Find(vm.id);
                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = vm.id,
                    UserRemark = vm.Reason,
                    Narration = "Product collection is deleted with Name:" + temp.ServiceTaxCategoryName,
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.SaleOrder).DocumentTypeId,
                    UploadDate = DateTime.Now,

                };
                new ActivityLogService(_unitOfWork).Create(al);

            _ServiceTaxCategoryService.Delete(vm.id);
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

            return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }


        [HttpGet]
        public ActionResult NextPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _ServiceTaxCategoryService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _ServiceTaxCategoryService.PrevId(id);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ServiceTaxCategory );

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
