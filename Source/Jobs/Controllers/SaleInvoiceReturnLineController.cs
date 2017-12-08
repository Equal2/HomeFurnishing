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
using Jobs.Helpers;

namespace Jobs.Controllers
{

    [Authorize]
    public class SaleInvoiceReturnLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleInvoiceReturnLineService _SaleInvoiceReturnLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public SaleInvoiceReturnLineController(ISaleInvoiceReturnLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleInvoiceReturnLineService = SaleOrder;
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
            var p = _SaleInvoiceReturnLineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        public ActionResult _ForInvoice(int id, int sid)
        {
            SaleInvoiceReturnLineFilterViewModel vm = new SaleInvoiceReturnLineFilterViewModel();
            vm.SaleInvoiceReturnHeaderId = id;
            vm.BuyerId = sid;
            SaleInvoiceReturnHeader Header = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_Filters", vm);
        }

        public ActionResult _ForReceipt(int id, int sid)
        {
            SaleInvoiceReturnLineFilterViewModel vm = new SaleInvoiceReturnLineFilterViewModel();
            vm.SaleInvoiceReturnHeaderId = id;
            vm.BuyerId = sid;
            SaleInvoiceReturnHeader Header = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_ReceiptFilters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(SaleInvoiceReturnLineFilterViewModel vm)
        {
            List<SaleInvoiceReturnLineViewModel> temp = _SaleInvoiceReturnLineService.GetSaleInvoiceForFilters(vm).ToList();
            SaleInvoiceReturnMasterDetailModel svm = new SaleInvoiceReturnMasterDetailModel();
            svm.SaleInvoiceReturnLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostReceipts(SaleInvoiceReturnLineFilterViewModel vm)
        {
            List<SaleInvoiceReturnLineViewModel> temp = _SaleInvoiceReturnLineService.GetSaleReceiptForFilters(vm).ToList();
            SaleInvoiceReturnMasterDetailModel svm = new SaleInvoiceReturnMasterDetailModel();
            svm.SaleInvoiceReturnLineViewModel = temp;
            return PartialView("_Results", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(SaleInvoiceReturnMasterDetailModel vm)
        {
            int Cnt = 0;

            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            int Gpk = 0;
            int Serial = _SaleInvoiceReturnLineService.GetMaxSr(vm.SaleInvoiceReturnLineViewModel.FirstOrDefault().SaleInvoiceReturnHeaderId);
            bool HeaderChargeEdit = false;

            SaleInvoiceReturnHeader Header = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(vm.SaleInvoiceReturnLineViewModel.FirstOrDefault().SaleInvoiceReturnHeaderId);

            SaleInvoiceSetting Settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new SaleInvoiceReturnLineChargeService(_unitOfWork).GetMaxProductCharge(Header.SaleInvoiceReturnHeaderId, "Web.SaleInvoiceReturnLines", "SaleInvoiceReturnHeaderId", "SaleInvoiceReturnLineId");

            int PersonCount = 0;
            int CalculationId = Settings.CalculationId;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            SaleDispatchReturnHeader GoodsRetHeader = new SaleDispatchReturnHeaderService(_unitOfWork).Find(Header.SaleDispatchReturnHeaderId ?? 0);

            if (ModelState.IsValid)
            {
                foreach (var item in vm.SaleInvoiceReturnLineViewModel)
                {
                    decimal balqty = (from p in db.ViewSaleInvoiceBalance
                                      where p.SaleInvoiceLineId == item.SaleInvoiceLineId
                                      select p.BalanceQty).FirstOrDefault();


                    if (item.Qty > 0 && item.Qty <= balqty)
                    {
                        SaleInvoiceReturnLine line = new SaleInvoiceReturnLine();
                        //var receipt = new SaleDispatchLineService(_unitOfWork).Find(item.SaleDispatchLineId );


                        line.SaleInvoiceReturnHeaderId = item.SaleInvoiceReturnHeaderId;
                        line.SaleInvoiceLineId = item.SaleInvoiceLineId;
                        line.Qty = item.Qty;
                        line.Sr = Serial++;
                        line.DiscountPer = item.DiscountPer;
                        line.DiscountAmount = item.DiscountAmount;
                        line.Rate = item.Rate;
                        line.DealQty = item.UnitConversionMultiplier * item.Qty;
                        line.DealUnitId = item.DealUnitId;
                        line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        line.Amount = item.RateAfterDiscount * line.DealQty;

                        line.Remark = item.Remark;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.SaleInvoiceReturnLineId = pk;


                        SaleDispatchReturnLine GLine = Mapper.Map<SaleInvoiceReturnLine, SaleDispatchReturnLine>(line);
                        GLine.SaleDispatchLineId = new SaleInvoiceLineService(_unitOfWork).Find(line.SaleInvoiceLineId).SaleDispatchLineId;
                        GLine.SaleDispatchReturnHeaderId = GoodsRetHeader.SaleDispatchReturnHeaderId;
                        GLine.SaleDispatchReturnLineId = Gpk;
                        GLine.Qty = line.Qty;
                        GLine.ObjectState = Model.ObjectState.Added;


                        SaleDispatchLine SaleDispatchLine = new SaleDispatchLineService(_unitOfWork).Find(GLine.SaleDispatchLineId);
                        PackingLine PackingLin = new PackingLineService(_unitOfWork).Find(SaleDispatchLine.PackingLineId);

                        StockViewModel StockViewModel = new StockViewModel();


                        if (Cnt == 0)
                        {
                            StockViewModel.StockHeaderId = GoodsRetHeader.StockHeaderId ?? 0;
                        }
                        else
                        {
                            if (GoodsRetHeader.StockHeaderId != null && GoodsRetHeader.StockHeaderId != 0)
                            {
                                StockViewModel.StockHeaderId = (int)GoodsRetHeader.StockHeaderId;
                            }
                            else
                            {
                                StockViewModel.StockHeaderId = -1;
                            }

                        }

                        StockViewModel.StockId = -Cnt;

                        StockViewModel.DocHeaderId = GoodsRetHeader.SaleDispatchReturnHeaderId;
                        StockViewModel.DocLineId = SaleDispatchLine.SaleDispatchLineId;
                        StockViewModel.DocTypeId = GoodsRetHeader.DocTypeId;
                        StockViewModel.StockHeaderDocDate = GoodsRetHeader.DocDate;
                        StockViewModel.StockDocDate = GoodsRetHeader.DocDate;
                        StockViewModel.DocNo = GoodsRetHeader.DocNo;
                        StockViewModel.DivisionId = GoodsRetHeader.DivisionId;
                        StockViewModel.SiteId = GoodsRetHeader.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = GoodsRetHeader.BuyerId;
                        StockViewModel.ProductId = PackingLin.ProductId;
                        StockViewModel.ProductUidId = PackingLin.ProductUidId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = GoodsRetHeader.GodownId;
                        StockViewModel.HeaderProcessId = Settings.ProcessId;
                        StockViewModel.GodownId = GoodsRetHeader.GodownId;
                        StockViewModel.Remark = Header.Remark;
                        StockViewModel.Status = Header.Status;
                        StockViewModel.ProcessId = null;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = GLine.Qty;
                        StockViewModel.Rate = null;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = PackingLin.Specification;
                        StockViewModel.Dimension1Id = PackingLin.Dimension1Id;
                        StockViewModel.Dimension2Id = PackingLin.Dimension2Id;
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
                            GoodsRetHeader.StockHeaderId = StockViewModel.StockHeaderId;
                        }


                        GLine.StockId = StockViewModel.StockId;


                        new SaleDispatchReturnLineService(_unitOfWork).Create(GLine);

                        line.SaleDispatchReturnLineId = GLine.SaleDispatchReturnLineId;
                        line.ObjectState = Model.ObjectState.Added;
                        _SaleInvoiceReturnLineService.Create(line);

                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.SaleInvoiceReturnLineId, HeaderTableId = item.SaleInvoiceReturnHeaderId, PersonID = Header.BuyerId, DealQty = line.DealQty });
                        Gpk++;
                        pk++;

                        Cnt = Cnt + 1;
                    }
                }

                new SaleDispatchReturnHeaderService(_unitOfWork).Update(GoodsRetHeader);

                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, vm.SaleInvoiceReturnLineViewModel.FirstOrDefault().SaleInvoiceReturnHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.SaleInvoiceReturnHeaderCharges", "Web.SaleInvoiceReturnLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                // Saving Charges
                foreach (var item in LineCharges)
                {

                    SaleInvoiceReturnLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, SaleInvoiceReturnLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    new SaleInvoiceReturnLineChargeService(_unitOfWork).Create(PoLineCharge);

                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        SaleInvoiceReturnHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, SaleInvoiceReturnHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.SaleInvoiceReturnLineViewModel.FirstOrDefault().SaleInvoiceReturnHeaderId;
                        POHeaderCharge.PersonID = Header.BuyerId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        new SaleInvoiceReturnHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new SaleInvoiceReturnHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        new SaleInvoiceReturnHeaderChargeService(_unitOfWork).Update(footercharge);
                    }

                }

                try
                {
                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Results", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.SaleInvoiceReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }



        private void PrepareViewBag(SaleInvoiceReturnLineViewModel vm)
        {
            //ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        [HttpGet]
        public ActionResult CreateLine(int id, int sid)
        {
            return _Create(id, sid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, int sid)
        {
            return _Create(id, sid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int id, int sid)
        {
            return _Create(id, sid);
        }
        public ActionResult _Create(int Id, int sid) //Id ==>Sale Order Header Id
        {
            SaleInvoiceReturnHeader H = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(Id);
            SaleInvoiceReturnLineViewModel s = new SaleInvoiceReturnLineViewModel();

            //Getting Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.Nature = H.Nature;

            s.SaleInvoiceReturnHeaderId = H.SaleInvoiceReturnHeaderId;
            s.SaleInvoiceReturnHeaderDocNo = H.DocNo;
            s.BuyerId = sid;
            s.DocTypeId = H.DocTypeId;
            s.DivisionId = H.DivisionId;
            s.SiteId = H.SiteId;
            ViewBag.LineMode = "Create";
            //PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult _CreatePost(SaleInvoiceReturnLineViewModel svm)
        {
            SaleInvoiceReturnHeader temp = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(svm.SaleInvoiceReturnHeaderId);
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            if (svm.SaleInvoiceReturnLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.SaleInvoiceReturnLineId <= 0)
            {


                decimal balqty = (from p in db.SaleInvoiceLine
                                  where p.SaleInvoiceLineId == svm.SaleInvoiceLineId
                                  select p.DealQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Invoice Qty");
                }
                if (svm.Qty == 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }

                if (svm.SaleInvoiceLineId <= 0)
                {
                    ModelState.AddModelError("SaleInvoiceLineId", "Sale Invoice field is required");
                }

                if (ModelState.IsValid)
                {
                    SaleInvoiceReturnLine s = Mapper.Map<SaleInvoiceReturnLineViewModel, SaleInvoiceReturnLine>(svm);
                    s.Sr = _SaleInvoiceReturnLineService.GetMaxSr(s.SaleInvoiceReturnHeaderId);
                    s.DiscountPer = svm.DiscountPer;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;


                    if (temp.SaleDispatchReturnHeaderId.HasValue)
                    {
                        SaleDispatchReturnHeader SaleDispatchReturnHeader = new SaleDispatchReturnHeaderService(_unitOfWork).Find((int)temp.SaleDispatchReturnHeaderId);

                        SaleDispatchReturnLine Gline = Mapper.Map<SaleInvoiceReturnLine, SaleDispatchReturnLine>(s);
                        Gline.SaleDispatchLineId = new SaleInvoiceLineService(_unitOfWork).Find(s.SaleInvoiceLineId).SaleDispatchLineId;
                        Gline.SaleDispatchReturnHeaderId = temp.SaleDispatchReturnHeaderId ?? 0;
                        Gline.Qty = svm.Qty;
                        Gline.Weight = svm.Weight;




                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = SaleDispatchReturnHeader.StockHeaderId ?? 0;
                        StockViewModel.DocHeaderId = SaleDispatchReturnHeader.SaleDispatchReturnHeaderId;
                        StockViewModel.DocLineId = Gline.SaleDispatchReturnLineId;
                        StockViewModel.DocTypeId = SaleDispatchReturnHeader.DocTypeId;
                        StockViewModel.StockHeaderDocDate = SaleDispatchReturnHeader.DocDate;
                        StockViewModel.StockDocDate = SaleDispatchReturnHeader.DocDate;
                        StockViewModel.DocNo = SaleDispatchReturnHeader.DocNo;
                        StockViewModel.DivisionId = SaleDispatchReturnHeader.DivisionId;
                        StockViewModel.SiteId = SaleDispatchReturnHeader.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = settings.ProcessId;
                        StockViewModel.PersonId = SaleDispatchReturnHeader.BuyerId;
                        StockViewModel.ProductId = svm.ProductId;
                        StockViewModel.ProductUidId = svm.ProductUidId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = SaleDispatchReturnHeader.GodownId;
                        StockViewModel.GodownId = SaleDispatchReturnHeader.GodownId;
                        StockViewModel.ProcessId = null;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = Gline.Qty;
                        StockViewModel.Weight_Iss = Gline.Weight;
                        StockViewModel.Weight_Rec = 0;
                        StockViewModel.Rate = null;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = svm.Specification;
                        StockViewModel.Dimension1Id = svm.Dimension1Id;
                        StockViewModel.Dimension2Id = svm.Dimension2Id;
                        StockViewModel.Remark = svm.Remark;
                        StockViewModel.Status = SaleDispatchReturnHeader.Status;
                        StockViewModel.CreatedBy = SaleDispatchReturnHeader.CreatedBy;
                        StockViewModel.CreatedDate = DateTime.Now;
                        StockViewModel.ModifiedBy = SaleDispatchReturnHeader.ModifiedBy;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }

                        Gline.StockId = StockViewModel.StockId;

                        Gline.ObjectState = Model.ObjectState.Added;
                        new SaleDispatchReturnLineService(_unitOfWork).Create(Gline);

                        s.SaleDispatchReturnLineId = Gline.SaleDispatchReturnLineId;

                        if (StockViewModel != null)
                        {
                            if (SaleDispatchReturnHeader.StockHeaderId == null)
                            {
                                SaleDispatchReturnHeader.StockHeaderId = StockViewModel.StockHeaderId;
                            }

                        }

                        if (SaleDispatchReturnHeader.Status != (int)StatusConstants.Drafted)
                        {
                            SaleDispatchReturnHeader.Status = (int)StatusConstants.Modified;
                        }
                        new SaleDispatchReturnHeaderService(_unitOfWork).Update(SaleDispatchReturnHeader);
                    }
                    s.ObjectState = Model.ObjectState.Added;
                    _SaleInvoiceReturnLineService.Create(s);


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            item.LineTableId = s.SaleInvoiceReturnLineId;
                            item.PersonID = temp.BuyerId;
                            item.HeaderTableId = s.SaleInvoiceReturnHeaderId;
                            item.ObjectState = Model.ObjectState.Added;
                            new SaleInvoiceReturnLineChargeService(_unitOfWork).Create(item);
                        }

                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {

                            if (item.Id > 0)
                            {

                                var footercharge = new SaleInvoiceReturnHeaderChargeService(_unitOfWork).Find(item.Id);
                                footercharge.Rate = item.Rate;
                                footercharge.Amount = item.Amount;
                                new SaleInvoiceReturnHeaderChargeService(_unitOfWork).Update(footercharge);

                            }

                            else
                            {
                                item.HeaderTableId = s.SaleInvoiceReturnHeaderId;
                                item.PersonID = temp.BuyerId;
                                item.ObjectState = Model.ObjectState.Added;
                                new SaleInvoiceReturnHeaderChargeService(_unitOfWork).Create(item);
                            }
                        }


                    SaleInvoiceReturnHeader temp2 = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(s.SaleInvoiceReturnHeaderId);
                    if (temp2.Status != (int)StatusConstants.Drafted)
                    {
                        temp2.Status = (int)StatusConstants.Modified;
                    }




                    new SaleInvoiceReturnHeaderService(_unitOfWork).Update(temp2);
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

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleInvoiceReturnHeaderId,
                        DocLineId = s.SaleInvoiceReturnLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));


                    return RedirectToAction("_Create", new { id = s.SaleInvoiceReturnHeaderId, sid = svm.BuyerId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                SaleInvoiceReturnLine line = _SaleInvoiceReturnLineService.Find(svm.SaleInvoiceReturnLineId);

                SaleInvoiceReturnLine ExRec = new SaleInvoiceReturnLine();
                ExRec = Mapper.Map<SaleInvoiceReturnLine>(line);



                decimal balqty = (from p in db.SaleInvoiceLine
                                  where p.SaleInvoiceLineId == svm.SaleInvoiceLineId
                                  select p.DealQty).FirstOrDefault();
                if (balqty + line.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Invoice Qty");
                }


                if (ModelState.IsValid)
                {
                    if (svm.Qty > 0)
                    {
                        line.DiscountPer = svm.DiscountPer;
                        line.Remark = svm.Remark;
                        line.Qty = svm.Qty;
                        line.Weight  = svm.Weight;
                        line.DealQty = svm.DealQty;
                        line.Amount = svm.Amount;
                        line.ModifiedBy = User.Identity.Name;
                        line.ModifiedDate = DateTime.Now;
                    }

                    _SaleInvoiceReturnLineService.Update(line);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = line,
                    });



                    if (temp.SaleDispatchReturnHeaderId.HasValue)
                    {
                        SaleDispatchReturnHeader SaleDispatchReturnHeader = new SaleDispatchReturnHeaderService(_unitOfWork).Find((int)temp.SaleDispatchReturnHeaderId);

                        SaleDispatchReturnLine GLine = new SaleDispatchReturnLineService(_unitOfWork).Find(line.SaleDispatchReturnLineId ?? 0);

                        SaleDispatchReturnLine ExRecR = new SaleDispatchReturnLine();
                        ExRecR = Mapper.Map<SaleDispatchReturnLine>(GLine);


                        GLine.Remark = line.Remark;
                        GLine.Qty = line.Qty;
                        GLine.DealQty = line.DealQty;
                        GLine.Weight = line.Weight;

                        GLine.ObjectState = Model.ObjectState.Modified;
                        new SaleDispatchReturnLineService(_unitOfWork).Update(GLine);

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = ExRecR,
                            Obj = GLine,
                        });


                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = SaleDispatchReturnHeader.StockHeaderId ?? 0;
                        StockViewModel.StockId = GLine.StockId ?? 0;
                        StockViewModel.DocHeaderId = SaleDispatchReturnHeader.SaleDispatchReturnHeaderId;
                        StockViewModel.DocLineId = GLine.SaleDispatchLineId;
                        StockViewModel.DocTypeId = SaleDispatchReturnHeader.DocTypeId;
                        StockViewModel.StockHeaderDocDate = SaleDispatchReturnHeader.DocDate;
                        StockViewModel.StockDocDate = SaleDispatchReturnHeader.DocDate;
                        StockViewModel.DocNo = SaleDispatchReturnHeader.DocNo;
                        StockViewModel.DivisionId = SaleDispatchReturnHeader.DivisionId;
                        StockViewModel.SiteId = SaleDispatchReturnHeader.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = settings.ProcessId;
                        StockViewModel.PersonId = SaleDispatchReturnHeader.BuyerId;
                        StockViewModel.ProductId = svm.ProductId;
                        StockViewModel.ProductUidId = svm.ProductUidId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = SaleDispatchReturnHeader.GodownId;
                        StockViewModel.GodownId = SaleDispatchReturnHeader.GodownId;
                        StockViewModel.ProcessId = null;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = svm.Qty;
                        StockViewModel.Weight_Iss = svm.Weight;
                        StockViewModel.Weight_Rec = 0;
                        StockViewModel.Rate = null;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = svm.Specification;
                        StockViewModel.Dimension1Id = svm.Dimension1Id;
                        StockViewModel.Dimension2Id = svm.Dimension2Id;
                        StockViewModel.Remark = svm.Remark;
                        StockViewModel.Status = SaleDispatchReturnHeader.Status;
                        StockViewModel.CreatedBy = SaleDispatchReturnHeader.CreatedBy;
                        StockViewModel.CreatedDate = SaleDispatchReturnHeader.CreatedDate;
                        StockViewModel.ModifiedBy = User.Identity.Name;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }

                        if (SaleDispatchReturnHeader.Status != (int)StatusConstants.Drafted)
                        {
                            SaleDispatchReturnHeader.Status = (int)StatusConstants.Modified;
                            new SaleDispatchReturnHeaderService(_unitOfWork).Update(SaleDispatchReturnHeader);
                        }
                    }

                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        new SaleInvoiceReturnHeaderService(_unitOfWork).Update(temp);
                    }



                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = new SaleInvoiceReturnLineChargeService(_unitOfWork).Find(item.Id);

                            SaleInvoiceReturnLineCharge ExRecLine = new SaleInvoiceReturnLineCharge();
                            ExRecLine = Mapper.Map<SaleInvoiceReturnLineCharge>(productcharge);
                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            productcharge.DealQty = item.DealQty;
                            new SaleInvoiceReturnLineChargeService(_unitOfWork).Update(productcharge);

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecLine,
                                Obj = productcharge,
                            });

                        }


                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = new SaleInvoiceReturnHeaderChargeService(_unitOfWork).Find(item.Id);

                            SaleInvoiceReturnHeaderCharge ExRecLine = new SaleInvoiceReturnHeaderCharge();
                            ExRecLine = Mapper.Map<SaleInvoiceReturnHeaderCharge>(footercharge);
                            footercharge.Rate = item.Rate;
                            footercharge.Amount = item.Amount;
                            new SaleInvoiceReturnHeaderChargeService(_unitOfWork).Update(footercharge);

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
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", svm);
                    }

                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = line.SaleInvoiceReturnHeaderId,
                        DocLineId = line.SaleInvoiceReturnLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    //End of Saving the Activity Log


                    return Json(new { success = true });
                }
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
            SaleInvoiceReturnLineViewModel temp = _SaleInvoiceReturnLineService.GetSaleInvoiceReturnLine(id);
            SaleInvoiceReturnHeader H = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(temp.SaleInvoiceReturnHeaderId);

            PrepareViewBag(temp);

            //Getting Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            if (temp == null)
            {
                return HttpNotFound();
            }
            ViewBag.LineMode = "Edit";
            return PartialView("_Create", temp);
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
            SaleInvoiceReturnLineViewModel temp = _SaleInvoiceReturnLineService.GetSaleInvoiceReturnLine(id);

            PrepareViewBag(temp);

            SaleInvoiceReturnHeader H = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(temp.SaleInvoiceReturnHeaderId);

            PrepareViewBag(temp);

            //Getting Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            if (temp == null)
            {
                return HttpNotFound();
            }
            ViewBag.LineMode = "Delete";
            return PartialView("_Create", temp);
        }

        public ActionResult _Detail(int id)
        {
            SaleInvoiceReturnLineViewModel temp = _SaleInvoiceReturnLineService.GetSaleInvoiceReturnLine(id);

            PrepareViewBag(temp);

            SaleInvoiceReturnHeader H = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(temp.SaleInvoiceReturnHeaderId);

            PrepareViewBag(temp);

            //Getting Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);

            if (temp == null)
            {
                return HttpNotFound();
            }
            ViewBag.LineMode = "Detail";
            return PartialView("_Create", temp);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult DeletePost(SaleInvoiceReturnLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            int? StockId = 0;

            SaleInvoiceReturnLine SaleInvoiceReturnLine = _SaleInvoiceReturnLineService.Find(vm.SaleInvoiceReturnLineId);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = SaleInvoiceReturnLine,
            });

            int? SaleDispatchReturnLineId = SaleInvoiceReturnLine.SaleDispatchReturnLineId;



            var chargeslist = new SaleInvoiceReturnLineChargeService(_unitOfWork).GetCalculationProductList(vm.SaleInvoiceReturnLineId);

            if (chargeslist != null)
                foreach (var item in chargeslist)
                {
                    new SaleInvoiceReturnLineChargeService(_unitOfWork).Delete(item.Id);
                }

            if (vm.footercharges != null)
                foreach (var item in vm.footercharges)
                {
                    var footer = new SaleInvoiceReturnHeaderChargeService(_unitOfWork).Find(item.Id);
                    footer.Rate = item.Rate;
                    footer.Amount = item.Amount;
                    new SaleInvoiceReturnHeaderChargeService(_unitOfWork).Update(footer);
                }

            _SaleInvoiceReturnLineService.Delete(SaleInvoiceReturnLine.SaleInvoiceReturnLineId);
            SaleInvoiceReturnHeader header = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(SaleInvoiceReturnLine.SaleInvoiceReturnHeaderId);
            int status = header.Status;
            if (header.Status != (int)StatusConstants.Drafted)
            {
                header.Status = (int)StatusConstants.Modified;
                new SaleInvoiceReturnHeaderService(_unitOfWork).Update(header);

                if (header.SaleDispatchReturnHeaderId.HasValue)
                {
                    SaleDispatchReturnHeader DispatchHeader = new SaleDispatchReturnHeaderService(_unitOfWork).Find(header.SaleDispatchReturnHeaderId.Value);
                    DispatchHeader.Status = header.Status;
                    new SaleDispatchReturnHeaderService(_unitOfWork).Update(DispatchHeader);
                }
            }


            if (SaleDispatchReturnLineId != null)
            {
                SaleDispatchReturnLine Gline = new SaleDispatchReturnLineService(_unitOfWork).Find((int)SaleDispatchReturnLineId);

                if (Gline != null)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Gline,
                    });

                    StockId = Gline.StockId;
                    new SaleDispatchReturnLineService(_unitOfWork).Delete(Gline);

                    if (StockId != null)
                    {
                        new StockService(_unitOfWork).DeleteStock((int)StockId);
                    }
                }
            }
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

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = header.DocTypeId,
                DocId = header.SaleInvoiceReturnHeaderId,
                DocLineId = SaleInvoiceReturnLine.SaleInvoiceReturnLineId,
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
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult GetPendingInvoices(int ProductId, int SaleInvoiceReturnHeaderId)
        {
            return Json(new SaleInvoiceHeaderService(_unitOfWork).GetPendingInvoices(ProductId, SaleInvoiceReturnHeaderId).ToList());
        }

        public JsonResult GetPendingInvoicesWithterm(int ProductId, int SaleInvoiceReturnHeaderId, string term)
        {
            return Json(new SaleInvoiceHeaderService(_unitOfWork).GetPendingInvoicesWithterm(ProductId, SaleInvoiceReturnHeaderId, term).ToList());
        }

        public JsonResult GetInvoiceDetail(int InvoiceLineId)
        {
            return Json(new SaleInvoiceLineService(_unitOfWork).GetSaleInvoiceLineBalance(InvoiceLineId));
        }

        public JsonResult GetSaleInvoices(int id, string term)//Receipt Header ID
        {
            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_SaleInvoiceReturnLineService.GetPendingSaleInvoiceHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSaleDispatchs(int id, string term)//Receipt Header ID
        {
            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_SaleInvoiceReturnLineService.GetPendingSaleReceiptHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {
            return Json(_SaleInvoiceReturnLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSaleInvoiceForProduct(string searchTerm, int pageSize, int pageNum, int filter)//SaleInvoiceReturnHeaderId
        {
            var Query = _SaleInvoiceReturnLineService.GetSaleInvoiceHelpListForProduct(filter, searchTerm);
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


    }
}
