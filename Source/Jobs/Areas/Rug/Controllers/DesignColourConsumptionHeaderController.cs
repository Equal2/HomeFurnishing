using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class DesignColourConsumptionHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IBomDetailService _BomDetailService;
        IProductService _ProductService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public DesignColourConsumptionHeaderController(IBomDetailService BomDetailService, IProductService ProductService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _BomDetailService = BomDetailService;
            _ProductService = ProductService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /Order/
        public ActionResult Index()
        {
            var Boms = _BomDetailService.GetDesignColourConsumptionHeaderViewModelForIndex();
            return View(Boms);
        }

        public ActionResult Index_Pending()
        {
            var Boms = _BomDetailService.GetDesignColourConsumptionHeaderViewModelForIndex().Where(i => i.BaseProductId == 0);
            return View("Index",Boms);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.DesignColourConsumption);

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
            return PartialView("CopyFromExisting");
        }


        public ActionResult Create(int? id, int? ColourId, string ProductQualityName, Decimal? Weight)
        {
            DesignColourConsumptionHeaderViewModel p = new DesignColourConsumptionHeaderViewModel();
            p.EntryMode = "Add";
            if (id != null)
            {
                p.ProductGroupId = (int)id;
                p.ColourId = (int)ColourId;
                p.ProductQualityName = ProductQualityName;
                p.Weight = Weight;
            }
            PrepareViewBag();
            return View("Create", p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DesignColourConsumptionHeaderViewModel svm)
        {

            if (ModelState.IsValid)
            {
                if (svm.BaseProductId == 0)
                {
                    ProductQuality Quality = new ProductQualityService(_unitOfWork).Find(svm.ProductQualityName);

                    FinishedProduct product = new FinishedProduct();

                    string ConsumptionProductName = svm.ProductGroupName.ToString().Trim() + "-" + svm.ColourName.ToString().Trim() + "-Bom";

                    product.ProductCode = ConsumptionProductName;
                    product.ProductName = ConsumptionProductName;
                    product.ProductGroupId = new ProductGroupService(_unitOfWork).Find(ProductGroupConstants.Bom).ProductGroupId;
                    product.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    product.IsActive = true;
                    product.ReferenceDocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductGroup).DocumentTypeId;
                    product.ReferenceDocId = svm.ProductGroupId;
                    product.StandardWeight = svm.Weight;
                    product.CreatedDate = DateTime.Now;
                    product.ModifiedDate = DateTime.Now;
                    product.CreatedBy = User.Identity.Name;
                    product.ModifiedBy = User.Identity.Name;
                    product.ObjectState = Model.ObjectState.Added;

                    product.IsSample = false;
                    product.ProductQualityId = Quality.ProductQualityId;
                    _ProductService.Create(product);


                    ProductGroup ProductGroup = new ProductGroupService(_unitOfWork).Find(svm.ProductGroupName);
                    Colour Colour = new ColourService(_unitOfWork).Find(svm.ColourName);
                    if (ProductGroup != null && Colour != null)
                    {
                        var ProductList = (from p in db.FinishedProduct
                                           where p.ProductGroupId == ProductGroup.ProductGroupId && p.ColourId == Colour.ColourId
                                           select new
                                           {
                                               ProductId = p.ProductId
                                           }).ToList();

                        foreach (var item in ProductList)
                        {
                            BomDetail bomdetail = new BomDetail();

                            bomdetail.BaseProductId = item.ProductId;
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

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.DesignColourConsumption).DocumentTypeId,
                        DocId = product.ProductId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    //return RedirectToAction("Create").Success("Data saved successfully");
                    return RedirectToAction("Edit", new { id = product.ProductId }).Success("Data saved Successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    Product product = _ProductService.Find((int)svm.BaseProductId);

                    Product ExRec = Mapper.Map<Product>(product);

                    string ConsumptionProductName = svm.ProductGroupName.ToString().Trim() + "-" + svm.ColourName.ToString().Trim() + "-Bom";

                    product.ProductCode = ConsumptionProductName;
                    product.ProductName = ConsumptionProductName;
                    product.ReferenceDocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductGroup).DocumentTypeId;
                    product.ReferenceDocId = svm.ProductGroupId;
                    product.ModifiedBy = User.Identity.Name;
                    product.ModifiedDate = DateTime.Now;
                    product.StandardWeight = svm.Weight;

                    StringBuilder logstring = new StringBuilder();

                    _ProductService.Update(product);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = product,
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
                        DocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.DesignColourConsumption).DocumentTypeId,
                        DocId = product.ProductId,
                        ActivityType = (int)ActivityTypeContants.Modified,                        
                        xEModifications = Modifications,                        
                    }));

                    return RedirectToAction("Index").Success("Data saved successfully");
                }
            }
            PrepareViewBag();
            return View(svm);
        }


        public ActionResult Edit(int? id)
        {
            DesignColourConsumptionHeaderViewModel bvm = new DesignColourConsumptionHeaderViewModel();
            bvm.EntryMode = "Edit";
            if (id != null && id != 0)
            {
                bvm = _BomDetailService.GetDesignColourConsumptionHeaderViewModel((int)id);
            }

            
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
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {
                Product product = _ProductService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = product,
                });


                IEnumerable<BomDetail> bomdetail = _BomDetailService.GetBomDetailList(vm.id);
                //Mark ObjectState.Delete to all the Bom Detail For Above Bom. 
                foreach (BomDetail item in bomdetail)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });
                    _BomDetailService.Delete(item.BomDetailId);
                }


                IEnumerable<BomDetail> temp = (from L in db.BomDetail
                            where L.ProductId == vm.id
                                               select L).ToList();


                foreach (var item in temp)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });

                    _BomDetailService.Delete(item.BomDetailId);
                }

                // Now delete the Parent Bom
                _ProductService.Delete(product);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.DesignColourConsumption).DocumentTypeId,
                    DocId = product.ProductId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,                  
                    xEModifications = Modifications,
                }));

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

        public JsonResult GetProductGroupDetailJson(int ProductGroupId)
        {
            ProductGroupQuality productgroupquality = new ProductGroupService(_unitOfWork).GetProductGroupQuality(ProductGroupId);
            List<ProductGroupQuality> ProductGroupQualityJson = new List<ProductGroupQuality>();

            ProductGroupQualityJson.Add(new ProductGroupQuality()
            {
                ProductGroupName = productgroupquality.ProductGroupName,
                ProductQualityName = productgroupquality.ProductQualityName,
                GrossWeight = productgroupquality.GrossWeight
            });

            return Json(ProductGroupQualityJson);
        }




        




        [HttpPost]
        public ActionResult CopyFromExisting(CopyFromExistingDesignColourConsumptionViewModel vm)
        {

            if (ModelState.IsValid)
            {
                Product OldProduct = _ProductService.Find(vm.ProductId);

                FinishedProduct NewProduct = new FinishedProduct();
                ProductGroup productgroup = new ProductGroupService(_unitOfWork).Find(vm.ProductGroupId);
                Colour Colour = new ColourService(_unitOfWork).Find(vm.ColourId);


                string ConsumptionProductName = productgroup.ProductGroupName.ToString().Trim() + "-" + Colour.ColourName.ToString().Trim() + "-Bom";

                var FirstProductQuality = (from p in db.FinishedProduct
                            where p.ProductGroupId == productgroup.ProductGroupId && p.ColourId == Colour.ColourId
                            select new
                            {
                                ProductQualityId = p.ProductQualityId
                            }).FirstOrDefault();


                NewProduct.ProductCode = ConsumptionProductName;
                NewProduct.ProductName = ConsumptionProductName;
                NewProduct.ProductGroupId = new ProductGroupService(_unitOfWork).Find(ProductGroupConstants.Bom).ProductGroupId;
                NewProduct.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                NewProduct.IsActive = true;
                NewProduct.CreatedDate = DateTime.Now;
                NewProduct.ModifiedDate = DateTime.Now;
                NewProduct.CreatedBy = User.Identity.Name;
                NewProduct.ModifiedBy = User.Identity.Name;
                NewProduct.ReferenceDocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductGroup).DocumentTypeId;
                NewProduct.ReferenceDocId = productgroup.ProductGroupId;
                NewProduct.IsSample = false;
                if (FirstProductQuality != null)
                {
                    NewProduct.ProductQualityId = FirstProductQuality.ProductQualityId;
                }
                NewProduct.ObjectState = Model.ObjectState.Added;
                _ProductService.Create(NewProduct);




                var ProductList = (from p in db.FinishedProduct
                                    where p.ProductGroupId == productgroup.ProductGroupId && p.ColourId == Colour.ColourId
                                    select new
                                    {
                                        ProductId = p.ProductId
                                    }).ToList();

                foreach (var item in ProductList)
                {
                    BomDetail bomdetail = new BomDetail();

                    bomdetail.BaseProductId = item.ProductId;
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


                IEnumerable<BomDetail> BomDetailList = new BomDetailService(_unitOfWork).GetBomDetailList(OldProduct.ProductId);

                foreach (BomDetail item in BomDetailList)
                {
                    BomDetail bomdetail = new BomDetail();
                    bomdetail.BaseProductId = NewProduct.ProductId;
                    bomdetail.BatchQty = item.BatchQty;
                    bomdetail.ConsumptionPer = item.ConsumptionPer;
                    bomdetail.Dimension1Id = item.Dimension1Id;
                    bomdetail.ProcessId = item.ProcessId;
                    bomdetail.ProductId = item.ProductId;
                    bomdetail.Qty = item.Qty;
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
                    return PartialView("CopyFromExisting", vm);

                }

                return Json(new { success = true, Url = "/DesignColourConsumptionHeader/Edit/" + NewProduct.ProductId });


            }

            return PartialView("CopyFromExisting", vm);

        }

        public JsonResult GetBaseProductId(int ProductGroupId, int ColourId)
        {
            ProductGroup ProductGroup = new ProductGroupService(_unitOfWork).Find(ProductGroupId);
            Colour Colour = new ColourService(_unitOfWork).Find(ColourId);

            if (ProductGroup != null && Colour != null)
            {
                string ConsumptionProductName = ProductGroup.ProductGroupName.ToString().Trim() + "-" + Colour.ColourName.ToString().Trim() + "-Bom";

                var Product = new ProductService(_unitOfWork).Find(ConsumptionProductName);

                if (Product != null)
                {
                    return Json(Product.ProductId);
                }
                else
                {
                    return Json(0);
                }
            }
            else
            {
                return Json(0);
            }
            
        }
    }
}
