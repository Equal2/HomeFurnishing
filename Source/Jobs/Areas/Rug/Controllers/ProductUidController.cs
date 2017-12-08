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
    public class ProductUidController : System.Web.Mvc.Controller
    {
          private ApplicationDbContext db = new ApplicationDbContext();

          IProductUidService _ProductUidService;
          IUnitOfWork _unitOfWork;
          IExceptionHandlingService _exception;
          public ProductUidController(IProductUidService ProductUidService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
          {
              _ProductUidService = ProductUidService;
              _unitOfWork = unitOfWork;
              _exception = exec;
          }
        // GET: /ProductUidMaster/
        
          public ActionResult Index()
          {              
              var ProductUid = _ProductUidService.GetProductUidList().ToList();
              return View(ProductUid);
              //return RedirectToAction("Create");
          }

          // GET: /ProductUidMaster/Create
        
          public ActionResult Create()
          {
              ProductUid vm = new ProductUid();
              vm.IsActive = true;
              return View("Create",vm);
          }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
          public ActionResult Post(ProductUid vm)
          {
              ProductUid pt = vm; 
              if (ModelState.IsValid)
              {                  


                  if(vm.ProductUIDId<=0)
                  { 
                  pt.CreatedDate = DateTime.Now;
                  pt.ModifiedDate = DateTime.Now;
                  pt.CreatedBy = User.Identity.Name;
                  pt.ModifiedBy = User.Identity.Name;
                  pt.ObjectState = Model.ObjectState.Added;
                  _ProductUidService.Create(pt);

                  ActivityLog log = new ActivityLog()
                  {
                      ActivityType = (int)(ActivityTypeContants.Added),
                      CreatedBy = User.Identity.Name,
                      CreatedDate = DateTime.Now,
                      DocId = pt.ProductUIDId,
                      Narration = "A new ProductUid is created with the Id " + pt.ProductUIDId,
                  };

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
                  else
                  {

                      ProductUid temp = _ProductUidService.Find(pt.ProductUIDId);
                      temp.ProductUidName = pt.ProductUidName;
                      temp.IsActive = pt.IsActive;
                      temp.ModifiedDate = DateTime.Now;
                      temp.ModifiedBy = User.Identity.Name;
                      temp.ObjectState = Model.ObjectState.Modified;
                      _ProductUidService.Update(temp);

                      ActivityLog log = new ActivityLog()
                      {
                          ActivityType = (int)(ActivityTypeContants.Modified),
                          CreatedBy = User.Identity.Name,
                          CreatedDate = DateTime.Now,
                          DocId = pt.ProductUIDId,
                          Narration = "Delivery Terms is modified with the name" + pt.ProductUidName,
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

                      return RedirectToAction("Index").Success("Data saved successfully");

                  }

              }
              return View("Create", vm);
          }


        // GET: /ProductMaster/Edit/5
        
        public ActionResult Edit(int id)
        {
            ProductUid pt = _ProductUidService.Find(id);
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
            ProductUid ProductUid = db.ProductUid.Find(id);
            if (ProductUid == null)
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
                var temp = _ProductUidService.Find(vm.id);
                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = vm.id,
                    UserRemark = vm.Reason,
                    Narration = "Delivery terms is deleted with Name:" + temp.ProductUidName,
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.SaleOrder).DocumentTypeId,
                    UploadDate = DateTime.Now,

                };
                new ActivityLogService(_unitOfWork).Create(al);
            

            _ProductUidService.Delete(vm.id);

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

        //[HttpGet]
        //public ActionResult NextPage(int id)//CurrentHeaderId
        //{
        //    var nextId = _ProductUidService.NextId(id);
        //    return RedirectToAction("Edit", new { id = nextId });
        //}
        //[HttpGet]
        //public ActionResult PrevPage(int id)//CurrentHeaderId
        //{
        //    var nextId = _ProductUidService.PrevId(id);
        //    return RedirectToAction("Edit", new { id = nextId });
        //}

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

        public JsonResult GetProductUidReceiveValidation(string ProductUID, bool PostedInStock, int GodownId)
        {
            return Json(new ProductUidService(_unitOfWork).ValidateUIDOnReceive(ProductUID, PostedInStock, GodownId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductUidValidation(string ProductUID, bool PostedInStock, int GodownId)
        {
            return Json(new ProductUidService(_unitOfWork).ValidateUID(ProductUID, PostedInStock, GodownId),JsonRequestBehavior.AllowGet);
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
