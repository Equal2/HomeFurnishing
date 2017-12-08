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
    public class ProductProcessController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IProductProcessService _ProductProcessService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ProductProcessController(IProductProcessService ProductProcess, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductProcessService = ProductProcess;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _ProductProcessService.GetProductProcessForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        private void PrepareViewBag(ProductProcessViewModel svm)
        {
        }

        public ActionResult _Create(int Id) //Id ==>Product Id
        {
            int ProductTypeId = (from p in db.Product
                                 where p.ProductId == Id
                                 select new
                                 {
                                     ProductTypeId = p.ProductGroup.ProductTypeId
                                 }).FirstOrDefault().ProductTypeId;

            ProductProcessViewModel s = new ProductProcessViewModel();
            s.ProductId = Id;
            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ProductProcessViewModel svm)
        {
            if (ModelState.IsValid)
            {        
                if(svm.ProductProcessId == 0)
                {
                    ProductProcess ProductProcess = new ProductProcess();

                    ProductProcess.ProductId = svm.ProductId;
                    ProductProcess.ProcessId = svm.ProcessId;
                    ProductProcess.Dimension1Id = svm.Dimension1Id;
                    ProductProcess.Dimension2Id = svm.Dimension2Id;
                    ProductProcess.Dimension3Id = svm.Dimension3Id;
                    ProductProcess.Dimension4Id = svm.Dimension4Id;
                    ProductProcess.QAGroupId = svm.QAGroupId;
                    ProductProcess.ProductRateGroupId = svm.ProductRateGroupId;
                    ProductProcess.Instructions = svm.Instructions;
                    
                    ProductProcess.CreatedDate = DateTime.Now;
                    ProductProcess.ModifiedDate = DateTime.Now;
                    ProductProcess.CreatedBy = User.Identity.Name;
                    ProductProcess.ModifiedBy = User.Identity.Name;
                    ProductProcess.ObjectState = Model.ObjectState.Added;
                    _ProductProcessService.Create(ProductProcess);


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
                    return RedirectToAction("_Create", new { id = svm.ProductId });
                }
                else
                {
                    ProductProcess ProductProcess = _ProductProcessService.Find(svm.ProductProcessId);
                    StringBuilder logstring = new StringBuilder();

                    ProductProcess.ProductId = svm.ProductId;
                    ProductProcess.ProductId = svm.ProductId;
                    ProductProcess.ProcessId = svm.ProcessId;
                    ProductProcess.Dimension1Id = svm.Dimension1Id;
                    ProductProcess.Dimension2Id = svm.Dimension2Id;
                    ProductProcess.Dimension3Id = svm.Dimension3Id;
                    ProductProcess.Dimension4Id = svm.Dimension4Id;
                    ProductProcess.QAGroupId = svm.QAGroupId;
                    ProductProcess.ProductRateGroupId = svm.ProductRateGroupId;
                    ProductProcess.Instructions = svm.Instructions;


                    ProductProcess.ModifiedDate = DateTime.Now;
                    ProductProcess.ModifiedBy = User.Identity.Name;
                    ProductProcess.ObjectState = Model.ObjectState.Modified;
                    _ProductProcessService.Update(ProductProcess);


                    //Saving the Activity Log
                        ActivityLog al = new ActivityLog()
                        {
                            ActivityType = (int)ActivityTypeContants.Modified,
                            DocId = ProductProcess.ProductProcessId,
                            CreatedDate = DateTime.Now,
                            Narration = logstring.ToString(),
                            CreatedBy = User.Identity.Name,
                            //DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.ProductProcess).DocumentTypeId,
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
            var ProdProcess = new ProductProcessService(_unitOfWork).Find(id);
            if (ProdProcess.ProcessId != null)
            {
                var Process = new ProcessService(_unitOfWork).Find((int)ProdProcess.ProcessId);
                ViewBag.ProcessName = Process.ProcessName;
            }
            else
            {
                ViewBag.ProcessName = "";
            }



            ProductProcessViewModel vm = new ProductProcessViewModel();
            vm.Instructions = ProdProcess.Instructions;
            vm.ProcessId = ProdProcess.ProcessId;
            vm.ProductId = ProdProcess.ProductId;
            vm.ProductProcessId = ProdProcess.ProductProcessId;
            vm.ProductRateGroupId = ProdProcess.ProductRateGroupId;
            vm.QAGroupId = ProdProcess.QAGroupId;
            vm.Sr = ProdProcess.Sr;

            return PartialView("_Create", vm);
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

            ProductProcess ProductProcess = _ProductProcessService.Find(id);
            if (ProductProcess == null)
            {
                return HttpNotFound();
            }

            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(ProductTypeId);
            //ProductProcess.ProductTypeSettings = Mapper.Map<ProductTypeSettings, ProductTypeSettingsViewModel>(settings);

            return View(ProductProcess);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ProductProcessViewModel vm)
        {
            ProductProcess ProductProcess = _ProductProcessService.Find(vm.ProductProcessId);
            _ProductProcessService.Delete(vm.ProductProcessId);

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


        public JsonResult CheckForValidationinEdit(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, int ProductProcessId)
        {
            var temp = (_ProductProcessService.CheckForProductDimensionExists(ProductId, Dimension1Id, Dimension2Id, Dimension3Id, Dimension4Id, ProcessId, ProductProcessId));
            return Json(new { returnvalue = temp });
        }

        public JsonResult CheckForValidation(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId)
        {
            var temp = (_ProductProcessService.CheckForProductDimensionExists(ProductId, Dimension1Id, Dimension2Id, Dimension3Id, Dimension4Id, ProcessId));
            return Json(new { returnvalue = temp });
        }
    }
}
