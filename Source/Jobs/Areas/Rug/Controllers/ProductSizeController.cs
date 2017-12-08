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
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class ProductSizeController : System.Web.Mvc.Controller
    {
          private ApplicationDbContext db = new ApplicationDbContext();

          IProductSizeService _ProductSizeService;
          IUnitOfWork _unitOfWork;
          public ProductSizeController(IProductSizeService ProductSizeService, IUnitOfWork unitOfWork)
          {
              _ProductSizeService = ProductSizeService;
              _unitOfWork = unitOfWork;
          }
        // GET: /ProductSizeMaster/
        
          public ActionResult Index()
          { 
              var ProductSize = _ProductSizeService.GetProductSizeList();
              return View(ProductSize);
              //return RedirectToAction("Create");
          }
        //
        //public ActionResult IndexForProduct(int id)
        //{
        //    var ProductSizeList = _ProductSizeService.GetProductSizeListForProduct(id).ToList();
        //    return View(ProductSizeList);
        //    //return RedirectToAction("Create");
        //}
        [HttpGet]
        public JsonResult IndexForProductGroup(int id)//ProductGroup id
        {
            var TEMP=_ProductSizeService.GetProductSizeListForProductGroup(id).ToList();

            return Json( TEMP,JsonRequestBehavior.AllowGet);           
        }

          // GET: /ProductSizeMaster/Create
        
          public ActionResult Create()
          {
              return View();
          }

        
        public ActionResult _Create()
        {
            return View();
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
          public ActionResult Create(ProductSize vm )
          {
             
              if (ModelState.IsValid)
              {
                  vm.CreatedDate = DateTime.Now;
                  vm.ModifiedDate = DateTime.Now;
                  vm.CreatedBy = User.Identity.Name;
                  vm.ModifiedBy = User.Identity.Name;
                  vm.ObjectState = Model.ObjectState.Added;
                  _ProductSizeService.Create(vm);
                  _unitOfWork.Save();
                  return RedirectToAction("Index").Success("Data saved successfully");
              }

             
              return View(vm);
          }


        // GET: /ProductMaster/Edit/5
        
        public ActionResult Edit(int id)
        {
            ProductSize pt = _ProductSizeService.GetProductSize(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View(pt);
        }

        // POST: /ProductMaster/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductSize pt)
        {
            if (ModelState.IsValid)
            {
                pt.ModifiedDate = DateTime.Now;
                pt.ModifiedBy = User.Identity.Name;
                pt.ObjectState = Model.ObjectState.Modified;
                _ProductSizeService.Update(pt);
                _unitOfWork.Save();
                return RedirectToAction("Index").Success("Data saved successfully");
            }
            return View(pt);
        }

        // GET: /ProductMaster/Delete/5
        
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductSize ProductSize = db.ProductSize.Find(id);
            if (ProductSize == null)
            {
                return HttpNotFound();
            }
            return View(ProductSize);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            //AFTER

            _ProductSizeService.Delete(id);
            _unitOfWork.Save();

            return RedirectToAction("Index").Success("Data deleted successfully");
        }

        public JsonResult GetCommonSizes(int size,int productcategoryid)
        {
            return Json(_ProductSizeService.GetCommanSizes(size, productcategoryid), JsonRequestBehavior.AllowGet);
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
