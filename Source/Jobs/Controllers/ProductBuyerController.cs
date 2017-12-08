using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductBuyerController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductBuyerService _ProductBuyerService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductBuyerController(IProductBuyerService ProductBuyerService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductBuyerService = ProductBuyerService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult ProductIndex(int id)//ProductId
        {
            var productInfo = new FinishedProductService(_unitOfWork).GetFinishedProduct(id);

            return RedirectToAction("Edit", "CarpetMaster", new { id = productInfo.ProductGroupId, sample = productInfo.IsSample });

        }

        // GET: /ProductBuyerMaster/
        
        public ActionResult Index(int ID)//ProductId
        {
            Product temp = new ProductService(_unitOfWork).Find(ID);
            ViewBag.Name = temp.ProductName;
            ViewBag.Id = ID;
            var ProductBuyer = _ProductBuyerService.GetProductBuyerList(ID).ToList();
            return View(ProductBuyer);
        }

        // GET: /ProductBuyerMaster/Create
        
        public ActionResult Create(int ProductId)
        {
            ProductBuyerViewModel vm = new ProductBuyerViewModel();
            Product temp = new ProductService(_unitOfWork).Find(ProductId);
            ViewBag.Name = temp.ProductName;
            ViewBag.Id = ProductId;
            vm.ProductId = ProductId;
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductBuyerViewModel vm)
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
                    _ProductBuyerService.Create(pt);

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

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductBuyer).DocumentTypeId,
                        DocId = pt.ProductBuyerId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("Create", new { ProductId=pt.ProductId}).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    ProductBuyer temp = _ProductBuyerService.Find(pt.ProductBuyerId);

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
                    _ProductBuyerService.Update(temp);

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
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductBuyer).DocumentTypeId,
                        DocId = temp.ProductBuyerId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", new { id=vm.ProductId}).Success("Data saved successfully");

                }

            }
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5
        
        public ActionResult Edit(int id)//ProductBuyerId
        {
            ProductBuyer pt = _ProductBuyerService.Find(id);
            ProductBuyerViewModel vm = AutoMapper.Mapper.Map<ProductBuyer, ProductBuyerViewModel>(pt);
            if (pt == null)
            {
                return HttpNotFound();
            }
            Product temp = new ProductService(_unitOfWork).Find(pt.ProductId);
            ViewBag.Name = temp.ProductName;
            ViewBag.Id = pt.ProductId;
            return View("Create", vm);
        }

        // GET: /ProductMaster/Delete/5
        
        public ActionResult Delete(int id)//ProductBuyer Id
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductBuyer ProductBuyer = db.ProductBuyer.Find(id);
            if (ProductBuyer == null)
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
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                var temp = _ProductBuyerService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                _ProductBuyerService.Delete(vm.id);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductBuyer).DocumentTypeId,
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
        public ActionResult NextPage(int id,int ProductId)//CurrentHeaderId
        {
            var nextId = _ProductBuyerService.NextId(id,ProductId);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id,int ProductId)//CurrentHeaderId
        {
            var nextId = _ProductBuyerService.PrevId(id,ProductId);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Buyer);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }
        [HttpGet]
        public ActionResult Export()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
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
