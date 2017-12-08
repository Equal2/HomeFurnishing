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
using JobInvoiceReceiveDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;
using Jobs.Helpers;

namespace Jobs.Controllers
{

    [Authorize]
    public class JobInvoiceReceiveLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        IJobReceiveLineService _JobReceiveLineService;
        IJobInvoiceLineService _JobInvoiceLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public JobInvoiceReceiveLineController(IJobReceiveLineService JobReceive, IJobInvoiceLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobReceiveLineService = JobReceive;
            _JobInvoiceLineService = SaleOrder;
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
            //var p = _JobInvoiceLineService.GetLineListForIndex(id).ToList();
            //return Json(p, JsonRequestBehavior.AllowGet);
            var p = Json(_JobInvoiceLineService.GetLineListForIndex(id).ToList(), JsonRequestBehavior.AllowGet);
            p.MaxJsonLength = int.MaxValue;
            return p;

        }

        public ActionResult _ForOrder(int id, int? JobworkrId)
        {
            JobInvoiceLineFilterViewModel vm = new JobInvoiceLineFilterViewModel();

            JobInvoiceHeader Header = new JobInvoiceHeaderService(_unitOfWork).Find(id);
            JobInvoiceSettings Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            if (JobworkrId.HasValue)
                vm.JobWorkerId = JobworkrId.Value;
            vm.JobInvoiceHeaderId = id;
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            vm.JobInvoiceSettings = Mapper.Map<JobInvoiceSettingsViewModel>(Settings);
            return PartialView("_OrderFilters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostOrders(JobInvoiceLineFilterViewModel vm)
        {
            JobInvoiceHeader Header = new JobInvoiceHeaderService(_unitOfWork).Find(vm.JobInvoiceHeaderId);
            List<JobInvoiceLineViewModel> temp = _JobInvoiceLineService.GetJobOrderForFiltersForInvoiceReceive(vm).ToList();
            JobInvoiceMasterDetailModel svm = new JobInvoiceMasterDetailModel();
            JobInvoiceSettings settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);


            svm.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            svm.JobInvoiceLineViewModel = temp;
            if ((settings.isVisibleJobReceive ?? false) == true)
            {
                return PartialView("_Results", svm);
            }
            else
            {
                return PartialView("_OrderResults", svm);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobInvoiceMasterDetailModel vm)
        {

            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            int Cnt = 0;
            int Cnt1 = 0;
            bool HeaderChargeEdit = false;
            int Serial = _JobInvoiceLineService.GetMaxSr(vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId);
            List<LineReferenceIds> RefIds = new List<LineReferenceIds>();
            List<LineChargeRates> LineChargeRates = new List<LineChargeRates>();
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();


            int IncentiveId = new ChargeService(_unitOfWork).GetChargeByName(ChargeConstants.Incentive).ChargeId;
            int PenaltyId = new ChargeService(_unitOfWork).GetChargeByName(ChargeConstants.Penalty).ChargeId;

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceReceiveDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");
            #endregion

            JobInvoiceHeader Header = new JobInvoiceHeaderService(_unitOfWork).Find(vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId);

            

            JobInvoiceSettings Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new JobInvoiceLineChargeService(_unitOfWork).GetMaxProductCharge(Header.JobInvoiceHeaderId, "Web.JobInvoiceLines", "JobInvoiceHeaderId", "JobInvoiceLineId");

            int PersonCount = 0;
            if (!Settings.CalculationId.HasValue)
            {
                throw new Exception("Calculation not configured in Invoice settings");
            }



            //int CalculationId = Settings.CalculationId ?? 0;
            int CalculationId = 0;

            if (Header.SalesTaxGroupPersonId != null)
                CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(Header.DocTypeId, (int)Header.SalesTaxGroupPersonId, Header.SiteId, Header.DivisionId) ?? 0;

            if (CalculationId == 0)
                CalculationId = Settings.CalculationId ?? 0;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();


            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                



                var JobOrderLineIds = vm.JobInvoiceLineViewModel.Where(m => m.DealQty > 0).Select(m => m.JobOrderLineId).ToArray();

                var JObOrderCostCenters = (from p in db.JobOrderLine
                                           join h in db.JobOrderHeader on p.JobOrderHeaderId equals h.JobOrderHeaderId
                                           where JobOrderLineIds.Contains(p.JobOrderLineId)
                                           select new { CostCenterId = h.CostCenterId, Rate = p.Rate, LineId = p.JobOrderLineId }).ToList();


                var BalanceQtyRecords = (from p in db.ViewJobOrderBalance
                                         where JobOrderLineIds.Contains(p.JobOrderLineId)
                                         select new { BAlQty = p.BalanceQty, LineId = p.JobOrderLineId }).ToList();

                var JobOrderLineRecords = (from p in db.JobOrderLine
                                           where JobOrderLineIds.Contains(p.JobOrderLineId)
                                           select p).ToList();

                var JobOrderLineCharges = (from p in db.JobOrderLineCharge
                                           where JobOrderLineIds.Contains(p.LineTableId) && (p.ChargeId == IncentiveId || p.ChargeId == PenaltyId)
                                           select p).ToList();

                var JobOrderHEaderIds = JobOrderLineRecords.Select(m => m.JobOrderHeaderId).ToArray();

                var JobOrderHEaderRecords = (from p in db.JobOrderHeader
                                             where JobOrderHEaderIds.Contains(p.JobOrderHeaderId)
                                             select p).ToList();


                foreach (var item in vm.JobInvoiceLineViewModel.Where(m => m.DealQty > 0))
                {
                    if (item.DealQty > 0 && item.Rate > 0)
                    {


                        #region "Tax Calculation Validation"
                        try
                        {
                            SiteDivisionSettings SiteDivisionSettings = new SiteDivisionSettingsService(_unitOfWork).GetSiteDivisionSettings(Header.SiteId, Header.DivisionId, Header.DocDate);
                            if (SiteDivisionSettings != null)
                            {
                                if (SiteDivisionSettings.IsApplicableGST == true)
                                {
                                    string ProductName = new ProductService(_unitOfWork).Find(item.ProductId).ProductName;

                                    if (item.SalesTaxGroupPersonId == 0 || item.SalesTaxGroupPersonId == null)
                                    {
                                        //ModelState.AddModelError("", "Sales Tax Group Person is not defined for party, it is required.");
                                        throw new Exception("Sales Tax Group Person is not defined for party, it is required.");
                                    }

                                    if (item.SalesTaxGroupProductId == 0 || item.SalesTaxGroupProductId == null)
                                    {
                                        //ModelState.AddModelError("", "Sales Tax Group Product is not defined for product, it is required.");
                                        throw new Exception("Sales Tax Group Product is not defined for product "+ ProductName  + ", it is required.");
                                    }

                                    if (item.SalesTaxGroupProductId != 0 && item.SalesTaxGroupProductId != null && item.SalesTaxGroupPersonId != 0 && item.SalesTaxGroupPersonId != null && CalculationId != null)
                                    {
                                        IEnumerable<ChargeRateSettings> ChargeRateSettingsList = new CalculationProductService(_unitOfWork).GetChargeRateSettingForValidation(CalculationId, Header.DocTypeId, Header.SiteId, Header.DivisionId, Header.ProcessId, (int)item.SalesTaxGroupPersonId, (int)item.SalesTaxGroupProductId);

                                        foreach (var ChargeRateSettings in ChargeRateSettingsList)
                                        {
                                            if (ChargeRateSettings.ChargeGroupSettingId == null)
                                            {
                                                //ModelState.AddModelError("", "Charge Group Setting is not defined for " + ChargeRateSettings.ChargeName + ".");
                                                throw new Exception("Charge Group Setting is not defined for " + ChargeRateSettings.ChargeName + " for product " + ProductName);
                                            }

                                            if (ChargeRateSettings.LedgerAccountCrName == LedgerAccountConstants.Charge || ChargeRateSettings.LedgerAccountDrName == LedgerAccountConstants.Charge)
                                            {
                                                if (ChargeRateSettings.ChargeGroupSettingId != null && ChargeRateSettings.ChargePer != 0 && ChargeRateSettings.ChargePer != null && ChargeRateSettings.ChargeLedgerAccountId == null)
                                                {
                                                    //ModelState.AddModelError("", "Ledger account is not defined for " + ChargeRateSettings.ChargeName + " in charge group settings.");
                                                    throw new Exception("Ledger account is not defined for " + ChargeRateSettings.ChargeName + " in charge group settings." + " for product " + ProductName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = _exception.HandleException(ex);
                            TempData["CSEXCL"] += message;
                            if (item.JobReceiveLineId != null && item.JobReceiveLineId != 0)
                                return PartialView("_Results", vm);
                            else
                                return PartialView("_OrderResults", vm);
                        }
                        #endregion




                        var temp = (from p in JObOrderCostCenters
                                    where p.LineId == item.JobOrderLineId
                                    select new { CostCenterId = p.CostCenterId, Rate = p.Rate }).FirstOrDefault();


                        var balqty = (from p in BalanceQtyRecords
                                      where p.LineId == item.JobOrderLineId
                                      select p.BAlQty).FirstOrDefault();

                        JobOrderLine JobOrderLine = JobOrderLineRecords.Where(m => m.JobOrderLineId == (item.JobOrderLineId)).FirstOrDefault();
                        JobOrderHeader JobOrderHeader = JobOrderHEaderRecords.Where(m => m.JobOrderHeaderId == (JobOrderLine.JobOrderHeaderId)).FirstOrDefault();


                        JobReceiveLine ReceiveLine = new JobReceiveLine();


                        if (item.JobReceiveLineId == 0)
                        {
                            JobReceiveHeader ReceiveHeader = new JobReceiveHeaderService(_unitOfWork).Find(Header.JobReceiveHeaderId.Value);

                            ReceiveLine.JobReceiveLineId = -Cnt;
                            ReceiveLine.JobReceiveHeaderId = Header.JobReceiveHeaderId.Value;
                            ReceiveLine.JobOrderLineId = item.JobOrderLineId;
                            ReceiveLine.ProductUidId = JobOrderLine.ProductUidId;
                            ReceiveLine.ProductId = JobOrderLine.ProductId;
                            ReceiveLine.Dimension1Id = JobOrderLine.Dimension1Id;
                            ReceiveLine.Dimension2Id = JobOrderLine.Dimension2Id;
                            ReceiveLine.Dimension3Id = JobOrderLine.Dimension3Id;
                            ReceiveLine.Dimension4Id = JobOrderLine.Dimension4Id;
                            ReceiveLine.Qty = item.ReceiveQty;
                            ReceiveLine.UnitConversionMultiplier = JobOrderLine.UnitConversionMultiplier;
                            ReceiveLine.DealUnitId = JobOrderLine.DealUnitId;
                            ReceiveLine.DealQty = ReceiveLine.UnitConversionMultiplier * ReceiveLine.Qty;
                            ReceiveLine.LossQty = item.LossQty;
                            ReceiveLine.PassQty = item.PassQty;
                            ReceiveLine.Sr = Serial;


                            if (JobOrderLineCharges.Where(m => m.LineTableId == item.JobOrderLineId && m.ChargeId == IncentiveId).FirstOrDefault() != null)
                            {
                                ReceiveLine.IncentiveRate = JobOrderLineCharges.Where(m => m.LineTableId == item.JobOrderLineId && m.ChargeId == IncentiveId).FirstOrDefault().Rate ?? 0;
                                ReceiveLine.IncentiveAmt = ReceiveLine.IncentiveRate * item.DealQty;
                            }
                            if (JobOrderLineCharges.Where(m => m.LineTableId == item.JobOrderLineId && m.ChargeId == PenaltyId).FirstOrDefault() != null)
                            {
                                ReceiveLine.PenaltyRate = JobOrderLineCharges.Where(m => m.LineTableId == item.JobOrderLineId && m.ChargeId == PenaltyId).FirstOrDefault().Rate ?? 0;
                                ReceiveLine.PenaltyAmt = ReceiveLine.PenaltyRate * item.DealQty;
                            }
                            ReceiveLine.LotNo = item.LotNo;
                            ReceiveLine.CreatedDate = DateTime.Now;
                            ReceiveLine.ModifiedDate = DateTime.Now;
                            ReceiveLine.CreatedBy = User.Identity.Name;
                            ReceiveLine.ModifiedBy = User.Identity.Name;

                            if (ReceiveLine.JobOrderLineId != null)
                            {
                                if (LineStatus.ContainsKey((int)ReceiveLine.JobOrderLineId))
                                {
                                    LineStatus[(int)ReceiveLine.JobOrderLineId] = LineStatus[(int)ReceiveLine.JobOrderLineId] + 1;
                                }
                                else
                                {
                                    LineStatus.Add((int)ReceiveLine.JobOrderLineId, ReceiveLine.Qty + ReceiveLine.LossQty);
                                }
                            }


                            if (JobOrderLine.ProductUidId.HasValue && JobOrderLine.ProductUidId.Value > 0)
                            {

                                ProductUid Uid = new ProductUidService(_unitOfWork).Find(JobOrderLine.ProductUidId.Value);

                                ReceiveLine.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                                ReceiveLine.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                                ReceiveLine.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                                ReceiveLine.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                                ReceiveLine.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                                ReceiveLine.ProductUidStatus = Uid.Status;
                                ReceiveLine.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                                ReceiveLine.ProductUidCurrentGodownId = Uid.CurrenctGodownId;

                                if (Header.JobWorkerId == Uid.LastTransactionPersonId || Header.SiteId == 17)
                                {

                                    Uid.LastTransactionDocId = ReceiveHeader.JobReceiveHeaderId;
                                    Uid.LastTransactionDocNo = ReceiveHeader.DocNo;
                                    Uid.LastTransactionDocTypeId = ReceiveHeader.DocTypeId;
                                    Uid.LastTransactionDocDate = ReceiveHeader.DocDate;
                                    Uid.LastTransactionPersonId = ReceiveHeader.JobWorkerId;
                                    Uid.CurrenctGodownId = ReceiveHeader.GodownId;
                                    Uid.CurrenctProcessId = ReceiveHeader.ProcessId;
                                    Uid.Status = (!string.IsNullOrEmpty(Settings.BarcodeStatusUpdate) ? Settings.BarcodeStatusUpdate : ProductUidStatusConstants.Receive);
                                    if (Uid.ProcessesDone == null)
                                    {
                                        Uid.ProcessesDone = "|" + ReceiveHeader.ProcessId.ToString() + "|";
                                    }
                                    else
                                    {
                                        Uid.ProcessesDone = Uid.ProcessesDone + ",|" + ReceiveHeader.ProcessId.ToString() + "|";
                                    }
                                    Uid.ObjectState = Model.ObjectState.Modified;
                                    db.ProductUid.Add(Uid);

                                }

                            }


                            if (Settings.isPostedInStock ?? false)
                            {
                                StockViewModel StockViewModel = new StockViewModel();

                                if (Cnt == 0)
                                {
                                    StockViewModel.StockHeaderId = ReceiveHeader.StockHeaderId ?? 0;
                                }
                                else
                                {
                                    if (ReceiveHeader.StockHeaderId != null && ReceiveHeader.StockHeaderId != 0)
                                    {
                                        StockViewModel.StockHeaderId = (int)ReceiveHeader.StockHeaderId;
                                    }
                                    else
                                    {
                                        StockViewModel.StockHeaderId = -1;
                                    }
                                }

                                StockViewModel.StockId = -Cnt;
                                StockViewModel.DocHeaderId = ReceiveHeader.JobReceiveHeaderId;
                                StockViewModel.DocLineId = ReceiveLine.JobReceiveLineId;
                                StockViewModel.DocTypeId = ReceiveHeader.DocTypeId;
                                StockViewModel.StockHeaderDocDate = ReceiveHeader.DocDate;
                                StockViewModel.StockDocDate = ReceiveHeader.DocDate;
                                StockViewModel.DocNo = ReceiveHeader.DocNo;
                                StockViewModel.DivisionId = ReceiveHeader.DivisionId;
                                StockViewModel.SiteId = ReceiveHeader.SiteId;
                                StockViewModel.CurrencyId = null;
                                StockViewModel.PersonId = ReceiveHeader.JobWorkerId;
                                StockViewModel.ProductId = JobOrderLine.ProductId;
                                StockViewModel.HeaderFromGodownId = null;
                                //Commented copying logic from single record entry
                                //StockViewModel.HeaderGodownId = ReceiveHeader.GodownId;
                                StockViewModel.HeaderGodownId = null;
                                StockViewModel.HeaderProcessId = ReceiveHeader.ProcessId;
                                StockViewModel.GodownId = (int)ReceiveHeader.GodownId;
                                StockViewModel.HeaderRemark = ReceiveHeader.Remark;
                                StockViewModel.Status = ReceiveHeader.Status;
                                StockViewModel.ProcessId = ReceiveHeader.ProcessId;
                                StockViewModel.LotNo = ReceiveLine.LotNo;

                                if (temp != null)
                                {
                                    StockViewModel.CostCenterId = temp.CostCenterId;
                                    StockViewModel.Rate = temp.Rate;
                                }
                                StockViewModel.Qty_Iss = 0;
                                StockViewModel.Qty_Rec = ReceiveLine.Qty;
                                StockViewModel.ExpiryDate = null;
                                StockViewModel.Specification = JobOrderLine.Specification;
                                StockViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                                StockViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                                StockViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                                StockViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                                StockViewModel.ProductUidId = ReceiveLine.ProductUidId;
                                StockViewModel.Remark = ReceiveLine.Remark;
                                StockViewModel.Status = ReceiveHeader.Status;
                                StockViewModel.CreatedBy = User.Identity.Name;
                                StockViewModel.CreatedDate = DateTime.Now;
                                StockViewModel.ModifiedBy = User.Identity.Name;
                                StockViewModel.ModifiedDate = DateTime.Now;

                                string StockPostingError = "";
                                StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                                if (StockPostingError != "")
                                {
                                    string message = StockPostingError;
                                    ModelState.AddModelError("", message);
                                    return PartialView("_Results", vm);
                                }

                                if (Cnt == 0)
                                {
                                    ReceiveHeader.StockHeaderId = StockViewModel.StockHeaderId;
                                }
                                ReceiveLine.StockId = StockViewModel.StockId;
                            }


                            if (Settings.isPostedInStockProcess ?? false)
                            {
                                StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                                if (ReceiveHeader.StockHeaderId != null && ReceiveHeader.StockHeaderId != 0)//If Transaction ReceiveHeader Table Has Stock ReceiveHeader Id Then It will Save Here.
                                {
                                    StockProcessViewModel.StockHeaderId = (int)ReceiveHeader.StockHeaderId;
                                }
                                else if (Settings.isPostedInStock ?? false)//If Stok ReceiveHeader is already posted during stock posting then this statement will Execute.So theat Stock ReceiveHeader will not generate again.
                                {
                                    StockProcessViewModel.StockHeaderId = -1;
                                }
                                else if (Cnt > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                                {
                                    StockProcessViewModel.StockHeaderId = -1;
                                }
                                else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                                {
                                    StockProcessViewModel.StockHeaderId = 0;
                                }

                                StockProcessViewModel.StockProcessId = -Cnt;
                                StockProcessViewModel.DocHeaderId = ReceiveHeader.JobReceiveHeaderId;
                                StockProcessViewModel.DocLineId = ReceiveLine.JobReceiveLineId;
                                StockProcessViewModel.DocTypeId = ReceiveHeader.DocTypeId;
                                StockProcessViewModel.StockHeaderDocDate = ReceiveHeader.DocDate;
                                StockProcessViewModel.StockProcessDocDate = ReceiveHeader.DocDate;
                                StockProcessViewModel.DocNo = ReceiveHeader.DocNo;
                                StockProcessViewModel.DivisionId = ReceiveHeader.DivisionId;
                                StockProcessViewModel.SiteId = ReceiveHeader.SiteId;
                                StockProcessViewModel.CurrencyId = null;
                                StockProcessViewModel.PersonId = ReceiveHeader.JobWorkerId;
                                StockProcessViewModel.ProductId = JobOrderLine.ProductId;
                                StockProcessViewModel.HeaderFromGodownId = null;
                                //Commented copying single entry logic
                                //StockProcessViewModel.HeaderProcessId = ReceiveHeader.ProcessId;
                                //StockProcessViewModel.HeaderGodownId = ReceiveHeader.GodownId;
                                StockProcessViewModel.HeaderGodownId = null;
                                StockProcessViewModel.HeaderProcessId = null;
                                StockProcessViewModel.GodownId = (int)ReceiveHeader.GodownId;
                                StockProcessViewModel.HeaderRemark = ReceiveHeader.Remark;
                                StockProcessViewModel.Status = ReceiveHeader.Status;
                                StockProcessViewModel.ProcessId = ReceiveHeader.ProcessId;
                                StockProcessViewModel.LotNo = ReceiveLine.LotNo;

                                if (temp != null)
                                {
                                    StockProcessViewModel.CostCenterId = temp.CostCenterId;
                                    StockProcessViewModel.Rate = temp.Rate;
                                }
                                StockProcessViewModel.Qty_Iss = ReceiveLine.Qty + ReceiveLine.LossQty;
                                StockProcessViewModel.Qty_Rec = 0;
                                StockProcessViewModel.ExpiryDate = null;
                                StockProcessViewModel.Specification = JobOrderLine.Specification;
                                StockProcessViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                                StockProcessViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                                StockProcessViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                                StockProcessViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                                StockProcessViewModel.Remark = ReceiveLine.Remark;
                                StockProcessViewModel.ProductUidId = ReceiveLine.ProductUidId;
                                StockProcessViewModel.Status = ReceiveHeader.Status;
                                StockProcessViewModel.CreatedBy = User.Identity.Name;
                                StockProcessViewModel.CreatedDate = DateTime.Now;
                                StockProcessViewModel.ModifiedBy = User.Identity.Name;
                                StockProcessViewModel.ModifiedDate = DateTime.Now;

                                string StockProcessPostingError = "";
                                StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                                if (StockProcessPostingError != "")
                                {
                                    string message = StockProcessPostingError;
                                    ModelState.AddModelError("", message);
                                    return PartialView("_Results", vm);
                                }

                                if ((Settings.isPostedInStock ?? false) == false)
                                {
                                    if (Cnt == 0)
                                    {
                                        ReceiveHeader.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                    }
                                }

                                ReceiveLine.StockProcessId = StockProcessViewModel.StockProcessId;
                            }



                            ReceiveLine.ObjectState = Model.ObjectState.Added;
                            db.JobReceiveLine.Add(ReceiveLine);
                            new JobReceiveLineStatusService(_unitOfWork).CreateLineStatusWithInvoice(ReceiveLine.JobReceiveLineId, ReceiveLine.PassQty, ReceiveLine.PassQty * JobOrderLine.UnitConversionMultiplier, ReceiveHeader.DocDate, ref db);


                            #region BomPost

                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            if (!string.IsNullOrEmpty(Settings.SqlProcConsumption))
                            {
                                var BomPostList = _JobReceiveLineService.GetBomPostingDataForWeaving(item.ProductId, item.Dimension1Id, item.Dimension2Id, null, null, ReceiveHeader.ProcessId, item.PassQty, ReceiveHeader.DocTypeId, Settings.SqlProcConsumption, ReceiveLine.JobOrderLineId, ReceiveLine.Weight).ToList();

                                foreach (var BomItem in BomPostList)
                                {
                                    JobReceiveBom BomPost = new JobReceiveBom();
                                    BomPost.JobReceiveBomId = -Cnt1;
                                    BomPost.JobReceiveHeaderId = ReceiveHeader.JobReceiveHeaderId;
                                    BomPost.JobReceiveLineId = ReceiveLine.JobReceiveLineId;
                                    BomPost.CreatedBy = User.Identity.Name;
                                    BomPost.CreatedDate = DateTime.Now;
                                    BomPost.ModifiedBy = User.Identity.Name;
                                    BomPost.ModifiedDate = DateTime.Now;
                                    BomPost.ProductId = BomItem.ProductId;
                                    BomPost.Qty = Convert.ToDecimal(BomItem.Qty);


                                    StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();
                                    if (ReceiveHeader.StockHeaderId != null && ReceiveHeader.StockHeaderId != 0)//If Transaction ReceiveHeader Table Has Stock ReceiveHeader Id Then It will Save Here.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = (int)ReceiveHeader.StockHeaderId;
                                    }
                                    else if (Settings.isPostedInStock ?? false)//If Stok ReceiveHeader is already posted during stock posting then this statement will Execute.So theat Stock ReceiveHeader will not generate again.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = -1;
                                    }
                                    else if (Cnt1 > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                                    {
                                        StockProcessBomViewModel.StockHeaderId = -1;
                                    }
                                    else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = 0;
                                    }

                                    StockProcessBomViewModel.StockProcessId = -Cnt1;
                                    StockProcessBomViewModel.DocHeaderId = ReceiveHeader.JobReceiveHeaderId;
                                    StockProcessBomViewModel.DocLineId = ReceiveLine.JobReceiveLineId;
                                    StockProcessBomViewModel.DocTypeId = ReceiveHeader.DocTypeId;
                                    StockProcessBomViewModel.StockHeaderDocDate = ReceiveHeader.DocDate;
                                    StockProcessBomViewModel.StockProcessDocDate = ReceiveHeader.DocDate;
                                    StockProcessBomViewModel.DocNo = ReceiveHeader.DocNo;
                                    StockProcessBomViewModel.DivisionId = ReceiveHeader.DivisionId;
                                    StockProcessBomViewModel.SiteId = ReceiveHeader.SiteId;
                                    StockProcessBomViewModel.CurrencyId = null;
                                    StockProcessBomViewModel.HeaderProcessId = null;
                                    StockProcessBomViewModel.PersonId = ReceiveHeader.JobWorkerId;
                                    StockProcessBomViewModel.ProductId = BomItem.ProductId;
                                    StockProcessBomViewModel.HeaderFromGodownId = null;
                                    StockProcessBomViewModel.HeaderGodownId = null;
                                    StockProcessBomViewModel.GodownId = ReceiveHeader.GodownId;
                                    StockProcessBomViewModel.ProcessId = ReceiveHeader.ProcessId;
                                    StockProcessBomViewModel.LotNo = null;
                                    StockProcessBomViewModel.CostCenterId = JobOrderHeader.CostCenterId;
                                    StockProcessBomViewModel.Qty_Iss = BomItem.Qty;
                                    StockProcessBomViewModel.Qty_Rec = 0;
                                    StockProcessBomViewModel.Rate = 0;
                                    StockProcessBomViewModel.ExpiryDate = null;
                                    StockProcessBomViewModel.Specification = null;
                                    StockProcessBomViewModel.Dimension1Id = null;
                                    StockProcessBomViewModel.Dimension2Id = null;
                                    StockProcessBomViewModel.Dimension3Id = null;
                                    StockProcessBomViewModel.Dimension4Id = null;
                                    StockProcessBomViewModel.Remark = null;
                                    StockProcessBomViewModel.Status = ReceiveHeader.Status;
                                    StockProcessBomViewModel.CreatedBy = User.Identity.Name;
                                    StockProcessBomViewModel.CreatedDate = DateTime.Now;
                                    StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
                                    StockProcessBomViewModel.ModifiedDate = DateTime.Now;

                                    string StockProcessPostingError = "";
                                    StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessBomViewModel, ref db);

                                    if (StockProcessPostingError != "")
                                    {
                                        ModelState.AddModelError("", StockProcessPostingError);
                                        return PartialView("_Results", vm);
                                    }

                                    BomPost.StockProcessId = StockProcessBomViewModel.StockProcessId;
                                    BomPost.ObjectState = Model.ObjectState.Added;
                                    //new JobReceiveBomService(_unitOfWork).Create(BomPost);
                                    db.JobReceiveBom.Add(BomPost);


                                    if (Settings.isPostedInStock == false && Settings.isPostedInStockProcess == false)
                                    {
                                        if (ReceiveHeader.StockHeaderId == null)
                                        {
                                            ReceiveHeader.StockHeaderId = StockProcessBomViewModel.StockHeaderId;
                                        }
                                    }
                                    Cnt1 = Cnt1 + 1;
                                }
                            }

                            #endregion


                            Cnt = Cnt + 1;

                            ReceiveHeader.ModifiedBy = User.Identity.Name;
                            ReceiveHeader.ModifiedDate = DateTime.Now;
                            ReceiveHeader.ObjectState = Model.ObjectState.Modified;
                            db.JobReceiveHeader.Add(ReceiveHeader);





                        }
                        else
                        {
                            ReceiveLine = new JobReceiveLineService(_unitOfWork).Find(item.JobReceiveLineId);
                        }

                        JobInvoiceLine line = new JobInvoiceLine();
                        line.JobInvoiceHeaderId = item.JobInvoiceHeaderId;
                        line.JobReceiveLineId = ReceiveLine.JobReceiveLineId;
                        line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        line.JobWorkerId = item.JobWorkerId;
                        line.Rate = item.Rate;
                        line.Sr = Serial++;
                        line.DealUnitId = item.DealUnitId;
                        line.Qty = item.PassQty;
                        line.DealQty = item.DealQty;
                        line.Amount = (item.DealQty * item.Rate);
                        line.IncentiveAmt = ReceiveLine.IncentiveAmt;
                        line.IncentiveRate = ReceiveLine.IncentiveRate;
                        line.CostCenterId = item.CostCenterId;
                        line.SalesTaxGroupProductId = item.SalesTaxGroupProductId;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.JobInvoiceLineId = pk;





                        line.ObjectState = Model.ObjectState.Added;
                        db.JobInvoiceLine.Add(line);

                        JobInvoiceLineStatus Status = new JobInvoiceLineStatus();
                        Status.JobInvoiceLineId = line.JobInvoiceLineId;
                        Status.ObjectState = Model.ObjectState.Added;
                        db.JobInvoiceLineStatus.Add(Status);

                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.JobInvoiceLineId, HeaderTableId = item.JobInvoiceHeaderId, PersonID = Header.JobWorkerId, DealQty = line.DealQty, CostCenterId = line.CostCenterId });
                        if (ReceiveLine.JobOrderLineId != null)
                        {
                            RefIds.Add(new LineReferenceIds { LineId = line.JobInvoiceLineId, RefLineId = (int)ReceiveLine.JobOrderLineId });
                        }


                        List<CalculationProductViewModel> ChargeRates = new CalculationProductService(_unitOfWork).GetChargeRates(CalculationId, Header.DocTypeId, Header.SiteId, Header.DivisionId,
                            Header.ProcessId, item.SalesTaxGroupPersonId, item.SalesTaxGroupProductId).ToList();
                        if (ChargeRates != null)
                        {
                            LineChargeRates.Add(new LineChargeRates { LineId = line.JobInvoiceLineId, ChargeRates = ChargeRates });
                        }

                        pk++;

                    }
                }

                int[] RecLineIds = null;
                RecLineIds = RefIds.Select(m => m.RefLineId).ToArray();

                var OrderLineCharges = (from p in db.JobOrderLine
                               where RecLineIds.Contains(p.JobOrderLineId)
                               join LineCharge in db.JobOrderLineCharge on p.JobOrderLineId equals LineCharge.LineTableId
                               join HeaderCharge in db.JobOrderHeaderCharges on p.JobOrderHeaderId equals HeaderCharge.HeaderTableId
                               group new { p, LineCharge, HeaderCharge } by new { p.JobOrderLineId } into g
                               select new
                               {
                                   LineId = g.Key.JobOrderLineId,
                                   HeaderCharges = g.Select(m => m.HeaderCharge).ToList(),
                                   Linecharges = g.Select(m => m.LineCharge).ToList(),
                               }).ToList();



                var LineListWithReferences = (from p in LineList
                                              join t in RefIds on p.LineTableId equals t.LineId
                                              join t2 in OrderLineCharges on t.RefLineId equals t2.LineId into OrderLineChargesTable
                                              from OrderLineChargesTab in OrderLineChargesTable.DefaultIfEmpty()
                                              join t3 in LineChargeRates on p.LineTableId equals t3.LineId into LineChargeRatesTable
                                              from LineChargeRatesTab in LineChargeRatesTable.DefaultIfEmpty()
                                              orderby p.LineTableId
                                              select new LineDetailListViewModel
                                              {
                                                  Amount = p.Amount,
                                                  DealQty = p.DealQty,
                                                  HeaderTableId = p.HeaderTableId,
                                                  LineTableId = p.LineTableId,
                                                  PersonID = p.PersonID,
                                                  Rate = p.Rate,
                                                  CostCenterId = p.CostCenterId,
                                                  RLineCharges = (OrderLineChargesTab == null ? null : Mapper.Map<List<LineChargeViewModel>>(OrderLineChargesTab.Linecharges)),
                                                  ChargeRates = LineChargeRatesTab.ChargeRates,
                                              }).ToList();


                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineListWithReferences, vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.JobInvoiceHeaderCharges", "Web.JobInvoiceLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                //Calculate Charges::::::
                //CalculateCharges(LineList, vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.JobInvoiceHeaderCharges", "Web.JobInvoiceLineCharges");


                //Saving Charges
                foreach (var item in LineCharges)
                {

                    JobInvoiceLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, JobInvoiceLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    db.JobInvoiceLineCharge.Add(PoLineCharge);
                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        JobInvoiceHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, JobInvoiceHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId;
                        if (PersonCount <= 1)
                            POHeaderCharge.PersonID = vm.JobInvoiceLineViewModel.FirstOrDefault().JobWorkerId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        db.JobInvoiceHeaderCharges.Add(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new JobInvoiceHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        if (PersonCount > 1 || footercharge.PersonID != vm.JobInvoiceLineViewModel.FirstOrDefault().JobWorkerId)
                            footercharge.PersonID = null;

                        footercharge.ObjectState = Model.ObjectState.Modified;
                        db.JobInvoiceHeaderCharges.Add(footercharge);
                    }

                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    //ReceiveHeader.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }
                Header.ObjectState = Model.ObjectState.Modified;
                db.JobInvoiceHeader.Add(Header);



                //new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyInvoiceMultiple(LineStatus, Header.DocDate, ref db);

                try
                {
                    JobInvoiceReceiveDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId), ref db);
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
                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_OrderResults", vm);
                }

                try
                {
                    JobInvoiceReceiveDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_OrderResults", vm);

        }



        private void PrepareViewBag(JobInvoiceLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            if (vm != null)
            {
                var temp = new JobInvoiceHeaderService(_unitOfWork).Find(vm.JobInvoiceHeaderId);
                ViewBag.DocNo = temp.DocNo;
            }
        }

        [HttpGet]
        public JsonResult ConsumptionIndex(int id)
        {
            var p = _JobReceiveLineService.GetConsumptionLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult ByProductIndex(int id)
        {
            var p = _JobReceiveLineService.GetByProductListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public ActionResult CreateLine(int Id, int? JobWorkerId, string LineNature)
        {
            return _Create(Id, JobWorkerId, LineNature);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int Id, int? JobWorkerId, string LineNature)
        {
            return _Create(Id, JobWorkerId, LineNature);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int Id, int? JobWorkerId, string LineNature)
        {
            return _Create(Id, JobWorkerId, LineNature);
        }


        public ActionResult _Create(int Id, int? JobWorkerId, string LineNature) //Id ==>Job Invoice Header Id
        {
            JobInvoiceHeader H = new JobInvoiceHeaderService(_unitOfWork).Find(Id);
            JobInvoiceLineViewModel s = new JobInvoiceLineViewModel();

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            
            if (H.SalesTaxGroupPersonId != null)
                s.CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(H.DocTypeId, (int)H.SalesTaxGroupPersonId, H.SiteId, H.DivisionId);

            if (s.CalculationId == null)
                s.CalculationId = settings.CalculationId;


            s.DocTypeId = H.DocTypeId;
            s.SiteId = H.SiteId;
            if (H.JobReceiveHeaderId != null)
                s.JobReceiveHeaderId = H.JobReceiveHeaderId.Value;

            s.DivisionId = H.DivisionId;

            if (JobWorkerId.HasValue)
                s.JobWorkerId = JobWorkerId.Value;
            
            s.JobInvoiceHeaderId = H.JobInvoiceHeaderId;
            s.SalesTaxGroupPersonId = H.SalesTaxGroupPersonId;

            //if (settings.isVisibleJobReceive == true || settings.isMandatoryJobReceive == true)
            //    s.LineNature = LineNatureConstants.ForReceive;
            //else if (settings.isVisibleJobOrder == true || settings.isMandatoryJobOrder == true)
            //    s.LineNature = LineNatureConstants.ForOrder;
            //else
            //    s.LineNature = LineNatureConstants.Direct;


            s.LineNature = LineNature;

            s.PassQty = 0;
            s.Rate = 0;

            PrepareViewBag(null);
            ViewBag.DocNo = H.DocNo;
            ViewBag.LineMode = "Create";
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobInvoiceLineViewModel svm)
        {
            #region BeforeSave
            bool BeforeSave = true;
            try
            {

                if (svm.JobInvoiceLineId <= 0)
                    BeforeSave = JobInvoiceReceiveDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobInvoiceHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobInvoiceReceiveDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobInvoiceHeaderId, EventModeConstants.Edit), ref db);

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

            JobInvoiceLine InvoiceLine = Mapper.Map<JobInvoiceLineViewModel, JobInvoiceLine>(svm);
            //JobReceiveLine ReceiveLine = Mapper.Map<JobInvoiceLineViewModel, JobReceiveLine>(svm);
            JobInvoiceHeader InvoiceHeader = new JobInvoiceHeaderService(_unitOfWork).Find(InvoiceLine.JobInvoiceHeaderId);
            

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(InvoiceHeader.DocTypeId, InvoiceHeader.DivisionId, InvoiceHeader.SiteId);
            //var jobreceivesettings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(ReceiveHeader.DocTypeId, ReceiveHeader.DivisionId, ReceiveHeader.SiteId);


            if (settings.isMandatoryJobOrder ?? true)
            {
                if (svm.JobOrderLineId <= 0)
                {
                    ModelState.AddModelError("JobOrderLineId", "Job Order field is required");
                }
            }


            if ((settings.IsVisibleReceiveQty ?? false) == true && svm.LineNature != LineNatureConstants.AdditionalCharges)
            {
                if (svm.ReceiveQty <= 0)
                    ModelState.AddModelError("ReceiveQty", "Receive Qty field is required");
            }



            #region "Tax Calculation Validation"
            SiteDivisionSettings SiteDivisionSettings = new SiteDivisionSettingsService(_unitOfWork).GetSiteDivisionSettings(InvoiceHeader.SiteId, InvoiceHeader.DivisionId, InvoiceHeader.DocDate);
            if (SiteDivisionSettings != null)
            {
                if (SiteDivisionSettings.IsApplicableGST == true)
                {
                    if (svm.SalesTaxGroupPersonId == 0 || svm.SalesTaxGroupPersonId == null)
                    {
                        ModelState.AddModelError("", "Sales Tax Group Person is not defined for party, it is required.");
                    }

                    if (svm.SalesTaxGroupProductId == 0 || svm.SalesTaxGroupProductId == null)
                    {
                        ModelState.AddModelError("", "Sales Tax Group Product is not defined for product, it is required.");
                    }

                    if (svm.SalesTaxGroupProductId != 0 && svm.SalesTaxGroupProductId != null && svm.SalesTaxGroupPersonId != 0 && svm.SalesTaxGroupPersonId != null && svm.CalculationId != null)
                    {
                        IEnumerable<ChargeRateSettings> ChargeRateSettingsList = new CalculationProductService(_unitOfWork).GetChargeRateSettingForValidation((int)svm.CalculationId, InvoiceHeader.DocTypeId, InvoiceHeader.SiteId, InvoiceHeader.DivisionId, InvoiceHeader.ProcessId, (int)svm.SalesTaxGroupPersonId, (int)svm.SalesTaxGroupProductId);

                        foreach (var item in ChargeRateSettingsList)
                        {
                            if (item.ChargeGroupSettingId == null)
                            {
                                ModelState.AddModelError("", "Charge Group Setting is not defined for " + item.ChargeName + ".");
                            }

                            if (item.LedgerAccountCrName == LedgerAccountConstants.Charge || item.LedgerAccountDrName == LedgerAccountConstants.Charge)
                            {
                                if (item.ChargeGroupSettingId != null && item.ChargePer != 0 && item.ChargePer != null && item.ChargeLedgerAccountId == null)
                                {
                                    ModelState.AddModelError("", "Ledger account is not defined for " + item.ChargeName + " in charge group settings.");
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            if (svm.JobInvoiceLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (svm.JobInvoiceLineId <= 0)
                {

                    JobReceiveLine ReceiveLine = new JobReceiveLine();

                    if (svm.JobReceiveLineId == 0)
                    {
                        if (InvoiceHeader.JobReceiveHeaderId != null)
                        {
                            JobReceiveHeader ReceiveHeader = new JobReceiveHeaderService(_unitOfWork).Find((int)InvoiceHeader.JobReceiveHeaderId);



                            StockViewModel StockViewModel = new StockViewModel();
                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                            //JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(ReceiveLine.JobOrderLineId);
                            //JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);


                            //Product Uid Generation
                            if (svm.ProductUidId == null && settings.isGenerateProductUid == true)
                            {
                                ProductUidHeader ProductUidHeader = new ProductUidHeader();
                                ProductUidHeader.ProductId = svm.ProductId;
                                ProductUidHeader.GenDocId = ReceiveHeader.JobReceiveHeaderId;
                                ProductUidHeader.GenDocNo = ReceiveHeader.DocNo;
                                ProductUidHeader.GenDocTypeId = ReceiveHeader.DocTypeId;
                                ProductUidHeader.GenDocDate = ReceiveHeader.DocDate;
                                ProductUidHeader.GenPersonId = ReceiveHeader.JobWorkerId;
                                ProductUidHeader.CreatedBy = User.Identity.Name;
                                ProductUidHeader.CreatedDate = DateTime.Now;
                                ProductUidHeader.ModifiedBy = User.Identity.Name;
                                ProductUidHeader.ModifiedDate = DateTime.Now;
                                ProductUidHeader.ObjectState = Model.ObjectState.Added;
                                db.ProductUidHeader.Add(ProductUidHeader);
                                ReceiveLine.ProductUidHeaderId = ProductUidHeader.ProductUidHeaderId;



                                ProductUid ProductUid = new ProductUid();
                                ProductUid.ProductUidHeaderId = ProductUidHeader.ProductUidHeaderId;
                                ProductUid.ProductUidName = svm.ProductUidName;
                                ProductUid.ProductId = svm.ProductId;
                                ProductUid.ProductUidSpecification = svm.Specification;
                                ProductUid.IsActive = true;
                                ProductUid.CreatedBy = User.Identity.Name;
                                ProductUid.CreatedDate = DateTime.Now;
                                ProductUid.ModifiedBy = User.Identity.Name;
                                ProductUid.ModifiedDate = DateTime.Now;
                                ProductUid.GenLineId = null;
                                ProductUid.GenDocId = ReceiveHeader.JobReceiveHeaderId;
                                ProductUid.GenDocNo = ReceiveHeader.DocNo;
                                ProductUid.GenDocTypeId = ReceiveHeader.DocTypeId;
                                ProductUid.GenDocDate = ReceiveHeader.DocDate;
                                ProductUid.GenPersonId = ReceiveHeader.JobWorkerId;
                                ProductUid.CurrenctProcessId = ReceiveHeader.ProcessId;
                                ProductUid.CurrenctGodownId = ReceiveHeader.GodownId;
                                ProductUid.Status = ProductUidStatusConstants.Receive;
                                ProductUid.LastTransactionDocId = ReceiveHeader.JobReceiveHeaderId;
                                ProductUid.LastTransactionDocNo = ReceiveHeader.DocNo;
                                ProductUid.LastTransactionDocTypeId = ReceiveHeader.DocTypeId;
                                ProductUid.LastTransactionDocDate = ReceiveHeader.DocDate;
                                ProductUid.LastTransactionPersonId = ReceiveHeader.JobWorkerId;
                                ProductUid.LastTransactionLineId = null;
                                ProductUid.ObjectState = Model.ObjectState.Added;
                                db.ProductUid.Add(ProductUid);
                                ReceiveLine.ProductUidId = ProductUid.ProductUIDId;
                            }

                            ReceiveLine.JobOrderLineId = svm.JobOrderLineId;
                            ReceiveLine.ProductUidId = svm.ProductUidId;
                            ReceiveLine.ProductId = svm.ProductId;
                            ReceiveLine.Dimension1Id = svm.Dimension1Id;
                            ReceiveLine.Dimension2Id = svm.Dimension2Id;
                            ReceiveLine.Dimension3Id = svm.Dimension3Id;
                            ReceiveLine.Dimension4Id = svm.Dimension4Id;
                            ReceiveLine.Qty = svm.ReceiveQty;
                            ReceiveLine.LossQty = svm.LossQty;
                            ReceiveLine.JobReceiveHeaderId = ReceiveHeader.JobReceiveHeaderId;
                            ReceiveLine.PassQty = svm.PassQty;
                            ReceiveLine.Weight = svm.Weight;
                            ReceiveLine.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                            ReceiveLine.DealUnitId = svm.DealUnitId;
                            ReceiveLine.DealQty = svm.DealQty;
                            ReceiveLine.IncentiveRate = svm.IncentiveRate;
                            ReceiveLine.IncentiveAmt = svm.IncentiveAmt;
                            ReceiveLine.PenaltyAmt = svm.PenaltyAmt;
                            ReceiveLine.PenaltyRate = svm.PenaltyRate;
                            ReceiveLine.LockReason = "Job invoice is created.";
                            ReceiveLine.CreatedDate = DateTime.Now;
                            ReceiveLine.ModifiedDate = DateTime.Now;
                            ReceiveLine.CreatedBy = User.Identity.Name;
                            ReceiveLine.ModifiedBy = User.Identity.Name;
                            ReceiveLine.Sr = _JobReceiveLineService.GetMaxSr(ReceiveLine.JobReceiveHeaderId);

                            //Posting in Stock
                            if (settings.isPostedInStock.HasValue && settings.isPostedInStock == true && svm.LineNature != LineNatureConstants.AdditionalCharges)
                            {
                                StockViewModel.StockHeaderId = ReceiveHeader.StockHeaderId ?? 0;
                                StockViewModel.DocHeaderId = ReceiveHeader.JobReceiveHeaderId;
                                StockViewModel.DocLineId = ReceiveLine.JobReceiveLineId;
                                StockViewModel.DocTypeId = ReceiveHeader.DocTypeId;
                                StockViewModel.StockHeaderDocDate = ReceiveHeader.DocDate;
                                StockViewModel.StockDocDate = ReceiveHeader.DocDate;
                                StockViewModel.DocNo = ReceiveHeader.DocNo;
                                StockViewModel.DivisionId = ReceiveHeader.DivisionId;
                                StockViewModel.SiteId = ReceiveHeader.SiteId;
                                StockViewModel.CurrencyId = null;
                                StockViewModel.HeaderProcessId = null;
                                StockViewModel.PersonId = ReceiveHeader.JobWorkerId;
                                StockViewModel.ProductId = svm.ProductId;
                                StockViewModel.HeaderFromGodownId = null;
                                StockViewModel.HeaderGodownId = null;
                                StockViewModel.GodownId = ReceiveHeader.GodownId;
                                StockViewModel.ProcessId = ReceiveHeader.ProcessId;
                                StockViewModel.LotNo = ReceiveLine.LotNo;
                                StockViewModel.CostCenterId = svm.CostCenterId;
                                StockViewModel.Qty_Iss = 0;
                                StockViewModel.Qty_Rec = ReceiveLine.Qty;
                                StockViewModel.Rate = svm.Rate;
                                StockViewModel.ExpiryDate = null;
                                StockViewModel.Specification = svm.Specification;
                                StockViewModel.Dimension1Id = svm.Dimension1Id;
                                StockViewModel.Dimension2Id = svm.Dimension2Id;
                                StockViewModel.Dimension3Id = svm.Dimension3Id;
                                StockViewModel.Dimension4Id = svm.Dimension4Id;
                                StockViewModel.HeaderRemark = ReceiveHeader.Remark;
                                StockViewModel.Remark = ReceiveLine.Remark;
                                StockViewModel.ProductUidId = ReceiveLine.ProductUidId;
                                StockViewModel.Status = ReceiveHeader.Status;
                                StockViewModel.CreatedBy = ReceiveHeader.CreatedBy;
                                StockViewModel.CreatedDate = DateTime.Now;
                                StockViewModel.ModifiedBy = ReceiveHeader.ModifiedBy;
                                StockViewModel.ModifiedDate = DateTime.Now;

                                string StockPostingError = "";
                                StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                                if (StockPostingError != "")
                                {
                                    ModelState.AddModelError("", StockPostingError);
                                    return PartialView("_Create", svm);
                                }

                                ReceiveLine.StockId = StockViewModel.StockId;

                                if (ReceiveHeader.StockHeaderId == null)
                                {
                                    ReceiveHeader.StockHeaderId = StockViewModel.StockHeaderId;
                                }
                            }




                            //Posting in StockProcess
                            if (settings.isPostedInStockProcess.HasValue && settings.isPostedInStockProcess == true && svm.LineNature != LineNatureConstants.AdditionalCharges)
                            {
                                if (ReceiveHeader.StockHeaderId != null && ReceiveHeader.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                                {
                                    StockProcessViewModel.StockHeaderId = (int)ReceiveHeader.StockHeaderId;
                                }
                                else if (settings.isPostedInStock.HasValue && settings.isPostedInStock == true)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                                {
                                    StockProcessViewModel.StockHeaderId = -1;
                                }
                                else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                                {
                                    StockProcessViewModel.StockHeaderId = 0;
                                }
                                StockProcessViewModel.DocHeaderId = ReceiveHeader.JobReceiveHeaderId;
                                StockProcessViewModel.DocLineId = ReceiveLine.JobReceiveLineId;
                                StockProcessViewModel.DocTypeId = ReceiveHeader.DocTypeId;
                                StockProcessViewModel.StockHeaderDocDate = ReceiveHeader.DocDate;
                                StockProcessViewModel.StockProcessDocDate = ReceiveHeader.DocDate;
                                StockProcessViewModel.DocNo = ReceiveHeader.DocNo;
                                StockProcessViewModel.DivisionId = ReceiveHeader.DivisionId;
                                StockProcessViewModel.SiteId = ReceiveHeader.SiteId;
                                StockProcessViewModel.CurrencyId = null;
                                StockProcessViewModel.HeaderProcessId = null;
                                StockProcessViewModel.PersonId = ReceiveHeader.JobWorkerId;
                                StockProcessViewModel.ProductId = svm.ProductId;
                                StockProcessViewModel.HeaderFromGodownId = null;
                                StockProcessViewModel.HeaderGodownId = null;
                                StockProcessViewModel.GodownId = ReceiveHeader.GodownId;
                                StockProcessViewModel.ProcessId = ReceiveHeader.ProcessId;
                                StockProcessViewModel.LotNo = ReceiveLine.LotNo;
                                StockProcessViewModel.CostCenterId = svm.CostCenterId;
                                StockProcessViewModel.Qty_Iss = ReceiveLine.Qty + ReceiveLine.LossQty;
                                StockProcessViewModel.Qty_Rec = 0;
                                StockProcessViewModel.Rate = svm.Rate;
                                StockProcessViewModel.ExpiryDate = null;
                                StockProcessViewModel.Specification = svm.Specification;
                                StockProcessViewModel.Dimension1Id = svm.Dimension1Id;
                                StockProcessViewModel.Dimension2Id = svm.Dimension2Id;
                                StockProcessViewModel.Dimension3Id = svm.Dimension3Id;
                                StockProcessViewModel.Dimension4Id = svm.Dimension4Id;
                                StockProcessViewModel.HeaderRemark = ReceiveHeader.Remark;
                                StockProcessViewModel.Remark = ReceiveLine.Remark;
                                StockProcessViewModel.ProductUidId = ReceiveLine.ProductUidId;
                                StockProcessViewModel.Status = ReceiveHeader.Status;
                                StockProcessViewModel.CreatedBy = User.Identity.Name;
                                StockProcessViewModel.CreatedDate = DateTime.Now;
                                StockProcessViewModel.ModifiedBy = User.Identity.Name;
                                StockProcessViewModel.ModifiedDate = DateTime.Now;

                                string StockProcessPostingError = "";
                                StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                                if (StockProcessPostingError != "")
                                {
                                    ModelState.AddModelError("", StockProcessPostingError);
                                    return PartialView("_Create", svm);
                                }

                                ReceiveLine.StockProcessId = StockProcessViewModel.StockProcessId;

                                if (settings.isPostedInStock == false)
                                {
                                    if (ReceiveHeader.StockHeaderId == null)
                                    {
                                        ReceiveHeader.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                    }
                                }
                            }

                            #region BomPost

                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            if (!string.IsNullOrEmpty(svm.JobInvoiceSettings.SqlProcConsumption))
                            {
                                var BomPostList = _JobReceiveLineService.GetBomPostingDataForWeaving(svm.ProductId, svm.Dimension1Id, svm.Dimension2Id, null, null, ReceiveHeader.ProcessId, ReceiveLine.PassQty, ReceiveHeader.DocTypeId, svm.JobInvoiceSettings.SqlProcConsumption, ReceiveLine.JobOrderLineId, ReceiveLine.Weight).ToList();

                                foreach (var item in BomPostList)
                                {
                                    JobReceiveBom BomPost = new JobReceiveBom();
                                    BomPost.JobReceiveHeaderId = ReceiveHeader.JobReceiveHeaderId;
                                    BomPost.JobReceiveLineId = ReceiveLine.JobReceiveLineId;
                                    BomPost.CreatedBy = User.Identity.Name;
                                    BomPost.CreatedDate = DateTime.Now;
                                    BomPost.ModifiedBy = User.Identity.Name;
                                    BomPost.ModifiedDate = DateTime.Now;
                                    BomPost.ProductId = item.ProductId;
                                    BomPost.Qty = Convert.ToDecimal(item.Qty);



                                    StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();
                                    if (ReceiveHeader.StockHeaderId != null && ReceiveHeader.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = (int)ReceiveHeader.StockHeaderId;
                                    }
                                    else if (svm.JobInvoiceSettings.isPostedInStock)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = -1;
                                    }
                                    else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = 0;
                                    }
                                    StockProcessBomViewModel.StockProcessId = -1;
                                    StockProcessBomViewModel.DocHeaderId = ReceiveHeader.JobReceiveHeaderId;
                                    StockProcessBomViewModel.DocLineId = ReceiveLine.JobReceiveLineId;
                                    StockProcessBomViewModel.DocTypeId = ReceiveHeader.DocTypeId;
                                    StockProcessBomViewModel.StockHeaderDocDate = ReceiveHeader.DocDate;
                                    StockProcessBomViewModel.StockProcessDocDate = ReceiveHeader.DocDate;
                                    StockProcessBomViewModel.DocNo = ReceiveHeader.DocNo;
                                    StockProcessBomViewModel.DivisionId = ReceiveHeader.DivisionId;
                                    StockProcessBomViewModel.SiteId = ReceiveHeader.SiteId;
                                    StockProcessBomViewModel.CurrencyId = null;
                                    StockProcessBomViewModel.HeaderProcessId = null;
                                    StockProcessBomViewModel.PersonId = ReceiveHeader.JobWorkerId;
                                    StockProcessBomViewModel.ProductId = item.ProductId;
                                    StockProcessBomViewModel.HeaderFromGodownId = null;
                                    StockProcessBomViewModel.HeaderGodownId = null;
                                    StockProcessBomViewModel.GodownId = ReceiveHeader.GodownId;
                                    StockProcessBomViewModel.ProcessId = ReceiveHeader.ProcessId;
                                    StockProcessBomViewModel.LotNo = ReceiveLine.LotNo;
                                    StockProcessBomViewModel.CostCenterId = svm.CostCenterId;
                                    StockProcessBomViewModel.Qty_Iss = item.Qty;
                                    StockProcessBomViewModel.Qty_Rec = 0;
                                    StockProcessBomViewModel.Rate = 0;
                                    StockProcessBomViewModel.ExpiryDate = null;
                                    StockProcessBomViewModel.Specification = null;
                                    StockProcessBomViewModel.Dimension1Id = null;
                                    StockProcessBomViewModel.Dimension2Id = null;
                                    StockProcessBomViewModel.Dimension3Id = null;
                                    StockProcessBomViewModel.Dimension4Id = null;
                                    StockProcessBomViewModel.Remark = null;
                                    StockProcessBomViewModel.Status = ReceiveHeader.Status;
                                    StockProcessBomViewModel.CreatedBy = User.Identity.Name;
                                    StockProcessBomViewModel.CreatedDate = DateTime.Now;
                                    StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
                                    StockProcessBomViewModel.ModifiedDate = DateTime.Now;

                                    string StockProcessPostingError = "";
                                    StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessBomViewModel, ref db);

                                    if (StockProcessPostingError != "")
                                    {
                                        ModelState.AddModelError("", StockProcessPostingError);
                                        return PartialView("_Create", svm);
                                    }

                                    BomPost.StockProcessId = StockProcessBomViewModel.StockProcessId;
                                    BomPost.ObjectState = Model.ObjectState.Added;
                                    db.JobReceiveBom.Add(BomPost);
                                    //new JobReceiveBomService(_unitOfWork).Create(BomPost);

                                    if (svm.JobInvoiceSettings.isPostedInStock == false && svm.JobInvoiceSettings.isPostedInStockProcess == false)
                                    {
                                        if (ReceiveHeader.StockHeaderId == null)
                                        {
                                            ReceiveHeader.StockHeaderId = StockProcessBomViewModel.StockHeaderId;
                                        }
                                    }


                                }
                            }

                            #endregion

                            if (svm.ProductUidId != null && svm.ProductUidId > 0)
                            {
                                ProductUid Produid = (from p in db.ProductUid
                                                      where p.ProductUIDId == svm.ProductUidId
                                                      select p).FirstOrDefault();


                                ReceiveLine.ProductUidLastTransactionDocId = Produid.LastTransactionDocId;
                                ReceiveLine.ProductUidLastTransactionDocDate = Produid.LastTransactionDocDate;
                                ReceiveLine.ProductUidLastTransactionDocNo = Produid.LastTransactionDocNo;
                                ReceiveLine.ProductUidLastTransactionDocTypeId = Produid.LastTransactionDocTypeId;
                                ReceiveLine.ProductUidLastTransactionPersonId = Produid.LastTransactionPersonId;
                                ReceiveLine.ProductUidStatus = Produid.Status;
                                ReceiveLine.ProductUidCurrentProcessId = Produid.CurrenctProcessId;
                                ReceiveLine.ProductUidCurrentGodownId = Produid.CurrenctGodownId;



                                Produid.LastTransactionDocId = ReceiveHeader.JobReceiveHeaderId;
                                Produid.LastTransactionDocNo = ReceiveHeader.DocNo;
                                Produid.LastTransactionDocTypeId = ReceiveHeader.DocTypeId;
                                Produid.LastTransactionDocDate = ReceiveHeader.DocDate;
                                Produid.LastTransactionPersonId = ReceiveHeader.JobWorkerId;
                                Produid.CurrenctGodownId = ReceiveHeader.GodownId;
                                Produid.CurrenctProcessId = ReceiveHeader.ProcessId;
                                Produid.Status = (!string.IsNullOrEmpty(settings.BarcodeStatusUpdate) ? settings.BarcodeStatusUpdate : ProductUidStatusConstants.Receive);

                                if (Produid.ProcessesDone == null)
                                {
                                    Produid.ProcessesDone = "|" + ReceiveHeader.ProcessId.ToString() + "|";
                                }
                                else
                                {
                                    Produid.ProcessesDone = Produid.ProcessesDone + ",|" + ReceiveHeader.ProcessId.ToString() + "|";
                                }

                                Produid.ObjectState = Model.ObjectState.Modified;
                                db.ProductUid.Add(Produid);
                            }

                            ReceiveLine.ObjectState = Model.ObjectState.Added;
                            db.JobReceiveLine.Add(ReceiveLine);

                            new JobReceiveLineStatusService(_unitOfWork).CreateLineStatusWithInvoice(ReceiveLine.JobReceiveLineId, (ReceiveLine.PassQty + ReceiveLine.LossQty), (ReceiveLine.PassQty + ReceiveLine.LossQty) * svm.UnitConversionMultiplier, InvoiceHeader.DocDate, ref db);

                            ReceiveHeader.ModifiedBy = User.Identity.Name;
                            ReceiveHeader.ModifiedDate = DateTime.Now;
                            ReceiveHeader.ObjectState = Model.ObjectState.Modified;
                            db.JobReceiveHeader.Add(ReceiveHeader);
                        }
                    }
                    else
                    {
                        ReceiveLine = new JobReceiveLineService(_unitOfWork).Find(svm.JobReceiveLineId);
                    }


                        InvoiceLine.JobReceiveLineId = ReceiveLine.JobReceiveLineId;
                        InvoiceLine.CreatedDate = DateTime.Now;
                        InvoiceLine.ModifiedDate = DateTime.Now;
                        InvoiceLine.Sr = _JobInvoiceLineService.GetMaxSr(InvoiceLine.JobInvoiceHeaderId);
                        InvoiceLine.Qty = svm.PassQty;
                        InvoiceLine.IncentiveAmt = ReceiveLine.IncentiveAmt;
                        InvoiceLine.IncentiveRate = ReceiveLine.IncentiveRate;
                        InvoiceLine.CreatedBy = User.Identity.Name;
                        InvoiceLine.ModifiedBy = User.Identity.Name;
                        InvoiceLine.ObjectState = Model.ObjectState.Added;
                        //_JobInvoiceLineService.Create(InvoiceLine);


                        JobInvoiceLineStatus Status = new JobInvoiceLineStatus();
                        Status.JobInvoiceLineId = InvoiceLine.JobInvoiceLineId;
                        Status.ObjectState = Model.ObjectState.Added;
                        db.JobInvoiceLineStatus.Add(Status);

                        //if (ReceiveLine.JobOrderLineId != null)
                        //{
                        //    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnInvoiceReceive((int)ReceiveLine.JobOrderLineId, InvoiceLine.JobInvoiceLineId, InvoiceHeader.DocDate, (ReceiveLine.Qty + ReceiveLine.LossQty), InvoiceLine.UnitConversionMultiplier, ref db, true);
                        //}

                        //if (ReceiveLine.JobOrderLineId != null)
                        //{
                        //    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnInvoice((int)ReceiveLine.JobOrderLineId, InvoiceLine.JobInvoiceLineId, InvoiceHeader.DocDate, InvoiceLine.Qty, InvoiceLine.UnitConversionMultiplier, ref db, true);
                        //}
                        //new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnInvoice(InvoiceLine.JobReceiveLineId, InvoiceLine.JobInvoiceLineId, InvoiceHeader.DocDate, InvoiceLine.Qty, ref db, true);

                        db.JobInvoiceLine.Add(InvoiceLine);



                        if (svm.linecharges != null)
                            foreach (var item in svm.linecharges)
                            {
                                item.LineTableId = InvoiceLine.JobInvoiceLineId;
                                item.PersonID = InvoiceLine.JobWorkerId;
                                item.HeaderTableId = InvoiceLine.JobInvoiceHeaderId;
                                item.CostCenterId = InvoiceLine.CostCenterId;
                                item.ObjectState = Model.ObjectState.Added;
                                //new JobInvoiceLineChargeService(_unitOfWork).Create(item);
                                db.JobInvoiceLineCharge.Add(item);
                            }

                        if (svm.footercharges != null)
                        {
                            int PersonCount = (from p in db.JobInvoiceLine
                                               where p.JobInvoiceHeaderId == InvoiceLine.JobInvoiceHeaderId
                                               group p by p.JobWorkerId into g
                                               select g).Count();

                            foreach (var item in svm.footercharges)
                            {

                                if (item.Id > 0)
                                {


                                    var footercharge = new JobInvoiceHeaderChargeService(_unitOfWork).Find(item.Id);
                                    if (PersonCount > 1 || footercharge.PersonID != InvoiceLine.JobWorkerId)
                                        footercharge.PersonID = null;

                                    footercharge.Rate = item.Rate;
                                    footercharge.Amount = item.Amount;
                                    footercharge.ObjectState = Model.ObjectState.Modified;
                                    db.JobInvoiceHeaderCharges.Add(footercharge);
                                    //new JobInvoiceHeaderChargeService(_unitOfWork).Update(footercharge);

                                }

                                else
                                {
                                    item.HeaderTableId = InvoiceLine.JobInvoiceHeaderId;
                                    item.PersonID = InvoiceLine.JobWorkerId;
                                    item.ObjectState = Model.ObjectState.Added;
                                    db.JobInvoiceHeaderCharges.Add(item);
                                    //new JobInvoiceHeaderChargeService(_unitOfWork).Create(item);
                                }
                            }

                        }

                        if (InvoiceHeader.Status != (int)StatusConstants.Drafted && InvoiceHeader.Status != (int)StatusConstants.Import)
                        {
                            InvoiceHeader.Status = (int)StatusConstants.Modified;
                            InvoiceHeader.ModifiedBy = User.Identity.Name;
                            InvoiceHeader.ModifiedDate = DateTime.Now;

                        }


                        //new JobInvoiceHeaderService(_unitOfWork).Update(InvoiceHeader);
                        //new JobReceiveHeaderService(_unitOfWork).Update(ReceiveHeader);

                        InvoiceHeader.ObjectState = Model.ObjectState.Modified;


                        db.JobInvoiceHeader.Add(InvoiceHeader);
                        

                        try
                        {
                            JobInvoiceReceiveDocEvents.onLineSaveEvent(this, new JobEventArgs(InvoiceLine.JobInvoiceHeaderId, InvoiceLine.JobInvoiceLineId, EventModeConstants.Add), ref db);
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
                            //_unitOfWork.Save();
                        }

                        catch (Exception ex)
                        {
                            string message = _exception.HandleException(ex);
                            TempData["CSEXCL"] += message;
                            PrepareViewBag(null);
                            ViewBag.DocNo = InvoiceHeader.DocNo;
                            return PartialView("_Create", svm);
                        }

                        try
                        {
                            JobInvoiceReceiveDocEvents.afterLineSaveEvent(this, new JobEventArgs(InvoiceLine.JobInvoiceHeaderId, InvoiceLine.JobInvoiceLineId, EventModeConstants.Add), ref db);
                        }
                        catch (Exception ex)
                        {
                            string message = _exception.HandleException(ex);
                            TempData["CSEXCL"] += message;
                        }

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = InvoiceHeader.DocTypeId,
                            DocId = InvoiceHeader.JobInvoiceHeaderId,
                            DocLineId = InvoiceLine.JobInvoiceLineId,
                            ActivityType = (int)ActivityTypeContants.Added,
                            DocNo = InvoiceHeader.DocNo,
                            DocDate = InvoiceHeader.DocDate,
                            DocStatus = InvoiceHeader.Status,
                        }));

                        return RedirectToAction("_Create", new { id = InvoiceLine.JobInvoiceHeaderId, JobWorkerId = InvoiceLine.JobWorkerId, LineNature = svm.LineNature });
                    
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    int status = InvoiceHeader.Status;

                    JobInvoiceLine InvoiceLine_Modify = _JobInvoiceLineService.Find(svm.JobInvoiceLineId);
                    JobReceiveLine ReceiveLine_Modify = _JobReceiveLineService.Find(svm.JobReceiveLineId);

                    if (InvoiceHeader.JobReceiveHeaderId != null)
                    {
                        if (InvoiceHeader.JobReceiveHeaderId == ReceiveLine_Modify.JobReceiveHeaderId)
                        {
                            JobReceiveHeader ReceiveHeader = new JobReceiveHeaderService(_unitOfWork).Find((int)InvoiceHeader.JobReceiveHeaderId);

                            JobReceiveLine ExRecLine = new JobReceiveLine();
                            ExRecLine = Mapper.Map<JobReceiveLine>(ReceiveLine_Modify);
                            JobReceiveLine RecLine = new JobReceiveLine();

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecLine,
                                Obj = ReceiveLine_Modify,
                            });


                            //JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(ReceiveLine.JobOrderLineId);
                            //JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);


                            if (svm.ProductUidId != null && settings.isGenerateProductUid == true)
                            {
                                ProductUidHeader ProductUidHeader = new ProductUidHeaderService(_unitOfWork).Find((int)ReceiveLine_Modify.ProductUidHeaderId);
                                ProductUidHeader.ProductId = svm.ProductId;
                                ProductUidHeader.Dimension1Id = svm.Dimension1Id;
                                ProductUidHeader.Dimension2Id = svm.Dimension2Id;
                                ProductUidHeader.Dimension3Id = svm.Dimension3Id;
                                ProductUidHeader.Dimension4Id = svm.Dimension4Id;
                                ProductUidHeader.ModifiedDate = DateTime.Now;
                                ProductUidHeader.ModifiedBy = User.Identity.Name;
                                ProductUidHeader.ObjectState = Model.ObjectState.Modified;
                                db.ProductUidHeader.Add(ProductUidHeader);

                                ProductUid ProductUid = new ProductUidService(_unitOfWork).Find((int)svm.ProductUidId);
                                ProductUid.ProductUidName = svm.ProductUidName;
                                ProductUid.ProductId = svm.ProductId;
                                ProductUid.ProductUidSpecification = svm.Specification;
                                ProductUid.Dimension1Id = svm.Dimension1Id;
                                ProductUid.Dimension2Id = svm.Dimension2Id;
                                ProductUid.Dimension3Id = svm.Dimension3Id;
                                ProductUid.Dimension4Id = svm.Dimension4Id;
                                ProductUid.ModifiedDate = DateTime.Now;
                                ProductUid.ModifiedBy = User.Identity.Name;
                                ProductUid.ObjectState = Model.ObjectState.Modified;
                                db.ProductUid.Add(ProductUid);
                            }


                            if (ReceiveLine_Modify.StockId != null)
                            {
                                StockViewModel StockViewModel = new StockViewModel();
                                StockViewModel.StockHeaderId = ReceiveHeader.StockHeaderId ?? 0;
                                StockViewModel.StockId = ReceiveLine_Modify.StockId ?? 0;
                                StockViewModel.DocHeaderId = ReceiveLine_Modify.JobReceiveHeaderId;
                                StockViewModel.DocLineId = ReceiveLine_Modify.JobReceiveLineId;
                                StockViewModel.DocTypeId = ReceiveHeader.DocTypeId;
                                StockViewModel.StockHeaderDocDate = ReceiveHeader.DocDate;
                                StockViewModel.StockDocDate = ReceiveHeader.DocDate;
                                StockViewModel.DocNo = ReceiveHeader.DocNo;
                                StockViewModel.DivisionId = ReceiveHeader.DivisionId;
                                StockViewModel.SiteId = ReceiveHeader.SiteId;
                                StockViewModel.CurrencyId = null;
                                StockViewModel.HeaderProcessId = ReceiveHeader.ProcessId;
                                StockViewModel.PersonId = ReceiveHeader.JobWorkerId;
                                StockViewModel.ProductId = svm.ProductId;
                                StockViewModel.HeaderFromGodownId = null;
                                StockViewModel.HeaderGodownId = ReceiveHeader.GodownId;
                                StockViewModel.GodownId = ReceiveHeader.GodownId;
                                StockViewModel.ProcessId = ReceiveHeader.ProcessId;
                                StockViewModel.LotNo = svm.LotNo;
                                StockViewModel.CostCenterId = svm.CostCenterId;
                                StockViewModel.Qty_Iss = 0;
                                StockViewModel.Qty_Rec = ReceiveLine_Modify.Qty;
                                StockViewModel.Rate = svm.Rate;
                                StockViewModel.ExpiryDate = null;
                                StockViewModel.Specification = svm.Specification;
                                StockViewModel.Dimension1Id = svm.Dimension1Id;
                                StockViewModel.Dimension2Id = svm.Dimension2Id;
                                StockViewModel.Dimension3Id = svm.Dimension3Id;
                                StockViewModel.Dimension4Id = svm.Dimension4Id;
                                StockViewModel.HeaderRemark = ReceiveHeader.Remark;
                                StockViewModel.Remark = svm.Remark;
                                StockViewModel.ProductUidId = ReceiveLine_Modify.ProductUidId;
                                StockViewModel.Status = ReceiveHeader.Status;
                                StockViewModel.CreatedBy = ReceiveLine_Modify.CreatedBy;
                                StockViewModel.CreatedDate = ReceiveLine_Modify.CreatedDate;
                                StockViewModel.ModifiedBy = User.Identity.Name;
                                StockViewModel.ModifiedDate = DateTime.Now;

                                string StockPostingError = "";
                                StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                                if (StockPostingError != "")
                                {
                                    ModelState.AddModelError("", StockPostingError);
                                    return PartialView("_Create", svm);
                                }
                            }




                            if (ReceiveLine_Modify.StockProcessId != null)
                            {
                                StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                                StockProcessViewModel.StockHeaderId = ReceiveHeader.StockHeaderId ?? 0;
                                StockProcessViewModel.StockProcessId = ReceiveLine_Modify.StockProcessId ?? 0;
                                StockProcessViewModel.DocHeaderId = ReceiveLine_Modify.JobReceiveHeaderId;
                                StockProcessViewModel.DocLineId = ReceiveLine_Modify.JobReceiveLineId;
                                StockProcessViewModel.DocTypeId = ReceiveHeader.DocTypeId;
                                StockProcessViewModel.StockHeaderDocDate = ReceiveHeader.DocDate;
                                StockProcessViewModel.StockProcessDocDate = ReceiveHeader.DocDate;
                                StockProcessViewModel.DocNo = ReceiveHeader.DocNo;
                                StockProcessViewModel.DivisionId = ReceiveHeader.DivisionId;
                                StockProcessViewModel.SiteId = ReceiveHeader.SiteId;
                                StockProcessViewModel.CurrencyId = null;
                                StockProcessViewModel.HeaderProcessId = ReceiveHeader.ProcessId;
                                StockProcessViewModel.PersonId = ReceiveHeader.JobWorkerId;
                                StockProcessViewModel.ProductId = svm.ProductId;
                                StockProcessViewModel.HeaderFromGodownId = null;
                                StockProcessViewModel.HeaderGodownId = ReceiveHeader.GodownId;
                                StockProcessViewModel.GodownId = ReceiveHeader.GodownId;
                                StockProcessViewModel.ProcessId = ReceiveHeader.ProcessId;
                                StockProcessViewModel.LotNo = svm.LotNo;
                                StockProcessViewModel.CostCenterId = svm.CostCenterId;
                                StockProcessViewModel.Qty_Iss = svm.ReceiveQty + svm.LossQty;
                                StockProcessViewModel.Qty_Rec = 0;
                                StockProcessViewModel.Rate = svm.Rate;
                                StockProcessViewModel.ExpiryDate = null;
                                StockProcessViewModel.Specification = svm.Specification;
                                StockProcessViewModel.Dimension1Id = svm.Dimension1Id;
                                StockProcessViewModel.Dimension2Id = svm.Dimension2Id;
                                StockProcessViewModel.Dimension3Id = svm.Dimension3Id;
                                StockProcessViewModel.Dimension4Id = svm.Dimension4Id;
                                StockProcessViewModel.HeaderRemark = ReceiveHeader.Remark;
                                StockProcessViewModel.Remark = svm.Remark;
                                StockProcessViewModel.ProductUidId = ReceiveLine_Modify.ProductUidId;
                                StockProcessViewModel.Status = ReceiveHeader.Status;
                                StockProcessViewModel.CreatedBy = ReceiveLine_Modify.CreatedBy;
                                StockProcessViewModel.CreatedDate = ReceiveLine_Modify.CreatedDate;
                                StockProcessViewModel.ModifiedBy = User.Identity.Name;
                                StockProcessViewModel.ModifiedDate = DateTime.Now;

                                string StockProcessPostingError = "";
                                StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                                if (StockProcessPostingError != "")
                                {
                                    ModelState.AddModelError("", StockProcessPostingError);
                                    return PartialView("_Create", svm);
                                }
                            }

                            ReceiveLine_Modify.PenaltyAmt = svm.PenaltyAmt;
                            ReceiveLine_Modify.PenaltyRate = svm.PenaltyRate;
                            ReceiveLine_Modify.IncentiveRate = svm.IncentiveRate;
                            ReceiveLine_Modify.Remark = svm.Remark;
                            ReceiveLine_Modify.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                            ReceiveLine_Modify.DealUnitId = svm.DealUnitId;
                            ReceiveLine_Modify.DealQty = svm.DealQty;
                            ReceiveLine_Modify.Qty = svm.ReceiveQty;
                            ReceiveLine_Modify.LossQty = svm.LossQty;
                            ReceiveLine_Modify.PassQty = svm.PassQty;
                            ReceiveLine_Modify.Weight = svm.Weight;
                            ReceiveLine_Modify.IncentiveAmt = svm.IncentiveAmt;
                            ReceiveLine_Modify.PenaltyAmt = svm.PenaltyAmt;
                            ReceiveLine_Modify.MfgDate = svm.MfgDate;
                            ReceiveLine_Modify.ModifiedDate = DateTime.Now;
                            ReceiveLine_Modify.ModifiedBy = User.Identity.Name;


                            ReceiveLine_Modify.ObjectState = Model.ObjectState.Modified;
                            db.JobReceiveLine.Add(ReceiveLine_Modify);

                            if (ReceiveLine_Modify.JobOrderLineId != null)
                            {
                                new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnReceive((int)ReceiveLine_Modify.JobOrderLineId, ReceiveLine_Modify.JobReceiveLineId, ReceiveHeader.DocDate, RecLine.Qty + RecLine.LossQty, ref db);
                            }


                            //_JobReceiveLineService.Update(temprec);

                            #region BomPost

                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            //Saving BOMPOST Data

                            if (!string.IsNullOrEmpty(svm.JobInvoiceSettings.SqlProcConsumption))
                            {
                                IEnumerable<JobReceiveBom> OldBomList = new JobReceiveBomService(_unitOfWork).GetBomForLine(ReceiveLine_Modify.JobReceiveLineId);

                                foreach (var item in OldBomList)
                                {
                                    if (item.StockProcessId != null)
                                    {
                                        new StockProcessService(_unitOfWork).Delete((int)item.StockProcessId);
                                    }
                                    new JobReceiveBomService(_unitOfWork).Delete(item.JobReceiveBomId);
                                }


                                var BomPostList = _JobReceiveLineService.GetBomPostingDataForWeaving(svm.ProductId, svm.Dimension1Id, svm.Dimension2Id, null, null, ReceiveHeader.ProcessId, ReceiveLine_Modify.PassQty, ReceiveHeader.DocTypeId, svm.JobInvoiceSettings.SqlProcConsumption, svm.JobOrderLineId, ReceiveLine_Modify.Weight).ToList();

                                foreach (var item in BomPostList)
                                {
                                    JobReceiveBom BomPost = new JobReceiveBom();
                                    BomPost.JobReceiveHeaderId = ReceiveHeader.JobReceiveHeaderId;
                                    BomPost.JobReceiveLineId = ReceiveLine_Modify.JobReceiveLineId;
                                    BomPost.CreatedBy = User.Identity.Name;
                                    BomPost.CreatedDate = DateTime.Now;
                                    BomPost.ModifiedBy = User.Identity.Name;
                                    BomPost.ModifiedDate = DateTime.Now;
                                    BomPost.ProductId = item.ProductId;
                                    BomPost.Qty = Convert.ToDecimal(item.Qty);



                                    StockProcessViewModel StockProcessBomViewModel = new StockProcessViewModel();
                                    if (ReceiveHeader.StockHeaderId != null && ReceiveHeader.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = (int)ReceiveHeader.StockHeaderId;
                                    }
                                    else if (svm.JobInvoiceSettings.isPostedInStock)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = -1;
                                    }
                                    else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                                    {
                                        StockProcessBomViewModel.StockHeaderId = 0;
                                    }
                                    StockProcessBomViewModel.DocHeaderId = ReceiveHeader.JobReceiveHeaderId;
                                    StockProcessBomViewModel.DocLineId = ReceiveLine_Modify.JobReceiveLineId;
                                    StockProcessBomViewModel.DocTypeId = ReceiveHeader.DocTypeId;
                                    StockProcessBomViewModel.StockHeaderDocDate = ReceiveHeader.DocDate;
                                    StockProcessBomViewModel.StockProcessDocDate = ReceiveHeader.DocDate;
                                    StockProcessBomViewModel.DocNo = ReceiveHeader.DocNo;
                                    StockProcessBomViewModel.DivisionId = ReceiveHeader.DivisionId;
                                    StockProcessBomViewModel.SiteId = ReceiveHeader.SiteId;
                                    StockProcessBomViewModel.CurrencyId = null;
                                    StockProcessBomViewModel.HeaderProcessId = null;
                                    StockProcessBomViewModel.PersonId = ReceiveHeader.JobWorkerId;
                                    StockProcessBomViewModel.ProductId = item.ProductId;
                                    StockProcessBomViewModel.HeaderFromGodownId = null;
                                    StockProcessBomViewModel.HeaderGodownId = null;
                                    StockProcessBomViewModel.GodownId = ReceiveHeader.GodownId;
                                    StockProcessBomViewModel.ProcessId = ReceiveHeader.ProcessId;
                                    StockProcessBomViewModel.LotNo = ReceiveLine_Modify.LotNo;
                                    StockProcessBomViewModel.CostCenterId = svm.CostCenterId;
                                    StockProcessBomViewModel.Qty_Iss = item.Qty;
                                    StockProcessBomViewModel.Qty_Rec = 0;
                                    StockProcessBomViewModel.Rate = 0;
                                    StockProcessBomViewModel.ExpiryDate = null;
                                    StockProcessBomViewModel.Specification = null;
                                    StockProcessBomViewModel.Dimension1Id = null;
                                    StockProcessBomViewModel.Dimension2Id = null;
                                    StockProcessBomViewModel.Dimension3Id = null;
                                    StockProcessBomViewModel.Dimension4Id = null;
                                    StockProcessBomViewModel.Remark = null;
                                    StockProcessBomViewModel.Status = ReceiveHeader.Status;
                                    StockProcessBomViewModel.CreatedBy = User.Identity.Name;
                                    StockProcessBomViewModel.CreatedDate = DateTime.Now;
                                    StockProcessBomViewModel.ModifiedBy = User.Identity.Name;
                                    StockProcessBomViewModel.ModifiedDate = DateTime.Now;

                                    string StockProcessPostingError = "";
                                    StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessBomViewModel, ref db);

                                    if (StockProcessPostingError != "")
                                    {
                                        ModelState.AddModelError("", StockProcessPostingError);
                                        return PartialView("_Create", svm);
                                    }

                                    BomPost.StockProcessId = StockProcessBomViewModel.StockProcessId;

                                    BomPost.ObjectState = Model.ObjectState.Added;
                                    //new JobReceiveBomService(_unitOfWork).Create(BomPost);
                                    db.JobReceiveBom.Add(BomPost);

                                    if (svm.JobInvoiceSettings.isPostedInStock == false && svm.JobInvoiceSettings.isPostedInStockProcess == false)
                                    {
                                        if (ReceiveHeader.StockHeaderId == null)
                                        {
                                            ReceiveHeader.StockHeaderId = StockProcessBomViewModel.StockHeaderId;
                                        }
                                    }
                                }
                            }

                            #endregion

                            ReceiveHeader.ModifiedBy = User.Identity.Name;
                            ReceiveHeader.ModifiedDate = DateTime.Now;
                            ReceiveHeader.ObjectState = Model.ObjectState.Modified;
                            db.JobReceiveHeader.Add(ReceiveHeader);


                        }
                    }

                    InvoiceLine_Modify.SalesTaxGroupProductId = svm.SalesTaxGroupProductId;
                    InvoiceLine_Modify.Amount = svm.Amount;
                    InvoiceLine_Modify.JobReceiveLineId = svm.JobReceiveLineId;
                    InvoiceLine_Modify.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    InvoiceLine_Modify.Qty = svm.PassQty;
                    InvoiceLine_Modify.DealQty = svm.DealQty;
                    InvoiceLine_Modify.DealUnitId = svm.DealUnitId;
                    InvoiceLine_Modify.Remark = svm.Remark;
                    InvoiceLine_Modify.Rate = svm.Rate;
                    InvoiceLine_Modify.IncentiveRate = svm.IncentiveRate;
                    InvoiceLine_Modify.IncentiveAmt = svm.IncentiveAmt;
                    InvoiceLine_Modify.RateDiscountPer = svm.RateDiscountPer;
                    InvoiceLine_Modify.RateDiscountAmt = svm.RateDiscountAmt;

                    InvoiceLine_Modify.ModifiedDate = DateTime.Now;
                    InvoiceLine_Modify.ModifiedBy = User.Identity.Name;
                    InvoiceLine_Modify.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceLine.Add(InvoiceLine_Modify);

                    //if (ReceiveLine_Modify.JobOrderLineId != null)
                    //{
                    //    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnInvoiceReceive((int)ReceiveLine_Modify.JobOrderLineId, InvoiceLine_Modify.JobInvoiceLineId, InvoiceHeader.DocDate, (ReceiveLine_Modify.Qty + ReceiveLine_Modify.LossQty), InvoiceLine_Modify.UnitConversionMultiplier, ref db, true);
                    //}

                    //if (ReceiveLine_Modify.JobOrderLineId != null)
                    //{
                    //    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnInvoice((int)ReceiveLine_Modify.JobOrderLineId, InvoiceLine_Modify.JobInvoiceLineId, InvoiceHeader.DocDate, InvoiceLine_Modify.Qty, InvoiceLine_Modify.UnitConversionMultiplier, ref db, true);
                    //}
                    //new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnInvoice(InvoiceLine_Modify.JobReceiveLineId, InvoiceLine_Modify.JobInvoiceLineId, InvoiceHeader.DocDate, InvoiceLine_Modify.Qty, ref db, true);


                    if (InvoiceHeader.Status != (int)StatusConstants.Drafted && InvoiceHeader.Status != (int)StatusConstants.Import)
                    {
                        InvoiceHeader.Status = (int)StatusConstants.Modified;
                        InvoiceHeader.ModifiedBy = User.Identity.Name;
                        InvoiceHeader.ModifiedDate = DateTime.Now;

                    }

                    InvoiceHeader.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceHeader.Add(InvoiceHeader);
                    

                    JobInvoiceLine ExRec = new JobInvoiceLine();
                    ExRec = Mapper.Map<JobInvoiceLine>(InvoiceLine_Modify);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = InvoiceLine_Modify,
                    });




                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = new JobInvoiceLineChargeService(_unitOfWork).Find(item.Id);

                            productcharge.LedgerAccountDrId = item.LedgerAccountDrId;
                            productcharge.LedgerAccountCrId = item.LedgerAccountCrId;
                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            productcharge.DealQty = item.DealQty;
                            productcharge.ObjectState = Model.ObjectState.Modified;
                            db.JobInvoiceLineCharge.Add(productcharge);
                            //new JobInvoiceLineChargeService(_unitOfWork).Update(productcharge);

                        }


                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = new JobInvoiceHeaderChargeService(_unitOfWork).Find(item.Id);

                            footercharge.Rate = item.Rate;
                            footercharge.Amount = item.Amount;

                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.JobInvoiceHeaderCharges.Add(footercharge);
                            //new JobInvoiceHeaderChargeService(_unitOfWork).Update(footercharge);

                        }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobInvoiceReceiveDocEvents.onLineSaveEvent(this, new JobEventArgs(InvoiceLine_Modify.JobInvoiceHeaderId, InvoiceLine_Modify.JobInvoiceLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        EventException = true;
                    }

                    try
                    {
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(null);
                        ViewBag.DocNo = InvoiceHeader.DocNo;
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobInvoiceReceiveDocEvents.afterLineSaveEvent(this, new JobEventArgs(InvoiceLine_Modify.JobInvoiceHeaderId, InvoiceLine_Modify.JobInvoiceLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = InvoiceHeader.DocTypeId,
                        DocId = InvoiceHeader.JobInvoiceHeaderId,
                        DocLineId = InvoiceLine_Modify.JobInvoiceLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = InvoiceHeader.DocNo,
                        DocDate = InvoiceHeader.DocDate,
                        xEModifications = Modifications,
                        DocStatus = InvoiceHeader.Status,
                    }));

                    //End of Saving the Activity Log

                    return Json(new { success = true });

                }
            }

            var ModelStateErrorList = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
            string Messsages = "";
            if (ModelStateErrorList.Count > 0)
            {
                foreach (var ModelStateError in ModelStateErrorList)
                {
                    foreach (var Error in ModelStateError)
                    {
                        if (!Messsages.Contains(Error.ErrorMessage))
                            Messsages = Error.ErrorMessage + System.Environment.NewLine;
                    }
                }
                if (Messsages != "")
                    ModelState.AddModelError("", Messsages);
            }

            PrepareViewBag(svm);
            ViewBag.DocNo = InvoiceHeader.DocNo;
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

        [HttpGet]
        private ActionResult _Modify(int id)
        {
            JobInvoiceLineViewModel temp = _JobInvoiceLineService.GetJobInvoiceReceiveLine(id);

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

            JobInvoiceHeader H = new JobInvoiceHeaderService(_unitOfWork).Find(temp.JobInvoiceHeaderId);

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);


            temp.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            temp.DocTypeId = H.DocTypeId;
            temp.SiteId = H.SiteId;
            temp.DivisionId = H.DivisionId;
            temp.SalesTaxGroupPersonId = H.SalesTaxGroupPersonId;


            if (H.SalesTaxGroupPersonId != null)
                temp.CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(H.DocTypeId, (int)H.SalesTaxGroupPersonId, H.SiteId, H.DivisionId);

            if (temp.CalculationId == null)
                temp.CalculationId = settings.CalculationId;

            PrepareViewBag(temp);


            if (temp.ProductNatureName == ProductNatureConstants.AdditionalCharges)
                temp.LineNature = LineNatureConstants.AdditionalCharges;
            else if (H.JobReceiveHeaderId != temp.JobReceiveHeaderId)
                temp.LineNature = LineNatureConstants.ForReceive;
            else if (temp.JobOrderLineId != null)
                temp.LineNature = LineNatureConstants.ForOrder;
            else
                temp.LineNature = LineNatureConstants.Direct;


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
            JobInvoiceLineViewModel temp = _JobInvoiceLineService.GetJobInvoiceReceiveLine(id);

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

            JobInvoiceHeader H = new JobInvoiceHeaderService(_unitOfWork).Find(temp.JobInvoiceHeaderId);
            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            if (H.SalesTaxGroupPersonId != null)
                temp.CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(H.DocTypeId, (int)H.SalesTaxGroupPersonId, H.SiteId, H.DivisionId);

            if (temp.CalculationId == null)
                temp.CalculationId = settings.CalculationId;

            PrepareViewBag(temp);

            if (temp.ProductNatureName == ProductNatureConstants.AdditionalCharges)
                temp.LineNature = LineNatureConstants.AdditionalCharges;
            else if (H.JobReceiveHeaderId != temp.JobReceiveHeaderId)
                temp.LineNature = LineNatureConstants.ForReceive;
            else if (temp.JobOrderLineId != null)
                temp.LineNature = LineNatureConstants.ForOrder;
            else
                temp.LineNature = LineNatureConstants.Direct;

            return PartialView("_Create", temp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public ActionResult DeletePost(JobInvoiceLineViewModel vm)
        {
            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceReceiveDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobInvoiceHeaderId, vm.JobInvoiceLineId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Validation failed before delete.";
            #endregion

            if (BeforeSave && !EventException)
            {
                int? StockId = 0;
                int? StockProcessId = 0;
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                if (vm.linecharges != null && vm.footercharges == null)
                {
                    ModelState.AddModelError("", "Something went wrong while deletion.Please try again.");
                    PrepareViewBag(vm);
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);
                }


                JobInvoiceLine InvoiceLine = db.JobInvoiceLine.Find(vm.JobInvoiceLineId);
                JobInvoiceHeader InvoiceHeader = db.JobInvoiceHeader.Find(InvoiceLine.JobInvoiceHeaderId);
                JobReceiveLine ReceiveLine = db.JobReceiveLine.Find(InvoiceLine.JobReceiveLineId);
                JobReceiveHeader ReceiveHeader = db.JobReceiveHeader.Find(InvoiceHeader.JobReceiveHeaderId ?? 0);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobInvoiceLine>(InvoiceLine),
                });

                JobInvoiceLineStatus Status = new JobInvoiceLineStatus();
                Status.JobInvoiceLineId = InvoiceLine.JobInvoiceLineId;
                db.JobInvoiceLineStatus.Attach(Status);

                Status.ObjectState = Model.ObjectState.Deleted;
                db.JobInvoiceLineStatus.Remove(Status);

                //_JobInvoiceLineService.Delete(JobInvoiceLine);
                InvoiceLine.ObjectState = Model.ObjectState.Deleted;
                db.JobInvoiceLine.Remove(InvoiceLine);

                var chargeslist = (from p in db.JobInvoiceLineCharge
                                   where p.LineTableId == vm.JobInvoiceLineId
                                   select p).ToList();

                if (chargeslist != null)
                    foreach (var item in chargeslist)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.JobInvoiceLineCharge.Remove(item);
                        //new JobInvoiceLineChargeService(_unitOfWork).Delete(item.Id);
                    }

                if (vm.footercharges != null)
                    foreach (var item in vm.footercharges)
                    {
                        var footer = new JobInvoiceHeaderChargeService(_unitOfWork).Find(item.Id);
                        footer.Rate = item.Rate;
                        footer.Amount = item.Amount;

                        footer.ObjectState = Model.ObjectState.Modified;
                        db.JobInvoiceHeaderCharges.Add(footer);
                        //new JobInvoiceHeaderChargeService(_unitOfWork).Update(footer);
                    }

                if (InvoiceHeader.Status != (int)StatusConstants.Drafted && InvoiceHeader.Status != (int)StatusConstants.Import)
                {
                    InvoiceHeader.Status = (int)StatusConstants.Modified;
                    InvoiceHeader.ModifiedBy = User.Identity.Name;
                    InvoiceHeader.ModifiedDate = DateTime.Now;
                    InvoiceHeader.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceHeader.Add(InvoiceHeader);


                }


                if (ReceiveLine.JobReceiveHeaderId == InvoiceHeader.JobReceiveHeaderId)
                {
                    JobReceiveLineStatus LineStatus = (from p in db.JobReceiveLineStatus
                                                        where p.JobReceiveLineId == ReceiveLine.JobReceiveLineId
                                                        select p).FirstOrDefault();

                    LineStatus.ObjectState = Model.ObjectState.Deleted;
                    db.JobReceiveLineStatus.Remove(LineStatus);

                    JobReceiveHeader Receiveheader = new JobReceiveHeaderService(_unitOfWork).Find(ReceiveLine.JobReceiveHeaderId);



                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<JobReceiveLine>(ReceiveLine),
                    });

                    if (ReceiveLine.JobOrderLineId != null)
                    {
                        new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnInvoiceReceive((int)ReceiveLine.JobOrderLineId, InvoiceLine.JobInvoiceLineId, InvoiceHeader.DocDate, 0, InvoiceLine.UnitConversionMultiplier, ref db, true);
                    }

                    StockId = ReceiveLine.StockId;
                    StockProcessId = ReceiveLine.StockProcessId;

                    if (vm.ProductUidId != null && vm.ProductUidId != 0)
                    {
                        ProductUid ProductUid = (from p in db.ProductUid
                                                    where p.ProductUIDId == vm.ProductUidId
                                                    select p).FirstOrDefault();

                        if (!(ReceiveLine.ProductUidLastTransactionDocNo == ProductUid.LastTransactionDocNo && ReceiveLine.ProductUidLastTransactionDocTypeId == ProductUid.LastTransactionDocTypeId) || InvoiceHeader.SiteId == 17)
                        {


                            if ((Receiveheader.DocNo != ProductUid.LastTransactionDocNo || Receiveheader.DocTypeId != ProductUid.LastTransactionDocTypeId))
                            {
                                ModelState.AddModelError("", "Bar Code Can't be deleted because this is already Proceed to another process.");
                                PrepareViewBag(vm);
                                return PartialView("_Create", vm);
                            }


                            ProductUid.LastTransactionDocDate = ReceiveLine.ProductUidLastTransactionDocDate;
                            ProductUid.LastTransactionDocId = ReceiveLine.ProductUidLastTransactionDocId;
                            ProductUid.LastTransactionDocNo = ReceiveLine.ProductUidLastTransactionDocNo;
                            ProductUid.LastTransactionDocTypeId = ReceiveLine.ProductUidLastTransactionDocTypeId;
                            ProductUid.LastTransactionPersonId = ReceiveLine.ProductUidLastTransactionPersonId;
                            ProductUid.CurrenctGodownId = ReceiveLine.ProductUidCurrentGodownId;
                            ProductUid.CurrenctProcessId = ReceiveLine.ProductUidCurrentProcessId;
                            ProductUid.Status = ReceiveLine.ProductUidStatus;

                            ProductUid.ObjectState = Model.ObjectState.Modified;

                            db.ProductUid.Add(ProductUid);

                            new StockUidService(_unitOfWork).DeleteStockUidForDocLineDB(Receiveheader.JobReceiveHeaderId, Receiveheader.DocTypeId, Receiveheader.SiteId, Receiveheader.DivisionId, ref db);

                        }
                    }

                    ReceiveLine.ObjectState = Model.ObjectState.Deleted;
                    db.JobReceiveLine.Remove(ReceiveLine);

                    if (StockId != null)
                    {
                        new StockService(_unitOfWork).DeleteStockDB((int)StockId, ref db, true);
                    }

                    if (StockProcessId != null)
                    {
                        new StockProcessService(_unitOfWork).DeleteStockProcessDB((int)StockProcessId, ref db, true);
                    }




                    //Receiveheader.Status = (int)StatusConstants.Modified;
                    Receiveheader.ModifiedDate = DateTime.Now;
                    Receiveheader.ModifiedBy = User.Identity.Name;
                    Receiveheader.ObjectState = Model.ObjectState.Modified;
                    db.JobReceiveHeader.Add(Receiveheader);


                    var Boms = (from p in db.JobReceiveBom
                                where p.JobReceiveLineId == vm.JobReceiveLineId
                                select p).ToList();

                    var StockProcessIds = Boms.Select(m => m.StockProcessId).ToArray();

                    var StockProcRecorcds = (from p in db.StockProcess
                                                where StockProcessIds.Contains(p.StockProcessId)
                                                select p).ToList();

                    foreach (var item in Boms)
                    {

                        if (item.StockProcessId != null)
                        {
                            var TempStockProcess = StockProcRecorcds.Where(m => m.StockProcessId == item.StockProcessId).FirstOrDefault();
                            TempStockProcess.ObjectState = Model.ObjectState.Deleted;
                            db.StockProcess.Remove(TempStockProcess);
                        }

                        item.ObjectState = Model.ObjectState.Deleted;
                        db.JobReceiveBom.Remove(item);
                    }
                }



                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    JobInvoiceReceiveDocEvents.onLineDeleteEvent(this, new JobEventArgs(InvoiceLine.JobInvoiceHeaderId, InvoiceLine.JobInvoiceLineId), ref db);
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
                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    PrepareViewBag(null);
                    return PartialView("_Create", vm);
                }

                try
                {
                    JobInvoiceReceiveDocEvents.afterLineDeleteEvent(this, new JobEventArgs(InvoiceLine.JobInvoiceHeaderId, InvoiceLine.JobInvoiceLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = InvoiceHeader.DocTypeId,
                    DocId = InvoiceHeader.JobInvoiceHeaderId,
                    DocLineId = InvoiceLine.JobInvoiceLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = InvoiceHeader.DocNo,
                    DocDate = InvoiceHeader.DocDate,
                    xEModifications = Modifications,
                    DocStatus = InvoiceHeader.Status,
                }));
            }

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

        public JsonResult GetPendingOrders(int JobWorkerId, string term, int Limit)
        {
            return Json(new JobOrderHeaderService(_unitOfWork).GetPendingJobOrdersWithPatternMatch(JobWorkerId, term, Limit).ToList());
        }

        public JsonResult GetOrderDetail(int OrderId, int InvoiceId)
        {
            return Json(new JobOrderLineService(_unitOfWork).GetLineDetailForInvoice(OrderId, InvoiceId));
        }

        public JsonResult GetReceiveDetail(int ReceiveId, int InvoiceId)
        {
            return Json(new JobInvoiceLineService(_unitOfWork).GetReceiveLineDetailForInvoice(ReceiveId, InvoiceId));
        }

        public JsonResult GetProductDetailJson(int ProductId, int JobInvoiceHeaderId)
        {
            ProductViewModel Product = new ProductService(_unitOfWork).GetProduct(ProductId);
            var LastRecord = _JobInvoiceLineService.GetLineListForIndex(JobInvoiceHeaderId).OrderByDescending(m => m.JobInvoiceLineId).FirstOrDefault();
            var JobInvoiceHeader = new JobInvoiceHeaderService(_unitOfWork).Find(JobInvoiceHeaderId);
            var Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(JobInvoiceHeader.DocTypeId, JobInvoiceHeader.DivisionId, JobInvoiceHeader.SiteId);

            Unit DealUnit = new Unit();
            if (Settings.isVisibleDealUnit == false)
            {
                DealUnit = new UnitService(_unitOfWork).Find(Product.UnitId);
            }
            else
            {
                if (LastRecord != null)
                    DealUnit = new UnitService(_unitOfWork).Find(LastRecord.DealUnitId);
                else
                    DealUnit = new UnitService(_unitOfWork).Find(Product.UnitId);
            }


            return Json(new
            {
                ProductId = Product.ProductId,
                StandardCost = Product.StandardCost,
                UnitId = Product.UnitId,
                DealUnitId = DealUnit.UnitId,
                DealUnitDecimalPlaces = DealUnit.DecimalPlaces,
                Specification = Product.ProductSpecification,
                SalesTaxGroupProductId = Product.SalesTaxGroupProductId,
                SalesTaxGroupProductName = Product.SalesTaxGroupProductName
            });
        }

        public JsonResult getunitconversiondetailjson(int productid, string unitid, string deliveryunitid)
        {
            UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(productid, unitid, deliveryunitid);
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

        public JsonResult GetPendingJobOrderProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            return new JsonpResult { Data = _JobInvoiceLineService.GetPendingProductsForJobInvoice(searchTerm, pageSize, pageNum, filter), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetPendingJobOrders(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            return new JsonpResult { Data = _JobInvoiceLineService.GetPendingJobOrdersForInvoice(searchTerm, pageSize, pageNum, filter), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetPendingJobReceives(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            return new JsonpResult { Data = _JobInvoiceLineService.GetPendingJobReceivesForInvoice(searchTerm, pageSize, pageNum, filter), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        //public JsonResult GetPendingJobReceive(int id, string term)//DocTypeId
        //{
        //    return Json(_JobInvoiceLineService.GetPendingJobReceive(id, term), JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult GetCustomProducts(int id, int JobWorkerId, string term, int Limit)//Indent Header ID
        //{

        //    var Header = db.JobInvoiceHeader.Find(id);

        //    var DocType = db.DocumentType.Where(m => m.DocumentTypeName == TransactionDoctypeConstants.TraceMapInvoice).FirstOrDefault();

        //    if (DocType != null)
        //    {
        //        if (Header.DocTypeId == DocType.DocumentTypeId)
        //        {
        //            return Json(_JobInvoiceLineService.GetProductHelpListForPendingTraceMapJobOrders(id, JobWorkerId, term, Limit), JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(_JobInvoiceLineService.GetProductHelpListForPendingJobOrders(id, JobWorkerId, term, Limit), JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    else
        //    {
        //        return Json(_JobInvoiceLineService.GetProductHelpListForPendingJobOrders(id, JobWorkerId, term, Limit), JsonRequestBehavior.AllowGet);
        //    }
        //}


        public ActionResult GetProductUidHelpList(string searchTerm, int pageSize, int pageNum, int filter)//SaleInvoiceHeaderId
        {
            JobInvoiceHeader Header = new JobInvoiceHeaderService(_unitOfWork).Find(filter);
            List<ComboBoxResult> ProductUidJson = null;

            //if (Header.JobReceiveHeaderId != null && Header.JobReceiveHeaderId != 0)
            //{
            //    ProductUidJson = _JobReceiveLineService.FGetProductUidHelpList((int)Header.JobReceiveHeaderId, searchTerm).ToList();
            //}
            //else
            //{
            //    ProductUidJson = _JobInvoiceLineService.FGetProductUidHelpList(filter, searchTerm).ToList();
            //}
            ProductUidJson = _JobInvoiceLineService.FGetProductUidHelpList(filter, searchTerm).ToList();


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

        public JsonResult SetSingleProductUid(string Ids)
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

        public ActionResult GetCustomProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _JobInvoiceLineService.GetCustomProducts(filter, searchTerm);
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

        public ActionResult GetJobOrderForProduct(string searchTerm, int pageSize, int pageNum, int filter)//SaleInvoiceReturnHeaderId
        {
            var Query = _JobInvoiceLineService.GetJobOrderHelpListForProduct(filter, searchTerm);
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

        public JsonResult SetSingleJobOrderLine(int Ids)
        {
            ComboBoxResult JobOrderJson = new ComboBoxResult();

            var JobOrderLine = from L in db.JobOrderLine
                                join H in db.JobOrderHeader on L.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
                                from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                                where L.JobOrderLineId == Ids
                                select new
                                {
                                    JobOrderLineId = L.JobOrderLineId,
                                    JobOrderNo = L.Product.ProductName
                                };

            JobOrderJson.id = JobOrderLine.FirstOrDefault().ToString();
            JobOrderJson.text = JobOrderLine.FirstOrDefault().JobOrderNo;

            return Json(JobOrderJson);
        }


        public ActionResult GetJobReceiveForProduct(string searchTerm, int pageSize, int pageNum, int filter)
        {
            var Query = _JobInvoiceLineService.GetJobReceiveHelpListForProduct(filter, searchTerm);
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

        public JsonResult SetSingleJobReceiveLine(int Ids)
        {
            ComboBoxResult JobReceiveJson = new ComboBoxResult();

            var JobReceiveLine = from L in db.JobReceiveLine
                                 join H in db.JobReceiveHeader on L.JobReceiveHeaderId equals H.JobReceiveHeaderId into JobReceiveHeaderTable
                                 from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                                 where L.JobReceiveLineId == Ids
                                 select new
                                 {
                                     JobReceiveLineId = L.JobReceiveLineId,
                                     JobReceiveNo = L.Product.ProductName
                                 };

            JobReceiveJson.id = JobReceiveLine.FirstOrDefault().ToString();
            JobReceiveJson.text = JobReceiveLine.FirstOrDefault().JobReceiveNo;

            return Json(JobReceiveJson);
        }


    }


}
