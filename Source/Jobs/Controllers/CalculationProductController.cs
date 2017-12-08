using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using AutoMapper;
using Model.ViewModel;
using System.Xml.Linq;

namespace Jobs.Controllers
{

    [Authorize]
    public class CalculationProductController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ICalculationProductService _CalculationProductService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public CalculationProductController(ICalculationProductService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _CalculationProductService = SaleOrder;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _CalculationProductService.GetCalculationListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        private void PrepareViewBag(CalculationProductViewModel vm)
        {

            //ViewBag.ChargesList = new ChargeService(_unitOfWork).GetChargeList().ToList();
            ViewBag.CalculateOnList = new ChargeService(_unitOfWork).GetCalculateOnListForProduct(vm.CalculationId).ToList();
            //ViewBag.ChargeTypeList = new ChargeTypeService(_unitOfWork).GetChargeTypeList().ToList();
            //ViewBag.CalculationProductList = new ChargeService(_unitOfWork).GetChargeList().ToList();
            ViewBag.LedgerAccountList = new LedgerAccountService(_unitOfWork).GetLedgerAccountList().ToList();
            //ViewBag.CostCenterList = new CostCenterService(_unitOfWork).GetCostCenterList().Take(10).ToList();
            //ViewBag.CalculationProductList = new GetCalculationProductListForDropDown().ToList();
            List<SelectListItem> AddList = new List<SelectListItem>();
            AddList.Add(new SelectListItem { Text = "Add", Value = ((int)AddDeductEnum.Add).ToString() });
            AddList.Add(new SelectListItem { Text = "Deduction", Value = ((int)AddDeductEnum.Deduct).ToString() });
            AddList.Add(new SelectListItem { Text = "Override", Value = ((int)AddDeductEnum.OverRide).ToString() });
            ViewBag.AddList = new SelectList(AddList, "Value", "Text");
            List<SelectListItem> AffectList = new List<SelectListItem>();
            AffectList.Add(new SelectListItem { Text = "No", Value = "false" });
            AffectList.Add(new SelectListItem { Text = "Yes", Value = "true" });
            ViewBag.AffectList = new SelectList(AffectList, "Value", "Text");
            List<SelectListItem> IncludedList = new List<SelectListItem>();
            IncludedList.Add(new SelectListItem { Text = "Yes", Value = "true" });
            IncludedList.Add(new SelectListItem { Text = "No", Value = "false" });
            ViewBag.IncludedList = new SelectList(IncludedList, "Value", "Text");

            List<SelectListItem> RateTypeList = new List<SelectListItem>();
            RateTypeList.Add(new SelectListItem { Text = Enum.GetName(typeof(RateTypeConstants), 1), Value = ((byte)(RateTypeConstants.Rate)).ToString() });
            RateTypeList.Add(new SelectListItem { Text = Enum.GetName(typeof(RateTypeConstants), 2), Value = ((byte)(RateTypeConstants.Percentage)).ToString() });
            RateTypeList.Add(new SelectListItem { Text = Enum.GetName(typeof(RateTypeConstants), 3), Value = ((byte)(RateTypeConstants.NA)).ToString() });
            ViewBag.RateTypeList = new SelectList(RateTypeList, "Value", "Text");
        }

        public ActionResult _Create(int CalculationId) //Id ==>Calculation Id
        {
            CalculationProductViewModel s = new CalculationProductViewModel();
            s.CalculationId = CalculationId;
            s.AffectCost = true;
            s.IsVisible = true;
            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(CalculationProductViewModel svm)
        {
            CalculationProduct s = Mapper.Map<CalculationProductViewModel, CalculationProduct>(svm);

            if (ModelState.IsValid)
            {
                if (svm.Id <= 0)
                {
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    _CalculationProductService.Create(s);


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", svm);

                    }

                    return RedirectToAction("_Create", new { CalculationId = s.CalculationId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
                    CalculationProduct temp1 = _CalculationProductService.Find(svm.Id);

                    CalculationProduct ExRec = Mapper.Map<CalculationProduct>(temp1);


                    temp1.AddDeduct = svm.AddDeduct;
                    temp1.AffectCost = svm.AffectCost;
                    temp1.CalculateOnId = svm.CalculateOnId;
                    //temp1.CalculationProductId = svm.Id;
                    temp1.Rate = svm.Rate;
                    temp1.ChargeId = svm.ChargeId;
                    temp1.ChargeTypeId = svm.ChargeTypeId;
                    temp1.CostCenterId = svm.CostCenterId;
                    temp1.IncludedInBase = svm.IncludedInBase;
                    temp1.IsActive = svm.IsActive;
                    temp1.Amount = svm.Amount;
                    temp1.Sr = svm.Sr;
                    temp1.RateType = svm.RateType;
                    temp1.IsVisible = svm.IsVisible;
                    temp1.IncludedCharges = svm.IncludedCharges;
                    temp1.IncludedChargesCalculation = svm.IncludedChargesCalculation;
                    temp1.ParentChargeId = svm.ParentChargeId;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    temp1.ObjectState = Model.ObjectState.Modified;
                    _CalculationProductService.Update(temp1);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
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
                        return PartialView("_Create", svm);
                    }

                    //LogActivity.LogActivityDetail(new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Calculation).DocumentTypeId,
                    //temp1.CalculationProductId,
                    //null,
                    //(int)ActivityTypeContants.Modified,
                    //"",
                    //User.Identity.Name, "", Modifications);

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Calculation).DocumentTypeId,
                        DocId = temp1.CalculationProductId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return Json(new { success = true });

                }
            }
            PrepareViewBag(svm);
            return PartialView("_Create", svm);
        }

        [HttpGet]
        public ActionResult _Edit(int id)
        {
            CalculationProductViewModel temp = _CalculationProductService.GetCalculationProduct(id);

            PrepareViewBag(temp);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", temp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(CalculationProductViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            CalculationProduct CalculationProduct = _CalculationProductService.Find(vm.Id);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = CalculationProduct,
            });

            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

            _CalculationProductService.Delete(CalculationProduct);

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
           // LogActivity.LogActivityDetail(new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Calculation).DocumentTypeId,
           //CalculationProduct.CalculationProductId,
           //null,
           //(int)ActivityTypeContants.Deleted,
           //"",
           //User.Identity.Name,
           //"", Modifications);

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Calculation).DocumentTypeId,
                DocId = CalculationProduct.CalculationProductId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                UserRemark = "",
                xEModifications = Modifications,
            }));

            return Json(new { success = true });
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
