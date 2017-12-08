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
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductUidUpdationController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductUidUpdationController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /ProductUidUpdationMaster/

        public ActionResult Create()
        {
            ProductUid vm = new ProductUid();
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductUid vm)
        {
            if (ModelState.IsValid)
            {
                if (vm.ProductUIDId != 0)
                {
                    ProductUid P = new ProductUidService(_unitOfWork).Find(vm.ProductUIDId);
                    P.Dimension1Id = vm.Dimension1Id;
                    P.Dimension2Id = vm.Dimension2Id;
                    P.Dimension3Id = vm.Dimension3Id;
                    P.Dimension4Id = vm.Dimension4Id;
                    P.ProductUidSpecification = vm.ProductUidSpecification;
                    P.ProductUidSpecification1 = vm.ProductUidSpecification1;
                    P.ModifiedDate = DateTime.Now;
                    P.ModifiedBy = User.Identity.Name;
                    P.ObjectState = Model.ObjectState.Modified;
                    db.ProductUid.Add(P);

                    try
                    {
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductUid).DocumentTypeId,
                        DocId = vm.ProductUIDId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("Create").Success("Data saved successfully");
                }
            }
            return View("Create", vm);
        }


        public JsonResult GetProductUidDetailJson(int ProductUidId)
        {
            var temp = (from P in db.ProductUid
                        where P.ProductUIDId == ProductUidId
                        select new
                        {
                            ProductId = P.ProductId,
                            ProductName = P.Product.ProductName,
                            Dimension1Id = P.Dimension1Id,
                            Dimension1Name = P.Dimension1.Dimension1Name,
                            Dimension2Id = P.Dimension2Id,
                            Dimension2Name = P.Dimension2.Dimension2Name,
                            Dimension3Id = P.Dimension3Id,
                            Dimension3Name = P.Dimension3.Dimension3Name,
                            Dimension4Id = P.Dimension4Id,
                            Dimension4Name = P.Dimension4.Dimension4Name,
                            ProductUidSpecification = P.ProductUidSpecification,
                            ProductUidSpecification1 = P.ProductUidSpecification1
                        }).FirstOrDefault();


            return Json(temp);
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
