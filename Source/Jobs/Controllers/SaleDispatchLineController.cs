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
    public class SaleDispatchLineController : System.Web.Mvc.Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleDispatchLineService _SaleDispatchLineService;
        IPackingLineService _PackingLineService;
        IStockService _StockService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public SaleDispatchLineController(IStockService StockService, ISaleDispatchLineService SaleDispatch, IUnitOfWork unitOfWork, IExceptionHandlingService exec, IPackingLineService packLineServ)
        {
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
            //var p = _SaleDispatchLineService.GetSaleDispatchLineListForIndex(id).ToList();
            var p = _SaleDispatchLineService.GetSaleDispatchLineListForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        public ActionResult _Index(int id, int Status)
        {
            ViewBag.Status = Status;
            ViewBag.SaleDispatchHeaderId = id;
            var p = _SaleDispatchLineService.GetSaleDispatchLineListForIndex(id).ToList();
            return PartialView(p);
        }
        public ActionResult _ForOrder(int id)
        {
            SaleDispatchFilterViewModel vm = new SaleDispatchFilterViewModel();
            vm.SaleDispatchHeaderId = id;
            SaleDispatchHeader H = new SaleDispatchHeaderService(_unitOfWork).Find(id);
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            return PartialView("_OrderFilters", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostOrders(SaleDispatchFilterViewModel vm)
        {
            List<SaleDispatchLineViewModel> temp = _SaleDispatchLineService.GetSaleOrdersForFilters(vm).ToList();
            SaleDispatchListViewModel svm = new SaleDispatchListViewModel();
            svm.SaleDispatchLineViewModel = temp;
            return PartialView("_Results", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(SaleDispatchListViewModel vm)
        {
            int Cnt = 0;


            SaleDispatchHeader Dh = new SaleDispatchHeaderService(_unitOfWork).Find(vm.SaleDispatchLineViewModel.FirstOrDefault().SaleDispatchHeaderId);

            SaleDispatchSetting Settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(Dh.DocTypeId, Dh.DivisionId, Dh.SiteId);

            SaleDispatchLine LastRecord = _SaleDispatchLineService.GetSaleDispatchLineList(Dh.SaleDispatchHeaderId).OrderByDescending(m => m.SaleDispatchLineId).FirstOrDefault();
            if (LastRecord == null && Settings.GodownId == null)
            {
                TempData["CSEXCL"] += "Please insert a record before creating from multiple";
                return PartialView("_Results", vm);
            }
            PackingHeader Ph = new PackingHeaderService(_unitOfWork).Find(Dh.PackingHeaderId.Value);

            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            int PackingPrimaryKey = 0;
            int DispatchPrimaryKey = 0;

            //SaleDispatchHeader Header = new SaleDispatchHeaderService(_unitOfWork).Find(vm.SaleDispatchLineViewModel.FirstOrDefault().SaleDispatchHeaderId);

            


            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            if (ModelState.IsValid)
            {
                foreach (var item in vm.SaleDispatchLineViewModel)
                {
                    decimal balqty = (from p in db.ViewSaleOrderBalance
                                      where p.SaleOrderLineId == item.SaleOrderLineId
                                      select p.BalanceQty).FirstOrDefault();
                    if (item.Qty > 0 && item.Qty <= balqty)
                    {

                        PackingLine Pl = new PackingLine();
                        SaleDispatchLine Dl = new SaleDispatchLine();

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
                        if (LastRecord != null)
                        {
                            StockViewModel.GodownId = LastRecord.GodownId;
                        }
                        else
                        {
                            StockViewModel.GodownId = Settings.GodownId;
                        }
                        StockViewModel.Remark = Dh.Remark;
                        StockViewModel.Status = Dh.Status;
                        //StockViewModel.ProcessId = Dh.ProcessId;
                        StockViewModel.LotNo = null;
                        //StockViewModel.CostCenterId = Dh.CostCenterId;
                        StockViewModel.Qty_Iss = item.Qty + (item.LossQty ?? 0);
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = 0;
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
                        Pl.Remark = item.Remark;
                        Pl.SaleOrderLineId = item.SaleOrderLineId;
                        Pl.Specification = item.Specification;
                        Pl.PackingLineId = PackingPrimaryKey++;
                        Pl.ObjectState = Model.ObjectState.Added;
                        _PackingLineService.Create(Pl);





                        Dl.CreatedBy = User.Identity.Name;
                        Dl.ModifiedBy = User.Identity.Name;
                        Dl.CreatedDate = DateTime.Now;
                        Dl.ModifiedDate = DateTime.Now;

                        if (LastRecord != null)
                        {
                            Dl.GodownId = LastRecord.GodownId;
                        }
                        else
                        {
                            Dl.GodownId = Settings.GodownId;
                        }

                        Dl.PackingLineId = Pl.PackingLineId;
                        Dl.Remark = item.Remark;
                        Dl.SaleDispatchHeaderId = Dh.SaleDispatchHeaderId;
                        Dl.SaleDispatchLineId = DispatchPrimaryKey++;
                        Dl.ObjectState = Model.ObjectState.Added;
                        _SaleDispatchLineService.Create(Dl);



                        LineList.Add(new LineDetailListViewModel { Amount = 0, Rate = 0, LineTableId = Dl.SaleDispatchLineId, HeaderTableId = item.SaleDispatchHeaderId, PersonID = Dh.SaleToBuyerId });

                        pk++;
                        Cnt = Cnt + 1;
                    }
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
                    return PartialView("_Results", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Dh.DocTypeId,
                    DocId = Dh.SaleDispatchHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Dh.DocNo,
                    DocDate = Dh.DocDate,
                    DocStatus = Dh.Status,
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
            SaleDispatchHeader H = new SaleDispatchHeaderService(_unitOfWork).Find(Id);
            SaleDispatchLineViewModel s = new SaleDispatchLineViewModel();



            //Getting Settings
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);
            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);



            s.IsSaleBased = IsSaleBased;
            s.SaleDispatchHeaderId = H.SaleDispatchHeaderId;
            s.SaleDispatchHeaderDocNo = H.DocNo;
            s.DocTypeId = H.DocTypeId;
            s.SiteId = H.SiteId;
            s.DivisionId = H.DivisionId;


            var LastDispatchLine = (from L in db.SaleDispatchLine
                                   join D in db.SaleDispatchLine on L.SaleDispatchLineId equals D.SaleDispatchLineId into SaleDispatchLineTable
                                   from SaleDispatchLineTab in SaleDispatchLineTable.DefaultIfEmpty()
                                   where L.SaleDispatchHeaderId == Id
                                   orderby L.SaleDispatchLineId descending
                                   select new
                                   {
                                       GodownId = SaleDispatchLineTab.GodownId
                                   }).FirstOrDefault();


            if (LastDispatchLine != null)
            {
                s.GodownId = LastDispatchLine.GodownId;
            }
            else
            {
                var PackingHeader = (from I in db.SaleDispatchHeader
                                               join D in db.SaleDispatchHeader on I.SaleDispatchHeaderId equals D.SaleDispatchHeaderId into SaleDispatchHeaderTable
                                               from SaleDispatchHeaderTab in SaleDispatchHeaderTable.DefaultIfEmpty()
                                               join P in db.PackingHeader on SaleDispatchHeaderTab.PackingHeaderId equals P.PackingHeaderId into PackingHeaderTable
                                               from PackingHeaderTab in PackingHeaderTable.DefaultIfEmpty()
                                               where I.SaleDispatchHeaderId == Id
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
        public ActionResult _CreatePost(SaleDispatchLineViewModel svm)
        {
            SaleDispatchSetting Settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(svm.DocTypeId,svm.DivisionId, svm.SiteId);
            SaleDispatchHeader Dh = new SaleDispatchHeaderService(_unitOfWork).Find(svm.SaleDispatchHeaderId);

            PackingHeader Ph = new PackingHeaderService(_unitOfWork).Find(Dh.PackingHeaderId.Value);

            if (svm.SaleDispatchLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.IsSaleBased == true && svm.SaleOrderLineId <= 0)
            {
                ModelState.AddModelError("SaleOrderLineId", "Sale Order field is required");
            }

            if (svm.Qty <= 0)
                ModelState.AddModelError("Qty", "The Qty field is required");



            if (Settings.IsMandatoryStockIn == true)
            {
                if (svm.StockInId == 0 || svm.StockInId == null)
                    ModelState.AddModelError("StockInId", "Stock In field is required");
            }

            if (svm.GodownId <= 0)
                ModelState.AddModelError("GodownId", "The Godown field is required");

            if (ModelState.IsValid)
            {
                if (svm.SaleDispatchLineId <= 0)
                {
                    PackingLine Pl = Mapper.Map<SaleDispatchLineViewModel, PackingLine>(svm);

                    SaleDispatchLine Dl = Mapper.Map<SaleDispatchLineViewModel, SaleDispatchLine>(svm);




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
                    StockViewModel.HeaderProcessId = null;
                    StockViewModel.PersonId = Dh.SaleToBuyerId;
                    StockViewModel.ProductId = Pl.ProductId;
                    StockViewModel.HeaderFromGodownId = null;
                    StockViewModel.HeaderGodownId = null;
                    StockViewModel.GodownId = Dl.GodownId;


                    if (svm.StockInId != null)
                    {
                        var StockIn = (from L in db.Stock where L.StockId == svm.StockInId select L).FirstOrDefault();
                        if (StockIn != null)
                        {
                            StockViewModel.ProcessId = StockIn.ProcessId;
                        }
                    }

                    //StockViewModel.ProcessId = Dl.FromProcessId;
                    StockViewModel.LotNo = svm.LotNo;
                    //StockViewModel.CostCenterId = svm.CostCenterId;
                    StockViewModel.Qty_Iss = Pl.Qty + (Pl.LossQty ?? 0) + (Pl.FreeQty ?? 0);
                    StockViewModel.Qty_Rec = 0;
                    StockViewModel.Rate = 0;
                    StockViewModel.ExpiryDate = null;
                    StockViewModel.Specification = svm.Specification;
                    StockViewModel.Dimension1Id = Pl.Dimension1Id;
                    StockViewModel.Dimension2Id = Pl.Dimension2Id;
                    StockViewModel.Remark = Dl.Remark;
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




                    if (svm.StockInId != null)
                    {
                        StockAdj Adj_IssQty = new StockAdj();
                        Adj_IssQty.StockInId = (int)svm.StockInId;
                        Adj_IssQty.StockOutId = (int)Dl.StockId;
                        Adj_IssQty.DivisionId = Dh.DivisionId;
                        Adj_IssQty.SiteId = Dh.SiteId;
                        Adj_IssQty.AdjustedQty = Pl.Qty + (Pl.LossQty ?? 0);
                        new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                    }



                    Pl.PackingHeaderId = Ph.PackingHeaderId;
                    Pl.FromProcessId = StockViewModel.ProcessId;
                    Pl.CreatedBy = User.Identity.Name;
                    Pl.CreatedDate = DateTime.Now;
                    Pl.ModifiedBy = User.Identity.Name;
                    Pl.ModifiedDate = DateTime.Now;
                    Pl.ObjectState = Model.ObjectState.Added;
                    _PackingLineService.Create(Pl);


                    Dl.SaleDispatchHeaderId = Dh.SaleDispatchHeaderId;
                    Dl.PackingLineId = Pl.PackingLineId;
                    Dl.StockInId = svm.StockInId;
                    Dl.CreatedBy = User.Identity.Name;
                    Dl.CreatedDate = DateTime.Now;
                    Dl.ModifiedBy = User.Identity.Name;
                    Dl.ModifiedDate = DateTime.Now;
                    Dl.ObjectState = Model.ObjectState.Added;
                    _SaleDispatchLineService.Create(Dl);





                    if (Dh.Status != (int)StatusConstants.Drafted)
                    {
                        Dh.Status = (int)StatusConstants.Modified;
                        Ph.Status = (int)StatusConstants.Modified;
                        new PackingHeaderService(_unitOfWork).Update(Ph);
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
                        DocTypeId = Dh.DocTypeId,
                        DocId = Dl.SaleDispatchHeaderId,
                        DocLineId = Dl.SaleDispatchLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = Dh.DocNo,
                        DocDate = Dh.DocDate,
                        DocStatus = Dh.Status,
                    }));


                    return RedirectToAction("_Create", new { id = Dh.SaleDispatchHeaderId, IsSaleBased = (Pl.SaleOrderLineId == null ? false : true) });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    int status = Dh.Status;


                    PackingLine Pl = _PackingLineService.Find(svm.PackingLineId.Value);

                    SaleDispatchLine Dl = _SaleDispatchLineService.Find(svm.SaleDispatchLineId);


                    PackingLine ExRec = new PackingLine();
                    ExRec = Mapper.Map<PackingLine>(Pl);

                    SaleDispatchLine ExRecD = new SaleDispatchLine();
                    ExRecD = Mapper.Map<SaleDispatchLine>(Dl);


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
                        StockViewModel.ProductId = Pl.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = null;
                        StockViewModel.GodownId = svm.GodownId;


                        if (svm.StockInId != null)
                        {
                            var StockIn = (from L in db.Stock where L.StockId == svm.StockInId select L).FirstOrDefault();
                            if (StockIn != null)
                            {
                                StockViewModel.ProcessId = StockIn.ProcessId;
                            }
                        }
                        //StockViewModel.ProcessId = Dl.FromProcessId;



                        StockViewModel.LotNo = svm.LotNo;
                        //StockViewModel.CostCenterId = Dh.CostCenterId;
                        StockViewModel.Qty_Iss = svm.Qty + (svm.LossQty ?? 0) + (svm.FreeQty ?? 0);
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = 0;
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


                    StockAdj Adj = (from L in db.StockAdj
                                    where L.StockOutId == Dl.StockId
                                    select L).FirstOrDefault();

                    if (Adj != null)
                    {
                        new StockAdjService(_unitOfWork).Delete(Adj);
                    }

                    if (svm.StockInId != null)
                    {
                        StockAdj Adj_IssQty = new StockAdj();
                        Adj_IssQty.StockInId = (int)svm.StockInId;
                        Adj_IssQty.StockOutId = (int)Dl.StockId;
                        Adj_IssQty.DivisionId = Dh.DivisionId;
                        Adj_IssQty.SiteId = Dh.SiteId;
                        Adj_IssQty.AdjustedQty = svm.Qty + (svm.LossQty ?? 0) + (svm.FreeQty ?? 0);
                        new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                    }



                    Pl.ProductUidId = svm.ProductUidId;
                    Pl.ProductId = svm.ProductId;
                    Pl.SaleOrderLineId = svm.SaleOrderLineId;
                    Pl.PassQty = svm.PassQty;
                    Pl.Qty = svm.Qty;
                    Pl.LossQty = svm.LossQty;
                    Pl.FreeQty = svm.FreeQty;
                    Pl.BaleNo = svm.BaleNo;
                    Pl.DealUnitId = svm.DealUnitId;
                    Pl.DealQty = svm.DealQty;
                    Pl.Remark = svm.Remark;
                    if (svm.StockInId != null)
                    {
                        var StockIn = (from L in db.Stock where L.StockId == svm.StockInId select L).FirstOrDefault();
                        if (StockIn != null)
                        {
                            Pl.FromProcessId = StockIn.ProcessId;
                        }
                    }
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





                    if (Dh.Status != (int)StatusConstants.Drafted)
                    {
                        Dh.Status = (int)StatusConstants.Modified;
                        Ph.Status = (int)StatusConstants.Modified;
                        new SaleDispatchHeaderService(_unitOfWork).Update(Dh);
                        new PackingHeaderService(_unitOfWork).Update(Ph);
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
                        DocTypeId = Dh.DocTypeId,
                        DocId = Dl.SaleDispatchHeaderId,
                        DocLineId = Dl.SaleDispatchLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = Dh.DocNo,
                        xEModifications = Modifications,
                        DocDate = Dh.DocDate,
                        DocStatus = Dh.Status,
                    }));

                    //End of Saving the Activity Log

                    return Json(new { success = true });

                }
            }
            PrepareViewBag();
            return PartialView("_CreateForSaleOrder", svm);
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
            SaleDispatchLine temp = _SaleDispatchLineService.Find(id);
            PackingLine Pl = _PackingLineService.Find(temp.PackingLineId);

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

            SaleDispatchHeader H = new SaleDispatchHeaderService(_unitOfWork).Find(temp.SaleDispatchHeaderId);
            PrepareViewBag();

            SaleDispatchLineViewModel vm = _SaleDispatchLineService.GetSaleDispatchLineForEdit(id);
            //Getting Settings
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);

            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            vm.SiteId = H.SiteId;
            vm.DivisionId = H.DivisionId;
            vm.DocTypeId = H.DocTypeId;

            if (Pl.SaleOrderLineId.HasValue && Pl.SaleOrderLineId.Value > 0)
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
            SaleDispatchLine temp = _SaleDispatchLineService.Find(id);
            PackingLine PackingLine = _PackingLineService.Find(temp.PackingLineId);

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


            SaleDispatchHeader H = new SaleDispatchHeaderService(_unitOfWork).Find(temp.SaleDispatchHeaderId);
            PrepareViewBag();

            SaleDispatchLineViewModel vm = _SaleDispatchLineService.GetSaleDispatchLineForEdit(id);
            //Getting Settings
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            if (PackingLine.SaleOrderLineId.HasValue && PackingLine.SaleOrderLineId.Value > 0)
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
        public ActionResult DeletePost(SaleDispatchLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            int? StockId = 0;

            SaleDispatchHeader Dh = new SaleDispatchHeaderService(_unitOfWork).Find(vm.SaleDispatchHeaderId);

            PackingHeader Ph = new PackingHeaderService(_unitOfWork).Find(Dh.PackingHeaderId.Value);

            int status = Dh.Status;

            PackingLine Pl = _PackingLineService.Find(vm.PackingLineId.Value);

            SaleDispatchLine Dl = _SaleDispatchLineService.Find(vm.SaleDispatchLineId);


            LogList.Add(new LogTypeViewModel
            {
                ExObj = Pl,
            });

            LogList.Add(new LogTypeViewModel
            {
                ExObj = Dl,
            });


            StockId = Dl.StockId;


            _SaleDispatchLineService.Delete(Dl);
            _PackingLineService.Delete(Pl);

            if (StockId != null)
            {
                StockAdj Adj = (from L in db.StockAdj
                                where L.StockOutId == StockId
                                select L).FirstOrDefault();

                if (Adj != null)
                {
                    new StockAdjService(_unitOfWork).Delete(Adj);
                }

                new StockService(_unitOfWork).DeleteStock((int)StockId);
            }


            if (Dh.Status != (int)StatusConstants.Drafted)
            {
                Dh.Status = (int)StatusConstants.Modified;
                Ph.Status = (int)StatusConstants.Modified;
                new SaleDispatchHeaderService(_unitOfWork).Update(Dh);
                new PackingHeaderService(_unitOfWork).Update(Ph);
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
                DocTypeId = Dh.DocTypeId,
                DocId = Dh.SaleDispatchHeaderId,
                DocLineId = Dl.SaleDispatchLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = Dh.DocNo,
                xEModifications = Modifications,
                DocDate = Dh.DocDate,
                DocStatus = Dh.Status,
            }));

            return Json(new { success = true });
        }

        public ActionResult _Detail(int id)
        {


            SaleDispatchLine temp = _SaleDispatchLineService.Find(id);
            PackingLine PackingLine = _PackingLineService.Find(temp.PackingLineId);

            SaleDispatchHeader H = new SaleDispatchHeaderService(_unitOfWork).Find(temp.SaleDispatchHeaderId);
            PrepareViewBag();

            SaleDispatchLineViewModel vm = _SaleDispatchLineService.GetSaleDispatchLineForEdit(id);
            //Getting Settings
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);

            if (temp == null)
            {
                return HttpNotFound();
            }
            if (PackingLine.SaleOrderLineId.HasValue && PackingLine.SaleOrderLineId.Value > 0)
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

        public JsonResult GetProductCodeDetailJson(string ProductCode, int SaleDispatchHeaderId)
        {
            Product Product = (from P in db.Product
                               where P.ProductCode == ProductCode
                               select P).FirstOrDefault();

            if (Product != null)
            {
                return GetProductDetailJson(Product.ProductId, SaleDispatchHeaderId);
            }
            else
            {
                return null;
            }
        }

        public JsonResult GetProductDetailJson(int ProductId, int SaleDispatchHeaderId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);

            SaleDispatchHeader Header = new SaleDispatchHeaderService(_unitOfWork).Find(SaleDispatchHeaderId);

            //ProductViewModel ProductJson = new ProductViewModel();

            //var DealUnitId = _SaleDispatchLineService.GetSaleDispatchLineList(SaleDispatchHeaderId).OrderByDescending(m => m.SaleDispatchLineId).FirstOrDefault();

            var DealUnitId = _PackingLineService.GetPackingLineForHeaderId(Header.PackingHeaderId ?? 0).OrderByDescending(m => m.PackingLineId).FirstOrDefault();

            var DlUnit = new UnitService(_unitOfWork).Find((DealUnitId == null) ? product.UnitId : DealUnitId.DealUnitId);


            return Json(new { ProductId = product.ProductId, StandardCost = product.StandardCost, UnitId = product.UnitId, UnitName = product.UnitName, DealUnitId = (DealUnitId == null) ? product.UnitId : DealUnitId.DealUnitId, DealUnitDecimalPlaces = DlUnit.DecimalPlaces, Specification = product.ProductSpecification, ProductCode = product.ProductCode, ProductName = product.ProductName });
        }


        public JsonResult GetOrderDetail(int OrderId)
        {
            return Json(new SaleOrderLineService(_unitOfWork).GetSaleOrderDetailForInvoice(OrderId));
        }

        public JsonResult getunitconversiondetailjson(int productid, string unitid, string DealUnitId, int SaleDispatchHeaderId)
        {

            SaleDispatchHeader Dispatch = new SaleDispatchHeaderService(_unitOfWork).Find(SaleDispatchHeaderId);

            var Settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(Dispatch.DocTypeId, Dispatch.DivisionId, Dispatch.SiteId);

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
                        join Dl in db.SaleOrderLine on L.SaleOrderLineId equals Dl.SaleOrderLineId into SaleOrderLineTable
                        from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
                        join P in db.Product on L.ProductId equals P.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join U in db.Units on ProductTab.UnitId equals U.UnitId into UnitTable
                        from UnitTab in UnitTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on L.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on L.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where L.SaleOrderLineId == SaleOrderLineId
                        select new
                        {
                            SaleOrderHeaderDocNo = L.SaleOrderNo,
                            UnitId = UnitTab.UnitId,
                            UnitName = UnitTab.UnitName,
                            DealUnitId = SaleOrderLineTab.DealUnitId,
                            Specification = SaleOrderLineTab.Specification,
                            UnitConversionMultiplier = SaleOrderLineTab.UnitConversionMultiplier,
                            ProductId = L.ProductId,
                            Dimension1Id = L.Dimension1Id,
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Id = L.Dimension2Id,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
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

        public JsonResult GetPendingSaleOrderCount(int ProductId, int SaleDispatchHeaderId)
        {
            int BuyerId = new SaleDispatchHeaderService(_unitOfWork).Find(SaleDispatchHeaderId).SaleToBuyerId;
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
                                    SaleOrderNo = L.Product.ProductName
                                };

            SaleOrderJson.id = SaleOrderLine.FirstOrDefault().ToString();
            SaleOrderJson.text = SaleOrderLine.FirstOrDefault().SaleOrderNo;

            return Json(SaleOrderJson);
        }

        public ActionResult GetCustomProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleDispatchLineService.GetCustomProducts(filter, searchTerm);
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

        public ActionResult GetSaleOrderForProduct(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleDispatchLineService.GetSaleOrderHelpListForProduct(filter, searchTerm);
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

            var Query = _SaleDispatchLineService.GetPendingProductsForSaleDispatch(filter, searchTerm);

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

        public JsonResult GetSaleOrder(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleDispatchLineService.GetPendingOrdersForDispatch(filter, searchTerm);

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

        public ActionResult GetFirstStockInForProduct(int SaleDispatchHeaderId, int GodownId, int ProductId, int? Dimension1Id, int? Dimension2Id)//DocTypeId
        {
            var Query = _SaleDispatchLineService.GetPendingStockInForDispatch(SaleDispatchHeaderId, GodownId, ProductId, Dimension1Id, Dimension2Id, "");
            var temp = Query.ToList();

            var count = Query.Count();


            if (count == 1)
            {
                if (temp != null)
                {
                    return Json(temp.FirstOrDefault());
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public ActionResult GetStockInForProduct(string searchTerm, int pageSize, int pageNum, int SaleDispatchHeaderId, int GodownId, int ProductId, int? Dimension1Id, int? Dimension2Id)//DocTypeId
        {
            var Query = _SaleDispatchLineService.GetPendingStockInForDispatch(SaleDispatchHeaderId, GodownId, ProductId, Dimension1Id, Dimension2Id, searchTerm);
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

        public JsonResult SetSingleStockIn(int Ids)
        {
            ComboBoxResult StockInJson = new ComboBoxResult();

            var StockInLine = from L in db.Stock
                                join H in db.StockHeader on L.StockHeaderId equals H.StockHeaderId into StockHeaderTable
                                from StockHeaderTab in StockHeaderTable.DefaultIfEmpty()
                                where L.StockId == Ids
                                select new
                                {
                                    StockInId = L.StockId,
                                    StockInNo = StockHeaderTab.DocNo
                                };

            StockInJson.id = StockInLine.FirstOrDefault().ToString();
            StockInJson.text = StockInLine.FirstOrDefault().StockInNo;

            return Json(StockInJson);
        }

        public JsonResult GetStockInDetailJson(int StockInId)
        {
            var temp = (from L in db.ViewStockInBalance
                        where L.StockInId == StockInId
                        select new
                        {
                            BalanceQty = L.BalanceQty,
                            LotNo = L.LotNo
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

        public JsonResult GetStockInBalance(int StockInId)
        {
            var temp = (from L in db.ViewStockInBalance where L.StockInId == StockInId select L).FirstOrDefault();
            if (temp != null)
            {
                return Json(temp.BalanceQty, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
