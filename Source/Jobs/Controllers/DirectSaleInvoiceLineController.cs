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
    public class DirectSaleInvoiceLineController : System.Web.Mvc.Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleInvoiceLineService _SaleInvoiceLineService;
        ISaleInvoiceLineDetailService _SaleInvoiceLineDetailService;
        ISaleDispatchLineService _SaleDispatchLineService;
        IPackingLineService _PackingLineService;
        IStockService _StockService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public DirectSaleInvoiceLineController(ISaleInvoiceLineService SaleInvoice, ISaleInvoiceLineDetailService SaleInvoiceDetail, IStockService StockService, ISaleDispatchLineService SaleDispatch, IUnitOfWork unitOfWork, IExceptionHandlingService exec, IPackingLineService packLineServ)
        {
            _SaleInvoiceLineService = SaleInvoice;
            _SaleInvoiceLineDetailService = SaleInvoiceDetail;
            _SaleDispatchLineService = SaleDispatch;
            _StockService = StockService;
            _PackingLineService = packLineServ;
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


        public ActionResult _Index(int id, int Status)
        {
            ViewBag.Status = Status;
            ViewBag.SaleInvoiceHeaderId = id;
            var p = _SaleInvoiceLineService.GetDirectSaleInvoiceLineListForIndex(id).ToList();
            return PartialView(p);
        }

        public ActionResult _ForOrder(int id)
        {
            SaleInvoiceFilterViewModel vm = new SaleInvoiceFilterViewModel();
            vm.SaleInvoiceHeaderId = id;
            SaleInvoiceHeader H = db.SaleInvoiceHeader.Find(id);
            SaleInvoiceSetting Settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            vm.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSettingsViewModel>(Settings);
            return PartialView("_OrderFilters", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostOrders(SaleInvoiceFilterViewModel vm)
        {
            List<DirectSaleInvoiceLineViewModel> temp = _SaleInvoiceLineService.GetSaleOrdersForFilters(vm).ToList();
            DirectSaleInvoiceListViewModel svm = new DirectSaleInvoiceListViewModel();

            SaleInvoiceHeader Header = db.SaleInvoiceHeader.Find(vm.SaleInvoiceHeaderId);
            //SaleInvoiceHeader Header = new SaleInvoiceHeaderService(_unitOfWork).Find(vm.SaleInvoiceHeaderId);
            SaleInvoiceSetting settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);

            svm.DirectSaleInvoiceLineViewModel = temp;
            return PartialView("_Results", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(DirectSaleInvoiceListViewModel vm)
        {
            int Cnt = 0;
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            SaleInvoiceHeader Sh = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(vm.DirectSaleInvoiceLineViewModel.FirstOrDefault().SaleInvoiceHeaderId);

            SaleDispatchHeader Dh = new SaleDispatchHeaderService(_unitOfWork).Find(Sh.SaleDispatchHeaderId.Value);



            SaleDispatchLine LastRecord = _SaleDispatchLineService.GetSaleDispatchLineList(Dh.SaleDispatchHeaderId).OrderByDescending(m => m.SaleDispatchLineId).FirstOrDefault();

            if (LastRecord == null)
            {
                if (vm.DirectSaleInvoiceLineViewModel.FirstOrDefault() != null)
                {
                    if (vm.DirectSaleInvoiceLineViewModel.FirstOrDefault().GodownId == null)
                    {
                        TempData["CSEXCL"] += "Please insert a record before creating from multiple";
                        return PartialView("_Results", vm);
                    }
                }
            }
            PackingHeader Ph = new PackingHeaderService(_unitOfWork).Find(Dh.PackingHeaderId.Value);

            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            int PackingPrimaryKey = 0;
            int DispatchPrimaryKey = 0;
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
                    decimal balqty = (from p in db.ViewSaleOrderBalance
                                      where p.SaleOrderLineId == item.SaleOrderLineId
                                      select p.BalanceQty).FirstOrDefault();
                    if (item.Qty > 0 && (item.Rate > 0) && item.Qty <= balqty)
                    {

                        PackingLine Pl = new PackingLine();

                        if (item.PackingLineId != null)
                        {
                            Pl = new PackingLineService(_unitOfWork).Find((int)item.PackingLineId);
                        }

                        SaleDispatchLine Dl = new SaleDispatchLine();
                        SaleInvoiceLine line = new SaleInvoiceLine();

                        StockViewModel StockViewModel = new StockViewModel();

                        if (Cnt == 0)
                        {
                            StockViewModel.StockHeaderId = Dh.StockHeaderId ?? 0;
                        }
                        else
                        {
                            if (Dh.StockHeaderId != null && Dh.StockHeaderId != 0)
                            {
                                StockViewModel.StockHeaderId = (int)Dh.StockHeaderId;
                            }
                            else
                            {
                                StockViewModel.StockHeaderId = -1;
                            }
                        }

                        StockViewModel.StockId = -Cnt;
                        StockViewModel.DocHeaderId = Dh.SaleDispatchHeaderId;
                        StockViewModel.DocLineId = Dl.SaleDispatchLineId;
                        StockViewModel.DocTypeId = Dh.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Dh.DocDate;
                        StockViewModel.StockDocDate = Dh.DocDate;
                        StockViewModel.DocNo = Dh.DocNo;
                        StockViewModel.DivisionId = Dh.DivisionId;
                        StockViewModel.SiteId = Dh.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = Dh.SaleToBuyerId;
                        StockViewModel.ProductId = item.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = null;
                        StockViewModel.HeaderProcessId = null;

                        if (item.GodownId != 0 && item.GodownId != null)
                            StockViewModel.GodownId = item.GodownId;
                        else
                            StockViewModel.GodownId = (int)LastRecord.GodownId;

                        StockViewModel.Remark = Dh.Remark;
                        StockViewModel.Status = Dh.Status;
                        //StockViewModel.ProcessId = Dh.ProcessId;

                        if (Pl != null)
                        {
                            var LastProcess = (from L in db.PackingLine 
                                                        join S in db.Stock on L.StockReceiveId equals S.StockId into StockTable from StockTab in StockTable.DefaultIfEmpty()
                                                        where L.PackingLineId == Pl.PackingLineId 
                                                        select new { ProcessId = StockTab.ProcessId }).FirstOrDefault();
                            if (LastProcess != null)
                                StockViewModel.ProcessId = LastProcess.ProcessId;
                        }

                        StockViewModel.LotNo = null;
                        //StockViewModel.CostCenterId = Dh.CostCenterId;
                        StockViewModel.Qty_Iss = item.Qty;
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = item.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = item.Specification;
                        StockViewModel.Dimension1Id = item.Dimension1Id;
                        StockViewModel.Dimension2Id = item.Dimension2Id;
                        StockViewModel.CreatedBy = User.Identity.Name;
                        StockViewModel.CreatedDate = DateTime.Now;
                        StockViewModel.ModifiedBy = User.Identity.Name;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel);

                        if (StockPostingError != "")
                        {
                            string message = StockPostingError;
                            ModelState.AddModelError("", message);
                            return PartialView("_Results", vm);
                        }

                        if (Cnt == 0)
                        {
                            Dh.StockHeaderId = StockViewModel.StockHeaderId;
                        }
                        Dl.StockId = StockViewModel.StockId;





                        if (item.PackingLineId == null)
                        {
                            Pl.BaleNo = item.BaleNo;
                            Pl.DealQty = item.Qty * item.UnitConversionMultiplier ?? 0;
                            Pl.DealUnitId = item.DealUnitId;
                            Pl.Dimension1Id = item.Dimension1Id;
                            Pl.Dimension2Id = item.Dimension2Id;
                            Pl.LotNo = item.LotNo;
                            Pl.CreatedBy = User.Identity.Name;
                            Pl.CreatedDate = DateTime.Now;
                            Pl.ModifiedBy = User.Identity.Name;
                            Pl.ModifiedDate = DateTime.Now;
                            Pl.PackingHeaderId = Ph.PackingHeaderId;
                            Pl.ProductId = item.ProductId;
                            Pl.Qty = item.Qty;
                            Pl.FreeQty = item.FreeQty;
                            Pl.Remark = item.Remark;
                            Pl.SaleOrderLineId = item.SaleOrderLineId;
                            Pl.Specification = item.Specification;
                            Pl.PackingLineId = PackingPrimaryKey++;
                            Pl.ObjectState = Model.ObjectState.Added;
                            _PackingLineService.Create(Pl);
                        }




                        Dl.CreatedBy = User.Identity.Name;
                        Dl.ModifiedBy = User.Identity.Name;
                        Dl.CreatedDate = DateTime.Now;
                        Dl.ModifiedDate = DateTime.Now;

                        if (item.GodownId != 0 && item.GodownId != null)
                            Dl.GodownId = item.GodownId;
                        else
                            Dl.GodownId = (int)LastRecord.GodownId;

                        Dl.PackingLineId = Pl.PackingLineId;
                        Dl.StockInId = Pl.StockReceiveId;
                        Dl.Remark = item.Remark;
                        Dl.SaleDispatchHeaderId = Dh.SaleDispatchHeaderId;
                        Dl.SaleDispatchLineId = DispatchPrimaryKey++;
                        Dl.ObjectState = Model.ObjectState.Added;
                        _SaleDispatchLineService.Create(Dl);




                        if (Pl.StockReceiveId != null)
                        {
                            StockAdj Adj_IssQty = new StockAdj();
                            Adj_IssQty.StockAdjId = -Cnt;
                            Adj_IssQty.StockInId = (int)Pl.StockReceiveId;
                            Adj_IssQty.StockOutId = (int)Dl.StockId;
                            Adj_IssQty.DivisionId = Sh.DivisionId;
                            Adj_IssQty.SiteId = Sh.SiteId;
                            Adj_IssQty.AdjustedQty = item.Qty;
                            Adj_IssQty.ObjectState = Model.ObjectState.Added;
                            new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                        }





                        line.SaleInvoiceHeaderId = item.SaleInvoiceHeaderId;
                        line.SaleOrderLineId = item.SaleOrderLineId;
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
                        line.DiscountPer = item.DiscountPer;
                        line.ProductId = item.ProductId;
                        line.Qty = item.Qty;
                        line.Remark = item.Remark;
                        line.SaleDispatchLineId = Dl.SaleDispatchLineId;
                        line.ObjectState = Model.ObjectState.Added;
                        _SaleInvoiceLineService.Create(line);

                        SaleInvoiceLineDetail linedetail = new SaleInvoiceLineDetail();
                        linedetail.SaleInvoiceLineId = line.SaleInvoiceLineId;
                        linedetail.RewardPoints = item.RewardPoints;
                        _SaleInvoiceLineDetailService.Create(linedetail);

                        if (Pl.SaleOrderLineId != null)
                        {
                            if (LineStatus.ContainsKey((int)Pl.SaleOrderLineId))
                            {
                                var LineStatusRow = LineStatus.Where(i => i.Key == Pl.SaleOrderLineId).FirstOrDefault();
                                LineStatus[LineStatusRow.Key] = LineStatusRow.Value + line.Qty;
                            }
                            else
                            {
                                LineStatus.Add((int)Pl.SaleOrderLineId, line.Qty);
                            }
                        }


                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.SaleInvoiceLineId, HeaderTableId = item.SaleInvoiceHeaderId, PersonID = Sh.BillToBuyerId });

                        pk++;
                        Cnt = Cnt + 1;
                    }
                }

                new SaleDispatchHeaderService(_unitOfWork).Update(Dh);

                new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyInvoiceMultiple(LineStatus, Sh.DocDate);

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
            s.SaleToBuyerId = H.SaleToBuyerId;
            s.DocDate = H.DocDate;
            s.SalesTaxGroupPersonId = H.SalesTaxGroupPersonId;


            var LastInvoiceLine = (from L in db.SaleInvoiceLine
                                   join D in db.SaleDispatchLine on L.SaleDispatchLineId equals D.SaleDispatchLineId into SaleDispatchLineTable
                                   from SaleDispatchLineTab in SaleDispatchLineTable.DefaultIfEmpty()
                                   where L.SaleInvoiceHeaderId == Id
                                   orderby L.SaleInvoiceLineId descending
                                   select new
                                   {
                                       GodownId = SaleDispatchLineTab.GodownId
                                   }).FirstOrDefault();


            if (LastInvoiceLine != null)
            {
                s.GodownId = LastInvoiceLine.GodownId;
            }
            else
            {
                var PackingHeader = (from I in db.SaleInvoiceHeader
                                               join D in db.SaleDispatchHeader on I.SaleDispatchHeaderId equals D.SaleDispatchHeaderId into SaleDispatchHeaderTable
                                               from SaleDispatchHeaderTab in SaleDispatchHeaderTable.DefaultIfEmpty()
                                               join P in db.PackingHeader on SaleDispatchHeaderTab.PackingHeaderId equals P.PackingHeaderId into PackingHeaderTable
                                               from PackingHeaderTab in PackingHeaderTable.DefaultIfEmpty()
                                               where I.SaleInvoiceHeaderId == Id
                                               select new
                                               {
                                                   GodownId = PackingHeaderTab.GodownId
                                               }).FirstOrDefault();

                if (PackingHeader != null)
                {
                    s.GodownId = PackingHeader.GodownId;
                }
            }
            

            ViewBag.LineMode = "Create";
            PrepareViewBag();
            if (IsSaleBased == true)
            {
                return PartialView("_CreateForSaleOrder", s);

            }
            else
            {
                return PartialView("_Create", s);
            }

        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult _CreatePost(DirectSaleInvoiceLineViewModel svm)
        {
            SaleInvoiceHeader Sh = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(svm.SaleInvoiceHeaderId);

            SaleDispatchHeader Dh = new SaleDispatchHeaderService(_unitOfWork).Find(Sh.SaleDispatchHeaderId.Value);

            PackingHeader Ph = new PackingHeaderService(_unitOfWork).Find(Dh.PackingHeaderId.Value);

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(Sh.DocTypeId, Sh.DivisionId, Sh.SiteId);

            if (svm.SaleInvoiceLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.IsSaleBased == true && svm.SaleOrderLineId <= 0)
            {
                ModelState.AddModelError("", "Sale Order field is required");
            }

            if (svm.Qty <= 0)
                ModelState.AddModelError("", "The Qty field is required");

            if (svm.GodownId <= 0)
                ModelState.AddModelError("", "The Godown field is required");

            
            #region "Tax Calculation Validation"
            //SiteDivisionSettings SiteDivisionSettings = new SiteDivisionSettingsService(_unitOfWork).GetSiteDivisionSettings(Sh.SiteId, Sh.DivisionId, Sh.DocDate);
            //if (SiteDivisionSettings != null)
            //{
            //    if (SiteDivisionSettings.IsApplicableGST == true)
            //    {
            //        if (svm.SalesTaxGroupPersonId == 0 || svm.SalesTaxGroupPersonId == null)
            //        {
            //            ModelState.AddModelError("", "Sales Tax Group Person is not defined for party, it is required.");
            //        }

            //        if (svm.SalesTaxGroupProductId == 0 || svm.SalesTaxGroupProductId == null)
            //        {
            //            ModelState.AddModelError("", "Sales Tax Group Product is not defined for product, it is required.");
            //        }

            //        if (svm.SalesTaxGroupProductId != 0 && svm.SalesTaxGroupProductId != null && svm.SalesTaxGroupPersonId != 0 && svm.SalesTaxGroupPersonId != null && svm.SaleInvoiceSettings.CalculationId != null && svm.SaleInvoiceSettings.ProcessId != null)
            //        {
            //            IEnumerable<ChargeRateSettings> ChargeRateSettingsList = new CalculationProductService(_unitOfWork).GetChargeRateSettingForValidation((int)svm.SaleInvoiceSettings.CalculationId, Sh.DocTypeId, Sh.SiteId, Sh.DivisionId, (int)svm.SaleInvoiceSettings.ProcessId, (int)svm.SalesTaxGroupPersonId, (int)svm.SalesTaxGroupProductId);

            //            foreach (var item in ChargeRateSettingsList)
            //            {
            //                if (item.ChargeGroupSettingId == null)
            //                {
            //                    ModelState.AddModelError("", "Charge Group Setting is not defined for " + item.ChargeName + ".");
            //                }

            //                if (item.LedgerAccountCrName == LedgerAccountConstants.Charge || item.LedgerAccountDrName == LedgerAccountConstants.Charge)
            //                {
            //                    if (item.ChargeGroupSettingId != null && item.ChargePer != 0 && item.ChargePer != null && item.ChargeLedgerAccountId == null)
            //                    {
            //                        ModelState.AddModelError("", "Ledger account is not defined for " + item.ChargeName + " in charge group settings.");
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion



            if (ModelState.IsValid)
            {
                if (svm.SaleInvoiceLineId <= 0)
                {
                    PackingLine Pl = Mapper.Map<DirectSaleInvoiceLineViewModel, PackingLine>(svm);

                    SaleDispatchLine Dl = Mapper.Map<DirectSaleInvoiceLineViewModel, SaleDispatchLine>(svm);

                    SaleInvoiceLine Sl = Mapper.Map<DirectSaleInvoiceLineViewModel, SaleInvoiceLine>(svm);
                    SaleInvoiceLineDetail Sid = Mapper.Map<DirectSaleInvoiceLineViewModel, SaleInvoiceLineDetail>(svm);



                    StockViewModel StockViewModel = new StockViewModel();
                    StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                    //Posting in Stock

                    StockViewModel.StockHeaderId = Dh.StockHeaderId ?? 0;
                    StockViewModel.DocHeaderId = Dh.SaleDispatchHeaderId;
                    StockViewModel.DocLineId = Dl.SaleDispatchLineId;
                    StockViewModel.DocTypeId = Dh.DocTypeId;
                    StockViewModel.StockHeaderDocDate = Dh.DocDate;
                    StockViewModel.StockDocDate = Dh.DocDate;
                    StockViewModel.DocNo = Dh.DocNo;
                    StockViewModel.DivisionId = Dh.DivisionId;
                    StockViewModel.SiteId = Dh.SiteId;
                    StockViewModel.CurrencyId = null;
                    StockViewModel.HeaderProcessId = settings.ProcessId;
                    StockViewModel.PersonId = Dh.SaleToBuyerId;
                    StockViewModel.ProductId = Sl.ProductId;
                    StockViewModel.ProductUidId = svm.ProductUidId;
                    StockViewModel.HeaderFromGodownId = null;
                    StockViewModel.HeaderGodownId = null;
                    StockViewModel.GodownId = Dl.GodownId;
                    //StockViewModel.ProcessId = Dl.FromProcessId;
                    StockViewModel.LotNo = svm.LotNo;
                    //StockViewModel.CostCenterId = svm.CostCenterId;
                    StockViewModel.Qty_Iss = Sl.Qty + (svm.FreeQty ?? 0);
                    StockViewModel.Qty_Rec = 0;
                    StockViewModel.Rate = Sl.Rate;
                    StockViewModel.ExpiryDate = null;
                    StockViewModel.Specification = svm.Specification;
                    StockViewModel.Dimension1Id = Sl.Dimension1Id;
                    StockViewModel.Dimension2Id = Sl.Dimension2Id;
                    StockViewModel.Remark = Sl.Remark;
                    StockViewModel.Status = Dh.Status;
                    StockViewModel.CreatedBy = Dh.CreatedBy;
                    StockViewModel.CreatedDate = DateTime.Now;
                    StockViewModel.ModifiedBy = Dh.ModifiedBy;
                    StockViewModel.ModifiedDate = DateTime.Now;

                    string StockPostingError = "";
                    StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }

                    Dl.StockId = StockViewModel.StockId;

                    if (Dh.StockHeaderId == null)
                    {
                        Dh.StockHeaderId = StockViewModel.StockHeaderId;
                    }



                    Sid.SaleInvoiceLineId = Sl.SaleInvoiceLineId;
                    _SaleInvoiceLineDetailService.Create(Sid);




                    if (svm.ProductUidId != null && svm.ProductUidId > 0)
                    {
                        ProductUid Produid = new ProductUidService(_unitOfWork).Find((int)svm.ProductUidId);


                        Pl.ProductUidLastTransactionDocId = Produid.LastTransactionDocId;
                        Pl.ProductUidLastTransactionDocDate = Produid.LastTransactionDocDate;
                        Pl.ProductUidLastTransactionDocNo = Produid.LastTransactionDocNo;
                        Pl.ProductUidLastTransactionDocTypeId = Produid.LastTransactionDocTypeId;
                        Pl.ProductUidLastTransactionPersonId = Produid.LastTransactionPersonId;
                        Pl.ProductUidStatus = Produid.Status;
                        Pl.ProductUidCurrentProcessId = Produid.CurrenctProcessId;
                        Pl.ProductUidCurrentGodownId = Produid.CurrenctGodownId;


                        if ((settings.DoNotUpdateProductUidStatus ?? false) == false)
                        {
                            Produid.LastTransactionDocId = Ph.PackingHeaderId;
                            Produid.LastTransactionDocNo = Ph.DocNo;
                            Produid.LastTransactionDocTypeId = Ph.DocTypeId;
                            Produid.LastTransactionDocDate = Ph.DocDate;
                            Produid.LastTransactionPersonId = Ph.JobWorkerId;
                            Produid.CurrenctGodownId = Ph.GodownId;
                            Produid.CurrenctProcessId = settings.ProcessId;
                            Produid.Status = ProductUidStatusConstants.Issue;

                            if (Produid.ProcessesDone == null)
                            {
                                Produid.ProcessesDone = "|" + settings.ProcessId.ToString() + "|";
                            }
                            else
                            {
                                Produid.ProcessesDone = Produid.ProcessesDone + ",|" + settings.ProcessId.ToString() + "|";
                            }

                            new ProductUidService(_unitOfWork).Update(Produid);
                        }
                    }


                    Pl.PackingHeaderId = Ph.PackingHeaderId;
                    Pl.CreatedBy = User.Identity.Name;
                    Pl.CreatedDate = DateTime.Now;
                    Pl.ModifiedBy = User.Identity.Name;
                    Pl.ModifiedDate = DateTime.Now;
                    Pl.ObjectState = Model.ObjectState.Added;
                    _PackingLineService.Create(Pl);


                    Dl.SaleDispatchHeaderId = Dh.SaleDispatchHeaderId;
                    Dl.PackingLineId = Pl.PackingLineId;
                    Dl.CreatedBy = User.Identity.Name;
                    Dl.CreatedDate = DateTime.Now;
                    Dl.ModifiedBy = User.Identity.Name;
                    Dl.ModifiedDate = DateTime.Now;
                    Dl.ObjectState = Model.ObjectState.Added;
                    _SaleDispatchLineService.Create(Dl);



                    Sl.SaleDispatchLineId = Dl.SaleDispatchLineId;
                    Sl.SaleInvoiceHeaderId = Sh.SaleInvoiceHeaderId;
                    Sl.DiscountPer = svm.DiscountPer;
                    Sl.Sr = _SaleInvoiceLineService.GetMaxSr(Sh.SaleInvoiceHeaderId);
                    Sl.CreatedDate = DateTime.Now;
                    Sl.ModifiedDate = DateTime.Now;
                    Sl.CreatedBy = User.Identity.Name;
                    Sl.ModifiedBy = User.Identity.Name;
                    Sl.ObjectState = Model.ObjectState.Added;
                    _SaleInvoiceLineService.Create(Sl);

                    if (Pl.SaleOrderLineId != null)
                    {
                        new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyOnInvoice((int)Pl.SaleOrderLineId, Sl.SaleInvoiceLineId, Sh.DocDate, Pl.Qty);
                    }



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
                        Dh.Status = (int)StatusConstants.Modified;
                        Ph.Status = (int)StatusConstants.Modified;
                        new PackingHeaderService(_unitOfWork).Update(Ph);
                        new SaleInvoiceHeaderService(_unitOfWork).Update(Sh);
                    }

                    new SaleDispatchHeaderService(_unitOfWork).Update(Dh);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag();
                        if (svm.SaleOrderLineId.HasValue && svm.SaleOrderLineId.Value > 0)
                            return PartialView("_CreateForSaleOrder", svm);
                        else
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


                    return RedirectToAction("_Create", new { id = Sh.SaleInvoiceHeaderId, IsSaleBased = (Sl.SaleOrderLineId == null ? false : true) });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    int status = Sh.Status;


                    PackingLine Pl = _PackingLineService.Find(svm.PackingLineId.Value);

                    SaleDispatchLine Dl = _SaleDispatchLineService.Find(svm.SaleDispatchLineId);

                    SaleInvoiceLine Sl = _SaleInvoiceLineService.Find(svm.SaleInvoiceLineId);

                    SaleInvoiceLineDetail Sid = _SaleInvoiceLineDetailService.Find(svm.SaleInvoiceLineId);

                    PackingLine ExRec = new PackingLine();
                    ExRec = Mapper.Map<PackingLine>(Pl);

                    SaleDispatchLine ExRecD = new SaleDispatchLine();
                    ExRecD = Mapper.Map<SaleDispatchLine>(Dl);

                    SaleInvoiceLine ExRecS = new SaleInvoiceLine();
                    ExRecS = Mapper.Map<SaleInvoiceLine>(Sl);

                    if (Dl.StockId != null)
                    {
                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = Dh.StockHeaderId ?? 0;
                        StockViewModel.StockId = Dl.StockId ?? 0;
                        StockViewModel.DocHeaderId = Dl.SaleDispatchHeaderId;
                        StockViewModel.DocLineId = Dl.SaleDispatchLineId;
                        StockViewModel.DocTypeId = Dh.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Dh.DocDate;
                        StockViewModel.StockDocDate = Dh.DocDate;
                        StockViewModel.DocNo = Dh.DocNo;
                        StockViewModel.DivisionId = Dh.DivisionId;
                        StockViewModel.SiteId = Dh.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = null;
                        StockViewModel.PersonId = Dh.SaleToBuyerId;
                        StockViewModel.ProductId = Sl.ProductId;
                        StockViewModel.ProductUidId = svm.ProductUidId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = null;
                        StockViewModel.GodownId = svm.GodownId;
                        //StockViewModel.ProcessId = Dl.FromProcessId;
                        StockViewModel.LotNo = svm.LotNo;
                        //StockViewModel.CostCenterId = Dh.CostCenterId;
                        StockViewModel.Qty_Iss = svm.Qty + (svm.FreeQty ?? 0);
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = svm.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = svm.Specification;
                        StockViewModel.Dimension1Id = svm.Dimension1Id;
                        StockViewModel.Dimension2Id = svm.Dimension2Id;
                        StockViewModel.Remark = svm.Remark;
                        StockViewModel.Status = Dh.Status;
                        StockViewModel.CreatedBy = Dl.CreatedBy;
                        StockViewModel.CreatedDate = Dl.CreatedDate;
                        StockViewModel.ModifiedBy = User.Identity.Name;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }
                    }




                    Pl.ProductUidId = svm.ProductUidId;
                    Pl.ProductId = svm.ProductId;
                    Pl.SaleOrderLineId = svm.SaleOrderLineId;
                    Pl.Qty = svm.Qty;
                    Pl.FreeQty = svm.FreeQty;
                    Pl.BaleNo = svm.BaleNo;
                    Pl.DealUnitId = svm.DealUnitId;
                    Pl.DealQty = svm.DealQty;
                    Pl.Remark = svm.Remark;
                    Pl.Specification = svm.Specification;
                    Pl.Dimension1Id = svm.Dimension1Id;
                    Pl.Dimension2Id = svm.Dimension2Id;
                    Pl.LotNo = svm.LotNo;
                    _PackingLineService.Update(Pl);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = Pl,
                    });



                    Dl.GodownId = svm.GodownId;
                    Dl.Remark = svm.Remark;
                    _SaleDispatchLineService.Update(Dl);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecD,
                        Obj = Dl,
                    });


                    Sl.Dimension1Id = svm.Dimension1Id;
                    Sl.Dimension2Id = svm.Dimension2Id;
                    Sl.ProductId = svm.ProductId;
                    Sl.DiscountPer = svm.DiscountPer;
                    Sl.DiscountAmount = svm.DiscountAmount;
                    Sl.PromoCodeId = svm.PromoCodeId;
                    Sl.Qty = svm.Qty;
                    Sl.Amount = svm.Amount;
                    Sl.Weight = svm.Weight;
                    Sl.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    Sl.DealQty = svm.DealQty;
                    Sl.DealUnitId = svm.DealUnitId;
                    Sl.Rate = svm.Rate;
                    Sl.SalesTaxGroupProductId = svm.SalesTaxGroupProductId;
                    Sl.Remark = svm.Remark;
                    Sl.ModifiedDate = DateTime.Now;
                    Sl.ModifiedBy = User.Identity.Name;
                    Sl.ObjectState = Model.ObjectState.Modified;
                    _SaleInvoiceLineService.Update(Sl);

                    Sid.RewardPoints = svm.RewardPoints;
                    _SaleInvoiceLineDetailService.Update(Sid);

                    if (Pl.SaleOrderLineId != null)
                    {
                        new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyOnInvoice((int)Pl.SaleOrderLineId, Sl.SaleInvoiceLineId, Sh.DocDate, Pl.Qty);
                    }


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecS,
                        Obj = Sl,
                    });


                    if (Sh.Status != (int)StatusConstants.Drafted)
                    {
                        Sh.Status = (int)StatusConstants.Modified;
                        Dh.Status = (int)StatusConstants.Modified;
                        Ph.Status = (int)StatusConstants.Modified;
                        new SaleInvoiceHeaderService(_unitOfWork).Update(Sh);
                        new SaleDispatchHeaderService(_unitOfWork).Update(Dh);
                        new PackingHeaderService(_unitOfWork).Update(Ph);
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
                        if (svm.SaleOrderLineId.HasValue && svm.SaleOrderLineId.Value > 0)
                            return PartialView("_CreateForSaleOrder", svm);
                        else
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

            var ModelStateErrorList = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();

            if (ModelStateErrorList.Count > 0)
            {
                foreach (var ModelStateError in ModelStateErrorList)
                {
                    foreach (var Error in ModelStateError)
                    {
                        ModelState.AddModelError("", Error.ErrorMessage);
                    }
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

            DirectSaleInvoiceLineViewModel vm = _SaleInvoiceLineService.GetDirectSaleInvoiceLineForEdit(id);
            //Getting Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            if (temp.SaleOrderLineId.HasValue && temp.SaleOrderLineId.Value > 0)
            {
                vm.IsSaleBased = true;
                return PartialView("_CreateForSaleOrder", vm);

            }
            else
            {
                vm.SaleToBuyerId = H.SaleToBuyerId;
                vm.DocDate = H.DocDate;
                vm.IsSaleBased = false;
                return PartialView("_Create", vm);
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

            if (temp.SaleOrderLineId.HasValue && temp.SaleOrderLineId.Value > 0)
            {
                vm.IsSaleBased = true;
                return PartialView("_CreateForSaleOrder", vm);

            }
            else
            {
                vm.IsSaleBased = false;
                return PartialView("_Create", vm);
            }
        }

        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DeletePost(DirectSaleInvoiceLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (vm.linecharges != null && vm.footercharges == null)
            {
                ModelState.AddModelError("", "Something went wrong while deletion.Please try again.");
                PrepareViewBag();
                ViewBag.LineMode = "Delete";
                return PartialView("_Create", vm);
            }

            int? StockId = 0;

            SaleInvoiceHeader Sh = new SaleInvoiceHeaderService(_unitOfWork).FindDirectSaleInvoice(vm.SaleInvoiceHeaderId);

            SaleDispatchHeader Dh = new SaleDispatchHeaderService(_unitOfWork).Find(Sh.SaleDispatchHeaderId.Value);

            PackingHeader Ph = new PackingHeaderService(_unitOfWork).Find(Dh.PackingHeaderId.Value);

            int status = Sh.Status;

            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(Sh.DocTypeId, Sh.DivisionId, Sh.SiteId);

            PackingLine Pl = _PackingLineService.Find(vm.PackingLineId.Value);

            SaleDispatchLine Dl = _SaleDispatchLineService.Find(vm.SaleDispatchLineId);

            SaleInvoiceLine Sl = _SaleInvoiceLineService.Find(vm.SaleInvoiceLineId);

            SaleInvoiceLineDetail Sid = _SaleInvoiceLineDetailService.Find(vm.SaleInvoiceLineId);


            if (vm.ProductUidId != null && vm.ProductUidId != 0)
            {
                ProductUid ProductUid = (from p in db.ProductUid
                                         where p.ProductUIDId == vm.ProductUidId
                                         select p).FirstOrDefault();

                if (!(Pl.ProductUidLastTransactionDocNo == ProductUid.LastTransactionDocNo && Pl.ProductUidLastTransactionDocTypeId == ProductUid.LastTransactionDocTypeId))
                {
                    if (settings.DoNotUpdateProductUidStatus ?? false == false)
                    {
                        if ((Ph.DocNo != ProductUid.LastTransactionDocNo || Ph.DocTypeId != ProductUid.LastTransactionDocTypeId))
                        {
                            ModelState.AddModelError("", "Bar Code Can't be deleted because this is already Proceed to another process.");
                            PrepareViewBag();
                            return PartialView("_Create", vm);
                        }


                        ProductUid.LastTransactionDocDate = Pl.ProductUidLastTransactionDocDate;
                        ProductUid.LastTransactionDocId = Pl.ProductUidLastTransactionDocId;
                        ProductUid.LastTransactionDocNo = Pl.ProductUidLastTransactionDocNo;
                        ProductUid.LastTransactionDocTypeId = Pl.ProductUidLastTransactionDocTypeId;
                        ProductUid.LastTransactionPersonId = Pl.ProductUidLastTransactionPersonId;
                        ProductUid.CurrenctGodownId = Pl.ProductUidCurrentGodownId;
                        ProductUid.CurrenctProcessId = Pl.ProductUidCurrentProcessId;
                        ProductUid.Status = Pl.ProductUidStatus;
                        ProductUid.ObjectState = Model.ObjectState.Modified;

                        db.ProductUid.Add(ProductUid);
                    }
                }
            }

            LogList.Add(new LogTypeViewModel
            {
                ExObj = Pl,
            });

            LogList.Add(new LogTypeViewModel
            {
                ExObj = Dl,
            });

            LogList.Add(new LogTypeViewModel
            {
                ExObj = Sl,
            });


            StockId = Dl.StockId;

            if (Pl.SaleOrderLineId != null)
            {
                new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyOnInvoice((int)Pl.SaleOrderLineId, Sid.SaleInvoiceLineId, Sh.DocDate, 0);
            }
            _SaleInvoiceLineDetailService.Delete(Sid);
            _SaleInvoiceLineService.Delete(Sl);
            _SaleDispatchLineService.Delete(Dl);

            if (Pl.PackingHeaderId == Dh.PackingHeaderId)
            {
                _PackingLineService.Delete(Pl);
            }

            if (StockId != null)
            {
                StockAdj Adj = (from L in db.StockAdj
                                where L.StockOutId == StockId
                                select L).FirstOrDefault();

                if (Adj != null)
                {
                    //new StockAdjService(_unitOfWork).Delete(Adj);
                    Adj.ObjectState = Model.ObjectState.Deleted;
                    new StockAdjService(_unitOfWork).Delete(Adj.StockAdjId);
                }

                new StockService(_unitOfWork).DeleteStock((int)StockId);
            }


            if (Sh.Status != (int)StatusConstants.Drafted)
            {
                Sh.Status = (int)StatusConstants.Modified;
                Dh.Status = (int)StatusConstants.Modified;
                Ph.Status = (int)StatusConstants.Modified;
                new SaleInvoiceHeaderService(_unitOfWork).Update(Sh);
                new SaleDispatchHeaderService(_unitOfWork).Update(Dh);
                new PackingHeaderService(_unitOfWork).Update(Ph);
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
            if (temp.SaleOrderLineId.HasValue && temp.SaleOrderLineId.Value > 0)
            {
                vm.IsSaleBased = true;
                return PartialView("_CreateForSaleOrder", vm);
            }
            else
            {
                vm.IsSaleBased = false;
                return PartialView("_Create", vm);
            }

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


            return Json(new { ProductId = product.ProductId, StandardCost = product.StandardCost, SaleRate = product.SaleRate, UnitId = product.UnitId, UnitName = product.UnitName, DealUnitId = (DealUnitId == null) ? product.UnitId : DealUnitId.DealUnitId, DealUnitDecimalPlaces = DlUnit.DecimalPlaces, Specification = product.ProductSpecification, ProductCode = product.ProductCode, ProductName = product.ProductName, Dimension1Id = product.DefaultDimension1Id, Dimension1Name = product.DefaultDimension1Name,
                              Dimension2Id = product.DefaultDimension2Id,
                              Dimension2Name = product.DefaultDimension2Name,
                              Dimension3Id = product.DefaultDimension3Id,
                              Dimension3Name = product.DefaultDimension3Name,
                              Dimension4Id = product.DefaultDimension4Id,
                              Dimension4Name = product.DefaultDimension4Name,
                              SalesTaxGroupProductId = product.SalesTaxGroupProductId,
                              SalesTaxGroupProductName = product.SalesTaxGroupProductName
            });
        }

        public JsonResult GetSaleOrders(int id, string term, int Limit)
        {
            return Json(_SaleInvoiceLineService.GetPendingOrdersForInvoice(id, term, Limit).ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOrderDetail(int OrderId)
        {
            return Json(new SaleOrderLineService(_unitOfWork).GetSaleOrderDetailForInvoice(OrderId));
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

        public JsonResult GetSaleOrderDetailJson(int SaleOrderLineId)
        {
            var temp = (from L in db.ViewSaleOrderBalance
                        where L.SaleOrderLineId == SaleOrderLineId
                        select new
                        {
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

        public JsonResult GetPendingSaleOrderCount(int ProductId, int BuyerId)
        {
            var temp = (from L in db.ViewSaleOrderBalance
                        where L.ProductId == ProductId && L.BuyerId == BuyerId
                        group new { L } by new { L.SaleOrderLineId } into Result
                        select new
                        {
                            Cnt = Result.Count()
                        }).FirstOrDefault();

            if (temp != null)
            {
                return Json(temp.Cnt);
            }
            else
            {
                return null;
            }
        }



        //public JsonResult GetCustomProducts(int id, string term, int Limit)//Indent Header ID
        //{
        //    return Json(_SaleInvoiceLineService.GetProductHelpList(id, term, Limit), JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult GetSaleOrderForProduct(int id, int ProductId, string term, int Limit)//Indent Header ID
        //{
        //    return Json(_SaleInvoiceLineService.GetSaleOrderHelpListForProduct(id, ProductId, term, Limit), JsonRequestBehavior.AllowGet);
        //}

        public JsonResult SetSingleSaleOrderLine(int Ids)
        {
            ComboBoxResult SaleOrderJson = new ComboBoxResult();

            var SaleOrderLine = from L in db.SaleOrderLine
                                join H in db.SaleOrderHeader on L.SaleOrderHeaderId equals H.SaleOrderHeaderId into SaleOrderHeaderTable
                                from SaleOrderHeaderTab in SaleOrderHeaderTable.DefaultIfEmpty()
                                where L.SaleOrderLineId == Ids
                                select new
                                {
                                    SaleOrderLineId = L.SaleOrderLineId,
                                    SaleOrderNo = SaleOrderHeaderTab.BuyerOrderNo
                                };

            SaleOrderJson.id = SaleOrderLine.FirstOrDefault().ToString();
            SaleOrderJson.text = SaleOrderLine.FirstOrDefault().SaleOrderNo;

            return Json(SaleOrderJson);
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

        public ActionResult GetSaleOrderForProduct(string searchTerm, int pageSize, int pageNum, int filter, int PersonId)//DocTypeId
        {
            var Query = _SaleInvoiceLineService.GetSaleOrderHelpListForProduct(filter, PersonId, searchTerm);
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

        //public JsonResult GetPromoCodeIdListJson(int ProductId, int BuyerId, DateTime DocDate)
        //{
        //    List<PromoCodeList> saleorderlistforproductjson = _SaleInvoiceLineService.FGetPromoCodeList(ProductId, BuyerId, DocDate).ToList();
        //    return Json(saleorderlistforproductjson);
        //}

        public ActionResult GetPromoCodeIdList(string searchTerm, int pageSize, int pageNum, int ProductId, int BuyerId, DateTime DocDate)
        {
            List<ComboBoxResult> saleorderlistforproductjson = _SaleInvoiceLineService.FGetPromoCodeList(ProductId, BuyerId, DocDate).ToList();


            var count = saleorderlistforproductjson.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = saleorderlistforproductjson;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult GetPromoCodeDicount(int PromoCodeId)
        {
            PromoCode PromoCode = new PromoCodeService(_unitOfWork).Find(PromoCodeId);
            return Json(PromoCode);
        }

        public JsonResult GetFirstPromoCode(int ProductId, int BuyerId, DateTime DocDate)
        {
            List<ComboBoxResult> PromoCode = _SaleInvoiceLineService.FGetPromoCodeList(ProductId, BuyerId, DocDate).ToList();
            return Json(PromoCode);
        }

        public ActionResult GetProductUidHelpList(string searchTerm, int pageSize, int pageNum, int filter)//SaleInvoiceHeaderId
        {
            List<ComboBoxResult> ProductUidJson = _SaleInvoiceLineService.FGetProductUidHelpList(filter, searchTerm).ToList();

            var count = ProductUidJson.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = ProductUidJson;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProductUid(string  Ids)
        {
            ComboBoxResult ProductUidJson = new ComboBoxResult();

            var ProductUid = from L in db.ProductUid
                                where L.ProductUidName == Ids
                                select new
                                {
                                    id = L.ProductUidName,
                                    text = L.ProductUidName
                                };

            ProductUidJson.id = ProductUid.FirstOrDefault().id;
            ProductUidJson.text = ProductUid.FirstOrDefault().text;

            return Json(ProductUidJson);
        }
    }
}
