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
using System.Data.SqlClient;
using Model.ViewModel;
using AutoMapper;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class CarpetMasterController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        List<string> UserRoles = new List<string>();
        IProductService _ProductService;
        IFinishedProductService _FinishedProductService;
        IProductGroupService _ProductGroupService;
        IProductSizeService _ProductSizeService;
        IDimension2Service _Dimension2Service;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public CarpetMasterController(IProductService CarpetMasterService, IFinishedProductService FinishedProductService, IUnitOfWork unitOfWork, IProductGroupService group, IProductSizeService size, IExceptionHandlingService exec)
        {
            _ProductService = CarpetMasterService;
            _FinishedProductService = FinishedProductService;
            _unitOfWork = unitOfWork;
            _ProductGroupService = group;
            _ProductSizeService = size;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
        }

        public void PrepareDivisionViewBag()
        {
            ViewBag.DivisionList = new DivisionService(_unitOfWork).GetDivisionList().ToList();
        }

        public ActionResult ChooseType(bool sample)
        {
            @ViewBag.Sample = sample;
            return PartialView("ChooseType");
        }
        [HttpGet]
        public ActionResult CopyFromExisting()
        {
            return PartialView("CopyFromExisting");
        }


        [HttpPost]
        public ActionResult CopyFromExisting(CopyFromExistingDesignViewModel vm)
        {
            bool IsSample = false;
            if (ModelState.IsValid)
            {
                ProductGroup oldProductGroup = new ProductGroupService(_unitOfWork).Find(vm.ProductGroupId);
                string oldcode = oldProductGroup.ProductGroupName;
                string ocode = oldcode.Replace("-", "");

                ProductGroup group = new ProductGroup();
                group.ProductGroupName = vm.ProductGroupName;
                group.ProductTypeId = oldProductGroup.ProductTypeId;
                group.ImageFileName = oldProductGroup.ImageFileName;
                group.ImageFolderName = oldProductGroup.ImageFolderName;
                group.CreatedBy = User.Identity.Name;
                group.CreatedDate = DateTime.Now;
                group.ModifiedBy = User.Identity.Name;
                group.ModifiedDate = DateTime.Now;
                group.IsActive = true;
                group.ObjectState = Model.ObjectState.Added;
                _ProductGroupService.Create(group);

                Dimension2Service D2 = new Dimension2Service(_unitOfWork);
                //D2.CreateByName(1, vm.ProductGroupName, group.ProductTypeId);

                Dimension2 Dim2 = new Dimension2();
                Dim2.Description = group.ProductGroupId.ToString();
                Dim2.Dimension2Name = group.ProductGroupName;
                Dim2.ProductTypeId = group.ProductTypeId;
                Dim2.IsSystemDefine = true;
                Dim2.IsActive = true;
                Dim2.CreatedBy = User.Identity.Name;
                Dim2.CreatedDate = DateTime.Now;
                Dim2.ModifiedBy = User.Identity.Name;
                Dim2.ModifiedDate = DateTime.Now;
                Dim2.ObjectState = Model.ObjectState.Added;
                D2.Create(Dim2);


                List<FinishedProduct> list = new FinishedProductService(_unitOfWork).GetFinishedProductForGroup(vm.ProductGroupId).ToList();
                int i = 0;
                int j = 0;
                foreach (FinishedProduct item in list)
                {
                    i++;
                    string productcode = item.ProductName;
                    string tes = group.ProductGroupName;
                    string ntes = tes.Replace("-", "");
                    string newcode = productcode.Replace(ocode, ntes);
                    //string tem=newcode.Replace("-", "");


                    FinishedProduct fp = new FinishedProduct();
                    fp.ProductId = i;
                    fp.ProductName = newcode;
                    fp.ProductCode = fp.ProductName;

                    fp.ProductGroupId = group.ProductGroupId;
                    fp.IsActive = item.IsActive;
                    fp.ProductCategoryId = item.ProductCategoryId;
                    fp.ProductCollectionId = item.ProductCollectionId;
                    fp.ProductQualityId = item.ProductQualityId;
                    fp.ProductDesignId = item.ProductDesignId;
                    fp.ProductDesignPatternId = item.ProductDesignPatternId;
                    fp.OriginCountryId = item.OriginCountryId;
                    fp.ProductStyleId = item.ProductStyleId;
                    fp.ProductInvoiceGroupId = item.ProductInvoiceGroupId;
                    fp.ColourId = item.ColourId;
                    fp.UnitId = item.UnitId;
                    fp.ProductionRemark = item.ProductionRemark;
                    fp.DiscontinuedDate = item.DiscontinuedDate;
                    fp.DrawBackTariffHeadId = item.DrawBackTariffHeadId;
                    fp.ProductManufacturerId = item.ProductManufacturerId;
                    fp.ProcessSequenceHeaderId = item.ProcessSequenceHeaderId;
                    fp.DescriptionOfGoodsId = item.DescriptionOfGoodsId;
                    fp.SalesTaxProductCodeId = item.SalesTaxProductCodeId;
                    fp.ContentId = item.ContentId;
                    fp.FaceContentId = item.FaceContentId;
                    fp.SalesTaxGroupProductId = item.SalesTaxGroupProductId;
                    fp.ProductDescription = fp.ProductName;
                    fp.ProductSpecification = item.ProductSpecification;
                    fp.StandardCost = item.StandardCost;
                    fp.StandardWeight = item.StandardWeight;
                    fp.GrossWeight = item.GrossWeight;
                    //fp.ImageFileName = item.ImageFileName;
                    //fp.ImageFolderName = item.ImageFolderName;
                    fp.ProductShapeId = item.ProductShapeId;
                    fp.SampleId = item.SampleId;
                    fp.CounterNo = item.CounterNo;
                    fp.Tags = item.Tags;
                    fp.CreatedBy = User.Identity.Name;
                    fp.CreatedDate = DateTime.Now;
                    fp.ModifiedBy = User.Identity.Name;
                    fp.ModifiedDate = DateTime.Now;
                    fp.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    fp.IsSample = item.IsSample;
                    IsSample = fp.IsSample;
                    fp.TraceType = item.TraceType;
                    fp.MapType = item.MapType;
                    fp.MapScale = item.MapScale;
                    fp.ObjectState = Model.ObjectState.Added;
                    _FinishedProductService.Create(fp);



                    List<ProductSizeViewModel> Sizes = new ProductSizeService(_unitOfWork).GetProductSizeForProduct(item.ProductId).ToList();

                    foreach (ProductSizeViewModel siz in Sizes)
                    {
                        ProductSize ssize = new ProductSize();
                        ssize.ProductSizeTypeId = siz.ProductSizeTypeId;
                        ssize.SizeId = siz.SizeId;
                        ssize.ProductId = fp.ProductId;
                        ssize.CreatedBy = User.Identity.Name;
                        ssize.CreatedDate = DateTime.Now;
                        ssize.ModifiedBy = User.Identity.Name;
                        ssize.ModifiedDate = DateTime.Now;
                        ssize.IsActive = true;
                        ssize.ObjectState = Model.ObjectState.Added;
                        _ProductSizeService.Create(ssize);
                    }



                    IEnumerable<UnitConversionViewModel> UnitConversionList = new UnitConversionService(_unitOfWork).GetProductUnitConversions(item.ProductId);

                    foreach (UnitConversionViewModel Uc in UnitConversionList)
                    {
                        UnitConversion UnitConversion = new UnitConversion();
                        UnitConversion.ProductId = fp.ProductId;
                        UnitConversion.FromQty = Uc.FromQty;
                        UnitConversion.FromUnitId = Uc.FromUnitId;
                        UnitConversion.ToQty = Uc.ToQty;
                        UnitConversion.ToUnitId = Uc.ToUnitId;
                        UnitConversion.UnitConversionForId = Uc.UnitConversionForId;
                        UnitConversion.Description = Uc.Description;
                        UnitConversion.CreatedBy = User.Identity.Name;
                        UnitConversion.CreatedDate = DateTime.Now;
                        UnitConversion.ModifiedBy = User.Identity.Name;
                        UnitConversion.ModifiedDate = DateTime.Now;
                        UnitConversion.ObjectState = Model.ObjectState.Added;
                        new UnitConversionService(_unitOfWork).Create(UnitConversion);
                    }



                    List<ProductTypeAttributeViewModel> pa = new ProductTypeAttributeService(_unitOfWork).GetAttributeForProduct(item.ProductId).ToList();

                    foreach (var attribute in pa)
                    {
                        ProductAttributes prodattr = new ProductAttributes()
                        {
                            ProductAttributeValue = attribute.DefaultValue,
                            ProductId = fp.ProductId,
                            ProductTypeAttributeId = attribute.ProductTypeAttributeId,
                            CreatedBy = User.Identity.Name,
                            ModifiedBy = User.Identity.Name,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now

                        };
                        prodattr.ObjectState = Model.ObjectState.Added;
                        new ProductAttributeService(_unitOfWork).Create(prodattr);
                    }


                    var Prod = (from P in db.ViewRugSize where P.ProductId == item.ProductId select P).FirstOrDefault();
                    if (fp.TraceType != "N/A")
                    {
                        //int ProductDesignNAId = new ProductDesignService(_unitOfWork).Find("NA").ProductDesignId;

                        //if (fp.ProductDesignId != null && fp.ProductDesignId != ProductDesignNAId)
                        //{
                        //    j--;
                        //    CreateTrace((int)fp.ProductDesignId, (int)fp.ProductGroupId, (int)Prod.StandardSizeID, (int)Prod.StencilSizeId,fp.ProductName, j);
                        //}
                        //else
                        //{
                        //    CreateTraceForProduct(fp.ProductName, (int)Prod.StencilSizeId, j);
                        //}
                    }
                    if (fp.MapType != "N/A")
                    {
                        j--;
                        CreateMap(fp.ProductName, (int)Prod.MapSizeId, j);
                    }


                    ////ForInsertingProductProcess

                    var ProductProcessList = (from P in db.ProductProcess
                                         where P.ProductId == item.ProductId
                                         select P).ToList();

                    foreach (var ProductProcess in ProductProcessList)
                    {
                        ProductProcess ProdProc = new ProductProcess()
                        {
                            CreatedBy = User.Identity.Name,
                            CreatedDate = DateTime.Now,
                            ModifiedBy = User.Identity.Name,
                            ModifiedDate = DateTime.Now,
                            ProcessId = ProductProcess.ProcessId,
                            QAGroupId = ProductProcess.QAGroupId,
                            ProductRateGroupId = ProductProcess.ProductRateGroupId,
                            Sr = ProductProcess.Sr,
                            ProductId = fp.ProductId,
                        };
                        ProdProc.ObjectState = Model.ObjectState.Added;
                        new ProductProcessService(_unitOfWork).Create(ProdProc);
                    }


                    //if (fp.ProcessSequenceHeaderId.HasValue && fp.ProcessSequenceHeaderId.Value > 0)
                    //{

                    //    //var ProcessSeqLines = (from p in db.ProcessSequenceLine
                    //    //                       where p.ProcessSequenceHeaderId == fp.ProcessSequenceHeaderId
                    //    //                       select p).ToList();

                    //    IEnumerable<ProductProcessViewModel> ProcessSeqLines = new ProcessSequenceHeaderService(_unitOfWork).FGetProductProcessFromProcessSequence((int)fp.ProcessSequenceHeaderId);

                    //    foreach (var ProcSeqLin in ProcessSeqLines)
                    //    {

                    //        ProductProcess ProdProc = new ProductProcess()
                    //        {
                    //            CreatedBy = User.Identity.Name,
                    //            CreatedDate = DateTime.Now,
                    //            ModifiedBy = User.Identity.Name,
                    //            ModifiedDate = DateTime.Now,
                    //            Sr = ProcSeqLin.Sr,
                    //            ProductId = fp.ProductId,
                    //        };
                    //        ProdProc.ObjectState = Model.ObjectState.Added;
                    //        new ProductProcessService(_unitOfWork).Create(ProdProc);
                    //    }
                    //}
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

                Dim2.Description = group.ProductGroupId.ToString();
                D2.Update(Dim2);

                _unitOfWork.Save();




                //Copying Existing Images
                string filename = System.Guid.NewGuid().ToString();
                string uploadfolder = group.ImageFolderName;
                string tempfilename = group.ImageFileName;

                if (!string.IsNullOrEmpty(uploadfolder) && string.IsNullOrEmpty(tempfilename))
                {
                    var xtemp = System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/" + tempfilename);
                    if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/" + tempfilename)))
                    {
                        string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/");
                        System.IO.File.Copy(Path.Combine(tuploadFolder, tempfilename), Path.Combine(tuploadFolder, group.ProductGroupName + "_" + filename + tempfilename.Substring(tempfilename.LastIndexOf('.'))));
                    }

                    //Deleting Thumbnail Image:

                    if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Thumbs/" + tempfilename)))
                    {
                        string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Thumbs");
                        System.IO.File.Copy(Path.Combine(tuploadFolder, tempfilename), Path.Combine(tuploadFolder, group.ProductGroupName + "_" + filename + tempfilename.Substring(tempfilename.LastIndexOf('.'))));
                    }

                    //Deleting Medium Image:
                    if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Medium/" + tempfilename)))
                    {
                        string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                        System.IO.File.Copy(Path.Combine(tuploadFolder, tempfilename), Path.Combine(tuploadFolder, group.ProductGroupName + "_" + filename + tempfilename.Substring(tempfilename.LastIndexOf('.'))));
                    }

                    group.ImageFileName = group.ProductGroupName + "_" + filename + tempfilename.Substring(tempfilename.LastIndexOf('.'));
                    group.ImageFolderName = uploadfolder;
                    group.ObjectState = Model.ObjectState.Modified;
                    _ProductGroupService.Update(group);

                    var Prodlist = _ProductService.GetProductListForGroup(group.ProductGroupId);
                    foreach (var Prod in Prodlist)
                    {
                        Product prod = _ProductService.Find(Prod.ProductId);

                        prod.ImageFileName = group.ImageFileName;
                        prod.ImageFolderName = group.ImageFolderName;
                        prod.ObjectState = Model.ObjectState.Modified;
                        _ProductService.Update(prod);
                    }

                    _unitOfWork.Save();

                }

                return Json(new { success = true, Url = "/CarpetMaster/Edit?id=" + group.ProductGroupId + "&sample=" + IsSample });
            }

            return PartialView("CopyFromExisting", vm);

        }

        [HttpGet]
        public ActionResult Report()
        {

            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.CarpetDesign);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);


        }

        [HttpGet]
        public ActionResult Index(int id)
        {
            bool sample = ((id == 0) ? false : true);
            ViewBag.Sample = sample;
            var temp = _ProductGroupService.GetCarpetListForIndex(sample);
            return View(temp);
        }
        [HttpGet]
        public ActionResult Create(bool sample)
        {
            PrepareDivisionViewBag();
            CarpetMasterViewModel vm = new CarpetMasterViewModel();
            vm.IsSample = sample;
            ViewBag.Sample = sample;
            ViewBag.SampleId = ((sample == false) ? 0 : 1);
            int producttypeid = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;
            List<ProductTypeAttributeViewModel> tem = new ProductTypeAttributeService(_unitOfWork).GetAttributeVMForProductType(producttypeid).ToList();
            vm.ProductTypeAttributes = tem;
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.IsActive = true;
            CarpetSkuSettings settings = new CarpetSkuSettingsService(_unitOfWork).GetCarpetSkuSettings(vm.DivisionId, vm.SiteId);


            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "CarpetSkuSettings").Warning("Please create Carpet Sku settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            vm.CarpetSkuSettings = Mapper.Map<CarpetSkuSettings, CarpetSkuSettingsViewModel>(settings);
            //For Setting Default Values because these fields are mandatory
            vm.ProductDesignId = settings.ProductDesignId;
            vm.OriginCountryId = settings.OriginCountryId;
            PrepareViewBag(null);
            return View(vm);
        }

        //public ActionResult TempRedirect(CarpetMasterViewModel vm)
        //{
        //    return View("Create", vm).Success("Data Saved Successfully");
        //}

        [HttpGet]
        public ActionResult Edit(int id, bool sample)//ProductGroupId
        {
            var temp = _ProductService.GetProductListForGroup(id);
            var productGroup = _ProductGroupService.Find(id);
            var product = temp.FirstOrDefault();


            if (product != null)
            {
                var productInfo = _FinishedProductService.GetFinishedProduct(product.ProductId);
                CarpetMasterViewModel vm = new CarpetMasterViewModel()
                {
                    DivisionId = productInfo.DivisionId,
                    DrawBackTariffHeadId = productInfo.DrawBackTariffHeadId,
                    ProcessSequenceHeaderId = productInfo.ProcessSequenceHeaderId,
                    ProductionRemark = productInfo.ProductionRemark,
                    ProductCategoryId = productInfo.ProductCategoryId ?? 0,
                    ProductCollectionId = productInfo.ProductCollectionId,
                    ProductGroupName = productGroup.ProductGroupName,
                    ProductGroupId = id,
                    ProductInvoiceGroupId = productInfo.ProductInvoiceGroupId,
                    ProductManufacturerId = productInfo.ProductManufacturerId,
                    ProductQualityId = productInfo.ProductQualityId,
                    ProductStyleId = productInfo.ProductStyleId,
                    ProductDesignId = productInfo.ProductDesignId,
                    IsActive = productInfo.IsActive,
                    ProductDesignPatternId = productInfo.ProductDesignPatternId,
                    ColourId = productInfo.ColourId ?? 0,
                    DescriptionOfGoodsId = productInfo.DescriptionOfGoodsId,
                    SalesTaxProductCodeId = productInfo.SalesTaxProductCodeId,
                    ContentId = productInfo.ContentId,
                    FaceContentId = productInfo.FaceContentId,
                    SampleId = productInfo.SampleId,
                    CounterNo = productInfo.CounterNo,
                    Tags = productInfo.Tags,
                    StandardCost = productInfo.StandardCost,
                    StandardWeight = productInfo.StandardWeight,
                    GrossWeight = productInfo.GrossWeight,
                    OriginCountryId = productInfo.OriginCountryId,
                    ImageFileName = productGroup.ImageFileName,
                    ImageFolderName = productGroup.ImageFolderName,
                    UnitId = productInfo.UnitId,
                    IsSample = productInfo.IsSample,
                    TraceType = productInfo.TraceType,
                    MapType = productInfo.MapType,

                };

                //Setting ProductSupplier Details
                var ProductSupplier = new ProductSupplierService(_unitOfWork).GetDefaultSupplier(product.ProductId);

                if (ProductSupplier != null)
                {
                    vm.ProductSupplierId = ProductSupplier.ProductSupplierId;
                    vm.SupplierId = ProductSupplier.SupplierId;
                    vm.LeadTime = ProductSupplier.LeadTime;
                    vm.MinimumOrderQty = ProductSupplier.MinimumOrderQty;
                    vm.MaximumOrderQty = ProductSupplier.MaximumOrderQty;
                    vm.Cost = ProductSupplier.Cost;
                }

                ViewBag.Sample = vm.IsSample;
                ViewBag.SampleId = ((vm.IsSample == false) ? 0 : 1);

                //vm.ProductTypeAttributes 
                vm.ProductTypeAttributes = new ProductTypeAttributeService(_unitOfWork).GetAttributeForProduct(product.ProductId).ToList();
                System.Web.HttpContext.Current.Session["list"] = vm.ProductTypeAttributes.ToList();
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                CarpetSkuSettings Stng = new CarpetSkuSettingsService(_unitOfWork).GetCarpetSkuSettings(vm.DivisionId, SiteId);
                vm.CarpetSkuSettings = Mapper.Map<CarpetSkuSettings, CarpetSkuSettingsViewModel>(Stng);
                PrepareDivisionViewBag();
                PrepareViewBag(vm);
                return View("Create", vm);
            }
            else
            {
                CarpetMasterViewModel vm = new CarpetMasterViewModel()
                {
                    ProductGroupName = productGroup.ProductGroupName,
                    ProductGroupId = id,
                    IsActive = productGroup.IsActive,
                    ImageFileName = productGroup.ImageFileName,
                    ImageFolderName = productGroup.ImageFolderName,
                    DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"],
                };
                ViewBag.Sample = sample;
                ViewBag.SampleId = ((sample == false) ? 0 : 1);
                vm.IsSample = sample;
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                CarpetSkuSettings Stng = new CarpetSkuSettingsService(_unitOfWork).GetCarpetSkuSettings(vm.DivisionId, SiteId);
                vm.CarpetSkuSettings = Mapper.Map<CarpetSkuSettings, CarpetSkuSettingsViewModel>(Stng);
                PrepareViewBag(vm);
                PrepareDivisionViewBag();
                int producttypeid = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;
                List<ProductTypeAttributeViewModel> tem = new ProductTypeAttributeService(_unitOfWork).GetAttributeVMForProductType(producttypeid).ToList();
                vm.ProductTypeAttributes = tem;
                return View("Create", vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CarpetMasterViewModel vm, FormCollection collection)
        {
            ViewBag.Sample = vm.IsSample;
            int sampleId = ((vm.IsSample == false) ? 0 : 1);
            ViewBag.SampleId = sampleId;
            //string Loss = collection["Losss"].ToString();
            //string PileHeight = collection["PileHeights"].ToString();
            if (ModelState.IsValid)
            {
                //Checking if new Product i.e. Create or Edit
                if (vm.ProductGroupId <= 0)
                {

                    ProductGroup group = new ProductGroup();
                    group.ProductGroupName = vm.ProductGroupName;
                    group.ProductTypeId = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;
                    group.CreatedBy = User.Identity.Name;
                    group.CreatedDate = DateTime.Now;
                    group.ModifiedBy = User.Identity.Name;
                    group.ModifiedDate = DateTime.Now;
                    group.IsActive = true;
                    _ProductGroupService.Create(group);

                    Dimension2Service D2 = new Dimension2Service(_unitOfWork);
                    Dimension2 Dim2 = new Dimension2();
                    Dim2.Description = group.ProductGroupId.ToString();
                    Dim2.Dimension2Name = group.ProductGroupName;
                    Dim2.ProductTypeId = group.ProductTypeId;
                    Dim2.IsSystemDefine = true;
                    Dim2.IsActive = true;
                    Dim2.CreatedBy = User.Identity.Name;
                    Dim2.CreatedDate = DateTime.Now;
                    Dim2.ModifiedBy = User.Identity.Name;
                    Dim2.ModifiedDate = DateTime.Now;
                    Dim2.ObjectState = Model.ObjectState.Added;
                    D2.Create(Dim2);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        PrepareDivisionViewBag();
                        PrepareViewBag(vm);
                        ModelState.AddModelError("", message);
                        return View("Create", vm);
                    }

                    Dim2.Description = group.ProductGroupId.ToString();
                    D2.Update(Dim2);

                    _unitOfWork.Save();

                    #region-ImageSaveLogic

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

                            string filecontent = Path.Combine(uploadFolder, group.ProductGroupName + "_" + filename);

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
                                    string tfileName = Path.Combine(tuploadFolder, vm.ProductGroupName + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, vm.ProductGroupName + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }

                            }

                            //var tempsave = _FinishedProductService.Find(pt.ProductId);

                            group.ImageFileName = vm.ProductGroupName + "_" + filename + temp2;
                            group.ImageFolderName = uploadfolder;
                            group.ObjectState = Model.ObjectState.Modified;
                            _ProductGroupService.Update(group);
                            _unitOfWork.Save();
                        }

                    }

                    #endregion


                    vm.ProductGroupId = group.ProductGroupId;
                    vm.ImageFolderName = group.ImageFolderName;
                    vm.ImageFileName = group.ImageFileName;
                    List<ProductTypeAttributeViewModel> temp = vm.ProductTypeAttributes;
                    System.Web.HttpContext.Current.Session["list"] = temp;
                    PrepareDivisionViewBag();
                    PrepareViewBag(vm);
                    return View("Create", vm).Success("Data saved successfully");

                }

                    //Edit the Data
                else
                {
                    ProductGroup group = new ProductGroupService(_unitOfWork).Find(vm.ProductGroupId);

                    Dimension2 Dim2 = new Dimension2Service(_unitOfWork).Find(vm.ProductGroupName);

                    if (group.ProductGroupName != vm.ProductGroupName)
                    {
                        group.ProductGroupName = vm.ProductGroupName;
                        group.ModifiedBy = User.Identity.Name;
                        group.ModifiedDate = DateTime.Now;
                        group.ObjectState = Model.ObjectState.Modified;
                        new ProductGroupService(_unitOfWork).Update(group);

                        Dim2.Dimension2Name = vm.ProductGroupName;
                        Dim2.ModifiedBy = User.Identity.Name;
                        Dim2.ModifiedDate = DateTime.Now;
                        Dim2.ObjectState = Model.ObjectState.Modified;
                        new Dimension2Service(_unitOfWork).Update(Dim2);


                        ActivityLog grouplog = new ActivityLog()
                        {
                            ActivityType = (int)ActivityTypeContants.Modified,
                            CreatedBy = User.Identity.Name,
                            CreatedDate = DateTime.Now,
                            DocId = group.ProductGroupId,
                            Narration = "Group name modified from " + group.ProductGroupName + " to " + vm.ProductGroupName,
                        };
                        new ActivityLogService(_unitOfWork).Create(grouplog);
                    }


                    



                    var temp = _ProductService.GetProductListForGroup(vm.ProductGroupId);


                    if (temp.Count() > 0)
                    {

                        int i = 0;
                        int j = 0;

                        foreach (var item in temp)
                        {

                            FinishedProduct prod = _FinishedProductService.Find(item.ProductId);

                            prod.ProductCategoryId = vm.ProductCategoryId;
                            prod.ProductCollectionId = vm.ProductCollectionId;
                            prod.ProductQualityId = vm.ProductQualityId;
                            prod.ProductDesignId = vm.ProductDesignId;
                            prod.DivisionId = vm.DivisionId;
                            prod.ProductDesignPatternId = vm.ProductDesignPatternId;
                            prod.OriginCountryId = vm.OriginCountryId;

                            if (vm.CarpetSkuSettings.isVisibleColour == true)
                            {
                                prod.ColourId = vm.ColourId;
                            }

                            prod.ProductInvoiceGroupId = vm.ProductInvoiceGroupId;
                            prod.ProductStyleId = vm.ProductStyleId;
                            prod.ProductManufacturerId = vm.ProductManufacturerId;
                            prod.DrawBackTariffHeadId = vm.DrawBackTariffHeadId;
                            prod.ProcessSequenceHeaderId = vm.ProcessSequenceHeaderId;
                            prod.ProductionRemark = vm.ProductionRemark;
                            //prod.ProductShapeId = vm.ProductShapeId;

                            prod.DescriptionOfGoodsId = vm.DescriptionOfGoodsId;
                            prod.SalesTaxProductCodeId = vm.SalesTaxProductCodeId;
                            prod.ContentId = vm.ContentId;
                            prod.StandardCost = vm.StandardCost;
                            prod.FaceContentId = vm.FaceContentId;
                            prod.StandardWeight = vm.StandardWeight;



                            //if (prod.GrossWeight != vm.GrossWeight)
                            //{
                            //    if (vm.GrossWeight > prod.GrossWeight)
                            //    {
                            //        if (prod.ColourId != null)
                            //        {
                            //            Colour Colour = new ColourService(_unitOfWork).Find((int)prod.ColourId);
                            //            Product BomProduct = new ProductService(_unitOfWork).Find(group.ProductGroupName.ToString().Trim() + "-" + Colour.ColourName.ToString().Trim() + "-Bom");
                            //            if (BomProduct != null)
                            //            {
                            //                BomProduct.StandardWeight = vm.GrossWeight;
                            //                _ProductService.Update(BomProduct);
                            //            }
                            //        }
                            //    }
                            //    else if (vm.GrossWeight < prod.GrossWeight)
                            //    {
                            //        string message = "Consumption is filled for this design.You can not change pile weight.You have to delete conumption first.";
                            //        PrepareDivisionViewBag();
                            //        PrepareViewBag(vm);
                            //        ModelState.AddModelError("", message);
                            //        return View("Create", vm);
                            //    }
                            //}

                            
                            prod.GrossWeight = vm.GrossWeight;



                            prod.SampleId = vm.SampleId;
                            prod.CounterNo = vm.CounterNo;
                            prod.Tags = vm.Tags;
                            prod.TraceType = vm.TraceType;
                            prod.MapType = vm.MapType;

                            try
                            {
                                prod.ObjectState = Model.ObjectState.Modified;
                                _FinishedProductService.Update(prod);
                            }
                            catch (Exception ex)
                            {

                            }



                            if (vm.ProductTypeAttributes != null)
                            {
                                foreach (var pta in vm.ProductTypeAttributes)
                                {
                                    //ProductAttributes productattribute = null;
                                    //if (pta.ProductAttributeId != 0)
                                    //{
                                    //    productattribute = new ProductAttributeService(_unitOfWork).Find(prod.ProductId, pta.ProductTypeAttributeId);
                                    //}
                                    ProductAttributes productattribute = new ProductAttributeService(_unitOfWork).Find(prod.ProductId, pta.ProductTypeAttributeId);

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
                                            ProductId = prod.ProductId,
                                            ProductTypeAttributeId = pta.ProductTypeAttributeId,
                                            CreatedBy = User.Identity.Name,
                                            ModifiedBy = User.Identity.Name,
                                            CreatedDate = DateTime.Now,
                                            ModifiedDate = DateTime.Now

                                        };
                                        pa.ObjectState = Model.ObjectState.Added;
                                        new ProductAttributeService(_unitOfWork).Create(pa);
                                    }

                                    var p = (from V in db.ViewRugSize
                                             join S in db.Size on V.StandardSizeID equals S.SizeId into SizeTable
                                             from SizeTab in SizeTable.DefaultIfEmpty()
                                             where V.ProductId == item.ProductId
                                             select new
                                             {
                                                 ProductShapeId = SizeTab.ProductShapeId
                                             }).FirstOrDefault();

                                    int? ProductShapeId = 0;
                                    if (p != null)
                                    {
                                        ProductShapeId = p.ProductShapeId;
                                    }




                                    //////////////////////For Saving Binding, Gachhai and PattiMuraiDurry Unit Conversion Detail///////////////////////////////////

                                    int UnitConversionBinding = (int)UnitConversionFors.Binding;
                                    int UnitConversionGachhai = (int)UnitConversionFors.Gachhai;
                                    int UnitConversionPattiMuraiDurry = (int)UnitConversionFors.PattiMuraiDurry;

                                    if (pta.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Gachhai || pta.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Binding
                                        || pta.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.PattiMuraiDurry)
                                    {
                                        int StandardSizeId = new ProductSizeService(_unitOfWork).GetProductSizeIndexForProduct(prod.ProductId).StandardSizeId;
                                        int ManufacturingSizeId = new ProductSizeService(_unitOfWork).GetProductSizeIndexForProduct(prod.ProductId).ManufacturingSizeId;
                                        int FinishingSizeId = new ProductSizeService(_unitOfWork).GetProductSizeIndexForProduct(prod.ProductId).FinishingSizeId;
                                        Size FinishingSizeForPerimeter = new SizeService(_unitOfWork).Find(FinishingSizeId);
                                        decimal Length = Math.Floor((FinishingSizeForPerimeter.Length + (FinishingSizeForPerimeter.LengthFraction / 12)) * 2);
                                        decimal Width = Math.Floor((FinishingSizeForPerimeter.Width + (FinishingSizeForPerimeter.WidthFraction / 12)) * 2);
                                        decimal LenghtAndWidth = Math.Floor(((FinishingSizeForPerimeter.Length + (FinishingSizeForPerimeter.LengthFraction / 12)) * 2) + ((FinishingSizeForPerimeter.Width + (FinishingSizeForPerimeter.WidthFraction / 12)) * 2));

                                        UnitConversion UnitConv = new UnitConversion();
                                        UnitConv.CreatedBy = User.Identity.Name;
                                        UnitConv.CreatedDate = DateTime.Now;
                                        UnitConv.ModifiedBy = User.Identity.Name;
                                        UnitConv.ModifiedDate = DateTime.Now;
                                        UnitConv.ProductId = prod.ProductId;
                                        UnitConv.FromQty = 1;
                                        UnitConv.FromUnitId = UnitConstants.Pieces;
                                        UnitConv.ToUnitId = UnitConstants.Feet;


                                        //To Enable User to create custom logic to get Unit Conversion these section is commented and it is generating from Sql Procedure

                                        //if (ProductShapeId == (int)ProductShapeConstants.Circle)
                                        //{
                                        //    UnitConv.ToQty = Math.Floor((FinishingSizeForPerimeter.Length + (FinishingSizeForPerimeter.LengthFraction / 12)) * (decimal)3.14);
                                        //}
                                        //else if (ProductShapeId == null || ProductShapeId == (int)ProductShapeConstants.Rectangle || vm.ProductShapeId == (int)ProductShapeConstants.Square)
                                        //{
                                        //    if (pta.DefaultValue == ProductTypeAttributeValuess.Length)
                                        //    {
                                        //        UnitConv.ToQty = Width;
                                        //    }
                                        //    if (pta.DefaultValue == ProductTypeAttributeValuess.Width)
                                        //    {
                                        //        UnitConv.ToQty = Length;
                                        //    }
                                        //    if (pta.DefaultValue == ProductTypeAttributeValuess.LengthAndWidth)
                                        //    {
                                        //        UnitConv.ToQty = LenghtAndWidth;
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    UnitConv.ToQty = LenghtAndWidth;
                                        //}

                                        if (vm.CarpetSkuSettings.PerimeterSizeTypeId == (int)ProductSizeTypeConstants.StandardSize)
                                        {
                                            UnitConv.ToQty = GetUnitConversionQty(StandardSizeId, UnitConstants.Feet, pta.DefaultValue);
                                        }
                                        else if (vm.CarpetSkuSettings.PerimeterSizeTypeId == (int)ProductSizeTypeConstants.ManufacturingSize)
                                        {
                                            UnitConv.ToQty = GetUnitConversionQty(ManufacturingSizeId, UnitConstants.Feet, pta.DefaultValue);
                                        }
                                        else
                                        {
                                            UnitConv.ToQty = GetUnitConversionQty(FinishingSizeId, UnitConstants.Feet, pta.DefaultValue);
                                        }


                                        if (pta.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Binding)
                                        {
                                            UnitConv.UnitConversionForId = (int)UnitConversionFors.Binding;

                                            UnitConversion BindingUnitConversion = (from U in db.UnitConversion
                                                                                    where U.ProductId == prod.ProductId && U.UnitConversionForId == UnitConversionBinding
                                                                                    select U).FirstOrDefault();


                                            if (BindingUnitConversion == null)
                                            {
                                                if (pta.DefaultValue != ProductTypeAttributeValuess.NA)
                                                {
                                                    new UnitConversionService(_unitOfWork).Create(UnitConv);
                                                }
                                            }
                                            else
                                            {
                                                if (pta.DefaultValue != ProductTypeAttributeValuess.NA)
                                                {
                                                    BindingUnitConversion.ToQty = UnitConv.ToQty;
                                                    new UnitConversionService(_unitOfWork).Update(BindingUnitConversion);
                                                }
                                                else
                                                {
                                                    new UnitConversionService(_unitOfWork).Delete(BindingUnitConversion);
                                                }
                                            }
                                        }

                                        if (pta.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Gachhai)
                                        {
                                            UnitConv.UnitConversionForId = (int)UnitConversionFors.Gachhai;

                                            UnitConversion GachhaiUnitConversion = (from U in db.UnitConversion
                                                                                    where U.ProductId == prod.ProductId && U.UnitConversionForId == UnitConversionGachhai
                                                                                    select U).FirstOrDefault();

                                            if (GachhaiUnitConversion == null)
                                            {
                                                if (pta.DefaultValue != ProductTypeAttributeValuess.NA)
                                                {
                                                    new UnitConversionService(_unitOfWork).Create(UnitConv);
                                                }
                                            }
                                            else
                                            {
                                                if (pta.DefaultValue != ProductTypeAttributeValuess.NA)
                                                {
                                                    GachhaiUnitConversion.ToQty = UnitConv.ToQty;
                                                    new UnitConversionService(_unitOfWork).Update(GachhaiUnitConversion);
                                                }
                                                else
                                                {
                                                    new UnitConversionService(_unitOfWork).Delete(GachhaiUnitConversion);
                                                }
                                            }
                                        }

                                        if (pta.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.PattiMuraiDurry)
                                        {
                                            UnitConv.UnitConversionForId = (int)UnitConversionFors.PattiMuraiDurry;

                                            UnitConversion PattiMuraiDurryUnitConversion = (from U in db.UnitConversion
                                                                                            where U.ProductId == prod.ProductId && U.UnitConversionForId == UnitConversionPattiMuraiDurry
                                                                                            select U).FirstOrDefault();

                                            if (PattiMuraiDurryUnitConversion == null)
                                            {
                                                if (pta.DefaultValue != ProductTypeAttributeValuess.NA)
                                                {
                                                    new UnitConversionService(_unitOfWork).Create(UnitConv);
                                                }
                                            }
                                            else
                                            {
                                                if (pta.DefaultValue != ProductTypeAttributeValuess.NA)
                                                {
                                                    PattiMuraiDurryUnitConversion.ToQty = UnitConv.ToQty;
                                                    new UnitConversionService(_unitOfWork).Update(PattiMuraiDurryUnitConversion);
                                                }
                                                else
                                                {
                                                    new UnitConversionService(_unitOfWork).Delete(PattiMuraiDurryUnitConversion);
                                                }
                                            }
                                        }
                                    }

                                    //////////////////////End For Saving Binding, Gachhai and PattiMuraiDurry Unit Conversion Detail///////////////////////////////////





                                }
                            }

                            if (prod.ProcessSequenceHeaderId.HasValue && prod.ProcessSequenceHeaderId.Value > 0)
                            {

                                //var ProcessSeqLines = (from p in db.ProcessSequenceLine
                                //                       where p.ProcessSequenceHeaderId == prod.ProcessSequenceHeaderId
                                //                       select p).ToList();

                                IEnumerable<ProductProcessViewModel> ProcessSeqLines = new ProcessSequenceHeaderService(_unitOfWork).FGetProductProcessFromProcessSequence((int)prod.ProcessSequenceHeaderId);

                                var ExistingProdProcess = (from p in db.ProductProcess
                                                           where p.ProductId == prod.ProductId
                                                           select p).ToList();

                                var PendingToUpdate = (from p in ProcessSeqLines
                                                       join t in ExistingProdProcess on p.ProcessId equals t.ProcessId
                                                       select p).ToList();

                                var PendingToCreate = (from p in ProcessSeqLines
                                                       join t in ExistingProdProcess
                                                       on p.ProcessId equals t.ProcessId into table
                                                       from tab in table.DefaultIfEmpty()
                                                       where tab == null
                                                       select p).ToList();

                                var PendingToDelete = (from p in ExistingProdProcess
                                                       join t in ProcessSeqLines on p.ProcessId equals t.ProcessId
                                                       into table
                                                       from tab in table.DefaultIfEmpty()
                                                       where tab == null
                                                       select p).ToList();


                                foreach (var PTU in PendingToUpdate)
                                {
                                    ProductProcess ProdProc = ExistingProdProcess.Where(m => m.ProcessId == PTU.ProcessId).FirstOrDefault();
                                    ProdProc.Sr = PTU.Sr;
                                    ProdProc.ModifiedBy = User.Identity.Name;
                                    ProdProc.ModifiedDate = DateTime.Now;
                                    ProdProc.ObjectState = Model.ObjectState.Modified;
                                    new ProductProcessService(_unitOfWork).Update(ProdProc);
                                }

                                foreach (var PTC in PendingToCreate)
                                {
                                    //int? ProductRateGroupId = null;
                                    //var TempProductRateGroup = (from Pp in db.ProductProcess
                                    //                            join Pt in db.Product on Pp.ProductId equals Pt.ProductId into ProductTable
                                    //                            from ProductTab in ProductTable.DefaultIfEmpty()
                                    //                            where Pp.ProcessId == PTC.ProcessId && ProductTab.ProductGroupId == prod.ProductGroupId
                                    //                            select new { ProductRateGroupId = Pp.ProductRateGroupId }).FirstOrDefault();
                                    //if (TempProductRateGroup != null)
                                    //    ProductRateGroupId = TempProductRateGroup.ProductRateGroupId;

                                    ProductProcess ProdProc = new ProductProcess()
                                    {
                                        //ProductRateGroupId = ProductRateGroupId,
                                        CreatedBy = User.Identity.Name,
                                        CreatedDate = DateTime.Now,
                                        ModifiedBy = User.Identity.Name,
                                        ModifiedDate = DateTime.Now,
                                        ProcessId = PTC.ProcessId,
                                        Sr = PTC.Sr,
                                        ProductId = prod.ProductId,
                                    };
                                    ProdProc.ObjectState = Model.ObjectState.Added;
                                    new ProductProcessService(_unitOfWork).Create(ProdProc);

                                }

                                foreach (var PTD in PendingToDelete)
                                {
                                    PTD.ObjectState = Model.ObjectState.Deleted;
                                    new ProductProcessService(_unitOfWork).Delete(PTD);
                                }


                                //foreach (var ProcSeqLin in ProcessSeqLines)
                                //{
                                //    var ExistingProdProcs = (from p in db.ProductProcess
                                //                             where p.ProcessId == ProcSeqLin.ProcessId && p.ProductId == prod.ProductId
                                //                             select p).FirstOrDefault();

                                //    ProductProcess ProdProc = new ProductProcess()
                                //    {
                                //        CreatedBy = User.Identity.Name,
                                //        CreatedDate = DateTime.Now,
                                //        ModifiedBy = User.Identity.Name,
                                //        ModifiedDate = DateTime.Now,
                                //        ProcessId = ProcSeqLin.ProcessId,
                                //        Sr = ProcSeqLin.Sequence,
                                //        ProductId = prod.ProductId,
                                //    };
                                //    ProdProc.ObjectState = Model.ObjectState.Added;
                                //    new ProductProcessService(_unitOfWork).Create(ProdProc);
                                //}
                            }



                            if (vm.ProductSupplierId > 0 || vm.SupplierId.HasValue)
                            {


                                if (vm.ProductSupplierId > 0 && vm.SupplierId.HasValue && vm.SupplierId.Value > 0)
                                {
                                    var ps = new ProductSupplierService(_unitOfWork).GetDefaultSupplier(prod.ProductId);

                                    ps.Cost = vm.Cost;
                                    ps.Default = true;
                                    ps.LeadTime = vm.LeadTime;
                                    ps.MaximumOrderQty = vm.MaximumOrderQty;
                                    ps.MinimumOrderQty = vm.MinimumOrderQty;
                                    ps.ModifiedBy = User.Identity.Name;
                                    ps.ModifiedDate = DateTime.Now;
                                    ps.ProductId = prod.ProductId;
                                    ps.SupplierId = vm.SupplierId.Value;
                                    ps.ObjectState = Model.ObjectState.Modified;

                                    new ProductSupplierService(_unitOfWork).Update(ps);

                                }
                                else if (vm.ProductSupplierId <= 0 && vm.SupplierId.HasValue && vm.SupplierId.Value > 0)
                                {
                                    ProductSupplier ps = new ProductSupplier();


                                    ps.Cost = vm.Cost;
                                    ps.Default = true;
                                    ps.LeadTime = vm.LeadTime;
                                    ps.MaximumOrderQty = vm.MaximumOrderQty;
                                    ps.MinimumOrderQty = vm.MinimumOrderQty;
                                    ps.CreatedDate = DateTime.Now;
                                    ps.CreatedBy = User.Identity.Name;
                                    ps.ModifiedBy = User.Identity.Name;
                                    ps.ModifiedDate = DateTime.Now;
                                    ps.ProductId = prod.ProductId;
                                    ps.SupplierId = vm.SupplierId.Value;
                                    ps.ObjectState = Model.ObjectState.Added;

                                    new ProductSupplierService(_unitOfWork).Create(ps);
                                }
                                else if (vm.ProductSupplierId > 0 && !vm.SupplierId.HasValue)
                                {
                                    var ps = new ProductSupplierService(_unitOfWork).GetDefaultSupplier(prod.ProductId);

                                    ps.ObjectState = Model.ObjectState.Deleted;

                                    new ProductSupplierService(_unitOfWork).Delete(ps);

                                }

                            }


                            Product P = _FinishedProductService.Find(item.ProductId);
                            var Sizes = new ProductSizeService(_unitOfWork).GetProductSizeIndexForProduct(item.ProductId);
                            int ProductDesignNAId = new ProductDesignService(_unitOfWork).Find("NA").ProductDesignId;
                            //if (vm.ProductDesignId != null && vm.ProductDesignId != ProductDesignNAId)
                            //{
                            //    j--;
                            //    CreateTrace((int)vm.ProductDesignId, (int)vm.ProductGroupId, Sizes.StandardSizeId, Sizes.StencilSizeId,P.ProductName, -j);
                            //}
                            //else
                            //{
                            //    j--;
                            //    CreateTraceForProduct(P.ProductName, (int)Sizes.StencilSizeId, -j);
                            //}
                            j--;
                            CreateMap(P.ProductName, Sizes.MapSizeId, -j);


                            var BomProduct = (from L in db.BomDetail
                                              join Pt in db.Product on L.ProductId equals Pt.ProductId into ProductTable
                                              from ProductTab in ProductTable.DefaultIfEmpty()
                                              where L.BaseProductId == prod.ProductId
                                              select ProductTab).FirstOrDefault();

                            if (BomProduct != null)
                            {
                                if (BomProduct.StandardWeight != vm.GrossWeight)
                                {
                                    BomProduct.StandardWeight = vm.GrossWeight;
                                    _ProductService.Update(BomProduct);
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
                            PrepareDivisionViewBag();
                            PrepareViewBag(vm);
                            return View("Create", vm);
                        }


                        #region

                        //Saving Image if file is uploaded
                        if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
                        {
                            string uploadfolder = group.ImageFolderName;
                            string tempfilename = group.ImageFileName;
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

                            string temp2 = "";
                            string filename = System.Guid.NewGuid().ToString();
                            foreach (string filekey in System.Web.HttpContext.Current.Request.Files.Keys)
                            {

                                HttpPostedFile pfile = System.Web.HttpContext.Current.Request.Files[filekey];
                                if (pfile.ContentLength <= 0) continue; //Skip unused file controls.    

                                temp2 = Path.GetExtension(pfile.FileName);

                                string uploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder);
                                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                                string filecontent = Path.Combine(uploadFolder, group.ProductGroupName + "_" + filename);

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
                                        string tfileName = Path.Combine(tuploadFolder, group.ProductGroupName + "_" + filename);

                                        //Let the image builder add the correct extension based on the output file type
                                        //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                        ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                    }
                                    else if (suffix == "_medium")
                                    {
                                        string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                        if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                        //Generate a filename (GUIDs are best).
                                        string tfileName = Path.Combine(tuploadFolder, group.ProductGroupName + "_" + filename);

                                        //Let the image builder add the correct extension based on the output file type
                                        //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                        ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                    }
                                }
                            }
                            var temsave = _ProductGroupService.Find(group.ProductGroupId);
                            temsave.ImageFileName = temsave.ProductGroupName + "_" + filename + temp2;
                            temsave.ImageFolderName = uploadfolder;
                            _ProductGroupService.Update(temsave);

                            var Prodlist = _ProductService.GetProductListForGroup(vm.ProductGroupId);
                            foreach (var item in Prodlist)
                            {
                                FinishedProduct prod = _FinishedProductService.Find(item.ProductId);

                                prod.ImageFileName = group.ImageFileName;
                                prod.ImageFolderName = group.ImageFolderName;
                                prod.ObjectState = Model.ObjectState.Modified;
                                _FinishedProductService.Update(prod);
                            }

                            _unitOfWork.Save();
                        }

                        #endregion


                    }
                    else
                    {
                        vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                        vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                        PrepareDivisionViewBag();
                        PrepareViewBag(vm);
                        return View("Create", vm);
                    }
                    PrepareDivisionViewBag();
                    PrepareViewBag(vm);
                    return RedirectToAction("Index", new { id = sampleId }).Success("Data Saved Sucessfully");
                }
            }
            PrepareDivisionViewBag();
            PrepareViewBag(vm);
            return View("Create", vm);
        }


        public ActionResult GetSize(string searchTerm, int pageSize, int pageNum, int ProductShapeId)
        {
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(GetSizeHelpList(ProductShapeId));


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public IEnumerable<ComboBoxList> GetSizeHelpList(int ProductShapeId)
        {
            //IEnumerable<ComboBoxList> prodList = db.Size.Where(m => m.IsActive == true && m.ProductShapeId == ProductShapeId).OrderBy(m => m.SizeName).Select(i => new ComboBoxList
            //{
            //    Id = i.SizeId,
            //    PropFirst = i.SizeName,
            //});


            var prodList = from p in db.Size
                           join ps in db.ProductShape on p.ProductShapeId equals ps.ProductShapeId into ProductShapeTable
                           from ProductShapeTab in ProductShapeTable.DefaultIfEmpty()
                           where p.IsActive == true && p.ProductShapeId == ProductShapeId
                           orderby p.SizeName
                           select new ComboBoxList
                           {
                               Id = p.SizeId,
                               PropFirst = p.SizeName + ProductShapeTab.ProductShapeShortName ?? "",
                           };

            return prodList.ToList();
        }


        public void PrepareViewBag(CarpetMasterViewModel vm)
        {
            ViewBag.ProductShapeList = new ProductShapeService(_unitOfWork).GetProductShapeList().ToList();
            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = "Half", Value = "Half" });
            temp.Add(new SelectListItem { Text = "Full", Value = "Full" });
            temp.Add(new SelectListItem { Text = "N/A", Value = "N/A" });

            if (vm == null)
            {
                ViewBag.TraceType = new SelectList(temp, "Value", "Text");
                ViewBag.MapType = new SelectList(temp, "Value", "Text");
            }
            else
            {
                ViewBag.TraceType = new SelectList(temp, "Value", "Text", vm.TraceType);
                ViewBag.MapType = new SelectList(temp, "Value", "Text", vm.MapType);
            }


        }




        [HttpGet]
        public ActionResult NextPage(int id, string name, bool sample)//CurrentHeaderId
        {
            var nextId = _ProductGroupService.NextIdForCarpet(id);
            return RedirectToAction("Edit", new { id = nextId, sample = sample });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, string name, bool sample)//CurrentHeaderId
        {
            var nextId = _ProductGroupService.PrevIdForCarpet(id);
            return RedirectToAction("Edit", new { id = nextId, sample = sample });
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
        public ActionResult Remove()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult AddSize(CarpetMasterViewModel vm)
        {
            PrepareViewBag(vm);

            vm.ProductTypeAttributes = (List<ProductTypeAttributeViewModel>)System.Web.HttpContext.Current.Session["list"];

            var ProductShape = new ProductShapeService(_unitOfWork).Find("Rectangle");

            if (ProductShape != null)
            {
                vm.ProductShapeId = ProductShape.ProductShapeId;
            }

            //vm.ProductShapeId = (int)ProductShapeConstants.Rectangle;

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            CarpetSkuSettings Stng = new CarpetSkuSettingsService(_unitOfWork).GetCarpetSkuSettings(DivisionId, SiteId);
            vm.CarpetSkuSettings = Mapper.Map<CarpetSkuSettings, CarpetSkuSettingsViewModel>(Stng);

            return PartialView("EditSize", vm);
        }
        [HttpGet]
        public ActionResult EditSize(int id)//ProductId
        {
            PrepareViewBag(null);

            var temp = _ProductService.GetFinishedProduct(id);

            CarpetMasterViewModel vm = _ProductSizeService.GetProductSizeIndexForProduct(id);

            vm.ProductCategoryId = temp.ProductCategoryId;
            vm.ProductCollectionId = temp.ProductCollectionId;
            vm.ProductQualityId = temp.ProductQualityId;
            vm.ProductDesignId = temp.ProductDesignId;
            vm.ProductInvoiceGroupId = temp.ProductInvoiceGroupId;
            vm.ProductStyleId = temp.ProductStyleId;
            vm.ProductManufacturerId = temp.ProductManufacturerId;
            vm.ProcessSequenceHeaderId = temp.ProcessSequenceHeaderId;
            vm.ProductionRemark = temp.ProductionRemark;
            vm.DrawBackTariffHeadId = temp.DrawBackTariffHeadId;
            vm.ProductGroupId = temp.ProductGroupId;
            vm.ProductGroupName = temp.ProductGroupName;
            vm.DivisionId = temp.DivisionId;
            vm.ProductCode = temp.ProductCode;
            vm.ProductName = temp.ProductName;
            vm.ProductDescription = temp.ProductDescription;
            vm.ProductShapeId = temp.ProductShapeId;
            vm.ColourId = (int)temp.ColourId;
            vm.DescriptionOfGoodsId = temp.DescriptionOfGoodsId;
            vm.SalesTaxProductCodeId = temp.SalesTaxProductCodeId;
            vm.OriginCountryId = temp.OriginCountryId;
            vm.TraceType = temp.TraceType;
            vm.MapType = temp.MapType;
            vm.MapScale = temp.MapScale;
            vm.CBM = temp.CBM;

            var bd = new BomDetailService(_unitOfWork).GetConsumptionForIndex(id);

            if (bd.Count() >0)
                vm.IsBomCreated = true;
            else
                vm.IsBomCreated = false ;

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            CarpetSkuSettings Stng = new CarpetSkuSettingsService(_unitOfWork).GetCarpetSkuSettings(vm.DivisionId, SiteId);
            vm.CarpetSkuSettings = Mapper.Map<CarpetSkuSettings, CarpetSkuSettingsViewModel>(Stng);

            ////Setting ProductSupplier Details
            //var ProductSupplier = new ProductSupplierService(_unitOfWork).GetDefaultSupplier(vm.ProductId);

            //if(ProductSupplier!=null)
            //{
            //    vm.ProductSupplierId = ProductSupplier.ProductSupplierId;
            //    vm.SupplierId = ProductSupplier.SupplierId;
            //    vm.LeadTime = ProductSupplier.LeadTime;
            //    vm.MinimumOrderQty = ProductSupplier.MinimumOrderQty;
            //    vm.MaximumOrderQty = ProductSupplier.MaximumOrderQty;
            //    vm.Cost = ProductSupplier.Cost;
            //}

            return PartialView("EditSize", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSizePost(CarpetMasterViewModel vm, FormCollection collection)
        {


            vm.ProductTypeAttributes = (List<ProductTypeAttributeViewModel>)System.Web.HttpContext.Current.Session["list"];
            if (vm.StandardSizeId == 0)
            {
                ModelState.AddModelError("StandardSizeId", "The Standard size field is required");
            }
            if (vm.ManufacturingSizeId == 0)
            {
                ModelState.AddModelError("ManufacturingSizeId", "The Manufacturing size field is required");
            }
            if (vm.FinishingSizeId == 0)
            {
                ModelState.AddModelError("FinishingSizeId", "The Finishing size filed is required");
            }
            //if (vm.StencilSizeId == 0)
            //{
            //    ModelState.AddModelError("StencilSizeId", "The Stencil size field is required");
            //}
            if (vm.MapSizeId == 0)
            {
                ModelState.AddModelError("MapSizeId", "The Map size field is required");
            }
            if (string.IsNullOrEmpty(vm.ProductCode))
            {
                ModelState.AddModelError("ProductCode", "The Product Code field is required");
            }
            if (string.IsNullOrEmpty(vm.ProductName))
            {
                ModelState.AddModelError("ProductName", "The Product Name field is required");
            }
            if (vm.ProductGroupName == vm.ProductName)
            {
                ModelState.AddModelError("ProductName", "The Product Name and product group name should not be same.");
            }


            if (ModelState.IsValid)
            {
                //Checking if the size is new or existing i.e. Create or Delete
                if (vm.ProductId == 0)
                {
                    FinishedProduct pro = new FinishedProduct();
                    pro.ProductName = vm.ProductName;
                    pro.ProductCode = vm.ProductCode;
                    pro.ProductDescription = vm.ProductDescription;
                    pro.ProductCategoryId = vm.ProductCategoryId;
                    pro.ProductCollectionId = vm.ProductCollectionId;
                    pro.ProductQualityId = vm.ProductQualityId;
                    pro.ProductDesignId = vm.ProductDesignId;
                    pro.ProductInvoiceGroupId = vm.ProductInvoiceGroupId;
                    pro.ProductDesignPatternId = vm.ProductDesignPatternId;
                    pro.ColourId = vm.ColourId;
                    pro.ContentId = vm.ContentId;
                    pro.FaceContentId = vm.FaceContentId;
                    pro.ProductShapeId = vm.ProductShapeId;
                    pro.SampleId = vm.SampleId;
                    pro.CounterNo = vm.CounterNo;
                    pro.DescriptionOfGoodsId = vm.DescriptionOfGoodsId;
                    pro.SalesTaxProductCodeId = vm.SalesTaxProductCodeId;
                    pro.StandardCost = vm.StandardCost;
                    pro.StandardWeight = vm.StandardWeight;
                    pro.GrossWeight = vm.GrossWeight;
                    pro.ProductStyleId = vm.ProductStyleId;
                    pro.ProductManufacturerId = vm.ProductManufacturerId;
                    pro.DrawBackTariffHeadId = vm.DrawBackTariffHeadId;
                    pro.ProcessSequenceHeaderId = vm.ProcessSequenceHeaderId;
                    pro.ProductionRemark = vm.ProductionRemark;
                    pro.ProductGroupId = vm.ProductGroupId;
                    pro.DivisionId = vm.DivisionId;
                    pro.OriginCountryId = vm.OriginCountryId;
                    pro.Tags = vm.Tags;
                    pro.CBM = vm.CBM;
                    pro.ImageFileName = vm.ImageFileName;
                    pro.ImageFolderName = vm.ImageFolderName;
                    pro.IsSample = vm.IsSample;
                    pro.IsActive = true;
                    pro.UnitId = UnitConstants.Pieces;
                    pro.TraceType = vm.TraceType;
                    pro.MapType = vm.MapType;
                    pro.MapScale = vm.MapScale;


                    pro.CreatedBy = User.Identity.Name;
                    pro.CreatedDate = DateTime.Now;
                    pro.ModifiedBy = User.Identity.Name;
                    pro.ModifiedDate = DateTime.Now;



                    //Code For Saving Product Specification
                    string StandardSize = "";
                    if (vm.StandardSizeId != 0)
                    {
                        StandardSize = new SizeService(_unitOfWork).Find(vm.StandardSizeId).SizeName;
                    }
                    pro.ProductSpecification = StandardSize;








                    _FinishedProductService.Create(pro);





                    //Standard Size Data
                    ProductSize standardsize = new ProductSize();
                    standardsize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.StandardSize);
                    //standardsize.ProductSizeTypeId = 1;
                    standardsize.SizeId = vm.StandardSizeId;
                    standardsize.ProductId = pro.ProductId;
                    standardsize.CreatedBy = User.Identity.Name;
                    standardsize.CreatedDate = DateTime.Now;
                    standardsize.ModifiedBy = User.Identity.Name;
                    standardsize.ModifiedDate = DateTime.Now;
                    standardsize.IsActive = true;
                    _ProductSizeService.Create(standardsize);

                    //Saving StardardUnitConversionData

                    UnitConversion StandardUnit;
                    string Mode;
                    CreateUnitConversion((byte)UnitConversionFors.Standard, vm.StandardSizeId, pro.ProductId, UnitConstants.SqYard, out StandardUnit, out Mode);
                    if (Mode == "Create")
                    {
                        new UnitConversionService(_unitOfWork).Create(StandardUnit);
                    }
                    else if (Mode == "Edit")
                    {
                        new UnitConversionService(_unitOfWork).Update(StandardUnit);
                    }


                    //Saving StardardUnitConversionData-FT2


                    UnitConversion StandardUnitFT2;
                    string ModeFT2;
                    CreateUnitConversion((byte)UnitConversionFors.Standard, vm.StandardSizeId, pro.ProductId, UnitConstants.SqFeet, out StandardUnitFT2, out ModeFT2);
                    if (ModeFT2 == "Create")
                    {
                        new UnitConversionService(_unitOfWork).Create(StandardUnitFT2);
                    }
                    else if (ModeFT2 == "Edit")
                    {
                        new UnitConversionService(_unitOfWork).Update(StandardUnitFT2);
                    }


                    

                    //Manufacturing Size Data
                    ProductSize Manufacturingsize = new ProductSize();
                    Manufacturingsize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.ManufacturingSize);
                    //Manufacturingsize.ProductSizeTypeId = 2;
                    Manufacturingsize.SizeId = vm.ManufacturingSizeId;
                    Manufacturingsize.ProductId = pro.ProductId;
                    Manufacturingsize.CreatedBy = User.Identity.Name;
                    Manufacturingsize.CreatedDate = DateTime.Now;
                    Manufacturingsize.ModifiedBy = User.Identity.Name;
                    Manufacturingsize.ModifiedDate = DateTime.Now;
                    Manufacturingsize.IsActive = true;
                    _ProductSizeService.Create(Manufacturingsize);

                    //Saving ManufacturingUnitConversionData


                    UnitConversion ManufacturingUnit;
                    string MMode;
                    CreateUnitConversion((byte)UnitConversionFors.Manufacturing, vm.ManufacturingSizeId, pro.ProductId, UnitConstants.SqYard, out ManufacturingUnit, out MMode);
                    if (MMode == "Create")
                    {
                        new UnitConversionService(_unitOfWork).Create(ManufacturingUnit);
                    }
                    else if (MMode == "Edit")
                    {
                        new UnitConversionService(_unitOfWork).Update(ManufacturingUnit);
                    }


                    //UnitConversion ManufacturingUnitSqMeter;
                    //string MModeSqMeter;
                    //CreateUnitConversion((byte)UnitConversionFors.Manufacturing, vm.ManufacturingSizeId, pro.ProductId, UnitConstants.SqMeter, out ManufacturingUnitSqMeter, out MModeSqMeter);
                    //if (MModeSqMeter == "Create")
                    //{
                    //    new UnitConversionService(_unitOfWork).Create(ManufacturingUnitSqMeter);
                    //}
                    //else if (MModeSqMeter == "Edit")
                    //{
                    //    new UnitConversionService(_unitOfWork).Update(ManufacturingUnitSqMeter);
                    //}



                    //Finishing Size Data
                    ProductSize Finishingsize = new ProductSize();
                    //Finishingsize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.FinishingSize);
                    Finishingsize.ProductSizeTypeId = 3;
                    Finishingsize.SizeId = vm.FinishingSizeId;
                    Finishingsize.ProductId = pro.ProductId;
                    Finishingsize.CreatedBy = User.Identity.Name;
                    Finishingsize.CreatedDate = DateTime.Now;
                    Finishingsize.ModifiedBy = User.Identity.Name;
                    Finishingsize.ModifiedDate = DateTime.Now;
                    Finishingsize.IsActive = true;
                    _ProductSizeService.Create(Finishingsize);



                    //Saving FinishingUnitConversionData

                    UnitConversion FinishingUnit;
                    string FMode;
                    CreateUnitConversion((byte)UnitConversionFors.Finishing, vm.FinishingSizeId, pro.ProductId, UnitConstants.SqYard, out FinishingUnit, out FMode);
                    if (FMode == "Create")
                    {
                        new UnitConversionService(_unitOfWork).Create(FinishingUnit);
                    }
                    else if (MMode == "Edit")
                    {
                        new UnitConversionService(_unitOfWork).Update(FinishingUnit);
                    }


                    //UnitConversion FinishingUnitSqMeter;
                    //string FModeSqMeter;
                    //CreateUnitConversion((byte)UnitConversionFors.Finishing, vm.FinishingSizeId, pro.ProductId, UnitConstants.SqMeter, out FinishingUnitSqMeter, out FModeSqMeter);
                    //if (FModeSqMeter == "Create")
                    //{
                    //    new UnitConversionService(_unitOfWork).Create(FinishingUnitSqMeter);
                    //}
                    //else if (FModeSqMeter == "Edit")
                    //{
                    //    new UnitConversionService(_unitOfWork).Update(FinishingUnitSqMeter);
                    //}


                    //Stencil Size Data
                    if (vm.StencilSizeId != 0)
                    {
                        ProductSize Stencilsize = new ProductSize();
                        Stencilsize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.StencilSize);
                        Stencilsize.SizeId = vm.StencilSizeId;
                        Stencilsize.ProductId = pro.ProductId;
                        Stencilsize.CreatedBy = User.Identity.Name;
                        Stencilsize.CreatedDate = DateTime.Now;
                        Stencilsize.ModifiedBy = User.Identity.Name;
                        Stencilsize.ModifiedDate = DateTime.Now;
                        Stencilsize.IsActive = true;
                        _ProductSizeService.Create(Stencilsize);
                    }



                    //Map Size Data
                    ProductSize Mapsize = new ProductSize();
                    Mapsize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.MapSize);
                    Mapsize.SizeId = vm.MapSizeId;
                    Mapsize.ProductId = pro.ProductId;
                    Mapsize.CreatedBy = User.Identity.Name;
                    Mapsize.CreatedDate = DateTime.Now;
                    Mapsize.ModifiedBy = User.Identity.Name;
                    Mapsize.ModifiedDate = DateTime.Now;
                    Mapsize.IsActive = true;
                    _ProductSizeService.Create(Mapsize);












                    #region "Saving Unit Conversions for that units which are defined in Settings."
                    if (vm.CarpetSkuSettings.UnitConversions != null && vm.CarpetSkuSettings.UnitConversions != "")
                    {
                        string[] UnitConversionArr = vm.CarpetSkuSettings.UnitConversions.Split(',');

                        for (int i = 0; i < UnitConversionArr.Length; i++)
                        {
                            string ConversionUnit = UnitConversionArr[i];
                            Unit Unit = new UnitService(_unitOfWork).Find(ConversionUnit);


                            UnitConversion StandardUnitConversion_Settings;
                            string StandardMode_Settings;
                            CreateUnitConversion((byte)UnitConversionFors.Standard, vm.StandardSizeId, pro.ProductId, ConversionUnit, out StandardUnitConversion_Settings, out StandardMode_Settings);

                            if (StandardUnitConversion_Settings.ToQty == 0 || StandardUnitConversion_Settings.ToQty == null)
                            {
                                string ProductCategory = "";
                                if (vm.ProductCategoryId != null)
                                {
                                    ProductCategory productCategory = new ProductCategoryService(_unitOfWork).Find((int)vm.ProductCategoryId);
                                    ProductCategory = productCategory.ProductCategoryName;
                                }

                                //if (ProductCategory == "NEPALI")
                                //{
                                //    Size Size = new SizeService(_unitOfWork).Find(vm.StandardSizeId);
                                //    string message = "Unable to get unit conversion to " + Unit.UnitName + " for size " + Size.SizeName;
                                //    ModelState.AddModelError("", message);
                                //    PrepareViewBag(vm);
                                //    return PartialView("EditSize", vm);
                                //}
                            }

                            if (StandardMode_Settings == "Create")
                            {
                                new UnitConversionService(_unitOfWork).Create(StandardUnitConversion_Settings);
                            }
                            else if (StandardMode_Settings == "Edit")
                            {
                                new UnitConversionService(_unitOfWork).Update(StandardUnitConversion_Settings);
                            }


                            UnitConversion ManufacturingUnitConversion_Settings;
                            string ManufacturingMode_Settings;
                            CreateUnitConversion((byte)UnitConversionFors.Manufacturing, vm.ManufacturingSizeId, pro.ProductId, ConversionUnit, out ManufacturingUnitConversion_Settings, out ManufacturingMode_Settings);


                            if (ManufacturingUnitConversion_Settings.ToQty == 0 || ManufacturingUnitConversion_Settings.ToQty == null)
                            {
                                string ProductCategory = "";
                                if (vm.ProductCategoryId != null)
                                {
                                    ProductCategory productCategory = new ProductCategoryService(_unitOfWork).Find((int)vm.ProductCategoryId);
                                    ProductCategory = productCategory.ProductCategoryName;
                                }

                                //if (ProductCategory == "NEPALI")
                                //{
                                //    Size Size = new SizeService(_unitOfWork).Find(vm.ManufacturingSizeId);
                                //    string message = "Unable to get unit conversion to " + Unit.UnitName + " for size " + Size.SizeName;
                                //    ModelState.AddModelError("", message);
                                //    PrepareViewBag(vm);
                                //    return PartialView("EditSize", vm);
                                //}
                            }


                            if (ManufacturingMode_Settings == "Create")
                            {
                                new UnitConversionService(_unitOfWork).Create(ManufacturingUnitConversion_Settings);
                            }
                            else if (ManufacturingMode_Settings == "Edit")
                            {
                                new UnitConversionService(_unitOfWork).Update(ManufacturingUnitConversion_Settings);
                            }




                            UnitConversion FinishingUnitConversion_Settings;
                            string FinishingMode_Settings;
                            CreateUnitConversion((byte)UnitConversionFors.Finishing, vm.FinishingSizeId, pro.ProductId, ConversionUnit, out FinishingUnitConversion_Settings, out FinishingMode_Settings);


                            if (FinishingUnitConversion_Settings.ToQty == 0 || FinishingUnitConversion_Settings.ToQty == null)
                            {
                                string ProductCategory = "";
                                if (vm.ProductCategoryId != null)
                                {
                                    ProductCategory productCategory = new ProductCategoryService(_unitOfWork).Find((int)vm.ProductCategoryId);
                                    ProductCategory = productCategory.ProductCategoryName;
                                }

                                //if (ProductCategory == "NEPALI")
                                //{
                                //    Size Size = new SizeService(_unitOfWork).Find(vm.FinishingSizeId);
                                //    string message = "Unable to get unit conversion to " + Unit.UnitName + " for size " + Size.SizeName;
                                //    ModelState.AddModelError("", message);
                                //    PrepareViewBag(vm);
                                //    return PartialView("EditSize", vm);
                                //}

                            }


                            if (FinishingMode_Settings == "Create")
                            {
                                new UnitConversionService(_unitOfWork).Create(FinishingUnitConversion_Settings);
                            }
                            else if (FinishingMode_Settings == "Edit")
                            {
                                new UnitConversionService(_unitOfWork).Update(FinishingUnitConversion_Settings);
                            }
                        }
                    }
                    #endregion





                    //Saving Binding Unit Conversion








                    int producttypeid = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;
                    List<ProductTypeAttributeViewModel> tem = vm.ProductTypeAttributes;

                    //foreach (var item in tem)
                    //{
                    //    item.DefaultValue = collection[item.Name + 's'].ToString();
                    //}


                    Size FinishingSizeForPerimeter = new SizeService(_unitOfWork).Find(vm.FinishingSizeId);

                    if (tem != null)
                    {
                        foreach (var item in tem)
                        {
                            ProductAttributes pa = new ProductAttributes();
                            pa.ProductTypeAttributeId = item.ProductTypeAttributeId;
                            pa.ProductId = pro.ProductId;
                            pa.ProductAttributeValue = item.DefaultValue;
                            pa.CreatedBy = User.Identity.Name;
                            pa.ModifiedBy = User.Identity.Name;
                            pa.CreatedDate = DateTime.Now;
                            pa.ModifiedDate = DateTime.Now;

                            new ProductAttributeService(_unitOfWork).Create(pa);


                            decimal Length = Math.Floor((FinishingSizeForPerimeter.Length + (FinishingSizeForPerimeter.LengthFraction / 12)) * 2);
                            decimal Width = Math.Floor((FinishingSizeForPerimeter.Width + (FinishingSizeForPerimeter.WidthFraction / 12)) * 2);
                            decimal LenghtAndWidth = Math.Floor(((FinishingSizeForPerimeter.Length + (FinishingSizeForPerimeter.LengthFraction / 12)) * 2) + ((FinishingSizeForPerimeter.Width + (FinishingSizeForPerimeter.WidthFraction / 12)) * 2));

                            //Added condition to skip ply of yarns case To be discussed and implemented
                            if (item.DefaultValue != ProductTypeAttributeValuess.NA && (item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Binding || item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Gachhai
                                || item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.PattiMuraiDurry))
                            {
                                UnitConversion UnitConv = new UnitConversion();
                                UnitConv.CreatedBy = User.Identity.Name;
                                UnitConv.CreatedDate = DateTime.Now;
                                UnitConv.ModifiedBy = User.Identity.Name;
                                UnitConv.ModifiedDate = DateTime.Now;
                                UnitConv.ProductId = pro.ProductId;
                                UnitConv.FromQty = 1;
                                UnitConv.FromUnitId = UnitConstants.Pieces;
                                UnitConv.ToUnitId = UnitConstants.Feet;

                                if (item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Binding)
                                {
                                    UnitConv.UnitConversionForId = (int)UnitConversionFors.Binding;
                                }

                                if (item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Gachhai)
                                {
                                    UnitConv.UnitConversionForId = (int)UnitConversionFors.Gachhai;
                                }

                                if (item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.PattiMuraiDurry)
                                {
                                    UnitConv.UnitConversionForId = (int)UnitConversionFors.PattiMuraiDurry;
                                }

                                //if (vm.ProductShapeId == (int)ProductShapeConstants.Circle)
                                //{
                                //    UnitConv.ToQty = Math.Floor((FinishingSizeForPerimeter.Length + (FinishingSizeForPerimeter.LengthFraction / 12)) * (decimal)3.14);
                                //}
                                //else if (vm.ProductShapeId == (int)ProductShapeConstants.Rectangle || vm.ProductShapeId == (int)ProductShapeConstants.Square)
                                //{
                                //    if (item.DefaultValue == ProductTypeAttributeValuess.Length)
                                //    {
                                //        UnitConv.ToQty = Width;
                                //    }
                                //    if (item.DefaultValue == ProductTypeAttributeValuess.Width)
                                //    {
                                //        UnitConv.ToQty = Length;
                                //    }
                                //    if (item.DefaultValue == ProductTypeAttributeValuess.LengthAndWidth)
                                //    {
                                //        UnitConv.ToQty = LenghtAndWidth;
                                //    }
                                //}
                                //else
                                //{
                                //    UnitConv.ToQty = LenghtAndWidth;
                                //}

                                //UnitConv.ToQty = GetUnitConversionQty(vm.FinishingSizeId, UnitConstants.Feet, item.DefaultValue);
                                if (vm.CarpetSkuSettings.PerimeterSizeTypeId == (int)ProductSizeTypeConstants.StandardSize)
                                {
                                    UnitConv.ToQty = GetUnitConversionQty(vm.StandardSizeId, UnitConstants.Feet, item.DefaultValue);
                                }
                                else if (vm.CarpetSkuSettings.PerimeterSizeTypeId == (int)ProductSizeTypeConstants.ManufacturingSize)
                                {
                                    UnitConv.ToQty = GetUnitConversionQty(vm.ManufacturingSizeId, UnitConstants.Feet, item.DefaultValue);
                                }
                                else
                                {
                                    UnitConv.ToQty = GetUnitConversionQty(vm.FinishingSizeId, UnitConstants.Feet, item.DefaultValue);
                                }
                                new UnitConversionService(_unitOfWork).Create(UnitConv);
                            }
                        }
                    }

                    //For Inserting Bom for New Product

                    //var ProductGroupProduct = (from Pg in db.ProductGroups
                    //                           join P in db.Product on Pg.ProductGroupName equals P.ProductName into ProductTable
                    //                           from ProductTab in ProductTable.DefaultIfEmpty()
                    //                           where Pg.ProductGroupId == vm.ProductGroupId
                    //                           select new
                    //                           {
                    //                               ProductGroupId = Pg.ProductGroupId,
                    //                               ProductId = (int?)ProductTab.ProductId ?? 0
                    //                           }).FirstOrDefault();

                    var BomProduct = (from P in db.FinishedProduct
                                               join Bd in db.BomDetail on P.ProductId equals Bd.BaseProductId into BomDetailTable from BomDetailTab in BomDetailTable.DefaultIfEmpty()
                                               where P.ProductGroupId == vm.ProductGroupId && P.ColourId == vm.ColourId
                                               select new
                                               {
                                                   ProductId = (int?)BomDetailTab.ProductId
                                               }).FirstOrDefault();


                    if (BomProduct != null)
                    {
                        if (BomProduct.ProductId != null && BomProduct.ProductId != 0)
                        {
                            BomDetail BomDetail = new BomDetail();
                            BomDetail.BaseProductId = pro.ProductId;
                            BomDetail.BatchQty = 1;
                            BomDetail.ConsumptionPer = 100;
                            BomDetail.CreatedBy = pro.CreatedBy;
                            BomDetail.ModifiedBy = pro.ModifiedBy;
                            BomDetail.CreatedDate = pro.CreatedDate;
                            BomDetail.ModifiedDate = pro.ModifiedDate;
                            BomDetail.Dimension1Id = null;
                            BomDetail.Dimension2Id = null;
                            BomDetail.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;
                            BomDetail.ProductId = (int)BomProduct.ProductId;
                            BomDetail.Qty = 1;

                            new BomDetailService(_unitOfWork).Create(BomDetail);
                        }
                    }

                    


                    //var bomproduct = (from p in db.Product
                    //                  where p.ProductGroupId == vm.ProductGroupId && p.ProductId != pro.ProductId
                    //                  select new
                    //                  {
                    //                      ProductId = p.ProductId
                    //                  }).FirstOrDefault();

                    //if (bomproduct != null)
                    //{ 
                    //    IEnumerable<BomDetail> BomDetailList = new BomDetailService(_unitOfWork).GetBomDetailList(bomproduct.ProductId);

                    //    foreach (BomDetail item in BomDetailList)
                    //    {
                    //        BomDetail BomDetail = new BomDetail();
                    //        BomDetail.BaseProductId = pro.ProductId;
                    //        BomDetail.BatchQty = item.BatchQty;
                    //        BomDetail.ConsumptionPer = item.ConsumptionPer;
                    //        BomDetail.CreatedBy = pro.CreatedBy;
                    //        BomDetail.ModifiedBy = pro.ModifiedBy;
                    //        BomDetail.CreatedDate = pro.CreatedDate;
                    //        BomDetail.ModifiedDate = pro.ModifiedDate;
                    //        BomDetail.Dimension1Id = item.Dimension1Id;
                    //        BomDetail.Dimension2Id = item.Dimension2Id;
                    //        BomDetail.ProcessId = item.ProcessId;
                    //        BomDetail.ProductId = item.ProductId;
                    //        BomDetail.Qty = item.Qty;

                    //        new BomDetailService(_unitOfWork).Create(BomDetail);
                    //    }
                    //}

                    if (pro.TraceType != "N/A")
                    {
                        //int ProductDesignNAId = new ProductDesignService(_unitOfWork).Find("NA").ProductDesignId;

                        //if (pro.ProductDesignId != null && pro.ProductDesignId != ProductDesignNAId)
                        //{
                        //    CreateTrace((int)pro.ProductDesignId, (int)pro.ProductGroupId, vm.StandardSizeId, vm.StencilSizeId,pro.ProductName, -1);
                        //}
                        //else
                        //{
                        //    CreateTraceForProduct(vm.ProductName, (int)vm.StencilSizeId, -1);
                        //}
                    }

                    if (pro.MapType != "N/A")
                    {
                        CreateMap(vm.ProductName, vm.MapSizeId, -2);
                    }



                    if (pro.ProcessSequenceHeaderId.HasValue && pro.ProcessSequenceHeaderId.Value > 0)
                    {

                        //var ProcessSeqLines = (from p in db.ProcessSequenceLine
                        //                       where p.ProcessSequenceHeaderId == pro.ProcessSequenceHeaderId
                        //                       select p).ToList();

                        IEnumerable<ProductProcessViewModel> ProcessSeqLines = new ProcessSequenceHeaderService(_unitOfWork).FGetProductProcessFromProcessSequence((int)pro.ProcessSequenceHeaderId);



                        foreach (var ProcSeqLin in ProcessSeqLines)
                        {
                            var ProductCategoryProcessSettings = new ProductCategoryProcessSettingsService(_unitOfWork).GetProductCategoryProcessSettings((int)pro.ProductCategoryId, (int)ProcSeqLin.ProcessId);
                            int? QAGroupId = null;
                            if (ProductCategoryProcessSettings != null)
                            {
                                QAGroupId = ProductCategoryProcessSettings.QAGroupId;
                            }

                            int? ProductRateGroupId = null;
                            if (ProcSeqLin != null)
                            {
                                if (ProcSeqLin.ProcessId != null && pro.ProductCategoryId != null)
                                {
                                    string ProcessName = new ProcessService(_unitOfWork).Find((int)ProcSeqLin.ProcessId).ProcessName;
                                    if (ProcessName == "Latexing Outside")
                                    {
                                        string ProductCategoryName = new ProductCategoryService(_unitOfWork).Find((int)pro.ProductCategoryId).ProductCategoryName;

                                        if (ProductCategoryName.ToUpper().Contains("HANDLOOM"))
                                        {
                                            ProductRateGroupId = new ProductRateGroupService(_unitOfWork).Find("Handloom").ProductRateGroupId;
                                        }
                                        else
                                        {
                                            ProductRateGroupId = new ProductRateGroupService(_unitOfWork).Find("Tufted").ProductRateGroupId;
                                        }
                                    }
                                    else
                                    {
                                        var TempProductRateGroup = (from Pp in db.ProductProcess
                                                                    join Pt in db.Product on Pp.ProductId equals Pt.ProductId into ProductTable
                                                                    from ProductTab in ProductTable.DefaultIfEmpty()
                                                                    where Pp.ProcessId == ProcSeqLin.ProcessId && ProductTab.ProductGroupId == pro.ProductGroupId
                                                                    select new { ProductRateGroupId = Pp.ProductRateGroupId }).FirstOrDefault();
                                        if (TempProductRateGroup != null)
                                            ProductRateGroupId = TempProductRateGroup.ProductRateGroupId;
                                    }
                                }
                            }

                            ProductProcess ProdProc = new ProductProcess()
                            {
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now,
                                ProcessId = ProcSeqLin.ProcessId,
                                QAGroupId = QAGroupId,
                                ProductRateGroupId = ProductRateGroupId,
                                Sr = ProcSeqLin.Sr,
                                ProductId = pro.ProductId,
                            };
                            ProdProc.ObjectState = Model.ObjectState.Added;
                            new ProductProcessService(_unitOfWork).Create(ProdProc);
                        }

                    }

                    //ForInserting RateListLines if rates are defined already

                    var exisitingRLLs = (from p in db.Product.AsNoTracking()
                                         join t in db.RateListLine.AsNoTracking() on p.ProductId equals t.ProductId
                                         where p.ProductGroupId == pro.ProductGroupId
                                         select t).ToList();

                    var RLHGroups = (from p in exisitingRLLs
                                     group p by p.RateListHeaderId
                                         into g
                                         select g).ToList();

                    foreach (var item in RLHGroups)
                    {
                        RateListLine Rll = new RateListLine();
                        Rll.ProductId = pro.ProductId;
                        Rll.Rate = item.Min(m => m.Rate);
                        Rll.Incentive = item.Min(m => m.Incentive);
                        Rll.Discount = item.Min(m => m.Discount);
                        Rll.Loss = item.Min(m => m.Loss);
                        Rll.RateListHeaderId = item.Key;
                        Rll.ModifiedBy = User.Identity.Name;
                        Rll.CreatedBy = User.Identity.Name;
                        Rll.ModifiedDate = DateTime.Now;
                        Rll.CreatedDate = DateTime.Now;

                        new RateListLineService(_unitOfWork).Create(Rll);

                    }

                    if (vm.SupplierId.HasValue && vm.SupplierId.Value > 0)
                    {

                        ProductSupplier ps = new ProductSupplier();


                        ps.Cost = vm.Cost;
                        ps.CreatedBy = User.Identity.Name;
                        ps.CreatedDate = DateTime.Now;
                        ps.Default = true;
                        ps.LeadTime = vm.LeadTime;
                        ps.MaximumOrderQty = vm.MaximumOrderQty;
                        ps.MinimumOrderQty = vm.MinimumOrderQty;
                        ps.ModifiedBy = User.Identity.Name;
                        ps.ModifiedDate = DateTime.Now;
                        ps.ProductId = pro.ProductId;
                        ps.SupplierId = vm.SupplierId.Value;
                        ps.ObjectState = Model.ObjectState.Added;

                        new ProductSupplierService(_unitOfWork).Create(ps);
                    }


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm);
                        return PartialView("EditSize", vm);

                    }
                    CarpetMasterViewModel temp = new CarpetMasterViewModel();
                    temp.ProductCategoryId = vm.ProductCategoryId;
                    temp.ProductCollectionId = vm.ProductCollectionId;
                    temp.ProductQualityId = vm.ProductQualityId;
                    temp.ProductDesignId = vm.ProductDesignId;
                    temp.ProductInvoiceGroupId = vm.ProductInvoiceGroupId;
                    temp.ProductStyleId = vm.ProductStyleId;
                    temp.ProductManufacturerId = vm.ProductManufacturerId;
                    temp.DrawBackTariffHeadId = vm.DrawBackTariffHeadId;
                    temp.ProcessSequenceHeaderId = vm.ProcessSequenceHeaderId;
                    temp.ProductionRemark = vm.ProductionRemark;
                    temp.ProductGroupId = vm.ProductGroupId;
                    temp.ProductGroupName = vm.ProductGroupName;
                    temp.DivisionId = vm.DivisionId;
                    temp.OriginCountryId = vm.OriginCountryId;
                    temp.ProductDesignPatternId = vm.ProductDesignPatternId;
                    temp.ColourId = vm.ColourId;
                    temp.ContentId = vm.ContentId;
                    temp.FaceContentId = vm.FaceContentId;
                    temp.SampleId = vm.SampleId;
                    temp.CounterNo = vm.CounterNo;
                    temp.DescriptionOfGoodsId = vm.DescriptionOfGoodsId;
                    temp.SalesTaxProductCodeId = vm.SalesTaxProductCodeId;
                    temp.StandardCost = vm.StandardCost;
                    temp.StandardWeight = vm.StandardWeight;
                    temp.GrossWeight = vm.GrossWeight;
                    temp.DivisionId = vm.DivisionId;
                    temp.OriginCountryId = vm.OriginCountryId;
                    temp.Tags = vm.Tags;
                    temp.ImageFileName = vm.ImageFileName;
                    temp.ImageFolderName = vm.ImageFolderName;
                    temp.IsSample = vm.IsSample;

                    return RedirectToAction("AddSize", temp);
                }
                //Edit part
                else
                {

                    var productsizes = _ProductSizeService.GetProductSizeForProduct(vm.ProductId);

                    ProductSize standardsize = _ProductSizeService.FindProductSize((int)ProductSizeTypeConstants.StandardSize, vm.ProductId);

                    if (standardsize != null)
                    {
                        standardsize.SizeId = vm.StandardSizeId;
                        _ProductSizeService.Update(standardsize);

                        UnitConversion StandardUnit;
                        string Mode;
                        CreateUnitConversion((byte)UnitConversionFors.Standard, vm.StandardSizeId, vm.ProductId, UnitConstants.SqYard, out StandardUnit, out Mode);
                        if (Mode == "Create")
                        {
                            new UnitConversionService(_unitOfWork).Create(StandardUnit);
                        }
                        else if (Mode == "Edit")
                        {
                            new UnitConversionService(_unitOfWork).Update(StandardUnit);
                        }


                        UnitConversion StandardUnitFT2;
                        string ModeFT2;
                        CreateUnitConversion((byte)UnitConversionFors.Standard, vm.StandardSizeId, vm.ProductId, UnitConstants.SqFeet, out StandardUnitFT2, out ModeFT2);
                        if (ModeFT2 == "Create")
                        {
                            new UnitConversionService(_unitOfWork).Create(StandardUnitFT2);
                        }
                        else if (ModeFT2 == "Edit")
                        {
                            new UnitConversionService(_unitOfWork).Update(StandardUnitFT2);
                        }

                    }
                    else
                    {
                        ProductSize stansize = new ProductSize();
                        stansize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.StandardSize);
                        stansize.SizeId = vm.StandardSizeId;
                        stansize.ProductId = vm.ProductId;
                        stansize.CreatedBy = User.Identity.Name;
                        stansize.CreatedDate = DateTime.Now;
                        stansize.ModifiedBy = User.Identity.Name;
                        stansize.ModifiedDate = DateTime.Now;
                        _ProductSizeService.Create(stansize);



                        //Saving StardardUnitConversionData
                        UnitConversion StandardUnit;
                        string Mode;
                        CreateUnitConversion((byte)UnitConversionFors.Standard, vm.StandardSizeId, vm.ProductId, UnitConstants.SqYard, out StandardUnit, out Mode);
                        if (Mode == "Create")
                        {
                            new UnitConversionService(_unitOfWork).Create(StandardUnit);
                        }
                        else if (Mode == "Edit")
                        {
                            new UnitConversionService(_unitOfWork).Update(StandardUnit);
                        }


                        //Saving StardardUnitConversionData-FT2
                        UnitConversion StandardUnitFT2;
                        string ModeFT2;
                        CreateUnitConversion((byte)UnitConversionFors.Standard, vm.StandardSizeId, vm.ProductId, UnitConstants.SqYard, out StandardUnitFT2, out ModeFT2);
                        if (ModeFT2 == "Create")
                        {
                            new UnitConversionService(_unitOfWork).Create(StandardUnitFT2);
                        }
                        else if (ModeFT2 == "Edit")
                        {
                            new UnitConversionService(_unitOfWork).Update(StandardUnitFT2);
                        }
                    }

                    ProductSize ManufacturingSize = _ProductSizeService.FindProductSize((int)ProductSizeTypeConstants.ManufacturingSize, vm.ProductId);

                    if (ManufacturingSize != null)
                    {
                        ManufacturingSize.SizeId = vm.ManufacturingSizeId;
                        _ProductSizeService.Update(ManufacturingSize);

                        UnitConversion ManufacturingUnit;
                        string MMode;
                        CreateUnitConversion((byte)UnitConversionFors.Manufacturing, vm.ManufacturingSizeId, vm.ProductId, UnitConstants.SqYard, out ManufacturingUnit, out MMode);
                        if (MMode == "Create")
                        {
                            new UnitConversionService(_unitOfWork).Create(ManufacturingUnit);
                        }
                        else if (MMode == "Edit")
                        {
                            new UnitConversionService(_unitOfWork).Update(ManufacturingUnit);
                        }
                    }
                    else
                    {
                        ProductSize ManuSize = new ProductSize();
                        ManuSize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.ManufacturingSize);
                        ManuSize.SizeId = vm.ManufacturingSizeId;
                        ManuSize.ProductId = vm.ProductId;
                        ManuSize.CreatedBy = User.Identity.Name;
                        ManuSize.CreatedDate = DateTime.Now;
                        ManuSize.ModifiedBy = User.Identity.Name;
                        ManuSize.ModifiedDate = DateTime.Now;
                        _ProductSizeService.Create(ManuSize);


                        //Saving ManufacturingUnitConversionData
                        UnitConversion ManufacturingUnit;
                        string MMode;
                        CreateUnitConversion((byte)UnitConversionFors.Manufacturing, vm.ManufacturingSizeId, vm.ProductId, UnitConstants.SqYard, out ManufacturingUnit, out MMode);
                        if (MMode == "Create")
                        {
                            new UnitConversionService(_unitOfWork).Create(ManufacturingUnit);
                        }
                        else if (MMode == "Edit")
                        {
                            new UnitConversionService(_unitOfWork).Update(ManufacturingUnit);
                        }

                    }

                    ProductSize FinishingSize = _ProductSizeService.FindProductSize((int)ProductSizeTypeConstants.FinishingSize, vm.ProductId);

                    if (FinishingSize != null)
                    {
                        FinishingSize.SizeId = vm.FinishingSizeId;
                        _ProductSizeService.Update(FinishingSize);


                        UnitConversion FinishingUnit;
                        string MMode;
                        CreateUnitConversion((byte)UnitConversionFors.Finishing, vm.FinishingSizeId, vm.ProductId, UnitConstants.SqYard, out FinishingUnit, out MMode);
                        if (MMode == "Create")
                        {
                            new UnitConversionService(_unitOfWork).Create(FinishingUnit);
                        }
                        else if (MMode == "Edit")
                        {
                            new UnitConversionService(_unitOfWork).Update(FinishingUnit);
                        }

                    }
                    else
                    {
                        ProductSize FinSize = new ProductSize();
                        FinSize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.FinishingSize);
                        FinSize.SizeId = vm.FinishingSizeId;
                        FinSize.ProductId = vm.ProductId;
                        FinSize.CreatedBy = User.Identity.Name;
                        FinSize.CreatedDate = DateTime.Now;
                        FinSize.ModifiedBy = User.Identity.Name;
                        FinSize.ModifiedDate = DateTime.Now;
                        _ProductSizeService.Create(FinSize);


                        //Saving FinishingUnitConversionData
                        UnitConversion FinishingUnit;
                        string FMode;
                        CreateUnitConversion((byte)UnitConversionFors.Finishing, vm.FinishingSizeId, vm.ProductId, UnitConstants.SqYard, out FinishingUnit, out FMode);
                        if (FMode == "Create")
                        {
                            new UnitConversionService(_unitOfWork).Create(FinishingUnit);
                        }
                        else if (FMode == "Edit")
                        {
                            new UnitConversionService(_unitOfWork).Update(FinishingUnit);
                        }
                    }




                    #region "Saving Unit Conversions for that units which are defined in Settings."
                    if (vm.CarpetSkuSettings.UnitConversions != null && vm.CarpetSkuSettings.UnitConversions != "")
                    {
                        string[] UnitConversionArr = vm.CarpetSkuSettings.UnitConversions.Split(',');

                        for (int i = 0; i < UnitConversionArr.Length; i++)
                        {
                            string ConversionUnit = UnitConversionArr[i];
                            Unit Unit = new UnitService(_unitOfWork).Find(ConversionUnit);


                            UnitConversion StandardUnitConversion_Settings;
                            string StandardMode_Settings;
                            CreateUnitConversion((byte)UnitConversionFors.Standard, vm.StandardSizeId, vm.ProductId, ConversionUnit, out StandardUnitConversion_Settings, out StandardMode_Settings);

                            if (StandardUnitConversion_Settings.ToQty == 0 || StandardUnitConversion_Settings.ToQty == null)
                            {
                                string ProductCategory = "";
                                if (vm.ProductCategoryId != null)
                                {
                                    ProductCategory productCategory = new ProductCategoryService(_unitOfWork).Find((int)vm.ProductCategoryId);
                                    ProductCategory = productCategory.ProductCategoryName;
                                }

                                //if (ProductCategory == "NEPALI")
                                //{
                                //    Size Size = new SizeService(_unitOfWork).Find(vm.StandardSizeId);
                                //    ModelState.AddModelError("StandardSizeId", "Unable to get unit conversion to " + Unit.UnitName + " for size " + Size.SizeName);
                                //}
                            }

                            if (StandardMode_Settings == "Create")
                            {
                                new UnitConversionService(_unitOfWork).Create(StandardUnitConversion_Settings);
                            }
                            else if (StandardMode_Settings == "Edit")
                            {
                                new UnitConversionService(_unitOfWork).Update(StandardUnitConversion_Settings);
                            }


                            UnitConversion ManufacturingUnitConversion_Settings;
                            string ManufacturingMode_Settings;
                            CreateUnitConversion((byte)UnitConversionFors.Manufacturing, vm.ManufacturingSizeId, vm.ProductId, ConversionUnit, out ManufacturingUnitConversion_Settings, out ManufacturingMode_Settings);


                            if (ManufacturingUnitConversion_Settings.ToQty == 0 || ManufacturingUnitConversion_Settings.ToQty == null)
                            {
                                string ProductCategory = "";
                                if (vm.ProductCategoryId != null)
                                {
                                    ProductCategory productCategory = new ProductCategoryService(_unitOfWork).Find((int)vm.ProductCategoryId);
                                    ProductCategory = productCategory.ProductCategoryName;
                                }

                                //if (ProductCategory == "NEPALI")
                                //{
                                //    Size Size = new SizeService(_unitOfWork).Find(vm.ManufacturingSizeId);
                                //    ModelState.AddModelError("ManufacturingSizeId", "Unable to get unit conversion to " + Unit.UnitName + " for size " + Size.SizeName);
                                //}
                            }


                            if (ManufacturingMode_Settings == "Create")
                            {
                                new UnitConversionService(_unitOfWork).Create(ManufacturingUnitConversion_Settings);
                            }
                            else if (ManufacturingMode_Settings == "Edit")
                            {
                                new UnitConversionService(_unitOfWork).Update(ManufacturingUnitConversion_Settings);
                            }


                            UnitConversion FinishingUnitConversion_Settings;
                            string FinishingMode_Settings;
                            CreateUnitConversion((byte)UnitConversionFors.Finishing, vm.FinishingSizeId, vm.ProductId, ConversionUnit, out FinishingUnitConversion_Settings, out FinishingMode_Settings);


                            if (FinishingUnitConversion_Settings.ToQty == 0 || FinishingUnitConversion_Settings.ToQty == null)
                            {
                                string ProductCategory = "";
                                if (vm.ProductCategoryId != null)
                                {
                                    ProductCategory productCategory = new ProductCategoryService(_unitOfWork).Find((int)vm.ProductCategoryId);
                                    ProductCategory = productCategory.ProductCategoryName;
                                }

                                //if (ProductCategory == "NEPALI")
                                //{
                                //    Size Size = new SizeService(_unitOfWork).Find(vm.FinishingSizeId);
                                //    ModelState.AddModelError("FinishingSizeId", "Unable to get unit conversion to " + Unit.UnitName + " for size " + Size.SizeName);
                                //}
                            }


                            if (FinishingMode_Settings == "Create")
                            {
                                new UnitConversionService(_unitOfWork).Create(FinishingUnitConversion_Settings);
                            }
                            else if (FinishingMode_Settings == "Edit")
                            {
                                new UnitConversionService(_unitOfWork).Update(FinishingUnitConversion_Settings);
                            }
                        }
                    }

                    #endregion




                    ProductSize StencilSize = _ProductSizeService.FindProductSize((int)ProductSizeTypeConstants.StencilSize, vm.ProductId);

                    if (vm.StencilSizeId != 0)
                    {
                        if (StencilSize != null)
                        {
                                StencilSize.SizeId = vm.StencilSizeId;
                                _ProductSizeService.Update(StencilSize);
                        }
                        else
                        {
                                ProductSize FinSize = new ProductSize();
                                FinSize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.StencilSize);
                                FinSize.SizeId = vm.StencilSizeId;
                                FinSize.ProductId = vm.ProductId;
                                FinSize.CreatedBy = User.Identity.Name;
                                FinSize.CreatedDate = DateTime.Now;
                                FinSize.ModifiedBy = User.Identity.Name;
                                FinSize.ModifiedDate = DateTime.Now;
                                _ProductSizeService.Create(FinSize);
                        }
                    }

                    ProductSize MapSize = _ProductSizeService.FindProductSize((int)ProductSizeTypeConstants.MapSize, vm.ProductId);

                    if (MapSize != null)
                    {
                        MapSize.SizeId = vm.MapSizeId;
                        _ProductSizeService.Update(MapSize);
                    }
                    else
                    {
                        ProductSize FinSize = new ProductSize();
                        FinSize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.MapSize);
                        FinSize.SizeId = vm.MapSizeId;
                        FinSize.ProductId = vm.ProductId;
                        FinSize.CreatedBy = User.Identity.Name;
                        FinSize.CreatedDate = DateTime.Now;
                        FinSize.ModifiedBy = User.Identity.Name;
                        FinSize.ModifiedDate = DateTime.Now;
                        _ProductSizeService.Create(FinSize);
                    }




                    //////////////////////For Saving Binding, Gachhai and PattiMuraiDurry Unit Conversion Detail///////////////////////////////////

                    int producttypeid = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;
                    List<ProductTypeAttributeViewModel> tem = vm.ProductTypeAttributes;



                    int UnitConversionBinding = (int)UnitConversionFors.Binding;
                    int UnitConversionGachhai = (int)UnitConversionFors.Gachhai;
                    int UnitConversionPattiMuraiDurry = (int)UnitConversionFors.PattiMuraiDurry;

                    if (tem != null)
                    {
                        foreach (var item in tem)
                        {


                            if ((item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Binding || item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Gachhai
                                || item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.PattiMuraiDurry))
                            {
                                Size FinishingSizeForPerimeter = new SizeService(_unitOfWork).Find(vm.FinishingSizeId);
                                decimal Length = Math.Floor((FinishingSizeForPerimeter.Length + (FinishingSizeForPerimeter.LengthFraction / 12)) * 2);
                                decimal Width = Math.Floor((FinishingSizeForPerimeter.Width + (FinishingSizeForPerimeter.WidthFraction / 12)) * 2);
                                decimal LenghtAndWidth = Math.Floor(((FinishingSizeForPerimeter.Length + (FinishingSizeForPerimeter.LengthFraction / 12)) * 2) + ((FinishingSizeForPerimeter.Width + (FinishingSizeForPerimeter.WidthFraction / 12)) * 2));

                                UnitConversion UnitConv = new UnitConversion();
                                UnitConv.CreatedBy = User.Identity.Name;
                                UnitConv.CreatedDate = DateTime.Now;
                                UnitConv.ModifiedBy = User.Identity.Name;
                                UnitConv.ModifiedDate = DateTime.Now;
                                UnitConv.ProductId = vm.ProductId;
                                UnitConv.FromQty = 1;
                                UnitConv.FromUnitId = UnitConstants.Pieces;
                                UnitConv.ToUnitId = UnitConstants.Feet;

                                if (vm.ProductShapeId == (int)ProductShapeConstants.Circle)
                                {
                                    UnitConv.ToQty = Math.Floor((FinishingSizeForPerimeter.Length + (FinishingSizeForPerimeter.LengthFraction / 12)) * (decimal)3.14);
                                }
                                else if (vm.ProductShapeId == (int)ProductShapeConstants.Rectangle || vm.ProductShapeId == (int)ProductShapeConstants.Square)
                                {
                                    if (item.DefaultValue == ProductTypeAttributeValuess.Length)
                                    {
                                        UnitConv.ToQty = Width;
                                    }
                                    if (item.DefaultValue == ProductTypeAttributeValuess.Width)
                                    {
                                        UnitConv.ToQty = Length;
                                    }
                                    if (item.DefaultValue == ProductTypeAttributeValuess.LengthAndWidth)
                                    {
                                        UnitConv.ToQty = LenghtAndWidth;
                                    }
                                }
                                else
                                {
                                    UnitConv.ToQty = LenghtAndWidth;
                                }


                                if (item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Binding)
                                {
                                    UnitConv.UnitConversionForId = (int)UnitConversionFors.Binding;

                                    UnitConversion BindingUnitConversion = (from U in db.UnitConversion
                                                                            where U.ProductId == vm.ProductId && U.UnitConversionForId == UnitConversionBinding
                                                                            select U).FirstOrDefault();


                                    if (BindingUnitConversion == null)
                                    {
                                        if (item.DefaultValue != ProductTypeAttributeValuess.NA)
                                        {
                                            new UnitConversionService(_unitOfWork).Create(UnitConv);
                                        }
                                    }
                                    else
                                    {
                                        if (item.DefaultValue != ProductTypeAttributeValuess.NA)
                                        {
                                            BindingUnitConversion.ToQty = UnitConv.ToQty;
                                            new UnitConversionService(_unitOfWork).Update(BindingUnitConversion);
                                        }
                                        else
                                        {
                                            new UnitConversionService(_unitOfWork).Delete(BindingUnitConversion);
                                        }
                                    }
                                }

                                if (item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.Gachhai)
                                {
                                    UnitConv.UnitConversionForId = (int)UnitConversionFors.Gachhai;

                                    UnitConversion GachhaiUnitConversion = (from U in db.UnitConversion
                                                                            where U.ProductId == vm.ProductId && U.UnitConversionForId == UnitConversionGachhai
                                                                            select U).FirstOrDefault();

                                    if (GachhaiUnitConversion == null)
                                    {
                                        if (item.DefaultValue != ProductTypeAttributeValuess.NA)
                                        {
                                            new UnitConversionService(_unitOfWork).Create(UnitConv);
                                        }
                                    }
                                    else
                                    {
                                        if (item.DefaultValue != ProductTypeAttributeValuess.NA)
                                        {
                                            GachhaiUnitConversion.ToQty = UnitConv.ToQty;
                                            new UnitConversionService(_unitOfWork).Update(GachhaiUnitConversion);
                                        }
                                        else
                                        {
                                            new UnitConversionService(_unitOfWork).Delete(GachhaiUnitConversion);
                                        }
                                    }
                                }

                                if (item.ProductTypeAttributeId == (int)ProductTypeAttributeTypess.PattiMuraiDurry)
                                {
                                    UnitConv.UnitConversionForId = (int)UnitConversionFors.PattiMuraiDurry;

                                    UnitConversion PattiMuraiDuryUnitConversion = (from U in db.UnitConversion
                                                                                   where U.ProductId == vm.ProductId && U.UnitConversionForId == UnitConversionPattiMuraiDurry
                                                                                   select U).FirstOrDefault();

                                    if (PattiMuraiDuryUnitConversion == null)
                                    {
                                        if (item.DefaultValue != ProductTypeAttributeValuess.NA)
                                        {
                                            new UnitConversionService(_unitOfWork).Create(UnitConv);
                                        }
                                    }
                                    else
                                    {
                                        if (item.DefaultValue != ProductTypeAttributeValuess.NA)
                                        {
                                            PattiMuraiDuryUnitConversion.ToQty = UnitConv.ToQty;
                                            new UnitConversionService(_unitOfWork).Update(PattiMuraiDuryUnitConversion);
                                        }
                                        else
                                        {
                                            new UnitConversionService(_unitOfWork).Delete(PattiMuraiDuryUnitConversion);
                                        }
                                    }
                                } 
                            }

                        }
                    }


                    //////////////////////End For Saving Binding, Gachhai and PattiMuraiDurry Unit Conversion Detail///////////////////////////////////



                    //var temp = _ProductService.Find(vm.ProductId);

                    //temp.ProductCode = vm.ProductCode;
                    //temp.ProductName = vm.ProductName;
                    //temp.ProductDescription = vm.ProductDescription;



                    ////Code For Saving Product Specification
                    //string StandardSize = "";
                    //if (vm.StandardSizeId != 0)
                    //{
                    //    StandardSize = new SizeService(_unitOfWork).Find(vm.StandardSizeId).SizeName;
                    //}
                    //temp.ProductSpecification = StandardSize;

                    //_ProductService.Update(temp);






                    var FinishedProductTemp = new FinishedProductService(_unitOfWork).Find(vm.ProductId);
                    FinishedProductTemp.ProductCode = vm.ProductCode;
                    FinishedProductTemp.ProductName = vm.ProductName;
                    FinishedProductTemp.ProductDescription = vm.ProductDescription;
                    FinishedProductTemp.CBM = vm.CBM;
                    FinishedProductTemp.ColourId = vm.ColourId;

                    //Code For Saving Product Specification
                    string StandardSize = "";
                    if (vm.StandardSizeId != 0)
                    {
                        StandardSize = new SizeService(_unitOfWork).Find(vm.StandardSizeId).SizeName;
                    }
                    FinishedProductTemp.ProductSpecification = StandardSize;

                    FinishedProductTemp.TraceType = vm.TraceType;
                    FinishedProductTemp.MapType = vm.MapType;
                    FinishedProductTemp.MapScale = vm.MapScale;
                    new FinishedProductService(_unitOfWork).Update(FinishedProductTemp);


                    if (vm.TraceType != "N/A")
                    {
                        //int ProductDesignNAId = new ProductDesignService(_unitOfWork).Find("NA").ProductDesignId;

                        //if (FinishedProductTemp.ProductDesignId != null && FinishedProductTemp.ProductDesignId != ProductDesignNAId)
                        //{
                        //    CreateTrace((int)FinishedProductTemp.ProductDesignId, (int)FinishedProductTemp.ProductGroupId, vm.StandardSizeId, vm.StencilSizeId, FinishedProductTemp.ProductName, -1);
                        //}
                        //else
                        //{
                        //    CreateTraceForProduct(vm.ProductName, (int)vm.StencilSizeId, -1);
                        //}
                    }

                    if (vm.MapType != "N/A")
                    {
                        CreateMap(vm.ProductName, vm.MapSizeId, -2);
                    }


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm);
                        return PartialView("EditSize", vm);
                    }

                    return Json(new { success = true });
                }
            }
            PrepareViewBag(vm);
            return PartialView("EditSize", vm);
        }

        // POST: /CarpetMasterMaster/Delete/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedLine(CarpetMasterViewModel vm)
        {
            int productId = vm.ProductId;


            var bomlist = new BomDetailService(_unitOfWork).GetBomDetailList(productId);

            foreach (var item in bomlist)
            {
                BomDetail b = new BomDetailService(_unitOfWork).Find(item.BomDetailId);
                new BomDetailService(_unitOfWork).Delete(b);

            }


            var sizelist = _ProductSizeService.GetProductSizeForProduct(productId);

            foreach (var item in sizelist)
            {
                ProductSize si = _ProductSizeService.Find(item.ProductSizeId);
                _ProductSizeService.Delete(si);

            }

            var productattributes = new ProductAttributeService(_unitOfWork).GetProductAttributesWithPid(vm.ProductId);

            foreach (var ite2 in productattributes)
            {
                new ProductAttributeService(_unitOfWork).Delete(ite2.ProductAttributeId);
            }

            var productbuyers = new ProductBuyerService(_unitOfWork).GetProductBuyerList(vm.ProductId);

            foreach (var item3 in productbuyers)
            {
                new ProductBuyerService(_unitOfWork).Delete(item3.ProductBuyerId);
            }


            var unitconversions = new UnitConversionService(_unitOfWork).GetProductUnitConversions(vm.ProductId);

            foreach (var item4 in unitconversions)
            {
                new UnitConversionService(_unitOfWork).Delete(item4.UnitConversionId);
            }

            var ProductProcess = (from p in db.ProductProcess
                                  where p.ProductId == productId
                                  select p).ToList();

            foreach (var item4 in ProductProcess)
            {
                new ProductProcessService(_unitOfWork).Delete(item4.ProductProcessId);
            }


            _ProductService.Delete(productId);

            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                PrepareViewBag(vm);
                ModelState.AddModelError("", message);
                return PartialView("EditSize", vm);
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public ActionResult Delete(int id)//ProductGroupId
        {

            var group = _ProductGroupService.GetProductGroup(id);

            if (group == null)
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
                var productslist = _ProductService.GetProductListForGroup(vm.id);
                var group = _ProductGroupService.Find(vm.id);
                foreach (var item in productslist)
                {
                    var productsize = _ProductSizeService.GetProductSizeForProduct(item.ProductId);
                    foreach (var ite in productsize)
                    {
                        _ProductSizeService.Delete(ite.ProductSizeId);
                    }

                    var productattributes = new ProductAttributeService(_unitOfWork).GetProductAttributesWithPid(item.ProductId);

                    foreach (var ite2 in productattributes)
                    {
                        new ProductAttributeService(_unitOfWork).Delete(ite2.ProductAttributeId);
                    }

                    var ProductProcess = (from p in db.ProductProcess
                                          where p.ProductId == item.ProductId
                                          select p).ToList();

                    foreach (var item3 in ProductProcess)
                    {
                        new ProductProcessService(_unitOfWork).Delete(item3.ProductProcessId);
                    }


                    var UnitConversions = (from p in db.UnitConversion
                                           where p.ProductId == item.ProductId
                                           select p).ToList();

                    foreach (var item3 in UnitConversions)
                    {
                        new UnitConversionService(_unitOfWork).Delete(item3);
                    }

                    _ProductService.Delete(item.ProductId);



                }
                Dimension2 Dim2 = new Dimension2Service(_unitOfWork).Find(group.ProductGroupName);
                new Dimension2Service(_unitOfWork).Delete(Dim2);

                _ProductGroupService.Delete(vm.id);


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

                string uploadfolder = group.ImageFolderName;
                string tempfilename = group.ImageFileName;

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

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteGroup(int id)
        //{
        //    var productslist = _ProductService.GetProductListForGroup(id);
        //    var group=_ProductGroupService.Find(id);
        //    foreach (var item in productslist)
        //    {
        //        var productsize = _ProductSizeService.GetProductSizeForProduct(item.ProductId);
        //        foreach (var ite in productsize)
        //        {
        //            _ProductSizeService.Delete(ite.ProductSizeId);
        //        }

        //        var productattributes = new ProductAttributeService(_unitOfWork).GetProductAttributesWithPid(item.ProductId);

        //        foreach(var ite2 in productattributes)
        //        {
        //            new ProductAttributeService(_unitOfWork).Delete(ite2.ProductAttributeId);
        //        }


        //        _ProductService.Delete(item.ProductId);
        //    }
        //    _ProductGroupService.Delete(id);
        //    _unitOfWork.Save();

        //    string uploadfolder = group.ImageFolderName;
        //    string tempfilename = group.ImageFileName;

        //    var xtemp = System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/" + tempfilename);
        //    if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/" + tempfilename)))
        //    {
        //        System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/" + tempfilename));
        //    }

        //    //Deleting Thumbnail Image:

        //    if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Thumbs/" + tempfilename)))
        //    {
        //        System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Thumbs/" + tempfilename));
        //    }

        //    //Deleting Medium Image:
        //    if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Medium/" + tempfilename)))
        //    {
        //        System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Medium/" + tempfilename));
        //    }

        //    return RedirectToAction("Index");
        //}






        //Function Saves Data in Unit Conversion Multiplier

        public void CreateUnitConversion(byte UnitConversionFor, int SizeId, int ProductId, string ToUnit, out UnitConversion UnitConvr, out string Mode)
        {
            decimal AreaFT2 = Convert.ToDecimal(new SizeService(_unitOfWork).Find(SizeId).Area);

            Size s = new SizeService(_unitOfWork).Find(SizeId);

            if (s.UnitId == "MET")
            { 
            using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
            {
                sqlConnection.Open();

                SqlCommand Totalf = new SqlCommand("SELECT * FROM Web.FuncGetSqFeetFromCMSizes( " + SizeId + ")", sqlConnection);

                AreaFT2 = Convert.ToDecimal(Totalf.ExecuteScalar() == DBNull.Value ? 0 : Totalf.ExecuteScalar());
            }
            }

            UnitConversion SizeExist = new UnitConversionService(_unitOfWork).GetUnitConversion(ProductId, UnitConversionFor, ToUnit);

            if (SizeExist != null)
            {

                if (ToUnit == UnitConstants.SqYard)
                {
                    using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
                    {
                        sqlConnection.Open();

                        SqlCommand Totalf = new SqlCommand("SELECT * FROM Web.FuncConvertSqFeetToSqYard( " + AreaFT2 + ")", sqlConnection);

                        SizeExist.ToQty = Convert.ToDecimal(Totalf.ExecuteScalar() == DBNull.Value ? 0 : Totalf.ExecuteScalar());
                    }
                }
                else if (ToUnit == UnitConstants.SqFeet)
                {
                    SizeExist.ToQty = AreaFT2;
                }
                else 
                {
                    SizeExist.ToQty = GetUnitConversionQty(SizeId, ToUnit);
                }

                UnitConvr = SizeExist;
                Mode = "Edit";
            }
            else
            {

                UnitConversion UnitConv = new UnitConversion();
                UnitConv.CreatedBy = User.Identity.Name;
                UnitConv.CreatedDate = DateTime.Now;
                UnitConv.ModifiedBy = User.Identity.Name;
                UnitConv.ModifiedDate = DateTime.Now;
                UnitConv.ProductId = ProductId;
                UnitConv.FromQty = 1;
                UnitConv.FromUnitId = UnitConstants.Pieces;
                UnitConv.ToUnitId = ToUnit;
                UnitConv.UnitConversionForId = UnitConversionFor;


                if (ToUnit == UnitConstants.SqYard)
                {
                    using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
                    {
                        sqlConnection.Open();

                        SqlCommand Totalf = new SqlCommand("SELECT * FROM Web.FuncConvertSqFeetToSqYard( " + AreaFT2 + ")", sqlConnection);

                        UnitConv.ToQty = Convert.ToDecimal(Totalf.ExecuteScalar() == DBNull.Value ? 0 : Totalf.ExecuteScalar());
                    }
                }
                else if (ToUnit == UnitConstants.SqFeet)
                {
                    UnitConv.ToQty = AreaFT2;
                }
                else
                {
                    UnitConv.ToQty = GetUnitConversionQty(SizeId, ToUnit);
                }

                Mode = "Create";
                UnitConvr = UnitConv;
            }
        }

        public JsonResult GetProductShapeShortName(int ProductShapeId)
        {
            string ProductShapeShortName = "";
            var temp = (from H in db.ProductShape
                        where H.ProductShapeId == ProductShapeId
                        select new { ProductShapeShortName = H.ProductShapeShortName }).FirstOrDefault();

            if (temp.ProductShapeShortName != null)
            {
                ProductShapeShortName = temp.ProductShapeShortName;
            }


            return Json(ProductShapeShortName);
        }

        public void CreateTraceForProduct(string ProductName, int StencilSizeId, int TraceProductId)
        {
            string TraceName = "";
            TraceName = ProductName + "-Trace";

            var temp = (from P in db.Product where P.ProductName == TraceName select P).FirstOrDefault();

            if (temp == null)
            {
                Product ProductTrace = new Product();
                ProductTrace.ProductId = TraceProductId;
                ProductTrace.ProductName = TraceName;
                if (TraceName.Length <= 20)
                {
                    ProductTrace.ProductCode = TraceName;
                }
                else
                {
                    ProductTrace.ProductCode = TraceName.Substring(0, 20);
                }

                ProductTrace.ProductGroupId = new ProductGroupService(_unitOfWork).Find(ProductGroupConstants.Trace).ProductGroupId;
                ProductTrace.UnitId = UnitConstants.Pieces;
                ProductTrace.IsActive = true;
                ProductTrace.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                ProductTrace.CreatedDate = DateTime.Now;
                ProductTrace.ModifiedDate = DateTime.Now;
                ProductTrace.CreatedBy = User.Identity.Name;
                ProductTrace.ModifiedBy = User.Identity.Name;
                ProductTrace.IsActive = true;

                ProductTrace.ObjectState = Model.ObjectState.Added;
                new ProductService(_unitOfWork).Create(ProductTrace);


                UnitConversion UnitConvTrace = new UnitConversion();
                UnitConvTrace.UnitConversionId = TraceProductId;
                UnitConvTrace.CreatedBy = User.Identity.Name;
                UnitConvTrace.CreatedDate = DateTime.Now;
                UnitConvTrace.ModifiedBy = User.Identity.Name;
                UnitConvTrace.ModifiedDate = DateTime.Now;
                UnitConvTrace.ProductId = ProductTrace.ProductId;
                UnitConvTrace.FromQty = 1;
                UnitConvTrace.FromUnitId = UnitConstants.Pieces;
                UnitConvTrace.ToUnitId = UnitConstants.SqYard;
                UnitConvTrace.UnitConversionForId = (byte)UnitConversionFors.Standard;

                //For Calculating Area in Sq.Yard.
                Decimal AreaInSqFeet = new SizeService(_unitOfWork).Find(StencilSizeId).Area;
                UnitConvTrace.ToQty = FConvertSqFeetToSqYard(AreaInSqFeet);

                new UnitConversionService(_unitOfWork).Create(UnitConvTrace);




                ProductSize Stencilsize = new ProductSize();
                Stencilsize.ProductSizeId = TraceProductId;
                Stencilsize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.StandardSize);
                Stencilsize.SizeId = StencilSizeId;
                Stencilsize.ProductId = ProductTrace.ProductId;
                Stencilsize.CreatedBy = User.Identity.Name;
                Stencilsize.CreatedDate = DateTime.Now;
                Stencilsize.ModifiedBy = User.Identity.Name;
                Stencilsize.ModifiedDate = DateTime.Now;
                Stencilsize.IsActive = true;
                new ProductSizeService(_unitOfWork).Create(Stencilsize);




            }
        }

        //public void CreateTrace(int ColourWaysId, int ProductGroupId, int StandardSizeId, int StencilSizeId, int TraceProductId)
        //{
        //    string TraceName = "";

        //    string ColourWaysName = (from H in db.ProductDesigns where H.ProductDesignId == ColourWaysId select H).FirstOrDefault().ProductDesignName;

        //    Size Size = new SizeService(_unitOfWork).Find(StandardSizeId);

        //    string SizeName = "";
        //    SizeName = Size.Length.ToString();
        //    if (Size.LengthFraction != 0 && Size.LengthFraction != null) SizeName = SizeName + Size.LengthFraction.ToString();
        //    SizeName = Size.Width.ToString();
        //    if (Size.WidthFraction != 0 && Size.WidthFraction != null) SizeName = SizeName + Size.WidthFraction.ToString();


        //    int ColourWaysDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ProductDesign).DocumentTypeId;

        //    string ProductGroupName = "";
        //    if (ProductGroupId != 0 && ProductGroupId != null)
        //    {
        //        ProductGroupName = (from H in db.ProductGroups where H.ProductGroupId == ProductGroupId select H).FirstOrDefault().ProductGroupName;
        //    }


        //    var FirstProductGroup = (from H in db.FinishedProduct
        //                             where H.ProductDesignId == ColourWaysId
        //                             orderby H.ProductId
        //                             select new
        //                             {
        //                                 H.ProductGroup.ProductGroupName
        //                             }).FirstOrDefault();

        //    if (FirstProductGroup != null)
        //    {
        //        TraceName = FirstProductGroup.ProductGroupName + "-Trace-" + SizeName;
        //    }
        //    else
        //    {
        //        TraceName = ProductGroupName + "-Trace-" + SizeName;
        //    }




        //    var temp = (from P in db.Product where P.ReferenceDocId == ColourWaysId && P.ReferenceDocTypeId == ColourWaysDocTypeId select P).FirstOrDefault();

        //    if (temp == null)
        //    {
        //        Product ProductTrace = new Product();
        //        ProductTrace.ProductId = TraceProductId;
        //        ProductTrace.ProductName = TraceName;
        //        if (TraceName.Length <= 20)
        //        {
        //            ProductTrace.ProductCode = TraceName;
        //        }
        //        else
        //        {
        //            ProductTrace.ProductCode = TraceName.Substring(0, 20);
        //        }

        //        ProductTrace.ProductGroupId = new ProductGroupService(_unitOfWork).Find(ProductGroupConstants.Trace).ProductGroupId;
        //        ProductTrace.UnitId = UnitConstants.Pieces;
        //        ProductTrace.ReferenceDocId = ColourWaysId;
        //        ProductTrace.ReferenceDocTypeId = ColourWaysDocTypeId;
        //        ProductTrace.IsActive = true;
        //        ProductTrace.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
        //        ProductTrace.CreatedDate = DateTime.Now;
        //        ProductTrace.ModifiedDate = DateTime.Now;
        //        ProductTrace.CreatedBy = User.Identity.Name;
        //        ProductTrace.ModifiedBy = User.Identity.Name;
        //        ProductTrace.IsActive = true;

        //        ProductTrace.ObjectState = Model.ObjectState.Added;
        //        new ProductService(_unitOfWork).Create(ProductTrace);


        //        UnitConversion UnitConvTrace = new UnitConversion();
        //        UnitConvTrace.UnitConversionId = TraceProductId;
        //        UnitConvTrace.CreatedBy = User.Identity.Name;
        //        UnitConvTrace.CreatedDate = DateTime.Now;
        //        UnitConvTrace.ModifiedBy = User.Identity.Name;
        //        UnitConvTrace.ModifiedDate = DateTime.Now;
        //        UnitConvTrace.ProductId = ProductTrace.ProductId;
        //        UnitConvTrace.FromQty = 1;
        //        UnitConvTrace.FromUnitId = UnitConstants.Pieces;
        //        UnitConvTrace.ToUnitId = UnitConstants.SqYard;
        //        UnitConvTrace.UnitConversionForId = (byte)UnitConversionFors.Standard;

        //        //For Calculating Area in Sq.Yard.
        //        Decimal AreaInSqFeet = new SizeService(_unitOfWork).Find(StencilSizeId).Area;
        //        UnitConvTrace.ToQty = FConvertSqFeetToSqYard(AreaInSqFeet);

        //        new UnitConversionService(_unitOfWork).Create(UnitConvTrace);




        //        ProductSize Stencilsize = new ProductSize();
        //        Stencilsize.ProductSizeId = TraceProductId;
        //        Stencilsize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.StandardSize);
        //        Stencilsize.SizeId = StencilSizeId;
        //        Stencilsize.ProductId = ProductTrace.ProductId;
        //        Stencilsize.CreatedBy = User.Identity.Name;
        //        Stencilsize.CreatedDate = DateTime.Now;
        //        Stencilsize.ModifiedBy = User.Identity.Name;
        //        Stencilsize.ModifiedDate = DateTime.Now;
        //        Stencilsize.IsActive = true;
        //        new ProductSizeService(_unitOfWork).Create(Stencilsize);

        //    }
        //}


        public void CreateTrace(int ColourWaysId, int ProductGroupId, int StandardSizeId, int StencilSizeId, string ProductName, int TraceProductId)
        {
            string TraceName = "";

            int ColourWaysDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ProductDesign).DocumentTypeId;

            SqlParameter SqlParameterColourWaysId = new SqlParameter("@ProductDesignId", ColourWaysId);
            SqlParameter SqlParameterStandardSizeId = new SqlParameter("@StandardSizeID", StandardSizeId);

            FirstProductName FirstProduct = db.Database.SqlQuery<FirstProductName>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetFirstProductForColourWayAndSize @ProductDesignId, @StandardSizeID", SqlParameterColourWaysId, SqlParameterStandardSizeId).FirstOrDefault();

            if (FirstProduct != null)
            {
                TraceName = FirstProduct.ProductName + "-Trace";
            }
            else
            {
                TraceName = ProductName + "-Trace";
            }


            var temp = (from P in db.Product where P.ProductName == TraceName select P).FirstOrDefault();

            if (temp == null)
            {
                Product ProductTrace = new Product();
                ProductTrace.ProductId = TraceProductId;
                ProductTrace.ProductName = TraceName;
                if (TraceName.Length <= 20)
                {
                    ProductTrace.ProductCode = TraceName;
                }
                else
                {
                    ProductTrace.ProductCode = TraceName.Substring(0, 20);
                }

                ProductTrace.ProductGroupId = new ProductGroupService(_unitOfWork).Find(ProductGroupConstants.Trace).ProductGroupId;
                ProductTrace.UnitId = UnitConstants.Pieces;
                ProductTrace.ReferenceDocId = ColourWaysId;
                ProductTrace.ReferenceDocTypeId = ColourWaysDocTypeId;
                ProductTrace.IsActive = true;
                ProductTrace.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                ProductTrace.CreatedDate = DateTime.Now;
                ProductTrace.ModifiedDate = DateTime.Now;
                ProductTrace.CreatedBy = User.Identity.Name;
                ProductTrace.ModifiedBy = User.Identity.Name;
                ProductTrace.IsActive = true;

                ProductTrace.ObjectState = Model.ObjectState.Added;
                new ProductService(_unitOfWork).Create(ProductTrace);


                UnitConversion UnitConvTrace = new UnitConversion();
                UnitConvTrace.UnitConversionId = TraceProductId;
                UnitConvTrace.CreatedBy = User.Identity.Name;
                UnitConvTrace.CreatedDate = DateTime.Now;
                UnitConvTrace.ModifiedBy = User.Identity.Name;
                UnitConvTrace.ModifiedDate = DateTime.Now;
                UnitConvTrace.ProductId = ProductTrace.ProductId;
                UnitConvTrace.FromQty = 1;
                UnitConvTrace.FromUnitId = UnitConstants.Pieces;
                UnitConvTrace.ToUnitId = UnitConstants.SqYard;
                UnitConvTrace.UnitConversionForId = (byte)UnitConversionFors.Standard;

                //For Calculating Area in Sq.Yard.
                Decimal AreaInSqFeet = new SizeService(_unitOfWork).Find(StencilSizeId).Area;
                UnitConvTrace.ToQty = FConvertSqFeetToSqYard(AreaInSqFeet);

                new UnitConversionService(_unitOfWork).Create(UnitConvTrace);




                ProductSize Stencilsize = new ProductSize();
                Stencilsize.ProductSizeId = TraceProductId;
                Stencilsize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.StandardSize);
                Stencilsize.SizeId = StencilSizeId;
                Stencilsize.ProductId = ProductTrace.ProductId;
                Stencilsize.CreatedBy = User.Identity.Name;
                Stencilsize.CreatedDate = DateTime.Now;
                Stencilsize.ModifiedBy = User.Identity.Name;
                Stencilsize.ModifiedDate = DateTime.Now;
                Stencilsize.IsActive = true;
                new ProductSizeService(_unitOfWork).Create(Stencilsize);


                if (TraceName != ProductName + "-Trace")
                {
                    int ProductAliasDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.ProductAlias).DocumentTypeId;

                    ProductAlias ProductAlias = new ProductAlias();
                    ProductAlias.DocTypeId = ProductAliasDocTypeId;
                    ProductAlias.ProductAliasName = ProductName + "-Trace";
                    ProductAlias.ProductId = ProductTrace.ProductId;
                    ProductAlias.Status = 0;
                    ProductAlias.IsActive = true;

                    ProductAlias.CreatedDate = DateTime.Now;
                    ProductAlias.ModifiedDate = DateTime.Now;
                    ProductAlias.CreatedBy = User.Identity.Name;
                    ProductAlias.ModifiedBy = User.Identity.Name;

                    ProductAlias.ObjectState = Model.ObjectState.Added;
                    new ProductAliasService(_unitOfWork).Create(ProductAlias);
                }


            }
        }

        public void CreateMap(string ProductName, int MapSizeId, int MapProductId)
        {
            string MapName = "";
            MapName = ProductName + "-Map";

            var temp = (from P in db.Product where P.ProductName == MapName select P).FirstOrDefault();

            if (temp == null)
            {
                Product ProductMap = new Product();
                ProductMap.ProductId = MapProductId;
                ProductMap.ProductName = MapName;
                if (MapName.Length <= 50)
                {
                    ProductMap.ProductCode = MapName;
                }
                else
                {
                    ProductMap.ProductCode = MapName.Substring(0, 50);
                }
                ProductMap.ProductGroupId = new ProductGroupService(_unitOfWork).Find(ProductGroupConstants.Map).ProductGroupId;
                ProductMap.UnitId = UnitConstants.Pieces;
                ProductMap.IsActive = true;
                ProductMap.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                ProductMap.CreatedDate = DateTime.Now;
                ProductMap.ModifiedDate = DateTime.Now;
                ProductMap.CreatedBy = User.Identity.Name;
                ProductMap.ModifiedBy = User.Identity.Name;
                ProductMap.IsActive = true;

                ProductMap.ObjectState = Model.ObjectState.Added;
                new ProductService(_unitOfWork).Create(ProductMap);





                UnitConversion UnitConvMap = new UnitConversion();
                UnitConvMap.UnitConversionId = MapProductId;
                UnitConvMap.CreatedBy = User.Identity.Name;
                UnitConvMap.CreatedDate = DateTime.Now;
                UnitConvMap.ModifiedBy = User.Identity.Name;
                UnitConvMap.ModifiedDate = DateTime.Now;
                UnitConvMap.ProductId = ProductMap.ProductId;
                UnitConvMap.FromQty = 1;
                UnitConvMap.FromUnitId = UnitConstants.Pieces;
                UnitConvMap.ToUnitId = UnitConstants.SqYard;
                UnitConvMap.UnitConversionForId = (byte)UnitConversionFors.Standard;
                //UnitConvMap.ToQty = new SizeService(_unitOfWork).Find(MapSizeId).Area;


                //For Calculating Area in Sq.Yard.
                Decimal AreaInSqFeet = new SizeService(_unitOfWork).Find(MapSizeId).Area;
                UnitConvMap.ToQty = FConvertSqFeetToSqYard(AreaInSqFeet);

                new UnitConversionService(_unitOfWork).Create(UnitConvMap);


                ProductSize Mapsize = new ProductSize();
                Mapsize.ProductSizeId = MapProductId;
                Mapsize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.StandardSize);
                Mapsize.SizeId = MapSizeId;
                Mapsize.ProductId = ProductMap.ProductId;
                Mapsize.CreatedBy = User.Identity.Name;
                Mapsize.CreatedDate = DateTime.Now;
                Mapsize.ModifiedBy = User.Identity.Name;
                Mapsize.ModifiedDate = DateTime.Now;
                Mapsize.IsActive = true;
                new ProductSizeService(_unitOfWork).Create(Mapsize);
            }
        }

        public Decimal FConvertSqFeetToSqYard(Decimal SqFeet)
        {
            Decimal SqYard = 0;
            using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
            {
                sqlConnection.Open();
                SqlCommand Totalf = new SqlCommand("SELECT * FROM Web.FuncConvertSqFeetToSqYard( " + SqFeet + ")", sqlConnection);
                SqYard = Convert.ToDecimal(Totalf.ExecuteScalar() == DBNull.Value ? 0 : Totalf.ExecuteScalar());
            }
            return SqYard;
        }

        [HttpGet]
        public JsonResult ProductProcessIndex(int id)//ProductGroup id
        {
            var TEMP = new ProductProcessService(_unitOfWork).GetMaxProductProcessListForDesign(id).ToList();

            return Json(TEMP, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditProductProcess(int id)
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

            return PartialView("EditProductProcess", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProductProcess(ProductProcessViewModel vm)
        {
            if (ModelState.IsValid)
            {
                //var ProdProcess = db.ProductProcess.Find(vm.ProductProcessId);

                var Product = db.Product.Find(vm.ProductId);

                var ProdProcRecords = new ProductProcessService(_unitOfWork).GetProductProcessListForDesign(Product.ProductGroupId.Value);

                var ProcFIlterdRecords = ProdProcRecords.Where(m => m.ProcessId == vm.ProcessId).ToList();

                foreach (var item in ProcFIlterdRecords)
                {
                    item.Instructions = vm.Instructions;
                    item.ProductRateGroupId = vm.ProductRateGroupId;
                    item.QAGroupId = vm.QAGroupId;
                    item.ObjectState = Model.ObjectState.Modified;
                    db.ProductProcess.Add(item);
                }

                try
                {
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("EditProductProcess", vm);
                }

                return Json(new { success = true });

            }

            return PartialView("EditProductProcess", vm);
        }

        public JsonResult GetCustomProductName(string ProductGroupName, string StandardSizeName, string ManufacturingSizeName, string ColourName)
        {
            FirstProductName CustomProductName = MakeCustomProductName(ProductGroupName, StandardSizeName, ManufacturingSizeName, ColourName);
            return Json(CustomProductName);
        }

        public FirstProductName MakeCustomProductName(string ProductGroupName, string StandardSizeName, string ManufacturingSizeName, string ColourName)
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            CarpetSkuSettings temp = new CarpetSkuSettingsService(_unitOfWork).GetCarpetSkuSettings(DivisionId, SiteId);

            string SizeName = "";
            if (temp.NameBaseOnSize == "ManufacturingSizeName")
                SizeName = ManufacturingSizeName;
            else
                SizeName = StandardSizeName;

            SqlParameter SqlParameterProductGroupName = new SqlParameter("@ProductGroupName", ProductGroupName);
            SqlParameter SqlParameterStandardSizeName = new SqlParameter("@StandardSizeName", SizeName);
            SqlParameter SqlParameterColourName = new SqlParameter("@ColourName", ColourName);

            FirstProductName CustomProductName = db.Database.SqlQuery<FirstProductName>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetCustomCarpetSkuName @ProductGroupName, @StandardSizeName, @ColourName", SqlParameterProductGroupName, SqlParameterStandardSizeName, SqlParameterColourName).FirstOrDefault();

            return CustomProductName;
        }





        public Decimal GetUnitConversionQty(int SizeId, string ToUnit, string Attribute = null)
        {
            SqlParameter SqlParameterSizeId = new SqlParameter("@SizeId", SizeId);
            SqlParameter SqlParameterToUnitId = new SqlParameter("@ToUnitId", ToUnit);
            SqlParameter SqlParameterAttribute = new SqlParameter("@Attribute", Attribute);
            if (Attribute != null)
            {
                UnitConversionQty UnitConversionQty = db.Database.SqlQuery<UnitConversionQty>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetUnitConversionForSize @SizeId, @ToUnitId, @Attribute", SqlParameterSizeId, SqlParameterToUnitId, SqlParameterAttribute).FirstOrDefault();
                if (UnitConversionQty != null)
                {
                    return UnitConversionQty.ToQty ?? 0;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                UnitConversionQty UnitConversionQty = db.Database.SqlQuery<UnitConversionQty>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetUnitConversionForSize @SizeId, @ToUnitId", SqlParameterSizeId, SqlParameterToUnitId).FirstOrDefault();
                if (UnitConversionQty != null)
                {
                    return UnitConversionQty.ToQty ?? 0;
                }
                else
                {
                    return 0;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSizePostWithMultipleColours(CarpetMasterViewModel vm, FormCollection collection)
        {
            string[] ColourArr = vm.ColourIdList.Split(',');


            for (int i = 0; i <= ColourArr.Length - 1; i++)
            {
                vm.ColourId = Convert.ToInt32(ColourArr[i]);


                var ProductGroup = new ProductGroupService(_unitOfWork).Find(vm.ProductGroupId);
                var StandardSize = new SizeService(_unitOfWork).Find(vm.StandardSizeId);
                var ManufacturingSize = new SizeService(_unitOfWork).Find(vm.ManufacturingSizeId);
                var Colour = new ColourService(_unitOfWork).Find(vm.ColourId);

                var ProductResult = MakeCustomProductName(ProductGroup.ProductGroupName, StandardSize.SizeName, ManufacturingSize.SizeName, Colour.ColourName);
                vm.ProductCode = ProductResult.ProductName;
                vm.ProductName = ProductResult.ProductName;

                EditSizePost(vm, collection);
            }
            CarpetMasterViewModel temp = new CarpetMasterViewModel();
            temp.ProductCategoryId = vm.ProductCategoryId;
            temp.ProductCollectionId = vm.ProductCollectionId;
            temp.ProductQualityId = vm.ProductQualityId;
            temp.ProductDesignId = vm.ProductDesignId;
            temp.ProductInvoiceGroupId = vm.ProductInvoiceGroupId;
            temp.ProductStyleId = vm.ProductStyleId;
            temp.ProductManufacturerId = vm.ProductManufacturerId;
            temp.DrawBackTariffHeadId = vm.DrawBackTariffHeadId;
            temp.ProcessSequenceHeaderId = vm.ProcessSequenceHeaderId;
            temp.ProductionRemark = vm.ProductionRemark;
            temp.ProductGroupId = vm.ProductGroupId;
            temp.ProductGroupName = vm.ProductGroupName;
            temp.DivisionId = vm.DivisionId;
            temp.OriginCountryId = vm.OriginCountryId;
            temp.ProductDesignPatternId = vm.ProductDesignPatternId;
            temp.ColourId = vm.ColourId;
            temp.ContentId = vm.ContentId;
            temp.FaceContentId = vm.FaceContentId;
            temp.SampleId = vm.SampleId;
            temp.CounterNo = vm.CounterNo;
            temp.DescriptionOfGoodsId = vm.DescriptionOfGoodsId;
            temp.SalesTaxProductCodeId = vm.SalesTaxProductCodeId;
            temp.StandardCost = vm.StandardCost;
            temp.StandardWeight = vm.StandardWeight;
            temp.GrossWeight = vm.GrossWeight;
            temp.DivisionId = vm.DivisionId;
            temp.OriginCountryId = vm.OriginCountryId;
            temp.Tags = vm.Tags;
            temp.ImageFileName = vm.ImageFileName;
            temp.ImageFolderName = vm.ImageFolderName;
            temp.IsSample = vm.IsSample;

            return RedirectToAction("AddSize", temp);
        }

        public JsonResult GetProductCategoryDetailJson(int ProductCategoryId)
        {
            var ProductCategoryDetail = (from Pg in db.ProductCategory
                                      where Pg.ProductCategoryId == ProductCategoryId
                                      select new
                                      {
                                          DefaultSalesTaxProductCodeId = Pg.DefaultSalesTaxProductCodeId,
                                          DefaultSalesTaxProductCodeName = Pg.DefaultSalesTaxProductCode.Code
                                      }).FirstOrDefault();

            return Json(ProductCategoryDetail);
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

    public class FirstProductName
    {
        public string ProductName { get; set; }
    }

    public class UnitConversionQty
    {
        public Decimal? ToQty { get; set; }
    }
}
