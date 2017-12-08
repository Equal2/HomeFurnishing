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
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;

namespace Jobs.Areas.Rug.Controllers
{

    [Authorize]
    public class SaleInvoiceLineController : System.Web.Mvc.Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleInvoiceLineService _SaleInvoiceLineService;
        ISaleDispatchLineService _SaleDispatchLineService;
        IStockService _StockService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public SaleInvoiceLineController(ISaleInvoiceLineService SaleInvoice, IStockService StockService, ISaleDispatchLineService SaleDispatch, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleInvoiceLineService = SaleInvoice;
            _SaleDispatchLineService = SaleDispatch;
            _StockService = StockService;
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
        public JsonResult Index(int Id, int Iter)
        {
            var SaleInvoiceLineViewModel = _SaleInvoiceLineService.GetSaleInvoiceLineListForIndex(Id, Iter);

            var Count = SaleInvoiceLineViewModel.Count();

            double x = 0;
            var p = SaleInvoiceLineViewModel.AsEnumerable().OrderBy(sx => double.TryParse(sx.BaleNo.Replace("-", "."), out x) ? x : 0).Skip(Iter * 1000).Take(1000).ToList();

            return Json(new { data = p, Pending = ((Iter * 1000) + 1000) < Count }, JsonRequestBehavior.AllowGet);

        }

        private void PrepareViewBag(SaleInvoiceLineViewModel s)
        {
            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = "Sq.Feet", Value = UnitConstants.SqFeet });
            temp.Add(new SelectListItem { Text = "Pieces", Value = UnitConstants.Pieces });
            temp.Add(new SelectListItem { Text = "Sq.Meter", Value = UnitConstants.SqMeter });


            ViewBag.DealUnitId = new SelectList(temp, "Value", "Text");

            //if (s == null)
            //    ViewBag.Priority = new SelectList(temp, "Value", "Text");
            //else
            //    ViewBag.Priority = new SelectList(temp, "Value", "Text", s.Priority);



            //ViewBag.DealUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        public ActionResult GetPackingNoPendingForInvoice(string searchTerm, int pageSize, int pageNum, int saledispatchheaderid)
        {
            SaleDispatchHeader saledispatchheader = new SaleDispatchHeaderService(_unitOfWork).Find(saledispatchheaderid);
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(_SaleInvoiceLineService.GetPackginNoPendingForInvoice(saledispatchheader.SaleToBuyerId, saledispatchheader.DocDate));


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonpResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
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


        public ActionResult _Create(int Id) //Id ==>Sale Invoice Header Id
        {
            SaleInvoiceHeader H = new SaleInvoiceHeaderService(_unitOfWork).GetSaleInvoiceHeaderDetail(Id);
            SaleInvoiceLineViewModel s = new SaleInvoiceLineViewModel();
            s.SaleInvoiceHeaderId = H.SaleInvoiceHeaderId;
            s.SalesTaxGroupProductId = 1;
            ViewBag.DocNo = H.DocNo;
            ViewBag.Status = H.Status;
            PrepareViewBag(null);
            ViewBag.LineMode = "Create";
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(SaleInvoiceLineViewModel svm)
        {
            List<LedgerViewModel> LedArr = new List<LedgerViewModel>();
            List<StockViewModel> StockArr = new List<StockViewModel>();

            SaleInvoiceLine s = Mapper.Map<SaleInvoiceLineViewModel, SaleInvoiceLine>(svm);
            SaleInvoiceHeader temp = new SaleInvoiceHeaderService(_unitOfWork).Find(s.SaleInvoiceHeaderId);

            if (svm.Rate <= 0)
            {
                ModelState.AddModelError("Rate", "Please Check Rate");
            }
            else if (svm.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Please Check Amount");
            }

            if (svm.SaleInvoiceLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }


            if (ModelState.IsValid)
            {
                if (svm.SaleInvoiceLineId == 0)
                {
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    _SaleInvoiceLineService.Create(s);

                    SaleInvoiceHeader header = new SaleInvoiceHeaderService(_unitOfWork).Find(s.SaleInvoiceHeaderId);
                    if (header.Status != (int)StatusConstants.Drafted)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        new SaleInvoiceHeaderService(_unitOfWork).Update(header);
                    }

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleInvoiceHeaderId,
                        DocLineId = s.SaleInvoiceLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.SaleInvoiceHeaderId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleInvoiceHeaderDetail header = new SaleInvoiceHeaderService(_unitOfWork).Find(svm.SaleInvoiceHeaderId);
                    StringBuilder logstring = new StringBuilder();
                    int status = header.Status;
                    SaleInvoiceLine line = _SaleInvoiceLineService.Find(svm.SaleInvoiceLineId);

                    SaleInvoiceLine ExRec = new SaleInvoiceLine();
                    ExRec = Mapper.Map<SaleInvoiceLine>(line);

                    line.Rate = svm.Rate;
                    line.Amount = svm.Amount;
                    line.Remark = svm.Remark;
                    line.ProductInvoiceGroupId = svm.ProductInvoiceGroupId;
                    //temp1.SalesTaxGroupId = (int) svm.SalesTaxGroupId;
                    line.ModifiedDate = DateTime.Now;
                    line.ModifiedBy = User.Identity.Name;
                    _SaleInvoiceLineService.Update(line);

                    header.Status = (int)StatusConstants.Modified;
                    new SaleInvoiceHeaderService(_unitOfWork).Update(header);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = line,
                    });




                    StockViewModel StockViewModel_Old = GetStockViewModelForInvoiceLine(line.SaleInvoiceLineId);



                    StockViewModel StockViewModel_New = new StockViewModel();
                    StockViewModel_New.DocHeaderId = StockViewModel_Old.DocHeaderId;
                    StockViewModel_New.DocLineId = StockViewModel_Old.DocLineId;
                    StockViewModel_New.DocTypeId = StockViewModel_Old.DocTypeId;
                    StockViewModel_New.StockHeaderDocDate = StockViewModel_Old.StockHeaderDocDate;
                    StockViewModel_New.DocNo = StockViewModel_Old.DocNo;
                    StockViewModel_New.DivisionId = StockViewModel_Old.DivisionId;
                    StockViewModel_New.SiteId = StockViewModel_Old.SiteId;
                    StockViewModel_New.CurrencyId = StockViewModel_Old.CurrencyId;
                    StockViewModel_New.HeaderProcessId = null;
                    StockViewModel_New.PersonId = StockViewModel_Old.PersonId;
                    StockViewModel_New.ProductId = StockViewModel_Old.ProductId;
                    StockViewModel_New.HeaderFromGodownId = null;
                    StockViewModel_New.HeaderGodownId = null;
                    StockViewModel_New.GodownId = StockViewModel_Old.GodownId;
                    StockViewModel_New.ProcessId = StockViewModel_Old.ProcessId;
                    StockViewModel_New.LotNo = StockViewModel_Old.LotNo;
                    StockViewModel_New.CostCenterId = null;
                    StockViewModel_New.Qty_Iss = StockViewModel_Old.Qty_Iss;
                    StockViewModel_New.Qty_Rec = 0;
                    StockViewModel_New.Rate = (Decimal?)svm.Rate;
                    StockViewModel_New.ExpiryDate = null;
                    StockViewModel_New.Specification = null;
                    StockViewModel_New.Dimension1Id = null;
                    StockViewModel_New.Dimension2Id = null;
                    StockViewModel_New.Remark = StockViewModel_Old.Remark;
                    StockViewModel_New.Status = StockViewModel_Old.Status;
                    StockViewModel_New.CreatedBy = StockViewModel_Old.CreatedBy;
                    StockViewModel_New.CreatedDate = StockViewModel_Old.CreatedDate;
                    StockViewModel_New.ModifiedBy = StockViewModel_Old.ModifiedBy;
                    StockViewModel_New.ModifiedDate = StockViewModel_Old.ModifiedDate;






                    LedgerViewModel LedgerViewModel_Old = GetLedgerViewModelForInvoiceLine(line.SaleInvoiceLineId);



                    LedgerViewModel LedgerViewModel_New = new LedgerViewModel();
                    LedgerViewModel_New.DocHeaderId = LedgerViewModel_Old.DocHeaderId;
                    LedgerViewModel_New.DocTypeId = LedgerViewModel_Old.DocTypeId;
                    LedgerViewModel_New.DocDate = LedgerViewModel_Old.DocDate;
                    LedgerViewModel_New.DocNo = LedgerViewModel_Old.DocNo;
                    LedgerViewModel_New.DivisionId = LedgerViewModel_Old.DivisionId;
                    LedgerViewModel_New.SiteId = LedgerViewModel_Old.SiteId;
                    LedgerViewModel_New.CostCenterId = null;
                    LedgerViewModel_New.AmtDr = LedgerViewModel_Old.AmtDr;
                    LedgerViewModel_New.AmtCr = 0;
                    LedgerViewModel_New.Remark = LedgerViewModel_Old.Remark;
                    LedgerViewModel_New.Status = LedgerViewModel_Old.Status;
                    LedgerViewModel_New.CreatedBy = LedgerViewModel_Old.CreatedBy;
                    LedgerViewModel_New.CreatedDate = LedgerViewModel_Old.CreatedDate;
                    LedgerViewModel_New.ModifiedBy = LedgerViewModel_Old.ModifiedBy;
                    LedgerViewModel_New.ModifiedDate = LedgerViewModel_Old.ModifiedDate;



                    //string StockPostingError = "";

                    //StockPostingError = new StockService(_unitOfWork).StockPost(StockViewModel_New, StockViewModel_Old);
                    //StockPostingError = new StockService(_unitOfWork).StockPost(StockViewModel_New, StockViewModel_Old);

                    //if (StockPostingError != "")
                    //{
                    //    ModelState.AddModelError("", StockPostingError);
                    //    return PartialView("_Create", svm);
                    //}                 
                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        return PartialView("_Create", svm);
                    }

                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = line.SaleInvoiceHeaderId,
                        DocLineId = line.SaleInvoiceLineId,
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

            ViewBag.Status = temp.Status;
            PrepareViewBag(svm);
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
        public ActionResult _Modify(int id)
        {
            SaleInvoiceLine temp = _SaleInvoiceLineService.GetSaleInvoiceLine(id);

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

            SaleInvoiceHeader H = new SaleInvoiceHeaderService(_unitOfWork).GetSaleInvoiceHeaderDetail(temp.SaleInvoiceHeaderId);
            ViewBag.DocNo = H.DocNo;
            SaleInvoiceLineViewModel s = _SaleInvoiceLineService.GetSaleInvoiceLineForLineId(id);
            PrepareViewBag(s);


            return PartialView("_Create", s);
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
            SaleInvoiceLine temp = _SaleInvoiceLineService.GetSaleInvoiceLine(id);

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

            SaleInvoiceHeader H = new SaleInvoiceHeaderService(_unitOfWork).GetSaleInvoiceHeaderDetail(temp.SaleInvoiceHeaderId);
            ViewBag.DocNo = H.DocNo;
            SaleInvoiceLineViewModel s = _SaleInvoiceLineService.GetSaleInvoiceLineForLineId(id);
            PrepareViewBag(s);


            return PartialView("_Create", s);
        }


        [HttpGet]
        private ActionResult _Detail(int id)
        {
            SaleInvoiceLine temp = _SaleInvoiceLineService.GetSaleInvoiceLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            SaleInvoiceHeader H = new SaleInvoiceHeaderService(_unitOfWork).GetSaleInvoiceHeaderDetail(temp.SaleInvoiceHeaderId);
            ViewBag.DocNo = H.DocNo;
            SaleInvoiceLineViewModel s = _SaleInvoiceLineService.GetSaleInvoiceLineForLineId(id);
            PrepareViewBag(s);


            return PartialView("_Create", s);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(SaleInvoiceLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            SaleInvoiceLine SaleInvoiceLine = _SaleInvoiceLineService.GetSaleInvoiceLine(vm.SaleInvoiceLineId);
            SaleInvoiceHeader saleinvoiceheader = new SaleInvoiceHeaderService(_unitOfWork).Find(SaleInvoiceLine.SaleInvoiceHeaderId);
            int status = saleinvoiceheader.Status;

            LogList.Add(new LogTypeViewModel
            {
                ExObj = SaleInvoiceLine,
            });

            StockViewModel StockViewModel_Old = GetStockViewModelForInvoiceLine(SaleInvoiceLine.SaleInvoiceLineId);
            string StockPostingError = "";

            StockPostingError = new StockService(_unitOfWork).StockPost(null, StockViewModel_Old);

            if (StockPostingError != "")
            {
                ModelState.AddModelError("", StockPostingError);
                return PartialView("_Create", vm);
            }

            if (SaleInvoiceLine.SaleOrderLineId.HasValue)
                //
                new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyOnInvoice(SaleInvoiceLine.SaleOrderLineId.Value, SaleInvoiceLine.SaleInvoiceLineId, saleinvoiceheader.DocDate, 0);

            _SaleInvoiceLineService.Delete(vm.SaleInvoiceLineId);

            if (saleinvoiceheader.Status != (int)StatusConstants.Drafted)
            {
                saleinvoiceheader.Status = (int)StatusConstants.Modified;
                saleinvoiceheader.ModifiedBy = User.Identity.Name;
                saleinvoiceheader.ModifiedDate = DateTime.Now;
                new SaleInvoiceHeaderService(_unitOfWork).Update(saleinvoiceheader);
            }


            SaleDispatchLine SaleDispatchLine = _SaleDispatchLineService.GetSaleDispatchLine(vm.SaleDispatchLineId);
            PackingLine packingline = new PackingLineService(_unitOfWork).Find(SaleDispatchLine.PackingLineId);

            if (packingline.ProductUidId != null && packingline.ProductUidId != 0)
            {
                ProductUid ProductUid = new ProductUidService(_unitOfWork).Find((int)packingline.ProductUidId);

                ProductUid.LastTransactionDocDate = SaleDispatchLine.ProductUidLastTransactionDocDate;
                ProductUid.LastTransactionDocId = SaleDispatchLine.ProductUidLastTransactionDocId;
                ProductUid.LastTransactionDocNo = SaleDispatchLine.ProductUidLastTransactionDocNo;
                ProductUid.LastTransactionDocTypeId = SaleDispatchLine.ProductUidLastTransactionDocTypeId;
                ProductUid.LastTransactionPersonId = SaleDispatchLine.ProductUidLastTransactionPersonId;
                ProductUid.CurrenctGodownId = SaleDispatchLine.ProductUidCurrentGodownId;
                ProductUid.CurrenctProcessId = SaleDispatchLine.ProductUidCurrentProcessId;
                ProductUid.Status = SaleDispatchLine.ProductUidStatus;
                ProductUid.IsActive = true;

                new ProductUidService(_unitOfWork).Update(ProductUid);
            }




            LogList.Add(new LogTypeViewModel
            {
                ExObj = SaleDispatchLine,
            });

            var DispatchLine = db.SaleDispatchLine.Find(vm.SaleDispatchLineId);

            _SaleDispatchLineService.Delete(vm.SaleDispatchLineId);

            if (DispatchLine != null && DispatchLine.StockId.HasValue && DispatchLine.StockId.Value > 0)
            {
                new StockService(_unitOfWork).DeleteStock((int)DispatchLine.StockId);
            }

            SaleDispatchHeader saledispatchheader = new SaleDispatchHeaderService(_unitOfWork).Find(SaleDispatchLine.SaleDispatchHeaderId);
            if (saledispatchheader.Status != (int)StatusConstants.Drafted)
            {
                saledispatchheader.Status = (int)StatusConstants.Modified;
                new SaleDispatchHeaderService(_unitOfWork).Update(saledispatchheader);
            }

            var TempPacking = (from L in db.SaleDispatchLine
                               join Pl in db.PackingLine on L.PackingLineId equals Pl.PackingLineId into PackingLineTable
                               from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                               where L.SaleDispatchLineId == vm.SaleDispatchLineId
                               select new
                               {
                                   PackingHeaderId = PackingLineTab.PackingHeaderId
                               }).FirstOrDefault();

            if (TempPacking != null)
            {
                PackingHeader PackingHeader = new PackingHeaderService(_unitOfWork).Find(TempPacking.PackingHeaderId);
                PackingHeader.Status = (int)ActivityTypeContants.Approved;
                new PackingHeaderService(_unitOfWork).Update(PackingHeader);
            }

            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                return PartialView("EditSize", vm);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = saleinvoiceheader.DocTypeId,
                DocId = saleinvoiceheader.SaleInvoiceHeaderId,
                DocLineId = SaleInvoiceLine.SaleInvoiceLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = saleinvoiceheader.DocNo,
                xEModifications = Modifications,
                DocDate = saleinvoiceheader.DocDate,
                DocStatus = saleinvoiceheader.Status,
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





        public JsonResult CheckForValidationinEdit(int ProductId, int SaleInvoiceHeaderId, int SaleInvoiceLineId)
        {
            var temp = (_SaleInvoiceLineService.CheckForProductExists(ProductId, SaleInvoiceHeaderId, SaleInvoiceLineId));
            return Json(new { returnvalue = temp });
        }

        public JsonResult CheckForValidation(int ProductId, int SaleInvoiceHeaderId)
        {
            var temp = (_SaleInvoiceLineService.CheckForProductExists(ProductId, SaleInvoiceHeaderId));
            return Json(new { returnvalue = temp });
        }

        [HttpGet]
        public ActionResult FillProducts(int id)
        {
            SaleInvoiceHeaderDetail saleinvoiceheaderdetail = new SaleInvoiceHeaderService(_unitOfWork).Find(id);
            SaleInvoiceFillProducts s = new SaleInvoiceFillProducts();
            s.SaleInvoiceHeaderId = saleinvoiceheaderdetail.SaleInvoiceHeaderId;
            s.SaleDispatchHeaderId = (int)saleinvoiceheaderdetail.SaleDispatchHeaderId;
            PrepareViewBag(null);

            return PartialView("FillProducts", s);
        }



        [HttpPost]
        public ActionResult FillProducts(SaleInvoiceFillProducts svm)
        {
            Decimal TotalAmount = 0;

            string BaleNoStr = "";

            string mroll = "";
            int froll = 0;
            int subRollCount = 0;
            string prevrollno = "";
            int Cnt = 0;
            int SaleAccountId = 0;
            List<LedgerViewModel> LedArr = new List<LedgerViewModel>();
            List<StockViewModel> StockArr = new List<StockViewModel>();
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            string[] PackingHeaderIdArr;

            db.Configuration.AutoDetectChangesEnabled = false;

            if (svm.PackingHeaderIds != "")
            {
                SaleInvoiceHeaderDetail saleinvoiceheaderdetail = db.SaleInvoiceHeaderDetail.Find(svm.SaleInvoiceHeaderId);
                SaleDispatchHeader saledispatchheader = db.SaleDispatchHeader.Find(saleinvoiceheaderdetail.SaleDispatchHeaderId);
                int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Packing).ProcessId;

                SaleAccountId = new AccountService(_unitOfWork).Find(new ProcessService(_unitOfWork).Find(ProcessConstants.Sales).AccountId).LedgerAccountId;

                PackingHeaderIdArr = svm.PackingHeaderIds.Split(new Char[] { ',' });

                int SaleDispatchLineId = 0;

                for (int i = 0; i <= PackingHeaderIdArr.Length - 1; i++)
                {
                    IEnumerable<SaleInvoiceLineViewModel> packingline = new SaleInvoiceLineService(_unitOfWork).GetPackingLineForProductDetail(Convert.ToInt32(PackingHeaderIdArr[i])).ToList();

                    if (packingline == null)
                    {
                        string message = "No records to fill.";
                        ModelState.AddModelError("", message);
                        PrepareViewBag(null);
                        return PartialView("FillProducts", svm);
                    }


                    #region Negatvie Sale Order Packing Validation

                    Decimal PendingOrderQtyForPacking = 0;
                    IEnumerable<PackingLineViewModel> PackingLine = new PackingLineService(_unitOfWork).GetPackingLineViewModelForHeaderId(Convert.ToInt32(PackingHeaderIdArr[i]));
                    string ValidationMsg = "";

                    foreach (PackingLineViewModel Line in PackingLine)
                    {
                        PendingOrderQtyForPacking = new PackingLineService(_unitOfWork).FGetPendingOrderQtyForPacking((int)Line.SaleOrderLineId, 0);
                        if (PendingOrderQtyForPacking < 0)
                        {
                            ValidationMsg = "Balance Qty For Product : " + Line.ProductName + " And Sale Order : " + Line.SaleOrderNo + " is going negative. Can't Submit Record.";
                            ModelState.AddModelError("", ValidationMsg);
                            PrepareViewBag(null);
                            return PartialView("FillProducts", svm);
                        }

                        if (new PackingLineService(_unitOfWork).FSaleOrderProductMatchWithPacking((int)Line.SaleOrderLineId, Line.ProductId) == false)
                        {
                            ValidationMsg = "Product : " + Line.ProductName + " does not exist in Sale Order : " + Line.SaleOrderNo + " . Can't Submit Record.";
                            ModelState.AddModelError("", ValidationMsg);
                            PrepareViewBag(null);
                            return PartialView("FillProducts", svm);
                        }
                    }

                    #endregion



                    PackingHeader PackingHeader = new PackingHeaderService(_unitOfWork).Find(Convert.ToInt32(PackingHeaderIdArr[i]));
                    PackingHeader.Status = (int)ActivityTypeContants.Closed;
                    PackingHeader.ObjectState = Model.ObjectState.Modified;
                    db.PackingHeader.Add(PackingHeader);


                    foreach (SaleInvoiceLineViewModel item in packingline)
                    {
                        SaleDispatchLineId = SaleDispatchLineId - 1;


                        //Code To Manage Invoice of Pcs And Cms

                        if (item.DealUnitId.ToUpper() != svm.DealUnitId.ToUpper())// Condition when user wants to create invoice in different unit from packing unit.For Example Packing is done in Sq.Feet and user want to create invoice in pcs.
                        {
                            if (svm.DealUnitId.ToUpper() == item.UnitId.ToUpper())//Condition when User want to create invoice in product unit not in deal unit.For Example Packing is done in Sq.Feet and user want to create invoice in pcs.
                            {
                                item.DealQty = item.Qty;
                                item.UnitConversionMultiplier = 1;
                            }
                            else
                            {
                                var UnitConversion = (from U in db.UnitConversion
                                                      where U.ProductId == item.ProductId && U.FromUnitId == item.UnitId && U.ToUnitId == svm.DealUnitId && U.UnitConversionForId == (int)UnitConversionFors.Standard
                                                      select new
                                                      {
                                                          UnitConversionMultiplier = U.ToQty
                                                      }).FirstOrDefault();

                                if (UnitConversion != null)
                                {
                                    item.DealQty = item.Qty * UnitConversion.UnitConversionMultiplier;
                                    item.UnitConversionMultiplier = UnitConversion.UnitConversionMultiplier;
                                }
                            }


                            if (svm.DealUnitId == item.SaleOrderDealUnitId)
                            {
                                item.Rate = item.SaleOrderRate;
                            }
                            else
                            {
                                item.Rate = 0;
                            }

                            item.DealUnitId = svm.DealUnitId;
                            item.Amount = (Decimal?)(item.DealQty * item.Rate) ?? 0;
                        }

                        //Code To Manage Invoice of Pcs And Cms


                        //SaleInvoiceHeader saleinvoiceheader = new SaleInvoiceHeaderService(_unitOfWork).Find(svm.SaleInvoiceHeaderId);
                        //int SalesTaxGroupId = new SalesTaxGroupService(_unitOfWork).GetSalesTaxGroupId(saleinvoiceheader.BillToBuyerId, item.ProductId);
                        //if (SalesTaxGroupId != 0) { saleinvoiceline.SalesTaxGroupId = SalesTaxGroupId; } else { saleinvoiceline.SalesTaxGroupId = null; }


                        //Stock.DocHeaderId = saleinvoiceheaderdetail.SaleInvoiceHeaderId;
                        //Stock.DocTypeId = saleinvoiceheaderdetail.DocTypeId;
                        //Stock.StockHeaderDocDate = saleinvoiceheaderdetail.DocDate;
                        //Stock.StockDocDate = saleinvoiceheaderdetail.DocDate;
                        //Stock.DocNo = saleinvoiceheaderdetail.DocNo;
                        //Stock.DivisionId = saleinvoiceheaderdetail.DivisionId;
                        //Stock.SiteId = saleinvoiceheaderdetail.SiteId;
                        //Stock.CurrencyId = saleinvoiceheaderdetail.CurrencyId;
                        //Stock.HeaderProcessId = null;
                        //Stock.PersonId = saleinvoiceheaderdetail.BillToBuyerId;
                        //Stock.ProductId = item.ProductId;
                        //Stock.HeaderFromGodownId = null;
                        //Stock.HeaderGodownId = null;
                        //Stock.GodownId = item.GodownId;
                        //Stock.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Packing).ProcessId;
                        //Stock.LotNo = item.LotNo;
                        //Stock.CostCenterId = null;
                        //Stock.Qty_Iss = item.Qty;
                        //Stock.Qty_Rec = 0;
                        //Stock.Rate = (Decimal?)item.Rate;
                        //Stock.ExpiryDate = null;
                        //Stock.Specification = null;
                        //Stock.Dimension1Id = null;
                        //Stock.Dimension2Id = null;
                        //Stock.Remark = saleinvoiceheaderdetail.Remark;
                        //Stock.Status = saleinvoiceheaderdetail.Status;
                        //Stock.CreatedBy = saleinvoiceheaderdetail.CreatedBy;
                        //Stock.CreatedDate = saleinvoiceheaderdetail.CreatedDate;
                        //Stock.ModifiedBy = saleinvoiceheaderdetail.ModifiedBy;
                        //Stock.ModifiedDate = saleinvoiceheaderdetail.ModifiedDate;
                        //StockArr.Add(Stock);




                        StockViewModel StockViewModel = new StockViewModel();
                        if (Cnt == 0)
                        {
                            StockViewModel.StockHeaderId = saledispatchheader.StockHeaderId ?? 0;
                        }
                        else
                        {
                            if (saledispatchheader.StockHeaderId != null && saledispatchheader.StockHeaderId != 0)
                            {
                                StockViewModel.StockHeaderId = (int)saledispatchheader.StockHeaderId;
                            }
                            else
                            {
                                StockViewModel.StockHeaderId = -1;
                            }
                        }

                        StockViewModel.StockId = -Cnt;
                        StockViewModel.DocHeaderId = saledispatchheader.SaleDispatchHeaderId;
                        StockViewModel.DocTypeId = saledispatchheader.DocTypeId;
                        StockViewModel.StockHeaderDocDate = saledispatchheader.DocDate;
                        StockViewModel.StockDocDate = saledispatchheader.DocDate;
                        StockViewModel.DocNo = saledispatchheader.DocNo;
                        StockViewModel.DivisionId = saledispatchheader.DivisionId;
                        StockViewModel.SiteId = saledispatchheader.SiteId;
                        StockViewModel.CurrencyId = saleinvoiceheaderdetail.CurrencyId;
                        StockViewModel.HeaderProcessId = null;
                        StockViewModel.PersonId = saleinvoiceheaderdetail.BillToBuyerId;
                        StockViewModel.ProductId = item.ProductId;
                        StockViewModel.ProductUidId = item.ProductUidId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = null;
                        StockViewModel.GodownId = item.GodownId;
                        StockViewModel.ProcessId = ProcessId;
                        StockViewModel.LotNo = item.LotNo;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = item.Qty;
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = (Decimal?)item.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = null;
                        StockViewModel.Dimension1Id = null;
                        StockViewModel.Dimension2Id = null;
                        StockViewModel.Remark = saledispatchheader.Remark;
                        StockViewModel.Status = saledispatchheader.Status;
                        StockViewModel.CreatedBy = saledispatchheader.CreatedBy;
                        StockViewModel.CreatedDate = saledispatchheader.CreatedDate;
                        StockViewModel.ModifiedBy = saledispatchheader.ModifiedBy;
                        StockViewModel.ModifiedDate = saledispatchheader.ModifiedDate;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                        if (StockPostingError != "")
                        {
                            string message = StockPostingError;
                            ModelState.AddModelError("", message);
                            return PartialView("FillProducts", svm);
                        }

                        if (Cnt == 0)
                        {
                            saledispatchheader.StockHeaderId = StockViewModel.StockHeaderId;
                        }


                        saledispatchheader.ObjectState = Model.ObjectState.Modified;
                        db.SaleDispatchHeader.Add(saledispatchheader);





                        SaleDispatchLine saledispatchline = new SaleDispatchLine();
                        saledispatchline.SaleDispatchHeaderId = (int)svm.SaleDispatchHeaderId;
                        saledispatchline.SaleDispatchLineId = SaleDispatchLineId;
                        saledispatchline.PackingLineId = item.PackingLineId;
                        //saledispatchline.LotNo = item.LotNo;
                        saledispatchline.GodownId = item.GodownId;
                        saledispatchline.StockId = StockViewModel.StockId;
                        saledispatchline.CreatedBy = User.Identity.Name;
                        saledispatchline.ModifiedBy = User.Identity.Name;
                        saledispatchline.CreatedDate = DateTime.Now;
                        saledispatchline.ModifiedDate = DateTime.Now.Date;






                        //For Inserting Product Uid Last Stage

                        if (item.ProductUidId != null)
                        {
                            ProductUid productuid = new ProductUidService(_unitOfWork).Find((int)item.ProductUidId);

                            saledispatchline.ProductUidLastTransactionDocId = productuid.LastTransactionDocId;
                            saledispatchline.ProductUidLastTransactionDocDate = productuid.LastTransactionDocDate;
                            saledispatchline.ProductUidLastTransactionDocNo = productuid.LastTransactionDocNo;
                            saledispatchline.ProductUidLastTransactionDocTypeId = productuid.LastTransactionDocTypeId;
                            saledispatchline.ProductUidLastTransactionPersonId = productuid.LastTransactionPersonId;
                            saledispatchline.ProductUidStatus = productuid.Status;
                            saledispatchline.ProductUidCurrentProcessId = productuid.CurrenctProcessId;
                            saledispatchline.ProductUidCurrentGodownId = productuid.CurrenctGodownId;

                            productuid.LastTransactionDocId = saledispatchheader.SaleDispatchHeaderId;
                            productuid.LastTransactionDocNo = saledispatchheader.DocNo;
                            productuid.LastTransactionDocTypeId = saledispatchheader.DocTypeId;
                            productuid.LastTransactionDocDate = saledispatchheader.DocDate;
                            productuid.LastTransactionPersonId = saledispatchheader.SaleToBuyerId;
                            productuid.CurrenctGodownId = null;
                            productuid.CurrenctProcessId = null;
                            productuid.CurrenctProcess = null;
                            productuid.Status = ProductUidStatusConstants.Dispatch;
                            productuid.IsActive = false;

                            productuid.ObjectState = Model.ObjectState.Modified;
                            db.ProductUid.Add(productuid);
                        }



                        saledispatchline.ObjectState = Model.ObjectState.Added;
                        db.SaleDispatchLine.Add(saledispatchline);

                        SaleInvoiceLine saleinvoiceline = new SaleInvoiceLine();
                        saleinvoiceline.SaleInvoiceLineId = SaleDispatchLineId - 1;
                        saleinvoiceline.SaleDispatchLineId = SaleDispatchLineId;
                        saleinvoiceline.SaleInvoiceHeaderId = svm.SaleInvoiceHeaderId;


                        //Change On 23/May/2015
                        saleinvoiceline.SaleOrderLineId = item.SaleOrderLineId;
                        saleinvoiceline.Qty = item.Qty;
                        saleinvoiceline.DealQty = item.DealQty;
                        saleinvoiceline.DealUnitId = item.DealUnitId;
                        saleinvoiceline.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        //End Change

                        saleinvoiceline.ProductId = item.ProductId;
                        saleinvoiceline.Rate = item.Rate;
                        saleinvoiceline.Amount = item.Amount;
                        saleinvoiceline.ProductInvoiceGroupId = item.ProductInvoiceGroupId;
                        saleinvoiceline.Remark = item.Remark;
                        saleinvoiceline.RateRemark = item.RateRemark;
                        saleinvoiceline.CreatedBy = User.Identity.Name;
                        saleinvoiceline.ModifiedBy = User.Identity.Name;
                        saleinvoiceline.CreatedDate = DateTime.Now;
                        saleinvoiceline.ModifiedDate = DateTime.Now.Date;
                        saleinvoiceline.ObjectState = Model.ObjectState.Added;
                        db.SaleInvoiceLine.Add(saleinvoiceline);

                        if (saleinvoiceline.SaleOrderLineId.HasValue)
                        {
                            if (LineStatus.ContainsKey(saleinvoiceline.SaleOrderLineId.Value))
                            {
                                LineStatus[saleinvoiceline.SaleOrderLineId.Value] = LineStatus[saleinvoiceline.SaleOrderLineId.Value] + saleinvoiceline.Qty;
                            }
                            else
                            {
                                LineStatus.Add(saleinvoiceline.SaleOrderLineId.Value, saleinvoiceline.Qty);
                            }

                        }


                        TotalAmount = TotalAmount + saleinvoiceline.Amount;


                        //For Generating Description Of Goods For Invoice Printing Purpose
                        if (item.DescriptionOfGoodsName != null)
                        {
                            if (saleinvoiceheaderdetail.DescriptionOfGoods != null)
                            {
                                if (!saleinvoiceheaderdetail.DescriptionOfGoods.Contains(item.DescriptionOfGoodsName))
                                {
                                    saleinvoiceheaderdetail.DescriptionOfGoods = saleinvoiceheaderdetail.DescriptionOfGoods + "," + item.DescriptionOfGoodsName;
                                }
                            }
                            else
                            {
                                saleinvoiceheaderdetail.DescriptionOfGoods = item.DescriptionOfGoodsName;
                            }
                        }



                        // For Generating Composition for invoice for printing purpose
                        if (item.FaceContentName != null)
                        {
                            if (saleinvoiceheaderdetail.Compositions != null)
                            {
                                if (!saleinvoiceheaderdetail.Compositions.Contains(item.FaceContentName + "(" + item.ProductInvoiceGroupName + ")"))
                                {
                                    saleinvoiceheaderdetail.Compositions = saleinvoiceheaderdetail.Compositions + "," + item.FaceContentName + "(" + item.ProductInvoiceGroupName + ")";
                                }
                            }
                            else
                            {
                                saleinvoiceheaderdetail.Compositions = item.FaceContentName + "(" + item.ProductInvoiceGroupName + ")";
                            }
                        }
                        // For Generating Composition for invoice for printing purpose





                        // For Generating Bale No String
                        if (BaleNoStr == "")
                        {
                            BaleNoStr = item.BaleNo.Replace("-", ".");
                        }
                        else
                        {
                            BaleNoStr = BaleNoStr + "," + item.BaleNo.Replace("-", ".");
                        }

                        Cnt = Cnt + 1;
                    }

                }

                //saleinvoiceheaderdetail.BaleNoSeries = mroll.Replace(".", "-");
                saleinvoiceheaderdetail.BaleNoSeries = GetBaleNoSeries(BaleNoStr, svm.SaleInvoiceHeaderId);
                saleinvoiceheaderdetail.ObjectState = Model.ObjectState.Modified;
                db.SaleInvoiceHeader.Add(saleinvoiceheaderdetail);


                //new SaleInvoiceHeaderService(_unitOfWork).Update(saleinvoiceheaderdetail);


                //string LedgerPostingError = "";

                //LedgerPostingError = LedgerPost(LedArr);

                //if (LedgerPostingError != "")
                //{
                //    ModelState.AddModelError("", LedgerPostingError);
                //    return PartialView("_Create", svm);
                //}


                //string StockPostingError = "";

                //StockPostingError = StockPost(StockArr);

                //if (StockPostingError != "")
                //{
                //    ModelState.AddModelError("", StockPostingError);
                //    return PartialView("_Create", svm);
                //}

                UpdateSaleQtyInvoiceMultiple(LineStatus, saleinvoiceheaderdetail.DocDate);

                try
                {
                    //_unitOfWork.Save();
                    db.SaveChanges();
                    db.Configuration.AutoDetectChangesEnabled = true;
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    PrepareViewBag(null);
                    return PartialView("FillProducts", svm);

                }

            }
            return Json(new { success = true });
        }


        public string GetBaleNoSeries(string BaleNoStr, int SaleInvoiceHeaderId)
        {
            string mroll = "";
            Decimal froll = 0;
            int subRollCount = 0;
            string prevrollno = "";
            int Cnt = 0;


            List<string> OldRollNoList = (from L in db.SaleInvoiceLine
                                          join Dl in db.SaleDispatchLine on L.SaleDispatchLineId equals Dl.SaleDispatchLineId into SaleDispatchLineTable
                                          from SaleDispatchLineTab in SaleDispatchLineTable.DefaultIfEmpty()
                                          join Pl in db.PackingLine on SaleDispatchLineTab.PackingLineId equals Pl.PackingLineId into PackingLineTable
                                          from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                                          where L.SaleInvoiceHeaderId == SaleInvoiceHeaderId
                                          select PackingLineTab.BaleNo).ToList();


            List<string> NewRollNoList = BaleNoStr.Split(new Char[] { ',' }).ToList();

            if (OldRollNoList != null && OldRollNoList.Count != 0)
            {
                NewRollNoList.AddRange(OldRollNoList);
            }


            var BaleNoList = NewRollNoList.OrderBy(o => float.Parse(o.ToString())).Distinct().ToList();

            //var BaleNoList = NewRollNoList.OrderBy(o => float.Parse(o.ToString().Replace("-","."))).Distinct().ToList();



            //string str = "1345";

            //string Test =  str.ToString().Substring(0, str.ToString().IndexOf("-") <= 0 ? str.Length : str.ToString().IndexOf("-"));

            //var BaleNoList = NewRollNoList.OrderBy(o => int.Parse(o.ToString().Substring(0, o.ToString().IndexOf("-") <= 0 ? o.Length : o.ToString().IndexOf("-")))).Distinct().ToList();




            foreach (string BaleNo in BaleNoList)
            {
                if (froll == 0)
                {
                    froll = Convert.ToDecimal(BaleNo);
                    mroll = BaleNo;
                }
                else if ((froll + 1) != Convert.ToDecimal(BaleNo))
                {
                    if (subRollCount >= 1)
                    {
                        mroll = mroll + "-" + prevrollno + "," + BaleNo;
                    }
                    else
                    {
                        mroll = mroll + "," + BaleNo;
                    }
                    froll = Convert.ToDecimal(BaleNo);
                    subRollCount = 0;
                }
                else
                {
                    froll = Convert.ToDecimal(BaleNo);
                    subRollCount += 1;
                }
                prevrollno = BaleNo;

                Cnt++;

                if (Cnt == BaleNoList.Count)
                {
                    if (froll != Convert.ToDecimal(BaleNo))
                    {
                        mroll = mroll + "," + BaleNo;
                    }
                    else
                    {
                        mroll = mroll + "-" + BaleNo;
                    }
                }
            }




            return mroll.Replace(".", "-");
        }







        public StockViewModel GetStockViewModelForInvoiceLine(int SaleInvoiceLineId)
        {
            int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Packing).ProcessId;

            StockViewModel StockViewModel = (from H in db.SaleInvoiceHeaderDetail
                                             join L in db.SaleInvoiceLine on H.SaleInvoiceHeaderId equals L.SaleInvoiceHeaderId into SaleInvoiceLineTable
                                             from SaleInvoiceLineTab in SaleInvoiceLineTable.DefaultIfEmpty()
                                             join Dl in db.SaleDispatchLine on SaleInvoiceLineTab.SaleDispatchLineId equals Dl.SaleDispatchLineId into SaleDispatchLineTable
                                             from SaleDispatchLineTab in SaleDispatchLineTable.DefaultIfEmpty()
                                             join Pl in db.PackingLine on SaleDispatchLineTab.PackingLineId equals Pl.PackingLineId into PackingLineTable
                                             from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                                             where SaleInvoiceLineTab.SaleInvoiceLineId == SaleInvoiceLineId
                                             select new StockViewModel
                                             {
                                                 DocHeaderId = H.SaleInvoiceHeaderId,
                                                 DocLineId = SaleInvoiceLineTab.SaleInvoiceLineId,
                                                 DocTypeId = H.DocTypeId,
                                                 StockHeaderDocDate = H.DocDate,
                                                 DocNo = H.DocNo,
                                                 DivisionId = H.DivisionId,
                                                 SiteId = H.SiteId,
                                                 CurrencyId = H.CurrencyId,
                                                 HeaderProcessId = null,
                                                 PersonId = H.BillToBuyerId,
                                                 ProductId = PackingLineTab.ProductId,
                                                 HeaderFromGodownId = null,
                                                 HeaderGodownId = null,
                                                 GodownId = SaleDispatchLineTab.GodownId,
                                                 ProcessId = ProcessId,
                                                 //LotNo = SaleDispatchLineTab.LotNo,
                                                 CostCenterId = null,
                                                 Qty_Iss = PackingLineTab.Qty,
                                                 Qty_Rec = 0,
                                                 Rate = (Decimal)SaleInvoiceLineTab.Rate,
                                                 ExpiryDate = null,
                                                 Specification = null,
                                                 Dimension1Id = null,
                                                 Dimension2Id = null,
                                                 Remark = H.Remark,
                                                 Status = H.Status,
                                                 CreatedBy = H.CreatedBy,
                                                 CreatedDate = H.CreatedDate,
                                                 ModifiedBy = H.ModifiedBy,
                                                 ModifiedDate = H.ModifiedDate
                                             }).FirstOrDefault();
            return StockViewModel;
        }





        public LedgerViewModel GetLedgerViewModelForInvoiceLine(int SaleInvoiceLineId)
        {
            int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Packing).ProcessId;

            LedgerViewModel LedgerViewModel = (from H in db.SaleInvoiceHeaderDetail
                                               join L in db.SaleInvoiceLine on H.SaleInvoiceHeaderId equals L.SaleInvoiceHeaderId into SaleInvoiceLineTable
                                               from SaleInvoiceLineTab in SaleInvoiceLineTable.DefaultIfEmpty()
                                               join Dl in db.SaleDispatchLine on SaleInvoiceLineTab.SaleDispatchLineId equals Dl.SaleDispatchLineId into SaleDispatchLineTable
                                               from SaleDispatchLineTab in SaleDispatchLineTable.DefaultIfEmpty()
                                               join Pl in db.PackingLine on SaleDispatchLineTab.PackingLineId equals Pl.PackingLineId into PackingLineTable
                                               from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                                               where SaleInvoiceLineTab.SaleInvoiceLineId == SaleInvoiceLineId
                                               select new LedgerViewModel
                                               {
                                                   DocHeaderId = H.SaleInvoiceHeaderId,
                                                   DocTypeId = H.DocTypeId,
                                                   DocDate = H.DocDate,
                                                   DocNo = H.DocNo,
                                                   DivisionId = H.DivisionId,
                                                   SiteId = H.SiteId,
                                                   HeaderLedgerAccountId = null,
                                                   LedgerAccountId = H.BillToBuyerId,
                                                   CostCenter = null,
                                                   AmtDr = PackingLineTab.Qty,
                                                   AmtCr = 0,
                                                   HeaderNarration = H.Remark,
                                                   Status = H.Status,
                                                   CreatedBy = H.CreatedBy,
                                                   CreatedDate = H.CreatedDate,
                                                   ModifiedBy = H.ModifiedBy,
                                                   ModifiedDate = H.ModifiedDate
                                               }).FirstOrDefault();
            return LedgerViewModel;
        }



        public string StockPost(IEnumerable<StockViewModel> StockViewModel)
        {
            string ErrorText = "";

            //Get Data from Stock VIew Model and post in Stock header model for insertion or updation
            StockHeader StockHeader = (from L in StockViewModel
                                       select new StockHeader
                                       {
                                           DocHeaderId = L.DocHeaderId,
                                           DocTypeId = L.DocTypeId,
                                           DocDate = L.StockHeaderDocDate,
                                           DocNo = L.DocNo,
                                           DivisionId = L.DivisionId,
                                           SiteId = L.SiteId,
                                           CurrencyId = L.CurrencyId,
                                           PersonId = L.PersonId,
                                           ProcessId = L.HeaderProcessId,
                                           FromGodownId = L.HeaderFromGodownId,
                                           GodownId = L.HeaderGodownId,
                                           Remark = L.Remark,
                                           Status = L.Status,
                                           CreatedBy = L.CreatedBy,
                                           CreatedDate = L.CreatedDate,
                                           ModifiedBy = L.ModifiedBy,
                                           ModifiedDate = L.ModifiedDate
                                       }).FirstOrDefault();

            var temp = new StockHeaderService(_unitOfWork).FindByDocHeader(StockHeader.DocHeaderId, null, StockHeader.DocTypeId, StockHeader.SiteId, StockHeader.DivisionId);


            //if record is found in Stock Header Table then it will edit it if data is not found in Stock header table than it will add a new record.
            if (temp == null)
            {
                StockHeader H = new StockHeader();

                H.DocHeaderId = StockHeader.DocHeaderId;
                H.DocTypeId = StockHeader.DocTypeId;
                H.DocDate = StockHeader.DocDate;
                H.DocNo = StockHeader.DocNo;
                H.DivisionId = StockHeader.DivisionId;
                H.SiteId = StockHeader.SiteId;
                H.CurrencyId = StockHeader.CurrencyId;
                H.PersonId = StockHeader.PersonId;
                H.ProcessId = StockHeader.ProcessId;
                H.FromGodownId = StockHeader.FromGodownId;
                H.GodownId = StockHeader.GodownId;
                H.Remark = StockHeader.Remark;
                H.Status = StockHeader.Status;
                H.CreatedBy = StockHeader.CreatedBy;
                H.CreatedDate = StockHeader.CreatedDate;
                H.ModifiedBy = StockHeader.ModifiedBy;
                H.ModifiedDate = StockHeader.ModifiedDate;

                H.ObjectState = Model.ObjectState.Added;
                db.StockHeader.Add(H);
            }
            else
            {
                IEnumerable<Stock> Stock = (from L in db.Stock
                                            where L.StockHeaderId == temp.StockHeaderId
                                            select L).ToList();

                foreach (Stock item in Stock)
                {
                    db.Stock.Remove(item);
                }



                temp.DocHeaderId = StockHeader.DocHeaderId;
                temp.DocTypeId = StockHeader.DocTypeId;
                temp.DocDate = StockHeader.DocDate;
                temp.DocNo = StockHeader.DocNo;
                temp.DivisionId = StockHeader.DivisionId;
                temp.SiteId = StockHeader.SiteId;
                temp.CurrencyId = StockHeader.CurrencyId;
                temp.PersonId = StockHeader.PersonId;
                temp.ProcessId = StockHeader.ProcessId;
                temp.FromGodownId = StockHeader.FromGodownId;
                temp.GodownId = StockHeader.GodownId;
                temp.Remark = StockHeader.Remark;
                temp.Status = StockHeader.Status;
                temp.CreatedBy = StockHeader.CreatedBy;
                temp.CreatedDate = StockHeader.CreatedDate;
                temp.ModifiedBy = StockHeader.ModifiedBy;
                temp.ModifiedDate = StockHeader.ModifiedDate;


                temp.ObjectState = Model.ObjectState.Modified;
                db.StockHeader.Add(temp);

            }

            IEnumerable<Stock> StockList = (from L in StockViewModel
                                            group new { L } by new { L.ProductId, L.StockDocDate, L.GodownId, L.CostCenterId, L.LotNo, L.ProcessId, L.Dimension1Id, L.Dimension2Id } into Result
                                            select new Stock
                                            {
                                                DocDate = Result.Key.StockDocDate,
                                                ProductId = Result.Key.ProductId,
                                                ProcessId = Result.Key.ProcessId,
                                                GodownId = Result.Key.GodownId,
                                                LotNo = Result.Key.LotNo,
                                                CostCenterId = Result.Key.CostCenterId,
                                                Qty_Iss = Result.Sum(i => i.L.Qty_Iss),
                                                Qty_Rec = Result.Sum(i => i.L.Qty_Rec),
                                                Rate = Result.Max(i => i.L.Rate),
                                                ExpiryDate = Result.Max(i => i.L.ExpiryDate),
                                                Specification = Result.Max(i => i.L.Specification),
                                                Dimension1Id = Result.Key.Dimension1Id,
                                                Dimension2Id = Result.Key.Dimension2Id,
                                                CreatedBy = Result.Max(i => i.L.CreatedBy),
                                                CreatedDate = Result.Max(i => i.L.CreatedDate),
                                                ModifiedBy = Result.Max(i => i.L.ModifiedBy),
                                                ModifiedDate = Result.Max(i => i.L.ModifiedDate)
                                            }).ToList();


            foreach (Stock item in StockList)
            {
                if (temp != null)
                {
                    item.StockHeaderId = temp.StockHeaderId;
                }


                item.ObjectState = Model.ObjectState.Added;
                db.Stock.Add(item);

                //StockBalance StockBalance = new StockBalanceService(_unitOfWork).Find(item.ProductId, item.Dimension1Id, item.Dimension2Id, item.Dimension3Id, item.Dimension4Id, item.ProcessId, item.LotNo, item.GodownId, item.CostCenterId);

                //if (StockBalance == null)
                //{

                //    StockBalance st = new StockBalance();
                //    st.ProductId = item.ProductId;
                //    st.Dimension1Id = item.Dimension1Id;
                //    st.Dimension2Id = item.Dimension2Id;
                //    st.ProcessId = item.ProcessId;
                //    st.GodownId = item.GodownId;
                //    st.CostCenterId = item.CostCenterId;
                //    st.LotNo = item.LotNo;
                //    st.Qty = st.Qty + item.Qty_Rec;
                //    st.Qty = st.Qty - item.Qty_Iss;

                //    //new StockBalanceService(_unitOfWork).Create(st);

                //    st.ObjectState = Model.ObjectState.Added;
                //    db.StockBalance.Add(st);


                //}
                //else
                //{
                //    StockBalance.Qty = StockBalance.Qty + item.Qty_Rec;
                //    StockBalance.Qty = StockBalance.Qty - item.Qty_Iss;
                //    //new StockBalanceService(_unitOfWork).Update(StockBalance);

                //    StockBalance.ObjectState = Model.ObjectState.Modified;
                //    db.StockBalance.Add(StockBalance);
                //}
            }

            return ErrorText;
        }




        public string LedgerPost(IEnumerable<LedgerViewModel> LedgerViewModel)
        {
            string ErrorText = "";

            var ledgertemp = (from L in LedgerViewModel
                              group new { L } by new { L.DocHeaderId } into Result
                              select new
                              {
                                  DocHeadewrId = Result.Key.DocHeaderId,
                                  TotalDrAmt = Result.Sum(i => i.L.AmtDr),
                                  TotalCrAmt = Result.Sum(i => i.L.AmtCr)
                              }).FirstOrDefault();

            if (ledgertemp.TotalCrAmt != ledgertemp.TotalDrAmt)
            {
                ErrorText = "Debit and credit side amounts are not equal.";
                return ErrorText;
            }

            //Get Data from Ledger VIew Model and post in ledger header model for insertion or updation
            LedgerHeader LedgerHeader = (from L in LedgerViewModel
                                         select new LedgerHeader
                                         {
                                             DocHeaderId = L.DocHeaderId,
                                             DocTypeId = L.DocTypeId,
                                             DocDate = L.DocDate,
                                             DocNo = L.DocNo,
                                             DivisionId = L.DivisionId,
                                             SiteId = L.SiteId,
                                             LedgerAccountId = L.HeaderLedgerAccountId,
                                             CreditDays = L.CreditDays,
                                             Narration = L.HeaderNarration,
                                             Remark = L.Remark,
                                             Status = L.Status,
                                             CreatedBy = L.CreatedBy,
                                             CreatedDate = L.CreatedDate,
                                             ModifiedBy = L.ModifiedBy,
                                             ModifiedDate = L.ModifiedDate
                                         }).FirstOrDefault();

            //For Checking that this Doc Header Id already exist in Ledger Header
            var temp = new LedgerHeaderService(_unitOfWork).FindByDocHeader(LedgerHeader.DocHeaderId, LedgerHeader.DocTypeId, LedgerHeader.SiteId, LedgerHeader.DivisionId);

            //if record is found in Ledger Header Table then it will edit it if data is not found in ledger header table than it will add a new record.
            if (temp == null)
            {
                //new LedgerHeaderService(_unitOfWork).Create(LedgerHeader);




                LedgerHeader H = new LedgerHeader();

                H.DocHeaderId = LedgerHeader.DocHeaderId;
                H.DocTypeId = LedgerHeader.DocTypeId;
                H.DocDate = LedgerHeader.DocDate;
                H.DocNo = LedgerHeader.DocNo;
                H.DivisionId = LedgerHeader.DivisionId;
                H.SiteId = LedgerHeader.SiteId;
                H.LedgerAccountId = LedgerHeader.LedgerAccountId;
                H.CreditDays = LedgerHeader.CreditDays;
                H.Narration = LedgerHeader.Narration;
                H.Remark = LedgerHeader.Remark;
                H.Status = LedgerHeader.Status;
                H.CreatedBy = LedgerHeader.CreatedBy;
                H.CreatedDate = LedgerHeader.CreatedDate;
                H.ModifiedBy = LedgerHeader.ModifiedBy;
                H.ModifiedDate = LedgerHeader.ModifiedDate;

                H.ObjectState = Model.ObjectState.Added;
                db.LedgerHeader.Add(H);

            }
            else
            {
                IEnumerable<Ledger> Ledger = (from L in db.Ledger
                                              where L.LedgerHeaderId == LedgerHeader.LedgerHeaderId
                                              select L).ToList();

                foreach (Ledger item in Ledger)
                {
                    db.Ledger.Remove(item);
                }


                temp.DocHeaderId = LedgerHeader.DocHeaderId;
                temp.DocTypeId = LedgerHeader.DocTypeId;
                temp.DocDate = LedgerHeader.DocDate;
                temp.DocNo = LedgerHeader.DocNo;
                temp.DivisionId = LedgerHeader.DivisionId;
                temp.SiteId = LedgerHeader.SiteId;
                temp.LedgerAccountId = LedgerHeader.LedgerAccountId;
                temp.CreditDays = LedgerHeader.CreditDays;
                temp.Narration = LedgerHeader.Narration;
                temp.Remark = LedgerHeader.Remark;
                temp.Status = LedgerHeader.Status;
                temp.CreatedBy = LedgerHeader.CreatedBy;
                temp.CreatedDate = LedgerHeader.CreatedDate;
                temp.ModifiedBy = LedgerHeader.ModifiedBy;
                temp.ModifiedDate = LedgerHeader.ModifiedDate;

                //new LedgerHeaderService(_unitOfWork).Update(temp);

                temp.ObjectState = Model.ObjectState.Modified;
                db.LedgerHeader.Add(temp);

            }

            IEnumerable<Ledger> LedgerList = (from L in LedgerViewModel
                                              group new { L } by new { L.LedgerAccountId, L.ContraLedgerAccountId, L.CostCenterId } into Result
                                              select new Ledger
                                              {
                                                  LedgerAccountId = Result.Key.LedgerAccountId,
                                                  ContraLedgerAccountId = Result.Key.ContraLedgerAccountId,
                                                  CostCenterId = Result.Key.CostCenterId,
                                                  AmtDr = Result.Sum(i => i.L.AmtDr),
                                                  AmtCr = Result.Sum(i => i.L.AmtCr),
                                                  Narration = Result.Max(i => i.L.Narration)
                                              }).ToList();

            foreach (Ledger item in LedgerList)
            {
                if (temp != null)
                {
                    item.LedgerHeaderId = temp.LedgerHeaderId;
                }

                //item.LedgerHeaderId = LedgerHeaderId;

                item.ObjectState = Model.ObjectState.Added;
                db.Ledger.Add(item);


            }

            return ErrorText;
        }

        [HttpGet]
        public ActionResult UpdateRates(int id)
        {
            UpdateRates s = new UpdateRates();
            s.SaleInvoiceHeaderId = id;

            return PartialView("UpdateRates", s);
        }


        [HttpPost]
        public ActionResult UpdateRates(UpdateRates svm)
        {
            if (ModelState.IsValid)
            {
                var temp = (from L in db.SaleInvoiceLine
                            join D in db.SaleDispatchLine on L.SaleDispatchLineId equals D.SaleDispatchLineId into SaleDispatchLineTable
                            from SaleDispatchLineTab in SaleDispatchLineTable.DefaultIfEmpty()
                            join P in db.PackingLine on SaleDispatchLineTab.PackingLineId equals P.PackingLineId into PackingLineTable
                            from PackingLineTab in PackingLineTable.DefaultIfEmpty()
                            where L.SaleInvoiceHeaderId == svm.SaleInvoiceHeaderId
                            select new
                                {
                                    SaleInvoiceLineId = L.SaleInvoiceLineId,
                                    DealQty = L.DealQty,
                                    BaleNo = PackingLineTab.BaleNo

                                }).ToList();

                var SaleInvoiceLineList = temp.Where(i => Convert.ToDecimal(i.BaleNo.Replace("-", ".")) >= svm.FromBaleNo && Convert.ToDecimal(i.BaleNo.Replace("-", ".")) <= svm.ToBaleNo)
                        .Select(m => new { SaleInvoiceLineId = m.SaleInvoiceLineId, DealQty = m.DealQty }).ToList();

                foreach (var item in SaleInvoiceLineList)
                {
                    SaleInvoiceLine SaleInvoiceLine = _SaleInvoiceLineService.Find(item.SaleInvoiceLineId);
                    SaleInvoiceLine.Rate = svm.Rate;
                    SaleInvoiceLine.Amount = (item.DealQty * svm.Rate);
                    SaleInvoiceLine.ProductInvoiceGroupId = svm.ProductInvoiceGroupId;
                    _SaleInvoiceLineService.Update(SaleInvoiceLine);
                }



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
                return RedirectToAction("UpdateRates", new { id = svm.SaleInvoiceHeaderId });
            }



            return PartialView("UpdateRates", svm);
        }


        public JsonResult GetRate(int ProductInvoiceGroupId)
        {
            var ProductInvoiceGroup = new ProductInvoiceGroupService(_unitOfWork).Find(ProductInvoiceGroupId);

            if (ProductInvoiceGroup != null)
            {
                return Json(ProductInvoiceGroup.Rate);
            }
            else
            {
                return Json(null);
            }
        }


        public void UpdateSaleQtyInvoiceMultiple(Dictionary<int, decimal> Qty, DateTime DocDate)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();





            using (ApplicationDbContext con = new ApplicationDbContext())
            {
                con.Configuration.AutoDetectChangesEnabled = false;
                con.Configuration.LazyLoadingEnabled = false;

                con.Database.CommandTimeout = 30000;
                con.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");
                var LineAndQty = (from p in con.SaleInvoiceLine
                                  where (IsdA).Contains(p.SaleOrderLineId.Value)
                                  group p by p.SaleOrderLineId into g
                                  select new
                                  {
                                      LineId = g.Key,
                                      Qty = g.Sum(m => m.Qty),
                                  }).ToList();

                string Ids2 = null;

                Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

                var LineStatus = (from p in db.SaleOrderLineStatus
                                  where IsdA.Contains(p.SaleOrderLineId.Value)
                                  select p).ToList();

                foreach (var item in LineStatus)
                {
                    item.InvoiceQty = Qty[item.SaleOrderLineId.Value] + (LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault().Qty);
                    item.ShipQty = item.InvoiceQty;
                    item.InvoiceDate = DocDate;
                    item.ShipDate = item.InvoiceDate;
                    item.ObjectState = Model.ObjectState.Modified;
                    db.SaleOrderLineStatus.Add(item);
                }
            }

            //var LineAndQty = (from p in db.SaleInvoiceLine
            //                  where (IsdA).Contains(p.SaleOrderLineId.ToString())
            //                  group p by p.SaleOrderLineId into g
            //                  select new
            //                  {
            //                      LineId = g.Key,
            //                      Qty = g.Sum(m => m.Qty),
            //                  }).ToList();



        }


        public void UpdateSaleQtyOnInvoice(int id, int InvoiceLineId, DateTime DocDate, decimal Qty)
        {

            var temp = (from p in db.SaleInvoiceLine
                        where p.SaleOrderLineId == id && p.SaleInvoiceLineId != InvoiceLineId
                        join t in db.SaleInvoiceHeader on p.SaleInvoiceHeaderId equals t.SaleInvoiceHeaderId
                        select new
                        {
                            Qty = p.Qty,
                            Date = t.DocDate,
                        }).ToList();
            decimal qty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(SaleStatusQtyConstants.InvoiceQty, qty, date, id);

        }






        public void UpdateStatusQty(string QtyType, decimal Qty, DateTime date, int Id)
        {

            SaleOrderLineStatus Stat = new SaleOrderLineStatusService(_unitOfWork).Find(Id);

            switch (QtyType)
            {
                case SaleStatusQtyConstants.CancelQty:
                    {
                        Stat.CancelQty = Qty;
                        Stat.CancelDate = date;
                        break;
                    }
                case SaleStatusQtyConstants.InvoiceQty:
                    {
                        Stat.InvoiceQty = Qty;
                        Stat.InvoiceDate = date;
                        Stat.ShipQty = Stat.InvoiceQty;
                        Stat.ShipDate = Stat.InvoiceDate;
                        break;
                    }
                case SaleStatusQtyConstants.AmendmentQty:
                    {
                        Stat.AmendmentQty = Qty;
                        Stat.AmendmentDate = date;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            Stat.ObjectState = Model.ObjectState.Modified;
            db.SaleOrderLineStatus.Add(Stat);

        }

                [HttpGet]
        public ActionResult UpdateRatesDesignWise(int id)
        {
            ViewBag.SaleInvoiceHeaderId = id;
            return View("UpdateRatesDesignWise");
        }


        public JsonResult GetSaleInvoiceDetail(int SaleInvoiceHeaderId)
        {
            var temp = (from L in db.SaleInvoiceLine
                        join P in db.Product on L.ProductId equals P.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join Pg in db.ProductGroups on ProductTab.ProductGroupId equals Pg.ProductGroupId into ProductGroupTable
                        from ProductGroupTab in ProductGroupTable.DefaultIfEmpty()
                        where L.SaleInvoiceHeaderId == SaleInvoiceHeaderId
                        group new { L, ProductGroupTab } by new { ProductGroupTab.ProductGroupId, ProductGroupTab.ProductGroupName, L.RateRemark } into Result
                        orderby Result.Key.ProductGroupName
                        select new
                        {
                            UniqueName = Result.Key.ProductGroupName + (Result.Key.RateRemark ?? ""),
                            ProductGroupId = Result.Key.ProductGroupId,
                            DesignName = Result.Key.ProductGroupName,
                            Remark = Result.Key.RateRemark
                        }).ToList();

            return Json(new { data = temp }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void UpdateRatesDesignWise(int SaleInvoiceHeaderId, int ProductGroupId, string Remark, Decimal? Rate)
        {
            if (Rate != null)
            {
                IEnumerable<SaleInvoiceLine> SaleInvoiceLineList = (from L in db.SaleInvoiceLine
                                       join P in db.Product on L.ProductId equals P.ProductId into ProductTable
                                       from ProductTab in ProductTable.DefaultIfEmpty()
                                       where L.SaleInvoiceHeaderId == SaleInvoiceHeaderId && ProductTab.ProductGroupId == ProductGroupId && (L.RateRemark ?? "") == Remark
                                       select L).ToList();

                foreach (var SaleInvoiceLine in SaleInvoiceLineList)
                {

                        SaleInvoiceLine.Rate = (Decimal)Rate;
                        SaleInvoiceLine.Amount = SaleInvoiceLine.DealQty * SaleInvoiceLine.Rate;
                        SaleInvoiceLine.ObjectState = Model.ObjectState.Modified;
                        db.SaleInvoiceLine.Add(SaleInvoiceLine);
                
                }

                try
                {
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                }
            }
        }
    }


    public class InvoiceDescriptionOfGoods
    {
        public string DescriptionOfGoods { get; set; }
    }

}
