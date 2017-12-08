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
using System.Linq;
using Reports.Presentation.Helper;

namespace Jobs.Areas.Rug.Controllers
{

    [Authorize]
    public class DesignConsumptionLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IBomDetailService _BomDetailService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public DesignConsumptionLineController(IBomDetailService BomDetail, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
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
            var p = _BomDetailService.GetDesignConsumptionFaceContentForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult IndexForOtherContent(int id)
        {
            var p = _BomDetailService.GetDesignConsumptionOtherContentForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult IndexForOverTuftContent(int id)
        {
            var p = _BomDetailService.GetDesignConsumptionOverTuftContentForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        private void PrepareViewBag(DesignConsumptionLineViewModel svm)
        {
            var ProductFaceContentGroups = from p in db.Product
                                           join pg in db.ProductGroups on p.ReferenceDocId equals pg.ProductGroupId into ProductGroupTable
                                           from ProductGroupTab in ProductGroupTable.DefaultIfEmpty()
                                           join fp in db.FinishedProduct on ProductGroupTab.ProductGroupId equals fp.ProductGroupId into FinishedProductTable
                                           from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
                                           join pcl in db.ProductContentLine on FinishedProductTab.FaceContentId equals pcl.ProductContentHeaderId into ProductContentLineTable
                                           from ProductContentLineTab in ProductContentLineTable.DefaultIfEmpty()
                                           where p.ProductId == svm.BaseProductId && ((int?)ProductContentLineTab.ProductGroupId ?? 0) != 0
                                           group new { ProductContentLineTab } by new { ProductContentLineTab.ProductGroupId } into Result
                                           select new
                                           {
                                               ProductGroupId = Result.Key.ProductGroupId
                                           };

            var TotalMainContents = (from L in db.BomDetail
                             join P in db.Product on L.ProductId equals P.ProductId into ProductTable
                             from ProductTab in ProductTable.DefaultIfEmpty()
                             join pcon in ProductFaceContentGroups on ProductTab.ProductGroupId equals pcon.ProductGroupId into ProductFaceContentTable
                             from ProductFaceContentTab in ProductFaceContentTable.DefaultIfEmpty()
                             where L.BaseProductId == svm.BaseProductId && ((int?)ProductFaceContentTab.ProductGroupId ?? 0) != 0
                             group new { L } by new { L.BaseProductId } into Result
                             select new
                             {
                                 TotalQty = Result.Sum(i => i.L.Qty)
                             }).FirstOrDefault();

            var TotalOtherContents = (from L in db.BomDetail
                                     join P in db.Product on L.ProductId equals P.ProductId into ProductTable
                                     from ProductTab in ProductTable.DefaultIfEmpty()
                                     join pcon in ProductFaceContentGroups on ProductTab.ProductGroupId equals pcon.ProductGroupId into ProductFaceContentTable
                                     from ProductFaceContentTab in ProductFaceContentTable.DefaultIfEmpty()
                                     where L.BaseProductId == svm.BaseProductId && ((int?)ProductFaceContentTab.ProductGroupId ?? 0) == 0
                                     group new { L } by new { L.BaseProductId } into Result
                                     select new
                                     {
                                         TotalQty = Result.Sum(i => i.L.Qty)
                                     }).FirstOrDefault();

            if (TotalMainContents != null  && svm.Weight!=0)
            {
                Decimal TotalMainContentPercentage = Math.Round(TotalMainContents.TotalQty * 100 / svm.Weight,2);
                ViewBag.LastTransaction = TotalMainContentPercentage + "% Main Contents filled, " + (100 - TotalMainContentPercentage) + " remaining.";
            }

            if (TotalOtherContents != null && svm.Weight != 0)
            { 
                Decimal TotalOtherContentPercentage = Math.Round(TotalOtherContents.TotalQty * 100 / svm.Weight, 2);
                ViewBag.LastTransaction = ViewBag.LastTransaction + (TotalOtherContentPercentage + "% Other Contents filled.").ToString();
            }
        }

        //public ActionResult _Create(int Id) //Id ==>Design Content Header Id
        //{
        //    DesignConsumptionLineViewModel s = new DesignConsumptionLineViewModel();
        //    s.BaseProductId = Id;
        //    s.ContentType = "Contents";
        //    DesignConsumptionLineViewModel temp = _BomDetailService.GetBaseProductDetail(Id);

        //    s.DesignName = temp.DesignName;
        //    s.QualityName = temp.QualityName;
        //    s.Weight = temp.Weight;

        //    PrepareViewBag(s);
        //    return PartialView("_Create", s);
        //}

        public ActionResult _CreateMainContentForBaseProduct(int Id) //Id ==>Design Content Header Id
        {
            DesignConsumptionLineViewModel s = new DesignConsumptionLineViewModel();
            s.BaseProductId = Id;
            s.ContentType = "Main Contents";
            DesignConsumptionLineViewModel temp = _BomDetailService.GetBaseProductDetail(Id);


            var MainContens = _BomDetailService.GetDesignConsumptionFaceContentForIndex(Id);
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
                s.ConsumptionPer = 100 - MainContens.Sum(m => m.ConsumptionPer);
            }


            s.DesignName = temp.DesignName;
            s.DesignId = temp.DesignId;
            s.QualityName = temp.QualityName;
            s.Weight = temp.Weight;
            s.BaseProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;


            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        public ActionResult _CreateOtherContentForBaseProduct(int Id) //Id ==>Design Content Header Id
        {
            DesignConsumptionLineViewModel s = new DesignConsumptionLineViewModel();
            s.BaseProductId = Id;
            s.ContentType = "Other Contents";
            DesignConsumptionLineViewModel temp = _BomDetailService.GetBaseProductDetail(Id);

            s.DesignName = temp.DesignName;
            s.DesignId = temp.DesignId;
            s.QualityName = temp.QualityName;
            s.Weight = temp.Weight;
            s.BaseProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;

            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        public ActionResult _CreateOverTuftContentForBaseProduct(int Id) //Id ==>Design Content Header Id
        {
            DesignConsumptionLineViewModel s = new DesignConsumptionLineViewModel();
            s.BaseProductId = Id;
            s.ContentType = "OverTuft Contents";
            DesignConsumptionLineViewModel temp = _BomDetailService.GetBaseProductDetail(Id);

            s.DesignName = temp.DesignName;
            s.DesignId = temp.DesignId;
            s.QualityName = temp.QualityName;
            s.Weight = temp.Weight;
            s.BaseProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.OverTuft).ProcessId;

            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        public ActionResult _CreateMainContent(int ProductGroupId, string QualityName, int ColourId, Decimal? Weight) //Id ==>Design Content Header Id
        {
            DesignConsumptionLineViewModel s = new DesignConsumptionLineViewModel();
            s.ContentType = "Main Contents";
            s.DesignName = new ProductGroupService(_unitOfWork).Find(ProductGroupId).ProductGroupName;
            s.DesignId = ProductGroupId;
            s.QualityName = QualityName;
            s.ColourName = new ColourService(_unitOfWork).Find(ColourId).ColourName;
            s.Weight = Weight ?? 0;
            s.BaseProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;

            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        public ActionResult _CreateOtherContent(int ProductGroupId, string QualityName, int ColourId, Decimal? Weight) //Id ==>Design Content Header Id
        {
            DesignConsumptionLineViewModel s = new DesignConsumptionLineViewModel();
            s.ContentType = "Other Contents";
            s.DesignName = new ProductGroupService(_unitOfWork).Find(ProductGroupId).ProductGroupName;
            s.DesignId = ProductGroupId;
            s.QualityName = QualityName;
            s.ColourName = new ColourService(_unitOfWork).Find(ColourId).ColourName;
            s.Weight = Weight ?? 0;
            s.BaseProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;

            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        public ActionResult _CreateOverTuftContent(int ProductGroupId, string QualityName, int ColourId, Decimal? Weight) //Id ==>Design Content Header Id
        {
            DesignConsumptionLineViewModel s = new DesignConsumptionLineViewModel();
            s.ContentType = "OverTuft Contents";
            s.DesignName = new ProductGroupService(_unitOfWork).Find(ProductGroupId).ProductGroupName;
            s.DesignId = ProductGroupId;
            s.QualityName = QualityName;
            s.ColourName = new ColourService(_unitOfWork).Find(ColourId).ColourName;
            s.Weight = Weight ?? 0;
            s.BaseProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.OverTuft).ProcessId;

            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(DesignConsumptionLineViewModel svm)
        {
            if (ModelState.IsValid)
            {
                FinishedProduct product = new FinishedProduct();
                if (svm.BaseProductId == 0)
                {
                    ProductQuality Quality = new ProductQualityService(_unitOfWork).Find(svm.QualityName);

                    string ConsumptionProductName = "";
                    if (svm.ColourName != "" && svm.ColourName != null)
                    {
                        ConsumptionProductName = svm.DesignName.ToString().Trim() + "-" + svm.ColourName.ToString().Trim() + "-Bom";
                    }
                    else
                    {
                        ConsumptionProductName = svm.DesignName.ToString().Trim() + "-Bom";
                    }


                    int ProductGroupId = new ProductGroupService(_unitOfWork).Find(svm.DesignName).ProductGroupId;

                    product.ProductCode = ConsumptionProductName;
                    product.ProductName = ConsumptionProductName;
                    product.ProductGroupId = new ProductGroupService(_unitOfWork).Find(ProductGroupConstants.Bom).ProductGroupId;
                    product.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    product.IsActive = true;
                    product.ReferenceDocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductGroup).DocumentTypeId;
                    product.ReferenceDocId = ProductGroupId;
                    product.StandardWeight = svm.Weight;
                    product.CreatedDate = DateTime.Now;
                    product.ModifiedDate = DateTime.Now;
                    product.CreatedBy = User.Identity.Name;
                    product.ModifiedBy = User.Identity.Name;
                    product.ObjectState = Model.ObjectState.Added;

                    product.IsSample = false;
                    product.ProductQualityId = Quality.ProductQualityId;
                    new ProductService(_unitOfWork).Create(product);


                    ProductGroup ProductGroup = new ProductGroupService(_unitOfWork).Find(svm.DesignName);
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
                            bomdetail.BaseProcessId = svm.BaseProcessId;
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


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.DesignColourConsumption).DocumentTypeId,
                        DocId = product.ProductId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));


                }
                else
                {
                    //product = new FinishedProductService(_unitOfWork).Find(svm.BaseProductId);
                    product.ProductId = svm.BaseProductId;
                }

                if (svm.BomDetailId == 0)
                {
                    BomDetail bomdetail = new BomDetail();

                    bomdetail.BaseProductId = svm.BaseProductId;
                    bomdetail.BatchQty = 1;
                    bomdetail.ConsumptionPer = svm.ConsumptionPer;
                    bomdetail.Dimension1Id = svm.Dimension1Id;
                    bomdetail.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;
                    bomdetail.BaseProcessId = svm.BaseProcessId;
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
                        PrepareViewBag(svm);
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
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.DesignConsumption).DocumentTypeId,
                        DocId = bomdetail.BomDetailId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    

                    if (svm.ContentType == "Main Contents")
                    {
                        return RedirectToAction("_CreateMainContentForBaseProduct", new { id = product.ProductId });
                    }
                    else if (svm.ContentType == "OverTuft Contents")
                    {
                        return RedirectToAction("_CreateOverTuftContentForBaseProduct", new { id = product.ProductId });
                    }
                    else
                    {
                        return RedirectToAction("_CreateOtherContentForBaseProduct", new { id = product.ProductId });
                    }
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
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.DesignConsumption).DocumentTypeId,
                        DocId = bomdetail.BomDetailId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return Json(new { success = true });
                }
            }

            PrepareViewBag(svm);
            return PartialView("_Create", svm);
        }


        public ActionResult _Edit(int id)
        {
            DesignConsumptionLineViewModel s = _BomDetailService.GetDesignConsumptionLineForEdit(id);

            DesignConsumptionLineViewModel temp = _BomDetailService.GetBaseProductDetail(s.BaseProductId);

            s.DesignName = temp.DesignName;
            s.DesignId = temp.DesignId;
            s.QualityName = temp.QualityName;
            s.Weight = temp.Weight;

            PrepareViewBag(s);

            if (s == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", s);
        }

        [HttpGet]
        public ActionResult _EditWithSKU(int id)
        {
            DesignConsumptionLineViewModel s = _BomDetailService.GetDesignConsumptionLineForEdit(id);

            DesignConsumptionLineViewModel temp = _BomDetailService.GetBaseProductDetail(s.BaseProductId);


            var product = db.Product.Find(s.BaseProductId);

            var pendingSKUConsumptionToUpdate = (from p in db.Product
                                                 join pg in db.ProductGroups on p.ProductGroupId equals pg.ProductGroupId
                                                 join pt in db.ProductTypes on pg.ProductTypeId equals pt.ProductTypeId
                                                 join bd1 in db.BomDetail on p.ProductId equals bd1.BaseProductId
                                                 join bd2 in db.BomDetail on bd1.ProductId equals bd2.BaseProductId
                                                 join prod in db.Product on bd2.ProductId equals prod.ProductId
                                                 join d1 in db.Dimension1 on bd2.Dimension1Id equals d1.Dimension1Id into table
                                                 from dtab in table.DefaultIfEmpty()
                                                 where pg.ProductGroupName == product.ProductName && bd2.Dimension1Id == s.Dimension1Id
                                                 && bd2.ProductId == s.ProductId
                                                 && bd2.BomDetailId != s.BomDetailId
                                                 group new { p, bd2, dtab, prod } by bd2.BomDetailId into g
                                                 orderby g.Max(m => m.p.ProductName)
                                                 select g.Select(m => new SKUBomViewModel { BaseProductName = m.p.ProductName, Shade = m.dtab.Dimension1Name, Qty = m.bd2.Qty, ProductName = m.prod.ProductName }).FirstOrDefault()).ToList();

            s.DesignName = temp.DesignName;
            s.QualityName = temp.QualityName;
            s.Weight = temp.Weight;
            s.SKUs = pendingSKUConsumptionToUpdate;
            PrepareViewBag(s);

            if (s == null)
            {
                return HttpNotFound();
            }
            return PartialView("_EditWithSKU", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditWithSKU(DesignConsumptionLineViewModel svm)
        {
            if (ModelState.IsValid)
            {

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                BomDetail bomdetail = _BomDetailService.Find(svm.BomDetailId);

                int? ExistingDim1Id = bomdetail.Dimension1Id;
                int ExistingProductId = bomdetail.ProductId;

                BomDetail ExRec = Mapper.Map<BomDetail>(bomdetail);

                bomdetail.BaseProductId = svm.BaseProductId;
                bomdetail.Dimension1Id = svm.Dimension1Id;
                bomdetail.ProductId = svm.ProductId;

                bomdetail.ModifiedDate = DateTime.Now;
                bomdetail.ModifiedBy = User.Identity.Name;
                bomdetail.ObjectState = Model.ObjectState.Modified;
                _BomDetailService.Update(bomdetail);

                var product = db.Product.Find(bomdetail.BaseProductId);

                var pendingSKUConsumptionToUpdate = (from p in db.Product
                                                     join pg in db.ProductGroups on p.ProductGroupId equals pg.ProductGroupId
                                                     join pt in db.ProductTypes on pg.ProductTypeId equals pt.ProductTypeId
                                                     join bd1 in db.BomDetail on p.ProductId equals bd1.BaseProductId
                                                     join bd2 in db.BomDetail on bd1.ProductId equals bd2.BaseProductId
                                                     where pg.ProductGroupName == product.ProductName && bd2.Dimension1Id == ExistingDim1Id
                                                     && bd2.ProductId == ExistingProductId
                                                     && bd2.BomDetailId != bomdetail.BomDetailId
                                                     group bd2 by bd2.BomDetailId into g
                                                     orderby g.Key
                                                     select g.FirstOrDefault()).ToList();

                foreach (var item in pendingSKUConsumptionToUpdate)
                {
                    item.ProductId = bomdetail.ProductId;
                    item.Dimension1Id = bomdetail.Dimension1Id;
                    item.ObjectState = Model.ObjectState.Modified;

                    _BomDetailService.Update(item);
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = bomdetail,
                });
                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                Modifications.Add("Updated in All SKU and Shades.");

                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_EditWithSKU", svm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.DesignConsumption).DocumentTypeId,
                    DocId = bomdetail.BomDetailId,
                    ActivityType = (int)ActivityTypeContants.Modified,
                    xEModifications = Modifications,
                }));

                return Json(new { success = true });

            }

            PrepareViewBag(svm);
            return PartialView("_EditWithSKU", svm);
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
        public ActionResult DeletePost(DesignConsumptionLineViewModel vm)
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
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.DesignConsumption).DocumentTypeId,
                DocId = BomDetail.BomDetailId,
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

        public JsonResult GetConsumptionTotalQty(int BaseProductId, Decimal TotalWeight, Decimal BomQty, int BomDetailId, int BaseProcessId)
        {
            var ProductFaceContentGroups = from p in db.Product
                                           join pg in db.ProductGroups on p.ReferenceDocId equals pg.ProductGroupId into ProductGroupTable
                                           from ProductGroupTab in ProductGroupTable.DefaultIfEmpty()
                                           join fp in db.FinishedProduct on ProductGroupTab.ProductGroupId equals fp.ProductGroupId into FinishedProductTable
                                           from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
                                           join pcl in db.ProductContentLine on FinishedProductTab.FaceContentId equals pcl.ProductContentHeaderId into ProductContentLineTable
                                           from ProductContentLineTab in ProductContentLineTable.DefaultIfEmpty()
                                           where p.ProductId == BaseProductId && ((int?)ProductContentLineTab.ProductGroupId ?? 0) != 0
                                           group new { ProductContentLineTab } by new { ProductContentLineTab.ProductGroupId } into Result
                                           select new
                                           {
                                               ProductGroupId = Result.Key.ProductGroupId
                                           };


            Decimal TotalFillQty = 0;
            var temp = (from L in db.BomDetail
                        join p in db.Product on L.ProductId equals p.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join pcon in ProductFaceContentGroups on ProductTab.ProductGroupId equals pcon.ProductGroupId into ProductFaceContentTable
                        from ProductFaceContentTab in ProductFaceContentTable.DefaultIfEmpty()
                        where L.BaseProductId == BaseProductId && L.BomDetailId != BomDetailId && L.BaseProcessId == BaseProcessId && ((int?)ProductFaceContentTab.ProductGroupId ?? 0) != 0
                                    group (L) by (L.BaseProductId) into Result
                                    select new
                                    {
                                        TotalQty = Result.Sum(i => i.Qty)
                                    }).FirstOrDefault();

            if (temp != null)
            {
                TotalFillQty  = temp.TotalQty;
            }

            return Json(TotalFillQty);
        }

        public JsonResult IsProductContent(int BaseProductId, int ProductId)
        {
            bool IsContent = true;
            var ProductFaceContentGroups = from p in db.Product
                                           join pg in db.ProductGroups on p.ReferenceDocId equals pg.ProductGroupId into ProductGroupTable
                                           from ProductGroupTab in ProductGroupTable.DefaultIfEmpty()
                                           join fp in db.FinishedProduct on ProductGroupTab.ProductGroupId equals fp.ProductGroupId into FinishedProductTable
                                           from FinishedProductTab in FinishedProductTable.DefaultIfEmpty()
                                           join pcl in db.ProductContentLine on FinishedProductTab.FaceContentId equals pcl.ProductContentHeaderId into ProductContentLineTable
                                           from ProductContentLineTab in ProductContentLineTable.DefaultIfEmpty()
                                           where p.ProductId == BaseProductId && ((int?)ProductContentLineTab.ProductGroupId ?? 0) != 0
                                           group new { ProductContentLineTab } by new { ProductContentLineTab.ProductGroupId } into Result
                                           select new
                                           {
                                               ProductGroupId = Result.Key.ProductGroupId
                                           };


            var temp = (from p in db.Product 
                        join pcon in ProductFaceContentGroups on p.ProductGroupId equals pcon.ProductGroupId into ProductFaceContentTable
                        from ProductFaceContentTab in ProductFaceContentTable.DefaultIfEmpty()
                        where p.ProductId == ProductId && ((int?)ProductFaceContentTab.ProductGroupId ?? 0) != 0
                        select new
                        {
                            ProductId = p.ProductId
                        }).FirstOrDefault();

            if (temp != null)
            {
                IsContent = true;
            }
            else
            {
                IsContent = false;
            }

            return Json(IsContent);
        }

        public JsonResult GetFaceContentProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var Query = _BomDetailService.GetFaceContentProductList(filter, searchTerm);

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult GetOtherContentProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var Query = _BomDetailService.GetOtherContentProductList(filter, searchTerm);

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult GetOverTuftContentProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var Query = _BomDetailService.GetOverTuftContentProductList(filter, searchTerm);

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }




        
    }
}
