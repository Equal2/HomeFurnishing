using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using AutoMapper;
using Presentation;
using Model.ViewModel;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductCustomGroupHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext context = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductCustomGroupHeaderService _ProductCustomGroupHeaderService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductCustomGroupHeaderController(IProductCustomGroupHeaderService PurchaseOrderHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductCustomGroupHeaderService = PurchaseOrderHeaderService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /ProductCustomGroupHeader/

        public ActionResult Index()
        {
            IQueryable<ProductCustomGroupHeader> p = _ProductCustomGroupHeaderService.GetProductCustomGroupHeaderList();
            return View(p);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductCustomGroup);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        [HttpGet]
        public ActionResult Remove()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }


        // GET: /ProductCustomGroupHeader/Create

        public ActionResult Create()
        {
            ProductCustomGroupHeader vm = new ProductCustomGroupHeader();
            return View("Create", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HeaderPost(ProductCustomGroupHeader svm)
        {
            if (ModelState.IsValid)
            {
                if (svm.ProductCustomGroupId == 0)
                {

                    svm.CreatedDate = DateTime.Now;
                    svm.ModifiedDate = DateTime.Now;
                    svm.CreatedBy = User.Identity.Name;
                    svm.ModifiedBy = User.Identity.Name;
                    _ProductCustomGroupHeaderService.Create(svm);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductCustomGroup).DocumentTypeId,
                        DocId = svm.ProductCustomGroupId,
                        ActivityType = (int)ActivityTypeContants.Added,                        
                    }));

                    //return RedirectToAction("Create", "SaleOrderLine", new { Id = s.ProductCustomGroupHeaderId }).Success("Data saved successfully");
                    return RedirectToAction("Edit", new { id = svm.ProductCustomGroupId }).Success("Data saved Successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductCustomGroupHeader temp = _ProductCustomGroupHeaderService.Find(svm.ProductCustomGroupId);

                    ProductCustomGroupHeader ExRec = Mapper.Map<ProductCustomGroupHeader>(temp);

                    temp.ProductCustomGroupName = svm.ProductCustomGroupName;
                    temp.IsActive = svm.IsActive;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    _ProductCustomGroupHeaderService.Update(temp);

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
                        return View("Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductCustomGroup).DocumentTypeId,
                        DocId = temp.ProductCustomGroupId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));    
                    
                    return RedirectToAction("Index").Success("Data saved successfully");
                }

            }
            return View("Create", svm);
        }


        // GET: /ProductCustomGroupHeader/Edit/5
        public ActionResult Edit(int id)
        {
            ProductCustomGroupHeader s = _ProductCustomGroupHeaderService.GetProductCustomGroupHeader(id);
            if (s == null)
            {
                return HttpNotFound();
            }
            return View("Create", s);
        }


        // GET: /PurchaseOrderHeader/Delete/5

        public ActionResult Delete(int id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductCustomGroupHeader ProductCustomGroupHeader = _ProductCustomGroupHeaderService.Find(id);
            if (ProductCustomGroupHeader == null)
            {
                return HttpNotFound();
            }
            ReasonViewModel rvm = new ReasonViewModel()
            {
                id = id,
            };
            return PartialView("_Reason", rvm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {
                //string temp = (Request["Redirect"].ToString());
                //first find the Purchase Order Object based on the ID. (sience this object need to marked to be deleted IE. ObjectState.Deleted)
                var ProductCustomGroupHeader = _ProductCustomGroupHeaderService.GetProductCustomGroupHeader(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ProductCustomGroupHeader,
                });

                //Then find all the Purchase Order Header Line associated with the above ProductType.
                var line = new ProductCustomGroupLineService(_unitOfWork).GetProductCustomGroupLineListForIndex(vm.id);

                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in line)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });

                    new ProductCustomGroupLineService(_unitOfWork).Delete(item.ProductCustomGroupLineId);
                }

                // Now delete the Purhcase Order Header
                _ProductCustomGroupHeaderService.Delete(vm.id);

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                //Commit the DB
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductCustomGroup).DocumentTypeId,
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
        public ActionResult NextPage(int id)//CurrentHeaderId
        {
            var nextId = _ProductCustomGroupHeaderService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _ProductCustomGroupHeaderService.PrevId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
