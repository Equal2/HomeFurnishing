using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;
using Jobs.Helpers;

namespace Jobs.Controllers
{

    [Authorize]
    public class SaleInvoiceLineController : System.Web.Mvc.Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleInvoiceLineService _SaleInvoiceLineService;
        ISaleInvoiceLineDetailService _SaleInvoiceLineDetailService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public SaleInvoiceLineController(ISaleInvoiceLineService SaleInvoice, ISaleInvoiceLineDetailService SaleInvoiceDetail, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleInvoiceLineService = SaleInvoice;
            _SaleInvoiceLineDetailService = SaleInvoiceDetail;
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
            //var p = _SaleInvoiceLineService.GetSaleInvoiceLineListForIndex(id).ToList();
            var p = _SaleInvoiceLineService.GetDirectSaleInvoiceLineListForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }



        public ActionResult _ForDispatch(int id)
        {
            SaleInvoiceFilterViewModel vm = new SaleInvoiceFilterViewModel();
            vm.SaleInvoiceHeaderId = id;
            vm.UpToDate = DateTime.Today.Date;
            SaleInvoiceHeader H = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(id);
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            return PartialView("_DispatchFilters", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostDispatch(SaleInvoiceFilterViewModel vm)
        {
            List<DirectSaleInvoiceLineViewModel> temp = _SaleInvoiceLineService.GetSaleDispatchForFilters(vm);
            DirectSaleInvoiceListViewModel svm = new DirectSaleInvoiceListViewModel();
            svm.DirectSaleInvoiceLineViewModel = temp;
            return PartialView("_Results", svm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(DirectSaleInvoiceListViewModel vm)
        {
            int Cnt = 0;

            SaleInvoiceHeader Sh = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(vm.DirectSaleInvoiceLineViewModel.FirstOrDefault().SaleInvoiceHeaderId);




            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            bool HeaderChargeEdit = false;

            //SaleInvoiceHeader Header = new SaleInvoiceHeaderService(_unitOfWork).Find(vm.DirectSaleInvoiceLineViewModel.FirstOrDefault().SaleInvoiceHeaderId);

            SaleInvoiceSetting Settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(Sh.DocTypeId, Sh.DivisionId, Sh.SiteId);

            int? MaxLineId = new SaleInvoiceLineChargeService(_unitOfWork).GetMaxProductCharge(Sh.SaleInvoiceHeaderId, "Web.SaleInvoiceLines", "SaleInvoiceHeaderId", "SaleInvoiceLineId");

            int PersonCount = 0;
            int CalculationId = Settings.CalculationId;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            if (ModelState.IsValid)
            {
                foreach (var item in vm.DirectSaleInvoiceLineViewModel)
                {
                    decimal balqty = (from p in db.ViewSaleDispatchBalance
                                      where p.SaleDispatchLineId == item.SaleDispatchLineId
                                      select p.BalanceQty).FirstOrDefault();
                    if (item.Qty > 0 && item.Qty <= balqty)
                    {

                        SaleInvoiceLine line = new SaleInvoiceLine();

                        line.SaleInvoiceHeaderId = item.SaleInvoiceHeaderId;
                        line.SaleDispatchLineId = item.SaleDispatchLineId;
                        line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        line.Rate = item.Rate;
                        line.DealUnitId = item.DealUnitId;
                        line.DealQty = item.Qty * item.UnitConversionMultiplier ?? 0;
                        line.DiscountPer = item.DiscountPer;
                        if (Settings.CalculateDiscountOnRate)
                        {
                            var temprate = item.Rate - (item.Rate * item.DiscountPer / 100);
                            line.Amount = line.DealQty * temprate ?? 0;
                        }
                        else
                        {
                            var DiscountAmt = (item.Rate * line.DealQty) * item.DiscountPer / 100;
                            line.Amount = (item.Rate * line.DealQty) - (DiscountAmt ?? 0);
                        }
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.SaleInvoiceLineId = pk;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.SaleOrderLineId = item.SaleOrderLineId;
                        line.DiscountPer = item.DiscountPer;
                        line.ProductId = item.ProductId;
                        line.Qty = item.Qty;
                        line.Remark = item.Remark;
                        line.SaleDispatchLineId = item.SaleDispatchLineId;
                        line.ObjectState = Model.ObjectState.Added;
                        _SaleInvoiceLineService.Create(line);


                        SaleInvoiceLineDetail linedetail = new SaleInvoiceLineDetail();
                        linedetail.SaleInvoiceLineId = line.SaleInvoiceLineId;
                        linedetail.RewardPoints = item.RewardPoints;
                        _SaleInvoiceLineDetailService.Create(linedetail);



                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.SaleInvoiceLineId, HeaderTableId = item.SaleInvoiceHeaderId, PersonID = Sh.BillToBuyerId });

                        pk++;
                        Cnt = Cnt + 1;
                    }
                }

                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, vm.DirectSaleInvoiceLineViewModel.FirstOrDefault().SaleInvoiceHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.SaleInvoiceHeaderCharges", "Web.SaleInvoiceLineCharges", out PersonCount, Sh.DocTypeId, Sh.SiteId, Sh.DivisionId);

                //Saving Charges
                foreach (var item in LineCharges)
                {
                    SaleInvoiceLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, SaleInvoiceLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    new SaleInvoiceLineChargeService(_unitOfWork).Create(PoLineCharge);

                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        SaleInvoiceHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, SaleInvoiceHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.DirectSaleInvoiceLineViewModel.FirstOrDefault().SaleInvoiceHeaderId;
                        POHeaderCharge.PersonID = Sh.BillToBuyerId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        new SaleInvoiceHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new SaleInvoiceHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        new SaleInvoiceHeaderChargeService(_unitOfWork).Update(footercharge);
                    }

                }

                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_Results", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Sh.DocTypeId,
                    DocId = Sh.SaleInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Sh.DocNo,
                    DocDate = Sh.DocDate,
                    DocStatus = Sh.Status,
                }));


                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }

        private void PrepareViewBag()
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        [HttpGet]
        public ActionResult CreateLine(int id, bool? IsSaleBased)
        {
            return _Create(id, IsSaleBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, bool? IsSaleBased)
        {
            return _Create(id, IsSaleBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int id, bool? IsSaleBased)
        {
            return _Create(id, IsSaleBased);
        }

        public ActionResult _Create(int Id, bool? IsSaleBased) //Id ==>Sale Order Header Id
        {
            SaleInvoiceHeader H = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(Id);
            DirectSaleInvoiceLineViewModel s = new DirectSaleInvoiceLineViewModel();



            //Getting Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.IsSaleBased = IsSaleBased;
            s.SaleInvoiceHeaderId = H.SaleInvoiceHeaderId;
            s.SaleInvoiceHeaderDocNo = H.DocNo;
            s.DocTypeId = H.DocTypeId;
            s.SiteId = H.SiteId;
            s.DivisionId = H.DivisionId;


            ViewBag.LineMode = "Create";
            PrepareViewBag();
            return PartialView("_Create", s);

        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult _CreatePost(DirectSaleInvoiceLineViewModel svm)
        {
            SaleInvoiceHeader Sh = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(svm.SaleInvoiceHeaderId);

            if (svm.SaleInvoiceLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.SaleDispatchLineId <= 0)
            {
                ModelState.AddModelError("SaleDispatchLineId", "Sale Dispatch field is required");
            }

            if (svm.Qty <= 0)
                ModelState.AddModelError("Qty", "The Qty field is required");

            if (ModelState.IsValid)
            {
                if (svm.SaleInvoiceLineId <= 0)
                {
                    SaleInvoiceLine Sl = Mapper.Map<DirectSaleInvoiceLineViewModel, SaleInvoiceLine>(svm);
                    SaleInvoiceLineDetail Sid = Mapper.Map<DirectSaleInvoiceLineViewModel, SaleInvoiceLineDetail>(svm);

                    Sl.SaleDispatchLineId = svm.SaleDispatchLineId;
                    Sl.SaleInvoiceHeaderId = Sh.SaleInvoiceHeaderId;
                    Sl.DiscountPer = svm.DiscountPer;
                    Sl.Sr = _SaleInvoiceLineService.GetMaxSr(Sh.SaleInvoiceHeaderId);
                    Sl.CreatedDate = DateTime.Now;
                    Sl.ModifiedDate = DateTime.Now;
                    Sl.CreatedBy = User.Identity.Name;
                    Sl.ModifiedBy = User.Identity.Name;
                    Sl.ObjectState = Model.ObjectState.Added;
                    _SaleInvoiceLineService.Create(Sl);

                    Sid.SaleInvoiceLineId = Sl.SaleInvoiceLineId;
                    _SaleInvoiceLineDetailService.Create(Sid);





                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            item.LineTableId = Sl.SaleInvoiceLineId;
                            item.PersonID = Sh.BillToBuyerId;
                            item.HeaderTableId = Sh.SaleInvoiceHeaderId;
                            item.ObjectState = Model.ObjectState.Added;
                            new SaleInvoiceLineChargeService(_unitOfWork).Create(item);
                        }

                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {

                            if (item.Id > 0)
                            {

                                var footercharge = new SaleInvoiceHeaderChargeService(_unitOfWork).Find(item.Id);
                                footercharge.Rate = item.Rate;
                                footercharge.Amount = item.Amount;
                                new SaleInvoiceHeaderChargeService(_unitOfWork).Update(footercharge);

                            }

                            else
                            {
                                item.HeaderTableId = Sh.SaleInvoiceHeaderId;
                                item.PersonID = Sh.BillToBuyerId;
                                item.ObjectState = Model.ObjectState.Added;
                                new SaleInvoiceHeaderChargeService(_unitOfWork).Create(item);
                            }
                        }



                    if (Sh.Status != (int)StatusConstants.Drafted)
                    {
                        Sh.Status = (int)StatusConstants.Modified;
                        new SaleInvoiceHeaderService(_unitOfWork).Update(Sh);
                    }

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag();
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Sh.DocTypeId,
                        DocId = Sl.SaleInvoiceHeaderId,
                        DocLineId = Sl.SaleInvoiceLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = Sh.DocNo,
                        DocDate = Sh.DocDate,
                        DocStatus = Sh.Status,
                    }));


                    return RedirectToAction("_Create", new { id = Sh.SaleInvoiceHeaderId, IsSaleBased = (Sl.SaleDispatchLineId == null ? false : true) });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    int status = Sh.Status;

                    SaleInvoiceLine Sl = _SaleInvoiceLineService.Find(svm.SaleInvoiceLineId);
                    SaleInvoiceLineDetail Sid = _SaleInvoiceLineDetailService.Find(svm.SaleInvoiceLineId);

                    SaleInvoiceLine ExRecS = new SaleInvoiceLine();
                    ExRecS = Mapper.Map<SaleInvoiceLine>(Sl);

                    Sl.Dimension1Id = svm.Dimension1Id;
                    Sl.Dimension2Id = svm.Dimension2Id;
                    Sl.ProductId = svm.ProductId;
                    Sl.DiscountPer = svm.DiscountPer;
                    Sl.Qty = svm.Qty;
                    Sl.Amount = svm.Amount;
                    Sl.Weight = svm.Weight;
                    Sl.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    Sl.DealQty = svm.DealQty;
                    Sl.DealUnitId = svm.DealUnitId;
                    Sl.Rate = svm.Rate;
                    Sl.Remark = svm.Remark;
                    Sl.ModifiedDate = DateTime.Now;
                    Sl.ModifiedBy = User.Identity.Name;
                    Sl.ObjectState = Model.ObjectState.Modified;
                    _SaleInvoiceLineService.Update(Sl);

                    if (svm.RewardPoints != null)
                    {
                        Sid.RewardPoints = svm.RewardPoints;
                    }
                    _SaleInvoiceLineDetailService.Update(Sid);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecS,
                        Obj = Sl,
                    });


                    if (Sh.Status != (int)StatusConstants.Drafted)
                    {
                        Sh.Status = (int)StatusConstants.Modified;
                        new SaleInvoiceHeaderService(_unitOfWork).Update(Sh);
                    }


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = new SaleInvoiceLineChargeService(_unitOfWork).Find(item.Id);
                            SaleInvoiceLineCharge ExRecLine = new SaleInvoiceLineCharge();
                            ExRecLine = Mapper.Map<SaleInvoiceLineCharge>(productcharge);

                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            new SaleInvoiceLineChargeService(_unitOfWork).Update(productcharge);
                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecLine,
                                Obj = productcharge,
                            });
                        }


                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = new SaleInvoiceHeaderChargeService(_unitOfWork).Find(item.Id);
                            SaleInvoiceHeaderCharge ExRecLine = new SaleInvoiceHeaderCharge();
                            ExRecLine = Mapper.Map<SaleInvoiceHeaderCharge>(footercharge);

                            footercharge.Rate = item.Rate;
                            footercharge.Amount = item.Amount;
                            new SaleInvoiceHeaderChargeService(_unitOfWork).Update(footercharge);
                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecLine,
                                Obj = footercharge,
                            });
                        }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag();
                        return PartialView("_Create", svm);
                    }

                    //Saving the Activity Log      

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Sh.DocTypeId,
                        DocId = Sl.SaleInvoiceHeaderId,
                        DocLineId = Sl.SaleInvoiceLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = Sh.DocNo,
                        xEModifications = Modifications,
                        DocDate = Sh.DocDate,
                        DocStatus = Sh.Status,
                    }));

                    //End of Saving the Activity Log

                    return Json(new { success = true });

                }
            }
            PrepareViewBag();
            return PartialView("_Create", svm);
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
            SaleInvoiceLine temp = _SaleInvoiceLineService.Find(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

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

            SaleInvoiceHeader H = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(temp.SaleInvoiceHeaderId);
            PrepareViewBag();

            DirectSaleInvoiceLineViewModel vm = _SaleInvoiceLineService.GetSaleInvoiceLineForEdit(id);
            //Getting Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            return PartialView("_Create", vm);
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
            SaleInvoiceLine temp = _SaleInvoiceLineService.Find(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

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


            SaleInvoiceHeader H = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(temp.SaleInvoiceHeaderId);
            PrepareViewBag();

            DirectSaleInvoiceLineViewModel vm = _SaleInvoiceLineService.GetDirectSaleInvoiceLineForEdit(id);
            //Getting Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            return PartialView("_Create", vm);
        }

        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DeletePost(DirectSaleInvoiceLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            SaleInvoiceHeader Sh = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(vm.SaleInvoiceHeaderId);

            int status = Sh.Status;

            SaleInvoiceLine Sl = _SaleInvoiceLineService.Find(vm.SaleInvoiceLineId);
            SaleInvoiceLineDetail Sid = _SaleInvoiceLineDetailService.Find(vm.SaleInvoiceLineId);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = Sl,
            });


            _SaleInvoiceLineDetailService.Delete(Sid);
            _SaleInvoiceLineService.Delete(Sl);
            
            

            if (Sh.Status != (int)StatusConstants.Drafted)
            {
                Sh.Status = (int)StatusConstants.Modified;
                new SaleInvoiceHeaderService(_unitOfWork).Update(Sh);
            }

            var chargeslist = new SaleInvoiceLineChargeService(_unitOfWork).GetCalculationProductList(vm.SaleInvoiceLineId);

            if (chargeslist != null)
                foreach (var item in chargeslist)
                {
                    new SaleInvoiceLineChargeService(_unitOfWork).Delete(item.Id);
                }

            if (vm.footercharges != null)
                foreach (var item in vm.footercharges)
                {
                    var footer = new SaleInvoiceHeaderChargeService(_unitOfWork).Find(item.Id);
                    footer.Rate = item.Rate;
                    footer.Amount = item.Amount;
                    new SaleInvoiceHeaderChargeService(_unitOfWork).Update(footer);
                }
            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                PrepareViewBag();
                return PartialView("_Create", vm);

            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = Sh.DocTypeId,
                DocId = Sh.SaleInvoiceHeaderId,
                DocLineId = Sl.SaleInvoiceLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = Sh.DocNo,
                xEModifications = Modifications,
                DocDate = Sh.DocDate,
                DocStatus = Sh.Status,
            }));

            return Json(new { success = true });
        }

        public ActionResult _Detail(int id)
        {


            SaleInvoiceLine temp = _SaleInvoiceLineService.Find(id);
            SaleInvoiceHeader H = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(temp.SaleInvoiceHeaderId);
            PrepareViewBag();

            DirectSaleInvoiceLineViewModel vm = _SaleInvoiceLineService.GetDirectSaleInvoiceLineForEdit(id);
            //Getting Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", vm);

        }

        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
            {
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);
                TempData.Remove("CSEXC");
            }
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult GetProductCodeDetailJson(string ProductCode, int SaleInvoiceHeaderId)
        {
            Product Product = (from P in db.Product
                               where P.ProductCode == ProductCode
                               select P).FirstOrDefault();

            if (Product != null)
            {
                return GetProductDetailJson(Product.ProductId, SaleInvoiceHeaderId);
            }
            else
            {
                return null;
            }
        }

        public JsonResult GetProductDetailJson(int ProductId, int SaleInvoiceHeaderId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);
            //ProductViewModel ProductJson = new ProductViewModel();

            var DealUnitId = _SaleInvoiceLineService.GetSaleInvoiceLineList(SaleInvoiceHeaderId).OrderByDescending(m => m.SaleInvoiceLineId).FirstOrDefault();

            var DlUnit = new UnitService(_unitOfWork).Find((DealUnitId == null) ? product.UnitId : DealUnitId.DealUnitId);


            return Json(new { ProductId = product.ProductId, StandardCost = product.StandardCost, UnitId = product.UnitId, UnitName = product.UnitName, DealUnitId = (DealUnitId == null) ? product.UnitId : DealUnitId.DealUnitId, DealUnitDecimalPlaces = DlUnit.DecimalPlaces, Specification = product.ProductSpecification, ProductCode = product.ProductCode, ProductName = product.ProductName });
        }

        

        public JsonResult GetDispatchDetail(int DispatchId)
        {
            return Json(new SaleDispatchLineService(_unitOfWork).GetSaleDispatchDetailForInvoice(DispatchId));
        }

        public JsonResult getunitconversiondetailjson(int productid, string unitid, string DealUnitId, int SaleInvoiceHeaderId)
        {

            SaleInvoiceHeader Invoice = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(SaleInvoiceHeaderId);

            var Settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(Invoice.DocTypeId, Invoice.DivisionId, Invoice.SiteId);

            if (Settings.UnitConversionForId.HasValue && Settings.UnitConversionForId > 0)
            {
                UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversionForUCF(productid, unitid, DealUnitId, Settings.UnitConversionForId ?? 0);
                List<SelectListItem> unitconversionjson = new List<SelectListItem>();
                if (uc != null)
                {
                    unitconversionjson.Add(new SelectListItem
                    {
                        Text = uc.Multiplier.ToString(),
                        Value = uc.Multiplier.ToString()
                    });
                }
                else
                {
                    unitconversionjson.Add(new SelectListItem
                    {
                        Text = "0",
                        Value = "0"
                    });
                }


                return Json(unitconversionjson);
            }
            else
            {
                UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(productid, unitid, DealUnitId);
                List<SelectListItem> unitconversionjson = new List<SelectListItem>();
                if (uc != null)
                {
                    unitconversionjson.Add(new SelectListItem
                    {
                        Text = uc.Multiplier.ToString(),
                        Value = uc.Multiplier.ToString()
                    });
                }
                else
                {
                    unitconversionjson.Add(new SelectListItem
                    {
                        Text = "0",
                        Value = "0"
                    });
                }


                return Json(unitconversionjson);
            }

        }

        public JsonResult GetProductUIDDetailJson(string ProductUIDNo)
        {
            ProductUidDetail productuiddetail = new ProductUidService(_unitOfWork).FGetProductUidDetail(ProductUIDNo);

            List<ProductUidDetail> ProductUidDetailJson = new List<ProductUidDetail>();

            if (productuiddetail != null)
            {
                ProductUidDetailJson.Add(new ProductUidDetail()
                {
                    ProductId = productuiddetail.ProductId,
                    ProductName = productuiddetail.ProductName,
                    ProductUidId = productuiddetail.ProductUidId,
                });
            }

            return Json(ProductUidDetailJson);
        }

        public JsonResult GetSaleDispatchDetailJson(int SaleDispatchLineId)
        {
            var temp = (from L in db.ViewSaleDispatchBalance
                        join Dl in db.SaleDispatchLine on L.SaleDispatchLineId equals Dl.SaleDispatchLineId into SaleDispatchLineTable from SaleDispatchLineTab in SaleDispatchLineTable.DefaultIfEmpty()
                        join Pl in db.PackingLine on SaleDispatchLineTab.PackingLineId equals Pl.PackingLineId into PackingLineTable from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                        join P in db.Product on L.ProductId equals P.ProductId into ProductTable from ProductTab in ProductTable.DefaultIfEmpty()
                        join U in db.Units on ProductTab.UnitId equals U.UnitId into UnitTable from UnitTab in UnitTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on PackingLineTab.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on PackingLineTab.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where L.SaleDispatchLineId == SaleDispatchLineId
                        select new
                        {
                            SaleDispatchHeaderDocNo = L.SaleDispatchNo,
                            UnitId = UnitTab.UnitId,
                            UnitName = UnitTab.UnitName,
                            DealUnitId = PackingLineTab.DealUnitId,
                            Specification = PackingLineTab.Specification,
                            UnitConversionMultiplier = PackingLineTab.UnitConversionMultiplier,
                            ProductId = L.ProductId,
                            Dimension1Id = L.Dimension1Id,
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Id = L.Dimension2Id,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            SaleOrderLineId = L.SaleOrderLineId,
                            Rate = L.Rate,
                            BalanceQty = L.BalanceQty
                        }).FirstOrDefault();

            if (temp != null)
            {
                return Json(temp);
            }
            else
            {
                return null;
            }
        }


        public JsonResult SetSingleSaleDispatchLine(int Ids)
        {
            ComboBoxResult SaleDispatchJson = new ComboBoxResult();

            var SaleDispatchLine = from L in db.SaleDispatchLine
                                   join H in db.SaleDispatchHeader on L.SaleDispatchHeaderId equals H.SaleDispatchHeaderId into SaleDispatchHeaderTable
                                   from SaleDispatchHeaderTab in SaleDispatchHeaderTable.DefaultIfEmpty()
                                   where L.SaleDispatchLineId == Ids
                                select new
                                {
                                    SaleDispatchLineId = L.SaleDispatchLineId,
                                    SaleDispatchNo = L.PackingLine.Product.ProductName
                                };

            SaleDispatchJson.id = SaleDispatchLine.FirstOrDefault().ToString();
            SaleDispatchJson.text = SaleDispatchLine.FirstOrDefault().SaleDispatchNo;

            return Json(SaleDispatchJson);
        }

        public ActionResult GetCustomProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleInvoiceLineService.GetCustomProducts(filter, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSaleDispatchForProduct(string searchTerm, int pageSize, int pageNum, int PersonId)//DocTypeId
        {
            var Query = _SaleInvoiceLineService.GetSaleDispatchHelpListForProduct(PersonId, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult GetCustomProductsForSaleDispatch(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var Query = _SaleInvoiceLineService.GetPendingProductsForSaleInvoice(filter, searchTerm);

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetSaleDispatch(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleInvoiceLineService.GetPendingDispatchForInvoice(filter, searchTerm);

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //return Json(_SaleInvoiceLineService.GetPendingDispatchForInvoice(id, term, Limit).ToList(), JsonRequestBehavior.AllowGet);
        }

    }
}
