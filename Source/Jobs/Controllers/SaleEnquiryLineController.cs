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
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;
using Jobs.Helpers;
using Model.DatabaseViews;

namespace Jobs.Controllers
{

    [Authorize]
    public class SaleEnquiryLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleEnquiryLineService _SaleEnquiryLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public SaleEnquiryLineController(ISaleEnquiryLineService SaleEnquiry, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleEnquiryLineService = SaleEnquiry;
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
            var p = _SaleEnquiryLineService.GetSaleEnquiryLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        private void PrepareViewBag(SaleEnquiryHeader H)
        {
            ViewBag.Docno = H.DocNo;
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            //ViewBag.BuyerSpecificationList = _SaleEnquiryLineService.GetBuyerSpecification(H.SaleToBuyerId);
            //ViewBag.BuyerSpecification1List = _SaleEnquiryLineService.GetBuyerSpecification1(H.SaleToBuyerId);
            //ViewBag.BuyerSpecification2List = _SaleEnquiryLineService.GetBuyerSpecification2(H.SaleToBuyerId);
            //ViewBag.BuyerSpecification3List = _SaleEnquiryLineService.GetBuyerSpecification3(H.SaleToBuyerId);
        }

        [HttpGet]
        public ActionResult CreateLine(int id)
        {
            return _Create(id);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id)
        {
            return _Create(id);
        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            SaleEnquiryHeader H = new SaleEnquiryHeaderService(_unitOfWork).GetSaleEnquiryHeader(Id);
            SaleEnquiryLineViewModel s = new SaleEnquiryLineViewModel();
            s.SaleEnquiryHeaderId = H.SaleEnquiryHeaderId;
            s.UnitId = "PCS";
            ViewBag.DocNo = H.DocNo;
            ViewBag.Status = H.Status;
            ViewBag.LineMode = "Create";


            var settings = new SaleEnquirySettingsService(_unitOfWork).GetSaleEnquirySettingsForDucument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleEnquirySettings = Mapper.Map<SaleEnquirySettings, SaleEnquirySettingsViewModel>(settings);

            var count = _SaleEnquiryLineService.GetSaleEnquiryLineListForIndex(Id).Count();
            if (count > 0)
            {
                s.DealUnitId = _SaleEnquiryLineService.GetSaleEnquiryLineListForIndex(Id).OrderByDescending(m => m.SaleEnquiryLineId).FirstOrDefault().DealUnitId;
            }
            else
            {
                s.DealUnitId = settings.DealUnitId;
            }

            ProductBuyerSettings ProductBuyerSettings = new ProductBuyerSettingsService(_unitOfWork).GetProductBuyerSettings(H.DivisionId, H.SiteId);
            s.ProductBuyerSettings = Mapper.Map<ProductBuyerSettings, ProductBuyerSettingsViewModel>(ProductBuyerSettings);

            var LastTransactionDetail = _SaleEnquiryLineService.GetLastTransactionDetail(Id);
            if (LastTransactionDetail != null)
            {
                if (LastTransactionDetail.BuyerSpecification != null && LastTransactionDetail.BuyerSpecification != "")
                {
                    ViewBag.LastTransaction = LastTransactionDetail.BuyerSpecification;
                }

                if (LastTransactionDetail.BuyerSpecification1 != null && LastTransactionDetail.BuyerSpecification1 != "")
                {
                    ViewBag.LastTransaction = ViewBag.LastTransaction + ", " + LastTransactionDetail.BuyerSpecification1;
                }

                if (LastTransactionDetail.BuyerSpecification2 != null && LastTransactionDetail.BuyerSpecification2 != "")
                {
                    ViewBag.LastTransaction = ViewBag.LastTransaction + ", " + LastTransactionDetail.BuyerSpecification2;
                }

                if (LastTransactionDetail.BuyerSpecification3 != null && LastTransactionDetail.BuyerSpecification3 != "")
                {
                    ViewBag.LastTransaction = ViewBag.LastTransaction + ", " + LastTransactionDetail.BuyerSpecification3;
                }
            }


            PrepareViewBag(H);
            return PartialView("_Create", s);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _PersonProductUidPost(PersonProductUidSummaryViewModel svm)
        {
            SaleEnquiryLine s = new SaleEnquiryLineService(_unitOfWork).Find((int)svm.GenDocLineId);
            SaleEnquiryHeader temp = new SaleEnquiryHeaderService(_unitOfWork).Find(s.SaleEnquiryHeaderId);


            if (ModelState.IsValid)
            {
                //var PersonProductUid = (from p in db.PersonProductUid
                //                        where p.GenLineId == s.SaleEnquiryLineId && p.GenDocTypeId == temp.DocTypeId && p.GenDocId == temp.SaleEnquiryHeaderId
                //                        select p).ToList();

                //foreach (var item2 in PersonProductUid)
                //{
                //    new PersonProductUidService(_unitOfWork).Delete(item2.PersonProductUidId);

                //}

                if (svm.JobInvoiceSummaryViewModel[0].PersonProductUidId == 0)
                {
                    foreach (var item in svm.JobInvoiceSummaryViewModel)
                    {
                        PersonProductUid PPU = new PersonProductUid();
                        PPU.GenLineId = s.SaleEnquiryLineId;
                        PPU.ProductUidName = item.ProductUidName;
                        PPU.ProductUidSpecification = item.ProductUidSpecification;
                        PPU.GenDocId = temp.SaleEnquiryHeaderId;
                        PPU.GenDocNo = temp.BuyerEnquiryNo;
                        PPU.GenDocTypeId = temp.DocTypeId;
                        PPU.CreatedDate = DateTime.Now;
                        PPU.ModifiedDate = DateTime.Now;
                        PPU.CreatedBy = User.Identity.Name;
                        PPU.ModifiedBy = User.Identity.Name;
                        PPU.ObjectState = Model.ObjectState.Added;
                        new PersonProductUidService(_unitOfWork).Create(PPU);

                    }
                }
                else
                {
                    foreach (var item in svm.JobInvoiceSummaryViewModel)
                    {
                        PersonProductUid PPU = new PersonProductUidService(_unitOfWork).Find(item.PersonProductUidId);
                        PPU.ProductUidName = item.ProductUidName;
                        PPU.ProductUidSpecification = item.ProductUidSpecification;
                        PPU.ModifiedDate = DateTime.Now;
                        PPU.ModifiedBy = User.Identity.Name;
                        PPU.ObjectState = Model.ObjectState.Modified ;
                        new PersonProductUidService(_unitOfWork).Update(PPU);

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
                    PrepareViewBag(temp);
                    return PartialView("_Create", svm);
                }

                //LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                //{
                //    DocTypeId = header.DocTypeId,
                //    DocId = header.SaleEnquiryHeaderId,
                //    DocLineId = s.SaleEnquiryLineId,
                //    ActivityType = (int)ActivityTypeContants.Added,
                //    DocNo = header.DocNo,
                //    DocDate = header.DocDate,
                //    DocStatus = header.Status,
                //}));


                return Json(new { success = true });
            }
            PrepareViewBag(temp);
            return PartialView("_Create", svm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(SaleEnquiryLineViewModel svm)
        {
            SaleEnquiryLine s = Mapper.Map<SaleEnquiryLineViewModel, SaleEnquiryLine>(svm);
            SaleEnquiryHeader temp = new SaleEnquiryHeaderService(_unitOfWork).Find(s.SaleEnquiryHeaderId);
            //if (Command == "Submit" && (s.ProductId == 0))
            //    return RedirectToAction("Submit", "SaleEnquiryHeader", new { id = s.SaleEnquiryHeaderId }).Success("Data saved successfully");

            if (svm.BuyerSpecification != null || svm.BuyerSpecification1 != null || svm.BuyerSpecification2 != null || svm.BuyerSpecification3 != null)
            {
                SaleEnquiryLine es = new SaleEnquiryLineService(_unitOfWork).Find_WithLineDetail(svm.SaleEnquiryHeaderId, svm.BuyerSpecification, svm.BuyerSpecification1, svm.BuyerSpecification2, svm.BuyerSpecification3);

                if (es != null)
                {
                    if (es != null && es.SaleEnquiryLineId != svm.SaleEnquiryLineId)
                    {
                        ModelState.AddModelError("ProductId", "This Detail is already Added !");
                    }

                }
            }




            if (svm.Qty <= 0)
            {
                //ModelState.AddModelError("Qty", "Please Check Qty");
                ViewBag.LineMode = "Create";
                string message = "Please Check Qty";
                TempData["CSEXCL"] += message;
                PrepareViewBag(temp);
                return PartialView("_Create", svm);
            }

            if (svm.DueDate < temp.DocDate)
            {
                ModelState.AddModelError("DueDate", "DueDate greater than DocDate");
            }

            if (svm.SaleEnquiryLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid)
            {
                if (svm.SaleEnquiryLineId <= 0)
                {
                    if (svm.BuyerSpecification != null || svm.BuyerSpecification1 != null || svm.BuyerSpecification2 != null || svm.BuyerSpecification3 != null)
                    {
                        ProductBuyer BP = new ProductBuyerService(_unitOfWork).Find(temp.SaleToBuyerId, svm.BuyerSpecification, svm.BuyerSpecification1, svm.BuyerSpecification2, svm.BuyerSpecification3);

                        if (BP != null)
                        {
                            s.ProductId = BP.ProductId;
                        }
                    }

                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    _SaleEnquiryLineService.Create(s);

                    SaleEnquiryLineExtended Extended = new SaleEnquiryLineExtended();
                    Extended.SaleEnquiryLineId = s.SaleEnquiryLineId;
                    Extended.BuyerSpecification = svm.BuyerSpecification;
                    Extended.BuyerSpecification1 = svm.BuyerSpecification1;
                    Extended.BuyerSpecification2 = svm.BuyerSpecification2;
                    Extended.BuyerSpecification3 = svm.BuyerSpecification3;
                    Extended.BuyerSku = svm.BuyerSku;
                    Extended.BuyerUpcCode= svm.BuyerUpcCode;
                    new SaleEnquiryLineExtendedService(_unitOfWork).Create(Extended);

                    //new SaleEnquiryLineStatusService(_unitOfWork).CreateLineStatus(s.SaleEnquiryLineId);

                    SaleEnquiryHeader header = new SaleEnquiryHeaderService(_unitOfWork).Find(s.SaleEnquiryHeaderId);
                    if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        header.ModifiedDate = DateTime.Now;
                        header.ModifiedBy = User.Identity.Name;
                        new SaleEnquiryHeaderService(_unitOfWork).Update(header);
                    }


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(temp);
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = header.DocTypeId,
                        DocId = header.SaleEnquiryHeaderId,
                        DocLineId = s.SaleEnquiryLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = header.DocNo,
                        DocDate = header.DocDate,
                        DocStatus = header.Status,
                    }));


                    return RedirectToAction("_Create", new { id = s.SaleEnquiryHeaderId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    SaleEnquiryHeader header = new SaleEnquiryHeaderService(_unitOfWork).Find(svm.SaleEnquiryHeaderId);
                    StringBuilder logstring = new StringBuilder();
                    int status = header.Status;
                    SaleEnquiryLine temp1 = _SaleEnquiryLineService.Find(svm.SaleEnquiryLineId);

                    SaleEnquiryLine ExRec = new SaleEnquiryLine();
                    ExRec = Mapper.Map<SaleEnquiryLine>(temp1);

                    //End of Tracking the Modifications::

                    temp1.DueDate = svm.DueDate;
                    temp1.ProductId = svm.ProductId;
                    temp1.Specification = svm.Specification;
                    temp1.Dimension1Id = svm.Dimension1Id;
                    temp1.Dimension2Id = svm.Dimension2Id;
                    temp1.Dimension3Id = svm.Dimension3Id;
                    temp1.Dimension4Id = svm.Dimension4Id;
                    temp1.Qty = svm.Qty;
                    temp1.UnitId = svm.UnitId;
                    temp1.DealQty = svm.DealQty ?? 0;
                    temp1.DealUnitId = svm.DealUnitId;
                    temp1.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    temp1.Rate = svm.Rate ?? 0;
                    temp1.Amount = svm.Amount ?? 0;
                    temp1.Remark = svm.Remark;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    _SaleEnquiryLineService.Update(temp1);

                    SaleEnquiryLineExtended Extended = new SaleEnquiryLineExtendedService(_unitOfWork).Find(svm.SaleEnquiryLineId);
                    Extended.BuyerSpecification = svm.BuyerSpecification;
                    Extended.BuyerSpecification1 = svm.BuyerSpecification1;
                    Extended.BuyerSpecification2 = svm.BuyerSpecification2;
                    Extended.BuyerSpecification3 = svm.BuyerSpecification3;
                    new SaleEnquiryLineExtendedService(_unitOfWork).Update(Extended);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
                    });

                    if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                    {

                        header.Status = (int)StatusConstants.Modified;
                        header.ModifiedBy = User.Identity.Name;
                        header.ModifiedDate = DateTime.Now;
                        new SaleEnquiryHeaderService(_unitOfWork).Update(header);

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
                        PrepareViewBag(temp);
                        return PartialView("_Create", svm);
                    }

                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp1.SaleEnquiryHeaderId,
                        DocLineId = temp1.SaleEnquiryLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    //End of Saving the Activity Log

                    return Json(new { success = true });

                }
            }

            //string messages = string.Join("; ", ModelState.Values
            //                            .SelectMany(x => x.Errors)
            //                            .Select(x => x.ErrorMessage));
            //TempData["CSEXCL"] += messages;
            ViewBag.Status = temp.Status;
            PrepareViewBag(temp);
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
        private ActionResult _Modify(int id)
        {
            SaleEnquiryLine temp = _SaleEnquiryLineService.GetSaleEnquiryLine(id);
            SaleEnquiryLineExtended Extended = new SaleEnquiryLineExtendedService(_unitOfWork).Find(id);

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

            SaleEnquiryHeader H = new SaleEnquiryHeaderService(_unitOfWork).GetSaleEnquiryHeader(temp.SaleEnquiryHeaderId);
            ViewBag.DocNo = H.DocNo;
            SaleEnquiryLineViewModel s = Mapper.Map<SaleEnquiryLine, SaleEnquiryLineViewModel>(temp);

            s.BuyerSpecification = Extended.BuyerSpecification;
            s.BuyerSpecification1 = Extended.BuyerSpecification1;
            s.BuyerSpecification2 = Extended.BuyerSpecification2;
            s.BuyerSpecification3 = Extended.BuyerSpecification3;
            s.BuyerSku = Extended.BuyerSku;
            s.BuyerUpcCode= Extended.BuyerUpcCode;


            var settings = new SaleEnquirySettingsService(_unitOfWork).GetSaleEnquirySettingsForDucument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleEnquirySettings = Mapper.Map<SaleEnquirySettings, SaleEnquirySettingsViewModel>(settings);

            ProductBuyerSettings ProductBuyerSettings = new ProductBuyerSettingsService(_unitOfWork).GetProductBuyerSettings(H.DivisionId, H.SiteId);
            s.ProductBuyerSettings = Mapper.Map<ProductBuyerSettings, ProductBuyerSettingsViewModel>(ProductBuyerSettings);

            PrepareViewBag(H);

            return PartialView("_Create", s);
        }

        [HttpGet]
        public ActionResult _PersonProductUidLine(int id)
        {
            return _PersonProductUid(id);
        }

        [HttpGet]
        private ActionResult _PersonProductUid(int id)
        {

            SaleEnquiryLine SIL = new SaleEnquiryLineService(_unitOfWork).Find(id);
            SaleEnquiryHeader  SIH = new SaleEnquiryHeaderService(_unitOfWork).Find(SIL.SaleEnquiryHeaderId);
            SaleEnquiryLineExtended SILE = new SaleEnquiryLineExtendedService(_unitOfWork).Find(id);

            IQueryable<PersonProductUidViewModel> p = new PersonProductUidService(_unitOfWork).GetPersonProductUidList(SIH.DocTypeId,SIH.SaleEnquiryHeaderId,SIL.SaleEnquiryLineId);

            PersonProductUidSummaryViewModel vm = new PersonProductUidSummaryViewModel();
            vm.JobInvoiceSummaryViewModel = p.ToList();
            vm.GenDocLineId = SIL.SaleEnquiryLineId;

            ViewBag.DocNo = (SIH.BuyerEnquiryNo ?? SIH.DocNo).ToString() + "-"+SILE.BuyerSpecification + "-" + SILE.BuyerSpecification1 + "-" + SILE.BuyerSpecification2 + "-" + SILE.BuyerSpecification3;
            return PartialView("_PersonProductUid", vm);

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
        private ActionResult _Delete(int id)
        {
            SaleEnquiryLine temp = _SaleEnquiryLineService.GetSaleEnquiryLine(id);
            SaleEnquiryLineExtended Extended = new SaleEnquiryLineExtendedService(_unitOfWork).Find(id);

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

            SaleEnquiryHeader H = new SaleEnquiryHeaderService(_unitOfWork).GetSaleEnquiryHeader(temp.SaleEnquiryHeaderId);
            ViewBag.DocNo = H.DocNo;


            SaleEnquiryLineViewModel s = Mapper.Map<SaleEnquiryLine, SaleEnquiryLineViewModel>(temp);

            s.BuyerSpecification = Extended.BuyerSpecification;
            s.BuyerSpecification1 = Extended.BuyerSpecification1;
            s.BuyerSpecification2 = Extended.BuyerSpecification2;
            s.BuyerSpecification3 = Extended.BuyerSpecification3;

            var settings = new SaleEnquirySettingsService(_unitOfWork).GetSaleEnquirySettingsForDucument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleEnquirySettings = Mapper.Map<SaleEnquirySettings, SaleEnquirySettingsViewModel>(settings);

            ProductBuyerSettings ProductBuyerSettings = new ProductBuyerSettingsService(_unitOfWork).GetProductBuyerSettings(H.DivisionId, H.SiteId);
            s.ProductBuyerSettings = Mapper.Map<ProductBuyerSettings, ProductBuyerSettingsViewModel>(ProductBuyerSettings);

            PrepareViewBag(H);

            return PartialView("_Create", s);
        }

        [HttpGet]
        private ActionResult _Detail(int id)
        {
            SaleEnquiryLine temp = _SaleEnquiryLineService.GetSaleEnquiryLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            SaleEnquiryHeader H = new SaleEnquiryHeaderService(_unitOfWork).GetSaleEnquiryHeader(temp.SaleEnquiryHeaderId);
            ViewBag.DocNo = H.DocNo;
            SaleEnquiryLineViewModel s = Mapper.Map<SaleEnquiryLine, SaleEnquiryLineViewModel>(temp);
            PrepareViewBag(H);

            return PartialView("_Create", s);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(SaleEnquiryLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();            

            new SaleEnquiryLineExtendedService(_unitOfWork).Delete(vm.SaleEnquiryLineId);

            SaleEnquiryLine SaleEnquiryLine = _SaleEnquiryLineService.Find(vm.SaleEnquiryLineId);
            //new SaleEnquiryLineStatusService(_unitOfWork).Delete(vm.SaleEnquiryLineId);
            
            SaleEnquiryHeader header = new SaleEnquiryHeaderService(_unitOfWork).Find(SaleEnquiryLine.SaleEnquiryHeaderId);

            var PersonProductUid = (from p in db.PersonProductUid
                                    where p.GenLineId == SaleEnquiryLine.SaleEnquiryLineId && p.GenDocTypeId == header.DocTypeId && p.GenDocId == header.SaleEnquiryHeaderId
                                    select p).ToList();

            foreach (var item2 in PersonProductUid)
            {
                new PersonProductUidService(_unitOfWork).Delete(item2.PersonProductUidId);
            }

            _SaleEnquiryLineService.Delete(vm.SaleEnquiryLineId);

            if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
            {
                header.Status = (int)StatusConstants.Modified;
                header.ModifiedBy = User.Identity.Name;
                header.ModifiedDate = DateTime.Now;
                new SaleEnquiryHeaderService(_unitOfWork).Update(header);
            }

            LogList.Add(new LogTypeViewModel
            {
                Obj = SaleEnquiryLine,
            });

            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

            try
            {
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                ViewBag.Docno = header.DocNo;
                ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
                return PartialView("_Create", vm);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = header.DocTypeId,
                DocId = header.SaleEnquiryHeaderId,
                DocLineId = SaleEnquiryLine.SaleEnquiryLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = header.DocNo,
                xEModifications = Modifications,
                DocDate = header.DocDate,
                DocStatus = header.Status,
            }));

            return Json(new { success = true });
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

        public JsonResult GetUnitConversionDetailJson(int ProductId, string UnitId, string DeliveryUnitId, int HeaderId)
        {

            int UnitConversionForId = new SaleEnquiryHeaderService(_unitOfWork).Find(HeaderId).UnitConversionForId;

            UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversionForUCF(ProductId, UnitId, DeliveryUnitId, UnitConversionForId);
            List<SelectListItem> UnitConversionJson = new List<SelectListItem>();
            if (uc != null)
            {
                UnitConversionJson.Add(new SelectListItem
                {
                    Text = uc.Multiplier.ToString(),
                    Value = uc.Multiplier.ToString()
                });
            }
            else
            {
                UnitConversionJson.Add(new SelectListItem
                {
                    Text = "0",
                    Value = "0"
                });
            }

            return Json(UnitConversionJson);
        }

        public JsonResult GetProductDetailJson(int ProductId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);
            List<Product> ProductJson = new List<Product>();

            ProductJson.Add(new Product()
            {
                ProductId = product.ProductId,
                StandardCost = product.StandardCost,
                UnitId = product.UnitId,
                ProductSpecification = product.ProductSpecification
            });

            return Json(ProductJson);
        }

        public JsonResult GetProductCustomDetailJson(int ProductId, int SaleEnquiryHeaderId)
        {
            SaleEnquiryHeader Header = new SaleEnquiryHeaderService(_unitOfWork).Find(SaleEnquiryHeaderId);

            ProductCustomDetailViewModel ProductCustomDetail = (from P in db.ViewProductBuyer
                                                                where P.ProductId == ProductId && P.BuyerId == Header.SaleToBuyerId
                                                                select new ProductCustomDetailViewModel
                                                                {
                                                                    BuyerSpecification = P.BuyerSpecification,
                                                                    BuyerSpecification1 = P.BuyerSpecification1,
                                                                    BuyerSpecification2 = P.BuyerSpecification2,
                                                                    BuyerSpecification3 = P.BuyerSpecification3
                                                                }).FirstOrDefault();



            List<ProductCustomDetailViewModel> ProductCustomDetailJson = new List<ProductCustomDetailViewModel>();

            ProductCustomDetailJson.Add(new ProductCustomDetailViewModel()
            {
                BuyerSpecification = ProductCustomDetail.BuyerSpecification,
                BuyerSpecification1 = ProductCustomDetail.BuyerSpecification1,
                BuyerSpecification2 = ProductCustomDetail.BuyerSpecification2,
                BuyerSpecification3 = ProductCustomDetail.BuyerSpecification3
            });

            return Json(ProductCustomDetailJson);
        }

        public JsonResult CheckForValidationinEdit(int ProductId, int SaleEnquiryHeaderId, int SaleEnquiryLineId)
        {
            var temp = (_SaleEnquiryLineService.CheckForProductExists(ProductId, SaleEnquiryHeaderId, SaleEnquiryLineId));
            return Json(new { returnvalue = temp });
        }

        public JsonResult CheckForValidation(int ProductId, int SaleEnquiryHeaderId)
        {
            var temp = (_SaleEnquiryLineService.CheckForProductExists(ProductId, SaleEnquiryHeaderId));
            return Json(new { returnvalue = temp });
        }


        public JsonResult GetBuyerSKU(int ProductId, int SaleEnquiryHeaderId)
        {
            string temp = (_SaleEnquiryLineService.GetBuyerSKU(ProductId, SaleEnquiryHeaderId));
            return Json(temp, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCustomProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleEnquiryLineService.GetCustomProducts(filter, searchTerm);
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

        public JsonResult SetSingleBuyerProduct(int Ids)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<ViewProductBuyer> prod = from p in db.ViewProductBuyer
                                        where p.ProductId == Ids
                                        select p;

            ProductJson.id = prod.FirstOrDefault().ProductId.ToString();
            ProductJson.text = prod.FirstOrDefault().ProductName;

            return Json(ProductJson);
        }
        public JsonResult GetLastTransactionDetailJson(int SaleEnquiryHeaderId)
        {
            var LastTransactionDetail = _SaleEnquiryLineService.GetLastTransactionDetail(SaleEnquiryHeaderId);
            return Json(LastTransactionDetail);
        }

        public ActionResult GetBuyerSpecification(string searchTerm, int pageSize, int pageNum, int filter)
        {
            var Query = _SaleEnquiryLineService.GetBuyerSpecification(searchTerm, filter);
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

        public ActionResult GetBuyerSpecification1(string searchTerm, int pageSize, int pageNum, int filter)
        {
            var Query = _SaleEnquiryLineService.GetBuyerSpecification1(searchTerm, filter);
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

        public ActionResult GetBuyerSpecification2(string searchTerm, int pageSize, int pageNum, int filter)
        {
            var Query = _SaleEnquiryLineService.GetBuyerSpecification2(searchTerm, filter);
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

        public ActionResult GetBuyerSpecification3(string searchTerm, int pageSize, int pageNum, int filter)
        {
            var Query = _SaleEnquiryLineService.GetBuyerSpecification3(searchTerm, filter);
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


        public JsonResult SetSingleBuyerSpecification(string Ids)
        {
            ComboBoxResult ProductJson = (from b in db.SaleEnquiryLineExtended
                                          where b.BuyerSpecification == Ids
                                          select new ComboBoxResult
                                      {
                                          id = b.BuyerSpecification,
                                          text = b.BuyerSpecification,
                                      }).FirstOrDefault();
            return Json(ProductJson);
        }

        public JsonResult SetSingleBuyerSpecification1(string Ids)
        {
            ComboBoxResult ProductJson = (from b in db.SaleEnquiryLineExtended
                                          where b.BuyerSpecification1 == Ids
                                          select new ComboBoxResult
                                          {
                                              id = b.BuyerSpecification1,
                                              text = b.BuyerSpecification1,
                                          }).FirstOrDefault();
            return Json(ProductJson);
        }

        public JsonResult SetSingleBuyerSpecification2(string Ids)
        {
            ComboBoxResult ProductJson = (from b in db.SaleEnquiryLineExtended
                                          where b.BuyerSpecification2 == Ids
                                          select new ComboBoxResult
                                          {
                                              id = b.BuyerSpecification2,
                                              text = b.BuyerSpecification2,
                                          }).FirstOrDefault();
            return Json(ProductJson);
        }

        public JsonResult SetSingleBuyerSpecification3(string Ids)
        {
            ComboBoxResult ProductJson = (from b in db.SaleEnquiryLineExtended
                                          where b.BuyerSpecification3 == Ids
                                          select new ComboBoxResult
                                          {
                                              id = b.BuyerSpecification3,
                                              text = b.BuyerSpecification3,
                                          }).FirstOrDefault();
            return Json(ProductJson);
        }
    }
}
