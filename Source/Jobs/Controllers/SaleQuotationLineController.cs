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
using Jobs.Helpers;
using Model.ViewModel;
using System.Xml.Linq;
using DocumentEvents;
using CustomEventArgs;
using SaleQuotationDocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class SaleQuotationLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleQuotationLineService _SaleQuotationLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public SaleQuotationLineController(ISaleQuotationLineService SaleQuotation, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleQuotationLineService = SaleQuotation;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult _ForSaleEnquiry(int id, int jid)
        {
            SaleQuotationLineFilterViewModel vm = new SaleQuotationLineFilterViewModel();

            SaleQuotationHeader Header = new SaleQuotationHeaderService(_unitOfWork).Find(id);

            SaleQuotationSettings Settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            vm.SaleQuotationSettings = Mapper.Map<SaleQuotationSettings, SaleQuotationSettingsViewModel>(Settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

            vm.DealUnitId = Settings.DealUnitId;
            vm.SaleQuotationHeaderId = id;
            vm.SaleToBuyerUd = jid;
            PrepareViewBag(null);
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(SaleQuotationLineFilterViewModel vm)
        {
            List<SaleQuotationLineViewModel> temp = _SaleQuotationLineService.GetSaleEnquiriesForFilters(vm).ToList();

            bool UnitConvetsionException = (from p in temp
                                            where p.UnitConversionException == true
                                            select p).Any();

            if (UnitConvetsionException)
            {
                ModelState.AddModelError("", "Unit Conversion are missing for few Products");
            }

            SaleQuotationMasterDetailModel svm = new SaleQuotationMasterDetailModel();
            svm.SaleQuotationLineViewModel = temp;

            SaleQuotationHeader Header = new SaleQuotationHeaderService(_unitOfWork).Find(vm.SaleQuotationHeaderId);

            SaleQuotationSettings Settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

            svm.SaleQuotationSettings = Mapper.Map<SaleQuotationSettings, SaleQuotationSettingsViewModel>(Settings);


            return PartialView("_Results", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(SaleQuotationMasterDetailModel vm)
        {
            int Cnt = 0;
            int Serial = _SaleQuotationLineService.GetMaxSr(vm.SaleQuotationLineViewModel.FirstOrDefault().SaleQuotationHeaderId);
            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            List<SaleQuotationLine> BarCodesPendingToUpdate = new List<SaleQuotationLine>();

            bool BeforeSave = true;
            try
            {
                BeforeSave = SaleQuotationDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.SaleQuotationLineViewModel.FirstOrDefault().SaleQuotationHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");


            int pk = 0;
            bool HeaderChargeEdit = false;

            SaleQuotationHeader Header = new SaleQuotationHeaderService(_unitOfWork).Find(vm.SaleQuotationLineViewModel.FirstOrDefault().SaleQuotationHeaderId);

            SaleQuotationSettings Settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new SaleQuotationLineChargeService(_unitOfWork).GetMaxProductCharge(Header.SaleQuotationHeaderId, "Web.SaleQuotationLines", "SaleQuotationHeaderId", "SaleQuotationLineId");

            int PersonCount = 0;

            decimal Qty = vm.SaleQuotationLineViewModel.Where(m => m.Rate > 0).Sum(m => m.Qty);

            int CalculationId = Settings.CalculationId ?? 0;
            //List<string> uids = new List<string>();

            //if (!string.IsNullOrEmpty(Settings.SqlProcGenProductUID))
            //{
            //    uids = _SaleQuotationLineService.GetProcGenProductUids(Header.DocTypeId, Qty, Header.DivisionId, Header.SiteId);
            //}

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                foreach (var item in vm.SaleQuotationLineViewModel)
                {
                    //if (item.Qty > 0 &&  ((Settings.isMandatoryRate.HasValue && Settings.isMandatoryRate == true )? item.Rate > 0 : 1 == 1))
                    if (item.Qty > 0)
                    {
                        if (item.UnitConversionMultiplier == 0 ||item.UnitConversionMultiplier == null)
                        {
                            item.UnitConversionMultiplier = 1;
                        }

                        SaleQuotationLine line = new SaleQuotationLine();

                        



                        


                        line.SaleQuotationHeaderId = item.SaleQuotationHeaderId;
                        line.SaleEnquiryLineId = item.SaleEnquiryLineId;
                        line.ProductId = item.ProductId;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.Dimension3Id = item.Dimension3Id;
                        line.Dimension4Id = item.Dimension4Id;
                        line.Specification = item.Specification;
                        line.Qty = item.Qty;
                        line.Sr = Serial++;
                        line.Rate = item.Rate;
                        line.DealQty = item.UnitConversionMultiplier * item.Qty;
                        line.DealUnitId = item.DealUnitId;
                        //line.Amount = line.DealQty * line.Rate;
                        line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        line.DiscountPer = item.DiscountPer;
                        if ((Settings.CalculateDiscountOnRate ?? false) == true)
                        {
                            var temprate = item.Rate - (item.Rate * item.DiscountPer / 100);
                            line.DiscountAmount = (item.Rate * item.DiscountPer / 100) * line.DealQty;
                            line.Amount = line.DealQty * temprate ?? 0;
                        }
                        else
                        {
                            var DiscountAmt = (item.Rate * line.DealQty) * item.DiscountPer / 100;
                            line.DiscountAmount = DiscountAmt;
                            line.Amount = (item.Rate * line.DealQty) - (DiscountAmt ?? 0);
                        }
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.SaleQuotationLineId = pk;
                        line.ObjectState = Model.ObjectState.Added;
                        //_SaleQuotationLineService.Create(line);
                        db.SaleQuotationLine.Add(line);

                        



                        if (Settings.CalculationId.HasValue)
                        {
                            LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.SaleQuotationLineId, HeaderTableId = item.SaleQuotationHeaderId, PersonID = Header.SaleToBuyerId, DealQty = line.DealQty });
                        }
                        pk++;
                        Cnt = Cnt + 1;
                    }

                }
                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }
                //new SaleQuotationHeaderService(_unitOfWork).Update(Header);
                Header.ObjectState = Model.ObjectState.Modified;
                db.SaleQuotationHeader.Add(Header);


                try
                {
                    if (Settings.CalculationId.HasValue)
                    {
                        new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, vm.SaleQuotationLineViewModel.FirstOrDefault().SaleQuotationHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.SaleQuotationHeaderCharges", "Web.SaleQuotationLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                        //Saving Charges
                        foreach (var item in LineCharges)
                        {

                            SaleQuotationLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, SaleQuotationLineCharge>(item);
                            PoLineCharge.ObjectState = Model.ObjectState.Added;
                            db.SaleQuotationLineCharge.Add(PoLineCharge);
                            //new SaleQuotationLineChargeService(_unitOfWork).Create(PoLineCharge);

                        }


                        //Saving Header charges
                        for (int i = 0; i < HeaderCharges.Count(); i++)
                        {

                            if (!HeaderChargeEdit)
                            {
                                SaleQuotationHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, SaleQuotationHeaderCharge>(HeaderCharges[i]);
                                POHeaderCharge.HeaderTableId = vm.SaleQuotationLineViewModel.FirstOrDefault().SaleQuotationHeaderId;
                                POHeaderCharge.PersonID = Header.SaleToBuyerId;
                                POHeaderCharge.ObjectState = Model.ObjectState.Added;
                                db.SaleQuotationHeaderCharge.Add(POHeaderCharge);
                                //new SaleQuotationHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                            }
                            else
                            {
                                var footercharge = new SaleQuotationHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                                footercharge.Rate = HeaderCharges[i].Rate;
                                footercharge.Amount = HeaderCharges[i].Amount;
                                footercharge.ObjectState = Model.ObjectState.Modified;
                                db.SaleQuotationHeaderCharge.Add(footercharge);
                                //new SaleQuotationHeaderChargeService(_unitOfWork).Update(footercharge);
                            }

                        }
                    }



                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_Results", vm);
                }

                try
                {
                    SaleQuotationDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.SaleQuotationLineViewModel.FirstOrDefault().SaleQuotationHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                try
                {
                    if (EventException)
                    { throw new Exception(); }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_Results", vm);
                }


                try
                {
                    SaleQuotationDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.SaleQuotationLineViewModel.FirstOrDefault().SaleQuotationHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.SaleQuotationHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,                  
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus=Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }


        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _SaleQuotationLineService.GetSaleQuotationLineListForIndex(id).ToList();

            SaleQuotationHeader Header = new SaleQuotationHeaderService(_unitOfWork).Find(id);

            return Json(p, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult _Index(int id, int Status)
        {
            ViewBag.Status = Status;
            ViewBag.SaleQuotationHeaderId = id;
            var p = _SaleQuotationLineService.GetSaleQuotationLineListForIndex(id).ToList();
            return PartialView(p);
        }



        private void PrepareViewBag(SaleQuotationLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            if (vm != null)
            {
                SaleQuotationHeaderViewModel H = new SaleQuotationHeaderService(_unitOfWork).GetSaleQuotationHeader(vm.SaleQuotationHeaderId);
                ViewBag.DocNo = H.DocTypeName + "-" + H.DocNo;
            }
        }

        [HttpGet]
        public ActionResult CreateLine(int id, bool? IsRefBased)
        {
            return _Create(id, null, IsRefBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, bool? IsRefBased)
        {
            return _Create(id, null, IsRefBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int id, bool? IsRefBased)
        {
            return _Create(id, null, IsRefBased);
        }

        public ActionResult _Create(int Id, DateTime? date, bool? IsRefBased) //Id ==>Sale Order Header Id
        {
            SaleQuotationHeader H = new SaleQuotationHeaderService(_unitOfWork).Find(Id);
            SaleQuotationLineViewModel s = new SaleQuotationLineViewModel();

            //Getting Settings
            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleQuotationSettings = Mapper.Map<SaleQuotationSettings, SaleQuotationSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            var count = _SaleQuotationLineService.GetSaleQuotationLineListForIndex(Id).Count();
            if (count > 0)
            {
                s.DealUnitId = _SaleQuotationLineService.GetSaleQuotationLineListForIndex(Id).OrderByDescending(m => m.SaleQuotationLineId).FirstOrDefault().DealUnitId;
            }
            else
            {
                s.DealUnitId = settings.DealUnitId;
            }
            s.SaleQuotationHeaderId = H.SaleQuotationHeaderId;
            ViewBag.Status = H.Status;
            s.IsRefBased = IsRefBased ?? true;
            s.DocTypeId = H.DocTypeId;
            s.SiteId = H.SiteId;
            s.DivisionId = H.DivisionId;
            s.SalesTaxGroupPersonId = H.SalesTaxGroupPersonId;
            //if (date != null) s.DueDate = date??DateTime.Today;
            PrepareViewBag(s);
            ViewBag.LineMode = "Create";
            //if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            //{
            //    ViewBag.CSEXCL = TempData["CSEXCL"];
            //    TempData["CSEXCL"] = null;
            //}
            if (IsRefBased == true)
            {
                return PartialView("_CreateForSaleEnquiry", s);

            }
            else
            {
                return PartialView("_Create", s);
            }

        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(SaleQuotationLineViewModel svm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            bool BeforeSave = true;
            SaleQuotationHeader temp = new SaleQuotationHeaderService(_unitOfWork).Find(svm.SaleQuotationHeaderId);

            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            if (settings != null)
            {
                if (svm.Rate <= 0)
                {
                    ModelState.AddModelError("Rate", "The Rate field is required");
                }
            }

            #region BeforeSave
            try
            {

                if (svm.SaleQuotationLineId <= 0)
                    BeforeSave = SaleQuotationDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.SaleQuotationHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = SaleQuotationDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.SaleQuotationHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");
            #endregion


            if (svm.Qty <= 0)
                ModelState.AddModelError("Qty", "The Qty is required");

            if (svm.DealQty <= 0)
            {
                ModelState.AddModelError("DealQty", "DealQty field is required");
            }

            SaleQuotationLine s = Mapper.Map<SaleQuotationLineViewModel, SaleQuotationLine>(svm);

            ViewBag.Status = temp.Status;




            if (svm.SaleQuotationLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                if (svm.SaleQuotationLineId <= 0)
                {
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.Sr = _SaleQuotationLineService.GetMaxSr(s.SaleQuotationHeaderId);
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    db.SaleQuotationLine.Add(s);



                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            item.LineTableId = s.SaleQuotationLineId;
                            item.PersonID = temp.SaleToBuyerId;
                            item.DealQty = s.DealQty;
                            item.HeaderTableId = temp.SaleQuotationHeaderId;
                            item.ObjectState = Model.ObjectState.Added;
                            db.SaleQuotationLineCharge.Add(item);
                        }

                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {

                            if (item.Id > 0)
                            {

                                var footercharge = new SaleQuotationHeaderChargeService(_unitOfWork).Find(item.Id);
                                footercharge.Rate = item.Rate;
                                footercharge.Amount = item.Amount;
                                footercharge.ObjectState = Model.ObjectState.Modified;
                                db.SaleQuotationHeaderCharge.Add(footercharge);

                            }

                            else
                            {
                                item.HeaderTableId = s.SaleQuotationHeaderId;
                                item.PersonID = temp.SaleToBuyerId;
                                item.ObjectState = Model.ObjectState.Added;
                                db.SaleQuotationHeaderCharge.Add(item);
                            }
                        }


                    //SaleQuotationHeader header = new SaleQuotationHeaderService(_unitOfWork).Find(s.SaleQuotationHeaderId);
                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;

                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.SaleQuotationHeader.Add(temp);


                    

                    try
                    {
                        SaleQuotationDocEvents.onLineSaveEvent(this, new JobEventArgs(s.SaleQuotationHeaderId, s.SaleQuotationLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        EventException = true;
                    }



                    try
                    {
                        if (EventException)
                        { throw new Exception(); }

                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(svm);

                        if (svm.SaleEnquiryLineId != null)
                        {
                            return PartialView("_CreateForSaleEnquiry", svm);
                        }
                        else
                        {
                            return PartialView("_Create", svm);
                        }

                    }


                    try
                    {
                        SaleQuotationDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.SaleQuotationHeaderId, s.SaleQuotationLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleQuotationHeaderId,
                        DocLineId = s.SaleQuotationLineId,
                        ActivityType = (int)ActivityTypeContants.Added,                       
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus=temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = svm.SaleQuotationHeaderId, IsRefBased = (s.SaleEnquiryLineId == null ? false : true) });

                }


                else
                {
                    SaleQuotationLine templine = (from p in db.SaleQuotationLine
                                             where p.SaleQuotationLineId == s.SaleQuotationLineId
                                             select p).FirstOrDefault();

                    SaleQuotationLine ExTempLine = new SaleQuotationLine();
                    ExTempLine = Mapper.Map<SaleQuotationLine>(templine);

                    

                    templine.ProductId = s.ProductId;
                    templine.SaleEnquiryLineId = s.SaleEnquiryLineId;
                    templine.DealUnitId = s.DealUnitId;
                    templine.DealQty = s.DealQty;
                    templine.DiscountPer = s.DiscountPer;
                    templine.DiscountAmount = s.DiscountAmount;
                    templine.Rate = s.Rate;
                    templine.Amount = s.Amount;
                    templine.Remark = s.Remark;
                    templine.Qty = s.Qty;
                    templine.Remark = s.Remark;
                    templine.Dimension1Id = s.Dimension1Id;
                    templine.Dimension2Id = s.Dimension2Id;
                    templine.Dimension3Id = s.Dimension3Id;
                    templine.Dimension4Id = s.Dimension4Id;
                    templine.UnitConversionMultiplier = s.UnitConversionMultiplier;
                    templine.Specification = s.Specification;
                    templine.SalesTaxGroupProductId = s.SalesTaxGroupProductId;
                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.SaleQuotationLine.Add(templine);

                    int Status = 0;
                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        Status = temp.Status;
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                    }


                    temp.ObjectState = Model.ObjectState.Modified;
                    db.SaleQuotationHeader.Add(temp);






                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExTempLine,
                        Obj = templine
                    });

                    if (svm.linecharges != null)
                    {
                        var ProductChargeList = (from p in db.SaleQuotationLineCharge
                                                 where p.LineTableId == templine.SaleQuotationLineId
                                                 select p).ToList();

                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = (ProductChargeList.Where(m => m.Id == item.Id)).FirstOrDefault();

                            var ExProdcharge = Mapper.Map<SaleQuotationLineCharge>(productcharge);
                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            productcharge.DealQty = templine.DealQty;
                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExProdcharge,
                                Obj = productcharge
                            });
                            productcharge.ObjectState = Model.ObjectState.Modified;
                            db.SaleQuotationLineCharge.Add(productcharge);
                        }
                    }

                    if (svm.footercharges != null)
                    {
                        var footerChargerecords = (from p in db.SaleQuotationHeaderCharge
                                                   where p.HeaderTableId == temp.SaleQuotationHeaderId
                                                   select p).ToList();

                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = footerChargerecords.Where(m => m.Id == item.Id).FirstOrDefault();
                            var Exfootercharge = Mapper.Map<SaleQuotationHeaderCharge>(footercharge);
                            footercharge.Rate = item.Rate;
                            footercharge.Amount = item.Amount;
                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = Exfootercharge,
                                Obj = footercharge,
                            });
                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.SaleQuotationHeaderCharge.Add(footercharge);
                        }
                    }


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        SaleQuotationDocEvents.onLineSaveEvent(this, new JobEventArgs(s.SaleQuotationHeaderId, templine.SaleQuotationLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        EventException = true;
                    }

                    try
                    {
                        if (EventException)
                        { throw new Exception(); }
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(svm);
                        if (svm.SaleEnquiryLineId != null)
                        {
                            return PartialView("_CreateForSaleEnquiry", svm);
                        }
                        else
                        {
                            return PartialView("_Create", svm);
                        }

                    }

                    try
                    {
                        SaleQuotationDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.SaleQuotationHeaderId, templine.SaleQuotationLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = templine.SaleQuotationHeaderId,
                        DocLineId = templine.SaleQuotationLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,                       
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus=temp.Status,
                    }));

                    //End of Saving the Activity Log

                    return Json(new { success = true });
                }

            }
            PrepareViewBag(svm);
            if (svm.SaleEnquiryLineId != null)
            {
                return PartialView("_CreateForSaleEnquiry", svm);
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
            SaleQuotationLineViewModel temp = _SaleQuotationLineService.GetSaleQuotationLine(id);

            SaleQuotationHeader H = new SaleQuotationHeaderService(_unitOfWork).Find(temp.SaleQuotationHeaderId);

            //Getting Settings
            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);


            temp.SaleQuotationSettings = Mapper.Map<SaleQuotationSettings, SaleQuotationSettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            //ViewBag.DocNo = H.DocNo;

            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(temp);

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

            //if (string.IsNullOrEmpty(temp.LockReason))
            //    ViewBag.LineMode = "Edit";
            //else
            //    TempData["CSEXCL"] += temp.LockReason;

            if (temp.SaleEnquiryLineId != null)
            {
                return PartialView("_CreateForSaleEnquiry", temp);

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
            SaleQuotationLineViewModel temp = _SaleQuotationLineService.GetSaleQuotationLine(id);

            SaleQuotationHeader H = new SaleQuotationHeaderService(_unitOfWork).Find(temp.SaleQuotationHeaderId);

            //Getting Settings
            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleQuotationSettings = Mapper.Map<SaleQuotationSettings, SaleQuotationSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(temp);
            //ViewBag.LineMode = "Delete";

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

            if (temp.SaleEnquiryLineId != null)
            {
                return PartialView("_CreateForSaleEnquiry", temp);
            }
            else
            {
                return PartialView("_Create", temp);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(SaleQuotationLineViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = SaleQuotationDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.SaleQuotationHeaderId, vm.SaleQuotationLineId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Validation failed before delete.";

            if (BeforeSave && !EventException)
            {

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                SaleQuotationLine SaleQuotationLine = (from p in db.SaleQuotationLine
                                             where p.SaleQuotationLineId == vm.SaleQuotationLineId
                                             select p).FirstOrDefault();
                SaleQuotationHeader header = (from p in db.SaleQuotationHeader
                                         where p.SaleQuotationHeaderId == SaleQuotationLine.SaleQuotationHeaderId
                                         select p).FirstOrDefault();




                LogList.Add(new LogTypeViewModel
                {
                    Obj = Mapper.Map<SaleQuotationLine>(SaleQuotationLine),
                });



                //_SaleQuotationLineService.Delete(SaleQuotationLine);
                SaleQuotationLine.ObjectState = Model.ObjectState.Deleted;
                db.SaleQuotationLine.Remove(SaleQuotationLine);




                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    db.SaleQuotationHeader.Add(header);
                }

                var chargeslist = (from p in db.SaleQuotationLineCharge
                                   where p.LineTableId == vm.SaleQuotationLineId
                                   select p).ToList();

                if (chargeslist != null)
                    foreach (var item in chargeslist)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.SaleQuotationLineCharge.Remove(item);
                    }

                if (vm.footercharges != null)
                    foreach (var item in vm.footercharges)
                    {
                        var footer = (from p in db.SaleQuotationHeaderCharge
                                      where p.Id == item.Id
                                      select p).FirstOrDefault();

                        footer.Rate = item.Rate;
                        footer.Amount = item.Amount;
                        footer.ObjectState = Model.ObjectState.Modified;
                        db.SaleQuotationHeaderCharge.Add(footer);
                    }







                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    SaleQuotationDocEvents.onLineDeleteEvent(this, new JobEventArgs(SaleQuotationLine.SaleQuotationHeaderId, SaleQuotationLine.SaleQuotationLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                try
                {
                    if (EventException)
                        throw new Exception();

                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    PrepareViewBag(vm);
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);

                }

                try
                {
                    SaleQuotationDocEvents.afterLineDeleteEvent(this, new JobEventArgs(SaleQuotationLine.SaleQuotationHeaderId, SaleQuotationLine.SaleQuotationLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                //Saving the Activity Log

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.SaleQuotationHeaderId,
                    DocLineId = SaleQuotationLine.SaleQuotationLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,                  
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus=header.Status,
                }));
            }

            return Json(new { success = true });

        }



        public JsonResult GetProductDetailJson(int ProductId, int SaleQuotationId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);
            //List<Product> ProductJson = new List<Product>();

            //ProductJson.Add(new Product()
            //{
            //    ProductId = product.ProductId,
            //    StandardCost = product.StandardCost,
            //    UnitId = product.UnitId
            //});            

            var DealUnitId = _SaleQuotationLineService.GetSaleQuotationLineListForIndex(SaleQuotationId).OrderByDescending(m => m.SaleQuotationLineId).FirstOrDefault();

            //Decimal Rate = _SaleQuotationLineService.GetJobRate(SaleQuotationId, ProductId);

            Decimal Rate = 0;
            Decimal Discount = 0;
            Decimal Incentive = 0;
            Decimal Loss = 0;





            var Record = new SaleQuotationHeaderService(_unitOfWork).Find(SaleQuotationId);

            var Settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(Record.DocTypeId, Record.DivisionId, Record.SiteId);

            var DlUnit = new UnitService(_unitOfWork).Find(Settings.DealUnitId == null ? product.UnitId : Settings.DealUnitId);

            return Json(new { ProductId = product.ProductId, 
                StandardCost = Rate, 
                Discount = Discount, 
                Incentive = Incentive, 
                Loss = Loss,
                UnitId = product.UnitId,
                DealUnitId = DlUnit.UnitId, 
                DealUnitDecimalPlaces = DlUnit.DecimalPlaces, 
                Specification = product.ProductSpecification, 
                SalesTaxGroupProductId = product.SalesTaxGroupProductId,
                SalesTaxGroupProductName = product.SalesTaxGroupProductName});
        }
        public JsonResult getunitconversiondetailjson(int prodid, string UnitId, string DealUnitId, int SaleQuotationId)
        {


            var Header = new SaleQuotationHeaderService(_unitOfWork).Find(SaleQuotationId);

            int DOctypeId = Header.DocTypeId;
            int siteId = Header.SiteId;
            int DivId = Header.DivisionId;


            UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(prodid, UnitId, (int)Header.UnitConversionForId, DealUnitId);


            byte DecimalPlaces = new UnitService(_unitOfWork).Find(DealUnitId).DecimalPlaces;
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

        //public JsonResult GetPendingSaleEnquiries(int ProductId)
        //{
        //    return Json(new SaleEnquiryHeaderService(_unitOfWork).GetPendingSaleEnquiries(ProductId).ToList());
        //}

        //public JsonResult GetSaleEnquiryDetail(int SaleEnquiryLineId, int SaleQuotationHeaderId)
        //{
        //    var temp = new SaleEnquiryLineService(_unitOfWork).GetSaleEnquiryDetailBalance(SaleEnquiryLineId);

        //    var DealUnitId = _SaleQuotationLineService.GetSaleQuotationLineListForIndex(SaleQuotationHeaderId).OrderByDescending(m => m.SaleQuotationLineId).FirstOrDefault();

        //    var Record = new SaleQuotationHeaderService(_unitOfWork).Find(SaleQuotationHeaderId);

        //    var Settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(Record.DocTypeId, Record.DivisionId, Record.SiteId);

        //    var DlUnit = new UnitService(_unitOfWork).Find((DealUnitId == null) ? (Settings.DealUnitId == null ? temp.UnitId : Settings.DealUnitId) : DealUnitId.DealUnitId);
        //    temp.DealunitDecimalPlaces = DlUnit.DecimalPlaces;
        //    temp.DealUnitId = DlUnit.UnitId;

        //    return Json(temp);
        //}


        public JsonResult GetSaleEnquiries(int id, string term, int Limit)//Order Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_SaleQuotationLineService.GetPendingSaleEnquiriesWithPatternMatch(id, term, Limit), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingSaleEnquiriesHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            var temp = _SaleQuotationLineService.GetPendingSaleEnquiryHelpList(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            //var count = _SaleQuotationLineService.GetPendingSaleEnquiryHelpList(filter, searchTerm).Count();
            var count = temp.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        //public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        //{
        //    return Json(_SaleQuotationLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        //}

        public ActionResult GetCustomProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleQuotationLineService.GetCustomProducts(filter, searchTerm);
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

        public ActionResult SetFlagForAllowRepeatProcess()
        {
            bool AllowRepeatProcess = true;

            return Json(AllowRepeatProcess);
        }

        public ActionResult IsProcessDone(string ProductUidName, int ProcessId)
        {
            ProductUid ProductUid = new ProductUidService(_unitOfWork).Find(ProductUidName);
            int ProductUidId = 0;
            if (ProductUid != null)
            {
                ProductUidId = ProductUid.ProductUIDId;
            }

            return Json(new ProductUidService(_unitOfWork).IsProcessDone(ProductUidId, ProcessId));
        }

        public JsonResult GetProductPrevProcess(int ProductId, int GodownId, int DocTypeId)
        {
            ProductPrevProcess ProductPrevProcess = new ProductService(_unitOfWork).FGetProductPrevProcess(ProductId, GodownId, DocTypeId);
            List<ProductPrevProcess> ProductPrevProcessJson = new List<ProductPrevProcess>();

            if (ProductPrevProcess != null)
            {
                ProductPrevProcessJson.Add(new ProductPrevProcess()
                {
                    ProcessId = ProductPrevProcess.ProcessId
                });
                return Json(ProductPrevProcessJson);
            }
            else
            {
                return null;
            }

        }

        public ActionResult GetCustomProductGroups(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleQuotationLineService.GetCustomProductGroups(filter, searchTerm);
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

        public ActionResult GetSaleEnquiryForProduct(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleQuotationLineService.GetSaleEnquiryHelpListForProduct(filter, searchTerm);
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

        public JsonResult GetSaleEnquiryDetail(int SaleEnquiryLineId)
        {
            return Json(new SaleQuotationLineService(_unitOfWork).GetSaleEnquiryDetailForQuotation(SaleEnquiryLineId));
        }

        public JsonResult SetSingleSaleEnquiryLine(int Ids)
        {
            ComboBoxResult SaleEnquiryJson = new ComboBoxResult();

            var SaleEnquiryLine = from L in db.SaleEnquiryLine
                                   join H in db.SaleEnquiryHeader on L.SaleEnquiryHeaderId equals H.SaleEnquiryHeaderId into SaleEnquiryHeaderTable
                                   from SaleEnquiryHeaderTab in SaleEnquiryHeaderTable.DefaultIfEmpty()
                                   where L.SaleEnquiryLineId == Ids
                                   select new
                                   {
                                       SaleEnquiryLineId = L.SaleEnquiryLineId,
                                       SaleEnquiryNo = L.Product.ProductName
                                   };

            SaleEnquiryJson.id = SaleEnquiryLine.FirstOrDefault().ToString();
            SaleEnquiryJson.text = SaleEnquiryLine.FirstOrDefault().SaleEnquiryNo;

            return Json(SaleEnquiryJson);
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

    }
}
