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
using Model.ViewModel;

namespace Jobs.Controllers
{
    
    [Authorize]
    public class ProductConsumptionLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IBomDetailService _BomDetailService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ProductConsumptionLineController(IBomDetailService BomDetail, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
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

        private void PrepareViewBag(ProductConsumptionLineViewModel svm)
        {
        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            int ProductTypeId = (from p in db.Product
                                 where p.ProductId == Id
                                 select new
                                 {
                                     ProductTypeId = p.ProductGroup.ProductTypeId
                                 }).FirstOrDefault().ProductTypeId;

            ProductConsumptionLineViewModel s = new ProductConsumptionLineViewModel();
            s.BaseProductId = Id;

            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(ProductTypeId);
            s.ProductTypeSettings = Mapper.Map<ProductTypeSettings, ProductTypeSettingsViewModel>(settings);

            var MainContens = _BomDetailService.GetConsumptionForIndex(Id);
            var LastMainContentLine = (from L in MainContens
                                       orderby L.BomDetailId descending
                                       select new
                                       {
                                           BomDetailId = L.BomDetailId,
                                           ProductId = L.ProductId
                                       }).FirstOrDefault();
            if (LastMainContentLine != null)
            {
                s.ProductId = LastMainContentLine.ProductId;
            }

            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ProductConsumptionLineViewModel svm)
        {
            if (ModelState.IsValid)
            {        
                if(svm.BomDetailId == 0)
                {
                    BomDetail bomdetail = new BomDetail();

                    bomdetail.BaseProductId = svm.BaseProductId;
                    bomdetail.BatchQty = 1;
                    //bomdetail.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Silai).ProcessId;
                    bomdetail.BaseProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Manufacturing).ProcessId;
                    bomdetail.ProductId = svm.ProductId;
                    bomdetail.ProcessId = svm.ProcessId;
                    bomdetail.Dimension1Id = svm.Dimension1Id;
                    bomdetail.Dimension2Id = svm.Dimension2Id;
                    bomdetail.Dimension3Id = svm.Dimension3Id;
                    bomdetail.Dimension4Id = svm.Dimension4Id;
                    bomdetail.Qty = svm.Qty;
                    bomdetail.MBQ = svm.MBQ;
                    bomdetail.StdCost = svm.StdCost;
                    bomdetail.StdTime = svm.StdTime;

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
                    bomdetail.BaseProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Manufacturing).ProcessId;
                    bomdetail.ProductId = svm.ProductId;
                    bomdetail.ProcessId = svm.ProcessId;
                    bomdetail.Dimension1Id = svm.Dimension1Id;
                    bomdetail.Dimension2Id = svm.Dimension2Id;
                    bomdetail.Dimension3Id = svm.Dimension3Id;
                    bomdetail.Dimension4Id = svm.Dimension4Id;
                    bomdetail.Qty = svm.Qty;
                    bomdetail.MBQ = svm.MBQ;
                    bomdetail.StdCost = svm.StdCost;
                    bomdetail.StdTime = svm.StdTime;


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
            ProductConsumptionLineViewModel s = _BomDetailService.GetProductConsumptionLineForEdit(id);

            PrepareViewBag(s);

            int ProductTypeId = (from p in db.Product
                                 where p.ProductId == s.BaseProductId
                                 select new
                                 {
                                     ProductTypeId = p.ProductGroup.ProductTypeId
                                 }).FirstOrDefault().ProductTypeId;

            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(ProductTypeId);
            s.ProductTypeSettings = Mapper.Map<ProductTypeSettings, ProductTypeSettingsViewModel>(settings);

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

            int ProductTypeId = (from p in db.Product
                                 where p.ProductId == id
                                 select new
                                 {
                                     ProductTypeId = p.ProductGroup.ProductTypeId
                                 }).FirstOrDefault().ProductTypeId;

            BomDetail BomDetail = _BomDetailService.Find(id);
            if (BomDetail == null)
            {
                return HttpNotFound();
            }

            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(ProductTypeId);
            //BomDetail.ProductTypeSettings = Mapper.Map<ProductTypeSettings, ProductTypeSettingsViewModel>(settings);

            return View(BomDetail);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ProductConsumptionLineViewModel vm)
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


        public JsonResult CheckForValidationinEdit(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, int BaseProductId, int BomDetailId)
        {
            var temp = (_BomDetailService.CheckForProductDimensionExists(ProductId, Dimension1Id, Dimension2Id, Dimension3Id, Dimension4Id, ProcessId, BaseProductId, BomDetailId));
            return Json(new { returnvalue = temp });
        }

        public JsonResult CheckForValidation(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, int BaseProductId)
        {
            var temp = (_BomDetailService.CheckForProductDimensionExists(ProductId, Dimension1Id, Dimension2Id, Dimension3Id, Dimension4Id, ProcessId, BaseProductId));
            return Json(new { returnvalue = temp });
        }
    }
}
