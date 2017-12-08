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
using Model.ViewModel;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class ProductManufacturingStyleController : System.Web.Mvc.Controller
    {
          private ApplicationDbContext db = new ApplicationDbContext();

          IProductManufacturingStyleService _ProductManufacturingStyleService;
          IUnitOfWork _unitOfWork;
          public ProductManufacturingStyleController(IProductManufacturingStyleService ProductManufacturingStyleService, IUnitOfWork unitOfWork)
          {
              _ProductManufacturingStyleService = ProductManufacturingStyleService;
              _unitOfWork = unitOfWork;
          }
        // GET: /ProductMaster/
          public ActionResult Index()
          { 
              var ProductManufacturingStyle = _ProductManufacturingStyleService.GetProductManufacturingStyleList().ToList();
              return View(ProductManufacturingStyle);
          }

        // GET: /ProductMaster/Create
          public ActionResult Create()
          {       
              return View();
          }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
          public ActionResult Create(ProductStyle pt)
          {
              if (ModelState.IsValid)
              {
                  pt.CreatedDate = DateTime.Now;
                  pt.ModifiedDate = DateTime.Now;
                  pt.CreatedBy = User.Identity.Name;
                  pt.ModifiedBy = User.Identity.Name;
                  pt.ObjectState = Model.ObjectState.Added;
                  _ProductManufacturingStyleService.Create(pt);
                  _unitOfWork.Save();
                  return RedirectToAction("Index");
              }

              return View(pt);
          }


        // GET: /ProductMaster/Edit/5
        public ActionResult Edit(int id)
        {
            ProductStyle pt = _ProductManufacturingStyleService.GetProductManufacturingStyle(id);
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
        public ActionResult Edit(ProductStyle pt)
        {           

            if (ModelState.IsValid)
            {
                pt.ModifiedDate = DateTime.Now;
                pt.ModifiedBy = User.Identity.Name;
                pt.ObjectState = Model.ObjectState.Modified;
                _ProductManufacturingStyleService.Update(pt);
                _unitOfWork.Save();
                return RedirectToAction("Index");
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
            ProductStyle ProductManufacturingStyle = db.ProductStyle.Find(id);
            if (ProductManufacturingStyle == null)
            {
                return HttpNotFound();
            }
            return View(ProductManufacturingStyle);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductStyle ProductManufacturingStyle = db.ProductStyle.Find(id);
            db.ProductStyle.Remove(ProductManufacturingStyle);
            db.SaveChanges();
            return RedirectToAction("Index");
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
