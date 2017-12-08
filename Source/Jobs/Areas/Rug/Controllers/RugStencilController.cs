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

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class RugStencilController : System.Web.Mvc.Controller
    {
          private ApplicationDbContext db = new ApplicationDbContext();

          IRugStencilService _RugStencilService;
          IUnitOfWork _unitOfWork;
          IExceptionHandlingService _exception;
          public RugStencilController(IRugStencilService RugStencilService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
          {
              _RugStencilService = RugStencilService;
              _unitOfWork = unitOfWork;
              _exception = exec;
          }

        public void PrepareViewBag(RugStencilViewModel vm)
          {

              List<SelectListItem> temp = new List<SelectListItem>();
              temp.Add(new SelectListItem { Text = "Half", Value = "Half" });
              temp.Add(new SelectListItem { Text = "Full", Value = "Full" });

            if(vm==null)
            {
                ViewBag.HalfFull = new SelectList(temp, "Value", "Text");
            }
            else
            {
                ViewBag.HalfFull = new SelectList(temp, "Value", "Text", vm.FullHalf);
            }


          }



        // GET: /RugStencilMaster/
        
          public ActionResult Index()
          { 
              var RugStencil = _RugStencilService.GetRugStencilList();
              return View(RugStencil);
              //return RedirectToAction("Create");
          }

          // GET: /RugStencilMaster/Create
        
          public ActionResult Create()
          {
              RugStencilViewModel vm = new RugStencilViewModel();
              PrepareViewBag(null);
              vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
              return View("Create",vm);
          }

        public JsonResult GetRugSizeList(int id)//Design Id
        {

            var p = _RugStencilService.GetRugSizes(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        
        public ActionResult AddStencilSize(RugStencilViewModel vm)
        {
            RugStencilViewModel tvm = new RugStencilViewModel();
            tvm.FullHalf = vm.FullHalf;
            tvm.DivisionId = vm.DivisionId;
            tvm.ProductDesignId = vm.ProductDesignId;
            tvm.ProductSizeId = vm.ProductSizeId;
            tvm.ProductSizeName = vm.ProductSizeName;
            tvm.StencilSizeId = 0;
            tvm.StencilSizeName="";
            //vm.ProductDesignName = new ProductDesignService(_unitOfWork).Find(vm.ProductDesignId).ProductDesignName;
            //vm = new RugStencilViewModel();
            return PartialView("AddStencilSize", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStencilSizePost(RugStencilViewModel vm)
        {

            if(ModelState.IsValid)
            {

                Product p = new Product();
                p.ProductCode = vm.StencilName;
                p.ProductName = vm.StencilName;
                p.IsActive = true;
                p.CreatedBy = User.Identity.Name;
                p.ModifiedBy = User.Identity.Name;
                p.DivisionId = vm.DivisionId;
                p.CreatedDate = DateTime.Now;
                p.ModifiedDate = DateTime.Now;

                new ProductService(_unitOfWork).Create(p);

                RugStencil rs = new RugStencil();
                rs.FullHalf = vm.FullHalf;
                rs.CreatedBy = User.Identity.Name;
                rs.CreatedDate = DateTime.Now;
                rs.ModifiedBy = User.Identity.Name;
                rs.ModifiedDate = DateTime.Now;
                rs.ProductDesignId = vm.ProductDesignId;
                rs.ProductSizeId = vm.ProductSizeId;
                rs.StencilId = p.ProductId;
                _RugStencilService.Create(rs);


                ProductSize ps = new ProductSize();

                ps.CreatedBy = User.Identity.Name;
                ps.CreatedDate = DateTime.Now;
                ps.IsActive = true;
                ps.ModifiedBy = User.Identity.Name;
                ps.ModifiedDate = DateTime.Now;
                ps.ProductId = p.ProductId;
                ps.ProductSizeTypeId = new ProductSizeTypeService(_unitOfWork).Find(SizeTypeConstants.Standard).ProductSizeTypeId;
                ps.SizeId = vm.StencilSizeId;

                new ProductSizeService(_unitOfWork).Create(ps);

                try
                {
                    _unitOfWork.Save();
                }
               
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("AddStencilSize", vm);

                }

                return Json(new { success = true });

            }
            return PartialView("AddStencilSize", vm);

        }


     


        // GET: /ProductMaster/Edit/5
        
        public ActionResult Edit(int id)
        {
            RugStencil pt = _RugStencilService.Find(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", pt);
        }

        // POST: /ProductMaster/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(RugStencil pt)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        RugStencil temp = _RugStencilService.Find(pt.RugStencilId);
        //        temp.RugStencilName = pt.RugStencilName;
        //        temp.IsActive = pt.IsActive;
        //        temp.ModifiedDate = DateTime.Now;
        //        temp.ModifiedBy = User.Identity.Name;
        //        temp.ObjectState = Model.ObjectState.Modified;
        //        _RugStencilService.Update(temp);

        //        try
        //        {
        //            _unitOfWork.Save();
        //        }
        //        catch (DbEntityValidationException dbex)
        //        {
        //            string message = _exception.HandleEntityValidationException(dbex);
        //            ModelState.AddModelError("", message);
        //            return View("Create", pt);
        //        }
        //        catch (DbUpdateException du)
        //        {
        //            string message = _exception.HandleUpdateException(du);
        //            ModelState.AddModelError("", message);
        //            return View("Create", pt);
        //        }
        //        catch (Exception ex)
        //        {
        //            string message = _exception.HandleException(ex);
        //            ModelState.AddModelError("", message);
        //            return View("Create", pt);

        //        }


        //        return RedirectToAction("Index").Success("Data saved successfully");
        //    }
        //    return View("Create", pt);
        //}

        // GET: /ProductMaster/Delete/5
        
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RugStencil RugStencil = db.RugStencil.Find(id);
            if (RugStencil == null)
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
            if(ModelState.IsValid)
            {
                var temp = _RugStencilService.Find(vm.id);
                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = vm.id,
                    UserRemark = vm.Reason,
                    Narration = "Stencil is deleted with Name:" + temp.StencilId,
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.SaleOrder).DocumentTypeId,
                    UploadDate = DateTime.Now,

                };
                new ActivityLogService(_unitOfWork).Create(al);
            

            _RugStencilService.Delete(vm.id);

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
        public ActionResult NextPage(int id)//CurrentHeaderId
        {
            var nextId = _RugStencilService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _RugStencilService.PrevId(id);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.RugStencil  );

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
