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
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Core.Common;
using Model.ViewModel;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class RateListController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IRateListService _RateListService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public RateListController(IRateListService RateListService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _RateListService = RateListService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }

        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        void PrepareViewBag()
        {
            //ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            //ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            //ViewBag.id = id;
        }

        public ActionResult Index()
        {
            var s = _RateListService.GetWeavingRateListForIndex();
            return View(s);
        }


        public ActionResult Create(int id)
        {
            RateList vm = new RateList();
            PrepareViewBag();
            vm.DocTypeId = id;
            return View("Create", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(RateListViewModel vm)
        {
            RateList s = AutoMapper.Mapper.Map<RateListViewModel, RateList>(vm);


            if (ModelState.IsValid)
            {

                if (!vm.RateListId.HasValue || vm.RateListId.Value == 0)
                {
                    
                    s.DocId = vm.ProductGroupId;
                    s.DealUnitId = UnitConstants.SqYard;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    _RateListService.Create(s);


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



                    return RedirectToAction("Index").Success("Data saved successfully");
                }
                else
                {

                    RateList tempRateList = _RateListService.Find(s.RateListId);

                    if(tempRateList.WEF > s.WEF)
                    {
                        ModelState.AddModelError("WEF", "Date cannot be older.");
                        return View("Create", vm);
                    }

                    RateListHistory History = new RateListHistory();
                    History = AutoMapper.Mapper.Map<RateList, RateListHistory>(tempRateList);
                    History.ObjectState = Model.ObjectState.Added;
                    History.CreatedBy = User.Identity.Name;
                    History.CreatedDate = DateTime.Now;
                    History.ModifiedBy = User.Identity.Name;
                    History.ModifiedDate = DateTime.Now;
                    new RateListHistoryService(_unitOfWork).Create(History);


                    tempRateList.Rate = s.Rate;
                    tempRateList.WEF = s.WEF;
                    tempRateList.Loss = s.Loss;
                    tempRateList.UnCountedQty = s.UnCountedQty;
                    tempRateList.ModifiedDate = DateTime.Now;
                    tempRateList.ModifiedBy = User.Identity.Name;
                    tempRateList.ObjectState = Model.ObjectState.Modified;
                    _RateListService.Update(tempRateList);


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

                    return RedirectToAction("Index").Success("Data saved successfully");
                }
            }
            return View("Create", vm);
        }

        public ActionResult Edit(int ProductGroup,int PersonRateGroup,int ? RateListId)
        {

            if (!RateListId.HasValue)
            {
                RateListViewModel vm = _RateListService.GetNewRateListForWeaving(ProductGroup, PersonRateGroup);
                return View("Create", vm);
            }
            else if (RateListId.Value >= 0)
            {
                RateListViewModel vm = _RateListService.GetRateListVM(RateListId.Value);
                return View("Create", vm);
            }
            else
                return HttpNotFound();
            //RateList s = _RateListService.Find(id);            
            //if (s == null)
            //{
            //    return HttpNotFound();
            //}
            //PrepareViewBag();
            //return View("Create", s);
            //return View();
        }


        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RateList RateList = _RateListService.Find(id);
            if (RateList == null)
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
                var temp = _RateListService.Find(vm.id);

                _RateListService.Delete(vm.id);


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
            var nextId = _RateListService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _RateListService.PrevId(id);
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
