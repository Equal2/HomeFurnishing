using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Model.ViewModel;
using System.Xml.Linq;

namespace Jobs.Areas.Rug.Controllers
{

    [Authorize]
    public class ProductConsumptionLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IBomDetailService _BomDetailService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ProductConsumptionLineController(IBomDetailService BomDetail, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _BomDetailService = BomDetail;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public JsonResult IndexForFaceContent(int id)
        {
            var p = _BomDetailService.GetDesignConsumptionFaceContentForIndexForProduct(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult IndexForOtherContent(int id)
        {
            var p = _BomDetailService.GetDesignConsumptionOtherContentForIndexForProduct(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }


        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            ProductConsumptionLineViewModel temp = _BomDetailService.GetBaseProductDetailForProduct(Id);
            return PartialView("_Create", temp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ProductConsumptionLineViewModel svm)
        {
            if (ModelState.IsValid)
            {
                if (svm.BomDetailId == 0)
                {
                    BomDetail bomdetail = new BomDetail();

                    bomdetail.BaseProductId = svm.BaseProductId;
                    bomdetail.BatchQty = 1;
                    bomdetail.ConsumptionPer = svm.ConsumptionPer;
                    bomdetail.Dimension1Id = svm.Dimension1Id;
                    bomdetail.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;
                    bomdetail.ProductId = svm.ProductId;
                    bomdetail.Qty = svm.Qty;

                    bomdetail.CreatedDate = DateTime.Now;
                    bomdetail.ModifiedDate = DateTime.Now;
                    bomdetail.CreatedBy = User.Identity.Name;
                    bomdetail.ModifiedBy = User.Identity.Name;
                    bomdetail.ObjectState = Model.ObjectState.Added;
                    _BomDetailService.Create(bomdetail);


                    if (bomdetail.BaseProductId == bomdetail.ProductId)
                    {
                        //return View(svm).Danger(DataValidationMsg);
                        ModelState.AddModelError("", "Invalid Product is Selected!");
                        return PartialView("_Create", svm);
                    }

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
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductConsumption).DocumentTypeId,
                        DocId = bomdetail.BomDetailId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("_Create", new { id = svm.BaseProductId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
                    BomDetail bomdetail = _BomDetailService.Find(svm.BomDetailId);

                    BomDetail ExRec = Mapper.Map<BomDetail>(bomdetail);

                    bomdetail.BaseProductId = svm.BaseProductId;
                    bomdetail.BatchQty = 1;
                    bomdetail.ConsumptionPer = svm.ConsumptionPer;
                    bomdetail.Dimension1Id = svm.Dimension1Id;
                    bomdetail.ProductId = svm.ProductId;
                    bomdetail.Qty = svm.Qty;


                    bomdetail.ModifiedDate = DateTime.Now;
                    bomdetail.ModifiedBy = User.Identity.Name;
                    bomdetail.ObjectState = Model.ObjectState.Modified;
                    _BomDetailService.Update(bomdetail);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = bomdetail,
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
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductConsumption).DocumentTypeId,
                        DocId = bomdetail.BomDetailId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return Json(new { success = true });
                }
            }
            return PartialView("_Create", svm);
        }


        public ActionResult _Edit(int id)
        {
            ProductConsumptionLineViewModel s = _BomDetailService.GetDesignConsumptionLineForEditForProduct(id);
            ProductConsumptionLineViewModel temp = _BomDetailService.GetBaseProductDetailForProduct(s.BaseProductId);
            s.ProductName = temp.ProductName;
            s.QualityName = temp.QualityName;
            s.Weight = temp.Weight;

            if (s == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", s);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BomDetail BomDetail = _BomDetailService.Find(id);
            if (BomDetail == null)
            {
                return HttpNotFound();
            }
            return View(BomDetail);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ProductConsumptionLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            BomDetail BomDetail = _BomDetailService.Find(vm.BomDetailId);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = BomDetail,
            });

            _BomDetailService.Delete(vm.BomDetailId);
            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("EditSize", vm);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductConsumption).DocumentTypeId,
                DocId = vm.BomDetailId,
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

        public JsonResult GetProductDetailJson(int ProductId)
        {
            ProductWithGroupAndUnit productgroupandunit = _BomDetailService.GetProductGroupAndUnit(ProductId);
            List<ProductWithGroupAndUnit> productgroupandunitJson = new List<ProductWithGroupAndUnit>();

            productgroupandunitJson.Add(new ProductWithGroupAndUnit()
            {
                ProductGroupId = productgroupandunit.ProductGroupId,
                ProductGroupName = productgroupandunit.ProductGroupName,
                UnitName = productgroupandunit.UnitName
            });

            return Json(productgroupandunitJson);
        }


        public JsonResult CheckForValidationinEdit(int ProductId, int? Dimension1Id, int BaseProductId, int BomDetailId, int BaseProcessId)
        {
            var temp = (_BomDetailService.CheckForProductShadeExists(ProductId, Dimension1Id, BaseProductId, BomDetailId, BaseProcessId));
            return Json(new { returnvalue = temp });
        }

        public JsonResult CheckForValidation(int ProductId, int? Dimension1Id, int BaseProductId, int BaseProcessId)
        {
            var temp = (_BomDetailService.CheckForProductShadeExists(ProductId, Dimension1Id, BaseProductId, BaseProcessId));
            return Json(new { returnvalue = temp });
        }
    }
}
