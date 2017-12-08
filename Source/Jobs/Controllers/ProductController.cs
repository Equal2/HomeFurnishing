using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Presentation.ViewModels;
using Model.ViewModels;
using Core.Common;
using System.Configuration;
using System.IO;
using ImageResizer;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Reports.Reports;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();


        IProductService _ProductService;
        IFinishedProductService _FinishedProductService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductController(IProductService ProductService, IFinishedProductService FinishedProductService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductService = ProductService;
            _FinishedProductService = FinishedProductService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public void PrepareViewBag()
        {
            ViewBag.SalesTaxList = new SalesTaxGroupProductService(_unitOfWork).GetSalesTaxGroupProductList().ToList();
            ViewBag.UnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        // GET: /ProductMaster/

        public ActionResult Index()
        {
            var Product = _ProductService.GetProductListForIndex();
            return View(Product);
        }


        // GET: /ProductMaster/Create

        public ActionResult Create()
        {
            ProductViewModel p = new ProductViewModel();
            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            p.IsActive = true;
            PrepareViewBag();
            return View("Create", p);

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductViewModel pvm)
        {
            Product pt1 = AutoMapper.Mapper.Map<ProductViewModel, Product>(pvm);
            if (ModelState.IsValid)
            {

                if (pvm.ProductId <= 0)
                {

                    pt1.CreatedDate = DateTime.Now;
                    pt1.ModifiedDate = DateTime.Now;
                    pt1.CreatedBy = User.Identity.Name;
                    pt1.ModifiedBy = User.Identity.Name;
                    pt1.IsActive = true;
                    pt1.ObjectState = Model.ObjectState.Added;
                    _ProductService.Create(pt1);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", pvm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product).DocumentTypeId,
                        DocId = pt1.ProductId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));


                    return RedirectToAction("Create").Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    FinishedProduct pt = _FinishedProductService.Find(pvm.ProductId);

                    FinishedProduct ExRec = Mapper.Map<FinishedProduct>(pt);

                    pt.ProductName = pvm.ProductName;
                    pt.ProductCode = pvm.ProductCode;
                    pt.ProductDescription = pvm.ProductDescription;
                    pt.StandardCost = pvm.StandardCost;
                    pt.SaleRate = pvm.SaleRate;
                    pt.ProductCategoryId = pvm.ProductCategoryId;
                    pt.ProductGroupId = pvm.ProductGroupId;
                    pt.ProductCollectionId = pvm.ProductCollectionId;
                    pt.ProductQualityId = pvm.ProductQualityId;
                    pt.ProductDesignId = pvm.ProductDesignId;
                    pt.ProductInvoiceGroupId = pvm.ProductInvoiceGroupId;
                    pt.ProductStyleId = pvm.ProductStyleId;
                    pt.ProductManufacturerId = pvm.ProductManufacturerId;
                    pt.DrawBackTariffHeadId = pvm.DrawBackTariffHeadId;
                    pt.ProcessSequenceHeaderId = pvm.ProcessSequenceHeaderId;
                    pt.SalesTaxGroupProductId = pvm.SalesTaxGroupProductId;
                    pt.UnitId = pvm.UnitId;
                    pt.IsActive = pvm.IsActive;

                    pt.ModifiedDate = DateTime.Now;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Modified;
                    _ProductService.Update(pt);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = pt,
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
                        PrepareViewBag();
                        return View("Create", pvm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product).DocumentTypeId,
                        DocId = pt.ProductId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index").Success("Data saved successfully");

                }
            }
            PrepareViewBag();
            return View("Create", pvm);
        }

        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            PrepareViewBag();
            ProductViewModel pt = _ProductService.GetProduct(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", pt);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            ProductViewModel Product = _ProductService.GetProduct(id);
            if (Product == null)
            {
                return HttpNotFound();
            }

            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {

                var temp = _ProductService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                _ProductService.Delete(vm.id);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product).DocumentTypeId,
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
            var nextId = _ProductService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _ProductService.PrevId(id);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }
















        //For RawMaterial&& Other Material Master




        public void PrepareMaterialViewBag(MaterialViewModel ma)
        {
            ViewBag.UnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            ViewBag.SalesTaxGroup = new SalesTaxGroupProductService(_unitOfWork).GetSalesTaxGroupProductList().ToList();
            List<SelectListItem> tem = new List<SelectListItem>();
            tem.Add(new SelectListItem { Text = "Yes", Value = "True" });
            tem.Add(new SelectListItem { Text = "No", Value = "False" });
            if (ma == null)
            {
                ViewBag.LotManagement = new SelectList(tem, "Value", "Text");
            }
            else
            {
                ViewBag.LotManagement = new SelectList(tem, "Value", "Text", ma.LotManagement);
            }
        }

        public ActionResult ProductTypeIndex(int id)//NatureId
        {
            var producttype = new ProductTypeService(_unitOfWork).GetProductTypeListForMaterial(id).Where(m => m.IsActive != false).ToList();

            ViewBag.ProductNatureName = new ProductNatureService(_unitOfWork).Find(id).ProductNatureName;




            if (producttype.Count() == 0)
            {
                ViewBag.PrevLink = Request.UrlReferrer.ToString();
                ViewBag.Message = "No ProductType found for this section.";
                return View("~/Views/Shared/NotFound.cshtml");
            }
            if (producttype.Count() == 1)
                return RedirectToAction("MaterialIndex", new { id = producttype.FirstOrDefault().ProductTypeId });
            else
                return View("ProductTypeIndexForMaterial", producttype);
        }


         //GET: /ProductMaster/

        public ActionResult MaterialIndex(int id, string IndexFilterParameter)//// Changed To ProductTypeId
        {
            IQueryable<ProductIndexViewModel> Product = null;

            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(id);
            if (settings != null)
            {
                ViewBag.ImportMenuId = settings.ImportMenuId;
            }

            if (IndexFilterParameter == "" || IndexFilterParameter == null)
            {
                if (settings != null)
                {
                    IndexFilterParameter = settings.IndexFilterParameter;
                }
            }

            if (IndexFilterParameter == IndexFilterParameterConstants.Active)
            {
                Product = _ProductService.GetProductListForMaterial(id).Where(m => m.IsActive == true && m.DiscontinueDate == null);
            }
            else if (IndexFilterParameter == IndexFilterParameterConstants.Discontinue)
            {
                Product = _ProductService.GetProductListForMaterial(id).Where(m => m.DiscontinueDate != null);
            }
            else if (IndexFilterParameter == IndexFilterParameterConstants.InActive)
            {
                Product = _ProductService.GetProductListForMaterial(id).Where(m => m.IsActive == false);
            }
            else
            {
                Product = _ProductService.GetProductListForMaterial(id);
            }





            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
            ViewBag.id = id;



            return View(Product);
        }

        //public ActionResult MaterialIndex_Active(int id)
        //{
        //    var Product = _ProductService.GetProductListForMaterial(id).Where(m => m.IsActive == true && m.DiscontinueDate == null);
        //    ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
        //    ViewBag.id = id;

        //    var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(id);
        //    if (settings != null)
        //    {
        //        ViewBag.ImportMenuId = settings.ImportMenuId;
        //    }

        //    return View(Product);
        //}

        //public ActionResult MaterialIndex_Discontinue(int id)
        //{
        //    var Product = _ProductService.GetProductListForMaterial(id).Where(m => m.DiscontinueDate != null);
        //    ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
        //    ViewBag.id = id;

        //    var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(id);
        //    if (settings != null)
        //    {
        //        ViewBag.ImportMenuId = settings.ImportMenuId;
        //    }

        //    return View(Product);
        //}

        //public ActionResult MaterialIndex_InActive(int id)
        //{
        //    var Product = _ProductService.GetProductListForMaterial(id).Where(m => m.IsActive == false);
        //    ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
        //    ViewBag.id = id;

        //    var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(id);
        //    if (settings != null)
        //    {
        //        ViewBag.ImportMenuId = settings.ImportMenuId;
        //    }

        //    return View(Product);
        //}



        //public ActionResult ProductGroupIndex(int id)//ProductTypeId
        //{
        //    var productGroup = new ProductGroupService(_unitOfWork).GetProductGroupList(id);
        //    if (productGroup.Count() == 0)
        //    {
        //        ViewBag.PrevLink = Request.UrlReferrer.ToString();
        //        ViewBag.Message = "No ProductGroup found for this section.";
        //        return View("~/Views/Shared/NotFound.cshtml");
        //    }
        //    if (productGroup.Count() == 1)
        //        return RedirectToAction("MaterialIndex", new { id = productGroup.FirstOrDefault().ProductGroupId });
        //    else
        //        return View("ProductGroupIndex", productGroup);
        //}


        // GET: /ProductMaster/Create

        public ActionResult CreateMaterial(int id)//ProductType Id
        {
            MaterialViewModel p = new MaterialViewModel();

            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            p.IsActive = true;
            p.ProductTypeId = id;
            var ProductType = new ProductTypeService(_unitOfWork).Find(id);
            ViewBag.Name = ProductType.ProductTypeName;
            ViewBag.id = id;
            ViewBag.ProductGroupList = new ProductGroupService(_unitOfWork).GetProductGroupListForItemType(id);

            List<ProductTypeAttributeViewModel> tem = new ProductTypeAttributeService(_unitOfWork).GetAttributeVMForProductType(id).ToList();
            p.ProductTypeAttributes = tem;

            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(id);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "ProductTypeSettings", new { id = id }).Warning("Please create Product Type Settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            p.ProductTypeSettings = Mapper.Map<ProductTypeSettings, ProductTypeSettingsViewModel>(settings);
            p.UnitId = settings.UnitId;
            p.ProductCode = new ProductService(_unitOfWork).FGetNewCode((int)p.ProductTypeId, settings.SqlProcProductCode);

            PrepareMaterialViewBag(null);
            return View("CreateMaterial", p);

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostMaterial(MaterialViewModel pvm)
        {
            Product pt1 = new Product();

            
            if (pvm.ProductGroupId <= 0)
            {
                ModelState.AddModelError("ProductGroupId", "Product Group field is required");
            }

            ProductTypeSettings settings = new ProductTypeSettings();

            if (pvm.ProductTypeId != null)
            {
                settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument((int)pvm.ProductTypeId);
            }

            if (ModelState.IsValid)
            {
                ProductGroup group = new ProductGroupService(_unitOfWork).Find(pvm.ProductGroupId);
                ProductType Type = new ProductTypeService(_unitOfWork).Find(group.ProductTypeId);


                //Checking for Create or Edit(<=0 =====>CREATE)
                if (pvm.ProductId <= 0)
                {

                    pt1.ProductName = pvm.ProductName;
                    pt1.ProductCode = pvm.ProductCode;
                    pt1.ProductDescription = pvm.ProductDescription;
                    pt1.ProductGroupId = pvm.ProductGroupId;
                    pt1.ProductCategoryId = pvm.ProductCategoryId;
                    pt1.ProductSpecification = pvm.ProductSpecification;
                    pt1.StandardCost = pvm.StandardCost;
                    pt1.SaleRate = pvm.SaleRate;
                    pt1.Tags = pvm.Tags;
                    pt1.UnitId = pvm.UnitId;
                    pt1.SalesTaxGroupProductId = pvm.SalesTaxGroupProductId;
                    pt1.IsActive = pvm.IsActive;
                    pt1.DivisionId = pvm.DivisionId;
                    pt1.ProfitMargin = pvm.ProfitMargin;
                    pt1.CarryingCost = pvm.CarryingCost;
                    pt1.DefaultDimension1Id = pvm.DefaultDimension1Id;
                    pt1.DefaultDimension2Id = pvm.DefaultDimension2Id;
                    pt1.DefaultDimension3Id = pvm.DefaultDimension3Id;
                    pt1.DefaultDimension4Id = pvm.DefaultDimension4Id;
                    pt1.DiscontinueDate = pvm.DiscontinueDate;
                    pt1.DiscontinueReason = pvm.DiscontinueReason;
                    pt1.SalesTaxProductCodeId = pvm.SalesTaxProductCodeId;
                    pt1.CreatedDate = DateTime.Now;
                    pt1.ModifiedDate = DateTime.Now;
                    pt1.CreatedBy = User.Identity.Name;
                    pt1.ModifiedBy = User.Identity.Name;
                    pt1.IsActive = true;

                    pt1.ObjectState = Model.ObjectState.Added;
                    _ProductService.Create(pt1);


                    //Saving ProductSite Details
                    ProductSiteDetail psd = new ProductSiteDetail();
                    psd.MinimumOrderQty = pvm.MinimumOrderQty;
                    psd.ReOrderLevel = pvm.ReOrderLevel;
                    psd.GodownId = pvm.GodownId;
                    psd.BinLocationId = pvm.BinLocationId;
                    psd.SiteId = pvm.SiteId;
                    psd.DivisionId = pvm.DivisionId;
                    psd.ProductId = pt1.ProductId;
                    psd.LotManagement = pvm.LotManagement;
                    psd.CreatedBy = User.Identity.Name;
                    psd.ModifiedBy = User.Identity.Name;
                    psd.CreatedDate = DateTime.Now;
                    psd.ModifiedDate = DateTime.Now;
                    new ProductSiteDetailService(_unitOfWork).Create(psd);

                    if (pvm.ProductTypeAttributes != null)
                    {
                        foreach (var pta in pvm.ProductTypeAttributes)
                        {
                            ProductAttributes productattribute = new ProductAttributeService(_unitOfWork).Find(pt1.ProductId, pta.ProductTypeAttributeId);

                            if (productattribute != null)
                            {
                                productattribute.ProductAttributeValue = pta.DefaultValue;
                                productattribute.ObjectState = Model.ObjectState.Modified;
                                new ProductAttributeService(_unitOfWork).Update(productattribute);
                            }
                            else
                            {
                                ProductAttributes pa = new ProductAttributes()
                                {
                                    ProductAttributeValue = pta.DefaultValue,
                                    ProductId = pt1.ProductId,
                                    ProductTypeAttributeId = pta.ProductTypeAttributeId,
                                    CreatedBy = User.Identity.Name,
                                    ModifiedBy = User.Identity.Name,
                                    CreatedDate = DateTime.Now,
                                    ModifiedDate = DateTime.Now

                                };
                                pa.ObjectState = Model.ObjectState.Added;
                                new ProductAttributeService(_unitOfWork).Create(pa);
                            }
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
                        PrepareMaterialViewBag(null);
                        ViewBag.Name = Type.ProductTypeName;
                        ViewBag.id = group.ProductTypeId;
                        ViewBag.ProductGroupList = new ProductGroupService(_unitOfWork).GetProductGroupListForItemType(group.ProductTypeId);
                        return View("CreateMaterial", pvm);

                    }




                    #region

                    //Saving Images if any uploaded after UnitOfWorkSave

                    if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
                    {
                        //For checking the first time if the folder exists or not-----------------------------
                        string uploadfolder;
                        int MaxLimit;
                        int.TryParse(ConfigurationManager.AppSettings["MaxFileUploadLimit"], out MaxLimit);
                        var x = (from iid in db.Counter
                                 select iid).FirstOrDefault();
                        if (x == null)
                        {

                            uploadfolder = System.Guid.NewGuid().ToString();
                            Counter img = new Counter();
                            img.ImageFolderName = uploadfolder;
                            img.ModifiedBy = User.Identity.Name;
                            img.CreatedBy = User.Identity.Name;
                            img.ModifiedDate = DateTime.Now;
                            img.CreatedDate = DateTime.Now;
                            new CounterService(_unitOfWork).Create(img);
                            _unitOfWork.Save();
                        }

                        else
                        { uploadfolder = x.ImageFolderName; }


                        //For checking if the image contents length is greater than 100 then create a new folder------------------------------------

                        if (!Directory.Exists(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder))) Directory.CreateDirectory(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder));

                        int count = Directory.GetFiles(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder)).Length;

                        if (count >= MaxLimit)
                        {
                            uploadfolder = System.Guid.NewGuid().ToString();
                            var u = new CounterService(_unitOfWork).Find(x.CounterId);
                            u.ImageFolderName = uploadfolder;
                            new CounterService(_unitOfWork).Update(u);
                            _unitOfWork.Save();
                        }


                        //Saving Thumbnails images:
                        Dictionary<string, string> versions = new Dictionary<string, string>();

                        //Define the versions to generate
                        versions.Add("_thumb", "maxwidth=100&maxheight=100"); //Crop to square thumbnail
                        versions.Add("_medium", "maxwidth=200&maxheight=200"); //Fit inside 400x400 area, jpeg

                        string temp2 = "";
                        string filename = System.Guid.NewGuid().ToString();
                        foreach (string filekey in System.Web.HttpContext.Current.Request.Files.Keys)
                        {

                            HttpPostedFile pfile = System.Web.HttpContext.Current.Request.Files[filekey];
                            if (pfile.ContentLength <= 0) continue; //Skip unused file controls.  

                            temp2 = Path.GetExtension(pfile.FileName);

                            string uploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder);
                            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                            string filecontent = Path.Combine(uploadFolder, pvm.ProductName + "_" + filename);

                            //pfile.SaveAs(filecontent);
                            ImageBuilder.Current.Build(new ImageJob(pfile, filecontent, new Instructions(), false, true));


                            //Generate each version
                            foreach (string suffix in versions.Keys)
                            {
                                if (suffix == "_thumb")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Thumbs");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, pvm.ProductName + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, pvm.ProductName + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }

                            }

                            //var tempsave = _FinishedProductService.Find(pt.ProductId);

                            var tempsave = _ProductService.Find(pt1.ProductId);

                            tempsave.ImageFileName = pvm.ProductName + "_" + filename + temp2;
                            tempsave.ImageFolderName = uploadfolder;
                            _ProductService.Update(tempsave);
                            _unitOfWork.Save();
                        }

                    }

                    #endregion

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product).DocumentTypeId,
                        DocId = pt1.ProductId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));


                    if (settings != null)
                    {
                        if (settings.isVisibleConsumptionDetail == true || settings.isVisibleProductProcessDetail == true)
                        {
                            return RedirectToAction("EditMaterial", new { id = pt1.ProductId }).Success("Data saved Successfully");
                        }
                        else
                        {
                            return RedirectToAction("CreateMaterial", new { id = group.ProductTypeId }).Success("Data saved successfully");
                        }
                    }
                    else
                    {
                        return RedirectToAction("CreateMaterial", new { id = group.ProductTypeId }).Success("Data saved successfully");
                    }
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    Product pt = _ProductService.Find(pvm.ProductId);
                    Product ExRec = Mapper.Map<Product>(pt);

                    pt.ProductName = pvm.ProductName;
                    pt.ProductCode = pvm.ProductCode;
                    pt.ProductDescription = pvm.ProductDescription;
                    pt.StandardCost = pvm.StandardCost;
                    pt.SaleRate = pvm.SaleRate;
                    pt.ProductGroupId = pvm.ProductGroupId;
                    pt.ProductCategoryId = pvm.ProductCategoryId;
                    pt.ProductSpecification = pvm.ProductSpecification;
                    pt.SalesTaxGroupProductId = pvm.SalesTaxGroupProductId;
                    pt.ProfitMargin = pvm.ProfitMargin;
                    pt.CarryingCost = pvm.CarryingCost;
                    pt.DefaultDimension1Id = pvm.DefaultDimension1Id;
                    pt.DefaultDimension2Id = pvm.DefaultDimension2Id;
                    pt.DefaultDimension3Id = pvm.DefaultDimension3Id;
                    pt.DefaultDimension4Id = pvm.DefaultDimension4Id;
                    pt.DiscontinueDate = pvm.DiscontinueDate;
                    pt.DiscontinueReason = pvm.DiscontinueReason;
                    pt.SalesTaxProductCodeId = pvm.SalesTaxProductCodeId;
                    pt.Tags = pvm.Tags;
                    pt.UnitId = pvm.UnitId;
                    pt.IsActive = pvm.IsActive;

                    pt.ModifiedDate = DateTime.Now;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Modified;
                    _ProductService.Update(pt);


                    if (pvm.ProductSiteDetailId <= 0)
                    {
                        ProductSiteDetail psd = new ProductSiteDetail();
                        psd.MinimumOrderQty = pvm.MinimumOrderQty;
                        psd.ReOrderLevel = pvm.ReOrderLevel;
                        psd.GodownId = pvm.GodownId;
                        psd.BinLocationId = pvm.BinLocationId;
                        psd.SiteId = pvm.SiteId;
                        psd.DivisionId = pvm.DivisionId;
                        psd.ProductId = pvm.ProductId;
                        psd.LotManagement = pvm.LotManagement;
                        psd.CreatedBy = User.Identity.Name;
                        psd.ModifiedBy = User.Identity.Name;
                        psd.CreatedDate = DateTime.Now;
                        psd.ModifiedDate = DateTime.Now;

                        new ProductSiteDetailService(_unitOfWork).Create(psd);

                    }
                    else
                    {
                        ProductSiteDetail psd = new ProductSiteDetailService(_unitOfWork).Find(pvm.ProductSiteDetailId);
                        psd.MinimumOrderQty = pvm.MinimumOrderQty;
                        psd.ReOrderLevel = pvm.ReOrderLevel;
                        psd.GodownId = pvm.GodownId;
                        psd.BinLocationId = pvm.BinLocationId;
                        psd.LotManagement = pvm.LotManagement;
                        psd.ModifiedBy = User.Identity.Name;
                        psd.ModifiedDate = DateTime.Now;

                        new ProductSiteDetailService(_unitOfWork).Update(psd);
                    }

                    if (pvm.ProductTypeAttributes != null)
                    {
                        foreach (var pta in pvm.ProductTypeAttributes)
                        {
                            ProductAttributes productattribute = new ProductAttributeService(_unitOfWork).Find(pt.ProductId, pta.ProductTypeAttributeId);

                            if (productattribute != null)
                            {
                                productattribute.ProductAttributeValue = pta.DefaultValue;
                                productattribute.ObjectState = Model.ObjectState.Modified;
                                new ProductAttributeService(_unitOfWork).Update(productattribute);
                            }
                            else
                            {
                                ProductAttributes pa = new ProductAttributes()
                                {
                                    ProductAttributeValue = pta.DefaultValue,
                                    ProductId = pt.ProductId,
                                    ProductTypeAttributeId = pta.ProductTypeAttributeId,
                                    CreatedBy = User.Identity.Name,
                                    ModifiedBy = User.Identity.Name,
                                    CreatedDate = DateTime.Now,
                                    ModifiedDate = DateTime.Now

                                };
                                pa.ObjectState = Model.ObjectState.Added;
                                new ProductAttributeService(_unitOfWork).Create(pa);
                            }
                        }
                    }

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = pt,
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
                        PrepareMaterialViewBag(pvm);
                        ViewBag.Name = Type.ProductTypeName;
                        ViewBag.id = group.ProductTypeId;
                        ViewBag.ProductGroupList = new ProductGroupService(_unitOfWork).GetProductGroupListForItemType(group.ProductTypeId);
                        return View("CreateMaterial", pvm);
                    }                  

                    #region

                    //Saving Image if file is uploaded
                    if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
                    {
                        string uploadfolder = pt.ImageFolderName;
                        string tempfilename = pt.ImageFileName;
                        if (uploadfolder == null)
                        {
                            var x = (from iid in db.Counter
                                     select iid).FirstOrDefault();
                            if (x == null)
                            {

                                uploadfolder = System.Guid.NewGuid().ToString();
                                Counter img = new Counter();
                                img.ImageFolderName = uploadfolder;
                                img.ModifiedBy = User.Identity.Name;
                                img.CreatedBy = User.Identity.Name;
                                img.ModifiedDate = DateTime.Now;
                                img.CreatedDate = DateTime.Now;
                                new CounterService(_unitOfWork).Create(img);
                                _unitOfWork.Save();
                            }
                            else
                            { uploadfolder = x.ImageFolderName; }

                        }
                        //Deleting Existing Images

                        var xtemp = System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/" + tempfilename);
                        if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/" + tempfilename)))
                        {
                            System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/" + tempfilename));
                        }

                        //Deleting Thumbnail Image:

                        if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Thumbs/" + tempfilename)))
                        {
                            System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Thumbs/" + tempfilename));
                        }

                        //Deleting Medium Image:
                        if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Medium/" + tempfilename)))
                        {
                            System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Medium/" + tempfilename));
                        }

                        //Saving Thumbnails images:
                        Dictionary<string, string> versions = new Dictionary<string, string>();

                        //Define the versions to generate
                        versions.Add("_thumb", "maxwidth=100&maxheight=100"); //Crop to square thumbnail
                        versions.Add("_medium", "maxwidth=200&maxheight=200"); //Fit inside 400x400 area, jpeg                            
                        var temo = pt.ProductName.Replace(" ", string.Empty);
                        string temp2 = "";
                        string filename = System.Guid.NewGuid().ToString();
                        foreach (string filekey in System.Web.HttpContext.Current.Request.Files.Keys)
                        {

                            HttpPostedFile pfile = System.Web.HttpContext.Current.Request.Files[filekey];
                            if (pfile.ContentLength <= 0) continue; //Skip unused file controls.    

                            temp2 = Path.GetExtension(pfile.FileName);

                            string uploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder);
                            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                            string filecontent = Path.Combine(uploadFolder, pt.ProductName + "_" + filename);

                            //pfile.SaveAs(filecontent);

                            ImageBuilder.Current.Build(new ImageJob(pfile, filecontent, new Instructions(), false, true));

                            //Generate each version
                            foreach (string suffix in versions.Keys)
                            {
                                if (suffix == "_thumb")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Thumbs");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, pt.ProductName + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, pt.ProductName + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }
                            }
                        }
                        var temsave = _ProductService.Find(pt.ProductId);
                        temsave.ImageFileName = pt.ProductName + "_" + filename + temp2;
                        temsave.ImageFolderName = uploadfolder;
                        _ProductService.Update(temsave);
                        _unitOfWork.Save();
                    }

                    #endregion

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.UnitConversion).DocumentTypeId,
                        DocId = pt.ProductId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));     

                    return RedirectToAction("MaterialIndex", new { id = group.ProductTypeId }).Success("Data saved successfully");
                }

            }
            PrepareMaterialViewBag(pvm);
            ViewBag.ProductGroupList = new ProductGroupService(_unitOfWork).GetProductGroupListForItemType(pvm.ProductTypeId ?? 0);
            return View("CreateMaterial", pvm);
        }

        // GET: /ProductMaster/Edit/5

        public ActionResult EditMaterial(int id)
        {

            MaterialViewModel pt = _ProductService.GetMaterialProduct(id);

            ProductGroup group = new ProductGroupService(_unitOfWork).Find(pt.ProductGroupId);
            ProductType Type = new ProductTypeService(_unitOfWork).Find(group.ProductTypeId);

            ViewBag.Name = Type.ProductTypeName;
            ViewBag.id = group.ProductTypeId;
            ViewBag.ProductGroupList = new ProductGroupService(_unitOfWork).GetProductGroupListForItemType(group.ProductTypeId);

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            pt.DivisionId = DivisionId;
            pt.SiteId = SiteId;
            pt.ProductTypeId = Type.ProductTypeId;
            ProductSiteDetail psd = new ProductSiteDetailService(_unitOfWork).FindforSite(SiteId, DivisionId, pt.ProductId);
            if (psd != null)
            {
                pt.ProductSiteDetailId = psd.ProductSiteDetailId;
                pt.MinimumOrderQty = psd.MinimumOrderQty;
                pt.ReOrderLevel = psd.ReOrderLevel;
                pt.GodownId = psd.GodownId;
                pt.BinLocationId = psd.BinLocationId;
                pt.LotManagement = psd.LotManagement;
            }

            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(Type.ProductTypeId);
            pt.ProductTypeSettings = Mapper.Map<ProductTypeSettings, ProductTypeSettingsViewModel>(settings);


            List<ProductTypeAttributeViewModel> tem = new ProductTypeAttributeService(_unitOfWork).GetAttributeForProduct(id).ToList();
            pt.ProductTypeAttributes = tem;

            if (pt == null)
            {
                return HttpNotFound();
            }
            PrepareMaterialViewBag(pt);
            return View("CreateMaterial", pt);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult DeleteMaterial(int id)
        {
            ProductViewModel Product = _ProductService.GetProduct(id);
            if (Product == null)
            {
                return HttpNotFound();
            }

            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMaterialConfirmed(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {
                var temp = _ProductService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });


                List<ProductSiteDetail> psd = new ProductSiteDetailService(_unitOfWork).GetSiteDetailForProduct(vm.id).ToList();

                foreach (ProductSiteDetail item in psd)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });
                    new ProductSiteDetailService(_unitOfWork).Delete(item.ProductSiteDetailId);
                }

                IEnumerable<ProductAttributes> Pa = new ProductAttributeService(_unitOfWork).GetProductAttributesWithPid(vm.id);

                foreach (ProductAttributes item in Pa)
                {
                    new ProductAttributeService(_unitOfWork).Delete(item.ProductAttributeId);
                }


                IEnumerable<BomDetail> Bd = new BomDetailService(_unitOfWork).GetBomDetailList(vm.id);

                foreach (BomDetail item in Bd)
                {
                    new BomDetailService(_unitOfWork).Delete(item.BomDetailId);
                }

                _ProductService.Delete(vm.id);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product).DocumentTypeId,
                    DocId = vm.id,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                }));               

                //Deleting Existing Images

                var xtemp = System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + temp.ImageFolderName + "/" + temp.ImageFileName);
                if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + temp.ImageFolderName + "/" + temp.ImageFileName)))
                {
                    System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + temp.ImageFolderName + "/" + temp.ImageFileName));
                }

                //Deleting Thumbnail Image:

                if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + temp.ImageFolderName + "/Thumbs/" + temp.ImageFileName)))
                {
                    System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + temp.ImageFolderName + "/Thumbs/" + temp.ImageFileName));
                }

                //Deleting Medium Image:
                if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + temp.ImageFolderName + "/Medium/" + temp.ImageFileName)))
                {
                    System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + temp.ImageFolderName + "/Medium/" + temp.ImageFileName));
                }

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }


        [HttpGet]
        public ActionResult NextPageMaterial(int id, int nid)//CurrentHeaderId,NatureId
        {
            var nextId = _ProductService.NextMaterialId(id, nid);
            return RedirectToAction("EditMaterial", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPageMaterial(int id, int nid)//CurrentHeaderId,NatureId
        {
            var nextId = _ProductService.PrevMaterialId(id, nid);
            return RedirectToAction("EditMaterial", new { id = nextId });
        }

        public ActionResult ChooseType(int id)
        {
            ViewBag.id = id;

            //var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(id);

            //if (settings == null && UserRoles.Contains("SysAdmin"))
            //{
            //    return RedirectToAction("Create", "ProductTypeSettings", new { id = id }).Warning("Please create Product Type Settings");
            //}
            //else if (settings == null && !UserRoles.Contains("SysAdmin"))
            //{
            //    return View("~/Views/Shared/InValidSettings.cshtml");
            //}

            return PartialView("ChooseType");
        }
        [HttpGet]
        public ActionResult CopyFromExisting(int id)
        {
            ViewBag.id = id;

            CopyFromExistingProductViewModel vm = new CopyFromExistingProductViewModel();
            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(id);
            vm.ProductTypeSettings = Mapper.Map<ProductTypeSettings, ProductTypeSettingsViewModel>(settings);

            return PartialView("CopyFromExisting", vm);
        }

        [HttpGet]
        public ActionResult CopyFromExistingConsumption(int ProductId)
        {
            //ViewBag.id = id;

            int ProductTypeId = (from P in db.Product where P.ProductId == ProductId select new { ProductTypeId = P.ProductGroup.ProductTypeId }).FirstOrDefault().ProductTypeId;
            CopyFromExistingProductViewModel vm = new CopyFromExistingProductViewModel();
            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(ProductTypeId);
            vm.ProductTypeSettings = Mapper.Map<ProductTypeSettings, ProductTypeSettingsViewModel>(settings);
            vm.ProductId = ProductId;

            return PartialView("CopyFromExisting", vm);
        }

        [HttpPost]
        public ActionResult CopyFromExisting(CopyFromExistingProductViewModel vm)
        {

            if (ModelState.IsValid)
            {
                Product FromProduct = _ProductService.Find(vm.FromProductId);
                Product NewProduct = new FinishedProduct();

                if (vm.ProductId == null)
                {
                    if (vm.ProductCode == "" || vm.ProductCode == null)
                    {
                        if (vm.ProductName.Length > 20)
                        {
                            NewProduct.ProductCode = vm.ProductName.ToString().Substring(0, 20);
                        }
                        else
                        {
                            NewProduct.ProductCode = vm.ProductName.ToString().Substring(0, vm.ProductName.Length);
                        }
                    }
                    else
                    {
                        NewProduct.ProductCode = vm.ProductCode;
                    }


                    NewProduct.ProductName = vm.ProductName;
                    NewProduct.ProductDescription = vm.ProductName;
                    NewProduct.ProductGroupId = FromProduct.ProductGroupId;
                    NewProduct.ProductCategoryId = FromProduct.ProductCategoryId;
                    NewProduct.ProductSpecification = FromProduct.ProductSpecification;
                    NewProduct.StandardCost = FromProduct.StandardCost;
                    NewProduct.SaleRate = FromProduct.SaleRate;
                    NewProduct.SalesTaxGroupProductId = FromProduct.SalesTaxGroupProductId;
                    NewProduct.UnitId = FromProduct.UnitId;
                    NewProduct.DivisionId = FromProduct.DivisionId;
                    NewProduct.IsActive = true;
                    NewProduct.CreatedDate = DateTime.Now;
                    NewProduct.ModifiedDate = DateTime.Now;
                    NewProduct.CreatedBy = User.Identity.Name;
                    NewProduct.ModifiedBy = User.Identity.Name;
                    NewProduct.ObjectState = Model.ObjectState.Added;
                    _ProductService.Create(NewProduct);
                }
                else
                {
                    NewProduct = _ProductService.Find((int)vm.ProductId);
                }



                IEnumerable<BomDetail> BomDetailList = new BomDetailService(_unitOfWork).GetBomDetailList(FromProduct.ProductId);

                foreach (BomDetail item in BomDetailList)
                {
                    BomDetail BomDetail = new BomDetail();
                    BomDetail.BaseProductId = NewProduct.ProductId;
                    BomDetail.BaseProcessId = item.BaseProcessId;
                    BomDetail.BatchQty = item.BatchQty;
                    BomDetail.ConsumptionPer = item.ConsumptionPer;

                    if (vm.BomProductId != 0 && vm.BomProductId != null && vm.ReplacingBomProductId != 0 && vm.ReplacingBomProductId != null && vm.BomProductId == item.ProductId)
                    {
                        BomDetail.ProductId = (int)vm.ReplacingBomProductId;
                    }
                    else
                    {
                        BomDetail.ProductId = item.ProductId;
                    }

                    if (vm.BomDimension1Id != 0 && vm.BomDimension1Id != null && vm.ReplacingBomDimension1Id != 0 && vm.ReplacingBomDimension1Id != null && vm.BomDimension1Id == item.Dimension1Id)
                    {
                        BomDetail.Dimension1Id = (int)vm.ReplacingBomDimension1Id;
                    }
                    else
                    {
                        BomDetail.Dimension1Id = item.Dimension1Id;
                    }

                    if (vm.BomDimension2Id != 0 && vm.BomDimension2Id != null && vm.ReplacingBomDimension2Id != 0 && vm.ReplacingBomDimension2Id != null && vm.BomDimension2Id == item.Dimension2Id)
                    {
                        BomDetail.Dimension2Id = (int)vm.ReplacingBomDimension2Id;
                    }
                    else
                    {
                        BomDetail.Dimension2Id = item.Dimension2Id;
                    }

                    if (vm.BomDimension3Id != 0 && vm.BomDimension3Id != null && vm.ReplacingBomDimension3Id != 0 && vm.ReplacingBomDimension3Id != null && vm.BomDimension3Id == item.Dimension3Id)
                    {
                        BomDetail.Dimension3Id = (int)vm.ReplacingBomDimension3Id;
                    }
                    else
                    {
                        BomDetail.Dimension3Id = item.Dimension3Id;
                    }

                    if (vm.BomDimension4Id != 0 && vm.BomDimension4Id != null && vm.ReplacingBomDimension4Id != 0 && vm.ReplacingBomDimension4Id != null && vm.BomDimension4Id == item.Dimension4Id)
                    {
                        BomDetail.Dimension4Id = (int)vm.ReplacingBomDimension4Id;
                    }
                    else
                    {
                        BomDetail.Dimension4Id = item.Dimension4Id;
                    }

                    BomDetail.ProcessId = item.ProcessId;
                    BomDetail.Qty = item.Qty;
                    BomDetail.MBQ = item.MBQ;
                    BomDetail.StdCost = item.StdCost;
                    BomDetail.StdTime = item.StdTime;
                    BomDetail.CreatedDate = DateTime.Now;
                    BomDetail.ModifiedDate = DateTime.Now;
                    BomDetail.CreatedBy = User.Identity.Name;
                    BomDetail.ModifiedBy = User.Identity.Name;
                    BomDetail.ObjectState = Model.ObjectState.Added;
                    new BomDetailService(_unitOfWork).Create(BomDetail);
                }


                List<ProductTypeAttributeViewModel> pa = new ProductTypeAttributeService(_unitOfWork).GetAttributeForProduct(vm.FromProductId).ToList();

                foreach (var attribute in pa)
                {
                    ProductAttributes prodattr = new ProductAttributes()
                    {
                        ProductAttributeValue = attribute.DefaultValue,
                        ProductId = NewProduct.ProductId,
                        ProductTypeAttributeId = attribute.ProductTypeAttributeId,
                        CreatedBy = User.Identity.Name,
                        ModifiedBy = User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now

                    };
                    prodattr.ObjectState = Model.ObjectState.Added;
                    new ProductAttributeService(_unitOfWork).Create(prodattr);
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

                return Json(new { success = true, Url = "/Product/EditMaterial/" + NewProduct.ProductId });


            }

            return PartialView("CopyFromExisting", vm);

        }

        //public ActionResult GeneratePrints(string Ids, int DocTypeId)
        //{

        //    if (!string.IsNullOrEmpty(Ids))
        //    {
               

        //        try
        //        {

        //            List<byte[]> PdfStream = new List<byte[]>();
        //            foreach (var item in Ids.Split(',').Select(Int32.Parse))
        //            {

        //                DirectReportPrint drp = new DirectReportPrint();
                        
        //                var pd = db.Product.Find(item);

        //                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
        //                {
        //                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product).DocumentTypeId,
        //                    DocId = pd.ProductId,
        //                    ActivityType = (int)ActivityTypeContants.Print,
        //                }));

        //                byte[] Pdf;

        //                Pdf = drp.DirectDocumentPrint("Web.spRep_FinishedProductConsumption_HomeFurnishing", User.Identity.Name, item);
        //                PdfStream.Add(Pdf);

        //                //if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Modified || pd.Status == (int)StatusConstants.Import)
        //                //{
        //                //    //LogAct(item.ToString());
        //                //    Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

        //                //    PdfStream.Add(Pdf);
        //                //}
        //                //else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
        //                //{
        //                //    Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

        //                //    PdfStream.Add(Pdf);
        //                //}
        //                //else
        //                //{
        //                //    Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);
        //                //    PdfStream.Add(Pdf);
        //                //}

        //            }

        //            PdfMerger pm = new PdfMerger();

        //            byte[] Merge = pm.MergeFiles(PdfStream);

        //            if (Merge != null)
        //                return File(Merge, "application/pdf");
        //        }

        //        catch (Exception ex)
        //        {
        //            string message = _exception.HandleException(ex);
        //            return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
        //        }



        //        return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet);

        //    }
        //    return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

        //}


        public ActionResult Import(int id)//Document Type Id
        {
            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(id);

            if (settings != null)
            {
                if (settings.ImportMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.ImportMenuId);

                    if (menuviewmodel == null)
                    {
                        return View("~/Views/Shared/UnderImplementation.cshtml");
                    }
                    else if (!string.IsNullOrEmpty(menuviewmodel.URL))
                    {
                        return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + id + "?MenuId=" + menuviewmodel.MenuId);
                    }
                    else
                    {
                        return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = id });
                    }
                }
            }
            return RedirectToAction("Index", new { id = id });
        }

        public JsonResult GetProductGroupDetailJson(int ProductGroupId)
        {
            var ProductGroupDetail = (from Pg in db.ProductGroups
                                      where Pg.ProductGroupId == ProductGroupId
                                      select new
                                      {
                                          DefaultSalesTaxProductCodeId = Pg.DefaultSalesTaxProductCodeId,
                                          DefaultSalesTaxProductCodeName = Pg.DefaultSalesTaxProductCode.Code
                                      }).FirstOrDefault();

            return Json(ProductGroupDetail);
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
