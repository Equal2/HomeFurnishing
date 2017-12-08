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
using System.Data.SqlClient;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductTraceController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IProductService _ProductService;
        IFinishedProductService _FinishedProductService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        public ProductTraceController(IProductService ProductService, IFinishedProductService FinishedProductService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductService = ProductService;
            _FinishedProductService = FinishedProductService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public void PrepareMaterialViewBag(int Id)
        {
            ViewBag.UnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            ViewBag.SalesTaxGroup = new SalesTaxGroupProductService(_unitOfWork).GetSalesTaxGroupProductList().ToList();
            ViewBag.ProductGroupList = new ProductGroupService(_unitOfWork).GetProductGroupListForItemType(Id);
        }

        public ActionResult ProductTypeIndex(int id)//NatureId
        {
            var producttype = new ProductTypeService(_unitOfWork).GetProductTypeListForMaterial(id).ToList();
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


        // GET: /ProductMaster/

        public ActionResult MaterialIndex(int id)//// Changed To ProductTypeId
        {
            var Product = _ProductService.GetProductListForMaterial(id);
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
            ViewBag.id = id;
            return View(Product);
        }

        // GET: /ProductMaster/Create

        public ActionResult CreateMaterial(int id)//ProductType Id
        {
            MaterialViewModel p = new MaterialViewModel();

            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            p.IsActive = true;
            p.UnitId = UnitConstants.Pieces;
            p.ProductTypeId = id;
            var ProductType = new ProductTypeService(_unitOfWork).Find(id);
            ViewBag.Name = ProductType.ProductTypeName;
            ViewBag.id = id;
            PrepareMaterialViewBag(id);
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
            ProductGroup group = new ProductGroupService(_unitOfWork).Find(pvm.ProductGroupId);
            ProductType Type = new ProductTypeService(_unitOfWork).Find(group.ProductTypeId);

            if (pvm.ProductGroupId <= 0)
            {
                ModelState.AddModelError("ProductGroupId", "Product Group field is required");
            }
            if (ModelState.IsValid)
            {
                //Checking for Create or Edit(<=0 =====>CREATE)
                if (pvm.ProductId <= 0)
                {

                    var ProductSKu = db.Product.Find(pvm.ProductSKUId);

                    var PRoductSize = db.ProductSize.Where(m => m.ProductId == ProductSKu.ProductId && m.ProductSizeTypeId == (int)ProductSizeTypeConstants.StencilSize).FirstOrDefault();

                    var Size = db.Size.Find(PRoductSize.SizeId);

                    pt1.ProductName = pvm.ProductName;
                    pt1.ProductCode = pvm.ProductCode;
                    pt1.ProductGroupId = pvm.ProductGroupId;
                    pt1.StandardCost = pvm.StandardCost;
                    pt1.UnitId = pvm.UnitId;
                    pt1.SalesTaxGroupProductId = pvm.SalesTaxGroupProductId;
                    pt1.UnitId = UnitConstants.Pieces;
                    pt1.IsActive = pvm.IsActive;
                    pt1.DivisionId = pvm.DivisionId;
                    pt1.CreatedDate = DateTime.Now;
                    pt1.ModifiedDate = DateTime.Now;
                    pt1.CreatedBy = User.Identity.Name;
                    pt1.ModifiedBy = User.Identity.Name;
                    pt1.ReferenceDocId = pvm.ProductSKUId;
                    pt1.ReferenceDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Product).DocumentTypeId;
                    pt1.IsActive = true;

                    pt1.ObjectState = Model.ObjectState.Added;
                    _ProductService.Create(pt1);


                    //Standard Size Data
                    ProductSize standardsize = new ProductSize();
                    standardsize.ProductSizeTypeId = (int)(ProductSizeTypeConstants.StandardSize);
                    //standardsize.ProductSizeTypeId = 1;
                    standardsize.SizeId = PRoductSize.SizeId;
                    standardsize.ProductId = pt1.ProductId;
                    standardsize.CreatedBy = User.Identity.Name;
                    standardsize.CreatedDate = DateTime.Now;
                    standardsize.ModifiedBy = User.Identity.Name;
                    standardsize.ModifiedDate = DateTime.Now;
                    standardsize.IsActive = true;
                    new ProductSizeService(_unitOfWork).Create(standardsize);


                    UnitConversion UnitConv = new UnitConversion();
                    UnitConv.CreatedBy = User.Identity.Name;
                    UnitConv.CreatedDate = DateTime.Now;
                    UnitConv.ModifiedBy = User.Identity.Name;
                    UnitConv.ModifiedDate = DateTime.Now;
                    UnitConv.ProductId = pt1.ProductId;
                    UnitConv.FromQty = 1;
                    UnitConv.FromUnitId = UnitConstants.Pieces;
                    UnitConv.ToUnitId = UnitConstants.SqYard;
                    UnitConv.UnitConversionForId = (byte)UnitConversionFors.Standard;



                    using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
                    {
                        sqlConnection.Open();

                        SqlCommand Totalf = new SqlCommand("SELECT * FROM Web.FuncConvertSqFeetToSqYard( " + Size.Area + ")", sqlConnection);

                        UnitConv.ToQty = Convert.ToDecimal(Totalf.ExecuteScalar() == DBNull.Value ? 0 : Totalf.ExecuteScalar());
                    }


                    new UnitConversionService(_unitOfWork).Create(UnitConv);


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareMaterialViewBag(group.ProductTypeId);
                        ViewBag.Name = Type.ProductTypeName;
                        ViewBag.id = group.ProductTypeId;
                        return View("CreateMaterial", pvm);

                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product).DocumentTypeId,
                        DocId = pt1.ProductId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

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



                    return RedirectToAction("CreateMaterial", new { id = group.ProductTypeId }).Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    Product pt = _ProductService.Find(pvm.ProductId);
                    Product ExRec = Mapper.Map<Product>(pt);

                    pt.ProductName = pvm.ProductName;
                    pt.ProductCode = pvm.ProductCode;
                    pt.StandardCost = pvm.StandardCost;
                    pt.ProductGroupId = pvm.ProductGroupId;
                    pt.SalesTaxGroupProductId = pvm.SalesTaxGroupProductId;
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
                        PrepareMaterialViewBag(group.ProductTypeId);
                        ViewBag.Name = Type.ProductTypeName;
                        ViewBag.id = group.ProductTypeId;
                        return View("CreateMaterial", pvm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Product).DocumentTypeId,
                        DocId = pt.ProductId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

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


                    return RedirectToAction("MaterialIndex", new { id = group.ProductTypeId }).Success("Data saved successfully");
                }

            }
            PrepareMaterialViewBag(group.ProductTypeId);
            return View("CreateMaterial", pvm);
        }

        // GET: /ProductMaster/Edit/5

        public ActionResult EditMaterial(int id)
        {

            MaterialViewModel pt = _ProductService.GetMaterialProduct(id);

            if (pt.ReferenceDocId.HasValue)
            {
                var ProductSku = db.Product.Find(pt.ReferenceDocId);
                pt.ProductSKUId = ProductSku.ProductId;
            }


            ProductGroup group = new ProductGroupService(_unitOfWork).Find(pt.ProductGroupId);
            ProductType Type = new ProductTypeService(_unitOfWork).Find(group.ProductTypeId);

            ViewBag.Name = Type.ProductTypeName;
            ViewBag.id = group.ProductTypeId;

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            pt.DivisionId = DivisionId;
            pt.SiteId = SiteId;

            if (pt == null)
            {
                return HttpNotFound();
            }
            PrepareMaterialViewBag(group.ProductTypeId);
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


        public JsonResult GetProductHelpList(string searchTerm, int pageSize, int pageNum)//filter:PersonId
        {

            return new JsonpResult
            {
                Data = _ProductService.GetProductHelpList(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
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
