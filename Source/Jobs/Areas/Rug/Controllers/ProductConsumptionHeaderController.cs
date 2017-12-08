using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Model.ViewModels;
using Data.Models;
using Data.Infrastructure;
using Service;
using AutoMapper;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Text;
using System.Configuration;
using System.IO;
using ImageResizer;
using Model.ViewModel;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class ProductConsumptionHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IBomDetailService _BomDetailService;
        IProductService _ProductService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ProductConsumptionHeaderController(IBomDetailService BomDetailService, IProductService ProductService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _BomDetailService = BomDetailService;
            _ProductService = ProductService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }
        // GET: /Order/
        public ActionResult Index()
        {
            var Boms = _BomDetailService.GetDesignConsumptionHeaderViewModelForIndexForProduct();
            return View(Boms);
        }

        [HttpGet]
        public ActionResult NextPage(int id, string name)//BomId
        {
            var nextId = _BomDetailService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, string name)//BomId
        {
            var nextId = _BomDetailService.PrevId(id);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductConsumption);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        [HttpGet]
        public ActionResult Remove()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }




        public void PrepareViewBag()
        {
            //ViewBag.PersonRateGroupList = new PersonRateGroupService(_unitOfWork).GetPersonRateGroupList().ToList();
        }



        public ActionResult ChooseType()
        {
            return PartialView("ChooseType");
        }
        [HttpGet]
        public ActionResult CopyFromExisting()
        {
            CopyFromExistingProductConsumptionViewModel vm = new CopyFromExistingProductConsumptionViewModel();
            if (Session["ProductConsumptionFromPId"] != null)
            {
                vm.FromProductId = (int)Session["ProductConsumptionFromPId"];
            }
            return PartialView("CopyFromExisting",vm);
        }


        public ActionResult Create()
        {
            ProductConsumptionHeaderViewModel p = new ProductConsumptionHeaderViewModel();
            PrepareViewBag();
            return View("Create", p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductConsumptionHeaderViewModel svm)
        {
            Product SProd = new ProductService(_unitOfWork).Find(svm.ProductId);
            if (ModelState.IsValid)
            {
                if (svm.BaseProductId == 0)
                {
                    Product product = new Product();



                    if (SProd.ProductName.Length > 16)
                    {
                        product.ProductCode = SProd.ProductName.ToString().Substring(0, 16) + "-Bom";
                    }
                    else
                    {
                        product.ProductCode = SProd.ProductName.ToString().Substring(0, SProd.ProductName.Length) + "-Bom";
                    }

                    product.ProductName = SProd.ProductName + "-Bom";
                    product.ProductGroupId = new ProductGroupService(_unitOfWork).Find(ProductGroupConstants.Bom).ProductGroupId;
                    product.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    product.IsActive = true;
                    product.ReferenceDocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product).DocumentTypeId;
                    product.ReferenceDocId = svm.ProductId;


                    product.CreatedDate = DateTime.Now;
                    product.ModifiedDate = DateTime.Now;
                    product.CreatedBy = User.Identity.Name;
                    product.ModifiedBy = User.Identity.Name;
                    product.ObjectState = Model.ObjectState.Added;
                    _ProductService.Create(product);




                    var ExistingRec = _BomDetailService.GetExistingBaseProduct(SProd.ProductId);
                    if (ExistingRec != null)
                    {
                        ExistingRec.ProductId = product.ProductId;
                        ExistingRec.CreatedDate = DateTime.Now;
                        ExistingRec.ModifiedBy = User.Identity.Name;
                        _BomDetailService.Update(ExistingRec);
                    }
                    else
                    {
                        BomDetail bomdetail = new BomDetail();
                        bomdetail.BaseProductId = SProd.ProductId;
                        bomdetail.BatchQty = 1;
                        bomdetail.ConsumptionPer = 100;
                        bomdetail.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;
                        bomdetail.ProductId = product.ProductId;
                        bomdetail.Qty = 1;

                        bomdetail.CreatedDate = DateTime.Now;
                        bomdetail.ModifiedDate = DateTime.Now;
                        bomdetail.CreatedBy = User.Identity.Name;
                        bomdetail.ModifiedBy = User.Identity.Name;
                        bomdetail.ObjectState = Model.ObjectState.Added;
                        _BomDetailService.Create(bomdetail);
                    }



                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View(svm);

                    }

                    //return RedirectToAction("Create").Success("Data saved successfully");
                    return RedirectToAction("Edit", new { id = product.ProductId }).Success("Data saved Successfully");
                }
                else
                {
                    Product product = _ProductService.Find(svm.BaseProductId);

                    if (SProd.ProductName.Length > 16)
                    {
                        product.ProductCode = SProd.ProductName.ToString().Substring(0, 16) + "-Bom";
                    }
                    else
                    {
                        product.ProductCode = SProd.ProductName.ToString().Substring(0, SProd.ProductName.Length) + "-Bom";
                    }


                    product.ProductName = SProd.ProductName + "-Bom";
                    product.ReferenceDocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product).DocumentTypeId;
                    product.ReferenceDocId = svm.ProductId;
                    product.ModifiedBy = User.Identity.Name;
                    product.ModifiedDate = DateTime.Now;

                    StringBuilder logstring = new StringBuilder();

                    _ProductService.Update(product);





                    ////Saving Activity Log::
                    ActivityLog al = new ActivityLog()
                    {
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocId = svm.BaseProductId,
                        Narration = logstring.ToString(),
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.Identity.Name,
                        //DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.ProcessSequence).DocumentTypeId,

                    };
                    new ActivityLogService(_unitOfWork).Create(al);
                    //End of Saving ActivityLog

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
                    return RedirectToAction("Index").Success("Data saved successfully");
                }
            }
            PrepareViewBag();
            return View(svm);
        }


        public ActionResult Edit(int id)
        {
            ProductConsumptionHeaderViewModel bvm = _BomDetailService.GetDesignConsumptionHeaderViewModelForProduct(id);
            PrepareViewBag();
            if (bvm == null)
            {
                return HttpNotFound();
            }
            return View("Create", bvm);
        }


        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = _ProductService.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            if (ModelState.IsValid)
            {
                Product product = _ProductService.Find(vm.id);


                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = vm.id,
                    UserRemark = vm.Reason,
                    Narration = "Bom is deleted with Name:" + product.ProductName,
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.SaleOrder).DocumentTypeId,
                    UploadDate = DateTime.Now,

                };
                new ActivityLogService(_unitOfWork).Create(al);


                IEnumerable<BomDetail> bomdetail = _BomDetailService.GetBomDetailList(vm.id);
                //Mark ObjectState.Delete to all the Bom Detail For Above Bom. 
                foreach (BomDetail item in bomdetail)
                {
                    _BomDetailService.Delete(item.BomDetailId);
                }


                var temp = (from L in db.BomDetail
                            where L.ProductId == vm.id
                            select new { BomDetailId = L.BomDetailId }).ToList();


                foreach (var item in temp)
                {
                    _BomDetailService.Delete(item.BomDetailId);
                }

                // Now delete the Parent Bom
                _ProductService.Delete(product);


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
                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
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
            ProductQuality productgroupquality = new ProductQualityService(_unitOfWork).FindProductQuality(ProductId);

            Product Prod = new ProductService(_unitOfWork).Find(ProductId);

            return Json(new { ProductName = Prod.ProductName, ProductQualityName = productgroupquality.ProductQualityName });
        }




        [HttpPost]
        public ActionResult CopyFromExisting(CopyFromExistingProductConsumptionViewModel vm)
        {

            if (ModelState.IsValid)
            {
                Session["ProductConsumptionFromPId"] = vm.FromProductId;
                Product NewProduct = new Product();
                Product prod = new ProductService(_unitOfWork).Find(vm.ToProductId);

                if (prod.ProductName.Length > 16)
                {
                    NewProduct.ProductCode = prod.ProductName.ToString().Substring(0, 16) + "-Bom";
                }
                else
                {
                    NewProduct.ProductCode = prod.ProductName.ToString().Substring(0, prod.ProductName.Length) + "-Bom";
                }

                NewProduct.ProductName = prod.ProductName + "-Bom";
                NewProduct.ProductGroupId = new ProductGroupService(_unitOfWork).Find(ProductGroupConstants.Bom).ProductGroupId;
                NewProduct.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                NewProduct.IsActive = true;
                NewProduct.CreatedDate = DateTime.Now;
                NewProduct.ModifiedDate = DateTime.Now;
                NewProduct.CreatedBy = User.Identity.Name;
                NewProduct.ModifiedBy = User.Identity.Name;
                NewProduct.ReferenceDocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product).DocumentTypeId;
                NewProduct.ReferenceDocId = prod.ProductId;
                NewProduct.ObjectState = Model.ObjectState.Added;
                _ProductService.Create(NewProduct);


                var ExistingRec = _BomDetailService.GetExistingBaseProduct(prod.ProductId);
                if (ExistingRec != null)
                {
                    ExistingRec.ProductId = NewProduct.ProductId;
                    ExistingRec.CreatedDate = DateTime.Now;
                    ExistingRec.ModifiedBy = User.Identity.Name;
                    _BomDetailService.Update(ExistingRec);
                }
                else
                {
                    BomDetail bomdetail = new BomDetail();

                    bomdetail.BaseProductId = prod.ProductId;
                    bomdetail.BatchQty = 1;
                    bomdetail.ConsumptionPer = 100;
                    bomdetail.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;
                    bomdetail.ProductId = NewProduct.ProductId;
                    bomdetail.Qty = 1;

                    bomdetail.CreatedDate = DateTime.Now;
                    bomdetail.ModifiedDate = DateTime.Now;
                    bomdetail.CreatedBy = User.Identity.Name;
                    bomdetail.ModifiedBy = User.Identity.Name;
                    bomdetail.ObjectState = Model.ObjectState.Added;
                    _BomDetailService.Create(bomdetail);
                }

                IEnumerable<BomDetail> bomdetList = new BomDetailService(_unitOfWork).GetBomDetailList(vm.FromProductId);

                foreach (BomDetail item in bomdetList)
                {
                    BomDetail bomdet = new BomDetail();
                    bomdet.BaseProductId = NewProduct.ProductId;
                    bomdet.BatchQty = item.BatchQty;
                    bomdet.ConsumptionPer = item.ConsumptionPer;
                    bomdet.Dimension1Id = item.Dimension1Id;
                    bomdet.ProcessId = item.ProcessId;
                    bomdet.ProductId = item.ProductId;
                    bomdet.Qty = item.Qty;
                    bomdet.CreatedDate = DateTime.Now;
                    bomdet.ModifiedDate = DateTime.Now;
                    bomdet.CreatedBy = User.Identity.Name;
                    bomdet.ModifiedBy = User.Identity.Name;
                    bomdet.ObjectState = Model.ObjectState.Added;
                    _BomDetailService.Create(bomdet);
                }


                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("CopyFromExisting", vm);

                }

                return Json(new { success = true, Url = "/ProductConsumptionHeader/Edit/" + NewProduct.ProductId });


            }

            return PartialView("CopyFromExisting", vm);

        }

    }
}
