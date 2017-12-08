using System;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModel;
using System.Collections.Generic;
using AutoMapper;
using System.Xml.Linq;

namespace Jobs.Areas.Rug.Controllers
{

    [Authorize]
    public class ProductContentLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductContentLineService _ProductContentLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ProductContentLineController(IProductContentLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductContentLineService = SaleOrder;
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
            var p = _ProductContentLineService.GetProductContentLineListForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            ProductContentHeader H = new ProductContentHeaderService(_unitOfWork).Find(Id);
            ProductContentLine s = new ProductContentLine();
            s.ProductContentHeaderId = H.ProductContentHeaderId;
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ProductContentLine svm)
        {

            if (svm.ProductGroupId <= 0)
            {
                ModelState.AddModelError("ProductGroupId", "Please select ProductGroup");
            }

            if (ModelState.IsValid)
            {
                if (svm.ProductContentLineId == 0)
                {
                    svm.CreatedDate = DateTime.Now;
                    svm.ModifiedDate = DateTime.Now;
                    svm.CreatedBy = User.Identity.Name;
                    svm.ModifiedBy = User.Identity.Name;
                    svm.ObjectState = Model.ObjectState.Added;
                    _ProductContentLineService.Create(svm);


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
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductContent).DocumentTypeId,
                        DocId = svm.ProductContentHeaderId,
                        DocLineId=svm.ProductContentLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("_Create", new { id = svm.ProductContentHeaderId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductContentLine temp1 = _ProductContentLineService.Find(svm.ProductContentLineId);

                    ProductContentLine ExRec = Mapper.Map<ProductContentLine>(temp1);
                    //End of Tracking the Modifications::


                    temp1.ProductGroupId = svm.ProductGroupId;
                    temp1.ContentPer = svm.ContentPer;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    _ProductContentLineService.Update(temp1);

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
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductContent).DocumentTypeId,
                        DocId = temp1.ProductContentHeaderId,
                        DocLineId=temp1.ProductContentLineId,
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
            ProductContentLine temp = _ProductContentLineService.GetProductContentLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", temp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ProductContentLine vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            ProductContentLine ProductContentLine = _ProductContentLineService.GetProductContentLine(vm.ProductContentLineId);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = ProductContentLine,
            });

            _ProductContentLineService.Delete(vm.ProductContentLineId);
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
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductContent).DocumentTypeId,
                DocId = vm.ProductContentHeaderId,
                DocLineId=vm.ProductContentLineId,
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
