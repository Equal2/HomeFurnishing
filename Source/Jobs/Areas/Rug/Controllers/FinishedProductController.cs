using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using Model.ViewModels;
using System.Configuration;
using System.IO;
using ImageResizer;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class FinishedProductController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IFinishedProductService _FinishedProductService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public FinishedProductController(IFinishedProductService FinishedProductService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _FinishedProductService = FinishedProductService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /FinishedProductMaster/
        
        public ActionResult Index(int id,bool sample)//ProductType Id
        {
            var FinishedProduct = _FinishedProductService.GetFinishedProductList(id,sample);//ProductTypeId
            ViewBag.id = id;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
            ViewBag.Sample = sample;
            return View(FinishedProduct);
        }
        public ActionResult ProductTypeIndex(int id)
        {
            var producttype = new ProductTypeService(_unitOfWork).GetFinishedProductTypeListWithNoCustomUI().ToList();
            ViewBag.Sample = ((id==0)?"False":"True");
            return View("ProductTypeIndex", producttype);
        }

        // GET: /FinishedProductMaster/Create
        
        public ActionResult Create(int id,bool sample)//ProductTypeId
        {
            FinishedProductViewModel vm = new FinishedProductViewModel();
            vm.IsSample = sample;
            ViewBag.Sample = sample;
            vm.IsActive = true;
            vm.ProductTypeId = id;
            vm.ProductTypeName = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(FinishedProductViewModel vm)
        {

            if (ModelState.IsValid)
            {
                if (vm.ProductId <= 0)
                {
                    FinishedProduct pt = AutoMapper.Mapper.Map<FinishedProductViewModel, FinishedProduct>(vm);
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.UnitId = UnitConstants.Pieces;
                    pt.ObjectState = Model.ObjectState.Added;
                    _FinishedProductService.Create(pt);

                    if (!(vm.SizeId <= 0))
                    {
                        ProductSize ps = new ProductSize();
                        var pstid = (int)ProductSizeTypeConstants.StandardSize;
                        ps.SizeId = vm.SizeId;
                        ps.ProductId = vm.ProductId;
                        ps.ProductSizeTypeId = pstid;
                        ps.CreatedBy = User.Identity.Name;
                        ps.ModifiedBy = User.Identity.Name;
                        ps.CreatedDate = DateTime.Now;
                        ps.ModifiedDate = DateTime.Now;
                        new ProductSizeService(_unitOfWork).Create(ps);
                    }
                    try
                    {
                        _unitOfWork.Save();
                    }
                   
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", vm);
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

                            string filecontent = Path.Combine(uploadFolder, vm.ProductName + "_" + filename);

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
                                    string tfileName = Path.Combine(tuploadFolder, vm.ProductName + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, vm.ProductName + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }

                            }

                            //var tempsave = _FinishedProductService.Find(pt.ProductId);

                            pt.ImageFileName = pt.ProductName + "_" + filename + temp2;
                            pt.ImageFolderName = uploadfolder;
                            pt.ObjectState = Model.ObjectState.Modified;
                            _FinishedProductService.Update(pt);
                            _unitOfWork.Save();
                        }

                    }

                    #endregion
           



                    return RedirectToAction("Create", new { id=vm.ProductTypeId,sample=vm.IsSample}).Success("Data saved successfully");
                }
                else
                {

                    FinishedProduct temp = _FinishedProductService.Find(vm.ProductId);
                    temp.ProductName = vm.ProductName;
                    temp.ProductCode = vm.ProductCode;
                    temp.ProductGroupId = vm.ProductGroupId;
                    temp.ProductCategoryId = vm.ProductCategoryId;
                    temp.ProductCollectionId = vm.ProductCollectionId;
                    temp.SampleId = vm.SampleId;
                    temp.CounterNo = vm.CounterNo;
                    temp.ProductQualityId = vm.ProductQualityId;
                    temp.OriginCountryId = vm.OriginCountryId;
                    temp.ProductDesignId = vm.ProductDesignId;
                    temp.ProductInvoiceGroupId = vm.ProductInvoiceGroupId;
                    temp.ProductDesignPatternId = vm.ProductDesignPatternId;
                    temp.DrawBackTariffHeadId = vm.DrawBackTariffHeadId;
                    temp.ProductStyleId = vm.ProductStyleId;
                    temp.DescriptionOfGoodsId = vm.DescriptionOfGoodsId;
                    temp.ColourId = vm.ColourId;
                    temp.StandardCost = vm.StandardCost;
                    temp.ProductManufacturerId = vm.ProductManufacturerId;
                    temp.StandardWeight = vm.StandardWeight;
                    temp.ProcessSequenceHeaderId = vm.ProcessSequenceHeaderId;
                    temp.CBM = vm.CBM;
                    temp.ContentId = vm.ContentId;
                    temp.Tags = vm.Tags;
                    temp.FaceContentId = vm.FaceContentId;
                    temp.ProductSpecification = vm.ProductSpecification;
                    temp.IsActive = vm.IsActive;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _FinishedProductService.Update(temp);

                    if (!(vm.SizeId <= 0))
                    {
                        var pstid = (int)ProductSizeTypeConstants.StandardSize; ;
                        ProductSize ps = new ProductSizeService(_unitOfWork).FindProductSize(pstid, vm.ProductId);
                        if (ps != null)
                        {
                            ps.SizeId = vm.SizeId;
                            ps.ProductId = vm.ProductId;
                            ps.ModifiedDate = DateTime.Now;
                            ps.ModifiedBy = User.Identity.Name;
                            ps.ObjectState = Model.ObjectState.Modified;
                            new ProductSizeService(_unitOfWork).Update(ps);
                        }
                        else
                        {
                            ProductSize pss = new ProductSize();
                            var psid = (int)ProductSizeTypeConstants.StandardSize;
                            pss.ProductId = vm.ProductId;
                            pss.SizeId = vm.SizeId;
                            pss.ProductSizeTypeId = psid;
                            pss.CreatedBy = User.Identity.Name;
                            pss.ModifiedBy = User.Identity.Name;
                            pss.CreatedDate = DateTime.Now;
                            pss.ModifiedDate = DateTime.Now;
                            new ProductSizeService(_unitOfWork).Create(pss);
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
                        return View("Create", vm);
                    }


                    #region

                    //Saving Image if file is uploaded
                    if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
                    {
                        string uploadfolder = temp.ImageFolderName;
                        string tempfilename = temp.ImageFileName;
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

                            string filecontent = Path.Combine(uploadFolder, temp.ProductName + "_" + filename);

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
                                    string tfileName = Path.Combine(tuploadFolder, temp.ProductName + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, temp.ProductName + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }
                            }
                        }
                        var temsave = _FinishedProductService.Find(temp.ProductId);
                        temsave.ImageFileName = temsave.ProductName + "_" + filename + temp2;
                        temsave.ImageFolderName = uploadfolder;
                        _FinishedProductService.Update(temsave);
                        _unitOfWork.Save();
                    }

                    #endregion                 



                    return RedirectToAction("Index", new { id=vm.ProductTypeId,sample=vm.IsSample}).Success("Data saved successfully");

                }

            }
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5
        
        public ActionResult Edit(int id)
        {
            FinishedProduct pt = _FinishedProductService.Find(id);
            FinishedProductViewModel vm = AutoMapper.Mapper.Map<FinishedProduct, FinishedProductViewModel>(pt);
            var pstid = (int) ProductSizeTypeConstants.StandardSize;
            ProductSize tem = new ProductSizeService(_unitOfWork).FindProductSize(pstid, pt.ProductId);
            ProductType typ = new ProductTypeService(_unitOfWork).GetTypeForProduct(id);
            vm.ProductTypeId = typ.ProductTypeId;
            vm.ProductTypeName = typ.ProductTypeName;
            ViewBag.Sample = vm.IsSample;
            if (tem != null)
            {
                vm.SizeId = tem.SizeId;
            }
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", vm);
        }


        // GET: /ProductMaster/Delete/5
        
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinishedProduct FinishedProduct = db.FinishedProduct.Find(id);
            if (FinishedProduct == null)
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
            if (ModelState.IsValid)
            {
                var temp = _FinishedProductService.Find(vm.id);

                List<ProductSizeViewModel> sizelist = new ProductSizeService(_unitOfWork).GetProductSizeForProduct(vm.id).ToList();
                List<ProductProcess> ProcessList = new ProductProcessService(_unitOfWork).GetProductProcessIdListByProductId(vm.id).ToList();
                List<FinishedProductConsumptionLineViewModel> BOMDetailList =new  BomDetailService(_unitOfWork).GetFinishedProductConsumptionForIndex(vm.id).ToList();
                List<ProductSiteDetail> SiteDetail=new ProductSiteDetailService(_unitOfWork).GetSiteDetailForProduct(vm.id).ToList();


                   foreach (var item in sizelist)
                    {
                        new ProductSizeService(_unitOfWork).Delete(item.ProductSizeId);
                    }
                
                    foreach (var item in ProcessList)
                    {
                        new ProductProcessService(_unitOfWork).Delete(item.ProductProcessId);
                    }
                    foreach(var item in BOMDetailList)
                    {
                        new BomDetailService(_unitOfWork).Delete(item.BomDetailId);
                    }
                    foreach(var item in SiteDetail)
                    {
                    new ProductSiteDetailService(_unitOfWork).Delete(item.ProductSiteDetailId);
                    }
                    
                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = vm.id,
                    UserRemark = vm.Reason,
                    Narration = "Product is deleted with Name:" + temp.ProductName,
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.SaleOrder).DocumentTypeId,
                    UploadDate = DateTime.Now,

                };
                new ActivityLogService(_unitOfWork).Create(al);


                _FinishedProductService.Delete(vm.id);

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

        [HttpGet]
        public ActionResult NextPage(int id,int tid)//CurrentHeaderId
        {
            var nextId = _FinishedProductService.NextId(id,tid);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id,int tid)//CurrentHeaderId
        {
            var nextId = _FinishedProductService.PrevId(id,tid);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.FinishedProduct );

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }


        //ProductBuyer Code:

        public JsonResult ProductBuyerIndex(int ID)//ProductId
        {
            Product temp = new ProductService(_unitOfWork).Find(ID);
            ViewBag.Name = temp.ProductName;
            ViewBag.Id = ID;
            var ProductBuyer = new ProductBuyerService(_unitOfWork).GetProductBuyerList(ID).ToList();
            return Json(ProductBuyer, JsonRequestBehavior.AllowGet);
        }

        // GET: /ProductBuyerMaster/Create

        public ActionResult _Create(int ProductId)
        {
            ProductBuyerViewModel vm = new ProductBuyerViewModel();
            Product temp = new ProductService(_unitOfWork).Find(ProductId);
            ViewBag.Name = temp.ProductName;
            ViewBag.Id = ProductId;
            vm.ProductId = ProductId;
            return PartialView("_Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ProductBuyerViewModel vm)
        {
            ProductBuyer pt = AutoMapper.Mapper.Map<ProductBuyerViewModel, ProductBuyer>(vm);
            if (ModelState.IsValid)
            {

                if (vm.ProductBuyerId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    new ProductBuyerService(_unitOfWork).Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", vm);

                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductBuyer).DocumentTypeId,
                        DocId = pt.ProductBuyerId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("_Create", new { ProductId = pt.ProductId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductBuyer temp = new ProductBuyerService(_unitOfWork).Find(pt.ProductBuyerId);

                    ProductBuyer ExRec = new ProductBuyer();
                    ExRec = Mapper.Map<ProductBuyer>(temp);

                    temp.BuyerId = pt.BuyerId;
                    temp.BuyerSku = pt.BuyerSku;
                    temp.BuyerSpecification = pt.BuyerSpecification;
                    temp.BuyerSpecification1 = pt.BuyerSpecification1;
                    temp.BuyerSpecification2 = pt.BuyerSpecification2;
                    temp.BuyerSpecification3 = pt.BuyerSpecification3;
                    temp.BuyerSpecification4 = pt.BuyerSpecification4;
                    temp.BuyerSpecification5 = pt.BuyerSpecification5;
                    temp.BuyerSpecification6 = pt.BuyerSpecification6;
                    temp.BuyerUpcCode = pt.BuyerUpcCode;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    new ProductBuyerService(_unitOfWork).Update(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
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
                        return PartialView("_Create", pt);

                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductBuyer).DocumentTypeId,
                        DocId = temp.ProductBuyerId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return Json(new { success = true });

                }

            }
            return PartialView("_Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult _Edit(int id)//ProductBuyerId
        {
            ProductBuyer pt = new ProductBuyerService(_unitOfWork).Find(id);
            ProductBuyerViewModel vm = AutoMapper.Mapper.Map<ProductBuyer, ProductBuyerViewModel>(pt);
            if (pt == null)
            {
                return HttpNotFound();
            }
            Product temp = new ProductService(_unitOfWork).Find(pt.ProductId);
            ViewBag.Name = temp.ProductName;
            ViewBag.Id = pt.ProductId;
            return PartialView("_Create", vm);
        }
    
        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProductBuyerConfirmed(ProductBuyerViewModel vm)
        {
            if (ModelState.IsValid)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                var temp = new ProductBuyerService(_unitOfWork).Find(vm.ProductBuyerId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                new ProductBuyerService(_unitOfWork).Delete(vm.ProductBuyerId);
                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Create", vm);
                }

                //Logging Activity
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductBuyer).DocumentTypeId,
                    DocId = vm.ProductBuyerId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    xEModifications = Modifications,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Create", vm);
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
