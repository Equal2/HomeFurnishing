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
using Core.Common;
using System.Net.Http;
using System.IO;
using System.Web.UI.WebControls;
using System.Web.UI;
using Presentation.ViewModels;
using Model.ViewModels;
using AutoMapper;
using System.Configuration;
using Jobs.Helpers;
using Presentation;
using System.Text;
using System.Web.Script.Serialization;
using System.Data.Entity.Validation;

namespace Jobs.Areas.Rug.Controllers
{
    
    [Authorize]
    public class FinishedProductConsumptionLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IBomDetailService _BomDetailService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public FinishedProductConsumptionLineController(IBomDetailService BomDetail, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _BomDetailService = BomDetail;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _BomDetailService.GetFinishedProductConsumptionForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        private void PrepareViewBag(FinishedProductConsumptionLineViewModel svm)
        {
        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            FinishedProductConsumptionLineViewModel s = new FinishedProductConsumptionLineViewModel();
            s.BaseProductId = Id;
            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(FinishedProductConsumptionLineViewModel svm)
        {
            if (ModelState.IsValid)
            {        
                if(svm.BomDetailId == 0)
                {
                    BomDetail bomdetail = new BomDetail();

                    bomdetail.BaseProductId = svm.BaseProductId;
                    bomdetail.BatchQty = 1;
                    bomdetail.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Silai).ProcessId;
                    bomdetail.ProductId = svm.ProductId;
                    bomdetail.Qty = svm.Qty;

                    bomdetail.CreatedDate = DateTime.Now;
                    bomdetail.ModifiedDate = DateTime.Now;
                    bomdetail.CreatedBy = User.Identity.Name;
                    bomdetail.ModifiedBy = User.Identity.Name;
                    bomdetail.ObjectState = Model.ObjectState.Added;
                    _BomDetailService.Create(bomdetail);


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
                    return RedirectToAction("_Create", new { id = svm.BaseProductId });
                }
                else
                {
                    BomDetail bomdetail = _BomDetailService.Find(svm.BomDetailId);
                    StringBuilder logstring = new StringBuilder();

                    bomdetail.BaseProductId = svm.BaseProductId;
                    bomdetail.BatchQty = 1;
                    bomdetail.ProductId = svm.ProductId;
                    bomdetail.Qty = svm.Qty;


                    bomdetail.ModifiedDate = DateTime.Now;
                    bomdetail.ModifiedBy = User.Identity.Name;
                    bomdetail.ObjectState = Model.ObjectState.Modified;
                    _BomDetailService.Update(bomdetail);


                    //Saving the Activity Log
                        ActivityLog al = new ActivityLog()
                        {
                            ActivityType = (int)ActivityTypeContants.Modified,
                            DocId = bomdetail.BomDetailId,
                            CreatedDate = DateTime.Now,
                            Narration = logstring.ToString(),
                            CreatedBy = User.Identity.Name,
                            //DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.BomDetail).DocumentTypeId,
                        };
                        new ActivityLogService(_unitOfWork).Create(al);
                    //End of Saving the Activity Log


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
                    return Json(new { success = true });
                }
            }

            PrepareViewBag(svm);
            return PartialView("_Create",svm);
        }

        
        public ActionResult _Edit(int id)
        {
            FinishedProductConsumptionLineViewModel s = _BomDetailService.GetFinishedProductConsumptionLineForEdit(id);

            PrepareViewBag(s);

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
        public ActionResult DeletePost(FinishedProductConsumptionLineViewModel vm)
        {
            BomDetail BomDetail = _BomDetailService.Find(vm.BomDetailId);
            _BomDetailService.Delete(vm.BomDetailId);

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


        public JsonResult CheckForValidationinEdit(int ProductId, int BaseProductId, int BomDetailId)
        {
            var temp = (_BomDetailService.CheckForProductExists(ProductId, BaseProductId, BomDetailId));
            return Json(new { returnvalue = temp });
        }

        public JsonResult CheckForValidation(int ProductId, int BaseProductId)
        {
            var temp = (_BomDetailService.CheckForProductExists(ProductId, BaseProductId));
            return Json(new { returnvalue = temp });
        }
    }
}
