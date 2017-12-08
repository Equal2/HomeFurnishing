using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Presentation.Helper;
using Services.Customize;
using Components.ExceptionHandlers;
using Models.Customize.ViewModels;
using Models.Customize.Models;
using TimePlanValidator;
using TimePlanValidator.ViewModels;
using Models.BasicSetup.ViewModels;
using CookieNotifier;

namespace Customize.Controllers
{

    [Authorize]
    public class JobOrderLineController : System.Web.Mvc.Controller
    {
        List<string> UserRoles = new List<string>();

        IJobOrderLineService _JobOrderLineService;
        IJobOrderHeaderService _jobOrderHeaderService;
        IJobOrderSettingsService _jobOrderSettingsService;
        IExceptionHandler _exception;
        IDocumentValidation _validator;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public JobOrderLineController(IJobOrderLineService JobOrder, IExceptionHandler exec, IJobOrderHeaderService JobOrderHeaderServ, IJobOrderSettingsService jobOrderSettingsServ
            , IDocumentValidation validator)
        {
            _JobOrderLineService = JobOrder;
            _jobOrderHeaderService = JobOrderHeaderServ;
            _jobOrderSettingsService = jobOrderSettingsServ;
            _exception = exec;
            _validator = validator;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
        }

        public ActionResult _ForProdOrder(int id, int jid)
        {
            JobOrderLineFilterViewModel vm = new JobOrderLineFilterViewModel();

            var Header = _jobOrderHeaderService.Find(id);

            var Settings = _jobOrderSettingsService.GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            vm.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(Settings);
            vm.DealUnitId = Settings.DealUnitId;
            vm.JobOrderHeaderId = id;
            vm.JobWorkerId = jid;
            PrepareViewBag(null);
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobOrderLineFilterViewModel vm)
        {

            if (vm.JobOrderSettings.isVisibleRate && vm.JobOrderSettings.isMandatoryRate && (vm.Rate == null || vm.Rate == 0))
            {
                ModelState.AddModelError("", "Rate is mandatory");
                PrepareViewBag(null);
                return PartialView("_Filters", vm);
            }


            List<JobOrderLineViewModel> temp = _JobOrderLineService.GetProdOrdersForFilters(vm).ToList();

            bool UnitConvetsionException = (from p in temp
                                            where p.UnitConversionException == true
                                            select p).Any();

            if (UnitConvetsionException)
            {
                ModelState.AddModelError("", "Unit Conversion are missing for few Products");
            }

            JobOrderMasterDetailModel svm = new JobOrderMasterDetailModel();
            svm.JobOrderLineViewModel = temp;

            JobOrderHeader Header = _jobOrderHeaderService.Find(vm.JobOrderHeaderId);

            JobOrderSettings Settings = _jobOrderSettingsService.GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            svm.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(Settings);

            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobOrderMasterDetailModel vm)
        {

            //List<string> uids = new List<string>();

            //if (!string.IsNullOrEmpty(Settings.SqlProcGenProductUID))
            //{
            //    uids = _JobOrderLineService.GetProcGenProductUids(Header.DocTypeId, Qty, Header.DivisionId, Header.SiteId);
            //}



            if (ModelState.IsValid)
            {

                try
                {
                    _JobOrderLineService.SaveMultiple(vm, User.Identity.Name);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_Results", vm);
                }


                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }


        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _JobOrderLineService.GetJobOrderLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult ConsumptionIndex(int id)
        {
            var p = _JobOrderLineService.GetConsumptionLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        private void PrepareViewBag(JobOrderLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = _JobOrderLineService.GetUnitList();
            if (vm != null)
            {
                JobOrderHeaderViewModel H = _jobOrderHeaderService.GetJobOrderHeader(vm.JobOrderHeaderId);
                ViewBag.DocNo = H.DocTypeName + "-" + H.DocNo;
            }
        }

        [HttpGet]
        public ActionResult CreateLine(int id, bool? IsProdBased)
        {
            return _Create(id, null, IsProdBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, bool? IsProdBased)
        {
            return _Create(id, null, IsProdBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int id, bool? IsProdBased)
        {
            return _Create(id, null, IsProdBased);
        }

        public ActionResult _Create(int Id, DateTime? date, bool? IsProdBased) //Id ==>Sale Order Header Id
        {
            JobOrderHeader H = _jobOrderHeaderService.Find(Id);
            JobOrderLineViewModel s = new JobOrderLineViewModel();

            //Getting Settings
            var settings = _jobOrderSettingsService.GetJobOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);

            var count = _JobOrderLineService.GetJobOrderLineListForIndex(Id).Count();
            if (count > 0)
            {
                s.NonCountedQty = _JobOrderLineService.GetJobOrderLineListForIndex(Id).OrderByDescending(m => m.JobOrderLineId).FirstOrDefault().NonCountedQty;
                s.LossQty = _JobOrderLineService.GetJobOrderLineListForIndex(Id).OrderByDescending(m => m.JobOrderLineId).FirstOrDefault().LossQty;
                s.DealUnitId = _JobOrderLineService.GetJobOrderLineListForIndex(Id).OrderByDescending(m => m.JobOrderLineId).FirstOrDefault().DealUnitId;
            }
            else
            {
                s.NonCountedQty = settings.NonCountedQty;
                s.LossQty = settings.LossQty;
                s.DealUnitId = settings.DealUnitId;
            }
            s.GodownId = H.GodownId;
            s.AllowRepeatProcess = false;
            s.JobOrderHeaderId = H.JobOrderHeaderId;
            ViewBag.Status = H.Status;
            s.IsProdOrderBased = IsProdBased ?? true;
            s.DocTypeId = H.DocTypeId;
            s.SiteId = H.SiteId;
            s.DivisionId = H.DivisionId;
            //if (date != null) s.DueDate = date??DateTime.Today;
            PrepareViewBag(s);
            ViewBag.LineMode = "Create";
            //if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            //{
            //    ViewBag.CSEXCL = TempData["CSEXCL"];
            //    TempData["CSEXCL"] = null;
            //}
            if (IsProdBased == true)
            {
                return PartialView("_CreateForProdOrder", s);

            }
            else
            {
                return PartialView("_Create", s);
            }

        }






        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobOrderLineViewModel svm)
        {

            bool BeforeSave = true;
            JobOrderHeader temp = _jobOrderHeaderService.Find(svm.JobOrderHeaderId);

            var settings = _jobOrderSettingsService.GetJobOrderSettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            if (settings != null)
            {
                if (settings.isVisibleProcessLine == true && settings.isMandatoryProcessLine == true && (svm.FromProcessId <= 0 || svm.FromProcessId == null))
                {
                    ModelState.AddModelError("FromProcessId", "The Process field is required");
                }
                if (settings.isVisibleRate == true && settings.isMandatoryRate == true && svm.Rate <= 0)
                {
                    ModelState.AddModelError("Rate", "The Rate field is required");
                }
                if (settings.isVisibleProductUID == true && settings.isMandatoryProductUID == true && !svm.ProductUidId.HasValue)
                {
                    ModelState.AddModelError("ProductUidIdName", "The ProductUid field is required");
                }
            }


            if (svm.Qty <= 0)
                ModelState.AddModelError("Qty", "The Qty is required");

            if (svm.DealQty <= 0)
            {
                ModelState.AddModelError("DealQty", "DealQty field is required");
            }



            ViewBag.Status = temp.Status;


            if (svm.DueDate != null && temp.DocDate > svm.DueDate)
            {
                ModelState.AddModelError("DueDate", "Duedate should be greater than Docdate");
            }

            if (svm.JobOrderLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid && BeforeSave)
            {

                if (svm.JobOrderLineId <= 0)
                {
                    try
                    {
                        _JobOrderLineService.Create(svm, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(svm);

                        if (svm.ProdOrderLineId != null)
                        {
                            return PartialView("_CreateForProdOrder", svm);
                        }
                        else
                        {
                            return PartialView("_Create", svm);
                        }

                    }

                    return RedirectToAction("_Create", new { id = svm.JobOrderHeaderId, IsProdBased = (svm.ProdOrderLineId == null ? false : true) });

                }


                else
                {

                    try
                    {
                        _JobOrderLineService.Update(svm, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(svm);
                        if (svm.ProdOrderLineId != null)
                        {
                            return PartialView("_CreateForProdOrder", svm);
                        }
                        else
                        {
                            return PartialView("_Create", svm);
                        }
                    }

                    return Json(new { success = true });
                }

            }
            PrepareViewBag(svm);
            if (svm.ProdOrderLineId != null)
            {
                return PartialView("_CreateForProdOrder", svm);
            }
            else
            {
                return PartialView("_Create", svm);
            }
        }





        [HttpGet]
        public ActionResult _ModifyLine(int id)
        {
            return _Modify(id);
        }

        [HttpGet]
        public ActionResult _ModifyLineAfterSubmit(int id)
        {
            return _Modify(id);
        }

        [HttpGet]
        public ActionResult _ModifyLineAfterApprove(int id)
        {
            return _Modify(id);
        }

        private ActionResult _Modify(int id)
        {
            JobOrderLineViewModel temp = _JobOrderLineService.GetJobOrderLine(id);

            JobOrderHeader H = _jobOrderHeaderService.Find(temp.JobOrderHeaderId);

            //Getting Settings
            var settings = _jobOrderSettingsService.GetJobOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);

            //ViewBag.DocNo = H.DocNo;
            temp.GodownId = H.GodownId;
            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(temp);

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = _validator.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXCL"] += ExceptionMsg;
            #endregion

            if ((TimePlanValidation || Continue))
                ViewBag.LineMode = "Edit";


            if (temp.ProdOrderLineId != null)
            {
                return PartialView("_CreateForProdOrder", temp);

            }
            else
            {
                return PartialView("_Create", temp);
            }
        }


        [HttpGet]
        public ActionResult _DeleteLine(int id)
        {
            return _Delete(id);
        }
        [HttpGet]
        public ActionResult _DeleteLine_AfterSubmit(int id)
        {
            return _Delete(id);
        }

        [HttpGet]
        public ActionResult _DeleteLine_AfterApprove(int id)
        {
            return _Delete(id);
        }

        private ActionResult _Delete(int id)
        {
            JobOrderLineViewModel temp = _JobOrderLineService.GetJobOrderLine(id);

            JobOrderHeader H = _jobOrderHeaderService.Find(temp.JobOrderHeaderId);

            //Getting Settings
            var settings = _jobOrderSettingsService.GetJobOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);

            temp.GodownId = H.GodownId;
            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(temp);
            //ViewBag.LineMode = "Delete";

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = _validator.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXCL"] += ExceptionMsg;
            #endregion

            if ((TimePlanValidation || Continue))
                ViewBag.LineMode = "Delete";

            if (temp.ProdOrderLineId != null)
            {
                return PartialView("_CreateForProdOrder", temp);
            }
            else
            {
                return PartialView("_Create", temp);
            }
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobOrderLineViewModel vm)
        {
            bool BeforeSave = true;

            if (BeforeSave)
            {
                try
                {
                    _JobOrderLineService.Delete(vm, User.Identity.Name);
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    PrepareViewBag(vm);
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);

                }
            }

            return Json(new { success = true });

        }



        public JsonResult GetProductDetailJson(int ProductId, int JobOrderId)
        {
            ProductViewModel product = _JobOrderLineService.GetProduct(ProductId);

            var DealUnitId = _JobOrderLineService.GetJobOrderLineListForIndex(JobOrderId).OrderByDescending(m => m.JobOrderLineId).FirstOrDefault();

            //Decimal Rate = _JobOrderLineService.GetJobRate(JobOrderId, ProductId);

            Decimal Rate = 0;
            Decimal Discount = 0;
            Decimal Incentive = 0;
            Decimal Loss = 0;

            IEnumerable<JobRate> RateList = _JobOrderLineService.GetJobRate(JobOrderId, ProductId);

            if (RateList != null)
            {
                Rate = RateList.FirstOrDefault().Rate ?? 0;
                Discount = RateList.FirstOrDefault().DiscountRate ?? 0;
                Incentive = RateList.FirstOrDefault().IncentiveRate ?? 0;
                Loss = RateList.FirstOrDefault().LossRate ?? 0;
            }


            var Record = _jobOrderHeaderService.Find(JobOrderId);

            var Settings = _jobOrderSettingsService.GetJobOrderSettingsForDocument(Record.DocTypeId, Record.DivisionId, Record.SiteId);

            var DlUnit = _JobOrderLineService.FindUnit(!string.IsNullOrEmpty(Settings.JobUnitId) ? Settings.JobUnitId : ((DealUnitId == null) ? (Settings.DealUnitId == null ? product.UnitId : Settings.DealUnitId) : DealUnitId.DealUnitId));

            return Json(new
            {
                ProductId = product.ProductId,
                StandardCost = Rate,
                Discount = Discount,
                Incentive = Incentive,
                Loss = Loss,
                UnitId = (!string.IsNullOrEmpty(Settings.JobUnitId) ? Settings.JobUnitId : product.UnitId),
                DealUnitId = !string.IsNullOrEmpty(Settings.JobUnitId) ? Settings.JobUnitId : ((DealUnitId == null) ? (Settings.DealUnitId == null ? product.UnitId : Settings.DealUnitId) : DealUnitId.DealUnitId),
                DealUnitDecimalPlaces = DlUnit.DecimalPlaces,
                Specification = product.ProductSpecification
            });
        }
        public JsonResult getunitconversiondetailjson(int prodid, string UnitId, string DealUnitId, int JobOrderId)
        {


            var Header = _jobOrderHeaderService.Find(JobOrderId);

            int DOctypeId = Header.DocTypeId;
            int siteId = Header.SiteId;
            int DivId = Header.DivisionId;


            var uc = _JobOrderLineService.GetUnitConversion(prodid, UnitId, (int)Header.UnitConversionForId, DealUnitId);


            byte DecimalPlaces = _JobOrderLineService.FindUnit(DealUnitId).DecimalPlaces;
            string Text;
            string Value;


            if (uc != null)
            {
                Text = uc.Multiplier.ToString();
                Value = uc.Multiplier.ToString();
            }
            else
            {
                Text = "0";
                Value = "0";
            }


            return Json(new { Text = Text, Value = Value, DecimalPlace = DecimalPlaces });
        }

        public JsonResult GetPendingProdOrders(int ProductId)
        {
            return Json(_JobOrderLineService.GetPendingProdOrders(ProductId).ToList());
        }

        public JsonResult GetProdOrderDetail(int ProdOrderLineId, int JobOrderHeaderId)
        {
            var temp = _JobOrderLineService.GetProdOrderDetailBalance(ProdOrderLineId);

            var DealUnitId = _JobOrderLineService.GetJobOrderLineListForIndex(JobOrderHeaderId).OrderByDescending(m => m.JobOrderLineId).FirstOrDefault();

            var Record = _jobOrderHeaderService.Find(JobOrderHeaderId);

            var Settings = _jobOrderSettingsService.GetJobOrderSettingsForDocument(Record.DocTypeId, Record.DivisionId, Record.SiteId);

            var DlUnit = _JobOrderLineService.FindUnit((DealUnitId == null) ? (Settings.DealUnitId == null ? temp.UnitId : Settings.DealUnitId) : DealUnitId.DealUnitId);
            temp.DealunitDecimalPlaces = DlUnit.DecimalPlaces;
            temp.DealUnitId = DlUnit.UnitId;

            return Json(temp);
        }

        public JsonResult GetProdOrderForBarCode(int ProdUId, int JobOrderHeaderId)
        {
            var temp = _JobOrderLineService.GetProdOrderForProdUid(ProdUId);
            var detail = GetProdOrderDetail(temp.ProdOrderLineId, JobOrderHeaderId);

            return Json(new { ProdOrder = temp, Detail = detail });
        }


        public JsonResult GetProdOrders(int id, string term, int Limit)//Order Header ID
        {
            return Json(_JobOrderLineService.GetPendingProdOrdersWithPatternMatch(id, term, Limit), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingProdOrdersHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            var temp = _JobOrderLineService.GetPendingProdOrderHelpList(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _JobOrderLineService.GetPendingProdOrderHelpList(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {
            return Json(_JobOrderLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetFlagForAllowRepeatProcess()
        {
            bool AllowRepeatProcess = true;

            return Json(AllowRepeatProcess);
        }

        public ActionResult IsProcessDone(string ProductUidName, int ProcessId)
        {
            //var ProductUid = _JobOrderLineService.FindProductUid(ProductUidName);
            //int ProductUidId = 0;
            //if (ProductUid != null)
            //{
            //    ProductUidId = ProductUid.ProductUIDId;
            //}
            return Json(_JobOrderLineService.CheckProductUidProcessDone(ProductUidName, ProcessId));
        }



        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
            {
                GenerateCookie.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);
                TempData.Remove("CSEXC");
            }
            if (disposing)
            {
                _JobOrderLineService.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
