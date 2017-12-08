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
    public class ProductDesignController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductDesignService _ProductDesignService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductDesignController(IProductDesignService ProductDesignService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductDesignService = ProductDesignService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }



        public ActionResult ColourWaysIndex()
        {

            var pro = _ProductDesignService.GetProductDesignListForColourWays();
            return View("ColourWaysIndex", pro);

        }

        public ActionResult CreateColourWays()
        {
            ProductDesign vm = new ProductDesign();
            vm.ProductTypeId = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;
            vm.IsActive = true;
            return View("ColourWays", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ColourWaysPost(ProductDesign vm)
        {
            ProductDesign pt = vm;

            if (ModelState.IsValid)
            {

                if (vm.ProductDesignId <= 0)
                {

                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ProductDesignService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("ColourWays", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductDesign).DocumentTypeId,
                        DocId = pt.ProductDesignId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));


                    return RedirectToAction("CreateColourWays").Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductDesign temp = _ProductDesignService.Find(pt.ProductDesignId);

                    ProductDesign ExRec = Mapper.Map<ProductDesign>(temp);

                    temp.ProductDesignName = pt.ProductDesignName;
                    temp.IsActive = pt.IsActive;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _ProductDesignService.Update(temp);

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
                        return View("ColourWays", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductDesign).DocumentTypeId,
                        DocId = temp.ProductDesignId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("ColourWaysIndex").Success("Data saved successfully");
                }


            }
            return View("ColourWays", vm);
        }



        public ActionResult EditColourWays(int id)
        {
            ProductDesign pt = _ProductDesignService.Find(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("ColourWays", pt);
        }



        // GET: /ProductMaster/Delete/5

        public ActionResult DeleteColourWays(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductDesign ProductDesign = _ProductDesignService.Find(id);

            if (ProductDesign == null)
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
        public ActionResult DeleteColourWays(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {
                var temp = _ProductDesignService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                _ProductDesignService.Delete(vm.id);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductDesign).DocumentTypeId,
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
        public ActionResult cNextPage(int id)//CurrentHeaderId
        {
            int ptypeid = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;
            var nextId = _ProductDesignService.NextId(id, ptypeid);
            return RedirectToAction("EditColourWays", new { id = nextId });
        }
        [HttpGet]
        public ActionResult cPrevPage(int id)//CurrentHeaderId
        {
            int ptypeid = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;
            var nextId = _ProductDesignService.PrevId(id, ptypeid);
            return RedirectToAction("EditColourWays", new { id = nextId });
        }
















        [Authorize]
        public ActionResult ProductTypeIndex()
        {
            var producttype = _ProductDesignService.GetProductTypeList().ToList();
            return View("ProductTypeIndex", producttype);
        }



        // GET: /ProductMaster/

        public ActionResult Index(int id)
        {
            var ProductDesign = _ProductDesignService.GetProductDesignList(id).ToList();
            ViewBag.id = id;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
            return View(ProductDesign);
            //return RedirectToAction("Create");
        }

        // GET: /ProductMaster/Create

        public ActionResult Create(int id)
        {
            ProductDesign vm = new ProductDesign();
            vm.IsActive = true;
            vm.ProductTypeId = id;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
            ViewBag.id = id;
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductDesign vm)
        {
            ProductDesign pt = vm;

            if (ModelState.IsValid)
            {
                if (vm.ProductDesignId <= 0)
                {

                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ProductDesignService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        ViewBag.id = pt.ProductTypeId;
                        ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId ?? 0).ProductTypeName;
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductDesign).DocumentTypeId,
                        DocId = pt.ProductDesignId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    ViewBag.id = pt.ProductTypeId;
                    ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId ?? 0).ProductTypeName;
                    return RedirectToAction("Create", new { id = pt.ProductTypeId }).Success("Data saved successfully");

                }

                else
                {

                    ProductDesign temp = _ProductDesignService.Find(pt.ProductDesignId);

                    temp.ProductDesignName = pt.ProductDesignName;
                    temp.IsActive = pt.IsActive;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _ProductDesignService.Update(temp);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId ?? 0).ProductTypeName;
                        ViewBag.id = pt.ProductTypeId;
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductDesign).DocumentTypeId,
                        DocId = temp.ProductDesignId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                    }));


                    ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId ?? 0).ProductTypeName;
                    ViewBag.id = pt.ProductTypeId;
                    return RedirectToAction("Index", new { id = pt.ProductTypeId }).Success("Data saved successfully");

                }


            }
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(vm.ProductTypeId ?? 0).ProductTypeName;
            ViewBag.id = pt.ProductTypeId;
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            ProductDesign pt = _ProductDesignService.Find(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProductTypeList = new ProductTypeService(_unitOfWork).GetProductTypeList().ToList();
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId ?? 0).ProductTypeName;
            ViewBag.id = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId ?? 0).ProductTypeId;
            return View("Create", pt);
        }


        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductDesign ProductDesign = _ProductDesignService.Find(id);

            if (ProductDesign == null)
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

            if (ModelState.IsValid)
            {
                var temp = _ProductDesignService.Find(vm.id);

                _ProductDesignService.Delete(vm.id);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductDesign).DocumentTypeId,
                    DocId = vm.id,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }


        [HttpGet]
        public ActionResult NextPage(int id, int ptypeid)//CurrentHeaderId
        {
            var nextId = _ProductDesignService.NextId(id, ptypeid);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, int ptypeid)//CurrentHeaderId
        {
            var nextId = _ProductDesignService.PrevId(id, ptypeid);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Design);

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
