using System;
using System.Collections.Generic;
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
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class ProductCollectionController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductCollectionService _ProductCollectionService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductCollectionController(IProductCollectionService ProductCollectionService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductCollectionService = ProductCollectionService;
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
            var ProductCollection = _ProductCollectionService.GetProductCollectionList(id).ToList();
            ViewBag.id = id;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
            return View(ProductCollection);
        }

        public ActionResult ProductTypeIndex()
        {
            var producttype = new ProductTypeService(_unitOfWork).GetFinishiedProductTypeList().ToList();
            return View("ProductTypeIndex", producttype);
        }

        // GET: /ProductMaster/Create

        public ActionResult Create(int id)
        {
            ProductCollection vm = new ProductCollection();
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
        public ActionResult Post(ProductCollection vm)
        {
            ProductCollection pt = vm;

            if (ModelState.IsValid)
            {

                if (vm.ProductCollectionId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ProductCollectionService.Create(pt);

                    int RefDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ProductCollection).DocumentTypeId;

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
                        DocTypeId = RefDocTypeId,
                        DocId = pt.ProductCollectionId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    ViewBag.id = vm.ProductTypeId;
                    ViewBag.Name = new ProductTypeService(_unitOfWork).Find(vm.ProductTypeId).ProductTypeName;
                    return RedirectToAction("Edit", new { id = pt.ProductCollectionId }).Success("Data saved successfully");
                }

                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
                    ProductCollection temp = _ProductCollectionService.Find(pt.ProductCollectionId);
                    int RefDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ProductCollection).DocumentTypeId;

                    ProductCollection ExRec = Mapper.Map<ProductCollection>(temp);

                    temp.ProductCollectionName = pt.ProductCollectionName;
                    temp.IsActive = pt.IsActive;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _ProductCollectionService.Update(temp);

                    var ProcSeqHeader = (from p in db.ProcessSequenceHeader
                                         where p.ReferenceDocId == temp.ProductCollectionId && p.ReferenceDocTypeId == RefDocTypeId
                                         select p).FirstOrDefault();

                    if (ProcSeqHeader != null)
                    {
                        ProcSeqHeader.ProcessSequenceHeaderName = temp.ProductCollectionName + "-" + new DivisionService(_unitOfWork).Find((int)HttpContext.Session["DivisionId"]).DivisionName;
                    }


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
                        DocTypeId = RefDocTypeId,
                        DocId = temp.ProductCollectionId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    ViewBag.id = pt.ProductTypeId;
                    ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId).ProductTypeName;
                    return RedirectToAction("Index", new { id = vm.ProductTypeId }).Success("Data saved successfully");

                }

            }
            ViewBag.id = vm.ProductTypeId;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(vm.ProductTypeId).ProductTypeName;
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            ProductCollection pt = _ProductCollectionService.GetProductCollection(id);
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
            ProductCollection ProductCollection = _ProductCollectionService.GetProductCollection(id);

            if (ProductCollection == null)
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
                var temp = _ProductCollectionService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                int RefDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ProductCollection).DocumentTypeId;

                var ProcSeqHeader = (from p in db.ProcessSequenceHeader
                                     where p.ReferenceDocId == temp.ProductCollectionId && p.ReferenceDocTypeId == RefDocTypeId
                                     select p).FirstOrDefault();

                if (ProcSeqHeader != null)
                {
                    var ProcSeqLines = (from p in db.ProcessSequenceLine
                                        where p.ProcessSequenceHeaderId == p.ProcessSequenceHeaderId
                                        select p).ToList();

                    foreach (var item in ProcSeqLines)
                        new ProcessSequenceLineService(_unitOfWork).Delete(item);

                    new ProcessSequenceHeaderService(_unitOfWork).Delete(ProcSeqHeader);
                }

                _ProductCollectionService.Delete(vm.id);

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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductCollection).DocumentTypeId,
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
            var nextId = _ProductCollectionService.NextId(id, ptypeid);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, int ptypeid)//CurrentHeaderId
        {
            var nextId = _ProductCollectionService.PrevId(id, ptypeid);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Collection);

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
