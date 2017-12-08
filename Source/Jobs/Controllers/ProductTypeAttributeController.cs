using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Presentation.ViewModels;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductTypeAttributeController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductTypeAttributeService _ProductTypeAttributeService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductTypeAttributeController(IProductTypeAttributeService ProductTypeAttributeService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductTypeAttributeService = ProductTypeAttributeService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult ProductTypeIndex()
        {
            var producttype = new ProductTypeService(_unitOfWork).GetProductTypeList().ToList();
            return View("ProductTypeIndex", producttype);
        }



        // GET: /ProductMaster/

        public ActionResult Index(int Id) //Id ==>ProductTypeId
        {
            ViewBag.id = Id;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(Id).ProductTypeName;
            ViewBag.ProductTypeId = Id;
            ViewBag.ProductType = new ProductTypeService(_unitOfWork).Find(Id).ProductTypeName;
            var Index = _ProductTypeAttributeService.GetProductTypeAttributesList(Id).ToList();
            return View(Index);
        }

        private void PrepareViewBag(ProductTypeAttribute pt)
        {


            List<SelectListItem> Types = new List<SelectListItem>();
            Types.Add(new SelectListItem { Text = "Text", Value = "Text" });
            Types.Add(new SelectListItem { Text = "Number", Value = "Number" });
            Types.Add(new SelectListItem { Text = "List", Value = "List" });
            Types.Add(new SelectListItem { Text = "Date", Value = "Date" });
            if (pt == null)
            {
                ViewBag.DataType = new SelectList(Types, "Value", "Text");
            }
            else
            {
                ViewBag.DataType = new SelectList(Types, "Value", "Text", pt.DataType);
            }
        }

        // GET: /ProductMaster/Create

        public ActionResult Create(int Id) //Id ==>ProductTypeId
        {
            ProductTypeAttribute TypeAttribute = new ProductTypeAttribute();
            TypeAttribute.ProductType_ProductTypeId = Id;
            TypeAttribute.IsActive = true;
            ViewBag.id = Id;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(Id).ProductTypeName;
            PrepareViewBag(null);
            return View(TypeAttribute);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductTypeAttribute pt)
        {
            if (ModelState.IsValid)
            {

                if (pt.ProductTypeAttributeId <= 0)
                {

                    pt.CreatedBy = User.Identity.Name;
                    pt.CreatedDate = System.DateTime.Now;
                    pt.ModifiedDate = System.DateTime.Now;
                    pt.ModifiedBy = User.Identity.Name;
                    _ProductTypeAttributeService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        ViewBag.id = pt.ProductType_ProductTypeId;
                        ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductType_ProductTypeId).ProductTypeName;
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductTypeAttribute).DocumentTypeId,
                        DocId = pt.ProductTypeAttributeId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("Create", new { id = pt.ProductType_ProductTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductTypeAttribute TypeAttribute = _ProductTypeAttributeService.Find(pt.ProductTypeAttributeId);

                    ProductTypeAttribute ExRec = Mapper.Map<ProductTypeAttribute>(TypeAttribute);

                    TypeAttribute.IsActive = pt.IsActive;
                    TypeAttribute.DefaultValue = pt.DefaultValue;
                    TypeAttribute.Name = pt.Name;
                    TypeAttribute.IsMandatory = pt.IsMandatory;
                    TypeAttribute.DataType = pt.DataType;
                    TypeAttribute.ListItem = pt.ListItem;
                    TypeAttribute.ModifiedDate = DateTime.Now;
                    TypeAttribute.ModifiedBy = User.Identity.Name;
                    _ProductTypeAttributeService.Update(TypeAttribute);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = TypeAttribute,
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
                        ViewBag.id = pt.ProductType_ProductTypeId;
                        ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductType_ProductTypeId).ProductTypeName;
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductTypeAttribute).DocumentTypeId,
                        DocId = TypeAttribute.ProductTypeAttributeId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));                   

                    return RedirectToAction("Index", new { id = TypeAttribute.ProductType_ProductTypeId }).Success("Data saved successfully");
                }

            }
            PrepareViewBag(pt);
            return View(pt);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)//->ProductTypeAttributeId
        {

            var pt = _ProductTypeAttributeService.GetProductTypeAttribute(id);
            ViewBag.id = pt.ProductType_ProductTypeId;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductType_ProductTypeId).ProductTypeName;
            PrepareViewBag(pt);
            return View("Create", pt);
        }


        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)//->ProductTypeAttributeId
        {
            ProductTypeAttribute TypeAttribute = _ProductTypeAttributeService.Find(id);

            if (TypeAttribute == null)
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
                var temp = _ProductTypeAttributeService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });
                _ProductTypeAttributeService.Delete(vm.id);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductTypeAttribute).DocumentTypeId,
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
        public ActionResult NextPage(int id, int ptypeid)//CurrentHeaderId
        {
            var nextId = _ProductTypeAttributeService.NextId(id, ptypeid);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, int ptypeid)//CurrentHeaderId
        {
            var nextId = _ProductTypeAttributeService.PrevId(id, ptypeid);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductTypeAttribute);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

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
