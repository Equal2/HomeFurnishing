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

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class ProductQualityController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductQualityService _ProductQualityService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductQualityController(IProductQualityService ProductQualityService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductQualityService = ProductQualityService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /ProductMaster/

        public ActionResult Index(int id)
        {
            var ProductQuality = _ProductQualityService.GetProductQualityList(id);
            ViewBag.id = id;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
            return View(ProductQuality);
            //return RedirectToAction("Create");
        }

        public ActionResult ProductTypeIndex()
        {
            var producttype = new ProductTypeService(_unitOfWork).GetFinishiedProductTypeList().ToList();
            return View("ProductTypeIndex", producttype);
        }

        // GET: /ProductMaster/Create

        public ActionResult Create(int id)//ProductType Id
        {
            ProductQuality vm = new ProductQuality();
            vm.IsActive = true;
            vm.ProductTypeId = id;
            ViewBag.id = id;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(vm.ProductTypeId).ProductTypeName;
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductQuality vm)
        {
            ProductQuality pt = vm;

            if (ModelState.IsValid)
            {
                if (vm.ProductQualityId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ProductQualityService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        ViewBag.id = vm.ProductTypeId;
                        ViewBag.Name = new ProductTypeService(_unitOfWork).Find(vm.ProductTypeId).ProductTypeName;
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductQuality).DocumentTypeId,
                        DocId = pt.ProductQualityId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));
                    
                    return RedirectToAction("Create", new { id = vm.ProductTypeId }).Success("Data saved successfully");

                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
                    ProductQuality temp = _ProductQualityService.Find(pt.ProductQualityId);

                    ProductQuality ExRec = Mapper.Map<ProductQuality>(temp);

                    temp.ProductQualityName = pt.ProductQualityName;
                    temp.IsActive = pt.IsActive;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _ProductQualityService.Update(temp);

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
                        ViewBag.id = pt.ProductTypeId;
                        ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId).ProductTypeName;
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductQuality).DocumentTypeId,
                        DocId = temp.ProductQualityId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));
                 
                    return RedirectToAction("Index", new { id = pt.ProductTypeId }).Success("Data saved successfully");
                }

            }
            ViewBag.id = vm.ProductTypeId;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(vm.ProductTypeId).ProductTypeName;
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            ProductQuality pt = _ProductQualityService.Find(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            ViewBag.id = pt.ProductTypeId;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId).ProductTypeName;
            return View("Create", pt);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductQuality ProductQuality = _ProductQualityService.Find(id);

            if (ProductQuality == null)
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

                var temp = _ProductQualityService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                _ProductQualityService.Delete(vm.id);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductQuality).DocumentTypeId,
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
        public ActionResult NextPage(int id, int ptypeid)//CurrentHeaderId
        {
            var nextId = _ProductQualityService.NextId(id, ptypeid);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, int ptypeid)//CurrentHeaderId
        {
            var nextId = _ProductQualityService.PrevId(id, ptypeid);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Quality);

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
