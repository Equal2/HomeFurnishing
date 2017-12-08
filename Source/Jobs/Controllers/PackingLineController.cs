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
    public class PackingLineController : System.Web.Mvc.Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPackingLineService _PackingLineService;
        IStockService _StockService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public PackingLineController(IStockService StockService, IPackingLineService Packing, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PackingLineService = Packing;
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
        public JsonResult Index(int id)
        {
            //var p = _PackingLineService.GetPackingLineListForIndex(id).ToList();
            var p = _PackingLineService.GetPackingLineListForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        public ActionResult _Index(int id, int Status)
        {
            ViewBag.Status = Status;
            ViewBag.PackingHeaderId = id;
            var p = _PackingLineService.GetPackingLineListForIndex(id).ToList();
            return PartialView(p);
        }
        public ActionResult _ForOrder(int id)
        {
            PackingFilterViewModel vm = new PackingFilterViewModel();
            vm.PackingHeaderId = id;
            PackingHeader H = new PackingHeaderService(_unitOfWork).Find(id);
            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.PackingSettings = Mapper.Map<PackingSetting, PackingSettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            return PartialView("_OrderFilters", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostOrders(PackingFilterViewModel vm)
        {
            List<PackingLineViewModel> temp = _PackingLineService.GetSaleOrdersForFilters(vm).ToList();
            PackingListViewModel svm = new PackingListViewModel();
            svm.PackingLineViewModel = temp;
            return PartialView("_Results", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PackingListViewModel vm)
        {
            int Cnt = 0;

            PackingHeader Dh = new PackingHeaderService(_unitOfWork).Find(vm.PackingLineViewModel.FirstOrDefault().PackingHeaderId);

            PackingSetting Settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(Dh.DocTypeId, Dh.DivisionId, Dh.SiteId);

            
            
            int pk = 0;
            int PackingPrimaryKey = 0;



            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            if (ModelState.IsValid)
            {
                foreach (var item in vm.PackingLineViewModel)
                {
                    decimal balqty = (from p in db.ViewSaleOrderBalance
                                      where p.SaleOrderLineId == item.SaleOrderLineId
                                      select p.BalanceQty).FirstOrDefault();
                    if (item.Qty > 0 && item.Qty <= balqty)
                    {

                        PackingLine Dl = new PackingLine();

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
                        StockViewModel.DocHeaderId = Dh.PackingHeaderId;
                        StockViewModel.DocLineId = Dl.PackingLineId;
                        StockViewModel.DocTypeId = Dh.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Dh.DocDate;
                        StockViewModel.StockDocDate = Dh.DocDate;
                        StockViewModel.DocNo = Dh.DocNo;
                        StockViewModel.DivisionId = Dh.DivisionId;
                        StockViewModel.SiteId = Dh.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = Dh.BuyerId;
                        StockViewModel.ProductId = item.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = null;
                        StockViewModel.HeaderProcessId = null;
                        StockViewModel.GodownId = Dh.GodownId;
                        StockViewModel.Remark = Dh.Remark;
                        StockViewModel.Status = Dh.Status;
                        StockViewModel.ProcessId = item.FromProcessId;
                        StockViewModel.LotNo = null;
                        StockViewModel.Qty_Iss = item.Qty;
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
                        Dl.StockIssueId = StockViewModel.StockId;


                        Dl.BaleNo = item.BaleNo;
                        Dl.DealQty = item.Qty * item.UnitConversionMultiplier ?? 0;
                        Dl.DealUnitId = item.DealUnitId;
                        Dl.Dimension1Id = item.Dimension1Id;
                        Dl.Dimension2Id = item.Dimension2Id;
                        Dl.LotNo = item.LotNo;
                        Dl.CreatedBy = User.Identity.Name;
                        Dl.CreatedDate = DateTime.Now;
                        Dl.ModifiedBy = User.Identity.Name;
                        Dl.ModifiedDate = DateTime.Now;
                        Dl.PackingHeaderId = Dh.PackingHeaderId;
                        Dl.ProductId = item.ProductId;
                        Dl.Qty = item.Qty;
                        Dl.Remark = item.Remark;
                        Dl.SaleOrderLineId = item.SaleOrderLineId;
                        Dl.Specification = item.Specification;
                        Dl.PackingLineId = PackingPrimaryKey++;
                        Dl.CreatedBy = User.Identity.Name;
                        Dl.ModifiedBy = User.Identity.Name;
                        Dl.CreatedDate = DateTime.Now;
                        Dl.ModifiedDate = DateTime.Now;
                        Dl.ObjectState = Model.ObjectState.Added;
                        _PackingLineService.Create(Dl);

                        pk++;
                        Cnt = Cnt + 1;
                    }
                }

                new PackingHeaderService(_unitOfWork).Update(Dh);


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
                    DocId = Dh.PackingHeaderId,
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
            PackingHeader H = new PackingHeaderService(_unitOfWork).Find(Id);
            PackingLineViewModel s = new PackingLineViewModel();


            //Getting Settings
            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.PackingSettings = Mapper.Map<PackingSetting, PackingSettingsViewModel>(settings);
            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.IsSaleBased = IsSaleBased;
            s.PackingHeaderId = H.PackingHeaderId;
            s.DocTypeId = H.DocTypeId;
            s.SiteId = H.SiteId;
            s.DivisionId = H.DivisionId;


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
        public ActionResult _CreatePost(PackingLineViewModel svm)
        {
            PackingSetting Settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(svm.DocTypeId,svm.DivisionId, svm.SiteId);
            PackingHeader Dh = new PackingHeaderService(_unitOfWork).Find(svm.PackingHeaderId);


            if (svm.PackingLineId <= 0)
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

            if (svm.SaleOrderLineId != 0 && svm.SaleOrderLineId != null)
            {
                var SaleOrderBalance = (from L in db.ViewSaleOrderBalance where L.SaleOrderLineId == svm.SaleOrderLineId select L).FirstOrDefault();

                if (SaleOrderBalance != null)
                { 
                    if (svm.Qty > SaleOrderBalance.BalanceQty)
                    {
                        ModelState.AddModelError("SaleOrderLineId", "Pack qty is greater then Order Qty.");
                    }
                }
            }

            if (svm.StockInId != 0 && svm.StockInId != null)
            {
                var StockINBalance = (from L in db.ViewStockInBalance where L.StockInId == svm.StockInId select L).FirstOrDefault();

                if (StockINBalance != null)
                {
                    if (svm.Qty > StockINBalance.BalanceQty)
                    {
                        ModelState.AddModelError("StockInId", "Pack qty is greater then Stock Balance Qty.");
                    }
                }
            }



            if (ModelState.IsValid)
            {
                if (svm.PackingLineId <= 0)
                {
                    PackingLine Dl = Mapper.Map<PackingLineViewModel, PackingLine>(svm);

                    StockViewModel StockViewModel_Issue = new StockViewModel();
                    //Posting in Stock

                    StockViewModel_Issue.StockHeaderId = Dh.StockHeaderId ?? 0;
                    StockViewModel_Issue.DocHeaderId = Dh.PackingHeaderId;
                    StockViewModel_Issue.DocLineId = Dl.PackingLineId;
                    StockViewModel_Issue.DocTypeId = Dh.DocTypeId;
                    StockViewModel_Issue.StockHeaderDocDate = Dh.DocDate;
                    StockViewModel_Issue.StockDocDate = Dh.DocDate;
                    StockViewModel_Issue.DocNo = Dh.DocNo;
                    StockViewModel_Issue.DivisionId = Dh.DivisionId;
                    StockViewModel_Issue.SiteId = Dh.SiteId;
                    StockViewModel_Issue.CurrencyId = null;
                    StockViewModel_Issue.HeaderProcessId = null;
                    StockViewModel_Issue.PersonId = Dh.BuyerId;
                    StockViewModel_Issue.ProductId = Dl.ProductId;
                    StockViewModel_Issue.HeaderFromGodownId = null;
                    StockViewModel_Issue.HeaderGodownId = null;
                    StockViewModel_Issue.GodownId = Dh.GodownId;


                    if (svm.StockInId != null)
                    {
                        var StockIn = (from L in db.Stock where L.StockId == svm.StockInId select L).FirstOrDefault();
                        if (StockIn != null)
                        {
                            StockViewModel_Issue.ProcessId = StockIn.ProcessId;
                        }
                    }

                    //StockViewModel.ProcessId = Dl.FromProcessId;
                    StockViewModel_Issue.LotNo = svm.LotNo;
                    //StockViewModel.CostCenterId = svm.CostCenterId;
                    StockViewModel_Issue.Qty_Iss = Dl.Qty + (Dl.LossQty ?? 0) + (Dl.FreeQty ?? 0);
                    StockViewModel_Issue.Qty_Rec = 0;
                    StockViewModel_Issue.Rate = 0;
                    StockViewModel_Issue.ExpiryDate = null;
                    StockViewModel_Issue.Specification = svm.Specification;
                    StockViewModel_Issue.Dimension1Id = Dl.Dimension1Id;
                    StockViewModel_Issue.Dimension2Id = Dl.Dimension2Id;
                    StockViewModel_Issue.Remark = Dl.Remark;
                    StockViewModel_Issue.Status = Dh.Status;
                    StockViewModel_Issue.CreatedBy = Dh.CreatedBy;
                    StockViewModel_Issue.CreatedDate = DateTime.Now;
                    StockViewModel_Issue.ModifiedBy = Dh.ModifiedBy;
                    StockViewModel_Issue.ModifiedDate = DateTime.Now;

                    string StockPostingError = "";
                    StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel_Issue);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }





                    StockViewModel StockViewModel_Receive = new StockViewModel();
                    //Posting in Stock

                    StockViewModel_Receive.StockId = -1;
                    StockViewModel_Receive.StockHeaderId = Dh.StockHeaderId ?? -1;
                    StockViewModel_Receive.DocHeaderId = Dh.PackingHeaderId;
                    StockViewModel_Receive.DocLineId = Dl.PackingLineId;
                    StockViewModel_Receive.DocTypeId = Dh.DocTypeId;
                    StockViewModel_Receive.StockHeaderDocDate = Dh.DocDate;
                    StockViewModel_Receive.StockDocDate = Dh.DocDate;
                    StockViewModel_Receive.DocNo = Dh.DocNo;
                    StockViewModel_Receive.DivisionId = Dh.DivisionId;
                    StockViewModel_Receive.SiteId = Dh.SiteId;
                    StockViewModel_Receive.CurrencyId = null;
                    StockViewModel_Receive.HeaderProcessId = null;
                    StockViewModel_Receive.PersonId = Dh.BuyerId;
                    StockViewModel_Receive.ProductId = Dl.ProductId;
                    StockViewModel_Receive.HeaderFromGodownId = null;
                    StockViewModel_Receive.HeaderGodownId = null;
                    StockViewModel_Receive.GodownId = Dh.GodownId;

                    Process PackingProcess = new ProcessService(_unitOfWork).Find(ProcessConstants.Packing);
                    if (PackingProcess != null)
                        StockViewModel_Receive.ProcessId = PackingProcess.ProcessId;

                    StockViewModel_Receive.LotNo = svm.LotNo;
                    //StockViewModel.CostCenterId = svm.CostCenterId;
                    StockViewModel_Receive.Qty_Iss = 0;
                    StockViewModel_Receive.Qty_Rec = Dl.Qty + (Dl.LossQty ?? 0) + (Dl.FreeQty ?? 0);
                    StockViewModel_Receive.Rate = 0;
                    StockViewModel_Receive.ExpiryDate = null;
                    StockViewModel_Receive.Specification = svm.Specification;
                    StockViewModel_Receive.Dimension1Id = Dl.Dimension1Id;
                    StockViewModel_Receive.Dimension2Id = Dl.Dimension2Id;
                    StockViewModel_Receive.Remark = Dl.Remark;
                    StockViewModel_Receive.Status = Dh.Status;
                    StockViewModel_Receive.CreatedBy = Dh.CreatedBy;
                    StockViewModel_Receive.CreatedDate = DateTime.Now;
                    StockViewModel_Receive.ModifiedBy = Dh.ModifiedBy;
                    StockViewModel_Receive.ModifiedDate = DateTime.Now;

                    StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel_Receive);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }

                    Dl.StockIssueId = StockViewModel_Issue.StockId;
                    Dl.StockReceiveId = StockViewModel_Receive.StockId;

                    if (Dh.StockHeaderId == null)
                    {
                        Dh.StockHeaderId = StockViewModel_Issue.StockHeaderId;
                    }

                    
                    if (svm.StockInId != null)
                    {
                        StockAdj Adj_IssQty = new StockAdj();
                        Adj_IssQty.StockInId = (int)svm.StockInId;
                        Adj_IssQty.StockOutId = (int)Dl.StockIssueId;
                        Adj_IssQty.DivisionId = Dh.DivisionId;
                        Adj_IssQty.SiteId = Dh.SiteId;
                        Adj_IssQty.AdjustedQty = Dl.Qty + (Dl.LossQty ?? 0);
                        new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                    }




                    Dl.PackingHeaderId = Dh.PackingHeaderId;
                    Dl.FromProcessId = StockViewModel_Issue.ProcessId;
                    Dl.StockInId = svm.StockInId;
                    Dl.CreatedBy = User.Identity.Name;
                    Dl.CreatedDate = DateTime.Now;
                    Dl.ModifiedBy = User.Identity.Name;
                    Dl.ModifiedDate = DateTime.Now;
                    Dl.ObjectState = Model.ObjectState.Added;
                    _PackingLineService.Create(Dl);


                    if (Dh.Status != (int)StatusConstants.Drafted)
                    {
                        Dh.Status = (int)StatusConstants.Modified;
                    }

                    new PackingHeaderService(_unitOfWork).Update(Dh);

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
                        DocId = Dl.PackingHeaderId,
                        DocLineId = Dl.PackingLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = Dh.DocNo,
                        DocDate = Dh.DocDate,
                        DocStatus = Dh.Status,
                    }));


                    return RedirectToAction("_Create", new { id = Dh.PackingHeaderId, IsSaleBased = (Dl.SaleOrderLineId == null ? false : true) });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    int status = Dh.Status;


                    PackingLine Dl = _PackingLineService.Find(svm.PackingLineId);




                    PackingLine ExRecD = new PackingLine();
                    ExRecD = Mapper.Map<PackingLine>(Dl);


                    if (Dl.StockIssueId != null)
                    {
                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = Dh.StockHeaderId ?? 0;
                        StockViewModel.StockId = Dl.StockIssueId ?? 0;
                        StockViewModel.DocHeaderId = Dl.PackingHeaderId;
                        StockViewModel.DocLineId = Dl.PackingLineId;
                        StockViewModel.DocTypeId = Dh.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Dh.DocDate;
                        StockViewModel.StockDocDate = Dh.DocDate;
                        StockViewModel.DocNo = Dh.DocNo;
                        StockViewModel.DivisionId = Dh.DivisionId;
                        StockViewModel.SiteId = Dh.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = null;
                        StockViewModel.PersonId = Dh.BuyerId;
                        StockViewModel.ProductId = Dl.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = null;
                        StockViewModel.GodownId = Dh.GodownId;


                        if (svm.StockInId != null)
                        {
                            var StockIn = (from L in db.Stock where L.StockId == svm.StockInId select L).FirstOrDefault();
                            if (StockIn != null)
                            {
                                StockViewModel.ProcessId = StockIn.ProcessId;
                            }
                        }

                        StockViewModel.LotNo = svm.LotNo;
                        //StockViewModel.CostCenterId = Dh.CostCenterId;
                        StockViewModel.Qty_Iss = svm.Qty;
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


                    if (Dl.StockReceiveId != null)
                    {
                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = Dh.StockHeaderId ?? 0;
                        StockViewModel.StockId = Dl.StockReceiveId ?? 0;
                        StockViewModel.DocHeaderId = Dl.PackingHeaderId;
                        StockViewModel.DocLineId = Dl.PackingLineId;
                        StockViewModel.DocTypeId = Dh.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Dh.DocDate;
                        StockViewModel.StockDocDate = Dh.DocDate;
                        StockViewModel.DocNo = Dh.DocNo;
                        StockViewModel.DivisionId = Dh.DivisionId;
                        StockViewModel.SiteId = Dh.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = null;
                        StockViewModel.PersonId = Dh.BuyerId;
                        StockViewModel.ProductId = Dl.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = null;
                        StockViewModel.GodownId = Dh.GodownId;

                        Process PackingProcess = new ProcessService(_unitOfWork).Find(ProcessConstants.Packing);
                        if (PackingProcess != null)
                            StockViewModel.ProcessId = PackingProcess.ProcessId;

                        StockViewModel.LotNo = svm.LotNo;
                        //StockViewModel.CostCenterId = Dh.CostCenterId;
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = svm.Qty;
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
                                    where L.StockOutId == Dl.StockIssueId
                                    select L).FirstOrDefault();

                    if (Adj != null)
                    {
                        new StockAdjService(_unitOfWork).Delete(Adj);
                    }

                    if (svm.StockInId != null)
                    {
                        StockAdj Adj_IssQty = new StockAdj();
                        Adj_IssQty.StockInId = (int)svm.StockInId;
                        Adj_IssQty.StockOutId = (int)Dl.StockIssueId;
                        Adj_IssQty.DivisionId = Dh.DivisionId;
                        Adj_IssQty.SiteId = Dh.SiteId;
                        Adj_IssQty.AdjustedQty = svm.Qty;
                        new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                    }



                    Dl.ProductUidId = svm.ProductUidId;
                    Dl.ProductId = svm.ProductId;
                    Dl.SaleOrderLineId = svm.SaleOrderLineId;
                    Dl.Qty = svm.Qty;
                    Dl.BaleNo = svm.BaleNo;
                    Dl.DealUnitId = svm.DealUnitId;
                    Dl.DealQty = svm.DealQty;
                    Dl.Remark = svm.Remark;
                    if (svm.StockInId != null)
                    {
                        var StockIn = (from L in db.Stock where L.StockId == svm.StockInId select L).FirstOrDefault();
                        if (StockIn != null)
                        {
                            Dl.FromProcessId = StockIn.ProcessId;
                        }
                    }
                    Dl.Specification = svm.Specification;
                    Dl.Dimension1Id = svm.Dimension1Id;
                    Dl.Dimension2Id = svm.Dimension2Id;
                    Dl.LotNo = svm.LotNo;
                    _PackingLineService.Update(Dl);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecD,
                        Obj = Dl,
                    });





                    if (Dh.Status != (int)StatusConstants.Drafted)
                    {
                        Dh.Status = (int)StatusConstants.Modified;
                        new PackingHeaderService(_unitOfWork).Update(Dh);
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
                        DocId = Dl.PackingHeaderId,
                        DocLineId = Dl.PackingLineId,
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
            PackingLine temp = _PackingLineService.Find(id);

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

            PackingHeader H = new PackingHeaderService(_unitOfWork).Find(temp.PackingHeaderId);
            PrepareViewBag();

            PackingLineViewModel vm = _PackingLineService.GetPackingLineForEdit(id);
            //Getting Settings
            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.PackingSettings = Mapper.Map<PackingSetting, PackingSettingsViewModel>(settings);

            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            vm.SiteId = H.SiteId;
            vm.DivisionId = H.DivisionId;
            vm.DocTypeId = H.DocTypeId;

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
            PackingLine temp = _PackingLineService.Find(id);
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


            PackingHeader H = new PackingHeaderService(_unitOfWork).Find(temp.PackingHeaderId);
            PrepareViewBag();

            PackingLineViewModel vm = _PackingLineService.GetPackingLineForEdit(id);
            //Getting Settings
            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.PackingSettings = Mapper.Map<PackingSetting, PackingSettingsViewModel>(settings);
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
        public ActionResult DeletePost(PackingLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            int? StockIssueId = 0;
            int? StockReceiveId = 0;

            PackingHeader Dh = new PackingHeaderService(_unitOfWork).Find(vm.PackingHeaderId);

            int status = Dh.Status;

            PackingLine Dl = _PackingLineService.Find(vm.PackingLineId);


            LogList.Add(new LogTypeViewModel
            {
                ExObj = Dl,
            });


            StockIssueId = Dl.StockIssueId;
            StockReceiveId = Dl.StockReceiveId;


            _PackingLineService.Delete(Dl);

            if (StockIssueId != null)
            {
                StockAdj Adj = (from L in db.StockAdj
                                where L.StockOutId == StockIssueId
                                select L).FirstOrDefault();

                if (Adj != null)
                {
                    new StockAdjService(_unitOfWork).Delete(Adj);
                }

                new StockService(_unitOfWork).DeleteStock((int)StockIssueId);
                new StockService(_unitOfWork).DeleteStock((int)StockReceiveId);
            }


            if (Dh.Status != (int)StatusConstants.Drafted)
            {
                Dh.Status = (int)StatusConstants.Modified;
                new PackingHeaderService(_unitOfWork).Update(Dh);
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
                DocId = Dh.PackingHeaderId,
                DocLineId = Dl.PackingLineId,
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
            PackingLine temp = _PackingLineService.Find(id);

            PackingHeader H = new PackingHeaderService(_unitOfWork).Find(temp.PackingHeaderId);
            PrepareViewBag();

            PackingLineViewModel vm = _PackingLineService.GetPackingLineForEdit(id);
            //Getting Settings
            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.PackingSettings = Mapper.Map<PackingSetting, PackingSettingsViewModel>(settings);

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

        public JsonResult GetProductCodeDetailJson(string ProductCode, int PackingHeaderId)
        {
            Product Product = (from P in db.Product
                               where P.ProductCode == ProductCode
                               select P).FirstOrDefault();

            if (Product != null)
            {
                return GetProductDetailJson(Product.ProductId, PackingHeaderId);
            }
            else
            {
                return null;
            }
        }

        public JsonResult GetProductDetailJson(int ProductId, int PackingHeaderId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);

            PackingHeader Header = new PackingHeaderService(_unitOfWork).Find(PackingHeaderId);

            //ProductViewModel ProductJson = new ProductViewModel();

            //var DealUnitId = _PackingLineService.GetPackingLineList(PackingHeaderId).OrderByDescending(m => m.PackingLineId).FirstOrDefault();

            var DealUnitId = _PackingLineService.GetPackingLineForHeaderId(Header.PackingHeaderId).OrderByDescending(m => m.PackingLineId).FirstOrDefault();

            var DlUnit = new UnitService(_unitOfWork).Find((DealUnitId == null) ? product.UnitId : DealUnitId.DealUnitId);


            return Json(new { ProductId = product.ProductId, StandardCost = product.StandardCost, UnitId = product.UnitId, UnitName = product.UnitName, DealUnitId = (DealUnitId == null) ? product.UnitId : DealUnitId.DealUnitId, DealUnitDecimalPlaces = DlUnit.DecimalPlaces, Specification = product.ProductSpecification, ProductCode = product.ProductCode, ProductName = product.ProductName });
        }


        public JsonResult GetOrderDetail(int OrderId)
        {
            return Json(new SaleOrderLineService(_unitOfWork).GetSaleOrderDetailForInvoice(OrderId));
        }

        public JsonResult getunitconversiondetailjson(int productid, string unitid, string DealUnitId, int PackingHeaderId)
        {

            PackingHeader Packing = new PackingHeaderService(_unitOfWork).Find(PackingHeaderId);

            var Settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(Packing.DocTypeId, Packing.DivisionId, Packing.SiteId);

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
            var temp = (from L in db.ViewSaleOrderBalanceForCancellation
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

        public JsonResult GetPendingSaleOrderCount(int ProductId, int PackingHeaderId)
        {
            int BuyerId = new PackingHeaderService(_unitOfWork).Find(PackingHeaderId).BuyerId;
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
            var Query = _PackingLineService.GetCustomProducts(filter, searchTerm);
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
            var Query = _PackingLineService.GetSaleOrderHelpListForProduct(filter, searchTerm);
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

        public JsonResult GetCustomProductsForPacking(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var Query = _PackingLineService.GetPendingProductsForPacking(filter, searchTerm);

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
            var Query = _PackingLineService.GetPendingOrdersForPacking(filter, searchTerm);

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

        public ActionResult GetFirstStockInForProduct(int PackingHeaderId, int GodownId, int ProductId, int? Dimension1Id, int? Dimension2Id)//DocTypeId
        {
            var Query = _PackingLineService.GetPendingStockInForPacking(PackingHeaderId, GodownId, ProductId, Dimension1Id, Dimension2Id, "");
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

        public ActionResult GetStockInForProduct(string searchTerm, int pageSize, int pageNum, int PackingHeaderId, int GodownId, int ProductId, int? Dimension1Id, int? Dimension2Id)//DocTypeId
        {
            var Query = _PackingLineService.GetPendingStockInForPacking(PackingHeaderId, GodownId, ProductId, Dimension1Id, Dimension2Id, searchTerm);
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
