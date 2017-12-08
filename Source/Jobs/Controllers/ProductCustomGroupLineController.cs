using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using AutoMapper;
using Model.ViewModel;
using System.Xml.Linq;

namespace Jobs.Controllers
{

    [Authorize]
    public class ProductCustomGroupLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductCustomGroupLineService _ProductCustomGroupLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ProductCustomGroupLineController(IProductCustomGroupLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductCustomGroupLineService = SaleOrder;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _ProductCustomGroupLineService.GetProductCustomGroupLineListForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            ProductCustomGroupHeader H = new ProductCustomGroupHeaderService(_unitOfWork).Find(Id);
            ProductCustomGroupLine s = new ProductCustomGroupLine();
            s.ProductCustomGroupHeaderId = H.ProductCustomGroupId;
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ProductCustomGroupLine svm)
        {

            if (svm.Qty <= 0)
            {
                ModelState.AddModelError("Qty", "Please Check Qty");
            }

            if (ModelState.IsValid)
            {
                if (svm.ProductCustomGroupLineId == 0)
                {
                    svm.CreatedDate = DateTime.Now;
                    svm.ModifiedDate = DateTime.Now;
                    svm.CreatedBy = User.Identity.Name;
                    svm.ModifiedBy = User.Identity.Name;
                    svm.ObjectState = Model.ObjectState.Added;
                    _ProductCustomGroupLineService.Create(svm);

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

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductCustomGroup).DocumentTypeId,
                        DocId=svm.ProductCustomGroupHeaderId,
                        DocLineId = svm.ProductCustomGroupLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("_Create", new { id = svm.ProductCustomGroupHeaderId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductCustomGroupLine temp1 = _ProductCustomGroupLineService.Find(svm.ProductCustomGroupLineId);

                    ProductCustomGroupLine ExRec = Mapper.Map<ProductCustomGroupLine>(temp1);

                    temp1.ProductId = svm.ProductId;
                    temp1.Qty = svm.Qty;

                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    _ProductCustomGroupLineService.Update(temp1);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
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
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductCustomGroup).DocumentTypeId,
                        DocId=temp1.ProductCustomGroupHeaderId,
                        DocLineId = temp1.ProductCustomGroupLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return Json(new { success = true });

                }
            }
            return PartialView("_Create", svm);
        }

        [HttpGet]
        public ActionResult _Edit(int id)
        {
            ProductCustomGroupLine temp = _ProductCustomGroupLineService.GetProductCustomGroupLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", temp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ProductCustomGroupLine vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            ProductCustomGroupLine ProductCustomGroupLine = _ProductCustomGroupLineService.GetProductCustomGroupLine(vm.ProductCustomGroupLineId);
            LogList.Add(new LogTypeViewModel
            {
                ExObj = ProductCustomGroupLine,
            });

            _ProductCustomGroupLineService.Delete(vm.ProductCustomGroupLineId);
            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("EditLine", vm);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductCustomGroup).DocumentTypeId,
                DocId=vm.ProductCustomGroupHeaderId,
                DocLineId = vm.ProductCustomGroupLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,                
                xEModifications = Modifications,
            }));          

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
