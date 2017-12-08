using System;
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
using System.Collections.Generic;
using System.Xml.Linq;
using Jobs.Helpers;


namespace Jobs.Controllers
{
    [Authorize]
    public class MachineController : System.Web.Mvc.Controller
    {
          private ApplicationDbContext db = new ApplicationDbContext();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();
        IProductUidService _ProductUidService;
          IUnitOfWork _unitOfWork;
          IExceptionHandlingService _exception;
          public MachineController(IProductUidService ProductUidService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
          {
              _ProductUidService = ProductUidService;
              _unitOfWork = unitOfWork;
              _exception = exec;
          }
        // GET: /ProductUidMaster/
        
          public ActionResult Index(int Id)
          { 
            ViewBag.id = Id;
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(Id).DocumentTypeName;
            //var ProductUid = _ProductUidService.GetProductUidListMachine(Id);
            var ProductUid = _ProductUidService.GetProductUidListMachineDetail(Id);            
            return View(ProductUid);
          }

        // GET: /ProductUidMaster/Create
        

        public ActionResult Create(int id)
          {
            int GoDownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];
            ProductUid vm = new ProductUid();
            vm.IsActive = true;
            vm.GenDocTypeId = id;
            vm.CurrenctGodownId = GoDownId;
            ViewBag.id = id;
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
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
            ViewBag.id = vm.GenDocTypeId;
            if (vm.ProductUIDId <= 0)
            {
                var Test = (from p in db.ProductUid where p.ProductUidName == pt.ProductUidName && p.ProductId == pt.ProductId && p.GenDocTypeId == pt.GenDocTypeId select p).ToList();
                if (Test.Count() > 0)
                {
                    ModelState.AddModelError("ProductUidName", "Already Exist");
                }
            }
            else
            {
                var Test = (from p in db.ProductUid where p.ProductUidName == pt.ProductUidName && p.ProductId == pt.ProductId && p.GenDocTypeId == pt.GenDocTypeId && p.ProductUIDId != pt.ProductUIDId select p).ToList();
                if (Test.Count() > 0)
                {
                    ModelState.AddModelError("ProductUidName", "Already Exist");
                }
            }
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
                      DocTypeId=pt.GenDocTypeId,
                      Narration = "Machine Name" + pt.ProductUidName,
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
                   
                    return RedirectToAction("Index", new { id = vm.GenDocTypeId }).Success("Data saved successfully");
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
                          DocTypeId = pt.GenDocTypeId,
                          Narration = "Machine Name" + pt.ProductUidName,
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
                    return RedirectToAction("Index", new { id = vm.GenDocTypeId }).Success("Data saved successfully");                   

                  }

              }
              return View("Create", vm);
          }


        // GET: /ProductMaster/Edit/5
        
        public ActionResult Edit(int id)
        {
            ProductUid pt = _ProductUidService.Find(id);
            ViewBag.id = pt.GenDocTypeId;
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
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {
                var temp = _ProductUidService.Find(vm.id);
                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });
            _ProductUidService.Delete(vm.id);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductUid).DocumentTypeId,
                    DocId = vm.id,

                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }


        public ActionResult NextPage(int id, int GenDocTypeId)//CurrentHeaderId
        {
            var nextId = _ProductUidService.NextId(id, GenDocTypeId);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, int GenDocTypeId)//CurrentHeaderId
        {
            var nextId = _ProductUidService.PrevId(id, GenDocTypeId);
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


        //public JsonResult GetProductUidValidation(string ProductUID, bool PostedInStock, int GodownId)
        //{
        //    return Json(new ProductUidService(_unitOfWork).ValidateUID(ProductUID, PostedInStock, GodownId),JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult GetProductUidReceiveValidation(string ProductUID, bool PostedInStock, int GodownId)
        //{
        //    return Json(new ProductUidService(_unitOfWork).ValidateUIDOnReceive(ProductUID, PostedInStock, GodownId), JsonRequestBehavior.AllowGet);
        //}


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
