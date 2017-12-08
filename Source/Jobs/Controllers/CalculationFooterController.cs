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
    public class CalculationFooterController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ICalculationFooterService _CalculationFooterService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public CalculationFooterController(ICalculationFooterService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _CalculationFooterService = SaleOrder;
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
            var p = _CalculationFooterService.GetCalculationListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        private void PrepareViewBag(CalculationFooterViewModel vm)
        {

            //ViewBag.ChargesList = new ChargeService(_unitOfWork).GetChargeList().ToList();
            //ViewBag.ChargeTypeList = new ChargeTypeService(_unitOfWork).GetChargeTypeList().ToList();
            ViewBag.CalculateOnList = new ChargeService(_unitOfWork).GetCalculateOnListForFooter(vm.CalculationId).ToList();
            //ViewBag.CalculationFooterList = new CalculationFooterService(_unitOfWork).GetCalculationFooterList().ToList();
            ViewBag.LedgerAccountList = new LedgerAccountService(_unitOfWork).GetLedgerAccountList().ToList();
            //ViewBag.CostCenterList = new CostCenterService(_unitOfWork).GetCostCenterList().TGetLedgerAccountListake(10).ToList();
            // ViewBag.CalculationFooterList = _CalculationFooterService.GetCalculationFooterListForDropDown().ToList();
            //ViewBag.CalculationProductList =  new  CalculationProductService(_unitOfWork).GetCalculationProductListForDropDown().ToList();
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

        public ActionResult _Create(int CalculationID) //Id ==>Calculation Id
        {
            CalculationFooterViewModel s = new CalculationFooterViewModel();
            s.CalculationId = CalculationID;
            s.AffectCost = true;
            PrepareViewBag(s);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(CalculationFooterViewModel svm)
        {
            CalculationFooter s = Mapper.Map<CalculationFooterViewModel, CalculationFooter>(svm);

            if (ModelState.IsValid)
            {
                if (svm.Id <= 0)
                {
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    _CalculationFooterService.Create(s);


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(svm);
                        return PartialView("_Create", svm);

                    }

                    return RedirectToAction("_Create", new { CalculationID = s.CalculationId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    CalculationFooter temp1 = _CalculationFooterService.Find(svm.Id);

                    CalculationFooter ExRec = Mapper.Map<CalculationFooter>(temp1);

                    temp1.AddDeduct = svm.AddDeduct;
                    temp1.AffectCost = svm.AffectCost;
                    temp1.CalculateOnId = svm.CalculateOnId;
                    temp1.CalculationFooterLineId = svm.Id;
                    temp1.ChargeId = svm.ChargeId;
                    temp1.RateType = svm.RateType;
                    temp1.Amount = svm.Amount;
                    temp1.Rate = svm.Rate;
                    temp1.ProductChargeId = svm.ProductChargeId;
                    temp1.ChargeTypeId = svm.ChargeTypeId;
                    temp1.CostCenterId = svm.CostCenterId;
                    temp1.IncludedInBase = svm.IncludedInBase;
                    temp1.IsActive = svm.IsActive;
                    temp1.ParentChargeId = svm.ParentChargeId;
                    temp1.ProductChargeId = svm.ProductChargeId;
                    temp1.Sr = svm.Sr;
                    temp1.IsVisible = svm.IsVisible;
                    temp1.IncludedCharges = svm.IncludedCharges;
                    temp1.IncludedChargesCalculation = svm.IncludedChargesCalculation;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    temp1.ObjectState = Model.ObjectState.Modified;
                    _CalculationFooterService.Update(temp1);

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
                        PrepareViewBag(svm);
                        return PartialView("_Create", svm);

                    }
                   // LogActivity.LogActivityDetail(new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Calculation).DocumentTypeId,
                   //temp1.CalculationFooterLineId,
                   //null,
                   //(int)ActivityTypeContants.Modified,
                   //"",
                   //User.Identity.Name, "", Modifications);
                   // return Json(new { success = true });



                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Calculation).DocumentTypeId,
                        DocId = temp1.CalculationFooterLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                }
            }
            PrepareViewBag(svm);
            return PartialView("_Create", svm);
        }

        [HttpGet]
        public ActionResult _Edit(int id)
        {
            CalculationFooterViewModel temp = _CalculationFooterService.GetCalculationFooter(id);

            PrepareViewBag(temp);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", temp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(CalculationFooterViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            CalculationFooter CalculationFooter = _CalculationFooterService.Find(vm.Id);
            LogList.Add(new LogTypeViewModel
            {
                ExObj = CalculationFooter,
            });
            _CalculationFooterService.Delete(CalculationFooter);
            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
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

            //LogActivity.LogActivityDetail(new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Calculation).DocumentTypeId,
            //CalculationFooter.CalculationFooterLineId,
            //null,
            //(int)ActivityTypeContants.Deleted,
            //"",
            //User.Identity.Name,
            //"", Modifications);

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Calculation).DocumentTypeId,
                DocId = CalculationFooter.CalculationFooterLineId,
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
