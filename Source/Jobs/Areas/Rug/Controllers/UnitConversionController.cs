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
    public class UnitConversionController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IUnitConversionService _UnitConversionService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public UnitConversionController(IUnitConversionService UnitConversionService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _UnitConversionService = UnitConversionService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult ProductIndex(int id)//ProductId
        {
            var productInfo = new FinishedProductService(_unitOfWork).GetFinishedProduct(id);

            return RedirectToAction("Edit", "CarpetMaster", new { id = productInfo.ProductGroupId, sample = productInfo.IsSample });

        }

        public void PrepareViewBag(int id)//ProductId
        {
            Product temp = new ProductService(_unitOfWork).Find(id);
            ViewBag.Name = temp.ProductName;
            ViewBag.Id = id;

            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();
            ViewBag.UnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        // GET: /UnitConversionMaster/

        public ActionResult Index(int ID)//ProductId
        {
            PrepareViewBag(ID);
            var UnitConversion = _UnitConversionService.GetProductUnitConversions(ID).ToList();
            return View(UnitConversion);
        }

        // GET: /UnitConversionMaster/Create

        public ActionResult Create(int ProductId)
        {
            UnitConversionViewModel vm = new UnitConversionViewModel();
            vm.ProductId = ProductId;
            Product Temp = new ProductService(_unitOfWork).Find(ProductId);
            vm.FromUnitId = Temp.UnitId;
            vm.FromQty = 1;
            vm.ToUnitId = Temp.UnitId;
            vm.ToQty = 1;
            PrepareViewBag(ProductId);
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(UnitConversionViewModel vm)
        {
            UnitConversion pt = AutoMapper.Mapper.Map<UnitConversionViewModel, UnitConversion>(vm);
            if (ModelState.IsValid)
            {

                if (vm.UnitConversionId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _UnitConversionService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(pt.ProductId ?? 0);
                        return View("Create", vm);

                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.UnitConversion).DocumentTypeId,
                        DocId = pt.UnitConversionId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("Create", new { ProductId = pt.ProductId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    UnitConversion temp = _UnitConversionService.Find(pt.UnitConversionId);
                    UnitConversion ExRec = new UnitConversion();
                    ExRec = Mapper.Map<UnitConversion>(temp);


                    temp.FromQty = pt.FromQty;
                    temp.FromUnitId = pt.FromUnitId;
                    temp.ToQty = pt.ToQty;
                    temp.ToUnitId = pt.ToUnitId;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.Description = pt.Description;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _UnitConversionService.Update(temp);
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
                        PrepareViewBag(pt.ProductId ?? 0);
                        return View("Create", pt);

                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.UnitConversion).DocumentTypeId,
                        DocId = temp.UnitConversionId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", new { id = vm.ProductId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag(pt.ProductId ?? 0);
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)//UnitConversionId
        {
            UnitConversion pt = _UnitConversionService.Find(id);
            UnitConversionViewModel vm = AutoMapper.Mapper.Map<UnitConversion, UnitConversionViewModel>(pt);
            if (pt == null)
            {
                return HttpNotFound();
            }

            PrepareViewBag(pt.ProductId ?? 0);
            return View("Create", vm);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)//UnitConversion Id
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnitConversion UnitConversion = db.UnitConversion.Find(id);
            if (UnitConversion == null)
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
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                var temp = _UnitConversionService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                _UnitConversionService.Delete(vm.id);
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

                //Logging Activity
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.UnitConversion).DocumentTypeId,
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
        public ActionResult NextPage(int id, int ProductId)//CurrentHeaderId
        {
            var nextId = _UnitConversionService.NextId(id, ProductId);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, int ProductId)//CurrentHeaderId
        {
            var nextId = _UnitConversionService.PrevId(id, ProductId);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.UnitConversion);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        [HttpGet]
        public ActionResult Export()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
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
