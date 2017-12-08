using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Surya.India.Model.Models;
using Surya.India.Data.Models;
using Surya.India.Service;
using Surya.India.Data.Infrastructure;
using Surya.India.Presentation.ViewModels;
using Surya.India.Presentation;
using Surya.India.Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Surya.India.Model.ViewModel;

namespace Surya.India.Web
{
    [Authorize]
    public class PurchaseWaybillController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IPurchaseWaybillService _PurchaseWaybillService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public PurchaseWaybillController(IPurchaseWaybillService PurchaseWaybillService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseWaybillService = PurchaseWaybillService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }
        // GET: /ProductMaster/
        
        public ActionResult Index()
        {
            var PurchaseWaybill = _PurchaseWaybillService.GetPurchaseWaybillList();
            return View(PurchaseWaybill);
        }

        private void PrepareViewBag()
        {
            ViewBag.ShipMethodList = new ShipMethodService(_unitOfWork).GetShipMethodList().ToList();
            List<SelectListItem> FreightType = new List<SelectListItem>();
            FreightType.Add(new SelectListItem { Text = "To Pay", Value = "To Pay" });
            FreightType.Add(new SelectListItem { Text = "Paid", Value = "Paid" });
            FreightType.Add(new SelectListItem { Text = "To be Billed", Value = "To be Billed" });

            ViewBag.FreightTypeList = new SelectList(FreightType, "Text", "Value");
        }

        // GET: /ProductMaster/Create
        
        public ActionResult Create()
        {
            PrepareViewBag();

            PurchaseWaybillViewModel vm = new PurchaseWaybillViewModel();
            vm.DocDate = DateTime.Now;
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.EntryNo = _PurchaseWaybillService.GetMaxDocNo();
            return View("Create",vm);
        }

        // GET: /ProductMaster/Edit/5
        
        public ActionResult Edit(int id)
        {
            PurchaseWaybillViewModel pt = _PurchaseWaybillService.GetPurchaseWaybill(id);
            PrepareViewBag();
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", pt);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(PurchaseWaybillViewModel vm)
        {
            PurchaseWaybill pt = AutoMapper.Mapper.Map<PurchaseWaybillViewModel, PurchaseWaybill>(vm);
            if (ModelState.IsValid)
            {
                if(vm.PurchaseWaybillId<=0)
                { 
                pt.CreatedDate = DateTime.Now;
                pt.ModifiedDate = DateTime.Now;
                pt.CreatedBy = User.Identity.Name;
                pt.ModifiedBy = User.Identity.Name;
                pt.ObjectState = Model.ObjectState.Added;
                _PurchaseWaybillService.Create(pt);

                ActivityLog log = new ActivityLog()
                {
                    ActivityType = (int)(ActivityTypeContants.Added),
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = pt.PurchaseWaybillId,
                     Narration="A new PurchaseWaybill is created with the id"+pt.PurchaseWaybillId,

                };
                new ActivityLogService(_unitOfWork).Create(log);

                try
                {
                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    PrepareViewBag();
                    return View("Create", vm);

                }
                PrepareViewBag();
                return RedirectToAction("Create").Success("Data saved successfully");
                }

                else
                {
                    PurchaseWaybill temp = _PurchaseWaybillService.Find(pt.PurchaseWaybillId);

                    temp.EntryNo = pt.EntryNo;
                    temp.DocNo = pt.DocNo;
                    temp.DocDate = pt.DocDate;
                    temp.ConsignerId = pt.ConsignerId;
                    temp.ReferenceDocNo = pt.ReferenceDocNo;
                    temp.ShipMethodId = pt.ShipMethodId;
                    temp.DeliveryPoint = pt.DeliveryPoint;
                    temp.EstimatedDeliveryDate = pt.EstimatedDeliveryDate;
                    temp.FreightType = pt.FreightType;
                    temp.FromCityId = pt.FromCityId;
                    temp.ToCityId = pt.ToCityId;
                    temp.ProductDescription = pt.ProductDescription;
                    temp.PrivateMark = pt.PrivateMark;
                    temp.NoOfPackages = pt.NoOfPackages;
                    temp.ActualWeight = pt.ActualWeight;
                    temp.ChargedWeight = pt.ChargedWeight;
                    temp.ContainerNo = pt.ContainerNo;
                    temp.FreightAmt = pt.FreightAmt;
                    temp.OtherCharges = pt.OtherCharges;
                    temp.ServiceTax = pt.ServiceTax;
                    temp.ServiceTaxPer = pt.ServiceTaxPer;
                    temp.TotalAmount = pt.TotalAmount;
                    temp.Remark = pt.Remark;
                    temp.IsDoorDelivery = pt.IsDoorDelivery;
                    temp.IsPOD = pt.IsPOD;

                    _PurchaseWaybillService.Update(temp);

                    ActivityLog log = new ActivityLog()
                    {
                        ActivityType = (int)(ActivityTypeContants.Added),
                        CreatedBy = User.Identity.Name,
                        CreatedDate = DateTime.Now,
                       Narration="PurchaseWaybill is modified with the name "+temp.DocNo,
                        DocId = pt.PurchaseWaybillId,

                    };
                    new ActivityLogService(_unitOfWork).Create(log);

                    try
                    {
                        _unitOfWork.Save();
                    }
                   
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", pt);

                    }
                    PrepareViewBag();
                    return RedirectToAction("Index").Success("Data saved successfully");
                }

            }
            PrepareViewBag();
            return View("Create",vm);
        }

        // GET: /ProductMaster/Delete/5
        
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseWaybill PurchaseWaybill = _PurchaseWaybillService.Find(id);

            if (PurchaseWaybill == null)
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
                var temp = _PurchaseWaybillService.Find(vm.id);
                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = vm.id,
                    UserRemark = vm.Reason,
                    Narration = "Purchase Waybill is deleted with Name:" + temp.DocNo,
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(DocumentCategoryConstants.SaleOrder).DocumentTypeId,
                    UploadDate = DateTime.Now,

                };
                new ActivityLogService(_unitOfWork).Create(al);

                _PurchaseWaybillService.Delete(vm.id);

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
            var nextId = _PurchaseWaybillService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _PurchaseWaybillService.PrevId(id);
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
